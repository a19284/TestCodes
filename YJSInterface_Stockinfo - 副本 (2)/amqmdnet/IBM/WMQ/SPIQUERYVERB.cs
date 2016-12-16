namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SPIQUERYVERB
    {
        public int verbId;
        public int maxInOutVersion;
        public int maxInVersion;
        public int maxOutVersion;
        public int flags;
    }
}

