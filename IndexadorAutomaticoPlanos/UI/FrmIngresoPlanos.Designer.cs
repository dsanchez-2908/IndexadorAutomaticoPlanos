namespace IndexadorAutomaticoPlanos.UI
{
    partial class FrmIngresoPlanos
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
            this.btnEscanear = new System.Windows.Forms.Button();
            this.chkIncluirSubcarpetas = new System.Windows.Forms.CheckBox();
            this.txtRutaCarpeta = new System.Windows.Forms.TextBox();
            this.btnSeleccionarCarpeta = new System.Windows.Forms.Button();
            this.lblCarpeta = new System.Windows.Forms.Label();
            this.pnlCentral = new System.Windows.Forms.Panel();
            this.dgvArchivos = new System.Windows.Forms.DataGridView();
            this.colSeleccionar = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colNombre = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRuta = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTamano = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFechaModificacion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEstado = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlInferior = new System.Windows.Forms.Panel();
            this.lblEstadisticas = new System.Windows.Forms.Label();
            this.btnEliminar = new System.Windows.Forms.Button();
            this.btnVerDetalles = new System.Windows.Forms.Button();
            this.btnLimpiar = new System.Windows.Forms.Button();
            this.btnIndexar = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblProgreso = new System.Windows.Forms.Label();
            this.pnlSuperior.SuspendLayout();
            this.pnlCentral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvArchivos)).BeginInit();
            this.pnlInferior.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlSuperior
            // 
            this.pnlSuperior.Controls.Add(this.btnEscanear);
            this.pnlSuperior.Controls.Add(this.chkIncluirSubcarpetas);
            this.pnlSuperior.Controls.Add(this.txtRutaCarpeta);
            this.pnlSuperior.Controls.Add(this.btnSeleccionarCarpeta);
            this.pnlSuperior.Controls.Add(this.lblCarpeta);
            this.pnlSuperior.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSuperior.Location = new System.Drawing.Point(0, 0);
            this.pnlSuperior.Name = "pnlSuperior";
            this.pnlSuperior.Padding = new System.Windows.Forms.Padding(10);
            this.pnlSuperior.Size = new System.Drawing.Size(1000, 80);
            this.pnlSuperior.TabIndex = 0;
            // 
            // btnEscanear
            // 
            this.btnEscanear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEscanear.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnEscanear.Location = new System.Drawing.Point(862, 35);
            this.btnEscanear.Name = "btnEscanear";
            this.btnEscanear.Size = new System.Drawing.Size(120, 30);
            this.btnEscanear.TabIndex = 4;
            this.btnEscanear.Text = "🔍 Escanear";
            this.btnEscanear.UseVisualStyleBackColor = true;
            this.btnEscanear.Click += new System.EventHandler(this.btnEscanear_Click);
            // 
            // chkIncluirSubcarpetas
            // 
            this.chkIncluirSubcarpetas.AutoSize = true;
            this.chkIncluirSubcarpetas.Checked = true;
            this.chkIncluirSubcarpetas.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIncluirSubcarpetas.Location = new System.Drawing.Point(13, 55);
            this.chkIncluirSubcarpetas.Name = "chkIncluirSubcarpetas";
            this.chkIncluirSubcarpetas.Size = new System.Drawing.Size(135, 19);
            this.chkIncluirSubcarpetas.TabIndex = 3;
            this.chkIncluirSubcarpetas.Text = "Incluir subcarpetas";
            this.chkIncluirSubcarpetas.UseVisualStyleBackColor = true;
            // 
            // txtRutaCarpeta
            // 
            this.txtRutaCarpeta.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRutaCarpeta.Location = new System.Drawing.Point(90, 25);
            this.txtRutaCarpeta.Name = "txtRutaCarpeta";
            this.txtRutaCarpeta.ReadOnly = true;
            this.txtRutaCarpeta.Size = new System.Drawing.Size(650, 23);
            this.txtRutaCarpeta.TabIndex = 2;
            // 
            // btnSeleccionarCarpeta
            // 
            this.btnSeleccionarCarpeta.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSeleccionarCarpeta.Location = new System.Drawing.Point(750, 24);
            this.btnSeleccionarCarpeta.Name = "btnSeleccionarCarpeta";
            this.btnSeleccionarCarpeta.Size = new System.Drawing.Size(100, 25);
            this.btnSeleccionarCarpeta.TabIndex = 1;
            this.btnSeleccionarCarpeta.Text = "📁 Seleccionar...";
            this.btnSeleccionarCarpeta.UseVisualStyleBackColor = true;
            this.btnSeleccionarCarpeta.Click += new System.EventHandler(this.btnSeleccionarCarpeta_Click);
            // 
            // lblCarpeta
            // 
            this.lblCarpeta.AutoSize = true;
            this.lblCarpeta.Location = new System.Drawing.Point(13, 28);
            this.lblCarpeta.Name = "lblCarpeta";
            this.lblCarpeta.Size = new System.Drawing.Size(54, 15);
            this.lblCarpeta.TabIndex = 0;
            this.lblCarpeta.Text = "Carpeta:";
            // 
            // pnlCentral
            // 
            this.pnlCentral.Controls.Add(this.dgvArchivos);
            this.pnlCentral.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCentral.Location = new System.Drawing.Point(0, 80);
            this.pnlCentral.Name = "pnlCentral";
            this.pnlCentral.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.pnlCentral.Size = new System.Drawing.Size(1000, 420);
            this.pnlCentral.TabIndex = 1;
            // 
            // dgvArchivos
            // 
            this.dgvArchivos.AllowUserToAddRows = false;
            this.dgvArchivos.AllowUserToDeleteRows = false;
            this.dgvArchivos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvArchivos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvArchivos.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSeleccionar,
            this.colNombre,
            this.colRuta,
            this.colTamano,
            this.colFechaModificacion,
            this.colEstado});
            this.dgvArchivos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvArchivos.Location = new System.Drawing.Point(10, 0);
            this.dgvArchivos.Name = "dgvArchivos";
            this.dgvArchivos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvArchivos.Size = new System.Drawing.Size(980, 420);
            this.dgvArchivos.TabIndex = 0;
            // 
            // colSeleccionar
            // 
            this.colSeleccionar.FillWeight = 10F;
            this.colSeleccionar.HeaderText = "✓";
            this.colSeleccionar.Name = "colSeleccionar";
            this.colSeleccionar.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colSeleccionar.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // colNombre
            // 
            this.colNombre.FillWeight = 25F;
            this.colNombre.HeaderText = "Nombre Archivo";
            this.colNombre.Name = "colNombre";
            this.colNombre.ReadOnly = true;
            // 
            // colRuta
            // 
            this.colRuta.FillWeight = 35F;
            this.colRuta.HeaderText = "Ruta";
            this.colRuta.Name = "colRuta";
            this.colRuta.ReadOnly = true;
            // 
            // colTamano
            // 
            this.colTamano.FillWeight = 10F;
            this.colTamano.HeaderText = "Tamaño";
            this.colTamano.Name = "colTamano";
            this.colTamano.ReadOnly = true;
            // 
            // colFechaModificacion
            // 
            this.colFechaModificacion.FillWeight = 15F;
            this.colFechaModificacion.HeaderText = "Fecha Modificación";
            this.colFechaModificacion.Name = "colFechaModificacion";
            this.colFechaModificacion.ReadOnly = true;
            // 
            // colEstado
            // 
            this.colEstado.FillWeight = 15F;
            this.colEstado.HeaderText = "Estado";
            this.colEstado.Name = "colEstado";
            this.colEstado.ReadOnly = true;
            // 
            // pnlInferior
            // 
            this.pnlInferior.Controls.Add(this.lblProgreso);
            this.pnlInferior.Controls.Add(this.progressBar);
            this.pnlInferior.Controls.Add(this.lblEstadisticas);
            this.pnlInferior.Controls.Add(this.btnEliminar);
            this.pnlInferior.Controls.Add(this.btnVerDetalles);
            this.pnlInferior.Controls.Add(this.btnLimpiar);
            this.pnlInferior.Controls.Add(this.btnIndexar);
            this.pnlInferior.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlInferior.Location = new System.Drawing.Point(0, 500);
            this.pnlInferior.Name = "pnlInferior";
            this.pnlInferior.Padding = new System.Windows.Forms.Padding(10);
            this.pnlInferior.Size = new System.Drawing.Size(1000, 100);
            this.pnlInferior.TabIndex = 2;
            // 
            // lblEstadisticas
            // 
            this.lblEstadisticas.AutoSize = true;
            this.lblEstadisticas.Location = new System.Drawing.Point(13, 73);
            this.lblEstadisticas.Name = "lblEstadisticas";
            this.lblEstadisticas.Size = new System.Drawing.Size(150, 15);
            this.lblEstadisticas.TabIndex = 4;
            this.lblEstadisticas.Text = "Total: 0 | Seleccionados: 0";
            // 
            // btnEliminar
            // 
            this.btnEliminar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEliminar.Location = new System.Drawing.Point(742, 13);
            this.btnEliminar.Name = "btnEliminar";
            this.btnEliminar.Size = new System.Drawing.Size(120, 35);
            this.btnEliminar.TabIndex = 3;
            this.btnEliminar.Text = "🗑️ Eliminar";
            this.btnEliminar.UseVisualStyleBackColor = true;
            this.btnEliminar.Click += new System.EventHandler(this.btnEliminar_Click);
            // 
            // btnVerDetalles
            // 
            this.btnVerDetalles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnVerDetalles.Location = new System.Drawing.Point(616, 13);
            this.btnVerDetalles.Name = "btnVerDetalles";
            this.btnVerDetalles.Size = new System.Drawing.Size(120, 35);
            this.btnVerDetalles.TabIndex = 2;
            this.btnVerDetalles.Text = "ℹ️ Ver Detalles";
            this.btnVerDetalles.UseVisualStyleBackColor = true;
            this.btnVerDetalles.Click += new System.EventHandler(this.btnVerDetalles_Click);
            // 
            // btnLimpiar
            // 
            this.btnLimpiar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLimpiar.Location = new System.Drawing.Point(490, 13);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Size = new System.Drawing.Size(120, 35);
            this.btnLimpiar.TabIndex = 1;
            this.btnLimpiar.Text = "🧹 Limpiar Lista";
            this.btnLimpiar.UseVisualStyleBackColor = true;
            this.btnLimpiar.Click += new System.EventHandler(this.btnLimpiar_Click);
            // 
            // btnIndexar
            // 
            this.btnIndexar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnIndexar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnIndexar.Location = new System.Drawing.Point(868, 13);
            this.btnIndexar.Name = "btnIndexar";
            this.btnIndexar.Size = new System.Drawing.Size(120, 35);
            this.btnIndexar.TabIndex = 0;
            this.btnIndexar.Text = "💾 Ingresar Archivos";
            this.btnIndexar.UseVisualStyleBackColor = true;
            this.btnIndexar.Click += new System.EventHandler(this.btnIndexar_Click);
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(13, 51);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(975, 15);
            this.progressBar.TabIndex = 5;
            this.progressBar.Visible = false;
            // 
            // lblProgreso
            // 
            this.lblProgreso.AutoSize = true;
            this.lblProgreso.Location = new System.Drawing.Point(13, 30);
            this.lblProgreso.Name = "lblProgreso";
            this.lblProgreso.Size = new System.Drawing.Size(0, 15);
            this.lblProgreso.TabIndex = 6;
            // 
            // FrmIndexacion
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Controls.Add(this.pnlCentral);
            this.Controls.Add(this.pnlInferior);
            this.Controls.Add(this.pnlSuperior);
            this.Name = "FrmIngresoPlanos";
            this.Text = "FASE 1: Ingreso de Planos (Archivos PDF)";
            this.Load += new System.EventHandler(this.FrmIngresoPlanos_Load);
            this.pnlSuperior.ResumeLayout(false);
            this.pnlSuperior.PerformLayout();
            this.pnlCentral.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvArchivos)).EndInit();
            this.pnlInferior.ResumeLayout(false);
            this.pnlInferior.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlSuperior;
        private System.Windows.Forms.Label lblCarpeta;
        private System.Windows.Forms.Button btnSeleccionarCarpeta;
        private System.Windows.Forms.TextBox txtRutaCarpeta;
        private System.Windows.Forms.CheckBox chkIncluirSubcarpetas;
        private System.Windows.Forms.Button btnEscanear;
        private System.Windows.Forms.Panel pnlCentral;
        private System.Windows.Forms.DataGridView dgvArchivos;
        private System.Windows.Forms.Panel pnlInferior;
        private System.Windows.Forms.Button btnIndexar;
        private System.Windows.Forms.Button btnLimpiar;
        private System.Windows.Forms.Button btnVerDetalles;
        private System.Windows.Forms.Button btnEliminar;
        private System.Windows.Forms.Label lblEstadisticas;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colSeleccionar;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNombre;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRuta;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTamano;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFechaModificacion;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEstado;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblProgreso;
    }
}
