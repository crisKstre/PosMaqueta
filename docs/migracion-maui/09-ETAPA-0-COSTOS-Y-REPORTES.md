# 09 · Etapa 0: costos, utilidad y rankings (H16 / parte de MIG-007)

Fecha: 2026-10-01. Base: `5d05101` más cambios locales de MIG-001/MIG-002. [Índice](README.md).

## Alcance implementado

Los reportes ahora descuentan el costo de los productos devueltos y calculan utilidad/margen sobre la venta neta. Los rankings descuentan cantidades y reembolsos y reparten el descuento global con la misma regla de MIG-002.

Se cubre H16 para datos coherentes y operaciones secuenciales. MIG-007 también incluye otros asuntos (precisión persistida, CSV, validación general de importes) que esta entrega no resuelve. La etapa 0 sigue abierta; no hay cambio de esquema, reparación automática ni aplicación MAUI creada.

## Definiciones

Son decisiones técnicas implementadas para mantener coherencia entre reportes y devoluciones; validar los ejemplos con el operador antes de desplegar.

| Campo | Significado |
|---|---|
| `TotalVendido` | Importe original de ventas no anuladas por fecha de venta, después de descuentos y antes de devoluciones |
| `TotalDevoluciones` | Reembolsos registrados por fecha de devolución |
| `TotalNeto` | Vendido menos devoluciones |
| `TotalCosto` | Costo original de las cantidades vendidas en el período, antes de devoluciones |
| `TotalCostoDevuelto` | Costo de las cantidades reintegradas en el período |
| `TotalCostoNeto` | Costo vendido menos costo reintegrado; puede ser negativo si solo hay devoluciones |
| `Utilidad` | Venta neta menos costo neto |
| `MargenPorcentaje` | Utilidad / venta neta × 100, solo cuando la venta neta es positiva |
| `TieneMargen` | Permite mostrar «—» cuando el porcentaje no aplica; el valor numérico compatible es 0 |

Cantidad de ventas, ticket promedio y cobros por medio de pago conservan su significado original, anterior a devoluciones. Se mantiene la regla de reembolso en efectivo. La utilidad es la métrica operativa existente, basada en importes de venta y costos registrados; no incorpora gastos generales ni constituye una liquidación tributaria. El cálculo de IVA no cambió: la tarjeta queda identificada como «IVA de ventas (bruto)».

## Imputación por fecha y costo histórico

Se utiliza `DetalleVenta.CostoUnitario`, guardado al vender. Un cambio posterior de costo o precio en Producto no altera el reporte. Los productos sin costo registrado aportan cero: no se inventa su costo y la utilidad puede estar sobreestimada por datos incompletos.

El costo reintegrado se imputa por `Devolucion.Fecha`. La fecha de la venta original puede quedar fuera del rango consultado. Si la venta tiene varias líneas del mismo producto, se agrupan cantidades/costos antes de relacionarlas con devoluciones, evitando multiplicar registros. Se distribuye su costo agregado según la cantidad devuelta (promedio ponderado), coherente con la identificación por producto del esquema legacy.

Para evitar perder residuos cuando ese promedio tiene decimales periódicos, el costo devuelto del período se calcula como diferencia entre costo acumulado hasta el final y costo acumulado anterior al inicio. La devolución completa recupera exactamente el costo agregado. Se conservan fracciones de costo: 0,25 kg a costo 601 representan 150,25; solo la presentación monetaria mantiene su formato actual a pesos enteros.

| Período (venta: 3 × 1000, costo unitario 600; devolución: 2 unidades) | Venta neta | Costo neto | Utilidad |
|---|---:|---:|---:|
| Venta y devolución el mismo día | 1000 | 600 | 400 |
| Día de venta, devolución posterior | 3000 | 1800 | 1200 |
| Día posterior con solo la devolución | -2000 | -1200 | -800 |
| Ambos días | 1000 | 600 | 400 |
| Devolución completa y venta en el mismo rango | 0 | 0 | 0 |

## Rankings y código compartido

`Entidades/RepartoVenta.cs` contiene ahora el reparto puro de MIG-002, reutilizado por `CalculoDevolucion` y `VentaDao`. Se trasladó la regla sin mantener dos implementaciones: reparto proporcional del cobro entre productos, pesos restantes por mayor fracción y empate por ID.

`VentaDao.ObtenerProductosPeriodo` utiliza dos consultas por rango: detalles de ventas no anuladas del período y devoluciones con costos originales/antecedentes. El reparto y la agregación final usan `decimal`. No hay una consulta por producto o venta. El resumen reutiliza esa proyección para sus costos; los rankings ordenan y aplican el límite **después** de netear. Los empates se resuelven por `IdProducto`.

El ranking conserva productos con saldo cero o negativo que tuvieron movimientos en el rango; un día con solo devoluciones puede mostrar cantidades negativas. Resta el `Subtotal` realmente guardado en la devolución: no recalcula ni sustituye silenciosamente un reembolso histórico por el importe que habría correspondido con la regla nueva.

Esta implementación carga los detalles relevantes en memoria y consulta antecedentes de devoluciones. No se midió rendimiento con historiales grandes; antes del piloto corresponde medir volúmenes reales, memoria, índices y latencias. SQLite conserva su almacenamiento numérico actual; no se promete eliminar sus limitaciones de precisión ni una instantánea coherente entre consultas concurrentes.

## Pantalla

`FormReportes` muestra «Venta neta», «Utilidad tras devol.» y «Margen s/venta neta». El desglose añade el dinero devuelto y el ranking identifica cantidades y ventas netas. Cuando el ingreso neto no es positivo, el margen muestra «—». Si falla la carga, se limpian las cifras previas y se informa el error, evitando presentar un reporte anterior como resultado del nuevo rango.

Se comprobó compilación de WinForms; la disposición visual y el uso interactivo quedan pendientes de validación manual.

## Histórico y limitaciones

- No se repararon los `Venta.Total` reducidos antes de MIG-001. En esas bases el resumen puede diferir del ranking, que reconstruye el reparto desde los detalles y descuento original. Requieren la conciliación del [documento 07](07-ETAPA-0-DEVOLUCIONES.md).
- Los reembolsos antiguos que ignoraron descuentos se muestran por el dinero efectivamente registrado; MIG-002 puede bloquear nuevas devoluciones sobre esa venta hasta conciliarla.
- Costos negativos, cantidades devueltas excedidas o falta del detalle original impiden generar la proyección y requieren conciliación. No se reparan ni se recortan a un valor aparentemente correcto.
- Cabeceras/ítems inconsistentes, ventas anuladas con devoluciones y datos alterados fuera del servicio requieren revisión histórica. Las anulaciones carecen de fecha de movimiento: se conserva la exclusión existente de ventas anuladas; no se ha construido un libro contable temporal de anulaciones.
- Sin prueba de SQL Server, UI interactiva, concurrencia, rendimiento o datos productivos. Tampoco se implementa el futuro protocolo offline.

## Evidencia y siguiente paso

Se reprodujeron siete fallos antes del cambio. Se añadieron once casos de reportes: parcial/total, costo original, descuentos, ranking y límite, devolución posterior, cantidades por kilo, anulación, promedio de líneas repetidas entre períodos, reembolso histórico real, costo desconocido y rango vacío. Los 21 casos focalizados (incluidos diez del cálculo de devoluciones) pasaron. Comandos/resultados en [validación](05-VALIDACION-Y-OPERACION.md) y [evidencia MIG-007](evidencias/pruebas-mig007.json).

El siguiente trabajo técnico recomendado de etapa 0 es caracterizar **H03/MIG-004**: fallos de auditoría o notificación después del commit de venta, y el riesgo de que un reintento duplique el cobro. Permanecen pendientes conciliación real, concurrencia, decisiones offline y validación operativa.
