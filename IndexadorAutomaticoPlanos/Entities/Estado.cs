namespace IndexadorAutomaticoPlanos.Entities
{
    /// <summary>
    /// Entidad que representa un estado genérico del sistema
    /// </summary>
    public class Estado
    {
        public int CdEstado { get; set; }
        public string DsEstado { get; set; } = string.Empty;
        public string? DsDescripcion { get; set; }
        public bool SnActivo { get; set; }
    }
}
