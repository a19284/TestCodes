namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Transactions;

    public class MQQueueManager : MQManagedObject, IDisposable
    {
        private int beginOptions;
        private string certificateLabel;
        private int certificateValPolicy;
        private string channel;
        private const int CLIENT_EXPLICIT = 2;
        private const int CLIENT_IMPLICIT = 1;
        private const int CLIENT_MANAGED_EXPLICIT = 6;
        private const int CLIENT_MANAGED_IMPLICIT = 5;
        private const int CLIENT_XA_EXPLICIT = 4;
        private const int CLIENT_XA_IMPLICIT = 3;
        private int commandLevel;
        private int connectionType;
        private string connName;
        private MQDisposer disposer;
        private ArrayList encryptionPolicySuiteB;
        private int fipsRequired;
        internal Hconn hConn;
        private ArrayList hdrCompList;
        private volatile bool isXAEnabled;
        private int keyResetCount;
        private string localAddress;
        private ArrayList mqairArray;
        private ArrayList msgCompList;
        private string msgExit;
        internal NmqiMQ nmqiConnector;
        internal static NmqiEnvironment nmqiEnv = NmqiFactory.GetInstance(null);
        private int options;
        private string password;
        internal IntPtr pCtl;
        private Phconn pHconn;
        private Hashtable properties;
        private string receiveExit;
        private string receiveUserData;
        private const string sccsid = "%Z% %W% %I% %E% %U%";
        private string securityExit;
        private string securityUserData;
        private string sendExit;
        private string sendUserData;
        private const int SERVER = 0;
        private int sharingConversations;
        private string sslCipherSpec;
        private string sslCryptoHardware;
        private string sslKeyRepository;
        private string sslPeerName;
        private string userId;

        public MQQueueManager()
        {
            this.certificateLabel = "";
            this.properties = new Hashtable();
            this.pCtl = IntPtr.Zero;
            base.TrConstructor("%Z% %W% %I% %E% %U%");
            this.Connect(null);
        }

        public MQQueueManager(string queueManagerName)
        {
            this.certificateLabel = "";
            this.properties = new Hashtable();
            this.pCtl = IntPtr.Zero;
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { queueManagerName });
            this.Connect(queueManagerName);
        }

        public MQQueueManager(string queueManagerName, Hashtable properties)
        {
            this.certificateLabel = "";
            this.properties = new Hashtable();
            this.pCtl = IntPtr.Zero;
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { queueManagerName, properties });
            this.properties = properties;
            this.Connect(queueManagerName);
        }

        public MQQueueManager(string queueManagerName, int Options)
        {
            this.certificateLabel = "";
            this.properties = new Hashtable();
            this.pCtl = IntPtr.Zero;
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { queueManagerName, Options });
            this.properties.Add("connectOptions", Options);
            this.Connect(queueManagerName);
        }

        public MQQueueManager(string queueManagerName, string Channel, string ConnName)
        {
            this.certificateLabel = "";
            this.properties = new Hashtable();
            this.pCtl = IntPtr.Zero;
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { queueManagerName, Channel, ConnName });
            this.properties.Add("channel", Channel);
            this.properties.Add("connectionName", ConnName);
            this.Connect(queueManagerName);
        }

        public MQQueueManager(string queueManagerName, int Options, string Channel, string ConnName)
        {
            this.certificateLabel = "";
            this.properties = new Hashtable();
            this.pCtl = IntPtr.Zero;
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { queueManagerName, Options, Channel, ConnName });
            this.properties.Add("connectOptions", Options);
            this.properties.Add("channel", Channel);
            this.properties.Add("connectionName", ConnName);
            this.Connect(queueManagerName);
        }

        public MQProcess AccessProcess(string processName, int openOptions)
        {
            uint method = 0x238;
            this.TrEntry(method, new object[] { processName, openOptions });
            MQProcess process = null;
            try
            {
                process = this.AccessProcess(processName, openOptions, null, null);
            }
            finally
            {
                base.TrExit(method);
            }
            return process;
        }

        public MQProcess AccessProcess(string processName, int openOptions, string queueManagerName, string alternateUserId)
        {
            MQProcess process2;
            uint method = 0x239;
            this.TrEntry(method, new object[] { processName, openOptions, queueManagerName, alternateUserId });
            try
            {
                process2 = new MQProcess(this, processName, openOptions, queueManagerName, alternateUserId);
            }
            finally
            {
                base.TrExit(method);
            }
            return process2;
        }

        public MQQueue AccessQueue(string queueName, int openOptions)
        {
            uint method = 0x236;
            this.TrEntry(method, new object[] { queueName, openOptions });
            MQQueue queue = null;
            try
            {
                queue = this.AccessQueue(queueName, openOptions, null, null, null);
            }
            finally
            {
                base.TrExit(method);
            }
            return queue;
        }

        public MQQueue AccessQueue(string queueName, int openOptions, string queueManagerName, string dynamicQueueName, string alternateUserId)
        {
            MQQueue queue2;
            uint method = 0x237;
            this.TrEntry(method, new object[] { queueName, openOptions, queueManagerName, dynamicQueueName, alternateUserId });
            try
            {
                queue2 = new MQQueue(this, queueName, openOptions, queueManagerName, dynamicQueueName, alternateUserId);
            }
            finally
            {
                base.TrExit(method);
            }
            return queue2;
        }

        public MQTopic AccessTopic(MQDestination destination, string topicName, string topicObject, int options)
        {
            uint method = 0x23e;
            this.TrEntry(method, new object[] { destination, topicName, topicObject, options });
            MQTopic topic = null;
            try
            {
                topic = new MQTopic(this, destination, topicName, topicObject, options);
            }
            finally
            {
                base.TrExit(method);
            }
            return topic;
        }

        public MQTopic AccessTopic(string topicName, string topicObject, int openAs, int options)
        {
            uint method = 570;
            this.TrEntry(method, new object[] { topicName, topicObject, openAs, options });
            MQTopic topic = null;
            try
            {
                topic = new MQTopic(this, topicName, topicObject, openAs, options);
            }
            finally
            {
                base.TrExit(method);
            }
            return topic;
        }

        public MQTopic AccessTopic(MQDestination destination, string topicName, string topicObject, int options, string altUserId)
        {
            uint method = 0x23f;
            this.TrEntry(method, new object[] { destination, topicName, topicObject, options, altUserId });
            MQTopic topic = null;
            try
            {
                topic = new MQTopic(this, destination, topicName, topicObject, options, altUserId);
            }
            finally
            {
                base.TrExit(method);
            }
            return topic;
        }

        public MQTopic AccessTopic(string topicName, string topicObject, int openAs, int options, string altUserId)
        {
            uint method = 0x23b;
            this.TrEntry(method, new object[] { topicName, topicObject, openAs, options, altUserId });
            MQTopic topic = null;
            try
            {
                topic = new MQTopic(this, topicName, topicObject, openAs, options, altUserId);
            }
            finally
            {
                base.TrExit(method);
            }
            return topic;
        }

        public MQTopic AccessTopic(string topicName, string topicObject, int options, string altUserId, string subName)
        {
            uint method = 0x23c;
            this.TrEntry(method, new object[] { topicName, topicObject, options, altUserId, subName });
            MQTopic topic = null;
            try
            {
                topic = new MQTopic(this, topicName, topicObject, options, altUserId, subName);
            }
            finally
            {
                base.TrExit(method);
            }
            return topic;
        }

        public MQTopic AccessTopic(MQDestination destination, string topicName, string topicObject, int options, string altUserId, string subName)
        {
            uint method = 0x240;
            this.TrEntry(method, new object[] { destination, topicName, topicObject, options, altUserId, subName });
            MQTopic topic = null;
            try
            {
                topic = new MQTopic(this, destination, topicName, topicObject, options, altUserId, subName);
            }
            finally
            {
                base.TrExit(method);
            }
            return topic;
        }

        public MQTopic AccessTopic(string topicName, string topicObject, int options, string altUserId, string subName, Hashtable parameters)
        {
            uint method = 0x23d;
            this.TrEntry(method, new object[] { topicName, topicObject, options, altUserId, subName, parameters });
            MQTopic topic = null;
            try
            {
                topic = new MQTopic(this, topicName, topicObject, options, altUserId, subName, parameters);
            }
            finally
            {
                base.TrExit(method);
            }
            return topic;
        }

        public MQTopic AccessTopic(MQDestination destination, string topicName, string topicObject, int options, string altUserId, string subName, Hashtable properties)
        {
            uint method = 0x241;
            this.TrEntry(method, new object[] { destination, topicName, topicObject, options, altUserId, subName, properties });
            MQTopic topic = null;
            try
            {
                topic = new MQTopic(this, destination, topicName, topicObject, options, altUserId, subName, properties);
            }
            finally
            {
                base.TrExit(method);
            }
            return topic;
        }

        protected void ActivateMessage(int Options, string queueName, string qmgrName, byte[] msgId)
        {
            uint method = 0x245;
            this.TrEntry(method, new object[] { Options, queueName, qmgrName, msgId });
            int compCode = 0;
            int reason = 0;
            try
            {
                MQSPIActivateOpts spiao = new MQSPIActivateOpts(Options);
                if (queueName != null)
                {
                    spiao.QName = queueName;
                }
                if (qmgrName != null)
                {
                    spiao.QMgrName = qmgrName;
                }
                spiao.MsgId = msgId;
                ((NmqiSP) this.nmqiConnector).SPIActivateMessage(this.hConn, ref spiao, out compCode, out reason);
                if (compCode != 0)
                {
                    base.qMgr.CheckHConnHealth(reason);
                    base.throwNewMQException(compCode, reason);
                }
            }
            catch (MQException exception)
            {
                compCode = exception.CompCode;
                reason = exception.Reason;
                throw exception;
            }
            finally
            {
                base.unsafe_compCode = compCode;
                base.unsafe_reason = reason;
                base.TrExit(method);
            }
        }

        public void Backout()
        {
            uint method = 580;
            this.TrEntry(method);
            int pCompCode = 0;
            int pReason = 0;
            try
            {
                this.nmqiConnector.MQBACK(this.hConn, out pCompCode, out pReason);
                if (pCompCode != 0)
                {
                    base.qMgr.CheckHConnHealth(pReason);
                    base.throwNewMQException(pCompCode, pReason);
                }
            }
            catch (MQException exception)
            {
                pCompCode = exception.CompCode;
                pReason = exception.Reason;
                throw exception;
            }
            finally
            {
                base.unsafe_compCode = pCompCode;
                base.unsafe_reason = pReason;
                base.TrExit(method);
            }
        }

        public void Begin()
        {
            uint method = 0x242;
            this.TrEntry(method);
            MQBeginOptions pBeginOptions = null;
            int pCompCode = 0;
            int pReason = 0;
            try
            {
                pBeginOptions = new MQBeginOptions();
                pBeginOptions.Options = this.beginOptions;
                MQBase.MQBO structMQBO = pBeginOptions.StructMQBO;
                this.nmqiConnector.MQBEGIN(this.hConn, pBeginOptions, out pCompCode, out pReason);
                if (pCompCode != 0)
                {
                    base.qMgr.CheckHConnHealth(pReason);
                    base.throwNewMQException(pCompCode, pReason);
                }
            }
            catch (MQException exception)
            {
                pCompCode = exception.CompCode;
                pReason = exception.Reason;
                throw exception;
            }
            finally
            {
                base.unsafe_compCode = pCompCode;
                base.unsafe_reason = pReason;
                base.TrExit(method);
            }
        }

        internal void CheckHConnHealth(int Reason)
        {
            int pCompCode = 0;
            int pReason = 0;
            base.TrText("MQQueueManager.CheckHConnHealth() Reason: 0x" + Reason.ToString("X8"));
            try
            {
                switch (Reason)
                {
                    case 0x893:
                    case 0x89b:
                    case 0x7d9:
                    case 0x872:
                        break;

                    default:
                        return;
                }
                if (((this.hConn != null) && (this.hConn.Value != -1)) && (this.hConn.Value != 0))
                {
                    this.nmqiConnector.MQBACK(this.hConn, out pCompCode, out pReason);
                    base.TrText("MQQueueManager.CheckHConnHealth() MQBACK Reason: 0x" + pReason.ToString("X8"));
                    this.nmqiConnector.MQDISC(this.pHconn, out pCompCode, out pReason);
                    base.TrText("MQQueueManager.CheckHConnHealth() MQDISC Reason: 0x" + pReason.ToString("X8"));
                }
                this.pHconn = nmqiEnv.NewPhconn();
                this.hConn = this.pHconn.HConn;
            }
            catch (MQException exception)
            {
                pCompCode = exception.CompCode;
                pReason = exception.Reason;
                throw exception;
            }
        }

        public void Commit()
        {
            uint method = 0x243;
            this.TrEntry(method);
            int pCompCode = 0;
            int pReason = 0;
            try
            {
                this.nmqiConnector.MQCMIT(this.hConn, out pCompCode, out pReason);
                if (pCompCode != 0)
                {
                    base.qMgr.CheckHConnHealth(pReason);
                    base.throwNewMQException(pCompCode, pReason);
                }
            }
            catch (MQException exception)
            {
                pCompCode = exception.CompCode;
                pReason = exception.Reason;
                throw exception;
            }
            finally
            {
                base.unsafe_compCode = pCompCode;
                base.unsafe_reason = pReason;
                base.TrExit(method);
            }
        }

        public void Connect()
        {
            uint method = 0x233;
            this.TrEntry(method);
            try
            {
                this.Connect(null);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public unsafe void Connect(string queueManagerName)
        {
            uint method = 0x30f;
            this.TrEntry(method, new object[] { queueManagerName });
            int pCompCode = 0;
            int pReason = 0;
            MQConnectOptions pConnectOpts = null;
            MQChannelDefinition definition = null;
            MQSSLConfigOptions options2 = null;
            MQConnectionSecurityParameters parameters = null;
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            IntPtr ptr3 = IntPtr.Zero;
            IntPtr ptr4 = IntPtr.Zero;
            IntPtr ptr5 = IntPtr.Zero;
            IntPtr ptr6 = IntPtr.Zero;
            string bindingTypeFromProperties = "";
            bool flag = false;
            base.TrText(method, "SCCSID: '%Z% %W% %I% %E% %U%'");
            try
            {
                if (this.IsConnected)
                {
                    base.TrText(method, "Already connected!");
                    base.throwNewMQException(1, 0x7d2);
                }
                base.TrText(method, "ConnectOptions: QMgr = '" + queueManagerName + "', ConnName = '" + this.connName + "', Channel = '" + this.channel + "'");
                this.options = MQEnvironment.GetIntegerProperty("connectOptions", this.properties, this.options);
                this.connName = MQEnvironment.GetStringProperty("connectionName", this.properties, MQEnvironment.ConnectionName);
                if (this.connName == null)
                {
                    this.connName = MQEnvironment.GetStringProperty("hostname", this.properties, MQEnvironment.Hostname);
                    if (this.connName != null)
                    {
                        int num5 = MQEnvironment.GetIntegerProperty("port", this.properties, MQEnvironment.Port);
                        if (num5 != 0)
                        {
                            this.connName = this.connName + "(" + num5.ToString() + ")";
                        }
                    }
                }
                this.channel = MQEnvironment.GetStringProperty("channel", this.properties, MQEnvironment.Channel);
                this.sslCipherSpec = MQEnvironment.GetStringProperty("SSL Cipher Spec", this.properties, MQEnvironment.SSLCipherSpec);
                this.sslPeerName = MQEnvironment.GetStringProperty("SSL Peer Name", this.properties, MQEnvironment.SSLPeerName);
                this.sslKeyRepository = MQEnvironment.GetStringProperty("SSL CertStores", this.properties, MQEnvironment.SSLKeyRepository);
                this.sslCryptoHardware = MQEnvironment.GetStringProperty("SSL CryptoHardware", this.properties, MQEnvironment.SSLCryptoHardware);
                this.securityExit = MQEnvironment.GetStringProperty("securityExit", this.properties, MQEnvironment.SecurityExit);
                this.securityUserData = MQEnvironment.GetStringProperty("securityUserData", this.properties, MQEnvironment.SecurityUserData);
                this.sendExit = MQEnvironment.GetStringProperty("sendExit", this.properties, MQEnvironment.SendExit);
                this.sendUserData = MQEnvironment.GetStringProperty("sendUserData", this.properties, MQEnvironment.SendUserData);
                this.sharingConversations = MQEnvironment.GetIntegerProperty("sharingConversations", this.properties, MQEnvironment.SharingConversations);
                this.receiveExit = MQEnvironment.GetStringProperty("receiveExit", this.properties, MQEnvironment.ReceiveExit);
                this.receiveUserData = MQEnvironment.GetStringProperty("receiveUserData", this.properties, MQEnvironment.ReceiveUserData);
                this.userId = MQEnvironment.GetStringProperty("userID", this.properties, MQEnvironment.UserId);
                this.password = MQEnvironment.GetStringProperty("password", this.properties, MQEnvironment.Password);
                this.mqairArray = MQEnvironment.GetArrayListProperty("mqairArray", this.properties, MQEnvironment.MQAIRArray);
                this.keyResetCount = MQEnvironment.GetIntegerProperty("keyResetCount", this.properties, MQEnvironment.KeyResetCount);
                this.fipsRequired = MQEnvironment.GetIntegerProperty("fipsRequired", this.properties, MQEnvironment.FipsRequired);
                this.encryptionPolicySuiteB = MQEnvironment.GetArrayListProperty("encryptionPolicySuiteB", this.properties, MQEnvironment.EncryptionPolicySuiteB);
                this.certificateValPolicy = MQEnvironment.GetIntegerProperty("certificateValPolicy", this.properties, MQEnvironment.CertificateValPolicy);
                this.hdrCompList = MQEnvironment.GetArrayListProperty("hdrCmpList", this.properties, MQEnvironment.HdrCompList);
                this.msgCompList = MQEnvironment.GetArrayListProperty("msgCmpList", this.properties, MQEnvironment.MsgCompList);
                this.localAddress = MQEnvironment.GetStringProperty("Local Address Property", this.properties, MQEnvironment.LocalAddressSetting);
                this.certificateLabel = MQEnvironment.CertificateLabel;
                if (this.connName != null)
                {
                    base.TrText(method, "Connection Name: " + this.connName);
                }
                if (this.channel != null)
                {
                    base.TrText(method, "Channel: " + this.channel);
                }
                if (this.sslCipherSpec != null)
                {
                    base.TrText(method, "SSLCipherSpec: " + this.sslCipherSpec);
                }
                if (this.sslPeerName != null)
                {
                    base.TrText(method, "SSLPeerName: " + this.sslPeerName);
                }
                if (this.sslKeyRepository != null)
                {
                    base.TrText(method, "SSLKeyRepository: " + this.sslKeyRepository);
                }
                if (this.sslCryptoHardware != null)
                {
                    base.TrText(method, "SSLCryptoHardware: " + this.sslCryptoHardware);
                }
                if (this.securityExit != null)
                {
                    base.TrText(method, "SecurityExit: " + this.securityExit);
                }
                if (this.securityUserData != null)
                {
                    base.TrText(method, "SecurityUserData: " + this.securityUserData);
                }
                if (this.sendExit != null)
                {
                    base.TrText(method, "SendExit: " + this.sendExit);
                }
                if (this.sendUserData != null)
                {
                    base.TrText(method, "SendUserData: " + this.sendUserData);
                }
                if (10 != this.sharingConversations)
                {
                    base.TrText(method, "SharingConversations: " + this.sharingConversations);
                }
                if (this.receiveExit != null)
                {
                    base.TrText(method, "ReceiveExit: " + this.receiveExit);
                }
                if (this.receiveUserData != null)
                {
                    base.TrText(method, "ReceiveUserData: " + this.receiveUserData);
                }
                if (this.msgExit != null)
                {
                    base.TrText(method, "MsgExit: " + this.msgExit);
                }
                if (this.keyResetCount != 0)
                {
                    base.TrText(method, "KeyResetCount: " + this.keyResetCount);
                }
                if (this.fipsRequired != 0)
                {
                    base.TrText(method, "FipsRequired: " + this.fipsRequired);
                }
                if (this.encryptionPolicySuiteB != null)
                {
                    string str2 = "EncryptionPolicySuiteB: [Size: " + this.encryptionPolicySuiteB.Count + "] ";
                    for (int i = 0; (i < this.encryptionPolicySuiteB.Count) && (i < 4); i++)
                    {
                        base.TrText(method, "Suite B: " + (str2 + this.encryptionPolicySuiteB[i] + ", "));
                    }
                }
                if (this.certificateValPolicy != 0)
                {
                    base.TrText(method, "CertificateValPolicy: " + this.certificateValPolicy);
                }
                if (this.localAddress != null)
                {
                    base.TrText(method, "LocalAddress: " + this.localAddress);
                }
                if ((((this.options & 0x20) == 0) && ((this.options & 0x80) == 0)) && ((this.options & 0x40) == 0))
                {
                    this.options |= 0x40;
                }
                base.TrText(method, "MQCNO.options: 0x" + this.options.ToString("X8"));
                if ((this.channel != null) || (this.connName != null))
                {
                    flag = true;
                }
                bindingTypeFromProperties = MQEnvironment.GetBindingTypeFromProperties(this.properties);
                if (bindingTypeFromProperties == null)
                {
                    if (flag)
                    {
                        if ((this.channel == null) || (this.connName == null))
                        {
                            throw new Exception("Channel and Connection MUST be specified");
                        }
                        bindingTypeFromProperties = MQEnvironment.GetBindingTypeFromEnv();
                        switch (bindingTypeFromProperties)
                        {
                            case "SERVER":
                            case null:
                                bindingTypeFromProperties = "CLIENT";
                                break;
                        }
                    }
                    else
                    {
                        bindingTypeFromProperties = MQEnvironment.GetBindingTypeFromEnv();
                    }
                }
                if (bindingTypeFromProperties == null)
                {
                    bindingTypeFromProperties = MQEnvironment.GetBindingTypeFromRegistry();
                }
                if (bindingTypeFromProperties == null)
                {
                    bindingTypeFromProperties = "MANAGEDCLIENT";
                }
                string str3 = bindingTypeFromProperties;
                if (str3 == null)
                {
                    goto Label_07E4;
                }
                if (!(str3 == "SERVER"))
                {
                    if (str3 == "CLIENT")
                    {
                        goto Label_07B4;
                    }
                    if (str3 == "XACLIENT")
                    {
                        goto Label_07C4;
                    }
                    if (str3 == "MANAGEDCLIENT")
                    {
                        goto Label_07D4;
                    }
                    goto Label_07E4;
                }
                this.connectionType = 0;
                goto Label_0802;
            Label_07B4:
                this.connectionType = flag ? 2 : 1;
                goto Label_0802;
            Label_07C4:
                this.connectionType = flag ? 4 : 3;
                goto Label_0802;
            Label_07D4:
                this.connectionType = flag ? 6 : 5;
                goto Label_0802;
            Label_07E4:
                this.nmqiConnector = null;
                throw new Exception("An invalid binding type (" + bindingTypeFromProperties + ") was specified");
            Label_0802:
                if (queueManagerName == null)
                {
                    queueManagerName = "";
                }
                base.objectType = 5;
                base.objectName = queueManagerName;
                base.TrText(method, string.Concat(new object[] { "BindingType = '", bindingTypeFromProperties, "' connectionType = ", this.connectionType }));
                if (this.connectionType == 0)
                {
                    this.nmqiConnector = nmqiEnv.GetMQI(0);
                }
                else if ((this.connectionType == 1) || (this.connectionType == 2))
                {
                    this.nmqiConnector = nmqiEnv.GetMQI(1);
                    if ((MQEnvironment.ProductIdentifier == null) || (MQEnvironment.ProductIdentifier.Length < 1))
                    {
                        MQEnvironment.SetProductId("CLIENT");
                    }
                }
                else if ((this.connectionType == 3) || (this.connectionType == 4))
                {
                    this.nmqiConnector = nmqiEnv.GetMQI(3);
                    if ((MQEnvironment.ProductIdentifier == null) || (MQEnvironment.ProductIdentifier.Length < 1))
                    {
                        MQEnvironment.SetProductId("XACLIENT");
                    }
                }
                else if ((this.connectionType == 5) || (this.connectionType == 6))
                {
                    this.nmqiConnector = nmqiEnv.GetMQI(2);
                    if ((MQEnvironment.ProductIdentifier == null) || (MQEnvironment.ProductIdentifier.Length < 1))
                    {
                        MQEnvironment.SetProductId("MANAGEDCLIENT");
                    }
                }
                if (this.nmqiConnector == null)
                {
                    throw new Exception("Error: No unsupported bindings specified");
                }
                if (((((2 == this.connectionType) || (4 == this.connectionType)) || ((6 == this.connectionType) || (this.options != 0))) || (((this.sslCipherSpec != null) || (this.userId != null)) || ((this.password != null) || (this.mqairArray != null)))) || (((this.sslKeyRepository != null) || (this.sslCryptoHardware != null)) || ((this.localAddress != null) || (10 != this.sharingConversations))))
                {
                    pConnectOpts = new MQConnectOptions();
                    definition = new MQChannelDefinition();
                    definition.MaxMessageLength = 0x6400000;
                    if (this.channel != null)
                    {
                        definition.ChannelName = this.channel;
                    }
                    if (this.connName != null)
                    {
                        definition.ConnectionName = this.connName;
                    }
                    if (this.options != 0)
                    {
                        pConnectOpts.Options = this.options;
                    }
                    if (this.securityExit != null)
                    {
                        definition.SecurityExit = this.securityExit;
                    }
                    if (this.securityUserData != null)
                    {
                        definition.SecurityUserData = this.securityUserData;
                    }
                    if (this.sendExit != null)
                    {
                        definition.SendExit = this.sendExit;
                    }
                    if (this.sendUserData != null)
                    {
                        definition.SendUserData = this.sendUserData;
                    }
                    definition.SharingConversations = this.sharingConversations;
                    definition.Version = 11;
                    if (this.receiveExit != null)
                    {
                        definition.ReceiveExit = this.receiveExit;
                    }
                    if (this.receiveUserData != null)
                    {
                        definition.ReceiveUserData = this.receiveUserData;
                    }
                    if (this.sslCipherSpec != null)
                    {
                        definition.SSLCipherSpec = this.sslCipherSpec;
                    }
                    if (this.hdrCompList != null)
                    {
                        definition.HdrCompList = this.hdrCompList;
                    }
                    if (this.msgCompList != null)
                    {
                        definition.MsgCompList = this.msgCompList;
                    }
                    if (this.localAddress != null)
                    {
                        definition.LocalAddress = this.localAddress;
                    }
                    if ((this.userId != null) || (this.password != null))
                    {
                        parameters = new MQConnectionSecurityParameters();
                        parameters.AuthenticationType = 1;
                        base.TrText("MQCSP UserId is set to - " + this.userId);
                        StringBuilder builder = new StringBuilder();
                        for (int j = 0; j < this.password.Length; j++)
                        {
                            builder.Append("x");
                        }
                        base.TrText("MQCSP Password is set to -" + builder.ToString());
                        if ((this.connectionType == 6) || (this.connectionType == 5))
                        {
                            parameters.UserId = this.userId;
                            parameters.Password = this.password;
                            pConnectOpts.SecurityParms = parameters;
                        }
                        else
                        {
                            if (this.userId != null)
                            {
                                ptr4 = Marshal.StringToCoTaskMemAnsi(this.userId);
                                parameters.CSPUserIdPtr = ptr4;
                                parameters.CSPUserIdLength = this.userId.Length;
                            }
                            if (this.password != null)
                            {
                                ptr5 = Marshal.StringToCoTaskMemAnsi(this.password);
                                parameters.CSPPasswordPtr = ptr5;
                                parameters.CSPPasswordLength = this.password.Length;
                            }
                            ptr3 = Marshal.AllocCoTaskMem(Marshal.SizeOf(parameters.StructMQCSP));
                            Marshal.StructureToPtr(parameters.StructMQCSP, ptr3, false);
                            pConnectOpts.SecurityParmsPtr = ptr3;
                        }
                    }
                    definition.TraceFields();
                    if (this.mqairArray != null)
                    {
                        ptr6 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(MQBase.MQAIR)) * this.mqairArray.Count);
                        for (int k = 0; k < this.mqairArray.Count; k++)
                        {
                            Marshal.StructureToPtr(((MQAuthenticationInformationRecord) this.mqairArray[k]).StructMQAIR, (IntPtr) (((int) ptr6) + (Marshal.SizeOf(typeof(MQBase.MQAIR)) * k)), false);
                        }
                    }
                    if ((((this.sslKeyRepository != null) || (this.sslCryptoHardware != null)) || ((IntPtr.Zero != ptr6) || (this.keyResetCount != 0))) || (((this.fipsRequired != 0) || (this.encryptionPolicySuiteB != null)) || ((this.certificateValPolicy != 0) || ((this.certificateLabel != null) && (this.certificateLabel.Length > 0)))))
                    {
                        options2 = new MQSSLConfigOptions();
                        if (this.sslKeyRepository != null)
                        {
                            options2.KeyRepository = this.sslKeyRepository;
                        }
                        if (this.sslCryptoHardware != null)
                        {
                            options2.CryptoHardware = this.sslCryptoHardware;
                        }
                        if (this.mqairArray != null)
                        {
                            options2.AuthInfoRecCount = this.mqairArray.Count;
                            options2.AuthInfoRecPtr = ptr6;
                        }
                        if (this.encryptionPolicySuiteB != null)
                        {
                            options2.EncryptionPolicySuiteB = this.encryptionPolicySuiteB;
                        }
                        if (this.certificateLabel != null)
                        {
                            byte[] buffer = new byte[0x40];
                            base.GetBytes(this.certificateLabel, ref buffer);
                            options2.CerfificateLabel = buffer;
                            definition.CertificateLabel = buffer;
                        }
                        options2.KeyResetCount = this.keyResetCount;
                        options2.FipsRequired = this.fipsRequired;
                        options2.CertificateValPolicy = this.certificateValPolicy;
                        if ((this.connectionType == 6) || (this.connectionType == 5))
                        {
                            pConnectOpts.SslConfigOptions = options2;
                        }
                        else
                        {
                            ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(options2.StructMQSCO));
                            Marshal.StructureToPtr(options2.StructMQSCO, ptr, false);
                            pConnectOpts.SSLConfigPtr = ptr;
                        }
                    }
                    if (this.sslPeerName == null)
                    {
                        switch (this.connectionType)
                        {
                            case 2:
                            case 4:
                                zero = Marshal.AllocCoTaskMem(Marshal.SizeOf(definition.StructMQCD));
                                Marshal.StructureToPtr(definition.StructMQCD, zero, false);
                                pConnectOpts.ClientConnPtr = zero;
                                break;

                            case 5:
                            case 6:
                                pConnectOpts.cd = definition;
                                break;
                        }
                        pConnectOpts.TraceFields();
                        if (this.pHconn == null)
                        {
                            this.pHconn = nmqiEnv.NewPhconn();
                        }
                        this.nmqiConnector.MQCONNX(queueManagerName, pConnectOpts, this.pHconn, out pCompCode, out pReason);
                        this.hConn = this.pHconn.HConn;
                    }
                    else
                    {
                        definition.SSLPeerName = this.sslPeerName;
                        definition.SSLPeerNameLength = this.sslPeerName.Length;
                        try
                        {
                            fixed (byte* numRef = definition.sslPeerName)
                            {
                                definition.SSLPeerNamePtr = (IntPtr) numRef;
                                zero = Marshal.AllocCoTaskMem(Marshal.SizeOf(definition.StructMQCD));
                                Marshal.StructureToPtr(definition.StructMQCD, zero, false);
                                pConnectOpts.ClientConnPtr = zero;
                                pConnectOpts.cd = definition;
                                pConnectOpts.TraceFields();
                                if (this.pHconn == null)
                                {
                                    this.pHconn = nmqiEnv.NewPhconn();
                                }
                                this.nmqiConnector.MQCONNX(queueManagerName, pConnectOpts, this.pHconn, out pCompCode, out pReason);
                                this.hConn = this.pHconn.HConn;
                            }
                        }
                        finally
                        {
                           // numRef = null;
                            base.TrExit(method);
                        }
                    }
                    if ((this.localAddress != null) && (this.localAddress.Length > 0))
                    {
                        definition.LocalAddress = this.localAddress;
                    }
                }
                else
                {
                    if (this.pHconn == null)
                    {
                        this.pHconn = nmqiEnv.NewPhconn();
                    }
                    this.nmqiConnector.MQCONN(queueManagerName, this.pHconn, out pCompCode, out pReason);
                    this.hConn = this.pHconn.HConn;
                }
                if (2 != pCompCode)
                {
                    base.qMgr = this;
                    base.isClosed = false;
                    if ((this.connectionType == 6) || (this.connectionType == 5))
                    {
                        this.disposer = new MQDisposer(this);
                    }
                }
                else
                {
                    base.throwNewMQException(pCompCode, pReason);
                }
                if (this.IsConnected)
                {
                    this.commandLevel = base.QueryAttribute(0x1f);
                }
            }
            catch (MQException exception)
            {
                pCompCode = exception.CompCode;
                pReason = exception.Reason;
                throw exception;
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(zero);
                    zero = IntPtr.Zero;
                }
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ptr);
                    ptr = IntPtr.Zero;
                }
                if (ptr3 != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ptr3);
                    ptr3 = IntPtr.Zero;
                }
                if (ptr4 != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ptr4);
                    ptr4 = IntPtr.Zero;
                }
                if (ptr5 != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ptr5);
                    ptr5 = IntPtr.Zero;
                }
                if (ptr6 != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ptr6);
                    ptr6 = IntPtr.Zero;
                }
                base.unsafe_compCode = pCompCode;
                base.unsafe_reason = pReason;
                base.TrExit(method);
            }
        }

        public virtual void Disconnect()
        {
            uint method = 0x234;
            this.TrEntry(method);
            int pCompCode = 0;
            int pReason = 0;
            try
            {
                if (this.IsConnected)
                {
                    try
                    {
                        base.Close();
                    }
                    catch (Exception exception)
                    {
                        base.TrException(method, exception);
                        base.TrText("MQQueueManager::Disconnect Close - [Exception thrown] " + exception.Message);
                    }
                    this.nmqiConnector.MQDISC(this.pHconn, out pCompCode, out pReason);
                    if (2 == pCompCode)
                    {
                        if (0x7e2 == pReason)
                        {
                            this.pHconn = nmqiEnv.NewPhconn();
                            this.hConn = this.pHconn.HConn;
                        }
                        else
                        {
                            base.throwNewMQException(pCompCode, pReason);
                        }
                    }
                    this.isXAEnabled = false;
                    base.qMgr = null;
                    this.commandLevel = 0;
                }
                GC.SuppressFinalize(this);
            }
            catch (MQException exception2)
            {
                pCompCode = exception2.CompCode;
                pReason = exception2.Reason;
                throw exception2;
            }
            finally
            {
                base.unsafe_compCode = pCompCode;
                base.unsafe_reason = pReason;
                base.TrExit(method);
            }
        }

        internal void Dispose(bool disposing)
        {
            uint method = 0x232;
            this.TrEntry(method, new object[] { disposing });
            try
            {
                if (this.IsConnected)
                {
                    try
                    {
                        if (!this.isXAEnabled)
                        {
                            this.Backout();
                        }
                    }
                    catch (Exception exception)
                    {
                        base.TrException(method, exception, 1);
                        base.TrText("MQQueueManager::Dispose Backout - [Exception thrown] " + exception.Message);
                    }
                    try
                    {
                        this.Disconnect();
                    }
                    catch (Exception exception2)
                    {
                        base.TrException(method, exception2, 2);
                        base.TrText("MQQueueManager::Dispose Disconnect - [Exception thrown] " + exception2.Message);
                    }
                }
                this.pHconn = nmqiEnv.NewPhconn();
                this.hConn = this.pHconn.HConn;
                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        ~MQQueueManager()
        {
            this.Dispose(false);
        }

        public MQAsyncStatus GetAsyncStatus()
        {
            uint method = 0x254;
            this.TrEntry(method);
            MQAsyncStatus pStat = new MQAsyncStatus();
            int pCompCode = 0;
            int pReason = 0;
            try
            {
                if (!this.IsConnected)
                {
                    base.TrText("Queue Manager not connected - throwing exception");
                    base.throwNewMQException(2, 0x7e2);
                }
                this.nmqiConnector.MQSTAT(this.hConn, 0, pStat, out pCompCode, out pReason);
                if (pCompCode != 0)
                {
                    base.TrText("Error in MQSTAT call - throwing exception");
                    base.qMgr.CheckHConnHealth(pReason);
                    base.throwNewMQException(pCompCode, pReason);
                }
            }
            catch (MQException exception)
            {
                pCompCode = exception.CompCode;
                pReason = exception.Reason;
                throw exception;
            }
            finally
            {
                base.unsafe_compCode = pCompCode;
                base.unsafe_reason = pReason;
                base.TrExit(method);
            }
            return pStat;
        }

        internal void PerformMsgProcessgingBeforePut(ref MQMessage mqMsg)
        {
            uint method = 0x24d;
            this.TrEntry(method, new object[] { mqMsg });
            try
            {
                mqMsg = new MQMarshalMessageForPut(mqMsg).ConstructMessageForSend();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void PerformMsgProcessingAfterPut(ref MQMessage message, byte[] tempMsgBuffer, int bodyCcsid, int encoding, string format)
        {
            uint method = 590;
            this.TrEntry(method, new object[] { message, tempMsgBuffer, bodyCcsid, encoding, format });
            try
            {
                message.ClearMessage();
                message.CharacterSet = bodyCcsid;
                message.Format = format;
                message.Encoding = encoding;
                if (tempMsgBuffer.Length != 0)
                {
                    message.Write(tempMsgBuffer);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Put(string qName, MQMessage msg)
        {
            uint method = 0x253;
            this.TrEntry(method, new object[] { qName, msg });
            try
            {
                this.Put(qName, null, msg, new MQPutMessageOptions(), null);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Put(int type, string destinationName, MQMessage message)
        {
            uint method = 0x248;
            this.TrEntry(method, new object[] { type, destinationName, message });
            try
            {
                this.Put(type, destinationName, message, new MQPutMessageOptions());
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Put(string qName, MQMessage msg, MQPutMessageOptions pmo)
        {
            uint method = 0x252;
            this.TrEntry(method, new object[] { qName, msg, pmo });
            try
            {
                this.Put(qName, null, msg, pmo, null);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Put(string qName, string qmName, MQMessage msg)
        {
            uint method = 0x251;
            this.TrEntry(method, new object[] { qName, qmName, msg });
            try
            {
                this.Put(qName, qmName, msg, new MQPutMessageOptions(), null);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Put(int type, string destinationName, MQMessage message, MQPutMessageOptions pmo)
        {
            uint method = 0x249;
            this.TrEntry(method, new object[] { type, destinationName, message, pmo });
            try
            {
                this.Put(type, destinationName, null, null, message, pmo);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Put(string qName, string qmName, MQMessage msg, MQPutMessageOptions pmo)
        {
            uint method = 0x250;
            this.TrEntry(method, new object[] { qName, qmName, msg, pmo });
            try
            {
                this.Put(qName, qmName, msg, pmo, null);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Put(int type, string destinationName, string qmName, string topicString, MQMessage message)
        {
            uint method = 0x24a;
            this.TrEntry(method, new object[] { type, destinationName, qmName, topicString, message });
            try
            {
                this.Put(type, destinationName, qmName, topicString, message, new MQPutMessageOptions());
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Put(string qName, string qmName, MQMessage msg, MQPutMessageOptions pmo, string altUserId)
        {
            uint method = 0x24f;
            this.TrEntry(method, new object[] { qName, qmName, msg, pmo, altUserId });
            try
            {
                this.Put(1, qName, qmName, null, msg, pmo, altUserId);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Put(int type, string destinationName, string qmName, string topicString, MQMessage message, MQPutMessageOptions pmo)
        {
            uint method = 0x24b;
            this.TrEntry(method, new object[] { type, destinationName, qmName, topicString, message, pmo });
            try
            {
                this.Put(type, destinationName, qmName, topicString, message, pmo, null);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Put(int type, string destinationName, string qmName, string topicString, MQMessage message, MQPutMessageOptions pmo, string altUserId)
        {
            uint method = 0x24c;
            this.TrEntry(method, new object[] { type, destinationName, qmName, topicString, message, pmo, altUserId });
            int pCompCode = 0;
            int pReason = 0;
            MQObjectDescriptor pObjDesc = new MQObjectDescriptor();
            if (type == 8)
            {
                pObjDesc.Version = 4;
                pObjDesc.ResolvedObjectString.VSBufSize = 0;
            }
            if ((Transaction.Current != null) && !this.isXAEnabled)
            {
                this.isXAEnabled = true;
            }
            pObjDesc.ObjectType = type;
            if (destinationName != null)
            {
                pObjDesc.ObjectName = destinationName;
            }
            if (topicString != null)
            {
                pObjDesc.ObjectString.VSString = topicString;
            }
            if (qmName != null)
            {
                pObjDesc.ObjectQMgrName = qmName;
            }
            if (altUserId != null)
            {
                pObjDesc.AlternateUserId = altUserId;
            }
            pObjDesc.CopyCHARVIntoOD();
            byte[] src = null;
            try
            {
                if (message == null)
                {
                    base.throwNewMQException(2, 0x7ea);
                }
                if (pmo == null)
                {
                    base.throwNewMQException(2, 0x87d);
                }
                pmo.ValidateOptions();
                src = message.GetBuffer();
                byte[] dst = new byte[src.Length];
                Buffer.BlockCopy(src, 0, dst, 0, src.Length);
                int characterSet = message.CharacterSet;
                int encoding = message.Encoding;
                string format = message.Format;
                this.PerformMsgProcessgingBeforePut(ref message);
                src = message.GetBuffer();
                this.nmqiConnector.MQPUT1(this.hConn, pObjDesc, message.md, pmo, src.Length, src, out pCompCode, out pReason);
                if (pCompCode == 0)
                {
                    this.PerformMsgProcessingAfterPut(ref message, dst, characterSet, encoding, format);
                }
                if (pCompCode != 0)
                {
                    base.qMgr.CheckHConnHealth(pReason);
                    base.throwNewMQException(pCompCode, pReason);
                }
            }
            catch (MQException exception)
            {
                pCompCode = exception.CompCode;
                pReason = exception.Reason;
                throw exception;
            }
            finally
            {
                base.unsafe_compCode = pCompCode;
                base.unsafe_reason = pReason;
                base.TrExit(method);
            }
        }

        protected void QuerySPI(int VerbId, ref int maxInOutVersion, ref int maxInVersion, ref int maxOutVersion, ref int flags)
        {
            uint method = 0x247;
            this.TrEntry(method, new object[] { VerbId, (int) maxInOutVersion, (int) maxInVersion, (int) maxOutVersion, (int) flags });
            int compCode = 0;
            int reason = 0;
            try
            {
                ((NmqiSP) this.nmqiConnector).SPIQuerySPI(this.hConn, VerbId, ref maxInOutVersion, ref maxInVersion, ref maxOutVersion, ref flags, out compCode, out reason);
                if (compCode != 0)
                {
                    base.qMgr.CheckHConnHealth(reason);
                    base.throwNewMQException(compCode, reason);
                }
            }
            catch (MQException exception)
            {
                compCode = exception.CompCode;
                reason = exception.Reason;
                throw exception;
            }
            finally
            {
                base.unsafe_compCode = compCode;
                base.unsafe_reason = reason;
                base.TrExit(method);
            }
        }

        protected void Syncpoint(int Options, int Action)
        {
            uint method = 0x246;
            this.TrEntry(method, new object[] { Options, Action });
            int compCode = 0;
            int reason = 0;
            try
            {
                MQSPISyncpointOpts spispo = new MQSPISyncpointOpts(Options);
                spispo.Action = Action;
                ((NmqiSP) this.nmqiConnector).SPISyncpoint(this.hConn, ref spispo, out compCode, out reason);
                if (compCode != 0)
                {
                    base.qMgr.CheckHConnHealth(reason);
                    base.throwNewMQException(compCode, reason);
                }
            }
            catch (MQException exception)
            {
                compCode = exception.CompCode;
                reason = exception.Reason;
                throw exception;
            }
            finally
            {
                base.unsafe_compCode = compCode;
                base.unsafe_reason = reason;
                base.TrExit(method);
            }
        }

        void IDisposable.Dispose()
        {
            uint method = 0x41d;
            this.TrEntry(method);
            try
            {
                this.Dispose(true);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public int AccountingConnOverride
        {
            get
            {
                return base.QueryAttribute(0x88);
            }
        }

        public int AccountingInterval
        {
            get
            {
                return base.QueryAttribute(0x87);
            }
        }

        public int ActivityRecording
        {
            get
            {
                return base.QueryAttribute(0x8a);
            }
        }

        public int AdoptNewMCACheck
        {
            get
            {
                return base.QueryAttribute(0x66);
            }
        }

        public int AdoptNewMCAInterval
        {
            get
            {
                return base.QueryAttribute(0x68);
            }
        }

        public int AdoptNewMCAType
        {
            get
            {
                return base.QueryAttribute(0x67);
            }
        }

        public DateTime AlterationDateTime
        {
            get
            {
                string str = base.QueryAttribute(0x7eb, 12);
                string str2 = base.QueryAttribute(0x7ec, 8);
                return DateTime.ParseExact(str.TrimEnd(new char[0]) + str2, "yyyy-MM-ddHH.mm.ss", new CultureInfo("en-US"));
            }
        }

        public int AuthorityEvent
        {
            get
            {
                return base.QueryAttribute(0x2f);
            }
        }

        public int BeginOptions
        {
            get
            {
                return this.beginOptions;
            }
            set
            {
                this.beginOptions = value;
            }
        }

        public int BridgeEvent
        {
            get
            {
                return base.QueryAttribute(0x4a);
            }
        }

        public int ChannelAutoDefinition
        {
            get
            {
                return base.QueryAttribute(0x37);
            }
        }

        public int ChannelAutoDefinitionEvent
        {
            get
            {
                return base.QueryAttribute(0x38);
            }
        }

        public string ChannelAutoDefinitionExit
        {
            get
            {
                return base.QueryAttribute(0x7ea, 0x80);
            }
        }

        public int ChannelEvent
        {
            get
            {
                return base.QueryAttribute(0x49);
            }
        }

        public int ChannelInitiatorAdapters
        {
            get
            {
                return base.QueryAttribute(0x65);
            }
        }

        public int ChannelInitiatorControl
        {
            get
            {
                return base.QueryAttribute(0x77);
            }
        }

        public int ChannelInitiatorDispatchers
        {
            get
            {
                return base.QueryAttribute(0x69);
            }
        }

        public int ChannelInitiatorTraceAutoStart
        {
            get
            {
                return base.QueryAttribute(0x75);
            }
        }

        public int ChannelInitiatorTraceTableSize
        {
            get
            {
                return base.QueryAttribute(0x76);
            }
        }

        public int ChannelMonitoring
        {
            get
            {
                return base.QueryAttribute(0x7a);
            }
        }

        public int ChannelStatistics
        {
            get
            {
                return base.QueryAttribute(0x81);
            }
        }

        public int CharacterSet
        {
            get
            {
                return base.QueryAttribute(2);
            }
        }

        public int ClusterSenderMonitoring
        {
            get
            {
                return base.QueryAttribute(0x7c);
            }
        }

        public int ClusterSenderStatistics
        {
            get
            {
                return base.QueryAttribute(130);
            }
        }

        public string ClusterWorkLoadData
        {
            get
            {
                return base.QueryAttribute(0x7f2, 0x20);
            }
        }

        public string ClusterWorkLoadExit
        {
            get
            {
                return base.QueryAttribute(0x7f1, 0x80);
            }
        }

        public int ClusterWorkLoadLength
        {
            get
            {
                return base.QueryAttribute(0x3a);
            }
        }

        public int ClusterWorkLoadMRU
        {
            get
            {
                return base.QueryAttribute(0x61);
            }
        }

        public int ClusterWorkLoadUseQ
        {
            get
            {
                return base.QueryAttribute(0x62);
            }
        }

        public int CodedCharSetId
        {
            get
            {
                return base.QueryAttribute(2);
            }
        }

        public int CommandEvent
        {
            get
            {
                return base.QueryAttribute(0x63);
            }
        }

        public string CommandInputQueueName
        {
            get
            {
                return base.QueryAttribute(0x7d3, 0x30);
            }
        }

        public int CommandLevel
        {
            get
            {
                return this.commandLevel;
            }
        }

        public int CommandServer
        {
            get
            {
                return base.QueryAttribute(120);
            }
        }

        public DateTime CreationDateTime
        {
            get
            {
                string str = base.QueryAttribute(0x7d4, 12);
                string str2 = base.QueryAttribute(0x7d5, 8);
                return DateTime.ParseExact(str.TrimEnd(new char[0]) + str2, "yyyy-MM-ddHH.mm.ss", new CultureInfo("en-US"));
            }
        }

        public string DeadLetterQueueName
        {
            get
            {
                return base.QueryAttribute(0x7d6, 0x30);
            }
        }

        public string DefaultTranmissionQueueName
        {
            get
            {
                return base.QueryAttribute(0x7e9, 0x30);
            }
        }

        public bool DistributionListCapable
        {
            get
            {
                return Convert.ToBoolean(base.QueryAttribute(0x22));
            }
        }

        public string DNSGroup
        {
            get
            {
                return base.QueryAttribute(0x817, 0x12);
            }
        }

        public int DNSWLM
        {
            get
            {
                return base.QueryAttribute(0x6a);
            }
        }

        public int ExpiryInterval
        {
            get
            {
                return base.QueryAttribute(0x27);
            }
        }

        public int InhibitEvent
        {
            get
            {
                return base.QueryAttribute(0x30);
            }
        }

        public int IPAddressVersion
        {
            get
            {
                return base.QueryAttribute(0x5d);
            }
        }

        public bool IsClient
        {
            get
            {
                return (this.connectionType != 0);
            }
        }

        public bool IsConnected
        {
            get
            {
                bool isHconnValid = this.IsHconnValid;
                if (this.hConn != null)
                {
                    base.TrText(string.Concat(new object[] { "MQQueueManager.IsConnected ", isHconnValid, " HConn: 0x", this.hConn.Value.ToString("X8") }));
                    return isHconnValid;
                }
                base.TrText("MQQueueManager.IsConnected " + isHconnValid + " HConn: (null)");
                return isHconnValid;
            }
        }

        internal bool IsHconnValid
        {
            get
            {
                return (((this.hConn != null) && (this.hConn.Value != 0)) && (-1 != this.hConn.Value));
            }
        }

        internal bool IsXAEnabled
        {
            get
            {
                return this.isXAEnabled;
            }
            set
            {
                this.isXAEnabled = true;
            }
        }

        public int KeepAlive
        {
            get
            {
                return base.QueryAttribute(0x73);
            }
        }

        public int ListenerTimer
        {
            get
            {
                return base.QueryAttribute(0x6b);
            }
        }

        public int LocalEvent
        {
            get
            {
                return base.QueryAttribute(0x31);
            }
        }

        public int LoggerEvent
        {
            get
            {
                return base.QueryAttribute(0x5e);
            }
        }

        public string LU62ARMSuffix
        {
            get
            {
                return base.QueryAttribute(0x81a, 2);
            }
        }

        public string LUGroupName
        {
            get
            {
                return base.QueryAttribute(0x818, 8);
            }
        }

        public string LUName
        {
            get
            {
                return base.QueryAttribute(0x819, 8);
            }
        }

        public int MaximumActiveChannels
        {
            get
            {
                return base.QueryAttribute(100);
            }
        }

        public int MaximumCurrentChannels
        {
            get
            {
                return base.QueryAttribute(0x6d);
            }
        }

        public int MaximumHandles
        {
            get
            {
                return base.QueryAttribute(11);
            }
        }

        public int MaximumLU62Channels
        {
            get
            {
                return base.QueryAttribute(0x6c);
            }
        }

        public int MaximumMessageLength
        {
            get
            {
                return base.QueryAttribute(13);
            }
        }

        public int MaximumPriority
        {
            get
            {
                return base.QueryAttribute(14);
            }
        }

        public int MaximumTCPChannels
        {
            get
            {
                return base.QueryAttribute(0x72);
            }
        }

        public int MaximumUncommittedMessages
        {
            get
            {
                return base.QueryAttribute(0x21);
            }
        }

        public int MQIAccounting
        {
            get
            {
                return base.QueryAttribute(0x85);
            }
        }

        public int MQIStatistics
        {
            get
            {
                return base.QueryAttribute(0x7f);
            }
        }

        public int OutboundPortMax
        {
            get
            {
                return base.QueryAttribute(140);
            }
        }

        public int OutboundPortMin
        {
            get
            {
                return base.QueryAttribute(110);
            }
        }

        public int PerformanceEvent
        {
            get
            {
                return base.QueryAttribute(0x35);
            }
        }

        public int Platform
        {
            get
            {
                return base.QueryAttribute(0x20);
            }
        }

        public int QueueAccounting
        {
            get
            {
                return base.QueryAttribute(0x86);
            }
        }

        public string QueueManagerDescription
        {
            get
            {
                return base.QueryAttribute(0x7de, 0x40);
            }
        }

        public string QueueManagerIdentifier
        {
            get
            {
                return base.QueryAttribute(0x7f0, 0x30);
            }
        }

        public int QueueMonitoring
        {
            get
            {
                return base.QueryAttribute(0x7b);
            }
        }

        public int QueueStatistics
        {
            get
            {
                return base.QueryAttribute(0x80);
            }
        }

        public int ReceiveTimeout
        {
            get
            {
                return base.QueryAttribute(0x6f);
            }
        }

        public int ReceiveTimeoutMin
        {
            get
            {
                return base.QueryAttribute(0x71);
            }
        }

        public int ReceiveTimeoutType
        {
            get
            {
                return base.QueryAttribute(0x70);
            }
        }

        public int RemoteEvent
        {
            get
            {
                return base.QueryAttribute(50);
            }
        }

        public string RepositoryName
        {
            get
            {
                return base.QueryAttribute(0x7f3, 0x30);
            }
        }

        public string RepositoryNameList
        {
            get
            {
                return base.QueryAttribute(0x7f4, 0x30);
            }
        }

        public int SharedQueueQueueManagerName
        {
            get
            {
                return base.QueryAttribute(0x4d);
            }
        }

        public int SSLEvent
        {
            get
            {
                return base.QueryAttribute(0x4b);
            }
        }

        public int SSLFips
        {
            get
            {
                return base.QueryAttribute(0x5c);
            }
        }

        public int SSLKeyResetCount
        {
            get
            {
                return base.QueryAttribute(0x4c);
            }
        }

        public int StartStopEvent
        {
            get
            {
                return base.QueryAttribute(0x34);
            }
        }

        public int StatisticsInterval
        {
            get
            {
                return base.QueryAttribute(0x83);
            }
        }

        public int SyncPointAvailability
        {
            get
            {
                return base.QueryAttribute(30);
            }
        }

        public string TCPName
        {
            get
            {
                return base.QueryAttribute(0x81b, 8);
            }
        }

        public int TCPStackType
        {
            get
            {
                return base.QueryAttribute(0x74);
            }
        }

        public int TraceRouteRecording
        {
            get
            {
                return base.QueryAttribute(0x89);
            }
        }

        public string TransmissionQueueName
        {
            get
            {
                return base.QueryAttribute(0x7e9, 0x30);
            }
        }

        public int TriggerInterval
        {
            get
            {
                return base.QueryAttribute(0x19);
            }
        }
    }
}

