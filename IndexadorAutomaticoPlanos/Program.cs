using IndexadorAutomaticoPlanos.UI;
using IndexadorAutomaticoPlanos.Utils;
using IndexadorAutomaticoPlanos.Security;
using IndexadorAutomaticoPlanos.DataAccess;

namespace IndexadorAutomaticoPlanos
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal de la aplicación
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Configurar la aplicación
            ApplicationConfiguration.Initialize();

            try
            {
                Logger.Info("========================================", "Program");
                Logger.Info("Iniciando Indexador Automático de Planos", "Program");
                Logger.Info("========================================", "Program");

                // Verificar conexión a la base de datos antes de continuar
                if (!VerificarConexionBaseDatos())
                {
                    Application.Exit();
                    return;
                }

                // Mostrar formulario de login
                bool continuar = true;
                while (continuar)
                {
                    using (FrmLogin frmLogin = new FrmLogin())
                    {
                        DialogResult resultado = frmLogin.ShowDialog();

                        if (resultado == DialogResult.OK)
                        {
                            // Login exitoso, abrir formulario principal
                            try
                            {
                                Logger.Info("Creando instancia de FrmPrincipal", "Program");
                                using (FrmPrincipal frmPrincipal = new FrmPrincipal())
                                {
                                    Logger.Info("Mostrando FrmPrincipal", "Program");
                                    DialogResult resultadoPrincipal = frmPrincipal.ShowDialog();

                                    // Si el resultado es Retry, significa que cerró sesión y quiere volver al login
                                    if (resultadoPrincipal == DialogResult.Retry)
                                    {
                                        continue; // Volver al login
                                    }
                                    else
                                    {
                                        continuar = false; // Salir de la aplicación
                                    }
                                }
                            }
                            catch (Exception exPrincipal)
                            {
                                Logger.Error("Error al crear o mostrar FrmPrincipal", exPrincipal, "Program");
                                MessageBox.Show(
                                    $"Error al abrir el formulario principal:\n\n{exPrincipal.Message}\n\nStack Trace:\n{exPrincipal.StackTrace}",
                                    "Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                                continuar = false;
                            }
                        }
                        else
                        {
                            // Usuario canceló el login, salir
                            continuar = false;
                        }
                    }
                }

                Logger.Info("========================================", "Program");
                Logger.Info("Cerrando Indexador Automático de Planos", "Program");
                Logger.Info("========================================", "Program");
            }
            catch (Exception ex)
            {
                Logger.Error("Error fatal en la aplicación", ex, "Program");
                MessageBox.Show(
                    $"Error fatal en la aplicación:\n\n{ex.Message}\n\nLa aplicación se cerrará.",
                    "Error Fatal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Verifica la conexión a la base de datos antes de iniciar la aplicación
        /// </summary>
        private static bool VerificarConexionBaseDatos()
        {
            try
            {
                DatabaseHelper db = new DatabaseHelper();
                if (!db.VerificarConexion(out string mensajeError))
                {
                    Logger.Error($"No se pudo conectar a la base de datos: {mensajeError}", null, "Program");

                    MessageBox.Show(
                        $"No se pudo conectar a la base de datos.\n\n{mensajeError}\n\n" +
                        "Verifique:\n" +
                        "• SQL Server está en ejecución\n" +
                        "• La cadena de conexión en App.config es correcta\n" +
                        "• Los scripts de base de datos fueron ejecutados\n\n" +
                        "La aplicación se cerrará.",
                        "Error de Conexión",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return false;
                }

                Logger.Info("Conexión a base de datos verificada correctamente", "Program");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Error al verificar conexión a base de datos", ex, "Program");

                MessageBox.Show(
                    $"Error al verificar la conexión:\n\n{ex.Message}\n\n" +
                    "Verifique la configuración de App.config.\n\n" +
                    "La aplicación se cerrará.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
        }
    }
}