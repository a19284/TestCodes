namespace IBM.WMQ
{
    using System;

    public class MQSPIGetOpts : MQBase
    {
        private IntPtr buf;
        public LPIGETOPT lpiGetOpt;
        public static readonly int lpiGETOPT_ASYNC_CONSUME = 0x40;
        public static readonly int lpiGETOPT_COMMIT = 2;
        public static readonly int lpiGETOPT_COMMIT_ASYNC = 8;
        public static readonly int lpiGETOPT_COMMIT_IF_YOU_LIKE = 4;
        public static readonly int lpiGETOPT_FULL_MESSAGE = 0x80;
        public static readonly int lpiGETOPT_INHERIT = 1;
        public static readonly int lpiGETOPT_REPEATING_GET = 0x20;
        public static readonly int lpiGETOPT_SHORT_TXN = 0x10;
        private static readonly byte[] lpiGETOPT_STRUCID = new byte[] { 0x4c, 0x47, 0x4f, 0x20 };
        internal const int MAX_SUPPORTED_VERSION = 3;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public MQSPIGetOpts() : this(0)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIGetOpts(int Options)
        {
            this.buf = IntPtr.Zero;
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { Options });
            this.lpiGetOpt = new LPIGETOPT();
            this.lpiGetOpt.strucId = lpiGETOPT_STRUCID;
            this.Version = 3;
            this.lpiGetOpt.options = Options;
            this.lpiGetOpt.queueEmpty = 0;
            this.lpiGetOpt.qTime = 0L;
            this.lpiGetOpt.inherited = 0;
        }

        public int Inherited
        {
            get
            {
                return this.lpiGetOpt.inherited;
            }
            set
            {
                this.lpiGetOpt.inherited = value;
            }
        }

        public int Options
        {
            get
            {
                return this.lpiGetOpt.options;
            }
            set
            {
                this.lpiGetOpt.options = value;
            }
        }

        public ulong QTime
        {
            get
            {
                return this.lpiGetOpt.qTime;
            }
            set
            {
                this.lpiGetOpt.qTime = value;
            }
        }

        public int QueueEmpty
        {
            get
            {
                return this.lpiGetOpt.queueEmpty;
            }
            set
            {
                this.lpiGetOpt.queueEmpty = value;
            }
        }

        public int Version
        {
            get
            {
                return this.lpiGetOpt.version;
            }
            set
            {
                if (this.lpiGetOpt.version < value)
                {
                    this.lpiGetOpt.version = Math.Min(value, 3);
                }
            }
        }
    }
}

