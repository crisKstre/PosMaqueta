# Hoja de ruta a producción — PosMaqueta (brief para Claude Code / IDE)

> Instrucciones de implementación priorizadas, derivadas de una auditoría del código real
> (4 frentes: datos/concurrencia, seguridad, lógica de negocio/tests, ops/despliegue).
> Cada tarea trae: objetivo, archivos, qué hacer, criterio de aceptación y tests.
> **Trabaja en orden de fases.** Dentro de cada fase, respeta el orden (hay dependencias).
> Stack: C# WinForms · .NET Framework 4.7.2 · ADO.NET crudo · SQLite (1 caja) / SQL Server (varias).

> **Actualización (2026-06-17) — implementado por Claude Code, con tests:** 0.A (caja obligatoria
> para vender; los cajeros pueden abrir/cerrar caja), 0.B (versionado de esquema `SchemaVersion` +
> aborto si la BD es más nueva; `esquema.sql` eliminado), 1.A (falla al arrancar en `SqlServer` sin
> `CadenaConexion`), 1.E (docs de backup con sufijo de fecha + restauración probada), 3.A
> (autorización en la capa de servicio), 3.D (sanitización de telemetría + usuario demo forzado a
> cambiar clave), 5.b (fechas con `InvariantCulture`), 5.c (importación sin código por nombre, sin
> duplicar), **0.C** (dinero en pesos enteros, venta por kilo) y **4.C** (pago mixto: tabla
> `PagoVenta`, arqueo/reportes por medio, UI de cobro). **Pendiente / a decidir:** 1.B
> (`Microsoft.Data.SqlClient`), 1.C / 1.D (idempotencia de venta / race de caja — para multi-caja),
> 1.E-infra (job real de backup), 2.x (instalador, config por caja), 3.B / 3.C (timeout de sesión,
> cifrado de secretos), **4.B** (devolución parcial — en curso). **4.A (DTE) fuera de alcance.**

## Reglas para el implementador
- No rompas lo que ya está bien: descuento de stock atómico (`VentaDao.cs:80-92`), anulación
  idempotente, hashing PBKDF2 (`Seguridad.cs`), SQL 100% parametrizado, IVA incluido (`Impuestos.cs`).
- Cada cambio de comportamiento necesita **test** (la suite usa BD SQLite temporal real, ver
  `PosMaqueta.Tests/ServiciosTestBase.cs`). Corre `dotnet test` y deja todo verde.
- Cambios de esquema: agrégalos en `AccesoData/DatabaseInitializer.cs` (DDL **y** `MigrarEsquema`)
  para que funcionen en BD nueva y existente. Ver Tarea 0.B (versionado de esquema).
- Decisiones que requieren al dueño/equipo van marcadas **[DECISIÓN]**: pregunta antes de asumir.

---

# FASE 0 — Bloqueantes de CONTROL e INTEGRIDAD (sirven incluso para 1 sola caja)

## 0.A — Exigir caja abierta para vender  🔴
**Problema:** se puede cobrar sin caja abierta y la venta queda con `IdCaja = NULL`, fuera de todo
arqueo (`Dominio/Servicios/VentaService.cs:281-289`; el esquema permite `IdCaja` NULL en
`DatabaseInitializer.cs:102`). Agujero de control interno.
**Qué hacer:**
- En `VentaService.CobrarVenta`, si no hay caja abierta → `throw new NegocioException("Debe abrir caja antes de vender")`. No registrar con `IdCaja=NULL`.
- En `FormVentas.btnCobrar_Click` (`:815-849`) deshabilitar el cobro / avisar si no hay caja abierta.
- Hacer `Venta.IdCaja` NOT NULL en el DDL nuevo (dejar las históricas NULL como están).
**Aceptación:** cobrar sin caja abierta lanza `NegocioException` y no inserta venta. Toda venta nueva tiene `IdCaja`.
**Tests:** `VentaServiceTests` — agregar “no permite cobrar sin caja abierta”; ajustar los tests existentes que hoy cobran sin abrir caja (validan un defecto).

## 0.B — Versionado de esquema app↔BD (prerrequisito de despliegue seguro)  🔴
**Problema:** `MigrarEsquema` (`DatabaseInitializer.cs:216-234`) es solo aditivo y no hay tabla de
versión. En multi-caja, una caja con binario viejo puede romper/corromper si otra ya migró.
**Qué hacer:**
- Crear tabla `SchemaVersion(Version INT)`. Definir `ESQUEMA_VERSION` constante en el código.
- Al arrancar: si la versión de BD > la del binario → **abortar con mensaje claro** (“esta caja está desactualizada, actualízala”). Si es menor → migrar y subir versión.
- Borrar `esquema.sql` (obsoleto y contradice el modelo) o regenerarlo desde el DDL real.
**Aceptación:** una caja con binario viejo contra BD nueva se niega a arrancar con mensaje entendible.
**Tests:** unit del chequeo de versión (mayor/igual/menor).

## 0.C — Dinero en entero CLP (consistencia de arqueo)  🟠→🔴 para producción
**Problema:** montos como punto flotante (REAL en SQLite); subtotales/totales no se re-redondean
→ descuadres de $1 (`Entidades/Producto.cs:21-24`, `VentaService.cs:178,216`, sumas en `VentaDao.cs:106`).
**Qué hacer [DECISIÓN sobre tipo]:** estandarizar CLP **entero**. Recomendado: almacenar montos como
`INTEGER`/`INT` (SQLite/SQL Server) y redondear `AwayFromZero` a peso en **cada** subtotal y total
antes de persistir y antes de sumar. Como no hay datos de producción aún, define el tipo correcto en
el DDL nuevo + migración para BD de desarrollo.
**Aceptación:** ningún monto persiste con decimales; arqueo y reportes cuadran al peso. Invariante de `Impuestos` sigue verde.
**Tests:** redondeo de subtotal con cantidad fraccionaria (Kg); suma de ventas == arqueo sin centavos fantasma.

---

# FASE 1 — Bloqueantes MULTI-CAJA (SQL Server real)

## 1.A — Conexión SQL Server real + fallo ruidoso  🔴
**Problema:** cadena por defecto apunta a `(localdb)\MSSQLLocalDB` (`AccesoData/ConfigBD.cs:35`), no
compartible en red; si falta `CadenaConexion` en modo `SqlServer`, se conecta silenciosamente a una
LocalDB fantasma.
**Qué hacer:** en modo `SqlServer`, si no hay `CadenaConexion` en `App.config` → **lanzar error al
arrancar** (no usar default LocalDB). Documentar la cadena real (Tarea de docs). Quitar/!marcar el default LocalDB.
**Aceptación:** `ProveedorBD=SqlServer` sin cadena = error claro al inicio, no BD fantasma.

## 1.B — Migrar a `Microsoft.Data.SqlClient`  🟠
**Problema:** `System.Data.SqlClient` (4.8.6) está deprecado (`AccesoData.csproj`, `ConexionBD.cs:2`, `LogRemoto.cs:3`). Ya usan `Microsoft.Data.Sqlite` para SQLite (inconsistente).
**Qué hacer:** reemplazar el paquete y los `using`. Revisar diferencias de cadena (en `Microsoft.Data.SqlClient` `Encrypt=true` es default → ver 1.C). Correr toda la suite.
**Aceptación:** compila y `dotnet test` verde con el nuevo cliente.

## 1.C — Resiliencia de red en el camino de venta  🔴
**Problema:** `con.Open()` sin timeout/retry (`ConexionBD.cs:13`); corte breve = error crudo. Si la red
cae durante `Commit()`, el cajero reintenta y **duplica la venta** (no hay idempotencia del INSERT).
**Qué hacer:**
- Añadir `Connect Timeout=5` y un **retry transitorio** (p. ej. 3 intentos con backoff) para errores de red/timeout en apertura de conexión y operaciones de solo-lectura.
- **Idempotencia de venta:** generar un `IdempotencyKey`/GUID en `VentaEnCurso` al iniciar el cobro; `RegistrarVenta` lo persiste con UNIQUE; si el reintento choca con la clave, tratar como “ya registrada” (no duplicar). [DECISIÓN: confirmar el enfoque de clave].
- **Indicador de conexión** en la UI (`FormPrincipal`/`FormVentas`): verde/rojo según ping al servidor; mensaje claro al fallar (distinguir “sin conexión, reintenta” de “bug”), mejorando `Presentacion/UI/Errores.cs:19`.
**Aceptación:** un corte de red breve no duplica ventas ni tira excepción cruda; el cajero ve estado de conexión.
**Tests:** simular `Commit` confirmado + ACK perdido → reintento no crea segunda venta (test con doble llamada y misma clave).

## 1.D — Arreglar race condition de apertura de caja  🟠
**Problema:** check-then-act en dos conexiones sin transacción (`Dominio/Servicios/CajaService.cs:41-55`);
dos cajas pueden quedar “Abierta” a la vez. `ObtenerCajaAbierta` toma la de mayor Id y enmascara la otra.
**Qué hacer:** índice único **filtrado** sobre `Estado='Abierta'` (SQL Server: `WHERE Estado='Abierta'`;
SQLite: índice único parcial) **o** transacción serializable en `AbrirCaja`. [DECISIÓN: ¿una caja por
terminal/usuario o una global? hoy es global — definir y reflejar en el índice].
**Aceptación:** dos aperturas concurrentes → solo una gana; la otra recibe `NegocioException`.
**Tests:** test que intente abrir dos veces y espere una sola caja abierta.

## 1.E — Respaldo automático del SQL Server central + restauración probada  🔴
**Problema:** en SQL Server la app no respalda (`RespaldoBD.SoportaArchivo=false`, `FormRespaldos.cs:71`);
la guía solo da un `BACKUP ... WITH INIT` manual que pisa la copia anterior.
**Qué hacer (fuera de la app, en infra):** script/job de backup **con sufijo de fecha** (no `WITH INIT`),
retención + copia fuera del disco, y **procedimiento de restauración documentado y probado**. Reutilizar
patrón de `pos-infra` (la sede ya tiene scripts de SQL Server). Documentar en `docs/DESPLIEGUE.md`.
**Aceptación:** backups versionados automáticos + una restauración real probada en limpio.

---

# FASE 2 — Despliegue y operación (para soportar N cajas en terreno)

## 2.A — Instalador + actualización coordinada  🔴
**Problema:** no hay instalador; cada caja se instala copiando carpeta y editando `App.config` a mano;
no hay forma de actualizar N cajas ni de evitar desincronización de esquema entre versiones.
**Qué hacer [DECISIÓN: ClickOnce vs MSI/WiX/Inno]:** recomendado **ClickOnce** (auto-update simple,
ideal para LAN) o MSI con updater. El instalador debe: instalar binarios, **preservar/gestionar el
`App.config` por caja**, y publicar versión. Combinar con el versionado de esquema (0.B): al actualizar,
todas las cajas convergen a la misma versión antes de tocar la BD.
**Aceptación:** instalar y actualizar una caja sin editar XML a mano; rollback posible.

## 2.B — Gestión/consistencia de configuración por caja  🟠
**Problema:** cada `.config` se edita a mano; sin verificación de que las cajas apuntan al mismo
servidor/BD; `TiendaId`/`CajaId` texto libre opt-in (dos vacíos colisionan en telemetría).
**Qué hacer:** plantilla de `App.config` + validación al arrancar (servidor/BD esperado, `CajaId` no vacío y único). Pantalla simple de “configuración de esta caja”.
**Aceptación:** una caja mal configurada se detecta al inicio con mensaje claro.

## 2.C — Documentación operativa para no-dev  🟠
**Qué hacer:** guía paso a paso (instalar SQL Server Express, habilitar TCP/IP, firewall, instalar la app,
qué hacer “si no vende”), checklist de despliegue y procedimiento de restauración. Hoy `docs/DESPLIEGUE.md`
asume nivel técnico.

---

# FASE 3 — Seguridad (endurecimiento previo a producción)

## 3.A — Autorización en la capa de servicio  🟠
**Problema:** el rol solo se chequea en la UI (ocultar botones / `if(!Sesion.EsAdmin) return;`),
saltable y sin defensa en profundidad (`FormPrincipal.cs:29-31`, `FormProductos.cs`, `FormCaja.cs:29`).
**Qué hacer:** validar rol en los métodos de servicio sensibles (crear/editar/eliminar usuarios y
productos, resetear password, anular venta, respaldos) usando `Sesion.UsuarioActual.Rol`; lanzar excepción si no autorizado.
**Aceptación:** invocar una operación admin con sesión de cajero lanza excepción aunque la UI lo permitiera.
**Tests:** servicio rechaza operación admin con rol cajero.

## 3.B — Timeout/bloqueo de sesión por inactividad  🟠
**Problema:** `Entidades/Sesion.cs` solo se limpia en logout manual; caja desatendida queda abierta.
**Qué hacer:** bloqueo por inactividad (timer configurable) que exige re-autenticación.
**Aceptación:** tras X minutos sin actividad, la app pide credenciales para continuar.

## 3.C — Secretos del `App.config` y red  🟠
**Problema:** clave SQL y de `pos_log` en texto plano (`App.config:12,24`); `TrustServerCertificate=true` (MITM); 1433 potencialmente expuesto.
**Qué hacer:** preferir `Integrated Security=true` (auth Windows, sin password) como opción por defecto;
si hay auth SQL, cifrar la sección con DPAPI/`configProtectedData`. `Encrypt=true` con certificado válido.
Asegurar que `pos_log` sea **solo INSERT** en `LogFallo` (ya creado así en la sede) y cerrar 1433 al exterior (solo LAN/Tailscale).
**Aceptación:** sin contraseñas en claro en disco; conexión cifrada.

## 3.D — Sanitizar telemetría + quitar usuario demo  🟠/🟡
**Qué hacer:** en `LogRemoto.Encolar` (`AccesoData/LogRemoto.cs:105-115`) redactar/recortar PII y posibles
secretos del `Mensaje`/`Excepcion` antes de enviar. Eliminar el seed `empleado/empleado123`
(`DatabaseInitializer.cs:304-313`) o forzar cambio de clave.
**Aceptación:** ningún dato sensible viaja a la sede; no existe usuario operable con clave pública.

---

# FASE 4 — Funcionalidad de POS chileno real

## 4.A — Boleta/DTE + libro de IVA  🟠 [DECISIÓN grande]
Persistir desglose Neto/IVA por venta (hoy se recalcula al vuelo y no se guarda), folio, y emisión de
documento tributario (SII) — probablemente vía proveedor/PSP externo. Reporte de IVA del período.
*(Integración externa; estimar y decidir alcance con el dueño.)*

## 4.B — Devolución parcial / nota de crédito  🟠
Hoy solo hay anulación total y **no impacta el arqueo de efectivo**. Implementar devolución por ítem
que reintegre stock y **registre el movimiento de efectivo** en la caja correspondiente.

## 4.C — Pago mixto  🟠
`Venta.MedioPago` es un solo string (`Entidades/Venta.cs:14`). Modelar múltiples medios en una venta
(efectivo + tarjeta) y reflejarlo en el arqueo (solo efectivo cuenta para el cajón).

---

# FASE 5 — Calidad y limpieza (menores)
- **Test de concurrencia real multihilo** del descuento de stock (hoy se simula en serie; `TestConfig.cs:5`
  fuerza serie por estado estático global). Sincronizar el estado `static` de `VentaService` (`:21-23`).
- Fechas: `DateTime.ParseExact(..., InvariantCulture)` al leer (`VentaDao.cs:199`, `CajaDao.cs:152`, `LogDao.cs:60`); considerar UTC en telemetría.
- Importación: permitir actualizar productos **sin** código de barras (hoy duplica) y mapear unidades con más cuidado (`ImportacionService.cs:59-61,161-165`).
- Evaluar migración a **.NET moderno** (8/9) a mediano plazo (4.7.2 está congelado).

---

## Resumen de prioridad
- **Antes de cualquier producción (incluida 1 caja):** Fase 0 completa.
- **Antes de multi-caja:** Fases 1 y 2.
- **Antes de manejar dinero/empleados en serio:** Fase 3.
- **Para operar como POS chileno formal:** Fase 4.
- Fase 5 en paralelo / continuo.

> Sugerencia de PRs: uno por tarea (0.A, 0.B, …), con sus tests, para revisión incremental.
