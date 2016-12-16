namespace IBM.WMQ
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;

    public class MQCommonServices : CommonServicesInterface
    {
        private const int CODESET_UCS = 0x4b0;
        private unsafe void* dumpPtr = null;
        private const int FAKE_TRACE_STATUS_VALUE = -1;
        private static readonly object initialiseLock = new object();
        private xcsINSERTS inserts;
        private static xcsStdCallDump_t msdnetDump = new xcsStdCallDump_t(MQCommonServices.FmtDumpCallback);
        private const string NATIVE_LOADER_CLASS = "IBM.WMQ.Nmqi.NativeManager";
        private object nativeManager;
        private static System.Type nativeManagerType = null;
        protected static string processPath = null;
        private unsafe static int* pTraceStatus = null;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private static string workPath = null;
        public static xcsConvertOOString_Delegate xcsConvertOOString;
        public static xcsDisplayCopyright_Delegate xcsDisplayCopyright;
        public static xcsDisplayDotNetMessage_Delegate xcsDisplayDotNetMessage;
        public static xcsDNInitTerm_Delegate xcsDNInitTerm;
        public static xcsFFSTFn_Delegate xcsFFSTFn;
        public static xcsGetEnvironmentString_Delegate xcsGetEnvironmentString;
        public static xcsGetMessageParts_Delegate xcsGetMessageParts;
        public static xcsPrintLines_Delegate xcsPrintLines;
        public static xcsSaveFileContextSccsId_Delegate xcsSaveFileContextSccsId;
        public static xtr_data_Delegate xtr_data;
        public static xtr_fnc_entryFn_Delegate xtr_fnc_entryFn;
        public static xtr_fnc_retcodeFn_Delegate xtr_fnc_retcodeFn;
        public static xtr_printfFn_Delegate xtr_printfFn;
        public static xtrFormatRC_Delegate xtrFormatRC;
        public static xtrGetFunction_Delegate xtrGetFunction;

        private MQCommonServices()
        {
            lock (initialiseLock)
            {
                if (this.nativeManager == null)
                {
                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    Assembly assembly = null;
                    for (int i = 0; i < assemblies.Length; i++)
                    {
                        if (assemblies[i].FullName.Contains("amqmdnet"))
                        {
                            assembly = assemblies[i];
                            break;
                        }
                    }
                    if (assembly == null)
                    {
                        assembly = Assembly.Load("amqmdnet, PublicKeyToken=dd3cb1c9aae9ec97, Version=" + CommonServices.GetVRMF() + ", Culture=Neutral");
                    }
                    nativeManagerType = assembly.GetType("IBM.WMQ.Nmqi.NativeManager");
                    this.nativeManager = assembly.CreateInstance("IBM.WMQ.Nmqi.NativeManager");
                    try
                    {
                        nativeManagerType.InvokeMember("InitializeNativeApis", BindingFlags.InvokeMethod, null, this.nativeManager, new object[] { "IBM.WMQ.MQCommonServices" });
                    }
                    catch (Exception exception)
                    {
                        throw exception;
                    }
                    this.Initialize();
                }
            }
        }

        private void ClearInserts()
        {
            this.inserts.xcsArithInsert1 = UIntPtr.Zero;
            this.inserts.xcsArithInsert2 = UIntPtr.Zero;
            this.inserts.xcsCommentInsert1 = null;
            this.inserts.xcsCommentInsert2 = null;
            this.inserts.xcsCommentInsert3 = null;
        }

        internal override uint ConvertString(string objectId, int fromCCSID, int toCCSID, byte[] inString, int inLength, ref byte[] outString, ref int outLength, int options, ref uint bytesConverted)
        {
            return xcsConvertOOString(fromCCSID, toCCSID, inString, inLength, outString, ref outLength, options, ref bytesConverted);
        }

        internal override void DisplayCopyright()
        {
            xcsDisplayCopyright();
        }

        public override uint DisplayMessage(string objectId, string qmgrName, uint returncode, uint mtype)
        {
            string str;
            int num2;
            base.GetCurrentContext(out str, out num2);
            if ((qmgrName == null) || (qmgrName.Length == 0))
            {
                return xcsDisplayDotNetMessage(null, returncode, this.inserts, mtype, Encoding.ASCII.GetBytes(str), num2);
            }
            return xcsDisplayDotNetMessage(Encoding.ASCII.GetBytes(qmgrName), returncode, this.inserts, mtype, Encoding.ASCII.GetBytes(str), num2);
        }

        public override unsafe uint FFST(string objectId, string sccsid, string lineno, uint component, string method, uint probeid, uint tag, ushort alert_num)
        {
            if (sccsid != null)
            {
                int num2;
                byte[] bytes = new byte[sccsid.Length + 1];
                Encoding.ASCII.GetBytes(sccsid, 0, sccsid.Length, bytes, 0);
                try
                {
                    num2 = Convert.ToInt32(lineno);
                }
                catch (Exception)
                {
                    num2 = 0;
                }
                fixed (byte* numRef = bytes)
                {
                    xcsSaveFileContextSccsId(numRef, num2);
                }
            }
            return xcsFFSTFn((ushort) component, 0, probeid, tag, this.inserts, this.dumpPtr, alert_num, null);
        }

        public override unsafe uint FFST(string objectId, string sccsid, string lineno, uint component, uint method, uint probeid, uint tag, ushort alert_num)
        {
            if (sccsid != null)
            {
                int num2;
                byte[] bytes = new byte[sccsid.Length + 1];
                Encoding.ASCII.GetBytes(sccsid, 0, sccsid.Length, bytes, 0);
                try
                {
                    num2 = Convert.ToInt32(lineno);
                }
                catch (Exception)
                {
                    num2 = 0;
                }
                fixed (byte* numRef = bytes)
                {
                    xcsSaveFileContextSccsId(numRef, num2);
                }
            }
            return xcsFFSTFn((ushort) component, (ushort) method, probeid, tag, this.inserts, this.dumpPtr, alert_num, null);
        }

        ~MQCommonServices()
        {
            lock (initialiseLock)
            {
                if (this.nativeManager != null)
                {
                    nativeManagerType.InvokeMember("UnloadSwitchingLibrary", BindingFlags.InvokeMethod, null, this.nativeManager, null);
                }
            }
        }

        private static uint FmtDumpCallback(ref xcsDumpCtrl DumpCtrl)
        {
            try
            {
                string userDomainName = Environment.OSVersion.ToString();
                if ((userDomainName != null) && (userDomainName.Length > 0))
                {
                    FmtPrintLines(ref DumpCtrl, "OS Version:         " + userDomainName);
                }
                userDomainName = Environment.Version.ToString();
                if ((userDomainName != null) && (userDomainName.Length > 0))
                {
                    FmtPrintLines(ref DumpCtrl, "CLR Version:        " + userDomainName);
                }
                userDomainName = Environment.WorkingSet.ToString();
                if ((userDomainName != null) && (userDomainName.Length > 0))
                {
                    FmtPrintLines(ref DumpCtrl, "Working Set:        " + userDomainName + " bytes");
                }
                userDomainName = Environment.HasShutdownStarted.ToString();
                if ((userDomainName != null) && (userDomainName.Length > 0))
                {
                    FmtPrintLines(ref DumpCtrl, "Shutdown Started:   " + userDomainName);
                }
                userDomainName = Thread.CurrentThread.ThreadState.ToString();
                if ((userDomainName != null) && (userDomainName.Length > 0))
                {
                    FmtPrintLines(ref DumpCtrl, "Thread State:       " + userDomainName);
                }
                userDomainName = Thread.CurrentThread.Priority.ToString();
                if ((userDomainName != null) && (userDomainName.Length > 0))
                {
                    FmtPrintLines(ref DumpCtrl, "Thread Priority:    " + userDomainName);
                }
                userDomainName = Thread.CurrentThread.GetApartmentState().ToString();
                if ((userDomainName != null) && (userDomainName.Length > 0))
                {
                    FmtPrintLines(ref DumpCtrl, "Apartment State:    " + userDomainName);
                }
                userDomainName = Thread.CurrentThread.IsThreadPoolThread.ToString();
                if ((userDomainName != null) && (userDomainName.Length > 0))
                {
                    FmtPrintLines(ref DumpCtrl, "Pool Thread:        " + userDomainName);
                }
                userDomainName = Thread.CurrentThread.IsBackground.ToString();
                if ((userDomainName != null) && (userDomainName.Length > 0))
                {
                    FmtPrintLines(ref DumpCtrl, "Background Thread:  " + userDomainName);
                }
                userDomainName = Environment.UserDomainName;
                if ((userDomainName != null) && (userDomainName.Length > 0))
                {
                    FmtPrintLines(ref DumpCtrl, "User Domain Name:   " + userDomainName);
                }
                userDomainName = Environment.UserName;
                if ((userDomainName != null) && (userDomainName.Length > 0))
                {
                    FmtPrintLines(ref DumpCtrl, "User Name:          " + userDomainName);
                }
                userDomainName = Environment.UserInteractive.ToString();
                if ((userDomainName != null) && (userDomainName.Length > 0))
                {
                    FmtPrintLines(ref DumpCtrl, "Interactive User:   " + userDomainName);
                }
                userDomainName = Environment.CommandLine;
                if ((userDomainName != null) && (userDomainName.Length > 0))
                {
                    FmtPrintLines(ref DumpCtrl, "Command Line:       " + userDomainName);
                }
                userDomainName = Environment.CurrentDirectory;
                if ((userDomainName != null) && (userDomainName.Length > 0))
                {
                    FmtPrintLines(ref DumpCtrl, "Current Directory:  " + userDomainName);
                }
                userDomainName = Environment.StackTrace;
                if ((userDomainName != null) && (userDomainName.Length > 0))
                {
                    FmtPrintLines(ref DumpCtrl, "Stack Trace:        \n" + userDomainName);
                }
            }
            catch (Exception exception)
            {
                try
                {
                    FmtPrintLines(ref DumpCtrl, "Exception during Component Dump\n" + exception.ToString());
                }
                catch (Exception)
                {
                }
            }
            return 0;
        }

        private static unsafe void FmtPrintLines(ref xcsDumpCtrl DumpCtrl, string Text)
        {
            Text = Text + "\n";
            if (Text.Length > 1)
            {
                byte[] bytes = new byte[Text.Length + 1];
                int index = Encoding.ASCII.GetBytes(Text, 0, Text.Length, bytes, 0);
                bytes[index] = 0;
                fixed (byte* numRef = bytes)
                {
                    xcsPrintLines(ref DumpCtrl, numRef);
                }
            }
        }

        public override uint GetArithInsert1()
        {
            return (uint) this.inserts.xcsArithInsert1;
        }

        public override uint GetArithInsert2()
        {
            return (uint) this.inserts.xcsArithInsert2;
        }

        public override string GetCommentInsert1()
        {
            return this.inserts.xcsCommentInsert1;
        }

        public override string GetCommentInsert2()
        {
            return this.inserts.xcsCommentInsert2;
        }

        public override string GetCommentInsert3()
        {
            return this.inserts.xcsCommentInsert3;
        }

        [DllImport("kernel32.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int GetCurrentThreadId();
        public override string GetDataLib()
        {
            return workPath;
        }

        internal override unsafe string GetEnvironmentValue(string name)
        {
            string str = null;
            if (name != null)
            {
                ushort length = 0x8000;
                byte[] bytes = new byte[length];
                uint num2 = 0x20006193;
                byte[] buffer2 = new byte[name.Length + 1];
                Encoding.ASCII.GetBytes(name, 0, name.Length, buffer2, 0);
                fixed (byte* numRef = bytes)
                {
                    fixed (byte* numRef2 = buffer2)
                    {
                        num2 = xcsGetEnvironmentString(numRef2, numRef, ref length);
                    }
                }
                if (num2 == 0)
                {
                    str = Encoding.ASCII.GetString(bytes, 0, length - 1);
                }
            }
            return str;
        }

        public override unsafe string GetFunctionName(uint component, uint method)
        {
            string str = null;
            byte* pBuffer = null;
            if (xtrGetFunction(component, (ushort) method, out pBuffer) == 0)
            {
                str = Marshal.PtrToStringAnsi(new IntPtr((void*) pBuffer));
            }
            return str;
        }

        public override uint GetMessage(string objectId, uint returncode, uint control, out string basicmessage, out string extendedmessage, out string replymessage, int basicLength, int extendedLength, int replyLength)
        {
            uint num = 0x20806058;
            int basicbufferlen = basicLength;
            int extendedbufferlen = extendedLength;
            int replybufferlen = replyLength;
            byte[] basicbuffer = new byte[basicbufferlen];
            byte[] extendedbuffer = new byte[extendedbufferlen];
            byte[] replybuffer = new byte[replybufferlen];
            basicmessage = "";
            extendedmessage = "";
            replymessage = "";
            int ccsid = 0x4b0;
            num = xcsGetMessageParts(returncode, control, this.inserts, ref ccsid, basicbuffer, ref basicbufferlen, extendedbuffer, ref extendedbufferlen, replybuffer, ref replybufferlen);
            if (((num == 0) || (0x10806132 == num)) || (0x10806133 == num))
            {
                Encoding encoding = new UnicodeEncoding();
                if ((control & 0x10) != 0)
                {
                    encoding = new ASCIIEncoding();
                }
                basicmessage = encoding.GetString(basicbuffer, 0, basicbufferlen);
                extendedmessage = encoding.GetString(extendedbuffer, 0, extendedbufferlen);
                replymessage = encoding.GetString(replybuffer, 0, replybufferlen);
            }
            return num;
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

        public override string GetProgramName()
        {
            return processPath;
        }

        private unsafe bool Initialize()
        {
            int* ppTraceStatus = null;
            this.inserts = new xcsINSERTS();
            this.ClearInserts();
            uint num = xcsDNInitTerm(1, msdnetDump, ref ppTraceStatus);
            lock (typeof(MQCommonServices))
            {
                if (pTraceStatus == null)
                {
                    pTraceStatus = ppTraceStatus;
                    if (pTraceStatus == null)
                    {
                        //int* numPtr2 = (int*) stackalloc byte[(((IntPtr) 1) * 4)];  吴冬 2016-6-22
                        int* numPtr2 = stackalloc int[1];
                        numPtr2[0] = -1;
                        pTraceStatus = numPtr2;
                    }
                    workPath = CommonServices.GetMQIniAttribute("WorkPath");
                    if ((workPath != null) && !workPath.EndsWith(@"\"))
                    {
                        workPath = workPath + @"\";
                    }
                    else
                    {
                        workPath = string.Empty;
                    }
                    try
                    {
                        processPath = Application.ExecutablePath;
                    }
                    catch
                    {
                    }
                    if (processPath == null)
                    {
                        processPath = "Unknown";
                    }
                }
            }
            return (0 == num);
        }

        public override unsafe string ReasonCodeName(int reason)
        {
            byte[] bytes = new byte[0x80];
            fixed (byte* numRef = bytes)
            {
                xtrFormatRC(numRef, reason);
            }
            char[] chArray2 = new char[2];
            chArray2[0] = ' ';
            char[] trimChars = chArray2;
            return Encoding.ASCII.GetString(bytes).Trim(trimChars);
        }

        public override void SetArithInsert1(uint insert)
        {
            this.inserts.xcsArithInsert1 = (UIntPtr) insert;
        }

        public override void SetArithInsert2(uint insert)
        {
            this.inserts.xcsArithInsert2 = (UIntPtr) insert;
        }

        public override void SetCommentInsert1(string insert)
        {
            this.inserts.xcsCommentInsert1 = insert;
        }

        public override void SetCommentInsert2(string insert)
        {
            this.inserts.xcsCommentInsert2 = insert;
        }

        public override void SetCommentInsert3(string insert)
        {
            this.inserts.xcsCommentInsert3 = insert;
        }

        internal override bool SetEnvironmentValue(string name, string value)
        {
            return SetEnvironmentVariable(name, value);
        }

        [DllImport("kernel32.dll", CallingConvention=CallingConvention.Cdecl)]
        private static extern bool SetEnvironmentVariable(string lpName, string lpValue);
        public override void SetValidInserts()
        {
            this.ClearInserts();
        }

        public override void TraceConstructor(string objectId, string sccsid)
        {
            if (this.TraceStatus())
            {
                this.TraceDetail(objectId, 0, 0, 0, "Constructing " + objectId + base.ExtractSCCSIDInfo(sccsid, " #N #L #V #D #T").TrimEnd(new char[0]));
            }
        }

        public override unsafe void TraceData(string objectId, uint component, uint method, ushort level, string caption, uint offset, int length, byte[] buf)
        {
            if (-1 != pTraceStatus[0])
            {
                uint num = 0;
                if (length == -1)
                {
                    num = (ushort) (buf.Length - offset);
                }
                else
                {
                    num = (ushort) length;
                }
               // fixed (void* voidRef = ((void*) &(buf[offset])))  吴冬 2016-6-22
                fixed (void* voidRef = &buf[offset])
                {
                    xtr_data((ushort) component, (ushort) method, voidRef, num);
                }
            }
        }

        public override unsafe void TraceDetail(string objectId, uint component, uint method, ushort level, string text)
        {
            if (-1 != pTraceStatus[0])
            {
                text = text.Replace("%", "%%");
                byte[] bytes = new byte[text.Length];
                Encoding.ASCII.GetBytes(text, 0, (bytes.Length > text.Length) ? text.Length : bytes.Length, bytes, 0);
                fixed (byte* numRef = bytes)
                {
                    xtr_printfFn(component, method, 0, numRef);
                }
            }
        }

        public override bool TraceEnabled()
        {
            return this.TraceStatus();
        }

        public override void TraceEntry(string objectId, uint component, uint method, object[] parameters)
        {
            xtr_fnc_entryFn(component, method, 0);
        }

        public override unsafe void TraceEnvironment()
        {
            if (-1 != pTraceStatus[0])
            {
                this.TraceText("OS Thread Id:       " + GetCurrentThreadId().ToString());
            }
            this.TraceText("CommonServices:     " + base.GetType().ToString());
        }

        public override void TraceException(string objectId, uint component, uint method, Exception ex)
        {
            string text = "Exception received\n" + ex.GetType().ToString() + "\nMessage: " + ex.Message + "\nStackTrace:\n" + ex.StackTrace;
            this.TraceDetail(string.Empty, component, method, 0, text);
        }

        public override void TraceExit(string objectId, uint component, uint method, object result, int index, int returnCode)
        {
            xtr_fnc_retcodeFn(component, method, (uint) returnCode);
        }

        public override unsafe bool TraceStatus()
        {
            return (-1 != pTraceStatus[0]);
        }

        protected void TraceText(string text)
        {
            this.TraceDetail(string.Empty, 0, 0, 0, text);
        }

        public override void TraceUserData(string objectId, uint component, uint method, ushort level, string caption, uint offset, int length, byte[] buf)
        {
            this.TraceData(objectId, component, method, level, caption, offset, length, buf);
        }

        public delegate uint xcsConvertOOString_Delegate(int fromCCSID, int toCCSID, [In] byte[] inString, int inLength, [Out] byte[] outString, ref int outLength, int options, ref uint bytesConverted);

        public delegate uint xcsDisplayCopyright_Delegate();

        public delegate uint xcsDisplayDotNetMessage_Delegate([In] byte[] qmgrName, uint returncode, MQCommonServices.xcsINSERTS inserts, uint mtype, [In] byte[] file, int line);

        public unsafe delegate uint xcsDNInitTerm_Delegate(uint flags, MQCommonServices.xcsStdCallDump_t function, ref int* ppTraceStatus);

        [StructLayout(LayoutKind.Sequential)]
        public struct xcsDumpCtrl
        {
            public unsafe void* Handle;
            private uint ThreadState;
            private uint Component;
            private uint Function;
            public unsafe void* pDumpPtrSet;
        }

        public unsafe delegate uint xcsFFSTFn_Delegate(ushort component, ushort function, uint probeid, uint tag, MQCommonServices.xcsINSERTS inserts, void* ptr_dump_data, ushort alert_num, byte[] message_key);

        public unsafe delegate uint xcsGetEnvironmentString_Delegate(byte* var, byte* buffer, ref ushort length);

        public delegate uint xcsGetMessageParts_Delegate(uint returncode, uint control, MQCommonServices.xcsINSERTS inserts, ref int ccsid, [Out] byte[] basicbuffer, ref int basicbufferlen, [Out] byte[] extendedbuffer, ref int extendedbufferlen, [Out] byte[] replybuffer, ref int replybufferlen);

        [StructLayout(LayoutKind.Sequential)]
        public struct xcsHPOOL
        {
            public uint subpoolId;
            public uint extentId;
            public uint offset;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct xcsINSERTS
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            public byte[] xcsEyeCatcher;
            public UIntPtr xcsArithInsert1;
            public UIntPtr xcsArithInsert2;
            public uint Padding;
            public string xcsCommentInsert1;
            public string xcsCommentInsert2;
            public string xcsCommentInsert3;
        }

        public unsafe delegate int xcsPrintLines_Delegate(ref MQCommonServices.xcsDumpCtrl DumpCtrl, byte* pString);

        public unsafe delegate uint xcsSaveFileContextSccsId_Delegate(byte* filename, int lineNo);

        public delegate uint xcsStdCallDump_t(ref MQCommonServices.xcsDumpCtrl DumpCtrl);

        [StructLayout(LayoutKind.Sequential)]
        public struct xtr_COMMON_TRACE
        {
            public int EarlyTrace;
            public int BinaryTrace;
            public int TimedTrace;
            public uint TraceMaxFileSize;
            public int TraceUserDataSize;
            public uint TraceOutputType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x100)]
            public byte[] TracePath;
            public uint zerodelim;
            public int RebootPersistent;
        }

        public unsafe delegate void xtr_data_Delegate(ushort comp, ushort func, void* ptr, uint length);

        public delegate void xtr_fnc_entryFn_Delegate(uint component, uint module, int init);

        public delegate void xtr_fnc_retcodeFn_Delegate(uint component, uint module, uint rc);

        public unsafe delegate uint xtr_printfFn_Delegate(uint component, uint module, ushort detail, byte* pFormatString);

        public unsafe delegate void xtrFormatRC_Delegate(byte* pBuffer, int reason);

        public unsafe delegate uint xtrGetFunction_Delegate(uint comp, ushort func, out byte* pBuffer);

        [StructLayout(LayoutKind.Sequential)]
        public struct xtrTRACECONTROLMEM
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            public byte[] StrucId;
            public uint memSize;
            public ulong initTime;
            public int initialised;
            public uint used_slots;
            public uint max_slots;
            public uint MasterSeq;
            public MQCommonServices.xtr_COMMON_TRACE common_args;
        }
    }
}

