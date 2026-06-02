namespace IndexadorAutomaticoPlanos.Entities
{
    /// <summary>
    /// Entidad que representa la relación entre un lote y un archivo
    /// Contiene información de procesamiento de imágenes
    /// </summary>
    public class LoteArchivo
    {
        public int CdLoteArchivo { get; set; }
        public int CdLote { get; set; }
        public int CdArchivo { get; set; }
        public int CdEstadoArchivoLote { get; set; }
        public string? DsRutaImagenJPG { get; set; }
        public string? DsImagenBase64 { get; set; }
        public string? DsTextoOCR { get; set; }
        public bool SnTieneOCR { get; set; }
        public DateTime FeAlta { get; set; }
        public int CdUsuarioAlta { get; set; }
        public DateTime? FeUltimaModificacion { get; set; }
        public int? CdUsuarioModificacion { get; set; }

        // Propiedades de navegación (no mapeadas directamente)
        public string? DsEstadoArchivoLote { get; set; }
        public string? DsNombreArchivo { get; set; }
        public string? DsRutaCompletaArchivo { get; set; }
        public string? DsNombreLote { get; set; }
    }
}
