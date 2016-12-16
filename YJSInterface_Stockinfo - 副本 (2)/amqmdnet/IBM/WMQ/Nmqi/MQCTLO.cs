namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Runtime.InteropServices;

    public class MQCTLO : MQBase
    {
        internal structMQCTLO mqctlo;
        private const string sccsid = "%Z% %W% %I% %E% %U%";

        public MQCTLO()
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%");
            this.mqctlo = new structMQCTLO();
            this.mqctlo.strucId = new byte[] { 0x43, 0x54, 0x4c, 0x4f };
            this.mqctlo.version = 1;
            this.mqctlo.options = 0;
            this.mqctlo.waitInterval = -1;
            this.mqctlo.connectionArea = IntPtr.Zero;
        }

        public int GetLength()
        {
            return Marshal.SizeOf(this.mqctlo);
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x494;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            zero = Marshal.AllocCoTaskMem(length);
            if (length > (b.Length - Offset))
            {
                length = b.Length - Offset;
            }
            try
            {
                Marshal.Copy(b, Offset, zero, length);
                this.mqctlo = (structMQCTLO) Marshal.PtrToStructure(zero, typeof(structMQCTLO));
            }
            finally
            {
                Marshal.FreeCoTaskMem(zero);
                base.TrExit(method);
            }
            return (Offset + this.GetLength());
        }

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x493;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            zero = Marshal.AllocCoTaskMem(length);
            try
            {
                Marshal.StructureToPtr(this.mqctlo, zero, false);
                Marshal.Copy(zero, b, Offset, length);
            }
            finally
            {
                Marshal.FreeCoTaskMem(zero);
                base.TrExit(method);
            }
            return length;
        }

        public IntPtr ConnectionArea
        {
            get
            {
                return this.mqctlo.connectionArea;
            }
            set
            {
                this.mqctlo.connectionArea = value;
            }
        }

        public int Options
        {
            get
            {
                return this.mqctlo.options;
            }
            set
            {
                this.mqctlo.options = value;
            }
        }

        public structMQCTLO StructMQCTLO
        {
            get
            {
                return this.mqctlo;
            }
            set
            {
                this.mqctlo = value;
            }
        }

        public int Version
        {
            get
            {
                return this.mqctlo.version;
            }
            set
            {
                this.mqctlo.version = value;
            }
        }

        public int WaitInterval
        {
            get
            {
                return this.mqctlo.waitInterval;
            }
            set
            {
                this.mqctlo.waitInterval = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct structMQCTLO
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            public byte[] strucId;
            public int version;
            public int options;
            public int waitInterval;
            public IntPtr connectionArea;
        }
    }
}

