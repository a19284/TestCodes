namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct structMQREQUEST_MSGS
    {
        public int version;
        public int hObj;
        public int receivedBytes;
        public int requestedBytes;
        public int maxMessageLength;
        public int getMessageOptions;
        public int waitInterval;
        public int queueStatus;
        public int requestFlags;
        public int globalMessageIndex;
        public short selectionIndex;
        public short mqmdVersion;
        public int codedCharSetId;
        public int encoding;
        public int msgSequenceNumber;
        public int offset;
        public int matchOptions;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x18)]
        public byte[] msgId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x18)]
        public byte[] correlId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x18)]
        public byte[] groupId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
        public byte[] msgToken;
    }
}

