# Gastos y movimientos de caja — WinForms

Acceso: **Caja → Gastos y movimientos**. El administrador registra y anula; el cajero consulta.

## Operación
- Gasto: categoría, descripción, monto entero CLP, medio de pago, origen y referencia opcional.
- Efectivo del cajón exige turno abierto y reduce su efectivo esperado.
- Transferencia, tarjeta o efectivo externo registran el gasto sin tocar el cajón.
- Retiro del dueño reduce efectivo, pero no se presenta como gasto operativo.
- Reposición aumenta efectivo, pero no se presenta como venta.
- Fecha, usuario y turno los asigna el servicio. No se habilitó carga retroactiva.
- Un registro externo sin caja abierta no se vincula posteriormente a otro turno.
- Anular exige motivo y conserva datos, usuario y fecha de ambas operaciones. No borra filas ni duplica compensaciones.
- Todo movimiento vinculado a un turno cerrado, incluso un gasto externo, queda protegido contra anulación. No hay reapertura ni corrección histórica en esta entrega.

## Arqueo
Fondo inicial + ventas en efectivo − devoluciones + reposiciones − retiros − gastos del cajón.
Los gastos externos no participan de esta suma. Los gastos del cajón se cuentan una sola vez en OtrasSalidas.
El listado informa gastos y entradas/salidas del período, excluyendo anulados de los totales. Seleccionar una fila muestra referencia y datos de anulación.

## Persistencia
Esquema v6: tabla MovimientoCaja, índices por fecha y turno; migración aditiva para SQLite y SQL Server.
Registro/anulación y auditoría se confirman en la misma transacción. El cierre calcula y persiste el arqueo en su transacción, bloqueando la misma fila de caja que los movimientos en SQL Server.
Respaldar la instalación y BD v5 antes de actualizar: el binario anterior rechaza el esquema v6.
Proveedores, compras, carga histórica y correcciones de turnos cerrados son entregas posteriores.

Las reglas de registro/anulación por administrador, consulta por cajero y bloqueo de turnos cerrados fueron confirmadas por el usuario el 15-10-2026.
