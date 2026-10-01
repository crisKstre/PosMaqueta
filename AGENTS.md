# Contexto de trabajo

- Comunicarse con el usuario en español.
- Para retomar la migración a .NET MAUI, leer primero [la memoria de continuidad](docs/migracion-maui/CONTINUIDAD.md) y después los documentos de la etapa correspondiente.
- La estrategia acordada es crear una aplicación MAUI nueva dentro de este repositorio y extraer, corregir y reutilizar progresivamente el código útil del proyecto actual. Evitar una refactorización general previa de WinForms.
- Distinguir acuerdos del usuario, propuestas pendientes y resultados efectivamente comprobados. La memoria contiene esa separación y el punto de reanudación.
- Al cerrar una sesión de migración, actualizar la memoria con los cambios, verificaciones, decisiones y siguiente paso. Contrastar siempre su estado con el código y Git actuales.
