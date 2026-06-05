namespace IndexadorAutomaticoPlanos.UI
{
    partial class FrmPreparacionLotes
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlSuperior = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblEstadisticas = new System.Windows.Forms.Label();
            this.pnlCentral = new System.Windows.Forms.Panel();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.grpArchivosDisponibles = new System.Windows.Forms.GroupBox();
            this.dgvArchivosDisponibles = new System.Windows.Forms.DataGridView();
            this.colCarpeta = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCantidad = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPaginasTotal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlBotonesIzq = new System.Windows.Forms.Panel();
            this.lblSeleccionados = new System.Windows.Forms.Label();
            this.btnCrearLote = new System.Windows.Forms.Button();
            this.grpLotes = new System.Windows.Forms.GroupBox();
            this.dgvLotes = new System.Windows.Forms.DataGridView();
            this.colNombreLote = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEstadoLote = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCantidadArchivos = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFechaCreacion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlBotonesDer = new System.Windows.Forms.Panel();
            this.btnDividirLote = new System.Windows.Forms.Button();
            this.btnRefrescar = new System.Windows.Forms.Button();
            this.pnlSuperior.SuspendLayout();
            this.pnlCentral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.grpArchivosDisponibles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvArchivosDisponibles)).BeginInit();
            this.pnlBotonesIzq.SuspendLayout();
            this.grpLotes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLotes)).BeginInit();
            this.pnlBotonesDer.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlSuperior
            // 
            this.pnlSuperior.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.pnlSuperior.Controls.Add(this.lblTitulo);
            this.pnlSuperior.Controls.Add(this.lblEstadisticas);
            this.pnlSuperior.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSuperior.Location = new System.Drawing.Point(0, 0);
            this.pnlSuperior.Name = "pnlSuperior";
            this.pnlSuperior.Size = new System.Drawing.Size(1400, 80);
            this.pnlSuperior.TabIndex = 0;
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(12, 12);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(291, 30);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "📦 Preparación de Lotes";
            // 
            // lblEstadisticas
            // 
            this.lblEstadisticas.AutoSize = true;
            this.lblEstadisticas.ForeColor = System.Drawing.Color.LightGray;
            this.lblEstadisticas.Location = new System.Drawing.Point(12, 50);
            this.lblEstadisticas.Name = "lblEstadisticas";
            this.lblEstadisticas.Size = new System.Drawing.Size(350, 15);
            this.lblEstadisticas.TabIndex = 1;
            this.lblEstadisticas.Text = "Archivos disponibles: 0 | Carpetas: 0 | Lotes creados: 0";
            // 
            // pnlCentral
            // 
            this.pnlCentral.Controls.Add(this.splitContainer);
            this.pnlCentral.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCentral.Location = new System.Drawing.Point(0, 80);
            this.pnlCentral.Name = "pnlCentral";
            this.pnlCentral.Padding = new System.Windows.Forms.Padding(10);
            this.pnlCentral.Size = new System.Drawing.Size(1400, 620);
            this.pnlCentral.TabIndex = 1;
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(10, 10);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.grpArchivosDisponibles);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.grpLotes);
            this.splitContainer.Size = new System.Drawing.Size(1380, 600);
            this.splitContainer.SplitterDistance = 680;
            this.splitContainer.TabIndex = 0;
            // 
            // grpArchivosDisponibles
            // 
            this.grpArchivosDisponibles.Controls.Add(this.dgvArchivosDisponibles);
            this.grpArchivosDisponibles.Controls.Add(this.pnlBotonesIzq);
            this.grpArchivosDisponibles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpArchivosDisponibles.Location = new System.Drawing.Point(0, 0);
            this.grpArchivosDisponibles.Name = "grpArchivosDisponibles";
            this.grpArchivosDisponibles.Size = new System.Drawing.Size(680, 600);
            this.grpArchivosDisponibles.TabIndex = 0;
            this.grpArchivosDisponibles.TabStop = false;
            this.grpArchivosDisponibles.Text = "Archivos Disponibles (por Carpeta)";
            // 
            // dgvArchivosDisponibles
            // 
            this.dgvArchivosDisponibles.AllowUserToAddRows = false;
            this.dgvArchivosDisponibles.AllowUserToDeleteRows = false;
            this.dgvArchivosDisponibles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvArchivosDisponibles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCarpeta,
            this.colCantidad,
            this.colPaginasTotal});
            this.dgvArchivosDisponibles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvArchivosDisponibles.Location = new System.Drawing.Point(3, 19);
            this.dgvArchivosDisponibles.Name = "dgvArchivosDisponibles";
            this.dgvArchivosDisponibles.ReadOnly = true;
            this.dgvArchivosDisponibles.Size = new System.Drawing.Size(674, 498);
            this.dgvArchivosDisponibles.TabIndex = 0;
            this.dgvArchivosDisponibles.SelectionChanged += new System.EventHandler(this.dgvArchivosDisponibles_SelectionChanged);
            // 
            // colCarpeta
            // 
            this.colCarpeta.HeaderText = "Carpeta / Archivo";
            this.colCarpeta.Name = "colCarpeta";
            this.colCarpeta.ReadOnly = true;
            this.colCarpeta.Width = 400;
            // 
            // colCantidad
            // 
            this.colCantidad.HeaderText = "Cantidad/Páginas";
            this.colCantidad.Name = "colCantidad";
            this.colCantidad.ReadOnly = true;
            this.colCantidad.Width = 120;
            // 
            // colPaginasTotal
            // 
            this.colPaginasTotal.HeaderText = "Total Págs/Tamaño";
            this.colPaginasTotal.Name = "colPaginasTotal";
            this.colPaginasTotal.ReadOnly = true;
            this.colPaginasTotal.Width = 130;
            // 
            // pnlBotonesIzq
            // 
            this.pnlBotonesIzq.Controls.Add(this.lblSeleccionados);
            this.pnlBotonesIzq.Controls.Add(this.btnCrearLote);
            this.pnlBotonesIzq.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBotonesIzq.Location = new System.Drawing.Point(3, 517);
            this.pnlBotonesIzq.Name = "pnlBotonesIzq";
            this.pnlBotonesIzq.Size = new System.Drawing.Size(674, 80);
            this.pnlBotonesIzq.TabIndex = 1;
            // 
            // lblSeleccionados
            // 
            this.lblSeleccionados.AutoSize = true;
            this.lblSeleccionados.Location = new System.Drawing.Point(10, 50);
            this.lblSeleccionados.Name = "lblSeleccionados";
            this.lblSeleccionados.Size = new System.Drawing.Size(143, 15);
            this.lblSeleccionados.TabIndex = 1;
            this.lblSeleccionados.Text = "Archivos seleccionados: 0";
            // 
            // btnCrearLote
            // 
            this.btnCrearLote.Enabled = false;
            this.btnCrearLote.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCrearLote.Location = new System.Drawing.Point(10, 10);
            this.btnCrearLote.Name = "btnCrearLote";
            this.btnCrearLote.Size = new System.Drawing.Size(180, 35);
            this.btnCrearLote.TabIndex = 0;
            this.btnCrearLote.Text = "➕ Crear Lote";
            this.btnCrearLote.UseVisualStyleBackColor = true;
            this.btnCrearLote.Click += new System.EventHandler(this.btnCrearLote_Click);
            // 
            // grpLotes
            // 
            this.grpLotes.Controls.Add(this.dgvLotes);
            this.grpLotes.Controls.Add(this.pnlBotonesDer);
            this.grpLotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpLotes.Location = new System.Drawing.Point(0, 0);
            this.grpLotes.Name = "grpLotes";
            this.grpLotes.Size = new System.Drawing.Size(696, 600);
            this.grpLotes.TabIndex = 0;
            this.grpLotes.TabStop = false;
            this.grpLotes.Text = "Lotes Creados";
            // 
            // dgvLotes
            // 
            this.dgvLotes.AllowUserToAddRows = false;
            this.dgvLotes.AllowUserToDeleteRows = false;
            this.dgvLotes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLotes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colNombreLote,
            this.colEstadoLote,
            this.colCantidadArchivos,
            this.colFechaCreacion});
            this.dgvLotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvLotes.Location = new System.Drawing.Point(3, 19);
            this.dgvLotes.Name = "dgvLotes";
            this.dgvLotes.ReadOnly = true;
            this.dgvLotes.Size = new System.Drawing.Size(690, 498);
            this.dgvLotes.TabIndex = 0;
            this.dgvLotes.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvLotes_CellDoubleClick);
            this.dgvLotes.SelectionChanged += new System.EventHandler(this.dgvLotes_SelectionChanged);
            // 
            // colNombreLote
            // 
            this.colNombreLote.HeaderText = "Nombre Lote";
            this.colNombreLote.Name = "colNombreLote";
            this.colNombreLote.ReadOnly = true;
            this.colNombreLote.Width = 150;
            // 
            // colEstadoLote
            // 
            this.colEstadoLote.HeaderText = "Estado";
            this.colEstadoLote.Name = "colEstadoLote";
            this.colEstadoLote.ReadOnly = true;
            this.colEstadoLote.Width = 220;
            // 
            // colCantidadArchivos
            // 
            this.colCantidadArchivos.HeaderText = "Archivos";
            this.colCantidadArchivos.Name = "colCantidadArchivos";
            this.colCantidadArchivos.ReadOnly = true;
            this.colCantidadArchivos.Width = 80;
            // 
            // colFechaCreacion
            // 
            this.colFechaCreacion.HeaderText = "Fecha Creación";
            this.colFechaCreacion.Name = "colFechaCreacion";
            this.colFechaCreacion.ReadOnly = true;
            this.colFechaCreacion.Width = 150;
            // 
            // pnlBotonesDer
            // 
            this.pnlBotonesDer.Controls.Add(this.btnDividirLote);
            this.pnlBotonesDer.Controls.Add(this.btnRefrescar);
            this.pnlBotonesDer.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBotonesDer.Location = new System.Drawing.Point(3, 517);
            this.pnlBotonesDer.Name = "pnlBotonesDer";
            this.pnlBotonesDer.Size = new System.Drawing.Size(690, 80);
            this.pnlBotonesDer.TabIndex = 1;
            // 
            // btnDividirLote
            // 
            this.btnDividirLote.Enabled = false;
            this.btnDividirLote.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnDividirLote.Location = new System.Drawing.Point(10, 10);
            this.btnDividirLote.Name = "btnDividirLote";
            this.btnDividirLote.Size = new System.Drawing.Size(180, 35);
            this.btnDividirLote.TabIndex = 0;
            this.btnDividirLote.Text = "✂️ Dividir Lote";
            this.btnDividirLote.UseVisualStyleBackColor = true;
            this.btnDividirLote.Click += new System.EventHandler(this.btnDividirLote_Click);
            // 
            // btnRefrescar
            // 
            this.btnRefrescar.Location = new System.Drawing.Point(200, 10);
            this.btnRefrescar.Name = "btnRefrescar";
            this.btnRefrescar.Size = new System.Drawing.Size(120, 35);
            this.btnRefrescar.TabIndex = 1;
            this.btnRefrescar.Text = "🔄 Refrescar";
            this.btnRefrescar.UseVisualStyleBackColor = true;
            this.btnRefrescar.Click += new System.EventHandler(this.btnRefrescar_Click);
            // 
            // FrmPreparacionLotes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1400, 700);
            this.Controls.Add(this.pnlCentral);
            this.Controls.Add(this.pnlSuperior);
            this.Name = "FrmPreparacionLotes";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Preparación de Lotes";
            this.Load += new System.EventHandler(this.FrmPreparacionLotes_Load);
            this.pnlSuperior.ResumeLayout(false);
            this.pnlSuperior.PerformLayout();
            this.pnlCentral.ResumeLayout(false);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.grpArchivosDisponibles.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvArchivosDisponibles)).EndInit();
            this.pnlBotonesIzq.ResumeLayout(false);
            this.pnlBotonesIzq.PerformLayout();
            this.grpLotes.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLotes)).EndInit();
            this.pnlBotonesDer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlSuperior;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblEstadisticas;
        private System.Windows.Forms.Panel pnlCentral;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.GroupBox grpArchivosDisponibles;
        private System.Windows.Forms.DataGridView dgvArchivosDisponibles;
        private System.Windows.Forms.Panel pnlBotonesIzq;
        private System.Windows.Forms.Label lblSeleccionados;
        private System.Windows.Forms.Button btnCrearLote;
        private System.Windows.Forms.GroupBox grpLotes;
        private System.Windows.Forms.DataGridView dgvLotes;
        private System.Windows.Forms.Panel pnlBotonesDer;
        private System.Windows.Forms.Button btnDividirLote;
        private System.Windows.Forms.Button btnRefrescar;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCarpeta;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCantidad;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPaginasTotal;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNombreLote;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEstadoLote;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCantidadArchivos;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFechaCreacion;
    }
}
