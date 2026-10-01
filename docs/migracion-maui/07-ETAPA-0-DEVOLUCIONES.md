# 07 · Etapa 0: corrección de devoluciones (MIG-001)

Fecha: 2026-10-01. Base: `5d05101` más cambios locales. [Índice](README.md).

## Regla implementada

`Venta.Total` conserva el importe cobrado originalmente, **después de descuentos y antes de devoluciones**. «Bruto» en este documento significa antes de devoluciones, no antes de descuentos ni de impuestos. Los pagos originales se conservan. Cada devolución registra su monto y reintegra el stock en la misma transacción, sin reducir `Venta.Total`.

El reporte suma las ventas no anuladas por `Venta.Fecha` y los reembolsos por `Devolucion.Fecha`. `TotalNeto = TotalVendido - TotalDevoluciones`. El turno de la devolución recibe la salida de efectivo, incluso si la venta se cobró con tarjeta, transferencia o pago mixto. Se conserva así la regla vigente del negocio.

| Operación | Bruto del período | Devoluciones | Neto |
|---|---:|---:|---:|
| Venta 3000 y devolución 2000 el mismo día | 3000 | 2000 | 1000 |
| Día de venta 3000, devolución al día siguiente | 3000 | 0 | 3000 |
| Día siguiente: devolución 2000, sin ventas nuevas | 0 | 2000 | -2000 |
| Ambos días juntos | 3000 | 2000 | 1000 |
| Devolución total en dos operaciones de 2000 y 1000 | 3000 | 3000 | 0 |

La cantidad de ventas y el ticket promedio conservan la venta original. El neto negativo en un período de solo reembolsos es válido. No se modifica el estado de anulación ni se permite anular una venta que ya tuvo devoluciones.

## Alcance y validación

Se elimina la resta a `Venta.Total` de `DevolucionDao.Registrar` y se aclaran los comentarios del modelo/reporte. No hay cambio de esquema ni migración automática de datos. Se añaden seis casos de integración SQLite: cuatro medios de pago (incluido mixto), devolución completa mediante dos parciales y devolución posterior en otro turno. Verifican historial, bruto/neto, pagos, stock, saldo en efectivo e imputación por fechas.

Antes del cambio de producción, las 13 pruebas de `DevolucionServiceTests` dieron 6 correctas y 7 fallidas: el fallo original y los seis casos nuevos. La evidencia posterior y comandos están en [validación](05-VALIDACION-Y-OPERACION.md) y [el JSON de esta corrección](evidencias/pruebas-mig001.json).

## Conciliación del histórico: pendiente sobre copia real

La corrección evita nuevas reducciones; **no repara los totales ya reducidos**. Un registro antiguo afectado seguirá apareciendo incorrecto hasta conciliarlo. Tampoco se debe sumar todas sus devoluciones de forma indiscriminada: puede haber registros corregidos, generados por otra versión o devoluciones nuevas posteriores a esta corrección.

Antes de desplegar sobre una instalación con devoluciones históricas:

1. Obtener un respaldo consistente, registrar versión y fecha de corte y trabajar en una copia aislada.
2. Inventariar cada venta con devoluciones: total guardado, pagos, suma de subtotales menos descuento global, devoluciones por fecha/turno y comprobantes disponibles.
3. Comparar las tres fuentes. `PagoVenta` pudo ser rellenado desde `Venta.Total` por `DatabaseInitializer`; no constituye por sí solo evidencia independiente del cobro original.
4. Clasificar diferencias y documentar una propuesta por `IdVenta` con valor anterior, nuevo y evidencia. Si hubo devoluciones antes y después del cambio, el desfase puede ser solo parte del total devuelto.
5. Conciliar cobros/reembolsos por día y turno, stock y comprobantes. No alterar pagos, movimientos ni arqueos cerrados para forzar coincidencias.
6. Solo después preparar una reparación transaccional específica con comprobación del valor anterior, ensayo de repetición sin efectos y respaldo. Esta sesión no implementa ni ejecuta esa reparación.

Consulta orientativa de **solo lectura**, para esquema v4; no ejecutada aquí contra datos reales ni SQL Server. Los importes derivados son candidatos para investigación, no autorizaciones de ajuste. Los agregados separados evitan multiplicar pagos por líneas o devoluciones:

```sql
SELECT v.IdVenta, v.Fecha, v.IdCaja, v.Anulada,
       v.Total AS TotalGuardado,
       p.TotalPagos,
       l.SubtotalLineas - v.Descuento AS TotalSegunDetalle,
       d.TotalDevuelto, d.PrimeraDevolucion, d.UltimaDevolucion,
       p.TotalPagos - v.Total AS DiferenciaPagos,
       l.SubtotalLineas - v.Descuento - v.Total AS DiferenciaDetalle
FROM Venta v
JOIN (
    SELECT IdVenta, SUM(Monto) AS TotalDevuelto,
           MIN(Fecha) AS PrimeraDevolucion, MAX(Fecha) AS UltimaDevolucion
    FROM Devolucion GROUP BY IdVenta
) d ON d.IdVenta = v.IdVenta
LEFT JOIN (
    SELECT IdVenta, SUM(Monto) AS TotalPagos
    FROM PagoVenta GROUP BY IdVenta
) p ON p.IdVenta = v.IdVenta
LEFT JOIN (
    SELECT IdVenta, SUM(Subtotal) AS SubtotalLineas
    FROM DetalleVenta GROUP BY IdVenta
) l ON l.IdVenta = v.IdVenta
ORDER BY v.Fecha, v.IdVenta;
```

Ejemplo diagnóstico: guardado 1000, pagos 3000, detalle menos descuento 3000, devuelto 2000 es compatible con H01. Guardado 3000, pagos 3000, detalle 3000 y devuelto 2000 ya conserva el original y no requiere sumar 2000. Fuentes ausentes, importes no coincidentes o una venta anulada con devoluciones requieren revisión individual.

## Pendientes de la etapa 0

- Conciliación y eventual reparación del histórico real: MIG-001 sigue pendiente en esa parte.
- MIG-002 se implementó posteriormente en esta fecha: ver [documento 08](08-ETAPA-0-DESCUENTOS.md). La corrección de MIG-001 no cambió ese cálculo; las devoluciones concurrentes siguen pendientes.
- H16/MIG-007 se abordó posteriormente: ver [costos y reportes](09-ETAPA-0-COSTOS-Y-REPORTES.md). El histórico y otros asuntos de MIG-007 siguen pendientes.
- SQL Server aislado, validación manual del operador y restantes decisiones operativas de etapa 0.

No se ha creado aún la aplicación MAUI, la API ni el sincronizador.
