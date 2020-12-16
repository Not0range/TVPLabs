using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace LB3
{
    public partial class Form1 : Form
    {
        static List<ProcessInfo> processes = new List<ProcessInfo>();
        static List<WindowInfo> windows = new List<WindowInfo>();

        int prev;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            List<string> strs = new List<string>();
            List<WindowEntry> entries = new List<WindowEntry>();
            EnumWindows((hwnd, i) =>
            {
                unsafe
                {
                    byte[] str = new byte[1024];
                    int c = 0;
                    fixed (void* p = &str[0])
                        c = SendMessage(hwnd, 0xD, (IntPtr)1024, new IntPtr(p));
                    WindowEntry windowEntry = new WindowEntry();
                    windowEntry.size = Marshal.SizeOf(typeof(WindowEntry));
                    GetWindowInfo(hwnd, ref windowEntry);
                    int id = 0;
                    GetWindowThreadProcessId(hwnd, ref id);
                    WindowInfo win = new WindowInfo(hwnd, id, Encoding.Unicode.GetString(str, 0, c * 2), (windowEntry.style & (1 << 28)) != 0);
                    EnumChild(win);
                    windows.Add(win);
                }
                return true;
            }, 0);

            IntPtr snap = CreateToolhelp32Snapshot(2, 0);
            ProcessEntry entry = new ProcessEntry();
            entry.size = Marshal.SizeOf(typeof(ProcessEntry));
            if(!Process32First(snap, ref entry))
            {
                MessageBox.Show(Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()).Message);
                return;
            }

            do
            {
                int priority = 0;
                IntPtr proc = OpenProcess(0x0400, false, entry.processID);
                if (proc != IntPtr.Zero)
                {
                    priority = GetPriorityClass(proc);
                    CloseHandle(proc);
                }
                List<string> modules = new List<string>();

                IntPtr m = CreateToolhelp32Snapshot(24, entry.processID);
                ModuleEntry mEntry = new ModuleEntry();
                mEntry.size = Marshal.SizeOf(typeof(ModuleEntry));
                if (Module32First(m, ref mEntry))
                {
                    do
                    {
                        modules.Add(mEntry.moduleName);
                    } while (Module32Next(m, ref mEntry));
                }

                ProcessInfo temp = new ProcessInfo(entry.processID, entry.exeFile, entry.threads, priority, modules.ToArray());
                processes.Add(temp);
                TreeNode node = new TreeNode(temp.name, 0, 0);
                EnumChild(node, temp.id);
                treeView1.Nodes.Add(node);
            } while (Process32Next(snap, ref entry));
            CloseHandle(snap);
        }

        private static void EnumChild(WindowInfo win)
        {
            IntPtr ch = GetWindow(win.hwnd, 5);
            while(ch != IntPtr.Zero)
            {
                unsafe
                {
                    byte[] str = new byte[1024];
                    int c = 0;
                    fixed (void* p = &str[0])
                        c = SendMessage(ch, 0xD, (IntPtr)1024, new IntPtr(p));
                    WindowEntry windowEntry = new WindowEntry();
                    windowEntry.size = Marshal.SizeOf(typeof(WindowEntry));
                    GetWindowInfo(ch, ref windowEntry);
                    WindowInfo winC = new WindowInfo(ch, win.processID, Encoding.Unicode.GetString(str, 0, c * 2), (windowEntry.style & (1 << 28)) != 0);
                    EnumChild(winC);
                    win.children.Add(winC);
                }
                ch = GetWindow(ch, 2);
            }
        }

        private static void EnumChild(TreeNode node, int id)
        {
            var infos = windows.FindAll(w => w.processID == id);
            foreach(var i in infos)
                EnumChild(node.Nodes.Add(i.caption, i.caption, i.visible ? 1 : 2, i.visible ? 1 : 2), i);
        }

        private static void EnumChild(TreeNode node, WindowInfo win)
        {
            foreach (var i in win.children)
                EnumChild(node.Nodes.Add(i.caption, i.caption, i.visible ? 1 : 2, i.visible ? 1 : 2), i);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateToolhelp32Snapshot(Int32 flags, Int32 th32ProcessID);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool Process32First(IntPtr snapshot, ref ProcessEntry lppe);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool Process32Next(IntPtr snapshot, ref ProcessEntry lppe);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool Module32First(IntPtr snapshot, ref ModuleEntry lppe);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool Module32Next(IntPtr snapshot, ref ModuleEntry lppe);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr OpenProcess(Int32 access, bool inherit, Int32 id);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern Int32 GetPriorityClass(IntPtr ptr);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetPriorityClass(IntPtr ptr, Int32 priority);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CloseHandle(IntPtr ptr);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool TerminateProcess(IntPtr ptr, Int32 exitCode);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetWindowInfo(IntPtr hwnd, ref WindowEntry winInfo);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr EnumWindows(EnumProc enumFunc, int param);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hwnd, int cmd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern Int32 SendMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern Int32 GetWindowThreadProcessId(IntPtr hwnd, ref Int32 processID);

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode parent = e.Node;
            while (parent.Level != 0)
                parent = parent.Parent;
            trackBar1.Value = 2;
            string priority = "Неопределён";
            ProcessInfo p = processes[parent.Index];
            label1.Text = "ID: " + p.id;
            label2.Text = "Количество потоков: " + p.threads;
            switch(p.priority)
            {
                case 0x8000:
                    priority = "Выше среднего";
                    trackBar1.Value = 3;
                    break;
                case 0x4000:
                    priority = "Ниже среднего";
                    trackBar1.Value = 1;
                    break;
                case 0x100:
                    priority = "Реального времени";
                    trackBar1.Value = 5;
                    break;
                case 0x80:
                    priority = "Высокий";
                    trackBar1.Value = 4;
                    break;
                case 0x40:
                    priority = "Низкий";
                    trackBar1.Value = 0;
                    break;
                case 0x20:
                    priority = "Обычный";
                    break;
            }
            trackBar1.Enabled = p.priority != 0;
            prev = trackBar1.Value;
            label3.Text = "Приоритет: " + priority;
            listBox1.Items.Clear();
            listBox1.Items.AddRange(processes[parent.Index].modules);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (prev == trackBar1.Value)
                return;

            int priority;
            switch(trackBar1.Value)
            {
                case 0:
                    priority = 0x40;
                    break;
                case 1:
                    priority = 0x4000;
                    break;
                case 2:
                    priority = 0x20;
                    break;
                case 3:
                    priority = 0x8000;
                    break;
                case 4:
                    priority = 0x80;
                    break;
                case 5:
                    priority = 0x100;
                    break;
                default:
                    trackBar1.Value = prev;
                    return;
            }
            IntPtr proc = OpenProcess(0x0200, false, processes[treeView1.SelectedNode.Index].id);
            if(SetPriorityClass(proc, priority))
            {
                prev = trackBar1.Value;
                CloseHandle(proc);
                switch (trackBar1.Value)
                {
                    case 3:
                        label3.Text = "Приоритет: Выше среднего";
                        break;
                    case 1:
                        label3.Text = "Приоритет: Ниже среднего";
                        break;
                    case 5:
                        label3.Text = "Приоритет: Реального времени";
                        break;
                    case 4:
                        label3.Text = "Приоритет: Высокий";
                        break;
                    case 0:
                        label3.Text = "Приоритет: Низкий";
                        break;
                    case 2:
                        label3.Text = "Приоритет: Обычный";
                        break;
                }
            }
            else
            {
                trackBar1.Value = prev;
                CloseHandle(proc);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IntPtr proc = OpenProcess(0x0001, false, processes[treeView1.SelectedNode.Index].id);
            if (proc != IntPtr.Zero)
            {
                TerminateProcess(proc, -1);
                int i = treeView1.SelectedNode.Index - 1;
                treeView1.Nodes.Remove(treeView1.SelectedNode);
                treeView1.SelectedNode = treeView1.Nodes[i];
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    unsafe struct ProcessEntry
    {
        public Int32 size;
        public UInt32 usage;
        public Int32 processID;
        public UIntPtr defaultHeap;
        public UInt32 moduleID;
        public Int32 threads;
        public UInt32 parent;
        public Int32 priClassBase;
        public UInt32 flags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string exeFile;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    unsafe struct ModuleEntry
    {
        public Int32 size;
        public Int32 moduleID;
        public Int32 processID;
        public Int32 globUsage;
        public Int32 procUsage;
        public IntPtr baseAddr;
        public Int32 baseSize;
        public IntPtr hModule;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string moduleName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string exeFile;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct WindowEntry
    {
        public Int32 size;
        public Int32 leftWin;
        public Int32 topWin;
        public Int32 rightWin;
        public Int32 bottomWin;
        public Int32 leftCl;
        public Int32 topCl;
        public Int32 rightCl;
        public Int32 bottomCl;
        public Int32 style;
        public Int32 exStyle;
        public Int32 status;
        public UInt32 xWinBorder;
        public UInt32 yWinBorder;
        public IntPtr winType;
        public Int16 version;
    }

    class ProcessInfo
    {
        public int id;

        public string name;

        public int threads;

        public int priority;

        public string[] modules;

        public ProcessInfo(int id, string name, int threads, int priority, string[] modules)
        {
            this.id = id;
            this.name = name;
            this.threads = threads;
            this.priority = priority;
            this.modules = modules;
        }
    }

    class WindowInfo
    {
        public IntPtr hwnd;

        public int processID;

        public string caption;

        public bool visible;

        public List<WindowInfo> children = new List<WindowInfo>();

        public WindowInfo(IntPtr hwnd, int processID, string caption, bool visible)
        {
            this.hwnd = hwnd;
            this.processID = processID;
            this.caption = caption;
            this.visible = visible;
        }

        public override string ToString()
        {
            return caption;
        }
    }

    delegate bool EnumProc(IntPtr hwnd, int param);
}
