using IndexadorAutomaticoPlanos.DataAccess;
using IndexadorAutomaticoPlanos.Entities;
using IndexadorAutomaticoPlanos.Utils;

namespace IndexadorAutomaticoPlanos.UI
{
    /// <summary>
    /// Formulario de Login del sistema
    /// </summary>
    public partial class FrmLogin : Form
    {
        private readonly UsuarioRepository _usuarioRepo;

        public FrmLogin()
        {
            InitializeComponent();
            _usuarioRepo = new UsuarioRepository();

            // Configurar eventos
            this.Load += FrmLogin_Load;
        }

        private void FrmLogin_Load(object? sender, EventArgs e)
        {
            try
            {
                // Verificar conexión a la base de datos
                DatabaseHelper db = new DatabaseHelper();
                if (!db.VerificarConexion(out string mensajeError))
                {
                    MessageBox.Show(
                        $"No se pudo conectar a la base de datos.\n\n{mensajeError}\n\nVerifique la configuración y que SQL Server esté en ejecución.",
                        "Error de Conexión",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    this.Close();
                    return;
                }

                // Focus en el campo de usuario
                txtUsuario.Focus();
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cargar formulario de login", ex, "FrmLogin");
                MessageBox.Show(
                    $"Error al iniciar la aplicación:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos())
                return;

            try
            {
                // Deshabilitar botón para evitar doble clic
                btnIngresar.Enabled = false;
                Cursor = Cursors.WaitCursor;

                string usuario = txtUsuario.Text.Trim();
                string clave = txtClave.Text;

                // Intentar autenticar
                Usuario? usuarioAutenticado = _usuarioRepo.Autenticar(usuario, clave);

                if (usuarioAutenticado == null)
                {
                    Logger.Warning($"Intento de login fallido para usuario: {usuario}", "FrmLogin");

                    MessageBox.Show(
                        "Usuario o contraseña incorrectos.",
                        "Error de Autenticación",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    txtClave.Clear();
                    txtClave.Focus();
                    return;
                }

                // Usuario autenticado correctamente
                Logger.Info($"Usuario {usuario} autenticado correctamente", "FrmLogin");

                // Guardar usuario en sesión
                SesionActual.UsuarioActual = usuarioAutenticado;

                // Verificar si tiene clave temporal o es primer ingreso
                if (usuarioAutenticado.SnClaveTemporal || usuarioAutenticado.SnPrimerIngreso)
                {
                    Logger.Info($"Usuario {usuario} debe cambiar su contraseña (Temporal: {usuarioAutenticado.SnClaveTemporal}, Primer ingreso: {usuarioAutenticado.SnPrimerIngreso})", "FrmLogin");

                    MessageBox.Show(
                        "Debe cambiar su contraseña antes de continuar.",
                        "Cambio de Contraseña Requerido",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    // Abrir formulario de cambio de clave
                    using (FrmCambiarClave frmCambiarClave = new FrmCambiarClave(true))
                    {
                        if (frmCambiarClave.ShowDialog() == DialogResult.OK)
                        {
                            // Clave cambiada exitosamente, continuar al sistema
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            // Usuario canceló el cambio de clave, cerrar sesión
                            SesionActual.CerrarSesion();
                            MessageBox.Show(
                                "Debe cambiar su contraseña para poder ingresar al sistema.",
                                "Acceso Denegado",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }
                    }
                }
                else
                {
                    // Login exitoso, abrir formulario principal
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error al intentar ingresar", ex, "FrmLogin");
                MessageBox.Show(
                    $"Error al intentar ingresar:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnIngresar.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void chkMostrarClave_CheckedChanged(object sender, EventArgs e)
        {
            txtClave.UseSystemPasswordChar = !chkMostrarClave.Checked;
        }

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text))
            {
                MessageBox.Show(
                    "Debe ingresar un usuario.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtUsuario.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtClave.Text))
            {
                MessageBox.Show(
                    "Debe ingresar una contraseña.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtClave.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Permite el ingreso con Enter en los campos de texto
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                if (txtUsuario.Focused)
                {
                    txtClave.Focus();
                    return true;
                }
                else if (txtClave.Focused)
                {
                    btnIngresar_Click(this, EventArgs.Empty);
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
