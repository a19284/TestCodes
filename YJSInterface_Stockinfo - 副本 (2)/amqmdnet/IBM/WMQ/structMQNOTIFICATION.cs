namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct structMQNOTIFICATION
    {
        public int version;
        public int hObj;
        public int notificationCode;
        public int notificationValue;
    }
}

