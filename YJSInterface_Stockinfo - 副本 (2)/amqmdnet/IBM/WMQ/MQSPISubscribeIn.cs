namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Runtime.InteropServices;

    public class MQSPISubscribeIn : NmqiObject
    {
        private LpiSD lpisd;
        internal const int MAX_SUPPORTED_VERSION = 1;
        private static readonly byte[] rfpVB_ID_SUBSCRIBE_IN = new byte[] { 0x53, 80, 0x42, 0x49 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private SPISUBSCRIBEINHDR spiSubscribeInHdr;
        private MQSubscriptionDescriptor subDesc;

        public MQSPISubscribeIn(NmqiEnvironment nmqiEnv) : this(nmqiEnv, 1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv });
        }

        public MQSPISubscribeIn(NmqiEnvironment nmqiEnv, int version) : base(nmqiEnv)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv, version });
            this.spiSubscribeInHdr = new SPISUBSCRIBEINHDR();
            this.spiSubscribeInHdr.ID = rfpVB_ID_SUBSCRIBE_IN;
            this.Version = version;
            this.lpisd = new LpiSD();
        }

        private int GetHdrLength()
        {
            return Marshal.SizeOf(this.spiSubscribeInHdr);
        }

        internal int GetLength()
        {
            return ((Marshal.SizeOf(this.spiSubscribeInHdr) + this.lpisd.GetLength()) + this.subDesc.GetRequiredBufferSizeWOResObj());
        }

        internal int GetRequiredBufferSize()
        {
            return ((Marshal.SizeOf(this.spiSubscribeInHdr) + this.lpisd.GetRequiredBufferSize()) + this.subDesc.GetRequiredBufferSize());
        }

        public byte[] ToBuffer()
        {
            uint method = 0x431;
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
            uint method = 0x432;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int hdrLength = this.GetHdrLength();
            this.spiSubscribeInHdr.length = this.GetLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiSubscribeInHdr, zero, false);
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

        public byte[] LpiSDSubId
        {
            get
            {
                return this.lpisd.SubId;
            }
            set
            {
                this.lpisd.SubId = value;
            }
        }

        public int LpiSDVersion
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
                return this.spiSubscribeInHdr.version;
            }
            set
            {
                if (this.spiSubscribeInHdr.version < value)
                {
                    this.spiSubscribeInHdr.version = Math.Min(value, 1);
                }
            }
        }
    }
}

