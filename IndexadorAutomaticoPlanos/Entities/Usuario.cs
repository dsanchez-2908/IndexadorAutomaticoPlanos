namespace IndexadorAutomaticoPlanos.Entities
{
    /// <summary>
    /// Entidad que representa un usuario del sistema
    /// </summary>
    public class Usuario
    {
        public int CdUsuario { get; set; }
        public string DsUsuario { get; set; } = string.Empty;
        public string DsClave { get; set; } = string.Empty;
        public string DsNombreCompleto { get; set; } = string.Empty;
        public bool SnClaveTemporal { get; set; }
        public bool SnPrimerIngreso { get; set; }
        public bool SnActivo { get; set; }
        public DateTime FeAlta { get; set; }
        public int? CdUsuarioAlta { get; set; }
        public DateTime? FeUltimaModificacion { get; set; }
        public int? CdUsuarioModificacion { get; set; }
    }
}
