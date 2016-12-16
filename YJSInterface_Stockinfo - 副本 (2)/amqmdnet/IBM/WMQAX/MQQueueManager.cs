namespace IBM.WMQAX
{
    using IBM.WMQ;
    using System;

    public class MQQueueManager : IBM.WMQ.MQQueueManager
    {
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public MQQueueManager()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQQueueManager(string m) : base(m)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { m });
        }

        public MQQueueManager(string queueManagerName, int Options) : base(queueManagerName, Options)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { queueManagerName, Options });
        }

        public IBM.WMQAX.MQQueue AccessQueue(string queueName, int openOptions)
        {
            return this.AccessQueue(queueName, openOptions, null, null, null);
        }

        public IBM.WMQAX.MQQueue AccessQueue(string queueName, int openOptions, string queueManagerName, string dynamicQueueName, string alternateUserId)
        {
            return new IBM.WMQAX.MQQueue(this, queueName, openOptions, queueManagerName, dynamicQueueName, alternateUserId);
        }

        public override void Disconnect()
        {
            uint method = 15;
            this.TrEntry(method);
            try
            {
                base.Disconnect();
            }
            finally
            {
                base.TrExit(method);
            }
        }
    }
}

