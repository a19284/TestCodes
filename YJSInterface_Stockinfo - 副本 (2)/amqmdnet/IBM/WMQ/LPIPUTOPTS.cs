namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct LPIPUTOPTS
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
        public byte[] strucId;
        public int version;
        public int options;
        public int msgIdReservation;
        public int objectType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
        public byte[] objectQMgrName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
        public byte[] objectName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
        public byte[] origQMgrName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
        public byte[] pidQMgr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
        public byte[] pidQName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x18)]
        public byte[] pidCorrelId;
    }
}

