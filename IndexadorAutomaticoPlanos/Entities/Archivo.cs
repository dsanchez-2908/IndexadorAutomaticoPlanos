namespace IndexadorAutomaticoPlanos.Entities
{
    /// <summary>
    /// Entidad que representa un archivo PDF ingresado al sistema
    /// </summary>
    public class Archivo
    {
        public int CdArchivo { get; set; }
        public string DsNombreArchivo { get; set; } = string.Empty;
        public string DsRutaCompleta { get; set; } = string.Empty;
        public string? DsNombreUltimaCarpeta { get; set; }
        public long NuTamanoBytes { get; set; }
        public int NuCantidadPaginas { get; set; }
        public DateTime FeModificacionArchivo { get; set; }
        public int CdEstadoArchivo { get; set; }
        public DateTime FeAlta { get; set; }
        public int CdUsuarioAlta { get; set; }
        public DateTime? FeUltimaModificacion { get; set; }
        public int? CdUsuarioModificacion { get; set; }

        // Propiedades de navegación (no mapeadas directamente)
        public string? DsEstadoArchivo { get; set; }

        // Propiedades calculadas para UI
        public string RutaCompletaConNombre => System.IO.Path.Combine(DsRutaCompleta, DsNombreArchivo);

        public string TamanoLegible
        {
            get
            {
                string[] sizes = { "B", "KB", "MB", "GB" };
                double len = NuTamanoBytes;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }
                return $"{len:0.##} {sizes[order]}";
            }
        }
    }
}
