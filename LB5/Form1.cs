using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LB5
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox1.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Clipboard.SetImage(Image.FromFile(textBox2.Text));
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MemoryMappedFile file = MemoryMappedFile.CreateNew("MapFile1", 1024);
            byte[] buffer = Encoding.UTF8.GetBytes(textBox3.Text);
            Stream s = file.CreateViewStream();
            s.Write(buffer, 0, buffer.Length);
            s.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "Изображения|*.png;*.jpeg;*.jpg;*.gif";
            if (dialog.ShowDialog() == DialogResult.OK)
                textBox2.Text = dialog.FileName;
        }
    }
}
