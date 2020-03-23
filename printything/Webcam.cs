using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video.DirectShow;
using AForge.Video;
using AForge.Imaging.Filters;

namespace printything
{
    public partial class Webcam : Form
    {
        public Webcam()
        {
            InitializeComponent();  
        }

        public Bitmap cap { get; set; }
        private FilterInfoCollection VideoCaptureDevices;
        private VideoCaptureDevice FinalVideo;
        public Bitmap canvas;
        public Graphics canvasGraphics;
        bool firstFrameGet = true;
        List<String> devices = new List<String> { };
        int frames = 0;
        int dropped = 0;
        int brightness = 10;
        int contrast = 35;


        private void Webcam_Load(object sender, EventArgs e)
        {
            timer1.Start();
            VideoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo VideoCaptureDevice in VideoCaptureDevices){ devices.Add(VideoCaptureDevice.Name); }
            if(devices.Count == 0)
            {
                button2.Enabled = false;

                StringFormat sf = new StringFormat();
                Bitmap b = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                Graphics g = Graphics.FromImage(b);
                sf.Alignment = StringAlignment.Center;
                Font fmompt = new Font("Lucida Console", 15);
                g.DrawString("No webcams found", fmompt, new SolidBrush(Color.Black), new RectangleF(0, 0, b.Width, b.Height), sf);
                pictureBox1.Image = b;
            }
            else
            {
                label1.Text = "Device: " + devices[0];
                FinalVideo = new VideoCaptureDevice(VideoCaptureDevices[0].MonikerString);
                FinalVideo.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);
                FinalVideo.Start();
            }
        }

        void FinalVideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                if (firstFrameGet)
                {
                    canvas = new Bitmap(eventArgs.Frame.Width, eventArgs.Frame.Height);
                    canvasGraphics = Graphics.FromImage(canvas);
                    canvasGraphics.DrawImage(eventArgs.Frame, 0, 0, canvas.Width, canvas.Height);
                    if (checkBox1.Checked)
                    {
                        Bitmap small = smol(canvas);
                        BrightnessCorrection f0 = new BrightnessCorrection(brightness);
                        f0.ApplyInPlace(small);
                        ContrastCorrection f1 = new ContrastCorrection(contrast);
                        f1.ApplyInPlace(small);
                        pictureBox1.Image = monoChrome(small);
                    }
                    else
                    {
                        pictureBox1.Image = canvas;
                    }

                    firstFrameGet = false;
                }
                else
                {
                    canvasGraphics.DrawImage(eventArgs.Frame, 0, 0, canvas.Width, canvas.Height);
                    if (checkBox1.Checked)
                    {
                        Bitmap small = smol(canvas);
                        BrightnessCorrection f0 = new BrightnessCorrection(brightness);
                        f0.ApplyInPlace(small);
                        ContrastCorrection f1 = new ContrastCorrection(contrast);
                        f1.ApplyInPlace(small);
                        pictureBox1.Image = monoChrome(small);
                    }
                    else
                    {
                        pictureBox1.Image = canvas;
                    }

                }
                frames++;
            }
            catch
            {
                //dropped frame
                dropped++;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        public Bitmap smol(Bitmap b)
        {
            ContrastCorrection filter = new ContrastCorrection(30);
            filter.ApplyInPlace(b);

            Bitmap ret = new Bitmap(b.Width / 4, b.Height / 4);
            Graphics rg = Graphics.FromImage(ret);
            rg.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            rg.DrawImage(b, 0, 0, ret.Width, ret.Height);
            return ret;
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
                    if (avg >= 0 && avg <= 64) { nCol = Color.FromArgb(0, 0, 0); }
                    else if (avg > 64 && avg <= 128) { nCol = Color.FromArgb(64, 64, 64); }
                    else if (avg > 128 && avg <= 192) { nCol = Color.FromArgb(128, 128, 128); }
                    else if (avg == 255) { nCol = Color.White; }
                    else { nCol = Color.FromArgb(192, 192, 192); }
                    g.DrawRectangle(new Pen(nCol), x, y, 1, 1);
                }
            }
            return canvas;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FinalVideo.Stop();
            this.cap = (Bitmap)pictureBox1.Image;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            FinalVideo = new VideoCaptureDevice(VideoCaptureDevices[0].MonikerString);
            FinalVideo.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);
            FinalVideo.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label2.Text = "Frames: " + frames + " | Dropped: " + dropped;
            brightness = trackBar1.Value;
            contrast = trackBar2.Value;
        }
    }
}
