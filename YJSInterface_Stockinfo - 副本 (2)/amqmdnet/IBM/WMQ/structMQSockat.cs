namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct structMQSockat
    {
        public int conversationId;
        public int requestId;
        public int type;
        public int parm1;
        public int parm2;
    }
}

