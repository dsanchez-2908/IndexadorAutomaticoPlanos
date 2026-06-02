using System.Data;
using System.Data.SqlClient;
using IndexadorAutomaticoPlanos.Entities;

namespace IndexadorAutomaticoPlanos.DataAccess
{
    /// <summary>
    /// Repositorio para operaciones CRUD de parámetros de configuración
    /// </summary>
    public class ParametroRepository
    {
        private readonly DatabaseHelper _db;

        public ParametroRepository()
        {
            _db = new DatabaseHelper();
        }

        /// <summary>
        /// Obtiene un parámetro por su clave
        /// </summary>
        public Parametro? ObtenerPorClave(string dsClaveParametro)
        {
            try
            {
                string query = @"
                    SELECT cdParametro, dsClaveParametro, dsValorParametro, dsDescripcion, 
                           feUltimaModificacion, cdUsuarioModificacion
                    FROM IAP_TD_PARAMETROS
                    WHERE dsClaveParametro = @dsClaveParametro";

                DataTable dt = _db.EjecutarConsulta(query, 
                    DatabaseHelper.CrearParametro("@dsClaveParametro", dsClaveParametro));

                if (dt.Rows.Count == 0)
                    return null;

                return MapearParametro(dt.Rows[0]);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener parámetro: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene el valor de un parámetro por su clave
        /// </summary>
        public string? ObtenerValor(string dsClaveParametro)
        {
            try
            {
                string query = @"
                    SELECT dsValorParametro 
                    FROM IAP_TD_PARAMETROS
                    WHERE dsClaveParametro = @dsClaveParametro";

                object? resultado = _db.EjecutarEscalar(query, 
                    DatabaseHelper.CrearParametro("@dsClaveParametro", dsClaveParametro));

                return resultado?.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener valor de parámetro: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene todos los parámetros
        /// </summary>
        public List<Parametro> ObtenerTodos()
        {
            try
            {
                string query = @"
                    SELECT cdParametro, dsClaveParametro, dsValorParametro, dsDescripcion, 
                           feUltimaModificacion, cdUsuarioModificacion
                    FROM IAP_TD_PARAMETROS
                    ORDER BY dsClaveParametro";

                DataTable dt = _db.EjecutarConsulta(query);
                List<Parametro> parametros = new List<Parametro>();

                foreach (DataRow row in dt.Rows)
                {
                    parametros.Add(MapearParametro(row));
                }

                return parametros;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener parámetros: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza el valor de un parámetro
        /// </summary>
        public bool ActualizarValor(string dsClaveParametro, string? dsValorParametro, int cdUsuarioModificacion)
        {
            try
            {
                string query = @"
                    UPDATE IAP_TD_PARAMETROS 
                    SET dsValorParametro = @dsValorParametro,
                        feUltimaModificacion = @feUltimaModificacion,
                        cdUsuarioModificacion = @cdUsuarioModificacion
                    WHERE dsClaveParametro = @dsClaveParametro";

                int filasAfectadas = _db.EjecutarComando(query,
                    DatabaseHelper.CrearParametro("@dsClaveParametro", dsClaveParametro),
                    DatabaseHelper.CrearParametro("@dsValorParametro", dsValorParametro),
                    DatabaseHelper.CrearParametro("@feUltimaModificacion", DateTime.Now),
                    DatabaseHelper.CrearParametro("@cdUsuarioModificacion", cdUsuarioModificacion));

                return filasAfectadas > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar parámetro: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Inserta un nuevo parámetro
        /// </summary>
        public int Insertar(Parametro parametro)
        {
            try
            {
                string query = @"
                    INSERT INTO IAP_TD_PARAMETROS 
                    (dsClaveParametro, dsValorParametro, dsDescripcion, feUltimaModificacion, cdUsuarioModificacion)
                    VALUES 
                    (@dsClaveParametro, @dsValorParametro, @dsDescripcion, @feUltimaModificacion, @cdUsuarioModificacion)";

                return _db.EjecutarComandoConRetornoId(query,
                    DatabaseHelper.CrearParametro("@dsClaveParametro", parametro.DsClaveParametro),
                    DatabaseHelper.CrearParametro("@dsValorParametro", parametro.DsValorParametro),
                    DatabaseHelper.CrearParametro("@dsDescripcion", parametro.DsDescripcion),
                    DatabaseHelper.CrearParametro("@feUltimaModificacion", DateTime.Now),
                    DatabaseHelper.CrearParametro("@cdUsuarioModificacion", parametro.CdUsuarioModificacion));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al insertar parámetro: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza un parámetro completo
        /// </summary>
        public bool Actualizar(Parametro parametro)
        {
            try
            {
                string query = @"
                    UPDATE IAP_TD_PARAMETROS 
                    SET dsValorParametro = @dsValorParametro,
                        dsDescripcion = @dsDescripcion,
                        feUltimaModificacion = @feUltimaModificacion,
                        cdUsuarioModificacion = @cdUsuarioModificacion
                    WHERE cdParametro = @cdParametro";

                int filasAfectadas = _db.EjecutarComando(query,
                    DatabaseHelper.CrearParametro("@cdParametro", parametro.CdParametro),
                    DatabaseHelper.CrearParametro("@dsValorParametro", parametro.DsValorParametro),
                    DatabaseHelper.CrearParametro("@dsDescripcion", parametro.DsDescripcion),
                    DatabaseHelper.CrearParametro("@feUltimaModificacion", DateTime.Now),
                    DatabaseHelper.CrearParametro("@cdUsuarioModificacion", parametro.CdUsuarioModificacion));

                return filasAfectadas > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar parámetro: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verifica si existe un parámetro con la clave especificada
        /// </summary>
        public bool ExisteParametro(string dsClaveParametro)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM IAP_TD_PARAMETROS WHERE dsClaveParametro = @dsClaveParametro";

                object? resultado = _db.EjecutarEscalar(query, 
                    DatabaseHelper.CrearParametro("@dsClaveParametro", dsClaveParametro));

                return Convert.ToInt32(resultado) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al verificar existencia de parámetro: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Mapea un DataRow a un objeto Parametro
        /// </summary>
        private Parametro MapearParametro(DataRow row)
        {
            return new Parametro
            {
                CdParametro = Convert.ToInt32(row["cdParametro"]),
                DsClaveParametro = row["dsClaveParametro"].ToString() ?? string.Empty,
                DsValorParametro = row["dsValorParametro"] != DBNull.Value ? row["dsValorParametro"].ToString() : null,
                DsDescripcion = row["dsDescripcion"] != DBNull.Value ? row["dsDescripcion"].ToString() : null,
                FeUltimaModificacion = Convert.ToDateTime(row["feUltimaModificacion"]),
                CdUsuarioModificacion = row["cdUsuarioModificacion"] != DBNull.Value ? Convert.ToInt32(row["cdUsuarioModificacion"]) : null
            };
        }
    }
}
