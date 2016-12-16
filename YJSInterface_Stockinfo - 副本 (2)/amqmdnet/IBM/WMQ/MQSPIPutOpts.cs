namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIPutOpts : MQBase
    {
        public LPIPUTOPTS lpiPutOpts;
        public const int lpiPUTOPTS_ASYNC = 0x20;
        public const int lpiPUTOPTS_BLANK_PADDED = 1;
        public const int lpiPUTOPTS_DEFERRED = 4;
        public const int lpiPUTOPTS_NONE = 0;
        public const int lpiPUTOPTS_PUT_AND_FORGET = 8;
        public const int lpiPUTOPTS_REMOTE_PUB_CLUSTER = 0x40;
        public const int lpiPUTOPTS_REMOTE_PUB_HIERARCHY = 0x80;
        public const int lpiPUTOPTS_SET_RETAINED = 0x100;
        private static readonly byte[] lpiPUTOPTS_STRUCID = new byte[] { 0x4c, 80, 0x4f, 80 };
        public const int lpiPUTOPTS_SYNCPOINT_IF_YOU_LIKE = 2;
        public const int lpiPUTOPTS_TRUSTED_PUBLISH = 0x1000;
        public const int lpiPUTOPTS_VERSION1 = 1;
        public const int lpiPUTOPTS_VERSION2 = 2;
        public const int lpiPUTOPTS_VERSION3 = 3;
        internal const int MAX_SUPPORTED_VERSION = 3;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public MQSPIPutOpts() : this(0)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIPutOpts(int Options)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { Options });
            this.lpiPutOpts = new LPIPUTOPTS();
            this.lpiPutOpts.strucId = lpiPUTOPTS_STRUCID;
            this.lpiPutOpts.version = 1;
            this.lpiPutOpts.options = Options;
            this.lpiPutOpts.msgIdReservation = 0;
            this.lpiPutOpts.objectType = 0;
            this.lpiPutOpts.objectQMgrName = new byte[0x30];
            this.lpiPutOpts.objectName = new byte[0x30];
            this.lpiPutOpts.origQMgrName = new byte[0x30];
            this.lpiPutOpts.pidQMgr = new byte[0x30];
            this.lpiPutOpts.pidQName = new byte[0x30];
            this.lpiPutOpts.pidCorrelId = new byte[0x18];
        }

        public static int GetLength(NmqiEnvironment env, int version)
        {
            switch (version)
            {
                case 1:
                    return GetLengthV1();

                case 2:
                    return GetLengthV2();

                case 3:
                    return GetLengthV3();
            }
            NmqiException exception = new NmqiException(env, -1, null, 2, 0x893, null);
            env.LastException = exception;
            throw exception;
        }

        public static int GetLengthV1()
        {
            return 12;
        }

        public static int GetLengthV2()
        {
            return (GetLengthV1() + 4);
        }

        public static int GetLengthV3()
        {
            return (((GetLengthV2() + 4) + 240) + 0x18);
        }

        public int GetRequiredBufferSize(NmqiEnvironment env, int ptrSize)
        {
            uint method = 0x361;
            this.TrEntry(method, new object[] { env, ptrSize });
            int length = GetLength(env, this.lpiPutOpts.version);
            base.TrExit(method, length);
            return length;
        }

        internal int ReadStruct(NmqiEnvironment env, byte[] b, int Offset)
        {
            uint method = 0x363;
            this.TrEntry(method, new object[] { env, b, Offset });
            IntPtr zero = IntPtr.Zero;
            int cb = 0;
            int num3 = 0;
            int version = BitConverter.ToInt32(b, Offset + 4);
            cb = GetLength(env, version);
            try
            {
                zero = Marshal.AllocCoTaskMem(cb);
                if (cb > (b.Length - Offset))
                {
                    cb = b.Length - Offset;
                }
                try
                {
                    Marshal.Copy(b, Offset, zero, cb);
                    this.lpiPutOpts = (LPIPUTOPTS) Marshal.PtrToStructure(zero, typeof(LPIPUTOPTS));
                }
                finally
                {
                    Marshal.FreeCoTaskMem(zero);
                }
                num3 = Offset + GetLength(env, version);
            }
            finally
            {
                base.TrExit(method);
            }
            return num3;
        }

        internal int WriteStruct(NmqiEnvironment env, byte[] b, int Offset)
        {
            uint method = 0x362;
            this.TrEntry(method, new object[] { env, b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = GetLength(env, this.lpiPutOpts.version);
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                try
                {
                    Marshal.StructureToPtr(this.lpiPutOpts, zero, false);
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

        public int MsgIdReservation
        {
            get
            {
                return this.lpiPutOpts.msgIdReservation;
            }
            set
            {
                this.Version = 2;
                this.lpiPutOpts.msgIdReservation = value;
            }
        }

        public byte[] ObjectName
        {
            get
            {
                return this.lpiPutOpts.objectName;
            }
            set
            {
                this.lpiPutOpts.objectName = value;
            }
        }

        public byte[] ObjectQMgrName
        {
            get
            {
                return this.lpiPutOpts.objectQMgrName;
            }
            set
            {
                this.lpiPutOpts.objectQMgrName = value;
            }
        }

        public int ObjectType
        {
            get
            {
                return this.lpiPutOpts.objectType;
            }
            set
            {
                this.lpiPutOpts.objectType = value;
            }
        }

        public int Options
        {
            get
            {
                return this.lpiPutOpts.options;
            }
            set
            {
                this.lpiPutOpts.options = value;
            }
        }

        public byte[] OrigQmgrName
        {
            get
            {
                return this.lpiPutOpts.origQMgrName;
            }
            set
            {
                this.lpiPutOpts.origQMgrName = value;
            }
        }

        public byte[] PidCorrelId
        {
            get
            {
                return this.lpiPutOpts.pidCorrelId;
            }
            set
            {
                this.lpiPutOpts.pidCorrelId = value;
            }
        }

        public byte[] PidQmgr
        {
            get
            {
                return this.lpiPutOpts.pidQMgr;
            }
            set
            {
                this.lpiPutOpts.pidQMgr = value;
            }
        }

        public byte[] PidQName
        {
            get
            {
                return this.lpiPutOpts.pidQName;
            }
            set
            {
                this.lpiPutOpts.pidQName = value;
            }
        }

        public int Version
        {
            get
            {
                return this.lpiPutOpts.version;
            }
            set
            {
                if (this.lpiPutOpts.version < value)
                {
                    this.lpiPutOpts.version = Math.Min(value, 3);
                }
            }
        }
    }
}

