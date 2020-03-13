using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace printything
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public int fomptsoze = 10;
        bool printingImage = false;
        Bitmap pub = null;
        Bitmap pubPure = null;
        Bitmap paper = new Bitmap(189, 12897);

        private void button1_Click(object sender, EventArgs e)
        {
            Console.Write(textBox1.Text);
            PrintDocument pd = new PrintDocument();
            Graphics gr = Graphics.FromImage(paper);
            //gr = e1.Graphics

            pd.PrintPage += delegate (object sender1, PrintPageEventArgs e1)
            {
                Font fmompt = new Font("Lucida Console", fomptsoze);
                e1.Graphics.DrawString(textBox1.Text, fmompt, new SolidBrush(Color.Black), new RectangleF(0, 3, pd.DefaultPageSettings.PrintableArea.Width, pd.DefaultPageSettings.PrintableArea.Height));
                int stranglengf = (int)e1.Graphics.MeasureString(textBox1.Text, fmompt).Width;
                int strangheite = (int)e1.Graphics.MeasureString(textBox1.Text, fmompt).Height;
                if (checkBox1.Checked)
                {
                    e1.Graphics.DrawRectangle(Pens.Black, 0, 0, stranglengf, strangheite + 3);
                }
                if (printingImage)
                {
                    e1.Graphics.DrawImage(pub, new Point(0, 0));
                }
                pd.PrinterSettings.PrinterName = "POS58 Printer";
            };

            pictureBox1.Image = paper;
            pd.Print();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            fomptsoze = (int)numericUpDown1.Value;
            textBox1.Font = new Font("Lucida Console", fomptsoze);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Input File";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    pub = (Bitmap)Image.FromFile(dlg.FileName);
                    printingImage = true;
                    Size s = calcImgSize(pub, 181);
                    pub = new Bitmap(pub, s);
                    hScrollBar1.Maximum = pub.Width;
                    pictureBox1.Image = pub;
                }
            }
        }
        
        public Size calcImgSize(Bitmap b, int width)
        {
            if(b.Width > b.Height)
            {
                b.RotateFlip(RotateFlipType.Rotate90FlipNone);
            }
            int sourceWidth = b.Width;
            int sourceHeight = b.Height;
            float nPercent = ((float)width / (float)sourceWidth);
            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            return new Size(destWidth, destHeight);
        }

        public Bitmap darkenImg(Bitmap b)
        {
            Graphics g = Graphics.FromImage(b);
            Color darkTrans = Color.FromArgb(64, 0, 0, 0);
            Bitmap dtrans = new Bitmap(b.Width, b.Height);
            g.DrawImage(b, 0, 0);
            g.FillRectangle(new SolidBrush(darkTrans), 0, 0, b.Width, b.Height);
            return b;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            pub = darkenImg(pub);
            pictureBox1.Image = pub;
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            Size s = calcImgSize(pub, hScrollBar1.Value);
            pub = new Bitmap(pub, s);
            pictureBox1.Image = pub;
            timer1.Stop();
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            timer1.Stop();
            timer1.Start();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (var form = new QRCode_Make())
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    Bitmap qrCode = form.retBit;
                    pub = qrCode;
                    printingImage = true;
                    Size s = calcImgSize(pub, 181);
                    pub = new Bitmap(pub, s);
                    hScrollBar1.Maximum = pub.Width;
                    pictureBox1.Image = pub;         
                }
            }
        }
    }
}
