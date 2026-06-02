using System.Data;
using System.Data.SqlClient;
using IndexadorAutomaticoPlanos.Security;

namespace IndexadorAutomaticoPlanos.DataAccess
{
    /// <summary>
    /// Clase helper para manejo de conexiones y operaciones de base de datos
    /// </summary>
    public class DatabaseHelper
    {
        private readonly string _cadenaConexion;

        public DatabaseHelper()
        {
            _cadenaConexion = ConfigManager.ObtenerCadenaConexion();
        }

        public DatabaseHelper(string cadenaConexion)
        {
            _cadenaConexion = cadenaConexion;
        }

        /// <summary>
        /// Crea y retorna una nueva conexión a la base de datos
        /// </summary>
        public SqlConnection ObtenerConexion()
        {
            return new SqlConnection(_cadenaConexion);
        }

        /// <summary>
        /// Verifica si la conexión a la base de datos es válida
        /// </summary>
        public bool VerificarConexion(out string mensajeError)
        {
            mensajeError = string.Empty;

            try
            {
                using (SqlConnection conn = ObtenerConexion())
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                mensajeError = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Ejecuta una consulta SELECT y retorna un DataTable
        /// </summary>
        public DataTable EjecutarConsulta(string query, params SqlParameter[] parametros)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = ObtenerConexion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parametros != null && parametros.Length > 0)
                        {
                            cmd.Parameters.AddRange(parametros);
                        }

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al ejecutar consulta: {ex.Message}", ex);
            }

            return dt;
        }

        /// <summary>
        /// Ejecuta un comando INSERT, UPDATE o DELETE
        /// </summary>
        public int EjecutarComando(string query, params SqlParameter[] parametros)
        {
            try
            {
                using (SqlConnection conn = ObtenerConexion())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parametros != null && parametros.Length > 0)
                        {
                            cmd.Parameters.AddRange(parametros);
                        }

                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al ejecutar comando: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ejecuta un comando y retorna el ID insertado (SCOPE_IDENTITY)
        /// </summary>
        public int EjecutarComandoConRetornoId(string query, params SqlParameter[] parametros)
        {
            try
            {
                using (SqlConnection conn = ObtenerConexion())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query + "; SELECT CAST(SCOPE_IDENTITY() AS INT);", conn))
                    {
                        if (parametros != null && parametros.Length > 0)
                        {
                            cmd.Parameters.AddRange(parametros);
                        }

                        object? resultado = cmd.ExecuteScalar();
                        return resultado != null ? Convert.ToInt32(resultado) : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al ejecutar comando con retorno de ID: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ejecuta un comando y retorna un valor escalar
        /// </summary>
        public object? EjecutarEscalar(string query, params SqlParameter[] parametros)
        {
            try
            {
                using (SqlConnection conn = ObtenerConexion())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parametros != null && parametros.Length > 0)
                        {
                            cmd.Parameters.AddRange(parametros);
                        }

                        return cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al ejecutar escalar: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ejecuta múltiples comandos en una transacción
        /// </summary>
        public bool EjecutarTransaccion(Action<SqlConnection, SqlTransaction> accion, out string mensajeError)
        {
            mensajeError = string.Empty;
            SqlConnection? conn = null;
            SqlTransaction? transaction = null;

            try
            {
                conn = ObtenerConexion();
                conn.Open();
                transaction = conn.BeginTransaction();

                accion(conn, transaction);

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                mensajeError = ex.Message;

                try
                {
                    transaction?.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    mensajeError += $" | Error en rollback: {rollbackEx.Message}";
                }

                return false;
            }
            finally
            {
                transaction?.Dispose();
                conn?.Close();
                conn?.Dispose();
            }
        }

        /// <summary>
        /// Crea un SqlParameter de manera simplificada
        /// </summary>
        public static SqlParameter CrearParametro(string nombre, object? valor)
        {
            return new SqlParameter(nombre, valor ?? DBNull.Value);
        }
    }
}
