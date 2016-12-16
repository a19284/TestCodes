namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct structMQFAPMQCNO
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
        public byte[] Id;
        public int Version;
        public uint Reserved;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x80)]
        public byte[] ConnTag;
    }
}

