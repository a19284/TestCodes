namespace IBM.WMQ.Nmqi
{
    using System;

    public class MQAsyncDelivery : NmqiObject
    {
        private ManagedHconn hConn;
        private MQProxyQueue proxyQueue;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private MQDispatchThread thread;

        public MQAsyncDelivery(NmqiEnvironment nmqiEnv) : base(nmqiEnv)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv });
        }

        public ManagedHconn HConn
        {
            get
            {
                return this.hConn;
            }
            set
            {
                this.hConn = value;
            }
        }

        public MQProxyQueue ProxyQueue
        {
            get
            {
                return this.proxyQueue;
            }
            set
            {
                this.proxyQueue = value;
            }
        }

        public MQDispatchThread Thread
        {
            get
            {
                return this.thread;
            }
            set
            {
                this.thread = value;
            }
        }
    }
}

