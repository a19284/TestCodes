namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class NmqiEnvironment : MQBase
    {
        private static string bitmode;
        private readonly Random branchQualifier = new Random();
        public const char CALLER_BASE_JAVA = 'B';
        public const char CALLER_UNKNOWN = 'U';
        public const char CALLER_WAS = 'W';
        public const char CALLER_WMQ_V6_LEG = 'N';
        public const char CALLER_WMQ_V7_LEG = 'M';
        private MQClientCfg cfg;
        private MQCcsidTable defaultCharset;
        public const int DONT_USE_SHARED_HCONN = 2;
        private NmqiException lastException;
        public const int LOCAL_CLIENT = 1;
        public const int LOCAL_SERVER = 0;
        public const int LOCAL_XACLIENT = 3;
        internal const int MQCLIENT_THREAD_DISPATCHER = 2;
        internal const int MQCLIENT_THREAD_RECEIVER = 1;
        internal const int MQCLIENT_THREAD_RECONNECT = 3;
        internal const string MQCLIENT_THREAD_TYPE = "MQ_CLIENT_THREAD_TYPE";
        private Guid MQDotnetRMIdentifier;
        private Hashtable mqiCache = new Hashtable();
        private static Hashtable MQResourceManagerIds = new Hashtable();
        private static int nextAvailablermid = 0;
        public static int nextAvailableRmid;
        public static object nextAvailableRmidLock = new object();
        protected NmqiPropertyHandler properties;
        public const int REMOTE = 2;
        private readonly Random resourceManagerIdentifier = new Random();
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        public const int UNDEFINED = -1;
        public const int USE_WORKER_THREAD = 1;

        public NmqiEnvironment(NmqiPropertyHandler nmqiPropertyHandler)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiPropertyHandler });
            this.properties = nmqiPropertyHandler;
            bitmode = Convert.ToString(Marshal.SizeOf(typeof(IntPtr)));
            this.cfg = new MQClientCfg(this);
            this.cfg.FindAndParse();
            if (Thread.GetNamedDataSlot("MQ_CLIENT_THREAD_TYPE") == null)
            {
                Thread.AllocateNamedDataSlot("MQ_CLIENT_THREAD_TYPE");
            }
        }

        private NmqiMQ GetInstance(string name)
        {
            uint method = 0x2e2;
            this.TrEntry(method, new object[] { name });
            NmqiMQ imq = null;
            try
            {
                imq = (NmqiMQ) Assembly.GetExecutingAssembly().CreateInstance(name, true, BindingFlags.CreateInstance, null, new object[] { this, 0 }, CultureInfo.CurrentCulture, null);
            }
            catch (Exception exception)
            {
                base.TrException(method, exception);
                throw;
            }
            finally
            {
                base.TrExit(method);
            }
            return imq;
        }

        public NmqiMQ GetMQI(int id)
        {
            uint method = 0x2e1;
            this.TrEntry(method, new object[] { id });
            NmqiMQ instance = null;
            string name = null;
            base.TrText(method, "Component ID Supplied -> " + id);
            switch (id)
            {
                case 0:
                    name = "IBM.WMQ.Nmqi.BindingsNmqiMQ";
                    break;

                case 1:
                    name = "IBM.WMQ.Nmqi.UnmanagedNmqiMQ";
                    break;

                case 2:
                    name = "IBM.WMQ.Nmqi.MQFAP";
                    break;

                case 3:
                    name = "IBM.WMQ.Nmqi.XAUnmanagedNmqiMQ";
                    break;

                default:
                {
                    string text1 = "The id '" + id + "' was not valid";
                    break;
                }
            }
            try
            {
                base.TrText(method, "Component Name -> " + name);
                lock (this.mqiCache.SyncRoot)
                {
                    instance = (NmqiMQ) this.mqiCache[name];
                    if (instance == null)
                    {
                        if (id == 2)
                        {
                            instance = new MQFAP(this);
                        }
                        else
                        {
                            instance = this.GetInstance(name);
                        }
                        this.mqiCache.Add(name, instance);
                    }
                    return instance;
                }
                return instance;
            }
            catch (MQException exception)
            {
                base.TrException(method, exception);
                throw;
            }
            finally
            {
                base.TrExit(method);
            }
            return instance;
        }

        public Guid GetMQResourceManagerIdentifier(string qmgrName)
        {
            Guid guid;
            lock (MQResourceManagerIds.SyncRoot)
            {
                if (MQResourceManagerIds.ContainsKey(qmgrName))
                {
                    return (Guid) MQResourceManagerIds[qmgrName];
                }
                byte[] buffer = new byte[0x10];
                base.GetBytes(qmgrName, ref buffer);
                this.resourceManagerIdentifier.NextBytes(buffer);
                guid = new Guid(buffer);
                MQResourceManagerIds.Add(qmgrName, guid);
            }
            return guid;
        }

        public string GetProperty(string name)
        {
            uint method = 0x3c2;
            this.TrEntry(method, new object[] { name });
            string property = this.properties.GetProperty(name);
            base.TrExit(method, property);
            return property;
        }

        public LpiNotifyDetails NewLpiND()
        {
            uint method = 0x3c6;
            this.TrEntry(method);
            LpiNotifyDetails result = new LpiNotifyDetails();
            base.TrExit(method, result);
            return result;
        }

        public LpiNotifyDetails NewLpiNotifyDetails()
        {
            uint method = 0x3d2;
            this.TrEntry(method);
            LpiNotifyDetails result = new LpiNotifyDetails();
            base.TrExit(method, result);
            return result;
        }

        public LpiSDSubProps NewLpiSDSubProps()
        {
            uint method = 0x3d3;
            this.TrEntry(method);
            LpiSDSubProps result = new LpiSDSubProps();
            base.TrExit(method, result);
            return result;
        }

        public LpiUSD NewLpiUSD()
        {
            uint method = 980;
            this.TrEntry(method);
            LpiUSD result = new LpiUSD();
            base.TrExit(method, result);
            return result;
        }

        public MQAuthenticationInformationRecord NewMQAIR()
        {
            uint method = 0x3c3;
            this.TrEntry(method);
            MQAuthenticationInformationRecord result = new MQAuthenticationInformationRecord();
            base.TrExit(method, result);
            return result;
        }

        public MQBeginOptions NewMQBO()
        {
            uint method = 0x3c8;
            this.TrEntry(method);
            MQBeginOptions result = new MQBeginOptions();
            base.TrExit(method, result);
            return result;
        }

        public MQCBC NewMQCBC()
        {
            uint method = 0x3cd;
            this.TrEntry(method);
            MQCBC result = new MQCBC();
            base.TrExit(method, result);
            return result;
        }

        public MQCBD NewMQCBD()
        {
            uint method = 0x3cf;
            this.TrEntry(method);
            MQCBD result = new MQCBD();
            base.TrExit(method, result);
            return result;
        }

        public MQChannelDefinition NewMQCD()
        {
            uint method = 0x2e3;
            this.TrEntry(method);
            MQChannelDefinition result = new MQChannelDefinition();
            base.TrExit(method, result);
            return result;
        }

        public MQCHARV NewMQCHARV(bool input, bool output)
        {
            uint method = 970;
            this.TrEntry(method, new object[] { input, output });
            MQCHARV result = new MQCHARV(input, output);
            base.TrExit(method, result);
            return result;
        }

        public MQConnectOptions NewMQCNO()
        {
            uint method = 740;
            this.TrEntry(method);
            MQConnectOptions result = new MQConnectOptions();
            base.TrExit(method, result);
            return result;
        }

        public MQConnectionSecurityParameters NewMQCSP()
        {
            uint method = 0x2e5;
            this.TrEntry(method);
            MQConnectionSecurityParameters result = new MQConnectionSecurityParameters();
            base.TrExit(method, result);
            return result;
        }

        public MQCTLO NewMQCTLO()
        {
            uint method = 0x3ce;
            this.TrEntry(method);
            MQCTLO result = new MQCTLO();
            base.TrExit(method, result);
            return result;
        }

        public MQChannelExit NewMQCXP()
        {
            uint method = 0x3cb;
            this.TrEntry(method);
            MQChannelExit result = new MQChannelExit();
            base.TrExit(method, result);
            return result;
        }

        public MQDLH NewMQDLH()
        {
            uint method = 0x43e;
            this.TrEntry(method);
            MQDLH result = new MQDLH();
            base.TrExit(method, result);
            return result;
        }

        public MQGetMessageOptions NewMQGMO()
        {
            uint method = 0x2e7;
            this.TrEntry(method);
            MQGetMessageOptions result = new MQGetMessageOptions();
            base.TrExit(method, result);
            return result;
        }

        public MQMessageDescriptor NewMQMD()
        {
            uint method = 0x2e8;
            this.TrEntry(method);
            MQMessageDescriptor result = new MQMessageDescriptor();
            base.TrExit(method, result);
            return result;
        }

        public MQObjectDescriptor NewMQOD()
        {
            uint method = 0x2e9;
            this.TrEntry(method);
            MQObjectDescriptor result = new MQObjectDescriptor();
            base.TrExit(method, result);
            return result;
        }

        public MQPropertyDescriptor NewMQPD()
        {
            uint method = 0x3cc;
            this.TrEntry(method);
            MQPropertyDescriptor result = new MQPropertyDescriptor();
            base.TrExit(method, result);
            return result;
        }

        public MQPutMessageOptions NewMQPMO()
        {
            uint method = 0x2e6;
            this.TrEntry(method);
            MQPutMessageOptions result = new MQPutMessageOptions();
            base.TrExit(method, result);
            return result;
        }

        public MQSSLConfigOptions NewMQSCO()
        {
            uint method = 0x3d0;
            this.TrEntry(method);
            MQSSLConfigOptions result = new MQSSLConfigOptions();
            base.TrExit(method, result);
            return result;
        }

        public MQSubscriptionDescriptor NewMQSD()
        {
            uint method = 0x3c4;
            this.TrEntry(method);
            MQSubscriptionDescriptor result = new MQSubscriptionDescriptor();
            base.TrExit(method, result);
            return result;
        }

        public MQSubscriptionRequestOptions NewMQSRO()
        {
            uint method = 0x3c5;
            this.TrEntry(method);
            MQSubscriptionRequestOptions result = new MQSubscriptionRequestOptions();
            base.TrExit(method, result);
            return result;
        }

        public MQAsyncStatus NewMQSTS()
        {
            uint method = 0x3c7;
            this.TrEntry(method);
            MQAsyncStatus result = new MQAsyncStatus();
            base.TrExit(method, result);
            return result;
        }

        public NmqiConnectOptions NewNmqiConnectOptions()
        {
            uint method = 0x2ea;
            this.TrEntry(method);
            NmqiConnectOptions result = new NmqiConnectOptions();
            base.TrExit(method, result);
            return result;
        }

        public Phconn NewPhconn()
        {
            uint method = 0x2eb;
            this.TrEntry(method);
            Phconn result = new Phconn(this);
            base.TrExit(method, result);
            return result;
        }

        public Phobj NewPhobj()
        {
            uint method = 0x2ec;
            this.TrEntry(method);
            Phobj result = new Phobj(this);
            base.TrExit(method, result);
            return result;
        }

        public QueueManagerInfo NewQueueManagerInfo()
        {
            uint method = 0x3c9;
            this.TrEntry(method);
            QueueManagerInfo result = new QueueManagerInfo(this);
            base.TrExit(method, result);
            return result;
        }

        public SpiConnectOptions NewSpiConnectOptions()
        {
            uint method = 0x3d5;
            this.TrEntry(method);
            SpiConnectOptions result = new SpiConnectOptions();
            base.TrExit(method, result);
            return result;
        }

        public MQSPIGetOpts NewSpiGetOptions()
        {
            uint method = 0x3d6;
            this.TrEntry(method);
            MQSPIGetOpts result = new MQSPIGetOpts();
            base.TrExit(method, result);
            return result;
        }

        public SpiOpenOptions NewSpiOpenOptions()
        {
            uint method = 0x3d1;
            this.TrEntry(method);
            SpiOpenOptions result = new SpiOpenOptions();
            base.TrExit(method, result);
            return result;
        }

        public MQSPIPutOpts NewSpiPutOptions()
        {
            uint method = 0x3d7;
            this.TrEntry(method);
            MQSPIPutOpts result = new MQSPIPutOpts();
            base.TrExit(method, result);
            return result;
        }

        public LpiSD NewSpiSD()
        {
            uint method = 0x3d8;
            this.TrEntry(method);
            LpiSD result = new LpiSD();
            base.TrExit(method, result);
            return result;
        }

        public int CCSID
        {
            get
            {
                return MQCcsidTable.GetDefaultEncoding();
            }
        }

        public MQClientCfg Cfg
        {
            get
            {
                return this.cfg;
            }
        }

        internal int GetBranchQualifier
        {
            get
            {
                return this.branchQualifier.Next();
            }
        }

        internal Guid GetResourceManagerIdentifier
        {
            get
            {
                return this.MQDotnetRMIdentifier;
            }
        }

        public Exception LastException
        {
            get
            {
                return this.lastException;
            }
            set
            {
                if (value is NmqiException)
                {
                    this.lastException = (NmqiException) value;
                }
                else if (value is MQException)
                {
                    this.lastException = new NmqiException(this, -1, null, ((MQException) value).CompCode, ((MQException) value).Reason, value);
                }
                else
                {
                    this.lastException = new NmqiException(this, -1, null, 2, 0x893, value);
                }
            }
        }

        public MQCcsidTable NativeCharset
        {
            get
            {
                return this.defaultCharset;
            }
        }

        internal int NextAvailableRmid
        {
            get
            {
                return Interlocked.Increment(ref nextAvailablermid);
            }
        }

        public NmqiPropertyHandler Properties
        {
            get
            {
                return this.properties;
            }
        }
    }
}

