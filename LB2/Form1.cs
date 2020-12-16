using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace LB2
{
    public partial class Form1 : Form
    {
        Random rand = new Random();

        Thread t1, t2;

        uint count1 = 0;
        uint count2 = 0;

        uint[] allCount1 = new uint[5];
        uint[] allCount2 = new uint[5];
        int index;

        uint n = 0;

        public Form1()
        {
            InitializeComponent();

            t1 = new Thread(() =>
            {
                while (true)
                {
                    int s = 0;
                    for (int i = 0; i < 10; i++)
                        s += rand.Next(0, 10000);
                    count1++;
                    Thread.Sleep(1);
                }
            });

            t2 = new Thread(() =>
            {
                while (true)
                {
                    int s = 0;
                    for (int i = 0; i < 10; i++)
                        s += rand.Next(0, 10000);
                    count2++;
                    Thread.Sleep(1);
                }
            });
            t1.Start();
            t2.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Исполняемые файлы|*.exe";
            openFile.Multiselect = false;
            if(openFile.ShowDialog() == DialogResult.OK)
                textBox1.Text = openFile.FileName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string str = "\"" + textBox1.Text + "\"";
            WinExec(str, 5);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            unsafe
            {
                ProcessInformation information = new ProcessInformation();
                
                StartupInfo startup = new StartupInfo();
                startup.cb = sizeof(StartupInfo);
                startup.lpReserved = 0;
                startup.lpDesktop = 0;
                startup.lpTitle = 0;
                startup.dwX = 0;
                startup.dwY = 0;
                startup.dwXSize = 0;
                startup.dwYSize = 0;
                startup.dwXCountChars = 0;
                startup.dwYCountChars = 0;
                startup.dwFillAttribute = 0;
                startup.dwFlags = 0;
                startup.wShowWindow = 0;
                startup.cbReserved2 = 0;
                startup.lpReserved2 = (IntPtr)null;
                startup.hStdInput = (IntPtr)null;
                startup.hStdOutput = (IntPtr)null;
                startup.hStdError = (IntPtr)null;
                CreateProcessA(textBox1.Text, null, (IntPtr)null, (IntPtr)null, true, 0, (IntPtr)null, null, ref startup, ref information);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            n++;
            label1.Text = count1.ToString();
            label2.Text = count2.ToString();

            allCount1[index] = count1;
            allCount2[index++] = count2;

            if (index >= allCount1.Length)
                index = 0;

            label3.Text = "Обычное усереднение: " + ((double)Sum(allCount1) / n);
            label4.Text = "Обычное усереднение: " + ((double)Sum(allCount2) / n);

            uint[] copy1 = CopySort(allCount1);
            uint[] copy2 = CopySort(allCount2);
            label5.Text = "Робастные оценки: " + ((double)Sum(copy1, 1, 3) / n);
            label6.Text = "Робастные оценки: " + ((double)Sum(copy2, 1, 3) / n);

            label7.Text = "Дисперсия: " + Dispercio(allCount1);
            label8.Text = "Дисперсия: " + Dispercio(allCount2);

            label9.Text = "Разница мин-макс: " + (copy1[4] - copy1[0]);
            label10.Text = "Разница мин-макс: " + (copy2[4] - copy2[0]);

            count1 = 0;
            count2 = 0;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            t1.Priority = (ThreadPriority)trackBar1.Value;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            t2.Priority = (ThreadPriority)trackBar2.Value;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            t1.Abort();
            t2.Abort();
        }

        private ulong Sum(uint[] array)
        {
            ulong s = 0;
            foreach (var a in array)
                s += a;
            return s;
        }

        private ulong Sum(uint[] array, int index, int count)
        {
            ulong s = 0;
            for (int i = index; i < index + count; i++)
                s += array[i];
            return s;
        }

        private uint[] CopySort(uint[] array)
        {
            uint[] copy = new uint[5];
            Array.Copy(array, copy, 5);
            Array.Sort(copy);
            return copy;
        }

        private double Dispercio(uint[] array)
        {
            double a = 0;
            for (int i = 0; i < array.Length; i++)
                a += (double)array[i] * (double)array[i] * 0.2;

            double b = 0;
            for (int i = 0; i < array.Length; i++)
                b += array[i] * 0.2;
            return a - b * b;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WinExec(string cmdLine, uint cmdShow);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint CreateProcessA(string appName, string cmd, 
            IntPtr processAttributes, IntPtr threadAttributes,
            bool inheritHandles, Int32 creationFlags, IntPtr enviroment, string currentDirectory,
            ref StartupInfo startupInfo, ref ProcessInformation ProcessInformation);
    }


    [StructLayout(LayoutKind.Sequential)]
    struct StartupInfo
    {
        public Int32 cb;
        public byte lpReserved;
        public byte lpDesktop;
        public byte lpTitle;
        public Int32 dwX;
        public Int32 dwY;
        public Int32 dwXSize;
        public Int32 dwYSize;
        public Int32 dwXCountChars;
        public Int32 dwYCountChars;
        public Int32 dwFillAttribute;
        public Int32 dwFlags;
        public Int16 wShowWindow;
        public Int16 cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ProcessInformation
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public Int32 dwProcessId;
        public Int32 dwThreadId;
    }
}
