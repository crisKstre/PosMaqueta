# Venta por peso en WinForms

Al elegir un producto configurado con unidad **Kg**, mediante tarjeta, búsqueda por nombre o código de barras, se abre un selector de peso. Ingresar kilos con hasta tres decimales, usando coma o punto (ejemplo: `0,350`), o seleccionar **Gramos** e ingresar `350`. El diálogo muestra cantidad y subtotal antes de confirmar. Cambiar de unidad limpia la entrada para evitar reinterpretar el mismo número.

El precio se expresa por kilo. El subtotal se redondea al peso chileno usando la regla existente. Cancelar conserva el carrito. Al agregar nuevamente el mismo producto, se suma la cantidad seleccionada y se valida el stock restante.

Para corregir una cantidad, pulsar la celda **Cant.** del carrito: el valor ingresado reemplaza la cantidad total de esa línea. En productos Kg, los botones −/+ también abren ese editor. En productos por unidad mantienen su incremento/decremento de una unidad. Para quitar la línea se utiliza ✕.

Carrito, detalle de la venta e inventario muestran hasta tres decimales; el formulario de edición de producto conserva esa precisión en stock y mínimo. Los productos por unidad requieren cantidades enteras en esta interfaz. La entrada nunca se reemplaza silenciosamente por 1.

No hay cambios de esquema ni integración automática con balanza en esta entrega. El peso es ingresado por el operador. Pruebas nuevas cubren coma/punto bajo culturas es-CL/en-US, gramos, entradas inválidas, disponibilidad, edición y cobro real con persistencia y descuento de stock en SQLite temporal.
