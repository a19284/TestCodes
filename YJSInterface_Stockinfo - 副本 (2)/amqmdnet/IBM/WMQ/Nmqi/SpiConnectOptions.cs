namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Runtime.InteropServices;

    public class SpiConnectOptions : MQBase
    {
        public const int lpiAUTH_TOKEN_NULL = -1;
        public const int lpiCONNECTION_CLASS_CLIENT = 2;
        public const int lpiCONNECTION_CLASS_DEFAULT = 0;
        public const int lpiCONNECTION_CLASS_SERVER = 1;
        private const int lpiPrivConnVERSION1 = 1;
        private const int lpiPrivConnVERSION2 = 2;
        public const int lpiPrivOpt_ALLOW_CANCEL = 0x80000;
        public const int lpiPrivOpt_ALLOW_PENDING_DISC = 0x40;
        public const int lpiPrivOpt_ASYNC_COMMIT = 0x10000000;
        public const int lpiPrivOpt_EARLY = 0x400000;
        public const int lpiPrivOpt_IGNORE_FORCED_SHUTDOWN = 0x100000;
        public const int lpiPrivOpt_INTERNAL = 1;
        public const int lpiPrivOpt_NO_MONITORING = 0x20000000;
        public const int lpiPrivOpt_NORUNAWAY = 0x10;
        public const int lpiPrivOpt_PRESUME_ABORT = 0x8000;
        public const int lpiPrivOpt_QUIESCE_NOWAIT = 0x200000;
        public const int lpiPrivOpt_REMOTE_PUB_CLUSTER = 0x40000000;
        public const uint lpiPrivOpt_REMOTE_PUB_HIERARCHY = 0x80000000;
        public const int lpiPrivOpt_RUNTIME = 0x80;
        public const int lpiPrivOpt_SEGMENT_NOCONVERT = 2;
        public const int lpiPrivOpt_SHUTDOWN_ENABLED = 0x20;
        public const int lpiPrivOpt_TRUSTED = 4;
        public const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal structSPICONNECTOPTIONS spiConnectOptions;

        public SpiConnectOptions()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.spiConnectOptions = new structSPICONNECTOPTIONS();
            this.spiConnectOptions.structId = new byte[] { 0x4c, 0x43, 0x4e, 0x4f };
            this.spiConnectOptions.version = 1;
            this.spiConnectOptions.options = 0;
            this.spiConnectOptions.connectionClass = 0;
            this.spiConnectOptions.authToken = 0;
            this.spiConnectOptions.uuid = new byte[0x30];
            this.spiConnectOptions.cluster = new byte[0x30];
            this.spiConnectOptions.pRecipientUUID = IntPtr.Zero;
            this.spiConnectOptions.qmOffset = 0;
            this.spiConnectOptions.pChannelName = IntPtr.Zero;
            this.spiConnectOptions.environment = IntPtr.Zero;
            this.spiConnectOptions.channel = IntPtr.Zero;
            this.spiConnectOptions.connectionName = IntPtr.Zero;
            this.spiConnectOptions.cmdLevel = 0;
            this.spiConnectOptions.subCmdLevel = new int[4];
        }

        ~SpiConnectOptions()
        {
        }

        public static int GetLength(NmqiEnvironment env, int version, int ptrSize)
        {
            switch (version)
            {
                case 1:
                    return GetLengthV1(ptrSize);

                case 2:
                    return GetLengthV2(ptrSize);
            }
            NmqiException exception = new NmqiException(env, -1, null, 2, 0x893, null);
            env.LastException = exception;
            throw exception;
        }

        public static int GetLengthV1(int ptrSize)
        {
            int num = 120 + (2 * ptrSize);
            int num2 = NmqiTools.Padding(ptrSize, 0, 8, 0x18);
            return (num + num2);
        }

        public static int GetLengthV2(int ptrSize)
        {
            int num = (GetLengthV1(ptrSize) + 20) + (3 * ptrSize);
            int num2 = NmqiTools.Padding(ptrSize, 0, 4, 12);
            return (num + num2);
        }

        public int GetRequiredBufferSize(NmqiEnvironment env, int ptrSize)
        {
            uint method = 0x49c;
            this.TrEntry(method, new object[] { env, ptrSize });
            int result = GetLength(env, this.spiConnectOptions.version, ptrSize);
            base.TrExit(method, result);
            return result;
        }

        internal int ReadStruct(NmqiEnvironment env, byte[] b, int Offset)
        {
            uint method = 0x49e;
            this.TrEntry(method, new object[] { env, b, Offset });
            IntPtr zero = IntPtr.Zero;
            int cb = 0;
            int version = BitConverter.ToInt32(b, Offset + 4);
            cb = GetLength(env, version, 4);
            zero = Marshal.AllocCoTaskMem(cb);
            if (cb > (b.Length - Offset))
            {
                cb = b.Length - Offset;
            }
            try
            {
                Marshal.Copy(b, Offset, zero, cb);
                this.spiConnectOptions = (structSPICONNECTOPTIONS) Marshal.PtrToStructure(zero, typeof(structSPICONNECTOPTIONS));
            }
            finally
            {
                Marshal.FreeCoTaskMem(zero);
                base.TrExit(method);
            }
            return (Offset + GetLength(env, version, 4));
        }

        internal int WriteStruct(NmqiEnvironment env, byte[] b, int Offset)
        {
            uint method = 0x49d;
            this.TrEntry(method, new object[] { env, b, Offset });
            IntPtr zero = IntPtr.Zero;
            int cb = GetLength(env, this.spiConnectOptions.version, 4);
            zero = Marshal.AllocCoTaskMem(cb);
            try
            {
                Marshal.StructureToPtr(this.spiConnectOptions, zero, false);
                Marshal.Copy(zero, b, Offset, cb);
            }
            finally
            {
                Marshal.FreeCoTaskMem(zero);
                base.TrExit(method);
            }
            return cb;
        }

        public int Authtoken
        {
            get
            {
                return this.spiConnectOptions.authToken;
            }
            set
            {
                this.spiConnectOptions.authToken = value;
            }
        }

        public IntPtr Channel
        {
            get
            {
                return this.spiConnectOptions.channel;
            }
            set
            {
                this.spiConnectOptions.channel = value;
            }
        }

        public byte[] Cluster
        {
            get
            {
                return this.spiConnectOptions.cluster;
            }
            set
            {
                this.spiConnectOptions.cluster = value;
            }
        }

        public int CmdLevel
        {
            get
            {
                return this.spiConnectOptions.cmdLevel;
            }
            set
            {
                this.spiConnectOptions.cmdLevel = value;
            }
        }

        public int ConnectionClass
        {
            get
            {
                return this.spiConnectOptions.connectionClass;
            }
            set
            {
                this.spiConnectOptions.connectionClass = value;
            }
        }

        public IntPtr ConnectionName
        {
            get
            {
                return this.spiConnectOptions.connectionName;
            }
            set
            {
                this.spiConnectOptions.connectionName = value;
            }
        }

        public IntPtr Environment
        {
            get
            {
                return this.spiConnectOptions.environment;
            }
            set
            {
                this.spiConnectOptions.environment = value;
            }
        }

        public int Options
        {
            get
            {
                return this.spiConnectOptions.options;
            }
            set
            {
                this.spiConnectOptions.options = value;
            }
        }

        public IntPtr PChannelName
        {
            get
            {
                return this.spiConnectOptions.pChannelName;
            }
            set
            {
                this.spiConnectOptions.pChannelName = value;
            }
        }

        public IntPtr PRecipientUUID
        {
            get
            {
                return this.spiConnectOptions.pRecipientUUID;
            }
            set
            {
                this.spiConnectOptions.pRecipientUUID = value;
            }
        }

        public int QMOffset
        {
            get
            {
                return this.spiConnectOptions.qmOffset;
            }
            set
            {
                this.spiConnectOptions.qmOffset = value;
            }
        }

        public int[] SubCmdLevel
        {
            get
            {
                return this.spiConnectOptions.subCmdLevel;
            }
            set
            {
                this.spiConnectOptions.subCmdLevel = value;
            }
        }

        public byte[] Uuid
        {
            get
            {
                return this.spiConnectOptions.uuid;
            }
            set
            {
                this.spiConnectOptions.uuid = value;
            }
        }

        public int Version
        {
            get
            {
                return this.spiConnectOptions.version;
            }
            set
            {
                this.spiConnectOptions.version = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct structSPICONNECTOPTIONS
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            public byte[] structId;
            public int version;
            public int options;
            public int connectionClass;
            public int authToken;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            public byte[] uuid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            public byte[] cluster;
            public IntPtr pRecipientUUID;
            public int qmOffset;
            public IntPtr pChannelName;
            public IntPtr environment;
            public IntPtr channel;
            public IntPtr connectionName;
            public int cmdLevel;
            public int[] subCmdLevel;
        }
    }
}

