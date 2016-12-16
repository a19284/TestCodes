namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Runtime.InteropServices;

    public class MQSPISubscribeOut : NmqiObject
    {
        private LpiSD lpisd;
        internal const int MAX_SUPPORTED_VERSION = 1;
        private static readonly byte[] rfpVB_ID_SUBSCRIBE_OUT = new byte[] { 0x53, 80, 0x42, 0x4f };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private SPISUBSCRIBEOUTHDR spiSubscribeOutHdr;
        private MQSubscriptionDescriptor subDesc;

        public MQSPISubscribeOut(NmqiEnvironment nmqiEnv) : this(nmqiEnv, 1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv });
        }

        public MQSPISubscribeOut(NmqiEnvironment nmqiEnv, int version) : base(nmqiEnv)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv, version });
            this.spiSubscribeOutHdr = new SPISUBSCRIBEOUTHDR();
            this.spiSubscribeOutHdr.ID = rfpVB_ID_SUBSCRIBE_OUT;
            this.Version = version;
            this.lpisd = new LpiSD();
            this.subDesc = new MQSubscriptionDescriptor();
        }

        private int GetHdrLength()
        {
            return Marshal.SizeOf(this.spiSubscribeOutHdr);
        }

        internal int GetLength()
        {
            return ((Marshal.SizeOf(this.spiSubscribeOutHdr) + this.lpisd.GetLength()) + this.subDesc.GetRequiredBufferSize());
        }

        public int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x439;
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
                this.spiSubscribeOutHdr = (SPISUBSCRIBEOUTHDR) Marshal.PtrToStructure(zero, typeof(SPISUBSCRIBEOUTHDR));
                Marshal.FreeCoTaskMem(zero);
                Offset += this.GetHdrLength();
                Offset = this.lpisd.ReadStruct(b, Offset);
                Offset = this.subDesc.ReadStruct(b, Offset, b.Length);
            }
            finally
            {
                base.TrExit(method, Offset);
            }
            return Offset;
        }

        public byte[] ToBuffer()
        {
            uint method = 0x437;
            this.TrEntry(method);
            byte[] b = new byte[this.GetLength()];
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
            uint method = 0x438;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int hdrLength = this.GetHdrLength();
            this.spiSubscribeOutHdr.length = this.GetLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiSubscribeOutHdr, zero, false);
                Marshal.Copy(zero, b, Offset, hdrLength);
                Marshal.FreeCoTaskMem(zero);
                hdrLength += this.lpisd.WriteStruct(b, Offset + hdrLength);
                hdrLength += this.subDesc.WriteStruct(b, Offset + hdrLength);
            }
            finally
            {
                base.TrExit(method, hdrLength);
            }
            return hdrLength;
        }

        public int Hsub
        {
            get
            {
                return this.spiSubscribeOutHdr.hSub;
            }
        }

        public int Length
        {
            get
            {
                return this.GetLength();
            }
        }

        public int LpiDestOpenOptions
        {
            get
            {
                return this.lpisd.DestOpenOptions;
            }
            set
            {
                this.lpisd.DestOpenOptions = value;
            }
        }

        public int LpiDestReadAhead
        {
            get
            {
                return this.lpisd.DestReadAhead;
            }
            set
            {
                this.lpisd.DestReadAhead = value;
            }
        }

        public int LpiPSPropertyStyle
        {
            get
            {
                return this.lpisd.LpiSubProps.PsPropertyStyle;
            }
            set
            {
                MQBase.structLPISDSUBPROPS lpiSubProps = this.lpisd.LpiSubProps;
                lpiSubProps.PsPropertyStyle = value;
                this.lpisd.LpiSubProps = lpiSubProps;
            }
        }

        public LpiSD Lpisd
        {
            get
            {
                return this.lpisd;
            }
            set
            {
                this.lpisd = value;
            }
        }

        public int LpiSDOptions
        {
            get
            {
                return this.lpisd.Options;
            }
            set
            {
                this.lpisd.Options = value;
            }
        }

        public int LpiVersion
        {
            get
            {
                return this.lpisd.Version;
            }
            set
            {
                this.lpisd.Version = value;
            }
        }

        public MQSubscriptionDescriptor SubDesc
        {
            get
            {
                return this.subDesc;
            }
            set
            {
                this.subDesc = value;
            }
        }

        public int Version
        {
            get
            {
                return this.spiSubscribeOutHdr.version;
            }
            set
            {
                if (this.spiSubscribeOutHdr.version < value)
                {
                    this.spiSubscribeOutHdr.version = Math.Min(value, 1);
                }
            }
        }
    }
}

