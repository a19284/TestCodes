namespace IBM.WMQ.Nmqi
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct XAINFO
    {
        public int xa_info_length;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x100)]
        public byte[] xa_info;
    }
}

