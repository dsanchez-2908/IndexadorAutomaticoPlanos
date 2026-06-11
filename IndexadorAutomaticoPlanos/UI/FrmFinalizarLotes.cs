using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using IndexadorAutomaticoPlanos.DataAccess;
using IndexadorAutomaticoPlanos.Entities;
using IndexadorAutomaticoPlanos.Security;
using IndexadorAutomaticoPlanos.Utils;

namespace IndexadorAutomaticoPlanos.UI
{
    /// <summary>
    /// Formulario para finalización de lotes:
    /// - Seleccionar lotes en estado "Pendiente de Finalizar"
    /// - Renombrar archivos PDF según nomenclatura
    /// - Generar/actualizar INDEX.csv
    /// - Cambiar estado a "Finalizado"
    /// </summary>
    public partial class FrmFinalizarLotes : Form
    {
        private readonly LoteRepository _loteRepo;
        private readonly string _pathRepositorio;
        private List<Lote> _lotesCargados;

        public FrmFinalizarLotes()
        {
            InitializeComponent();
            _loteRepo = new LoteRepository();
            _lotesCargados = new List<Lote>();

            _pathRepositorio = ConfigurationManager.AppSettings["PATH_REPOSITORIO"]
                ?? throw new Exception("PATH_REPOSITORIO no configurado en App.config");

            ConfigurarGrilla();

            // Evento para inicializar checkboxes después del binding
            dgvLotes.DataBindingComplete += DgvLotes_DataBindingComplete;
        }

        private void DgvLotes_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Inicializar todos los checkboxes a false
            foreach (DataGridViewRow row in dgvLotes.Rows)
            {
                row.Cells["Seleccionar"].Value = false;
            }
        }

        private void FrmFinalizarLotes_Load(object sender, EventArgs e)
        {
            try
            {
                CargarLotesPendientes();
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cargar formulario de finalización", ex, "FrmFinalizarLotes");
                MessageBox.Show(
                    $"Error al cargar formulario: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Configura las columnas de la grilla
        /// </summary>
        private void ConfigurarGrilla()
        {
            dgvLotes.MultiSelect = true;
            dgvLotes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLotes.AutoGenerateColumns = false;
            dgvLotes.AllowUserToAddRows = false;
            dgvLotes.AllowUserToDeleteRows = false;
            dgvLotes.ReadOnly = false; // Cambiar a false para permitir edición de checkbox
            dgvLotes.RowHeadersWidth = 25;

            // Columna de selección (checkbox) - NO bindeada
            var colSeleccionar = new DataGridViewCheckBoxColumn
            {
                Name = "Seleccionar",
                HeaderText = "✓",
                Width = 40,
                ReadOnly = false,
                FalseValue = false,
                TrueValue = true
            };
            dgvLotes.Columns.Add(colSeleccionar);

            // Columnas de datos - TODAS ReadOnly
            dgvLotes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CdLote",
                HeaderText = "Código",
                DataPropertyName = "CdLote",
                Width = 80,
                Visible = false,
                ReadOnly = true
            });

            dgvLotes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DsNombreLote",
                HeaderText = "Lote",
                DataPropertyName = "DsNombreLote",
                Width = 150,
                ReadOnly = true
            });

            dgvLotes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DsCarpetaOrigen",
                HeaderText = "Carpeta Origen",
                DataPropertyName = "DsCarpetaOrigen",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });

            dgvLotes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CantidadArchivos",
                HeaderText = "Archivos",
                DataPropertyName = "CantidadArchivos",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter },
                ReadOnly = true
            });

            dgvLotes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FeAlta",
                HeaderText = "Fecha Creación",
                DataPropertyName = "FeAlta",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" },
                ReadOnly = true
            });

            dgvLotes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DsEstadoLote",
                HeaderText = "Estado",
                DataPropertyName = "DsEstadoLote",
                Width = 150,
                ReadOnly = true
            });
        }

        /// <summary>
        /// Carga lotes en estado "Pendiente de Finalizar"
        /// </summary>
        private void CargarLotesPendientes()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                btnFinalizarSeleccionados.Enabled = false;

                _lotesCargados = _loteRepo.ObtenerLotesPendientesFinalizacion();
                dgvLotes.DataSource = null;
                dgvLotes.DataSource = _lotesCargados;

                if (_lotesCargados.Count == 0)
                {
                    MessageBox.Show(
                        "No hay lotes pendientes de finalización.",
                        "Información",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else
                {
                    btnFinalizarSeleccionados.Enabled = true;
                    Logger.Info($"{_lotesCargados.Count} lote(s) pendiente(s) cargado(s)", "FrmFinalizarLotes");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cargar lotes pendientes", ex, "FrmFinalizarLotes");
                MessageBox.Show(
                    $"Error al cargar lotes: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Procesa lotes seleccionados
        /// </summary>
        private async void btnFinalizarSeleccionados_Click(object sender, EventArgs e)
        {
            var lotesSeleccionados = ObtenerLotesSeleccionados();

            if (lotesSeleccionados.Count == 0)
            {
                MessageBox.Show(
                    "Debe seleccionar al menos un lote para finalizar.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // Confirmación
            var resultado = MessageBox.Show(
                $"¿Está seguro de finalizar {lotesSeleccionados.Count} lote(s) seleccionado(s)?\n\n" +
                "Este proceso:\n" +
                "- Renombrará los archivos PDF\n" +
                "- Generará/actualizará el archivo INDEX.csv\n" +
                "- Cambiará el estado a 'Finalizado'\n\n" +
                "El proceso puede tardar unos minutos.",
                "Confirmar Finalización",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (resultado != DialogResult.Yes)
            {
                return;
            }

            // Deshabilitar controles
            btnFinalizarSeleccionados.Enabled = false;
            btnActualizar.Enabled = false;
            btnCerrar.Enabled = false;
            dgvLotes.Enabled = false;
            pnlProgreso.Visible = true;
            progressBar.Maximum = lotesSeleccionados.Count;
            progressBar.Value = 0;

            try
            {
                int exitosos = 0;
                int fallidos = 0;
                var errores = new List<string>();

                foreach (var lote in lotesSeleccionados)
                {
                    lblProgreso.Text = $"Procesando lote {lote.DsNombreLote} ({progressBar.Value + 1}/{progressBar.Maximum})...";
                    Application.DoEvents();

                    try
                    {
                        await Task.Run(() => ProcesarLote(lote));
                        exitosos++;
                        Logger.Info($"Lote {lote.DsNombreLote} finalizado correctamente", "FrmFinalizarLotes");
                    }
                    catch (Exception ex)
                    {
                        fallidos++;
                        string mensajeError = $"{lote.DsNombreLote}: {ex.Message}";
                        errores.Add(mensajeError);
                        Logger.Error($"Error al finalizar lote {lote.DsNombreLote}", ex, "FrmFinalizarLotes");
                    }

                    progressBar.Value++;
                    Application.DoEvents();
                }

                // Mostrar resumen
                string mensaje = $"Proceso finalizado.\n\n" +
                                $"Lotes procesados correctamente: {exitosos}\n" +
                                $"Lotes con error: {fallidos}";

                if (errores.Any())
                {
                    mensaje += "\n\nErrores:\n" + string.Join("\n", errores.Take(5));
                    if (errores.Count > 5)
                    {
                        mensaje += $"\n... y {errores.Count - 5} error(es) más.";
                    }
                }

                MessageBox.Show(
                    mensaje,
                    "Resultado",
                    MessageBoxButtons.OK,
                    exitosos > 0 && fallidos == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning
                );

                // Recargar grilla
                CargarLotesPendientes();
            }
            catch (Exception ex)
            {
                Logger.Error("Error general al procesar lotes", ex, "FrmFinalizarLotes");
                MessageBox.Show(
                    $"Error general: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                // Restaurar controles
                btnFinalizarSeleccionados.Enabled = true;
                btnActualizar.Enabled = true;
                btnCerrar.Enabled = true;
                dgvLotes.Enabled = true;
                pnlProgreso.Visible = false;
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Procesa un lote individual: renombrar, CSV, cambiar estado
        /// </summary>
        private void ProcesarLote(Lote lote)
        {
            // 1. Obtener archivos del lote
            var archivos = _loteRepo.ObtenerArchivosParaFinalizacion(lote.CdLote);

            if (archivos.Count == 0)
            {
                throw new Exception("El lote no tiene archivos asociados");
            }

            // 2. Renombrar archivos y generar CSV
            var finalizador = new FinalizadorLotes(_pathRepositorio);
            var resultado = finalizador.ProcesarLote(archivos);

            if (!resultado.Exito)
            {
                throw new Exception(resultado.Mensaje);
            }

            // 3. Cambiar estado del lote a "Finalizado" (cdEstadoLote = 5)
            int cdEstadoFinalizado = 5;
            int cdUsuario = SesionActual.UsuarioActual?.CdUsuario ?? 1;

            _loteRepo.CambiarEstadoLote(lote.CdLote, cdEstadoFinalizado, cdUsuario);

            Logger.Info($"Lote {lote.DsNombreLote} finalizado: {resultado.ArchivosRenombrados.Count} archivo(s), CSV: {resultado.RutaArchivoCSV}", "FrmFinalizarLotes");
        }

        /// <summary>
        /// Obtiene los lotes seleccionados mediante checkboxes
        /// </summary>
        private List<Lote> ObtenerLotesSeleccionados()
        {
            var seleccionados = new List<Lote>();

            foreach (DataGridViewRow fila in dgvLotes.Rows)
            {
                var checkCell = fila.Cells["Seleccionar"] as DataGridViewCheckBoxCell;
                if (checkCell != null && checkCell.Value != null && (bool)checkCell.Value)
                {
                    var lote = fila.DataBoundItem as Lote;
                    if (lote != null)
                    {
                        seleccionados.Add(lote);
                    }
                }
            }

            return seleccionados;
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarLotesPendientes();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
