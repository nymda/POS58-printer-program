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

        private void Webcam_Load(object sender, EventArgs e)
        {
            VideoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo VideoCaptureDevice in VideoCaptureDevices){ comboBox1.Items.Add(VideoCaptureDevice.Name); }
            if(comboBox1.Items.Count == 0)
            {
                comboBox1.Enabled = false;
                button1.Enabled = false;
                button2.Enabled = false;

                StringFormat sf = new StringFormat();
                Bitmap b = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                Graphics g = Graphics.FromImage(b);
                sf.Alignment = StringAlignment.Center;
                Font fmompt = new Font("Lucida Console", 15);
                g.DrawString("No webcams found", fmompt, new SolidBrush(Color.Black), new RectangleF(0, 0, b.Width, b.Height), sf);
                pictureBox1.Image = b;
            }
            else{ comboBox1.SelectedIndex = 0; }
        }
        void FinalVideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap video = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = video;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            FinalVideo = new VideoCaptureDevice(VideoCaptureDevices[comboBox1.SelectedIndex].MonikerString);
            FinalVideo.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);
            FinalVideo.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FinalVideo.Stop();
            this.cap = (Bitmap)pictureBox1.Image;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
