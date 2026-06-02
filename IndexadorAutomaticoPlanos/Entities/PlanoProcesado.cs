namespace IndexadorAutomaticoPlanos.Entities
{
    /// <summary>
    /// Entidad que representa un plano procesado por OpenAI
    /// </summary>
    public class PlanoProcesado
    {
        public int CdPlano { get; set; }
        public int CdLoteArchivo { get; set; }
        public int? CdTipoPlano { get; set; }
        public string? DsExpediente { get; set; }
        public string? DsSeccion { get; set; }
        public string? DsManzana { get; set; }
        public string? DsParcela { get; set; }
        public string? DsDireccion { get; set; }
        public decimal? NuConfianzaTipoPlano { get; set; }
        public decimal? NuConfianzaExpediente { get; set; }
        public decimal? NuConfianzaSeccion { get; set; }
        public decimal? NuConfianzaManzana { get; set; }
        public decimal? NuConfianzaParcela { get; set; }
        public decimal? NuConfianzaDireccion { get; set; }
        public int? NuTokensPrompt { get; set; }
        public int? NuTokensCompletion { get; set; }
        public int? NuTokensTotal { get; set; }
        public int CdEstadoValidacion { get; set; }
        public string? DsObservaciones { get; set; }
        public DateTime FeAlta { get; set; }
        public int CdUsuarioAlta { get; set; }
        public DateTime? FeUltimaModificacion { get; set; }
        public int? CdUsuarioModificacion { get; set; }

        // Propiedades de navegación (no mapeadas directamente)
        public string? DsTipoPlano { get; set; }
        public string? DsEstadoValidacion { get; set; }
        public string? DsNombreArchivo { get; set; }
        public string? DsRutaImagenJPG { get; set; }
    }
}
