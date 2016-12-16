namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;

    public class ManagedHsub : NmqiObject, IBM.WMQ.Nmqi.Hobj
    {
        private int hsub;
        private long initialSubscriptionTime;
        private bool logicallyClosed;
        private ManagedHobj remoteHobj;
        public const string sccsid = "%Z% %W% %I% %E% %U%";
        private bool spiCall;
        private LpiSD spiSD;
        private MQSubscriptionDescriptor subDescriptor;

        public ManagedHsub(NmqiEnvironment env, int hsub, ManagedHobj hobj, MQSubscriptionDescriptor subDescriptor, LpiSD spiSD, bool spiCall) : base(env)
        {
            this.initialSubscriptionTime = DateTime.Now.Millisecond;
            this.hsub = -1;
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { env, hsub, hobj, subDescriptor });
            this.hsub = hsub;
            this.remoteHobj = hobj;
            this.subDescriptor = env.NewMQSD();
            this.subDescriptor.Version = subDescriptor.Version;
            if (subDescriptor.AlternateUserId != null)
            {
                this.subDescriptor.AlternateUserId = subDescriptor.AlternateUserId;
            }
            this.subDescriptor.AlternateSecurityId = subDescriptor.AlternateSecurityId;
            if (subDescriptor.ObjectName != null)
            {
                this.subDescriptor.ObjectName = subDescriptor.ObjectName;
            }
            if (subDescriptor.ObjectString.VSString != null)
            {
                this.subDescriptor.ObjectString.VSString = subDescriptor.ObjectString.VSString;
                this.subDescriptor.ObjectString.VSBufSize = subDescriptor.ObjectString.VSBufSize;
                this.subDescriptor.ObjectString.VSLength = subDescriptor.ObjectString.VSLength;
            }
            if (subDescriptor.SubName.VSString != null)
            {
                this.subDescriptor.SubName.VSString = subDescriptor.SubName.VSString;
                this.subDescriptor.SubName.VSBufSize = subDescriptor.SubName.VSBufSize;
            }
            if (subDescriptor.SelectionString.VSString != null)
            {
                this.subDescriptor.SelectionString.VSString = subDescriptor.SelectionString.VSString;
                this.subDescriptor.SelectionString.VSBufSize = subDescriptor.SelectionString.VSBufSize;
            }
            if (subDescriptor.ResObjectString.VSString != null)
            {
                this.subDescriptor.ResObjectString.VSString = subDescriptor.ResObjectString.VSString;
                this.subDescriptor.ResObjectString.VSBufSize = subDescriptor.ResObjectString.VSBufSize;
            }
            this.subDescriptor.Options = subDescriptor.Options;
            this.subDescriptor.SubExpiry = subDescriptor.SubExpiry;
            this.subDescriptor.SubLevel = subDescriptor.SubLevel;
            if (spiSD != null)
            {
                this.spiSD = env.NewSpiSD();
                try
                {
                    byte[] b = new byte[spiSD.GetRequiredBufferSize()];
                    spiSD.WriteStruct(b, 0);
                    this.spiSD.ReadStruct(b, 0);
                }
                catch (NmqiException)
                {
                }
            }
            this.spiCall = spiCall;
        }

        public int GeExpiryRemainder()
        {
            uint method = 0x595;
            this.TrEntry(method);
            long num2 = (DateTime.Now.Millisecond - this.initialSubscriptionTime) / 100L;
            if (this.subDescriptor.SubExpiry > num2)
            {
                int result = this.subDescriptor.SubExpiry - ((int) num2);
                base.TrExit(method, result, 1);
                return result;
            }
            base.TrExit(method, 1, 2);
            return 1;
        }

        public int Handle
        {
            get
            {
                return this.hsub;
            }
            set
            {
                this.hsub = value;
            }
        }

        public ManagedHobj Hobj
        {
            get
            {
                return this.remoteHobj;
            }
            set
            {
                this.remoteHobj = value;
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

        public MQSubscriptionDescriptor Mqsd
        {
            get
            {
                return this.subDescriptor;
            }
            set
            {
                this.subDescriptor = value;
            }
        }

        public int PropertyControl
        {
            get
            {
                return 1;
            }
        }

        public bool SpiCall
        {
            get
            {
                return this.spiCall;
            }
        }

        public LpiSD SpiSD
        {
            get
            {
                return this.spiSD;
            }
        }
    }
}

