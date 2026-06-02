namespace IndexadorAutomaticoPlanos.Entities
{
    /// <summary>
    /// Entidad que representa un parámetro de configuración del sistema
    /// </summary>
    public class Parametro
    {
        public int CdParametro { get; set; }
        public string DsClaveParametro { get; set; } = string.Empty;
        public string? DsValorParametro { get; set; }
        public string? DsDescripcion { get; set; }
        public DateTime FeUltimaModificacion { get; set; }
        public int? CdUsuarioModificacion { get; set; }
    }
}
