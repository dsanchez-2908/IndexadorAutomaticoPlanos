using System;

namespace IndexadorAutomaticoPlanos.Entities
{
    /// <summary>
    /// Entidad que representa un tipo de plano válido
    /// </summary>
    public class TipoPlano
    {
        public int CdTipoPlano { get; set; }
        public string DsTipoPlano { get; set; } = string.Empty;
        public string? DsDescripcion { get; set; }
        public bool SnActivo { get; set; }
        public DateTime FeAlta { get; set; }
    }
}
