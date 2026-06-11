using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using IndexadorAutomaticoPlanos.DataAccess;
using IndexadorAutomaticoPlanos.Entities;
using IndexadorAutomaticoPlanos.Security;
using IndexadorAutomaticoPlanos.Utils;

namespace IndexadorAutomaticoPlanos.UI
{
    /// <summary>
    /// Formulario modal para control de calidad de un lote específico
    /// FASE 6: Control detallado con edición, navegación y visor de imágenes
    /// </summary>
    public partial class FrmControlLote : Form
    {
        private readonly Lote _lote;
        private readonly LoteRepository _loteRepo;
        private List<ArchivoConResultadoIA> _archivos;
        private List<ArchivoConResultadoIA> _archivosFiltrados;
        private int _indiceActual = -1;
        private bool _cambiosPendientes = false;
        private bool _cargandoDatos = false;
        private float _zoomLevel = 1.0f;
        private List<TipoPlano> _tiposPlano;
        private string _pathRepositorio;

        // Variables para drag/pan de imagen
        private Image _imagenOriginal;
        private bool _isDragging = false;
        private Point _dragStartPoint;

        public FrmControlLote(Lote lote)
        {
            InitializeComponent();
            _lote = lote;
            _loteRepo = new LoteRepository();
            _archivos = new List<ArchivoConResultadoIA>();
            _archivosFiltrados = new List<ArchivoConResultadoIA>();
            _tiposPlano = new List<TipoPlano>();

            // Obtener ruta del repositorio
            _pathRepositorio = ConfigurationManager.AppSettings["PATH_REPOSITORIO"] ?? string.Empty;
        }

        private void FrmControlLote_Load(object sender, EventArgs e)
        {
            try
            {
                _cargandoDatos = true; // Deshabilitar eventos durante carga

                ConfigurarControles();
                CargarTiposPlano();
                CargarTiposPlanoFiltro();
                CargarArchivosLote();
                ActualizarEstadisticasLote();

                _cargandoDatos = false; // Habilitar eventos después de carga

                if (_archivos.Count > 0)
                {
                    SeleccionarArchivo(0);
                }
            }
            catch (Exception ex)
            {
                _cargandoDatos = false;
                Logger.Error($"Error al cargar control de lote {_lote.DsNombreLote}", ex, "FrmControlLote");
                MessageBox.Show($"Error al cargar el lote: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarControles()
        {
            // Título con nombre del lote
            lblTituloLote.Text = $"Control de Calidad - {_lote.DsNombreLote}";

            // Configurar grid
            dgvArchivos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvArchivos.MultiSelect = false;
            dgvArchivos.ReadOnly = true;
            dgvArchivos.AllowUserToAddRows = false;
            dgvArchivos.SelectionChanged += dgvArchivos_SelectionChanged;

            // Configurar visor de imagen
            picVisor.SizeMode = PictureBoxSizeMode.Zoom; // Inicial en Zoom para ajustar
            picVisor.Dock = DockStyle.Fill; // Ocupar todo el panel
            picVisor.MouseWheel += picVisor_MouseWheel;
            picVisor.Cursor = Cursors.Default;

            // Configurar trackbar de zoom
            trackZoom.Minimum = 50;
            trackZoom.Maximum = 400;
            trackZoom.Value = 100;
            trackZoom.TickFrequency = 25;
            trackZoom.LargeChange = 50;
            trackZoom.SmallChange = 25;

            // Configurar navegación con teclado
            this.KeyPreview = true;
            this.KeyDown += FrmControlLote_KeyDown;

            // Estado inicial de botones
            ActualizarEstadoBotones();
        }

        private void CargarTiposPlano()
        {
            try
            {
                _tiposPlano = _loteRepo.ObtenerTiposPlano();

                cmbTipoPlano.DataSource = _tiposPlano;
                cmbTipoPlano.DisplayMember = "DsTipoPlano";
                cmbTipoPlano.ValueMember = "CdTipoPlano";
                cmbTipoPlano.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cargar tipos de plano", ex, "FrmControlLote");
            }
        }

        private void CargarTiposPlanoFiltro()
        {
            try
            {
                // Cargar combo de filtros con opción "(Todos)"
                cmbFiltroTipoPlano.Items.Clear();
                cmbFiltroTipoPlano.Items.Add("(Todos)");

                foreach (var tipo in _tiposPlano)
                {
                    cmbFiltroTipoPlano.Items.Add(tipo.DsTipoPlano);
                }

                cmbFiltroTipoPlano.SelectedIndex = 0; // Seleccionar "(Todos)"
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cargar tipos de plano en filtro", ex, "FrmControlLote");
            }
        }

        private void CargarArchivosLote()
        {
            try
            {
                _archivos = _loteRepo.ObtenerArchivosConResultadosIA(_lote.CdLote);
                AplicarFiltros();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al cargar archivos del lote {_lote.CdLote}", ex, "FrmControlLote");
                throw;
            }
        }

        private void AplicarFiltros()
        {
            try
            {
                _archivosFiltrados = new List<ArchivoConResultadoIA>(_archivos);

                // Filtro por tipo de plano
                if (cmbFiltroTipoPlano.SelectedIndex > 0)
                {
                    string tipoSeleccionado = cmbFiltroTipoPlano.Text;
                    _archivosFiltrados = _archivosFiltrados
                        .Where(a => a.DsTipoPlano == tipoSeleccionado)
                        .ToList();
                }

                // Filtro por campos faltantes
                if (chkFiltroFaltantes.Checked)
                {
                    _archivosFiltrados = _archivosFiltrados
                        .Where(a => a.TieneCamposFaltantes())
                        .ToList();
                }

                // Filtro por porcentaje bajo
                if (numFiltroPorcentaje.Value > 0)
                {
                    decimal umbral = numFiltroPorcentaje.Value;
                    _archivosFiltrados = _archivosFiltrados
                        .Where(a =>
                        {
                            var menorConfianza = a.ObtenerMenorConfianza();
                            return menorConfianza.HasValue && menorConfianza.Value < umbral;
                        })
                        .ToList();
                }

                // Filtro por estado
                if (cmbFiltroEstado.SelectedIndex > 0)
                {
                    string estadoSeleccionado = cmbFiltroEstado.Text;
                    _archivosFiltrados = _archivosFiltrados
                        .Where(a => a.DsEstadoArchivoLote == estadoSeleccionado)
                        .ToList();
                }

                CargarGrillaArchivos();
            }
            catch (Exception ex)
            {
                Logger.Error("Error al aplicar filtros", ex, "FrmControlLote");
            }
        }

        private void CargarGrillaArchivos()
        {
            try
            {
                // Deshabilitar evento SelectionChanged temporalmente
                dgvArchivos.SelectionChanged -= dgvArchivos_SelectionChanged;

                DataTable dt = new DataTable();
                dt.Columns.Add("CdLoteArchivo", typeof(int));
                dt.Columns.Add("Archivo", typeof(string));
                dt.Columns.Add("Estado", typeof(string));
                dt.Columns.Add("% Tipo", typeof(string));
                dt.Columns.Add("% Exp", typeof(string));
                dt.Columns.Add("% Sec", typeof(string));
                dt.Columns.Add("% Manz", typeof(string));
                dt.Columns.Add("% Parc", typeof(string));
                dt.Columns.Add("Tipo Plano", typeof(string));
                dt.Columns.Add("Expediente", typeof(string));
                dt.Columns.Add("Sección", typeof(string));
                dt.Columns.Add("Manzana", typeof(string));
                dt.Columns.Add("Parcela", typeof(string));
                dt.Columns.Add("Modalidad", typeof(string));

                foreach (var archivo in _archivosFiltrados)
                {
                    dt.Rows.Add(
                        archivo.CdLoteArchivo,
                        archivo.DsNombreArchivo,
                        archivo.DsEstadoArchivoLote,
                        FormatearPorcentaje(archivo.NuConfianzaTipoPlano),
                        FormatearPorcentaje(archivo.NuConfianzaExpediente),
                        FormatearPorcentaje(archivo.NuConfianzaSeccion),
                        FormatearPorcentaje(archivo.NuConfianzaManzana),
                        FormatearPorcentaje(archivo.NuConfianzaParcela),
                        archivo.DsTipoPlano,
                        archivo.DsExpediente,
                        archivo.DsSeccion,
                        archivo.DsManzana,
                        archivo.DsParcela,
                        archivo.DsModalidadProcesamiento
                    );
                }

                dgvArchivos.DataSource = dt;

                // Ocultar columna ID
                if (dgvArchivos.Columns["CdLoteArchivo"] != null)
                {
                    dgvArchivos.Columns["CdLoteArchivo"].Visible = false;
                }

                // Colorear filas según estado y confianza
                ColorearFilas();

                lblInfoFiltros.Text = $"Mostrando {_archivosFiltrados.Count} de {_archivos.Count} archivos";

                // Rehabilitar evento SelectionChanged
                dgvArchivos.SelectionChanged += dgvArchivos_SelectionChanged;
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cargar grilla de archivos", ex, "FrmControlLote");
                // Asegurar que el evento se rehabilite incluso en caso de error
                dgvArchivos.SelectionChanged += dgvArchivos_SelectionChanged;
            }
        }

        private string FormatearPorcentaje(decimal? porcentaje)
        {
            if (!porcentaje.HasValue) return "-";
            return $"{porcentaje.Value:F0}%";
        }

        private void ColorearFilas()
        {
            foreach (DataGridViewRow row in dgvArchivos.Rows)
            {
                string estado = row.Cells["Estado"].Value?.ToString() ?? "";

                if (estado == "Controlado")
                {
                    row.DefaultCellStyle.BackColor = Color.LightGreen;
                }
                else if (estado == "Carátula Ilegible")
                {
                    row.DefaultCellStyle.BackColor = Color.LightCoral;
                }
                else
                {
                    // Verificar confianzas bajas
                    bool tieneConfianzaBaja = false;
                    for (int i = 3; i <= 7; i++) // Columnas de porcentajes
                    {
                        string valor = row.Cells[i].Value?.ToString() ?? "";
                        if (valor != "-" && int.TryParse(valor.Replace("%", ""), out int porcentaje))
                        {
                            if (porcentaje < 70)
                            {
                                row.Cells[i].Style.BackColor = Color.LightYellow;
                                tieneConfianzaBaja = true;
                            }
                        }
                    }
                }
            }
        }

        private void dgvArchivos_SelectionChanged(object sender, EventArgs e)
        {
            // Ignorar durante carga inicial
            if (_cargandoDatos) return;

            if (dgvArchivos.CurrentRow != null && dgvArchivos.CurrentRow.Cells["CdLoteArchivo"].Value != null)
            {
                int cdLoteArchivo = Convert.ToInt32(dgvArchivos.CurrentRow.Cells["CdLoteArchivo"].Value);
                int indice = _archivosFiltrados.FindIndex(a => a.CdLoteArchivo == cdLoteArchivo);
                if (indice >= 0 && indice != _indiceActual)
                {
                    SeleccionarArchivo(indice);
                }
            }
        }

        private void SeleccionarArchivo(int indice)
        {
            if (indice < 0 || indice >= _archivosFiltrados.Count) return;

            // Guardar cambios pendientes antes de cambiar (solo si NO estamos cargando)
            if (_cambiosPendientes && !_cargandoDatos)
            {
                var result = MessageBox.Show("Hay cambios sin guardar. ¿Desea guardarlos?", "Cambios Pendientes",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Cancel) return;
                if (result == DialogResult.Yes) GuardarCambiosArchivo();
            }

            // Deshabilitar eventos temporalmente durante asignación de valores
            bool estadoOriginalCarga = _cargandoDatos;
            _cargandoDatos = true;

            _indiceActual = indice;
            var archivo = _archivosFiltrados[indice];

            // Cargar datos en panel de edición
            lblNombreArchivo.Text = $"Archivo: {archivo.DsNombreArchivo}";
            lblEstadoArchivo.Text = $"Estado: {archivo.DsEstadoArchivoLote}";
            lblModalidad.Text = $"Modalidad: {archivo.DsModalidadProcesamiento ?? "N/A"}";

            // Cargar porcentajes
            lblConfianzaTipo.Text = $"Tipo: {FormatearPorcentaje(archivo.NuConfianzaTipoPlano)}";
            lblConfianzaExp.Text = $"Exp: {FormatearPorcentaje(archivo.NuConfianzaExpediente)}";
            lblConfianzaSec.Text = $"Sec: {FormatearPorcentaje(archivo.NuConfianzaSeccion)}";
            lblConfianzaManz.Text = $"Manz: {FormatearPorcentaje(archivo.NuConfianzaManzana)}";
            lblConfianzaParc.Text = $"Parc: {FormatearPorcentaje(archivo.NuConfianzaParcela)}";
            lblConfianzaDir.Text = $"Dir: {FormatearPorcentaje(archivo.NuConfianzaDireccion)}";

            // Cargar valores editables
            if (!string.IsNullOrEmpty(archivo.DsTipoPlano))
            {
                var tipo = _tiposPlano.FirstOrDefault(t => t.DsTipoPlano == archivo.DsTipoPlano);
                if (tipo != null)
                {
                    cmbTipoPlano.SelectedValue = tipo.CdTipoPlano;
                }
                else
                {
                    cmbTipoPlano.SelectedIndex = -1;
                }
            }
            else
            {
                cmbTipoPlano.SelectedIndex = -1;
            }

            txtExpediente.Text = archivo.DsExpediente ?? "";
            txtSeccion.Text = archivo.DsSeccion ?? "";
            txtManzana.Text = archivo.DsManzana ?? "";
            txtParcela.Text = archivo.DsParcela ?? "";
            txtDireccion.Text = archivo.DsDireccion ?? "";

            // Marcar campos obligatorios faltantes
            MarcarCamposFaltantes(archivo);

            // Cargar imagen
            CargarImagenArchivo(archivo);

            // Actualizar navegación
            lblNavegacion.Text = $"Archivo {indice + 1} de {_archivosFiltrados.Count}";
            ActualizarEstadoBotones();

            // Restaurar estado original y resetear flag de cambios
            _cargandoDatos = estadoOriginalCarga;
            _cambiosPendientes = false;
        }

        private void MarcarCamposFaltantes(ArchivoConResultadoIA archivo)
        {
            lblTipoPlano.ForeColor = string.IsNullOrWhiteSpace(archivo.DsTipoPlano) ? Color.Red : Color.Black;
            lblSeccion.ForeColor = string.IsNullOrWhiteSpace(archivo.DsSeccion) ? Color.Red : Color.Black;
            lblManzana.ForeColor = string.IsNullOrWhiteSpace(archivo.DsManzana) ? Color.Red : Color.Black;
            lblParcela.ForeColor = string.IsNullOrWhiteSpace(archivo.DsParcela) ? Color.Red : Color.Black;
        }

        private void CargarImagenArchivo(ArchivoConResultadoIA archivo)
        {
            try
            {
                Logger.Info($"CargarImagenArchivo: PATH_REPOSITORIO='{_pathRepositorio}', DsRutaImagenJpg='{archivo.DsRutaImagenJpg}'", "FrmControlLote");

                if (string.IsNullOrEmpty(archivo.DsRutaImagenJpg))
                {
                    picVisor.Image = null;
                    lblInfoImagen.Text = "Sin imagen disponible";
                    Logger.Warning($"Archivo {archivo.DsNombreArchivo} no tiene ruta de imagen configurada", "FrmControlLote");
                    return;
                }

                string rutaCompleta = Path.Combine(_pathRepositorio, archivo.DsRutaImagenJpg);
                Logger.Info($"Ruta completa de imagen: {rutaCompleta}", "FrmControlLote");

                if (File.Exists(rutaCompleta))
                {
                    // Liberar imagen anterior
                    if (picVisor.Image != null)
                    {
                        picVisor.Image.Dispose();
                    }

                    // Cargar nueva imagen
                    using (var fs = new FileStream(rutaCompleta, FileMode.Open, FileAccess.Read))
                    {
                        _imagenOriginal = Image.FromStream(fs);
                    }

                    // Aplicar el zoom actual (mantener el nivel de zoom del documento anterior)
                    AplicarZoom();

                    lblInfoImagen.Text = $"Imagen cargada: {archivo.DsRutaImagenJpg}";
                    Logger.Info($"Imagen cargada exitosamente: {rutaCompleta}", "FrmControlLote");
                }
                else
                {
                    picVisor.Image = null;
                    lblInfoImagen.Text = $"Imagen no encontrada: {rutaCompleta}";
                    Logger.Warning($"Imagen no encontrada: {rutaCompleta}", "FrmControlLote");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al cargar imagen del archivo {archivo.DsNombreArchivo}", ex, "FrmControlLote");
                picVisor.Image = null;
                lblInfoImagen.Text = $"Error al cargar imagen: {ex.Message}";
            }
        }

        // Eventos de cambio en campos editables
        private void cmbTipoPlano_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_cargandoDatos) _cambiosPendientes = true;
        }

        private void txtExpediente_TextChanged(object sender, EventArgs e)
        {
            if (!_cargandoDatos) _cambiosPendientes = true;
        }

        private void txtSeccion_TextChanged(object sender, EventArgs e)
        {
            if (!_cargandoDatos) _cambiosPendientes = true;
        }

        private void txtManzana_TextChanged(object sender, EventArgs e)
        {
            if (!_cargandoDatos) _cambiosPendientes = true;
        }

        private void txtParcela_TextChanged(object sender, EventArgs e)
        {
            if (!_cargandoDatos) _cambiosPendientes = true;
        }

        private void btnGuardarCambios_Click(object sender, EventArgs e)
        {
            GuardarCambiosArchivo();
        }

        private void GuardarCambiosArchivo()
        {
            try
            {
                if (_indiceActual < 0 || _indiceActual >= _archivosFiltrados.Count) return;

                var archivo = _archivosFiltrados[_indiceActual];

                if (!archivo.CdResultadoIA.HasValue)
                {
                    MessageBox.Show("Este archivo no tiene resultado de IA para actualizar", "Advertencia",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validar campos obligatorios
                if (cmbTipoPlano.SelectedIndex < 0 || string.IsNullOrWhiteSpace(txtSeccion.Text) ||
                    string.IsNullOrWhiteSpace(txtManzana.Text) || string.IsNullOrWhiteSpace(txtParcela.Text))
                {
                    MessageBox.Show("Debe completar los campos obligatorios: Tipo de Plano, Sección, Manzana y Parcela",
                        "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Crear objeto con cambios
                var resultadoActualizado = new ResultadoIA
                {
                    CdResultadoIA = archivo.CdResultadoIA.Value,
                    DsTipoPlano = cmbTipoPlano.Text,
                    DsExpediente = string.IsNullOrWhiteSpace(txtExpediente.Text) ? null : txtExpediente.Text,
                    DsSeccion = txtSeccion.Text,
                    DsManzana = txtManzana.Text,
                    DsParcela = txtParcela.Text,
                    DsDireccion = string.IsNullOrWhiteSpace(txtDireccion.Text) ? null : txtDireccion.Text,
                    CdUsuarioModificacion = SesionActual.UsuarioActual.CdUsuario
                };

                _loteRepo.ActualizarResultadoIA(resultadoActualizado);

                // Actualizar objeto en memoria
                archivo.DsTipoPlano = resultadoActualizado.DsTipoPlano;
                archivo.DsExpediente = resultadoActualizado.DsExpediente;
                archivo.DsSeccion = resultadoActualizado.DsSeccion;
                archivo.DsManzana = resultadoActualizado.DsManzana;
                archivo.DsParcela = resultadoActualizado.DsParcela;
                archivo.DsDireccion = resultadoActualizado.DsDireccion;

                _cambiosPendientes = false;

                // Refrescar grilla
                CargarGrillaArchivos();

                MessageBox.Show("Cambios guardados exitosamente", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                Logger.Info($"Resultado IA actualizado para archivo {archivo.DsNombreArchivo}", "FrmControlLote");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al guardar cambios del archivo", ex, "FrmControlLote");
                MessageBox.Show($"Error al guardar cambios: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Navegación
        private void btnAnterior_Click(object sender, EventArgs e)
        {
            if (_indiceActual > 0)
            {
                SeleccionarArchivo(_indiceActual - 1);
            }
        }

        private void btnSiguiente_Click(object sender, EventArgs e)
        {
            if (_indiceActual < _archivosFiltrados.Count - 1)
            {
                SeleccionarArchivo(_indiceActual + 1);
            }
        }

        private void FrmControlLote_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
            {
                btnAnterior_Click(sender, e);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Right)
            {
                btnSiguiente_Click(sender, e);
                e.Handled = true;
            }
        }

        private void ActualizarEstadoBotones()
        {
            btnAnterior.Enabled = _indiceActual > 0;
            btnSiguiente.Enabled = _indiceActual < _archivosFiltrados.Count - 1;
        }

        // Zoom
        private void trackZoom_Scroll(object sender, EventArgs e)
        {
            _zoomLevel = trackZoom.Value / 100f;
            lblZoom.Text = $"{trackZoom.Value}%";
            AplicarZoom();
        }

        private void picVisor_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0 && trackZoom.Value < trackZoom.Maximum)
            {
                trackZoom.Value = Math.Min(trackZoom.Value + 25, trackZoom.Maximum);
            }
            else if (e.Delta < 0 && trackZoom.Value > trackZoom.Minimum)
            {
                trackZoom.Value = Math.Max(trackZoom.Value - 25, trackZoom.Minimum);
            }
            trackZoom_Scroll(sender, e);
        }

        // Drag/Pan de imagen
        private void picVisor_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _zoomLevel > 1.0f)
            {
                _isDragging = true;
                // Guardar posición inicial del mouse en coordenadas de pantalla
                _dragStartPoint = e.Location;
                picVisor.Cursor = Cursors.SizeAll;
            }
        }

        private void picVisor_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                // Calcular cuánto se movió el mouse desde el inicio
                int deltaX = e.X - _dragStartPoint.X;
                int deltaY = e.Y - _dragStartPoint.Y;

                // Obtener la posición actual del scroll (valores negativos)
                int scrollX = pnlImagenScroll.AutoScrollPosition.X;
                int scrollY = pnlImagenScroll.AutoScrollPosition.Y;

                // Aplicar el movimiento (invertir el signo porque AutoScrollPosition usa valores negativos)
                pnlImagenScroll.AutoScrollPosition = new Point(
                    Math.Abs(scrollX - deltaX),
                    Math.Abs(scrollY - deltaY)
                );

                // Actualizar el punto de inicio para el siguiente movimiento
                _dragStartPoint = e.Location;
            }
        }

        private void picVisor_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            picVisor.Cursor = _zoomLevel > 1.0f ? Cursors.Hand : Cursors.Default;
        }

        private void AplicarZoom()
        {
            if (_imagenOriginal == null) return;

            try
            {
                // Liberar imagen anterior del PictureBox
                if (picVisor.Image != null && picVisor.Image != _imagenOriginal)
                {
                    picVisor.Image.Dispose();
                }

                // Obtener el tamaño del contenedor
                int anchoContenedor = pnlImagenScroll.ClientSize.Width;
                int altoContenedor = pnlImagenScroll.ClientSize.Height;

                // Calcular el tamaño que tendría la imagen al ajustarse al contenedor (100% = ajustado)
                float ratioImagen = (float)_imagenOriginal.Width / _imagenOriginal.Height;
                float ratioContenedor = (float)anchoContenedor / altoContenedor;

                int anchoBase, altoBase;

                if (ratioImagen > ratioContenedor)
                {
                    // La imagen es más ancha proporcionalmente, ajustar por ancho
                    anchoBase = anchoContenedor;
                    altoBase = (int)(anchoContenedor / ratioImagen);
                }
                else
                {
                    // La imagen es más alta proporcionalmente, ajustar por alto
                    altoBase = altoContenedor;
                    anchoBase = (int)(altoContenedor * ratioImagen);
                }

                // Aplicar el zoom sobre el tamaño base ajustado
                int nuevoAncho = (int)(anchoBase * _zoomLevel);
                int nuevoAlto = (int)(altoBase * _zoomLevel);

                // Si zoom <= 100%, usar el contenedor completo
                if (_zoomLevel <= 1.0f)
                {
                    picVisor.SizeMode = PictureBoxSizeMode.Zoom;
                    picVisor.Image = _imagenOriginal;
                    picVisor.Dock = DockStyle.Fill;
                }
                else
                {
                    // Para zoom > 100%, crear imagen escalada
                    picVisor.SizeMode = PictureBoxSizeMode.Zoom;
                    picVisor.Dock = DockStyle.None;
                    picVisor.Size = new Size(nuevoAncho, nuevoAlto);
                    picVisor.Image = _imagenOriginal;

                    // Centrar en el scroll
                    picVisor.Location = new Point(0, 0);
                }

                // Actualizar cursor según zoom
                picVisor.Cursor = _zoomLevel > 1.0f ? Cursors.Hand : Cursors.Default;
            }
            catch (Exception ex)
            {
                Logger.Error("Error al aplicar zoom", ex, "FrmControlLote");
            }
        }

        // Acciones
        private void btnMarcarControlado_Click(object sender, EventArgs e)
        {
            try
            {
                if (_indiceActual < 0) return;

                var archivo = _archivosFiltrados[_indiceActual];

                // Validar que tenga campos obligatorios
                if (archivo.TieneCamposFaltantes())
                {
                    MessageBox.Show("No se puede marcar como controlado. Faltan campos obligatorios.",
                        "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _loteRepo.MarcarArchivoComoControlado(archivo.CdLoteArchivo, SesionActual.UsuarioActual.CdUsuario);

                MessageBox.Show("Archivo marcado como controlado", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Recargar y avanzar
                CargarArchivosLote();
                ActualizarEstadisticasLote();

                if (_indiceActual < _archivosFiltrados.Count - 1)
                {
                    SeleccionarArchivo(_indiceActual + 1);
                }
                else if (_archivosFiltrados.Count > 0)
                {
                    SeleccionarArchivo(0);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error al marcar archivo como controlado", ex, "FrmControlLote");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnMarcarIlegible_Click(object sender, EventArgs e)
        {
            try
            {
                if (_indiceActual < 0) return;

                var result = MessageBox.Show(
                    "¿Está seguro de marcar este archivo como CARÁTULA ILEGIBLE?\n\nEsto indica que la imagen no permite identificar los datos.",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;

                var archivo = _archivosFiltrados[_indiceActual];
                _loteRepo.MarcarArchivoComoIlegible(archivo.CdLoteArchivo, SesionActual.UsuarioActual.CdUsuario);

                MessageBox.Show("Archivo marcado como ilegible", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Recargar y avanzar
                CargarArchivosLote();
                ActualizarEstadisticasLote();

                if (_indiceActual < _archivosFiltrados.Count - 1)
                {
                    SeleccionarArchivo(_indiceActual + 1);
                }
                else if (_archivosFiltrados.Count > 0)
                {
                    SeleccionarArchivo(0);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error al marcar archivo como ilegible", ex, "FrmControlLote");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnFinalizarLote_Click(object sender, EventArgs e)
        {
            try
            {
                var stats = _loteRepo.ObtenerEstadisticasLote(_lote.CdLote);

                if (stats.Pendientes > 0)
                {
                    var result = MessageBox.Show(
                        $"ADVERTENCIA:\n\n" +
                        $"Total de archivos: {stats.Total}\n" +
                        $"Controlados: {stats.Controlados}\n" +
                        $"Ilegibles: {stats.Ilegibles}\n" +
                        $"Pendientes: {stats.Pendientes}\n\n" +
                        $"¿Desea finalizar el lote de todas formas?",
                        "Archivos Pendientes", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result != DialogResult.Yes) return;
                }

                bool exito = _loteRepo.FinalizarControlLote(_lote.CdLote, SesionActual.UsuarioActual.CdUsuario, out string mensaje);

                if (exito)
                {
                    MessageBox.Show("Lote finalizado exitosamente. Estado cambiado a 'Pendiente de Finalizar'",
                        "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(mensaje, "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al finalizar lote {_lote.CdLote}", ex, "FrmControlLote");
                MessageBox.Show($"Error al finalizar lote: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActualizarEstadisticasLote()
        {
            try
            {
                var stats = _loteRepo.ObtenerEstadisticasLote(_lote.CdLote);
                lblEstadisticas.Text = $"Total: {stats.Total} | Controlados: {stats.Controlados} | " +
                                      $"Ilegibles: {stats.Ilegibles} | Pendientes: {stats.Pendientes}";
            }
            catch (Exception ex)
            {
                Logger.Error("Error al actualizar estadísticas", ex, "FrmControlLote");
            }
        }

        // Filtros
        private void btnAplicarFiltros_Click(object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        private void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            cmbFiltroTipoPlano.SelectedIndex = 0;
            cmbFiltroEstado.SelectedIndex = 0;
            chkFiltroFaltantes.Checked = false;
            numFiltroPorcentaje.Value = 0;
            AplicarFiltros();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            if (_cambiosPendientes)
            {
                var result = MessageBox.Show("Hay cambios sin guardar. ¿Desea salir de todas formas?",
                    "Cambios Pendientes", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;
            }

            this.Close();
        }

        private void FrmControlLote_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Verificar cambios pendientes al cerrar
            if (_cambiosPendientes && e.CloseReason == CloseReason.UserClosing)
            {
                var result = MessageBox.Show("Hay cambios sin guardar. ¿Desea salir de todas formas?",
                    "Cambios Pendientes", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Liberar imágenes
            if (_imagenOriginal != null && _imagenOriginal != picVisor.Image)
            {
                _imagenOriginal.Dispose();
                _imagenOriginal = null;
            }

            if (picVisor.Image != null)
            {
                picVisor.Image.Dispose();
                picVisor.Image = null;
            }
        }
    }
}
