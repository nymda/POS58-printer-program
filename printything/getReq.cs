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
    public partial class getReq : Form
    {
        public getReq(string purl = "")
        {
            InitializeComponent();
            prevUrl = purl;
        }

        public Bitmap res { get; set; }
        public string prevUrl { get; set; }

        private void getReq_Load(object sender, EventArgs e)
        {
            textBox1.Text = prevUrl;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WebClient w = new WebClient();
            Bitmap bmp;

            if (checkBox1.Checked)
            {
                w.Credentials = new NetworkCredential(textBox2.Text, textBox3.Text);
            }

            byte[] bmpData = w.DownloadData(textBox1.Text);
            using (var ms = new MemoryStream(bmpData)){ bmp = new Bitmap(ms); }

            pictureBox1.Image = bmp;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.res = (Bitmap)pictureBox1.Image;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
