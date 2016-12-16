namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SPIRESERVEOUT
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
        public byte[] ID;
        public int version;
        public int length;
        public int actualReservation;
        public int tagIncrementOffset;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x18)]
        public byte[] baseReservationTag;
    }
}

