using System.Data;
using System.Data.SqlClient;
using IndexadorAutomaticoPlanos.Entities;
using IndexadorAutomaticoPlanos.Utils;

namespace IndexadorAutomaticoPlanos.DataAccess
{
    /// <summary>
    /// Repositorio para operaciones CRUD de lotes y su relación con archivos
    /// </summary>
    public class LoteRepository
    {
        private readonly DatabaseHelper _db;
        private readonly ArchivoRepository _archivoRepo;

        public LoteRepository()
        {
            _db = new DatabaseHelper();
            _archivoRepo = new ArchivoRepository();
        }

        /// <summary>
        /// Obtiene el siguiente número secuencial para un lote
        /// </summary>
        public int ObtenerSiguienteNumeroLote()
        {
            try
            {
                string query = @"
                    SELECT ISNULL(MAX(CAST(SUBSTRING(dsNombreLote, 6, 6) AS INT)), 0) + 1
                    FROM IAP_TD_LOTES
                    WHERE dsNombreLote LIKE 'LOTE_%'";

                object? resultado = _db.EjecutarEscalar(query);
                return resultado != null ? Convert.ToInt32(resultado) : 1;
            }
            catch (Exception ex)
            {
                Logger.Error("Error al obtener siguiente número de lote", ex, "LoteRepository");
                throw new Exception($"Error al obtener número de lote: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Genera el nombre del lote en formato LOTE_NNNNNN
        /// </summary>
        public string GenerarNombreLote()
        {
            int numero = ObtenerSiguienteNumeroLote();
            return $"LOTE_{numero:D6}";
        }

        /// <summary>
        /// Obtiene archivos disponibles para asociar a lotes (estado = Ingresado)
        /// Agrupados por carpeta
        /// </summary>
        public List<Archivo> ObtenerArchivosDisponibles()
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
                    WHERE e.dsEstado = 'Ingresado'
                    ORDER BY a.dsNombreUltimaCarpeta, a.dsNombreArchivo";

                DataTable dt = _db.EjecutarConsulta(query);
                return _archivoRepo.MapearListaArchivos(dt);
            }
            catch (Exception ex)
            {
                Logger.Error("Error al obtener archivos disponibles", ex, "LoteRepository");
                throw new Exception($"Error al obtener archivos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene estadísticas de archivos disponibles agrupados por carpeta
        /// </summary>
        public DataTable ObtenerEstadisticasPorCarpeta()
        {
            try
            {
                string query = @"
                    SELECT 
                        a.dsNombreUltimaCarpeta AS Carpeta,
                        COUNT(*) AS CantidadArchivos,
                        SUM(a.nuCantidadPaginas) AS TotalPaginas,
                        MIN(a.feAlta) AS FechaIngreso
                    FROM IAP_TD_ARCHIVOS a
                    INNER JOIN IAP_TV_ESTADOS_ARCHIVO e ON a.cdEstadoArchivo = e.cdEstadoArchivo
                    WHERE e.dsEstado = 'Ingresado'
                    GROUP BY a.dsNombreUltimaCarpeta
                    ORDER BY a.dsNombreUltimaCarpeta";

                return _db.EjecutarConsulta(query);
            }
            catch (Exception ex)
            {
                Logger.Error("Error al obtener estadísticas por carpeta", ex, "LoteRepository");
                throw new Exception($"Error al obtener estadísticas: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Crea un nuevo lote y asocia archivos
        /// Respeta la regla: archivos de la misma carpeta deben estar juntos
        /// </summary>
        public int CrearLote(List<int> idsArchivos, int cdUsuarioAlta)
        {
            SqlConnection? conn = null;
            SqlTransaction? transaction = null;

            try
            {
                // Validar que todos los archivos sean de la misma carpeta
                string queryValidacion = @"
                    SELECT DISTINCT dsNombreUltimaCarpeta 
                    FROM IAP_TD_ARCHIVOS 
                    WHERE cdArchivo IN ({0})";

                string idsString = string.Join(",", idsArchivos);
                DataTable dtCarpetas = _db.EjecutarConsulta(string.Format(queryValidacion, idsString));

                if (dtCarpetas.Rows.Count > 1)
                {
                    throw new Exception("No se pueden asociar archivos de diferentes carpetas al mismo lote.");
                }

                conn = _db.ObtenerConexion();
                conn.Open();
                transaction = conn.BeginTransaction();

                // Generar nombre del lote
                string nombreLote = GenerarNombreLote();

                // Insertar el lote
                string queryLote = @"
                    INSERT INTO IAP_TD_LOTES 
                    (dsNombreLote, cdEstadoLote, nuCantidadArchivos, feAlta, cdUsuarioAlta)
                    VALUES 
                    (@dsNombreLote, 1, @nuCantidadArchivos, GETDATE(), @cdUsuarioAlta);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                SqlCommand cmdLote = new SqlCommand(queryLote, conn, transaction);
                cmdLote.Parameters.AddWithValue("@dsNombreLote", nombreLote);
                cmdLote.Parameters.AddWithValue("@nuCantidadArchivos", idsArchivos.Count);
                cmdLote.Parameters.AddWithValue("@cdUsuarioAlta", cdUsuarioAlta);

                int cdLote = (int)cmdLote.ExecuteScalar();

                // Asociar archivos al lote
                string queryAsociar = @"
                    INSERT INTO IAP_TD_LOTE_ARCHIVOS 
                    (cdLote, cdArchivo, cdEstadoArchivoLote, nuOrden, feAlta, cdUsuarioAlta)
                    VALUES 
                    (@cdLote, @cdArchivo, 1, @nuOrden, GETDATE(), @cdUsuarioAlta)";

                for (int i = 0; i < idsArchivos.Count; i++)
                {
                    SqlCommand cmdAsociar = new SqlCommand(queryAsociar, conn, transaction);
                    cmdAsociar.Parameters.AddWithValue("@cdLote", cdLote);
                    cmdAsociar.Parameters.AddWithValue("@cdArchivo", idsArchivos[i]);
                    cmdAsociar.Parameters.AddWithValue("@nuOrden", i + 1);
                    cmdAsociar.Parameters.AddWithValue("@cdUsuarioAlta", cdUsuarioAlta);
                    cmdAsociar.ExecuteNonQuery();
                }

                // Actualizar estado de archivos a "Asociado a Lote"
                string queryActualizarEstado = @"
                    UPDATE IAP_TD_ARCHIVOS 
                    SET cdEstadoArchivo = (SELECT cdEstadoArchivo FROM IAP_TV_ESTADOS_ARCHIVO WHERE dsEstado = 'Asociado a Lote'),
                        feUltimaModificacion = GETDATE(),
                        cdUsuarioModificacion = @cdUsuario
                    WHERE cdArchivo IN ({0})";

                SqlCommand cmdActualizar = new SqlCommand(string.Format(queryActualizarEstado, idsString), conn, transaction);
                cmdActualizar.Parameters.AddWithValue("@cdUsuario", cdUsuarioAlta);
                cmdActualizar.ExecuteNonQuery();

                transaction.Commit();

                Logger.Info($"Lote {nombreLote} creado con {idsArchivos.Count} archivos (ID: {cdLote})", "LoteRepository");
                return cdLote;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Logger.Error($"Error al crear lote", ex, "LoteRepository");
                throw new Exception($"Error al crear lote: {ex.Message}", ex);
            }
            finally
            {
                transaction?.Dispose();
                conn?.Close();
                conn?.Dispose();
            }
        }

        /// <summary>
        /// Obtiene todos los lotes con información detallada
        /// </summary>
        public List<Lote> ObtenerTodosLotes()
        {
            try
            {
                string query = @"
                    SELECT 
                        l.cdLote, l.dsNombreLote, l.cdEstadoLote, l.nuCantidadArchivos,
                        l.feAlta, l.cdUsuarioAlta, l.feUltimaModificacion, l.cdUsuarioModificacion,
                        el.dsEstado AS DsEstadoLote
                    FROM IAP_TD_LOTES l
                    INNER JOIN IAP_TV_ESTADOS_LOTE el ON l.cdEstadoLote = el.cdEstadoLote
                    ORDER BY l.cdLote DESC";

                DataTable dt = _db.EjecutarConsulta(query);
                return MapearListaLotes(dt);
            }
            catch (Exception ex)
            {
                Logger.Error("Error al obtener lotes", ex, "LoteRepository");
                throw new Exception($"Error al obtener lotes: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene los archivos asociados a un lote específico
        /// </summary>
        public List<LoteArchivo> ObtenerArchivosPorLote(int cdLote)
        {
            try
            {
                string query = @"
                    SELECT 
                        la.cdLoteArchivo, la.cdLote, la.cdArchivo, la.cdEstadoArchivoLote,
                        la.nuOrden, la.feAlta, la.cdUsuarioAlta, la.feUltimaModificacion,
                        la.cdUsuarioModificacion,
                        eal.dsEstado AS DsEstadoArchivoLote,
                        a.dsNombreArchivo AS DsNombreArchivo,
                        a.dsRutaCompleta + '\' + a.dsNombreArchivo AS DsRutaCompletaArchivo,
                        a.nuCantidadPaginas AS NuCantidadPaginas,
                        a.dsNombreUltimaCarpeta AS DsNombreUltimaCarpeta,
                        l.dsNombreLote AS DsNombreLote
                    FROM IAP_TD_LOTE_ARCHIVOS la
                    INNER JOIN IAP_TV_ESTADOS_ARCHIVO_LOTE eal ON la.cdEstadoArchivoLote = eal.cdEstadoArchivoLote
                    INNER JOIN IAP_TD_ARCHIVOS a ON la.cdArchivo = a.cdArchivo
                    INNER JOIN IAP_TD_LOTES l ON la.cdLote = l.cdLote
                    WHERE la.cdLote = @cdLote
                    ORDER BY la.nuOrden";

                DataTable dt = _db.EjecutarConsulta(query,
                    DatabaseHelper.CrearParametro("@cdLote", cdLote));

                return MapearListaLoteArchivos(dt);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al obtener archivos del lote {cdLote}", ex, "LoteRepository");
                throw new Exception($"Error al obtener archivos del lote: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Divide un lote existente en dos lotes
        /// El lote original mantiene la primera mitad, se crea un nuevo lote con la segunda mitad
        /// </summary>
        public int DividirLote(int cdLoteOriginal, int cdUsuario)
        {
            SqlConnection? conn = null;
            SqlTransaction? transaction = null;

            try
            {
                // Obtener archivos del lote original
                List<LoteArchivo> archivos = ObtenerArchivosPorLote(cdLoteOriginal);

                if (archivos.Count < 2)
                {
                    throw new Exception("No se puede dividir un lote con menos de 2 archivos.");
                }

                conn = _db.ObtenerConexion();
                conn.Open();
                transaction = conn.BeginTransaction();

                // Calcular punto de división
                int puntoDiv = archivos.Count / 2;
                List<int> idsSegundaMitad = archivos.Skip(puntoDiv).Select(a => a.CdArchivo).ToList();

                // Obtener nombre del lote original
                string queryNombreOriginal = "SELECT dsNombreLote FROM IAP_TD_LOTES WHERE cdLote = @cdLote";
                SqlCommand cmdNombreOriginal = new SqlCommand(queryNombreOriginal, conn, transaction);
                cmdNombreOriginal.Parameters.AddWithValue("@cdLote", cdLoteOriginal);
                string nombreOriginal = (string)cmdNombreOriginal.ExecuteScalar();

                // Crear nuevo lote
                string nombreNuevoLote = GenerarNombreLote();
                string queryNuevoLote = @"
                    INSERT INTO IAP_TD_LOTES 
                    (dsNombreLote, cdEstadoLote, nuCantidadArchivos, feAlta, cdUsuarioAlta)
                    VALUES 
                    (@dsNombreLote, 1, @nuCantidadArchivos, GETDATE(), @cdUsuario);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                SqlCommand cmdNuevoLote = new SqlCommand(queryNuevoLote, conn, transaction);
                cmdNuevoLote.Parameters.AddWithValue("@dsNombreLote", nombreNuevoLote);
                cmdNuevoLote.Parameters.AddWithValue("@nuCantidadArchivos", idsSegundaMitad.Count);
                cmdNuevoLote.Parameters.AddWithValue("@cdUsuario", cdUsuario);

                int cdNuevoLote = (int)cmdNuevoLote.ExecuteScalar();

                // Mover archivos de segunda mitad al nuevo lote
                string queryMover = @"
                    UPDATE IAP_TD_LOTE_ARCHIVOS 
                    SET cdLote = @cdNuevoLote,
                        feUltimaModificacion = GETDATE(),
                        cdUsuarioModificacion = @cdUsuario
                    WHERE cdArchivo IN ({0})";

                string idsString = string.Join(",", idsSegundaMitad);
                SqlCommand cmdMover = new SqlCommand(string.Format(queryMover, idsString), conn, transaction);
                cmdMover.Parameters.AddWithValue("@cdNuevoLote", cdNuevoLote);
                cmdMover.Parameters.AddWithValue("@cdUsuario", cdUsuario);
                cmdMover.ExecuteNonQuery();

                // Actualizar cantidad de archivos en lote original
                string queryActualizarOriginal = @"
                    UPDATE IAP_TD_LOTES 
                    SET nuCantidadArchivos = @nuCantidad,
                        feUltimaModificacion = GETDATE(),
                        cdUsuarioModificacion = @cdUsuario
                    WHERE cdLote = @cdLote";

                SqlCommand cmdActualizarOriginal = new SqlCommand(queryActualizarOriginal, conn, transaction);
                cmdActualizarOriginal.Parameters.AddWithValue("@nuCantidad", puntoDiv);
                cmdActualizarOriginal.Parameters.AddWithValue("@cdUsuario", cdUsuario);
                cmdActualizarOriginal.Parameters.AddWithValue("@cdLote", cdLoteOriginal);
                cmdActualizarOriginal.ExecuteNonQuery();

                transaction.Commit();

                Logger.Info($"Lote {nombreOriginal} dividido: {puntoDiv} archivos quedan en original, {idsSegundaMitad.Count} movidos a {nombreNuevoLote}", "LoteRepository");
                return cdNuevoLote;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Logger.Error($"Error al dividir lote {cdLoteOriginal}", ex, "LoteRepository");
                throw new Exception($"Error al dividir lote: {ex.Message}", ex);
            }
            finally
            {
                transaction?.Dispose();
                conn?.Close();
                conn?.Dispose();
            }
        }

        /// <summary>
        /// Actualiza el estado de un lote
        /// </summary>
        public bool ActualizarEstadoLote(int cdLote, int cdNuevoEstado, int cdUsuario)
        {
            try
            {
                string query = @"
                    UPDATE IAP_TD_LOTES 
                    SET cdEstadoLote = @cdEstado,
                        feUltimaModificacion = GETDATE(),
                        cdUsuarioModificacion = @cdUsuario
                    WHERE cdLote = @cdLote";

                int filasAfectadas = _db.EjecutarComando(query,
                    DatabaseHelper.CrearParametro("@cdEstado", cdNuevoEstado),
                    DatabaseHelper.CrearParametro("@cdUsuario", cdUsuario),
                    DatabaseHelper.CrearParametro("@cdLote", cdLote));

                Logger.Info($"Estado del lote {cdLote} actualizado a estado {cdNuevoEstado}", "LoteRepository");
                return filasAfectadas > 0;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al actualizar estado del lote {cdLote}", ex, "LoteRepository");
                throw new Exception($"Error al actualizar estado: {ex.Message}", ex);
            }
        }

        #region Mapeo de entidades

        private List<Lote> MapearListaLotes(DataTable dt)
        {
            List<Lote> lotes = new List<Lote>();
            foreach (DataRow row in dt.Rows)
            {
                lotes.Add(MapearLote(row));
            }
            return lotes;
        }

        private Lote MapearLote(DataRow row)
        {
            return new Lote
            {
                CdLote = Convert.ToInt32(row["cdLote"]),
                DsNombreLote = row["dsNombreLote"].ToString() ?? string.Empty,
                CdEstadoLote = Convert.ToInt32(row["cdEstadoLote"]),
                NuCantidadArchivos = Convert.ToInt32(row["nuCantidadArchivos"]),
                FeAlta = Convert.ToDateTime(row["feAlta"]),
                CdUsuarioAlta = Convert.ToInt32(row["cdUsuarioAlta"]),
                FeUltimaModificacion = row["feUltimaModificacion"] != DBNull.Value ? Convert.ToDateTime(row["feUltimaModificacion"]) : null,
                CdUsuarioModificacion = row["cdUsuarioModificacion"] != DBNull.Value ? Convert.ToInt32(row["cdUsuarioModificacion"]) : null,
                DsEstadoLote = row["DsEstadoLote"].ToString()
            };
        }

        private List<LoteArchivo> MapearListaLoteArchivos(DataTable dt)
        {
            List<LoteArchivo> loteArchivos = new List<LoteArchivo>();
            foreach (DataRow row in dt.Rows)
            {
                loteArchivos.Add(MapearLoteArchivo(row));
            }
            return loteArchivos;
        }

        private LoteArchivo MapearLoteArchivo(DataRow row)
        {
            return new LoteArchivo
            {
                CdLoteArchivo = Convert.ToInt32(row["cdLoteArchivo"]),
                CdLote = Convert.ToInt32(row["cdLote"]),
                CdArchivo = Convert.ToInt32(row["cdArchivo"]),
                CdEstadoArchivoLote = Convert.ToInt32(row["cdEstadoArchivoLote"]),
                NuOrden = row["nuOrden"] != DBNull.Value ? Convert.ToInt32(row["nuOrden"]) : null,
                FeAlta = Convert.ToDateTime(row["feAlta"]),
                CdUsuarioAlta = Convert.ToInt32(row["cdUsuarioAlta"]),
                FeUltimaModificacion = row["feUltimaModificacion"] != DBNull.Value ? Convert.ToDateTime(row["feUltimaModificacion"]) : null,
                CdUsuarioModificacion = row["cdUsuarioModificacion"] != DBNull.Value ? Convert.ToInt32(row["cdUsuarioModificacion"]) : null,
                DsEstadoArchivoLote = row["DsEstadoArchivoLote"].ToString(),
                DsNombreArchivo = row["DsNombreArchivo"].ToString(),
                DsRutaCompletaArchivo = row["DsRutaCompletaArchivo"].ToString(),
                NuCantidadPaginas = row["NuCantidadPaginas"] != DBNull.Value ? Convert.ToInt32(row["NuCantidadPaginas"]) : null,
                DsNombreUltimaCarpeta = row["DsNombreUltimaCarpeta"].ToString(),
                DsNombreLote = row["DsNombreLote"].ToString()
            };
        }

        #endregion

        #region Métodos para Preparación de Imágenes (Fase 4)

        /// <summary>
        /// Actualiza los datos de imagen procesada en un archivo de lote
        /// </summary>
        public void ActualizarDatosImagen(
            int cdLoteArchivo,
            string rutaImagenJpg,
            string imagenBase64,
            string? resultadoOcr,
            bool tieneOcr,
            int dpi,
            string esquinaRecorte,
            decimal porcentajeRecorteHorizontal,
            decimal porcentajeRecorteVertical,
            int cdUsuarioModificacion)
        {
            try
            {
                string query = @"
                    UPDATE IAP_TD_LOTE_ARCHIVOS
                    SET dsRutaImagenJpg = @dsRutaImagenJpg,
                        txImagenBase64 = @txImagenBase64,
                        txResultadoOcr = @txResultadoOcr,
                        snTieneOcr = @snTieneOcr,
                        nuDpiProcesamiento = @nuDpiProcesamiento,
                        dsEsquinaRecorte = @dsEsquinaRecorte,
                        nuPorcentajeRecorteHorizontal = @nuPorcentajeRecorteHorizontal,
                        nuPorcentajeRecorteVertical = @nuPorcentajeRecorteVertical,
                        feUltimaModificacion = GETDATE(),
                        cdUsuarioModificacion = @cdUsuarioModificacion
                    WHERE cdLoteArchivo = @cdLoteArchivo";

                _db.EjecutarComando(query,
                    DatabaseHelper.CrearParametro("@cdLoteArchivo", cdLoteArchivo),
                    DatabaseHelper.CrearParametro("@dsRutaImagenJpg", rutaImagenJpg),
                    DatabaseHelper.CrearParametro("@txImagenBase64", imagenBase64),
                    DatabaseHelper.CrearParametro("@txResultadoOcr", (object?)resultadoOcr ?? DBNull.Value),
                    DatabaseHelper.CrearParametro("@snTieneOcr", tieneOcr),
                    DatabaseHelper.CrearParametro("@nuDpiProcesamiento", dpi),
                    DatabaseHelper.CrearParametro("@dsEsquinaRecorte", esquinaRecorte),
                    DatabaseHelper.CrearParametro("@nuPorcentajeRecorteHorizontal", porcentajeRecorteHorizontal),
                    DatabaseHelper.CrearParametro("@nuPorcentajeRecorteVertical", porcentajeRecorteVertical),
                    DatabaseHelper.CrearParametro("@cdUsuarioModificacion", cdUsuarioModificacion));

                Logger.Info($"Datos de imagen actualizados para LoteArchivo {cdLoteArchivo}", "LoteRepository");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al actualizar datos de imagen para LoteArchivo {cdLoteArchivo}", ex, "LoteRepository");
                throw new Exception($"Error al actualizar datos de imagen: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza el estado de un archivo dentro de un lote
        /// </summary>
        public void ActualizarEstadoArchivoLote(int cdLoteArchivo, int nuevoEstado, int cdUsuarioModificacion)
        {
            try
            {
                string query = @"
                    UPDATE IAP_TD_LOTE_ARCHIVOS
                    SET cdEstadoArchivoLote = @cdEstadoArchivoLote,
                        feUltimaModificacion = GETDATE(),
                        cdUsuarioModificacion = @cdUsuarioModificacion
                    WHERE cdLoteArchivo = @cdLoteArchivo";

                _db.EjecutarComando(query,
                    DatabaseHelper.CrearParametro("@cdLoteArchivo", cdLoteArchivo),
                    DatabaseHelper.CrearParametro("@cdEstadoArchivoLote", nuevoEstado),
                    DatabaseHelper.CrearParametro("@cdUsuarioModificacion", cdUsuarioModificacion));

                Logger.Info($"Estado actualizado para LoteArchivo {cdLoteArchivo} a estado {nuevoEstado}", "LoteRepository");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al actualizar estado de LoteArchivo {cdLoteArchivo}", ex, "LoteRepository");
                throw new Exception($"Error al actualizar estado de archivo: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza el estado de un lote y todos sus archivos en una transacción
        /// </summary>
        public void ActualizarEstadoLoteYArchivos(int cdLote, int nuevoEstadoLote, int nuevoEstadoArchivo, int cdUsuarioModificacion)
        {
            SqlConnection? conn = null;
            SqlTransaction? transaction = null;

            try
            {
                conn = _db.ObtenerConexion();
                conn.Open();
                transaction = conn.BeginTransaction();

                // Actualizar estado del lote
                string queryLote = @"
                    UPDATE IAP_TD_LOTES
                    SET cdEstadoLote = @cdEstadoLote,
                        feUltimaModificacion = GETDATE(),
                        cdUsuarioModificacion = @cdUsuarioModificacion
                    WHERE cdLote = @cdLote";

                using (var cmdLote = new SqlCommand(queryLote, conn, transaction))
                {
                    cmdLote.Parameters.AddWithValue("@cdLote", cdLote);
                    cmdLote.Parameters.AddWithValue("@cdEstadoLote", nuevoEstadoLote);
                    cmdLote.Parameters.AddWithValue("@cdUsuarioModificacion", cdUsuarioModificacion);
                    cmdLote.ExecuteNonQuery();
                }

                // Actualizar estado de todos los archivos del lote
                string queryArchivos = @"
                    UPDATE IAP_TD_LOTE_ARCHIVOS
                    SET cdEstadoArchivoLote = @cdEstadoArchivoLote,
                        feUltimaModificacion = GETDATE(),
                        cdUsuarioModificacion = @cdUsuarioModificacion
                    WHERE cdLote = @cdLote";

                using (var cmdArchivos = new SqlCommand(queryArchivos, conn, transaction))
                {
                    cmdArchivos.Parameters.AddWithValue("@cdLote", cdLote);
                    cmdArchivos.Parameters.AddWithValue("@cdEstadoArchivoLote", nuevoEstadoArchivo);
                    cmdArchivos.Parameters.AddWithValue("@cdUsuarioModificacion", cdUsuarioModificacion);
                    cmdArchivos.ExecuteNonQuery();
                }

                transaction.Commit();
                Logger.Info($"Estado actualizado para Lote {cdLote}: Lote={nuevoEstadoLote}, Archivos={nuevoEstadoArchivo}", "LoteRepository");
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Logger.Error($"Error al actualizar estados del Lote {cdLote}", ex, "LoteRepository");
                throw new Exception($"Error al actualizar estados del lote: {ex.Message}", ex);
            }
            finally
            {
                transaction?.Dispose();
                conn?.Close();
                conn?.Dispose();
            }
        }

        /// <summary>
        /// Obtiene los lotes en estado "Pendiente de Preparar Imágenes"
        /// </summary>
        public List<Lote> ObtenerLotesPendientesPreparacion()
        {
            try
            {
                string query = @"
                    SELECT 
                        l.cdLote, l.dsNombreLote, l.cdEstadoLote, l.nuCantidadArchivos,
                        l.feAlta, l.cdUsuarioAlta, l.feUltimaModificacion, l.cdUsuarioModificacion,
                        e.dsEstado AS DsEstadoLote
                    FROM IAP_TD_LOTES l
                    INNER JOIN IAP_TV_ESTADOS_LOTE e ON l.cdEstadoLote = e.cdEstadoLote
                    WHERE l.cdEstadoLote = 1
                    ORDER BY l.feAlta";

                DataTable dt = _db.EjecutarConsulta(query);
                return MapearListaLotes(dt);
            }
            catch (Exception ex)
            {
                Logger.Error("Error al obtener lotes pendientes de preparación", ex, "LoteRepository");
                throw new Exception($"Error al obtener lotes pendientes: {ex.Message}", ex);
            }
        }

        #endregion

        #region Métodos para Procesamiento por IA (Fase 5)

        /// <summary>
        /// Obtiene los lotes en estado "Pendiente de Procesamiento por IA" (estado 2)
        /// </summary>
        public List<Lote> ObtenerLotesPendientesProcesarIA()
        {
            try
            {
                string query = @"
                    SELECT 
                        l.cdLote, l.dsNombreLote, l.cdEstadoLote, l.nuCantidadArchivos,
                        l.feAlta, l.cdUsuarioAlta, l.feUltimaModificacion, l.cdUsuarioModificacion,
                        e.dsEstado AS DsEstadoLote
                    FROM IAP_TD_LOTES l
                    INNER JOIN IAP_TV_ESTADOS_LOTE e ON l.cdEstadoLote = e.cdEstadoLote
                    WHERE l.cdEstadoLote = 2
                    ORDER BY l.feAlta";

                DataTable dt = _db.EjecutarConsulta(query);
                return MapearListaLotes(dt);
            }
            catch (Exception ex)
            {
                Logger.Error("Error al obtener lotes pendientes de procesar por IA", ex, "LoteRepository");
                throw new Exception($"Error al obtener lotes pendientes de IA: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene los archivos de un lote con sus datos de imagen y OCR para procesamiento por IA
        /// </summary>
        public List<LoteArchivo> ObtenerArchivosParaProcesar(int cdLote)
        {
            try
            {
                string query = @"
                    SELECT 
                        la.cdLoteArchivo, la.cdLote, la.cdArchivo, la.cdEstadoArchivoLote,
                        la.nuOrden, la.feAlta, la.cdUsuarioAlta, 
                        la.feUltimaModificacion, la.cdUsuarioModificacion,
                        la.dsRutaImagenJpg, la.txImagenBase64, la.txResultadoOcr, la.snTieneOcr,
                        la.nuDpiProcesamiento, la.dsEsquinaRecorte, 
                        la.nuPorcentajeRecorteHorizontal, la.nuPorcentajeRecorteVertical,
                        a.dsNombreArchivo, a.dsRutaCompleta, a.nuCantidadPaginas,
                        e.dsEstado AS DsEstadoArchivoLote
                    FROM IAP_TD_LOTE_ARCHIVOS la
                    INNER JOIN IAP_TD_ARCHIVOS a ON la.cdArchivo = a.cdArchivo
                    INNER JOIN IAP_TV_ESTADOS_ARCHIVO_LOTE e ON la.cdEstadoArchivoLote = e.cdEstadoArchivoLote
                    WHERE la.cdLote = @cdLote
                    ORDER BY la.nuOrden, la.cdLoteArchivo";

                DataTable dt = _db.EjecutarConsulta(query,
                    DatabaseHelper.CrearParametro("@cdLote", cdLote));

                var archivos = new List<LoteArchivo>();
                foreach (DataRow row in dt.Rows)
                {
                    var archivo = MapearLoteArchivo(row);
                    // Construir ruta completa del archivo
                    archivo.DsRutaCompletaArchivo = row["dsRutaCompleta"].ToString() + "\\" + row["dsNombreArchivo"].ToString();
                    archivos.Add(archivo);
                }

                return archivos;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al obtener archivos para procesar del lote {cdLote}", ex, "LoteRepository");
                throw new Exception($"Error al obtener archivos del lote: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Guarda el resultado del procesamiento por IA en la base de datos
        /// </summary>
        public int GuardarResultadoIA(ResultadoIA resultado)
        {
            try
            {
                string query = @"
                    INSERT INTO IAP_TD_RESULTADOS_IA (
                        cdLoteArchivo, txNombreArchivo, dsTipoPlano, dsExpediente, 
                        dsSeccion, dsManzana, dsParcela, dsDireccion,
                        nuConfianzaTipoPlano, nuConfianzaExpediente, nuConfianzaSeccion, 
                        nuConfianzaManzana, nuConfianzaParcela, nuConfianzaDireccion,
                        nuPromptTokens, nuCompletionTokens, nuTotalTokens,
                        dsModalidadProcesamiento, nuIntentos, txRespuestaCompleta, txMensajeError,
                        feAlta, cdUsuarioAlta
                    ) VALUES (
                        @cdLoteArchivo, @txNombreArchivo, @dsTipoPlano, @dsExpediente,
                        @dsSeccion, @dsManzana, @dsParcela, @dsDireccion,
                        @nuConfianzaTipoPlano, @nuConfianzaExpediente, @nuConfianzaSeccion,
                        @nuConfianzaManzana, @nuConfianzaParcela, @nuConfianzaDireccion,
                        @nuPromptTokens, @nuCompletionTokens, @nuTotalTokens,
                        @dsModalidadProcesamiento, @nuIntentos, @txRespuestaCompleta, @txMensajeError,
                        GETDATE(), @cdUsuarioAlta
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var parametros = new SqlParameter[]
                {
                    DatabaseHelper.CrearParametro("@cdLoteArchivo", resultado.CdLoteArchivo),
                    DatabaseHelper.CrearParametro("@txNombreArchivo", resultado.TxNombreArchivo),
                    DatabaseHelper.CrearParametro("@dsTipoPlano", resultado.DsTipoPlano),
                    DatabaseHelper.CrearParametro("@dsExpediente", resultado.DsExpediente),
                    DatabaseHelper.CrearParametro("@dsSeccion", resultado.DsSeccion),
                    DatabaseHelper.CrearParametro("@dsManzana", resultado.DsManzana),
                    DatabaseHelper.CrearParametro("@dsParcela", resultado.DsParcela),
                    DatabaseHelper.CrearParametro("@dsDireccion", resultado.DsDireccion),
                    DatabaseHelper.CrearParametro("@nuConfianzaTipoPlano", resultado.NuConfianzaTipoPlano),
                    DatabaseHelper.CrearParametro("@nuConfianzaExpediente", resultado.NuConfianzaExpediente),
                    DatabaseHelper.CrearParametro("@nuConfianzaSeccion", resultado.NuConfianzaSeccion),
                    DatabaseHelper.CrearParametro("@nuConfianzaManzana", resultado.NuConfianzaManzana),
                    DatabaseHelper.CrearParametro("@nuConfianzaParcela", resultado.NuConfianzaParcela),
                    DatabaseHelper.CrearParametro("@nuConfianzaDireccion", resultado.NuConfianzaDireccion),
                    DatabaseHelper.CrearParametro("@nuPromptTokens", resultado.NuPromptTokens),
                    DatabaseHelper.CrearParametro("@nuCompletionTokens", resultado.NuCompletionTokens),
                    DatabaseHelper.CrearParametro("@nuTotalTokens", resultado.NuTotalTokens),
                    DatabaseHelper.CrearParametro("@dsModalidadProcesamiento", resultado.DsModalidadProcesamiento),
                    DatabaseHelper.CrearParametro("@nuIntentos", resultado.NuIntentos),
                    DatabaseHelper.CrearParametro("@txRespuestaCompleta", resultado.TxRespuestaCompleta),
                    DatabaseHelper.CrearParametro("@txMensajeError", resultado.TxMensajeError),
                    DatabaseHelper.CrearParametro("@cdUsuarioAlta", resultado.CdUsuarioAlta)
                };

                object? resultadoId = _db.EjecutarEscalar(query, parametros);
                int cdResultadoIA = resultadoId != null ? Convert.ToInt32(resultadoId) : 0;

                Logger.Info($"Resultado IA guardado exitosamente. ID: {cdResultadoIA}, Archivo: {resultado.TxNombreArchivo}", "LoteRepository");
                return cdResultadoIA;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al guardar resultado IA para archivo {resultado.TxNombreArchivo}", ex, "LoteRepository");
                throw new Exception($"Error al guardar resultado IA: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza el estado de un archivo en el lote durante el procesamiento por IA
        /// </summary>
        public void ActualizarEstadoProcesamiento(int cdLoteArchivo, int cdEstadoArchivoLote, int? cdResultadoIA = null)
        {
            try
            {
                string query = @"
                    UPDATE IAP_TD_LOTE_ARCHIVOS 
                    SET cdEstadoArchivoLote = @cdEstadoArchivoLote,
                        feUltimaModificacion = GETDATE()
                    WHERE cdLoteArchivo = @cdLoteArchivo";

                var parametros = new SqlParameter[]
                {
                    DatabaseHelper.CrearParametro("@cdLoteArchivo", cdLoteArchivo),
                    DatabaseHelper.CrearParametro("@cdEstadoArchivoLote", cdEstadoArchivoLote)
                };

                _db.EjecutarComando(query, parametros);
                Logger.Info($"Estado de archivo actualizado. cdLoteArchivo: {cdLoteArchivo}, Estado: {cdEstadoArchivoLote}", "LoteRepository");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al actualizar estado de procesamiento para archivo {cdLoteArchivo}", ex, "LoteRepository");
                throw new Exception($"Error al actualizar estado: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Marca un lote como procesado (cambia al estado 3: Pendiente de Control de Calidad)
        /// Solo si todos sus archivos fueron procesados exitosamente
        /// </summary>
        public bool MarcarLoteComoProcesado(int cdLote, int cdUsuarioModificacion)
        {
            try
            {
                // Verificar que todos los archivos del lote hayan sido procesados
                string queryVerificar = @"
                    SELECT COUNT(*) 
                    FROM IAP_TD_LOTE_ARCHIVOS
                    WHERE cdLote = @cdLote 
                      AND cdEstadoArchivoLote NOT IN (
                          SELECT cdEstadoArchivoLote 
                          FROM IAP_TV_ESTADOS_ARCHIVO_LOTE 
                          WHERE dsEstado IN ('Procesado', 'Pendiente de Controlar')
                      )";

                object? pendientes = _db.EjecutarEscalar(queryVerificar,
                    DatabaseHelper.CrearParametro("@cdLote", cdLote));

                int numPendientes = pendientes != null ? Convert.ToInt32(pendientes) : 0;

                if (numPendientes > 0)
                {
                    Logger.Info($"Lote {cdLote} aún tiene {numPendientes} archivos sin procesar. No se cambiará el estado.", "LoteRepository");
                    return false;
                }

                // Actualizar estado del lote a "Pendiente de Control de Calidad" (3)
                string queryActualizar = @"
                    UPDATE IAP_TD_LOTES 
                    SET cdEstadoLote = 3,
                        feUltimaModificacion = GETDATE(),
                        cdUsuarioModificacion = @cdUsuarioModificacion
                    WHERE cdLote = @cdLote";

                var parametros = new SqlParameter[]
                {
                    DatabaseHelper.CrearParametro("@cdLote", cdLote),
                    DatabaseHelper.CrearParametro("@cdUsuarioModificacion", cdUsuarioModificacion)
                };

                _db.EjecutarComando(queryActualizar, parametros);
                Logger.Info($"Lote {cdLote} marcado como procesado (Estado 3: Pendiente de Control de Calidad)", "LoteRepository");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al marcar lote {cdLote} como procesado", ex, "LoteRepository");
                throw new Exception($"Error al actualizar estado del lote: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene el ID del estado de archivo en lote por nombre
        /// </summary>
        public int ObtenerIdEstadoArchivoLotePorNombre(string nombreEstado)
        {
            try
            {
                string query = @"
                    SELECT cdEstadoArchivoLote 
                    FROM IAP_TV_ESTADOS_ARCHIVO_LOTE 
                    WHERE dsEstado = @nombreEstado";

                object? resultado = _db.EjecutarEscalar(query,
                    DatabaseHelper.CrearParametro("@nombreEstado", nombreEstado));

                if (resultado == null || resultado == DBNull.Value)
                {
                    throw new Exception($"No se encontró el estado de archivo '{nombreEstado}'");
                }

                return Convert.ToInt32(resultado);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al obtener ID del estado '{nombreEstado}'", ex, "LoteRepository");
                throw;
            }
        }

        #endregion
    }
}
