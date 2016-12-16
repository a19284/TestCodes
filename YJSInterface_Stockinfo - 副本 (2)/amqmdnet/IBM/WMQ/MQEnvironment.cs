namespace IBM.WMQ
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Text;

    public sealed class MQEnvironment : MQBase
    {
        private static string certificateLablel = string.Empty;
        private static int certificateValPolicy;
        private static string channel = null;
        private static string connectionName;
        private const string DOTNET_MANAGED_PRODID = "MQNM";
        private const string DOTNET_UNMANAGED_PRODID = "MQNU";
        private static ArrayList encryptionPolicySuiteB;
        private static int fipsRequired;
        private static ArrayList hdrCompList;
        private static string hostname = null;
        private static int keyResetCount = 0;
        private static string localAddressSetting;
        private static ArrayList mqairArray;
        private static ArrayList msgCompList;
        private static string password;
        private static int port = 0x586;
        private static string productIdentifier;
        public static Hashtable properties = new Hashtable();
        private static string receiveExit;
        private static string receiveUserData;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private static string securityExit;
        private static string securityUserData;
        private static string sendExit;
        private static string sendUserData;
        private static int sharingConversations;
        private static bool sslCertRevocationCheck = false;
        private static string sslCipherSpec = null;
        private static string sslCryptoHardware;
        private static string sslKeyRepository = "*SYSTEM";
        private static string sslPeerName = null;
        private static string userId;

        static MQEnvironment()
        {
            sslCipherSpec = null;
            sslPeerName = null;
            sslKeyRepository = null;
            sslCryptoHardware = null;
            securityExit = null;
            sendExit = null;
            receiveExit = null;
            userId = null;
            password = null;
            mqairArray = null;
            keyResetCount = 0;
            fipsRequired = 0;
            encryptionPolicySuiteB = null;
            certificateValPolicy = 0;
            hdrCompList = null;
            msgCompList = null;
            sharingConversations = 10;
            localAddressSetting = null;
            productIdentifier = null;
            connectionName = null;
        }

        private MQEnvironment()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        internal static ArrayList GetArrayListProperty(string key, Hashtable properties, ArrayList defaultValue)
        {
            object obj2 = null;
            if (properties != null)
            {
                obj2 = properties[key];
            }
            if (obj2 == null)
            {
                obj2 = MQEnvironment.properties[key];
            }
            if (obj2 == null)
            {
                obj2 = defaultValue;
            }
            return (ArrayList) obj2;
        }

        internal static string GetBindingTypeFromEnv()
        {
            string str = null;
            string propertyValue = CommonServices.GetPropertyValue("NMQ_MQ_LIB");
            if (propertyValue != null)
            {
                switch (propertyValue.ToLower())
                {
                    case "mqm.dll":
                        str = "SERVER";
                        goto Label_00D5;

                    case "mqic32.dll":
                    case "mqic.dll":
                        str = "CLIENT";
                        goto Label_00D5;

                    case "mqic32xa.dll":
                    case "mqicxa.dll":
                        str = "XACLIENT";
                        goto Label_00D5;

                    case "managed":
                        str = "MANAGEDCLIENT";
                        goto Label_00D5;
                }
                str = "CLIENT";
            }
        Label_00D5:
            if (str != null)
            {
                CommonServices.TraceText("BindingType from NMQ_MQ_LIB");
            }
            return str;
        }

        internal static string GetBindingTypeFromProperties(Hashtable properties)
        {
            string str = null;
            string str2 = GetStringProperty("transport", properties, null);
            switch (str2)
            {
                case "SERVER":
                case "MQSeries Bindings":
                    str = "SERVER";
                    break;

                case "CLIENT":
                case "MQSeries Client":
                    str = "CLIENT";
                    break;

                case "XACLIENT":
                case "MQSeries XA Client":
                    str = "XACLIENT";
                    break;

                case "MANAGEDCLIENT":
                case "MQSeries Managed Client":
                    str = "MANAGEDCLIENT";
                    break;

                case null:
                    return str;

                default:
                    str = null;
                    break;
            }
            if (str != null)
            {
                CommonServices.TraceText("BindingType from property (" + str2 + ")");
            }
            return str;
        }

        internal static string GetBindingTypeFromRegistry()
        {
            string str = null;
            using (RegistryKey key = CommonServices.OpenRegistryKey(CommonServices.GetInstallationKey() + @"\Components"))
            {
                if (key != null)
                {
                    string str2 = (string) key.GetValue("Server");
                    if ((str2 != null) && ("INSTALLED" == str2.ToUpper()))
                    {
                        str = "SERVER";
                    }
                    else
                    {
                        str = "CLIENT";
                    }
                }
                else
                {
                    str = "MANAGEDCLIENT";
                }
            }
            if (str != null)
            {
                CommonServices.TraceText("BindingType from installed components");
            }
            return str;
        }

        internal static int GetIntegerProperty(string key, Hashtable properties, int defaultValue)
        {
            object obj2 = null;
            if (properties != null)
            {
                obj2 = properties[key];
            }
            if (obj2 == null)
            {
                obj2 = MQEnvironment.properties[key];
            }
            if (obj2 == null)
            {
                obj2 = defaultValue;
            }
            return Convert.ToInt32(obj2);
        }

        internal static object GetObjectProperty(string key, Hashtable properties, object defaultValue)
        {
            object obj2 = null;
            if (properties != null)
            {
                obj2 = properties[key];
            }
            if (obj2 == null)
            {
                obj2 = MQEnvironment.properties[key];
            }
            if (obj2 == null)
            {
                obj2 = defaultValue;
            }
            return obj2;
        }

        internal static string GetStringProperty(string key, Hashtable properties, string defaultValue)
        {
            object obj2 = null;
            if (properties != null)
            {
                obj2 = properties[key];
            }
            if (obj2 == null)
            {
                obj2 = MQEnvironment.properties[key];
            }
            if (obj2 == null)
            {
                obj2 = defaultValue;
            }
            if (obj2 != null)
            {
                return Convert.ToString(obj2);
            }
            return null;
        }

        internal static void SetProductId(string connectionTypeName)
        {
            StringBuilder builder = new StringBuilder();
            string str2 = connectionTypeName;
            if (str2 == null)
            {
                return;
            }
            if (!(str2 == "CLIENT") && !(str2 == "XACLIENT"))
            {
                if (!(str2 == "MANAGEDCLIENT"))
                {
                    return;
                }
            }
            else
            {
                builder.Append("MQNU");
                goto Label_0054;
            }
            builder.Append("MQNM");
        Label_0054:
            try
            {
                int mQVersion = MQVersion;
                int mQRelease = MQRelease;
                int num3 = 0;
                int num4 = 0;
                string[] strArray = CommonServices.GetVRMF().Split(new char[] { '.' });
                if (strArray.Length >= 3)
                {
                    num3 = int.Parse(strArray[2]);
                    if (strArray.Length >= 4)
                    {
                        string s = strArray[3];
                        if (strArray.Length > 4)
                        {
                            s = s + strArray[4];
                        }
                        num4 = int.Parse(s);
                    }
                }
                if (mQVersion <= 9)
                {
                    builder.Append(0);
                    builder.Append(mQVersion);
                }
                else
                {
                    builder.Append(mQVersion);
                }
                if (mQRelease <= 9)
                {
                    builder.Append(0);
                    builder.Append(mQRelease);
                }
                else
                {
                    builder.Append(mQRelease);
                }
                if (num3 <= 9)
                {
                    builder.Append(0);
                    builder.Append(num3);
                }
                else
                {
                    builder.Append(num3);
                }
                if (num4 <= 9)
                {
                    builder.Append(0);
                    builder.Append(num4);
                }
                else
                {
                    builder.Append(num4);
                }
            }
            catch (Exception)
            {
            }
            productIdentifier = builder.ToString().PadRight(12, '0');
        }

        public static string AssemblyName
        {
            get
            {
                return Assembly.GetAssembly(new MQEnvironment().GetType()).FullName;
            }
        }

        public static string CertificateLabel
        {
            get
            {
                return certificateLablel;
            }
            set
            {
                certificateLablel = value;
            }
        }

        public static int CertificateValPolicy
        {
            get
            {
                return certificateValPolicy;
            }
            set
            {
                certificateValPolicy = value;
            }
        }

        public static string Channel
        {
            get
            {
                return channel;
            }
            set
            {
                channel = value;
            }
        }

        public static string ConnectionName
        {
            get
            {
                return connectionName;
            }
            set
            {
                connectionName = value;
            }
        }

        public static ArrayList EncryptionPolicySuiteB
        {
            get
            {
                return encryptionPolicySuiteB;
            }
            set
            {
                if (value == null)
                {
                    encryptionPolicySuiteB = null;
                }
                else
                {
                    encryptionPolicySuiteB = new ArrayList(value);
                }
            }
        }

        public static int FipsRequired
        {
            get
            {
                return fipsRequired;
            }
            set
            {
                fipsRequired = value;
            }
        }

        public static ArrayList HdrCompList
        {
            get
            {
                return hdrCompList;
            }
            set
            {
                if (value == null)
                {
                    hdrCompList = null;
                }
                else
                {
                    hdrCompList = new ArrayList(value);
                }
            }
        }

        public static string Hostname
        {
            get
            {
                return hostname;
            }
            set
            {
                hostname = value;
            }
        }

        public static int KeyResetCount
        {
            get
            {
                return keyResetCount;
            }
            set
            {
                keyResetCount = value;
            }
        }

        public static string LocalAddressSetting
        {
            get
            {
                return localAddressSetting;
            }
            set
            {
                localAddressSetting = value;
            }
        }

        public static ArrayList MQAIRArray
        {
            get
            {
                return mqairArray;
            }
            set
            {
                if (value == null)
                {
                    mqairArray = null;
                }
                else
                {
                    mqairArray = new ArrayList(value);
                }
            }
        }

        public static int MQCSDApplied
        {
            get
            {
                return CommonServices.MQCSDApplied;
            }
        }

        public static int MQRelease
        {
            get
            {
                return CommonServices.MQRelease;
            }
        }

        public static int MQVersion
        {
            get
            {
                return CommonServices.MQVersion;
            }
        }

        public static ArrayList MsgCompList
        {
            get
            {
                return msgCompList;
            }
            set
            {
                if (value == null)
                {
                    msgCompList = null;
                }
                else
                {
                    msgCompList = new ArrayList(value);
                }
            }
        }

        public static string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
            }
        }

        public static int Port
        {
            get
            {
                return port;
            }
            set
            {
                port = value;
            }
        }

        public static string ProductIdentifier
        {
            get
            {
                return productIdentifier;
            }
            set
            {
                productIdentifier = value;
            }
        }

        public static string ReceiveExit
        {
            get
            {
                return receiveExit;
            }
            set
            {
                receiveExit = value;
            }
        }

        public static string ReceiveUserData
        {
            get
            {
                return receiveUserData;
            }
            set
            {
                receiveUserData = value;
            }
        }

        public static string SecurityExit
        {
            get
            {
                return securityExit;
            }
            set
            {
                securityExit = value;
            }
        }

        public static string SecurityUserData
        {
            get
            {
                return securityUserData;
            }
            set
            {
                securityUserData = value;
            }
        }

        public static string SendExit
        {
            get
            {
                return sendExit;
            }
            set
            {
                sendExit = value;
            }
        }

        public static string SendUserData
        {
            get
            {
                return sendUserData;
            }
            set
            {
                sendUserData = value;
            }
        }

        public static int SharingConversations
        {
            get
            {
                return sharingConversations;
            }
            set
            {
                sharingConversations = value;
            }
        }

        public static bool SSLCertRevocationCheck
        {
            get
            {
                return sslCertRevocationCheck;
            }
            set
            {
                sslCertRevocationCheck = value;
            }
        }

        public static string SSLCipherSpec
        {
            get
            {
                return sslCipherSpec;
            }
            set
            {
                sslCipherSpec = value;
            }
        }

        public static string SSLCryptoHardware
        {
            get
            {
                return sslCryptoHardware;
            }
            set
            {
                sslCryptoHardware = value;
            }
        }

        public static string SSLKeyRepository
        {
            get
            {
                return sslKeyRepository;
            }
            set
            {
                sslKeyRepository = value;
            }
        }

        public static string SSLPeerName
        {
            get
            {
                return sslPeerName;
            }
            set
            {
                sslPeerName = value;
            }
        }

        public static string UserId
        {
            get
            {
                return userId;
            }
            set
            {
                userId = value;
            }
        }
    }
}

