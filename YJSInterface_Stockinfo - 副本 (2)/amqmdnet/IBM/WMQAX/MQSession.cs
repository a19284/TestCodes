namespace IBM.WMQAX
{
    using IBM.WMQ;
    using System;

    public class MQSession : MQBaseObject
    {
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public MQSession()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public IBM.WMQAX.MQGetMessageOptions AccessGetMessageOptions()
        {
            IBM.WMQAX.MQGetMessageOptions options2;
            uint method = 0x13;
            this.TrEntry(method);
            try
            {
                options2 = new IBM.WMQAX.MQGetMessageOptions();
            }
            finally
            {
                base.TrExit(method);
            }
            return options2;
        }

        public IBM.WMQAX.MQMessage AccessMessage()
        {
            IBM.WMQAX.MQMessage message2;
            uint method = 0x11;
            this.TrEntry(method);
            try
            {
                message2 = new IBM.WMQAX.MQMessage();
            }
            finally
            {
                base.TrExit(method);
            }
            return message2;
        }

        public IBM.WMQAX.MQPutMessageOptions AccessPutMessageOptions()
        {
            IBM.WMQAX.MQPutMessageOptions options2;
            uint method = 0x12;
            this.TrEntry(method);
            try
            {
                options2 = new IBM.WMQAX.MQPutMessageOptions();
            }
            finally
            {
                base.TrExit(method);
            }
            return options2;
        }

        public IBM.WMQAX.MQQueueManager AccessQueueManager(string qm)
        {
            IBM.WMQAX.MQQueueManager manager2;
            uint method = 0x10;
            this.TrEntry(method, new object[] { qm });
            try
            {
                manager2 = new IBM.WMQAX.MQQueueManager(qm);
            }
            finally
            {
                base.TrExit(method);
            }
            return manager2;
        }

        public string ReasonCodeName(int reason)
        {
            return CommonServices.ReasonCodeName(reason);
        }

        public int ExceptionThreshold
        {
            get
            {
                return MQBase.s_ExceptionThreshold;
            }
            set
            {
                MQBase.s_ExceptionThreshold = value;
            }
        }

        public override int ReasonCode
        {
            get
            {
                return base.unsafe_reason;
            }
            set
            {
                base.unsafe_reason = value;
            }
        }
    }
}

