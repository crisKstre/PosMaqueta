# 10 · Confirmación de venta: H03 / parte de MIG-004

Fecha: 2026-10-01. [Índice](README.md). Cambios locales sobre `5d05101` y correcciones anteriores.

## Fallos reproducidos

- Un fallo al insertar `LogMovimiento` ocurría después del commit: la venta, pagos y stock quedaban guardados, pero el carrito permanecía abierto. El reintento podía registrar otra venta.
- Un suscriptor de `NotificadorCambios` que lanzaba una excepción impedía devolver el ID de la venta confirmada y no dejaba notificar a los siguientes suscriptores.

Ambos escenarios fallaron en pruebas SQLite aisladas antes de la corrección. La auditoría se hizo fallar con un trigger temporal que rechaza exclusivamente la acción «Venta».

## Corrección aplicada

`VentaDao.RegistrarVenta` inserta la auditoría en la misma transacción que cabecera, detalles, pagos y descuento de stock. El actor/nombre se obtienen del usuario de la venta. Un fallo de auditoría provoca rollback completo; el carrito permanece disponible para un reintento válido. No hay cambio de esquema ni backfill.

`VentaService` retira el carrito cobrado inmediatamente después del retorno exitoso del DAO, antes de logging técnico o notificaciones. Se elimina la segunda escritura de auditoría que antes se hacía desde el servicio. Los carritos en pausa se conservan.

`NotificadorCambios` aísla las excepciones por suscriptor, registra el error técnico y continúa con los restantes. Esta protección aplica a todos sus avisos; los observadores no deben validar ni decidir transacciones. Las excepciones asincrónicas de suscriptores `async void` no quedan cubiertas por este mecanismo síncrono.

`FormVentas` separa el cobro de la confirmación/refresco. Si este último falla después de recibir el ID, informa que esa venta quedó registrada y que no debe volver a cobrarse, en vez de mostrar un error genérico de operación.

## Validación

Se añadieron tres casos: rollback de venta/detalles/pagos/stock ante fallo de auditoría y reintento con un único registro; observador fallido con recepción de ID y continuidad de otros avisos; conservación de carrito pausado y actor correcto en auditoría. WinForms compiló sin errores ni advertencias.

Comandos ejecutados:

```powershell
dotnet test PosMaqueta.Tests/PosMaqueta.Tests.csproj --no-restore --filter 'FullyQualifiedName~ConfirmacionVentaTests' --logger 'trx;LogFileName=h03-antes.trx' --results-directory PosMaqueta.Tests/obj/h03
dotnet test PosMaqueta.Tests/PosMaqueta.Tests.csproj --no-restore --filter 'FullyQualifiedName~ConfirmacionVentaTests' --logger 'trx;LogFileName=h03-focalizadas.trx' --results-directory PosMaqueta.Tests/obj/h03
dotnet build Presentacion/Presentacion.csproj --no-restore
dotnet test PosMaqueta.Tests/PosMaqueta.Tests.csproj --no-restore --filter 'FullyQualifiedName!~SqlServerSmokeTests' --logger 'trx;LogFileName=h03-despues.trx' --results-directory PosMaqueta.Tests/obj/h03
```

Las dos primeras ejecuciones corresponden a los dos casos originales; después se añadió el tercero. Reejecutarlas ahora prueba la clase ampliada y corregida. TRX locales ignorados y [evidencia resumida](evidencias/pruebas-h03.json). No se ejecutaron SQL Server ni la interfaz interactiva; no se tocaron datos productivos.

## Límites y paso siguiente

Esta corrección acotada **no completa MIG-004**: falta identidad persistente de operación, respuesta recuperable después de perder conexión durante el commit, reinicio del proceso, doble invocación concurrente y protocolo de sincronización/outbox. Se implementarán con el núcleo y flujo MAUI de etapas 1–2, según el plan. La auditoría de devoluciones/anulaciones y otros servicios sigue usando su flujo anterior; no se extendió esta transacción de venta a todos los módulos.

Con las correcciones y pruebas de esta sesión puede iniciarse técnicamente la etapa 1: extraer las reglas compartidas sin duplicarlas y preparar el proyecto MAUI Windows/Android. Es un avance parcial respecto de la dependencia formal de etapa 0: histórico real, validación del operador, SQL Server aislado, políticas offline y hardware siguen pendientes y no deben darse por aprobados. No reemplazar la aplicación productiva hasta cumplir las puertas de aceptación correspondientes.

El usuario pidió cuidar el presupuesto de uso restante; se priorizó este riesgo y no se inició una refactorización general, instalación de SDK/workloads ni creación de MAUI en esta sesión.
