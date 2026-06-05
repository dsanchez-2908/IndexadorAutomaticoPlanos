namespace IndexadorAutomaticoPlanos.UI
{
    partial class FrmPrincipal
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
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.menuArchivo = new System.Windows.Forms.ToolStripMenuItem();
            this.menuCerrarSesion = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuSalir = new System.Windows.Forms.ToolStripMenuItem();
            this.menuProcesos = new System.Windows.Forms.ToolStripMenuItem();
            this.menuIngresoLotes = new System.Windows.Forms.ToolStripMenuItem();
            this.menuPreparacionLotes = new System.Windows.Forms.ToolStripMenuItem();
            this.menuPreparacionImagenes = new System.Windows.Forms.ToolStripMenuItem();
            this.menuProcesamientoIA = new System.Windows.Forms.ToolStripMenuItem();
            this.menuValidacionLotes = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFinalizarLotes = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAdministracion = new System.Windows.Forms.ToolStripMenuItem();
            this.menuUsuarios = new System.Windows.Forms.ToolStripMenuItem();
            this.menuParametros = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuCambiarClave = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAyuda = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAcercaDe = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabelUsuario = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusLabelFecha = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuArchivo,
            this.menuProcesos,
            this.menuAdministracion,
            this.menuAyuda});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1200, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip";
            // 
            // menuArchivo
            // 
            this.menuArchivo.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuCerrarSesion,
            this.toolStripSeparator1,
            this.menuSalir});
            this.menuArchivo.Name = "menuArchivo";
            this.menuArchivo.Size = new System.Drawing.Size(60, 20);
            this.menuArchivo.Text = "&Archivo";
            // 
            // menuCerrarSesion
            // 
            this.menuCerrarSesion.Name = "menuCerrarSesion";
            this.menuCerrarSesion.Size = new System.Drawing.Size(180, 22);
            this.menuCerrarSesion.Text = "Cerrar Sesión";
            this.menuCerrarSesion.Click += new System.EventHandler(this.menuCerrarSesion_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // menuSalir
            // 
            this.menuSalir.Name = "menuSalir";
            this.menuSalir.Size = new System.Drawing.Size(180, 22);
            this.menuSalir.Text = "&Salir";
            this.menuSalir.Click += new System.EventHandler(this.menuSalir_Click);
            // 
            // menuProcesos
            // 
            this.menuProcesos.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuIngresoLotes,
            this.menuPreparacionLotes,
            this.menuPreparacionImagenes,
            this.menuProcesamientoIA,
            this.menuValidacionLotes,
            this.menuFinalizarLotes});
            this.menuProcesos.Name = "menuProcesos";
            this.menuProcesos.Size = new System.Drawing.Size(66, 20);
            this.menuProcesos.Text = "&Procesos";
            // 
            // menuIngresoLotes
            // 
            this.menuIngresoLotes.Name = "menuIngresoLotes";
            this.menuIngresoLotes.Size = new System.Drawing.Size(230, 22);
            this.menuIngresoLotes.Text = "1. Ingreso de Planos";
            this.menuIngresoLotes.Click += new System.EventHandler(this.menuIngresoLotes_Click);
            // 
            // menuPreparacionLotes
            // 
            this.menuPreparacionLotes.Name = "menuPreparacionLotes";
            this.menuPreparacionLotes.Size = new System.Drawing.Size(215, 22);
            this.menuPreparacionLotes.Text = "2. Preparación de Lotes";
            this.menuPreparacionLotes.Click += new System.EventHandler(this.menuPreparacionLotes_Click);
            // 
            // menuPreparacionImagenes
            // 
            this.menuPreparacionImagenes.Name = "menuPreparacionImagenes";
            this.menuPreparacionImagenes.Size = new System.Drawing.Size(215, 22);
            this.menuPreparacionImagenes.Text = "3. Preparación de Imágenes";
            this.menuPreparacionImagenes.Click += new System.EventHandler(this.menuPreparacionImagenes_Click);
            // 
            // menuProcesamientoIA
            // 
            this.menuProcesamientoIA.Name = "menuProcesamientoIA";
            this.menuProcesamientoIA.Size = new System.Drawing.Size(215, 22);
            this.menuProcesamientoIA.Text = "4. Procesamiento OpenAI";
            // 
            // menuValidacionLotes
            // 
            this.menuValidacionLotes.Name = "menuValidacionLotes";
            this.menuValidacionLotes.Size = new System.Drawing.Size(215, 22);
            this.menuValidacionLotes.Text = "5. Validación de Lotes";
            // 
            // menuFinalizarLotes
            // 
            this.menuFinalizarLotes.Name = "menuFinalizarLotes";
            this.menuFinalizarLotes.Size = new System.Drawing.Size(215, 22);
            this.menuFinalizarLotes.Text = "6. Finalizar Lotes";
            // 
            // menuAdministracion
            // 
            this.menuAdministracion.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuUsuarios,
            this.menuParametros,
            this.toolStripSeparator2,
            this.menuCambiarClave});
            this.menuAdministracion.Name = "menuAdministracion";
            this.menuAdministracion.Size = new System.Drawing.Size(100, 20);
            this.menuAdministracion.Text = "&Administración";
            // 
            // menuUsuarios
            // 
            this.menuUsuarios.Name = "menuUsuarios";
            this.menuUsuarios.Size = new System.Drawing.Size(180, 22);
            this.menuUsuarios.Text = "Usuarios";
            // 
            // menuParametros
            // 
            this.menuParametros.Name = "menuParametros";
            this.menuParametros.Size = new System.Drawing.Size(180, 22);
            this.menuParametros.Text = "Parámetros";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
            // 
            // menuCambiarClave
            // 
            this.menuCambiarClave.Name = "menuCambiarClave";
            this.menuCambiarClave.Size = new System.Drawing.Size(180, 22);
            this.menuCambiarClave.Text = "Cambiar Contraseña";
            this.menuCambiarClave.Click += new System.EventHandler(this.menuCambiarClave_Click);
            // 
            // menuAyuda
            // 
            this.menuAyuda.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuAcercaDe});
            this.menuAyuda.Name = "menuAyuda";
            this.menuAyuda.Size = new System.Drawing.Size(53, 20);
            this.menuAyuda.Text = "Ay&uda";
            // 
            // menuAcercaDe
            // 
            this.menuAcercaDe.Name = "menuAcercaDe";
            this.menuAcercaDe.Size = new System.Drawing.Size(180, 22);
            this.menuAcercaDe.Text = "Acerca de...";
            this.menuAcercaDe.Click += new System.EventHandler(this.menuAcercaDe_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabelUsuario,
            this.statusLabelFecha});
            this.statusStrip.Location = new System.Drawing.Point(0, 678);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1200, 22);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip";
            // 
            // statusLabelUsuario
            // 
            this.statusLabelUsuario.Name = "statusLabelUsuario";
            this.statusLabelUsuario.Size = new System.Drawing.Size(47, 17);
            this.statusLabelUsuario.Text = "Usuario";
            // 
            // statusLabelFecha
            // 
            this.statusLabelFecha.Name = "statusLabelFecha";
            this.statusLabelFecha.Size = new System.Drawing.Size(1138, 17);
            this.statusLabelFecha.Spring = true;
            this.statusLabelFecha.Text = "Fecha";
            this.statusLabelFecha.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // FrmPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip;
            this.Name = "FrmPrincipal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Indexador Automático de Planos";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem menuArchivo;
        private System.Windows.Forms.ToolStripMenuItem menuCerrarSesion;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem menuSalir;
        private System.Windows.Forms.ToolStripMenuItem menuProcesos;
        private System.Windows.Forms.ToolStripMenuItem menuIngresoLotes;
        private System.Windows.Forms.ToolStripMenuItem menuPreparacionLotes;
        private System.Windows.Forms.ToolStripMenuItem menuPreparacionImagenes;
        private System.Windows.Forms.ToolStripMenuItem menuProcesamientoIA;
        private System.Windows.Forms.ToolStripMenuItem menuValidacionLotes;
        private System.Windows.Forms.ToolStripMenuItem menuFinalizarLotes;
        private System.Windows.Forms.ToolStripMenuItem menuAdministracion;
        private System.Windows.Forms.ToolStripMenuItem menuUsuarios;
        private System.Windows.Forms.ToolStripMenuItem menuParametros;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem menuCambiarClave;
        private System.Windows.Forms.ToolStripMenuItem menuAyuda;
        private System.Windows.Forms.ToolStripMenuItem menuAcercaDe;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabelUsuario;
        private System.Windows.Forms.ToolStripStatusLabel statusLabelFecha;
    }
}
