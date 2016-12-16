namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MQPD
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
        public byte[] strucId;
        public int Version;
        public int Options;
        public int Support;
        public int Context;
        public int CopyOptions;
    }
}

