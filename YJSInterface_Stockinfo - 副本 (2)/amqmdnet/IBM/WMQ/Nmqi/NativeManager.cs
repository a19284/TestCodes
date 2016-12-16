namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class NativeManager
    {
        private static bool bindingsInitialized = false;
        private static bool commonServiceInitialized = false;
        public lpiSPIActivateMessage_Delegate lpiSPIActivateMessage;
        public lpiSPIGet_Delegate lpiSPIGet;
        public lpiSPINotify_Delegate lpiSPINotify;
        public lpiSPIOpen_Delegate lpiSPIOpen;
        public lpiSPIPut_Delegate lpiSPIPut;
        public lpiSPISubscribe_Delegate lpiSPISubscribe;
        public lpiSPISyncPoint_Delegate lpiSPISyncPoint;
        public MQBACK_Delegate MQBACK;
        public MQBEGIN_Delegate MQBEGIN;
        public MQCLOSE_Delegate MQCLOSE;
        public MQCMIT_Delegate MQCMIT;
        public MQCONN_Delegate MQCONN;
        public MQCONNX_Delegate MQCONNX;
        public MQCTL_Delegate MQCTL;
        public MQDISC_Delegate MQDISC;
        public MQGET_Delegate MQGET;
        public MQINQ_Delegate MQINQ;
        public MQOPEN_Delegate MQOPEN;
        public MQOPEN_Delegate_IntPtr MQOPEN_IntPtr;
        public MQPUT_Delegate MQPUT;
        public MQPUT1_Delegate MQPUT1;
        public MQPUT1_Delegate_IntPtr MQPUT1_IntPtr;
        public MQSET_Delegate MQSET;
        public MQSTAT_Delegate MQSTAT;
        public MQSUB_Delegate MQSUB;
        public MQSUB_Delegate_IntPtr MQSUB_IntPtr;
        public MQSUBRQ_Delegate MQSUBRQ;
        public NativeACMQCB_Delegate NativeACMQCB;
        private static string nativeDllPath = null;
        private static object nativeSynchronizer = new object();
        public nmqiMultiBufMQPut_Delegate nmqiMultiBufMQPut;
        public nmqiMultiBufMQPut1_Delegate nmqiMultiBufMQPut1;
        internal static IntPtr pMqeDll = IntPtr.Zero;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private static bool unmanagedInitialized = false;
        private static bool unmanagedXaInitialized = false;
        public zstMQGET_Delegate zstMQGET;
        public zstMQGET_Bindings_Delegate zstMQGET_Bindings;
        public zstSPI_Delegate zstSPI;
        public zstSPINewHObj_Delegate zstSPINewHObj;

        private static string GetDynamicDllPath(string methodName, string mode)
        {
            string str = null;
            if (methodName.StartsWith("xcs") || methodName.StartsWith("xtr"))
            {
                str = "mqecs.dll";
            }
            else if (((methodName == "zstSPI") || (methodName == "zstSPINewHObj")) || ((methodName == "zstMQGET") || methodName.StartsWith("lpiSPI")))
            {
                str = "mqz.dll";
            }
            else if (((methodName == "NativeACMQCB") || (methodName == "nmqiMultiBufMQPut")) || (methodName == "nmqiMultiBufMQPut1"))
            {
                str = "amqmdnac.dll";
            }
            else if (mode == "MQSeries Client")
            {
                str = "mqdc.dll";
            }
            else if (mode == "MQSeries XA Client")
            {
                str = "mqdc.dll";
            }
            else if (mode == "MQSeries Bindings")
            {
                str = "mqm.dll";
            }
            return (nativeDllPath + str);
        }

        private static Type ImportNativeApis(Type classType, string className, string mode)
        {
            AssemblyName name = new AssemblyName();
            name.Name = className + "Assembly";
            TypeBuilder builder3 = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave).DefineDynamicModule(className + "Module", className + ".dll").DefineType(className + "Type", TypeAttributes.AnsiClass);
            MethodInfo[] methods = classType.GetMethods(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < methods.GetLength(0); i++)
            {
                MethodInfo info = methods[i];
                ParameterInfo[] parameters = info.GetParameters();
                int length = parameters.GetLength(0);
                Type[] parameterTypes = new Type[length];
                ParameterAttributes[] attributesArray = new ParameterAttributes[length];
                for (int j = 0; j < length; j++)
                {
                    parameterTypes[j] = parameters[j].ParameterType;
                    attributesArray[j] = parameters[j].Attributes;
                }
                MethodBuilder builder4 = builder3.DefinePInvokeMethod(info.Name, GetDynamicDllPath(info.Name, mode), info.Attributes, info.CallingConvention, info.ReturnType, parameterTypes, CallingConvention.Cdecl, CharSet.Ansi);
                for (int k = 0; k < length; k++)
                {
                    builder4.DefineParameter(k + 1, attributesArray[k], parameters[k].Name);
                }
                builder4.SetImplementationFlags(info.GetMethodImplementationFlags());
            }
            return builder3.CreateType();
        }

        private void InitializeBindingsSpecificApis(Type importedApis)
        {
            this.MQBEGIN = Delegate.CreateDelegate(typeof(MQBEGIN_Delegate), importedApis.GetMethod("MQBEGIN")) as MQBEGIN_Delegate;
            this.lpiSPIActivateMessage = Delegate.CreateDelegate(typeof(lpiSPIActivateMessage_Delegate), importedApis.GetMethod("lpiSPIActivateMessage")) as lpiSPIActivateMessage_Delegate;
            this.lpiSPISyncPoint = Delegate.CreateDelegate(typeof(lpiSPISyncPoint_Delegate), importedApis.GetMethod("lpiSPISyncPoint")) as lpiSPISyncPoint_Delegate;
            this.lpiSPIPut = Delegate.CreateDelegate(typeof(lpiSPIPut_Delegate), importedApis.GetMethod("lpiSPIPut")) as lpiSPIPut_Delegate;
            this.lpiSPIGet = Delegate.CreateDelegate(typeof(lpiSPIGet_Delegate), importedApis.GetMethod("lpiSPIGet")) as lpiSPIGet_Delegate;
            this.lpiSPINotify = Delegate.CreateDelegate(typeof(lpiSPINotify_Delegate), importedApis.GetMethod("lpiSPINotify")) as lpiSPINotify_Delegate;
            this.lpiSPISubscribe = Delegate.CreateDelegate(typeof(lpiSPISubscribe_Delegate), importedApis.GetMethod("lpiSPISubscribe")) as lpiSPISubscribe_Delegate;
            this.lpiSPIOpen = Delegate.CreateDelegate(typeof(lpiSPIOpen_Delegate), importedApis.GetMethod("lpiSPIOpen")) as lpiSPIOpen_Delegate;
            this.zstMQGET_Bindings = Delegate.CreateDelegate(typeof(zstMQGET_Bindings_Delegate), importedApis.GetMethod("zstMQGET")) as zstMQGET_Bindings_Delegate;
        }

        private static void InitializeCommonServiceApis(Type importedApis)
        {
            MQCommonServices.xcsDNInitTerm = Delegate.CreateDelegate(typeof(MQCommonServices.xcsDNInitTerm_Delegate), importedApis.GetMethod("xcsDNInitTerm_e")) as MQCommonServices.xcsDNInitTerm_Delegate;
            MQCommonServices.xtr_fnc_entryFn = Delegate.CreateDelegate(typeof(MQCommonServices.xtr_fnc_entryFn_Delegate), importedApis.GetMethod("xtr_fnc_entryFn_e")) as MQCommonServices.xtr_fnc_entryFn_Delegate;
            MQCommonServices.xtr_fnc_retcodeFn = Delegate.CreateDelegate(typeof(MQCommonServices.xtr_fnc_retcodeFn_Delegate), importedApis.GetMethod("xtr_fnc_retcodeFn_e")) as MQCommonServices.xtr_fnc_retcodeFn_Delegate;
            MQCommonServices.xtr_printfFn = Delegate.CreateDelegate(typeof(MQCommonServices.xtr_printfFn_Delegate), importedApis.GetMethod("xtr_printfFn_e")) as MQCommonServices.xtr_printfFn_Delegate;
            MQCommonServices.xtr_data = Delegate.CreateDelegate(typeof(MQCommonServices.xtr_data_Delegate), importedApis.GetMethod("xtr_data_e")) as MQCommonServices.xtr_data_Delegate;
            MQCommonServices.xtrFormatRC = Delegate.CreateDelegate(typeof(MQCommonServices.xtrFormatRC_Delegate), importedApis.GetMethod("xtrFormatRC_e")) as MQCommonServices.xtrFormatRC_Delegate;
            MQCommonServices.xtrGetFunction = Delegate.CreateDelegate(typeof(MQCommonServices.xtrGetFunction_Delegate), importedApis.GetMethod("xtrGetFunction_e")) as MQCommonServices.xtrGetFunction_Delegate;
            MQCommonServices.xcsSaveFileContextSccsId = Delegate.CreateDelegate(typeof(MQCommonServices.xcsSaveFileContextSccsId_Delegate), importedApis.GetMethod("xcsSaveFileContextSccsId_e")) as MQCommonServices.xcsSaveFileContextSccsId_Delegate;
            MQCommonServices.xcsFFSTFn = Delegate.CreateDelegate(typeof(MQCommonServices.xcsFFSTFn_Delegate), importedApis.GetMethod("xcsFFSTFn_e")) as MQCommonServices.xcsFFSTFn_Delegate;
            MQCommonServices.xcsPrintLines = Delegate.CreateDelegate(typeof(MQCommonServices.xcsPrintLines_Delegate), importedApis.GetMethod("xcsPrintLines_e")) as MQCommonServices.xcsPrintLines_Delegate;
            MQCommonServices.xcsGetMessageParts = Delegate.CreateDelegate(typeof(MQCommonServices.xcsGetMessageParts_Delegate), importedApis.GetMethod("xcsGetMessageParts_e")) as MQCommonServices.xcsGetMessageParts_Delegate;
            MQCommonServices.xcsDisplayDotNetMessage = Delegate.CreateDelegate(typeof(MQCommonServices.xcsDisplayDotNetMessage_Delegate), importedApis.GetMethod("xcsDisplayDotNetMessage_e")) as MQCommonServices.xcsDisplayDotNetMessage_Delegate;
            MQCommonServices.xcsDisplayCopyright = Delegate.CreateDelegate(typeof(MQCommonServices.xcsDisplayCopyright_Delegate), importedApis.GetMethod("xcsDisplayCopyright_e")) as MQCommonServices.xcsDisplayCopyright_Delegate;
            MQCommonServices.xcsConvertOOString = Delegate.CreateDelegate(typeof(MQCommonServices.xcsConvertOOString_Delegate), importedApis.GetMethod("xcsConvertOOString_e")) as MQCommonServices.xcsConvertOOString_Delegate;
            MQCommonServices.xcsGetEnvironmentString = Delegate.CreateDelegate(typeof(MQCommonServices.xcsGetEnvironmentString_Delegate), importedApis.GetMethod("xcsGetEnvironmentString_e")) as MQCommonServices.xcsGetEnvironmentString_Delegate;
        }

        public void InitializeNativeApis(string mode)
        {
            try
            {
                lock (nativeSynchronizer)
                {
                    if (nativeDllPath == null)
                    {
                        nativeDllPath = CommonServices.GetInstallationPath();//读取安装地址 吴冬 
                        nativeDllPath ="C:\\Program Files (x86)\\IBM\\WebSphere MQ";
                        //nativeDllPath = System.Environment.CurrentDirectory;

                        nativeDllPath = nativeDllPath.TrimEnd(new char[] { '\\' });
                        if (CommonServices.Is64bitCLR())
                        {
                            nativeDllPath = nativeDllPath + @"\bin64\";
                        }
                        else
                        {
                            nativeDllPath = nativeDllPath + @"\bin\";
                        }
                        pMqeDll = NativeMQEDll.LoadLibrary(nativeDllPath + "mqe.dll");
                        if (pMqeDll == IntPtr.Zero)
                        {
                            throw new Exception("Failed to load mqe.dll from folder " + nativeDllPath);
                        }
                    }
                    Type importedApis = null;
                    if ((mode == "MQSeries Client") && !unmanagedInitialized)
                    {
                        importedApis = ImportNativeApis(typeof(NativeUnmanaged), "NativeUnmanaged", mode);
                        this.InitializeNativeCommonApis(importedApis);
                        this.InitializeUnmanagedSpecificApis(importedApis);
                        unmanagedInitialized = true;
                    }
                    else if ((mode == "MQSeries XA Client") && !unmanagedXaInitialized)
                    {
                        importedApis = ImportNativeApis(typeof(NativeXAUnmanaged), "NativeXAUnmanaged", mode);
                        this.InitializeNativeCommonApis(importedApis);
                        this.InitializeUnmanagedXASpecificApis(importedApis);
                        unmanagedXaInitialized = true;
                    }
                    else if ((mode == "MQSeries Bindings") && !bindingsInitialized)
                    {
                        importedApis = ImportNativeApis(typeof(NativeBindings), "NativeBindings", mode);
                        this.InitializeNativeCommonApis(importedApis);
                        this.InitializeBindingsSpecificApis(importedApis);
                        bindingsInitialized = true;
                    }
                    else if ((mode == "IBM.WMQ.MQCommonServices") && !commonServiceInitialized)
                    {
                        InitializeCommonServiceApis(ImportNativeApis(typeof(NativeCommonService), "NativeCommonService", mode));
                        commonServiceInitialized = true;
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        private void InitializeNativeCommonApis(Type importedApis)
        {
            this.MQCLOSE = Delegate.CreateDelegate(typeof(MQCLOSE_Delegate), importedApis.GetMethod("MQCLOSE")) as MQCLOSE_Delegate;
            this.MQCONN = Delegate.CreateDelegate(typeof(MQCONN_Delegate), importedApis.GetMethod("MQCONN")) as MQCONN_Delegate;
            this.MQCONNX = Delegate.CreateDelegate(typeof(MQCONNX_Delegate), importedApis.GetMethod("MQCONNX")) as MQCONNX_Delegate;
            this.MQDISC = Delegate.CreateDelegate(typeof(MQDISC_Delegate), importedApis.GetMethod("MQDISC")) as MQDISC_Delegate;
            this.MQOPEN = Delegate.CreateDelegate(typeof(MQOPEN_Delegate), importedApis.GetMethod("MQOPEN", new Type[] { typeof(int), typeof(MQBase.MQOD).MakeByRefType(), typeof(int), typeof(int).MakeByRefType(), typeof(int).MakeByRefType(), typeof(int).MakeByRefType() })) as MQOPEN_Delegate;
            this.MQOPEN_IntPtr = Delegate.CreateDelegate(typeof(MQOPEN_Delegate_IntPtr), importedApis.GetMethod("MQOPEN", new Type[] { typeof(int), typeof(IntPtr), typeof(int), typeof(int).MakeByRefType(), typeof(int).MakeByRefType(), typeof(int).MakeByRefType() })) as MQOPEN_Delegate_IntPtr;
            this.MQPUT = Delegate.CreateDelegate(typeof(MQPUT_Delegate), importedApis.GetMethod("MQPUT")) as MQPUT_Delegate;
            this.MQPUT1 = Delegate.CreateDelegate(typeof(MQPUT1_Delegate), importedApis.GetMethod("MQPUT1", new Type[] { typeof(int), typeof(MQBase.MQOD).MakeByRefType(), typeof(MQBase.MQMD).MakeByRefType(), typeof(MQBase.MQPMO).MakeByRefType(), typeof(int), typeof(byte).MakeArrayType(), typeof(int).MakeByRefType(), typeof(int).MakeByRefType() })) as MQPUT1_Delegate;
            this.MQPUT1_IntPtr = Delegate.CreateDelegate(typeof(MQPUT1_Delegate_IntPtr), importedApis.GetMethod("MQPUT1", new Type[] { typeof(int), typeof(IntPtr), typeof(MQBase.MQMD).MakeByRefType(), typeof(MQBase.MQPMO).MakeByRefType(), typeof(int), typeof(byte).MakeArrayType(), typeof(int).MakeByRefType(), typeof(int).MakeByRefType() })) as MQPUT1_Delegate_IntPtr;
            this.MQGET = Delegate.CreateDelegate(typeof(MQGET_Delegate), importedApis.GetMethod("MQGET")) as MQGET_Delegate;
            this.MQINQ = Delegate.CreateDelegate(typeof(MQINQ_Delegate), importedApis.GetMethod("MQINQ")) as MQINQ_Delegate;
            this.MQSET = Delegate.CreateDelegate(typeof(MQSET_Delegate), importedApis.GetMethod("MQSET")) as MQSET_Delegate;
            this.MQSUB = Delegate.CreateDelegate(typeof(MQSUB_Delegate), importedApis.GetMethod("MQSUB", new Type[] { typeof(int), typeof(MQBase.MQSD).MakeByRefType(), typeof(int).MakeByRefType(), typeof(int).MakeByRefType(), typeof(int).MakeByRefType(), typeof(int).MakeByRefType() })) as MQSUB_Delegate;
            this.MQSUB_IntPtr = Delegate.CreateDelegate(typeof(MQSUB_Delegate_IntPtr), importedApis.GetMethod("MQSUB", new Type[] { typeof(int), typeof(IntPtr), typeof(int).MakeByRefType(), typeof(int).MakeByRefType(), typeof(int).MakeByRefType(), typeof(int).MakeByRefType() })) as MQSUB_Delegate_IntPtr;
            this.MQSUBRQ = Delegate.CreateDelegate(typeof(MQSUBRQ_Delegate), importedApis.GetMethod("MQSUBRQ")) as MQSUBRQ_Delegate;
            this.MQSTAT = Delegate.CreateDelegate(typeof(MQSTAT_Delegate), importedApis.GetMethod("MQSTAT")) as MQSTAT_Delegate;
            this.MQCMIT = Delegate.CreateDelegate(typeof(MQCMIT_Delegate), importedApis.GetMethod("MQCMIT")) as MQCMIT_Delegate;
            this.MQBACK = Delegate.CreateDelegate(typeof(MQBACK_Delegate), importedApis.GetMethod("MQBACK")) as MQBACK_Delegate;
            this.NativeACMQCB = Delegate.CreateDelegate(typeof(NativeACMQCB_Delegate), importedApis.GetMethod("NativeACMQCB")) as NativeACMQCB_Delegate;
            this.MQCTL = Delegate.CreateDelegate(typeof(MQCTL_Delegate), importedApis.GetMethod("MQCTL")) as MQCTL_Delegate;
        }

        private void InitializeUnmanagedSpecificApis(Type importedApis)
        {
            this.nmqiMultiBufMQPut = Delegate.CreateDelegate(typeof(nmqiMultiBufMQPut_Delegate), importedApis.GetMethod("nmqiMultiBufMQPut")) as nmqiMultiBufMQPut_Delegate;
            this.nmqiMultiBufMQPut1 = Delegate.CreateDelegate(typeof(nmqiMultiBufMQPut1_Delegate), importedApis.GetMethod("nmqiMultiBufMQPut1")) as nmqiMultiBufMQPut1_Delegate;
            this.zstSPI = Delegate.CreateDelegate(typeof(zstSPI_Delegate), importedApis.GetMethod("zstSPI")) as zstSPI_Delegate;
            this.zstSPINewHObj = Delegate.CreateDelegate(typeof(zstSPINewHObj_Delegate), importedApis.GetMethod("zstSPINewHObj")) as zstSPINewHObj_Delegate;
            this.zstMQGET = Delegate.CreateDelegate(typeof(zstMQGET_Delegate), importedApis.GetMethod("zstMQGET")) as zstMQGET_Delegate;
        }

        private void InitializeUnmanagedXASpecificApis(Type importedApis)
        {
            this.zstSPI = Delegate.CreateDelegate(typeof(zstSPI_Delegate), importedApis.GetMethod("zstSPI")) as zstSPI_Delegate;
            this.zstSPINewHObj = Delegate.CreateDelegate(typeof(zstSPINewHObj_Delegate), importedApis.GetMethod("zstSPINewHObj")) as zstSPINewHObj_Delegate;
            this.zstMQGET = Delegate.CreateDelegate(typeof(zstMQGET_Delegate), importedApis.GetMethod("zstMQGET")) as zstMQGET_Delegate;
        }

        public void UnloadSwitchingLibrary()
        {
            lock (nativeSynchronizer)
            {
                if (pMqeDll != IntPtr.Zero)
                {
                    NativeMQEDll.FreeLibrary(pMqeDll);
                    pMqeDll = IntPtr.Zero;
                }
            }
        }

        public delegate void lpiSPIActivateMessage_Delegate(int hConn, ref LPIACTIVATE ActivateOpts, out int compCode, out int reason);

        public delegate void lpiSPIGet_Delegate(int hConn, int hObj, ref MQBase.MQMD mqmd, ref MQBase.MQGMO mqgmo, int bufferLength, [Out] byte[] buffer, out int dataLength, ref LPIGETOPT GetData, out int compCode, out int reason);

        public delegate void lpiSPINotify_Delegate(int hconn, int options, ref MQBase.structLPINOTIFYDETAILS notifyDetails, out int compCode, out int reason);

        public delegate void lpiSPIOpen_Delegate(int hConn, IntPtr ObjDesc, IntPtr Options, out int pHobj, out int pCompCode, out int pReason);

        public delegate void lpiSPIPut_Delegate(int hConn, int hObj, ref MQBase.MQMD mqmd, ref MQBase.MQPMO mqpmo, int bufferLength, [In] byte[] buffer, ref LPIPUTOPTS PutOptsIn, out int compCode, out int reason);

        public delegate void lpiSPISubscribe_Delegate(int hConn, IntPtr lpiSD, IntPtr subDesc, ref int pHobj, out int pHsub, out int pCompCode, out int pReason);

        public delegate void lpiSPISyncPoint_Delegate(int hConn, ref LPISPO SyncPointOpts, out int compCode, out int reason);

        public delegate void MQBACK_Delegate(int hconn, out int pCompCode, out int pReason);

        public delegate void MQBEGIN_Delegate(int hconn, ref MQBase.MQBO mqBO, out int pCompCode, out int pReason);

        public delegate void MQCLOSE_Delegate(int hConn, ref int hObj, int options, out int compCode, out int reason);

        public delegate void MQCMIT_Delegate(int hconn, out int pCompCode, out int pReason);

        public delegate void MQCONN_Delegate(string qMgrName, out int pHconn, out int pCompCode, out int pReason);

        public delegate void MQCONNX_Delegate(string qMgrName, ref MQBase.MQCNO mqcno, out int pHconn, out int pCompCode, out int pReason);

        public delegate void MQCTL_Delegate(int hConn, int operation, IntPtr pControlOpts, out int compCode, out int reason);

        public delegate void MQDISC_Delegate(out int hConn, out int compCode, out int reason);

        public delegate void MQGET_Delegate(int hConn, int hObj, ref MQBase.MQMD mqmd, ref MQBase.MQGMO mqgmo, int bufferLength, [Out] byte[] buffer, out int dataLength, out int compCode, out int reason);

        public delegate void MQINQ_Delegate(int hconn, int hobj, int SelectorCount, [In] int[] pSelectors, int IntAttrCount, [Out] int[] pIntAttrs, int CharAttrLength, [Out] byte[] pCharAttrs, out int pCompCode, out int pReason);

        public delegate void MQOPEN_Delegate(int hConn, ref MQBase.MQOD mqod, int options, out int hObj, out int compCode, out int reason);

        public delegate void MQOPEN_Delegate_IntPtr(int hConn, IntPtr odPtr, int options, out int hObj, out int compCode, out int reason);

        public delegate void MQPUT_Delegate(int hConn, int hObj, ref MQBase.MQMD mqmd, ref MQBase.MQPMO mqpmo, int length, [In] byte[] buffer, out int compCode, out int reason);

        public delegate void MQPUT1_Delegate(int hConn, ref MQBase.MQOD mqod, ref MQBase.MQMD mqmd, ref MQBase.MQPMO mqpmo, int length, [In] byte[] buffer, out int compCode, out int reason);

        public delegate void MQPUT1_Delegate_IntPtr(int hConn, IntPtr mqodPtr, ref MQBase.MQMD mqmd, ref MQBase.MQPMO mqpmo, int length, [In] byte[] buffer, out int compCode, out int reason);

        public delegate void MQSET_Delegate(int hconn, int hobj, int SelectorCount, [In] int[] pSelectors, int IntAttrCount, [In] int[] pIntAttrs, int CharAttrLength, [In] byte[] pCharAttrs, out int pCompCode, out int pReason);

        public delegate void MQSTAT_Delegate(int hconn, int Type, ref MQBase.MQSTS Stat, out int pCompCode, out int pReason);

        public delegate void MQSUB_Delegate(int hConn, ref MQBase.MQSD mqsd, ref int hObj, out int hSub, out int compCode, out int reason);

        public delegate void MQSUB_Delegate_IntPtr(int hConn, IntPtr sdPtr, ref int hObj, out int hSub, out int compCode, out int reason);

        public delegate void MQSUBRQ_Delegate(int hConn, int hSub, int action, ref MQBase.MQSRO mqsro, out int compCode, out int reason);

        public delegate void NativeACMQCB_Delegate(int hConn, int operation, int hObj, IntPtr mqcbd, IntPtr mqmd, IntPtr mqgmo, MQBase.CallbackDelegate myDelegate, int bindingsMode, out int compCode, out int reason);

        private static class NativeMQEDll
        {
            [DllImport("kernel32.dll")]
            public static extern bool FreeLibrary(IntPtr hModule);
            [DllImport("kernel32.dll")]
            public static extern IntPtr LoadLibrary(string switchingDllToLoad);
        }

        public delegate void nmqiMultiBufMQPut_Delegate(int hConn, int hObj, ref MQBase.MQMD mqmd, ref MQBase.MQPMO mqpmo, int numBuffers, [In] byte[] buffer1, int length1, [In] byte[] buffer2, int length2, [In] byte[] buffer3, int length3, [In] byte[] buffer4, int length4, [In] byte[] buffer5, int length5, [In] byte[] buffer6, int length6, out int compCode, out int reason);

        public delegate void nmqiMultiBufMQPut1_Delegate(int hConn, ref MQBase.MQOD mqod, ref MQBase.MQMD mqmd, ref MQBase.MQPMO mqpmo, int numBuffers, [In] byte[] buffer1, int length1, [In] byte[] buffer2, int length2, [In] byte[] buffer3, int length3, [In] byte[] buffer4, int length4, [In] byte[] buffer5, int length5, [In] byte[] buffer6, int length6, out int compCode, out int reason);

        public delegate void zstMQGET_Bindings_Delegate(int hConn, int hObj, ref MQBase.MQMD mqmd, ref MQBase.MQGMO mqgmo, int bufferLength, [Out] byte[] buffer, out int dataLength, ref MQBase.lpiGETOPT GetData, out int compCode, out int reason);

        public delegate void zstMQGET_Delegate(int hConn, int hObj, ref MQBase.MQMD md, ref MQBase.MQGMO gmo, int bufferLength, byte[] buffer, out int dataLength, ref MQBase.lpiGETOPT lpiGetOpts, out int compCode, out int reason);

        public delegate void zstSPI_Delegate(int hConn, int verbId, int hObj, [In, Out] byte[] pInOut, [In] byte[] pIn, [Out] byte[] pOut, out int compCode, out int reason);

        public delegate void zstSPINewHObj_Delegate(int hConn, int verbId, ref int hObj, [In, Out] byte[] pInOut, [In] byte[] pIn, [Out] byte[] pOut, out int compCode, out int reason);
    }
}

