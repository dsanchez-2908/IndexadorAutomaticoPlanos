using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IndexadorAutomaticoPlanos.Entities;

namespace IndexadorAutomaticoPlanos.Utils
{
    /// <summary>
    /// Clase encargada de la finalización de lotes:
    /// - Renombrado de archivos PDF según nomenclatura
    /// - Generación/actualización de archivo INDEX.csv
    /// - Manejo de transacciones con rollback
    /// </summary>
    public class FinalizadorLotes
    {
        private readonly string _pathRepositorio;
        private List<OperacionRenombrado> _operacionesRealizadas;

        public FinalizadorLotes(string pathRepositorio)
        {
            _pathRepositorio = pathRepositorio;
            _operacionesRealizadas = new List<OperacionRenombrado>();
        }

        /// <summary>
        /// Procesa un lote completo: renombra archivos, genera CSV y retorna resultado
        /// </summary>
        public ResultadoFinalizacion ProcesarLote(List<ArchivoParaFinalizar> archivos)
        {
            var resultado = new ResultadoFinalizacion();
            _operacionesRealizadas.Clear();

            try
            {
                // Validar que todos los archivos tengan datos completos
                var archivosSinDatos = archivos.Where(a => !a.TieneDatosCompletos()).ToList();
                if (archivosSinDatos.Any())
                {
                    resultado.Exito = false;
                    resultado.Mensaje = $"Hay {archivosSinDatos.Count} archivo(s) sin datos completos. " +
                                       $"Complete los datos antes de finalizar.";
                    resultado.ArchivosConError.AddRange(archivosSinDatos.Select(a => a.DsNombreArchivo));
                    return resultado;
                }

                // Paso 1: Renombrar archivos
                foreach (var archivo in archivos)
                {
                    try
                    {
                        var operacion = RenombrarArchivo(archivo);
                        _operacionesRealizadas.Add(operacion);
                        resultado.ArchivosRenombrados.Add(operacion);
                    }
                    catch (Exception ex)
                    {
                        // Error en renombrado → Rollback y abortar
                        Logger.Error($"Error al renombrar archivo {archivo.DsNombreArchivo}", ex, "FinalizadorLotes");
                        RealizarRollback();

                        resultado.Exito = false;
                        resultado.Mensaje = $"Error al renombrar '{archivo.DsNombreArchivo}': {ex.Message}";
                        resultado.ArchivosConError.Add(archivo.DsNombreArchivo);
                        return resultado;
                    }
                }

                // Paso 2: Generar/Actualizar INDEX.csv
                try
                {
                    string carpetaDestino = Path.GetDirectoryName(_operacionesRealizadas[0].RutaDestino) ?? string.Empty;
                    string archivoCSV = Path.Combine(carpetaDestino, "INDEX.csv");

                    GenerarActualizarCSV(archivoCSV, _operacionesRealizadas);
                    resultado.RutaArchivoCSV = archivoCSV;
                }
                catch (Exception ex)
                {
                    // Error en CSV → Rollback y abortar
                    Logger.Error("Error al generar archivo INDEX.csv", ex, "FinalizadorLotes");
                    RealizarRollback();

                    resultado.Exito = false;
                    resultado.Mensaje = $"Error al generar INDEX.csv: {ex.Message}";
                    return resultado;
                }

                // Éxito
                resultado.Exito = true;
                resultado.Mensaje = $"Lote finalizado exitosamente. {_operacionesRealizadas.Count} archivo(s) procesado(s).";
                Logger.Info($"Lote finalizado: {_operacionesRealizadas.Count} archivos renombrados", "FinalizadorLotes");

                return resultado;
            }
            catch (Exception ex)
            {
                Logger.Error("Error general al procesar lote", ex, "FinalizadorLotes");
                RealizarRollback();

                resultado.Exito = false;
                resultado.Mensaje = $"Error al procesar lote: {ex.Message}";
                return resultado;
            }
        }

        /// <summary>
        /// Renombra un archivo según la nomenclatura definida
        /// </summary>
        private OperacionRenombrado RenombrarArchivo(ArchivoParaFinalizar archivo)
        {
            string rutaCompleta = Path.Combine(_pathRepositorio, archivo.DsRutaCompleta, archivo.DsNombreArchivo);

            if (!File.Exists(rutaCompleta))
            {
                throw new FileNotFoundException($"No se encontró el archivo: {rutaCompleta}");
            }

            // Verificar que el archivo no esté abierto
            if (EstaArchivoAbierto(rutaCompleta))
            {
                throw new IOException($"El archivo está abierto por otro proceso: {archivo.DsNombreArchivo}");
            }

            string nuevoNombre;
            string carpetaDestino = Path.GetDirectoryName(rutaCompleta) ?? string.Empty;

            // Procesar archivos ilegibles
            if (archivo.EsIlegible)
            {
                nuevoNombre = "CARATULA_ILEGIBLE.pdf";
                carpetaDestino = Path.Combine(carpetaDestino, "ILEGIBLE");

                // Crear carpeta ILEGIBLE si no existe
                if (!Directory.Exists(carpetaDestino))
                {
                    Directory.CreateDirectory(carpetaDestino);
                }
            }
            else
            {
                // Generar nombre según nomenclatura
                nuevoNombre = GenerarNombreArchivo(archivo);
            }

            // Resolver duplicados
            string rutaDestino = Path.Combine(carpetaDestino, nuevoNombre);
            rutaDestino = ResolverNombreDuplicado(rutaDestino);

            // Renombrar archivo
            File.Move(rutaCompleta, rutaDestino);

            Logger.Info($"Archivo renombrado: {archivo.DsNombreArchivo} → {Path.GetFileName(rutaDestino)}", "FinalizadorLotes");

            return new OperacionRenombrado
            {
                Archivo = archivo,
                RutaOrigen = rutaCompleta,
                RutaDestino = rutaDestino,
                NombreOriginal = archivo.DsNombreArchivo,
                NombreNuevo = Path.GetFileName(rutaDestino)
            };
        }

        /// <summary>
        /// Genera el nombre del archivo según la nomenclatura definida
        /// </summary>
        private string GenerarNombreArchivo(ArchivoParaFinalizar archivo)
        {
            var partes = new List<string>();

            // Expediente (opcional)
            if (!string.IsNullOrWhiteSpace(archivo.DsExpediente))
            {
                partes.Add(SanitizarNombre(archivo.DsExpediente));
            }

            // Tipo (obligatorio)
            partes.Add(SanitizarNombre(archivo.DsTipoPlano ?? "TIPO"));

            // Dirección (obligatoria)
            partes.Add(SanitizarNombre(archivo.DsDireccion ?? "SIN_DIRECCION"));

            // Nomenclatura (obligatoria)
            partes.Add(SanitizarNombre(archivo.ObtenerNomenclatura()));

            return string.Join("-", partes) + ".pdf";
        }

        /// <summary>
        /// Sanitiza un nombre eliminando caracteres especiales
        /// </summary>
        private string SanitizarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return "VACIO";

            // Eliminar caracteres no válidos para nombres de archivo
            string invalidos = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            string patron = $"[{Regex.Escape(invalidos)}]";

            string sanitizado = Regex.Replace(nombre, patron, "");

            // Reemplazar espacios múltiples por uno solo
            sanitizado = Regex.Replace(sanitizado, @"\s+", " ");

            // Trim
            sanitizado = sanitizado.Trim();

            return string.IsNullOrWhiteSpace(sanitizado) ? "VACIO" : sanitizado;
        }

        /// <summary>
        /// Resuelve nombres duplicados agregando sufijo numérico
        /// Formato: archivo.pdf → archivo(2).pdf → archivo(3).pdf
        /// </summary>
        private string ResolverNombreDuplicado(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
            {
                return rutaArchivo;
            }

            string directorio = Path.GetDirectoryName(rutaArchivo) ?? string.Empty;
            string nombreSinExtension = Path.GetFileNameWithoutExtension(rutaArchivo);
            string extension = Path.GetExtension(rutaArchivo);

            int contador = 2; // Empieza en 2 porque el primero no tiene número
            string nuevaRuta;

            do
            {
                string nuevoNombre = $"{nombreSinExtension}({contador}){extension}";
                nuevaRuta = Path.Combine(directorio, nuevoNombre);
                contador++;
            }
            while (File.Exists(nuevaRuta));

            return nuevaRuta;
        }

        /// <summary>
        /// Verifica si un archivo está abierto por otro proceso
        /// </summary>
        private bool EstaArchivoAbierto(string rutaArchivo)
        {
            try
            {
                using (FileStream stream = File.Open(rutaArchivo, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    stream.Close();
                }
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }

        /// <summary>
        /// Genera o actualiza el archivo INDEX.csv (append si existe)
        /// </summary>
        private void GenerarActualizarCSV(string rutaCSV, List<OperacionRenombrado> operaciones)
        {
            bool archivoExiste = File.Exists(rutaCSV);

            using (var writer = new StreamWriter(rutaCSV, append: true, Encoding.UTF8))
            {
                // Escribir encabezado solo si el archivo no existe
                if (!archivoExiste)
                {
                    writer.WriteLine("NombreLote,NombreOriginal,NombreNuevo,TipoPlano,Expediente,Seccion,Manzana,Parcela,Direccion");
                }

                // Escribir registros
                foreach (var op in operaciones)
                {
                    var archivo = op.Archivo;
                    string linea = $"{EscaparCSV(archivo.DsNombreLote)}," +
                                  $"{EscaparCSV(op.NombreOriginal)}," +
                                  $"{EscaparCSV(op.NombreNuevo)}," +
                                  $"{EscaparCSV(archivo.DsTipoPlano ?? "")}," +
                                  $"{EscaparCSV(archivo.DsExpediente ?? "")}," +
                                  $"{EscaparCSV(archivo.DsSeccion ?? "")}," +
                                  $"{EscaparCSV(archivo.DsManzana ?? "")}," +
                                  $"{EscaparCSV(archivo.DsParcela ?? "")}," +
                                  $"{EscaparCSV(archivo.DsDireccion ?? "")}";

                    writer.WriteLine(linea);
                }
            }

            Logger.Info($"Archivo INDEX.csv {(archivoExiste ? "actualizado" : "creado")} con {operaciones.Count} registro(s)", "FinalizadorLotes");
        }

        /// <summary>
        /// Escapa valores para CSV (comillas si contiene coma, comillas o salto de línea)
        /// </summary>
        private string EscaparCSV(string valor)
        {
            if (string.IsNullOrEmpty(valor)) return "";

            if (valor.Contains(",") || valor.Contains("\"") || valor.Contains("\n") || valor.Contains("\r"))
            {
                return $"\"{valor.Replace("\"", "\"\"")}\"";
            }

            return valor;
        }

        /// <summary>
        /// Realiza rollback de todas las operaciones realizadas
        /// </summary>
        private void RealizarRollback()
        {
            Logger.Warning($"Iniciando rollback de {_operacionesRealizadas.Count} operación(es)", "FinalizadorLotes");

            foreach (var operacion in _operacionesRealizadas.AsEnumerable().Reverse())
            {
                try
                {
                    if (File.Exists(operacion.RutaDestino))
                    {
                        File.Move(operacion.RutaDestino, operacion.RutaOrigen);
                        Logger.Info($"Rollback: {operacion.NombreNuevo} → {operacion.NombreOriginal}", "FinalizadorLotes");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error en rollback de archivo {operacion.NombreNuevo}", ex, "FinalizadorLotes");
                }
            }

            _operacionesRealizadas.Clear();
        }
    }

    /// <summary>
    /// Representa una operación de renombrado realizada
    /// </summary>
    public class OperacionRenombrado
    {
        public ArchivoParaFinalizar Archivo { get; set; } = null!;
        public string RutaOrigen { get; set; } = string.Empty;
        public string RutaDestino { get; set; } = string.Empty;
        public string NombreOriginal { get; set; } = string.Empty;
        public string NombreNuevo { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resultado del proceso de finalización de un lote
    /// </summary>
    public class ResultadoFinalizacion
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public List<OperacionRenombrado> ArchivosRenombrados { get; set; } = new List<OperacionRenombrado>();
        public List<string> ArchivosConError { get; set; } = new List<string>();
        public string RutaArchivoCSV { get; set; } = string.Empty;
    }
}
