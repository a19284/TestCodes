namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct structMQID
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
        public byte[] Id;
        public byte FapLevel;
        public byte IDFlags;
        public byte IDEFlags;
        public byte ErrFlags;
        public ushort Reserved;
        public ushort MaxMessagesPerBatch;
        public uint MaxTransmissionSize;
        public uint MaxMessageSize;
        public uint MessageSequenceWrapValue;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=20)]
        public byte[] ChannelName;
        public byte IDFlags2;
        public byte IDEFlags2;
        public ushort Ccsid;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
        public byte[] QueueManagerName;
        public uint HeartbeatInterval;
        public ushort EFLLength;
        public byte ErrFlags2;
        public byte Reserved1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
        public byte[] HdrCompList;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
        public byte[] MsgCompList;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
        public byte[] Reserved2;
        public uint SSLKeyReset;
        public int ConvPerSocket;
        public byte IDFlags3;
        public byte IDEFlags3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
        public byte[] Reserved3;
        public int ProcessIdentifier;
        public int ThreadIdentifier;
        public int TraceIdentifier;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
        public byte[] ProductIdentifier;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
        public byte[] QueueManagerId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=10)]
        public ushort[] Pal;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
        public byte[] R;
    }
}

