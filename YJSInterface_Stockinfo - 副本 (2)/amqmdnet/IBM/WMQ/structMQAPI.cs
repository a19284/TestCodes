namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack=1)]
    internal struct structMQAPI
    {
        [FieldOffset(0)]
        public int CallLength;
        [FieldOffset(4)]
        public int CompCode;
        [FieldOffset(8)]
        public int Flags;
        [FieldOffset(12)]
        public int Handle;
        [FieldOffset(8)]
        public int Reason;
        [FieldOffset(4)]
        public int ReturnCode;
        [FieldOffset(12)]
        public int Rmid;
    }
}

