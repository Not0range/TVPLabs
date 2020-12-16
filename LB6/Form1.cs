using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LB6
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach(var d in drives)
            {
                tabControl1.TabPages.Add(d.VolumeLabel + "(" + d.Name + ")");
                TableLayoutPanel panel = new TableLayoutPanel();
                panel.RowCount = 4;
                panel.ColumnCount = 2;
                panel.Dock = DockStyle.Fill;
                Label[,] l = new Label[4, 2];
                for (int i = 0; i < l.GetLength(0); i++)
                    for (int j = 0; j < l.GetLength(1); j++)
                        l[i, j] = new Label();

                l[0, 0].Text = "Файловая система";
                l[0, 1].Text = d.DriveFormat;
                l[1, 0].Text = "Тип логического диска";
                l[1, 1].Text = GetTypeText(d.DriveType);
                l[2, 0].Text = "Общий объём";
                l[2, 1].Text = String.Format("{0:F3} ГБ", (double)d.TotalSize / (1024 * 1024 * 1024));
                l[3, 0].Text = "Свободный объём";
                l[3, 1].Text = String.Format("{0:F3} ГБ", (double)d.TotalFreeSpace / (1024 * 1024 * 1024));

                for (int i = 0; i < l.GetLength(0); i++)
                    for (int j = 0; j < l.GetLength(1); j++)
                        panel.Controls.Add(l[i, j], j, i);

                tabControl1.TabPages[tabControl1.TabPages.Count - 1].Controls.Add(panel);
            }
        }

        private string GetTypeText(DriveType driveType)
        {
            switch(driveType)
            {
                case DriveType.Fixed:
                    return "Фиксированный";
                case DriveType.CDRom:
                    return "Дисковод";
                case DriveType.Network:
                    return "Сетевой";
                case DriveType.Removable:
                    return "Внешний накопитель";
                case DriveType.Ram:
                    return "Оперативная память";
                default:
                    return "Неопределено";
            }
        }
    }
}
