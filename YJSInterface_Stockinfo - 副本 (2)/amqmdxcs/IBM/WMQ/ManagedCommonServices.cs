namespace IBM.WMQ
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Management;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;

    public class ManagedCommonServices : CommonServicesInterface
    {
        protected static StringWriter addrMode = new StringWriter();
        protected uint arithInsert1;
        protected uint arithInsert2;
        protected static DateTime buildDate;
        protected static string cmvcLevelName = null;
        protected string commentInsert1 = string.Empty;
        protected string commentInsert2 = string.Empty;
        protected string commentInsert3 = string.Empty;
        protected string commentInsert4 = string.Empty;
        protected static string companyName = null;
        private static int counter = -1;
        protected string cultureName = string.Empty;
        private static ResourceManager defaultResources = null;
        private static bool defTraceIsOn = false;
        private static int defTraceUserDataSize = -1;
        private string extendedPrefix;
        private const string FFST_BLANK_LINE = "";
        private const string FFST_COMP_FOOTER = "";
        private const string FFST_COMP_HEADER1_FORMAT = "Component Dumps (Thread {0})";
        private const string FFST_COMP_HEADER2 = "------< MSDNET \".NET\" >-----";
        private const string FFST_CONFIG_FOOTER = "";
        private const string FFST_CONFIG_FORMAT = "{0}={1}";
        private const string FFST_CONFIG_HEADER = "Configuration Values";
        private static string[] FFST_CONFIG_LIST = new string[] { "NMQ_MQ_LIB", "MQCSCLASS", "MQTRACEPATH", "MQTRACELEVEL", "MQTRACEUSERDATASIZE", "MQERRORPATH", "MQLOGLEVEL", "MQFFSTLEVEL", "MQSERVER", "MQ_COMM", "MQCHLLIB", "MQCHLTAB", "NO_AMQERR", "MQS_IPC_HOST" };
        private const string FFST_CONFIG_NONE = "None";
        private const string FFST_ENVVAR_FOOTER = "";
        private const string FFST_ENVVAR_FORMAT = "{0}={1}";
        private const string FFST_ENVVAR_HEADER = "Environment Variables";
        private const string FFST_FILE_EXTENSION = ".FDC";
        private const string FFST_HDR_ADDRMODE = "Addressing Mode   :- ";
        private const string FFST_HDR_ADDRMODE_FORMAT = "{0}-bit";
        private const string FFST_HDR_APPLICATIONNAME = "Application Name  :- MQM";
        private const string FFST_HDR_ARITH1 = "Arith1            :- ";
        private const string FFST_HDR_ARITH2 = "Arith2            :- ";
        private const string FFST_HDR_BLANK_LABEL = "                     ";
        private const string FFST_HDR_BUILDDATE = "Build Date        :- ";
        private const string FFST_HDR_BUILDDATE_FORMAT = "MMM d yyy";
        private const string FFST_HDR_BUILDTYPE = "Build Type        :- IKAP - (Production)";
        private const string FFST_HDR_CMVCLEVEL = "Build Level       :- ";
        private const string FFST_HDR_COMMENT1 = "Comment1          :- ";
        private const string FFST_HDR_COMMENT2 = "Comment2          :- ";
        private const string FFST_HDR_COMMENT3 = "Comment3          :- ";
        private const string FFST_HDR_COMPONENT = "Component         :- ";
        private const string FFST_HDR_DATETIME = "Date/Time         :- ";
        private const string FFST_HDR_DATETIME_FORMAT = @"ddd MMMM dd HH:mm:ss G\MTzz yyyy";
        private const string FFST_HDR_FDCSEQUENCENO = "FDCSequenceNumber :- ";
        private const string FFST_HDR_HOSTNAME = "Host Name         :- ";
        private const int FFST_HDR_LABEL_WIDTH = 0x15;
        private const string FFST_HDR_LEFT = "| ";
        private const string FFST_HDR_LINE = "+-----------------------------------------------------------------------------+";
        private const string FFST_HDR_LINENO = "Line Number       :- ";
        private const string FFST_HDR_LVLS = "LVLS              :- ";
        private const string FFST_HDR_MAJORERRORCODE = "Major Errorcode   :- ";
        private const string FFST_HDR_MINORERRORCODE = "Minor Errorcode   :- ";
        private const string FFST_HDR_OPSYS = "Operating System  :- ";
        private const string FFST_HDR_OPSYS_FORMAT = "{0} {1}";
        private const string FFST_HDR_PIDS = "PIDS              :- 5724H7220";
        private const string FFST_HDR_PROBEDESC = "Probe Description :- ";
        private const string FFST_HDR_PROBEID = "Probe Id          :- ";
        private const string FFST_HDR_PROBEID_FORMAT = "{0}{1}{2}";
        private const string FFST_HDR_PROBESEVERITY = "Probe Severity    :- ";
        private const string FFST_HDR_PROBETYPE = "Probe Type        :- ";
        private const string FFST_HDR_PROCESSID = "Process           :- ";
        private const string FFST_HDR_PROCESSNAME = "Process Name      :- ";
        private const string FFST_HDR_PRODUCTLONGNAME = "Product Long Name :- ";
        private const string FFST_HDR_RIGHT = " |";
        private const string FFST_HDR_SCCSINFO = "SCCS Info         :- ";
        private const string FFST_HDR_THREADID = "Thread            :- ";
        private const string FFST_HDR_TITLE = "WebSphere MQ First Failure Symptom Report";
        private const string FFST_HDR_USERID = "UserID            :- ";
        private const string FFST_HDR_UTC_OFFSET = "UTC Time Offset   :- ";
        private const string FFST_HDR_UTCTIME = "UTC Time          :- ";
        private const string FFST_HDR_VENDOR = "Vendor            :- ";
        private const int FFST_HDR_WIDTH = 0x4b;
        private const string FFST_HIST_FOOTER = "";
        private const string FFST_HIST_FORMAT = "-{0}{1}";
        private const string FFST_HIST_HEADER = "MQM Trace History";
        private const string FFST_STACK_FOOTER = "";
        private const string FFST_STACK_HEADER = "MQM Function Stack";
        private static string ffstFileName = null;
        protected static bool ffstIsOn = true;
        private static string ffstPath = null;
        private static int ffstSequenceNo = 0;
        protected static StringWriter hostName = new StringWriter();
        protected string idString = string.Empty;
        protected string indent = "--";
        private bool inFFST;
        private const string LOG_EVENTLOG_NAME = "Application";
        private const string LOG_EVENTLOG_SOURCE = "WebSphere MQ Managed Client";
        private const string LOG_FILE_EXTENSION = ".LOG";
        private static int LOG_PAGEWIDTH = 0x4f;
        protected static bool logIsOn = true;
        private static string logPath = null;
        private static uint logRolloverSize = 0x200000;
        private static string mqHostName = null;
        private static bool MQTraceWasEnabled = false;
        private static readonly char[] MSG_TRIM_CHARS = new char[] { '\n', '\r', ' ' };
        protected static StringWriter opSys = new StringWriter();
        protected static int processId = 0;
        protected static string processName = null;
        protected static string productName = null;
        private static Hashtable PseudoEnvironmentVariables = new Hashtable();
        private string replyPrefix;
        private static string RESOURCE_STUB = "amqmdxcs";
        protected ResourceManager resources;
        protected Encoding resourceTextEncoding;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private static bool staticsInitialised = false;
        protected int threadId;
        protected string timeString = string.Empty;
        protected static MQCommonServices.xtrTRACECONTROLMEM traceControlMem;
        private static string traceFileName = null;
        private ManagedTracePoint[] traceHistory;
        private int traceHistoryPos;
        protected static bool traceIsOn = false;
        private static DateTime traceLastWriteTimeUtc = new DateTime(0L);
        private static ushort traceLevel = 0;
        private static string tracePath = null;
        private static string traceShm = null;
        private ManagedTracePoint[] traceStack;
        private int traceStackPos;
        private static int traceUserDataSize = -1;
        private static TextWriter traceWriter = null;
        private const int TRC_CHECK_INTERVAL = 0x100;
        private const int TRC_DATA_BYTES_PER_LINE = 0x10;
        private const string TRC_DATA_CAPTION_FORMAT = "{0} !! - {1}:\r\n{0} Data:- {2}";
        private const string TRC_DATA_CONTENT_FORMAT = "{0}  0x{1} {2}: {3}";
        private const string TRC_DATA_SUPPRESS_FORMAT = "{0}  {1} lines suppressed, same as above";
        private const string TRC_DATE_FORMAT = "dd/MM/yyyy";
        private const string TRC_DETAIL_FORMAT = "{0} {1}   {2}   {3}";
        private const string TRC_ENTRYEXIT_FORMAT = "{0} {1}   {2}  {3}{4}";
        private const string TRC_FILE_EXTENSION = ".TRC";
        private const string TRC_HEADER_FORMAT = "Process : {0} ({1})\r\nHost : {2}\r\nOperating System : {3}\r\nProduct Long Name : {4}\r\nVersion : {5}    Level : {6}\r\nDate : {7}  Time : {8}\r\n\r\nCounter  TimeStamp          PID.TID   Data\r\n============================================================";
        private const string TRC_LINE_FORMAT = "{0} {1}   {2}  ";
        private const string TRC_TIME_FORMAT = "HH:mm:ss.ffffff";
        protected static string vrmf = null;
        private static string workPath = null;
        private static int XEE_COPY_BLOCK_SIZE = 0x8000;

        public ManagedCommonServices()
        {
            lock (typeof(ManagedCommonServices))
            {
                if (!staticsInitialised)
                {
                    uint num = 0;
                    int num2 = -1;
                    uint num3 = 1;
                    uint num4 = 1;
                    string s = null;
                    string propertyValue = null;
                    string str3 = null;
                    string str4 = null;
                    string str5 = null;
                    workPath = CommonServices.GetMQIniAttribute("WorkPath");
                    if (workPath != null)
                    {
                        workPath = workPath + @"\";
                    }
                    else
                    {
                        workPath = string.Empty;
                    }
                    s = CommonServices.GetPropertyValue("MQTRACELEVEL");
                    if ((s != null) && (s != string.Empty))
                    {
                        try
                        {
                            num = ushort.Parse(s);
                            num = Math.Min(num, 9);
                        }
                        catch
                        {
                        }
                    }
                    if (num > 0)
                    {
                        if (num == 1)
                        {
                            traceLevel = 0;
                        }
                        else
                        {
                            traceLevel = 2;
                        }
                        defTraceIsOn = traceIsOn = true;
                    }
                    else
                    {
                        defTraceIsOn = traceIsOn = false;
                    }
                    propertyValue = CommonServices.GetPropertyValue("MQTRACEUSERDATASIZE");
                    if ((propertyValue != null) && (propertyValue != string.Empty))
                    {
                        try
                        {
                            num2 = int.Parse(propertyValue);
                            num2 = Math.Max(num2, -1);
                        }
                        catch
                        {
                        }
                    }
                    defTraceUserDataSize = traceUserDataSize = num2;
                    tracePath = CommonServices.GetPropertyValue("MQTRACEPATH");
                    if (tracePath != null)
                    {
                        if (((tracePath != string.Empty) && !tracePath.EndsWith(@"\")) && !tracePath.EndsWith("/"))
                        {
                            tracePath = tracePath + @"\";
                        }
                    }
                    else
                    {
                        tracePath = workPath;
                        if (workPath != string.Empty)
                        {
                            tracePath = tracePath + @"\trace\";
                        }
                    }
                    str4 = CommonServices.GetPropertyValue("MQLOGLEVEL");
                    if ((str4 != null) && (str4 != string.Empty))
                    {
                        try
                        {
                            num4 = ushort.Parse(str4);
                            num4 = Math.Min(num4, 1);
                        }
                        catch
                        {
                        }
                    }
                    if (num4 == 1)
                    {
                        logIsOn = true;
                    }
                    else
                    {
                        logIsOn = false;
                    }
                    logPath = CommonServices.GetPropertyValue("MQERRORPATH");
                    if (logPath != null)
                    {
                        if (((logPath != string.Empty) && !logPath.EndsWith(@"\")) && !logPath.EndsWith("/"))
                        {
                            logPath = logPath + @"\";
                        }
                    }
                    else
                    {
                        logPath = workPath;
                        if (workPath != string.Empty)
                        {
                            logPath = logPath + @"errors\";
                        }
                    }
                    str5 = CommonServices.GetPropertyValue("MQMAXERRORLOGSIZE");
                    if ((str5 != null) && (str5 != string.Empty))
                    {
                        try
                        {
                            logRolloverSize = uint.Parse(str5);
                            if (logRolloverSize < 0x8000)
                            {
                                logRolloverSize = 0x8000;
                            }
                            else if (logRolloverSize > 0x80000000)
                            {
                                logRolloverSize = 0x80000000;
                            }
                        }
                        catch
                        {
                        }
                    }
                    str3 = CommonServices.GetPropertyValue("MQFFSTLEVEL");
                    if ((str3 != null) && (str3 != string.Empty))
                    {
                        try
                        {
                            num3 = ushort.Parse(str3);
                            num3 = Math.Min(num3, 1);
                        }
                        catch
                        {
                        }
                    }
                    if (num3 == 1)
                    {
                        ffstIsOn = true;
                    }
                    else
                    {
                        ffstIsOn = false;
                    }
                    ffstPath = CommonServices.GetPropertyValue("MQERRORPATH");
                    if (ffstPath != null)
                    {
                        if (((ffstPath != string.Empty) && !ffstPath.EndsWith(@"\")) && !ffstPath.EndsWith("/"))
                        {
                            ffstPath = ffstPath + @"\";
                        }
                    }
                    else
                    {
                        ffstPath = logPath;
                    }
                    try
                    {
                        processId = Process.GetCurrentProcess().Id;
                    }
                    catch
                    {
                    }
                    try
                    {
                        processName = Application.ExecutablePath;
                        buildDate = File.GetLastWriteTime(processName);
                    }
                    catch
                    {
                    }
                    if (processName == null)
                    {
                        processName = "Unknown";
                    }
                    cmvcLevelName = base.ExtractSCCSIDInfo("%Z% %W%  %I% %E% %U%", "#L").TrimEnd(new char[0]);
                    try
                    {
                        hostName.Write("{0}", Environment.MachineName);
                    }
                    catch
                    {
                    }
                    mqHostName = CommonServices.GetPropertyValue("MQS_IPC_HOST");
                    if (mqHostName == null)
                    {
                        mqHostName = Environment.MachineName;
                        int index = mqHostName.IndexOf(".");
                        if (index != -1)
                        {
                            mqHostName = Environment.MachineName.Substring(0, index);
                        }
                    }
                    ManagedCommonServices.traceShm = workPath;
                    if (workPath != string.Empty)
                    {
                        string traceShm = ManagedCommonServices.traceShm;
                        ManagedCommonServices.traceShm = traceShm + @"sockets\@SYSTEM\qmgrlocl\" + mqHostName + @"\" + CommonServices.GetInstallationId() + @"\";
                    }
                    ManagedCommonServices.traceShm = ManagedCommonServices.traceShm + "Trace.Shm";
                    try
                    {
                        opSys.Write("{0} {1}", Environment.OSVersion.ToString().Replace("Microsoft ", string.Empty), Environment.Version.ToString());
                    }
                    catch
                    {
                    }
                    try
                    {
                        addrMode.Write("{0}-bit", (IntPtr.Size == 4) ? "32" : "64");
                    }
                    catch
                    {
                    }
                    try
                    {
                        productName = ((AssemblyProductAttribute) CommonServices.csAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0]).Product;
                    }
                    catch
                    {
                    }
                    if (productName == null)
                    {
                        productName = "Unknown";
                    }
                    try
                    {
                        companyName = ((AssemblyCompanyAttribute) CommonServices.csAssembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)[0]).Company;
                    }
                    catch
                    {
                    }
                    if (companyName == null)
                    {
                        companyName = "Unknown";
                    }
                    vrmf = CommonServices.GetVRMF();
                    defaultResources = new ResourceManager(RESOURCE_STUB, Assembly.GetExecutingAssembly());
                    staticsInitialised = true;
                }
            }
            this.Initialize();
        }

        private void AccessResources(Assembly assembly)
        {
            CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
            if ((this.resources != null) && !(this.cultureName != currentUICulture.Name))
            {
                return;
            }
            string baseName = RESOURCE_STUB;
            switch (currentUICulture.TwoLetterISOLanguageName)
            {
                case "cs":
                case "hu":
                case "pl":
                    baseName = baseName + "." + currentUICulture.TwoLetterISOLanguageName;
                    this.resourceTextEncoding = Encoding.GetEncoding(0x4e2);
                    break;

                case "ru":
                    baseName = baseName + "." + currentUICulture.TwoLetterISOLanguageName;
                    this.resourceTextEncoding = Encoding.GetEncoding(0x4e3);
                    break;

                case "de":
                case "es":
                case "fr":
                case "it":
                case "pt":
                    baseName = baseName + "." + currentUICulture.TwoLetterISOLanguageName;
                    this.resourceTextEncoding = Encoding.GetEncoding(0x4e4);
                    break;

                case "ja":
                    baseName = baseName + "." + currentUICulture.TwoLetterISOLanguageName;
                    this.resourceTextEncoding = Encoding.GetEncoding(0x3a4);
                    break;

                case "ko":
                    baseName = baseName + "." + currentUICulture.TwoLetterISOLanguageName;
                    this.resourceTextEncoding = Encoding.GetEncoding(0x3b5);
                    break;

                case "zh":
                    switch (currentUICulture.Parent.Name)
                    {
                        case "zh-CHT":
                            baseName = baseName + "." + currentUICulture.Parent.Name;
                            this.resourceTextEncoding = Encoding.GetEncoding(950);
                            goto Label_029D;

                        case "zh-CHS":
                            baseName = baseName + "." + currentUICulture.Parent.Name;
                            this.resourceTextEncoding = Encoding.GetEncoding(0xd698);
                            goto Label_029D;
                    }
                    this.resourceTextEncoding = Encoding.GetEncoding(0x4e4);
                    break;

                default:
                    this.resourceTextEncoding = Encoding.GetEncoding(0x4e4);
                    break;
            }
        Label_029D:;
            this.TraceText("Accessing named resources '" + baseName + "' for culture '" + currentUICulture.Name + "'");
            if (baseName == RESOURCE_STUB)
            {
                this.resources = defaultResources;
            }
            else
            {
                try
                {
                    this.resources = new ResourceManager(baseName, assembly);
                }
                catch (ArgumentNullException)
                {
                    baseName = RESOURCE_STUB;
                    this.resourceTextEncoding = Encoding.GetEncoding(0x4e4);
                    goto Label_029D;
                }
            }
            this.extendedPrefix = null;
            this.replyPrefix = null;
            this.cultureName = currentUICulture.Name;
        }

        internal override uint ConvertString(string objectId, int fromCCSID, int toCCSID, byte[] inString, int inLength, ref byte[] outString, ref int outLength, int options, ref uint bytesConverted)
        {
            uint method = 0x30d;
            uint num2 = 0;
            this.TraceEntry(objectId, 0x48, method, null);
            try
            {
                Encoding srcEncoding = this.GetEncoding(fromCCSID);
                Encoding encoding = this.GetEncoding(toCCSID);
                byte[] bytes = Encoding.Convert(srcEncoding, encoding, inString, 0, inLength);
                if (outLength < bytes.Length)
                {
                    outLength = encoding.GetByteCount(encoding.GetChars(bytes, 0, outLength));
                    byte[] buffer2 = Encoding.Convert(encoding, srcEncoding, bytes, 0, outLength);
                    bytesConverted = (uint) buffer2.Length;
                    num2 = 0x10806055;
                }
                else
                {
                    outLength = bytes.Length;
                    bytesConverted = (uint) inLength;
                }
                outString = bytes;
            }
            finally
            {
                this.TraceExit(objectId, 0x48, method, (int) num2);
            }
            return num2;
        }

        internal override void DisplayCopyright()
        {
            string str;
            string str2;
            string str3;
            this.GetMessage(string.Empty, 0x6994, 7, out str3, out str, out str2, 1, 0, 0);
            Console.WriteLine(str3);
        }

        public override uint DisplayMessage(string objectId, string qmgrName, uint returncode, uint mtype)
        {
            string str4;
            int num4;
            uint method = 0x30b;
            uint tag = 0;
            DateTime now = DateTime.Now;
            int extendedLength = 1;
            uint control = 2;
            this.TraceEntry(objectId, 0x48, method, null);
            base.GetCurrentContext(out str4, out num4);
            try
            {
                string str;
                string str2;
                string str3;
                if ((mtype == 0x20) || (mtype == 0xf0000040))
                {
                    extendedLength = 0;
                    if (((returncode & 0xff000000) == 0) || ((returncode & 0xff000000) == 0x10000000))
                    {
                        control = 3;
                    }
                    control |= 4;
                }
                else
                {
                    if (!logIsOn)
                    {
                        return tag;
                    }
                    tag = this.GetMessage(objectId, returncode, 0x13, out str, out str2, out str3, 1, extendedLength, extendedLength);
                    if ((tag != 0) && (tag != 0x10806132))
                    {
                        return tag;
                    }
                    try
                    {
                        using (EventLog log = new EventLog("Application"))
                        {
                            EventLogEntryType warning;
                            int eventID = Convert.ToInt32((returncode & 0xffff).ToString("X4"));
                            if (!EventLog.SourceExists("WebSphere MQ Managed Client"))
                            {
                                EventLog.CreateEventSource("WebSphere MQ Managed Client", "Application");
                            }
                            log.Source = "WebSphere MQ Managed Client";
                            uint num10 = returncode & 0xff000000;
                            if (num10 != 0)
                            {
                                if (num10 != 0x10000000)
                                {
                                    goto Label_00FE;
                                }
                                warning = EventLogEntryType.Warning;
                            }
                            else
                            {
                                warning = EventLogEntryType.Information;
                            }
                            goto Label_0101;
                        Label_00FE:
                            warning = EventLogEntryType.Error;
                        Label_0101:;
                            log.WriteEntry(str + Environment.NewLine + str2 + Environment.NewLine + str3, warning, eventID);
                        }
                    }
                    catch (Exception exception)
                    {
                        this.TraceException(objectId, 0x48, method, exception);
                    }
                    if (CommonServices.GetPropertyValue("NO_AMQERR") != null)
                    {
                        return tag;
                    }
                    control |= 0x10;
                }
                control |= 8;
                tag = this.GetMessage(objectId, returncode, control, out str, out str2, out str3, 1, extendedLength, extendedLength);
                if ((tag != 0) && (tag != 0x10806132))
                {
                    return tag;
                }
                if ((mtype == 0x20) || (mtype == 0xf0000040))
                {
                    if ((mtype & 0xf0000000) == 0xf0000000)
                    {
                        string introduced31 = now.ToString("d");
                        Console.Error.WriteLine("{0} {1} {2}", introduced31, now.ToString("T"), str);
                        return tag;
                    }
                    Console.Error.WriteLine(str);
                    return tag;
                }
                lock (typeof(ManagedCommonServices))
                {
                    FileStream logFs = null;
                    tag = this.OpenLogFile(objectId, "AMQERR01.LOG", FileMode.OpenOrCreate, ref logFs, true);
                    if (tag == 0)
                    {
                        using (logFs)
                        {
                            FileStream stream2 = logFs;
                            tag = this.SetLogPosition(objectId, ref stream2);
                            if (tag == 0)
                            {
                                try
                                {
                                    using (TextWriter writer = new StreamWriter(logFs, this.resourceTextEncoding))
                                    {
                                        StringWriter writer2 = new StringWriter();
                                        StringWriter writer3 = new StringWriter();
                                        if ((mtype & 0xf0000000) == 0xf0000000)
                                        {
                                            string introduced32 = now.ToString("d");
                                            writer2.Write("{0} {1} -", introduced32, now.ToString("T"));
                                        }
                                        writer2.Write(" Process(" + processId.ToString("D") + "." + this.threadId.ToString("D") + ") User(" + Environment.UserName + ")");
                                        int num7 = (LOG_PAGEWIDTH - 10) - writer2.ToString().Length;
                                        if (num7 > 0)
                                        {
                                            num7 = Math.Min(processName.Length, num7);
                                            writer2.Write(" Program(" + processName.Substring(processName.Length - num7, num7) + ")");
                                        }
                                        num7 = (LOG_PAGEWIDTH - 15) - num4.ToString().Length;
                                        if (num7 > 0)
                                        {
                                            int num8;
                                            if (((num8 = str4.LastIndexOf(@"\")) != -1) || ((num8 = str4.LastIndexOf("/")) != -1))
                                            {
                                                str4 = str4.Substring(num8 + 1);
                                            }
                                            num7 = Math.Min(str4.Length, num7);
                                            writer3.Write("----- {0} : {1} ", str4.Substring(str4.Length - num7, num7), num4.ToString());
                                        }
                                        if (this.extendedPrefix == null)
                                        {
                                            string str5;
                                            string str6;
                                            this.GetMessage(objectId, 0x6160, 7, out this.extendedPrefix, out str5, out str6, 1, 0, 0);
                                            this.GetMessage(objectId, 0x6161, 7, out this.replyPrefix, out str5, out str6, 1, 0, 0);
                                        }
                                        writer.WriteLine(writer2.ToString());
                                        writer.WriteLine(str);
                                        writer.WriteLine(this.extendedPrefix);
                                        writer.Write(str2);
                                        writer.WriteLine(this.replyPrefix);
                                        writer.Write(str3);
                                        writer.WriteLine(writer3.ToString().PadRight(LOG_PAGEWIDTH, '-'));
                                    }
                                }
                                catch (Exception exception2)
                                {
                                    tag = 0x20006119;
                                    this.TraceException(objectId, 0x48, method, exception2);
                                    this.FFST(objectId, "%Z% %W%  %I% %E% %U%", "%C%", 0x48, method, 1, tag, 0);
                                }
                            }
                            if (stream2 != null)
                            {
                                stream2.Flush();
                                stream2.Close();
                            }
                        }
                    }
                    return tag;
                }
            }
            finally
            {
                this.TraceExit(objectId, 0x48, method, (int) tag);
            }
            return tag;
        }

        public override uint FFST(string objectId, string sccsid, string lineno, uint component, string method, uint probeid, uint tag, ushort alert_num)
        {
            uint num = 0x308;
            uint num2 = 0;
            DateTime now = DateTime.Now;
            TimeZone currentTimeZone = TimeZone.CurrentTimeZone;
            string str = currentTimeZone.IsDaylightSavingTime(now) ? currentTimeZone.DaylightName : currentTimeZone.StandardName;
            TimeSpan span = (TimeSpan) (now.ToUniversalTime() - new DateTime(0x7b2, 1, 1));
            double totalSeconds = span.TotalSeconds;
            TimeSpan span2 = (TimeSpan) (now - now.ToUniversalTime());
            int totalMinutes = (int) span2.TotalMinutes;
            this.TraceEntry(objectId, 0x48, num, null);
            try
            {
                if (!ffstIsOn || this.inFFST)
                {
                    return num2;
                }
                uint num7 = this.arithInsert1;
                this.inFFST = true;
                int totalWidth = ManagedTracePoint.CalculateHistoryIndent(this.traceHistory, this.traceHistoryPos, this.traceStackPos);
                this.arithInsert1 = (uint) processId;
                this.DisplayMessage(objectId, null, 0x10006183, 0xf0000004);
                this.arithInsert1 = num7;
                lock (typeof(ManagedCommonServices))
                {
                    if (ffstFileName == null)
                    {
                        ffstFileName = this.GetUniqueFileName(ffstPath, ".FDC");
                    }
                    if (ffstFileName != null)
                    {
                        string str2 = "INCORROUT";
                        int num8 = 0;
                        string str3 = this.threadId.ToString("D8");
                        TextWriter writer = File.AppendText(ffstFileName);
                        if (writer == null)
                        {
                            return num2;
                        }
                        try
                        {
                            try
                            {
                                writer.WriteLine("+-----------------------------------------------------------------------------+");
                                writer.WriteLine(this.GetFFSTHdrLine(string.Empty));
                                writer.WriteLine(this.GetFFSTHdrLine("WebSphere MQ First Failure Symptom Report"));
                                writer.WriteLine(this.GetFFSTHdrLine("".PadRight("WebSphere MQ First Failure Symptom Report".Length, '=')));
                                writer.WriteLine(this.GetFFSTHdrLine(string.Empty));
                                writer.WriteLine(this.GetFFSTHdrLine("Date/Time         :- " + now.ToString(@"ddd MMMM dd HH:mm:ss G\MTzz yyyy")));
                                writer.WriteLine(this.GetFFSTHdrLine("UTC Time          :- " + totalSeconds));
                                writer.WriteLine(this.GetFFSTHdrLine(string.Concat(new object[] { "UTC Time Offset   :- ", totalMinutes, " (", str, ")" })));
                                writer.WriteLine(this.GetFFSTHdrLine("Host Name         :- " + hostName));
                                writer.WriteLine(this.GetFFSTHdrLine("Operating System  :- " + opSys));
                                writer.WriteLine(this.GetFFSTHdrLine("PIDS              :- 5724H7220"));
                                writer.WriteLine(this.GetFFSTHdrLine("LVLS              :- " + vrmf));
                                writer.WriteLine(this.GetFFSTHdrLine("Product Long Name :- " + productName));
                                writer.WriteLine(this.GetFFSTHdrLine("Vendor            :- " + companyName));
                                writer.WriteLine(this.GetFFSTHdrLine("Probe Id          :- " + FUNCID.GetComponentId(component) + method.ToString() + probeid.ToString("D3")));
                                writer.WriteLine(this.GetFFSTHdrLine("Application Name  :- MQM"));
                                writer.WriteLine(this.GetFFSTHdrLine("Component         :- " + method));
                                writer.WriteLine(this.GetFFSTHdrLine("SCCS Info         :- " + base.ExtractSCCSIDInfo(sccsid, "#N #V").TrimEnd(new char[0])));
                                writer.WriteLine(this.GetFFSTHdrLine("Line Number       :- " + lineno));
                                writer.WriteLine(this.GetFFSTHdrLine("Build Date        :- " + buildDate.ToString("MMM d yyy")));
                                writer.WriteLine(this.GetFFSTHdrLine("Build Level       :- " + cmvcLevelName));
                                writer.WriteLine(this.GetFFSTHdrLine("Build Type        :- IKAP - (Production)"));
                                writer.WriteLine(this.GetFFSTHdrLine("UserID            :- " + Environment.UserName));
                                writer.WriteLine(this.GetFFSTHdrLine("Process Name      :- " + processName));
                                writer.WriteLine(this.GetFFSTHdrLine("Addressing Mode   :- " + addrMode));
                                writer.WriteLine(this.GetFFSTHdrLine("Process           :- " + processId.ToString("D8")));
                                writer.WriteLine(this.GetFFSTHdrLine("Thread            :- " + str3));
                                if (tag == 0)
                                {
                                    writer.WriteLine(this.GetFFSTHdrLine("Major Errorcode   :- OK"));
                                    goto Label_052A;
                                }
                                if ((tag == 0x40406109) || (tag == 0x40406110))
                                {
                                    str2 = "HALT";
                                }
                                else
                                {
                                    str2 = "MSGAMQ";
                                }
                                str2 = str2 + ((tag & 0xffff)).ToString("X4");
                                uint num10 = tag & 0xff000000;
                                if (num10 <= 0x10000000)
                                {
                                    switch (num10)
                                    {
                                        case 0:
                                            goto Label_04F5;

                                        case 0x10000000:
                                            goto Label_04FA;
                                    }
                                }
                                else if ((num10 != 0x20000000) && ((num10 == 0x40000000) || (num10 == 0x80000000)))
                                {
                                    goto Label_04FF;
                                }
                                goto Label_0504;
                            Label_04F5:
                                num8 = 4;
                                goto Label_0507;
                            Label_04FA:
                                num8 = 3;
                                goto Label_0507;
                            Label_04FF:
                                num8 = 1;
                                goto Label_0507;
                            Label_0504:
                                num8 = 2;
                            Label_0507:
                                writer.WriteLine(this.GetFFSTHdrLine("Major Errorcode   :- 0x" + tag.ToString("X8")));
                            Label_052A:
                                if (tag != 0x20006118)
                                {
                                    writer.WriteLine(this.GetFFSTHdrLine("Minor Errorcode   :- OK"));
                                }
                                else
                                {
                                    writer.WriteLine(this.GetFFSTHdrLine("Minor Errorcode   :- 0x" + this.arithInsert1.ToString("X8")));
                                }
                                writer.WriteLine(this.GetFFSTHdrLine("Probe Type        :- " + str2));
                                writer.WriteLine(this.GetFFSTHdrLine("Probe Severity    :- " + num8.ToString("D")));
                                writer.WriteLine(this.GetFFSTHdrLine("Probe Description :- "));
                                writer.WriteLine(this.GetFFSTHdrLine("FDCSequenceNumber :- " + ffstSequenceNo.ToString("D")));
                                if (this.arithInsert1 != 0)
                                {
                                    writer.WriteLine(this.GetFFSTHdrLine("Arith1            :- " + this.arithInsert1.ToString("D")));
                                }
                                if (this.arithInsert2 != 0)
                                {
                                    writer.WriteLine(this.GetFFSTHdrLine("Arith2            :- " + this.arithInsert2.ToString("D")));
                                }
                                if ((this.commentInsert1 != null) && (this.commentInsert1 != string.Empty))
                                {
                                    writer.WriteLine(this.GetFFSTHdrLine("Comment1          :- " + this.commentInsert1));
                                }
                                if ((this.commentInsert2 != null) && (this.commentInsert2 != string.Empty))
                                {
                                    writer.WriteLine(this.GetFFSTHdrLine("Comment2          :- " + this.commentInsert2));
                                }
                                if ((this.commentInsert3 != null) && (this.commentInsert3 != string.Empty))
                                {
                                    writer.WriteLine(this.GetFFSTHdrLine("Comment3          :- " + this.commentInsert3));
                                }
                                writer.WriteLine(this.GetFFSTHdrLine(string.Empty));
                                writer.WriteLine("+-----------------------------------------------------------------------------+");
                                int index = 0;
                                writer.WriteLine("MQM Function Stack");
                                try
                                {
                                    do
                                    {
                                        if (this.traceStack[index] != null)
                                        {
                                            writer.WriteLine(this.traceStack[index].Format(ManagedTracePoint.PointType.NameOnly));
                                        }
                                    }
                                    while (++index < this.traceStackPos);
                                }
                                catch
                                {
                                }
                                writer.WriteLine("");
                                index = this.traceHistoryPos;
                                writer.WriteLine("MQM Trace History");
                                try
                                {
                                Label_0761:
                                    if ((index == 250) || (this.traceHistory[index] == null))
                                    {
                                        index = 0;
                                    }
                                    if (this.traceHistory[index] != null)
                                    {
                                        if (this.traceHistory[index].trcType == ManagedTracePoint.PointType.Exit)
                                        {
                                            totalWidth--;
                                        }
                                        writer.WriteLine("-{0}{1}", string.Empty.PadRight(totalWidth, '-'), this.traceHistory[index].Format());
                                        if (this.traceHistory[index].trcType == ManagedTracePoint.PointType.Entry)
                                        {
                                            totalWidth++;
                                        }
                                        if (++index != this.traceHistoryPos)
                                        {
                                            goto Label_0761;
                                        }
                                    }
                                }
                                catch
                                {
                                }
                                writer.WriteLine("");
                                writer.WriteLine("");
                                writer.WriteLine("Component Dumps (Thread {0})", str3);
                                writer.WriteLine("------< MSDNET \".NET\" >-----");
                                try
                                {
                                    writer.WriteLine("OS Version:         " + Environment.OSVersion.ToString());
                                    writer.WriteLine("CLR Version:        " + Environment.Version.ToString());
                                    writer.WriteLine("Working Set:        " + Environment.WorkingSet.ToString() + " bytes");
                                    writer.WriteLine("Shutdown Started:   " + Environment.HasShutdownStarted.ToString());
                                    writer.WriteLine("Thread State:       " + Thread.CurrentThread.ThreadState.ToString());
                                    writer.WriteLine("Thread Priority:    " + Thread.CurrentThread.Priority.ToString());
                                    writer.WriteLine("Apartment State:    " + Thread.CurrentThread.GetApartmentState().ToString());
                                    writer.WriteLine("Pool Thread:        " + Thread.CurrentThread.IsThreadPoolThread.ToString());
                                    writer.WriteLine("Background Thread:  " + Thread.CurrentThread.IsBackground.ToString());
                                    writer.WriteLine("User Domain Name:   " + Environment.UserDomainName);
                                    writer.WriteLine("User Name:          " + Environment.UserName);
                                    writer.WriteLine("Interactive User:   " + Environment.UserInteractive.ToString());
                                    writer.WriteLine("Command Line:       " + Environment.CommandLine);
                                    writer.WriteLine("Current Directory:  " + Environment.CurrentDirectory);
                                    writer.WriteLine("Stack Trace:        " + Environment.NewLine + Environment.StackTrace);
                                }
                                catch
                                {
                                }
                                writer.WriteLine("");
                                writer.WriteLine("");
                                if (ffstSequenceNo == 0)
                                {
                                    writer.WriteLine("Environment Variables");
                                    try
                                    {
                                        SortedList list = new SortedList(Environment.GetEnvironmentVariables());
                                        foreach (DictionaryEntry entry in list)
                                        {
                                            writer.WriteLine("{0}={1}", entry.Key, entry.Value);
                                        }
                                    }
                                    catch
                                    {
                                    }
                                    writer.WriteLine("");
                                    writer.WriteLine("");
                                    writer.WriteLine("Configuration Values");
                                    bool flag = true;
                                    try
                                    {
                                        string propertyValue = null;
                                        foreach (string str5 in FFST_CONFIG_LIST)
                                        {
                                            propertyValue = CommonServices.GetPropertyValue(str5, false);
                                            if (propertyValue != null)
                                            {
                                                flag = false;
                                                writer.WriteLine("{0}={1}", str5, propertyValue);
                                            }
                                        }
                                    }
                                    catch
                                    {
                                    }
                                    if (flag)
                                    {
                                        writer.WriteLine("None");
                                    }
                                    writer.WriteLine("");
                                }
                                return num2;
                            }
                            catch (Exception)
                            {
                            }
                            return num2;
                        }
                        finally
                        {
                            ffstSequenceNo++;
                            try
                            {
                                writer.Flush();
                                writer.Close();
                            }
                            catch
                            {
                            }
                        }
                    }
                    return num2;
                }
            }
            finally
            {
                this.inFFST = false;
                this.TraceExit(objectId, 0x48, num, (int) num2);
            }
            return num2;
        }

        public override uint FFST(string objectId, string sccsid, string lineno, uint component, uint method, uint probeid, uint tag, ushort alert_num)
        {
            uint num = 0x308;
            uint num2 = 0;
            DateTime now = DateTime.Now;
            TimeZone currentTimeZone = TimeZone.CurrentTimeZone;
            string str = currentTimeZone.IsDaylightSavingTime(now) ? currentTimeZone.DaylightName : currentTimeZone.StandardName;
            TimeSpan span = (TimeSpan) (now.ToUniversalTime() - new DateTime(0x7b2, 1, 1));
            double totalSeconds = span.TotalSeconds;
            TimeSpan span2 = (TimeSpan) (now - now.ToUniversalTime());
            int totalMinutes = (int) span2.TotalMinutes;
            this.TraceEntry(objectId, 0x48, num, null);
            try
            {
                if (!ffstIsOn || this.inFFST)
                {
                    return num2;
                }
                uint num7 = this.arithInsert1;
                this.inFFST = true;
                int totalWidth = ManagedTracePoint.CalculateHistoryIndent(this.traceHistory, this.traceHistoryPos, this.traceStackPos);
                this.arithInsert1 = (uint) processId;
                this.DisplayMessage(objectId, null, 0x10006183, 0xf0000004);
                this.arithInsert1 = num7;
                lock (typeof(ManagedCommonServices))
                {
                    if (ffstFileName == null)
                    {
                        ffstFileName = this.GetUniqueFileName(ffstPath, ".FDC");
                    }
                    if (ffstFileName != null)
                    {
                        string str2 = "INCORROUT";
                        int num8 = 0;
                        string str3 = this.threadId.ToString("D8");
                        TextWriter writer = File.AppendText(ffstFileName);
                        if (writer == null)
                        {
                            return num2;
                        }
                        try
                        {
                            try
                            {
                                writer.WriteLine("+-----------------------------------------------------------------------------+");
                                writer.WriteLine(this.GetFFSTHdrLine(string.Empty));
                                writer.WriteLine(this.GetFFSTHdrLine("WebSphere MQ First Failure Symptom Report"));
                                writer.WriteLine(this.GetFFSTHdrLine("".PadRight("WebSphere MQ First Failure Symptom Report".Length, '=')));
                                writer.WriteLine(this.GetFFSTHdrLine(string.Empty));
                                writer.WriteLine(this.GetFFSTHdrLine("Date/Time         :- " + now.ToString(@"ddd MMMM dd HH:mm:ss G\MTzz yyyy")));
                                writer.WriteLine(this.GetFFSTHdrLine("UTC Time          :- " + totalSeconds));
                                writer.WriteLine(this.GetFFSTHdrLine(string.Concat(new object[] { "UTC Time Offset   :- ", totalMinutes, " (", str, ")" })));
                                writer.WriteLine(this.GetFFSTHdrLine("Host Name         :- " + hostName));
                                writer.WriteLine(this.GetFFSTHdrLine("Operating System  :- " + opSys));
                                writer.WriteLine(this.GetFFSTHdrLine("PIDS              :- 5724H7220"));
                                writer.WriteLine(this.GetFFSTHdrLine("LVLS              :- " + vrmf));
                                writer.WriteLine(this.GetFFSTHdrLine("Product Long Name :- " + productName));
                                writer.WriteLine(this.GetFFSTHdrLine("Vendor            :- " + companyName));
                                writer.WriteLine(this.GetFFSTHdrLine("Probe Id          :- " + FUNCID.GetComponentId(component) + method.ToString("D3") + probeid.ToString("D3")));
                                writer.WriteLine(this.GetFFSTHdrLine("Application Name  :- MQM"));
                                writer.WriteLine(this.GetFFSTHdrLine("Component         :- " + FUNCID.GetFunctionName(component, method)));
                                writer.WriteLine(this.GetFFSTHdrLine("SCCS Info         :- " + base.ExtractSCCSIDInfo(sccsid, "#N #V").TrimEnd(new char[0])));
                                writer.WriteLine(this.GetFFSTHdrLine("Line Number       :- " + lineno));
                                writer.WriteLine(this.GetFFSTHdrLine("Build Date        :- " + buildDate.ToString("MMM d yyy")));
                                writer.WriteLine(this.GetFFSTHdrLine("Build Level       :- " + cmvcLevelName));
                                writer.WriteLine(this.GetFFSTHdrLine("Build Type        :- IKAP - (Production)"));
                                writer.WriteLine(this.GetFFSTHdrLine("UserID            :- " + Environment.UserName));
                                writer.WriteLine(this.GetFFSTHdrLine("Process Name      :- " + processName));
                                writer.WriteLine(this.GetFFSTHdrLine("Addressing Mode   :- " + addrMode));
                                writer.WriteLine(this.GetFFSTHdrLine("Process           :- " + processId.ToString("D8")));
                                writer.WriteLine(this.GetFFSTHdrLine("Thread            :- " + str3));
                                if (tag == 0)
                                {
                                    writer.WriteLine(this.GetFFSTHdrLine("Major Errorcode   :- OK"));
                                    goto Label_0536;
                                }
                                if ((tag == 0x40406109) || (tag == 0x40406110))
                                {
                                    str2 = "HALT";
                                }
                                else
                                {
                                    str2 = "MSGAMQ";
                                }
                                str2 = str2 + ((tag & 0xffff)).ToString("X4");
                                uint num10 = tag & 0xff000000;
                                if (num10 <= 0x10000000)
                                {
                                    switch (num10)
                                    {
                                        case 0:
                                            goto Label_0501;

                                        case 0x10000000:
                                            goto Label_0506;
                                    }
                                }
                                else if ((num10 != 0x20000000) && ((num10 == 0x40000000) || (num10 == 0x80000000)))
                                {
                                    goto Label_050B;
                                }
                                goto Label_0510;
                            Label_0501:
                                num8 = 4;
                                goto Label_0513;
                            Label_0506:
                                num8 = 3;
                                goto Label_0513;
                            Label_050B:
                                num8 = 1;
                                goto Label_0513;
                            Label_0510:
                                num8 = 2;
                            Label_0513:
                                writer.WriteLine(this.GetFFSTHdrLine("Major Errorcode   :- 0x" + tag.ToString("X8")));
                            Label_0536:
                                if (tag != 0x20006118)
                                {
                                    writer.WriteLine(this.GetFFSTHdrLine("Minor Errorcode   :- OK"));
                                }
                                else
                                {
                                    writer.WriteLine(this.GetFFSTHdrLine("Minor Errorcode   :- 0x" + this.arithInsert1.ToString("X8")));
                                }
                                writer.WriteLine(this.GetFFSTHdrLine("Probe Type        :- " + str2));
                                writer.WriteLine(this.GetFFSTHdrLine("Probe Severity    :- " + num8.ToString("D")));
                                writer.WriteLine(this.GetFFSTHdrLine("Probe Description :- "));
                                writer.WriteLine(this.GetFFSTHdrLine("FDCSequenceNumber :- " + ffstSequenceNo.ToString("D")));
                                if (this.arithInsert1 != 0)
                                {
                                    writer.WriteLine(this.GetFFSTHdrLine("Arith1            :- " + this.arithInsert1.ToString("D")));
                                }
                                if (this.arithInsert2 != 0)
                                {
                                    writer.WriteLine(this.GetFFSTHdrLine("Arith2            :- " + this.arithInsert2.ToString("D")));
                                }
                                if ((this.commentInsert1 != null) && (this.commentInsert1 != string.Empty))
                                {
                                    writer.WriteLine(this.GetFFSTHdrLine("Comment1          :- " + this.commentInsert1));
                                }
                                if ((this.commentInsert2 != null) && (this.commentInsert2 != string.Empty))
                                {
                                    writer.WriteLine(this.GetFFSTHdrLine("Comment2          :- " + this.commentInsert2));
                                }
                                if ((this.commentInsert3 != null) && (this.commentInsert3 != string.Empty))
                                {
                                    writer.WriteLine(this.GetFFSTHdrLine("Comment3          :- " + this.commentInsert3));
                                }
                                writer.WriteLine(this.GetFFSTHdrLine(string.Empty));
                                writer.WriteLine("+-----------------------------------------------------------------------------+");
                                int index = 0;
                                writer.WriteLine("MQM Function Stack");
                                try
                                {
                                    do
                                    {
                                        if (this.traceStack[index] != null)
                                        {
                                            writer.WriteLine(this.traceStack[index].Format(ManagedTracePoint.PointType.NameOnly));
                                        }
                                    }
                                    while (++index < this.traceStackPos);
                                }
                                catch
                                {
                                }
                                writer.WriteLine("");
                                index = this.traceHistoryPos;
                                writer.WriteLine("MQM Trace History");
                                try
                                {
                                Label_076D:
                                    if ((index == 250) || (this.traceHistory[index] == null))
                                    {
                                        index = 0;
                                    }
                                    if (this.traceHistory[index] != null)
                                    {
                                        if (this.traceHistory[index].trcType == ManagedTracePoint.PointType.Exit)
                                        {
                                            totalWidth--;
                                        }
                                        writer.WriteLine("-{0}{1}", string.Empty.PadRight(totalWidth, '-'), this.traceHistory[index].Format());
                                        if (this.traceHistory[index].trcType == ManagedTracePoint.PointType.Entry)
                                        {
                                            totalWidth++;
                                        }
                                        if (++index != this.traceHistoryPos)
                                        {
                                            goto Label_076D;
                                        }
                                    }
                                }
                                catch
                                {
                                }
                                writer.WriteLine("");
                                writer.WriteLine("");
                                writer.WriteLine("Component Dumps (Thread {0})", str3);
                                writer.WriteLine("------< MSDNET \".NET\" >-----");
                                try
                                {
                                    writer.WriteLine("OS Version:         " + Environment.OSVersion.ToString());
                                    writer.WriteLine("CLR Version:        " + Environment.Version.ToString());
                                    writer.WriteLine("Working Set:        " + Environment.WorkingSet.ToString() + " bytes");
                                    writer.WriteLine("Shutdown Started:   " + Environment.HasShutdownStarted.ToString());
                                    writer.WriteLine("Thread State:       " + Thread.CurrentThread.ThreadState.ToString());
                                    writer.WriteLine("Thread Priority:    " + Thread.CurrentThread.Priority.ToString());
                                    writer.WriteLine("Apartment State:    " + Thread.CurrentThread.GetApartmentState().ToString());
                                    writer.WriteLine("Pool Thread:        " + Thread.CurrentThread.IsThreadPoolThread.ToString());
                                    writer.WriteLine("Background Thread:  " + Thread.CurrentThread.IsBackground.ToString());
                                    writer.WriteLine("User Domain Name:   " + Environment.UserDomainName);
                                    writer.WriteLine("User Name:          " + Environment.UserName);
                                    writer.WriteLine("Interactive User:   " + Environment.UserInteractive.ToString());
                                    writer.WriteLine("Command Line:       " + Environment.CommandLine);
                                    writer.WriteLine("Current Directory:  " + Environment.CurrentDirectory);
                                    writer.WriteLine("Stack Trace:        " + Environment.NewLine + Environment.StackTrace);
                                }
                                catch
                                {
                                }
                                writer.WriteLine("");
                                writer.WriteLine("");
                                if (ffstSequenceNo == 0)
                                {
                                    writer.WriteLine("Environment Variables");
                                    try
                                    {
                                        SortedList list = new SortedList(Environment.GetEnvironmentVariables());
                                        foreach (DictionaryEntry entry in list)
                                        {
                                            writer.WriteLine("{0}={1}", entry.Key, entry.Value);
                                        }
                                    }
                                    catch
                                    {
                                    }
                                    writer.WriteLine("");
                                    writer.WriteLine("");
                                    writer.WriteLine("Configuration Values");
                                    bool flag = true;
                                    try
                                    {
                                        string propertyValue = null;
                                        foreach (string str5 in FFST_CONFIG_LIST)
                                        {
                                            propertyValue = CommonServices.GetPropertyValue(str5, false);
                                            if (propertyValue != null)
                                            {
                                                flag = false;
                                                writer.WriteLine("{0}={1}", str5, propertyValue);
                                            }
                                        }
                                    }
                                    catch
                                    {
                                    }
                                    if (flag)
                                    {
                                        writer.WriteLine("None");
                                    }
                                    writer.WriteLine("");
                                }
                                return num2;
                            }
                            catch (Exception)
                            {
                            }
                            return num2;
                        }
                        finally
                        {
                            ffstSequenceNo++;
                            try
                            {
                                writer.Flush();
                                writer.Close();
                            }
                            catch
                            {
                            }
                        }
                    }
                    return num2;
                }
            }
            finally
            {
                this.inFFST = false;
                this.TraceExit(objectId, 0x48, num, (int) num2);
            }
            return num2;
        }

        private uint FileAddACE(string filePath, string trusteeName, uint accessMask, uint accessFlags, uint accessType)
        {
            uint method = 0x30e;
            uint num2 = 0;
            this.TraceEntry(string.Empty, 0x48, method, null);
            this.TraceText("FilePath : '" + filePath + "'");
            try
            {
                ManagementBaseObject obj2 = null;
                ManagementBaseObject[] objArray = null;
                ManagementPath path = new ManagementPath();
                path.Server = ".";
                path.NamespacePath = @"root\cimv2";
                path.RelativePath = "Win32_LogicalFileSecuritySetting.Path='" + filePath + "'";
                ManagementObject obj3 = new ManagementObject(path);
                ManagementBaseObject inParameters = obj3.InvokeMethod("GetSecurityDescriptor", null, null);
                num2 = Convert.ToUInt32(inParameters.Properties["ReturnValue"].Value);
                if (num2 != 0)
                {
                    return num2;
                }
                obj2 = (ManagementBaseObject) inParameters.Properties["Descriptor"].Value;
                ArrayList list = new ArrayList();
                try
                {
                    objArray = (ManagementBaseObject[]) inParameters.Properties["Dacl"].Value;
                    foreach (ManagementBaseObject obj5 in objArray)
                    {
                        list.Add(obj5);
                    }
                }
                catch
                {
                }
                Array array = Array.CreateInstance(typeof(ManagementBaseObject), (int) (list.Count + 1));
                list.CopyTo(array);
                ManagementBaseObject obj6 = new ManagementClass("Win32_Trustee");
                obj6.Properties["Name"].Value = trusteeName;
                ManagementBaseObject obj7 = new ManagementClass("Win32_ACE");
                obj7.Properties["AccessMask"].Value = accessMask;
                obj7.Properties["AceFlags"].Value = accessFlags;
                obj7.Properties["AceType"].Value = accessType;
                obj7.Properties["Trustee"].Value = obj6;
                array.SetValue(obj7, (int) (array.Length - 1));
                inParameters = obj3.GetMethodParameters("SetSecurityDescriptor");
                obj2.Properties["Dacl"].Value = (ManagementBaseObject[]) array;
                inParameters["Descriptor"] = obj2;
                return Convert.ToUInt32(obj3.InvokeMethod("SetSecurityDescriptor", inParameters, null).Properties["ReturnValue"].Value);
            }
            catch (Exception exception)
            {
                this.TraceException(string.Empty, 0x48, method, exception);
                num2 = 0x20006119;
            }
            finally
            {
                this.TraceExit(string.Empty, 0x48, method, (int) num2);
            }
            return num2;
        }

        public override uint GetArithInsert1()
        {
            return this.arithInsert1;
        }

        public override uint GetArithInsert2()
        {
            return this.arithInsert2;
        }

        public override string GetCommentInsert1()
        {
            return this.commentInsert1;
        }

        public override string GetCommentInsert2()
        {
            return this.commentInsert2;
        }

        public override string GetCommentInsert3()
        {
            return this.commentInsert3;
        }

        public override string GetDataLib()
        {
            return workPath;
        }

        private Encoding GetEncoding(int ccsid)
        {
            switch (ccsid)
            {
                case 420:
                case 0x1a7:
                case 0x1a8:
                case 0x129:
                case 0x115:
                case 0x116:
                case 280:
                case 0x11c:
                case 0x11d:
                case 290:
                case 0x111:
                case 0x346:
                case 0x367:
                case 880:
                case 0x389:
                case 0x401:
                    ccsid += 0x4e20;
                    break;

                case 0x32d:
                    ccsid = 0x6fb5;
                    break;

                case 0x333:
                    ccsid = 0x6faf;
                    break;

                case 0x390:
                    ccsid = 0x6fb0;
                    break;

                case 0x391:
                    ccsid = 0x6fb1;
                    break;

                case 0x393:
                    ccsid = 0x6fb3;
                    break;

                case 0x394:
                    ccsid = 0x6fb6;
                    break;

                case 920:
                    ccsid = 0x6fb7;
                    break;

                case 0x399:
                    ccsid = 0x6fbb;
                    break;

                case 0x39b:
                    ccsid = 0x6fbd;
                    break;

                case 0x3af:
                    ccsid = 0x3a4;
                    break;

                case 0x36e:
                    ccsid = 0x5182;
                    break;

                case 0x3ba:
                case 0x83ba:
                case 0x13ba:
                    ccsid = 0xcadc;
                    break;

                case 970:
                    ccsid = 0xcaed;
                    break;

                case 0x4d0:
                    ccsid = 0xfdee;
                    break;

                case 0x4d2:
                    ccsid = 0xfded;
                    break;

                case 0x4b8:
                    ccsid = 0xfde9;
                    break;

                case 0x441:
                    ccsid = 0x6fb4;
                    break;

                case 0x4b2:
                    ccsid = 0x4b0;
                    break;

                case 0x565:
                case 0x567:
                case 0x56a:
                case 0x1570:
                    ccsid = 0xd698;
                    break;

                case 0x7e6:
                    ccsid = 0xc42c;
                    break;

                case 0x15e1:
                    ccsid = 0x3b5;
                    break;
            }
            return Encoding.GetEncoding(ccsid);
        }

        internal override string GetEnvironmentValue(string name)
        {
            string str = null;
            if (name == null)
            {
                return str;
            }
            if (PseudoEnvironmentVariables.ContainsKey(name))
            {
                return (string) PseudoEnvironmentVariables[name];
            }
            return Environment.GetEnvironmentVariable(name);
        }

        private string GetFFSTHdrLine(string lineContent)
        {
            string str = string.Empty;
            bool flag = true;
            do
            {
                int num;
                if (flag)
                {
                    num = 0x4b;
                }
                else
                {
                    num = 0x36;
                }
                int length = Math.Min(lineContent.Length, num);
                str = str + "| ";
                if (!flag)
                {
                    str = str + "                     ";
                }
                str = str + lineContent.Substring(0, length).PadRight(num) + " |";
                lineContent = lineContent.Substring(length);
                if (lineContent.Length != 0)
                {
                    str = str + Environment.NewLine;
                }
                flag = false;
            }
            while (lineContent.Length > 0);
            return str;
        }

        public override string GetFunctionName(uint component, uint method)
        {
            return FUNCID.GetFunctionName(component, method);
        }

        public override uint GetMessage(string objectId, uint returncode, uint control, out string basicmessage, out string extendedmessage, out string replymessage, int basicLength, int extendedLength, int replyLength)
        {
            uint method = 780;
            uint num2 = 0;
            uint num3 = returncode;
            this.TraceEntry(objectId, 0x48, method, null);
            this.TraceText("Returncode: 0x" + returncode.ToString("X8") + " Control: 0x" + control.ToString("X8"));
            try
            {
                basicmessage = string.Empty;
                extendedmessage = string.Empty;
                replymessage = string.Empty;
                if ((control & 3) <= 1)
                {
                    return 0x20806128;
                }
                this.commentInsert4 = returncode.ToString("X8");
            Label_007D:
                if ((num3 & 0x800000) == 0x800000)
                {
                    num3 = 0x6090;
                    num2 = 0x10806132;
                }
                string str = "AMQ" + ((num3 & 0xffff)).ToString("X4");
                try
                {
                    string format = null;
                    this.AccessResources(Assembly.Load("amqmdmsg"));
                    if (basicLength != 0)
                    {
                        if ((control & 3) == 2)
                        {
                            basicmessage = str + ": ";
                        }
                        else
                        {
                            basicmessage = string.Empty;
                        }
                        if (((format = this.resources.GetString(str + ".MSG")) == null) && ((format = defaultResources.GetString(str + ".MSG")) == null))
                        {
                            throw new Exception();
                        }
                        basicmessage = basicmessage + format;
                        basicmessage = this.SubstituteInserts(basicmessage, control);
                    }
                    if (extendedLength != 0)
                    {
                        if (((format = this.resources.GetString(str + ".XPL")) == null) && ((format = defaultResources.GetString(str + ".XPL")) == null))
                        {
                            throw new Exception();
                        }
                        extendedmessage = this.SubstituteInserts(format, control);
                    }
                    if (replyLength != 0)
                    {
                        if (((format = this.resources.GetString(str + ".URESP")) == null) && ((format = defaultResources.GetString(str + ".URESP")) == null))
                        {
                            throw new Exception();
                        }
                        replymessage = this.SubstituteInserts(format, control);
                    }
                    return num2;
                }
                catch (Exception)
                {
                    if (num3 == 0x6090)
                    {
                        return 0x20006118;
                    }
                    num3 = 0x800000;
                    goto Label_007D;
                }
            }
            finally
            {
                this.TraceExit(objectId, 0x48, method, (int) num2);
            }
            return num2;
        }

        public override int GetMQCSDApplied()
        {
            return CommonServices.GetMQCSDApplied();
        }

        public override int GetMQRelease()
        {
            return CommonServices.csMQRelease;
        }

        public override int GetMQVersion()
        {
            return CommonServices.csMQVersion;
        }

        public string GetMQVRMF()
        {
            return CommonServices.GetVRMF();
        }

        public override string GetProgramName()
        {
            return processName;
        }

        private string GetUniqueFileName(string path, string extension)
        {
            string str = null;
            try
            {
                int num = -1;
                do
                {
                    num++;
                    str = string.Concat(new object[] { path, "AMQ", processId, ".", num, extension });
                }
                while (File.Exists(str));
            }
            catch (Exception)
            {
            }
            return str;
        }

        private bool Initialize()
        {
            this.threadId = Thread.CurrentThread.ManagedThreadId;
            this.idString = (processId.ToString("D") + "." + this.threadId.ToString("D")).PadRight(9);
            this.traceStack = new ManagedTracePoint[70];
            this.traceStackPos = 0x13;
            this.traceHistory = new ManagedTracePoint[250];
            this.traceHistoryPos = 0;
            return true;
        }

        private static bool MQTraceEnabled()
        {
            bool flag = false;
            try
            {
                FileInfo info = new FileInfo(traceShm);
                if (info.LastWriteTimeUtc != traceLastWriteTimeUtc)
                {
                    traceLastWriteTimeUtc = info.LastWriteTimeUtc;
                    FileStream input = new FileStream(traceShm, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    BinaryReader reader = new BinaryReader(input);
                    traceControlMem.StrucId = reader.ReadBytes(4);
                    traceControlMem.memSize = reader.ReadUInt32();
                    traceControlMem.initTime = reader.ReadUInt64();
                    traceControlMem.initialised = reader.ReadInt32();
                    traceControlMem.used_slots = reader.ReadUInt32();
                    traceControlMem.max_slots = reader.ReadUInt32();
                    traceControlMem.MasterSeq = reader.ReadUInt32();
                    traceControlMem.common_args.EarlyTrace = reader.ReadInt32();
                    traceControlMem.common_args.BinaryTrace = reader.ReadInt32();
                    traceControlMem.common_args.TimedTrace = reader.ReadInt32();
                    traceControlMem.common_args.TraceMaxFileSize = reader.ReadUInt32();
                    traceControlMem.common_args.TraceUserDataSize = reader.ReadInt32();
                    traceControlMem.common_args.TraceOutputType = reader.ReadUInt32();
                    traceControlMem.common_args.TracePath = reader.ReadBytes(0x100);
                    traceControlMem.common_args.zerodelim = reader.ReadUInt32();
                    traceControlMem.common_args.RebootPersistent = reader.ReadInt32();
                    reader.Close();
                }
            }
            catch
            {
            }
            if ((traceControlMem.used_slots > 0) | (traceControlMem.common_args.EarlyTrace != 0))
            {
                flag = true;
            }
            return flag;
        }

        private uint OpenLogFile(string objectId, string filename, FileMode fmode, ref FileStream logFs, bool lockFile)
        {
            uint method = 0x309;
            int num2 = 0;
            uint tag = 0;
            this.TraceEntry(objectId, 0x48, method, null);
            try
            {
                logFs = null;
                do
                {
                    try
                    {
                        if (logFs != null)
                        {
                            goto Label_0070;
                        }
                        FileMode open = fmode;
                        if (fmode == FileMode.OpenOrCreate)
                        {
                            open = FileMode.Open;
                        }
                    Label_0027:
                        try
                        {
                            logFs = new FileStream(logPath + filename, open, FileAccess.ReadWrite, FileShare.ReadWrite);
                            if (open == FileMode.Create)
                            {
                                this.FileAddACE(logPath + filename, "Everyone", 0x1f01ff, 3, 0);
                            }
                        }
                        catch (FileNotFoundException)
                        {
                            if ((fmode != FileMode.OpenOrCreate) || (open != FileMode.Open))
                            {
                                throw;
                            }
                            open = FileMode.Create;
                            goto Label_0027;
                        }
                    Label_0070:
                        if (lockFile && (logFs != null))
                        {
                            logFs.Lock(0L, logFs.Length);
                        }
                        return 0;
                    }
                    catch (UnauthorizedAccessException exception)
                    {
                        tag = 0x20806021;
                        this.TraceException(objectId, 0x48, method, exception);
                        return tag;
                    }
                    catch (IOException exception2)
                    {
                        tag = 0x20006119;
                        if (++num2 >= 100)
                        {
                            this.TraceException(objectId, 0x48, method, exception2);
                            this.FFST(objectId, "%Z% %W%  %I% %E% %U%", "%C%", 0x48, method, 1, tag, 0);
                            return tag;
                        }
                        Thread.Sleep(100);
                    }
                    catch (Exception exception3)
                    {
                        tag = 0x20006119;
                        this.TraceException(objectId, 0x48, method, exception3);
                        this.FFST(objectId, "%Z% %W%  %I% %E% %U%", "%C%", 0x48, method, 2, tag, 0);
                        return tag;
                    }
                }
                while (tag != 0);
            }
            finally
            {
                this.TraceExit(objectId, 0x48, method, (int) tag);
                if ((tag != 0) && (logFs != null))
                {
                    try
                    {
                        logFs.Close();
                        logFs = null;
                    }
                    catch
                    {
                    }
                }
            }
            return tag;
        }

        private bool OpenTraceFile()
        {
            lock (typeof(ManagedCommonServices))
            {
                bool flag = false;
                if (traceFileName == null)
                {
                    flag = true;
                    traceFileName = this.GetUniqueFileName(tracePath, ".TRC");
                }
                if ((traceFileName != null) && !CommonServices.IsXmsInUse())
                {
                    if (traceWriter == null)
                    {
                        traceWriter = TextWriter.Synchronized(File.AppendText(traceFileName));
                    }
                    if (flag && (traceWriter != null))
                    {
                        DateTime now = DateTime.Now;
                        traceWriter.WriteLine("Process : {0} ({1})\r\nHost : {2}\r\nOperating System : {3}\r\nProduct Long Name : {4}\r\nVersion : {5}    Level : {6}\r\nDate : {7}  Time : {8}\r\n\r\nCounter  TimeStamp          PID.TID   Data\r\n============================================================", new object[] { processName, addrMode, hostName, opSys, productName, vrmf, cmvcLevelName, now.ToString("dd/MM/yyyy"), now.ToString("HH:mm:ss.ffffff") });
                        CommonServices.TraceEnvironment();
                    }
                }
            }
            return (traceWriter != null);
        }

        public override string ReasonCodeName(int reason)
        {
            return reason.ToString();
        }

        public override void SetArithInsert1(uint insert)
        {
            this.arithInsert1 = insert;
        }

        public override void SetArithInsert2(uint insert)
        {
            this.arithInsert2 = insert;
        }

        public override void SetCommentInsert1(string insert)
        {
            this.commentInsert1 = insert;
        }

        public override void SetCommentInsert2(string insert)
        {
            this.commentInsert2 = insert;
        }

        public override void SetCommentInsert3(string insert)
        {
            this.commentInsert3 = insert;
        }

        internal override bool SetEnvironmentValue(string name, string value)
        {
            bool flag = false;
            if (name != null)
            {
                if (value == string.Empty)
                {
                    value = null;
                }
                PseudoEnvironmentVariables[name] = value;
                if (this.GetEnvironmentValue(name) == value)
                {
                    flag = true;
                }
            }
            return flag;
        }

        private uint SetLogPosition(string objectId, ref FileStream logFs)
        {
            uint method = 0x30a;
            uint tag = 0;
            this.TraceEntry(objectId, 0x48, method, null);
            try
            {
                if (logFs.Length > logRolloverSize)
                {
                    FileStream stream = null;
                    for (int i = 2; i > 1; i--)
                    {
                        string path = logPath + "AMQERR" + i.ToString("D2") + ".LOG";
                        if (File.Exists(path))
                        {
                            string str2 = logPath + "AMQERR" + ((i + 1)).ToString("D2") + ".LOG";
                            try
                            {
                                if (File.Exists(str2))
                                {
                                    File.Delete(str2);
                                }
                                File.Move(path, str2);
                            }
                            catch (Exception exception)
                            {
                                tag = 0x20006119;
                                this.TraceException(objectId, 0x48, method, exception);
                                this.FFST(objectId, "%Z% %W%  %I% %E% %U%", "%C%", 0x48, method, 1, tag, 0);
                                return tag;
                            }
                        }
                    }
                    tag = this.OpenLogFile(objectId, "AMQERR02.LOG", FileMode.Create, ref stream, false);
                    if (tag == 0)
                    {
                        byte[] buffer = new byte[Math.Min(logFs.Length, (long) XEE_COPY_BLOCK_SIZE)];
                        FileStream stream2 = stream;
                        try
                        {
                            int count = 0;
                            while ((count = logFs.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                stream.Write(buffer, 0, count);
                            }
                        }
                        catch (Exception exception2)
                        {
                            tag = 0x20006119;
                            this.TraceException(objectId, 0x48, method, exception2);
                            this.FFST(objectId, "%Z% %W%  %I% %E% %U%", "%C%", 0x48, method, 2, tag, 0);
                            return tag;
                        }
                        finally
                        {
                            if (stream2 != null)
                            {
                                stream2.Dispose();
                            }
                        }
                    }
                    logFs.SetLength(0L);
                }
                logFs.Seek(0L, SeekOrigin.End);
            }
            finally
            {
                this.TraceExit(objectId, 0x48, method, (int) tag);
            }
            return tag;
        }

        public override void SetValidInserts()
        {
            this.arithInsert1 = 0;
            this.arithInsert2 = 0;
            this.commentInsert1 = string.Empty;
            this.commentInsert2 = string.Empty;
            this.commentInsert3 = string.Empty;
            this.commentInsert4 = string.Empty;
        }

        private string SubstituteInserts(string format, uint control)
        {
            string str = string.Empty;
            if (format != null)
            {
                string str2 = null;
                StringWriter writer = new StringWriter();
                writer.Write(format, new object[] { this.arithInsert1.ToString("D"), this.arithInsert2.ToString("D"), (this.commentInsert1 == null) ? string.Empty : this.commentInsert1, (this.commentInsert2 == null) ? string.Empty : this.commentInsert2, (this.commentInsert3 == null) ? string.Empty : this.commentInsert3, this.commentInsert4, this.arithInsert1.ToString("X"), this.arithInsert2.ToString("X") });
                str2 = writer.ToString();
                if ((control & 8) != 0)
                {
                    str = string.Empty;
                    while (str2.Length > LOG_PAGEWIDTH)
                    {
                        int index = str2.IndexOf(Environment.NewLine);
                        int num2 = 0;
                        if ((index == -1) || (index > LOG_PAGEWIDTH))
                        {
                            index = Math.Min(LOG_PAGEWIDTH, str2.Length - 1);
                            int num3 = str2.LastIndexOf(' ', index);
                            if (num3 != -1)
                            {
                                index = num3;
                                num2 = 1;
                            }
                        }
                        else
                        {
                            num2 = 2;
                        }
                        str = str + str2.Substring(0, index) + Environment.NewLine;
                        str2 = str2.Substring(index + num2);
                    }
                    if (str2.Length > 0)
                    {
                        str = str + str2 + Environment.NewLine;
                    }
                }
                else
                {
                    str = str2;
                }
                str = str.TrimEnd(MSG_TRIM_CHARS);
                if ((control & 4) == 0)
                {
                    str = str + Environment.NewLine;
                }
            }
            return str;
        }

        public override void TraceConstructor(string objectId, string sccsid)
        {
            if (this.TraceStatus())
            {
                this.TraceDetail(objectId, 0, 0, 0, "Constructing " + objectId + base.ExtractSCCSIDInfo(sccsid, " #N #L #V #D #T").TrimEnd(new char[0]));
            }
        }

        public override void TraceData(string objectId, uint component, uint method, ushort level, string caption, uint offset, int length, byte[] buf)
        {
            try
            {
                if ((traceLevel >= level) && this.TraceStatus((Interlocked.Increment(ref counter) % 0x100) == 0))
                {
                    int index = (int) offset;
                    int num2 = 0;
                    char[] chArray = null;
                    string str = string.Empty;
                    string str2 = string.Empty;
                    int num3 = 0;
                    int num4 = 0;
                    if (length == -1)
                    {
                        num2 = buf.Length;
                    }
                    else
                    {
                        num2 = index + length;
                        if (num2 > buf.Length)
                        {
                            num2 = buf.Length;
                        }
                    }
                    if (index != num2)
                    {
                        StringWriter writer = new StringWriter();
                        writer.Write("{0} {1}   {2}  ", counter.ToString("X8"), DateTime.Now.ToString("HH:mm:ss.ffffff"), this.idString);
                        if (caption != null)
                        {
                            traceWriter.WriteLine("{0} !! - {1}:\r\n{0} Data:- {2}", writer, caption, objectId);
                        }
                        chArray = Encoding.ASCII.GetChars(buf, index, num2 - index);
                        while (index < num2)
                        {
                            int num5 = index;
                            int num6 = Math.Min(num5 + 0x10, num2);
                            string str3 = string.Empty;
                            string str4 = string.Empty;
                            while (index < num6)
                            {
                                byte num7 = buf[index];
                                str3 = str3 + num7.ToString("X2") + " ";
                                switch (num7)
                                {
                                    case 0:
                                    case 10:
                                    case 13:
                                        str4 = str4 + '.';
                                        break;

                                    default:
                                        str4 = str4 + chArray[(int) ((IntPtr) (index - offset))];
                                        break;
                                }
                                index++;
                            }
                            if ((str3 == str2) && (index != num2))
                            {
                                num3++;
                                num4 = num5;
                                continue;
                            }
                            switch (num3)
                            {
                                case 0:
                                    break;

                                case 1:
                                    traceWriter.WriteLine("{0}  0x{1} {2}: {3}", new object[] { writer, num4.ToString("X8"), str2.PadRight(0x30), str });
                                    break;

                                default:
                                    traceWriter.WriteLine("{0}  {1} lines suppressed, same as above", writer, num3.ToString());
                                    break;
                            }
                            num3 = 0;
                            traceWriter.WriteLine("{0}  0x{1} {2}: {3}", new object[] { writer, num5.ToString("X8"), str3.PadRight(0x30), str4 });
                            str2 = str3;
                            str = str4;
                        }
                        traceWriter.Flush();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public override void TraceDetail(string objectId, uint component, uint method, ushort level, string text)
        {
            try
            {
                if ((traceLevel >= level) && this.TraceStatus((Interlocked.Increment(ref counter) % 0x100) == 0))
                {
                    traceWriter.WriteLine("{0} {1}   {2}   {3}", new object[] { counter.ToString("X8"), DateTime.Now.ToString("HH:mm:ss.ffffff"), this.idString, text });
                    traceWriter.Flush();
                }
            }
            catch (Exception)
            {
            }
        }

        public override bool TraceEnabled()
        {
            return traceIsOn;
        }

        public override void TraceEntry(string objectId, uint component, uint method, object[] parameters)
        {
            if (this.traceStackPos < 50)
            {
                if (this.traceStack[this.traceStackPos] == null)
                {
                    this.traceStack[this.traceStackPos++] = new ManagedTracePoint(component, method, parameters);
                }
                else
                {
                    this.traceStack[this.traceStackPos++].SetEntry(component, method, parameters);
                }
            }
            if (this.traceHistoryPos == 250)
            {
                this.traceHistoryPos = 0;
            }
            if (this.traceHistory[this.traceHistoryPos] == null)
            {
                this.traceHistory[this.traceHistoryPos++] = new ManagedTracePoint(component, method, parameters);
            }
            else
            {
                this.traceHistory[this.traceHistoryPos++].SetEntry(component, method, parameters);
            }
            try
            {
                if (this.TraceStatus((Interlocked.Increment(ref counter) % 0x100) == 0))
                {
                    traceWriter.WriteLine("{0} {1}   {2}  {3}{4}", new object[] { counter.ToString("X8"), DateTime.Now.ToString("HH:mm:ss.ffffff"), this.idString, this.indent, this.traceHistory[this.traceHistoryPos - 1].Format() });
                    traceWriter.Flush();
                }
            }
            catch
            {
            }
            finally
            {
                this.indent = this.indent + "-";
            }
        }

        public override void TraceEnvironment()
        {
            this.TraceText("CommonServices:     " + base.GetType().ToString());
        }

        public override void TraceException(string objectId, uint component, uint method, Exception ex)
        {
            this.TraceDetail(objectId, component, method, 0, ex.ToString());
        }

        public void TraceExit(string objectId, uint component, uint method, int returnCode)
        {
            this.TraceExit(objectId, component, method, null, 0, returnCode);
        }

        public override void TraceExit(string objectId, uint component, uint method, object result, int index, int returnCode)
        {
            if (this.traceStackPos > 0)
            {
                this.traceStackPos--;
            }
            if (this.traceHistoryPos == 250)
            {
                this.traceHistoryPos = 0;
            }
            if (this.traceHistory[this.traceHistoryPos] == null)
            {
                this.traceHistory[this.traceHistoryPos++] = new ManagedTracePoint(component, method, result, returnCode);
            }
            else
            {
                this.traceHistory[this.traceHistoryPos++].SetExit(component, method, result, returnCode);
            }
            try
            {
                this.indent = this.indent.Remove(0, 1);
                if (this.TraceStatus((Interlocked.Increment(ref counter) % 0x100) == 0))
                {
                    traceWriter.WriteLine("{0} {1}   {2}  {3}{4}", new object[] { counter.ToString("X8"), DateTime.Now.ToString("HH:mm:ss.ffffff"), this.idString, this.indent, this.traceHistory[this.traceHistoryPos - 1].Format() });
                    traceWriter.Flush();
                }
            }
            catch (Exception)
            {
            }
        }

        public override bool TraceStatus()
        {
            return this.TraceStatus(false);
        }

        private bool TraceStatus(bool checkTrace)
        {
            if (checkTrace && !defTraceIsOn)
            {
                bool flag = MQTraceEnabled();
                if (flag != MQTraceWasEnabled)
                {
                    lock (typeof(ManagedCommonServices))
                    {
                        if (flag != MQTraceWasEnabled)
                        {
                            try
                            {
                                if (flag)
                                {
                                    traceLevel = 0;
                                    traceUserDataSize = traceControlMem.common_args.TraceUserDataSize;
                                    traceUserDataSize = Math.Max(traceUserDataSize, -1);
                                }
                            }
                            catch
                            {
                            }
                            finally
                            {
                                traceIsOn = MQTraceWasEnabled = flag;
                            }
                        }
                    }
                }
            }
            if (traceIsOn)
            {
                if ((traceWriter == null) && !this.OpenTraceFile())
                {
                    return false;
                }
            }
            else if (traceWriter != null)
            {
                traceWriter.Flush();
                traceWriter.Close();
                traceWriter = null;
            }
            return traceIsOn;
        }

        protected void TraceText(string text)
        {
            this.TraceDetail(string.Empty, 0, 0, 0, text);
        }

        public override void TraceUserData(string objectId, uint component, uint method, ushort level, string caption, uint offset, int length, byte[] buf)
        {
            if (traceUserDataSize != 0)
            {
                if (traceUserDataSize > 0)
                {
                    length = Math.Min(traceUserDataSize, (length == -1) ? buf.Length : length);
                }
                this.TraceData(objectId, component, method, level, caption, offset, length, buf);
            }
        }
    }
}

