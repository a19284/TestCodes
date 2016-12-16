namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIGetInOut : MQBase
    {
        internal MQGetMessageOptions gmo;
        public const int MAX_SUPPORTED_VERSION = 2;
        internal MQMessageDescriptor md;
        private static readonly byte[] rfpVB_ID_GET_INOUT = new byte[] { 0x53, 80, 0x47, 0x55 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPIGETNOUTHDR spiGetInOutHdr;

        public MQSPIGetInOut() : this(2)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIGetInOut(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiGetInOutHdr = new SPIGETNOUTHDR();
            this.spiGetInOutHdr.ID = rfpVB_ID_GET_INOUT;
            this.Version = version;
            this.md = null;
            this.gmo = null;
        }

        private int GetHdrLength()
        {
            return Marshal.SizeOf(this.spiGetInOutHdr);
        }

        private int GetHdrVersionLength()
        {
            uint method = 0x284;
            this.TrEntry(method);
            switch (this.Version)
            {
                case 1:
                case 2:
                {
                    int hdrLength = this.GetHdrLength();
                    base.TrExit(method, hdrLength, 1);
                    return hdrLength;
                }
            }
            base.TrExit(method, 0, 2);
            return 0;
        }

        internal int GetLength()
        {
            return ((Marshal.SizeOf(this.spiGetInOutHdr) + this.md.GetLength()) + this.gmo.GetLength());
        }

        internal int GetVersionLength()
        {
            return ((this.GetHdrVersionLength() + this.md.GetVersionLength()) + this.gmo.GetVersionLength());
        }

        public int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x287;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int hdrLength = this.GetHdrLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                if (hdrLength > (b.Length - Offset))
                {
                    hdrLength = b.Length - Offset;
                }
                Marshal.Copy(b, Offset, zero, hdrLength);
                this.spiGetInOutHdr = (SPIGETNOUTHDR) Marshal.PtrToStructure(zero, typeof(SPIGETNOUTHDR));
                Marshal.FreeCoTaskMem(zero);
                Offset += this.GetHdrVersionLength();
                Offset = this.md.ReadStruct(b, Offset);
                Offset = this.gmo.ReadStruct(b, Offset);
            }
            finally
            {
                base.TrExit(method, Offset);
            }
            return Offset;
        }

        public byte[] ToBuffer()
        {
            uint method = 0x285;
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
            uint method = 0x286;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int hdrVersionLength = this.GetHdrVersionLength();
            this.spiGetInOutHdr.length = this.GetVersionLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiGetInOutHdr, zero, false);
                Marshal.Copy(zero, b, Offset, hdrVersionLength);
                Marshal.FreeCoTaskMem(zero);
                hdrVersionLength += this.md.WriteStruct(b, Offset + hdrVersionLength);
                hdrVersionLength += this.gmo.WriteStruct(b, Offset + hdrVersionLength);
            }
            finally
            {
                base.TrExit(method, hdrVersionLength);
            }
            return hdrVersionLength;
        }

        public MQGetMessageOptions GetMsgOpts
        {
            get
            {
                return this.gmo;
            }
            set
            {
                this.gmo = value;
            }
        }

        public int Length
        {
            get
            {
                return this.GetVersionLength();
            }
        }

        public MQMessageDescriptor MsgDesc
        {
            get
            {
                return this.md;
            }
            set
            {
                this.md = value;
            }
        }

        public SPIGETNOUTHDR StructSpiGetIntOut
        {
            get
            {
                return this.spiGetInOutHdr;
            }
            set
            {
                this.spiGetInOutHdr = value;
            }
        }

        public int Version
        {
            get
            {
                return this.spiGetInOutHdr.version;
            }
            set
            {
                if (this.spiGetInOutHdr.version < value)
                {
                    this.spiGetInOutHdr.version = Math.Min(value, 2);
                }
            }
        }
    }
}

