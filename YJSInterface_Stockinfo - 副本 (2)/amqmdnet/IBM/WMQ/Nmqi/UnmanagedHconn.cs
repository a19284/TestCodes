namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;

    public class UnmanagedHconn : NmqiObject, Hconn
    {
        private byte[] connectionId;
        private NmqiMQ mqi;
        private Hconn parent;
        private Phconn pHconn;
        private QueueManagerInfo qMgrInfo;
        private bool rectonnectable;
        public const string sccsid = "%Z% %W%  %I% %E% %U%";
        private object sharedHConnLock;
        private int shareOption;
        private MQSPIQueryOut spiqo;
        private object useWorkerThread;
        private int value_;

        public UnmanagedHconn(NmqiEnvironment env, object userworkerThread) : base(env)
        {
            this.sharedHConnLock = new object();
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { env, userworkerThread });
            this.useWorkerThread = userworkerThread;
        }

        public UnmanagedHconn(NmqiEnvironment env, object userworkerThread, int value) : base(env)
        {
            this.sharedHConnLock = new object();
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { env, userworkerThread, value });
            this.value_ = value;
            this.useWorkerThread = userworkerThread;
        }

        public static UnmanagedHconn GetUnmanagedHconn(NmqiEnvironment env, object userWorkerThread, Hconn hconn)
        {
            if (hconn is UnmanagedHconn)
            {
                return (UnmanagedHconn) hconn;
            }
            if (hconn is HconnAdapter)
            {
                return new UnmanagedHconn(env, userWorkerThread, 0);
            }
            return new UnmanagedHconn(env, userWorkerThread);
        }

        public override string ToString()
        {
            uint method = 760;
            this.TrEntry(method);
            string result = "0x" + Convert.ToString(this.value_);
            base.TrExit(method, result);
            return result;
        }

        internal void UpdateHconn(NmqiMQ mqInstance, Phconn phconn)
        {
            uint method = 0x2f9;
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
                if (this.rectonnectable)
                {
                    this.qMgrInfo = NmqiTools.GetQueueManagerInfo(base.env, this.mqi, this);
                }
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

        public bool Reconnectable
        {
            get
            {
                return this.rectonnectable;
            }
            set
            {
                this.rectonnectable = value;
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
                int convPerSocket = 0;
                try
                {
                    int hConn = this.Value;
                    int hObj = 0;
                    int compCode = 0;
                    int reason = 0;
                    MQSPIShareCnvIn @in = new MQSPIShareCnvIn();
                    MQSPIShareCnvInOut @out = new MQSPIShareCnvInOut();
                    MQSPIShareCnvOut out2 = new MQSPIShareCnvOut();
                    byte[] pOut = out2.ToBuffer();
                    if (UnmanagedNmqiMQ.nativeManager != null)
                    {
                        UnmanagedNmqiMQ.nativeManager.zstSPI(hConn, 14, hObj, @out.ToBuffer(), @in.ToBuffer(), pOut, out compCode, out reason);
                        if ((compCode == 0) && (reason == 0))
                        {
                            out2.ReadStruct(pOut, 0);
                            convPerSocket = out2.ConvPerSocket;
                        }
                    }
                }
                catch (Exception)
                {
                }
                return convPerSocket;
            }
        }

        public MQSPIQueryOut Spiqo
        {
            get
            {
                return this.spiqo;
            }
            set
            {
                this.spiqo = value;
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

