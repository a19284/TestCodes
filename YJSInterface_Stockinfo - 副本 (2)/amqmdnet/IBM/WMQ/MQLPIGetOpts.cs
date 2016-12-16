namespace IBM.WMQ
{
    using System;

    public class MQLPIGetOpts : MQBase
    {
        private MQBase.lpiGETOPT getOpts = new MQBase.lpiGETOPT();
        public static readonly int lpiGETOPT_ASYNC_CONSUME = 0x40;
        public static readonly int lpiGETOPT_COMMIT = 2;
        public static readonly int lpiGETOPT_COMMIT_ASYNC = 8;
        public static readonly int lpiGETOPT_COMMIT_IF_YOU_LIKE = 4;
        public static readonly int lpiGETOPT_CURRENT_VERSION = 3;
        public static readonly int lpiGETOPT_FULL_MESSAGE = 0x80;
        public static readonly int lpiGETOPT_INHERIT = 1;
        public static readonly int lpiGETOPT_REPEATING_GET = 0x20;
        public static readonly int lpiGETOPT_SHORT_TXN = 0x10;
        private static readonly byte[] lpiGETOPT_STRUCID = new byte[] { 0x4c, 0x47, 0x4f, 0x20 };
        public static readonly int lpiGETOPT_VERSION_1 = 1;
        public static readonly int lpiGETOPT_VERSION_2 = 2;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public MQLPIGetOpts()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.getOpts.StrucId = lpiGETOPT_STRUCID;
            this.getOpts.Version = lpiGETOPT_CURRENT_VERSION;
            this.getOpts.Options = 0;
            this.getOpts.QueueEmpty = 0;
            this.getOpts.QTime = 0L;
            this.getOpts.Inherited = 0;
            this.getOpts.Padding = 0;
            this.getOpts.PropFlags = 0;
            this.getOpts.PublishFlags = 0;
            this.getOpts.Spare1 = 0;
            this.getOpts.NewMsgBufferLen = 0;
            this.getOpts.NewMsgBuffer = IntPtr.Zero;
            this.getOpts.MaxMsgBufferLen = 0;
        }

        public int GetOptions()
        {
            return this.getOpts.Options;
        }

        public void SetOptions(int options)
        {
            this.getOpts.Options = options;
        }

        public int Inherited
        {
            get
            {
                return this.getOpts.Inherited;
            }
            set
            {
                this.getOpts.Inherited = value;
            }
        }

        public int MaxMsgBufferLen
        {
            get
            {
                return this.getOpts.MaxMsgBufferLen;
            }
            set
            {
                this.getOpts.MaxMsgBufferLen = value;
            }
        }

        public IntPtr NewMsgBuffer
        {
            get
            {
                return this.getOpts.NewMsgBuffer;
            }
            set
            {
                this.getOpts.NewMsgBuffer = value;
            }
        }

        public int NewMsgBufferLen
        {
            get
            {
                return this.getOpts.NewMsgBufferLen;
            }
            set
            {
                this.getOpts.NewMsgBufferLen = value;
            }
        }

        public byte Padding
        {
            get
            {
                return this.getOpts.Padding;
            }
            set
            {
                this.getOpts.Padding = value;
            }
        }

        public byte PropFlags
        {
            get
            {
                return this.getOpts.PropFlags;
            }
            set
            {
                this.getOpts.PropFlags = value;
            }
        }

        public short PublishFlags
        {
            get
            {
                return this.getOpts.PublishFlags;
            }
            set
            {
                this.getOpts.PublishFlags = value;
            }
        }

        public ulong QTime
        {
            get
            {
                return this.getOpts.QTime;
            }
            set
            {
                this.getOpts.QTime = value;
            }
        }

        public int QueueEmpty
        {
            get
            {
                return this.getOpts.QueueEmpty;
            }
            set
            {
                this.getOpts.QueueEmpty = value;
            }
        }

        public MQBase.lpiGETOPT StructGETOPT
        {
            get
            {
                return this.getOpts;
            }
            set
            {
                this.getOpts = value;
            }
        }

        public int Version
        {
            get
            {
                return this.getOpts.Version;
            }
            set
            {
                this.getOpts.Version = value;
            }
        }
    }
}

