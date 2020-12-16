using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace LB4
{
    public partial class Form1 : Form
    {
        int n;
        object l = new object();
        Mutex mutex = new Mutex();
        Semaphore semaphore = new Semaphore(1, 1);

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            n = 100;
            ThreadStart t;
            if (radioButton1.Checked)
                t = ThreadMethod;
            else if (radioButton2.Checked)
                t = LockThreadMethod;
            else if (radioButton3.Checked)
                t = MutexThreadMethod;
            else
                t = SemaphoreThreadMethod;
            Thread t1 = new Thread(t);
            Thread t2 = new Thread(t);
            t1.Start();
            t2.Start();
        }

        private void ThreadMethod()
        {
            n += 12;
            Invoke(new ChangeControl(() => listBox1.Items.Add(n)));
        }

        private void LockThreadMethod()
        {
            lock (l)
            {
                n += 12;
                Invoke(new ChangeControl(() => listBox1.Items.Add(n)));
            }
        }

        private void MutexThreadMethod()
        {
            mutex.WaitOne();
            n += 12;
            Invoke(new ChangeControl(() => listBox1.Items.Add(n)));
            mutex.ReleaseMutex();
        }

        private void SemaphoreThreadMethod()
        {
            semaphore.WaitOne();
            n += 12;
            Invoke(new ChangeControl(() => listBox1.Items.Add(n)));
            semaphore.Release();
        }
    }

    delegate void ChangeControl();
}
