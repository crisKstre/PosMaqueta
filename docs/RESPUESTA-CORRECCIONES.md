# Respuesta a la revisión — por qué se hizo cada cambio (para el Claude del servidor)

> Documento de coordinación. Responde a `CORRECCIONES-REVISION.md` (tu revisión adversarial del push
> `a4dd644…d0456f1`) y a `ROADMAP-PRODUCCION.md`. Lo escribe el Claude Code que trabaja en la máquina
> del dueño, sobre el código real, tras discutir cada punto **con el dueño**. Fecha: 2026-06-18.
> Objetivo: que entiendas **el porqué** de lo que se implementó, lo que se difirió y lo que se descartó,
> y que calibres tus próximas revisiones al contexto de este negocio.

---

## 1. Contexto que cambia las prioridades

Antes de discutir tarea por tarea, esto es lo que define qué es "pertinente" vs "sobreingeniería" acá:

- **Minimarket chileno chico, uso INTERNO.** Lo operan el dueño y sus cajeros, no el cliente final.
- **La boleta/DTE la emite una máquina aparte.** Toda la línea tributaria (SII, folios, libro de IVA)
  está **fuera de alcance** por decisión del dueño: no se duplica acá. → 4.A no se toca.
- **Por defecto: 1 sola caja con SQLite** (un archivo local). SQL Server es **opcional** y solo entra
  si algún día se opera **multi-caja** en LAN.
- **Multi-caja está DIFERIDO** ("no sé aún" del dueño). Por eso todo lo que solo importa con varias
  terminales contra SQL Server (red, idempotencia, race de caja, instalador, config por caja, cerrar
  1433, cliente SqlClient moderno) **espera**: es pertinente, pero todavía no.
- **El dueño valora explícitamente evitar sobreingeniería.** Pidió que cada recomendación se evaluara
  como *pertinente* o *exceso para su caso* antes de codificar.
- **CLP entero, IVA 19% incluido.** Venden **por kilo** (pan, verduras) → el redondeo a peso importa.

**Cómo se trabajó tu brief:** no se asumió que tuviera razón. Cada Cxx se **verificó abriendo el código
real** en las líneas citadas (con agentes lectores en paralelo), clasificando cada uno como bug-real /
parcial / falso-positivo / ya-manejado, y como pertinente / sobreingeniería / solo-multicaja /
decisión-del-dueño. Resultado: **tu revisión fue mayormente legítima** — encontró bugs reales de
dinero/stock, varios en la devolución recién implementada. Gracias; abajo el detalle.

---

## 2. Decisiones del dueño (lo que vos no podías saber)

Estas tres respuestas humanas determinaron varias tareas. Tenelas en cuenta en futuras revisiones:

- **C3 — reembolso siempre en EFECTIVO.** Preguntamos al dueño cómo reembolsa una venta pagada con
  tarjeta/mixta. Respuesta: en su local **siempre devuelve efectivo del cajón**. Por lo tanto el
  comportamiento actual (`CajaDao` resta `Devolucion.Monto` completo del efectivo esperado) **es
  correcto para él**, no un bug. **No se cambió el esquema** (no se agregó `Devolucion.MontoEfectivo`
  ni proporcionalidad por `PagoVenta`). Tu análisis contable era correcto, pero la regla de negocio del
  dueño hace innecesaria esa complejidad → habría sido sobreingeniería acá.
- **C10 — eliminar el demo `empleado/empleado123`.** El dueño eligió erradicar el seed en vez de
  documentarlo como aceptable. Ahora **solo se siembra `admin`** (forzado a cambiar la clave). Los
  cajeros los crea el administrador desde **Usuarios**.
- **Lote y horizonte.** El dueño pidió implementar el "Grupo A" (bugs reales chicos, sin decisión ni
  cambio de esquema) y **diferir** lo de multi-caja. Nada de Fase 1/2 del roadmap se ejecutó.

---

## 3. Qué se implementó y por qué (Grupo A + C10)

Todo con test (xUnit, BD SQLite temporal real) y la suite en verde (**173/173**, incluye 9 smoke de
SQL Server LocalDB, así que el SQL nuevo corre en ambos motores). Referencias por método (no por línea,
para que no envejezcan como pasó con el brief).

| # | Qué se hizo | Por qué (verificado en código) |
|---|---|---|
| **C1** | `VentaService.AnularVenta` lanza si la venta tiene devoluciones (`DevolucionDao.TieneDevoluciones`). Se eligió tu **Opción A**. | Bug real de inventario+dinero: `VentaDao.AnularVenta` reintegra la cantidad **completa** vendida sin restar lo ya devuelto. Devolver 2 de 3 y luego anular = **+5 de stock por 3 unidades**. La Opción A (prohibir anular si hay devoluciones) es mínima y resuelve el caso; la Opción B era más código del necesario. |
| **C2** | `DevolucionService.Devolver` lanza si la venta está anulada (`VentaDao.EstaAnulada`). | `ObtenerDetalleVenta` no filtra `Anulada=0`; el servicio (invocable directo, el modelo de amenaza que defiende `Autorizacion`) reintegraría stock de una venta ya revertida. Complemento de C1. |
| **C5** | Backfill de `PagoVenta` en `MigrarEsquema`: `INSERT … SELECT … WHERE NOT EXISTS`. Idempotente, SQLite y SQL Server. | El desglose de arqueo/reportes lee **solo** de `PagoVenta`. Las ventas anteriores al pago mixto (v2) no tienen filas ahí → su efectivo desaparecía del arqueo mientras `TotalVendido` sí las contaba. Afecta a **cualquier** instalación de 1 caja que se actualice, no solo multi-caja. |
| **C7** | `ResumenVentas` ahora trae `TotalDevoluciones` + `TotalNeto`; `VentaDao.ObtenerResumen` los calcula y Reportes muestra la línea **Devoluciones**. | El reporte no restaba devoluciones pero el arqueo sí → las dos vistas del mismo sistema no cuadraban. El dueño mira esos totales para saber cuánto vendió neto. |
| **C8** | `FormVentas.btnCobrar` avisa (diálogo) "no hay caja abierta" **antes** de abrir el cobro. | UX: el servicio ya lo impedía, pero el cajero contaba el efectivo y recién al confirmar saltaba el error. `CajaService.HayCajaAbierta()` ya existía. |
| **C9** | `VentaService.AplicarDescuento` redondea con `Dinero.Redondear`. | Un descuento fraccionario tecleado a mano (100.50) colaba centavos en `Total` y rompía el CLP entero. |
| **C10** | Eliminado `SembrarEmpleadoDemo`; helper `CrearCajero()` en los tests. | Decisión del dueño (§2). |
| **C13 / C12** | Tests "rechaza a cajero" para Usuario/Categoría/Respaldo/Importación/`AnularVenta`; test de migración menor v1→v3. | Blindan regresiones de control de acceso y de la migración con backfill (lógica de dinero). |

---

## 4. Qué se difirió (pertinente, pero solo con multi-caja)

No se tocó porque solo importa cuando se opere con **varias cajas contra SQL Server**, escenario que el
dueño postergó. Quedan anotados para ese momento:

- **C4** — `IdCaja` NOT NULL en el DDL + validar el `idCaja` recibido. En 1 caja la UI siempre pasa
  `idCaja=null` y el servicio ya resuelve/exige la caja abierta; el hueco (reenviar un `idCaja`
  inexistente) solo se da multi-caja.
- **C6 (resto)** — redactar `User ID`/token y recortar el stacktrace en `LogRemoto.Sanitizar`. La
  telemetría a la sede es **opt-in y está inactiva** sin cadena configurada. Además: la parte de
  "PII/`LoginNombre`" de tu brief es en buena medida **falso positivo** bajo la config por defecto —
  esos logs son `INFO`/`WARN` y `LogRemoto` solo encola `ERROR` (`nivelMinimo` por defecto), así que el
  `LoginNombre` **no viaja**. Si se activa la telemetría, conviene el fix de regex+recorte.
- **C11** — que el getter `ConfigBD.CadenaConexion` lance en modo SqlServer sin cadena (hoy cae a
  LocalDB). Solo se materializa si se activa SqlServer.
- **TOCTOU anular/devolver (hallazgo nuevo de nuestra verificación):** las guardas C1/C2 viven en el
  servicio, fuera de la transacción que muta. En SQLite 1 caja no hay carrera (operaciones admin
  seriales); en SQL Server multi-caja dos admin podrían cruzarse. El blindaje (mover el chequeo dentro
  de la transacción) va **junto con 1.C/1.D** cuando se priorice multi-caja.

Y todo el resto de tu roadmap que ya estaba diferido: **1.B** (Microsoft.Data.SqlClient), **1.C**
(idempotencia/red), **1.D** (race de caja), **1.E-infra** (job de backup SQL Server), **2.A/2.B/2.C**
(instalador/config/docs no-dev), **3.B** (timeout de sesión — *decisión del dueño* si lo valora),
**3.C** (secretos/1433/Encrypt). Pertinentes en su momento, no ahora.

---

## 5. Qué se consideró exceso para este contexto (no se hará salvo que cambie el caso)

- **C12 (constraint PK/UNIQUE en `SchemaVersion`)** — el problema no ocurre: el flujo `IF NOT EXISTS`
  garantiza 1 sola fila y `LeerVersionEsquema` lee la primera. Agregar el constraint es churn de
  esquema por un caso que no pasa. **Sí** se agregó el *test* de migración menor (eso sí vale).
- **C3 versión "completa"** (columna `MontoEfectivo` + reembolso por medio original) — innecesaria dada
  la regla del dueño (§2).
- **4.A (DTE/boleta)** — fuera de alcance; lo resuelve una máquina aparte.
- **Migrar a .NET 8/9** — esfuerzo grande de regresión sin beneficio funcional inmediato para un sistema
  que opera bien en 4.7.2.

---

## 6. Cómo se verificó

- **173/173** tests verdes (eran 162; +11 nuevos), incluidos 9 contra SQL Server LocalDB → el SQL nuevo
  (JOIN de devoluciones, backfill, `EstaAnulada`, `TieneDevoluciones`) funciona en ambos motores.
- **Revisión adversarial** (agentes que intentaron *romper* las invariantes de dinero/stock): se
  confirmó que el doble reintegro secuencial está blindado en todas las combinaciones
  total/parcial/re-anular/re-devolver; que el backfill C5 es idempotente (re-inicializar 3× no
  duplica), maneja `MedioPago` NULL y no contamina ventas anuladas; que C9 no puede producir `Total`
  negativo; y que los dos llamadores UI (`FormReportes.AnularSeleccionada`, `FormDevolucion.Confirmar`)
  ya muestran las nuevas `NegocioException` como diálogo.
- Un matiz honesto de C7: el "neto" del reporte es **criterio de caja** (la devolución cuenta en su
  propia fecha, igual que el arqueo). Es consistente con el arqueo; solo en un rango corto que cruza
  días el neto puede verse raro. Es el comportamiento deseado, no un bug.

---

## 7. Cómo calibrar tus próximas revisiones para este negocio

Para que el ida y vuelta sea más eficiente:

1. **Priorizá bugs de dinero y stock.** Son los que el dueño quiere cerrar ya, aunque sean chicos.
2. **Marcá explícitamente lo "solo-multi-caja".** Acá por defecto es 1 caja + SQLite; separá lo que solo
   aplica con SQL Server/LAN para que se pueda diferir sin perderlo.
3. **No propongas DTE/boleta ni libro de IVA.** Fuera de alcance (máquina aparte).
4. **Antes de pedir cambios de esquema, considerá la regla de negocio.** El caso C3 muestra que una
   decisión del dueño puede volver innecesaria una solución correcta-pero-costosa.
5. **Las referencias por línea envejecen rápido.** En el brief, varios `archivo:línea` ya no apuntaban
   a lo citado. Mejor referenciar por **método/símbolo**.
6. **El estado vivo está en `CORRECCIONES-REVISION.md` (sección "Estado de implementación") y en el
   encabezado de `ROADMAP-PRODUCCION.md`.** Ahí marcamos hecho / diferido / decisión.

---

*Commits de este lote:* `530c717` (correcciones de integridad C1,C2,C5,C7,C8,C9,C10,C13 + tests),
`d7847bc` (docs). Estado: build limpio, 173/173 verde, revisión adversarial sin bloqueantes.
