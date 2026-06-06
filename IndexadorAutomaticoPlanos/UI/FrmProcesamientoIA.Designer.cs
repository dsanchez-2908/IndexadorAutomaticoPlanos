namespace IndexadorAutomaticoPlanos.UI
{
    partial class FrmProcesamientoIA
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
            this.pnlTitulo = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.pnlIzquierdo = new System.Windows.Forms.Panel();
            this.dgvLotes = new System.Windows.Forms.DataGridView();
            this.lblLotesPendientes = new System.Windows.Forms.Label();
            this.pnlConfiguracion = new System.Windows.Forms.Panel();
            this.btnGuardarPrompts = new System.Windows.Forms.Button();
            this.chkUsarOcr = new System.Windows.Forms.CheckBox();
            this.lblPromptImagen = new System.Windows.Forms.Label();
            this.txtPromptImagen = new System.Windows.Forms.TextBox();
            this.lblPromptOcr = new System.Windows.Forms.Label();
            this.txtPromptOcr = new System.Windows.Forms.TextBox();
            this.lblConfiguracion = new System.Windows.Forms.Label();
            this.pnlInferior = new System.Windows.Forms.Panel();
            this.lblEstado = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.btnRefrescar = new System.Windows.Forms.Button();
            this.btnProcesar = new System.Windows.Forms.Button();
            this.pnlTitulo.SuspendLayout();
            this.pnlIzquierdo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLotes)).BeginInit();
            this.pnlConfiguracion.SuspendLayout();
            this.pnlInferior.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTitulo
            // 
            this.pnlTitulo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.pnlTitulo.Controls.Add(this.lblTitulo);
            this.pnlTitulo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitulo.Location = new System.Drawing.Point(0, 0);
            this.pnlTitulo.Name = "pnlTitulo";
            this.pnlTitulo.Size = new System.Drawing.Size(1200, 60);
            this.pnlTitulo.TabIndex = 0;
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(15, 13);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(340, 32);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Procesamiento por OpenAI";
            // 
            // pnlIzquierdo
            // 
            this.pnlIzquierdo.Controls.Add(this.dgvLotes);
            this.pnlIzquierdo.Controls.Add(this.lblLotesPendientes);
            this.pnlIzquierdo.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlIzquierdo.Location = new System.Drawing.Point(0, 60);
            this.pnlIzquierdo.Name = "pnlIzquierdo";
            this.pnlIzquierdo.Padding = new System.Windows.Forms.Padding(10);
            this.pnlIzquierdo.Size = new System.Drawing.Size(400, 640);
            this.pnlIzquierdo.TabIndex = 1;
            // 
            // dgvLotes
            // 
            this.dgvLotes.AllowUserToAddRows = false;
            this.dgvLotes.AllowUserToDeleteRows = false;
            this.dgvLotes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvLotes.BackgroundColor = System.Drawing.Color.White;
            this.dgvLotes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvLotes.Location = new System.Drawing.Point(10, 33);
            this.dgvLotes.Name = "dgvLotes";
            this.dgvLotes.RowHeadersWidth = 51;
            this.dgvLotes.RowTemplate.Height = 25;
            this.dgvLotes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvLotes.Size = new System.Drawing.Size(380, 597);
            this.dgvLotes.TabIndex = 1;
            // 
            // lblLotesPendientes
            // 
            this.lblLotesPendientes.AutoSize = true;
            this.lblLotesPendientes.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblLotesPendientes.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblLotesPendientes.Location = new System.Drawing.Point(10, 10);
            this.lblLotesPendientes.Name = "lblLotesPendientes";
            this.lblLotesPendientes.Size = new System.Drawing.Size(286, 23);
            this.lblLotesPendientes.TabIndex = 0;
            this.lblLotesPendientes.Text = "Lotes Pendientes de Procesar por IA";
            // 
            // pnlConfiguracion
            // 
            this.pnlConfiguracion.Controls.Add(this.btnGuardarPrompts);
            this.pnlConfiguracion.Controls.Add(this.chkUsarOcr);
            this.pnlConfiguracion.Controls.Add(this.lblPromptImagen);
            this.pnlConfiguracion.Controls.Add(this.txtPromptImagen);
            this.pnlConfiguracion.Controls.Add(this.lblPromptOcr);
            this.pnlConfiguracion.Controls.Add(this.txtPromptOcr);
            this.pnlConfiguracion.Controls.Add(this.lblConfiguracion);
            this.pnlConfiguracion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlConfiguracion.Location = new System.Drawing.Point(400, 60);
            this.pnlConfiguracion.Name = "pnlConfiguracion";
            this.pnlConfiguracion.Padding = new System.Windows.Forms.Padding(10);
            this.pnlConfiguracion.Size = new System.Drawing.Size(800, 640);
            this.pnlConfiguracion.TabIndex = 2;
            // 
            // btnGuardarPrompts
            // 
            this.btnGuardarPrompts.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnGuardarPrompts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardarPrompts.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnGuardarPrompts.ForeColor = System.Drawing.Color.White;
            this.btnGuardarPrompts.Location = new System.Drawing.Point(620, 587);
            this.btnGuardarPrompts.Name = "btnGuardarPrompts";
            this.btnGuardarPrompts.Size = new System.Drawing.Size(160, 35);
            this.btnGuardarPrompts.TabIndex = 6;
            this.btnGuardarPrompts.Text = "Guardar Prompts";
            this.btnGuardarPrompts.UseVisualStyleBackColor = false;
            this.btnGuardarPrompts.Click += new System.EventHandler(this.btnGuardarPrompts_Click);
            // 
            // chkUsarOcr
            // 
            this.chkUsarOcr.AutoSize = true;
            this.chkUsarOcr.Checked = true;
            this.chkUsarOcr.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUsarOcr.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.chkUsarOcr.Location = new System.Drawing.Point(20, 593);
            this.chkUsarOcr.Name = "chkUsarOcr";
            this.chkUsarOcr.Size = new System.Drawing.Size(330, 27);
            this.chkUsarOcr.TabIndex = 5;
            this.chkUsarOcr.Text = "Usar Modalidad Híbrida (OCR + Imagen)";
            this.chkUsarOcr.UseVisualStyleBackColor = true;
            // 
            // lblPromptImagen
            // 
            this.lblPromptImagen.AutoSize = true;
            this.lblPromptImagen.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPromptImagen.Location = new System.Drawing.Point(20, 320);
            this.lblPromptImagen.Name = "lblPromptImagen";
            this.lblPromptImagen.Size = new System.Drawing.Size(251, 20);
            this.lblPromptImagen.TabIndex = 4;
            this.lblPromptImagen.Text = "Prompt para Procesamiento Imagen:";
            // 
            // txtPromptImagen
            // 
            this.txtPromptImagen.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtPromptImagen.Location = new System.Drawing.Point(20, 343);
            this.txtPromptImagen.Multiline = true;
            this.txtPromptImagen.Name = "txtPromptImagen";
            this.txtPromptImagen.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtPromptImagen.Size = new System.Drawing.Size(760, 230);
            this.txtPromptImagen.TabIndex = 3;
            // 
            // lblPromptOcr
            // 
            this.lblPromptOcr.AutoSize = true;
            this.lblPromptOcr.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPromptOcr.Location = new System.Drawing.Point(20, 50);
            this.lblPromptOcr.Name = "lblPromptOcr";
            this.lblPromptOcr.Size = new System.Drawing.Size(226, 20);
            this.lblPromptOcr.TabIndex = 2;
            this.lblPromptOcr.Text = "Prompt para Procesamiento OCR:";
            // 
            // txtPromptOcr
            // 
            this.txtPromptOcr.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtPromptOcr.Location = new System.Drawing.Point(20, 73);
            this.txtPromptOcr.Multiline = true;
            this.txtPromptOcr.Name = "txtPromptOcr";
            this.txtPromptOcr.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtPromptOcr.Size = new System.Drawing.Size(760, 230);
            this.txtPromptOcr.TabIndex = 1;
            // 
            // lblConfiguracion
            // 
            this.lblConfiguracion.AutoSize = true;
            this.lblConfiguracion.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblConfiguracion.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblConfiguracion.Location = new System.Drawing.Point(10, 10);
            this.lblConfiguracion.Name = "lblConfiguracion";
            this.lblConfiguracion.Size = new System.Drawing.Size(212, 23);
            this.lblConfiguracion.TabIndex = 0;
            this.lblConfiguracion.Text = "Configuración de Prompts";
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
            this.pnlInferior.Size = new System.Drawing.Size(1200, 100);
            this.pnlInferior.TabIndex = 3;
            // 
            // lblEstado
            // 
            this.lblEstado.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblEstado.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEstado.Location = new System.Drawing.Point(13, 13);
            this.lblEstado.Name = "lblEstado";
            this.lblEstado.Size = new System.Drawing.Size(1174, 20);
            this.lblEstado.TabIndex = 3;
            this.lblEstado.Text = "Listo para procesar lotes";
            this.lblEstado.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(13, 36);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(1174, 23);
            this.progressBar.TabIndex = 2;
            // 
            // btnRefrescar
            // 
            this.btnRefrescar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefrescar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.btnRefrescar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefrescar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnRefrescar.ForeColor = System.Drawing.Color.White;
            this.btnRefrescar.Location = new System.Drawing.Point(842, 65);
            this.btnRefrescar.Name = "btnRefrescar";
            this.btnRefrescar.Size = new System.Drawing.Size(160, 25);
            this.btnRefrescar.TabIndex = 1;
            this.btnRefrescar.Text = "Refrescar";
            this.btnRefrescar.UseVisualStyleBackColor = false;
            this.btnRefrescar.Click += new System.EventHandler(this.btnRefrescar_Click);
            // 
            // btnProcesar
            // 
            this.btnProcesar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnProcesar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnProcesar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProcesar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnProcesar.ForeColor = System.Drawing.Color.White;
            this.btnProcesar.Location = new System.Drawing.Point(1008, 65);
            this.btnProcesar.Name = "btnProcesar";
            this.btnProcesar.Size = new System.Drawing.Size(179, 25);
            this.btnProcesar.TabIndex = 0;
            this.btnProcesar.Text = "Procesar Seleccionados";
            this.btnProcesar.UseVisualStyleBackColor = false;
            this.btnProcesar.Click += new System.EventHandler(this.btnProcesar_Click);
            // 
            // FrmProcesamientoIA
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Controls.Add(this.pnlConfiguracion);
            this.Controls.Add(this.pnlIzquierdo);
            this.Controls.Add(this.pnlInferior);
            this.Controls.Add(this.pnlTitulo);
            this.Name = "FrmProcesamientoIA";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Procesamiento por OpenAI";
            this.Load += new System.EventHandler(this.FrmProcesamientoIA_Load);
            this.pnlTitulo.ResumeLayout(false);
            this.pnlTitulo.PerformLayout();
            this.pnlIzquierdo.ResumeLayout(false);
            this.pnlIzquierdo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLotes)).EndInit();
            this.pnlConfiguracion.ResumeLayout(false);
            this.pnlConfiguracion.PerformLayout();
            this.pnlInferior.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlTitulo;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel pnlIzquierdo;
        private System.Windows.Forms.DataGridView dgvLotes;
        private System.Windows.Forms.Label lblLotesPendientes;
        private System.Windows.Forms.Panel pnlConfiguracion;
        private System.Windows.Forms.Label lblConfiguracion;
        private System.Windows.Forms.TextBox txtPromptOcr;
        private System.Windows.Forms.Label lblPromptOcr;
        private System.Windows.Forms.TextBox txtPromptImagen;
        private System.Windows.Forms.Label lblPromptImagen;
        private System.Windows.Forms.CheckBox chkUsarOcr;
        private System.Windows.Forms.Button btnGuardarPrompts;
        private System.Windows.Forms.Panel pnlInferior;
        private System.Windows.Forms.Button btnProcesar;
        private System.Windows.Forms.Button btnRefrescar;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblEstado;
    }
}
