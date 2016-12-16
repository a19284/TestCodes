namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct LPIACTIVATE
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
        public byte[] strucId;
        public int version;
        public int options;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
        public byte[] qName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
        public byte[] qMgrName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x18)]
        public byte[] msgId;
    }
}

