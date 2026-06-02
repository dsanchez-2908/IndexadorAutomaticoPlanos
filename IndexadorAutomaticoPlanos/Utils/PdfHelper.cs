using System.IO;
using IndexadorAutomaticoPlanos.Utils;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace IndexadorAutomaticoPlanos.Utils
{
    /// <summary>
    /// Utilidad para validar y obtener información de archivos PDF
    /// </summary>
    public static class PdfHelper
    {
        /// <summary>
        /// Valida si un archivo es un PDF válido verificando su firma (magic bytes)
        /// </summary>
        /// <param name="rutaArchivo">Ruta completa del archivo</param>
        /// <returns>True si es un PDF válido</returns>
        public static bool EsPdfValido(string rutaArchivo)
        {
            try
            {
                if (!File.Exists(rutaArchivo))
                    return false;

                // Verificar extensión
                if (!Path.GetExtension(rutaArchivo).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                    return false;

                // Verificar que el archivo no esté vacío
                FileInfo fileInfo = new FileInfo(rutaArchivo);
                if (fileInfo.Length < 4)
                    return false;

                // Leer los primeros 4 bytes para verificar la firma PDF (%PDF)
                using (FileStream fs = new FileStream(rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byte[] buffer = new byte[4];
                    fs.Read(buffer, 0, 4);

                    // PDF signature: %PDF (0x25 0x50 0x44 0x46)
                    return buffer[0] == 0x25 && 
                           buffer[1] == 0x50 && 
                           buffer[2] == 0x44 && 
                           buffer[3] == 0x46;
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"Error al validar PDF {rutaArchivo}: {ex.Message}", "PdfHelper");
                return false;
            }
        }

        /// <summary>
        /// Obtiene el tamaño de un archivo en bytes
        /// </summary>
        public static long ObtenerTamano(string rutaArchivo)
        {
            try
            {
                if (!File.Exists(rutaArchivo))
                    return 0;

                FileInfo fileInfo = new FileInfo(rutaArchivo);
                return fileInfo.Length;
            }
            catch (Exception ex)
            {
                Logger.Warning($"Error al obtener tamaño de {rutaArchivo}: {ex.Message}", "PdfHelper");
                return 0;
            }
        }

        /// <summary>
        /// Obtiene la fecha de última modificación del archivo
        /// </summary>
        public static DateTime ObtenerFechaModificacion(string rutaArchivo)
        {
            try
            {
                if (!File.Exists(rutaArchivo))
                    return DateTime.MinValue;

                FileInfo fileInfo = new FileInfo(rutaArchivo);
                return fileInfo.LastWriteTime;
            }
            catch (Exception ex)
            {
                Logger.Warning($"Error al obtener fecha de modificación de {rutaArchivo}: {ex.Message}", "PdfHelper");
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Cuenta las páginas de un PDF usando la librería PDFsharp
        /// </summary>
        public static int ContarPaginas(string rutaArchivo)
        {
            try
            {
                if (!File.Exists(rutaArchivo))
                {
                    Logger.Warning($"Archivo no existe: {rutaArchivo}", "PdfHelper.ContarPaginas");
                    return 0;
                }

                Logger.Debug($"Analizando PDF con PDFsharp: {Path.GetFileName(rutaArchivo)}", "PdfHelper.ContarPaginas");

                // Usar PDFsharp para abrir el documento y obtener el número de páginas
                using (PdfDocument document = PdfReader.Open(rutaArchivo, PdfDocumentOpenMode.InformationOnly))
                {
                    int pageCount = document.PageCount;
                    Logger.Info($"Resultado: {pageCount} página(s) para {Path.GetFileName(rutaArchivo)}", "PdfHelper.ContarPaginas");
                    return pageCount;
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"Error al contar páginas de {rutaArchivo}: {ex.Message}", "PdfHelper.ContarPaginas");

                // Intentar método fallback (manual)
                try
                {
                    Logger.Debug($"Intentando método fallback para {Path.GetFileName(rutaArchivo)}", "PdfHelper.ContarPaginas");
                    return ContarPaginasManual(rutaArchivo);
                }
                catch
                {
                    return 1; // Por defecto, asumir 1 página
                }
            }
        }

        /// <summary>
        /// Método fallback manual para contar páginas cuando PDFsharp falla
        /// </summary>
        private static int ContarPaginasManual(string rutaArchivo)
        {
            // Leer el archivo en modo binario y convertir a string ASCII
            byte[] bytes = File.ReadAllBytes(rutaArchivo);
            string contenido = System.Text.Encoding.ASCII.GetString(bytes);

            // Método 1: Buscar el patrón /Count N en el catálogo de páginas
            var matchCount = System.Text.RegularExpressions.Regex.Match(contenido, @"/Type\s*/Pages.*?/Count\s+(\d+)", System.Text.RegularExpressions.RegexOptions.Singleline);
            if (matchCount.Success)
            {
                int count = int.Parse(matchCount.Groups[1].Value);
                if (count > 0)
                {
                    Logger.Debug($"Método fallback encontró /Count: {count}", "PdfHelper.ContarPaginasManual");
                    return count;
                }
            }

            // Método 2: Contar objetos individuales de tipo /Page (no /Pages)
            var matches = System.Text.RegularExpressions.Regex.Matches(contenido, @"/Type\s*/Page[\s/\[]");
            int pageCount = matches.Count;

            Logger.Debug($"Método fallback encontró {pageCount} objetos /Page", "PdfHelper.ContarPaginasManual");
            return pageCount > 0 ? pageCount : 1;
        }

        /// <summary>
        /// Obtiene información completa de un archivo PDF
        /// </summary>
        public static PdfInfo ObtenerInformacion(string rutaArchivo)
        {
            return new PdfInfo
            {
                RutaCompleta = rutaArchivo,
                NombreArchivo = Path.GetFileName(rutaArchivo),
                DirectorioPadre = Path.GetDirectoryName(rutaArchivo) ?? string.Empty,
                TamanoBytes = ObtenerTamano(rutaArchivo),
                FechaModificacion = ObtenerFechaModificacion(rutaArchivo),
                EsValido = EsPdfValido(rutaArchivo),
                NumeroPaginas = ContarPaginas(rutaArchivo)
            };
        }

        /// <summary>
        /// Valida que se pueda acceder a un archivo (permisos de lectura)
        /// </summary>
        public static bool TienePermisoLectura(string rutaArchivo)
        {
            try
            {
                using (FileStream fs = File.Open(rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Convierte bytes a formato legible (KB, MB, GB)
        /// </summary>
        public static string FormatearTamano(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }

    /// <summary>
    /// Clase para encapsular información de un PDF
    /// </summary>
    public class PdfInfo
    {
        public string RutaCompleta { get; set; } = string.Empty;
        public string NombreArchivo { get; set; } = string.Empty;
        public string DirectorioPadre { get; set; } = string.Empty;
        public long TamanoBytes { get; set; }
        public DateTime FechaModificacion { get; set; }
        public bool EsValido { get; set; }
        public int NumeroPaginas { get; set; }
        public bool EsDuplicado { get; set; }

        public string TamanoLegible => PdfHelper.FormatearTamano(TamanoBytes);
    }
}
