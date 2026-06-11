namespace IndexadorAutomaticoPlanos.Entities
{
    /// <summary>
    /// Entidad que representa un archivo de lote con todos sus metadatos
    /// para el proceso de finalización (renombrado y generación de índice)
    /// </summary>
    public class ArchivoParaFinalizar
    {
        // Datos de LoteArchivo
        public int CdLoteArchivo { get; set; }
        public int CdArchivo { get; set; }
        public int CdEstadoArchivoLote { get; set; }
        public string DsEstadoArchivoLote { get; set; } = string.Empty;

        // Datos del Archivo
        public string DsNombreArchivo { get; set; } = string.Empty;
        public string DsRutaCompleta { get; set; } = string.Empty;
        public string DsNombreUltimaCarpeta { get; set; } = string.Empty;

        // Datos del Lote
        public string DsNombreLote { get; set; } = string.Empty;

        // Datos de ResultadoIA (pueden ser null si no hay resultado)
        public int? CdResultadoIA { get; set; }
        public string? DsTipoPlano { get; set; }
        public string? DsExpediente { get; set; }
        public string? DsSeccion { get; set; }
        public string? DsManzana { get; set; }
        public string? DsParcela { get; set; }
        public string? DsDireccion { get; set; }

        /// <summary>
        /// Indica si el archivo está marcado como "Carátula Ilegible" (Estado 6)
        /// </summary>
        public bool EsIlegible => CdEstadoArchivoLote == EstadosArchivoLote.CaratulaIlegible;

        /// <summary>
        /// Verifica si el archivo tiene los datos mínimos necesarios para renombrar
        /// </summary>
        public bool TieneDatosCompletos()
        {
            if (EsIlegible) return true; // Los ilegibles siempre se pueden procesar

            // Verificar datos obligatorios
            return !string.IsNullOrWhiteSpace(DsTipoPlano) &&
                   !string.IsNullOrWhiteSpace(DsSeccion) &&
                   !string.IsNullOrWhiteSpace(DsManzana) &&
                   !string.IsNullOrWhiteSpace(DsParcela);
        }

        /// <summary>
        /// Obtiene la nomenclatura (Seccion_Manzana_Parcela)
        /// </summary>
        public string ObtenerNomenclatura()
        {
            if (string.IsNullOrWhiteSpace(DsSeccion) ||
                string.IsNullOrWhiteSpace(DsManzana) ||
                string.IsNullOrWhiteSpace(DsParcela))
            {
                return "SIN_NOMENCLATURA";
            }

            return $"{DsSeccion}_{DsManzana}_{DsParcela}";
        }
    }
}
