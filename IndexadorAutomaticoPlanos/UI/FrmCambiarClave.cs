using IndexadorAutomaticoPlanos.DataAccess;
using IndexadorAutomaticoPlanos.Entities;
using IndexadorAutomaticoPlanos.Security;
using IndexadorAutomaticoPlanos.Utils;

namespace IndexadorAutomaticoPlanos.UI
{
    /// <summary>
    /// Formulario para cambiar la contraseña del usuario
    /// </summary>
    public partial class FrmCambiarClave : Form
    {
        private readonly UsuarioRepository _usuarioRepo;
        private readonly bool _esCambioObligatorio;

        /// <summary>
        /// Constructor del formulario
        /// </summary>
        /// <param name="esCambioObligatorio">Indica si el cambio de clave es obligatorio (primer ingreso o clave temporal)</param>
        public FrmCambiarClave(bool esCambioObligatorio = false)
        {
            InitializeComponent();
            _usuarioRepo = new UsuarioRepository();
            _esCambioObligatorio = esCambioObligatorio;

            this.Load += FrmCambiarClave_Load;
        }

        private void FrmCambiarClave_Load(object? sender, EventArgs e)
        {
            try
            {
                // Si es cambio obligatorio, no permitir cancelar
                if (_esCambioObligatorio)
                {
                    btnCancelar.Enabled = false;
                    lblInfo.Text = "Debe cambiar su contraseña para poder continuar.\n" +
                                   "La contraseña debe tener al menos 6 caracteres.";
                    lblInfo.ForeColor = Color.Red;

                    // Si es primer ingreso, no pedir clave actual
                    if (SesionActual.UsuarioActual?.SnPrimerIngreso == true || 
                        SesionActual.UsuarioActual?.SnClaveTemporal == true)
                    {
                        lblClaveActual.Visible = false;
                        txtClaveActual.Visible = false;
                    }
                }

                txtClaveActual.Focus();
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cargar formulario de cambio de clave", ex, "FrmCambiarClave");
                MessageBox.Show(
                    $"Error al cargar el formulario:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos())
                return;

            try
            {
                btnGuardar.Enabled = false;
                Cursor = Cursors.WaitCursor;

                if (!SesionActual.EstaAutenticado)
                {
                    MessageBox.Show(
                        "No hay una sesión activa. Debe iniciar sesión nuevamente.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    return;
                }

                int cdUsuario = SesionActual.ObtenerIdUsuario();

                // Verificar clave actual si es necesario
                if (txtClaveActual.Visible)
                {
                    string claveActual = txtClaveActual.Text;
                    Usuario? usuario = _usuarioRepo.ObtenerPorId(cdUsuario);

                    if (usuario == null || !Encriptacion.VerificarClave(claveActual, usuario.DsClave))
                    {
                        MessageBox.Show(
                            "La contraseña actual es incorrecta.",
                            "Error de Validación",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        txtClaveActual.Clear();
                        txtClaveActual.Focus();
                        return;
                    }
                }

                // Cambiar la clave
                string nuevaClave = txtNuevaClave.Text;
                bool exito = _usuarioRepo.CambiarClave(cdUsuario, nuevaClave, false, cdUsuario);

                if (exito)
                {
                    Logger.Info($"Contraseña cambiada exitosamente para usuario {SesionActual.ObtenerNombreUsuario()}", "FrmCambiarClave");

                    MessageBox.Show(
                        "Contraseña cambiada exitosamente.",
                        "Éxito",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        "No se pudo cambiar la contraseña. Intente nuevamente.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cambiar contraseña", ex, "FrmCambiarClave");
                MessageBox.Show(
                    $"Error al cambiar la contraseña:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnGuardar.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            if (_esCambioObligatorio)
            {
                MessageBox.Show(
                    "Debe cambiar su contraseña para poder continuar.",
                    "Cambio Obligatorio",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void chkMostrarClaves_CheckedChanged(object sender, EventArgs e)
        {
            bool mostrar = chkMostrarClaves.Checked;
            txtClaveActual.UseSystemPasswordChar = !mostrar;
            txtNuevaClave.UseSystemPasswordChar = !mostrar;
            txtConfirmarClave.UseSystemPasswordChar = !mostrar;
        }

        private bool ValidarCampos()
        {
            // Validar clave actual si es visible
            if (txtClaveActual.Visible && string.IsNullOrWhiteSpace(txtClaveActual.Text))
            {
                MessageBox.Show(
                    "Debe ingresar su contraseña actual.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtClaveActual.Focus();
                return false;
            }

            // Validar nueva clave
            if (string.IsNullOrWhiteSpace(txtNuevaClave.Text))
            {
                MessageBox.Show(
                    "Debe ingresar una nueva contraseña.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtNuevaClave.Focus();
                return false;
            }

            // Validar fortaleza de la clave
            if (!Encriptacion.ValidarFortalezaClave(txtNuevaClave.Text, out string mensajeError))
            {
                MessageBox.Show(
                    mensajeError,
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtNuevaClave.Focus();
                return false;
            }

            // Validar que nueva clave no sea igual a la actual
            if (txtClaveActual.Visible && 
                txtClaveActual.Text == txtNuevaClave.Text)
            {
                MessageBox.Show(
                    "La nueva contraseña debe ser diferente a la actual.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtNuevaClave.Clear();
                txtNuevaClave.Focus();
                return false;
            }

            // Validar confirmación
            if (string.IsNullOrWhiteSpace(txtConfirmarClave.Text))
            {
                MessageBox.Show(
                    "Debe confirmar la nueva contraseña.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtConfirmarClave.Focus();
                return false;
            }

            // Validar que las claves coincidan
            if (txtNuevaClave.Text != txtConfirmarClave.Text)
            {
                MessageBox.Show(
                    "La nueva contraseña y la confirmación no coinciden.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtConfirmarClave.Clear();
                txtConfirmarClave.Focus();
                return false;
            }

            return true;
        }
    }
}
