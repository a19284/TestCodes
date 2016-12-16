namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIProdIdOut : MQBase
    {
        internal const int MAX_SUPPORTED_VERSION = 1;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPIPRODIDOUTHDR spiPrdIdOut;

        public MQSPIProdIdOut() : this(1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIProdIdOut(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiPrdIdOut = new SPIPRODIDOUTHDR();
            this.spiPrdIdOut.ID = new byte[] { 0x53, 80, 0x44, 0x4f };
            this.spiPrdIdOut.clientProductId = new byte[12];
            this.spiPrdIdOut.serverProductId = new byte[12];
            this.Version = version;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiPrdIdOut);
        }

        internal int GetVersionLength()
        {
            return this.GetLength();
        }

        public byte[] ToBuffer()
        {
            uint method = 0x46f;
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
            uint method = 0x470;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            this.spiPrdIdOut.length = this.GetVersionLength();
            int length = this.spiPrdIdOut.length;
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiPrdIdOut, zero, false);
                Marshal.Copy(zero, b, Offset, length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, length);
            }
            return length;
        }

        public byte[] ClientProductIdentifier
        {
            get
            {
                return this.spiPrdIdOut.clientProductId;
            }
        }

        public int Length
        {
            get
            {
                return this.GetVersionLength();
            }
        }

        public byte[] ServerProductIdentifier
        {
            get
            {
                return this.spiPrdIdOut.serverProductId;
            }
        }

        public int Version
        {
            get
            {
                return this.spiPrdIdOut.version;
            }
            set
            {
                if (this.spiPrdIdOut.version < value)
                {
                    this.spiPrdIdOut.version = Math.Min(value, 1);
                }
            }
        }
    }
}

