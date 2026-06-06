using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
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
    /// Formulario para el procesamiento de lotes por OpenAI
    /// Fase 5: Extracción de datos estructurados de planos mediante IA
    /// </summary>
    public partial class FrmProcesamientoIA : Form
    {
        private readonly LoteRepository _loteRepo;
        private readonly ParametroRepository _parametroRepo;
        private List<Lote> _lotesCargados;
        private bool _procesando = false;

        public FrmProcesamientoIA()
        {
            InitializeComponent();
            _loteRepo = new LoteRepository();
            _parametroRepo = new ParametroRepository();
            _lotesCargados = new List<Lote>();
        }

        private void FrmProcesamientoIA_Load(object sender, EventArgs e)
        {
            try
            {
                ConfigurarControles();
                CargarPrompts();
                CargarLotesPendientes();
                ActualizarEstado("Formulario cargado correctamente");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cargar formulario de procesamiento IA: {ex.Message}", null, "FrmProcesamientoIA");
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
            dgvLotes.MultiSelect = true;
            dgvLotes.ReadOnly = true;
            dgvLotes.AllowUserToAddRows = false;
            dgvLotes.AllowUserToDeleteRows = false;
            dgvLotes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Configurar barra de progreso
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;

            // Configurar prompts con fuente monoespaciada
            txtPromptOcr.Font = new Font("Consolas", 9F);
            txtPromptImagen.Font = new Font("Consolas", 9F);
        }

        /// <summary>
        /// Carga los prompts desde la parametrización
        /// </summary>
        private void CargarPrompts()
        {
            try
            {
                var parametroOcr = _parametroRepo.ObtenerPorClave("OPENAI_API_PROMPT_OCR");
                if (parametroOcr != null)
                {
                    txtPromptOcr.Text = parametroOcr.DsValorParametro ?? string.Empty;
                }

                var parametroImagen = _parametroRepo.ObtenerPorClave("OPENAI_API_PROMPT_IMAGEN");
                if (parametroImagen != null)
                {
                    txtPromptImagen.Text = parametroImagen.DsValorParametro ?? string.Empty;
                }

                Logger.Info("Prompts cargados desde parametrización", "FrmProcesamientoIA");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cargar prompts: {ex.Message}", null, "FrmProcesamientoIA");
                MessageBox.Show($"Error al cargar prompts: {ex.Message}", "Advertencia",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Carga los lotes pendientes de procesamiento por IA
        /// </summary>
        private void CargarLotesPendientes()
        {
            try
            {
                ActualizarEstado("Cargando lotes pendientes...");

                _lotesCargados = _loteRepo.ObtenerLotesPendientesProcesarIA();

                var dt = new DataTable();
                dt.Columns.Add("Seleccionar", typeof(bool));
                dt.Columns.Add("CdLote", typeof(int));
                dt.Columns.Add("Nombre Lote", typeof(string));
                dt.Columns.Add("Archivos", typeof(int));
                dt.Columns.Add("Fecha Alta", typeof(DateTime));
                dt.Columns.Add("Estado", typeof(string));

                foreach (var lote in _lotesCargados)
                {
                    dt.Rows.Add(false, lote.CdLote, lote.DsNombreLote, lote.NuCantidadArchivos,
                        lote.FeAlta, lote.DsEstadoLote);
                }

                dgvLotes.DataSource = dt;

                // Configurar columna de checkbox
                if (dgvLotes.Columns["Seleccionar"] != null)
                {
                    dgvLotes.Columns["Seleccionar"].ReadOnly = false;
                    dgvLotes.Columns["Seleccionar"].Width = 80;
                }

                // Ocultar columna de ID
                if (dgvLotes.Columns["CdLote"] != null)
                {
                    dgvLotes.Columns["CdLote"].Visible = false;
                }

                ActualizarEstado($"{_lotesCargados.Count} lote(s) pendiente(s) de procesar por IA");
                Logger.Info("Cargados {_lotesCargados.Count} lotes pendientes de procesamiento por IA", "FrmProcesamientoIA");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cargar lotes pendientes: {ex.Message}", null, "FrmProcesamientoIA");
                MessageBox.Show($"Error al cargar lotes: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Actualiza el label de estado
        /// </summary>
        private void ActualizarEstado(string mensaje)
        {
            if (lblEstado.InvokeRequired)
            {
                lblEstado.Invoke(new Action<string>(ActualizarEstado), mensaje);
                return;
            }

            lblEstado.Text = mensaje;
            lblEstado.Refresh();
            Logger.Info("Estado UI: {mensaje}", "FrmProcesamientoIA");
        }

        /// <summary>
        /// Actualiza la barra de progreso
        /// </summary>
        private void ActualizarProgreso(int valor, int maximo)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new Action<int, int>(ActualizarProgreso), valor, maximo);
                return;
            }

            progressBar.Maximum = maximo;
            progressBar.Value = Math.Min(valor, maximo);
            progressBar.Refresh();
        }

        /// <summary>
        /// Maneja el clic en el botón Refrescar
        /// </summary>
        private void btnRefrescar_Click(object sender, EventArgs e)
        {
            if (_procesando)
            {
                MessageBox.Show("Hay un procesamiento en curso. Espere a que finalice.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            CargarLotesPendientes();
        }

        /// <summary>
        /// Maneja el clic en el botón Guardar Prompts
        /// </summary>
        private void btnGuardarPrompts_Click(object sender, EventArgs e)
        {
            try
            {
                // Guardar prompt OCR
                var parametroOcr = _parametroRepo.ObtenerPorClave("OPENAI_API_PROMPT_OCR");
                if (parametroOcr != null)
                {
                    parametroOcr.DsValorParametro = txtPromptOcr.Text;
                    _parametroRepo.Actualizar(parametroOcr);
                }

                // Guardar prompt Imagen
                var parametroImagen = _parametroRepo.ObtenerPorClave("OPENAI_API_PROMPT_IMAGEN");
                if (parametroImagen != null)
                {
                    parametroImagen.DsValorParametro = txtPromptImagen.Text;
                    _parametroRepo.Actualizar(parametroImagen);
                }

                MessageBox.Show("Prompts guardados exitosamente", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Logger.Info("Prompts actualizados en parametrización", "FrmProcesamientoIA");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al guardar prompts: {ex.Message}", null, "FrmProcesamientoIA");
                MessageBox.Show($"Error al guardar prompts: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Maneja el clic en el botón Procesar
        /// </summary>
        private async void btnProcesar_Click(object sender, EventArgs e)
        {
            if (_procesando)
            {
                MessageBox.Show("Ya hay un procesamiento en curso", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Obtener lotes seleccionados
            var lotesSeleccionados = ObtenerLotesSeleccionados();

            if (lotesSeleccionados.Count == 0)
            {
                MessageBox.Show("Debe seleccionar al menos un lote para procesar", "Advertencia",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Confirmación
            string modalidad = chkUsarOcr.Checked ? "Híbrida (OCR + Imagen)" : "Solo Imagen";
            string mensaje = $"¿Desea procesar {lotesSeleccionados.Count} lote(s) usando modalidad {modalidad}?\n\n" +
                           $"Este proceso puede tardar varios minutos u horas dependiendo de la cantidad de archivos.\n\n" +
                           $"Archivos totales: {lotesSeleccionados.Sum(l => l.NuCantidadArchivos)}";

            var resultado = MessageBox.Show(mensaje, "Confirmar Procesamiento",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resultado != DialogResult.Yes)
                return;

            // Iniciar procesamiento asíncrono
            await ProcesarLotesSeleccionados(lotesSeleccionados);
        }

        /// <summary>
        /// Obtiene la lista de lotes seleccionados en el grid
        /// </summary>
        private List<Lote> ObtenerLotesSeleccionados()
        {
            var seleccionados = new List<Lote>();

            foreach (DataGridViewRow row in dgvLotes.Rows)
            {
                if (row.Cells["Seleccionar"].Value != null &&
                    (bool)row.Cells["Seleccionar"].Value)
                {
                    int cdLote = Convert.ToInt32(row.Cells["CdLote"].Value);
                    var lote = _lotesCargados.FirstOrDefault(l => l.CdLote == cdLote);
                    if (lote != null)
                    {
                        seleccionados.Add(lote);
                    }
                }
            }

            return seleccionados;
        }

        /// <summary>
        /// Procesa los lotes seleccionados de manera asíncrona
        /// </summary>
        private async Task ProcesarLotesSeleccionados(List<Lote> lotes)
        {
            _procesando = true;
            DeshabilitarControles();

            try
            {
                int totalLotes = lotes.Count;
                int loteActual = 0;
                int archivosExitosos = 0;
                int archivosError = 0;
                var lotesConErrores = new List<string>();

                foreach (var lote in lotes)
                {
                    loteActual++;
                    ActualizarEstado($"Procesando lote {loteActual}/{totalLotes}: {lote.DsNombreLote}...");

                    // Obtener archivos del lote
                    var archivos = _loteRepo.ObtenerArchivosParaProcesar(lote.CdLote);
                    Logger.Info("Lote {lote.DsNombreLote}: {archivos.Count} archivos a procesar", "FrmProcesamientoIA");

                    int archivosProcesadosLote = 0;
                    int archivosErrorLote = 0;

                    for (int i = 0; i < archivos.Count; i++)
                    {
                        var archivo = archivos[i];
                        ActualizarEstado($"Lote {loteActual}/{totalLotes} - Archivo {i + 1}/{archivos.Count}: {archivo.DsNombreArchivo}");
                        ActualizarProgreso(i + 1, archivos.Count);

                        try
                        {
                            bool exitoso = await ProcesarArchivo(archivo, chkUsarOcr.Checked);
                            if (exitoso)
                            {
                                archivosProcesadosLote++;
                                archivosExitosos++;
                            }
                            else
                            {
                                archivosErrorLote++;
                                archivosError++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Error al procesar archivo {archivo.DsNombreArchivo}: {ex.Message}", null, "FrmProcesamientoIA");
                            archivosErrorLote++;
                            archivosError++;
                        }
                    }

                    // Si todos los archivos fueron procesados exitosamente, marcar lote como procesado
                    if (archivosErrorLote == 0)
                    {
                        bool loteActualizado = _loteRepo.MarcarLoteComoProcesado(lote.CdLote, SesionActual.UsuarioActual.CdUsuario);
                        if (loteActualizado)
                        {
                            Logger.Info("Lote {lote.DsNombreLote} marcado como procesado (Estado 3)", "FrmProcesamientoIA");
                        }
                    }
                    else
                    {
                        lotesConErrores.Add($"{lote.DsNombreLote} ({archivosErrorLote} errores)");
                        Logger.Info("Lote {lote.DsNombreLote} tiene {archivosErrorLote} archivos con error. Se mantiene en estado pendiente.", "FrmProcesamientoIA");
                    }
                }

                ActualizarProgreso(100, 100);

                // Mensaje de resumen
                string mensajeResumen = $"Procesamiento completado:\n\n" +
                                      $"Lotes procesados: {totalLotes}\n" +
                                      $"Archivos exitosos: {archivosExitosos}\n" +
                                      $"Archivos con error: {archivosError}";

                if (lotesConErrores.Count > 0)
                {
                    mensajeResumen += $"\n\nLotes con errores:\n{string.Join("\n", lotesConErrores)}";
                    MessageBox.Show(mensajeResumen, "Procesamiento Completado con Advertencias",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show(mensajeResumen, "Procesamiento Completado",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Refrescar lista de lotes
                CargarLotesPendientes();
            }
            catch (Exception ex)
            {
                Logger.Error("Error en procesamiento de lotes: {ex.Message}", null, "FrmProcesamientoIA");
                MessageBox.Show($"Error durante el procesamiento: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _procesando = false;
                HabilitarControles();
                ActualizarEstado("Listo para procesar lotes");
                ActualizarProgreso(0, 100);
            }
        }

        /// <summary>
        /// Procesa un archivo individual con OpenAI
        /// </summary>
        private async Task<bool> ProcesarArchivo(LoteArchivo archivo, bool usarModalidadHibrida)
        {
            try
            {
                Logger.Info("Iniciando procesamiento de archivo: {archivo.DsNombreArchivo}", "FrmProcesamientoIA");

                // Obtener estado "Enviado a IA"
                int estadoEnviadoIA = _loteRepo.ObtenerIdEstadoArchivoLotePorNombre("Enviado a IA");

                // Actualizar estado del archivo a "Enviado a IA"
                _loteRepo.ActualizarEstadoProcesamiento(archivo.CdLoteArchivo, estadoEnviadoIA);

                // Crear procesador de OpenAI
                var procesador = new OpenAIProcesador();

                // Procesar según modalidad
                ResultadoProcesamientoIA resultado;

                if (usarModalidadHibrida && archivo.SnTieneOcr)
                {
                    resultado = await procesador.ProcesarHibridoAsync(
                        archivo.DsNombreArchivo!,
                        archivo.TxResultadoOcr,
                        archivo.TxImagenBase64,
                        SesionActual.UsuarioActual.CdUsuario);
                }
                else if (!string.IsNullOrWhiteSpace(archivo.TxImagenBase64))
                {
                    resultado = await procesador.ProcesarPorImagenAsync(
                        archivo.DsNombreArchivo!,
                        archivo.TxImagenBase64,
                        SesionActual.UsuarioActual.CdUsuario);
                }
                else
                {
                    Logger.Error("Archivo {archivo.DsNombreArchivo} no tiene imagen ni OCR disponible", null, "FrmProcesamientoIA");
                    int estadoError = _loteRepo.ObtenerIdEstadoArchivoLotePorNombre("Error");
                    _loteRepo.ActualizarEstadoProcesamiento(archivo.CdLoteArchivo, estadoError);
                    return false;
                }

                // Guardar resultado en base de datos
                if (resultado.Exitoso && resultado.RespuestaIA != null)
                {
                    var resultadoIA = new ResultadoIA
                    {
                        CdLoteArchivo = archivo.CdLoteArchivo,
                        TxNombreArchivo = archivo.DsNombreArchivo,
                        DsTipoPlano = resultado.RespuestaIA.TipoPlano,
                        DsExpediente = resultado.RespuestaIA.Expediente,
                        DsSeccion = resultado.RespuestaIA.Seccion,
                        DsManzana = resultado.RespuestaIA.Manzana,
                        DsParcela = resultado.RespuestaIA.Parcela,
                        DsDireccion = resultado.RespuestaIA.Direccion,
                        NuConfianzaTipoPlano = resultado.RespuestaIA.Confianza?.TipoPlano,
                        NuConfianzaExpediente = resultado.RespuestaIA.Confianza?.Expediente,
                        NuConfianzaSeccion = resultado.RespuestaIA.Confianza?.Seccion,
                        NuConfianzaManzana = resultado.RespuestaIA.Confianza?.Manzana,
                        NuConfianzaParcela = resultado.RespuestaIA.Confianza?.Parcela,
                        NuConfianzaDireccion = resultado.RespuestaIA.Confianza?.Direccion,
                        NuPromptTokens = resultado.Usage?.PromptTokens,
                        NuCompletionTokens = resultado.Usage?.CompletionTokens,
                        NuTotalTokens = resultado.Usage?.TotalTokens,
                        DsModalidadProcesamiento = resultado.ModalidadProcesamiento,
                        NuIntentos = resultado.Intentos,
                        TxRespuestaCompleta = resultado.RespuestaCompleta,
                        CdUsuarioAlta = SesionActual.UsuarioActual.CdUsuario
                    };

                    int cdResultadoIA = _loteRepo.GuardarResultadoIA(resultadoIA);

                    // Actualizar estado a "Pendiente de Controlar"
                    int estadoPendienteControlar = _loteRepo.ObtenerIdEstadoArchivoLotePorNombre("Pendiente de Controlar");
                    _loteRepo.ActualizarEstadoProcesamiento(archivo.CdLoteArchivo, estadoPendienteControlar, cdResultadoIA);

                    Logger.Info("Archivo {archivo.DsNombreArchivo} procesado exitosamente. Tokens: {resultado.Usage?.TotalTokens}", "FrmProcesamientoIA");
                    return true;
                }
                else
                {
                    // Guardar error
                    var resultadoIA = new ResultadoIA
                    {
                        CdLoteArchivo = archivo.CdLoteArchivo,
                        TxNombreArchivo = archivo.DsNombreArchivo,
                        TxMensajeError = resultado.MensajeError,
                        DsModalidadProcesamiento = resultado.ModalidadProcesamiento,
                        NuIntentos = resultado.Intentos,
                        CdUsuarioAlta = SesionActual.UsuarioActual.CdUsuario
                    };

                    _loteRepo.GuardarResultadoIA(resultadoIA);

                    // Actualizar estado a "Error"
                    int estadoError = _loteRepo.ObtenerIdEstadoArchivoLotePorNombre("Error");
                    _loteRepo.ActualizarEstadoProcesamiento(archivo.CdLoteArchivo, estadoError);

                    Logger.Error("Error al procesar archivo {archivo.DsNombreArchivo}: {resultado.MensajeError}", null, "FrmProcesamientoIA");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Excepción al procesar archivo {archivo.DsNombreArchivo}: {ex.Message}", null, "FrmProcesamientoIA");

                // Guardar error en base de datos
                try
                {
                    var resultadoIA = new ResultadoIA
                    {
                        CdLoteArchivo = archivo.CdLoteArchivo,
                        TxNombreArchivo = archivo.DsNombreArchivo,
                        TxMensajeError = $"Excepción: {ex.Message}",
                        DsModalidadProcesamiento = usarModalidadHibrida ? "Hibrido-Error" : "Imagen-Error",
                        NuIntentos = 1,
                        CdUsuarioAlta = SesionActual.UsuarioActual.CdUsuario
                    };

                    _loteRepo.GuardarResultadoIA(resultadoIA);

                    int estadoError = _loteRepo.ObtenerIdEstadoArchivoLotePorNombre("Error");
                    _loteRepo.ActualizarEstadoProcesamiento(archivo.CdLoteArchivo, estadoError);
                }
                catch
                {
                    // Ignorar error al guardar error
                }

                return false;
            }
        }

        /// <summary>
        /// Deshabilita los controles durante el procesamiento
        /// </summary>
        private void DeshabilitarControles()
        {
            btnProcesar.Enabled = false;
            btnRefrescar.Enabled = false;
            btnGuardarPrompts.Enabled = false;
            dgvLotes.Enabled = false;
            chkUsarOcr.Enabled = false;
            txtPromptOcr.Enabled = false;
            txtPromptImagen.Enabled = false;
        }

        /// <summary>
        /// Habilita los controles después del procesamiento
        /// </summary>
        private void HabilitarControles()
        {
            btnProcesar.Enabled = true;
            btnRefrescar.Enabled = true;
            btnGuardarPrompts.Enabled = true;
            dgvLotes.Enabled = true;
            chkUsarOcr.Enabled = true;
            txtPromptOcr.Enabled = true;
            txtPromptImagen.Enabled = true;
        }
    }
}
