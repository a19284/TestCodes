namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Runtime.InteropServices;

    public class LpiSD : MQBase
    {
        internal structLPISD lpiSD;
        public const int lpiSD_CURRENT_VERSION = 1;
        public const int lpiSD_VERSION_1 = 1;
        internal structLPISD32 lpiSD32;
        public const int lpiSDO_DEREGISTER_ALL = 0x80;
        public const int lpiSDO_IGNORE_HOBJS = 0x10000;
        public const int lpiSDO_INCLUDE_STREAM_NAME = 0x200;
        public const int lpiSDO_INFORM_IF_RETAINED = 0x400;
        public const int lpiSDO_JOIN_EXCLUSIVE = 8;
        public const int lpiSDO_JOIN_SHARED = 0x10;
        public const int lpiSDO_LEAVE_ONLY = 0x20;
        public const int lpiSDO_LOCKED = 0x40;
        public const int lpiSDO_MIGRATED_SUB = 0x100000;
        public const int lpiSDO_NO_AUTH = 0x80000;
        public const int lpiSDO_NONE = 0;
        public const int lpiSDO_OPEN_DESTINATION = 0x20000;
        public const int lpiSDO_SUBTYPE_ADMIN = 0x40000;
        public const int lpiSDO_SUBTYPE_PROXY = 0x20000;
        public const int lpiSDO_TRADITIONAL_IDENTITY = 0x800;
        private int lpiSDStructSize;
        private const string sccsid = "%Z% %W% %I% %E% %U%";
        private const int SIZEOF_SUBID = 0x18;
        public MQCHARV subIdentity;
        private const int SUBIDSTRING_OFFSET = 0x20;
        private bool useNativePtrSz;

        public LpiSD()
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%");
            this.lpiSD = new structLPISD();
            this.lpiSD32 = new structLPISD32();
            this.StructId = new byte[] { 0x4c, 0x53, 0x44, 0x20 };
            this.Version = 1;
            this.Options = 0;
            this.subIdentity = new MQCHARV(true, false);
            this.SubId = new byte[0x18];
            this.DestReadAhead = 0;
            this.DestOpenOptions = 0;
            this.LpiSubProps = new MQBase.structLPISDSUBPROPS();
            this.useNativePtrSz = true;
        }

        internal void CopyCHARVIntoLPISD()
        {
            uint method = 0x354;
            this.TrEntry(method);
            this.lpiSD.subIdentity = this.subIdentity.mqcharv;
            base.TrExit(method);
        }

        private void copyToLpiSD()
        {
            uint method = 0x403;
            this.TrEntry(method);
            this.lpiSD.destOpenOptions = this.lpiSD32.destOpenOptions;
            this.lpiSD.destReadAhead = this.lpiSD32.destReadAhead;
            this.lpiSD.options = this.lpiSD32.options;
            this.lpiSD.structId = this.lpiSD32.structId;
            this.lpiSD.subId = this.lpiSD32.subId;
            this.lpiSD.subProps = this.lpiSD32.subProps;
            this.lpiSD.version = this.lpiSD32.version;
            base.TrExit(method);
        }

        private void copyToLpiSD32()
        {
            uint method = 0x402;
            this.TrEntry(method);
            this.lpiSD32.destOpenOptions = this.lpiSD.destOpenOptions;
            this.lpiSD32.destReadAhead = this.lpiSD.destReadAhead;
            this.lpiSD32.options = this.lpiSD.options;
            this.lpiSD32.structId = this.lpiSD.structId;
            this.lpiSD32.subId = this.lpiSD.subId;
            this.lpiSD32.subProps = this.lpiSD.subProps;
            this.lpiSD32.version = this.lpiSD.version;
            base.TrExit(method);
        }

        ~LpiSD()
        {
        }

        internal int GetLength()
        {
            uint method = 0x401;
            this.TrEntry(method);
            if (this.useNativePtrSz)
            {
                int num2 = Marshal.SizeOf(this.lpiSD);
                base.TrExit(method, num2, 1);
                return num2;
            }
            int result = Marshal.SizeOf(this.lpiSD32);
            base.TrExit(method, result, 2);
            return result;
        }

        private int GetPaddedLength(string str)
        {
            uint method = 0x353;
            this.TrEntry(method, new object[] { str });
            int length = 0;
            int num3 = 0;
            if (str != null)
            {
                length = str.Length;
                num3 = (4 - (length & 3)) & 3;
            }
            else
            {
                length = num3 = 0;
            }
            int result = length + num3;
            base.TrExit(method, result);
            return result;
        }

        public int GetRequiredBufferSize()
        {
            return (this.GetLength() + this.GetPaddedLength(this.subIdentity.VSString));
        }

        public int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x356;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int num2 = 0;
            int requiredBufferSize = this.GetRequiredBufferSize();
            if (requiredBufferSize > (b.Length - Offset))
            {
                requiredBufferSize = b.Length - Offset;
            }
            try
            {
                zero = Marshal.AllocCoTaskMem(requiredBufferSize);
                this.lpiSDStructSize = requiredBufferSize;
                Marshal.Copy(b, Offset, zero, requiredBufferSize);
                if (this.useNativePtrSz)
                {
                    this.lpiSD = (structLPISD) Marshal.PtrToStructure(zero, typeof(structLPISD));
                    this.copyToLpiSD32();
                }
                else
                {
                    this.lpiSD32 = (structLPISD32) Marshal.PtrToStructure(zero, typeof(structLPISD32));
                    this.copyToLpiSD();
                }
                num2 = Offset + requiredBufferSize;
                this.subIdentity.ReadStruct(b, Offset, Offset + 0x20);
                this.subIdentity.GetEndPosAligned(Offset);
            }
            finally
            {
                Marshal.FreeCoTaskMem(zero);
                base.TrExit(method);
            }
            return num2;
        }

        public int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x355;
            this.TrEntry(method, new object[] { b, Offset });
            int stringPos = Offset;
            int requiredBufferSize = this.GetRequiredBufferSize();
            int length = this.GetLength();
            IntPtr zero = IntPtr.Zero;
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                this.lpiSDStructSize = length;
                if (this.useNativePtrSz)
                {
                    Marshal.StructureToPtr(this.lpiSD, zero, false);
                }
                else
                {
                    Marshal.StructureToPtr(this.lpiSD32, zero, false);
                }
                Marshal.Copy(zero, b, Offset, length);
                stringPos += length;
                stringPos = this.subIdentity.WriteStruct(b, Offset, Offset + 0x20, stringPos);
            }
            finally
            {
                Marshal.FreeCoTaskMem(zero);
                base.TrExit(method);
            }
            zero = IntPtr.Zero;
            return requiredBufferSize;
        }

        public int DestOpenOptions
        {
            get
            {
                return this.lpiSD.destOpenOptions;
            }
            set
            {
                this.lpiSD.destOpenOptions = value;
                this.lpiSD32.destOpenOptions = value;
            }
        }

        public int DestReadAhead
        {
            get
            {
                return this.lpiSD.destReadAhead;
            }
            set
            {
                this.lpiSD.destReadAhead = value;
                this.lpiSD32.destReadAhead = value;
            }
        }

        public MQBase.structLPISDSUBPROPS LpiSubProps
        {
            get
            {
                return this.lpiSD.subProps;
            }
            set
            {
                this.lpiSD.subProps = value;
                this.lpiSD32.subProps = value;
            }
        }

        public int Options
        {
            get
            {
                return this.lpiSD.options;
            }
            set
            {
                this.lpiSD.options = value;
                this.lpiSD32.options = value;
            }
        }

        public byte[] StructId
        {
            get
            {
                return this.lpiSD.structId;
            }
            set
            {
                this.lpiSD.structId = value;
                this.lpiSD32.structId = value;
            }
        }

        public byte[] SubId
        {
            get
            {
                return this.lpiSD.subId;
            }
            set
            {
                this.lpiSD.subId = value;
                this.lpiSD32.subId = value;
            }
        }

        public MQCHARV SubIdentity
        {
            get
            {
                return this.subIdentity;
            }
            set
            {
                this.subIdentity = value;
            }
        }

        public bool UseNativePtrSz
        {
            get
            {
                return this.useNativePtrSz;
            }
            set
            {
                this.useNativePtrSz = value;
                this.subIdentity.UseNativePtrSz = value;
            }
        }

        public int Version
        {
            get
            {
                return this.lpiSD.version;
            }
            set
            {
                this.lpiSD.version = value;
                this.lpiSD32.version = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct structLPISD
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            public byte[] structId;
            public int version;
            public int options;
            public MQBase.structMQCHARV subIdentity;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x18)]
            public byte[] subId;
            public int destReadAhead;
            public int destOpenOptions;
            public MQBase.structLPISDSUBPROPS subProps;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct structLPISD32
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            public byte[] structId;
            public int version;
            public int options;
            public MQBase.structMQCHARV32 subIdentity;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x18)]
            public byte[] subId;
            public int destReadAhead;
            public int destOpenOptions;
            public MQBase.structLPISDSUBPROPS subProps;
        }
    }
}

