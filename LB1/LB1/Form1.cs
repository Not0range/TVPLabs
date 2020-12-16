using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace LB1
{
    public partial class Form1 : Form
    {
        const int HEAP_ZERO_MEMORY = 0x00000008;

        const int MEM_COMMIT = 0x00001000;

        const int MEM_RESERVE = 0x00002000;

        const int MEM_DECOMMIT = 0x00004000;

        const int MEM_RELEASE = 0x00008000;

        const int PAGE_READWRITE = 0x04;

        UIntPtr defaultHeap;

        UIntPtr heap;

        UIntPtr regionPtr;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Structs.SystemInfo info = new Structs.SystemInfo();
            unsafe { GetSystemInfo(new IntPtr(&info)); }
            label1.Text = "OEM Id: " + info.OemId;
            label2.Text = "Архитектура процессора: ";
            switch (info.ProcessorArchitecture)
            {
                case 9:
                    label2.Text += "x64";
                    break;
                case 5:
                    label2.Text += "ARM";
                    break;
                case 12:
                    label2.Text += "ARM64";
                    break;
                case 6:
                    label2.Text += "Intel Itanium-based";
                    break;
                case 0:
                    label2.Text += "x86";
                    break;
                default:
                    label2.Text += "Неопределено";
                    break;
            }
            label3.Text = "Размер страницы памяти: " + info.PageSize + " Байт";
            label4.Text = "Количество процессоров (ядер): " + info.NumberOfProcessors;
            label5.Text = "Минимальный адрес: 0x" + info.MinimumApplicationAddress.ToUInt32().ToString("X8");
            label6.Text = "Максимальный адрес: 0x" + info.MaximumApplicationAddress.ToUInt32().ToString("X8");

            Structs.MemoryStatus status = new Structs.MemoryStatus();
            unsafe
            {
                status.Lenght = sizeof(Structs.MemoryStatus);
                GlobalMemoryStatus(new IntPtr(&status));
            }
            progressBar1.Value = status.MemoryLoad;
            label14.Text = status.MemoryLoad + "%";
            label8.Text = "Общий объём физической памяти " + status.TotalPhys / (1024 * 1024) + " МБ";
            label9.Text = "Доступный объём физической памяти " + status.AvailPhys / (1024 * 1024) + " МБ";
            label10.Text = "Общий объём файла подкачки " + status.TotalPageFile / (1024 * 1024) + " МБ";
            label11.Text = "Доступный объём файла подкачки " + status.AvailPageFile / (1024 * 1024) + " МБ";
            label12.Text = "Общий объём виртуальной памяти " + status.TotalVirtual / (1024 * 1024) + " МБ";
            label13.Text = "Доступный объём виртуальной памяти " + status.AvailVirtual / (1024 * 1024) + " МБ";

            Structs.MemoryBasicInfo memory = new Structs.MemoryBasicInfo();
            int[,] mem = new int[3, 3];
            unsafe
            {
                for (UIntPtr addr = UIntPtr.Zero;
                    VirtualQuery(addr, new IntPtr(&memory), sizeof(Structs.MemoryBasicInfo)) == sizeof(Structs.MemoryBasicInfo)
                    && addr.ToUInt32() < (uint)(1024 * 1024 * 1024) * 3;
                    addr += memory.RegionSize)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    DataGridViewTextBoxCell[] cells = new DataGridViewTextBoxCell[3];
                    cells[0] = new DataGridViewTextBoxCell();
                    cells[0].Value = "0x" + addr.ToUInt32().ToString("X8") + " - " + (addr + memory.RegionSize).ToUInt32().ToString("X8");
                    cells[1] = new DataGridViewTextBoxCell();
                    cells[1].Value = memory.State == 0x1000 ? "Зафиксированная" : (memory.State == 0x10000 ? "Свободная" : "Зарезервированная");
                    cells[2] = new DataGridViewTextBoxCell();
                    cells[2].Value = memory.Type == 0x1000000 ? "Образ" : (memory.Type == 0x40000 ? "Раздел" : (memory.Type == 0x20000 ? "Частная" : "-"));
                    row.Cells.AddRange(cells);

                    int r = 0;
                    int c;
                    if (addr.ToUInt32() < (uint)(1024 * 1024 * 1024))
                    {
                        dataGridView1.Rows.Add(row);
                        c = 0;
                    }
                    else if (addr.ToUInt32() < (uint)(1024 * 1024 * 1024) * 2)
                    {
                        dataGridView2.Rows.Add(row);
                        c = 1;
                    }
                    else
                    {
                        dataGridView3.Rows.Add(row);
                        c = 2;
                    }

                    switch(memory.State)
                    {
                        case 0x1000:
                            r = 0;
                            break;
                        case 0x10000:
                            r = 1;
                            break;
                        case 0x2000:
                            r = 2;
                            break;
                    }

                    mem[r, c] += memory.RegionSize;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                DataGridViewRow[] rows = new DataGridViewRow[3];
                for (int l = 0; l < rows.Length; l++)
                    rows[l] = new DataGridViewRow();

                for (int j = 0; j < rows.Length; j++)
                {
                    DataGridViewTextBoxCell[] cells = new DataGridViewTextBoxCell[4];

                    for (int l = 0; l < cells.Length; l++)
                        cells[l] = new DataGridViewTextBoxCell();

                    switch(j)
                    {
                        case 0:
                            cells[0].Value = "Зафиксированная";
                            break;
                        case 1:
                            cells[0].Value = "Свободная";
                            break;
                        case 2:
                            cells[0].Value = "Зарезервированная";
                            break;
                    }
                    cells[2].Value = mem[j, i] / (1024 * 1024) + " МБ";
                    rows[j].Cells.AddRange(cells);
                }

                switch(i)
                {
                    case 0:
                        dataGridView4.Rows.AddRange(rows);
                        break;
                    case 1:
                        dataGridView5.Rows.AddRange(rows);
                        break;
                    case 2:
                        dataGridView6.Rows.AddRange(rows);
                        break;
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Structs.MemoryStatus status = new Structs.MemoryStatus();
            unsafe
            {
                status.Lenght = sizeof(Structs.MemoryStatus);
                GlobalMemoryStatus(new IntPtr(&status));
            }

            progressBar1.Value = status.MemoryLoad;
            label14.Text = status.MemoryLoad + "%";
            label8.Text = "Общий объём физической памяти " + status.TotalPhys / (1024 * 1024) + " МБ";
            label9.Text = "Доступный объём физической памяти " + status.AvailPhys / (1024 * 1024) + " МБ";
            label10.Text = "Общий объём файла подкачки " + status.TotalPageFile / (1024 * 1024) + " МБ";
            label11.Text = "Доступный объём файла подкачки " + status.AvailPageFile / (1024 * 1024) + " МБ";
            label12.Text = "Общий объём виртуальной памяти " + status.TotalVirtual / (1024 * 1024) + " МБ";
            label13.Text = "Доступный объём виртуальной памяти " + status.AvailVirtual / (1024 * 1024) + " МБ";

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += Bw_DoWork;
            bw.RunWorkerAsync();
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            int[,] mem = new int[3, 3];
            Invoke(new Action(() =>
            {
                dataGridView1.Rows.Clear();
                dataGridView2.Rows.Clear();
                dataGridView3.Rows.Clear();
            }));
            Structs.MemoryBasicInfo memory = new Structs.MemoryBasicInfo();
            unsafe
            {
                for (UIntPtr addr = UIntPtr.Zero;
                    VirtualQuery(addr, new IntPtr(&memory), sizeof(Structs.MemoryBasicInfo)) == sizeof(Structs.MemoryBasicInfo)
                    && addr.ToUInt32() < (uint)(1024 * 1024 * 1024) * 3;
                    addr += memory.RegionSize)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    DataGridViewTextBoxCell[] cells = new DataGridViewTextBoxCell[3];
                    cells[0] = new DataGridViewTextBoxCell();
                    cells[0].Value = "0x" + addr.ToUInt32().ToString("X8") + " - " + (addr + memory.RegionSize).ToUInt32().ToString("X8");
                    cells[1] = new DataGridViewTextBoxCell();
                    cells[1].Value = memory.State == 0x1000 ? "Зафиксированная" : (memory.State == 0x10000 ? "Свободная" : "Зарезервированная");
                    cells[2] = new DataGridViewTextBoxCell();
                    cells[2].Value = memory.Type == 0x1000000 ? "Образ" : (memory.Type == 0x40000 ? "Раздел" : (memory.Type == 0x20000 ? "Частная" : "-"));
                    row.Cells.AddRange(cells);

                    int r = 0;
                    int c;
                    if (addr.ToUInt32() < (uint)(1024 * 1024 * 1024))
                    {
                        Invoke(new Action(() => dataGridView1.Rows.Add(row)));
                        c = 0;
                    }
                    else if (addr.ToUInt32() < (uint)(1024 * 1024 * 1024) * 2)
                    {
                        Invoke(new Action(() => dataGridView2.Rows.Add(row)));
                        c = 1;
                    }
                    else
                    {
                        Invoke(new Action(() => dataGridView3.Rows.Add(row)));
                        c = 2;
                    }

                    switch (memory.State)
                    {
                        case 0x1000:
                            r = 0;
                            break;
                        case 0x10000:
                            r = 1;
                            break;
                        case 0x2000:
                            r = 2;
                            break;
                    }

                    mem[r, c] += memory.RegionSize;
                }
            }

            Invoke(new Action(() =>
            {
                for (int i = 0; i < 3; i++)
                {
                    dataGridView4.Rows[i].Cells[1].Value = dataGridView4.Rows[i].Cells[2].Value;
                    dataGridView4.Rows[i].Cells[2].Value = mem[i, 0] /(1024 * 1024) + " МБ";
                    dataGridView4.Rows[i].Cells[3].Value = Math.Abs(int.Parse(dataGridView4.Rows[i].Cells[1].Value.ToString()
                    .Remove(dataGridView4.Rows[i].Cells[1].Value.ToString().Length - 3)) - mem[i, 0] / (1024 * 1024)) + " МБ";
                }
                for (int i = 0; i < 3; i++)
                {
                    dataGridView5.Rows[i].Cells[1].Value = dataGridView5.Rows[i].Cells[2].Value;
                    dataGridView5.Rows[i].Cells[2].Value = mem[i, 1] / (1024 * 1024) + " МБ";
                    dataGridView5.Rows[i].Cells[3].Value = Math.Abs(int.Parse(dataGridView5.Rows[i].Cells[1].Value.ToString()
                    .Remove(dataGridView5.Rows[i].Cells[1].Value.ToString().Length - 3)) - mem[i, 1] / (1024 * 1024)) + " МБ";
                }
                for (int i = 0; i < 3; i++)
                {
                    dataGridView6.Rows[i].Cells[1].Value = dataGridView6.Rows[i].Cells[2].Value;
                    dataGridView6.Rows[i].Cells[2].Value = mem[i, 2] / (1024 * 1024) + " МБ";
                    dataGridView6.Rows[i].Cells[3].Value = Math.Abs(int.Parse(dataGridView6.Rows[i].Cells[1].Value.ToString()
                    .Remove(dataGridView6.Rows[i].Cells[1].Value.ToString().Length - 3)) - mem[i, 2] / (1024 * 1024)) + " МБ";
                }
            }));
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern void GetSystemInfo(IntPtr ptr);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern void GlobalMemoryStatus(IntPtr ptr);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern Int32 VirtualQuery(UIntPtr address, IntPtr ptr, Int32 lenght);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern UIntPtr VirtualAlloc(UIntPtr address, Int32 size, Int32 allocationType, Int32 protect);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool VirtualFree(UIntPtr address, Int32 size, Int32 freeType);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern UIntPtr GetProcessHeap();

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern UIntPtr HeapCreate(Int32 option, Int32 initialSize, Int32 maxSize);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern UIntPtr HeapAlloc(UIntPtr ptr, Int32 flags, Int32 bytes);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool HeapFree(UIntPtr ptr, Int32 flags, UIntPtr mem);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool HeapDestroy(UIntPtr ptr);

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox2.Enabled = (sender as CheckBox).Checked;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !(char.IsDigit(e.KeyChar) || e.KeyChar > 'A' && e.KeyChar < 'F' || e.KeyChar > 'a' && e.KeyChar < 'f'
                || (Keys)e.KeyChar == Keys.Back || (Keys)e.KeyChar == Keys.Delete);
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !(char.IsDigit(e.KeyChar) || (Keys)e.KeyChar == Keys.Back || (Keys)e.KeyChar == Keys.Delete);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((sender as TabControl).SelectedIndex == 1 && button9.Text == "Приостановить")
                timer1.Start();
            else
                timer1.Stop();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            uint addr = 0;
            int size;
            if (!(int.TryParse(textBox1.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out size)
                && size != 0 && (!checkBox1.Checked || checkBox1.Checked && uint.TryParse(textBox2.Text, out addr))))
            {
                MessageBox.Show("Ошибка ввода");
                return;
            }

            regionPtr = VirtualAlloc(checkBox1.Checked ? new UIntPtr(addr) : (UIntPtr)null, size, MEM_RESERVE, PAGE_READWRITE);

            textBox1.Enabled = false;
            textBox2.Enabled = false;
            checkBox1.Enabled = false;

            button1.Enabled = false;
            button2.Enabled = true;
            button4.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            VirtualAlloc(regionPtr, int.Parse(textBox1.Text), MEM_COMMIT, PAGE_READWRITE);
            button2.Enabled = false;
            button3.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            VirtualFree(regionPtr, 0, MEM_DECOMMIT);

            button2.Enabled = true;
            button3.Enabled = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            VirtualFree(regionPtr, 0, MEM_RELEASE);

            textBox1.Enabled = true;
            textBox2.Enabled = checkBox1.Checked;
            checkBox1.Enabled = true;

            button1.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            byte[][] strs = new byte[listBox1.Items.Count][];
            int l = 0;
            for(int i = 0; i < strs.Length; i++)
            {
                strs[i] = Encoding.Unicode.GetBytes(listBox1.Items[i].ToString());
                l += strs[i].Length;
            }

            defaultHeap = GetProcessHeap();
            UIntPtr ptr = HeapAlloc(defaultHeap, HEAP_ZERO_MEMORY, l);
            int offset = 0;
            for (int i = 0; i < strs.Length; i++)
                for (int j = 0; j < strs.Length; j++)
                    Marshal.WriteByte(ptr, offset++, strs[i][j]);

            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            button8.Enabled = true;

            label16.Text = "Куча по умолчанию (0x" + ptr.ToUInt32().ToString("X8") + ")";
        }

        private void button8_Click(object sender, EventArgs e)
        {
            HeapFree(defaultHeap, 0, (UIntPtr)null);

            button7.Enabled = true;
            button8.Enabled = false;
            button5.Enabled = true;
            button6.Enabled = true;

            label16.Text = "Куча по умолчанию";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (textBox3.Text != "")
            {
                listBox1.Items.Add(textBox3.Text);
                textBox3.Text = "";
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if((sender as Button).Text == "Приостановить")
            {
                (sender as Button).Text = "Продолжить";
                timer1.Stop();
            }
            else
            {
                (sender as Button).Text = "Приостановить";
                timer1.Start();
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            int l = int.Parse(textBox4.Text);
            if(!button12.Enabled)
                heap = HeapCreate(0, l * 20, 0);
            UIntPtr ptr = HeapAlloc(heap, HEAP_ZERO_MEMORY, l * 20);

            button10.Enabled = false;
            button11.Enabled = true;
            button12.Enabled = true;
            label18.Text = "Создание кучи (0x" + ptr.ToUInt32().ToString("X8") + ")";
        }

        private void button11_Click(object sender, EventArgs e)
        {
            HeapFree(heap, 0, (UIntPtr)null);

            button10.Enabled = true;
            button11.Enabled = false;
            button12.Enabled = true;
            label18.Text = "Создание кучи";
        }

        private void button12_Click(object sender, EventArgs e)
        {
            HeapDestroy(heap);

            button10.Enabled = true;
            button11.Enabled = false;
            button12.Enabled = false;
            label18.Text = "Создание кучи";
        }
    }
}
