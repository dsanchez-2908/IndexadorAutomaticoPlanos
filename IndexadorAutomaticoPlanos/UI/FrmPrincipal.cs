using IndexadorAutomaticoPlanos.Entities;
using IndexadorAutomaticoPlanos.Utils;

namespace IndexadorAutomaticoPlanos.UI
{
    /// <summary>
    /// Formulario principal MDI de la aplicación
    /// </summary>
    public partial class FrmPrincipal : Form
    {
        private System.Windows.Forms.Timer? _timerFecha;

        public FrmPrincipal()
        {
            try
            {
                InitializeComponent();

                this.Load += FrmPrincipal_Load;
                this.FormClosing += FrmPrincipal_FormClosing;

                Logger.Info("FrmPrincipal constructor completado", "FrmPrincipal");
            }
            catch (Exception ex)
            {
                Logger.Error("Error en constructor FrmPrincipal", ex, "FrmPrincipal");
                MessageBox.Show(
                    $"Error al inicializar el formulario principal:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Error Fatal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                throw;
            }
        }

        private void FrmPrincipal_Load(object? sender, EventArgs e)
        {
            try
            {
                Logger.Info("FrmPrincipal_Load iniciado", "FrmPrincipal");

                // Verificar que haya un usuario autenticado
                if (!SesionActual.EstaAutenticado)
                {
                    Logger.Warning("Intento de cargar FrmPrincipal sin sesión activa", "FrmPrincipal");
                    MessageBox.Show(
                        "No hay una sesión activa. La aplicación se cerrará.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                Logger.Info($"Usuario autenticado: {SesionActual.ObtenerNombreUsuario()}", "FrmPrincipal");

                // Configurar barra de estado con manejo defensivo
                if (statusLabelUsuario != null)
                {
                    string textoUsuario = $"Usuario: {SesionActual.ObtenerNombreCompleto()} ({SesionActual.ObtenerNombreUsuario()})";
                    Logger.Info($"Configurando status usuario: {textoUsuario}", "FrmPrincipal");
                    statusLabelUsuario.Text = textoUsuario;
                }
                else
                {
                    Logger.Warning("statusLabelUsuario es null", "FrmPrincipal");
                }

                if (statusLabelFecha != null)
                {
                    statusLabelFecha.Text = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy - HH:mm:ss");
                }
                else
                {
                    Logger.Warning("statusLabelFecha es null", "FrmPrincipal");
                }

                // Iniciar timer para actualizar la fecha
                _timerFecha = new System.Windows.Forms.Timer();
                _timerFecha.Interval = 1000; // 1 segundo
                _timerFecha.Tick += TimerFecha_Tick;
                _timerFecha.Start();

                Logger.Info($"Usuario {SesionActual.ObtenerNombreUsuario()} ingresó al sistema", "FrmPrincipal");

                // TODO: Aquí se podría abrir automáticamente un Dashboard en el MDI
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cargar formulario principal", ex, "FrmPrincipal");
                MessageBox.Show(
                    $"Error al cargar el formulario principal:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void TimerFecha_Tick(object? sender, EventArgs e)
        {
            if (statusLabelFecha != null)
            {
                statusLabelFecha.Text = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy - HH:mm:ss");
            }
        }

        private void FrmPrincipal_FormClosing(object? sender, FormClosingEventArgs e)
        {
            try
            {
                // Detener el timer
                if (_timerFecha != null)
                {
                    _timerFecha.Stop();
                    _timerFecha.Dispose();
                }

                // Confirmar cierre
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    DialogResult resultado = MessageBox.Show(
                        "¿Está seguro que desea salir de la aplicación?",
                        "Confirmar Salida",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (resultado == DialogResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                // Cerrar sesión y loguear
                if (SesionActual.EstaAutenticado)
                {
                    Logger.Info($"Usuario {SesionActual.ObtenerNombreUsuario()} cerró la aplicación", "FrmPrincipal");
                    SesionActual.CerrarSesion();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cerrar formulario principal", ex, "FrmPrincipal");
            }
        }

        private void menuCerrarSesion_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult resultado = MessageBox.Show(
                    "¿Está seguro que desea cerrar la sesión actual?",
                    "Confirmar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (resultado == DialogResult.Yes)
                {
                    Logger.Info($"Usuario {SesionActual.ObtenerNombreUsuario()} cerró sesión", "FrmPrincipal");
                    SesionActual.CerrarSesion();

                    // Cerrar el formulario principal y volver al login
                    this.DialogResult = DialogResult.Retry; // Usamos Retry para indicar que debe volver al login
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cerrar sesión", ex, "FrmPrincipal");
                MessageBox.Show(
                    $"Error al cerrar sesión:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void menuSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void menuCambiarClave_Click(object sender, EventArgs e)
        {
            try
            {
                using (FrmCambiarClave frmCambiarClave = new FrmCambiarClave(false))
                {
                    frmCambiarClave.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error al abrir formulario de cambio de clave", ex, "FrmPrincipal");
                MessageBox.Show(
                    $"Error al abrir el formulario:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void menuAcercaDe_Click(object sender, EventArgs e)
        {
            string mensaje = "Indexador Automático de Planos\n" +
                           "Versión 1.0.0\n\n" +
                           "Sistema para la indexación masiva de planos mediante OpenAI.\n\n" +
                           "Desarrollado en C# .NET 10 - Windows Forms\n" +
                           $"© {DateTime.Now.Year}";

            MessageBox.Show(
                mensaje,
                "Acerca de",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void menuIngresoLotes_Click(object sender, EventArgs e)
        {
            try
            {
                FrmIngresoPlanos frmIngresoPlanos = new FrmIngresoPlanos();
                AbrirFormularioHijo(frmIngresoPlanos);
                Logger.Info("Abriendo formulario de ingreso de planos", "FrmPrincipal");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al abrir formulario de ingreso de planos", ex, "FrmPrincipal");
                MessageBox.Show(
                    $"Error al abrir el formulario de ingreso de planos:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void menuPreparacionLotes_Click(object sender, EventArgs e)
        {
            try
            {
                FrmPreparacionLotes frmPreparacionLotes = new FrmPreparacionLotes();
                AbrirFormularioHijo(frmPreparacionLotes);
                Logger.Info("Abriendo formulario de preparación de lotes", "FrmPrincipal");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al abrir formulario de preparación de lotes", ex, "FrmPrincipal");
                MessageBox.Show(
                    $"Error al abrir el formulario de preparación de lotes:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void menuPreparacionImagenes_Click(object sender, EventArgs e)
        {
            try
            {
                FrmPreparacionImagenes frmPreparacionImagenes = new FrmPreparacionImagenes();
                AbrirFormularioHijo(frmPreparacionImagenes);
                Logger.Info("Abriendo formulario de preparación de imágenes", "FrmPrincipal");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al abrir formulario de preparación de imágenes", ex, "FrmPrincipal");
                MessageBox.Show(
                    $"Error al abrir el formulario de preparación de imágenes:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void menuProcesamientoIA_Click(object sender, EventArgs e)
        {
            try
            {
                FrmProcesamientoIA frmProcesamientoIA = new FrmProcesamientoIA();
                AbrirFormularioHijo(frmProcesamientoIA);
                Logger.Info("Abriendo formulario de procesamiento por OpenAI", "FrmPrincipal");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al abrir formulario de procesamiento por OpenAI", ex, "FrmPrincipal");
                MessageBox.Show(
                    $"Error al abrir el formulario de procesamiento por OpenAI:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void menuControlCalidad_Click(object sender, EventArgs e)
        {
            try
            {
                FrmControlCalidad frmControlCalidad = new FrmControlCalidad();
                AbrirFormularioHijo(frmControlCalidad);
                Logger.Info("Abriendo formulario de control de calidad", "FrmPrincipal");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al abrir formulario de control de calidad", ex, "FrmPrincipal");
                MessageBox.Show(
                    $"Error al abrir el formulario de control de calidad:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void menuFinalizarLotes_Click(object sender, EventArgs e)
        {
            try
            {
                FrmFinalizarLotes frmFinalizarLotes = new FrmFinalizarLotes();
                AbrirFormularioHijo(frmFinalizarLotes);
                Logger.Info("Abriendo formulario de finalización de lotes", "FrmPrincipal");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al abrir formulario de finalización de lotes", ex, "FrmPrincipal");
                MessageBox.Show(
                    $"Error al abrir el formulario de finalización de lotes:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Método auxiliar para abrir formularios MDI hijos (evita duplicados)
        /// </summary>
        private void AbrirFormularioHijo(Form formulario)
        {
            try
            {
                // Verificar si ya hay una instancia del formulario abierta
                foreach (Form hijo in this.MdiChildren)
                {
                    if (hijo.GetType() == formulario.GetType())
                    {
                        hijo.Activate();
                        formulario.Dispose();
                        return;
                    }
                }

                // Si no existe, abrir el formulario
                formulario.MdiParent = this;
                formulario.Show();
            }
            catch (Exception ex)
            {
                Logger.Error("Error al abrir formulario hijo", ex, "FrmPrincipal");
                MessageBox.Show(
                    $"Error al abrir el formulario:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
