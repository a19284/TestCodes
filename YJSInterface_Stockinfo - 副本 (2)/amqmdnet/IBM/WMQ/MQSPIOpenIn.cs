namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIOpenIn : NmqiObject
    {
        internal const int MAX_SUPPORTED_VERSION = 1;
        private MQObjectDescriptor mqod;
        private SpiOpenOptions openOptions;
        private static readonly byte[] rfpVB_ID_OPEN_IN = new byte[] { 0x53, 80, 0x4f, 0x49 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private SPIOPENINHDR spiOpenInHdr;

        public MQSPIOpenIn(NmqiEnvironment nmqiEnv) : this(nmqiEnv, 1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv });
        }

        public MQSPIOpenIn(NmqiEnvironment nmqiEnv, int version) : base(nmqiEnv)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv, version });
            this.spiOpenInHdr = new SPIOPENINHDR();
            this.spiOpenInHdr.ID = rfpVB_ID_OPEN_IN;
            this.Version = version;
            this.openOptions = new SpiOpenOptions();
            this.mqod = new MQObjectDescriptor();
        }

        private int GetHdrLength()
        {
            return Marshal.SizeOf(this.spiOpenInHdr);
        }

        internal int GetLength()
        {
            return ((Marshal.SizeOf(this.spiOpenInHdr) + this.openOptions.GetLength()) + this.mqod.GetVersionLength());
        }

        internal int GetRequiredBufferSize()
        {
            return ((Marshal.SizeOf(this.spiOpenInHdr) + this.openOptions.GetLength()) + this.mqod.GetRequiredBufferSize());
        }

        public byte[] ToBuffer()
        {
            uint method = 0x428;
            this.TrEntry(method);
            byte[] b = new byte[this.GetRequiredBufferSize()];
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
            uint method = 0x429;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int hdrLength = this.GetHdrLength();
            this.spiOpenInHdr.length = this.GetRequiredBufferSize();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetRequiredBufferSize());
                Marshal.StructureToPtr(this.spiOpenInHdr, zero, false);
                Marshal.Copy(zero, b, Offset, hdrLength);
                Marshal.FreeCoTaskMem(zero);
                hdrLength += this.openOptions.WriteStruct(b, Offset + hdrLength);
                hdrLength += this.mqod.WriteStruct(b, Offset + hdrLength);
            }
            finally
            {
                base.TrExit(method, hdrLength);
            }
            return hdrLength;
        }

        public int DefPersistence
        {
            get
            {
                return this.openOptions.DefPersistence;
            }
            set
            {
                this.openOptions.DefPersistence = value;
            }
        }

        public int DefPutResponseType
        {
            get
            {
                return this.openOptions.DefPutResponseType;
            }
            set
            {
                this.openOptions.DefPutResponseType = value;
            }
        }

        public int DefReadAhead
        {
            get
            {
                return this.openOptions.DefReadAhead;
            }
            set
            {
                this.openOptions.DefReadAhead = value;
            }
        }

        public int LpiVersion
        {
            get
            {
                return this.openOptions.Version;
            }
            set
            {
                this.openOptions.Version = value;
            }
        }

        public MQObjectDescriptor MQOpenDescriptor
        {
            get
            {
                return this.mqod;
            }
            set
            {
                this.mqod = value;
            }
        }

        public SpiOpenOptions OpenOptions
        {
            get
            {
                return this.openOptions;
            }
            set
            {
                this.openOptions = value;
            }
        }

        public int Options
        {
            get
            {
                return this.openOptions.Options;
            }
            set
            {
                this.openOptions.Options = value;
            }
        }

        public int PropertyControl
        {
            get
            {
                return this.openOptions.PropertyControl;
            }
            set
            {
                this.openOptions.PropertyControl = value;
            }
        }

        public int Version
        {
            get
            {
                return this.spiOpenInHdr.version;
            }
            set
            {
                if (this.spiOpenInHdr.version < value)
                {
                    this.spiOpenInHdr.version = Math.Min(value, 1);
                }
            }
        }
    }
}

