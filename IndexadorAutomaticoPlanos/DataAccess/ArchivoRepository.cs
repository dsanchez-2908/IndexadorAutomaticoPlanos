using System.Data;
using System.Data.SqlClient;
using IndexadorAutomaticoPlanos.Entities;
using IndexadorAutomaticoPlanos.Utils;

namespace IndexadorAutomaticoPlanos.DataAccess
{
    /// <summary>
    /// Repositorio para operaciones CRUD de archivos PDF
    /// </summary>
    public class ArchivoRepository
    {
        private readonly DatabaseHelper _db;

        public ArchivoRepository()
        {
            _db = new DatabaseHelper();
        }

        /// <summary>
        /// Inserta un nuevo archivo en la base de datos
        /// </summary>
        public int Insertar(Archivo archivo)
        {
            try
            {
                string query = @"
                    INSERT INTO IAP_TD_ARCHIVOS 
                    (dsNombreArchivo, dsRutaCompleta, dsNombreUltimaCarpeta, nuTamanoBytes,
                     nuCantidadPaginas, feModificacionArchivo, cdEstadoArchivo, 
                     feAlta, cdUsuarioAlta)
                    VALUES 
                    (@dsNombreArchivo, @dsRutaCompleta, @dsNombreUltimaCarpeta, @nuTamanoBytes,
                     @nuCantidadPaginas, @feModificacionArchivo, @cdEstadoArchivo, 
                     GETDATE(), @cdUsuarioAlta);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                object? resultado = _db.EjecutarEscalar(query,
                    DatabaseHelper.CrearParametro("@dsNombreArchivo", archivo.DsNombreArchivo),
                    DatabaseHelper.CrearParametro("@dsRutaCompleta", archivo.DsRutaCompleta),
                    DatabaseHelper.CrearParametro("@dsNombreUltimaCarpeta", (object?)archivo.DsNombreUltimaCarpeta ?? DBNull.Value),
                    DatabaseHelper.CrearParametro("@nuTamanoBytes", archivo.NuTamanoBytes),
                    DatabaseHelper.CrearParametro("@nuCantidadPaginas", archivo.NuCantidadPaginas),
                    DatabaseHelper.CrearParametro("@feModificacionArchivo", archivo.FeModificacionArchivo),
                    DatabaseHelper.CrearParametro("@cdEstadoArchivo", archivo.CdEstadoArchivo),
                    DatabaseHelper.CrearParametro("@cdUsuarioAlta", archivo.CdUsuarioAlta));

                return resultado != null ? Convert.ToInt32(resultado) : 0;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al insertar archivo {archivo.DsNombreArchivo}", ex, "ArchivoRepository");
                throw new Exception($"Error al insertar archivo: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verifica si un archivo ya existe en la base de datos por ruta completa y nombre
        /// </summary>
        public bool ExisteArchivo(string rutaCompleta, string nombreArchivo)
        {
            try
            {
                string query = @"
                    SELECT COUNT(1) 
                    FROM IAP_TD_ARCHIVOS 
                    WHERE dsRutaCompleta = @rutaCompleta 
                      AND dsNombreArchivo = @nombreArchivo";

                object? resultado = _db.EjecutarEscalar(query,
                    DatabaseHelper.CrearParametro("@rutaCompleta", rutaCompleta),
                    DatabaseHelper.CrearParametro("@nombreArchivo", nombreArchivo));

                return resultado != null && Convert.ToInt32(resultado) > 0;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al verificar existencia de archivo {nombreArchivo}", ex, "ArchivoRepository");
                throw new Exception($"Error al verificar archivo: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene todos los archivos con información de estado
        /// </summary>
        public List<Archivo> ObtenerTodos()
        {
            try
            {
                string query = @"
                    SELECT 
                        a.cdArchivo, a.dsNombreArchivo, a.dsRutaCompleta, 
                        a.dsNombreUltimaCarpeta, a.nuTamanoBytes, a.nuCantidadPaginas,
                        a.feModificacionArchivo, a.cdEstadoArchivo, 
                        a.feAlta, a.cdUsuarioAlta, a.feUltimaModificacion, 
                        a.cdUsuarioModificacion, e.dsEstado as DsEstadoArchivo
                    FROM IAP_TD_ARCHIVOS a
                    INNER JOIN IAP_TV_ESTADOS_ARCHIVO e ON a.cdEstadoArchivo = e.cdEstadoArchivo
                    ORDER BY a.feAlta DESC";

                DataTable dt = _db.EjecutarConsulta(query);
                return MapearListaArchivos(dt);
            }
            catch (Exception ex)
            {
                Logger.Error("Error al obtener todos los archivos", ex, "ArchivoRepository");
                throw new Exception($"Error al obtener archivos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene archivos filtrados por estado
        /// </summary>
        public List<Archivo> ObtenerPorEstado(int cdEstado)
        {
            try
            {
                string query = @"
                    SELECT 
                        a.cdArchivo, a.dsNombreArchivo, a.dsRutaCompleta, 
                        a.dsNombreUltimaCarpeta, a.nuTamanoBytes, a.nuCantidadPaginas,
                        a.feModificacionArchivo, a.cdEstadoArchivo, 
                        a.feAlta, a.cdUsuarioAlta, a.feUltimaModificacion, 
                        a.cdUsuarioModificacion, e.dsEstado as DsEstadoArchivo
                    FROM IAP_TD_ARCHIVOS a
                    INNER JOIN IAP_TV_ESTADOS_ARCHIVO e ON a.cdEstadoArchivo = e.cdEstadoArchivo
                    WHERE a.cdEstadoArchivo = @cdEstado
                    ORDER BY a.feAlta DESC";

                DataTable dt = _db.EjecutarConsulta(query,
                    DatabaseHelper.CrearParametro("@cdEstado", cdEstado));

                return MapearListaArchivos(dt);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al obtener archivos por estado {cdEstado}", ex, "ArchivoRepository");
                throw new Exception($"Error al obtener archivos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene un archivo por su ID
        /// </summary>
        public Archivo? ObtenerPorId(int cdArchivo)
        {
            try
            {
                string query = @"
                    SELECT 
                        a.cdArchivo, a.dsNombreArchivo, a.dsRutaCompleta, 
                        a.dsNombreUltimaCarpeta, a.nuTamanoBytes, a.nuCantidadPaginas,
                        a.feModificacionArchivo, a.cdEstadoArchivo, 
                        a.feAlta, a.cdUsuarioAlta, a.feUltimaModificacion, 
                        a.cdUsuarioModificacion, e.dsEstado as DsEstadoArchivo
                    FROM IAP_TD_ARCHIVOS a
                    INNER JOIN IAP_TV_ESTADOS_ARCHIVO e ON a.cdEstadoArchivo = e.cdEstadoArchivo
                    WHERE a.cdArchivo = @cdArchivo";

                DataTable dt = _db.EjecutarConsulta(query,
                    DatabaseHelper.CrearParametro("@cdArchivo", cdArchivo));

                if (dt.Rows.Count == 0)
                    return null;

                return MapearArchivo(dt.Rows[0]);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al obtener archivo por ID {cdArchivo}", ex, "ArchivoRepository");
                throw new Exception($"Error al obtener archivo: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza el estado de un archivo
        /// </summary>
        public bool ActualizarEstado(int cdArchivo, int nuevoEstado, int cdUsuarioModificacion)
        {
            try
            {
                string query = @"
                    UPDATE IAP_TD_ARCHIVOS
                    SET cdEstadoArchivo = @nuevoEstado,
                        feUltimaModificacion = GETDATE(),
                        cdUsuarioModificacion = @cdUsuarioModificacion
                    WHERE cdArchivo = @cdArchivo";

                int filasAfectadas = _db.EjecutarComando(query,
                    DatabaseHelper.CrearParametro("@cdArchivo", cdArchivo),
                    DatabaseHelper.CrearParametro("@nuevoEstado", nuevoEstado),
                    DatabaseHelper.CrearParametro("@cdUsuarioModificacion", cdUsuarioModificacion));

                return filasAfectadas > 0;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al actualizar estado del archivo {cdArchivo}", ex, "ArchivoRepository");
                throw new Exception($"Error al actualizar estado: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Elimina lógicamente un archivo (cambia su estado a Eliminado)
        /// </summary>
        public bool EliminarLogico(int cdArchivo, int cdUsuarioModificacion)
        {
            try
            {
                // Obtener el ID del estado "Eliminado" (asumimos cdEstado = 5)
                // En una implementación más robusta, deberíamos buscarlo dinámicamente
                return ActualizarEstado(cdArchivo, 5, cdUsuarioModificacion);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al eliminar lógicamente archivo {cdArchivo}", ex, "ArchivoRepository");
                throw new Exception($"Error al eliminar archivo: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene la cantidad de archivos por estado
        /// </summary>
        public Dictionary<string, int> ObtenerEstadisticas()
        {
            try
            {
                string query = @"
                    SELECT e.dsEstado, COUNT(*) as Cantidad
                    FROM IAP_TD_ARCHIVOS a
                    INNER JOIN IAP_TV_ESTADOS_ARCHIVO e ON a.cdEstadoArchivo = e.cdEstadoArchivo
                    GROUP BY e.dsEstado";

                DataTable dt = _db.EjecutarConsulta(query);
                Dictionary<string, int> estadisticas = new Dictionary<string, int>();

                foreach (DataRow row in dt.Rows)
                {
                    string estado = row["dsEstado"].ToString() ?? "Sin Estado";
                    int cantidad = Convert.ToInt32(row["Cantidad"]);
                    estadisticas[estado] = cantidad;
                }

                return estadisticas;
            }
            catch (Exception ex)
            {
                Logger.Error("Error al obtener estadísticas de archivos", ex, "ArchivoRepository");
                throw new Exception($"Error al obtener estadísticas: {ex.Message}", ex);
            }
        }

        #region Métodos Privados de Mapeo

        private Archivo MapearArchivo(DataRow row)
        {
            return new Archivo
            {
                CdArchivo = Convert.ToInt32(row["cdArchivo"]),
                DsNombreArchivo = row["dsNombreArchivo"].ToString() ?? string.Empty,
                DsRutaCompleta = row["dsRutaCompleta"].ToString() ?? string.Empty,
                DsNombreUltimaCarpeta = row["dsNombreUltimaCarpeta"] == DBNull.Value ? null : row["dsNombreUltimaCarpeta"].ToString(),
                NuTamanoBytes = Convert.ToInt64(row["nuTamanoBytes"]),
                NuCantidadPaginas = Convert.ToInt32(row["nuCantidadPaginas"]),
                FeModificacionArchivo = Convert.ToDateTime(row["feModificacionArchivo"]),
                CdEstadoArchivo = Convert.ToInt32(row["cdEstadoArchivo"]),
                FeAlta = Convert.ToDateTime(row["feAlta"]),
                CdUsuarioAlta = Convert.ToInt32(row["cdUsuarioAlta"]),
                FeUltimaModificacion = row["feUltimaModificacion"] == DBNull.Value ? null : Convert.ToDateTime(row["feUltimaModificacion"]),
                CdUsuarioModificacion = row["cdUsuarioModificacion"] == DBNull.Value ? null : Convert.ToInt32(row["cdUsuarioModificacion"]),
                DsEstadoArchivo = row["DsEstadoArchivo"].ToString()
            };
        }

        public List<Archivo> MapearListaArchivos(DataTable dt)
        {
            List<Archivo> archivos = new List<Archivo>();
            foreach (DataRow row in dt.Rows)
            {
                archivos.Add(MapearArchivo(row));
            }
            return archivos;
        }

        #endregion
    }
}
