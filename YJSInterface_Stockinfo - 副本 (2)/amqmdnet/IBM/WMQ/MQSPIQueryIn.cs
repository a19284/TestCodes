namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIQueryIn : MQBase
    {
        internal const int MAX_SUPPORTED_VERSION = 2;
        private static readonly byte[] rfpVB_ID_QUERY_IN = new byte[] { 0x53, 80, 0x51, 0x49 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPIQUERYIN spiQueryIn;

        public MQSPIQueryIn() : this(2)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIQueryIn(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiQueryIn = new SPIQUERYIN();
            this.spiQueryIn.ID = rfpVB_ID_QUERY_IN;
            this.Version = version;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiQueryIn);
        }

        internal int GetVersionLength()
        {
            return this.GetLength();
        }

        public byte[] ToBuffer()
        {
            uint method = 0x297;
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

        public int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x298;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            this.spiQueryIn.length = this.GetVersionLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiQueryIn, zero, false);
                Marshal.Copy(zero, b, Offset, this.spiQueryIn.length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, this.spiQueryIn.length);
            }
            return this.spiQueryIn.length;
        }

        public int Length
        {
            get
            {
                return this.GetVersionLength();
            }
        }

        public int Version
        {
            get
            {
                return this.spiQueryIn.version;
            }
            set
            {
                if (this.spiQueryIn.version < value)
                {
                    this.spiQueryIn.version = Math.Min(value, 2);
                }
            }
        }
    }
}

