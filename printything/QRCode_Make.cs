using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace printything
{
    public partial class QRCode_Make : Form
    {
        bool firstClick = true;
        public Bitmap retBit { get; set; }

        public QRCode_Make()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            if (firstClick)
            {
                textBox1.Text = "";
                firstClick = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap bmp;
            try
            {
                WebClient w = new WebClient();
                byte[] barr = null;
                barr = w.DownloadData(@"https://api.qrserver.com/v1/create-qr-code/?size=181x181&data=" + textBox1.Text.Replace(" ", "%20"));
                using (var ms = new MemoryStream(barr))
                {
                    bmp = new Bitmap(ms);
                }
                pictureBox1.Image = bmp;
                button2.Enabled = true;
            }
            catch
            {
                bmp = new Bitmap(181, 181);
                Graphics g = Graphics.FromImage(bmp);
                Pen thicc = new Pen(Brushes.Black, 15);
                Font font = new Font("Lucida Console", 12);

                // :(

                g.FillRectangle(Brushes.White, 0, 0, 181, 181);
                g.FillRectangle(Brushes.Black, 45, 45, 15, 15);
                g.FillRectangle(Brushes.Black, 90, 45, 15, 15);
                g.DrawEllipse(thicc, 50, 70, 45, 45);
                g.FillRectangle(Brushes.White, 0, 90, 181, 90);
                g.DrawString("API Connection\nfailed.", font, Brushes.Black, new PointF(5, 5));
                pictureBox1.Image = bmp;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.retBit = (Bitmap)pictureBox1.Image;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void QRCode_Make_Load(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
