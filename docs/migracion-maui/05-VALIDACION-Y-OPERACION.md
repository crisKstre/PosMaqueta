# 05 · Validación, despliegue y operación

Fecha: 2026-09-24. [Índice](README.md).

## H03 local / parte de MIG-004 · 01-10-2026

**215 casos correctos, 0 fallidos, 0 omitidos; 9 casos SQL Server excluidos.** Dos fallos reproducidos antes de corregir (auditoría y observador) y dos pruebas focalizadas aprobadas después; se añadió un tercer caso de conservación del carrito pausado/actor, validado en la suite completa. WinForms compila sin errores ni advertencias. Comandos y límites en [documento 10](10-ETAPA-0-CONFIRMACION-VENTA.md); [evidencia](evidencias/pruebas-h03.json). No se ejecutaron SQL Server ni UI interactiva. Esto no certifica idempotencia persistente, recuperación tras caída ni concurrencia.

## H16 / parte de MIG-007 · 01-10-2026 (posterior a MIG-002)

La suite seleccionada pasó **212 casos: 212 correctos, 0 fallidos, 0 omitidos**; los 9 casos SQL Server siguen excluidos. Se añadieron once casos de reportes. Siete fallaron antes del cambio; después pasaron 21 focalizados (once de reportes y diez del cálculo de devoluciones).

```powershell
dotnet test PosMaqueta.Tests/PosMaqueta.Tests.csproj --no-restore --filter 'FullyQualifiedName~ReportesDevolucionesTests' --logger 'trx;LogFileName=mig007-antes.trx' --results-directory PosMaqueta.Tests/obj/mig007
dotnet test PosMaqueta.Tests/PosMaqueta.Tests.csproj --no-restore --filter 'FullyQualifiedName~ReportesDevolucionesTests|FullyQualifiedName~CalculoDevolucionTests' --logger 'trx;LogFileName=mig007-focalizadas.trx' --results-directory PosMaqueta.Tests/obj/mig007
dotnet build Presentacion/Presentacion.csproj --no-restore
dotnet test PosMaqueta.Tests/PosMaqueta.Tests.csproj --no-restore --filter 'FullyQualifiedName!~SqlServerSmokeTests' --logger 'trx;LogFileName=mig007-despues.trx' --results-directory PosMaqueta.Tests/obj/mig007
```

El primer resultado corresponde a los siete casos iniciales sobre el código anterior; hoy la clase ampliada ejecuta once sobre el corregido. WinForms compiló con 0 errores/advertencias; SDK 9.0.300/net472 y acceso autorizado al SDK. [Evidencia estructurada](evidencias/pruebas-mig007.json); [reglas y límites](09-ETAPA-0-COSTOS-Y-REPORTES.md). T30 tiene ahora cobertura SQLite de costo/utilidad/rankings por fecha; no se certificaron SQL Server, interfaz interactiva, concurrencia, históricos reales ni rendimiento.

## MIG-002 · 01-10-2026 (posterior a MIG-001)

La suite seleccionada pasó **201 casos: 201 correctos, 0 fallidos, 0 omitidos**. Los 9 casos SQL Server siguen excluidos. Los nueve primeros casos de MIG-002 fallaron sobre el código anterior; tras completar el cambio pasaron 35 casos focalizados de devoluciones. Se añadieron 22 casos sobre los 179 de MIG-001.

```powershell
dotnet test PosMaqueta.Tests/PosMaqueta.Tests.csproj --no-restore --filter 'FullyQualifiedName~DevolucionDescuentosTests' --logger 'trx;LogFileName=mig002-antes.trx' --results-directory PosMaqueta.Tests/obj/mig002
dotnet test PosMaqueta.Tests/PosMaqueta.Tests.csproj --no-restore --filter 'FullyQualifiedName~Devolucion' --logger 'trx;LogFileName=mig002-focalizadas.trx' --results-directory PosMaqueta.Tests/obj/mig002
dotnet build Presentacion/Presentacion.csproj --no-restore
dotnet test PosMaqueta.Tests/PosMaqueta.Tests.csproj --no-restore --filter 'FullyQualifiedName!~SqlServerSmokeTests' --logger 'trx;LogFileName=mig002-despues.trx' --results-directory PosMaqueta.Tests/obj/mig002
```

El primer resultado corresponde a los nueve casos existentes antes del cambio; repetir ahora ese comando ejecuta la clase ampliada sobre el código corregido. La compilación de WinForms terminó con 0 errores/advertencias. SDK 9.0.300/net472, con acceso autorizado fuera del sandbox. [Evidencia estructurada](evidencias/pruebas-mig002.json); [reglas y límites](08-ETAPA-0-DESCUENTOS.md). No se ejecutaron la interfaz interactiva, SQL Server, carreras entre procesos ni conciliación real. T04 y la repetición de productos en una solicitud tienen cobertura; T05 concurrente/T06 y costo neto de T30 siguen pendientes.

## Corrección de etapa 0 · 01-10-2026

Sobre `5d05101` más cambios locales, la suite seleccionada pasó **179 casos: 179 correctos, 0 fallidos, 0 omitidos**. Se excluyeron nuevamente los 9 casos SQL Server. Los seis casos nuevos de devoluciones y el fallo original se reprodujeron antes de corregir: 13 casos focalizados, 6 correctos y 7 fallidos.

```powershell
dotnet test PosMaqueta.Tests/PosMaqueta.Tests.csproj --no-restore --filter 'FullyQualifiedName~DevolucionServiceTests' --logger 'trx;LogFileName=mig001-antes.trx' --results-directory PosMaqueta.Tests/obj/mig001
dotnet test PosMaqueta.Tests/PosMaqueta.Tests.csproj --no-restore --filter 'FullyQualifiedName!~SqlServerSmokeTests' --logger 'trx;LogFileName=mig001-despues.trx' --results-directory PosMaqueta.Tests/obj/mig001
```

El primer comando registra el estado **anterior** al cambio de producción; ejecutarlo sobre el código corregido ya no reproduce los fallos. TRX locales ignorados por Git; [evidencia estructurada](evidencias/pruebas-mig001.json). Se compilaron bibliotecas y pruebas con SDK 9.0.300/net472; fue necesario autorizar acceso al SDK fuera del sandbox. No se ejecutaron UI, SQL Server ni conciliación sobre datos reales. Reglas, alcance y limitaciones en [el documento 07](07-ETAPA-0-DEVOLUCIONES.md).

## Pruebas iniciales sobre el legado · 24-09-2026

Se ejecutó este comando desde la raíz, sin cambiar código fuente:

```powershell
dotnet test PosMaqueta.Tests/PosMaqueta.Tests.csproj --no-restore --filter 'FullyQualifiedName!~SqlServerSmokeTests' --logger 'trx;LogFileName=baseline-maui.trx' --results-directory .artifacts/maui-analysis
```

| Dato | Resultado |
|---|---|
| Commit analizado | `5d05101` |
| SDK / plataforma | .NET SDK 9.0.300, Windows x64 |
| Target ejecutado | .NET Framework 4.7.2 |
| Ejecución | 2026-09-24, 14:09–14:10, offset -03:00 |
| Casos seleccionados | 173 |
| Correctos | 172 |
| Fallidos | 1 |
| Omitidos por el ejecutor | 0 |
| SQL Server | 9 casos excluidos mediante filtro; no ejecutados |
| Resultado del proceso | Código 1: suite con fallo |

El primer intento encontró una restricción de acceso de MSBuild a `AppData/Local/Microsoft SDKs`. Se volvió a ejecutar con el permiso correspondiente. Se compilaron Entidades, AccesoData, Dominio y las pruebas; esto no acredita build ni ejecución de la interfaz WinForms o MAUI.

Se conserva un [resumen estructurado extraído del TRX](evidencias/pruebas-base.json). El TRX original de esta sesión se conserva localmente bajo `PosMaqueta.Tests/obj/maui-analysis/baseline-maui.trx` (salida ignorada por Git). El comando anterior vuelve a generarlo en `.artifacts/maui-analysis`.

### Fallo observado

```text
PosMaqueta.Tests.DevolucionServiceTests.ResumenVentas_refleja_las_devoluciones
Assert.Equal(): Expected 3000; Actual 1000,0
DevolucionServiceTests.cs:118
```

El test vende 3 unidades a $1.000 y devuelve 2. Espera bruto $3.000, devolución $2.000 y neto $1.000. `DevolucionDao.Registrar` ya reduce `Venta.Total` a $1.000; `ResumenVentas.TotalNeto` volvería a restar $2.000 y resultaría -$1.000. El test se detiene en la primera aserción: el segundo cálculo se deduce del código, no de una segunda aserción ejecutada.

No se corrigió durante el análisis inicial. El 01-10-2026 se corrigió para nuevas devoluciones, como se registra arriba. La parte histórica de MIG-001 permanece pendiente de conciliación.

Las pruebas SQL actuales usan `PosMaqueta_Test` en LocalDB y ejecutan `DROP TABLE` durante su preparación. No se ejecutaron para evitar alterar una base preexistente con ese nombre. La etapa de implementación debe crear una base única desechable por ejecución y usar permisos limitados. `TestConfig` desactiva paralelismo porque sesión, configuración y carritos son estáticos; hoy no hay prueba real de concurrencia distribuida.

## Qué está demostrado y qué no

Demostrado: inventario, relaciones entre proyectos, esquema y flujos inspeccionados; compilación de bibliotecas/tests del legado y resultado anterior. El código contiene pruebas de descuentos, venta por kg, pagos mixtos, stock desactualizado, anulación repetida, autorización, migración, respaldo y seguridad.

Pendiente: cubrir huecos hallados, ejecutar SQL Server de forma aislada, certificar hardware, rendimiento, UX, despliegue y funcionamiento en MAUI. No hay una aplicación MAUI implementada, backend ni sincronizador que se puedan certificar ahora. La siguiente matriz define trabajo futuro, no resultados ya obtenidos.

## Matriz de aceptación futura

| ID | Escenario | Resultado exigido | Nivel |
|---|---|---|---|
| T01 | Kg, redondeo a peso, descuentos combinados y venta cero | Importes exactos y reglas iguales en Windows/Android/servidor | Dominio |
| T02 | Pago mixto con fracciones, monto negativo o suma distinta | Rechazo antes de commit; cada importe CLP válido | Dominio/aplicación |
| T03 | Devolución 2000 sobre venta 3000 | Bruto 3000, devuelto 2000, neto 1000 | Integración |
| T04 | Devoluciones parciales con descuento global/residuos | Acumulado por línea no supera lo cobrado | Dominio/integración |
| T05 | Misma línea repetida o devolución simultánea | Sin doble reintegro/reembolso | Integración SQL |
| T06 | Anular y devolver simultáneamente | Una transición válida; la otra se rechaza sin efectos parciales | Integración SQL |
| T07 | Dos ventas por última unidad / dos aperturas del mismo terminal | Stock válido; un solo turno abierto | Integración |
| T08 | Cierre y cobro simultáneos; IdCaja ajeno/cerrado | Resultado serializable por turno; venta no cae en turno incorrecto | Integración |
| T09 | Cajero llama directamente gestión, reportes o cierre con faltante | Autorización aplicada fuera de UI y aprobador auditado | Aplicación/API |
| T10 | Doble toque / fallo de refresco tras commit / timeout | Una venta y un pago lógico; resultado recuperable por ID | ViewModel/integración |
| T11 | Cerrar proceso antes y después del commit local | Todo o nada; borrador/resultado recuperable | Persistencia/dispositivo |
| T12 | Migrar v1/v2/v3/v4, version futura, fallo de migración | Conserva datos; no modifica BD incompatible; recuperación ensayada | Migraciones |
| T13 | Dos dispositivos con cupos, sin red | No consumen cupo del otro; bloqueo claro cuando se agota | Sync |
| T14 | API confirma y se pierde respuesta; retry concurrente | Mismo recibo y un solo efecto | API/Sync |
| T15 | Pull recibe eco de venta local | No duplica descuento de stock/cupo ni totales | Sync |
| T16 | Cambios remotos mientras hay ventas locales pendientes | Proyección combina ambos una vez; ningún pendiente se pierde | Sync |
| T17 | Lote duplicado, desordenado o dependencia ausente | Espera/reintenta de forma recuperable; no aplica parcialmente una venta | Sync |
| T18 | Corte al aplicar página de cambios/cursor | Página y cursor se guardan juntos; repetir no cambia totales | Sync |
| T19 | Cursor antiguo/snapshot con cambios concurrentes | Bootstrap coherente y catch-up sin saltos | Sync |
| T20 | Cierre offline con venta aún no recibida | Cierre provisional hasta completar secuencia; conciliación exacta | Sync |
| T21 | Permiso vencido, reloj atrasado, usuario/terminal revocado | Bloqueo de nuevas acciones según política; pendientes preservados | Seguridad/Sync |
| T22 | Restaurar backup antiguo o clonar dispositivo | No reutiliza cupo ni reaplica operaciones; modo recuperación | Recuperación |
| T23 | Android suspendido/terminado durante envío | Reanuda desde outbox; sin depender del último callback | Dispositivo |
| T24 | CSV es-CL, delimitadores, nombres repetidos, costo vacío/cero | Previsualización inequívoca; importación idempotente cuando corresponda | Parser/integración |
| T25 | Copia/restauración con WAL y actividad concurrente | Snapshot consistente; integridad y sumas verificadas | Recuperación |
| T26 | Base/hardware sin espacio, BD ocupada, batería/corte eléctrico | No anuncia venta no duradera; mensaje recuperable y datos conciliables | Dispositivo |
| T27 | Release con trimming, navegación y serialización | Funciona igual que flujo validado; sin bindings faltantes | UI/paquete |
| T28 | Lector rápido, teclado, rotación, accesibilidad y lista grande | Escaneo único, foco correcto y UI utilizable | UI/hardware |
| T29 | Actualización con turno abierto/cola pendiente | Mantiene pendientes e IDs; pospone migración incompatible | Despliegue |
| T30 | Devolución posterior a día de venta / costos y ranking | Contabiliza por fecha de operación; costo neto y márgenes definidos | Reportes |
| T31 | Venta recibida antes de su devolución offline y cierre de publicación | Otra terminal no puede devolverla durante esa ventana; desbloqueo solo tras completar dependencias | Sync/concurrencia |

## Estrategia de pruebas

1. Tests puros de Domain: dinero, descuentos, límites, estado y cálculos con reloj controlado.
2. Application/Presentation: dobles de puertos para autorización, cancelación, estados y navegación; sin arrancar MAUI.
3. Integración SQLite real por prueba; bases aisladas, rollback, migraciones y reinicio. No reemplazar transacciones por mocks.
4. API y SQL Server real desechable; autenticación, concurrencia, inbox y cambios. Fallos entre pasos de commit/envío.
5. Sync con dos bases SQLite y servidor real; transporte que introduce duplicación, pérdida, desorden y latencia.
6. UI/hardware en Windows y Android, en **Release**, con un conjunto pequeño de recorridos críticos más aceptación manual del cajero.

No reutilizar fixtures con `ConfigBD` global en los nuevos tests. La suite nueva debe poder aislar instancias y probar dos sesiones concurrentes. Incluir pruebas de compatibilidad de protocolo entre versión actual y anterior durante las ventanas de despliegue.

Los builds Release importan por el trimming y la resolución de tipos/bindings; no considerar suficiente que el emulador en Debug funcione. [Trimming en MAUI](https://learn.microsoft.com/en-us/dotnet/maui/deployment/trimming?view=net-maui-10.0).

## Objetivos de rendimiento propuestos

Son metas para medir, no benchmarks obtenidos. Etapa 0 debe sustituir el dataset de referencia si los volúmenes reales son mayores.

| Métrica | Objetivo inicial | Condición de medición |
|---|---|---|
| Búsqueda local | p95 menor a 200 ms después de debounce | 10.000 productos, terminal de menor capacidad admitido |
| Confirmación local | p95 menor a 500 ms | 30 líneas, auditoría/outbox incluidos, persistencia duradera |
| Arranque utilizable | Menor a 4 s en arranque habitual | Release; separar migración/bootstrap del arranque normal |
| Convergencia | Sin diferencias tras 1.000 operaciones y 20 cortes simulados | Dos terminales; tiempo de vaciado medido y publicado |
| Interacción | Sin bloqueo perceptible al escribir, escanear o navegar | Carga simultánea de sync y catálogo |

Registrar tamaño de base, memoria, batería y latencia de SQLite con `FULL`. Si no se cumplen metas, optimizar consultas, lotes y paginación antes de introducir un framework nuevo.

## Compilación y entrega

- Fijar SDK compatible mediante `global.json`, versiones de paquetes y workload set; registrar JDK/Android SDK y Windows SDK usados. En este equipo solo se observó SDK 9.0.300 sin workloads: prepararlo es una tarea de implementación, no realizada.
- Pipeline por PR: núcleo, ViewModels, migraciones, API y sincronización. Builds Windows/Android para cambios que afecten cliente; Release instalable en hitos.
- Windows: paquete MSIX firmado como primera opción de distribución interna, verificando certificados, ubicación de datos y actualización. Existen modalidades empaquetadas y no empaquetadas; decidir según instalación real. [Despliegue Windows](https://learn.microsoft.com/en-us/dotnet/maui/windows/deployment/overview?view=net-maui-10.0).
- Android: APK firmado para piloto interno; evaluar canal administrado o Play privado para distribución. Custodiar clave de firma, conservar ApplicationId y ensayar actualización conservando datos. No asumir permisos de almacenamiento generales. [Publicación Android](https://learn.microsoft.com/en-us/dotnet/maui/android/deployment/publish-cli?view=net-maui-10.0).
- Secretos, certificados y claves de firma fuera del repositorio; configuración no sensible versionada. Dependencias nativas y permisos comprobados en hardware real.
- Migrar esquema central desde herramienta administrativa con exclusión mutua, no desde cada móvil al arrancar. Cliente comprueba versión local antes de escritura.
- Versionar por separado app, esquema local, esquema central, protocolo y reglas financieras. Ampliar contratos primero; retirar versiones solo después de conciliar terminales desconectados.

No se instala ni publica nada como parte de este documento.

## Operación y observabilidad

En la caja: mostrar conexión, última sincronización, ventas pendientes, antigüedad del stock y turno conciliado/provisional. Mensajes de negocio claros: «Venta guardada; se enviará cuando vuelva la conexión», «Cupo disponible agotado», «Se requiere conexión para esta devolución».

En administración: métricas de antigüedad/tamaño de outbox, operaciones en revisión, latencia de recepción, terminales sin contacto, cupos retenidos, diferencias de arqueo y resultado de respaldos. Correlación por OperationId; logs sin contraseñas, tokens, cadenas de conexión ni payloads financieros completos por defecto. Telemetría no participa del commit comercial.

La sincronización al iniciar/reanudar y manual es obligatoria. En Android, usar trabajo persistente del sistema para oportunidades adicionales; un temporizador .NET no garantiza ejecución con la app suspendida. No prometer sincronización continua en segundo plano. [Trabajo en segundo plano Android](https://developer.android.com/develop/background-work/background-tasks).

## Recuperación operativa

| Incidente | Procedimiento |
|---|---|
| API/red caída | Continuar solo dentro de permisos/cupos; conservar pendientes y reintentar |
| Confirmación visual incierta | Consultar venta por OperationId antes de repetir cobro |
| Operación en revisión | Preservar documento y pago, abrir incidencia con correlación, corregir mediante operación compensatoria |
| Disco lleno/corrupto | Bloquear confirmaciones nuevas; preservar datos/evidencia; recuperar copia y pendientes bajo conciliación |
| Dispositivo perdido | Revocar enrolamiento, retener cupos inciertos y reconstruir desde recibos/backups disponibles |
| Fallo de actualización | Mantener datos/cola; volver solo a binario compatible o corregir hacia adelante |
| Restauración de servidor | Congelar escrituras, recuperar backup, conciliar recibos/operaciones desde terminales y reanudar controladamente |

Objetivo de recuperación propuesto: RTO de 2 horas para recuperar una caja con backup y personal disponible; debe ensayarse. Para operaciones ya replicadas, buscar RPO de servidor de hasta 15 minutos mediante su estrategia de backup; aún requiere acuerdo y prueba. Una caída de proceso local no debe perder commits confirmados. Una pérdida física de equipo puede perder toda su ventana sin sincronizar ni respaldo externo: el RPO offline depende de esa ventana, no es cero por diseño.

## Puerta de producción

No habilitar venta distribuida hasta completar pruebas financieras, de concurrencia, sync y recuperación; conciliación de datos de origen; lector y UX reales; paquetes firmados; esquema/protocolo compatibles; soporte operativo y política de cupos/permiso/devolución aceptada. La aprobación es sobre el piloto y sus resultados, no sobre una promesa de portabilidad.
