namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIPutInOut : MQBase
    {
        public const int MAX_SUPPORTED_VERSION = 1;
        internal MQMessageDescriptor md;
        internal MQPutMessageOptions pmo;
        private static readonly byte[] rfpVB_ID_PUT_INOUT = new byte[] { 0x53, 80, 80, 0x55 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPIPUTINOUTHDR spiPutInOutHdr;

        public MQSPIPutInOut() : this(1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIPutInOut(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiPutInOutHdr = new SPIPUTINOUTHDR();
            this.spiPutInOutHdr.ID = rfpVB_ID_PUT_INOUT;
            this.Version = version;
            this.md = null;
            this.pmo = null;
        }

        private int GetHdrLength()
        {
            return Marshal.SizeOf(this.spiPutInOutHdr);
        }

        private int GetHdrVersionLength()
        {
            uint method = 0x290;
            this.TrEntry(method);
            if (this.Version == 1)
            {
                int hdrLength = this.GetHdrLength();
                base.TrExit(method, hdrLength, 1);
                return hdrLength;
            }
            base.TrExit(method, 0, 2);
            return 0;
        }

        internal int GetLength()
        {
            return ((Marshal.SizeOf(this.spiPutInOutHdr) + this.md.GetLength()) + this.pmo.GetLength());
        }

        internal int GetVersionLength()
        {
            return ((this.GetHdrVersionLength() + this.md.GetVersionLength()) + this.pmo.GetVersionLength());
        }

        public int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x293;
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
                this.spiPutInOutHdr = (SPIPUTINOUTHDR) Marshal.PtrToStructure(zero, typeof(SPIPUTINOUTHDR));
                Marshal.FreeCoTaskMem(zero);
                Offset += this.GetHdrVersionLength();
                Offset = this.md.ReadStruct(b, Offset);
                Offset = this.pmo.ReadStruct(b, Offset);
            }
            finally
            {
                base.TrExit(method, Offset);
            }
            return Offset;
        }

        public byte[] ToBuffer()
        {
            uint method = 0x291;
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
            uint method = 0x292;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int hdrVersionLength = this.GetHdrVersionLength();
            this.spiPutInOutHdr.length = this.GetVersionLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiPutInOutHdr, zero, false);
                Marshal.Copy(zero, b, Offset, hdrVersionLength);
                Marshal.FreeCoTaskMem(zero);
                hdrVersionLength += this.md.WriteStruct(b, Offset + hdrVersionLength);
                hdrVersionLength += this.pmo.WriteStruct(b, Offset + hdrVersionLength);
            }
            finally
            {
                base.TrExit(method, hdrVersionLength);
            }
            return hdrVersionLength;
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

        public MQPutMessageOptions PutMsgOpts
        {
            get
            {
                return this.pmo;
            }
            set
            {
                this.pmo = value;
            }
        }

        public int Version
        {
            get
            {
                return this.spiPutInOutHdr.version;
            }
            set
            {
                if (this.spiPutInOutHdr.version < value)
                {
                    this.spiPutInOutHdr.version = Math.Min(value, 1);
                }
            }
        }
    }
}

