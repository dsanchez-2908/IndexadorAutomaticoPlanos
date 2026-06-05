namespace IndexadorAutomaticoPlanos.UI
{
    partial class FrmPreparacionImagenes
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlSuperior = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.pnlLotes = new System.Windows.Forms.Panel();
            this.dgvLotes = new System.Windows.Forms.DataGridView();
            this.lblLotesTitulo = new System.Windows.Forms.Label();
            this.pnlConfiguracion = new System.Windows.Forms.Panel();
            this.grpParametros = new System.Windows.Forms.GroupBox();
            this.lblEsquina = new System.Windows.Forms.Label();
            this.cboEsquina = new System.Windows.Forms.ComboBox();
            this.lblDpi = new System.Windows.Forms.Label();
            this.numDpi = new System.Windows.Forms.NumericUpDown();
            this.lblPorcentajeVertical = new System.Windows.Forms.Label();
            this.numPorcentajeVertical = new System.Windows.Forms.NumericUpDown();
            this.lblPorcentajeHorizontal = new System.Windows.Forms.Label();
            this.numPorcentajeHorizontal = new System.Windows.Forms.NumericUpDown();
            this.chkOcr = new System.Windows.Forms.CheckBox();
            this.pnlPreview = new System.Windows.Forms.Panel();
            this.grpPreview = new System.Windows.Forms.GroupBox();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.btnCargarPdf = new System.Windows.Forms.Button();
            this.lblPreview = new System.Windows.Forms.Label();
            this.pnlInferior = new System.Windows.Forms.Panel();
            this.btnProcesar = new System.Windows.Forms.Button();
            this.btnRefrescar = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblEstado = new System.Windows.Forms.Label();
            this.pnlSuperior.SuspendLayout();
            this.pnlLotes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLotes)).BeginInit();
            this.pnlConfiguracion.SuspendLayout();
            this.grpParametros.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDpi)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPorcentajeVertical)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPorcentajeHorizontal)).BeginInit();
            this.pnlPreview.SuspendLayout();
            this.grpPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.pnlInferior.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlSuperior
            // 
            this.pnlSuperior.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.pnlSuperior.Controls.Add(this.lblTitulo);
            this.pnlSuperior.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSuperior.Location = new System.Drawing.Point(0, 0);
            this.pnlSuperior.Name = "pnlSuperior";
            this.pnlSuperior.Size = new System.Drawing.Size(1400, 60);
            this.pnlSuperior.TabIndex = 0;
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(12, 12);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(307, 32);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Preparación de Imágenes";
            // 
            // pnlLotes
            // 
            this.pnlLotes.Controls.Add(this.dgvLotes);
            this.pnlLotes.Controls.Add(this.lblLotesTitulo);
            this.pnlLotes.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLotes.Location = new System.Drawing.Point(0, 60);
            this.pnlLotes.Name = "pnlLotes";
            this.pnlLotes.Padding = new System.Windows.Forms.Padding(10);
            this.pnlLotes.Size = new System.Drawing.Size(400, 640);
            this.pnlLotes.TabIndex = 1;
            // 
            // dgvLotes
            // 
            this.dgvLotes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvLotes.Location = new System.Drawing.Point(10, 30);
            this.dgvLotes.Name = "dgvLotes";
            this.dgvLotes.Size = new System.Drawing.Size(380, 600);
            this.dgvLotes.TabIndex = 1;
            // 
            // lblLotesTitulo
            // 
            this.lblLotesTitulo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblLotesTitulo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblLotesTitulo.Location = new System.Drawing.Point(10, 10);
            this.lblLotesTitulo.Name = "lblLotesTitulo";
            this.lblLotesTitulo.Size = new System.Drawing.Size(380, 20);
            this.lblLotesTitulo.TabIndex = 0;
            this.lblLotesTitulo.Text = "Lotes Pendientes de Preparación";
            // 
            // pnlConfiguracion
            // 
            this.pnlConfiguracion.Controls.Add(this.grpParametros);
            this.pnlConfiguracion.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlConfiguracion.Location = new System.Drawing.Point(400, 60);
            this.pnlConfiguracion.Name = "pnlConfiguracion";
            this.pnlConfiguracion.Padding = new System.Windows.Forms.Padding(10);
            this.pnlConfiguracion.Size = new System.Drawing.Size(1000, 170);
            this.pnlConfiguracion.TabIndex = 2;
            // 
            // grpParametros
            // 
            this.grpParametros.Controls.Add(this.chkOcr);
            this.grpParametros.Controls.Add(this.numPorcentajeHorizontal);
            this.grpParametros.Controls.Add(this.lblPorcentajeHorizontal);
            this.grpParametros.Controls.Add(this.numPorcentajeVertical);
            this.grpParametros.Controls.Add(this.lblPorcentajeVertical);
            this.grpParametros.Controls.Add(this.numDpi);
            this.grpParametros.Controls.Add(this.lblDpi);
            this.grpParametros.Controls.Add(this.cboEsquina);
            this.grpParametros.Controls.Add(this.lblEsquina);
            this.grpParametros.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpParametros.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpParametros.Location = new System.Drawing.Point(10, 10);
            this.grpParametros.Name = "grpParametros";
            this.grpParametros.Size = new System.Drawing.Size(980, 150);
            this.grpParametros.TabIndex = 0;
            this.grpParametros.TabStop = false;
            this.grpParametros.Text = "Parámetros de Procesamiento";
            // 
            // lblEsquina
            // 
            this.lblEsquina.AutoSize = true;
            this.lblEsquina.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEsquina.Location = new System.Drawing.Point(15, 35);
            this.lblEsquina.Name = "lblEsquina";
            this.lblEsquina.Size = new System.Drawing.Size(105, 15);
            this.lblEsquina.TabIndex = 0;
            this.lblEsquina.Text = "Esquina a recortar:";
            // 
            // cboEsquina
            // 
            this.cboEsquina.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboEsquina.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboEsquina.FormattingEnabled = true;
            this.cboEsquina.Location = new System.Drawing.Point(130, 32);
            this.cboEsquina.Name = "cboEsquina";
            this.cboEsquina.Size = new System.Drawing.Size(180, 23);
            this.cboEsquina.TabIndex = 1;
            // 
            // lblDpi
            // 
            this.lblDpi.AutoSize = true;
            this.lblDpi.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDpi.Location = new System.Drawing.Point(340, 35);
            this.lblDpi.Name = "lblDpi";
            this.lblDpi.Size = new System.Drawing.Size(31, 15);
            this.lblDpi.TabIndex = 2;
            this.lblDpi.Text = "DPI:";
            // 
            // numDpi
            // 
            this.numDpi.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.numDpi.Location = new System.Drawing.Point(380, 32);
            this.numDpi.Name = "numDpi";
            this.numDpi.Size = new System.Drawing.Size(100, 23);
            this.numDpi.TabIndex = 3;
            // 
            // lblPorcentajeVertical
            // 
            this.lblPorcentajeVertical.AutoSize = true;
            this.lblPorcentajeVertical.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPorcentajeVertical.Location = new System.Drawing.Point(15, 70);
            this.lblPorcentajeVertical.Name = "lblPorcentajeVertical";
            this.lblPorcentajeVertical.Size = new System.Drawing.Size(102, 15);
            this.lblPorcentajeVertical.TabIndex = 4;
            this.lblPorcentajeVertical.Text = "% Recorte Vertical:";
            // 
            // numPorcentajeVertical
            // 
            this.numPorcentajeVertical.DecimalPlaces = 1;
            this.numPorcentajeVertical.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.numPorcentajeVertical.Location = new System.Drawing.Point(130, 67);
            this.numPorcentajeVertical.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numPorcentajeVertical.Name = "numPorcentajeVertical";
            this.numPorcentajeVertical.Size = new System.Drawing.Size(80, 23);
            this.numPorcentajeVertical.TabIndex = 5;
            this.numPorcentajeVertical.Value = new decimal(new int[] { 15, 0, 0, 0 });
            // 
            // lblPorcentajeHorizontal
            // 
            this.lblPorcentajeHorizontal.AutoSize = true;
            this.lblPorcentajeHorizontal.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPorcentajeHorizontal.Location = new System.Drawing.Point(230, 70);
            this.lblPorcentajeHorizontal.Name = "lblPorcentajeHorizontal";
            this.lblPorcentajeHorizontal.Size = new System.Drawing.Size(117, 15);
            this.lblPorcentajeHorizontal.TabIndex = 6;
            this.lblPorcentajeHorizontal.Text = "% Recorte Horizontal:";
            // 
            // numPorcentajeHorizontal
            // 
            this.numPorcentajeHorizontal.DecimalPlaces = 1;
            this.numPorcentajeHorizontal.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.numPorcentajeHorizontal.Location = new System.Drawing.Point(355, 67);
            this.numPorcentajeHorizontal.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numPorcentajeHorizontal.Name = "numPorcentajeHorizontal";
            this.numPorcentajeHorizontal.Size = new System.Drawing.Size(80, 23);
            this.numPorcentajeHorizontal.TabIndex = 7;
            this.numPorcentajeHorizontal.Value = new decimal(new int[] { 20, 0, 0, 0 });
            // 
            // chkOcr
            // 
            this.chkOcr.AutoSize = true;
            this.chkOcr.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkOcr.Location = new System.Drawing.Point(15, 110);
            this.chkOcr.Name = "chkOcr";
            this.chkOcr.Size = new System.Drawing.Size(224, 19);
            this.chkOcr.TabIndex = 8;
            this.chkOcr.Text = "Ejecutar OCR (Tesseract spa+eng)";
            this.chkOcr.UseVisualStyleBackColor = true;
            // 
            // pnlPreview
            // 
            this.pnlPreview.Controls.Add(this.grpPreview);
            this.pnlPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPreview.Location = new System.Drawing.Point(400, 230);
            this.pnlPreview.Name = "pnlPreview";
            this.pnlPreview.Padding = new System.Windows.Forms.Padding(10);
            this.pnlPreview.Size = new System.Drawing.Size(1000, 470);
            this.pnlPreview.TabIndex = 3;
            // 
            // grpPreview
            // 
            this.grpPreview.Controls.Add(this.lblPreview);
            this.grpPreview.Controls.Add(this.btnCargarPdf);
            this.grpPreview.Controls.Add(this.picPreview);
            this.grpPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpPreview.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpPreview.Location = new System.Drawing.Point(10, 10);
            this.grpPreview.Name = "grpPreview";
            this.grpPreview.Size = new System.Drawing.Size(980, 470);
            this.grpPreview.TabIndex = 0;
            this.grpPreview.TabStop = false;
            this.grpPreview.Text = "Previsualización del Recorte";
            // 
            // picPreview
            // 
            this.picPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picPreview.BackColor = System.Drawing.Color.White;
            this.picPreview.Location = new System.Drawing.Point(15, 60);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(950, 380);
            this.picPreview.TabIndex = 0;
            this.picPreview.TabStop = false;
            // 
            // btnCargarPdf
            // 
            this.btnCargarPdf.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCargarPdf.Location = new System.Drawing.Point(15, 25);
            this.btnCargarPdf.Name = "btnCargarPdf";
            this.btnCargarPdf.Size = new System.Drawing.Size(200, 30);
            this.btnCargarPdf.TabIndex = 1;
            this.btnCargarPdf.Text = "Cargar PDF para Preview";
            this.btnCargarPdf.UseVisualStyleBackColor = true;
            this.btnCargarPdf.Click += new System.EventHandler(this.btnCargarPdf_Click);
            // 
            // lblPreview
            // 
            this.lblPreview.AutoSize = true;
            this.lblPreview.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPreview.Location = new System.Drawing.Point(230, 32);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Size = new System.Drawing.Size(260, 15);
            this.lblPreview.TabIndex = 2;
            this.lblPreview.Text = "Cargue un PDF para ver el recorte propuesto";
            // 
            // pnlInferior
            // 
            this.pnlInferior.Controls.Add(this.lblEstado);
            this.pnlInferior.Controls.Add(this.progressBar);
            this.pnlInferior.Controls.Add(this.btnRefrescar);
            this.pnlInferior.Controls.Add(this.btnProcesar);
            this.pnlInferior.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlInferior.Location = new System.Drawing.Point(0, 700);
            this.pnlInferior.Name = "pnlInferior";
            this.pnlInferior.Padding = new System.Windows.Forms.Padding(10);
            this.pnlInferior.Size = new System.Drawing.Size(1400, 80);
            this.pnlInferior.TabIndex = 4;
            // 
            // btnProcesar
            // 
            this.btnProcesar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnProcesar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnProcesar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnProcesar.ForeColor = System.Drawing.Color.White;
            this.btnProcesar.Location = new System.Drawing.Point(1170, 15);
            this.btnProcesar.Name = "btnProcesar";
            this.btnProcesar.Size = new System.Drawing.Size(200, 50);
            this.btnProcesar.TabIndex = 0;
            this.btnProcesar.Text = "Procesar Lotes Seleccionados";
            this.btnProcesar.UseVisualStyleBackColor = false;
            this.btnProcesar.Click += new System.EventHandler(this.btnProcesar_Click);
            // 
            // btnRefrescar
            // 
            this.btnRefrescar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefrescar.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnRefrescar.Location = new System.Drawing.Point(1050, 15);
            this.btnRefrescar.Name = "btnRefrescar";
            this.btnRefrescar.Size = new System.Drawing.Size(100, 50);
            this.btnRefrescar.TabIndex = 1;
            this.btnRefrescar.Text = "Refrescar";
            this.btnRefrescar.UseVisualStyleBackColor = true;
            this.btnRefrescar.Click += new System.EventHandler(this.btnRefrescar_Click);
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(15, 45);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(1020, 23);
            this.progressBar.TabIndex = 2;
            // 
            // lblEstado
            // 
            this.lblEstado.AutoSize = true;
            this.lblEstado.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEstado.Location = new System.Drawing.Point(15, 20);
            this.lblEstado.Name = "lblEstado";
            this.lblEstado.Size = new System.Drawing.Size(117, 15);
            this.lblEstado.TabIndex = 3;
            this.lblEstado.Text = "Listo para procesar...";
            // 
            // FrmPreparacionImagenes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1400, 780);
            this.Controls.Add(this.pnlPreview);
            this.Controls.Add(this.pnlConfiguracion);
            this.Controls.Add(this.pnlLotes);
            this.Controls.Add(this.pnlInferior);
            this.Controls.Add(this.pnlSuperior);
            this.MinimumSize = new System.Drawing.Size(1200, 700);
            this.Name = "FrmPreparacionImagenes";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Preparación de Imágenes";
            this.Load += new System.EventHandler(this.FrmPreparacionImagenes_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmPreparacionImagenes_FormClosing);
            this.pnlSuperior.ResumeLayout(false);
            this.pnlSuperior.PerformLayout();
            this.pnlLotes.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLotes)).EndInit();
            this.pnlConfiguracion.ResumeLayout(false);
            this.grpParametros.ResumeLayout(false);
            this.grpParametros.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDpi)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPorcentajeVertical)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPorcentajeHorizontal)).EndInit();
            this.pnlPreview.ResumeLayout(false);
            this.grpPreview.ResumeLayout(false);
            this.grpPreview.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.pnlInferior.ResumeLayout(false);
            this.pnlInferior.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlSuperior;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel pnlLotes;
        private System.Windows.Forms.DataGridView dgvLotes;
        private System.Windows.Forms.Label lblLotesTitulo;
        private System.Windows.Forms.Panel pnlConfiguracion;
        private System.Windows.Forms.GroupBox grpParametros;
        private System.Windows.Forms.Label lblEsquina;
        private System.Windows.Forms.ComboBox cboEsquina;
        private System.Windows.Forms.Label lblDpi;
        private System.Windows.Forms.NumericUpDown numDpi;
        private System.Windows.Forms.Label lblPorcentajeVertical;
        private System.Windows.Forms.NumericUpDown numPorcentajeVertical;
        private System.Windows.Forms.Label lblPorcentajeHorizontal;
        private System.Windows.Forms.NumericUpDown numPorcentajeHorizontal;
        private System.Windows.Forms.CheckBox chkOcr;
        private System.Windows.Forms.Panel pnlPreview;
        private System.Windows.Forms.GroupBox grpPreview;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.Button btnCargarPdf;
        private System.Windows.Forms.Label lblPreview;
        private System.Windows.Forms.Panel pnlInferior;
        private System.Windows.Forms.Button btnProcesar;
        private System.Windows.Forms.Button btnRefrescar;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblEstado;
    }
}
