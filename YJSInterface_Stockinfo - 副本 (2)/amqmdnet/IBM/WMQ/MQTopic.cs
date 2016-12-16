namespace IBM.WMQ
{
    using System;
    using System.Collections;
    using System.Text;

    public class MQTopic : MQDestination
    {
        private bool durable;
        private bool managed;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private bool subscribed;
        private MQSubscription subscriptionReference;
        private MQDestination unmanagedDestinationReference;

        public MQTopic(MQQueueManager qMgr, MQDestination destination, string topicName, string topicObject, int options) : this(qMgr, destination, topicName, topicObject, options, null, null, null)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { qMgr, destination, topicName, topicObject, options });
        }

        public MQTopic(MQQueueManager qMgr, string topicName, string topicObject, int openAs, int options) : this(qMgr, topicName, topicObject, openAs, options, null)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { qMgr, topicName, topicObject, openAs, options });
        }

        public MQTopic(MQQueueManager qMgr, MQDestination destination, string topicName, string topicObject, int options, string altUserId) : this(qMgr, destination, topicName, topicObject, options, altUserId, null, null)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { qMgr, destination, topicName, topicObject, options, altUserId });
        }

        public MQTopic(MQQueueManager qMgr, string topicName, string topicObject, int openAs, int options, string altUserId) : base(8, qMgr, topicObject, options, altUserId)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { qMgr, topicName, topicObject, openAs, options, altUserId });
            if (openAs == 1)
            {
                this.OpenForSubscription(null, topicName, null, null);
            }
            else
            {
                this.OpenForPublication(topicName);
            }
        }

        public MQTopic(MQQueueManager qMgr, string topicName, string topicObject, int options, string altUserId, string subscriptionName) : this(qMgr, topicName, topicObject, options, altUserId, subscriptionName, null)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { qMgr, topicName, topicObject, options, altUserId, subscriptionName });
        }

        public MQTopic(MQQueueManager qMgr, MQDestination destination, string topicName, string topicObject, int options, string altUserId, string subscriptionName) : this(qMgr, destination, topicName, topicObject, options, altUserId, subscriptionName, null)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { qMgr, destination, topicName, topicObject, options, altUserId, subscriptionName });
        }

        public MQTopic(MQQueueManager qMgr, string topicName, string topicObject, int options, string altUserId, string subscriptionName, Hashtable parameters) : this(qMgr, null, topicName, topicObject, options, altUserId, subscriptionName, parameters)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { qMgr, topicName, topicObject, options, altUserId, subscriptionName, parameters });
        }

        public MQTopic(MQQueueManager qMgr, MQDestination destination, string topicName, string topicObject, int options, string altUserId, string subscriptionName, Hashtable parameters) : base(8, qMgr, topicObject, options, altUserId)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { qMgr, destination, topicName, topicObject, options, altUserId, subscriptionName, parameters });
            this.OpenForSubscription(destination, topicName, subscriptionName, parameters);
        }

        public override void Close()
        {
            uint method = 0x2d7;
            this.TrEntry(method);
            try
            {
                if (this.unmanagedDestinationReference != null)
                {
                    base.isClosed = true;
                    if (this.subscriptionReference != null)
                    {
                        this.subscriptionReference.Close();
                        this.subscriptionReference = null;
                    }
                }
                else
                {
                    base.Close();
                    if (this.subscriptionReference != null)
                    {
                        if ((this.subscriptionReference.objectHandle != null) && (this.subscriptionReference.objectHandle.HOBJ.Handle != -1))
                        {
                            this.subscriptionReference.Close();
                        }
                        this.subscriptionReference = null;
                    }
                }
                base.qMgr = null;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private MQSubscriptionDescriptor CreateMQSubscriptionDescriptor(string topicName, string subscriptionName, Hashtable parameters)
        {
            uint method = 0x2d3;
            this.TrEntry(method, new object[] { topicName, subscriptionName, parameters });
            MQSubscriptionDescriptor descriptor = null;
            try
            {
                descriptor = new MQSubscriptionDescriptor();
                if ((base.AlternateUserId != null) && (base.AlternateUserId.Length > 0))
                {
                    descriptor.AlternateUserId = base.AlternateUserId;
                }
                else
                {
                    base.AlternateUserId = descriptor.AlternateUserId;
                }
                if ((base.objectName != null) && (base.objectName.Length > 0))
                {
                    descriptor.ObjectName = base.objectName;
                }
                else
                {
                    base.objectName = descriptor.ObjectName;
                }
                if ((topicName != null) && (topicName.Length > 0))
                {
                    descriptor.ObjectString.VSString = topicName;
                }
                descriptor.Options = base.OpenOptions;
                if ((subscriptionName != null) && (subscriptionName.Length > 0))
                {
                    descriptor.SubName.VSString = subscriptionName;
                }
                if (parameters == null)
                {
                    return descriptor;
                }
                if (parameters.ContainsKey("AlternateSecurityId"))
                {
                    string str = (string) parameters["AlternateSecurityId"];
                    if ((str != null) && (str.Length > 0))
                    {
                        descriptor.AlternateSecurityId = str;
                    }
                }
                if (parameters.ContainsKey("PublicationAccountingToken"))
                {
                    byte[] buffer = (byte[]) parameters["PublicationAccountingToken"];
                    if ((buffer != null) && (buffer.Length > 0))
                    {
                        descriptor.PubAccountingToken = base.GetString(buffer);
                    }
                }
                if (parameters.ContainsKey("PublicationApplicationIdData"))
                {
                    string str2 = (string) parameters["PublicationApplicationIdData"];
                    if ((str2 != null) && (str2.Length > 0))
                    {
                        descriptor.PubApplIdentityData = str2;
                    }
                }
                if (parameters.ContainsKey("PublicationPriority"))
                {
                    descriptor.PubPriority = (int) parameters["PublicationPriority"];
                }
                if (parameters.ContainsKey("SubscriptionCorrelationId"))
                {
                    byte[] buffer2 = (byte[]) parameters["SubscriptionCorrelationId"];
                    if ((buffer2 != null) && (buffer2.Length > 0))
                    {
                        descriptor.SubCorrelId = base.GetString(buffer2);
                    }
                }
                if (parameters.ContainsKey("SubscriptionExpiry"))
                {
                    descriptor.SubExpiry = (int) parameters["SubscriptionExpiry"];
                }
                if (parameters.ContainsKey("SubscriptionUserData"))
                {
                    string str3 = (string) parameters["SubscriptionUserData"];
                    if ((str3 != null) && (str3.Length > 0))
                    {
                        descriptor.SubUserData.VSString = str3;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return descriptor;
        }

        public bool IsDurable()
        {
            uint method = 0x2d4;
            this.TrEntry(method);
            base.TrExit(method, this.durable);
            return this.durable;
        }

        public bool IsManaged()
        {
            uint method = 0x2d5;
            this.TrEntry(method);
            base.TrExit(method, this.managed);
            return this.managed;
        }

        public bool IsSubscribed()
        {
            uint method = 0x2d6;
            this.TrEntry(method);
            base.TrExit(method, this.subscribed);
            return this.subscribed;
        }

        private void OpenForPublication(string topicName)
        {
            uint method = 0x2d1;
            this.TrEntry(method, new object[] { topicName });
            try
            {
                this.subscribed = false;
                MQObjectDescriptor od = base.CreateMQObjectDescriptor();
                od.Version = 4;
                if ((topicName != null) && (topicName.Length > 0))
                {
                    od.ObjectString.VSString = topicName;
                    base.TrData(method, 0, "Topic Name : ", Encoding.ASCII.GetBytes(topicName));
                }
                base.Open(ref od);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void OpenForSubscription(MQDestination destination, string topicName, string subscriptionName, Hashtable parameters)
        {
            uint method = 0x2d2;
            this.TrEntry(method, new object[] { destination, topicName, subscriptionName, parameters });
            try
            {
                int openOptions = base.OpenOptions;
                this.subscribed = true;
                if (destination == null)
                {
                    base.objectHandle = MQQueueManager.nmqiEnv.NewPhobj();
                }
                else
                {
                    base.objectHandle = destination.objectHandle;
                }
                this.unmanagedDestinationReference = destination;
                bool flag = (openOptions & 8) == 8;
                bool flag2 = (subscriptionName != null) && (subscriptionName.Length > 0);
                this.durable = flag && flag2;
                bool flag3 = (openOptions & 0x20) == 0x20;
                bool flag4 = destination == null;
                this.managed = flag3 | flag4;
                if (flag4)
                {
                    base.OpenOptions |= 0x20;
                }
                MQSubscriptionDescriptor mqSD = this.CreateMQSubscriptionDescriptor(topicName, subscriptionName, parameters);
                this.subscriptionReference = new MQSubscription(base.qMgr, this, mqSD);
                base.hObj = base.objectHandle.HOBJ;
                bool isOpen = this.subscriptionReference.IsOpen;
                base.isClosed = !isOpen;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public MQSubscription SubscriptionReference
        {
            get
            {
                return this.subscriptionReference;
            }
        }

        public MQDestination UnmanagedDestinationReference
        {
            get
            {
                return this.unmanagedDestinationReference;
            }
        }
    }
}

