# Memoria de continuidad — migración MAUI

**Actualizada:** 2026-10-01. **Estado:** correcciones preparatorias MIG-001/MIG-002/H16 y H03 local verificadas; 215 pruebas correctas. Se puede iniciar técnicamente etapa 1, sin declarar cerrada etapa 0: histórico, concurrencia y validaciones operativas pendientes. MAUI aún sin implementar.

## Pedido y acuerdos del usuario

El usuario pidió analizar el proyecto completo, investigar arquitectura MAUI y generar una propuesta por etapas y documentación técnica. Confirmó:

1. Comunicación en español.
2. Plataformas iniciales: **Windows y Android**.
3. **Varios dispositivos sincronizados, capaces de trabajar sin conexión**.
4. Estrategia: **crear una aplicación MAUI nueva dentro del mismo repositorio**, trasladando progresivamente la lógica existente, corrigiéndola y desacoplándola. Aceptó esta recomendación al comparar comenzar desde cero con refactorizar el proyecto actual.
5. Guardar el plan y la memoria para trabajar en la migración más tarde (sesión del 24-09-2026).
6. El 01-10-2026 pidió leer la memoria y comenzar la corrección de devoluciones de etapa 0; se inició MIG-001.
7. Después pidió continuar con MIG-002: descuentos e ítems repetidos. No se extendió el encargo a implementar MAUI ni a corregir datos productivos.
8. Al pedir «vamos a lo siguiente», se continuó con el próximo paso documentado: costos, utilidad y rankings de H16/MIG-007. No se aplicaron reparaciones históricas.
9. Pidió hacer lo óptimo antes de migrar, cuidando un 27% restante de uso en la ventana de cinco horas. Se priorizó H03 con cambio acotado, sin iniciar MAUI ni ampliar la refactorización legacy.

La estrategia combina una interfaz nueva con extracción selectiva de reglas y pruebas. No requiere reescribir todo el negocio ni refactorizar exhaustivamente WinForms antes de poder avanzar. Conservar la aplicación actual operativa durante la transición; validar un flujo completo por vez.

## Documentación disponible

Comenzar por el [índice](README.md). El expediente contiene:

- [Diagnóstico](01-DIAGNOSTICO.md): inventario, reutilización y hallazgos H01–H16.
- [Arquitectura](02-ARQUITECTURA.md): MAUI XAML/MVVM, núcleo independiente, SQLite local y API central.
- [Datos y sincronización](03-DATOS-Y-SINCRONIZACION.md): IDs, transacciones, outbox/inbox, stock, publicación, migración y recuperación.
- [Plan por etapas](04-PLAN-POR-ETAPAS.md): etapas 0–6, backlog MIG-001–MIG-010, entregables y puertas de aceptación.
- [Validación y operación](05-VALIDACION-Y-OPERACION.md): pruebas ejecutadas y matriz futura T01–T31.
- [Decisiones y fuentes](06-DECISIONES-Y-FUENTES.md): acuerdos frente a propuestas y referencias oficiales.
- [Corrección de devoluciones](07-ETAPA-0-DEVOLUCIONES.md): reglas por fecha/turno, cambio aplicado, conciliación histórica y pendientes.
- [Descuentos e ítems repetidos](08-ETAPA-0-DESCUENTOS.md): reparto/redondeo, vista previa común, pruebas y compatibilidad histórica de MIG-002.
- [Costos y reportes](09-ETAPA-0-COSTOS-Y-REPORTES.md): costos históricos/netos, utilidad, rankings, reglas por fecha y límites de H16/MIG-007.
- [Confirmación de venta](10-ETAPA-0-CONFIRMACION-VENTA.md): auditoría transaccional, aislamiento de observadores y límites de H03/MIG-004.
- [Inventario de código](evidencias/inventario-codigo.json) y [resultado estructurado de pruebas](evidencias/pruebas-base.json).

Los README de la raíz y de `docs` enlazan la propuesta. `AGENTS.md` en la raíz apunta a esta memoria para facilitar la continuidad entre sesiones.

## Estado comprobado del proyecto

- Base analizada: commit `5d05101`. Revisar HEAD y cambios locales al retomar; los datos siguientes corresponden a esa revisión.
- Cinco proyectos: Presentacion, Dominio, AccesoData, Entidades y PosMaqueta.Tests.
- WinForms sobre .NET Framework 4.7.2; proyectos SDK-style, C# 8.
- 89 archivos C#, 13.696 líneas físicas, contando pruebas/diseñadores y excluyendo bin/obj.
- SQLite o SQL Server; esquema versión 4. No existe todavía API ni sincronización operativa entre bases desconectadas.
- Servicios dependientes de DAOs concretos, sesión/carritos/configuración estáticos y una caja abierta global.
- Entorno observado: SDK .NET 9.0.300, Windows x64, sin workloads MAUI. No se instaló SDK nuevo ni herramientas móviles.
- El 01-10-2026 se cambió `DevolucionDao.Registrar` para conservar `Venta.Total`, se aclararon comentarios en `VentaDao`/`ResumenVentas` y se agregaron seis casos de pruebas. No se cambió esquema ni datos productivos. No se crearon proyectos MAUI, backend o sincronizador.
- MIG-002 agrega `Dominio/CalculoDevolucion.cs`, consultas de importes originales/acumulados, cálculo común en `DevolucionService` y vista previa de WinForms. Se añadieron 22 casos más (12 de integración y 10 puros); el esquema sigue en v4.
- H16/MIG-007 agrega `Entidades/RepartoVenta.cs` como única regla de reparto compartida por devoluciones y reportes; modifica `VentaDao`, modelos de resumen/ranking y `FormReportes`. Añade once casos de reportes. El esquema sigue en v4 y los datos productivos no se tocaron.
- H03 local: auditoría de venta dentro de `VentaDao.RegistrarVenta`, cierre de carrito antes de efectos secundarios, excepciones aisladas por suscriptor y UI que distingue venta confirmada de fallo visual. Tres casos nuevos; no se introdujo identidad persistente de operación ni se cambió esquema.
- Los documentos están guardados en el árbol de trabajo. No se ha realizado commit ni push como parte de estas sesiones.

## Referencia de pruebas y primer defecto

Referencia inicial del 24-09-2026: **173 casos: 172 correctos y 1 fallido**. Tras MIG-001: **179 correctos**. Tras MIG-002: **201 correctos**. Tras H16/MIG-007: **212 correctos, 0 fallidos**. Se excluyeron 9 casos de `SqlServerSmokeTests`, que recrean tablas en una LocalDB de nombre fijo (`PosMaqueta_Test`). No tratar esos casos como aprobados ni ejecutar su preparación sobre una base preexistente sin aislamiento.

Fallo: `PosMaqueta.Tests.DevolucionServiceTests.ResumenVentas_refleja_las_devoluciones`, línea 118. Esperado `3000`, obtenido `1000,0`.

Flujo original: venta $3.000, devolución $2.000. `DevolucionDao.Registrar` reducía `Venta.Total`; `ResumenVentas.TotalNeto` restaba de nuevo la devolución. **Corregido para nuevas devoluciones:** venta original $3.000, devuelto $2.000, neto $1.000. El histórico ya reducido NO se repara automáticamente; requiere conciliación sobre copia real.

Los seis casos nuevos fallaron antes del cambio y pasaron después: efectivo/tarjeta/transferencia/mixto, devolución completa mediante dos parciales, devolución en otro día/turno. Se verificaron stock, historial, pagos y arqueo. El reembolso sigue saliendo íntegramente del efectivo del turno actual. Evidencia: `evidencias/pruebas-mig001.json`; TRX locales ignorados en `PosMaqueta.Tests/obj/mig001/`.

Comando y resultado completo están en el documento de validación y en el JSON de evidencia. TRX original local: `PosMaqueta.Tests/obj/maui-analysis/baseline-maui.trx`, ignorado por Git. El primer intento requirió acceso fuera del sandbox a la carpeta de SDK de Microsoft; después pudo ejecutarse con autorización.

MIG-002: los nueve casos iniciales fallaron antes del cambio; después pasaron 35 casos focalizados y la suite completa seleccionada de 201. Se compiló WinForms sin errores ni advertencias. Evidencia: `evidencias/pruebas-mig002.json`, TRX en `PosMaqueta.Tests/obj/mig002/`. No se ejecutó la interfaz interactiva ni SQL Server, no se probaron periféricos ni se midió rendimiento/cobertura. H02 tiene ahora reproducción y corrección para uso secuencial; la concurrencia (H07/T05/T06) sigue pendiente.

Regla MIG-002 implementada: repartir proporcionalmente el cobro original entre productos con mayores restos/desempate por ID; calcular cada parcial por diferencia del monto acumulado redondeado. Se rechazan repetidos y se permite reembolso cero con reintegro de stock. Históricos que no concuerdan con pagos/detalles/importe acumulado se bloquean para conciliación, sin compensarlos automáticamente. La UI usa el mismo cálculo. Validar la regla con el operador antes del despliegue; no se confunde esta decisión técnica con una nueva aprobación explícita de negocio.

H16/MIG-007: siete fallos de reportes reproducidos antes de corregir; 21 pruebas focalizadas y las 212 seleccionadas correctas después. WinForms compila con 0 errores/advertencias. `TotalCosto` conserva el costo bruto; `TotalCostoDevuelto` y `TotalCostoNeto` permiten calcular `Utilidad = TotalNeto - TotalCostoNeto`. Se usa el costo original y la fecha de devolución; rankings netean descuentos, cantidades e importes antes de ordenar/limitar. Margen no aplicable con ingreso neto <= 0 (UI «—»). Los costos fraccionarios no se redondean por devolución. Evidencia en `evidencias/pruebas-mig007.json` y TRX locales `PosMaqueta.Tests/obj/mig007/`. No se probó SQL Server, UI interactiva ni rendimiento.

## Diseño propuesto y límites de aprobación

La recomendación técnica documentada es MAUI XAML/MVVM con CommunityToolkit.Mvvm, bibliotecas de negocio independientes, SQLite por dispositivo y API ASP.NET Core con servidor modular y SQL Server central. .NET 10/MAUI 10 fue la base recomendada a la fecha del análisis; comprobar soporte y herramientas vigentes al implementar.

La aceptación de la estrategia de migración no confirma automáticamente cada detalle de diseño. Siguen pendientes de validar con el negocio, antes de habilitar el piloto:

- Cupos de stock por dispositivo frente a permitir sobreventa offline y conciliarla.
- Duración del permiso offline: 24 horas es una propuesta, no un acuerdo.
- Restricción de devoluciones offline a ventas propias aún no publicadas y coordinación del cierre de publicación.
- Alojamiento del servidor, cantidad de terminales, volúmenes y duración habitual de cortes.
- Modelos de lectores/periféricos y versiones de Windows/Android a certificar.

Estas decisiones no impiden comenzar las correcciones financieras y la extracción de reglas independientes cuando el usuario pida retomar.

Conservar las reglas previas explícitas de `docs/RESPUESTA-CORRECCIONES.md`: reembolsos en efectivo y boleta/DTE emitida por un equipo externo. El nuevo pedido habilita estudiar multicaja/offline que antes estaba diferido; no cambia esas reglas automáticamente.

## Punto exacto para retomar

**Respaldo solicitado el 01-10-2026:** el usuario autorizó guardar este trabajo en GitHub, repositorio `crisKstre/PosMaqueta`, rama `master`. La entrega agrupa documentación y correcciones verificadas con 215 pruebas. Las menciones a «sin commit» de las sesiones anteriores describen su estado previo a esta publicación; consultar Git y `origin/master` para identificar la revisión efectiva y confirmar sincronización.

Cuando el usuario pida comenzar o continuar la migración:

1. Leer esta memoria, revisar Git y comprobar los cambios locales sobre `5d05101`. Preservar cambios ajenos. La corrección está en el árbol de trabajo, sin commit ni push.
2. **Siguiente trabajo técnico recomendado: iniciar etapa 1**, revisar herramientas instaladas y extraer las reglas compartidas para el primer flujo MAUI Windows/Android, sin duplicarlas. H03 local ya fue reproducido y corregido; no seguir agregando reparaciones generales de WinForms como prerrequisito. Este adelanto técnico no cierra formalmente etapa 0 ni autoriza un piloto productivo.
3. Completar la parte histórica de MIG-001 cuando exista una copia representativa: seguir `07-ETAPA-0-DEVOLUCIONES.md`, conciliar por venta/fecha/turno y preparar ajustes específicos. No sumar devoluciones masivamente ni modificar producción para completar esta tarea. Con totales históricos reducidos, resumen y ranking pueden diferir; ver límites del documento 09.
4. Con la referencia financiera estable, extraer solo las bibliotecas y contratos necesarios para el primer flujo MAUI. Mantener el código compartido en una ubicación única, sin dos copias que evolucionen por separado.
5. Crear el proyecto MAUI nuevo en este repositorio y avanzar hacia **abrir turno → buscar/escanear producto → cobrar → cerrar turno**, con persistencia local, pruebas y Windows/Android.

No hacer una limpieza general de WinForms como prerrequisito. La implementación de API/sincronización sigue las dependencias del plan; no presentar la primera venta local como producto distribuido terminado. No permitir escritores legacy y MAUI simultáneos sobre el mismo inventario productivo sin un protocolo compatible.

## Cierre de esta sesión

Se reprodujeron dos fallos H03: auditoría que falla después del commit con carrito abierto y suscriptor que oculta la confirmación. Se corrigieron con auditoría transaccional e aislamiento de avisos; se agregó un tercer caso de carrito pausado/actor. **215 pruebas seleccionadas correctas, 0 fallidas**, con 9 SQL Server excluidas. WinForms compila con 0 errores/advertencias; `git diff --check` correcto. Evidencia: `evidencias/pruebas-h03.json`, TRX en `PosMaqueta.Tests/obj/h03/`. No se probó UI interactiva, SQL Server ni datos reales. Cambios acumulados locales sobre `5d05101`, sin commit ni push.

MIG-004 sigue parcialmente pendiente para etapas 1–2: OperationId persistente, commit ambiguo por pérdida de conexión, reinicio, doble invocación concurrente y outbox. La auditoría de otros servicios conserva su flujo anterior. Pendientes de etapa 0: conciliación real, operador, políticas offline, hardware y SQL Server aislado. Se cuidaron los recursos cerrando esta preparación, sin instalar herramientas ni empezar MAUI en esta sesión.
