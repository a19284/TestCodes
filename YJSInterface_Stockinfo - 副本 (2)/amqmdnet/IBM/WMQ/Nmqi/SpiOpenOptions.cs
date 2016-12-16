namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Runtime.InteropServices;

    public class SpiOpenOptions : MQBase
    {
        public const int lpiOPENOPT_VERSION_1 = 1;
        public const int lpiOPENOPTS_NONE = 0;
        public const int lpiOPENOPTS_SAVE_IDENTITY_CTXT = 1;
        public const int lpiOPENOPTS_SAVE_ORIGIN_CTXT = 2;
        public const int lpiOPENOPTS_SAVE_USER_CTXT = 4;
        public const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal MQBase.structSPIOPENOPTIONS spiOpenOptions;

        public SpiOpenOptions()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.spiOpenOptions = new MQBase.structSPIOPENOPTIONS();
            this.spiOpenOptions.structId = new byte[] { 0x4c, 80, 0x4f, 0x4f };
            this.spiOpenOptions.version = 1;
            this.spiOpenOptions.options = 0;
            this.spiOpenOptions.lpiOpts = 0;
            this.spiOpenOptions.defPersistence = -1;
            this.spiOpenOptions.defPutResponseType = -1;
            this.spiOpenOptions.defReadAhead = -1;
            this.spiOpenOptions.propertyControl = -1;
        }

        ~SpiOpenOptions()
        {
        }

        public int GetLength()
        {
            return Marshal.SizeOf(this.spiOpenOptions);
        }

        public int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x4a3;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                if (length > (b.Length - Offset))
                {
                    length = b.Length - Offset;
                }
                try
                {
                    Marshal.Copy(b, Offset, zero, length);
                    this.spiOpenOptions = (MQBase.structSPIOPENOPTIONS) Marshal.PtrToStructure(zero, typeof(MQBase.structSPIOPENOPTIONS));
                }
                finally
                {
                    Marshal.FreeCoTaskMem(zero);
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return (Offset + this.GetLength());
        }

        public int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x4a2;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                try
                {
                    Marshal.StructureToPtr(this.spiOpenOptions, zero, false);
                    Marshal.Copy(zero, b, Offset, length);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(zero);
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return length;
        }

        public int DefPersistence
        {
            get
            {
                return this.spiOpenOptions.defPersistence;
            }
            set
            {
                this.spiOpenOptions.defPersistence = value;
            }
        }

        public int DefPutResponseType
        {
            get
            {
                return this.spiOpenOptions.defPutResponseType;
            }
            set
            {
                this.spiOpenOptions.defPutResponseType = value;
            }
        }

        public int DefReadAhead
        {
            get
            {
                return this.spiOpenOptions.defReadAhead;
            }
            set
            {
                this.spiOpenOptions.defReadAhead = value;
            }
        }

        public int LpiOpts
        {
            get
            {
                return this.spiOpenOptions.lpiOpts;
            }
            set
            {
                this.spiOpenOptions.lpiOpts = value;
            }
        }

        public int Options
        {
            get
            {
                return this.spiOpenOptions.options;
            }
            set
            {
                this.spiOpenOptions.options = value;
            }
        }

        public int PropertyControl
        {
            get
            {
                return this.spiOpenOptions.propertyControl;
            }
            set
            {
                this.spiOpenOptions.propertyControl = value;
            }
        }

        public MQBase.structSPIOPENOPTIONS StructSPIOPENOPTIONS
        {
            get
            {
                return this.spiOpenOptions;
            }
            set
            {
                this.spiOpenOptions = value;
            }
        }

        public int Version
        {
            get
            {
                return this.spiOpenOptions.version;
            }
            set
            {
                this.spiOpenOptions.version = value;
            }
        }
    }
}

