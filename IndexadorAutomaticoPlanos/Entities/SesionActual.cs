namespace IndexadorAutomaticoPlanos.Entities
{
    /// <summary>
    /// Clase estática que mantiene la información del usuario logueado
    /// </summary>
    public static class SesionActual
    {
        public static Usuario? UsuarioActual { get; set; }

        public static bool EstaAutenticado => UsuarioActual != null;

        public static int ObtenerIdUsuario()
        {
            return UsuarioActual?.CdUsuario ?? 0;
        }

        public static string ObtenerNombreUsuario()
        {
            return UsuarioActual?.DsUsuario ?? string.Empty;
        }

        public static string ObtenerNombreCompleto()
        {
            return UsuarioActual?.DsNombreCompleto ?? string.Empty;
        }

        public static void CerrarSesion()
        {
            UsuarioActual = null;
        }
    }
}
