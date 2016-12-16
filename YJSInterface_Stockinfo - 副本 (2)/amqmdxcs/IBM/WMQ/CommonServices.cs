namespace IBM.WMQ
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Management;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;

    public sealed class CommonServices
    {
        [ThreadStatic]
        private static CommonServicesInterface cs = null;
        internal static Assembly csAssembly = null;
        private static Type csClassRef = null;
        internal static int csMQRelease = 0;
        internal static int csMQVersion = 0;
        private static string csVRMF = null;
        private const int DEFAULT_BASIC_LENGTH = 0x400;
        internal const string FFST_COMP_APARTMENTSTATE = "Apartment State:    ";
        internal const string FFST_COMP_BACKGROUNDTHREAD = "Background Thread:  ";
        internal const string FFST_COMP_CLRVER = "CLR Version:        ";
        internal const string FFST_COMP_COMMANDLINE = "Command Line:       ";
        internal const string FFST_COMP_CURRENTDIRECTORY = "Current Directory:  ";
        internal const string FFST_COMP_INTERACTIVEUSER = "Interactive User:   ";
        internal const string FFST_COMP_OSVER = "OS Version:         ";
        internal const string FFST_COMP_POOLTHREAD = "Pool Thread:        ";
        internal const string FFST_COMP_SHUTDOWNSTARTED = "Shutdown Started:   ";
        internal const string FFST_COMP_STACKTRACE = "Stack Trace:        ";
        internal const string FFST_COMP_THREADPRIORITY = "Thread Priority:    ";
        internal const string FFST_COMP_THREADSTATE = "Thread State:       ";
        internal const string FFST_COMP_USERDOMAIN = "User Domain Name:   ";
        internal const string FFST_COMP_USERNAME = "User Name:          ";
        internal const string FFST_COMP_WORKINGSET = "Working Set:        ";
        internal static string installationId = null;
        private static string installationKey = null;
        internal static string installationName = null;
        internal static string installationPath = null;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private static Hashtable UserSIDCache = new Hashtable();
        internal static Assembly wcfClientWmqAssembly = null;
        internal static bool wcfInUse = false;
        internal static Assembly xmsClientWmqAssembly = null;
        internal static bool xmsInUse = false;

        static CommonServices()
        {
            csAssembly = Assembly.GetExecutingAssembly();
            csVRMF = csAssembly.GetName().Version.ToString();
            string[] strArray = csVRMF.Split(new char[] { '.' });
            csMQVersion = Convert.ToInt32(strArray[0]);
            csMQRelease = Convert.ToInt32(strArray[1]);
            if (csAssembly.GlobalAssemblyCache)
            {
                installationName = GetRegistryValue(@"SOFTWARE\IBM\WebSphere MQ\Primary", "Name");
                installationKey = @"SOFTWARE\IBM\WebSphere MQ\Installation\" + installationName;
                installationPath = GetRegistryValue(installationKey, "FilePath");
                installationId = GetRegistryValue(installationKey, "Identifier");
            }
            else
            {
                string[] subKeyNames = OpenRegistryKey(@"SOFTWARE\IBM\WebSphere MQ\Installation").GetSubKeyNames();
                string location = csAssembly.Location;
                for (int i = 0; i < subKeyNames.Length; i++)
                {
                    installationKey = @"SOFTWARE\IBM\WebSphere MQ\Installation\" + subKeyNames[i];
                    string registryValue = GetRegistryValue(installationKey, "FilePath");
                    if (location.ToLower() == (registryValue + @"\bin\amqmdxcs.dll").ToLower())
                    {
                        installationName = subKeyNames[i];
                        installationPath = registryValue;
                        installationId = GetRegistryValue(installationKey, "Identifier");
                        return;
                    }
                }
            }
        }

        private CommonServices()
        {
        }

        private static void ByteSwapShort(ref byte[] buffer, int length)
        {
            for (int i = 0; i < length; i += 2)
            {
                byte num2 = buffer[i];
                buffer[i] = buffer[i + 1];
                buffer[i + 1] = num2;
            }
        }

        public static uint ConvertString(int fromCCSID, int toCCSID, byte[] inString, int inLength, out byte[] outString, ref int outLength, int options, out uint bytesConverted)
        {
            return ConvertString(string.Empty, fromCCSID, toCCSID, inString, inLength, out outString, ref outLength, options, out bytesConverted);
        }

        public static uint ConvertString(string objectId, int fromCCSID, int toCCSID, byte[] inString, int inLength, out byte[] outString, ref int outLength, int options, out uint bytesConverted)
        {
            uint num = 0x20806058;
            if (outLength == 0)
            {
                outLength = inLength * 4;
            }
            outString = new byte[outLength];
            bytesConverted = 0;
            if ((fromCCSID == 0x4b0) && (inLength >= 2))
            {
                if ((inString[0] == 0xfe) && (inString[1] == 0xff))
                {
                    options &= -241;
                    if ((toCCSID == fromCCSID) && ((options & 0x100) == 0x100))
                    {
                        options &= -3841;
                    }
                    else
                    {
                        options |= 0x10;
                    }
                }
                else if ((inString[0] == 0xff) && (inString[1] == 0xff))
                {
                    options &= -241;
                    if ((toCCSID == fromCCSID) && ((options & 0x200) == 0x200))
                    {
                        options &= -3841;
                    }
                    else
                    {
                        options |= 0x20;
                    }
                }
            }
            if ((fromCCSID == 0x4b0) && (fromCCSID == toCCSID))
            {
               // int lTemp = Convert.ToInt32("0xfffffffeL", 16);
                outLength = Math.Min(inLength, outLength) & unchecked((int)4294967294);
                Buffer.BlockCopy(inString, 0, outString, 0, outLength);
                bytesConverted = (uint) outLength;
                if (((options & 0x210) == 0x210) || ((options & 0x120) == 0x120))
                {
                    ByteSwapShort(ref outString, outLength);
                }
                return 0;
            }
            int num2 = options & -268439537;
            inLength = Math.Min(inLength, outLength * 4);
            if ((fromCCSID == 0x4b0) && ((options & 0x10) == 0x10))
            {
                ByteSwapShort(ref inString, inLength);
            }
            if ((cs != null) || CreateCommonServices())
            {
                num = cs.ConvertString(objectId, fromCCSID, toCCSID, inString, inLength, ref outString, ref outLength, num2, ref bytesConverted);
            }
            if (((fromCCSID == 0x4b0) && ((options & 0x10000000) != 0x10000000)) && ((options & 0x10) == 0x10))
            {
                ByteSwapShort(ref inString, inLength);
            }
            if (((num == 0) && (toCCSID == 0x4b0)) && ((options & 0x100) == 0x100))
            {
                ByteSwapShort(ref outString, outLength);
            }
            return num;
        }

        private static bool CreateCommonServices()
        {
            lock (typeof(CommonServices))
            {
                if (csClassRef == null)
                {
                    string name = null;
                    string propertyValue = GetPropertyValue("MQCSCLASS");
                    if (propertyValue != null)
                    {
                        if (propertyValue.IndexOf('(') != -1)
                        {
                            try
                            {
                                string[] strArray = propertyValue.Split(new char[] { '(', ')' });
                                csClassRef = Assembly.LoadFrom(strArray[0]).GetType(strArray[1], true);
                            }
                            catch
                            {
                            }
                        }
                        else
                        {
                            name = propertyValue;
                        }
                    }
                    if ((csClassRef == null) && (csAssembly != null))
                    {
                        if (name == null)
                        {
                            bool flag = false;
                            using (RegistryKey key = OpenRegistryKey(installationKey + @"\Components"))
                            {
                                propertyValue = GetPropertyValue("NMQ_MQ_LIB");
                                if ((key != null) && (((key.GetValue("Server") != null) || (key.GetValue(@"Local Clients\Windows NT Client") != null)) || (key.GetValue("XA_Client") != null)))
                                {
                                    flag = true;
                                }
                                if (!flag || ((propertyValue != null) && (propertyValue == "managed")))
                                {
                                    name = "IBM.WMQ.ManagedCommonServices";
                                }
                                else if (IsXmsInUse())
                                {
                                    name = "IBM.XMS.Client.WMQ.XmsManagedCommonServices";
                                }
                                else if (IsWcfInUse())
                                {
                                    name = "IBM.WMQ.WCF.MqWcfManagedCommonServices";
                                }
                                else
                                {
                                    name = "IBM.WMQ.MQCommonServices";
                                }
                            }
                        }
                        if (IsXmsInUse())
                        {
                            csClassRef = xmsClientWmqAssembly.GetType(name, true);
                        }
                        else if (IsWcfInUse())
                        {
                            csClassRef = wcfClientWmqAssembly.GetType(name, true);
                        }
                        else
                        {
                            csClassRef = csAssembly.GetType(name, true);
                        }
                    }
                }
            }
            if (csClassRef != null)
            {
                cs = (CommonServicesInterface) csClassRef.InvokeMember(null, BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, null, null);
            }
            if (cs != null)
            {
                TraceEnvironment();
            }
            return (null != cs);
        }

        public static void DisplayCopyright()
        {
            if ((cs != null) || CreateCommonServices())
            {
                cs.DisplayCopyright();
            }
        }

        public static uint DisplayMessage(string qmgrName, uint returncode, uint mtype)
        {
            return DisplayMessage(string.Empty, qmgrName, returncode, mtype);
        }

        public static uint DisplayMessage(string objectId, string qmgrName, uint returncode, uint mtype)
        {
            uint num = 0x20806058;
            if ((cs == null) && !CreateCommonServices())
            {
                return num;
            }
            return cs.DisplayMessage(objectId, qmgrName, returncode, mtype);
        }

        public static uint FFST(string sccsid, string lineno, uint component, uint method, uint probeid, uint tag, ushort alert_num)
        {
            return FFST(string.Empty, sccsid, lineno, component, method, probeid, tag, alert_num);
        }

        public static uint FFST(string objectId, string sccsid, string lineno, uint component, string method, uint probeid, uint tag, ushort alert_num)
        {
            uint num = 0x20806058;
            if ((cs == null) && !CreateCommonServices())
            {
                return num;
            }
            return cs.FFST(objectId, sccsid, lineno, component, method, probeid, tag, alert_num);
        }

        public static uint FFST(string objectId, string sccsid, string lineno, uint component, uint method, uint probeid, uint tag, ushort alert_num)
        {
            uint num = 0x20806058;
            if ((cs == null) && !CreateCommonServices())
            {
                return num;
            }
            return cs.FFST(objectId, sccsid, lineno, component, method, probeid, tag, alert_num);
        }

        public static string GetEnvironmentVariable(string name)
        {
            if ((cs == null) && !CreateCommonServices())
            {
                return null;
            }
            return cs.GetEnvironmentValue(name);
        }

        public static string GetFunctionName(uint component, uint method)
        {
            if ((cs == null) && !CreateCommonServices())
            {
                return null;
            }
            return cs.GetFunctionName(component, method);
        }

        public static string GetInstallationId()
        {
            return installationId;
        }

        public static string GetInstallationKey()
        {
            return installationKey;
        }

        public static string GetInstallationName()
        {
            return installationName;
        }

        public static string GetInstallationPath()
        {
            return installationPath;
        }

        public static uint GetMessage(uint returncode, uint control, out string basicmessage)
        {
            return GetMessage(string.Empty, returncode, control, out basicmessage);
        }

        public static uint GetMessage(string objectId, uint returncode, uint control, out string basicmessage)
        {
            string str;
            string str2;
            return GetMessage(objectId, returncode, control, out basicmessage, out str, out str2, 0x400, 0, 0);
        }

        public static uint GetMessage(uint returncode, uint control, out string basicmessage, out string extendedmessage, out string replymessage, int basicLength, int extendedLength, int replyLength)
        {
            return GetMessage(string.Empty, returncode, control, out basicmessage, out extendedmessage, out replymessage, basicLength, extendedLength, replyLength);
        }

        public static uint GetMessage(string objectId, uint returncode, uint control, out string basicmessage, out string extendedmessage, out string replymessage, int basicLength, int extendedLength, int replyLength)
        {
            if ((cs != null) || CreateCommonServices())
            {
                return cs.GetMessage(objectId, returncode, control, out basicmessage, out extendedmessage, out replymessage, basicLength, extendedLength, replyLength);
            }
            basicmessage = "";
            extendedmessage = "";
            replymessage = "";
            return 0x20806058;
        }

        internal static int GetMQCSDApplied()
        {
            int num = 0;
            try
            {
                string[] strArray = csVRMF.Split(new char[] { '.' });
                if (strArray.Length >= 4)
                {
                    num = (int.Parse(strArray[2]) * 100) + int.Parse(strArray[3]);
                }
            }
            catch
            {
            }
            return num;
        }

        public static string GetMQIniAttribute(string valueName)
        {
            string keyName = null;
            string str2 = valueName;
            if (str2 != null)
            {
                if (!(str2 == "FilePath"))
                {
                    if (str2 == "WorkPath")
                    {
                        keyName = @"SOFTWARE\IBM\WebSphere MQ";
                    }
                }
                else
                {
                    keyName = installationKey;
                }
            }
            if (keyName != null)
            {
                return GetRegistryValue(keyName, valueName);
            }
            return null;
        }

        public static string GetPropertyValue(string propertyName)
        {
            return GetPropertyValue(propertyName, true);
        }

        public static string GetPropertyValue(string propertyName, bool tryEnvironment)
        {
            string str = null;
            try
            {
                NameValueCollection appSettings = ConfigurationManager.AppSettings;
                if (appSettings != null)
                {
                    string[] values = appSettings.GetValues(propertyName);
                    if (values != null)
                    {
                        str = values[0];
                    }
                }
            }
            catch (Exception)
            {
            }
            if (!tryEnvironment || (str != null))
            {
                return str;
            }
            if (cs != null)
            {
                return cs.GetEnvironmentValue(propertyName);
            }
            return Environment.GetEnvironmentVariable(propertyName);
        }

        public static string GetRegistryValue(string keyName, string valueName)
        {
            string str = null;
            using (RegistryKey key = OpenRegistryKey(keyName))
            {
                if (key != null)
                {
                    object obj2 = key.GetValue(valueName);
                    if (obj2 != null)
                    {
                        str = obj2.ToString();
                    }
                }
            }
            return str;
        }

        public static byte[] GetUserSID(string user, string domain)
        {
            string str = null;
            string key = null;
            Type type;
            Monitor.Enter(type = typeof(CommonServices));
            try
            {
                key = "name='" + user + "'";
                if (domain != null)
                {
                    key = key + " and domain='" + domain + "'";
                }
                if (UserSIDCache.ContainsKey(key))
                {
                    str = (string) UserSIDCache[key];
                }
                else
                {
                    try
                    {
                        SelectQuery query = new SelectQuery("SELECT * FROM Win32_UserAccount where " + key);
                        ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                        foreach (ManagementObject obj2 in searcher.Get())
                        {
                            str = obj2["SID"].ToString();
                        }
                    }
                    catch (Exception exception)
                    {
                        TraceException(exception);
                    }
                    finally
                    {
                        UserSIDCache.Add(key, str);
                    }
                }
            }
            catch
            {
            }
            finally
            {
                Monitor.Exit(type);
            }
            byte[] dst = null;
            if (str != null)
            {
                try
                {
                    string[] strArray = str.Split(new char[] { '-' });
                    TraceText("SID for " + key + " is '" + str + "'");
                    if ((strArray.Length < 4) || !(strArray[0] == "S"))
                    {
                        throw new Exception("SID returned is invalid");
                    }
                    dst = new byte[8 + ((strArray.Length - 3) * 4)];
                    dst[0] = Convert.ToByte(strArray[1]);
                    dst[1] = Convert.ToByte((int) (strArray.Length - 3));
                    byte[] bytes = BitConverter.GetBytes(Convert.ToInt64(strArray[2]));
                    Array.Reverse(bytes);
                    Buffer.BlockCopy(bytes, 2, dst, 2, 6);
                    for (int i = 3; i < strArray.Length; i++)
                    {
                        Buffer.BlockCopy(BitConverter.GetBytes(Convert.ToUInt32(strArray[i])), 0, dst, 8 + ((i - 3) * 4), 4);
                    }
                    return dst;
                }
                catch (Exception exception2)
                {
                    dst = null;
                    TraceException(exception2);
                    return dst;
                }
            }
            TraceText("No SID found for " + key);
            return dst;
        }

        public static string GetVRMF()
        {
            return csVRMF;
        }

        public static bool Is64bitCLR()
        {
            return (8 == Marshal.SizeOf(typeof(IntPtr)));
        }

        public static bool IsWcfInUse()
        {
            if (!wcfInUse)
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++)
                {
                    if (assemblies[i].FullName.Contains("IBM.WMQ.WCF"))
                    {
                        wcfInUse = true;
                        wcfClientWmqAssembly = assemblies[i];
                        break;
                    }
                }
            }
            return wcfInUse;
        }

        public static bool IsXmsInUse()
        {
            if (!xmsInUse)
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++)
                {
                    if (assemblies[i].FullName.Contains("IBM.XMS.Client.WMQ"))
                    {
                        xmsInUse = true;
                        xmsClientWmqAssembly = assemblies[i];
                        break;
                    }
                }
            }
            return xmsInUse;
        }

        public static RegistryKey OpenRegistryKey(string keyName)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName);
            if (key == null)
            {
                string name = keyName.Replace("SOFTWARE", @"SOFTWARE\Wow6432Node");
                if (keyName != name)
                {
                    key = Registry.LocalMachine.OpenSubKey(name);
                }
            }
            return key;
        }

        public static string ReasonCodeName(int reason)
        {
            if ((cs == null) && !CreateCommonServices())
            {
                return "";
            }
            return cs.ReasonCodeName(reason);
        }

        public static bool SetEnvironmentVariable(string name, string value)
        {
            if ((cs == null) && !CreateCommonServices())
            {
                return false;
            }
            return cs.SetEnvironmentValue(name, value);
        }

        public static void SetValidInserts()
        {
            if ((cs != null) || CreateCommonServices())
            {
                cs.SetValidInserts();
            }
        }

        public static void TraceConstructor(string objectId, string sccsid)
        {
            if ((cs != null) || CreateCommonServices())
            {
                cs.TraceConstructor(objectId, sccsid);
            }
        }

        public static void TraceData(uint component, uint method, ushort level, string caption, uint offset, int length, byte[] buf)
        {
            TraceData(string.Empty, component, method, level, caption, offset, length, buf);
        }

        public static void TraceData(string objectId, uint component, uint method, ushort level, string caption, uint offset, int length, byte[] buf)
        {
            if ((cs != null) || CreateCommonServices())
            {
                cs.TraceData(objectId, component, method, level, caption, offset, length, buf);
            }
        }

        public static void TraceDetail(ushort level, string text)
        {
            TraceDetail(string.Empty, 0, 0, level, text);
        }

        public static void TraceDetail(string objectId, ushort level, string text)
        {
            TraceDetail(objectId, 0, 0, level, text);
        }

        public static void TraceDetail(uint component, uint method, ushort level, string text)
        {
            TraceDetail(string.Empty, component, method, level, text);
        }

        public static void TraceDetail(string objectId, uint component, uint method, ushort level, string text)
        {
            if ((cs != null) || CreateCommonServices())
            {
                cs.TraceDetail(objectId, component, method, level, text);
            }
        }

        public static bool TraceEnabled()
        {
            if ((cs == null) && !CreateCommonServices())
            {
                return false;
            }
            return cs.TraceEnabled();
        }

        public static void TraceEntry(uint component, uint method)
        {
            TraceEntry(string.Empty, component, method, null);
        }

        public static void TraceEntry(string objectId, uint component, uint method)
        {
            TraceEntry(objectId, component, method, null);
        }

        public static void TraceEntry(string objectId, uint component, uint method, object[] parameters)
        {
            if ((cs != null) || CreateCommonServices())
            {
                cs.TraceEntry(objectId, component, method, parameters);
            }
        }

        public static void TraceEnvironment()
        {
            if ((cs != null) || CreateCommonServices())
            {
                TraceText("OS Version:         " + Environment.OSVersion.ToString());
                TraceText("CLR Version:        " + Environment.Version.ToString());
                TraceText("MachineName:        " + Environment.MachineName);
                TraceText("Command Line:       " + Environment.CommandLine);
                TraceText("ApartmentState:     " + Thread.CurrentThread.GetApartmentState().ToString());
                TraceText("IsThreadPoolThread: " + Thread.CurrentThread.IsThreadPoolThread.ToString());
                TraceText("UserName:           " + Environment.UserName);
                TraceText("UserDomainName:     " + Environment.UserDomainName);
                cs.TraceEnvironment();
                try
                {
                    foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
                    {
                        TraceText("Loaded:             " + module.FileName);
                    }
                }
                catch (Win32Exception)
                {
                    TraceText("Ignore Win32Exception");
                }
            }
        }

        public static void TraceException(Exception ex)
        {
            TraceException(string.Empty, 0, 0, ex);
        }

        public static void TraceException(string objectId, Exception ex)
        {
            TraceException(objectId, 0, 0, ex);
        }

        public static void TraceException(uint component, uint method, Exception ex)
        {
            TraceException(string.Empty, component, method, ex);
        }

        public static void TraceException(string objectId, uint component, uint method, Exception ex)
        {
            if ((cs != null) || CreateCommonServices())
            {
                cs.TraceException(objectId, component, method, ex);
            }
        }

        public static void TraceExit(uint component, uint method, int returnCode)
        {
            TraceExit(string.Empty, component, method, null, 0, returnCode);
        }

        public static void TraceExit(string objectId, uint component, uint method, int returnCode)
        {
            TraceExit(objectId, component, method, null, 0, returnCode);
        }

        public static void TraceExit(string objectId, uint component, uint method, int index, int returnCode)
        {
            TraceExit(objectId, component, method, null, index, returnCode);
        }

        public static void TraceExit(string objectId, uint component, uint method, object result, int returnCode)
        {
            TraceExit(objectId, component, method, result, 0, returnCode);
        }

        public static void TraceExit(string objectId, uint component, uint method, object result, int index, int returnCode)
        {
            if ((cs != null) || CreateCommonServices())
            {
                cs.TraceExit(objectId, component, method, result, index, returnCode);
            }
        }

        public static bool TraceStatus()
        {
            if ((cs == null) && !CreateCommonServices())
            {
                return false;
            }
            return cs.TraceStatus();
        }

        public static void TraceText(string text)
        {
            TraceDetail(string.Empty, 0, 0, 0, text);
        }

        public static void TraceText(string objectId, string text)
        {
            TraceDetail(objectId, 0, 0, 0, text);
        }

        public static void TraceText(uint component, uint method, string text)
        {
            TraceDetail(string.Empty, component, method, 0, text);
        }

        public static void TraceText(string objectId, uint component, uint method, string text)
        {
            TraceDetail(objectId, component, method, 0, text);
        }

        public static void TraceUserData(string objectId, uint component, uint method, ushort level, string caption, uint offset, int length, byte[] buf)
        {
            if ((cs != null) || CreateCommonServices())
            {
                cs.TraceUserData(objectId, component, method, level, caption, offset, length, buf);
            }
        }

        public static uint ArithInsert1
        {
            get
            {
                if ((cs == null) && !CreateCommonServices())
                {
                    return 0;
                }
                return cs.GetArithInsert1();
            }
            set
            {
                if ((cs != null) || CreateCommonServices())
                {
                    cs.SetArithInsert1(value);
                }
            }
        }

        public static uint ArithInsert2
        {
            get
            {
                if ((cs == null) && !CreateCommonServices())
                {
                    return 0;
                }
                return cs.GetArithInsert2();
            }
            set
            {
                if ((cs != null) || CreateCommonServices())
                {
                    cs.SetArithInsert2(value);
                }
            }
        }

        public static string CommentInsert1
        {
            get
            {
                if ((cs == null) && !CreateCommonServices())
                {
                    return "";
                }
                return cs.GetCommentInsert1();
            }
            set
            {
                if ((cs != null) || CreateCommonServices())
                {
                    cs.SetCommentInsert1(value);
                }
            }
        }

        public static string CommentInsert2
        {
            get
            {
                if ((cs == null) && !CreateCommonServices())
                {
                    return "";
                }
                return cs.GetCommentInsert2();
            }
            set
            {
                if ((cs != null) || CreateCommonServices())
                {
                    cs.SetCommentInsert2(value);
                }
            }
        }

        public static string CommentInsert3
        {
            get
            {
                if ((cs == null) && !CreateCommonServices())
                {
                    return "";
                }
                return cs.GetCommentInsert3();
            }
            set
            {
                if ((cs != null) || CreateCommonServices())
                {
                    cs.SetCommentInsert3(value);
                }
            }
        }

        public static Type CommonServicesClass
        {
            get
            {
                return csClassRef;
            }
            set
            {
                if ((csClassRef == null) && (GetPropertyValue("MQCSCLASS") == null))
                {
                    csClassRef = value;
                }
            }
        }

        public static string DataLib
        {
            get
            {
                if ((cs == null) && !CreateCommonServices())
                {
                    return @"C:\Program Files\IBM\WebSphere MQ";
                }
                return cs.GetDataLib();
            }
        }

        public static int MQCSDApplied
        {
            get
            {
                return GetMQCSDApplied();
            }
        }

        public static int MQRelease
        {
            get
            {
                if ((cs == null) && !CreateCommonServices())
                {
                    return 0;
                }
                return cs.GetMQRelease();
            }
        }

        public static int MQVersion
        {
            get
            {
                if ((cs == null) && !CreateCommonServices())
                {
                    return 0;
                }
                return cs.GetMQVersion();
            }
        }

        public static string ProgramName
        {
            get
            {
                if ((cs == null) && !CreateCommonServices())
                {
                    return string.Empty;
                }
                return cs.GetProgramName();
            }
        }
    }
}

