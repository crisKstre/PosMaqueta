# 06 · Decisiones de arquitectura y fuentes

Fecha de consulta: **2026-09-24**. [Índice](README.md).

## Registro de decisiones

Las decisiones técnicas son propuestas para revisión en etapa 0, no implementaciones ya realizadas. El usuario confirmó plataformas, necesidad de operación offline compartida y la estrategia de crear una aplicación MAUI nueva en el mismo repositorio con extracción y corrección gradual del código existente (ADR-016). Los detalles operativos marcados como pendientes siguen sin confirmar.

| ID | Estado | Decisión | Motivo y costo | Cuándo revisarla |
|---|---|---|---|---|
| ADR-001 | Alcance confirmado | Windows y Android primero | Concentra certificación en dispositivos solicitados | Si se requiere iOS/macOS/web |
| ADR-002 | Alcance confirmado | Varias terminales con escritura offline | Obliga a persistencia local y conciliación distribuida | Si se admite requerir conexión para cobrar |
| ADR-003 | Propuesta | MAUI 10, XAML y MVVM Toolkit | Encaje con C#, UI nativa y pruebas; implica reescribir WinForms | Tras prototipo de UX/periféricos o cambio a estrategia web |
| ADR-004 | Propuesta | Núcleo independiente + puertos en Application | Reutilizar reglas sin SQL/UI ni sesión global; requiere refactor | Si el prototipo demuestra una separación más simple equivalente |
| ADR-005 | Propuesta | SQLite/ADO.NET local inicialmente | Aprovecha SQL y pruebas; exige trabajo controlado fuera de UI | Si mediciones justifican ORM o proveedor diferente |
| ADR-006 | Propuesta | API modular y SQL Server central | Retira acceso SQL de clientes; una autoridad de consolidación | Por necesidades operativas reales, no por expectativa de escala |
| ADR-007 | Propuesta | Outbox/inbox e IDs estables | Reintentos sin duplicar; más tablas, monitoreo y pruebas | No omitir mientras existan escrituras desconectadas |
| ADR-008 | Por validar con negocio | Cupos de stock por terminal | Evita sobreventa por partición; limita flexibilidad offline | Si se acepta sobreventa y conciliación posterior |
| ADR-009 | Por validar con negocio | Permiso offline limitado; primeras sesiones online | Reduce exposición de credenciales; no garantiza revocación inmediata | Según duración real de cortes y modelo de terminal administrado |
| ADR-010 | Por validar con negocio | Devolución offline solo propia y no publicada | Evita doble devolución distribuida; restringe operación | Si se desarrollan derechos de devolución reservados |
| ADR-011 | Regla previa conservada | Reembolso en efectivo; DTE externo | Decisión explícita en docs del proyecto | Solo por nueva decisión del negocio |
| ADR-012 | Propuesta | Venta original inmutable y correcciones separadas | Soluciona ambigüedad del reporte; exige conciliación del legado | Al acordar el modelo financiero en etapa 0 |
| ADR-013 | Propuesta | Puente netstandard2.0 para reglas compartidas | Permite convivencia con net472 sin bloquear MAUI | Retirarlo al dar de baja WinForms |
| ADR-014 | Propuesta | Corte por instalación sin dos escritores incompatibles | Evita inventarios divergentes y doble importación | Si se financia un adaptador legacy al protocolo |
| ADR-015 | Propuesta | Release firmado y actualización compatible | Hardware, trimming y datos requieren validación real | Por cambios de canal/dispositivos, conservando compatibilidad |
| ADR-016 | Estrategia confirmada | Nueva aplicación MAUI en el mismo repositorio + extracción y corrección gradual del código útil | Rehacer UI y capacidades nuevas aprovechando reglas y pruebas; evitar refactorizar todo WinForms previamente | Si cambia el alcance o aparecen impedimentos demostrados durante los prototipos |

No se recomiendan MediatR, un bus de mensajes, repositorios genéricos, event sourcing completo, Kubernetes ni un servicio por módulo como condición inicial. Ninguna de esas herramientas sustituye las invariantes transaccionales definidas para este POS.

## Base documental del repositorio

| Referencia | Uso en el análisis |
|---|---|
| [README actual](../../README.md) | Funciones declaradas y entorno legacy; contrastado con código |
| [Arquitectura actual](../ARQUITECTURA.md) | Capas, flujos y convenciones existentes |
| [Modelo actual](../MODELO-DATOS.md) | Tablas y tipos; cotejados con inicializador y DAOs |
| [Despliegue actual](../DESPLIEGUE.md) | SQLite individual y SQL Server LAN |
| [Manual actual](../MANUAL-USUARIO.md) | Operaciones y vocabulario del cajero |
| [Roadmap previo](../ROADMAP-PRODUCCION.md) | Pendientes de integridad, multicaja y operación |
| [Revisión previa](../CORRECCIONES-REVISION.md) | Hipótesis y correcciones históricas; no asumidas como estado actual |
| [Decisiones del dueño y correcciones](../RESPUESTA-CORRECCIONES.md) | Reembolsos en efectivo, DTE externo y alcance anterior |

La petición actual de MAUI y sincronización cambia el motivo de aplazar multicaja/migración, pero no invalida otras reglas del negocio. Los resultados históricos de pruebas no sustituyen la [ejecución de esta sesión](05-VALIDACION-Y-OPERACION.md).

## Fuentes primarias consultadas

| Fuente oficial | Qué respalda | Aplicación a esta propuesta |
|---|---|---|
| [.NET MAUI: política de soporte](https://dotnet.microsoft.com/en-us/platform/support/policy/maui) | Versiones y fechas de soporte | MAUI 10 está soportado; MAUI 9 finalizó el 12-05-2026; revisar mantenimiento en cada hito |
| [Plataformas soportadas](https://learn.microsoft.com/en-us/dotnet/maui/supported-platforms?view=net-maui-10.0) | Destinos y herramientas por plataforma | Windows/Android primero; Apple requiere trabajo de certificación propio |
| [MVVM](https://learn.microsoft.com/en-us/dotnet/architecture/maui/mvvm) | Separación vista/modelo de presentación y comandos | Rehacer formularios como páginas/ViewModels comprobables |
| [MVVM Toolkit](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) | Objetos observables y comandos | Reducir código repetitivo sin imponer otro framework de aplicación |
| [Inyección de dependencias MAUI](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/dependency-injection?view=net-maui-10.0) | Contenedor y ciclos de vida | Composición explícita; scopes de sesión definidos por la app |
| [Blazor Hybrid](https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/?view=aspnetcore-10.0) | Ejecución nativa y presentación en WebView | Alternativa evaluada; no elegida por falta de UI web reutilizable |
| [.NET Standard](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0) | Compatibilidad entre implementaciones .NET | Puente de bibliotecas puras para WinForms 4.7.2 |
| [SQLite en MAUI](https://learn.microsoft.com/en-us/dotnet/maui/data-cloud/database-sqlite?view=net-maui-10.0) | Persistencia local y alternativa Microsoft.Data.Sqlite | Mantener proveedor inicialmente detrás de puertos |
| [Límites async de Microsoft.Data.Sqlite](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/async) | Métodos async ejecutan de forma síncrona | Trabajador local y transacciones cortas; no bloquear UI |
| [Backup online de SQLite](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/backup) | API de snapshot de base SQLite | Sustituir copia de archivo expuesta a escrituras |
| [SQLite PRAGMA synchronous](https://www.sqlite.org/pragma.html#pragma_synchronous) | Durabilidad según modo de sincronización | Medir `FULL` para confirmaciones locales del POS |
| [Microsoft.Data.SqlClient](https://learn.microsoft.com/en-us/sql/connect/ado-net/introduction-microsoft-data-sqlclient-namespace?view=sql-server-ver17) | Proveedor SQL Server y compatibilidad | Adaptador central moderno; no dependencia móvil |
| [Diseño offline en Android](https://developer.android.com/topic/architecture/data-layer/offline-first) | Fuente local, red, colas y conflictos | Orientación conceptual para la réplica local MAUI |
| [Transactional outbox](https://learn.microsoft.com/en-us/azure/architecture/databases/guide/transactional-out-box-cosmos) | Guardar cambio y evento en la misma transacción | Adaptación a SQLite/SQL Server; no se adopta Cosmos DB |
| [SecureStorage](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/secure-storage?view=net-maui-10.0) | Almacenamiento protegido y particularidades del backup Android | Tokens/material local protegidos; recuperación de identidad separada de datos |
| [Ciclo de vida](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/app-lifecycle?view=net-maui-10.0) | Eventos y estados de ventanas/apps | Persistir antes de suspensión y recuperar desde almacenamiento |
| [Bindings compilados](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/compiled-bindings?view=net-maui-10.0) | Validación de bindings y `x:DataType` | Detectar errores y reducir trabajo dinámico |
| [Trimming MAUI](https://learn.microsoft.com/en-us/dotnet/maui/deployment/trimming?view=net-maui-10.0) | Comportamiento y restricciones de publicación | Probar navegación, serialización y paquetes en Release |
| [Publicación Windows](https://learn.microsoft.com/en-us/dotnet/maui/windows/deployment/overview?view=net-maui-10.0) | Modalidades de despliegue | Elegir instalación firmada y ensayar actualización |
| [Publicación Android por CLI](https://learn.microsoft.com/en-us/dotnet/maui/android/deployment/publish-cli?view=net-maui-10.0) | Empaquetado y firma | Custodia de clave y canal interno de piloto |
| [Trabajo en segundo plano Android](https://developer.android.com/develop/background-work/background-tasks) | Restricciones y opciones del sistema | No depender de temporizadores en memoria para sincronización |

Las fuentes fundamentan capacidades y límites de las tecnologías. La división de proyectos, cupos, política de devolución, etapas, estimaciones y metas de rendimiento son decisiones de ingeniería de esta propuesta, no recomendaciones textuales de Microsoft ni resultados medidos. Las páginas Learn pueden presentar varias versiones en el contenido extraído: comprobar el selector de versión y los requisitos del workload que efectivamente se fije.

## Seguimiento de vigencia

Antes de iniciar y en cada hito: verificar versión soportada de MAUI, SDK/workload, compatibilidad de proveedores nativos y periféricos, Android SDK/JDK, Windows SDK y requisitos del canal de distribución. Si el calendario cruza el fin de soporte de MAUI 10, incorporar la actualización a la versión estable soportada y repetir pruebas de dispositivo; no mantener MAUI 10 solo porque el núcleo siga en .NET 10.
