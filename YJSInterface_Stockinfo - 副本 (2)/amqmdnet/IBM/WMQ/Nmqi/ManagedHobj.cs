namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;

    public class ManagedHobj : NmqiObject, IBM.WMQ.Nmqi.Hobj
    {
        private MQCBD callbackDesc;
        private MQGetMessageOptions callbackGetMsgOpts;
        private MQMessageDescriptor callbackMsgDesc;
        private bool callbackRegistered;
        private bool callbackSuspended;
        private int defaultPersistence;
        private int defaultPropertyControl;
        private int defaultPutResponseType;
        private int defaultReadAhead;
        private ManagedHobj localHobj;
        private bool logicallyClosed;
        private MQObjectDescriptor objectDescriptor;
        private string objectName;
        private int objectType;
        private int openOptions;
        private string originalObjectName;
        private ManagedHsub parentHsub;
        private MQProxyQueue proxyQueue;
        private bool reconnectable;
        public const string sccsid = "%Z% %W% %I% %E% %U%";
        private bool spiCall;
        private SpiOpenOptions spiOpenOptions;
        private int value_;

        protected ManagedHobj(NmqiEnvironment env) : base(env)
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { env });
            this.value_ = 0;
        }

        public ManagedHobj(NmqiEnvironment env, int value) : base(env)
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { env, value });
            this.value_ = value;
        }

        public ManagedHobj(NmqiEnvironment env, int hSub, ManagedHobj hObj) : base(env)
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { env, hSub, hObj });
            this.value_ = hSub;
            this.localHobj = hObj;
        }

        public ManagedHobj(NmqiEnvironment env, int hobjInteger, MQProxyQueue proxyQueue, string objectName, int objectType, int openOptions, int defaultPersistence, int defaultPutReponseType, int defaultReadAhead, int defaultPropertyControl, MQObjectDescriptor objectDescriptor) : base(env)
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { env, hobjInteger, proxyQueue, objectName, objectType, openOptions, defaultPersistence, defaultPutReponseType, defaultReadAhead, defaultPropertyControl, objectDescriptor });
            this.value_ = hobjInteger;
            this.proxyQueue = proxyQueue;
            this.objectName = objectName;
            this.objectType = objectType;
            this.openOptions = openOptions;
            this.defaultPersistence = defaultPersistence;
            this.defaultPutResponseType = defaultPutReponseType;
            this.defaultReadAhead = defaultReadAhead;
            this.objectDescriptor = objectDescriptor;
            this.defaultPropertyControl = defaultPropertyControl;
        }

        public ManagedHobj(NmqiEnvironment env, int hobjInteger, MQProxyQueue proxyQueue, string objectName, int objectType, int openOptions, int defaultPersistence, int defaultPutReponseType, int defaultReadAhead, int defaultPropertyControl, MQObjectDescriptor objectDescriptor, SpiOpenOptions openOpts, bool spiCall) : base(env)
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { env, hobjInteger, proxyQueue, objectName, objectType, openOptions, defaultPersistence, defaultPutReponseType, defaultReadAhead, defaultPropertyControl, objectDescriptor });
            this.value_ = hobjInteger;
            this.proxyQueue = proxyQueue;
            this.objectName = objectName;
            this.objectType = objectType;
            this.openOptions = openOptions;
            this.defaultPersistence = defaultPersistence;
            this.defaultPutResponseType = defaultPutReponseType;
            this.defaultReadAhead = defaultReadAhead;
            this.objectDescriptor = objectDescriptor;
            this.defaultPropertyControl = defaultPropertyControl;
            this.spiOpenOptions = openOpts;
            this.spiCall = spiCall;
        }

        public bool Equals(IBM.WMQ.Nmqi.Hobj obj)
        {
            uint method = 0x2fc;
            this.TrEntry(method, new object[] { obj });
            bool result = obj.Handle.Equals(obj.Handle);
            base.TrExit(method, result);
            return result;
        }

        public static IBM.WMQ.Nmqi.Hobj GetHobj(NmqiEnvironment env, ManagedHobj localhobj)
        {
            if (localhobj == null)
            {
                return new ManagedHobj(env);
            }
            switch (localhobj.value_)
            {
                case 0:
                case -1:
                    return new ManagedHobj(env);
            }
            return localhobj;
        }

        public static ManagedHobj GetManagedHobj(NmqiEnvironment env, IBM.WMQ.Nmqi.Hobj hobj)
        {
            if (hobj is ManagedHobj)
            {
                return (ManagedHobj) hobj;
            }
            if (hobj.Equals(0))
            {
                return new ManagedHobj(env, 0);
            }
            return new ManagedHobj(env, -1);
        }

        public void SetHobj(Phobj phobj)
        {
            uint method = 0x2fb;
            this.TrEntry(method);
            phobj.HOBJ = this;
            base.TrExit(method);
        }

        public void SetupCallback(MQCBD callbackDesc, int operation, MQMessageDescriptor callbackMsgDesc, MQGetMessageOptions callbackGetMsgOpts)
        {
            uint method = 0x594;
            this.TrEntry(method, new object[] { callbackDesc, operation, callbackMsgDesc, callbackGetMsgOpts });
            this.callbackDesc = callbackDesc;
            this.callbackMsgDesc = callbackMsgDesc;
            this.callbackGetMsgOpts = callbackGetMsgOpts;
            this.callbackRegistered = (operation & 0x100) != 0;
            this.callbackSuspended = (operation & 0x10000) != 0;
            base.TrExit(method);
        }

        public override string ToString()
        {
            uint method = 0x2fa;
            this.TrEntry(method);
            string result = "0x" + Convert.ToString(this.value_);
            base.TrExit(method, result);
            return result;
        }

        public MQCBD CallbackDescriptor
        {
            get
            {
                return this.callbackDesc;
            }
        }

        public MQGetMessageOptions CallbackGetMessageOptions
        {
            get
            {
                return this.callbackGetMsgOpts;
            }
        }

        public MQMessageDescriptor CallbackMessageDescriptor
        {
            get
            {
                return this.callbackMsgDesc;
            }
        }

        public virtual int DefPersistence
        {
            get
            {
                return this.defaultPersistence;
            }
            set
            {
                this.defaultPersistence = value;
            }
        }

        public virtual int DefPropertyControl
        {
            get
            {
                return this.defaultPropertyControl;
            }
            set
            {
                this.defaultPropertyControl = value;
            }
        }

        public virtual int DefPutResponseType
        {
            get
            {
                return this.defaultPutResponseType;
            }
            set
            {
                this.defaultPutResponseType = value;
            }
        }

        public virtual int DefReadAhead
        {
            get
            {
                return this.defaultReadAhead;
            }
            set
            {
                this.defaultReadAhead = value;
            }
        }

        public virtual int Handle
        {
            get
            {
                return this.value_;
            }
            set
            {
                this.value_ = value;
            }
        }

        public virtual ManagedHobj Hobj
        {
            get
            {
                return this.localHobj;
            }
            set
            {
                this.localHobj = value;
            }
        }

        public bool IsCallbackRegistered
        {
            get
            {
                return this.callbackRegistered;
            }
        }

        public bool IsCallbackSuspended
        {
            get
            {
                return this.callbackSuspended;
            }
        }

        public bool LogicallyClosed
        {
            get
            {
                return this.logicallyClosed;
            }
            set
            {
                this.logicallyClosed = value;
            }
        }

        public MQObjectDescriptor Mqod
        {
            get
            {
                return this.objectDescriptor;
            }
            set
            {
                this.objectDescriptor = value;
            }
        }

        public int OpenOptions
        {
            get
            {
                return this.openOptions;
            }
            set
            {
                this.openOptions = value;
            }
        }

        public string OriginalObjectName
        {
            get
            {
                return this.originalObjectName;
            }
            set
            {
                this.originalObjectName = value;
            }
        }

        internal ManagedHsub ParentHsub
        {
            get
            {
                return this.parentHsub;
            }
            set
            {
                this.parentHsub = value;
            }
        }

        public virtual MQProxyQueue ProxyQueue
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

        public bool Reconnectable
        {
            get
            {
                return this.reconnectable;
            }
            set
            {
                this.reconnectable = value;
            }
        }

        public bool SpiCall
        {
            get
            {
                return this.spiCall;
            }
        }

        public SpiOpenOptions SpiOpenOpts
        {
            get
            {
                return this.spiOpenOptions;
            }
            set
            {
                this.spiOpenOptions = value;
            }
        }

        public virtual int Value
        {
            get
            {
                return this.value_;
            }
            set
            {
                this.value_ = value;
            }
        }
    }
}

