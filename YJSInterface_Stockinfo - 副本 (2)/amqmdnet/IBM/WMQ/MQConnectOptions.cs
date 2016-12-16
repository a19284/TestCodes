namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Runtime.InteropServices;

    public class MQConnectOptions : MQBase
    {
        internal MQChannelDefinition cd;
        private MQBase.MQCNO mqCNO;
        private MQSSLConfigOptions mqSCO;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private MQConnectionSecurityParameters securityParms;

        public MQConnectOptions()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.mqCNO.StrucId = new byte[] { 0x43, 0x4e, 0x4f, 0x20 };
            this.mqCNO.Version = 1;
            this.mqCNO.Options = 0;
            this.mqCNO.ClientConnOffset = 0;
            this.mqCNO.ClientConnPtr = IntPtr.Zero;
            this.mqCNO.ConnTag = new byte[0x80];
            this.mqCNO.SSLConfigPtr = IntPtr.Zero;
            this.mqCNO.SSLConfigOffset = 0;
            this.mqCNO.SecurityParmsPtr = IntPtr.Zero;
            this.mqCNO.SecurityParmsOffset = 0;
            this.mqCNO.ConnectionId = new byte[0x18];
        }

        public override void AddFieldsToFormatter(NmqiStructureFormatter fmt)
        {
            base.AddFieldsToFormatter(fmt);
            fmt.Add("version", this.Version);
            fmt.Add("options", this.Options);
            fmt.Add("clientConnOffset", this.ClientConnOffset);
            fmt.Add("clientConnPtr", this.ClientConnPtr);
            fmt.Add("connTag", this.ConnTag);
            fmt.Add("_SSLConfigPtr", this.SSLConfigPtr);
            fmt.Add("_SSLConfigOffset", this.SSLConfigOffset);
            fmt.Add("connectionId", this.ConnectionId);
            fmt.Add("securityParmsOffset", this.SecurityParmsOffset);
            fmt.Add("securityParmsPtr", this.SecurityParmsPtr);
        }

        internal void CheckValidity(ref int compCode, ref int reason)
        {
            int num2;
            uint method = 0x72;
            this.TrEntry(method, new object[] { (int) compCode, (int) reason });
            compCode = 0;
            reason = 0;
            if (compCode == 0)
            {
                num2 = this.mqCNO.Options & 0x301;
                if ((num2 & (num2 - 1)) != 0)
                {
                    compCode = 2;
                    reason = 0x7fe;
                }
            }
            if (compCode == 0)
            {
                num2 = this.mqCNO.Options & 0xe0;
                if ((num2 & (num2 - 1)) != 0)
                {
                    compCode = 2;
                    reason = 0x7fe;
                }
            }
            if (compCode == 0)
            {
                num2 = this.mqCNO.Options & 30;
                if ((num2 != 0) && ((this.mqCNO.Version < 3) || ((num2 & (num2 - 1)) != 0)))
                {
                    compCode = 2;
                    reason = 0x7fe;
                }
            }
            if (compCode == 0)
            {
                num2 = this.mqCNO.Options & 0x50000;
                if ((num2 & (num2 - 1)) != 0)
                {
                    compCode = 2;
                    reason = 0x7fe;
                }
            }
            if (((compCode == 0) && ((this.mqCNO.Options & 0x80000) != 0)) && ((this.mqCNO.Options & 0x100000) != 0))
            {
                compCode = 2;
                reason = 0x7fe;
            }
            if (((compCode == 0) && (((this.mqCNO.Options & 0x80000) != 0) || ((this.mqCNO.Options & 0x100000) != 0))) && this.mqCNO.ClientConnPtr.Equals(IntPtr.Zero))
            {
                compCode = 2;
                reason = 0x8e5;
            }
            base.TrExit(method);
        }

        public MQChannelDefinition ClientConn
        {
            get
            {
                return this.cd;
            }
            set
            {
                this.cd = value;
                if (this.cd == null)
                {
                    this.ClientConnPtr = IntPtr.Zero;
                }
                else
                {
                    IntPtr zero = IntPtr.Zero;
                    zero = Marshal.AllocCoTaskMem(Marshal.SizeOf(this.cd.StructMQCD));
                    Marshal.StructureToPtr(this.cd.StructMQCD, zero, false);
                    this.ClientConnPtr = zero;
                }
            }
        }

        public int ClientConnOffset
        {
            get
            {
                return this.mqCNO.ClientConnOffset;
            }
            set
            {
                this.Version = (this.Version < 2) ? 2 : this.Version;
                this.mqCNO.ClientConnOffset = value;
            }
        }

        public IntPtr ClientConnPtr
        {
            get
            {
                return this.mqCNO.ClientConnPtr;
            }
            set
            {
                this.Version = (this.Version < 2) ? 2 : this.Version;
                if (this.mqCNO.ClientConnPtr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(this.mqCNO.ClientConnPtr);
                    this.mqCNO.ClientConnPtr = IntPtr.Zero;
                }
                this.mqCNO.ClientConnPtr = value;
            }
        }

        public byte[] ConnectionId
        {
            get
            {
                return (byte[]) this.mqCNO.ConnectionId.Clone();
            }
            set
            {
                this.Version = (this.Version < 5) ? 5 : this.Version;
                value.CopyTo(this.mqCNO.ConnectionId, 0);
            }
        }

        public byte[] ConnTag
        {
            get
            {
                return (byte[]) this.mqCNO.ConnTag.Clone();
            }
            set
            {
                this.Version = (this.Version < 3) ? 3 : this.Version;
                value.CopyTo(this.mqCNO.ConnTag, 0);
            }
        }

        public int Options
        {
            get
            {
                return this.mqCNO.Options;
            }
            set
            {
                this.mqCNO.Options = value;
            }
        }

        public MQConnectionSecurityParameters SecurityParms
        {
            get
            {
                return this.securityParms;
            }
            set
            {
                this.Version = (this.Version < 5) ? 5 : this.Version;
                this.securityParms = value;
            }
        }

        public int SecurityParmsOffset
        {
            get
            {
                return this.mqCNO.SecurityParmsOffset;
            }
            set
            {
                this.Version = (this.Version < 5) ? 5 : this.Version;
                this.mqCNO.SecurityParmsOffset = value;
            }
        }

        public IntPtr SecurityParmsPtr
        {
            get
            {
                return this.mqCNO.SecurityParmsPtr;
            }
            set
            {
                this.Version = (this.Version < 5) ? 5 : this.Version;
                this.mqCNO.SecurityParmsPtr = value;
            }
        }

        public int SSLConfigOffset
        {
            get
            {
                return this.mqCNO.SSLConfigOffset;
            }
            set
            {
                this.Version = (this.Version < 4) ? 4 : this.Version;
                this.mqCNO.SSLConfigOffset = value;
            }
        }

        public MQSSLConfigOptions SslConfigOptions
        {
            get
            {
                return this.mqSCO;
            }
            set
            {
                this.mqSCO = value;
            }
        }

        public IntPtr SSLConfigPtr
        {
            get
            {
                return this.mqCNO.SSLConfigPtr;
            }
            set
            {
                this.Version = (this.Version < 4) ? 4 : this.Version;
                this.mqCNO.SSLConfigPtr = value;
            }
        }

        public MQBase.MQCNO StructMQCNO
        {
            get
            {
                return this.mqCNO;
            }
            set
            {
                this.mqCNO = value;
            }
        }

        public int Version
        {
            get
            {
                return this.mqCNO.Version;
            }
            set
            {
                this.mqCNO.Version = value;
            }
        }
    }
}

