namespace WindowsFormsApplication1
{
    partial class frmPrincipal
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPrincipal));
            this.btnProcesar = new System.Windows.Forms.Button();
            this.BuscaRuta = new System.Windows.Forms.FolderBrowserDialog();
            this.btnBuscarRuta = new System.Windows.Forms.Button();
            this.txtRuta = new System.Windows.Forms.TextBox();
            this.prgBarra = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // btnProcesar
            // 
            this.btnProcesar.Location = new System.Drawing.Point(115, 73);
            this.btnProcesar.Name = "btnProcesar";
            this.btnProcesar.Size = new System.Drawing.Size(113, 42);
            this.btnProcesar.TabIndex = 0;
            this.btnProcesar.Text = "Procesar";
            this.btnProcesar.UseVisualStyleBackColor = true;
            this.btnProcesar.Click += new System.EventHandler(this.btnProcesar_Click);
            // 
            // BuscaRuta
            // 
            this.BuscaRuta.ShowNewFolderButton = false;
            this.BuscaRuta.Tag = "Buscar carpeta con archivos NC1";
            // 
            // btnBuscarRuta
            // 
            this.btnBuscarRuta.Location = new System.Drawing.Point(24, 26);
            this.btnBuscarRuta.Name = "btnBuscarRuta";
            this.btnBuscarRuta.Size = new System.Drawing.Size(35, 29);
            this.btnBuscarRuta.TabIndex = 2;
            this.btnBuscarRuta.Text = "...";
            this.btnBuscarRuta.UseVisualStyleBackColor = true;
            this.btnBuscarRuta.Click += new System.EventHandler(this.btnBuscarRuta_Click);
            // 
            // txtRuta
            // 
            this.txtRuta.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.txtRuta.Location = new System.Drawing.Point(81, 31);
            this.txtRuta.Name = "txtRuta";
            this.txtRuta.ReadOnly = true;
            this.txtRuta.Size = new System.Drawing.Size(236, 20);
            this.txtRuta.TabIndex = 3;
            // 
            // prgBarra
            // 
            this.prgBarra.Location = new System.Drawing.Point(24, 134);
            this.prgBarra.Name = "prgBarra";
            this.prgBarra.Size = new System.Drawing.Size(293, 25);
            this.prgBarra.TabIndex = 4;
            this.prgBarra.Visible = false;
            // 
            // frmPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(344, 171);
            this.Controls.Add(this.prgBarra);
            this.Controls.Add(this.txtRuta);
            this.Controls.Add(this.btnBuscarRuta);
            this.Controls.Add(this.btnProcesar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmPrincipal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CNC Adaptador";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnProcesar;
        private System.Windows.Forms.FolderBrowserDialog BuscaRuta;
        private System.Windows.Forms.Button btnBuscarRuta;
        private System.Windows.Forms.TextBox txtRuta;
        private System.Windows.Forms.ProgressBar prgBarra;
    }
}

