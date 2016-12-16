namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct structMQCAUT
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
        public byte[] Id;
        public int AuthType;
        public int CAUTUserIdMaxLen;
        public int CAUTPasswdMaxLen;
        public int CAUTUserIdLen;
        public int CAUTPasswordLen;
    }
}

