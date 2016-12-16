namespace IBM.WMQ
{
    using System;

    public class MQSPISyncpointOpts : MQBase
    {
        private IntPtr buf;
        private static readonly byte[] lpiSPO_STRUCID = new byte[] { 0x4c, 0x53, 0x4f, 0x20 };
        public LPISPO lpiSyncpointOpts;
        internal const int MAX_SUPPORTED_VERSION = 1;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public MQSPISyncpointOpts() : this(0)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPISyncpointOpts(int Options)
        {
            this.buf = IntPtr.Zero;
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { Options });
            this.lpiSyncpointOpts = new LPISPO();
            this.lpiSyncpointOpts.strucId = lpiSPO_STRUCID;
            this.Version = 1;
            this.lpiSyncpointOpts.options = Options;
            this.lpiSyncpointOpts.action = 0;
        }

        public int Action
        {
            get
            {
                return this.lpiSyncpointOpts.action;
            }
            set
            {
                this.lpiSyncpointOpts.action = value;
            }
        }

        public int Options
        {
            get
            {
                return this.lpiSyncpointOpts.options;
            }
            set
            {
                this.lpiSyncpointOpts.options = value;
            }
        }

        public int Version
        {
            get
            {
                return this.lpiSyncpointOpts.version;
            }
            set
            {
                if (this.lpiSyncpointOpts.version < value)
                {
                    this.lpiSyncpointOpts.version = Math.Min(value, 1);
                }
            }
        }
    }
}

