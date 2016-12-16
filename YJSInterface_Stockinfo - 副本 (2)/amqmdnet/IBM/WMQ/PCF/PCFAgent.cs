namespace IBM.WMQ.PCF
{
    using IBM.WMQ;
    using System;
    using System.Collections;

    public class PCFAgent
    {
        private MQQueueManager qMgr;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private bool tempQueueManager;
        private int waitInterval;

        public PCFAgent()
        {
            this.waitInterval = 0x7530;
        }

        public PCFAgent(MQQueueManager qMgr)
        {
            this.waitInterval = 0x7530;
            this.qMgr = qMgr;
            this.tempQueueManager = false;
        }

        public PCFAgent(string qManagerName)
        {
            this.waitInterval = 0x7530;
            this.qMgr = new MQQueueManager(qManagerName);
            this.tempQueueManager = true;
        }

        public void Connect(MQQueueManager qMgr)
        {
            this.qMgr = qMgr;
        }

        public void Connect(string qManagerName)
        {
            this.qMgr = new MQQueueManager(qManagerName);
        }

        public void Disconnect()
        {
            if (this.tempQueueManager)
            {
                this.qMgr.Disconnect();
            }
        }

        protected MQMessage[] Send(int command, PCFParameter[] parameters)
        {
            if (parameters == null)
            {
                throw new Exception("Must specify parameters!");
            }
            MQQueue queue = this.qMgr.AccessQueue("SYSTEM.DEFAULT.MODEL.QUEUE", 0x2021);
            MQMessage message = new MQMessage();
            message.ReplyToQueueName = queue.Name;
            message.MessageType = 1;
            message.Feedback = 0;
            message.Format = "MQADMIN ";
            message.Report = 0;
            MQCFH.Write(message, command, parameters.Length);
            for (int i = 0; i < parameters.Length; i++)
            {
                parameters[i].Write(message);
            }
            MQQueue queue2 = this.qMgr.AccessQueue(this.qMgr.CommandInputQueueName, 0x2030);
            MQPutMessageOptions pmo = new MQPutMessageOptions();
            pmo.Options = 0x40;
            queue2.Put(message, pmo);
            MQGetMessageOptions gmo = new MQGetMessageOptions();
            gmo.Options = 0x2001;
            gmo.WaitInterval = this.waitInterval;
            gmo.MatchOptions = 2;
            ArrayList list = new ArrayList();
            MQMessage message2 = null;
            int compCode = 0;
            int reason = 0;
            int num4 = 1;
            do
            {
                message2 = new MQMessage();
                message2.CorrelationId = message.MessageId;
                queue.Get(message2, gmo);
                message2.SkipBytes(20);
                num4 = message2.ReadInt4();
                compCode = message2.ReadInt4();
                reason = message2.ReadInt4();
                message2.Seek(0);
                if (compCode != 0)
                {
                    throw new PCFException(compCode, reason);
                }
                list.Add(message2);
            }
            while (num4 == 0);
            queue2.Close();
            queue.Close();
            return (MQMessage[]) list.ToArray(typeof(MQMessage));
        }

        public void SetWaitInterval(int seconds)
        {
            this.waitInterval = seconds * 0x3e8;
        }
    }
}

