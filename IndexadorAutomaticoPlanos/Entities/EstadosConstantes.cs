namespace IndexadorAutomaticoPlanos.Entities
{
    /// <summary>
    /// Constantes para los códigos de estados de lote
    /// IMPORTANTE: Estos valores deben coincidir con la tabla IAP_TV_ESTADOS_LOTE
    /// </summary>
    public static class EstadosLote
    {
        /// <summary>
        /// Estado 1: Lote creado, pendiente de preparar las imágenes
        /// </summary>
        public const int PendientePreparar = 1;

        /// <summary>
        /// Estado 2: Lote listo para envío a OpenAI
        /// </summary>
        public const int PendienteProcesarIA = 2;

        /// <summary>
        /// Estado 3: Procesado por IA, pendiente de control de calidad
        /// </summary>
        public const int PendienteControlCalidad = 3;

        /// <summary>
        /// Estado 4: Lote controlado y pendiente de finalización
        /// </summary>
        public const int PendienteFinalizar = 4;

        /// <summary>
        /// Estado 5: Lote finalizado y archivos renombrados
        /// </summary>
        public const int Finalizado = 5;

        /// <summary>
        /// Estado 6: Lote con errores durante el procesamiento
        /// </summary>
        public const int ConError = 6;
    }

    /// <summary>
    /// Constantes para los códigos de estados de archivo de lote
    /// IMPORTANTE: Estos valores deben coincidir con la tabla IAP_TV_ESTADOS_ARCHIVO_LOTE
    /// </summary>
    public static class EstadosArchivoLote
    {
        /// <summary>
        /// Estado 1: Archivo pendiente de preparar imagen
        /// </summary>
        public const int PendientePreparar = 1;

        /// <summary>
        /// Estado 2: Imagen preparada y lista para procesar
        /// </summary>
        public const int ImagenPreparada = 2;

        /// <summary>
        /// Estado 3: Procesado por IA
        /// </summary>
        public const int ProcesadoPorIA = 3;

        /// <summary>
        /// Estado 4: Pendiente de controlar
        /// </summary>
        public const int PendienteControlar = 4;

        /// <summary>
        /// Estado 5: Controlado (control de calidad aprobado)
        /// </summary>
        public const int Controlado = 5;

        /// <summary>
        /// Estado 6: Carátula Ilegible (no se pueden leer los datos)
        /// </summary>
        public const int CaratulaIlegible = 6;

        /// <summary>
        /// Estado 7: Con Error
        /// </summary>
        public const int ConError = 7;

        /// <summary>
        /// Estado 8: Asociado
        /// </summary>
        public const int Asociado = 8;

        /// <summary>
        /// Estado 9: Imagen Extraída
        /// </summary>
        public const int ImagenExtraida = 9;

        /// <summary>
        /// Estado 10: Enviado a IA
        /// </summary>
        public const int EnviadoIA = 10;

        /// <summary>
        /// Estado 11: Procesado
        /// </summary>
        public const int Procesado = 11;

        /// <summary>
        /// Estado 12: Error
        /// </summary>
        public const int Error = 12;

        /// <summary>
        /// Estado 13: Validado
        /// </summary>
        public const int Validado = 13;

        /// <summary>
        /// Estado 14: Finalizado
        /// </summary>
        public const int Finalizado = 14;
    }
}
