namespace IBM.WMQ
{
    using System;

    internal class MQRequestEntry : MQBase
    {
        private MQRequestEntry next;
        private MQRequestEntry prev;
        private MQTSH reply;
        private int requestId;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public MQRequestEntry()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.next = null;
            this.prev = null;
            this.requestId = -1;
            this.reply = null;
        }

        internal MQRequestEntry Next
        {
            get
            {
                return this.next;
            }
            set
            {
                this.next = value;
            }
        }

        internal MQRequestEntry Previous
        {
            get
            {
                return this.prev;
            }
            set
            {
                this.prev = value;
            }
        }

        internal MQTSH Reply
        {
            get
            {
                return this.reply;
            }
            set
            {
                this.reply = value;
            }
        }

        internal int RequestID
        {
            get
            {
                return this.requestId;
            }
            set
            {
                this.requestId = value;
            }
        }
    }
}

