namespace IBM.WMQ
{
    using System;

    public class MQBeginOptions : MQBaseObject
    {
        private MQBase.MQBO mqBO = new MQBase.MQBO();
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public MQBeginOptions()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.mqBO.StrucID = new byte[] { 0x42, 0x4f, 0x20, 0x20 };
            this.mqBO.Version = 1;
            this.mqBO.Options = 0;
        }

        public int Options
        {
            get
            {
                return this.mqBO.Options;
            }
            set
            {
                this.mqBO.Options = value;
            }
        }

        public MQBase.MQBO StructMQBO
        {
            get
            {
                return this.mqBO;
            }
            set
            {
                this.mqBO = value;
            }
        }

        public int Version
        {
            get
            {
                return this.mqBO.Version;
            }
            set
            {
                this.mqBO.Version = value;
            }
        }
    }
}

