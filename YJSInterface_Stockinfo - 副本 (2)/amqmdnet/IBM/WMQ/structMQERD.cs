namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct structMQERD
    {
        public uint ErrorDataLength;
        public uint ReturnCode;
        public uint ErrorData;
    }
}

