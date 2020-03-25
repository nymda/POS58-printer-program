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
using System.Drawing.Imaging;

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
        string device = "";
        int brightness = 10;
        int contrast = 35;
        public Int32[] pixels;
        protected GCHandle PixHandle;
        public int fpsCounter = 0;

        //disables [X] button
        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        private void Webcam_Load(object sender, EventArgs e)
        {
            timer2.Start();
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
                device = devices[0];
                FinalVideo = new VideoCaptureDevice(VideoCaptureDevices[0].MonikerString);
                FinalVideo.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);
                FinalVideo.Start();
            }
        }

        bool flip = true;
        void FinalVideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (flip || !checkBox3.Checked)
            {
                try
                {
                    if (firstFrameGet)
                    {
                        Bitmap smallPreDraw = smol(eventArgs.Frame);
                        Bitmap mono = monoLocked(smallPreDraw);

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
                            pictureBox1.Image = monoLocked(small);
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
                            pictureBox1.Image = monoLocked(small);
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
                    dropped++;
                }
                fpsCounter++;
                flip = !flip;
            }
            else
            {
                flip = !flip;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        public Bitmap smol(Bitmap b)
        {
            if (checkBox2.Checked)
            {
                Bitmap ret = new Bitmap(320, 240);
                Graphics rg = Graphics.FromImage(ret);
                rg.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                rg.DrawImage(b, 0, 0, 320, 240);
                return ret;
            }
            else
            {
                //shouldnt be nessicary, shit if i know why it is
                Bitmap ret = new Bitmap(b.Width, b.Height);
                Graphics rg = Graphics.FromImage(ret);
                rg.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                rg.DrawImage(b, 0, 0, b.Width, b.Height);
                return ret;
            }
        }

        public Bitmap monoLocked(Bitmap btm)
        {
            BitmapData BtmpDt = btm.LockBits(new Rectangle(0, 0, btm.Width, btm.Height), ImageLockMode.ReadWrite, btm.PixelFormat);
            IntPtr pointer = BtmpDt.Scan0;
            int size = Math.Abs(BtmpDt.Stride) * btm.Height;
            byte[] pixels = new byte[size];
            Marshal.Copy(pointer, pixels, 0, size);

            int counter = 0;
            List<int> rgbStore = new List<int> { };
            for (int b = 0; b < pixels.Length; b++)
            {
                if (counter == 3)
                {
                    //process bitmap here
                    int avg = rgbStore[0] + rgbStore[1] + rgbStore[2];
                    avg = avg / 3;

                    if (avg >= 0 && avg <= 64)
                    {
                        avg = 0;
                    }
                    else if (avg > 64 && avg <= 128)
                    {
                        avg = 64;
                    }
                    else if (avg > 128 && avg <= 192)
                    {
                        avg = 128;
                    }
                    else if (avg == 255)
                    {
                        avg = 255;
                    }
                    else
                    {
                        avg = 192;
                    }

                    pixels[b - 3] = (byte)avg;
                    pixels[b - 2] = (byte)avg;
                    pixels[b - 1] = (byte)avg;
                    pixels[b] = 255;


                    //reset data for next chunk
                    counter = 0;
                    rgbStore = new List<int> { };
                }
                else
                {
                    counter++;
                    rgbStore.Add(pixels[b]);
                }
            }
            Marshal.Copy(pixels, 0, pointer, size);
            btm.UnlockBits(BtmpDt);

            return btm;
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

        public void buildTitleBox()
        {
            this.Text = string.Format("| Webcam | Frame: {0} | Dropped : {1} | Device: {2} |", frames, dropped, device);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            buildTitleBox();
            brightness = trackBar1.Value;
            contrast = trackBar2.Value;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            FinalVideo.Stop();
            this.DialogResult = DialogResult.Abort;
            this.Close();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            string fps = fpsCounter.ToString();
            if(fps.Length == 1)
            {
                fps = "0" + fps;
            }
            label1.Text = "Fps: " + fps;
            fpsCounter = 0;
        }
    }
}
