namespace IndexadorAutomaticoPlanos.UI
{
    partial class FrmCambiarClave
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
            this.panelPrincipal = new System.Windows.Forms.Panel();
            this.lblInfo = new System.Windows.Forms.Label();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.chkMostrarClaves = new System.Windows.Forms.CheckBox();
            this.txtConfirmarClave = new System.Windows.Forms.TextBox();
            this.txtNuevaClave = new System.Windows.Forms.TextBox();
            this.txtClaveActual = new System.Windows.Forms.TextBox();
            this.lblConfirmarClave = new System.Windows.Forms.Label();
            this.lblNuevaClave = new System.Windows.Forms.Label();
            this.lblClaveActual = new System.Windows.Forms.Label();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.panelPrincipal.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelPrincipal
            // 
            this.panelPrincipal.BackColor = System.Drawing.Color.White;
            this.panelPrincipal.Controls.Add(this.lblInfo);
            this.panelPrincipal.Controls.Add(this.btnCancelar);
            this.panelPrincipal.Controls.Add(this.btnGuardar);
            this.panelPrincipal.Controls.Add(this.chkMostrarClaves);
            this.panelPrincipal.Controls.Add(this.txtConfirmarClave);
            this.panelPrincipal.Controls.Add(this.txtNuevaClave);
            this.panelPrincipal.Controls.Add(this.txtClaveActual);
            this.panelPrincipal.Controls.Add(this.lblConfirmarClave);
            this.panelPrincipal.Controls.Add(this.lblNuevaClave);
            this.panelPrincipal.Controls.Add(this.lblClaveActual);
            this.panelPrincipal.Controls.Add(this.lblTitulo);
            this.panelPrincipal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPrincipal.Location = new System.Drawing.Point(0, 0);
            this.panelPrincipal.Name = "panelPrincipal";
            this.panelPrincipal.Size = new System.Drawing.Size(500, 400);
            this.panelPrincipal.TabIndex = 0;
            // 
            // lblInfo
            // 
            this.lblInfo.ForeColor = System.Drawing.Color.Gray;
            this.lblInfo.Location = new System.Drawing.Point(50, 280);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(400, 40);
            this.lblInfo.TabIndex = 0;
            this.lblInfo.Text = "La contraseña debe tener al menos 6 caracteres.";
            // 
            // btnCancelar
            // 
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelar.Location = new System.Drawing.Point(260, 340);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(120, 35);
            this.btnCancelar.TabIndex = 6;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // btnGuardar
            // 
            this.btnGuardar.Location = new System.Drawing.Point(120, 340);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(120, 35);
            this.btnGuardar.TabIndex = 5;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // chkMostrarClaves
            // 
            this.chkMostrarClaves.AutoSize = true;
            this.chkMostrarClaves.Location = new System.Drawing.Point(50, 250);
            this.chkMostrarClaves.Name = "chkMostrarClaves";
            this.chkMostrarClaves.Size = new System.Drawing.Size(150, 19);
            this.chkMostrarClaves.TabIndex = 4;
            this.chkMostrarClaves.Text = "Mostrar contraseñas";
            this.chkMostrarClaves.UseVisualStyleBackColor = true;
            this.chkMostrarClaves.CheckedChanged += new System.EventHandler(this.chkMostrarClaves_CheckedChanged);
            // 
            // txtConfirmarClave
            // 
            this.txtConfirmarClave.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtConfirmarClave.Location = new System.Drawing.Point(50, 210);
            this.txtConfirmarClave.Name = "txtConfirmarClave";
            this.txtConfirmarClave.Size = new System.Drawing.Size(400, 27);
            this.txtConfirmarClave.TabIndex = 3;
            this.txtConfirmarClave.UseSystemPasswordChar = true;
            // 
            // txtNuevaClave
            // 
            this.txtNuevaClave.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtNuevaClave.Location = new System.Drawing.Point(50, 155);
            this.txtNuevaClave.Name = "txtNuevaClave";
            this.txtNuevaClave.Size = new System.Drawing.Size(400, 27);
            this.txtNuevaClave.TabIndex = 2;
            this.txtNuevaClave.UseSystemPasswordChar = true;
            // 
            // txtClaveActual
            // 
            this.txtClaveActual.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtClaveActual.Location = new System.Drawing.Point(50, 100);
            this.txtClaveActual.Name = "txtClaveActual";
            this.txtClaveActual.Size = new System.Drawing.Size(400, 27);
            this.txtClaveActual.TabIndex = 1;
            this.txtClaveActual.UseSystemPasswordChar = true;
            // 
            // lblConfirmarClave
            // 
            this.lblConfirmarClave.AutoSize = true;
            this.lblConfirmarClave.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblConfirmarClave.Location = new System.Drawing.Point(50, 188);
            this.lblConfirmarClave.Name = "lblConfirmarClave";
            this.lblConfirmarClave.Size = new System.Drawing.Size(156, 19);
            this.lblConfirmarClave.TabIndex = 0;
            this.lblConfirmarClave.Text = "Confirmar nueva clave:";
            // 
            // lblNuevaClave
            // 
            this.lblNuevaClave.AutoSize = true;
            this.lblNuevaClave.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblNuevaClave.Location = new System.Drawing.Point(50, 133);
            this.lblNuevaClave.Name = "lblNuevaClave";
            this.lblNuevaClave.Size = new System.Drawing.Size(92, 19);
            this.lblNuevaClave.TabIndex = 0;
            this.lblNuevaClave.Text = "Nueva clave:";
            // 
            // lblClaveActual
            // 
            this.lblClaveActual.AutoSize = true;
            this.lblClaveActual.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblClaveActual.Location = new System.Drawing.Point(50, 78);
            this.lblClaveActual.Name = "lblClaveActual";
            this.lblClaveActual.Size = new System.Drawing.Size(89, 19);
            this.lblClaveActual.TabIndex = 0;
            this.lblClaveActual.Text = "Clave actual:";
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.lblTitulo.Location = new System.Drawing.Point(130, 20);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(241, 30);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Cambiar Contraseña";
            // 
            // FrmCambiarClave
            // 
            this.AcceptButton = this.btnGuardar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(500, 400);
            this.Controls.Add(this.panelPrincipal);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmCambiarClave";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cambiar Contraseña";
            this.panelPrincipal.ResumeLayout(false);
            this.panelPrincipal.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelPrincipal;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblClaveActual;
        private System.Windows.Forms.Label lblNuevaClave;
        private System.Windows.Forms.Label lblConfirmarClave;
        private System.Windows.Forms.TextBox txtClaveActual;
        private System.Windows.Forms.TextBox txtNuevaClave;
        private System.Windows.Forms.TextBox txtConfirmarClave;
        private System.Windows.Forms.CheckBox chkMostrarClaves;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Label lblInfo;
    }
}
