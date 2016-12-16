﻿namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SPISYNCPOINTINOUT
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
        public byte[] ID;
        public int version;
        public int length;
    }
}

