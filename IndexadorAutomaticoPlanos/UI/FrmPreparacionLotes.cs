using IndexadorAutomaticoPlanos.DataAccess;
using IndexadorAutomaticoPlanos.Entities;
using IndexadorAutomaticoPlanos.Utils;
using System.Data;

namespace IndexadorAutomaticoPlanos.UI
{
    /// <summary>
    /// Formulario para preparación de lotes de archivos PDF
    /// Permite agrupar archivos por carpeta y crear lotes para procesamiento por OpenAI
    /// </summary>
    public partial class FrmPreparacionLotes : Form
    {
        private readonly LoteRepository _loteRepo;
        private readonly ArchivoRepository _archivoRepo;
        private List<Archivo> _archivosDisponibles;
        private List<Lote> _lotesCreados;

        public FrmPreparacionLotes()
        {
            InitializeComponent();
            _loteRepo = new LoteRepository();
            _archivoRepo = new ArchivoRepository();
            _archivosDisponibles = new List<Archivo>();
            _lotesCreados = new List<Lote>();
        }

        private void FrmPreparacionLotes_Load(object sender, EventArgs e)
        {
            try
            {
                ConfigurarDataGridViews();
                CargarDatos();
                ActualizarEstadisticas();
                Logger.Info("Formulario de preparación de lotes cargado", "FrmPreparacionLotes");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cargar formulario de preparación de lotes", ex, "FrmPreparacionLotes");
                MessageBox.Show($"Error al cargar el formulario:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarDataGridViews()
        {
            // Configurar grilla de archivos disponibles
            dgvArchivosDisponibles.AutoGenerateColumns = false;
            dgvArchivosDisponibles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvArchivosDisponibles.MultiSelect = true;
            dgvArchivosDisponibles.AllowUserToAddRows = false;
            dgvArchivosDisponibles.ReadOnly = true;

            // Configurar grilla de lotes
            dgvLotes.AutoGenerateColumns = false;
            dgvLotes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLotes.MultiSelect = false;
            dgvLotes.AllowUserToAddRows = false;
            dgvLotes.ReadOnly = true;
        }

        private void CargarDatos()
        {
            try
            {
                // Cargar archivos disponibles
                _archivosDisponibles = _loteRepo.ObtenerArchivosDisponibles();
                CargarArchivosEnGrilla();

                // Cargar lotes creados
                _lotesCreados = _loteRepo.ObtenerTodosLotes();
                CargarLotesEnGrilla();

                Logger.Info($"Datos cargados: {_archivosDisponibles.Count} archivos disponibles, {_lotesCreados.Count} lotes creados", "FrmPreparacionLotes");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cargar datos", ex, "FrmPreparacionLotes");
                throw;
            }
        }

        private void CargarArchivosEnGrilla()
        {
            dgvArchivosDisponibles.Rows.Clear();

            // Agrupar por carpeta
            var archivosPorCarpeta = _archivosDisponibles
                .GroupBy(a => a.DsNombreUltimaCarpeta)
                .OrderBy(g => g.Key);

            foreach (var grupo in archivosPorCarpeta)
            {
                // Agregar fila de encabezado de carpeta
                int idxCarpeta = dgvArchivosDisponibles.Rows.Add();
                var rowCarpeta = dgvArchivosDisponibles.Rows[idxCarpeta];
                rowCarpeta.Cells["colCarpeta"].Value = grupo.Key;
                rowCarpeta.Cells["colCantidad"].Value = grupo.Count();
                rowCarpeta.Cells["colPaginasTotal"].Value = grupo.Sum(a => a.NuCantidadPaginas);
                rowCarpeta.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
                rowCarpeta.DefaultCellStyle.Font = new Font(dgvArchivosDisponibles.Font, FontStyle.Bold);
                rowCarpeta.Tag = "GRUPO";

                // Agregar archivos del grupo
                foreach (var archivo in grupo.OrderBy(a => a.DsNombreArchivo))
                {
                    int idx = dgvArchivosDisponibles.Rows.Add();
                    var row = dgvArchivosDisponibles.Rows[idx];
                    row.Cells["colCarpeta"].Value = $"   {archivo.DsNombreArchivo}";
                    row.Cells["colCantidad"].Value = archivo.NuCantidadPaginas;
                    row.Cells["colPaginasTotal"].Value = archivo.TamanoLegible;
                    row.Tag = archivo;
                }
            }
        }

        private void CargarLotesEnGrilla()
        {
            dgvLotes.Rows.Clear();

            foreach (var lote in _lotesCreados)
            {
                int idx = dgvLotes.Rows.Add();
                var row = dgvLotes.Rows[idx];
                row.Cells["colNombreLote"].Value = lote.DsNombreLote;
                row.Cells["colEstadoLote"].Value = lote.DsEstadoLote;
                row.Cells["colCantidadArchivos"].Value = lote.NuCantidadArchivos;
                row.Cells["colFechaCreacion"].Value = lote.FeAlta.ToString("yyyy-MM-dd HH:mm");
                row.Tag = lote;

                // Colorear según estado
                if (lote.DsEstadoLote == "Finalizado")
                    row.DefaultCellStyle.BackColor = System.Drawing.Color.LightGreen;
                else if (lote.DsEstadoLote?.Contains("Pendiente") == true)
                    row.DefaultCellStyle.BackColor = System.Drawing.Color.LightYellow;
            }
        }

        private void ActualizarEstadisticas()
        {
            int totalArchivos = _archivosDisponibles.Count;
            int totalLotes = _lotesCreados.Count;
            int carpetasDistintas = _archivosDisponibles.Select(a => a.DsNombreUltimaCarpeta).Distinct().Count();

            lblEstadisticas.Text = $"Archivos disponibles: {totalArchivos} | Carpetas: {carpetasDistintas} | Lotes creados: {totalLotes}";
        }

        private void btnCrearLote_Click(object sender, EventArgs e)
        {
            try
            {
                // Obtener carpeta seleccionada
                if (dgvArchivosDisponibles.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Debe seleccionar una carpeta o archivos para crear un lote.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Recolectar archivos seleccionados
                List<int> idsArchivos = new List<int>();
                string? carpetaSeleccionada = null;

                foreach (DataGridViewRow row in dgvArchivosDisponibles.SelectedRows)
                {
                    if (row.Tag is Archivo archivo)
                    {
                        idsArchivos.Add(archivo.CdArchivo);

                        // Validar que todos sean de la misma carpeta
                        if (carpetaSeleccionada == null)
                            carpetaSeleccionada = archivo.DsNombreUltimaCarpeta;
                        else if (carpetaSeleccionada != archivo.DsNombreUltimaCarpeta)
                        {
                            MessageBox.Show("No puede seleccionar archivos de diferentes carpetas.\n\nLos archivos de una misma carpeta deben permanecer juntos en el mismo lote.", "Error de Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                }

                if (idsArchivos.Count == 0)
                {
                    MessageBox.Show("Debe seleccionar al menos un archivo para crear el lote.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Validar cantidad máxima (advertencia si > 100)
                if (idsArchivos.Count > 100)
                {
                    DialogResult result = MessageBox.Show(
                        $"Ha seleccionado {idsArchivos.Count} archivos.\n\nSe recomienda un máximo de 100 archivos por lote para un procesamiento óptimo.\n\n¿Desea continuar de todos modos?",
                        "Advertencia de Cantidad",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result != DialogResult.Yes)
                        return;
                }

                DialogResult confirmacion = MessageBox.Show(
                    $"¿Desea crear un lote con {idsArchivos.Count} archivo(s) de la carpeta '{carpetaSeleccionada}'?",
                    "Confirmar Creación de Lote",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmacion != DialogResult.Yes)
                    return;

                int cdUsuario = SesionActual.UsuarioActual?.CdUsuario ?? 1;
                int cdLote = _loteRepo.CrearLote(idsArchivos, cdUsuario);

                MessageBox.Show($"Lote creado exitosamente.\n\nNombre: LOTE_{cdLote:D6}\nArchivos: {idsArchivos.Count}", "Lote Creado", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Recargar datos
                CargarDatos();
                ActualizarEstadisticas();
            }
            catch (Exception ex)
            {
                Logger.Error("Error al crear lote", ex, "FrmPreparacionLotes");
                MessageBox.Show($"Error al crear el lote:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDividirLote_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvLotes.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Debe seleccionar un lote para dividir.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var row = dgvLotes.SelectedRows[0];
                if (row.Tag is not Lote lote)
                    return;

                if (lote.NuCantidadArchivos < 2)
                {
                    MessageBox.Show("No se puede dividir un lote con menos de 2 archivos.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult confirmacion = MessageBox.Show(
                    $"¿Desea dividir el lote {lote.DsNombreLote}?\n\nCantidad actual de archivos: {lote.NuCantidadArchivos}\n\nSe creará un nuevo lote con aproximadamente la mitad de los archivos.",
                    "Confirmar División de Lote",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmacion != DialogResult.Yes)
                    return;

                int cdUsuario = SesionActual.UsuarioActual?.CdUsuario ?? 1;
                int cdNuevoLote = _loteRepo.DividirLote(lote.CdLote, cdUsuario);

                MessageBox.Show($"Lote dividido exitosamente.\n\nLote original: {lote.DsNombreLote}\nNuevo lote creado: LOTE_{cdNuevoLote:D6}", "División Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Recargar datos
                CargarDatos();
                ActualizarEstadisticas();
            }
            catch (Exception ex)
            {
                Logger.Error("Error al dividir lote", ex, "FrmPreparacionLotes");
                MessageBox.Show($"Error al dividir el lote:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefrescar_Click(object sender, EventArgs e)
        {
            try
            {
                CargarDatos();
                ActualizarEstadisticas();
                MessageBox.Show("Datos actualizados correctamente.", "Actualización", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Error("Error al refrescar datos", ex, "FrmPreparacionLotes");
                MessageBox.Show($"Error al actualizar los datos:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvLotes_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || dgvLotes.SelectedRows.Count == 0)
                    return;

                var row = dgvLotes.SelectedRows[0];
                if (row.Tag is not Lote lote)
                    return;

                // Obtener archivos del lote
                List<LoteArchivo> archivos = _loteRepo.ObtenerArchivosPorLote(lote.CdLote);

                string detalles = $"Lote: {lote.DsNombreLote}\n";
                detalles += $"Estado: {lote.DsEstadoLote}\n";
                detalles += $"Cantidad de archivos: {lote.NuCantidadArchivos}\n";
                detalles += $"Fecha de creación: {lote.FeAlta:yyyy-MM-dd HH:mm:ss}\n\n";
                detalles += "Archivos:\n";
                detalles += "─────────────────────────────────────\n";

                foreach (var archivo in archivos.OrderBy(a => a.NuOrden))
                {
                    detalles += $"{archivo.NuOrden}. {archivo.DsNombreArchivo} ({archivo.NuCantidadPaginas} págs)\n";
                }

                MessageBox.Show(detalles, "Detalle del Lote", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Error("Error al mostrar detalle de lote", ex, "FrmPreparacionLotes");
                MessageBox.Show($"Error al obtener los detalles:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvArchivosDisponibles_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                int seleccionados = 0;
                foreach (DataGridViewRow row in dgvArchivosDisponibles.SelectedRows)
                {
                    if (row.Tag is Archivo)
                        seleccionados++;
                }

                btnCrearLote.Enabled = seleccionados > 0;
                lblSeleccionados.Text = $"Archivos seleccionados: {seleccionados}";
            }
            catch (Exception ex)
            {
                Logger.Error("Error en selección de archivos", ex, "FrmPreparacionLotes");
            }
        }

        private void dgvLotes_SelectionChanged(object sender, EventArgs e)
        {
            btnDividirLote.Enabled = dgvLotes.SelectedRows.Count > 0;
        }
    }
}
