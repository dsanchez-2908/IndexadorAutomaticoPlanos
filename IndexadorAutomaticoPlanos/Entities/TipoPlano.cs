namespace IndexadorAutomaticoPlanos.Entities
{
    /// <summary>
    /// Entidad que representa un tipo de plano
    /// </summary>
    public class TipoPlano
    {
        public int CdTipoPlano { get; set; }
        public string DsTipoPlano { get; set; } = string.Empty;
        public bool SnActivo { get; set; }
    }
}
