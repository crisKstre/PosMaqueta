# Migración a .NET MAUI: propuesta técnica

**Fecha:** 24 de septiembre de 2026. **Base analizada:** commit `5d05101`.
**Estado al 01-10-2026:** correcciones preparatorias MIG-001/MIG-002/H16 y H03 local verificadas, con 215 pruebas correctas y WinForms compilado. Próximo paso técnico: etapa 1/MAUI. Histórico, concurrencia y validaciones de etapa 0 pendientes; aún no hay implementación MAUI.

**Para retomar:** leer la [memoria de continuidad](CONTINUIDAD.md). El usuario acordó crear una aplicación MAUI nueva en este mismo repositorio y extraer, corregir y reutilizar progresivamente el código útil, sin refactorizar todo WinForms de antemano.

## Recomendación

Migrar de forma incremental a **.NET MAUI con XAML y MVVM**, un núcleo de negocio independiente de la interfaz, **SQLite en cada terminal** y una **API ASP.NET Core central** para consolidar y sincronizar datos. Mantener el servidor como un monolito modular: este POS no necesita microservicios.

El usuario confirmó **Windows y Android** y **varios dispositivos sincronizados que puedan trabajar sin conexión**. Por tanto, la sincronización es parte del alcance final, aunque se entregue después de estabilizar la venta local. Cambiar WinForms por MAUI sin rediseñar la persistencia no cumple ese objetivo.

Se recomienda partir de **.NET 10 / MAUI 10**, con el nivel de mantenimiento vigente al implementar. MAUI 10 tiene fin de soporte publicado para el **11 de mayo de 2027**; su ciclo no equivale al soporte LTS del runtime. El plan debe incluir actualizaciones de MAUI durante la migración. [Política oficial de soporte](https://dotnet.microsoft.com/en-us/platform/support/policy/maui).

## Documentos

| Documento | Qué permite decidir o implementar |
|---|---|
| [01 · Diagnóstico](01-DIAGNOSTICO.md) | Inventario, dependencias, reutilización, problemas actuales y evidencia |
| [02 · Arquitectura](02-ARQUITECTURA.md) | Proyectos, MVVM, inyección de dependencias, navegación, seguridad y adaptadores |
| [03 · Datos y sincronización](03-DATOS-Y-SINCRONIZACION.md) | Contrato offline, stock, transacciones, API, migración de datos y recuperación |
| [04 · Plan por etapas](04-PLAN-POR-ETAPAS.md) | Entregables, dependencias, esfuerzo orientativo, puertas de salida y reversión |
| [05 · Validación y operación](05-VALIDACION-Y-OPERACION.md) | Resultado de pruebas actuales, matriz futura, CI, despliegue y soporte |
| [06 · Decisiones y fuentes](06-DECISIONES-Y-FUENTES.md) | Alternativas evaluadas, decisiones de arquitectura y bibliografía oficial |
| [07 · Etapa 0: devoluciones](07-ETAPA-0-DEVOLUCIONES.md) | Corrección aplicada, reglas por fecha/turno, conciliación del histórico y pendientes |
| [08 · Etapa 0: descuentos](08-ETAPA-0-DESCUENTOS.md) | Reparto, redondeo acumulado, rechazo de repetidos y vista previa común |
| [09 · Etapa 0: costos y reportes](09-ETAPA-0-COSTOS-Y-REPORTES.md) | Costo neto, utilidad/margen y rankings por fecha de devolución |
| [10 · Confirmación de venta](10-ETAPA-0-CONFIRMACION-VENTA.md) | Auditoría atómica, avisos aislados y paso técnico a etapa 1 |

## Conclusiones del análisis inicial (24-09-2026)

- Hay **5 proyectos, 89 archivos C# y 13.696 líneas físicas**, incluidas pruebas y diseñadores, excluidos `bin/obj`. La interfaz suma 7.327 líneas: el esfuerzo principal no es cambiar el framework de destino.
- Las reglas de CLP, IVA incluido, descuentos, pagos mixtos y costos históricos son una base aprovechable. La interfaz WinForms se reconstruye; los servicios y DAOs requieren refactorización.
- `Dominio` depende de `AccesoData`; los servicios crean DAOs concretos y comparten sesión/carritos estáticos. Es necesario invertir esas dependencias.
- Las pruebas ejecutadas dieron **172 correctas y 1 fallida, de 173**. Las 9 pruebas de SQL Server se excluyeron expresamente; no se cuentan como aprobadas ni omitidas por el ejecutor.
- La devolución modifica `Venta.Total` y el resumen vuelve a restarla. Debe corregirse y reconciliarse el histórico antes de usarlo como referencia de migración.
- El esquema actual representa una caja abierta global. No identifica terminales operativas ni tiene identidad global, cursor de cambios o idempotencia de ventas.

## Alcance y reglas conservadas

Se incluyen todos los módulos actuales: acceso, usuarios, catálogo/categorías, importación CSV, ventas simultáneas, cobro, caja/arqueo, reportes, anulaciones/devoluciones, auditoría y respaldos. La fase final agrega enrolamiento de dispositivos, sincronización y seguimiento de incidencias.

Se conservan las decisiones documentadas en [RESPUESTA-CORRECCIONES.md](../RESPUESTA-CORRECCIONES.md): devolución en efectivo y boleta/DTE emitida por una máquina aparte. La sincronización del registro POS **no acredita un pago bancario** ni hace que el terminal de tarjetas opere offline.

iOS y macOS quedan como extensiones posteriores. MAUI admite Android, iOS, Windows y macOS mediante Mac Catalyst; Linux y navegador no son destinos oficiales de esta aplicación MAUI. El Windows 7 mencionado en el README actual no puede mantenerse como destino MAUI. [Plataformas oficiales](https://learn.microsoft.com/en-us/dotnet/maui/supported-platforms?view=net-maui-10.0).

## Decisiones operativas aún por validar

La propuesta puede prepararse sin bloquear el análisis, pero estas decisiones condicionan el piloto:

| Decisión | Base propuesta | Consecuencia |
|---|---|---|
| Stock sin conexión | Cupos de venta por dispositivo/producto | Evita vender la misma última unidad en dos terminales; puede bloquear una venta aunque otra terminal tenga cupo |
| Tiempo máximo offline | Permiso offline renovable, provisionalmente 24 horas | Revocaciones no son instantáneas durante una desconexión; duración a validar con el negocio |
| Devoluciones offline | Solo ventas propias aún no publicadas; las demás requieren servidor | Evita doble reembolso entre terminales; ampliar exige reservas de derechos de devolución |
| Servidor | Una API con una base central por instalación; alojamiento por decidir | Puede estar en el local o alojada externamente; no se propone replicación entre varios servidores |
| Dispositivos | Windows 11 x64 y Android 10+ arm64 como matriz inicial de producto | Son objetivos de certificación propuestos, no mínimos técnicos del framework |
| Periféricos | HID primero; cámara/impresora según equipos reales | Hay que registrar modelos, interfaces y SDK antes del compromiso de compatibilidad |

No se puede prometer venta ilimitada offline, stock global siempre exacto y ausencia total de sobreventa a la vez. La política de cupos propuesta hace explícita esa limitación.

## Orden recomendado

1. Estabilizar reglas y pruebas; prototipar SQLite, periféricos y sincronización en Windows/Android.
2. Extraer el núcleo y contratos reutilizables mientras WinForms sigue operativo.
3. Entregar una sección completa de venta local MAUI y después la paridad funcional.
4. Integrar API, identidad de terminales y sincronización con pruebas de cortes/reintentos.
5. Migrar una instalación piloto con conciliación y avanzar gradualmente.

El detalle de estimaciones y criterios de aceptación está en el [plan](04-PLAN-POR-ETAPAS.md). No se propone una sustitución completa en un único despliegue.
