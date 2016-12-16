namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct structMQTSHM
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
        public byte[] _Id;
        public uint _TransLength;
        public int _ConversationId;
        public int _RequestId;
        public byte _Encoding;
        public byte _SegmentType;
        public byte _ControlFlags1;
        public byte _ControlFlags2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
        public byte[] _LUWID;
        public int _MQEncoding;
        public ushort _Ccsid;
        public ushort _Reserved;
    }
}

