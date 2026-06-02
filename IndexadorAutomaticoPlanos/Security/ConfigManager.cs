using System.Configuration;

namespace IndexadorAutomaticoPlanos.Security
{
    /// <summary>
    /// Clase para manejo de configuración de la aplicación (App.config)
    /// </summary>
    public static class ConfigManager
    {
        private const string CONEXION_KEY = "ConexionBD";
        private const string CONEXION_ENCRIPTADA_KEY = "ConexionBDEncriptada";

        /// <summary>
        /// Obtiene la cadena de conexión a la base de datos
        /// Primero intenta obtener la versión encriptada, si no existe usa la normal
        /// </summary>
        /// <returns>Cadena de conexión</returns>
        public static string ObtenerCadenaConexion()
        {
            try
            {
                // Intentar obtener la cadena encriptada
                string? conexionEncriptada = ConfigurationManager.AppSettings[CONEXION_ENCRIPTADA_KEY];

                if (!string.IsNullOrEmpty(conexionEncriptada))
                {
                    return Encriptacion.DesencriptarCadena(conexionEncriptada);
                }

                // Si no hay encriptada, usar la normal
                string? conexionNormal = ConfigurationManager.AppSettings[CONEXION_KEY];
                return conexionNormal ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener la cadena de conexión. Verifique la configuración.", ex);
            }
        }

        /// <summary>
        /// Guarda una cadena de conexión encriptada en App.config
        /// </summary>
        /// <param name="cadenaConexion">Cadena de conexión en texto plano</param>
        public static void GuardarCadenaConexionEncriptada(string cadenaConexion)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                string cadenaEncriptada = Encriptacion.EncriptarCadena(cadenaConexion);

                if (config.AppSettings.Settings[CONEXION_ENCRIPTADA_KEY] != null)
                {
                    config.AppSettings.Settings[CONEXION_ENCRIPTADA_KEY].Value = cadenaEncriptada;
                }
                else
                {
                    config.AppSettings.Settings.Add(CONEXION_ENCRIPTADA_KEY, cadenaEncriptada);
                }

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar la cadena de conexión encriptada.", ex);
            }
        }

        /// <summary>
        /// Obtiene un valor de configuración del App.config
        /// </summary>
        /// <param name="clave">Clave del setting</param>
        /// <returns>Valor del setting o cadena vacía si no existe</returns>
        public static string ObtenerConfiguracion(string clave)
        {
            return ConfigurationManager.AppSettings[clave] ?? string.Empty;
        }

        /// <summary>
        /// Guarda un valor de configuración en App.config
        /// </summary>
        /// <param name="clave">Clave del setting</param>
        /// <param name="valor">Valor a guardar</param>
        public static void GuardarConfiguracion(string clave, string valor)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                if (config.AppSettings.Settings[clave] != null)
                {
                    config.AppSettings.Settings[clave].Value = valor;
                }
                else
                {
                    config.AppSettings.Settings.Add(clave, valor);
                }

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar la configuración '{clave}'.", ex);
            }
        }

        /// <summary>
        /// Construye una cadena de conexión a SQL Server
        /// </summary>
        public static string ConstruirCadenaConexion(string servidor, string baseDatos, string usuario, string clave, bool integratedSecurity = false)
        {
            if (integratedSecurity)
            {
                return $"Server={servidor};Database={baseDatos};Integrated Security=True;TrustServerCertificate=True;";
            }
            else
            {
                return $"Server={servidor};Database={baseDatos};User Id={usuario};Password={clave};TrustServerCertificate=True;";
            }
        }

        /// <summary>
        /// Verifica si existe una cadena de conexión configurada
        /// </summary>
        public static bool ExisteCadenaConexion()
        {
            string conexion = ObtenerCadenaConexion();
            return !string.IsNullOrEmpty(conexion);
        }
    }
}
