namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Runtime.InteropServices;

    public class SpiGetOptions : MQBase
    {
        public const int lpiGETOPT_ASYNC_CONSUME = 0x40;
        public const int lpiGETOPT_CANCEL_WAIT = 0x400;
        public const int lpiGETOPT_COMMIT = 2;
        public const int lpiGETOPT_COMMIT_ASYNC = 8;
        public const int lpiGETOPT_COMMIT_IF_YOU_LIKE = 4;
        public const int lpiGETOPT_FULL_MESSAGE = 0x80;
        public const int lpiGETOPT_INHERIT = 1;
        public const int lpiGETOPT_QTIME = 0x100;
        public const int lpiGETOPT_REPEATING_GET = 0x20;
        public const int lpiGETOPT_RESET_QTIME = 0x200;
        public const int lpiGETOPT_RESIZE_BUF = 0x800;
        public const int lpiGETOPT_SHORT_TXN = 0x10;
        public const int lpiGETOPT_VERSION_2 = 2;
        public const int lpiGETOPT_VERSION_3 = 3;
        public const int lpiGETOPT_VERSION_4 = 4;
        public const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal structSPIGETOPTIONS spiGetOptions;

        public SpiGetOptions()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.spiGetOptions = new structSPIGETOPTIONS();
            this.spiGetOptions.structId = new byte[] { 0x4c, 0x47, 0x4f, 0x20 };
            this.spiGetOptions.version = 2;
            this.spiGetOptions.options = 0;
            this.spiGetOptions.queueEmpty = 0;
            this.spiGetOptions.qtime = 0L;
            this.spiGetOptions.inherited = 0;
            this.spiGetOptions.publishFlags = 0;
            this.spiGetOptions.padding = 0;
            this.spiGetOptions.newMsgBuffer = IntPtr.Zero;
            this.spiGetOptions.newMsgBufferLen = 0;
            this.spiGetOptions.maxMsgBufferLen = 0;
            this.spiGetOptions.spare1 = 0;
        }

        ~SpiGetOptions()
        {
        }

        public static int GetLength(NmqiEnvironment env, int version, int ptrSize)
        {
            switch (version)
            {
                case 2:
                    return GetLengthV2();

                case 3:
                    return GetLengthV3();

                case 4:
                    return GetLengthV4(ptrSize);
            }
            NmqiException exception = new NmqiException(env, -1, null, 2, 0x893, null);
            env.LastException = exception;
            throw exception;
        }

        private static int GetLengthV2()
        {
            return 0x20;
        }

        private static int GetLengthV3()
        {
            return (GetLengthV2() + 4);
        }

        private static int GetLengthV4(int ptrSize)
        {
            return ((GetLengthV3() + 12) + ptrSize);
        }

        public int GetRequiredBufferSize(NmqiEnvironment env, int ptrSize)
        {
            uint method = 0x49f;
            this.TrEntry(method, new object[] { env, ptrSize });
            int result = GetLength(env, this.spiGetOptions.version, ptrSize);
            base.TrExit(method, result);
            return result;
        }

        internal int ReadStruct(NmqiEnvironment env, byte[] b, int Offset)
        {
            uint method = 0x4a1;
            this.TrEntry(method, new object[] { env, b, Offset });
            IntPtr zero = IntPtr.Zero;
            int cb = 0;
            int version = BitConverter.ToInt32(b, Offset + 4);
            cb = GetLength(env, version, 4);
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
                    this.spiGetOptions = (structSPIGETOPTIONS) Marshal.PtrToStructure(zero, typeof(structSPIGETOPTIONS));
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
            return (Offset + GetLength(env, version, 4));
        }

        internal int WriteStruct(NmqiEnvironment env, byte[] b, int Offset)
        {
            uint method = 0x4a0;
            this.TrEntry(method, new object[] { env, b, Offset });
            IntPtr zero = IntPtr.Zero;
            int cb = GetLength(env, this.spiGetOptions.version, 4);
            try
            {
                zero = Marshal.AllocCoTaskMem(cb);
                try
                {
                    Marshal.StructureToPtr(this.spiGetOptions, zero, false);
                    Marshal.Copy(zero, b, Offset, cb);
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
            return cb;
        }

        public int Inherited
        {
            get
            {
                return this.spiGetOptions.inherited;
            }
            set
            {
                this.spiGetOptions.inherited = value;
            }
        }

        public IntPtr NewMsgBuffer
        {
            get
            {
                return this.spiGetOptions.newMsgBuffer;
            }
            set
            {
                this.spiGetOptions.newMsgBuffer = value;
            }
        }

        public int NewMsgBufferLen
        {
            get
            {
                return this.spiGetOptions.newMsgBufferLen;
            }
            set
            {
                this.spiGetOptions.newMsgBufferLen = value;
            }
        }

        public int Options
        {
            get
            {
                return this.spiGetOptions.options;
            }
            set
            {
                this.spiGetOptions.options = value;
            }
        }

        public int PChannelName
        {
            get
            {
                return this.spiGetOptions.maxMsgBufferLen;
            }
            set
            {
                this.spiGetOptions.maxMsgBufferLen = value;
            }
        }

        public ushort PublishFlags
        {
            get
            {
                return this.spiGetOptions.publishFlags;
            }
            set
            {
                this.spiGetOptions.publishFlags = value;
            }
        }

        public ulong Qtime
        {
            get
            {
                return this.spiGetOptions.qtime;
            }
            set
            {
                this.spiGetOptions.qtime = value;
            }
        }

        public int QueueEmpty
        {
            get
            {
                return this.spiGetOptions.queueEmpty;
            }
            set
            {
                this.spiGetOptions.queueEmpty = value;
            }
        }

        public int Version
        {
            get
            {
                return this.spiGetOptions.version;
            }
            set
            {
                this.spiGetOptions.version = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct structSPIGETOPTIONS
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            public byte[] structId;
            public int version;
            public int options;
            public int queueEmpty;
            public ulong qtime;
            public int inherited;
            public ushort publishFlags;
            public ushort padding;
            public IntPtr newMsgBuffer;
            public int newMsgBufferLen;
            public int maxMsgBufferLen;
            public int spare1;
        }
    }
}

