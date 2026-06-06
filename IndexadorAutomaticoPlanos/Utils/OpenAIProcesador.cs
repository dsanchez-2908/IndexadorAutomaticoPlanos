using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;
using Newtonsoft.Json;
using IndexadorAutomaticoPlanos.Entities;
using IndexadorAutomaticoPlanos.DataAccess;
using IndexadorAutomaticoPlanos.Utils;

namespace IndexadorAutomaticoPlanos.Utils
{
    /// <summary>
    /// Procesador para interactuar con la API de OpenAI
    /// Maneja procesamiento por OCR, imagen y modalidad híbrida
    /// </summary>
    public class OpenAIProcesador
    {
        private OpenAIClient? _cliente;
        private string? _modelo;
        private int _maxReintentos = 3;
        private readonly ParametroRepository _parametroRepo;

        public OpenAIProcesador()
        {
            _parametroRepo = new ParametroRepository();
            InicializarCliente();
        }

        /// <summary>
        /// Inicializa el cliente de OpenAI con la API key desde parametrización
        /// </summary>
        private void InicializarCliente()
        {
            try
            {
                // Obtener API key desde parametrización
                var parametroApiKey = _parametroRepo.ObtenerPorClave("OPENAI_API_KEY");
                if (parametroApiKey == null || string.IsNullOrWhiteSpace(parametroApiKey.DsValorParametro))
                {
                    throw new InvalidOperationException("No se encontró la API key de OpenAI en la parametrización");
                }

                string apiKey = parametroApiKey.DsValorParametro;

                // Obtener modelo
                var parametroModelo = _parametroRepo.ObtenerPorClave("OPENAI_MODELO");
                _modelo = parametroModelo?.DsValorParametro ?? "gpt-4o-mini";

                // Obtener máximo de reintentos
                var parametroReintentos = _parametroRepo.ObtenerPorClave("OPENAI_MAX_REINTENTOS");
                if (parametroReintentos != null && int.TryParse(parametroReintentos.DsValorParametro, out int reintentos))
                {
                    _maxReintentos = reintentos;
                }

                // Inicializar cliente
                _cliente = new OpenAIClient(apiKey);

                Logger.Info($"Cliente OpenAI inicializado correctamente. Modelo: {_modelo}", "OpenAIProcesador");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al inicializar cliente de OpenAI: {ex.Message}", ex, "OpenAIProcesador");
                throw;
            }
        }

        /// <summary>
        /// Procesa un archivo usando modalidad híbrida: intenta primero con OCR, luego con imagen
        /// </summary>
        public async Task<ResultadoProcesamientoIA> ProcesarHibridoAsync(
            string nombreArchivo,
            string? textoOcr,
            string? imagenBase64,
            int cdUsuarioAlta)
        {
            Logger.Info($"Iniciando procesamiento híbrido para: {nombreArchivo}", "OpenAIProcesador");

            ResultadoProcesamientoIA? resultado = null;
            int intentosOcr = 0;
            int intentosImagen = 0;

            // Intentar primero con OCR si está disponible
            if (!string.IsNullOrWhiteSpace(textoOcr))
            {
                Logger.Info($"Intentando procesamiento por OCR: {nombreArchivo}", "OpenAIProcesador");
                resultado = await ProcesarPorOcrAsync(nombreArchivo, textoOcr, cdUsuarioAlta);
                intentosOcr = resultado.Intentos;

                // Verificar si se obtuvieron los campos obligatorios
                if (resultado.Exitoso && TieneCamposObligatorios(resultado.RespuestaIA))
                {
                    Logger.Info($"Procesamiento por OCR exitoso con campos completos: {nombreArchivo}", "OpenAIProcesador");
                    resultado.ModalidadProcesamiento = "Hibrido-OCR";
                    return resultado;
                }
                else
                {
                    Logger.Info($"OCR incompleto o fallido, intentando con imagen: {nombreArchivo}", "OpenAIProcesador");
                }
            }

            // Si OCR falló o no estaba disponible, intentar con imagen
            if (!string.IsNullOrWhiteSpace(imagenBase64))
            {
                Logger.Info($"Intentando procesamiento por imagen: {nombreArchivo}", "OpenAIProcesador");
                resultado = await ProcesarPorImagenAsync(nombreArchivo, imagenBase64, cdUsuarioAlta);
                intentosImagen = resultado.Intentos;

                if (resultado.Exitoso)
                {
                    resultado.ModalidadProcesamiento = intentosOcr > 0 ? "Hibrido-Imagen" : "Imagen";
                    resultado.Intentos = intentosOcr + intentosImagen;
                    return resultado;
                }
            }

            // Si ambos fallaron
            if (resultado == null)
            {
                resultado = new ResultadoProcesamientoIA
                {
                    Exitoso = false,
                    MensajeError = "No hay OCR ni imagen disponible para procesar",
                    ModalidadProcesamiento = "Hibrido-Error",
                    Intentos = 1
                };
            }
            else
            {
                resultado.ModalidadProcesamiento = "Hibrido-Error";
                resultado.Intentos = intentosOcr + intentosImagen;
            }

            return resultado;
        }

        /// <summary>
        /// Procesa un archivo usando solo el texto OCR
        /// </summary>
        public async Task<ResultadoProcesamientoIA> ProcesarPorOcrAsync(
            string nombreArchivo,
            string textoOcr,
            int cdUsuarioAlta)
        {
            try
            {
                // Obtener prompt para OCR
                var parametroPrompt = _parametroRepo.ObtenerPorClave("OPENAI_API_PROMPT_OCR");
                if (parametroPrompt == null || string.IsNullOrWhiteSpace(parametroPrompt.DsValorParametro))
                {
                    return new ResultadoProcesamientoIA
                    {
                        Exitoso = false,
                        MensajeError = "No se encontró el prompt para procesamiento por OCR",
                        ModalidadProcesamiento = "OCR",
                        Intentos = 1
                    };
                }

                string promptSistema = parametroPrompt.DsValorParametro;
                string promptUsuario = $"Nombre del archivo: {nombreArchivo}\n\nTexto OCR:\n{textoOcr}";

                // Llamar a la API con reintentos
                return await LlamarAPIConReintentosAsync(nombreArchivo, promptSistema, promptUsuario, "OCR", cdUsuarioAlta);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error en procesamiento por OCR de {nombreArchivo}", ex, "OpenAIProcesador");
                return new ResultadoProcesamientoIA
                {
                    Exitoso = false,
                    MensajeError = $"Error en procesamiento por OCR: {ex.Message}",
                    ModalidadProcesamiento = "OCR",
                    Intentos = 1
                };
            }
        }

        /// <summary>
        /// Procesa un archivo usando la imagen en base64
        /// </summary>
        public async Task<ResultadoProcesamientoIA> ProcesarPorImagenAsync(
            string nombreArchivo,
            string imagenBase64,
            int cdUsuarioAlta)
        {
            try
            {
                // Obtener prompt para imagen
                var parametroPrompt = _parametroRepo.ObtenerPorClave("OPENAI_API_PROMPT_IMAGEN");
                if (parametroPrompt == null || string.IsNullOrWhiteSpace(parametroPrompt.DsValorParametro))
                {
                    return new ResultadoProcesamientoIA
                    {
                        Exitoso = false,
                        MensajeError = "No se encontró el prompt para procesamiento por imagen",
                        ModalidadProcesamiento = "Imagen",
                        Intentos = 1
                    };
                }

                string promptSistema = parametroPrompt.DsValorParametro;
                string promptUsuario = $"Nombre del archivo: {nombreArchivo}";

                // Llamar a la API con reintentos (incluirá imagen)
                return await LlamarAPIConReintentosAsync(nombreArchivo, promptSistema, promptUsuario, "Imagen", cdUsuarioAlta, imagenBase64);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error en procesamiento por imagen de {nombreArchivo}", ex, "OpenAIProcesador");
                return new ResultadoProcesamientoIA
                {
                    Exitoso = false,
                    MensajeError = $"Error en procesamiento por imagen: {ex.Message}",
                    ModalidadProcesamiento = "Imagen",
                    Intentos = 1
                };
            }
        }

        /// <summary>
        /// Llama a la API de OpenAI con reintentos exponenciales
        /// </summary>
        private async Task<ResultadoProcesamientoIA> LlamarAPIConReintentosAsync(
            string nombreArchivo,
            string promptSistema,
            string promptUsuario,
            string modalidad,
            int cdUsuarioAlta,
            string? imagenBase64 = null)
        {
            if (_cliente == null)
            {
                throw new InvalidOperationException("Cliente de OpenAI no inicializado");
            }

            int intentos = 0;
            Exception? ultimaExcepcion = null;

            while (intentos < _maxReintentos)
            {
                intentos++;
                try
                {
                    Logger.Info($"Intento {intentos}/{_maxReintentos} para {nombreArchivo} ({modalidad})", "OpenAIProcesador");

                    // Crear mensajes
                    var mensajes = new List<ChatMessage>
                    {
                        new SystemChatMessage(promptSistema)
                    };

                    // Si hay imagen, crear mensaje con imagen
                    if (!string.IsNullOrWhiteSpace(imagenBase64))
                    {
                        var contentParts = new List<ChatMessageContentPart>
                        {
                            ChatMessageContentPart.CreateTextPart(promptUsuario),
                            ChatMessageContentPart.CreateImagePart(
                                BinaryData.FromBytes(Convert.FromBase64String(imagenBase64)),
                                "image/jpeg")
                        };
                        mensajes.Add(new UserChatMessage(contentParts));
                    }
                    else
                    {
                        mensajes.Add(new UserChatMessage(promptUsuario));
                    }

                    // Configurar opciones de chat
                    var opciones = new ChatCompletionOptions
                    {
                        MaxOutputTokenCount = 1000,
                        Temperature = 0.1f, // Baja temperatura para respuestas más deterministas
                        ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
                    };

                    // Llamar a la API
                    var chatClient = _cliente.GetChatClient(_modelo);
                    var respuesta = await chatClient.CompleteChatAsync(mensajes, opciones);

                    // Parsear respuesta
                    string jsonRespuesta = respuesta.Value.Content[0].Text;
                    Logger.Info($"Respuesta recibida de OpenAI para {nombreArchivo}: {jsonRespuesta.Substring(0, Math.Min(200, jsonRespuesta.Length))}...", "OpenAIProcesador");

                    var respuestaIA = ParsearRespuesta(jsonRespuesta);

                    // Extraer tokens consumidos
                    var usage = respuesta.Value.Usage;
                    var usageTokens = new UsageTokens
                    {
                        PromptTokens = usage.InputTokenCount,
                        CompletionTokens = usage.OutputTokenCount,
                        TotalTokens = usage.TotalTokenCount
                    };

                    return new ResultadoProcesamientoIA
                    {
                        Exitoso = true,
                        RespuestaIA = respuestaIA,
                        Usage = usageTokens,
                        RespuestaCompleta = jsonRespuesta,
                        ModalidadProcesamiento = modalidad,
                        Intentos = intentos
                    };
                }
                catch (Exception ex)
                {
                    ultimaExcepcion = ex;
                    Logger.Error($"Error en intento {intentos} para {nombreArchivo}", ex, "OpenAIProcesador");

                    // Si no es el último intento, esperar con backoff exponencial
                    if (intentos < _maxReintentos)
                    {
                        int delayMs = (int)Math.Pow(2, intentos) * 1000; // 2s, 4s, 8s, etc.
                        Logger.Info($"Esperando {delayMs}ms antes del siguiente intento...", "OpenAIProcesador");
                        await Task.Delay(delayMs);
                    }
                }
            }

            // Todos los intentos fallaron
            return new ResultadoProcesamientoIA
            {
                Exitoso = false,
                MensajeError = $"Fallaron {intentos} intentos. Último error: {ultimaExcepcion?.Message}",
                ModalidadProcesamiento = modalidad,
                Intentos = intentos
            };
        }

        /// <summary>
        /// Parsea la respuesta JSON de OpenAI a una entidad RespuestaOpenAI
        /// </summary>
        private RespuestaOpenAI ParsearRespuesta(string jsonRespuesta)
        {
            try
            {
                // Deserializar con manejo case-insensitive
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                var respuesta = JsonConvert.DeserializeObject<RespuestaOpenAI>(jsonRespuesta, settings);

                if (respuesta == null)
                {
                    throw new JsonException("La respuesta deserializada es null");
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                Logger.Error("Error al parsear respuesta JSON de OpenAI", ex, "OpenAIProcesador");
                Logger.Error($"JSON recibido: {jsonRespuesta}", null, "OpenAIProcesador");
                throw new InvalidOperationException($"Error al parsear respuesta de OpenAI: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verifica si la respuesta tiene los campos obligatorios completos
        /// Campos obligatorios: TipoPlano, Seccion, Manzana, Parcela
        /// (Expediente puede ser null)
        /// </summary>
        private bool TieneCamposObligatorios(RespuestaOpenAI? respuesta)
        {
            if (respuesta == null)
                return false;

            return !string.IsNullOrWhiteSpace(respuesta.TipoPlano) &&
                   !string.IsNullOrWhiteSpace(respuesta.Seccion) &&
                   !string.IsNullOrWhiteSpace(respuesta.Manzana) &&
                   !string.IsNullOrWhiteSpace(respuesta.Parcela);
        }
    }

    /// <summary>
    /// Clase para encapsular el resultado del procesamiento
    /// </summary>
    public class ResultadoProcesamientoIA
    {
        public bool Exitoso { get; set; }
        public RespuestaOpenAI? RespuestaIA { get; set; }
        public UsageTokens? Usage { get; set; }
        public string? RespuestaCompleta { get; set; }
        public string? MensajeError { get; set; }
        public string? ModalidadProcesamiento { get; set; }
        public int Intentos { get; set; }
    }
}
