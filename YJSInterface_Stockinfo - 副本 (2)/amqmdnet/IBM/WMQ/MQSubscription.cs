namespace IBM.WMQ
{
    using System;

    public class MQSubscription : MQManagedObject
    {
        private const string sccsid = "@(#) lib/dotnet/pc/winnt/nmqi/MQSubscription.cs, dotnet, p000  1.3 09/12/14 05:16:29";
        private MQTopic topic;

        internal MQSubscription(MQQueueManager qMgr, MQTopic mqTopic, MQSubscriptionDescriptor mqSD)
        {
            base.TrConstructor("@(#) lib/dotnet/pc/winnt/nmqi/MQSubscription.cs, dotnet, p000  1.3 09/12/14 05:16:29", new object[] { qMgr, mqTopic, mqSD });
            base.qMgr = qMgr;
            this.topic = mqTopic;
            base.name = mqSD.ObjectString.VSString;
            this.Subscribe(mqSD);
        }

        public override void Close()
        {
            uint method = 0x77;
            this.TrEntry(method);
            int pCompCode = 0;
            int pReason = 0;
            try
            {
                if (base.qMgr != null)
                {
                    base.Close();
                    if (base.qMgr.IsConnected && base.IsOpen)
                    {
                        base.qMgr.nmqiConnector.MQCLOSE(base.qMgr.hConn, base.objectHandle, base.CloseOptions, out pCompCode, out pReason);
                        base.objectHandle = null;
                        base.objectName = "";
                        if (pCompCode != 0)
                        {
                            base.qMgr.CheckHConnHealth(pReason);
                            base.throwNewMQException(pCompCode, pReason);
                        }
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Inquire(int[] selectors, int[] intAttrs, byte[] charAttrs)
        {
        }

        public int QueryAttribute(int attributeType)
        {
            return 0;
        }

        public string QueryAttribute(int attributeType, int stringMaxLength)
        {
            return null;
        }

        public int RequestPublicationUpdate(int options)
        {
            uint method = 0x2bf;
            this.TrEntry(method, new object[] { options });
            MQSubscriptionRequestOptions mqsro = new MQSubscriptionRequestOptions();
            mqsro.Options = options;
            int pCompCode = 0;
            int pReason = 0;
            try
            {
                base.qMgr.nmqiConnector.MQSUBRQ(base.qMgr.hConn, base.objectHandle.HOBJ, 1, ref mqsro, out pCompCode, out pReason);
                if (pCompCode != 0)
                {
                    base.qMgr.CheckHConnHealth(pReason);
                    base.throwNewMQException(pCompCode, pReason);
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return mqsro.NumPublications;
        }

        public void Set(int[] selectors, int[] intAttrs, byte[] charAttrs)
        {
        }

        public void SetAttribute(int attributeType, int attributeValue)
        {
        }

        public void SetAttribute(int attributeType, string attributeValue, int attributeMaxLength)
        {
        }

        internal void Subscribe(MQSubscriptionDescriptor mqSD)
        {
            uint method = 0x2be;
            this.TrEntry(method, new object[] { mqSD });
            int pCompCode = 0;
            int pReason = 0;
            try
            {
                base.objectHandle = MQQueueManager.nmqiEnv.NewPhobj();
                base.qMgr.nmqiConnector.MQSUB(base.qMgr.hConn, mqSD, this.topic.objectHandle, base.objectHandle, out pCompCode, out pReason);
                if (pCompCode != 0)
                {
                    base.isClosed = true;
                    base.qMgr.CheckHConnHealth(pReason);
                    base.throwNewMQException(pCompCode, pReason);
                }
                base.isClosed = false;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public string AlternateUserId
        {
            get
            {
                return null;
            }
        }

        public int OpenOptions
        {
            get
            {
                return -1;
            }
        }
    }
}

