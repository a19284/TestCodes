namespace IBM.WMQ.Nmqi
{
    using System;

    public class BindingsHconn : NmqiObject, Hconn
    {
        private byte[] connectionId;
        private NmqiMQ mqi;
        private Hconn parent;
        private Phconn pHconn;
        private QueueManagerInfo qMgrInfo;
        public const string sccsid = "%Z% %W%  %I% %E% %U%";
        private object sharedHConnLock;
        private int shareOption;
        private object useWorkerThread;
        private int value_;

        public BindingsHconn(NmqiEnvironment env, object userworkerThread) : base(env)
        {
            this.sharedHConnLock = new object();
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { env, userworkerThread });
            this.useWorkerThread = userworkerThread;
        }

        public BindingsHconn(NmqiEnvironment env, object userworkerThread, int value) : base(env)
        {
            this.sharedHConnLock = new object();
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { env, userworkerThread, value });
            this.value_ = value;
            this.useWorkerThread = userworkerThread;
        }

        public static BindingsHconn GetBindingsHconn(NmqiEnvironment env, object userWorkerThread, Hconn hconn)
        {
            if (hconn is BindingsHconn)
            {
                return (BindingsHconn) hconn;
            }
            if (hconn is HconnAdapter)
            {
                return new BindingsHconn(env, userWorkerThread, 0);
            }
            return new BindingsHconn(env, userWorkerThread);
        }

        public override string ToString()
        {
            uint method = 0x311;
            this.TrEntry(method);
            string result = "0x" + Convert.ToString(this.value_);
            base.TrExit(method, result);
            return result;
        }

        internal void UpdateHconn(NmqiMQ mqInstance, Phconn phconn)
        {
            uint method = 0x312;
            this.TrEntry(method, new object[] { mqInstance, phconn });
            try
            {
                this.MQI = mqInstance;
                Hconn hconn = null;
                switch (this.value_)
                {
                    case -1:
                        hconn = new HconnAdapter(-1);
                        break;

                    case 0:
                        hconn = new HconnAdapter(0);
                        break;

                    default:
                        hconn = this;
                        if (this.qMgrInfo == null)
                        {
                            this.qMgrInfo = NmqiTools.GetQueueManagerInfo(base.env, mqInstance, this);
                        }
                        break;
                }
                phconn.HConn = hconn;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public int Ccsid
        {
            get
            {
                return this.qMgrInfo.Ccsid;
            }
        }

        public int CmdLevel
        {
            get
            {
                return this.qMgrInfo.CommandLevel;
            }
        }

        public byte[] ConnectionId
        {
            get
            {
                return this.connectionId;
            }
            set
            {
                this.connectionId = value;
            }
        }

        public int FapLevel
        {
            get
            {
                return -1;
            }
        }

        public NmqiMQ MQI
        {
            get
            {
                return this.mqi;
            }
            set
            {
                this.mqi = value;
            }
        }

        public string Name
        {
            get
            {
                return this.qMgrInfo.Name;
            }
        }

        public Hconn Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                this.parent = value;
            }
        }

        public Phconn PHconn
        {
            get
            {
                return this.pHconn;
            }
            set
            {
                this.pHconn = value;
            }
        }

        public int Platform
        {
            get
            {
                return this.qMgrInfo.Platform;
            }
        }

        public int ShareOption
        {
            get
            {
                return this.shareOption;
            }
            set
            {
                this.shareOption = value;
            }
        }

        public int SharingConversations
        {
            get
            {
                return -1;
            }
        }

        public string Uid
        {
            get
            {
                return this.qMgrInfo.Uid;
            }
        }

        public int Value
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

