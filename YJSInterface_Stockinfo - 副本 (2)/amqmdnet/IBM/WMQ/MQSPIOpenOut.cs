namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIOpenOut : NmqiObject
    {
        internal const int MAX_SUPPORTED_VERSION = 1;
        private MQObjectDescriptor mqod;
        private SpiOpenOptions openOptions;
        private static readonly byte[] rfpVB_ID_OPEN_OUT = new byte[] { 0x53, 80, 0x4f, 0x4f };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private SPIOPENOUTHDR spiOpenOutHdr;

        public MQSPIOpenOut(NmqiEnvironment nmqiEnv) : this(nmqiEnv, 1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv });
        }

        public MQSPIOpenOut(NmqiEnvironment nmqiEnv, int version) : base(nmqiEnv)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv, version });
            this.spiOpenOutHdr = new SPIOPENOUTHDR();
            this.spiOpenOutHdr.ID = rfpVB_ID_OPEN_OUT;
            this.Version = version;
            this.openOptions = new SpiOpenOptions();
            this.mqod = new MQObjectDescriptor();
        }

        private int GetHdrLength()
        {
            return Marshal.SizeOf(this.spiOpenOutHdr);
        }

        internal int GetLength()
        {
            return ((Marshal.SizeOf(this.spiOpenOutHdr) + this.openOptions.GetLength()) + this.mqod.GetVersionLength());
        }

        internal int GetRequiredBufferSize()
        {
            return ((Marshal.SizeOf(this.spiOpenOutHdr) + this.openOptions.GetLength()) + this.mqod.GetRequiredBufferSize());
        }

        public int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x430;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int hdrLength = this.GetHdrLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetRequiredBufferSize());
                if (hdrLength > (b.Length - Offset))
                {
                    hdrLength = b.Length - Offset;
                }
                Marshal.Copy(b, Offset, zero, hdrLength);
                this.spiOpenOutHdr = (SPIOPENOUTHDR) Marshal.PtrToStructure(zero, typeof(SPIOPENOUTHDR));
                Marshal.FreeCoTaskMem(zero);
                Offset += this.GetHdrLength();
                Offset = this.openOptions.ReadStruct(b, Offset);
                Offset = this.mqod.ReadStruct(b, Offset, b.Length);
            }
            finally
            {
                base.TrExit(method, Offset);
            }
            return Offset;
        }

        public byte[] ToBuffer()
        {
            uint method = 0x42e;
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
            uint method = 0x42f;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int hdrLength = this.GetHdrLength();
            try
            {
                this.spiOpenOutHdr.length = this.GetRequiredBufferSize();
                zero = Marshal.AllocCoTaskMem(this.GetRequiredBufferSize());
                Marshal.StructureToPtr(this.spiOpenOutHdr, zero, false);
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

        public bool UseNativePtrSz
        {
            get
            {
                return this.mqod.UseNativePtrSz;
            }
            set
            {
                this.mqod.UseNativePtrSz = value;
            }
        }

        public int Version
        {
            get
            {
                return this.spiOpenOutHdr.version;
            }
            set
            {
                if (this.spiOpenOutHdr.version < value)
                {
                    this.spiOpenOutHdr.version = Math.Min(value, 1);
                }
            }
        }
    }
}

