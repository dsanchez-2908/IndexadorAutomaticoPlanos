using IndexadorAutomaticoPlanos.DataAccess;
using IndexadorAutomaticoPlanos.Entities;
using IndexadorAutomaticoPlanos.Utils;

namespace IndexadorAutomaticoPlanos.UI
{
    /// <summary>
    /// Formulario para ingreso masivo de planos (archivos PDF)
    /// </summary>
    public partial class FrmIngresoPlanos : Form
    {
        private readonly ArchivoRepository _archivoRepo;
        private List<PdfInfo> _archivosEscaneados;

        public FrmIngresoPlanos()
        {
            InitializeComponent();
            _archivoRepo = new ArchivoRepository();
            _archivosEscaneados = new List<PdfInfo>();
        }

        private void FrmIngresoPlanos_Load(object sender, EventArgs e)
        {
            try
            {
                ConfigurarDataGridView();
                ActualizarEstadisticas();
                Logger.Info("Formulario de ingreso de planos cargado", "FrmIngresoPlanos");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cargar formulario de ingreso de planos", ex, "FrmIngresoPlanos");
                MessageBox.Show($"Error al cargar el formulario:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarDataGridView()
        {
            dgvArchivos.AutoGenerateColumns = false;
            dgvArchivos.AllowUserToAddRows = false;
            dgvArchivos.AllowUserToDeleteRows = false;
            dgvArchivos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvArchivos.MultiSelect = true;
            dgvArchivos.ReadOnly = false;
            colSeleccionar.ReadOnly = false;
        }

        private void ActualizarEstadisticas()
        {
            int total = dgvArchivos.Rows.Count;
            int seleccionados = 0;
            foreach (DataGridViewRow row in dgvArchivos.Rows)
            {
                if (row.Cells["colSeleccionar"].Value != null && (bool)row.Cells["colSeleccionar"].Value)
                    seleccionados++;
            }
            lblEstadisticas.Text = $"Total: {total} | Seleccionados: {seleccionados}";
        }

        private void btnSeleccionarCarpeta_Click(object sender, EventArgs e)
        {
            try
            {
                using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
                {
                    folderDialog.Description = "Seleccione la carpeta que contiene los archivos PDF";
                    folderDialog.ShowNewFolderButton = false;
                    if (folderDialog.ShowDialog() == DialogResult.OK)
                        txtRutaCarpeta.Text = folderDialog.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error al seleccionar carpeta", ex, "FrmIngresoPlanos");
            }
        }

        private async void btnEscanear_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRutaCarpeta.Text))
            {
                MessageBox.Show("Debe seleccionar una carpeta para escanear.", "Carpeta Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!Directory.Exists(txtRutaCarpeta.Text))
            {
                MessageBox.Show("La carpeta seleccionada no existe.", "Carpeta No Encontrada", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                DeshabilitarControles(true);
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Marquee;
                lblProgreso.Text = "Escaneando carpeta...";
                Logger.Info($"Iniciando escaneo de carpeta: {txtRutaCarpeta.Text}", "FrmIngresoPlanos");
                await Task.Run(() => EscanearCarpeta(txtRutaCarpeta.Text, chkIncluirSubcarpetas.Checked));
                CargarArchivosEnGrilla();
                Logger.Info($"Escaneo completado. Archivos encontrados: {_archivosEscaneados.Count}", "FrmIngresoPlanos");
                if (_archivosEscaneados.Count == 0)
                    MessageBox.Show("No se encontraron archivos PDF en la carpeta seleccionada.", "Sin Resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show($"Se encontraron {_archivosEscaneados.Count} archivo(s) PDF.", "Escaneo Completado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Error("Error al escanear carpeta", ex, "FrmIngresoPlanos");
                MessageBox.Show($"Error al escanear la carpeta:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressBar.Visible = false;
                progressBar.Style = ProgressBarStyle.Blocks;
                lblProgreso.Text = "";
                DeshabilitarControles(false);
            }
        }

        private void EscanearCarpeta(string rutaCarpeta, bool incluirSubcarpetas)
        {
            _archivosEscaneados.Clear();
            SearchOption searchOption = incluirSubcarpetas ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            string[] archivosPdf = Directory.GetFiles(rutaCarpeta, "*.pdf", searchOption);
            foreach (string rutaArchivo in archivosPdf)
            {
                try
                {
                    if (!PdfHelper.EsPdfValido(rutaArchivo))
                    {
                        Logger.Warning($"Archivo inválido omitido: {rutaArchivo}", "FrmIngresoPlanos");
                        continue;
                    }
                    string nombreArchivo = Path.GetFileName(rutaArchivo);
                    string directorioPadre = Path.GetDirectoryName(rutaArchivo) ?? "";
                    if (_archivoRepo.ExisteArchivo(directorioPadre, nombreArchivo))
                    {
                        Logger.Info($"Archivo duplicado omitido: {rutaArchivo}", "FrmIngresoPlanos");
                        PdfInfo infoDuplicado = PdfHelper.ObtenerInformacion(rutaArchivo);
                        infoDuplicado.EsDuplicado = true;
                        _archivosEscaneados.Add(infoDuplicado);
                        continue;
                    }
                    PdfInfo info = PdfHelper.ObtenerInformacion(rutaArchivo);
                    _archivosEscaneados.Add(info);
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Error al procesar archivo {rutaArchivo}: {ex.Message}", "FrmIngresoPlanos");
                }
            }
        }

        private void CargarArchivosEnGrilla()
        {
            if (dgvArchivos.InvokeRequired)
            {
                dgvArchivos.Invoke(new Action(CargarArchivosEnGrilla));
                return;
            }
            dgvArchivos.Rows.Clear();
            foreach (PdfInfo info in _archivosEscaneados)
            {
                string estado = info.EsDuplicado ? "⚠️ Duplicado" : info.EsValido ? "✅ Válido" : "❌ Inválido";
                int rowIndex = dgvArchivos.Rows.Add();
                DataGridViewRow row = dgvArchivos.Rows[rowIndex];
                row.Cells["colSeleccionar"].Value = !info.EsDuplicado && info.EsValido;
                row.Cells["colNombre"].Value = info.NombreArchivo;
                row.Cells["colRuta"].Value = info.DirectorioPadre;
                row.Cells["colTamano"].Value = PdfHelper.FormatearTamano(info.TamanoBytes);
                row.Cells["colFechaModificacion"].Value = info.FechaModificacion.ToString("yyyy-MM-dd HH:mm");
                row.Cells["colEstado"].Value = estado;
                if (info.EsDuplicado)
                    row.DefaultCellStyle.BackColor = System.Drawing.Color.LightYellow;
                else if (!info.EsValido)
                    row.DefaultCellStyle.BackColor = System.Drawing.Color.LightCoral;
            }
            ActualizarEstadisticas();
        }

        private void DeshabilitarControles(bool deshabilitar)
        {
            btnSeleccionarCarpeta.Enabled = !deshabilitar;
            btnEscanear.Enabled = !deshabilitar;
            chkIncluirSubcarpetas.Enabled = !deshabilitar;
            btnIndexar.Enabled = !deshabilitar;
            btnLimpiar.Enabled = !deshabilitar;
            btnVerDetalles.Enabled = !deshabilitar;
            btnEliminar.Enabled = !deshabilitar;
        }

        private async void btnIndexar_Click(object sender, EventArgs e)
        {
            List<DataGridViewRow> filasSeleccionadas = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in dgvArchivos.Rows)
            {
                if (row.Cells["colSeleccionar"].Value != null && (bool)row.Cells["colSeleccionar"].Value)
                    filasSeleccionadas.Add(row);
            }
            if (filasSeleccionadas.Count == 0)
            {
                MessageBox.Show("No hay archivos seleccionados para ingresar.\n\nMarque los archivos que desea ingresar usando la columna de selección.", "Sin Archivos Seleccionados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DialogResult confirmacion = MessageBox.Show($"¿Desea ingresar {filasSeleccionadas.Count} archivo(s) seleccionado(s) a la base de datos?\n\nLos archivos serán registrados con estado 'Ingresado'.", "Confirmar Ingreso de Archivos", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmacion != DialogResult.Yes)
                return;
            try
            {
                DeshabilitarControles(true);
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Maximum = filasSeleccionadas.Count;
                progressBar.Value = 0;
                int exitosos = 0, duplicados = 0, errores = 0;
                int estadoIngresado = 1;
                foreach (DataGridViewRow row in filasSeleccionadas)
                {
                    try
                    {
                        string nombreArchivo = row.Cells["colNombre"].Value?.ToString() ?? "";
                        string rutaCompleta = row.Cells["colRuta"].Value?.ToString() ?? "";
                        PdfInfo? info = _archivosEscaneados.FirstOrDefault(a => a.NombreArchivo == nombreArchivo && a.DirectorioPadre == rutaCompleta);
                        if (info == null)
                        {
                            errores++;
                            continue;
                        }
                        if (info.EsDuplicado)
                        {
                            duplicados++;
                            progressBar.Value++;
                            lblProgreso.Text = $"Ingresando: {info.NombreArchivo}... ({progressBar.Value}/{progressBar.Maximum})";
                            Application.DoEvents();
                            continue;
                        }
                        Archivo archivo = new Archivo
                        {
                            DsNombreArchivo = info.NombreArchivo,
                            DsRutaCompleta = info.DirectorioPadre,
                            DsNombreUltimaCarpeta = Path.GetFileName(info.DirectorioPadre),
                            NuTamanoBytes = info.TamanoBytes,
                            NuCantidadPaginas = info.NumeroPaginas,
                            FeModificacionArchivo = info.FechaModificacion,
                            CdEstadoArchivo = estadoIngresado,
                            CdUsuarioAlta = SesionActual.UsuarioActual?.CdUsuario ?? 1
                        };
                        int cdArchivo = _archivoRepo.Insertar(archivo);
                        if (cdArchivo > 0)
                        {
                            exitosos++;
                            row.Cells["colEstado"].Value = "✅ Ingresado";
                            row.DefaultCellStyle.BackColor = System.Drawing.Color.LightGreen;
                            Logger.Info($"Archivo ingresado: {nombreArchivo} (ID: {cdArchivo})", "FrmIngresoPlanos");
                        }
                        else
                        {
                            errores++;
                            row.Cells["colEstado"].Value = "❌ Error";
                            Logger.Warning($"No se pudo ingresar archivo: {nombreArchivo}", "FrmIngresoPlanos");
                        }
                    }
                    catch (Exception exArchivo)
                    {
                        errores++;
                        string nombre = row.Cells["colNombre"].Value?.ToString() ?? "desconocido";
                        Logger.Error($"Error al ingresar archivo {nombre}", exArchivo, "FrmIngresoPlanos");
                    }
                    progressBar.Value++;
                    lblProgreso.Text = $"Procesando... ({progressBar.Value}/{progressBar.Maximum})";
                    Application.DoEvents();
                }
                string mensaje = $"Proceso completado:\n\n✅ Ingresados exitosamente: {exitosos}\n⚠️ Duplicados omitidos: {duplicados}\n❌ Errores: {errores}";
                MessageBox.Show(mensaje, "Ingreso Completado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Logger.Info($"Ingreso completado: {exitosos} exitosos, {duplicados} duplicados, {errores} errores", "FrmIngresoPlanos");
            }
            catch (Exception ex)
            {
                Logger.Error("Error durante el proceso de ingreso", ex, "FrmIngresoPlanos");
                MessageBox.Show($"Error durante el ingreso:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressBar.Visible = false;
                progressBar.Value = 0;
                lblProgreso.Text = "";
                DeshabilitarControles(false);
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            if (dgvArchivos.Rows.Count == 0)
            {
                MessageBox.Show("No hay archivos en la lista para limpiar.", "Lista Vacía", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DialogResult confirmacion = MessageBox.Show("¿Está seguro que desea limpiar toda la lista de archivos?\n\nEsta acción no afecta la base de datos.", "Confirmar Limpieza", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmacion == DialogResult.Yes)
            {
                dgvArchivos.Rows.Clear();
                _archivosEscaneados.Clear();
                ActualizarEstadisticas();
                Logger.Info("Lista de archivos limpiada", "FrmIngresoPlanos");
            }
        }

        private void btnVerDetalles_Click(object sender, EventArgs e)
        {
            if (dgvArchivos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Debe seleccionar un archivo para ver sus detalles.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DataGridViewRow row = dgvArchivos.SelectedRows[0];
            string nombreArchivo = row.Cells["colNombre"].Value?.ToString() ?? "";
            string rutaCompleta = row.Cells["colRuta"].Value?.ToString() ?? "";
            PdfInfo? info = _archivosEscaneados.FirstOrDefault(a => a.NombreArchivo == nombreArchivo && a.DirectorioPadre == rutaCompleta);
            if (info == null)
            {
                MessageBox.Show("No se encontró la información del archivo seleccionado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string detalles = $"Archivo: {info.NombreArchivo}\n\nRuta Completa: {info.RutaCompleta}\nDirectorio: {info.DirectorioPadre}\nÚltima Carpeta: {Path.GetFileName(info.DirectorioPadre)}\n\nTamaño: {PdfHelper.FormatearTamano(info.TamanoBytes)} ({info.TamanoBytes:N0} bytes)\nPáginas: {info.NumeroPaginas}\nFecha Modificación: {info.FechaModificacion:yyyy-MM-dd HH:mm:ss}\n\nEstado:\n  • Válido: {(info.EsValido ? "Sí" : "No")}\n  • Duplicado: {(info.EsDuplicado ? "Sí" : "No")}\n  • Tiene Permiso Lectura: {(PdfHelper.TienePermisoLectura(info.RutaCompleta) ? "Sí" : "No")}";
            MessageBox.Show(detalles, "Detalles del Archivo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvArchivos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Debe seleccionar uno o más archivos para eliminar de la lista.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DialogResult confirmacion = MessageBox.Show($"¿Está seguro que desea eliminar {dgvArchivos.SelectedRows.Count} archivo(s) de la lista?\n\nEsta acción no afecta la base de datos.", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmacion != DialogResult.Yes)
                return;
            try
            {
                List<DataGridViewRow> filasAEliminar = new List<DataGridViewRow>();
                foreach (DataGridViewRow row in dgvArchivos.SelectedRows)
                    filasAEliminar.Add(row);
                foreach (DataGridViewRow row in filasAEliminar)
                {
                    string nombreArchivo = row.Cells["colNombre"].Value?.ToString() ?? "";
                    string rutaCompleta = row.Cells["colRuta"].Value?.ToString() ?? "";
                    PdfInfo? info = _archivosEscaneados.FirstOrDefault(a => a.NombreArchivo == nombreArchivo && a.DirectorioPadre == rutaCompleta);
                    if (info != null)
                        _archivosEscaneados.Remove(info);
                    dgvArchivos.Rows.Remove(row);
                }
                ActualizarEstadisticas();
                Logger.Info($"{filasAEliminar.Count} archivo(s) eliminado(s) de la lista", "FrmIngresoPlanos");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al eliminar archivos de la lista", ex, "FrmIngresoPlanos");
                MessageBox.Show($"Error al eliminar archivos:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvArchivos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvArchivos.Columns[e.ColumnIndex].Name == "colSeleccionar")
            {
                dgvArchivos.CommitEdit(DataGridViewDataErrorContexts.Commit);
                ActualizarEstadisticas();
            }
        }
    }
}
