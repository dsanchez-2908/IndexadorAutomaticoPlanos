using System.Security.Cryptography;
using System.Text;

namespace IndexadorAutomaticoPlanos.Security
{
    /// <summary>
    /// Clase para manejo de encriptación de contraseñas y cadenas de conexión
    /// </summary>
    public static class Encriptacion
    {
        /// <summary>
        /// Encripta una contraseña usando BCrypt
        /// </summary>
        /// <param name="clave">Contraseña en texto plano</param>
        /// <returns>Hash de la contraseña</returns>
        public static string EncriptarClave(string clave)
        {
            return BCrypt.Net.BCrypt.HashPassword(clave, BCrypt.Net.BCrypt.GenerateSalt(11));
        }

        /// <summary>
        /// Verifica si una contraseña coincide con un hash
        /// </summary>
        /// <param name="clave">Contraseña en texto plano</param>
        /// <param name="hash">Hash almacenado</param>
        /// <returns>True si la contraseña es correcta</returns>
        public static bool VerificarClave(string clave, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(clave, hash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Encripta una cadena usando DPAPI (Data Protection API de Windows)
        /// El cifrado es específico del usuario de Windows actual
        /// </summary>
        /// <param name="textoPlano">Texto a encriptar</param>
        /// <returns>Texto encriptado en Base64</returns>
        public static string EncriptarCadena(string textoPlano)
        {
            if (string.IsNullOrEmpty(textoPlano))
                return string.Empty;

            byte[] datos = Encoding.UTF8.GetBytes(textoPlano);
            byte[] datosEncriptados = ProtectedData.Protect(datos, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(datosEncriptados);
        }

        /// <summary>
        /// Desencripta una cadena usando DPAPI
        /// </summary>
        /// <param name="textoEncriptado">Texto encriptado en Base64</param>
        /// <returns>Texto en claro</returns>
        public static string DesencriptarCadena(string textoEncriptado)
        {
            if (string.IsNullOrEmpty(textoEncriptado))
                return string.Empty;

            try
            {
                byte[] datosEncriptados = Convert.FromBase64String(textoEncriptado);
                byte[] datos = ProtectedData.Unprotect(datosEncriptados, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(datos);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al desencriptar la cadena. Asegúrese de que esté ejecutando con el mismo usuario de Windows que encriptó los datos.", ex);
            }
        }

        /// <summary>
        /// Valida que una contraseña cumpla con los requisitos mínimos de seguridad
        /// </summary>
        /// <param name="clave">Contraseña a validar</param>
        /// <param name="mensajeError">Mensaje de error si no es válida</param>
        /// <returns>True si la contraseña es válida</returns>
        public static bool ValidarFortalezaClave(string clave, out string mensajeError)
        {
            mensajeError = string.Empty;

            if (string.IsNullOrEmpty(clave))
            {
                mensajeError = "La contraseña no puede estar vacía.";
                return false;
            }

            if (clave.Length < 6)
            {
                mensajeError = "La contraseña debe tener al menos 6 caracteres.";
                return false;
            }

            // Opcional: Agregar más validaciones según necesidad
            // - Al menos una mayúscula
            // - Al menos una minúscula
            // - Al menos un número
            // - Al menos un carácter especial

            return true;
        }
    }
}
