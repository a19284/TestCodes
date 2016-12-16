namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct SPIGETOUTHDR
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
        public byte[] ID;
        public int version;
        public int length;
        public int getStatus;
        public int msgLength;
        public int reserved;
        public int inherited;
        public uint qTimeHigh;
        public uint qTimeLow;
    }
}

