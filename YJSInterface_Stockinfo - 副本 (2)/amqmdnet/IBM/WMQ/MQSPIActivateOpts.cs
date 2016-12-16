namespace IBM.WMQ
{
    using System;

    public class MQSPIActivateOpts : MQBase
    {
        private IntPtr buf;
        private static readonly byte[] lpiACTIVATE_STRUCID = new byte[] { 0x4c, 0x41, 0x43, 0x54 };
        public LPIACTIVATE lpiActivateOpts;
        internal const int MAX_SUPPORTED_VERSION = 1;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public MQSPIActivateOpts() : this(0)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIActivateOpts(int Options)
        {
            this.buf = IntPtr.Zero;
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { Options });
            this.lpiActivateOpts = new LPIACTIVATE();
            this.lpiActivateOpts.strucId = lpiACTIVATE_STRUCID;
            this.Version = 1;
            this.lpiActivateOpts.options = Options;
            this.lpiActivateOpts.qName = new byte[0x30];
            this.lpiActivateOpts.qMgrName = new byte[0x30];
            this.lpiActivateOpts.msgId = new byte[0x18];
        }

        public byte[] MsgId
        {
            get
            {
                return (byte[]) this.lpiActivateOpts.msgId.Clone();
            }
            set
            {
                if (value != null)
                {
                    value.CopyTo(this.lpiActivateOpts.msgId, 0);
                }
            }
        }

        public int Options
        {
            get
            {
                return this.lpiActivateOpts.options;
            }
            set
            {
                this.lpiActivateOpts.options = value;
            }
        }

        public string QMgrName
        {
            get
            {
                return base.GetString(this.lpiActivateOpts.qMgrName);
            }
            set
            {
                if (value != null)
                {
                    base.GetBytes(value, ref this.lpiActivateOpts.qMgrName);
                }
            }
        }

        public string QName
        {
            get
            {
                return base.GetString(this.lpiActivateOpts.qName);
            }
            set
            {
                if (value != null)
                {
                    base.GetBytes(value, ref this.lpiActivateOpts.qName);
                }
            }
        }

        public int Version
        {
            get
            {
                return this.lpiActivateOpts.version;
            }
            set
            {
                if (this.lpiActivateOpts.version < value)
                {
                    this.lpiActivateOpts.version = Math.Min(value, 1);
                }
            }
        }
    }
}

