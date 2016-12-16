namespace IBM.WMQ
{
    using System;

    public class MQConnectionSecurityParameters : MQBase
    {
        private MQBase.MQCSP mqCSP;
        private string password;
        private const string sccsid = "%Z% %W% %I% %E% %U%";
        private string userId;

        public MQConnectionSecurityParameters()
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%");
            this.mqCSP.StrucId = new byte[] { 0x43, 0x53, 80, 0x20 };
            this.mqCSP.Version = 1;
            this.mqCSP.AuthenticationType = 0;
            this.mqCSP.Reserved1 = new byte[4];
            this.mqCSP.CSPUserIdPtr = IntPtr.Zero;
            this.mqCSP.CSPUserIdOffset = 0;
            this.mqCSP.CSPUserIdLength = 0;
            this.mqCSP.Reserved2 = new byte[8];
            this.mqCSP.CSPPasswordPtr = IntPtr.Zero;
            this.mqCSP.CSPPasswordOffset = 0;
            this.mqCSP.CSPPasswordLength = 0;
        }

        public int AuthenticationType
        {
            get
            {
                return this.mqCSP.AuthenticationType;
            }
            set
            {
                this.mqCSP.AuthenticationType = value;
            }
        }

        public int CSPPasswordLength
        {
            get
            {
                return this.mqCSP.CSPPasswordLength;
            }
            set
            {
                this.mqCSP.CSPPasswordLength = value;
            }
        }

        public int CSPPasswordOffset
        {
            get
            {
                return this.mqCSP.CSPPasswordOffset;
            }
            set
            {
                this.mqCSP.CSPPasswordOffset = value;
            }
        }

        public IntPtr CSPPasswordPtr
        {
            get
            {
                return this.mqCSP.CSPPasswordPtr;
            }
            set
            {
                this.mqCSP.CSPPasswordPtr = value;
            }
        }

        public int CSPUserIdLength
        {
            get
            {
                return this.mqCSP.CSPUserIdLength;
            }
            set
            {
                this.mqCSP.CSPUserIdLength = value;
            }
        }

        public int CSPUserIdOffset
        {
            get
            {
                return this.mqCSP.CSPUserIdOffset;
            }
            set
            {
                this.mqCSP.CSPUserIdOffset = value;
            }
        }

        public IntPtr CSPUserIdPtr
        {
            get
            {
                return this.mqCSP.CSPUserIdPtr;
            }
            set
            {
                this.mqCSP.CSPUserIdPtr = value;
            }
        }

        public string Password
        {
            get
            {
                return this.password;
            }
            set
            {
                this.password = value;
            }
        }

        public MQBase.MQCSP StructMQCSP
        {
            get
            {
                return this.mqCSP;
            }
            set
            {
                this.mqCSP = value;
            }
        }

        public string UserId
        {
            get
            {
                return this.userId;
            }
            set
            {
                this.userId = value;
            }
        }

        public int Version
        {
            get
            {
                return this.mqCSP.Version;
            }
            set
            {
                this.mqCSP.Version = value;
            }
        }
    }
}

