namespace IBM.WMQAX
{
    using IBM.WMQ;
    using System;

    public class MQQueue : IBM.WMQ.MQQueue
    {
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public MQQueue(IBM.WMQAX.MQQueueManager qMgr, string queueName, int openOptions, string queueManagerName, string dynamicQueueName, string alternateUserId) : base(qMgr, queueName, openOptions, queueManagerName, dynamicQueueName, alternateUserId)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { qMgr, queueName, openOptions, queueManagerName, dynamicQueueName, alternateUserId });
        }
    }
}

