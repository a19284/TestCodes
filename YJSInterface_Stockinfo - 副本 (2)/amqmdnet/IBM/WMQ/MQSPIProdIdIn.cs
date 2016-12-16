namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIProdIdIn : MQBase
    {
        internal const int MAX_SUPPORTED_VERSION = 1;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPIPRODIDINHDR spiPrdIdIn;

        public MQSPIProdIdIn() : this(1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIProdIdIn(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiPrdIdIn = new SPIPRODIDINHDR();
            this.spiPrdIdIn.ID = new byte[] { 0x53, 80, 0x44, 0x49 };
            this.spiPrdIdIn.clientProductId = new byte[12];
            this.Version = version;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiPrdIdIn);
        }

        internal int GetVersionLength()
        {
            return this.GetLength();
        }

        public byte[] ToBuffer()
        {
            uint method = 0x46b;
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
            uint method = 0x46c;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            this.spiPrdIdIn.length = this.GetVersionLength();
            int length = this.spiPrdIdIn.length;
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiPrdIdIn, zero, false);
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
                return this.spiPrdIdIn.clientProductId;
            }
            set
            {
                this.spiPrdIdIn.clientProductId = value;
            }
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
                return this.spiPrdIdIn.version;
            }
            set
            {
                if (this.spiPrdIdIn.version < value)
                {
                    this.spiPrdIdIn.version = Math.Min(value, 1);
                }
            }
        }
    }
}

