namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct structMQOPENPRIV
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
        public byte[] Id;
        public int Version;
        public int Length;
        public int DefPersistence;
        public int DefPutResponseType;
        public int DefReadAhead;
        public int PropertyControl;
    }
}

