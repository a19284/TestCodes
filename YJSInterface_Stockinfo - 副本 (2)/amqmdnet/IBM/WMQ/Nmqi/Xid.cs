namespace IBM.WMQ.Nmqi
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Xid
    {
        public int formatID;
        public byte gtrid_length;
        public byte bqual_length;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x80)]
        public byte[] data;
    }
}

