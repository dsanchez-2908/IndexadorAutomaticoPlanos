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
                DsCarpetaOrigen = row.Table.Columns.Contains("dsCarpetaOrigen") ? row["dsCarpetaOrigen"]?.ToString() : null,
                CdEstadoLote = Convert.ToInt32(row["cdEstadoLote"]),
                NuCantidadArchivos = row.Table.Columns.Contains("nuCantidadArchivos") ? Convert.ToInt32(row["nuCantidadArchivos"]) : 0,
                CantidadArchivos = row.Table.Columns.Contains("CantidadArchivos") ? Convert.ToInt32(row["CantidadArchivos"]) : 0,
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
                        a.dsNombreArchivo,
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
                    // Mapeo específico para procesamiento IA (sin campos innecesarios)
                    var archivo = new LoteArchivo
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

                        // Campos de procesamiento de imagen (Fase 4)
                        DsRutaImagenJpg = row["dsRutaImagenJpg"]?.ToString(),
                        TxImagenBase64 = row["txImagenBase64"]?.ToString(),
                        TxResultadoOcr = row["txResultadoOcr"]?.ToString(),
                        SnTieneOcr = row["snTieneOcr"] != DBNull.Value && Convert.ToBoolean(row["snTieneOcr"]),
                        NuDpiProcesamiento = row["nuDpiProcesamiento"] != DBNull.Value ? Convert.ToInt32(row["nuDpiProcesamiento"]) : null,
                        DsEsquinaRecorte = row["dsEsquinaRecorte"]?.ToString(),
                        NuPorcentajeRecorteHorizontal = row["nuPorcentajeRecorteHorizontal"] != DBNull.Value ? Convert.ToDecimal(row["nuPorcentajeRecorteHorizontal"]) : null,
                        NuPorcentajeRecorteVertical = row["nuPorcentajeRecorteVertical"] != DBNull.Value ? Convert.ToDecimal(row["nuPorcentajeRecorteVertical"]) : null,

                        // Campos de navegación necesarios para IA
                        DsEstadoArchivoLote = row["DsEstadoArchivoLote"]?.ToString(),
                        DsNombreArchivo = row["dsNombreArchivo"]?.ToString()
                    };

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
                // Primero intentar búsqueda exacta
                string query = @"
                    SELECT cdEstadoArchivoLote 
                    FROM IAP_TV_ESTADOS_ARCHIVO_LOTE 
                    WHERE dsEstado = @nombreEstado";

                object? resultado = _db.EjecutarEscalar(query,
                    DatabaseHelper.CrearParametro("@nombreEstado", nombreEstado));

                if (resultado != null && resultado != DBNull.Value)
                {
                    return Convert.ToInt32(resultado);
                }

                // Si no encuentra, intentar búsqueda flexible (por si hay problemas de codificación)
                // Usar LIKE y remover acentos para comparar
                string queryFlexible = @"
                    SELECT cdEstadoArchivoLote 
                    FROM IAP_TV_ESTADOS_ARCHIVO_LOTE 
                    WHERE REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                            dsEstado COLLATE Modern_Spanish_CI_AI,
                            'á', 'a'), 'é', 'e'), 'í', 'i'), 'ó', 'o'), 'ú', 'u')
                        LIKE 
                          REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                            @nombreEstado COLLATE Modern_Spanish_CI_AI,
                            'á', 'a'), 'é', 'e'), 'í', 'i'), 'ó', 'o'), 'ú', 'u')";

                resultado = _db.EjecutarEscalar(queryFlexible,
                    DatabaseHelper.CrearParametro("@nombreEstado", nombreEstado));

                if (resultado == null || resultado == DBNull.Value)
                {
                    throw new Exception($"No se encontró el estado de archivo '{nombreEstado}'");
                }

                Logger.Warning($"Estado '{nombreEstado}' encontrado con búsqueda flexible (posible problema de codificación)", "LoteRepository");
                return Convert.ToInt32(resultado);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al obtener ID del estado '{nombreEstado}'", ex, "LoteRepository");
                throw;
            }
        }

        #endregion

        #region Métodos para Control de Calidad (Fase 6)

        /// <summary>
        /// Obtiene los lotes pendientes de control de calidad (Estado 3)
        /// </summary>
        public List<Lote> ObtenerLotesPendientesControlCalidad()
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
                    WHERE l.cdEstadoLote = 3
                    ORDER BY l.feAlta";

                DataTable dt = _db.EjecutarConsulta(query);
                return MapearListaLotes(dt);
            }
            catch (Exception ex)
            {
                Logger.Error("Error al obtener lotes pendientes de control de calidad", ex, "LoteRepository");
                throw new Exception($"Error al obtener lotes pendientes de control: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene los archivos de un lote con sus resultados de IA para control de calidad
        /// </summary>
        public List<ArchivoConResultadoIA> ObtenerArchivosConResultadosIA(int cdLote)
        {
            try
            {
                string query = @"
                    SELECT 
                        la.cdLoteArchivo, la.cdLote, la.cdArchivo, la.cdEstadoArchivoLote,
                        la.nuOrden, la.dsRutaImagenJpg,
                        a.dsNombreArchivo,
                        ea.dsEstado AS DsEstadoArchivoLote,
                        r.cdResultadoIA,
                        r.dsTipoPlano, r.dsExpediente, r.dsSeccion, r.dsManzana, r.dsParcela, r.dsDireccion,
                        r.nuConfianzaTipoPlano, r.nuConfianzaExpediente, r.nuConfianzaSeccion,
                        r.nuConfianzaManzana, r.nuConfianzaParcela, r.nuConfianzaDireccion,
                        r.dsModalidadProcesamiento, r.nuIntentos
                    FROM IAP_TD_LOTE_ARCHIVOS la
                    INNER JOIN IAP_TD_ARCHIVOS a ON la.cdArchivo = a.cdArchivo
                    INNER JOIN IAP_TV_ESTADOS_ARCHIVO_LOTE ea ON la.cdEstadoArchivoLote = ea.cdEstadoArchivoLote
                    LEFT JOIN IAP_TD_RESULTADOS_IA r ON la.cdLoteArchivo = r.cdLoteArchivo
                    WHERE la.cdLote = @cdLote
                    ORDER BY la.nuOrden, la.cdLoteArchivo";

                DataTable dt = _db.EjecutarConsulta(query,
                    DatabaseHelper.CrearParametro("@cdLote", cdLote));

                var archivos = new List<ArchivoConResultadoIA>();
                foreach (DataRow row in dt.Rows)
                {
                    var archivo = new ArchivoConResultadoIA
                    {
                        // Datos del archivo
                        CdLoteArchivo = Convert.ToInt32(row["cdLoteArchivo"]),
                        CdLote = Convert.ToInt32(row["cdLote"]),
                        CdArchivo = Convert.ToInt32(row["cdArchivo"]),
                        CdEstadoArchivoLote = Convert.ToInt32(row["cdEstadoArchivoLote"]),
                        NuOrden = row["nuOrden"] != DBNull.Value ? Convert.ToInt32(row["nuOrden"]) : null,
                        DsRutaImagenJpg = row["dsRutaImagenJpg"]?.ToString(),
                        DsNombreArchivo = row["dsNombreArchivo"]?.ToString(),
                        DsEstadoArchivoLote = row["DsEstadoArchivoLote"]?.ToString(),

                        // Resultado de IA (puede ser null si no se procesó)
                        CdResultadoIA = row["cdResultadoIA"] != DBNull.Value ? Convert.ToInt32(row["cdResultadoIA"]) : null,
                        DsTipoPlano = row["dsTipoPlano"]?.ToString(),
                        DsExpediente = row["dsExpediente"]?.ToString(),
                        DsSeccion = row["dsSeccion"]?.ToString(),
                        DsManzana = row["dsManzana"]?.ToString(),
                        DsParcela = row["dsParcela"]?.ToString(),
                        DsDireccion = row["dsDireccion"]?.ToString(),
                        NuConfianzaTipoPlano = row["nuConfianzaTipoPlano"] != DBNull.Value ? Convert.ToDecimal(row["nuConfianzaTipoPlano"]) : null,
                        NuConfianzaExpediente = row["nuConfianzaExpediente"] != DBNull.Value ? Convert.ToDecimal(row["nuConfianzaExpediente"]) : null,
                        NuConfianzaSeccion = row["nuConfianzaSeccion"] != DBNull.Value ? Convert.ToDecimal(row["nuConfianzaSeccion"]) : null,
                        NuConfianzaManzana = row["nuConfianzaManzana"] != DBNull.Value ? Convert.ToDecimal(row["nuConfianzaManzana"]) : null,
                        NuConfianzaParcela = row["nuConfianzaParcela"] != DBNull.Value ? Convert.ToDecimal(row["nuConfianzaParcela"]) : null,
                        NuConfianzaDireccion = row["nuConfianzaDireccion"] != DBNull.Value ? Convert.ToDecimal(row["nuConfianzaDireccion"]) : null,
                        DsModalidadProcesamiento = row["dsModalidadProcesamiento"]?.ToString(),
                        NuIntentos = row["nuIntentos"] != DBNull.Value ? Convert.ToInt32(row["nuIntentos"]) : null
                    };

                    archivos.Add(archivo);
                }

                return archivos;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al obtener archivos con resultados IA del lote {cdLote}", ex, "LoteRepository");
                throw new Exception($"Error al obtener archivos del lote: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza los campos corregidos manualmente de un resultado de IA
        /// </summary>
        public void ActualizarResultadoIA(ResultadoIA resultado)
        {
            try
            {
                string query = @"
                    UPDATE IAP_TD_RESULTADOS_IA
                    SET dsTipoPlano = @dsTipoPlano,
                        dsExpediente = @dsExpediente,
                        dsSeccion = @dsSeccion,
                        dsManzana = @dsManzana,
                        dsParcela = @dsParcela,
                        dsDireccion = @dsDireccion,
                        feUltimaModificacion = GETDATE(),
                        cdUsuarioModificacion = @cdUsuarioModificacion
                    WHERE cdResultadoIA = @cdResultadoIA";

                _db.EjecutarComando(query,
                    DatabaseHelper.CrearParametro("@cdResultadoIA", resultado.CdResultadoIA),
                    DatabaseHelper.CrearParametro("@dsTipoPlano", (object?)resultado.DsTipoPlano ?? DBNull.Value),
                    DatabaseHelper.CrearParametro("@dsExpediente", (object?)resultado.DsExpediente ?? DBNull.Value),
                    DatabaseHelper.CrearParametro("@dsSeccion", (object?)resultado.DsSeccion ?? DBNull.Value),
                    DatabaseHelper.CrearParametro("@dsManzana", (object?)resultado.DsManzana ?? DBNull.Value),
                    DatabaseHelper.CrearParametro("@dsParcela", (object?)resultado.DsParcela ?? DBNull.Value),
                    DatabaseHelper.CrearParametro("@dsDireccion", (object?)resultado.DsDireccion ?? DBNull.Value),
                    DatabaseHelper.CrearParametro("@cdUsuarioModificacion", resultado.CdUsuarioModificacion));

                Logger.Info($"Resultado IA actualizado: {resultado.CdResultadoIA}", "LoteRepository");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al actualizar resultado IA {resultado.CdResultadoIA}", ex, "LoteRepository");
                throw new Exception($"Error al actualizar resultado: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Marca un archivo como controlado
        /// </summary>
        public void MarcarArchivoComoControlado(int cdLoteArchivo, int cdUsuario)
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
                    DatabaseHelper.CrearParametro("@cdEstadoArchivoLote", EstadosArchivoLote.Controlado),
                    DatabaseHelper.CrearParametro("@cdUsuarioModificacion", cdUsuario));

                Logger.Info($"Archivo {cdLoteArchivo} marcado como controlado", "LoteRepository");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al marcar archivo {cdLoteArchivo} como controlado", ex, "LoteRepository");
                throw new Exception($"Error al marcar archivo como controlado: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Marca un archivo como carátula ilegible
        /// </summary>
        public void MarcarArchivoComoIlegible(int cdLoteArchivo, int cdUsuario)
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
                    DatabaseHelper.CrearParametro("@cdEstadoArchivoLote", EstadosArchivoLote.CaratulaIlegible),
                    DatabaseHelper.CrearParametro("@cdUsuarioModificacion", cdUsuario));

                Logger.Info($"Archivo {cdLoteArchivo} marcado como ilegible", "LoteRepository");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al marcar archivo {cdLoteArchivo} como ilegible", ex, "LoteRepository");
                throw new Exception($"Error al marcar archivo como ilegible: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Finaliza el control de un lote, cambiándolo a estado 4 (Pendiente de Finalizar)
        /// NOTA: Permite finalizar aunque haya archivos pendientes (el control puede ser parcial)
        /// </summary>
        public bool FinalizarControlLote(int cdLote, int cdUsuarioModificacion, out string mensaje)
        {
            try
            {
                // Actualizar estado del lote a "Pendiente de Finalizar" (4)
                string queryActualizar = @"
                    UPDATE IAP_TD_LOTES 
                    SET cdEstadoLote = @estadoPendienteFinalizar,
                        feUltimaModificacion = GETDATE(),
                        cdUsuarioModificacion = @cdUsuarioModificacion
                    WHERE cdLote = @cdLote";

                _db.EjecutarComando(queryActualizar,
                    DatabaseHelper.CrearParametro("@cdLote", cdLote),
                    DatabaseHelper.CrearParametro("@estadoPendienteFinalizar", EstadosLote.PendienteFinalizar),
                    DatabaseHelper.CrearParametro("@cdUsuarioModificacion", cdUsuarioModificacion));

                mensaje = $"Lote {cdLote} finalizado exitosamente";
                Logger.Info(mensaje, "LoteRepository");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al finalizar control del lote {cdLote}", ex, "LoteRepository");
                mensaje = $"Error al finalizar lote: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Obtiene estadísticas de control de un lote
        /// </summary>
        public (int Total, int Controlados, int Ilegibles, int Pendientes) ObtenerEstadisticasLote(int cdLote)
        {
            try
            {
                string query = @"
                    SELECT 
                        COUNT(*) AS Total,
                        SUM(CASE WHEN la.cdEstadoArchivoLote = @estadoControlado THEN 1 ELSE 0 END) AS Controlados,
                        SUM(CASE WHEN la.cdEstadoArchivoLote = @estadoIlegible THEN 1 ELSE 0 END) AS Ilegibles,
                        SUM(CASE WHEN la.cdEstadoArchivoLote NOT IN (@estadoControlado, @estadoIlegible) THEN 1 ELSE 0 END) AS Pendientes
                    FROM IAP_TD_LOTE_ARCHIVOS la
                    WHERE la.cdLote = @cdLote";

                DataTable dt = _db.EjecutarConsulta(query,
                    DatabaseHelper.CrearParametro("@cdLote", cdLote),
                    DatabaseHelper.CrearParametro("@estadoControlado", EstadosArchivoLote.Controlado),
                    DatabaseHelper.CrearParametro("@estadoIlegible", EstadosArchivoLote.CaratulaIlegible));

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    return (
                        Convert.ToInt32(row["Total"]),
                        Convert.ToInt32(row["Controlados"]),
                        Convert.ToInt32(row["Ilegibles"]),
                        Convert.ToInt32(row["Pendientes"])
                    );
                }

                return (0, 0, 0, 0);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al obtener estadísticas del lote {cdLote}", ex, "LoteRepository");
                throw new Exception($"Error al obtener estadísticas: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene todos los tipos de plano activos
        /// </summary>
        public List<TipoPlano> ObtenerTiposPlano()
        {
            try
            {
                string query = @"
                    SELECT cdTipoPlano, dsTipoPlano, dsDescripcion, snActivo, feAlta
                    FROM IAP_TV_TIPOS_PLANO
                    WHERE snActivo = 1
                    ORDER BY dsTipoPlano";

                DataTable dt = _db.EjecutarConsulta(query);

                var tipos = new List<TipoPlano>();
                foreach (DataRow row in dt.Rows)
                {
                    tipos.Add(new TipoPlano
                    {
                        CdTipoPlano = Convert.ToInt32(row["cdTipoPlano"]),
                        DsTipoPlano = row["dsTipoPlano"].ToString() ?? string.Empty,
                        DsDescripcion = row["dsDescripcion"]?.ToString(),
                        SnActivo = Convert.ToBoolean(row["snActivo"]),
                        FeAlta = Convert.ToDateTime(row["feAlta"])
                    });
                }

                return tipos;
            }
            catch (Exception ex)
            {
                Logger.Error("Error al obtener tipos de plano", ex, "LoteRepository");
                throw new Exception($"Error al obtener tipos de plano: {ex.Message}", ex);
            }
        }

        #endregion

        #region Finalización de Lotes

        /// <summary>
        /// Obtiene lotes en estado "Pendiente de Finalizar" (cdEstadoLote = 5)
        /// </summary>
        public List<Lote> ObtenerLotesPendientesFinalizacion()
        {
            try
            {
                string query = @"
                    SELECT 
                        l.cdLote, l.dsNombreLote, l.dsCarpetaOrigen, l.cdEstadoLote,
                        l.feAlta, l.cdUsuarioAlta, l.feUltimaModificacion, l.cdUsuarioModificacion,
                        el.dsEstado AS DsEstadoLote,
                        COUNT(la.cdLoteArchivo) AS CantidadArchivos
                    FROM IAP_TD_LOTES l
                    INNER JOIN IAP_TV_ESTADOS_LOTE el ON l.cdEstadoLote = el.cdEstadoLote
                    LEFT JOIN IAP_TD_LOTE_ARCHIVOS la ON l.cdLote = la.cdLote
                    WHERE l.cdEstadoLote = 4
                    GROUP BY l.cdLote, l.dsNombreLote, l.dsCarpetaOrigen, l.cdEstadoLote,
                             l.feAlta, l.cdUsuarioAlta, l.feUltimaModificacion, l.cdUsuarioModificacion,
                             el.dsEstado
                    ORDER BY l.feAlta DESC";

                DataTable dt = _db.EjecutarConsulta(query);
                return MapearListaLotes(dt);
            }
            catch (Exception ex)
            {
                Logger.Error("Error al obtener lotes pendientes de finalización", ex, "LoteRepository");
                throw new Exception($"Error al obtener lotes: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene archivos de un lote con todos sus metadatos para finalización
        /// Incluye información de ResultadoIA y estado del archivo
        /// </summary>
        public List<ArchivoParaFinalizar> ObtenerArchivosParaFinalizacion(int cdLote)
        {
            try
            {
                string query = @"
                    SELECT 
                        la.cdLoteArchivo,
                        la.cdArchivo,
                        la.cdEstadoArchivoLote,
                        eal.dsEstado AS DsEstadoArchivoLote,
                        a.dsNombreArchivo,
                        a.dsRutaCompleta,
                        a.dsNombreUltimaCarpeta,
                        ri.cdResultadoIA,
                        ri.dsTipoPlano,
                        ri.dsExpediente,
                        ri.dsSeccion,
                        ri.dsManzana,
                        ri.dsParcela,
                        ri.dsDireccion,
                        l.dsNombreLote
                    FROM IAP_TD_LOTE_ARCHIVOS la
                    INNER JOIN IAP_TD_ARCHIVOS a ON la.cdArchivo = a.cdArchivo
                    INNER JOIN IAP_TV_ESTADOS_ARCHIVO_LOTE eal ON la.cdEstadoArchivoLote = eal.cdEstadoArchivoLote
                    LEFT JOIN IAP_TD_RESULTADOS_IA ri ON la.cdLoteArchivo = ri.cdLoteArchivo
                    INNER JOIN IAP_TD_LOTES l ON la.cdLote = l.cdLote
                    WHERE la.cdLote = @cdLote
                    ORDER BY a.dsNombreArchivo";

                var parametros = new SqlParameter[]
                {
                    new SqlParameter("@cdLote", cdLote)
                };

                DataTable dt = _db.EjecutarConsulta(query, parametros);

                var archivos = new List<ArchivoParaFinalizar>();
                foreach (DataRow row in dt.Rows)
                {
                    archivos.Add(new ArchivoParaFinalizar
                    {
                        CdLoteArchivo = Convert.ToInt32(row["cdLoteArchivo"]),
                        CdArchivo = Convert.ToInt32(row["cdArchivo"]),
                        CdEstadoArchivoLote = Convert.ToInt32(row["cdEstadoArchivoLote"]),
                        DsEstadoArchivoLote = row["DsEstadoArchivoLote"].ToString() ?? string.Empty,
                        DsNombreArchivo = row["dsNombreArchivo"].ToString() ?? string.Empty,
                        DsRutaCompleta = row["dsRutaCompleta"].ToString() ?? string.Empty,
                        DsNombreUltimaCarpeta = row["dsNombreUltimaCarpeta"].ToString() ?? string.Empty,
                        CdResultadoIA = row["cdResultadoIA"] != DBNull.Value ? Convert.ToInt32(row["cdResultadoIA"]) : (int?)null,
                        DsTipoPlano = row["dsTipoPlano"] != DBNull.Value ? row["dsTipoPlano"].ToString() : null,
                        DsExpediente = row["dsExpediente"] != DBNull.Value ? row["dsExpediente"].ToString() : null,
                        DsSeccion = row["dsSeccion"] != DBNull.Value ? row["dsSeccion"].ToString() : null,
                        DsManzana = row["dsManzana"] != DBNull.Value ? row["dsManzana"].ToString() : null,
                        DsParcela = row["dsParcela"] != DBNull.Value ? row["dsParcela"].ToString() : null,
                        DsDireccion = row["dsDireccion"] != DBNull.Value ? row["dsDireccion"].ToString() : null,
                        DsNombreLote = row["dsNombreLote"].ToString() ?? string.Empty
                    });
                }

                return archivos;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al obtener archivos para finalización del lote {cdLote}", ex, "LoteRepository");
                throw new Exception($"Error al obtener archivos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cambia el estado de un lote
        /// </summary>
        public void CambiarEstadoLote(int cdLote, int cdEstadoLote, int cdUsuarioModificacion)
        {
            try
            {
                string query = @"
                    UPDATE IAP_TD_LOTES
                    SET cdEstadoLote = @cdEstadoLote,
                        feUltimaModificacion = GETDATE(),
                        cdUsuarioModificacion = @cdUsuarioModificacion
                    WHERE cdLote = @cdLote";

                var parametros = new SqlParameter[]
                {
                    new SqlParameter("@cdLote", cdLote),
                    new SqlParameter("@cdEstadoLote", cdEstadoLote),
                    new SqlParameter("@cdUsuarioModificacion", cdUsuarioModificacion)
                };

                int filasAfectadas = _db.EjecutarComando(query, parametros);

                if (filasAfectadas == 0)
                {
                    throw new Exception($"No se encontró el lote con código {cdLote}");
                }

                Logger.Info($"Estado del lote {cdLote} cambiado a {cdEstadoLote}", "LoteRepository");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al cambiar estado del lote {cdLote}", ex, "LoteRepository");
                throw new Exception($"Error al cambiar estado: {ex.Message}", ex);
            }
        }

        #endregion
    }
}
