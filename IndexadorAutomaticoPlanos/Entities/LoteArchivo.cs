namespace IndexadorAutomaticoPlanos.Entities
{
    /// <summary>
    /// Entidad que representa la relación entre un lote y un archivo PDF
    /// </summary>
    public class LoteArchivo
    {
        public int CdLoteArchivo { get; set; }
        public int CdLote { get; set; }
        public int CdArchivo { get; set; }
        public int CdEstadoArchivoLote { get; set; }
        public int? NuOrden { get; set; }
        public DateTime FeAlta { get; set; }
        public int CdUsuarioAlta { get; set; }
        public DateTime? FeUltimaModificacion { get; set; }
        public int? CdUsuarioModificacion { get; set; }

        // Propiedades de procesamiento de imágenes (Fase 4)
        public string? DsRutaImagenJpg { get; set; }
        public string? TxImagenBase64 { get; set; }
        public string? TxResultadoOcr { get; set; }
        public bool SnTieneOcr { get; set; }
        public int? NuDpiProcesamiento { get; set; }
        public string? DsEsquinaRecorte { get; set; }
        public decimal? NuPorcentajeRecorteHorizontal { get; set; }
        public decimal? NuPorcentajeRecorteVertical { get; set; }

        // Propiedades de navegación (no mapeadas directamente)
        public string? DsEstadoArchivoLote { get; set; }
        public string? DsNombreArchivo { get; set; }
        public string? DsRutaCompletaArchivo { get; set; }
        public string? DsNombreLote { get; set; }
        public int? NuCantidadPaginas { get; set; }
        public string? DsNombreUltimaCarpeta { get; set; }
    }
}
