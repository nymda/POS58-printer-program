using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
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

        public Font fmompt = new Font("Lucida Console", 10);
        public Graphics paperGraphics;
        public Graphics holdGraphics;
        Bitmap pub = null;
        Bitmap holdImagePreview = new Bitmap(189, 274);
        Bitmap paper = new Bitmap(189, 274);
        public bool centered = false;
        public bool rotateLargeImage = false;
        public int defPubSize = 0;

        private void button1_Click(object sender, EventArgs e)
        {
            printFinalImage(paper);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            pub = new Bitmap(1, 1);
            paperGraphics = Graphics.FromImage(paper);
            holdGraphics = Graphics.FromImage(holdImagePreview);
            paperGraphics.FillRectangle(Brushes.White, 0, 0, 181, 274);
            holdGraphics.FillRectangle(Brushes.White, 0, 0, 181, 274);
            pictureBox1.Image = holdImagePreview;
            pictureBox2.Image = paper;
        }

        public void refreshPaperPreview()
        {
            pictureBox2.Image = paper;
        }

        public void printFinalImage(Bitmap output)
        {
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += delegate (object sender1, PrintPageEventArgs e1)
            {
                e1.Graphics.DrawImage(paper, new Point(0, 0));             
                pd.PrinterSettings.PrinterName = "POS58 Printer";
            };
            pd.Print();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Input File";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    pub = (Bitmap)Image.FromFile(dlg.FileName);
                    Size s = calcImgSize(pub, 181, true);
                    pub = new Bitmap(pub, s);
                    holdGraphics.FillRectangle(Brushes.White, 0, 0, 181, 274);
                    holdGraphics.DrawImage(pub, hScrollBar2.Value, hScrollBar3.Value);
                    defPubSize = pub.Width;
                    pictureBox1.Image = holdImagePreview;
                }
            }
        }
        
        public Size calcImgSize(Bitmap b, int width, bool firstRun)
        {
            if (firstRun)
            {
                firstRun = false;
                if (b.Width > b.Height)
                {
                    using (var form = new confirmFlip())
                    {
                        var result = form.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            rotateLargeImage = true;
                            b.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        }                       
                    }
                }
            }
            else
            {
                if (b.Width > b.Height){
                    if (rotateLargeImage)
                    {
                        b.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    }
                }
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

        public Bitmap lightenImg(Bitmap b)
        {
            Graphics g = Graphics.FromImage(b);
            Color darkTrans = Color.FromArgb(64, 255, 255, 255);
            Bitmap dtrans = new Bitmap(b.Width, b.Height);
            g.DrawImage(b, 0, 0);
            g.FillRectangle(new SolidBrush(darkTrans), 0, 0, b.Width, b.Height);
            return b;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            holdImagePreview = darkenImg(holdImagePreview);
            pictureBox1.Image = holdImagePreview;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            holdImagePreview = lightenImg(holdImagePreview);
            pictureBox1.Image = holdImagePreview;
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            holdGraphics.FillRectangle(Brushes.White, 0, 0, 181, 274);
            Size s = calcImgSize(pub, hScrollBar1.Value, false);
            pub = new Bitmap(pub, s);
            holdGraphics.FillRectangle(Brushes.White, hScrollBar2.Value, hScrollBar3.Value, 181, 274);
            holdGraphics.DrawImage(pub, hScrollBar2.Value, hScrollBar3.Value);
            pictureBox1.Image = holdImagePreview;
            setSizeDelay.Stop();
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            setSizeDelay.Stop();
            setSizeDelay.Start();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (var form = new QRCode_Make())
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    pub = form.retBit;
                    Size s = calcImgSize(pub, 181, true);
                    pub = new Bitmap(pub, s);
                    holdGraphics.FillRectangle(Brushes.White, 0, 0, 181, 274);
                    holdGraphics.DrawImage(pub, hScrollBar2.Value, hScrollBar3.Value);
                    hScrollBar1.Maximum = pub.Width * 2;
                    pictureBox1.Image = holdImagePreview;       
                }
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            centered = !centered;
            if (centered)
            {
                richTextBox1.SelectionAlignment = HorizontalAlignment.Center;
            }
            else
            {
                richTextBox1.SelectionAlignment = HorizontalAlignment.Left;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (fontDialog.ShowDialog() != DialogResult.Cancel)
            {
                richTextBox1.Font = fontDialog.Font;
                fmompt = fontDialog.Font;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //place text on paper
            StringFormat sf = new StringFormat();
            if (centered)
            {
                sf.Alignment = StringAlignment.Center;
            }
            else
            {
                sf.Alignment = StringAlignment.Near;
            }
            paperGraphics.DrawString(richTextBox1.Text, fmompt, new SolidBrush(Color.Black), new RectangleF(hScrollBar5.Value, hScrollBar4.Value, 179, 274), sf);
            refreshPaperPreview();
        }

        public Bitmap removeWhite(Bitmap inp)
        {
            Bitmap tmp = new Bitmap(inp.Width, inp.Height);
            Graphics g = Graphics.FromImage(tmp);
            for(int x = 0; x < tmp.Width; x++)
            {
                for (int y = 0; y < tmp.Height; y++)
                {
                    Color thisPx = inp.GetPixel(x, y);
                    if ((thisPx.R != 255) && (thisPx.G != 255) && (thisPx.B != 255))
                    {
                        g.DrawRectangle(new Pen(inp.GetPixel(x, y)), x, y, 1, 1);
                    }
                }
            }
            return tmp;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            paperGraphics.DrawRectangle(Pens.Black, new Rectangle(0, 0, 179, 273));
            refreshPaperPreview();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            paperGraphics.FillRectangle(Brushes.White, 0, 0, 181, 274);
            refreshPaperPreview();
        }

        private void button8_Click(object sender, EventArgs e)
        {          
            paperGraphics.DrawImage(removeWhite((Bitmap)pictureBox1.Image), new Point(0, 0));
            refreshPaperPreview();
        }

        private void hScrollBar2_Scroll(object sender, ScrollEventArgs e)
        {
            setSizeDelay.Stop();
            setSizeDelay.Start();
        }

        private void hScrollBar3_Scroll(object sender, ScrollEventArgs e)
        {
            setSizeDelay.Stop();
            setSizeDelay.Start();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            try { hScrollBar1.Value = defPubSize; }
            catch { }
            try { hScrollBar2.Value = 0; }
            catch { }
            try { hScrollBar3.Value = 0; }
            catch { }
            try { hScrollBar4.Value = 0; }
            catch { }
            try { hScrollBar5.Value = 0; }
            catch { }
            if(pub != null) { 
                setSizeDelay.Stop();
                setSizeDelay.Start();
            }
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            pub = new Bitmap(1, 1);
            holdImagePreview = new Bitmap(189, 274);
            paper = new Bitmap(189, 274);

            paperGraphics = Graphics.FromImage(paper);
            holdGraphics = Graphics.FromImage(holdImagePreview);
            paperGraphics.FillRectangle(Brushes.White, 0, 0, 181, 274);
            holdGraphics.FillRectangle(Brushes.White, 0, 0, 181, 274);
            pictureBox1.Image = holdImagePreview;
            pictureBox2.Image = paper;

            richTextBox1.Text = "";
        }

        private void button13_Click(object sender, EventArgs e)
        {

        }
    }
}
