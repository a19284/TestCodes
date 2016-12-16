namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct LPISPO
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
        public byte[] strucId;
        public int version;
        public int options;
        public int action;
    }
}

