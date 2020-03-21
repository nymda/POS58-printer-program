using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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
        public Graphics heightGraphGraphics;
        Bitmap pub = null;
        Bitmap holdImagePreview = new Bitmap(189, 390);
        Bitmap paper = new Bitmap(189, 390);
        Bitmap loadedImagePure;
        Bitmap heightGraphDisplay = new Bitmap(10, 390);
        public bool centered = false;
        public bool rotateLargeImage = false;
        public int defPubSize = 0;
        public int paperHeight = 390;
        public bool whiteText = false;
        public int cutHeight = 390;
        public string prevGetReqUrl = "";

        private void button1_Click(object sender, EventArgs e)
        {
            printFinalImage(paper);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            heightGraphGraphics = Graphics.FromImage(heightGraphDisplay);
            pub = new Bitmap(1, 1);
            paperGraphics = Graphics.FromImage(paper);
            holdGraphics = Graphics.FromImage(holdImagePreview);
            paperGraphics.FillRectangle(Brushes.White, 0, 0, 190, 390);
            holdGraphics.FillRectangle(Brushes.White, 0, 0, 190, 390);
            pictureBox1.Image = holdImagePreview;
            pictureBox2.Image = paper;
            holdGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            holdGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            heightGraphGraphics.FillRectangle(Brushes.White, 0, 0, 10, 390);
            pictureBox3.Image = heightGraphDisplay;
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
                    loadedImagePure = (Bitmap)Image.FromFile(dlg.FileName);
                    pub = (Bitmap)Image.FromFile(dlg.FileName);
                    Size s = calcImgSize(pub, 190, true);
                    pub = new Bitmap(pub, s);
                    holdGraphics.FillRectangle(Brushes.White, 0, 0, 190, 390);
                    holdGraphics.DrawImage(pub, hScrollBar2.Value, hScrollBar3.Value);
                    defPubSize = pub.Width;
                    hScrollBar2.Minimum = (pub.Width * 3) * -1;
                    hScrollBar2.Maximum = pub.Width;
                    hScrollBar3.Minimum = (pub.Height * 3) * -1;
                    hScrollBar4.Maximum = pub.Height;
                    pictureBox1.Image = holdImagePreview;
                }
            }
        }
        
        public Size calcImgSize(Bitmap b, int width, bool showRotDialog)
        {
            if (showRotDialog)
            {
                if (b.Width > b.Height)
                {
                    using (var form = new confirmFlip())
                    {
                        var result = form.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            b.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            loadedImagePure.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        }
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
            dtrans.Dispose();
            g.Dispose();
            return b;
        }

        public Bitmap lightenImg(Bitmap b)
        {
            Graphics g = Graphics.FromImage(b);
            Color darkTrans = Color.FromArgb(64, 255, 255, 255);
            Bitmap dtrans = new Bitmap(b.Width, b.Height);
            g.DrawImage(b, 0, 0);
            g.FillRectangle(new SolidBrush(darkTrans), 0, 0, b.Width, b.Height);
            dtrans.Dispose();
            g.Dispose();
            return b;
        }

        public Bitmap increaseContrast(Bitmap b, int contrast)
        {
            Bitmap canvas = new Bitmap(b.Width, b.Height);
            Graphics g = Graphics.FromImage(canvas);

            for (int x = 0; x < b.Width; x++)
            {
                for (int y = 0; y < b.Height; y++)
                {
                    float factor = (259 * (contrast + 255)) / (255 * (259 - contrast));
                    factor = 5;
                    Color tmpColor = b.GetPixel(x, y);
                    float nRed = truncate(factor * (tmpColor.R - 128f) + 128f);
                    float nGre = truncate(factor * (tmpColor.G - 128f) + 128f);
                    float nBlu = truncate(factor * (tmpColor.B - 128f) + 128f);
                    Color nCol = Color.FromArgb((int)nRed, (int)nGre, (int)nBlu);
                    g.DrawRectangle(new Pen(nCol), x, y, 1, 1);
                }
            }

            return canvas;
        }

        public Bitmap autoProcessImage(Bitmap b, int stage = 0)
        {
            //todo

            Bitmap canvas = new Bitmap(b.Width, b.Height);
            Graphics g = Graphics.FromImage(b);

            if (stage == 0)
            {
                //calculate average brightness val of image
                List<int> Brightnesses = new List<int> { };
                for (int x = 0; x < b.Width; x++)
                {
                    for (int y = 0; y < b.Height; y++)
                    {
                        Color tmpColor = b.GetPixel(x, y);
                        int[] values = { tmpColor.R, tmpColor.B, tmpColor.G };
                        int brightness = (int)values.Average();
                        Brightnesses.Add(brightness);
                    }
                }

                int avgImgBrightness = (int)Brightnesses.Average();

                if(avgImgBrightness > 128)
                {
                    autoProcessImage(darkenImg(b), 0);
                }
                else
                {
                    autoProcessImage(b, 1);
                }
            }
            if(stage == 1)
            {
                //calculate the number of different contrast areas within the image
            }
            return null;
        }

        public Bitmap monoChrome(Bitmap b)
        {
            Bitmap canvas = new Bitmap(b.Width, b.Height);
            Graphics g = Graphics.FromImage(canvas);

            for (int x = 0; x < b.Width; x++)
            {
                for (int y = 0; y < b.Height; y++)
                {
                    Color tmpColor = b.GetPixel(x, y);
                    int[] values = { tmpColor.R, tmpColor.B, tmpColor.G };
                    double avg = values.Average();
                    Color nCol;
                    if(avg > 0 && avg <= 64){ nCol = Color.FromArgb(0, 0, 0); }
                    else if(avg > 64 && avg <= 128){ nCol = Color.FromArgb(64, 64, 64); }
                    else if (avg > 128 && avg <= 192){ nCol = Color.FromArgb(128, 128, 128); }
                    else if (avg == 255) { nCol = Color.White; }
                    else { nCol = Color.FromArgb(192, 192, 192); }
                    g.DrawRectangle(new Pen(nCol), x, y, 1, 1);
                }
            }

            return canvas;
        }

        public float truncate(float i)
        {
            if(i > 255)
            {
                return 255;
            }
            else if(i < 0)
            {
                return 0;
            }
            else
            {
                return i;
            }
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
            holdGraphics.FillRectangle(Brushes.White, 0, 0, 190, 390);
            Size s = calcImgSize(pub, hScrollBar1.Value, false);
            pub = new Bitmap(loadedImagePure, s);
            holdGraphics.FillRectangle(Brushes.White, hScrollBar2.Value, hScrollBar3.Value, 190, 390);
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
                    Size s = calcImgSize(pub, 190, true);
                    pub = new Bitmap(pub, s);
                    holdGraphics.FillRectangle(Brushes.White, 0, 0, 190, 390);
                    holdGraphics.DrawImage(pub, hScrollBar2.Value, hScrollBar3.Value);
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
            fontDialog.Font = fmompt;
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
            if (!whiteText)
            {
                paperGraphics.DrawString(richTextBox1.Text, fmompt, new SolidBrush(Color.Black), new RectangleF(hScrollBar5.Value, hScrollBar4.Value, 179, 390), sf);
            }
            else
            {
                paperGraphics.DrawString(richTextBox1.Text, fmompt, new SolidBrush(Color.White), new RectangleF(hScrollBar5.Value, hScrollBar4.Value, 179, 390), sf);
            }
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
            paperGraphics.DrawRectangle(Pens.Black, new Rectangle(0, 0, 180, cutHeight - 1));
            refreshPaperPreview();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            paperGraphics.FillRectangle(Brushes.White, 0, 0, 190, 390);
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

        private void button7_Click_1(object sender, EventArgs e)
        {
            pub.Dispose();
            loadedImagePure.Dispose();
            holdImagePreview.Dispose();
            paper.Dispose();

            pub = new Bitmap(1, 1);
            loadedImagePure = new Bitmap(1, 1);
            holdImagePreview = new Bitmap(189, 390);
            paper = new Bitmap(189, 390);

            paperGraphics = Graphics.FromImage(paper);
            holdGraphics = Graphics.FromImage(holdImagePreview);
            paperGraphics.FillRectangle(Brushes.White, 0, 0, 190, 390);
            holdGraphics.FillRectangle(Brushes.White, 0, 0, 190, 390);
            pictureBox1.Image = holdImagePreview;
            pictureBox2.Image = paper;

            richTextBox1.Text = "";

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
            if (pub != null)
            {
                setSizeDelay.Stop();
                setSizeDelay.Start();
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            string response = specialEvent(richTextBox1.Text);
            if (response == "0")
            {
                PrintDocument pd = new PrintDocument();
                pd.PrintPage += delegate (object sender1, PrintPageEventArgs e1)
                {
                    StringFormat sf = new StringFormat();
                    if (centered) { sf.Alignment = StringAlignment.Center; }
                    else { sf.Alignment = StringAlignment.Near; }
                    e1.Graphics.DrawString(richTextBox1.Text, fmompt, new SolidBrush(Color.Black), new RectangleF(0, 3, pd.DefaultPageSettings.PrintableArea.Width, pd.DefaultPageSettings.PrintableArea.Height), sf);
                    pd.PrinterSettings.PrinterName = "POS58 Printer";
                };
                pd.Print();
            }
            else
            {
                richTextBox1.Text = response;
            }
        }

        private string specialEvent(string t)
        {
            if(t == "$ALPHA")
            {
                return "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz01234567890!£$%^&*()";
            }
            if (t == "$CREDS")
            {
                return "PRIMPT - Written by kned, Designed for the POS58 Printer. Free to use and modify.";
            }
            if (t == "$SHADE")
            {
                return "███▓▓▓▒▒▒░░░\n███▓▓▓▒▒▒░░░\n███▓▓▓▒▒▒░░░\n███▓▓▓▒▒▒░░░\n███▓▓▓▒▒▒░░░";
            }
            if (Regex.Match(t, "[$][R][A][N][D][.].*").Success)
            {
                string[] split = t.Split('.');
                int count;
                try{ count = Int32.Parse(split[1]); }
                catch{ count = 10; }
                string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                Random r = new Random();
                return new string(Enumerable.Repeat(chars, count).Select(s => s[r.Next(s.Length)]).ToArray());
            }
            return "0";
        }

        private void button15_Click(object sender, EventArgs e)
        {
            paperGraphics.FillRectangle(Brushes.White, 0, cutHeight, 190, 390 - cutHeight);
            refreshPaperPreview();
        }

        private void radioButton1_CheckedChanged_1(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                whiteText = false;
            }
            else
            {
                whiteText = true;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            cutHeight = Math.Abs(trackBar1.Value);
            button15.Text = "Cut to " + cutHeight + "px";

            heightGraphGraphics.FillRectangle(Brushes.White, 0, 0, 10, 390);
            heightGraphGraphics.DrawLine(Pens.Black, 0, cutHeight - 1, 10, cutHeight - 1);
            pictureBox3.Image = heightGraphDisplay;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            ContrastCorrection filter = new ContrastCorrection(15);
            filter.ApplyInPlace(holdImagePreview);
            pictureBox1.Image = holdImagePreview;
        }

        private void button17_Click(object sender, EventArgs e)
        {
            holdImagePreview = monoChrome(holdImagePreview);
            pictureBox1.Image = holdImagePreview;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            using (var form = new Webcam())
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    loadedImagePure = form.cap.Clone(new Rectangle(0, 0, form.cap.Width, form.cap.Height), System.Drawing.Imaging.PixelFormat.DontCare);
                    pub = form.cap;
                    Size s = calcImgSize(pub, 190, true);
                    pub = new Bitmap(pub, s);
                    holdGraphics.FillRectangle(Brushes.White, 0, 0, 190, 390);
                    holdGraphics.DrawImage(pub, hScrollBar2.Value, hScrollBar3.Value);
                    defPubSize = pub.Width;
                    pictureBox1.Image = holdImagePreview;
                }
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            using (var form = new getReq(prevGetReqUrl))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    prevGetReqUrl = form.prevUrl;
                    loadedImagePure = form.res.Clone(new Rectangle(0, 0, form.res.Width, form.res.Height), System.Drawing.Imaging.PixelFormat.DontCare);
                    pub = form.res;
                    Size s = calcImgSize(pub, 190, true);
                    pub = new Bitmap(pub, s);
                    holdGraphics.FillRectangle(Brushes.White, 0, 0, 190, 390);
                    holdGraphics.DrawImage(pub, hScrollBar2.Value, hScrollBar3.Value);
                    defPubSize = pub.Width;
                    pictureBox1.Image = holdImagePreview;
                }
            }
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }
    }
}
