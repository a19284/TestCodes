namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIActivateIn : MQBase
    {
        public const int MAX_SUPPORTED_VERSION = 1;
        private static readonly byte[] rfpVB_ID_ACTIVATE_IN = new byte[] { 0x53, 80, 0x41, 0x49 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPIACTIVATEIN spiActivateIn;

        public MQSPIActivateIn() : this(1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIActivateIn(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiActivateIn = new SPIACTIVATEIN();
            this.spiActivateIn.ID = rfpVB_ID_ACTIVATE_IN;
            this.Version = version;
            this.spiActivateIn.options = 0;
            this.spiActivateIn.qName = new byte[0x30];
            this.spiActivateIn.qMgrName = new byte[0x30];
            this.spiActivateIn.msgId = new byte[0x18];
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiActivateIn);
        }

        internal int GetVersionLength()
        {
            return this.GetLength();
        }

        public byte[] ToBuffer()
        {
            uint method = 0x279;
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
            uint method = 0x27a;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            this.spiActivateIn.length = this.GetVersionLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiActivateIn, zero, false);
                Marshal.Copy(zero, b, Offset, this.spiActivateIn.length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, this.spiActivateIn.length);
            }
            return this.spiActivateIn.length;
        }

        public int Length
        {
            get
            {
                return this.GetVersionLength();
            }
        }

        public byte[] MsgId
        {
            get
            {
                return (byte[]) this.spiActivateIn.msgId.Clone();
            }
            set
            {
                if (value != null)
                {
                    value.CopyTo(this.spiActivateIn.msgId, 0);
                }
            }
        }

        public int Options
        {
            get
            {
                return this.spiActivateIn.options;
            }
            set
            {
                this.spiActivateIn.options = value;
            }
        }

        public string QMgrName
        {
            get
            {
                return base.GetString(this.spiActivateIn.qMgrName);
            }
            set
            {
                if (value != null)
                {
                    base.GetBytes(value, ref this.spiActivateIn.qMgrName);
                }
            }
        }

        public string QName
        {
            get
            {
                return base.GetString(this.spiActivateIn.qName);
            }
            set
            {
                if (value != null)
                {
                    base.GetBytes(value, ref this.spiActivateIn.qName);
                }
            }
        }

        public SPIACTIVATEIN StructSpiActivateIn
        {
            get
            {
                return this.spiActivateIn;
            }
            set
            {
                this.spiActivateIn = value;
            }
        }

        public int Version
        {
            get
            {
                return this.spiActivateIn.version;
            }
            set
            {
                if (this.spiActivateIn.version < value)
                {
                    this.spiActivateIn.version = Math.Min(value, 1);
                }
            }
        }
    }
}

