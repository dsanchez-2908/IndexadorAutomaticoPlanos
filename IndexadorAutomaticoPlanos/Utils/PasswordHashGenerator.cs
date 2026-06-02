using IndexadorAutomaticoPlanos.Security;
using System;

namespace IndexadorAutomaticoPlanos.Utils
{
    /// <summary>
    /// Clase auxiliar para generar hashes de contraseñas
    /// </summary>
    public static class PasswordHashGenerator
    {
        /// <summary>
        /// Genera un hash de BCrypt para una contraseña
        /// Útil para crear scripts SQL con contraseñas encriptadas
        /// </summary>
        public static string GenerarHash(string password)
        {
            return Encriptacion.EncriptarClave(password);
        }

        /// <summary>
        /// Verifica si un hash es válido para una contraseña
        /// </summary>
        public static bool VerificarHash(string password, string hash)
        {
            return Encriptacion.VerificarClave(password, hash);
        }

        /// <summary>
        /// Método para testing - muestra información detallada
        /// </summary>
        public static void ProbarHash()
        {
            Console.WriteLine("=== Generador de Hash BCrypt ===\n");

            // Probar con la contraseña 123
            string password = "123";
            Console.WriteLine($"Generando hash para: '{password}'");

            string hash1 = GenerarHash(password);
            Console.WriteLine($"Hash generado: {hash1}");
            Console.WriteLine($"Longitud: {hash1.Length} caracteres");

            bool verifica = VerificarHash(password, hash1);
            Console.WriteLine($"¿Verifica correctamente?: {verifica}\n");

            // Probar con el hash actual de la BD
            string hashBD = "$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy";
            Console.WriteLine($"Probando hash actual de BD: {hashBD}");
            bool verificaBD = VerificarHash(password, hashBD);
            Console.WriteLine($"¿Verifica correctamente?: {verificaBD}\n");

            if (!verificaBD)
            {
                Console.WriteLine("⚠ EL HASH ACTUAL NO FUNCIONA!");
                Console.WriteLine($"Usar este hash en su lugar:\n{hash1}");
            }
            else
            {
                Console.WriteLine("✓ El hash actual es válido");
            }
        }
    }
}
