# Catálogo de proveedores (legacy)

Acceso: Proveedores en el menú lateral, o Ctrl+8. Administrador crea, edita, desactiva y reactiva; cajero consulta. Doble clic muestra la ficha completa.

Nombre obligatorio; RUT, contacto, teléfono, correo, dirección y notas opcionales. Si se informa RUT, se normaliza y valida el dígito verificador. Su índice único incluye inactivos; sin RUT pueden existir varias fichas. Nombre no es identificador único.

Los proveedores se desactivan, no se eliminan. Alta, edición y estado se confirman junto con su registro de auditoría en una transacción. El catálogo no cambia stock, caja, precios ni gastos. Relaciones con productos, compras, saldos y pagos se definirán en otra entrega.

Esquema v7: tabla Proveedor e índice de RUT opcional para SQLite y SQL Server; migración aditiva e idempotente desde v6. Los respaldos de SQLite incluyen las fichas. Antes de actualizar una instalación, conservar aplicación y base anteriores; un binario v6 no abre una base v7.
