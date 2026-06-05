using IndexadorAutomaticoPlanos.DataAccess;
using IndexadorAutomaticoPlanos.Entities;
using IndexadorAutomaticoPlanos.Utils;
using System.Configuration;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace IndexadorAutomaticoPlanos.UI
{
    /// <summary>
    /// Formulario para preparar imágenes de lotes antes de procesamiento por IA
    /// </summary>
    public partial class FrmPreparacionImagenes : Form
    {
        private readonly LoteRepository _loteRepository;
        private readonly ImagenProcesador _imagenProcesador;
        private List<Lote> _lotesPendientes = new();
        private SixLabors.ImageSharp.Image? _imagenPreview;
        private string? _rutaPdfPreview;
        private bool _procesando = false;
        private CancellationTokenSource? _cts;

        public FrmPreparacionImagenes()
        {
            InitializeComponent();
            _loteRepository = new LoteRepository();
            _imagenProcesador = new ImagenProcesador();
        }

        private void FrmPreparacionImagenes_Load(object sender, EventArgs e)
        {
            try
            {
                ConfigurarControles();
                CargarLotesPendientes();
                Logger.Info("Formulario de preparación de imágenes cargado", "FrmPreparacionImagenes");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cargar formulario de preparación de imágenes", ex, "FrmPreparacionImagenes");
                MessageBox.Show($"Error al cargar el formulario: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarControles()
        {
            // Configurar ComboBox de esquinas
            cboEsquina.Items.Clear();
            cboEsquina.Items.AddRange(new object[]
            {
                "Inferior Derecha",
                "Inferior Izquierda",
                "Superior Derecha",
                "Superior Izquierda"
            });
            cboEsquina.SelectedIndex = 0; // Inferior Derecha por defecto

            // Configurar NumericUpDown DPI
            numDpi.Minimum = 72;
            numDpi.Maximum = 600;
            numDpi.Value = 300;
            numDpi.Increment = 50;

            // Configurar NumericUpDown Porcentaje Vertical
            numPorcentajeVertical.Minimum = 5;
            numPorcentajeVertical.Maximum = 100;
            numPorcentajeVertical.Value = 15;
            numPorcentajeVertical.Increment = 5;
            numPorcentajeVertical.DecimalPlaces = 1;

            // Configurar NumericUpDown Porcentaje Horizontal
            numPorcentajeHorizontal.Minimum = 5;
            numPorcentajeHorizontal.Maximum = 100;
            numPorcentajeHorizontal.Value = 20;
            numPorcentajeHorizontal.Increment = 5;
            numPorcentajeHorizontal.DecimalPlaces = 1;

            // CheckBox OCR
            chkOcr.Checked = false;

            // DataGridView configuración
            dgvLotes.MultiSelect = true;
            dgvLotes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLotes.AllowUserToAddRows = false;
            dgvLotes.AllowUserToDeleteRows = false;
            dgvLotes.ReadOnly = true;
            dgvLotes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // PictureBox
            picPreview.SizeMode = PictureBoxSizeMode.Zoom;
            picPreview.BorderStyle = BorderStyle.FixedSingle;

            // ProgressBar
            progressBar.Visible = false;
            progressBar.Style = ProgressBarStyle.Blocks;

            // Eventos para actualizar preview
            numPorcentajeVertical.ValueChanged += (s, e) => ActualizarPreview();
            numPorcentajeHorizontal.ValueChanged += (s, e) => ActualizarPreview();
            cboEsquina.SelectedIndexChanged += (s, e) => ActualizarPreview();
        }

        private void CargarLotesPendientes()
        {
            try
            {
                _lotesPendientes = _loteRepository.ObtenerLotesPendientesPreparacion();

                dgvLotes.DataSource = null;
                dgvLotes.DataSource = _lotesPendientes.Select(l => new
                {
                    l.CdLote,
                    Lote = l.DsNombreLote,
                    Archivos = l.NuCantidadArchivos,
                    Estado = l.DsEstadoLote,
                    Creado = l.FeAlta.ToString("dd/MM/yyyy HH:mm")
                }).ToList();

                lblEstado.Text = $"{_lotesPendientes.Count} lote(s) pendiente(s) de preparación";

                Logger.Info($"Cargados {_lotesPendientes.Count} lotes pendientes", "FrmPreparacionImagenes");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cargar lotes pendientes", ex, "FrmPreparacionImagenes");
                MessageBox.Show($"Error al cargar lotes: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCargarPdf_Click(object sender, EventArgs e)
        {
            try
            {
                using var openFileDialog = new OpenFileDialog
                {
                    Title = "Seleccionar PDF para previsualización",
                    Filter = "Archivos PDF|*.pdf",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _rutaPdfPreview = openFileDialog.FileName;
                    CargarPrevisualizacion();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cargar PDF para preview", ex, "FrmPreparacionImagenes");
                MessageBox.Show($"Error al cargar PDF: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarPrevisualizacion()
        {
            if (string.IsNullOrEmpty(_rutaPdfPreview) || !File.Exists(_rutaPdfPreview))
                return;

            try
            {
                Cursor = Cursors.WaitCursor;
                btnCargarPdf.Enabled = false;

                // Liberar imagen anterior
                _imagenPreview?.Dispose();

                // Extraer primera página
                int dpi = (int)numDpi.Value;
                _imagenPreview = _imagenProcesador.ExtraerPrimeraPaginaComoImagen(_rutaPdfPreview, dpi);

                // Mostrar en PictureBox
                ActualizarPreview();

                Logger.Info($"Preview cargado desde {Path.GetFileName(_rutaPdfPreview)}", "FrmPreparacionImagenes");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cargar previsualización", ex, "FrmPreparacionImagenes");
                MessageBox.Show($"Error al generar previsualización: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                btnCargarPdf.Enabled = true;
            }
        }

        private void ActualizarPreview()
        {
            if (_imagenPreview == null) return;

            try
            {
                // Convertir ImageSharp Image a System.Drawing.Bitmap para WinForms PictureBox
                using var ms = new MemoryStream();
                _imagenPreview.Save(ms, new JpegEncoder());
                ms.Position = 0;

                var bitmap = new System.Drawing.Bitmap(ms);

                // Dibujar rectángulo de recorte
                using var graphics = System.Drawing.Graphics.FromImage(bitmap);
                var esquina = ObtenerEsquinaSeleccionada();
                var porcentajeHorizontal = numPorcentajeHorizontal.Value;
                var porcentajeVertical = numPorcentajeVertical.Value;

                var rectangulo = _imagenProcesador.CalcularRectanguloRecorte(
                    bitmap.Width, bitmap.Height, esquina, porcentajeHorizontal, porcentajeVertical);

                // Dibujar rectángulo
                using var pen = new System.Drawing.Pen(System.Drawing.Color.Red, 3);
                graphics.DrawRectangle(pen, rectangulo.X, rectangulo.Y, rectangulo.Width, rectangulo.Height);

                // Mostrar en PictureBox
                picPreview.Image?.Dispose();
                picPreview.Image = bitmap;

                lblPreview.Text = $"Preview: {rectangulo.Width}x{rectangulo.Height} px " +
                                 $"(H:{porcentajeHorizontal}% V:{porcentajeVertical}% desde {cboEsquina.Text})";
            }
            catch (Exception ex)
            {
                Logger.Error("Error al actualizar preview", ex, "FrmPreparacionImagenes");
            }
        }

        private EsquinaRecorte ObtenerEsquinaSeleccionada()
        {
            return cboEsquina.SelectedIndex switch
            {
                0 => EsquinaRecorte.InferiorDerecha,
                1 => EsquinaRecorte.InferiorIzquierda,
                2 => EsquinaRecorte.SuperiorDerecha,
                3 => EsquinaRecorte.SuperiorIzquierda,
                _ => EsquinaRecorte.InferiorDerecha
            };
        }

        private async void btnProcesar_Click(object sender, EventArgs e)
        {
            if (_procesando)
            {
                MessageBox.Show("Ya hay un procesamiento en curso.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (dgvLotes.SelectedRows.Count == 0)
            {
                MessageBox.Show("Debe seleccionar al menos un lote para procesar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Confirmar
            int cantidadLotes = dgvLotes.SelectedRows.Count;
            var result = MessageBox.Show(
                $"¿Confirma que desea procesar {cantidadLotes} lote(s)?\n\n" +
                $"Parámetros:\n" +
                $"- DPI: {numDpi.Value}\n" +
                $"- Esquina: {cboEsquina.Text}\n" +
                $"- Recorte Horizontal: {numPorcentajeHorizontal.Value}%\n" +
                $"- Recorte Vertical: {numPorcentajeVertical.Value}%\n" +
                $"- OCR: {(chkOcr.Checked ? "Sí" : "No")}",
                "Confirmar procesamiento",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            await ProcesarLotesSeleccionados();
        }

        private async Task ProcesarLotesSeleccionados()
        {
            _procesando = true;
            _cts = new CancellationTokenSource();

            try
            {
                // Obtener IDs de lotes seleccionados
                var lotesSeleccionados = dgvLotes.SelectedRows.Cast<DataGridViewRow>()
                    .Select(r => Convert.ToInt32(r.Cells["CdLote"].Value))
                    .ToList();

                // Configurar UI
                btnProcesar.Enabled = false;
                btnCargarPdf.Enabled = false;
                dgvLotes.Enabled = false;
                progressBar.Visible = true;
                progressBar.Value = 0;

                // Parámetros de procesamiento
                int dpi = (int)numDpi.Value;
                var esquina = ObtenerEsquinaSeleccionada();
                decimal porcentajeHorizontal = numPorcentajeHorizontal.Value;
                decimal porcentajeVertical = numPorcentajeVertical.Value;
                bool ejecutarOcr = chkOcr.Checked;

                Logger.Info($"Iniciando procesamiento de {lotesSeleccionados.Count} lote(s)", "FrmPreparacionImagenes");

                // Procesar en segundo plano
                var (exitosos, errores, lotesConErrores) = await Task.Run(() => ProcesarLotes(lotesSeleccionados, dpi, esquina, porcentajeHorizontal, porcentajeVertical, ejecutarOcr, _cts.Token));

                // Mostrar resultado
                if (errores == 0)
                {
                    MessageBox.Show($"Procesamiento completado exitosamente.\n\n" +
                        $"Total procesados: {exitosos} archivo(s)",
                        "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    string mensaje = $"Procesamiento completado con ERRORES.\n\n" +
                        $"Archivos exitosos: {exitosos}\n" +
                        $"Archivos con error: {errores}\n\n" +
                        $"Los lotes con errores permanecen pendientes para reintento.\n" +
                        $"Revise los logs para más detalles.";
                    MessageBox.Show(mensaje, "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Recargar grilla
                CargarLotesPendientes();
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Procesamiento cancelado.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Error("Error en procesamiento de lotes", ex, "FrmPreparacionImagenes");
                MessageBox.Show($"Error durante el procesamiento: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _procesando = false;
                btnProcesar.Enabled = true;
                btnCargarPdf.Enabled = true;
                dgvLotes.Enabled = true;
                progressBar.Visible = false;
                _cts?.Dispose();
                _cts = null;
            }
        }

        private (int exitosos, int errores, int lotesConErrores) ProcesarLotes(
            List<int> idsLotes,
            int dpi,
            EsquinaRecorte esquina,
            decimal porcentajeHorizontal,
            decimal porcentajeVertical,
            bool ejecutarOcr,
            CancellationToken cancellationToken)
        {
            int totalProcesados = 0;
            int totalErrores = 0;
            int lotesConErrores = 0;

            foreach (var cdLote in idsLotes)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    var lote = _lotesPendientes.FirstOrDefault(l => l.CdLote == cdLote);
                    if (lote == null) continue;

                    ActualizarEstado($"Procesando lote: {lote.DsNombreLote}...");

                    // Obtener archivos del lote
                    var archivosLote = _loteRepository.ObtenerArchivosPorLote(cdLote);

                    // Reiniciar contadores para este lote
                    int archivosExitosos = 0;
                    int archivosConError = 0;

                    // Procesar archivos con límite de concurrencia
                    var semaphore = new SemaphoreSlim(4); // Máximo 4 PDFs simultáneos
                    var tareas = new List<Task>();

                    foreach (var archivoLote in archivosLote)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        tareas.Add(Task.Run(async () =>
                        {
                            await semaphore.WaitAsync(cancellationToken);
                            try
                            {
                                ProcesarArchivo(archivoLote, lote.DsNombreLote, dpi, esquina, porcentajeHorizontal, porcentajeVertical, ejecutarOcr);
                                Interlocked.Increment(ref archivosExitosos);
                                Interlocked.Increment(ref totalProcesados);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error($"Error al procesar archivo {archivoLote.DsNombreArchivo}", ex, "FrmPreparacionImagenes");
                                Interlocked.Increment(ref archivosConError);
                                Interlocked.Increment(ref totalErrores);
                            }
                            finally
                            {
                                semaphore.Release();
                            }

                            // Actualizar progreso
                            int progreso = (archivosExitosos + archivosConError) * 100 / archivosLote.Count;
                            ActualizarProgreso(progreso);
                        }, cancellationToken));
                    }

                    Task.WaitAll(tareas.ToArray());

                    // Solo actualizar estado si NO hubo errores EN ESTE LOTE
                    if (archivosConError == 0)
                    {
                        // Actualizar estado del lote a "Pendiente de Procesar por IA"
                        int estadoLote = ObtenerIdEstadoLote("Pendiente de Procesar por IA");
                        int estadoArchivo = ObtenerIdEstadoArchivoLote("Imagen Extraída");

                        _loteRepository.ActualizarEstadoLoteYArchivos(
                            cdLote, estadoLote, estadoArchivo, SesionActual.UsuarioActual!.CdUsuario);

                        Logger.Info($"Lote {lote.DsNombreLote} procesado completamente: {archivosExitosos} archivos exitosos", "FrmPreparacionImagenes");
                    }
                    else
                    {
                        Logger.Warning($"Lote {lote.DsNombreLote} tiene {archivosConError} errores de {archivosLote.Count} archivos. Estado NO actualizado.", "FrmPreparacionImagenes");
                        lotesConErrores++;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error al procesar lote {cdLote}", ex, "FrmPreparacionImagenes");
                    totalErrores++;
                    lotesConErrores++;
                }
            }

            ActualizarEstado($"Procesamiento completado: {totalProcesados} exitosos, {totalErrores} errores");
            return (totalProcesados, totalErrores, lotesConErrores);
        }

        private void ProcesarArchivo(
            LoteArchivo archivoLote,
            string nombreLote,
            int dpi,
            EsquinaRecorte esquina,
            decimal porcentajeHorizontal,
            decimal porcentajeVertical,
            bool ejecutarOcr)
        {
            try
            {
                Logger.Info($"Procesando archivo: {archivoLote.DsNombreArchivo}", "FrmPreparacionImagenes");

                // Procesar con ImagenProcesador
                var resultado = _imagenProcesador.ProcesarArchivoPdf(
                    archivoLote.DsRutaCompletaArchivo!,
                    nombreLote,
                    dpi,
                    esquina,
                    porcentajeHorizontal,
                    porcentajeVertical,
                    ejecutarOcr);

                if (resultado.Exitoso)
                {
                    // Guardar en base de datos
                    _loteRepository.ActualizarDatosImagen(
                        archivoLote.CdLoteArchivo,
                        resultado.RutaImagenJpg!,
                        resultado.ImagenBase64!,
                        resultado.TextoOcr,
                        resultado.TieneOcr,
                        dpi,
                        esquina.ToString(),
                        porcentajeHorizontal,
                        porcentajeVertical,
                        SesionActual.UsuarioActual!.CdUsuario);

                    Logger.Info($"Archivo procesado exitosamente: {archivoLote.DsNombreArchivo}", "FrmPreparacionImagenes");
                }
                else
                {
                    throw new Exception(resultado.MensajeError ?? "Error desconocido");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al procesar {archivoLote.DsNombreArchivo}", ex, "FrmPreparacionImagenes");
                throw;
            }
        }

        private int ObtenerIdEstadoLote(string nombreEstado)
        {
            // Estados: 1=Pendiente Preparar Imágenes, 2=Pendiente Procesar IA, etc.
            return nombreEstado switch
            {
                "Pendiente de Procesar por IA" => 2,
                _ => 1
            };
        }

        private int ObtenerIdEstadoArchivoLote(string nombreEstado)
        {
            // Estados archivo lote: 8=Asociado, 9=Imagen Extraída, etc.
            return nombreEstado switch
            {
                "Imagen Extraída" => 9,
                _ => 8
            };
        }

        private void ActualizarEstado(string mensaje)
        {
            if (lblEstado.InvokeRequired)
            {
                lblEstado.Invoke(new Action(() => lblEstado.Text = mensaje));
            }
            else
            {
                lblEstado.Text = mensaje;
            }
        }

        private void ActualizarProgreso(int porcentaje)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new Action(() => progressBar.Value = Math.Min(porcentaje, 100)));
            }
            else
            {
                progressBar.Value = Math.Min(porcentaje, 100);
            }
        }

        private void btnRefrescar_Click(object sender, EventArgs e)
        {
            CargarLotesPendientes();
        }

        private void FrmPreparacionImagenes_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_procesando)
            {
                var result = MessageBox.Show(
                    "Hay un procesamiento en curso. ¿Desea cancelarlo y cerrar?",
                    "Confirmar cierre",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                _cts?.Cancel();
            }

            // Liberar recursos
            _imagenPreview?.Dispose();
            _imagenProcesador?.Dispose();
            picPreview.Image?.Dispose();
        }
    }
}
