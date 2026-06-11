namespace IndexadorAutomaticoPlanos.Entities
{
    /// <summary>
    /// Entidad que representa un lote de archivos a procesar
    /// </summary>
    public class Lote
    {
        public int CdLote { get; set; }
        public string DsNombreLote { get; set; } = string.Empty;
        public string? DsCarpetaOrigen { get; set; }
        public int NuCantidadArchivos { get; set; }
        public int CdEstadoLote { get; set; }
        public DateTime FeAlta { get; set; }
        public int CdUsuarioAlta { get; set; }
        public DateTime? FeUltimaModificacion { get; set; }
        public int? CdUsuarioModificacion { get; set; }

        // Propiedades de navegación (no mapeadas directamente)
        public string? DsEstadoLote { get; set; }
        public int CantidadArchivos { get; set; }
    }
}
