using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApplication1
{
    public partial class frmPrincipal : Form
    {
        public frmPrincipal()
        {
            InitializeComponent();
        }

        private void btnProcesar_Click(object sender, EventArgs e)
        {
            System.IO.StreamReader t1;
            System.IO.StreamWriter t2;
            string[] archivos;
            string[] directorios;
            System.Collections.ArrayList linea;
            System.Collections.ArrayList nc1;
            float x1, x2, y1, y2, d1, d2;

            if (txtRuta.Text == "")
            {
                MessageBox.Show("Debe indicar la carpeta para buscar archivos NC1", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                return;
            }

            if (Directory.Exists(txtRuta.Text + "\\Resultados"))
            {
                Directory.Delete(txtRuta.Text + "\\Resultados", true);
            }

            archivos = Directory.GetFiles(txtRuta.Text, "*.nc1", System.IO.SearchOption.AllDirectories);
            directorios = Directory.GetDirectories(txtRuta.Text);

            Directory.CreateDirectory(txtRuta.Text + "\\Resultados");

            foreach (string ruta in directorios)
            {
                Directory.CreateDirectory(txtRuta.Text + "\\Resultados" + ruta.Replace(txtRuta.Text, ""));
            }

            int barra = 1;
            foreach (string dato in archivos)
            {
                prgBarra.Minimum = 0;
                prgBarra.Maximum = archivos.Count();
                prgBarra.Visible = true;

                this.Refresh();

                prgBarra.Value = barra;
                barra++;

                t1 = new StreamReader(dato,System.Text.Encoding.Default);
                linea = new System.Collections.ArrayList();
                nc1 = new System.Collections.ArrayList();

                while (!t1.EndOfStream)
                {
                    linea.Add(t1.ReadLine());
                }
                t1.Close();

                bool bo = false;
                int i=0;
                foreach (string texto in linea)
                {
                    if (linea.Count == i + 1)
                    {
                        nc1.Add(texto);
                    }
                    else
                    {
                        //if (bo == false || linea[i + 1].ToString().IndexOf("EN") != -1 || texto.IndexOf("EN") != -1 || linea[i + 1].ToString().IndexOf("BO") != -1 || texto.IndexOf("BO") != -1
                        //    || linea[i + 1].ToString().IndexOf("IK") != -1 || texto.IndexOf("KA") != -1 || linea[i + 1].ToString().IndexOf("KA") != -1 || texto.IndexOf("IK") != -1)

                        if (bo == false || linea[i].ToString()=="EN" || linea[i].ToString()=="BO" || linea[i].ToString()=="IK" || linea[i].ToString() == "KA" || linea[i+1].ToString() == "EN" 
                            || linea[i+1].ToString() == "BO" || linea[i+1].ToString() == "IK" || linea[i+1].ToString() == "KA")
                        {
                            if (linea[i].ToString() == "BO")
                            {
                                bo = true;
                            }

                            if (linea[i].ToString() == "KA")
                            {
                                bo = false;
                            }

                            nc1.Add(texto);
                        }
                        else
                        {
                            x1 = (float)Convert.ToDouble(texto.Substring(4, 10));
                            x2 = (float)Convert.ToDouble(linea[i + 1].ToString().Substring(4, 10));

                            y1 = (float)Convert.ToDouble(texto.Substring(17, 10));
                            y2 = (float)Convert.ToDouble(linea[i + 1].ToString().Substring(17, 10));

                            d1 = (float)Convert.ToDouble(texto.Substring(27, 9));
                            d2 = (float)Convert.ToDouble(linea[i + 1].ToString().Substring(27, 9));

                            if (!(Math.Abs(x1 - x2) < 5 && Math.Abs(y1 - y2) < 5 && d1 == d2))
                            {
                                nc1.Add(texto);
                            }
                        }

                        i++;
                    }
                }

                t2 = new StreamWriter(txtRuta.Text + "\\Resultados" + dato.Replace(txtRuta.Text,""), true, System.Text.Encoding.Default);

                foreach(string texto in nc1)
                {
                    t2.WriteLine(texto);
                }

                t2.Close();
            }

            MessageBox.Show("Proceso Terminado", "Proceso", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            prgBarra.Visible = false;

            System.Diagnostics.Process.Start(@txtRuta.Text + "\\Resultados");
        }

        private void btnBuscarRuta_Click(object sender, EventArgs e)
        {
            BuscaRuta.Reset();
            BuscaRuta.ShowDialog();
            txtRuta.Text = BuscaRuta.SelectedPath;
        }
    }
}
