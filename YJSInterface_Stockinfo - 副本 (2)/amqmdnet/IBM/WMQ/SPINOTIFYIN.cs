namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SPINOTIFYIN
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
        public byte[] ID;
        public int version;
        public int length;
        public int options;
        public int reason;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x18)]
        public byte[] connectionId;
    }
}

