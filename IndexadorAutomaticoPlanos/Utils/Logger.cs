using System.Data.SqlClient;
using IndexadorAutomaticoPlanos.DataAccess;
using IndexadorAutomaticoPlanos.Entities;

namespace IndexadorAutomaticoPlanos.Utils
{
    /// <summary>
    /// Sistema de logging centralizado que escribe en archivo y base de datos
    /// </summary>
    public static class Logger
    {
        private static readonly string RutaLogs = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private static readonly object LockArchivo = new object();

        static Logger()
        {
            // Crear directorio de logs si no existe
            if (!Directory.Exists(RutaLogs))
            {
                Directory.CreateDirectory(RutaLogs);
            }
        }

        /// <summary>
        /// Registra un mensaje de nivel Info
        /// </summary>
        public static void Info(string mensaje, string? modulo = null)
        {
            Registrar(LogLevel.Info, mensaje, modulo, null);
        }

        /// <summary>
        /// Registra un mensaje de advertencia
        /// </summary>
        public static void Warning(string mensaje, string? modulo = null)
        {
            Registrar(LogLevel.Warning, mensaje, modulo, null);
        }

        /// <summary>
        /// Registra un error
        /// </summary>
        public static void Error(string mensaje, Exception? excepcion = null, string? modulo = null)
        {
            Registrar(LogLevel.Error, mensaje, modulo, excepcion);
        }

        /// <summary>
        /// Registra un mensaje de debug
        /// </summary>
        public static void Debug(string mensaje, string? modulo = null)
        {
            Registrar(LogLevel.Debug, mensaje, modulo, null);
        }

        /// <summary>
        /// Método principal de registro
        /// </summary>
        private static void Registrar(LogLevel nivel, string mensaje, string? modulo, Exception? excepcion)
        {
            try
            {
                DateTime ahora = DateTime.Now;
                string mensajeCompleto = FormatearMensaje(nivel, mensaje, modulo, excepcion, ahora);

                // Escribir en archivo
                EscribirEnArchivo(mensajeCompleto, ahora);

                // Escribir en base de datos (de manera asíncrona para no bloquear)
                Task.Run(() => EscribirEnBaseDatos(nivel, mensaje, modulo, excepcion, ahora));
            }
            catch (Exception ex)
            {
                // Si falla el logging, escribir en consola o archivo de emergencia
                EscribirLogEmergencia($"Error en sistema de logging: {ex.Message}");
            }
        }

        /// <summary>
        /// Formatea el mensaje de log
        /// </summary>
        private static string FormatearMensaje(LogLevel nivel, string mensaje, string? modulo, Exception? excepcion, DateTime fecha)
        {
            string nivelStr = nivel.ToString().ToUpper().PadRight(8);
            string moduloStr = string.IsNullOrEmpty(modulo) ? "" : $"[{modulo}] ";
            string fechaStr = fecha.ToString("yyyy-MM-dd HH:mm:ss.fff");

            string mensajeFormateado = $"{fechaStr} | {nivelStr} | {moduloStr}{mensaje}";

            if (excepcion != null)
            {
                mensajeFormateado += $"\nExcepción: {excepcion.Message}";
                mensajeFormateado += $"\nStackTrace: {excepcion.StackTrace}";

                if (excepcion.InnerException != null)
                {
                    mensajeFormateado += $"\nInner Exception: {excepcion.InnerException.Message}";
                }
            }

            return mensajeFormateado;
        }

        /// <summary>
        /// Escribe el log en un archivo
        /// </summary>
        private static void EscribirEnArchivo(string mensaje, DateTime fecha)
        {
            try
            {
                string nombreArchivo = $"Log_{fecha:yyyyMMdd}.txt";
                string rutaCompleta = Path.Combine(RutaLogs, nombreArchivo);

                lock (LockArchivo)
                {
                    File.AppendAllText(rutaCompleta, mensaje + Environment.NewLine);
                }
            }
            catch
            {
                // Silencioso si falla la escritura en archivo
            }
        }

        /// <summary>
        /// Escribe el log en la base de datos
        /// </summary>
        private static void EscribirEnBaseDatos(LogLevel nivel, string mensaje, string? modulo, Exception? excepcion, DateTime fecha)
        {
            try
            {
                DatabaseHelper db = new DatabaseHelper();

                string query = @"
                    INSERT INTO IAP_TD_LOGS 
                    (dsNivel, dsModulo, dsMensaje, dsExcepcion, cdUsuario, dsUsuario, feRegistro)
                    VALUES 
                    (@dsNivel, @dsModulo, @dsMensaje, @dsExcepcion, @cdUsuario, @dsUsuario, @feRegistro)";

                string? dsExcepcion = null;
                if (excepcion != null)
                {
                    dsExcepcion = $"{excepcion.Message}\n{excepcion.StackTrace}";
                    if (excepcion.InnerException != null)
                    {
                        dsExcepcion += $"\nInner: {excepcion.InnerException.Message}";
                    }
                }

                int? cdUsuario = SesionActual.EstaAutenticado ? SesionActual.ObtenerIdUsuario() : null;
                string? dsUsuario = SesionActual.EstaAutenticado ? SesionActual.ObtenerNombreUsuario() : null;

                db.EjecutarComando(query,
                    DatabaseHelper.CrearParametro("@dsNivel", nivel.ToString()),
                    DatabaseHelper.CrearParametro("@dsModulo", modulo),
                    DatabaseHelper.CrearParametro("@dsMensaje", mensaje),
                    DatabaseHelper.CrearParametro("@dsExcepcion", dsExcepcion),
                    DatabaseHelper.CrearParametro("@cdUsuario", cdUsuario),
                    DatabaseHelper.CrearParametro("@dsUsuario", dsUsuario),
                    DatabaseHelper.CrearParametro("@feRegistro", fecha));
            }
            catch
            {
                // Silencioso si falla la escritura en BD (ya está en archivo)
            }
        }

        /// <summary>
        /// Escribe en un archivo de log de emergencia cuando el sistema de logging falla
        /// </summary>
        private static void EscribirLogEmergencia(string mensaje)
        {
            try
            {
                string rutaEmergencia = Path.Combine(RutaLogs, "Emergency.txt");
                string mensajeCompleto = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {mensaje}";

                lock (LockArchivo)
                {
                    File.AppendAllText(rutaEmergencia, mensajeCompleto + Environment.NewLine);
                }
            }
            catch
            {
                // Último recurso: escribir en consola
                Console.WriteLine($"EMERGENCY LOG: {mensaje}");
            }
        }

        /// <summary>
        /// Obtiene los logs de un rango de fechas
        /// </summary>
        public static List<LogEntry> ObtenerLogs(DateTime? fechaDesde = null, DateTime? fechaHasta = null, LogLevel? nivel = null, string? modulo = null)
        {
            try
            {
                DatabaseHelper db = new DatabaseHelper();
                List<SqlParameter> parametros = new List<SqlParameter>();

                string query = @"
                    SELECT TOP 1000 
                        cdLog, dsNivel, dsModulo, dsMensaje, dsExcepcion, 
                        cdUsuario, dsUsuario, feRegistro
                    FROM IAP_TD_LOGS
                    WHERE 1=1";

                if (fechaDesde.HasValue)
                {
                    query += " AND feRegistro >= @fechaDesde";
                    parametros.Add(DatabaseHelper.CrearParametro("@fechaDesde", fechaDesde.Value));
                }

                if (fechaHasta.HasValue)
                {
                    query += " AND feRegistro <= @fechaHasta";
                    parametros.Add(DatabaseHelper.CrearParametro("@fechaHasta", fechaHasta.Value));
                }

                if (nivel.HasValue)
                {
                    query += " AND dsNivel = @nivel";
                    parametros.Add(DatabaseHelper.CrearParametro("@nivel", nivel.Value.ToString()));
                }

                if (!string.IsNullOrEmpty(modulo))
                {
                    query += " AND dsModulo = @modulo";
                    parametros.Add(DatabaseHelper.CrearParametro("@modulo", modulo));
                }

                query += " ORDER BY feRegistro DESC";

                var dt = db.EjecutarConsulta(query, parametros.ToArray());
                List<LogEntry> logs = new List<LogEntry>();

                foreach (System.Data.DataRow row in dt.Rows)
                {
                    logs.Add(new LogEntry
                    {
                        CdLog = Convert.ToInt64(row["cdLog"]),
                        DsNivel = row["dsNivel"].ToString() ?? string.Empty,
                        DsModulo = row["dsModulo"] != DBNull.Value ? row["dsModulo"].ToString() : null,
                        DsMensaje = row["dsMensaje"].ToString() ?? string.Empty,
                        DsExcepcion = row["dsExcepcion"] != DBNull.Value ? row["dsExcepcion"].ToString() : null,
                        CdUsuario = row["cdUsuario"] != DBNull.Value ? Convert.ToInt32(row["cdUsuario"]) : null,
                        DsUsuario = row["dsUsuario"] != DBNull.Value ? row["dsUsuario"].ToString() : null,
                        FeRegistro = Convert.ToDateTime(row["feRegistro"])
                    });
                }

                return logs;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener logs: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Limpia logs antiguos (más de X días)
        /// </summary>
        public static int LimpiarLogsAntiguos(int diasAntiguedad = 90)
        {
            try
            {
                DatabaseHelper db = new DatabaseHelper();
                DateTime fechaLimite = DateTime.Now.AddDays(-diasAntiguedad);

                string query = "DELETE FROM IAP_TD_LOGS WHERE feRegistro < @fechaLimite";

                return db.EjecutarComando(query, 
                    DatabaseHelper.CrearParametro("@fechaLimite", fechaLimite));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al limpiar logs antiguos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Limpia archivos de log antiguos
        /// </summary>
        public static void LimpiarArchivosLogsAntiguos(int diasAntiguedad = 90)
        {
            try
            {
                DateTime fechaLimite = DateTime.Now.AddDays(-diasAntiguedad);

                var archivos = Directory.GetFiles(RutaLogs, "Log_*.txt");

                foreach (var archivo in archivos)
                {
                    FileInfo fileInfo = new FileInfo(archivo);
                    if (fileInfo.LastWriteTime < fechaLimite)
                    {
                        File.Delete(archivo);
                    }
                }
            }
            catch (Exception ex)
            {
                EscribirLogEmergencia($"Error al limpiar archivos de log: {ex.Message}");
            }
        }
    }
}
