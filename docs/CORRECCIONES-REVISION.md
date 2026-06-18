# Correcciones post-revisión — PosMaqueta (brief para Claude Code / IDE)

> Derivado de una revisión adversarial del push `a4dd644…d0456f1` contra la auditoría previa
> (ver `ROADMAP-PRODUCCION.md`). Lista solo lo que quedó **incorrecto** en lo que SÍ se implementó.
> NO incluye lo diferido por diseño (DTE 4.A, offline, 1.B/1.C/1.D/2.x/3.B/3.C).
> Formato por tarea: problema · archivos (archivo:línea) · qué hacer · criterio · tests.
> **Orden:** P0 (bloqueante de la devolución) → P1 (mayores) → P2 (limpieza). Una tarea por PR.
> Reglas: cada cambio con test (BD SQLite temporal real, `ServiciosTestBase.cs`); no romper el
> descuento de stock atómico (`VentaDao.cs:95-107`) ni la anulación idempotente.

---

# P0 — Integridad de la DEVOLUCIÓN (4.B no es usable hasta cerrar esto)

## C1 — Coordinar Anulación ↔ Devolución (doble reintegro de stock)  🔴 BLOQUEANTE
**Problema:** `VentaDao.AnularVenta` (`AccesoData/DAO/VentaDao.cs:300-345`) reintegra el stock de TODA
la cantidad vendida sin restar lo ya devuelto, y `DevolucionService.Devolver` no mira si la venta está
anulada. Secuencia real desde la UI (devolver 2 de 3 en Reportes y luego Anular la venta): stock recibe
+2 y +3 = **+5 por 3 unidades**. Inflación de inventario + descuadre de efectivo (anular no toca caja,
devolver sí).
**Qué hacer [DECISIÓN — elegir una regla]:**
- **Opción A (simple, recomendada):** prohibir anular una venta que ya tenga devoluciones
  → en `AnularVenta`/`VentaService.AnularVenta`, si `CantidadDevuelta(idVenta) > 0` lanzar
  `NegocioException("La venta tiene devoluciones; no se puede anular")`. Y bloquear devolver una venta
  anulada (ver C2). Con eso las dos rutas quedan mutuamente excluyentes y desaparece el doble reintegro.
- **Opción B (completa):** anular reintegra solo `Cantidad − CantidadDevuelta` por ítem y revierte el
  efectivo de las devoluciones previas. Más correcto contablemente, más código.
**Criterio:** ninguna combinación de anular+devolver reintegra más stock que las unidades vendidas, ni
descuadra el efectivo.
**Tests:** `devolver 2 de 3 y luego anular → stock final = +3, no +5` (o, opción A, `anular tras
devolver lanza`); y el inverso (`devolver una venta anulada lanza`, ver C2).

## C2 — Bloquear devolución de ventas anuladas a nivel de servicio  🟠 MAYOR
**Problema:** `DevolucionService.Devolver` usa `ventaDao.ObtenerDetalleVenta(idVenta)`
(`VentaDao.cs:236`), que NO filtra `Anulada=0`. La UI solo lista no-anuladas, pero el servicio
—invocable directo, el modelo de amenaza que defiende `Autorizacion`— reintegraría stock y sacaría
efectivo de una venta ya revertida.
**Qué hacer:** en `DevolucionService.Devolver` cargar la venta y `if (venta.Anulada) throw
new NegocioException("La venta está anulada")` antes de validar ítems.
**Criterio:** devolver una venta anulada lanza `NegocioException`, sin tocar stock ni caja.
**Tests:** `Devolver_venta_anulada_lanza`.

## C3 — La devolución debe salir del MEDIO DE PAGO ORIGINAL, no siempre del efectivo  🟠 MAYOR
**Problema:** `CajaDao` resta el `Monto` completo de la devolución al efectivo esperado
(`CajaService.cs:35-37`, `CajaDao.cs:114-118`) sin mirar los `PagoVenta` de la venta. Si la venta fue
100% tarjeta, devolverla descuadra el cajón (sobrante sistemático). Con pago mixto recién agregado, esto
es un error de integridad de dinero.
**Qué hacer [DECISIÓN — método de reembolso]:** registrar en la devolución cuánto se reembolsa **en
efectivo** y cuánto en otros medios. Recomendado: reembolsar en proporción a los `PagoVenta` de la venta
(o que el admin elija), y que **el arqueo descuente SOLO la porción en efectivo**. Agregar columna
`MontoEfectivo` (o tabla de medios) a `Devolucion`; `CajaDao` resta `SUM(Devolucion.MontoEfectivo)`,
no el `Monto` total.
**Criterio:** devolver una venta pagada 100% con tarjeta NO altera el efectivo esperado del arqueo;
una mixta descuenta solo su parte de efectivo.
**Tests:** `Devolucion_de_venta_tarjeta_no_afecta_efectivo`; `Devolucion_de_venta_mixta_descuenta_solo_efectivo`.

---

# P1 — Mayores

## C4 — `Venta.IdCaja` NOT NULL en el esquema + validar la caja recibida  🟠 MAYOR
**Problema:** la regla "sin ventas huérfanas" (0.A) es solo de aplicación; el DDL sigue permitiendo
`IdCaja=NULL` (`DatabaseInitializer.cs:112` SQLite, `:220` SQL Server; entidad `Venta.IdCaja int?`,
`Venta.cs:9`). El overload `CobrarVenta(..., idCaja)` reenvía el id sin validar que exista/esté abierta.
**Qué hacer:** `IdCaja` NOT NULL en el DDL nuevo (subir `ESQUEMA_VERSION`; las históricas NULL se
quedan). En `CobrarVenta`, si se pasa `idCaja`, validar que corresponde a una caja **abierta**; si no,
resolver la caja abierta o lanzar.
**Criterio:** no se puede insertar una venta sin `IdCaja`; pasar un `idCaja` inválido lanza.
**Tests:** `cobrar con idCaja inexistente/cerrada lanza`; (las nuevas siempre con IdCaja, ya cubierto).

## C5 — Backfill de `PagoVenta` para BDs existentes  🟠 MAYOR
**Problema:** el desglose de arqueo/reportes lee SOLO de `PagoVenta` (`CajaDao.cs:92-110`,
`VentaDao.cs:124-145`); las ventas previas a esta versión no tienen filas en `PagoVenta`
(`MigrarEsquema`, `DatabaseInitializer.cs:288-307`, no las backfillea) → su efectivo desaparece del
desglose mientras `TotalVendido` sí las cuenta. El arqueo de una caja con ventas viejas subestima el efectivo.
**Qué hacer:** en la migración que introduce `PagoVenta`, **backfillear**: por cada `Venta` sin pagos,
insertar un `PagoVenta(IdVenta, MedioPago = Venta.MedioPago, Monto = Venta.Total)`. Idempotente
(solo donde no existan pagos).
**Criterio:** tras migrar una BD con ventas viejas, el desglose por medio cuadra con `TotalVendido`.
**Tests:** `migracion_backfillea_pagos_de_ventas_existentes` (sembrar venta sin PagoVenta, migrar, verificar 1 pago por el total).

## C6 — Ampliar la sanitización de telemetría (3.D)  🟠 MAYOR
**Problema:** `LogRemoto.Sanitizar` (`AccesoData/LogRemoto.cs:299-304`) solo redacta `password=`/`pwd=`.
Deja pasar `User ID=`/`Uid=`, tokens/keys, y **toda la PII/nombres de usuario** que los propios logs
incrustan (p. ej. `UsuarioService.cs:141` loguea `LoginNombre`). Además la `Excepcion` no se recorta
(`LogRemoto.cs:112-113`).
**Qué hacer:** extender la(s) regex para redactar también `user id|uid|user|api[_-]?key|token|
authorization|bearer` con sus valores; aplicar `Recortar` también al stacktrace (`Excepcion`).
[DECISIÓN] definir política de PII: si los mensajes deben llevar `LoginNombre`, considerar enviar solo
`IdUsuario` numérico aguas arriba.
**Criterio:** un mensaje con `User ID=u`, un token y un `LoginNombre` no viaja en claro a la sede.
**Tests:** ampliar `LogRemotoTests` con casos de `User ID=`, token, y longitud de `Excepcion`.

## C7 — Reportes (`ResumenVentas`) deben reflejar devoluciones  🟠 MAYOR
**Problema:** `ResumenVentas` no resta devoluciones (`Entidades/ResumenVentas.cs`,
`VentaDao.ObtenerResumen`), así que `TotalVendido` y el desglose por medio quedan sobreestimados; el
arqueo de caja sí las considera → inconsistencia entre reporte y arqueo.
**Qué hacer:** agregar `TotalDevoluciones` (y neto) a `ResumenVentas` y restarlas en el período.
**Criterio:** reporte y arqueo coinciden en el neto tras una devolución.
**Tests:** `resumen_resta_devoluciones`.

---

# P2 — Limpieza / defensa en profundidad

## C8 — UI: avisar "no hay caja abierta" ANTES del diálogo de cobro  🟡
`FormVentas.btnCobrar_Click` (`Presentacion/Forms/FormVentas.cs:815-867`) abre el diálogo de
efectivo/mixto y recién al llamar `CobrarVenta` (línea 849) salta la excepción. Agregar
`if (!cajaService.HayCajaAbierta()) { Aviso...; return; }` antes de abrir el diálogo.
**Criterio:** sin caja abierta, no se llega a contar efectivo.

## C9 — Redondear el descuento manual al total  🟡
`VentaEnCurso.DescuentoSolicitado` se fija sin pasar por `Dinero.Redondear` (`VentaService.cs:48`,
`VentaEnCurso.cs:22-23`) → un descuento fraccionario cuela fracciones en `Total`. Redondear el
descuento (y/o el total) con `Dinero.Redondear`. **Test:** descuento `100.50` → total entero.

## C10 — Erradicar (o decidir) el usuario demo `empleado`  🟡
`empleado/empleado123` sigue sembrándose (`DatabaseInitializer.cs:373-391`), solo mitigado con
`DebeCambiarPassword=1`; `empleado123` aún sirve para el primer login. [DECISIÓN] eliminar el seed del
empleado (dejar solo el admin inicial forzado a cambiar) o documentarlo como aceptable.

## C11 — Neutralizar el default LocalDB en origen (1.A)  🟡
El fail-loud vive en `Program.cs:74-81` (capa UI); `ConfigBD.CadenaConexion` (`AccesoData/ConfigBD.cs:34-35`)
SIGUE devolviendo `(localdb)\MSSQLLocalDB`. Hacer que el **getter** lance si `Proveedor==SqlServer` y no
hay cadena, para que ninguna entrada (tests, jobs) caiga silenciosamente a LocalDB.
**Criterio:** `ConfigBD.CadenaConexion` lanza en modo SqlServer sin cadena. (Ajustar `SqlServerSmokeTests` que fija la cadena.)

## C12 — `SchemaVersion` robusta + test de migración "menor"  🟡
Tabla sin PK/UNIQUE (`DatabaseInitializer.cs:71-73,173-176`); `LeerVersionEsquema` sin `LIMIT 1`. Añadir
constraint de fila única (o `TOP 1/LIMIT 1`). Agregar el test que falta: BD en `Version=1` → reinicializar
→ sube a `ESQUEMA_VERSION` sin perder datos (`EsquemaVersionTests`).

## C13 — Completar tests de rechazo de autorización  🟡
Solo `ProductoService` y `DevolucionService` tienen test de "rechaza a cajero". Agregar el equivalente
para `UsuarioService` (Crear/ResetearPassword), `CategoriaService`, `RespaldoService`,
`ImportacionService` y `VentaService.AnularVenta`, para que una regresión que borre un `ExigirAdmin()` se atrape.

---

## Prioridad
- **Bloquear merge de la devolución (4.B) hasta C1–C3.**
- C4–C7 antes de operar multi-caja / instalaciones existentes.
- C8–C13 limpieza continua. Sugerencia: un PR por Cxx con su test.
