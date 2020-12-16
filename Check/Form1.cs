using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Check
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = Clipboard.GetText();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = Clipboard.GetImage();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Stream s = MemoryMappedFile.OpenExisting("MapFile1").CreateViewStream();
            byte[] buffer = new byte[s.Length];
            s.Read(buffer, 0, (int)s.Length);
            textBox2.Text = Encoding.UTF8.GetString(buffer);
            s.Close();
        }
    }
}
