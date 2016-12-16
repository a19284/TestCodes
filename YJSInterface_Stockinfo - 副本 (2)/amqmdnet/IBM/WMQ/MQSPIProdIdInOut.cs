namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIProdIdInOut : MQBase
    {
        internal const int MAX_SUPPORTED_VERSION = 1;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPIPRODIDINOUTHDR spiPrdIdInOut;

        public MQSPIProdIdInOut() : this(1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIProdIdInOut(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiPrdIdInOut = new SPIPRODIDINOUTHDR();
            this.spiPrdIdInOut.ID = new byte[] { 0x53, 80, 0x44, 0x55 };
            this.Version = version;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiPrdIdInOut);
        }

        internal int GetVersionLength()
        {
            return this.GetLength();
        }

        public byte[] ToBuffer()
        {
            uint method = 0x46d;
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
            uint method = 0x46e;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            this.spiPrdIdInOut.length = this.GetVersionLength();
            int length = this.spiPrdIdInOut.length;
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiPrdIdInOut, zero, false);
                Marshal.Copy(zero, b, Offset, length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, length);
            }
            return length;
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
                return this.spiPrdIdInOut.version;
            }
            set
            {
                if (this.spiPrdIdInOut.version < value)
                {
                    this.spiPrdIdInOut.version = Math.Min(value, 1);
                }
            }
        }
    }
}

