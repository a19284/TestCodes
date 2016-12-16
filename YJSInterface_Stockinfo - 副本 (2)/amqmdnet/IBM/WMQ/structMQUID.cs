namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct structMQUID
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
        public byte[] Id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
        public byte[] UserIdentifier;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
        public byte[] Password;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x40)]
        public byte[] LongUserId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=40)]
        public byte[] UserSecurityId;
    }
}

