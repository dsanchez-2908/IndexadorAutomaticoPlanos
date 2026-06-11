using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using IndexadorAutomaticoPlanos.DataAccess;
using IndexadorAutomaticoPlanos.Entities;
using IndexadorAutomaticoPlanos.Utils;

namespace IndexadorAutomaticoPlanos.UI
{
    /// <summary>
    /// Formulario principal para control de calidad de lotes procesados por IA
    /// FASE 6: Validación y Control de Calidad
    /// </summary>
    public partial class FrmControlCalidad : Form
    {
        private readonly LoteRepository _loteRepo;
        private List<Lote> _lotesCargados;

        public FrmControlCalidad()
        {
            InitializeComponent();
            _loteRepo = new LoteRepository();
            _lotesCargados = new List<Lote>();
        }

        private void FrmControlCalidad_Load(object sender, EventArgs e)
        {
            try
            {
                ConfigurarControles();
                CargarLotesPendientes();
                ActualizarEstado("Lotes cargados correctamente");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cargar formulario de control de calidad", ex, "FrmControlCalidad");
                MessageBox.Show($"Error al cargar el formulario: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Configura los controles del formulario
        /// </summary>
        private void ConfigurarControles()
        {
            // Configurar grid de lotes
            dgvLotes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLotes.MultiSelect = false;
            dgvLotes.ReadOnly = true;
            dgvLotes.AllowUserToAddRows = false;
            dgvLotes.AllowUserToDeleteRows = false;
            dgvLotes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Doble clic para abrir lote
            dgvLotes.DoubleClick += dgvLotes_DoubleClick;
        }

        /// <summary>
        /// Carga los lotes pendientes de control de calidad
        /// </summary>
        private void CargarLotesPendientes()
        {
            try
            {
                _lotesCargados = _loteRepo.ObtenerLotesPendientesControlCalidad();

                // Crear DataTable para el grid
                DataTable dt = new DataTable();
                dt.Columns.Add("CdLote", typeof(int));
                dt.Columns.Add("Nombre Lote", typeof(string));
                dt.Columns.Add("Cant. Archivos", typeof(int));
                dt.Columns.Add("Fecha Alta", typeof(DateTime));
                dt.Columns.Add("Estado", typeof(string));

                foreach (var lote in _lotesCargados)
                {
                    // Obtener estadísticas del lote
                    var stats = _loteRepo.ObtenerEstadisticasLote(lote.CdLote);
                    string estadoDetalle = $"{stats.Controlados}/{stats.Total} controlados";

                    dt.Rows.Add(
                        lote.CdLote,
                        lote.DsNombreLote,
                        lote.NuCantidadArchivos,
                        lote.FeAlta,
                        estadoDetalle
                    );
                }

                dgvLotes.DataSource = dt;

                // Ocultar columna de ID
                if (dgvLotes.Columns["CdLote"] != null)
                {
                    dgvLotes.Columns["CdLote"].Visible = false;
                }

                ActualizarEstado($"{_lotesCargados.Count} lote(s) pendiente(s) de control de calidad");
                Logger.Info($"Cargados {_lotesCargados.Count} lotes pendientes de control", "FrmControlCalidad");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cargar lotes pendientes", ex, "FrmControlCalidad");
                MessageBox.Show($"Error al cargar lotes: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Actualiza el label de estado
        /// </summary>
        private void ActualizarEstado(string mensaje)
        {
            lblEstado.Text = mensaje;
            lblEstado.Refresh();
        }

        /// <summary>
        /// Abre el formulario de control de lote al hacer doble clic
        /// </summary>
        private void dgvLotes_DoubleClick(object sender, EventArgs e)
        {
            AbrirControlLote();
        }

        /// <summary>
        /// Evento click del botón Abrir Lote
        /// </summary>
        private void btnAbrirLote_Click(object sender, EventArgs e)
        {
            AbrirControlLote();
        }

        /// <summary>
        /// Abre el formulario modal de control de lote
        /// </summary>
        private void AbrirControlLote()
        {
            try
            {
                if (dgvLotes.CurrentRow == null)
                {
                    MessageBox.Show("Debe seleccionar un lote para controlar", "Advertencia",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int cdLote = Convert.ToInt32(dgvLotes.CurrentRow.Cells["CdLote"].Value);
                var lote = _lotesCargados.FirstOrDefault(l => l.CdLote == cdLote);

                if (lote == null)
                {
                    MessageBox.Show("No se encontró el lote seleccionado", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Abrir formulario modal de control de lote
                using (var frmLote = new FrmControlLote(lote))
                {
                    frmLote.ShowDialog(this);

                    // Refrescar lista después de cerrar el modal
                    CargarLotesPendientes();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error al abrir control de lote", ex, "FrmControlCalidad");
                MessageBox.Show($"Error al abrir el lote: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Evento click del botón Refrescar
        /// </summary>
        private void btnRefrescar_Click(object sender, EventArgs e)
        {
            CargarLotesPendientes();
        }

        /// <summary>
        /// Evento click del botón Cerrar
        /// </summary>
        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
