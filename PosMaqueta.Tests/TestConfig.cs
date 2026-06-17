using Xunit;

// Los tests de integración comparten ConfigBD.CadenaConexion (estado global) y el estado estático
// de VentaService, así que la suite corre en SERIE para evitar carreras entre clases.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
