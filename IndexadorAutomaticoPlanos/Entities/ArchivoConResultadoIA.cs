namespace IndexadorAutomaticoPlanos.Entities
{
    /// <summary>
    /// Entidad que combina un archivo de lote con su resultado de IA
    /// Utilizada para control de calidad
    /// </summary>
    public class ArchivoConResultadoIA
    {
        // Datos del archivo en lote
        public int CdLoteArchivo { get; set; }
        public int CdLote { get; set; }
        public int CdArchivo { get; set; }
        public int CdEstadoArchivoLote { get; set; }
        public int? NuOrden { get; set; }
        public string? DsRutaImagenJpg { get; set; }
        public string? DsNombreArchivo { get; set; }
        public string? DsEstadoArchivoLote { get; set; }

        // Resultado de IA (puede ser null si no fue procesado)
        public int? CdResultadoIA { get; set; }
        public string? DsTipoPlano { get; set; }
        public string? DsExpediente { get; set; }
        public string? DsSeccion { get; set; }
        public string? DsManzana { get; set; }
        public string? DsParcela { get; set; }
        public string? DsDireccion { get; set; }

        // Confianzas (porcentajes)
        public decimal? NuConfianzaTipoPlano { get; set; }
        public decimal? NuConfianzaExpediente { get; set; }
        public decimal? NuConfianzaSeccion { get; set; }
        public decimal? NuConfianzaManzana { get; set; }
        public decimal? NuConfianzaParcela { get; set; }
        public decimal? NuConfianzaDireccion { get; set; }

        // Metadatos de procesamiento
        public string? DsModalidadProcesamiento { get; set; }
        public int? NuIntentos { get; set; }

        /// <summary>
        /// Verifica si el archivo tiene campos obligatorios faltantes
        /// </summary>
        public bool TieneCamposFaltantes()
        {
            return string.IsNullOrWhiteSpace(DsTipoPlano) ||
                   string.IsNullOrWhiteSpace(DsSeccion) ||
                   string.IsNullOrWhiteSpace(DsManzana) ||
                   string.IsNullOrWhiteSpace(DsParcela);
        }

        /// <summary>
        /// Obtiene el menor porcentaje de confianza de los campos obligatorios
        /// </summary>
        public decimal? ObtenerMenorConfianza()
        {
            var confianzas = new List<decimal?>
            {
                NuConfianzaTipoPlano,
                NuConfianzaSeccion,
                NuConfianzaManzana,
                NuConfianzaParcela
            }.Where(c => c.HasValue).Select(c => c!.Value).ToList();

            return confianzas.Any() ? confianzas.Min() : null;
        }
    }
}
