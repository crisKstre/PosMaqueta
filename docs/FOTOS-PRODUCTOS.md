# Fotos de productos — WinForms

## Uso

Guardar primero el producto en Inventario. Pulsar su columna **Foto** para ver la imagen. El administrador puede **Elegir foto**, **Quitar foto** y **Guardar**. Seleccionar o quitar solo cambia la vista previa hasta confirmar; Cancelar conserva la foto guardada. El cajero puede verla, pero los cambios requieren autorización también en el servicio.

Se aceptan JPG, PNG y BMP de hasta 10 MB y 24 megapíxeles (máximo 10.000 píxeles por lado). La app corrige la orientación EXIF, conserva la proporción y convierte a JPEG sobre blanco: imagen de hasta 512 píxeles y miniatura de hasta 96. El original, sus metadatos y la ruta del archivo no se guardan. El archivo original puede moverse o borrarse después de guardar.

Ventas muestra miniaturas, nombre, precio, unidad y stock. La foto permite agregar al carrito, incluido el selector de peso para productos Kg. Los productos sin foto siguen operativos con un marcador neutral. El diseño distribuye de una a cuatro columnas según el ancho disponible.

## Persistencia y actualización

Esquema v5: tabla `FotoProducto`, una fila por producto, con imagen y miniatura binarias. Se crea tanto en bases nuevas como existentes y su eliminación acompaña a la del producto. Se mantienen variantes SQLite y SQL Server. Reemplazar/quitar foto y registrar la auditoría es una transacción.

Antes de actualizar una instalación existente, crear y conservar un respaldo con la versión anterior. La migración es aditiva, pero el binario de esquema v4 rechazará la base marcada v5. Un eventual retroceso requiere planificar juntos aplicación y datos. Esta entrega no instala ni migra una base de operación del cliente.

Las fotos están dentro de la base; el respaldo/restauración actual las incluye sin carpetas externas. Restaurar un respaldo anterior a las fotos recupera el estado anterior de los datos y volverá a crear la tabla al iniciar la aplicación nueva.

## Rendimiento y comprobaciones

Las consultas normales de productos no cargan imágenes. Ventas sigue limitada a 60 tarjetas. Inventario carga miniaturas de celdas dibujadas; cada formulario mantiene una caché de hasta 128 entradas y libera recursos al recargar o cerrar. Una miniatura ausente o inválida muestra el marcador neutral.

Pruebas añadidas: normalización, límites, producto sin foto, reemplazo/eliminación, permisos, archivo inválido sin pérdida de foto anterior, borrado del producto, reversión ante fallo de auditoría, migración desde v4 e idempotencia, restauración en otra carpeta y persistencia binaria en SQL Server/LocalDB cuando esté disponible. Registrar resultados efectivamente ejecutados en la continuidad del proyecto.
