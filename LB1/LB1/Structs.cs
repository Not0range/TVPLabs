using System;
using System.Runtime.InteropServices;

namespace LB1
{
    public static class Structs
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SystemInfo
        {
            public UInt16 ProcessorArchitecture;
            public UInt32 PageSize;
            public UInt32 AllocationGranularity;
            public UInt32 ActiveProcessorMask;
            public UIntPtr MinimumApplicationAddress;
            public UInt32 NumberOfProcessors;
            public UInt32 ProcessorType;
            public UIntPtr MaximumApplicationAddress;
            public UInt32 OemId;
            public UInt16 Reserved;
            public UInt16 ProcessorLevel;
            public UInt16 ProcessorRevision;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MemoryStatus
        {
            public Int32 Lenght;
            public Int32 MemoryLoad;
            public UInt32 TotalPhys;
            public UInt32 AvailPhys;
            public UInt32 TotalPageFile;
            public UInt32 AvailPageFile;
            public UInt32 TotalVirtual;
            public UInt32 AvailVirtual;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct MemoryBasicInfo
        {
            public UIntPtr BaseAddress;
            public UIntPtr AllocationBase;
            public Int32 AllocationProtect;
            public Int32 RegionSize;
            public Int32 State;
            public Int32 Protect;
            public Int32 Type;
        }
    }
}
