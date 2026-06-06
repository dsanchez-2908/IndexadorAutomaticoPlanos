namespace IndexadorAutomaticoPlanos.Entities
{
    /// <summary>
    /// Entidad que representa el resultado del procesamiento de un archivo por OpenAI
    /// Mapea la tabla IAP_TD_RESULTADOS_IA
    /// </summary>
    public class ResultadoIA
    {
        // Identificadores
        public int CdResultadoIA { get; set; }
        public int CdLoteArchivo { get; set; }

        // Campos extraídos del plano
        public string? TxNombreArchivo { get; set; }
        public string? DsTipoPlano { get; set; }
        public string? DsExpediente { get; set; }
        public string? DsSeccion { get; set; }
        public string? DsManzana { get; set; }
        public string? DsParcela { get; set; }
        public string? DsDireccion { get; set; }

        // Niveles de confianza (0.00 a 1.00)
        public decimal? NuConfianzaTipoPlano { get; set; }
        public decimal? NuConfianzaExpediente { get; set; }
        public decimal? NuConfianzaSeccion { get; set; }
        public decimal? NuConfianzaManzana { get; set; }
        public decimal? NuConfianzaParcela { get; set; }
        public decimal? NuConfianzaDireccion { get; set; }

        // Tokens consumidos
        public int? NuPromptTokens { get; set; }
        public int? NuCompletionTokens { get; set; }
        public int? NuTotalTokens { get; set; }

        // Metadata de procesamiento
        public string? DsModalidadProcesamiento { get; set; } // 'OCR', 'Imagen', 'Hibrido'
        public int NuIntentos { get; set; } = 1;
        public string? TxRespuestaCompleta { get; set; } // JSON completo de respuesta
        public string? TxMensajeError { get; set; }

        // Auditoría
        public DateTime FeAlta { get; set; }
        public int CdUsuarioAlta { get; set; }
        public DateTime? FeUltimaModificacion { get; set; }
        public int? CdUsuarioModificacion { get; set; }

        // Propiedades de navegación (no mapeadas directamente)
        public string? DsNombreLote { get; set; }
        public string? DsEstadoArchivoLote { get; set; }
    }

    /// <summary>
    /// Clase para deserializar la respuesta JSON de OpenAI
    /// Estructura esperada de la respuesta del modelo
    /// </summary>
    public class RespuestaOpenAI
    {
        public string? Archivo { get; set; }
        public string? TipoPlano { get; set; }
        public string? Expediente { get; set; }
        public string? Seccion { get; set; }
        public string? Manzana { get; set; }
        public string? Parcela { get; set; }
        public string? Direccion { get; set; }
        public ConfianzaRespuesta? Confianza { get; set; }
    }

    /// <summary>
    /// Clase para deserializar el objeto de confianza en la respuesta JSON
    /// </summary>
    public class ConfianzaRespuesta
    {
        public decimal TipoPlano { get; set; }
        public decimal Expediente { get; set; }
        public decimal Seccion { get; set; }
        public decimal Manzana { get; set; }
        public decimal Parcela { get; set; }
        public decimal Direccion { get; set; }
    }

    /// <summary>
    /// Clase para representar el uso de tokens devuelto por OpenAI
    /// </summary>
    public class UsageTokens
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }
}
