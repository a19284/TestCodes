namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.IO;
    using System.Net;

    public class MQClientCfg : MQIniFile
    {
        public static ChoiceCfgProperty AMQ_RANDOM_NUMBER_TYPE = new ChoiceCfgProperty(null, "AMQ_RANDOM_NUMBER_TYPE", new string[] { "Standard", "Fast" });
        public static StringCfgProperty CHANNEL_CHLDEFDIRECTORY = new StringCfgProperty("CHANNELS", "ChannelDefinitionDirectory", null, 1);
        public static StringCfgProperty CHANNEL_CHLDEFFILE = new StringCfgProperty("CHANNELS", "ChannelDefinitionFile", null, 1);
        public static StringCfgProperty CHANNEL_RECON = new StringCfgProperty("CHANNELS", "DEFRECON", "NO", 2);
        public static StringCfgProperty CHANNEL_RECONDELAY = new StringCfgProperty("CHANNELS", "ReconDelay", null, 1);
        public static IntCfgProperty CHANNEL_RECONTIMEOUT = new IntCfgProperty("CHANNELS", "MQReconnectTimeout", 0x708, 1);
        public static StringCfgProperty CLIENTEXITPATH_EXITSDEFAULTPATH = new StringCfgProperty("ClientExitPath", "ExitsDefaultPath", null, 1);
        public static StringCfgProperty CLIENTEXITPATH_EXITSDEFAULTPATH64 = new StringCfgProperty("ClientExitPath", "ExitsDefaultPath64", null, 1);
        private static Hashtable configParameters = null;
        private const string DEFAULT_FILENAME = "mqclient.ini";
        public static CfgProperty ENV_MQ_COMM = new StringCfgProperty(null, "MQ_COMM", null, 1);
        public static StringCfgProperty ENV_MQ_FILE_PATH = new StringCfgProperty(null, "MQ_FILE_PATH", null, 1);
        public static StringCfgProperty ENV_MQ_LCLADDR = new StringCfgProperty(null, "MQ_LCLADDR", null, 1);
        public static StringCfgProperty ENV_MQ_READA = new StringCfgProperty(null, "MQ_READA", null, 1);
        public static IntCfgProperty ENV_MQCCSID = new IntCfgProperty(null, "MQCCSID", -1, 0);
        public static StringCfgProperty ENV_MQCHLLIB = new StringCfgProperty(null, "MQCHLLIB", null, 1);
        public static StringCfgProperty ENV_MQCHLTAB = new StringCfgProperty(null, "MQCHLTAB", null, 1);
        public static StringCfgProperty ENV_MQCLNTCF = new StringCfgProperty(null, "MQCLNTCF", null, 1);
        public static StringCfgProperty ENV_MQIPADDRV = new StringCfgProperty(null, "MQIPADDRV", null, 1);
        public static BoolCfgProperty ENV_MQPUT1DEFSYNC = new BoolCfgProperty("CHANNELS", "Put1DefaultAlwaysSync", false, 1);
        public static IntCfgProperty ENV_MQRCVBLKMIN = new IntCfgProperty(null, "MQRCVBLKMIN", -1, 1);
        public static StringCfgProperty ENV_MQRCVBLKTO = new StringCfgProperty(null, "MQRCVBLKTO", null, 1);
        public static StringCfgProperty ENV_MQSERVER = new StringCfgProperty(null, "MQSERVER", null, 1);
        private static Hashtable envToIniNamesMapper = null;
        private WebResponse iniConnection;
        private Stream iniStream;
        public static IntCfgProperty MESSAGEBUFFER_MAXIMUMSIZE = new IntCfgProperty("MessageBuffer", "MaximumSize", 100, 0);
        public static IntCfgProperty MESSAGEBUFFER_PURGETIME = new IntCfgProperty("MessageBuffer", "PurgeTime", 0, 0);
        public static IntCfgProperty MESSAGEBUFFER_UPDATEPERCENTAGE = new IntCfgProperty("MessageBuffer", "UpdatePercentage", 20, 0, 100);
        public static ChoiceCfgProperty PASSWORD_PROTECTION = new ChoiceCfgProperty("CHANNELS", "PasswordProtection", new string[] { "Compatible", "Always", "Optional" });
        private const string sccsid = "%Z% %W% %I% %E% %U%";
        public static StringCfgProperty SSL_SSLKeyRepository = new StringCfgProperty("SSL", "SSLKeyRepository", null, 1);
        public static IntCfgProperty SSL_SSLKeyResetCount = new IntCfgProperty("SSL", "SSLKeyResetCount", 0x8000);
        private static string[] stanzas = new string[] { "CHANNELS", "TCP", "ClientExitPath", "MessageBuffer" };
        public static IntCfgProperty TCP_CLNTRCVBUFFSIZE = new IntCfgProperty("TCP", "ClntRcvBuffSize", 0x8000, 0x400);
        public static IntCfgProperty TCP_CLNTSNDBUFFSIZE = new IntCfgProperty("TCP", "ClntSndBuffSize", 0x8000, 0x400);
        public static IntCfgProperty TCP_ENDPORT = new IntCfgProperty("TCP", "EndPort", 0, 0);
        public static BoolCfgProperty TCP_KEEPALIVE = new BoolCfgProperty("TCP", "KeepAlive", false, 1);
        public static BoolCfgProperty TCP_LINGER = new BoolCfgProperty("TCP", "Linger", false, 1);
        public static IntCfgProperty TCP_STRPORT = new IntCfgProperty("TCP", "StrPort", 0, 0);
        public static StringCfgProperty XMS_ENV_PROVIDER_VERSION = new StringCfgProperty(null, "XMSC_WMQ_OVERRIDEPROVIDERVERSION", "unspecified", 1);

        static MQClientCfg()
        {
            if (envToIniNamesMapper == null)
            {
                envToIniNamesMapper = new Hashtable(14);
                envToIniNamesMapper.Add("MQCCSID", "ccsid");
                envToIniNamesMapper.Add("MQSERVER", "serverconnectionparms");
                envToIniNamesMapper.Add("MQCHLLIB", "channeldefinitiondirectory");
                envToIniNamesMapper.Add("MQCHLTAB", "channeldefinitionfile");
                envToIniNamesMapper.Add("MQIPADDRV", "ipaddressversion");
                envToIniNamesMapper.Add("MQSSLFIPS", "sslfipsrequired");
                envToIniNamesMapper.Add("MQSSLRESET", "sslkeyresetcount");
                envToIniNamesMapper.Add("MQSSLCRYP", "sslcryptohardware");
                envToIniNamesMapper.Add("MQSSLKEYR", "sslkeyrepository");
                envToIniNamesMapper.Add("MQNAME", "localname");
                envToIniNamesMapper.Add("MQ_LCLADDR", "localaddress");
                envToIniNamesMapper.Add("MQRCVBLKTO", "blockingreceivetimeout");
                envToIniNamesMapper.Add("MQRCVBLKMIN", "minblockingreceivetimeout");
                envToIniNamesMapper.Add("MQ_COMM", "linger");
                envToIniNamesMapper.Add("PASSWORD_PROTECTION", "passwordprotection");
            }
            if (configParameters == null)
            {
                configParameters = new Hashtable(20);
                configParameters.Add("ccsid", "channels");
                configParameters.Add("serverconnectionparms", "channels");
                configParameters.Add("channeldefinitiondirectory", "channels");
                configParameters.Add("channeldefinitionfile", "channels");
                configParameters.Add("ipaddressversion", "tcp");
                configParameters.Add("library1", "tcp");
                configParameters.Add("keepalive", "tcp");
                configParameters.Add("clntsndbuffsize", "tcp");
                configParameters.Add("clntrcvbuffsize", "tcp");
                configParameters.Add("connect_timeout", "tcp");
                configParameters.Add("sslfipsrequired", "ssl");
                configParameters.Add("sslkeyresetcount", "ssl");
                configParameters.Add("sslcryptohardware", "ssl");
                configParameters.Add("sslkeyrepository", "ssl");
                configParameters.Add("exitsdefaultpath", "clientexitpath");
                configParameters.Add("exitsdefaultpath64", "clientexitpath");
                configParameters.Add("localname", "netbios");
                configParameters.Add("maximumsize", "messagebuffer");
                configParameters.Add("updatepercentage", "messagebuffer");
                configParameters.Add("purgetime", "messagebuffer");
                configParameters.Add("put1defaultalwayssync", "channels");
                configParameters.Add("LocalAddress", "tcp");
                configParameters.Add("blockingreceivetimeout", "tcp");
                configParameters.Add("minblockingreceivetimeout", "tcp");
                configParameters.Add("linger", "tcp");
                configParameters.Add("dnslookuponerror", "tcp");
                configParameters.Add("strport", "tcp");
                configParameters.Add("endport", "tcp");
                configParameters.Add("blocking", "tcp");
                configParameters.Add("maxtransize", "tcp");
                configParameters.Add("commstracebuffersize", "tcp");
                configParameters.Add("defrecon", "channels");
                configParameters.Add("mqreconnecttimeout", "channels");
                configParameters.Add("passwordprotection", "channels");
                configParameters.Add("amq_random_number_type", null);
            }
        }

        public MQClientCfg(NmqiEnvironment nmqiEnv) : base(nmqiEnv)
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%");
        }

        public void FindAndParse()
        {
            uint method = 50;
            this.TrEntry(method);
            try
            {
                this.FindClientIni();
                base.Parse(this.iniStream);
                if (this.iniStream != null)
                {
                    this.iniStream.Close();
                }
                if (this.iniConnection != null)
                {
                    this.iniConnection.Close();
                }
                this.OverwriteFromEnvironmentVariables();
                if (ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).HasFile)
                {
                    this.OverwriteFromDotnetConfigurationFile();
                }
            }
            catch (Exception exception)
            {
                base.TrException(method, exception);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void FindClientIni()
        {
            uint method = 0x3b;
            this.TrEntry(method);
            try
            {
                string environmentVariable = Environment.GetEnvironmentVariable(ENV_MQCLNTCF.name);
                if ((environmentVariable != null) && (environmentVariable.Length > 0))
                {
                    try
                    {
                        base.TrText("MQCLNTCF = " + environmentVariable);
                        try
                        {
                            this.iniConnection = WebRequest.Create(environmentVariable).GetResponse();
                            this.iniStream = this.iniConnection.GetResponseStream();
                        }
                        catch (UriFormatException exception)
                        {
                            base.TrException(method, exception);
                            this.iniStream = System.IO.File.Open(environmentVariable, FileMode.Open, FileAccess.Read);
                        }
                    }
                    catch (Exception exception2)
                    {
                        base.TrException(method, exception2);
                    }
                }
                if (this.iniStream == null)
                {
                    try
                    {
                        this.iniStream = System.IO.File.Open("mqclient.ini", FileMode.Open, FileAccess.Read);
                        if (this.iniStream != null)
                        {
                            base.TrText("mqclient.ini in working directory");
                        }
                    }
                    catch (Exception exception3)
                    {
                        base.TrText("mqclient.ini not in working directory");
                        base.TrException(method, exception3);
                    }
                }
                if (this.iniStream == null)
                {
                    string path = null;
                    try
                    {
                        path = CommonServices.GetMQIniAttribute("WorkPath");
                    }
                    catch (Exception exception4)
                    {
                        base.TrException(method, exception4);
                    }
                    if (path != null)
                    {
                        try
                        {
                            path = path + @"\mqclient.ini";
                            this.iniStream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read);
                            if (this.iniStream != null)
                            {
                                base.TrText("mqclient.ini in WMQ data directory: " + path);
                            }
                        }
                        catch (Exception exception5)
                        {
                            base.TrException(method, exception5);
                            base.TrText("mqclient.ini not in WMQ data directory: " + path);
                        }
                    }
                }
                if (this.iniStream == null)
                {
                    string str3 = Environment.GetEnvironmentVariable("HOMEDRIVE");
                    string str4 = Environment.GetEnvironmentVariable("HOMEPATH");
                    string str5 = str3 + str4 + @"\mqclient.ini";
                    if ((str4 != null) && (str3 != null))
                    {
                        try
                        {
                            this.iniStream = System.IO.File.Open(str5, FileMode.Open, FileAccess.Read);
                            if (this.iniStream != null)
                            {
                                base.TrText("mqclient.ini in home directory: " + str5);
                            }
                        }
                        catch (Exception exception6)
                        {
                            base.TrException(method, exception6);
                            base.TrText("mqclient.ini not in home directory: " + str5);
                        }
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public bool GetBoolValue(BoolCfgProperty boolProperty)
        {
            uint method = 0x37;
            this.TrEntry(method);
            string stringValue = null;
            string propertyNameAsWritten = this.GetPropertyNameAsWritten(boolProperty.name);
            if ((base.env != null) && (base.env.Properties != null))
            {
                stringValue = base.env.Properties.GetProperty(propertyNameAsWritten);
            }
            if ((stringValue == null) || (stringValue.Length == 0))
            {
                stringValue = (string) base.propertyHash[propertyNameAsWritten];
            }
            base.TrExit(method);
            return boolProperty.ParseValue(stringValue);
        }

        public string GetChoiceValue(ChoiceCfgProperty choiceProperty)
        {
            string propertyNameAsWritten = this.GetPropertyNameAsWritten(choiceProperty.name);
            string property = null;
            if ((base.env != null) && (base.env.Properties != null))
            {
                property = base.env.Properties.GetProperty(propertyNameAsWritten);
            }
            if ((property == null) || (property.Length == 0))
            {
                property = (string) base.propertyHash[propertyNameAsWritten];
            }
            return choiceProperty.ParseValue(property);
        }

        public int GetIntValue(IntCfgProperty intProperty)
        {
            uint method = 0x36;
            this.TrEntry(method);
            string stringValue = null;
            string propertyNameAsWritten = this.GetPropertyNameAsWritten(intProperty.name);
            if ((base.env != null) && (base.env.Properties != null))
            {
                stringValue = base.env.Properties.GetProperty(propertyNameAsWritten);
            }
            if ((stringValue == null) || (stringValue.Length == 0))
            {
                stringValue = (string) base.propertyHash[propertyNameAsWritten];
            }
            base.TrExit(method);
            return intProperty.ParseValue(stringValue);
        }

        public string GetPropertyNameAsWritten(string name)
        {
            if (name == null)
            {
                return name;
            }
            string str = name;
            if (envToIniNamesMapper.ContainsKey(name))
            {
                string str2 = Convert.ToString(envToIniNamesMapper[name]);
                string str3 = Convert.ToString(configParameters[str2.ToLower()]);
                if (((str3 != null) && (str3 != string.Empty)) && (str3.Length > 0))
                {
                    str = "com.ibm.mq.cfg." + str3.ToLower() + "." + Convert.ToString(envToIniNamesMapper[name]).ToLower();
                }
            }
            return str;
        }

        public string GetStringValue(StringCfgProperty stringProperty)
        {
            uint method = 0x35;
            this.TrEntry(method);
            string stringValue = null;
            string propertyNameAsWritten = this.GetPropertyNameAsWritten(stringProperty.name);
            if ((base.env != null) && (base.env.Properties != null))
            {
                stringValue = base.env.Properties.GetProperty(propertyNameAsWritten);
            }
            if ((stringValue == null) || (stringValue.Length == 0))
            {
                stringValue = (string) base.propertyHash[propertyNameAsWritten];
            }
            base.TrExit(method);
            return stringProperty.ParseValue(stringValue);
        }

        private void OverwriteFromDotnetConfigurationFile()
        {
            uint method = 0x34;
            this.TrEntry(method);
            string[] allKeys = null;
            string str2 = null;
            try
            {
                string str;
                NameValueCollection appSettings = ConfigurationManager.AppSettings;
                if (appSettings.HasKeys())
                {
                    foreach (string str3 in appSettings.AllKeys)
                    {
                        if (configParameters.Contains(str3.ToLower()))
                        {
                            string[] values = appSettings.GetValues(str3);
                            if (((values != null) && (values[0] != null)) && (values[0].Length > 0))
                            {
                                str = Convert.ToString(configParameters[str3.ToLower()]).ToLower();
                                if (str.Length > 0)
                                {
                                    str2 = "com.ibm.mq.cfg." + str + "." + str3.ToLower();
                                }
                                else
                                {
                                    str2 = str3.ToUpper();
                                }
                                base.propertyHash[str2] = values[0];
                            }
                        }
                    }
                }
                foreach (string str4 in stanzas)
                {
                    appSettings = (NameValueCollection) ConfigurationManager.GetSection(str4);
                    if (appSettings != null)
                    {
                        allKeys = appSettings.AllKeys;
                        if ((allKeys != null) && (allKeys.Length > 0))
                        {
                            foreach (string str5 in allKeys)
                            {
                                if (configParameters.Contains(str5.ToLower()))
                                {
                                    string[] strArray3 = appSettings.GetValues(str5);
                                    if (((strArray3 != null) && (strArray3[0] != null)) && (strArray3[0].Length > 0))
                                    {
                                        str = Convert.ToString(configParameters[str5.ToLower()]).ToLower();
                                        if (str.Length > 0)
                                        {
                                            str2 = "com.ibm.mq.cfg." + str + "." + str5.ToLower();
                                        }
                                        else
                                        {
                                            str2 = str5.ToUpper();
                                        }
                                        base.propertyHash[str2] = strArray3[0];
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                base.TrException(method, exception);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void OverwriteFromEnvironmentVariables()
        {
            uint method = 0x33;
            this.TrEntry(method);
            try
            {
                string environmentVariable = Environment.GetEnvironmentVariable(ENV_MQ_LCLADDR.name);
                if ((environmentVariable != null) && (environmentVariable.Length > 0))
                {
                    this.StorePropertyValues(ENV_MQ_LCLADDR.name, environmentVariable);
                }
                string str2 = Environment.GetEnvironmentVariable(ENV_MQRCVBLKTO.name);
                if ((str2 != null) && (str2.Length > 0))
                {
                    this.StorePropertyValues(ENV_MQRCVBLKTO.name, str2);
                }
                string str3 = Environment.GetEnvironmentVariable(ENV_MQRCVBLKMIN.name);
                if ((str3 != null) && (str3.Length > 0))
                {
                    this.StorePropertyValues(ENV_MQRCVBLKMIN.name, str3);
                }
                string str4 = Environment.GetEnvironmentVariable(ENV_MQ_COMM.name);
                if (str4 != null)
                {
                    str4 = str4.ToLower();
                    if (str4.IndexOf("nokeepalive") >= 0)
                    {
                        this.StorePropertyValues(TCP_KEEPALIVE.name, "N");
                    }
                    else
                    {
                        this.StorePropertyValues(TCP_KEEPALIVE.name, "Y");
                    }
                    if (str4.IndexOf("linger") >= 0)
                    {
                        this.StorePropertyValues(TCP_LINGER.name, "Y");
                    }
                }
                string str5 = Environment.GetEnvironmentVariable(ENV_MQIPADDRV.name);
                if ((str5 != null) && (str5.Length > 0))
                {
                    this.StorePropertyValues(ENV_MQIPADDRV.name, str5);
                }
                string str6 = Environment.GetEnvironmentVariable(ENV_MQCCSID.name);
                if ((str6 != null) && (str6.Length > 0))
                {
                    this.StorePropertyValues(ENV_MQCCSID.name, str6);
                }
                string str7 = Environment.GetEnvironmentVariable(ENV_MQ_READA.name);
                if ((str7 != null) && (str7.Length > 0))
                {
                    char[] separator = new char[] { ';' };
                    string[] strArray = str7.Split(separator, 3);
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        string str8 = strArray[i];
                        switch (i)
                        {
                            case 0:
                                this.StorePropertyValues(MESSAGEBUFFER_MAXIMUMSIZE.name, str8);
                                break;

                            case 1:
                                this.StorePropertyValues(MESSAGEBUFFER_UPDATEPERCENTAGE.name, str8);
                                break;

                            case 2:
                                this.StorePropertyValues(MESSAGEBUFFER_PURGETIME.name, str8);
                                break;
                        }
                    }
                }
                string str9 = Environment.GetEnvironmentVariable(ENV_MQSERVER.name);
                if ((str9 != null) && (str9.Length > 0))
                {
                    this.StorePropertyValues(ENV_MQSERVER.name, str9);
                }
                string str10 = Environment.GetEnvironmentVariable(ENV_MQCHLLIB.name);
                if ((str10 != null) && (str10.Length > 0))
                {
                    this.StorePropertyValues(ENV_MQCHLLIB.name, str10);
                }
                string str11 = Environment.GetEnvironmentVariable(ENV_MQCHLTAB.name);
                if ((str11 != null) && (str11.Length > 0))
                {
                    this.StorePropertyValues(ENV_MQCHLTAB.name, str11);
                }
                if (Environment.GetEnvironmentVariable("MQPUT1DEFSYNC") != null)
                {
                    this.StorePropertyValues(ENV_MQPUT1DEFSYNC.name, "Y");
                }
                string str13 = Environment.GetEnvironmentVariable(XMS_ENV_PROVIDER_VERSION.name);
                if ((str13 != null) && (str13.Length > 0))
                {
                    this.StorePropertyValues(XMS_ENV_PROVIDER_VERSION.name, str13);
                }
                string str14 = Environment.GetEnvironmentVariable(AMQ_RANDOM_NUMBER_TYPE.name);
                if ((str14 != null) && (str14.Length > 0))
                {
                    this.StorePropertyValues(AMQ_RANDOM_NUMBER_TYPE.name, str14);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void StorePropertyValues(string property, string value)
        {
            string str = property;
            if (envToIniNamesMapper.ContainsKey(property))
            {
                string str2 = Convert.ToString(envToIniNamesMapper[property]);
                string str3 = Convert.ToString(configParameters[str2]);
                str = "com.ibm.mq.cfg." + str3 + "." + str2;
            }
            base.propertyHash[str] = value;
        }

        public class BoolCfgProperty : MQClientCfg.CfgProperty
        {
            internal const int ANY = -1;
            private bool defaultValue;
            internal const int ONE_ZERO = 3;
            internal const int TRUE_FALSE = 2;
            private int type;
            internal const int Y_N = 1;

            public BoolCfgProperty(string stanza, string name, bool defaultValue, int type) : base(stanza, name)
            {
                this.defaultValue = defaultValue;
                this.type = type;
            }

            public bool ParseValue(string stringValue)
            {
                bool defaultValue = this.defaultValue;
                if (stringValue != null)
                {
                    if (((this.type == -1) || (this.type == 1)) && stringValue.StartsWith("Y"))
                    {
                        return true;
                    }
                    if (((this.type == -1) || (this.type == 1)) && stringValue.StartsWith("N"))
                    {
                        return false;
                    }
                    if (((this.type == -1) || (this.type == 2)) && "TRUE".Equals(stringValue.ToUpper()))
                    {
                        return true;
                    }
                    if (((this.type == -1) || (this.type == 2)) && "FALSE".Equals(stringValue.ToUpper()))
                    {
                        return false;
                    }
                    if (((this.type == -1) || (this.type == 2)) && "1".Equals(stringValue.ToUpper()))
                    {
                        return true;
                    }
                    if ((this.type != -1) && (this.type != 2))
                    {
                        return defaultValue;
                    }
                    if ("0".Equals(stringValue.ToUpper()))
                    {
                        defaultValue = false;
                    }
                }
                return defaultValue;
            }
        }

        public abstract class CfgProperty
        {
            internal string name;

            internal CfgProperty(string stanza, string name)
            {
                if (stanza == null)
                {
                    this.name = name;
                }
                else
                {
                    this.name = "com.ibm.mq.cfg." + stanza.ToLower() + "." + name.ToLower();
                }
            }
        }

        public class ChoiceCfgProperty : MQClientCfg.CfgProperty
        {
            internal string[] validChoices;

            internal ChoiceCfgProperty(string stanza, string name, string[] validChoices) : base(stanza, name)
            {
                this.validChoices = new string[validChoices.Length];
                for (int i = 0; i < validChoices.Length; i++)
                {
                    this.validChoices[i] = validChoices[i].ToUpper();
                }
            }

            public string ParseValue(string value)
            {
                if (value != null)
                {
                    string str = value.ToUpper();
                    foreach (string str2 in this.validChoices)
                    {
                        if (str2.Equals(str))
                        {
                            return str2;
                        }
                    }
                }
                return this.validChoices[0];
            }
        }

        public class IntCfgProperty : MQClientCfg.CfgProperty
        {
            private int defaultValue;
            private bool maxSet;
            private int maxValue;
            private bool minSet;
            private int minValue;

            public IntCfgProperty(string stanza, string name, int defaultValue) : base(stanza, name)
            {
                this.defaultValue = defaultValue;
                this.minSet = false;
                this.maxSet = false;
            }

            public IntCfgProperty(string stanza, string name, int defaultValue, int minValue) : base(stanza, name)
            {
                this.defaultValue = defaultValue;
                this.minValue = minValue;
                this.minSet = true;
                this.maxSet = false;
            }

            public IntCfgProperty(string stanza, string name, int defaultValue, int minValue, int maxValue) : base(stanza, name)
            {
                this.defaultValue = defaultValue;
                this.minValue = minValue;
                this.minSet = true;
                this.maxValue = maxValue;
                this.maxSet = true;
            }

            public int ParseValue(string stringValue)
            {
                bool flag = stringValue != null;
                int num = 0;
                if (flag)
                {
                    try
                    {
                        num = int.Parse(stringValue);
                    }
                    catch (Exception)
                    {
                        flag = false;
                    }
                    flag = (flag && (!this.minSet || (num >= this.minValue))) && (!this.maxSet || (num <= this.maxValue));
                }
                if (!flag)
                {
                    return this.defaultValue;
                }
                return num;
            }
        }

        public class StringCfgProperty : MQClientCfg.CfgProperty
        {
            private string defaultValue;
            private int maxLength;
            private int minLength;

            public StringCfgProperty(string stanza, string name, string defaultValue, int minLength) : base(stanza, name)
            {
                this.minLength = -1;
                this.maxLength = -1;
                this.defaultValue = defaultValue;
                this.minLength = minLength;
            }

            public StringCfgProperty(string stanza, string name, string defaultValue, int minLength, int maxLength) : base(stanza, name)
            {
                this.minLength = -1;
                this.maxLength = -1;
                this.defaultValue = defaultValue;
                this.minLength = minLength;
                this.maxLength = maxLength;
            }

            public string ParseValue(string stringValue)
            {
                if (((stringValue == null) || ((this.minLength >= 0) && (stringValue.Length < this.minLength))) || ((this.maxLength >= 0) && (stringValue.Length > this.maxLength)))
                {
                    return this.defaultValue;
                }
                return stringValue;
            }
        }
    }
}

