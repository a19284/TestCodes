namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQAuthenticationInformationRecord : MQBase, IDisposable
    {
        private MQBase.MQAIR mqAIR;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal IntPtr userNameBuffer = IntPtr.Zero;

        public MQAuthenticationInformationRecord()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.mqAIR.StrucId = new byte[] { 0x41, 0x49, 0x52, 0x20 };
            this.mqAIR.Version = 2;
            this.mqAIR.AuthInfoType = 1;
            this.mqAIR.AuthInfoConnName = new byte[0x108];
            this.mqAIR.LDAPUserNamePtr = IntPtr.Zero;
            this.mqAIR.LDAPUserNameOffset = 0;
            this.mqAIR.LDAPUserNameLength = 0;
            this.mqAIR.LDAPPassword = new byte[0x20];
            this.mqAIR.OCSPResponderURL = new byte[0x100];
        }

        internal void Dispose(bool disposing)
        {
            uint method = 11;
            this.TrEntry(method, new object[] { disposing });
            try
            {
                if (this.userNameBuffer != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(this.userNameBuffer);
                    this.userNameBuffer = IntPtr.Zero;
                }
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

        ~MQAuthenticationInformationRecord()
        {
            this.Dispose(false);
        }

        void IDisposable.Dispose()
        {
            uint method = 0x409;
            this.TrEntry(method);
            this.Dispose(true);
            base.TrExit(method);
        }

        public string AuthInfoConnName
        {
            get
            {
                return base.GetString(this.mqAIR.AuthInfoConnName);
            }
            set
            {
                byte[] authInfoConnName = this.mqAIR.AuthInfoConnName;
                base.GetBytes(value, ref authInfoConnName);
                this.mqAIR.AuthInfoConnName = authInfoConnName;
            }
        }

        public int AuthInfoType
        {
            get
            {
                return this.mqAIR.AuthInfoType;
            }
            set
            {
                this.mqAIR.AuthInfoType = value;
            }
        }

        public string LDAPPassword
        {
            get
            {
                return base.GetString(this.mqAIR.LDAPPassword);
            }
            set
            {
                byte[] lDAPPassword = this.mqAIR.LDAPPassword;
                base.GetBytes(value, ref lDAPPassword);
                this.mqAIR.LDAPPassword = lDAPPassword;
            }
        }

        public string LDAPUserName
        {
            get
            {
                if (this.userNameBuffer != IntPtr.Zero)
                {
                    return Marshal.PtrToStringAnsi(this.userNameBuffer);
                }
                return null;
            }
            set
            {
                if (this.userNameBuffer != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(this.userNameBuffer);
                }
                this.userNameBuffer = Marshal.StringToCoTaskMemAnsi(value);
                this.mqAIR.LDAPUserNamePtr = this.userNameBuffer;
                this.mqAIR.LDAPUserNameLength = value.Length;
            }
        }

        public int LDAPUserNameLength
        {
            get
            {
                return this.mqAIR.LDAPUserNameLength;
            }
            set
            {
                this.mqAIR.LDAPUserNameLength = value;
            }
        }

        public int LDAPUserNameOffset
        {
            get
            {
                return this.mqAIR.LDAPUserNameOffset;
            }
            set
            {
                this.mqAIR.LDAPUserNameOffset = value;
            }
        }

        public IntPtr LDAPUserNamePtr
        {
            get
            {
                return this.mqAIR.LDAPUserNamePtr;
            }
            set
            {
                this.mqAIR.LDAPUserNamePtr = value;
            }
        }

        public string OCSPResponderURL
        {
            get
            {
                return base.GetString(this.mqAIR.OCSPResponderURL);
            }
            set
            {
                byte[] oCSPResponderURL = this.mqAIR.OCSPResponderURL;
                base.GetBytes(value, ref oCSPResponderURL);
                this.mqAIR.OCSPResponderURL = oCSPResponderURL;
            }
        }

        public MQBase.MQAIR StructMQAIR
        {
            get
            {
                return this.mqAIR;
            }
            set
            {
                this.mqAIR = value;
            }
        }

        public int Version
        {
            get
            {
                return this.mqAIR.Version;
            }
            set
            {
                this.mqAIR.Version = value;
            }
        }
    }
}

