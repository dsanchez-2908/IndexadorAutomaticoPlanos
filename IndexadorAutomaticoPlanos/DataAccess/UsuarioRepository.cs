using System.Data;
using System.Data.SqlClient;
using IndexadorAutomaticoPlanos.Entities;
using IndexadorAutomaticoPlanos.Security;

namespace IndexadorAutomaticoPlanos.DataAccess
{
    /// <summary>
    /// Repositorio para operaciones CRUD de usuarios
    /// </summary>
    public class UsuarioRepository
    {
        private readonly DatabaseHelper _db;

        public UsuarioRepository()
        {
            _db = new DatabaseHelper();
        }

        /// <summary>
        /// Autentica un usuario por nombre de usuario y contraseña
        /// </summary>
        public Usuario? Autenticar(string dsUsuario, string dsClave)
        {
            try
            {
                string query = @"
                    SELECT cdUsuario, dsUsuario, dsClave, dsNombreCompleto, 
                           snClaveTemporal, snPrimerIngreso, snActivo, 
                           feAlta, cdUsuarioAlta, feUltimaModificacion, cdUsuarioModificacion
                    FROM IAP_TD_USUARIOS
                    WHERE dsUsuario = @dsUsuario AND snActivo = 1";

                DataTable dt = _db.EjecutarConsulta(query, 
                    DatabaseHelper.CrearParametro("@dsUsuario", dsUsuario));

                if (dt.Rows.Count == 0)
                    return null;

                DataRow row = dt.Rows[0];
                string hashAlmacenado = row["dsClave"].ToString() ?? string.Empty;

                // Verificar la contraseña usando BCrypt
                if (!Encriptacion.VerificarClave(dsClave, hashAlmacenado))
                    return null;

                return MapearUsuario(row);
            }
            catch (Exception ex)
            {
                Utils.Logger.Error($"Error al autenticar usuario: {ex.Message}", ex, "UsuarioRepository");
                throw new Exception($"Error al autenticar usuario: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        public Usuario? ObtenerPorId(int cdUsuario)
        {
            try
            {
                string query = @"
                    SELECT cdUsuario, dsUsuario, dsClave, dsNombreCompleto, 
                           snClaveTemporal, snPrimerIngreso, snActivo, 
                           feAlta, cdUsuarioAlta, feUltimaModificacion, cdUsuarioModificacion
                    FROM IAP_TD_USUARIOS
                    WHERE cdUsuario = @cdUsuario";

                DataTable dt = _db.EjecutarConsulta(query, 
                    DatabaseHelper.CrearParametro("@cdUsuario", cdUsuario));

                if (dt.Rows.Count == 0)
                    return null;

                return MapearUsuario(dt.Rows[0]);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener usuario: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene todos los usuarios activos
        /// </summary>
        public List<Usuario> ObtenerTodos(bool soloActivos = true)
        {
            try
            {
                string query = @"
                    SELECT cdUsuario, dsUsuario, dsClave, dsNombreCompleto, 
                           snClaveTemporal, snPrimerIngreso, snActivo, 
                           feAlta, cdUsuarioAlta, feUltimaModificacion, cdUsuarioModificacion
                    FROM IAP_TD_USUARIOS";

                if (soloActivos)
                    query += " WHERE snActivo = 1";

                query += " ORDER BY dsNombreCompleto";

                DataTable dt = _db.EjecutarConsulta(query);
                List<Usuario> usuarios = new List<Usuario>();

                foreach (DataRow row in dt.Rows)
                {
                    usuarios.Add(MapearUsuario(row));
                }

                return usuarios;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener usuarios: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Inserta un nuevo usuario
        /// </summary>
        public int Insertar(Usuario usuario)
        {
            try
            {
                string query = @"
                    INSERT INTO IAP_TD_USUARIOS 
                    (dsUsuario, dsClave, dsNombreCompleto, snClaveTemporal, snPrimerIngreso, 
                     snActivo, feAlta, cdUsuarioAlta)
                    VALUES 
                    (@dsUsuario, @dsClave, @dsNombreCompleto, @snClaveTemporal, @snPrimerIngreso, 
                     @snActivo, @feAlta, @cdUsuarioAlta)";

                return _db.EjecutarComandoConRetornoId(query,
                    DatabaseHelper.CrearParametro("@dsUsuario", usuario.DsUsuario),
                    DatabaseHelper.CrearParametro("@dsClave", usuario.DsClave),
                    DatabaseHelper.CrearParametro("@dsNombreCompleto", usuario.DsNombreCompleto),
                    DatabaseHelper.CrearParametro("@snClaveTemporal", usuario.SnClaveTemporal),
                    DatabaseHelper.CrearParametro("@snPrimerIngreso", usuario.SnPrimerIngreso),
                    DatabaseHelper.CrearParametro("@snActivo", usuario.SnActivo),
                    DatabaseHelper.CrearParametro("@feAlta", usuario.FeAlta),
                    DatabaseHelper.CrearParametro("@cdUsuarioAlta", usuario.CdUsuarioAlta));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al insertar usuario: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        public bool Actualizar(Usuario usuario)
        {
            try
            {
                string query = @"
                    UPDATE IAP_TD_USUARIOS 
                    SET dsUsuario = @dsUsuario,
                        dsNombreCompleto = @dsNombreCompleto,
                        snActivo = @snActivo,
                        feUltimaModificacion = @feUltimaModificacion,
                        cdUsuarioModificacion = @cdUsuarioModificacion
                    WHERE cdUsuario = @cdUsuario";

                int filasAfectadas = _db.EjecutarComando(query,
                    DatabaseHelper.CrearParametro("@cdUsuario", usuario.CdUsuario),
                    DatabaseHelper.CrearParametro("@dsUsuario", usuario.DsUsuario),
                    DatabaseHelper.CrearParametro("@dsNombreCompleto", usuario.DsNombreCompleto),
                    DatabaseHelper.CrearParametro("@snActivo", usuario.SnActivo),
                    DatabaseHelper.CrearParametro("@feUltimaModificacion", DateTime.Now),
                    DatabaseHelper.CrearParametro("@cdUsuarioModificacion", usuario.CdUsuarioModificacion));

                return filasAfectadas > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar usuario: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cambia la contraseña de un usuario
        /// </summary>
        public bool CambiarClave(int cdUsuario, string nuevaClave, bool esClaveTemporal, int cdUsuarioModificacion)
        {
            try
            {
                string claveEncriptada = Encriptacion.EncriptarClave(nuevaClave);

                string query = @"
                    UPDATE IAP_TD_USUARIOS 
                    SET dsClave = @dsClave,
                        snClaveTemporal = @snClaveTemporal,
                        snPrimerIngreso = 0,
                        feUltimaModificacion = @feUltimaModificacion,
                        cdUsuarioModificacion = @cdUsuarioModificacion
                    WHERE cdUsuario = @cdUsuario";

                int filasAfectadas = _db.EjecutarComando(query,
                    DatabaseHelper.CrearParametro("@cdUsuario", cdUsuario),
                    DatabaseHelper.CrearParametro("@dsClave", claveEncriptada),
                    DatabaseHelper.CrearParametro("@snClaveTemporal", esClaveTemporal),
                    DatabaseHelper.CrearParametro("@feUltimaModificacion", DateTime.Now),
                    DatabaseHelper.CrearParametro("@cdUsuarioModificacion", cdUsuarioModificacion));

                return filasAfectadas > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al cambiar contraseña: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Desactiva un usuario (no lo elimina físicamente)
        /// </summary>
        public bool Desactivar(int cdUsuario, int cdUsuarioModificacion)
        {
            try
            {
                string query = @"
                    UPDATE IAP_TD_USUARIOS 
                    SET snActivo = 0,
                        feUltimaModificacion = @feUltimaModificacion,
                        cdUsuarioModificacion = @cdUsuarioModificacion
                    WHERE cdUsuario = @cdUsuario";

                int filasAfectadas = _db.EjecutarComando(query,
                    DatabaseHelper.CrearParametro("@cdUsuario", cdUsuario),
                    DatabaseHelper.CrearParametro("@feUltimaModificacion", DateTime.Now),
                    DatabaseHelper.CrearParametro("@cdUsuarioModificacion", cdUsuarioModificacion));

                return filasAfectadas > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al desactivar usuario: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verifica si existe un usuario con el nombre especificado
        /// </summary>
        public bool ExisteUsuario(string dsUsuario, int? cdUsuarioExcluir = null)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM IAP_TD_USUARIOS WHERE dsUsuario = @dsUsuario";

                if (cdUsuarioExcluir.HasValue)
                    query += " AND cdUsuario != @cdUsuarioExcluir";

                var parametros = new List<SqlParameter> { DatabaseHelper.CrearParametro("@dsUsuario", dsUsuario) };

                if (cdUsuarioExcluir.HasValue)
                    parametros.Add(DatabaseHelper.CrearParametro("@cdUsuarioExcluir", cdUsuarioExcluir.Value));

                object? resultado = _db.EjecutarEscalar(query, parametros.ToArray());
                return Convert.ToInt32(resultado) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al verificar existencia de usuario: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Mapea un DataRow a un objeto Usuario
        /// </summary>
        private Usuario MapearUsuario(DataRow row)
        {
            return new Usuario
            {
                CdUsuario = Convert.ToInt32(row["cdUsuario"]),
                DsUsuario = row["dsUsuario"].ToString() ?? string.Empty,
                DsClave = row["dsClave"].ToString() ?? string.Empty,
                DsNombreCompleto = row["dsNombreCompleto"].ToString() ?? string.Empty,
                SnClaveTemporal = Convert.ToBoolean(row["snClaveTemporal"]),
                SnPrimerIngreso = Convert.ToBoolean(row["snPrimerIngreso"]),
                SnActivo = Convert.ToBoolean(row["snActivo"]),
                FeAlta = Convert.ToDateTime(row["feAlta"]),
                CdUsuarioAlta = row["cdUsuarioAlta"] != DBNull.Value ? Convert.ToInt32(row["cdUsuarioAlta"]) : null,
                FeUltimaModificacion = row["feUltimaModificacion"] != DBNull.Value ? Convert.ToDateTime(row["feUltimaModificacion"]) : null,
                CdUsuarioModificacion = row["cdUsuarioModificacion"] != DBNull.Value ? Convert.ToInt32(row["cdUsuarioModificacion"]) : null
            };
        }
    }
}
