# 08 · Etapa 0: descuentos e ítems repetidos (MIG-002)

Fecha: 2026-10-01. Base: `5d05101` más MIG-001 y cambios locales. [Índice](README.md).

## Resultado y alcance

Se corrigió el cálculo de devoluciones para respetar el descuento global, las ofertas ya aplicadas por producto y el redondeo de la venta. Se rechazan productos repetidos dentro de una misma solicitud, incluso si la suma no excede lo vendido; el llamador debe enviar una cantidad por producto. Se validan cantidades negativas, productos ajenos, selecciones vacías e importes históricos inconsistentes antes de escribir.

`CalculoDevolucion` es un cálculo puro dentro de Dominio, sin acceso a base ni estado global. Lo utilizan tanto la vista previa como `DevolucionService.Devolver`. El proyecto Dominio sigue dependiendo de AccesoData; esto no es todavía la extracción del núcleo MAUI de etapa 1.

Se mantienen las devoluciones en efectivo y la conservación de `Venta.Total` de MIG-001. No se cambió el esquema ni se repararon datos productivos. La concurrencia entre operaciones continúa pendiente (H07/T05/T06); las comprobaciones actuales se hacen antes de la transacción de escritura y no constituyen una garantía entre procesos.

## Reglas de cálculo implementadas

Las siguientes son decisiones técnicas de implementación para cumplir el límite de reembolso; no se presentan como nuevas políticas explícitamente aprobadas por el negocio. Validar los ejemplos con el operador antes del despliegue.

1. Agrupar detalles originales por `IdProducto`, sumando cantidad y `Subtotal`. La devolución legacy identifica productos, no `IdDetalle`. Si existen líneas históricas repetidas con distintos precios, se usa el importe agregado y su cantidad total (promedio ponderado).
2. Calcular el cobro original como `suma(Subtotal) - Venta.Descuento`. Los subtotales guardados ya incluyen las ofertas del producto y el redondeo por kilo. Exigir que coincida con la suma de `PagoVenta`; los importes deben ser pesos enteros no negativos.
3. Asignar a cada producto una parte proporcional de ese cobro: `cobro × subtotalProducto / subtotalVenta`. Tomar la parte entera y distribuir los pesos restantes por mayor fracción, con desempate por `IdProducto` ascendente. La suma asignada coincide exactamente con lo cobrado, independientemente del orden en que se devuelvan los productos.
4. Para una devolución parcial, calcular `redondear(asignado × cantidadDevueltaAcumulada / cantidadVendida) - importeYaReembolsado`. El redondeo es al peso, alejándose de cero en el punto medio, como en `Dinero.Redondear`. Cuando se devuelve toda la cantidad, el acumulado es exactamente el importe asignado.
5. Permitir una devolución de importe cero cuando hay cantidad para reintegrar: descuento total, producto gratuito o residuo de redondeo. Se guarda el movimiento de stock y un reembolso de cero, sin salida de efectivo.

| Caso | Resultado |
|---|---|
| Dos unidades de 1000, descuento global 1000 | Cobrado 1000; reembolsos de 500 y 500 |
| Dos unidades de 1000 con oferta 10%, descuento global 300 | Cobrado 1500; reembolsos de 750 y 750 |
| Tres unidades, cobrado 2000 | Reembolsos de 667, 666 y 667 |
| Tres productos de 100, descuento global 1 | Asignaciones de 100, 100 y 99 por orden de ID |
| Un kilo por 5, devuelto en diez partes de 0,1 | Reembolsos de 1, 0, 1, 0, 1, 0, 1, 0, 1 y 0 |
| Venta con descuento del 100% | Se reintegra el stock; reembolso 0 |

## Interfaz y contratos

- `VentaDao.ObtenerParaDevolucion` carga cabecera, pagos y detalle original. `DevolucionDao.ObtenerAcumulado` agrega cantidades y dinero previamente devueltos, separando cabeceras e ítems para evitar multiplicar importes por JOIN.
- `ObtenerDevolvibles` devuelve cantidades restantes y el importe que correspondería por devolverlas completas. `PrecioUnitario` pasa a ser una referencia neta: no es una fórmula válida para obtener el reembolso parcial por el redondeo acumulado.
- `Previsualizar` calcula los subtotales sin escribir datos y exige administrador. Ignora precios/subtotales aportados por el llamador.
- `Devolver` recalcula desde los datos persistidos y acepta un `montoEsperado` opcional. Si no coincide, rechaza y pide revisar la devolución; esta comprobación no sustituye la futura protección transaccional contra concurrencia.
- `FormDevolucion` muestra «Reembolso máx.» por producto y obtiene tanto el total visible como el de confirmación del servicio. Puede confirmar stock con reembolso cero. Los errores de conciliación se presentan al usuario y deshabilitan la confirmación.

## Compatibilidad e histórico

No se usa `Venta.Total` para calcular la asignación porque versiones anteriores a MIG-001 lo reducían al devolver. Una venta histórica con total reducido puede continuar sus devoluciones si pagos, detalles y devoluciones previas concuerdan; eso **no repara sus reportes**.

Se bloquea para conciliación si los pagos difieren del detalle menos descuento, si las cabeceras de devolución no coinciden con la suma de sus ítems, si hay cantidades excedidas/productos ajenos o si el dinero ya devuelto no coincide con la nueva regla acumulada. Esto incluye devoluciones antiguas que ignoraron el descuento y diferencias de redondeo, tanto por exceso como por defecto. No se compensan silenciosamente ni se recalcula dinero ya entregado.

`PagoVenta` pudo haberse rellenado desde `Venta.Total` en bases antiguas: una discrepancia exige contrastar comprobantes, no sobrescribir automáticamente una fuente con otra. Seguir la conciliación del [documento 07](07-ETAPA-0-DEVOLUCIONES.md).

## Pruebas y límites

Se reprodujeron nueve casos fallidos antes del cambio. Después pasaron 35 casos específicos, incluidos los 13 de MIG-001. Los 22 nuevos cubren descuentos combinados, repetidos sin efectos parciales, cantidades por kilo, descuentos totales, residuos independientes del orden, agrupación de líneas históricas, vista previa sin escrituras, importe esperado desactualizado, precios actuales distintos e históricos incompatibles.

Una prueba de invariantes recorre todos los descuentos enteros de 0 a 17 sobre dos productos, con veinte devoluciones intercaladas por escenario: ningún reembolso es negativo/fraccionario, el acumulado nunca supera lo cobrado y la devolución completa agota exactamente ese importe.

WinForms compiló con `dotnet build Presentacion/Presentacion.csproj --no-restore`, sin errores ni advertencias. Resultado completo y TRX: [validación](05-VALIDACION-Y-OPERACION.md) y [evidencia MIG-002](evidencias/pruebas-mig002.json).

No se ejecutó la interfaz interactiva ni SQL Server; tampoco se certificaron carreras entre devoluciones/anulaciones/cierre, recuperación tras un fallo posterior al commit o datos reales. Costos/utilidad/rankings netos se corrigieron posteriormente en el [documento 09](09-ETAPA-0-COSTOS-Y-REPORTES.md), que también trasladó el reparto puro a `Entidades/RepartoVenta.cs` para compartirlo. Siguen pendientes conciliación histórica, H03 y las restantes condiciones de salida de etapa 0.
