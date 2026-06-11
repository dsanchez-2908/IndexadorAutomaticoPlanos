using System.Windows.Forms;

namespace IndexadorAutomaticoPlanos.UI
{
    partial class FrmFinalizarLotes
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
            this.pnlContenido = new System.Windows.Forms.Panel();
            this.dgvLotes = new System.Windows.Forms.DataGridView();
            this.pnlInstrucciones = new System.Windows.Forms.Panel();
            this.lblInstrucciones = new System.Windows.Forms.Label();
            this.pnlProgreso = new System.Windows.Forms.Panel();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblProgreso = new System.Windows.Forms.Label();
            this.pnlBotones = new System.Windows.Forms.Panel();
            this.btnFinalizarSeleccionados = new System.Windows.Forms.Button();
            this.btnActualizar = new System.Windows.Forms.Button();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.pnlTitulo.SuspendLayout();
            this.pnlContenido.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLotes)).BeginInit();
            this.pnlInstrucciones.SuspendLayout();
            this.pnlProgreso.SuspendLayout();
            this.pnlBotones.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTitulo
            // 
            this.pnlTitulo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.pnlTitulo.Controls.Add(this.lblTitulo);
            this.pnlTitulo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitulo.Location = new System.Drawing.Point(0, 0);
            this.pnlTitulo.Name = "pnlTitulo";
            this.pnlTitulo.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);
            this.pnlTitulo.Size = new System.Drawing.Size(1200, 60);
            this.pnlTitulo.TabIndex = 0;
            // 
            // lblTitulo
            // 
            this.lblTitulo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(15, 10);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(1170, 40);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Finalizar Lotes - Renombrado de Archivos y Generación de Índice";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlContenido
            // 
            this.pnlContenido.Controls.Add(this.dgvLotes);
            this.pnlContenido.Controls.Add(this.pnlInstrucciones);
            this.pnlContenido.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContenido.Location = new System.Drawing.Point(0, 60);
            this.pnlContenido.Name = "pnlContenido";
            this.pnlContenido.Padding = new System.Windows.Forms.Padding(10);
            this.pnlContenido.Size = new System.Drawing.Size(1200, 540);
            this.pnlContenido.TabIndex = 1;
            // 
            // dgvLotes
            // 
            this.dgvLotes.AllowUserToAddRows = false;
            this.dgvLotes.AllowUserToDeleteRows = false;
            this.dgvLotes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvLotes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvLotes.Location = new System.Drawing.Point(10, 70);
            this.dgvLotes.Name = "dgvLotes";
            this.dgvLotes.ReadOnly = true;
            this.dgvLotes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvLotes.Size = new System.Drawing.Size(1180, 460);
            this.dgvLotes.TabIndex = 0;
            // 
            // pnlInstrucciones
            // 
            this.pnlInstrucciones.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(250)))), ((int)(((byte)(205)))));
            this.pnlInstrucciones.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlInstrucciones.Controls.Add(this.lblInstrucciones);
            this.pnlInstrucciones.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlInstrucciones.Location = new System.Drawing.Point(10, 10);
            this.pnlInstrucciones.Name = "pnlInstrucciones";
            this.pnlInstrucciones.Padding = new System.Windows.Forms.Padding(10);
            this.pnlInstrucciones.Size = new System.Drawing.Size(1180, 60);
            this.pnlInstrucciones.TabIndex = 1;
            // 
            // lblInstrucciones
            // 
            this.lblInstrucciones.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblInstrucciones.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblInstrucciones.Location = new System.Drawing.Point(10, 10);
            this.lblInstrucciones.Name = "lblInstrucciones";
            this.lblInstrucciones.Size = new System.Drawing.Size(1158, 38);
            this.lblInstrucciones.TabIndex = 0;
            this.lblInstrucciones.Text = "Seleccione uno o más lotes pendientes de finalización. El proceso renombrará los" +
    " archivos PDF según la nomenclatura definida, generará el archivo INDEX.csv y c" +
    "ambiará el estado a \"Finalizado\".";
            this.lblInstrucciones.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlProgreso
            // 
            this.pnlProgreso.Controls.Add(this.progressBar);
            this.pnlProgreso.Controls.Add(this.lblProgreso);
            this.pnlProgreso.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlProgreso.Location = new System.Drawing.Point(0, 600);
            this.pnlProgreso.Name = "pnlProgreso";
            this.pnlProgreso.Padding = new System.Windows.Forms.Padding(10);
            this.pnlProgreso.Size = new System.Drawing.Size(1200, 60);
            this.pnlProgreso.TabIndex = 2;
            this.pnlProgreso.Visible = false;
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBar.Location = new System.Drawing.Point(10, 35);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(1180, 15);
            this.progressBar.TabIndex = 1;
            // 
            // lblProgreso
            // 
            this.lblProgreso.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblProgreso.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblProgreso.Location = new System.Drawing.Point(10, 10);
            this.lblProgreso.Name = "lblProgreso";
            this.lblProgreso.Size = new System.Drawing.Size(1180, 25);
            this.lblProgreso.TabIndex = 0;
            this.lblProgreso.Text = "Procesando...";
            this.lblProgreso.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlBotones
            // 
            this.pnlBotones.Controls.Add(this.btnFinalizarSeleccionados);
            this.pnlBotones.Controls.Add(this.btnActualizar);
            this.pnlBotones.Controls.Add(this.btnCerrar);
            this.pnlBotones.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBotones.Location = new System.Drawing.Point(0, 660);
            this.pnlBotones.Name = "pnlBotones";
            this.pnlBotones.Padding = new System.Windows.Forms.Padding(10);
            this.pnlBotones.Size = new System.Drawing.Size(1200, 60);
            this.pnlBotones.TabIndex = 3;
            // 
            // btnFinalizarSeleccionados
            // 
            this.btnFinalizarSeleccionados.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(175)))), ((int)(((byte)(80)))));
            this.btnFinalizarSeleccionados.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFinalizarSeleccionados.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnFinalizarSeleccionados.ForeColor = System.Drawing.Color.White;
            this.btnFinalizarSeleccionados.Location = new System.Drawing.Point(10, 10);
            this.btnFinalizarSeleccionados.Name = "btnFinalizarSeleccionados";
            this.btnFinalizarSeleccionados.Size = new System.Drawing.Size(250, 40);
            this.btnFinalizarSeleccionados.TabIndex = 0;
            this.btnFinalizarSeleccionados.Text = "Finalizar Lotes Seleccionados";
            this.btnFinalizarSeleccionados.UseVisualStyleBackColor = false;
            this.btnFinalizarSeleccionados.Click += new System.EventHandler(this.btnFinalizarSeleccionados_Click);
            // 
            // btnActualizar
            // 
            this.btnActualizar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnActualizar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnActualizar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnActualizar.ForeColor = System.Drawing.Color.White;
            this.btnActualizar.Location = new System.Drawing.Point(270, 10);
            this.btnActualizar.Name = "btnActualizar";
            this.btnActualizar.Size = new System.Drawing.Size(150, 40);
            this.btnActualizar.TabIndex = 1;
            this.btnActualizar.Text = "Actualizar";
            this.btnActualizar.UseVisualStyleBackColor = false;
            this.btnActualizar.Click += new System.EventHandler(this.btnActualizar_Click);
            // 
            // btnCerrar
            // 
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.btnCerrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCerrar.ForeColor = System.Drawing.Color.White;
            this.btnCerrar.Location = new System.Drawing.Point(1040, 10);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(150, 40);
            this.btnCerrar.TabIndex = 2;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = false;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            // 
            // FrmFinalizarLotes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 720);
            this.Controls.Add(this.pnlContenido);
            this.Controls.Add(this.pnlProgreso);
            this.Controls.Add(this.pnlBotones);
            this.Controls.Add(this.pnlTitulo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = false;
            this.Name = "FrmFinalizarLotes";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Finalizar Lotes";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FrmFinalizarLotes_Load);
            this.pnlTitulo.ResumeLayout(false);
            this.pnlContenido.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLotes)).EndInit();
            this.pnlInstrucciones.ResumeLayout(false);
            this.pnlProgreso.ResumeLayout(false);
            this.pnlBotones.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlTitulo;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel pnlContenido;
        private System.Windows.Forms.DataGridView dgvLotes;
        private System.Windows.Forms.Panel pnlInstrucciones;
        private System.Windows.Forms.Label lblInstrucciones;
        private System.Windows.Forms.Panel pnlProgreso;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblProgreso;
        private System.Windows.Forms.Panel pnlBotones;
        private System.Windows.Forms.Button btnFinalizarSeleccionados;
        private System.Windows.Forms.Button btnActualizar;
        private System.Windows.Forms.Button btnCerrar;
    }
}
