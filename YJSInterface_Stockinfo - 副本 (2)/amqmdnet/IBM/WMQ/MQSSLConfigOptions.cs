namespace IBM.WMQ
{
    using System;
    using System.Collections;

    public class MQSSLConfigOptions : MQBase
    {
        private bool certRevocCheck;
        private MQBase.MQSCO mqSCO;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public MQSSLConfigOptions()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.mqSCO.StrucId = new byte[] { 0x53, 0x43, 0x4f, 0x20 };
            this.mqSCO.Version = 1;
            this.mqSCO.KeyRepository = new byte[0x100];
            this.mqSCO.CryptoHardware = new byte[0x100];
            this.mqSCO.AuthInfoRecCount = 0;
            this.mqSCO.AuthInfoRecOffset = 0;
            this.mqSCO.AuthInfoRecPtr = IntPtr.Zero;
            this.mqSCO.KeyResetCount = 0;
            this.mqSCO.FipsRequired = 0;
            this.mqSCO.EncryptionPolicySuiteB = new int[4];
            this.mqSCO.CertificateValPolicy = 0;
            this.mqSCO.CertificateLabel = new byte[0x40];
        }

        internal static bool Compare(byte[] item1, byte[] item2)
        {
            if (item1.Length != item2.Length)
            {
                return false;
            }
            for (int i = 0; i < item1.Length; i++)
            {
                if (item1[i] != item2[i])
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool Compare(int[] item1, int[] item2)
        {
            if (item1.Length != item2.Length)
            {
                return false;
            }
            for (int i = 0; i < item1.Length; i++)
            {
                if (item1[i] != item2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public bool Equals(MQSSLConfigOptions obj)
        {
            bool flag = true;
            if (obj == null)
            {
                return false;
            }
            return ((((((((((((flag && (this.StructMQSCO.AuthInfoRecCount == obj.StructMQSCO.AuthInfoRecCount)) && (this.StructMQSCO.AuthInfoRecOffset == obj.StructMQSCO.AuthInfoRecOffset)) && (this.StructMQSCO.AuthInfoRecPtr == obj.StructMQSCO.AuthInfoRecPtr)) && (this.StructMQSCO.CertificateValPolicy == obj.StructMQSCO.CertificateValPolicy)) && (this.StructMQSCO.FipsRequired == obj.StructMQSCO.FipsRequired)) && (this.StructMQSCO.KeyResetCount == obj.StructMQSCO.KeyResetCount)) && (this.StructMQSCO.Version == obj.StructMQSCO.Version)) && Compare(this.StructMQSCO.CertificateLabel, obj.StructMQSCO.CertificateLabel)) && Compare(this.StructMQSCO.CryptoHardware, obj.StructMQSCO.CryptoHardware)) && Compare(this.StructMQSCO.EncryptionPolicySuiteB, obj.StructMQSCO.EncryptionPolicySuiteB)) && Compare(this.StructMQSCO.KeyRepository, obj.StructMQSCO.KeyRepository)) && (this.CertRevocationCheck == obj.CertRevocationCheck));
        }

        public int AuthInfoRecCount
        {
            get
            {
                return this.mqSCO.AuthInfoRecCount;
            }
            set
            {
                this.mqSCO.AuthInfoRecCount = value;
            }
        }

        public int AuthInfoRecOffset
        {
            get
            {
                return this.mqSCO.AuthInfoRecOffset;
            }
            set
            {
                this.mqSCO.AuthInfoRecOffset = value;
            }
        }

        public IntPtr AuthInfoRecPtr
        {
            get
            {
                return this.mqSCO.AuthInfoRecPtr;
            }
            set
            {
                this.mqSCO.AuthInfoRecPtr = value;
            }
        }

        public byte[] CerfificateLabel
        {
            get
            {
                return this.mqSCO.CertificateLabel;
            }
            set
            {
                this.mqSCO.CertificateLabel = value;
            }
        }

        public int CertificateValPolicy
        {
            get
            {
                return this.mqSCO.CertificateValPolicy;
            }
            set
            {
                this.Version = (this.Version < 4) ? 4 : this.Version;
                this.mqSCO.CertificateValPolicy = value;
            }
        }

        internal bool CertRevocationCheck
        {
            get
            {
                return this.certRevocCheck;
            }
            set
            {
                this.certRevocCheck = value;
            }
        }

        public string CryptoHardware
        {
            get
            {
                return base.GetString(this.mqSCO.CryptoHardware);
            }
            set
            {
                byte[] cryptoHardware = this.mqSCO.CryptoHardware;
                base.GetBytes(value, ref cryptoHardware);
                this.mqSCO.CryptoHardware = cryptoHardware;
            }
        }

        public ArrayList EncryptionPolicySuiteB
        {
            get
            {
                ArrayList list = new ArrayList();
                for (int i = 0; i < this.mqSCO.EncryptionPolicySuiteB.Length; i++)
                {
                    list.Add(this.mqSCO.EncryptionPolicySuiteB[i]);
                }
                return list;
            }
            set
            {
                this.Version = (this.Version < 3) ? 3 : this.Version;
                for (int i = 0; i < 4; i++)
                {
                    if (i < value.Count)
                    {
                        this.mqSCO.EncryptionPolicySuiteB[i] = (int) value[i];
                        base.TrText("Added Encryption policy suite B: " + this.mqSCO.EncryptionPolicySuiteB[i].ToString());
                    }
                    else
                    {
                        this.mqSCO.EncryptionPolicySuiteB[i] = 0;
                        base.TrText("Added Encryption policy suite B: MQ_SUITE_B_NOT_AVAILABLE (Default)");
                    }
                }
            }
        }

        public int FipsRequired
        {
            get
            {
                return this.mqSCO.FipsRequired;
            }
            set
            {
                this.Version = (this.Version < 2) ? 2 : this.Version;
                this.mqSCO.FipsRequired = value;
            }
        }

        public string KeyRepository
        {
            get
            {
                return base.GetString(this.mqSCO.KeyRepository);
            }
            set
            {
                byte[] keyRepository = this.mqSCO.KeyRepository;
                base.GetBytes(value, ref keyRepository);
                this.mqSCO.KeyRepository = keyRepository;
            }
        }

        public int KeyResetCount
        {
            get
            {
                return this.mqSCO.KeyResetCount;
            }
            set
            {
                this.Version = (this.Version < 2) ? 2 : this.Version;
                this.mqSCO.KeyResetCount = value;
            }
        }

        public MQBase.MQSCO StructMQSCO
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

        public int Version
        {
            get
            {
                return this.mqSCO.Version;
            }
            set
            {
                this.mqSCO.Version = value;
            }
        }
    }
}

