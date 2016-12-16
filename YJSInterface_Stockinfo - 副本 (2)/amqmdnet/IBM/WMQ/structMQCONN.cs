namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct structMQCONN
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
        public byte[] QMgrName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x1c)]
        public byte[] ApplName;
        public int ApplType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
        public byte[] AcctToken;
        public uint Options;
        public int XOptions;
    }
}

