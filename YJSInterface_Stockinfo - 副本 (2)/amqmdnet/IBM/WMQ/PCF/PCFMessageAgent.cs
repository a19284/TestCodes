namespace IBM.WMQ.PCF
{
    using IBM.WMQ;
    using System;

    public class PCFMessageAgent : PCFAgent
    {
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public PCFMessageAgent()
        {
        }

        public PCFMessageAgent(MQQueueManager qManager) : base(qManager)
        {
        }

        public PCFMessageAgent(string qManagerName) : base(qManagerName)
        {
        }

        public PCFMessage[] Send(PCFMessage request)
        {
            return this.Send(request, true);
        }

        public PCFMessage[] Send(PCFMessage request, bool check)
        {
            MQMessage[] messageArray = base.Send(request.GetCommand(), request.GetParameters());
            PCFMessage[] messageArray2 = new PCFMessage[messageArray.Length];
            for (int i = 0; i < messageArray2.Length; i++)
            {
                messageArray2[i] = new PCFMessage(messageArray[i]);
            }
            return messageArray2;
        }
    }
}

