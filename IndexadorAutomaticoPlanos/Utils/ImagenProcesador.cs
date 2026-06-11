using PDFtoImage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using Tesseract;
using System.Configuration;
using ImageSharpImage = SixLabors.ImageSharp.Image;
using ImageSharpRectangle = SixLabors.ImageSharp.Rectangle;
using SkiaSharp;

namespace IndexadorAutomaticoPlanos.Utils
{
    /// <summary>
    /// Enumeración para las esquinas de recorte de imagen
    /// </summary>
    public enum EsquinaRecorte
    {
        InferiorDerecha,
        InferiorIzquierda,
        SuperiorDerecha,
        SuperiorIzquierda
    }

    /// <summary>
    /// Procesador de imágenes para preparación de PDFs para OpenAI
    /// Extrae primera página, recorta carátula, aplica OCR y convierte a base64
    /// </summary>
    public class ImagenProcesador : IDisposable
    {
        private readonly string _pathRepositorio;
        private readonly int _dpiDefault = 300;
        private TesseractEngine? _ocrEngine;
        private bool _disposed = false;
        private readonly object _ocrLock = new object(); // Lock para sincronizar acceso a Tesseract

        public ImagenProcesador()
        {
            _pathRepositorio = ConfigurationManager.AppSettings["PATH_REPOSITORIO"] 
                ?? throw new Exception("PATH_REPOSITORIO no configurado en App.config");

            if (!Directory.Exists(_pathRepositorio))
            {
                Directory.CreateDirectory(_pathRepositorio);
                Logger.Info($"Repositorio de imágenes creado: {_pathRepositorio}", "ImagenProcesador");
            }
        }

        /// <summary>
        /// Inicializa el motor OCR de Tesseract (solo si se va a usar)
        /// </summary>
        private void InicializarOcr()
        {
            if (_ocrEngine != null) return;

            try
            {
                // Buscar tessdata en múltiples ubicaciones posibles
                string[] posiblesTessdataPaths = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata"),
                    Path.Combine(Environment.CurrentDirectory, "tessdata"),
                    @"C:\Program Files\Tesseract-OCR\tessdata",
                    @"C:\tessdata"
                };

                string? tessdataPath = posiblesTessdataPaths.FirstOrDefault(Directory.Exists);

                if (tessdataPath == null)
                {
                    throw new Exception(
                        "No se encontró la carpeta tessdata. Debe contener los archivos de idioma de Tesseract. " +
                        $"Ubicaciones buscadas: {string.Join(", ", posiblesTessdataPaths)}");
                }

                _ocrEngine = new TesseractEngine(tessdataPath, "spa+eng", EngineMode.Default);
                Logger.Info($"Motor OCR inicializado con tessdata desde: {tessdataPath}", "ImagenProcesador");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al inicializar motor OCR", ex, "ImagenProcesador");
                throw new Exception("No se pudo inicializar Tesseract OCR. " +
                    "Asegúrese de que tessdata esté instalado con los idiomas spa.traineddata y eng.traineddata", ex);
            }
        }

        /// <summary>
        /// Extrae la primera página de un PDF y la convierte a imagen en memoria
        /// </summary>
        public ImageSharpImage ExtraerPrimeraPaginaComoImagen(string rutaPdf, int dpi = 0)
        {
            if (dpi <= 0) dpi = _dpiDefault;

            try
            {
                Logger.Info($"Extrayendo primera página de {Path.GetFileName(rutaPdf)} a {dpi} DPI", "ImagenProcesador");

                // Leer el archivo PDF completo en bytes
                byte[] pdfBytes = File.ReadAllBytes(rutaPdf);

                // PDFtoImage renderiza el PDF usando PDFium
                // Convertir PDF a imagen usando opciones de renderizado
                var options = new RenderOptions(Dpi: dpi, Width: null, Height: null, WithAnnotations: false, WithFormFill: false, WithAspectRatio: true);

                // Convertir a SKBitmap (página 0 = primera)
                using var skBitmap = Conversion.ToImage(pdfBytes, 0, options: options);

                // Convertir SKBitmap a bytes PNG
                using var skImage = SKImage.FromBitmap(skBitmap);
                using var skData = skImage.Encode(SKEncodedImageFormat.Png, 100);
                byte[] imageBytes = skData.ToArray();

                // Cargar los bytes PNG con ImageSharp
                using var ms = new MemoryStream(imageBytes);
                var imagenSharp = ImageSharpImage.Load(ms);

                Logger.Info($"Imagen extraída: {imagenSharp.Width}x{imagenSharp.Height} px", "ImagenProcesador");

                return imagenSharp;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al extraer página de {rutaPdf}", ex, "ImagenProcesador");
                throw new Exception($"Error al extraer primera página del PDF: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Recorta una imagen según la esquina y porcentajes especificados
        /// </summary>
        public ImageSharpImage RecortarImagen(ImageSharpImage imagenOriginal, EsquinaRecorte esquina, decimal porcentajeHorizontal, decimal porcentajeVertical)
        {
            if (porcentajeHorizontal <= 0 || porcentajeHorizontal > 100)
                throw new ArgumentException("El porcentaje horizontal debe estar entre 0 y 100", nameof(porcentajeHorizontal));
            if (porcentajeVertical <= 0 || porcentajeVertical > 100)
                throw new ArgumentException("El porcentaje vertical debe estar entre 0 y 100", nameof(porcentajeVertical));

            try
            {
                int anchoOriginal = imagenOriginal.Width;
                int altoOriginal = imagenOriginal.Height;

                // Calcular dimensiones del recorte
                int anchoRecorte = (int)(anchoOriginal * (porcentajeHorizontal / 100m));
                int altoRecorte = (int)(altoOriginal * (porcentajeVertical / 100m));

                // Calcular posición de inicio según esquina
                int x = 0, y = 0;
                switch (esquina)
                {
                    case EsquinaRecorte.InferiorDerecha:
                        x = anchoOriginal - anchoRecorte;
                        y = altoOriginal - altoRecorte;
                        break;
                    case EsquinaRecorte.InferiorIzquierda:
                        x = 0;
                        y = altoOriginal - altoRecorte;
                        break;
                    case EsquinaRecorte.SuperiorDerecha:
                        x = anchoOriginal - anchoRecorte;
                        y = 0;
                        break;
                    case EsquinaRecorte.SuperiorIzquierda:
                        x = 0;
                        y = 0;
                        break;
                }

                Logger.Info($"Recortando H:{porcentajeHorizontal}% V:{porcentajeVertical}% desde {esquina}: {anchoRecorte}x{altoRecorte} en ({x},{y})", 
                    "ImagenProcesador");

                // Clonar y recortar
                var imagenRecortada = imagenOriginal.Clone(ctx => 
                    ctx.Crop(new ImageSharpRectangle(x, y, anchoRecorte, altoRecorte)));

                return imagenRecortada;
            }
            catch (Exception ex)
            {
                Logger.Error("Error al recortar imagen", ex, "ImagenProcesador");
                throw new Exception($"Error al recortar imagen: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Convierte una imagen a base64
        /// </summary>
        public string ConvertirImagenABase64(ImageSharpImage imagen)
        {
            try
            {
                using var ms = new MemoryStream();
                imagen.Save(ms, new JpegEncoder { Quality = 90 });
                var bytes = ms.ToArray();
                var base64 = Convert.ToBase64String(bytes);

                Logger.Info($"Imagen convertida a base64: {base64.Length} caracteres ({bytes.Length} bytes)", 
                    "ImagenProcesador");

                return base64;
            }
            catch (Exception ex)
            {
                Logger.Error("Error al convertir imagen a base64", ex, "ImagenProcesador");
                throw new Exception($"Error al convertir imagen a base64: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ejecuta OCR sobre una imagen
        /// </summary>
        public string EjecutarOcr(ImageSharpImage imagen)
        {
            // CRÍTICO: Tesseract NO es thread-safe
            // Usar lock para garantizar que solo un thread procese OCR a la vez
            lock (_ocrLock)
            {
                try
                {
                    // Inicializar motor OCR si no está inicializado
                    if (_ocrEngine == null)
                    {
                        InicializarOcr();
                    }

                    Logger.Info("Ejecutando OCR sobre imagen", "ImagenProcesador");

                    // Convertir Image a formato compatible con Tesseract (Pix)
                    using var ms = new MemoryStream();
                    imagen.Save(ms, new JpegEncoder { Quality = 100 });
                    ms.Position = 0;

                    using var pix = Pix.LoadFromMemory(ms.ToArray());
                    using var page = _ocrEngine!.Process(pix);

                    string texto = page.GetText().Trim();

                    Logger.Info($"OCR completado: {texto.Length} caracteres extraídos", "ImagenProcesador");

                    return texto;
                }
                catch (Exception ex)
                {
                    Logger.Error("Error al ejecutar OCR", ex, "ImagenProcesador");
                    throw new Exception($"Error al ejecutar OCR: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// Guarda una imagen en el repositorio físico
        /// </summary>
        public string GuardarImagenEnRepositorio(ImageSharpImage imagen, string nombreLote, string nombreArchivo)
        {
            try
            {
                // Crear carpeta del lote si no existe
                string carpetaLote = Path.Combine(_pathRepositorio, nombreLote);
                if (!Directory.Exists(carpetaLote))
                {
                    Directory.CreateDirectory(carpetaLote);
                }

                // Construir ruta completa (cambiar extensión a .jpg)
                string nombreSinExtension = Path.GetFileNameWithoutExtension(nombreArchivo);
                string rutaDestino = Path.Combine(carpetaLote, $"{nombreSinExtension}.jpg");

                // Guardar con alta calidad
                imagen.Save(rutaDestino, new JpegEncoder { Quality = 95 });

                Logger.Info($"Imagen guardada: {rutaDestino}", "ImagenProcesador");

                return rutaDestino;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al guardar imagen para {nombreArchivo}", ex, "ImagenProcesador");
                throw new Exception($"Error al guardar imagen en repositorio: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Método orquestador completo: procesa un archivo PDF completo
        /// </summary>
        public ResultadoProcesamiento ProcesarArchivoPdf(
            string rutaPdf,
            string nombreLote,
            int dpi,
            EsquinaRecorte esquina,
            decimal porcentajeHorizontal,
            decimal porcentajeVertical,
            bool ejecutarOcr)
        {
            ImageSharpImage? imagenCompleta = null;
            ImageSharpImage? imagenRecortada = null;

            try
            {
                Logger.Info($"Iniciando procesamiento de {Path.GetFileName(rutaPdf)}", "ImagenProcesador");

                // 1. Extraer primera página como imagen
                imagenCompleta = ExtraerPrimeraPaginaComoImagen(rutaPdf, dpi);

                // 2. Recortar según parámetros
                imagenRecortada = RecortarImagen(imagenCompleta, esquina, porcentajeHorizontal, porcentajeVertical);

                // 3. Guardar imagen recortada en repositorio
                string rutaImagenGuardada = GuardarImagenEnRepositorio(
                    imagenRecortada, nombreLote, Path.GetFileName(rutaPdf));

                // 4. Convertir a base64
                string base64 = ConvertirImagenABase64(imagenRecortada);

                // 5. Ejecutar OCR si está habilitado
                string? textoOcr = null;
                if (ejecutarOcr)
                {
                    textoOcr = EjecutarOcr(imagenRecortada);
                }

                Logger.Info($"Procesamiento completado para {Path.GetFileName(rutaPdf)}", "ImagenProcesador");

                return new ResultadoProcesamiento
                {
                    Exitoso = true,
                    RutaImagenJpg = rutaImagenGuardada,
                    ImagenBase64 = base64,
                    TextoOcr = textoOcr,
                    TieneOcr = ejecutarOcr && !string.IsNullOrWhiteSpace(textoOcr)
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"Error en procesamiento completo de {rutaPdf}", ex, "ImagenProcesador");
                return new ResultadoProcesamiento
                {
                    Exitoso = false,
                    MensajeError = ex.Message
                };
            }
            finally
            {
                // CRÍTICO: Liberar memoria explícitamente
                imagenCompleta?.Dispose();
                imagenRecortada?.Dispose();

                // Forzar recolección de basura si se procesaron imágenes grandes
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        /// <summary>
        /// Calcula el rectángulo de recorte para previsualización (sin modificar la imagen)
        /// </summary>
        public ImageSharpRectangle CalcularRectanguloRecorte(int anchoImagen, int altoImagen, 
            EsquinaRecorte esquina, decimal porcentajeHorizontal, decimal porcentajeVertical)
        {
            int anchoRecorte = (int)(anchoImagen * (porcentajeHorizontal / 100m));
            int altoRecorte = (int)(altoImagen * (porcentajeVertical / 100m));

            int x = 0, y = 0;
            switch (esquina)
            {
                case EsquinaRecorte.InferiorDerecha:
                    x = anchoImagen - anchoRecorte;
                    y = altoImagen - altoRecorte;
                    break;
                case EsquinaRecorte.InferiorIzquierda:
                    x = 0;
                    y = altoImagen - altoRecorte;
                    break;
                case EsquinaRecorte.SuperiorDerecha:
                    x = anchoImagen - anchoRecorte;
                    y = 0;
                    break;
                case EsquinaRecorte.SuperiorIzquierda:
                    x = 0;
                    y = 0;
                    break;
            }

            return new ImageSharpRectangle(x, y, anchoRecorte, altoRecorte);
        }

        public void Dispose()
        {
            if (_disposed) return;

            _ocrEngine?.Dispose();
            _disposed = true;

            Logger.Info("ImagenProcesador liberado", "ImagenProcesador");
        }
    }

    /// <summary>
    /// Resultado del procesamiento de un archivo PDF
    /// </summary>
    public class ResultadoProcesamiento
    {
        public bool Exitoso { get; set; }
        public string? RutaImagenJpg { get; set; }
        public string? ImagenBase64 { get; set; }
        public string? TextoOcr { get; set; }
        public bool TieneOcr { get; set; }
        public string? MensajeError { get; set; }
    }
}
