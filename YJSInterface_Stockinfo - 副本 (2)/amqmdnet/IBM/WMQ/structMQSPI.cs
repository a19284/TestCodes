namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct structMQSPI
    {
        public int verbId;
        public int outStructVersion;
        public int outStructLength;
    }
}

