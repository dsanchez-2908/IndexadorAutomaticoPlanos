namespace IndexadorAutomaticoPlanos.Entities
{
    /// <summary>
    /// Entidad que representa una entrada de log
    /// </summary>
    public class LogEntry
    {
        public long CdLog { get; set; }
        public string DsNivel { get; set; } = string.Empty; // Info, Warning, Error, Debug
        public string? DsModulo { get; set; }
        public string DsMensaje { get; set; } = string.Empty;
        public string? DsExcepcion { get; set; }
        public int? CdUsuario { get; set; }
        public string? DsUsuario { get; set; }
        public DateTime FeRegistro { get; set; }
    }

    /// <summary>
    /// Enum para niveles de log
    /// </summary>
    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Debug
    }
}
