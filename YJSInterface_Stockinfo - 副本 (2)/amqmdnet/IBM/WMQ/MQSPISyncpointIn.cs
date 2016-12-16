namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPISyncpointIn : MQBase
    {
        internal const int MAX_SUPPORTED_VERSION = 1;
        private static readonly byte[] rfpVB_ID_SYNCPOINT_IN = new byte[] { 0x53, 80, 0x53, 0x49 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPISYNCPOINTIN spiSyncpointIn;

        public MQSPISyncpointIn() : this(1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPISyncpointIn(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiSyncpointIn = new SPISYNCPOINTIN();
            this.spiSyncpointIn.ID = rfpVB_ID_SYNCPOINT_IN;
            this.Version = version;
            this.spiSyncpointIn.options = 0;
            this.spiSyncpointIn.action = 0;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiSyncpointIn);
        }

        internal int GetVersionLength()
        {
            return this.GetLength();
        }

        public byte[] ToBuffer()
        {
            uint method = 0x2b6;
            this.TrEntry(method);
            byte[] b = new byte[this.GetVersionLength()];
            try
            {
                if (b != null)
                {
                    this.WriteStruct(b, 0);
                }
            }
            finally
            {
                base.TrExit(method, b);
            }
            return b;
        }

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x2b7;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            this.spiSyncpointIn.length = this.GetVersionLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiSyncpointIn, zero, false);
                Marshal.Copy(zero, b, Offset, this.spiSyncpointIn.length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, this.spiSyncpointIn.length);
            }
            return this.spiSyncpointIn.length;
        }

        public int Action
        {
            get
            {
                return this.spiSyncpointIn.action;
            }
            set
            {
                this.spiSyncpointIn.action = value;
            }
        }

        public int Length
        {
            get
            {
                return this.GetVersionLength();
            }
        }

        public int Options
        {
            get
            {
                return this.spiSyncpointIn.options;
            }
            set
            {
                this.spiSyncpointIn.options = value;
            }
        }

        public int Version
        {
            get
            {
                return this.spiSyncpointIn.version;
            }
            set
            {
                if (this.spiSyncpointIn.version < value)
                {
                    this.spiSyncpointIn.version = Math.Min(value, 1);
                }
            }
        }
    }
}

