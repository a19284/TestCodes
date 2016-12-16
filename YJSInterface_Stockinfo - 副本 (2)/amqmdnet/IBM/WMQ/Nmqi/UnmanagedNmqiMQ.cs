namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class UnmanagedNmqiMQ : MQBase, NmqiMQ, NmqiSP
    {
        private object asyncCallbackLock = new object();
        private Hashtable asyncHconnsUnmanaged = new Hashtable();
        private Hashtable asyncHobjsUnmanaged = new Hashtable();
        private int DEFAULT_PAYLOAD_SIZE = 0x1400;
        private NmqiEnvironment env;
        private static readonly object initialiseLock = new object();
        private int mqiOptions;
        public MQBase.CallbackDelegate myDelegate;
        internal static NativeManager nativeManager = null;
        public const string sccsid = "%Z% %W%  %I% %E% %U%";
        private object useWorkerThread;

        static UnmanagedNmqiMQ()
        {
            lock (initialiseLock)
            {
                if (nativeManager == null)
                {
                    nativeManager = new NativeManager();
                    nativeManager.InitializeNativeApis("MQSeries Client");
                }
            }
        }

        public UnmanagedNmqiMQ(NmqiEnvironment env, int options)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { env, options });
            this.env = env;
            this.mqiOptions = options;
            this.myDelegate = new MQBase.CallbackDelegate(this.NmqiConsumerMethodUM);
        }

        public void CheckCmdLevel(Hconn hconn)
        {
            uint method = 0x3e6;
            this.TrEntry(method, new object[] { hconn });
            base.TrExit(method);
        }

        ~UnmanagedNmqiMQ()
        {
            lock (initialiseLock)
            {
                if (nativeManager != null)
                {
                    nativeManager.UnloadSwitchingLibrary();
                }
            }
        }

        private UnmanagedHobj GetLocalHobj(Hobj hobj)
        {
            uint method = 0x2fe;
            this.TrEntry(method, new object[] { hobj });
            UnmanagedHobj unmanagedHobj = null;
            try
            {
                unmanagedHobj = UnmanagedHobj.GetUnmanagedHobj(this.env, hobj);
            }
            finally
            {
                base.TrExit(method);
            }
            return unmanagedHobj;
        }

        private void GetSharingConversationsValue(int hConn, ref int pSConvs)
        {
            uint method = 0x478;
            this.TrEntry(method, new object[] { hConn, (int) pSConvs });
            try
            {
                int hObj = 0;
                int compCode = 0;
                int reason = 0;
                MQSPIShareCnvIn @in = new MQSPIShareCnvIn();
                MQSPIShareCnvInOut @out = new MQSPIShareCnvInOut();
                MQSPIShareCnvOut out2 = new MQSPIShareCnvOut();
                byte[] pOut = out2.ToBuffer();
                nativeManager.zstSPI(hConn, 14, hObj, @out.ToBuffer(), @in.ToBuffer(), pOut, out compCode, out reason);
                if ((compCode == 0) && (reason == 0))
                {
                    out2.ReadStruct(pOut, 0);
                    base.TrText("Out Parameters for Share Cnv Flow are :");
                    base.TrText("convPerSocket : " + out2.ConvPerSocket);
                    pSConvs = out2.ConvPerSocket;
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private UnmanagedHconn GetUnmanagedHconn(Hconn hconn)
        {
            uint method = 0x2fd;
            this.TrEntry(method, new object[] { hconn });
            UnmanagedHconn hconn2 = null;
            try
            {
                hconn2 = UnmanagedHconn.GetUnmanagedHconn(this.env, this.useWorkerThread, hconn);
            }
            finally
            {
                base.TrExit(method);
            }
            return hconn2;
        }

        public void MQBACK(Hconn hconn, out int pCompCode, out int pReason)
        {
            uint method = 0x3dd;
            this.TrEntry(method, new object[] { hconn, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                int num2 = this.GetUnmanagedHconn(hconn).Value;
                nativeManager.MQBACK(num2, out pCompCode, out pReason);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQBEGIN(Hconn hconn, MQBeginOptions pBeginOptions, out int pCompCode, out int pReason)
        {
            uint method = 990;
            this.TrEntry(method, new object[] { hconn, pBeginOptions, "pCompCode : out", "pReason : out" });
            pCompCode = 2;
            pReason = 0x7dc;
            base.TrExit(method);
        }

        public void MQCB(Hconn hConn, int operation, MQCBD pCallbackDesc, Hobj hobj, MQMessageDescriptor pMsgDesc, MQGetMessageOptions getMsgOpts, out int pCompCode, out int pReason)
        {
            uint method = 0x3df;
            this.TrEntry(method, new object[] { hConn, operation, pCallbackDesc, hobj, pMsgDesc, getMsgOpts, "pCompCode : out", "pReason : out" });
            try
            {
                IntPtr ptr;
                UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(hConn);
                UnmanagedHobj localHobj = null;
                int hObj = 0;
                if (hobj != null)
                {
                    localHobj = this.GetLocalHobj(hobj);
                    localHobj.Hconn = unmanagedHconn;
                    localHobj.Mqcbd = pCallbackDesc;
                }
                else
                {
                    localHobj = new UnmanagedHobj(this.env, -1);
                    localHobj.Hconn = unmanagedHconn;
                    localHobj.Mqcbd = pCallbackDesc;
                }
                hObj = localHobj.Value;
                int key = unmanagedHconn.Value;
                string str = key + "." + hObj;
                if ((operation & 0x100) != 0)
                {
                    lock (this.asyncCallbackLock)
                    {
                        if (pCallbackDesc.CallbackType == 1)
                        {
                            if (!this.asyncHobjsUnmanaged.Contains(str))
                            {
                                this.asyncHobjsUnmanaged.Add(str, localHobj);
                                base.TrText("Added hObj(" + str + ") to the HOBJ hashtable");
                            }
                            if (!this.asyncHconnsUnmanaged.Contains(key))
                            {
                                this.asyncHconnsUnmanaged.Add(key, localHobj);
                                base.TrText(string.Concat(new object[] { "Added hConn(", key, "), localhobj)", localHobj.Value, ")to the HCONN hashtable" }));
                            }
                        }
                        else if (!this.asyncHconnsUnmanaged.Contains(key))
                        {
                            this.asyncHconnsUnmanaged.Add(key, localHobj);
                            base.TrText(string.Concat(new object[] { "Added hConn(", key, "), localhobj)", localHobj.Value, ")to the HCONN hashtable" }));
                        }
                        goto Label_02AD;
                    }
                }
                if ((operation & 0x200) != 0)
                {
                    lock (this.asyncCallbackLock)
                    {
                        if (pCallbackDesc.CallbackType == 1)
                        {
                            if (this.asyncHobjsUnmanaged.Contains(str))
                            {
                                this.asyncHobjsUnmanaged.Remove(str);
                                base.TrText("Removed hObj(" + str + ") from the HOBJ hashtable");
                            }
                        }
                        else if (this.asyncHconnsUnmanaged.Contains(key))
                        {
                            this.asyncHconnsUnmanaged.Remove(key);
                            base.TrText("Removed hconn(" + key + ") from the HCONN hashtable");
                        }
                    }
                }
            Label_02AD:
                ptr = IntPtr.Zero;
                ptr = Marshal.AllocCoTaskMem(pCallbackDesc.GetLength());
                Marshal.StructureToPtr(pCallbackDesc.StructMQCBD, ptr, false);
                IntPtr zero = IntPtr.Zero;
                zero = Marshal.AllocCoTaskMem(pMsgDesc.GetLength());
                Marshal.StructureToPtr(pMsgDesc.StructMQMD, zero, false);
                IntPtr ptr3 = IntPtr.Zero;
                ptr3 = Marshal.AllocCoTaskMem(getMsgOpts.GetLength());
                Marshal.StructureToPtr(getMsgOpts.StructMQGMO, ptr3, false);
                nativeManager.NativeACMQCB(key, operation, hObj, ptr, zero, ptr3, this.myDelegate, 1, out pCompCode, out pReason);
                if (pCompCode == 0)
                {
                    pCallbackDesc.StructMQCBD = (MQBase.structMQCBD) Marshal.PtrToStructure(ptr, typeof(MQBase.structMQCBD));
                }
                Marshal.FreeCoTaskMem(ptr);
                Marshal.FreeCoTaskMem(zero);
                Marshal.FreeCoTaskMem(ptr3);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQCLOSE(Hconn hconn, Phobj phobj, int options, out int pCompCode, out int pReason)
        {
            uint method = 0x2ff;
            this.TrEntry(method, new object[] { hconn, phobj, options, "pCompCode : out", "pReason : out" });
            try
            {
                int handle = phobj.HOBJ.Handle;
                nativeManager.MQCLOSE(hconn.Value, ref handle, options, out pCompCode, out pReason);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQCMIT(Hconn hconn, out int pCompCode, out int pReason)
        {
            uint method = 0x3dc;
            this.TrEntry(method, new object[] { hconn, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                int num2 = this.GetUnmanagedHconn(hconn).Value;
                nativeManager.MQCMIT(num2, out pCompCode, out pReason);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQCONN(string pQMgrName, Phconn phconn, out int pCompCode, out int pReason)
        {
            uint method = 0x301;
            this.TrEntry(method, new object[] { pQMgrName, phconn, "pCompCode : out", "pReason : out" });
            pCompCode = 2;
            pReason = 0x8e1;
            int maxInOutVersion = 0;
            int maxInVersion = 0;
            int maxOutVersion = 0;
            try
            {
                ProcessModuleCollection modules = Process.GetCurrentProcess().Modules;
                base.TrText(method, "The list of dlls entries for the current process after MQCONN are : ");
                foreach (ProcessModule module in modules)
                {
                    base.TrText(method, "Loaded: " + module.FileName);
                }
            }
            catch (Win32Exception exception)
            {
                base.TrText(method, "Ignoring Win32Exception");
                base.TrException(method, exception);
            }
            try
            {
                if (phconn == null)
                {
                    pCompCode = 2;
                    pReason = 0x7e2;
                }
                else
                {
                    try
                    {
                        UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(phconn.HConn);
                        int pHconn = unmanagedHconn.Value;
                        nativeManager.MQCONN(pQMgrName, out pHconn, out pCompCode, out pReason);
                        unmanagedHconn.Value = pHconn;
                        unmanagedHconn.UpdateHconn(this, phconn);
                        int verbId = -1;
                        int flags = -1;
                        this.SPIQuerySPI(unmanagedHconn, verbId, ref maxInOutVersion, ref maxInVersion, ref maxOutVersion, ref flags, out pCompCode, out pReason);
                        if ((pCompCode == 0) || (pCompCode == 1))
                        {
                            this.SetProductIdentifier(unmanagedHconn, MQEnvironment.ProductIdentifier);
                        }
                    }
                    catch (NmqiException exception2)
                    {
                        base.TrException(method, exception2, 1);
                        pCompCode = exception2.CompCode;
                        pReason = exception2.Reason;
                        throw new MQException(pCompCode, pReason);
                    }
                    catch (MQException exception3)
                    {
                        base.TrException(method, exception3, 2);
                        pCompCode = exception3.CompCode;
                        pReason = exception3.Reason;
                        throw new MQException(pCompCode, pReason);
                    }
                    catch (Exception exception4)
                    {
                        base.TrException(method, exception4, 3);
                        pCompCode = 2;
                        pReason = 0x7e2;
                        throw;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQCONNX(string pQMgrName, MQConnectOptions pConnectOpts, Phconn phconn, out int pCompCode, out int pReason)
        {
            uint method = 770;
            this.TrEntry(method, new object[] { pQMgrName, pConnectOpts, phconn, "pCompCode : out", "pReason : out" });
            try
            {
                MQBase.MQCNO structMQCNO = pConnectOpts.StructMQCNO;
                this.MQCONNX(pQMgrName, ref structMQCNO, null, phconn, out pCompCode, out pReason);
                pConnectOpts.StructMQCNO = structMQCNO;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void MQCONNX(string pQMgrName, ref MQBase.MQCNO pConnectOpts, Hconn parentHconn, Phconn phconn, out int pCompCode, out int pReason)
        {
            uint method = 0x303;
            this.TrEntry(method, new object[] { pQMgrName, (MQBase.MQCNO) pConnectOpts, parentHconn, phconn, "pCompCode : out", "pReason : out" });
            int maxInOutVersion = 0;
            int maxInVersion = 0;
            int maxOutVersion = 0;
            try
            {
                ProcessModuleCollection modules = Process.GetCurrentProcess().Modules;
                base.TrText(method, "The list of dlls entries for the current process after MQCONNX are : ");
                foreach (ProcessModule module in modules)
                {
                    base.TrText(method, "Loaded: " + module.FileName);
                }
            }
            catch (Win32Exception exception)
            {
                base.TrText(method, "Ignoring Win32Exception");
                base.TrException(method, exception);
            }
            try
            {
                if ((parentHconn != null) && !(parentHconn is UnmanagedHconn))
                {
                    pCompCode = 2;
                    pReason = 0x8f6;
                }
                if (phconn == null)
                {
                    pCompCode = 2;
                    pReason = 0x7e2;
                }
                else
                {
                    UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(phconn.HConn);
                    int pHconn = 0;
                    nativeManager.MQCONNX(pQMgrName, ref pConnectOpts, out pHconn, out pCompCode, out pReason);
                    if ((pCompCode == 0) || (pCompCode == 1))
                    {
                        unmanagedHconn.Value = pHconn;
                        unmanagedHconn.Parent = parentHconn;
                        unmanagedHconn.UpdateHconn(this, phconn);
                        unmanagedHconn.ConnectionId = pConnectOpts.ConnectionId;
                        int flags = 0;
                        this.SPIQuerySPI(unmanagedHconn, 0, ref maxInOutVersion, ref maxInVersion, ref maxOutVersion, ref flags, out pCompCode, out pReason);
                        if ((pCompCode == 0) || (pCompCode == 1))
                        {
                            this.SetProductIdentifier(unmanagedHconn, MQEnvironment.ProductIdentifier);
                        }
                    }
                }
            }
            catch (NmqiException exception2)
            {
                base.TrException(method, exception2, 1);
                pCompCode = exception2.CompCode;
                pReason = exception2.Reason;
                throw new MQException(pCompCode, pReason);
            }
            catch (MQException exception3)
            {
                base.TrException(method, exception3, 2);
                pCompCode = exception3.CompCode;
                pReason = exception3.Reason;
                throw new MQException(pCompCode, pReason);
            }
            catch (Exception exception4)
            {
                base.TrException(method, exception4, 3);
                pCompCode = 2;
                pReason = 0x7e2;
                throw;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQCTL(Hconn hConn, int operation, MQCTLO pControlOpts, out int pCompCode, out int pReason)
        {
            uint method = 0x3e0;
            this.TrEntry(method, new object[] { hConn, operation, pControlOpts, "pCompCode : out", "pReason : out" });
            try
            {
                int num2 = this.GetUnmanagedHconn(hConn).Value;
                IntPtr zero = IntPtr.Zero;
                zero = Marshal.AllocCoTaskMem(pControlOpts.GetLength());
                Marshal.StructureToPtr(pControlOpts.StructMQCTLO, zero, false);
                nativeManager.MQCTL(num2, operation, zero, out pCompCode, out pReason);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQDISC(Phconn phconn, out int pCompCode, out int pReason)
        {
            uint method = 0x307;
            this.TrEntry(method, new object[] { phconn, "pCompCode : out", "pReason : out" });
            try
            {
                if (phconn == null)
                {
                    pCompCode = 2;
                    pReason = 0x7e2;
                }
                else
                {
                    UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(phconn.HConn);
                    int hConn = unmanagedHconn.Value;
                    nativeManager.MQDISC(out hConn, out pCompCode, out pReason);
                    unmanagedHconn.Value = hConn;
                    unmanagedHconn.UpdateHconn(this, phconn);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQGET(Hconn hConn, Hobj hObj, MQGetMessageOptions gmo, ref MQMessage message, out int compCode, out int reason)
        {
            uint method = 0x653;
            this.TrEntry(method, new object[] { hConn, hObj, gmo, message, "pCompCode : out", "pReason : out" });
            int bufferLength = 0;
            int dataLength = 0;
            byte[] buffer = null;
            bool flag = true;
            int options = gmo.Options;
            try
            {
                if (message == null)
                {
                    base.throwNewMQException(2, 0x7ea);
                }
                if (gmo == null)
                {
                    base.throwNewMQException(2, 0x88a);
                }
                if ((gmo.Options & 0x1006) == 0)
                {
                    base.TrText("Setting explicit NO_SYNCPOINT");
                    gmo.Options |= 4;
                }
                if (hConn.CmdLevel >= 700)
                {
                    flag = (options & 0x1e000000) == 0;
                    if (((options & 0x8000000) != 0) || flag)
                    {
                        gmo.Options &= -134217729;
                        gmo.Options |= 0x2000000;
                    }
                }
                int characterSet = message.CharacterSet;
                message.ClearMessage();
                MQMessageDescriptor md = message.md;
                if (buffer == null)
                {
                    int num6 = gmo.Options;
                    buffer = new byte[this.DEFAULT_PAYLOAD_SIZE];
                    bufferLength = this.DEFAULT_PAYLOAD_SIZE;
                    gmo.Options &= -65;
                    if ((num6 & 0x4000) == 0x4000)
                    {
                        MQLPIGetOpts lpiGetOpts = new MQLPIGetOpts();
                        lpiGetOpts.SetOptions(lpiGetOpts.GetOptions() | MQLPIGetOpts.lpiGETOPT_FULL_MESSAGE);
                        this.zstMQGET(hConn, hObj, ref md, ref gmo, bufferLength, buffer, out dataLength, lpiGetOpts, out compCode, out reason);
                    }
                    else
                    {
                        this.MQGET(hConn, hObj, md, gmo, bufferLength, buffer, out dataLength, out compCode, out reason);
                    }
                    if (0x7da == reason)
                    {
                        bufferLength = dataLength;
                        buffer = new byte[bufferLength];
                        this.MQGET(hConn, hObj, md, gmo, bufferLength, buffer, out dataLength, out compCode, out reason);
                    }
                    while ((compCode != 0) && (0x820 == reason))
                    {
                        gmo.Options = num6;
                        bufferLength = dataLength;
                        buffer = new byte[bufferLength];
                        gmo.Options &= -65;
                        this.MQGET(hConn, hObj, md, gmo, bufferLength, buffer, out dataLength, out compCode, out reason);
                    }
                    if ((0x848 == reason) || (0x88e == reason))
                    {
                        string objectId = "Server Binding convert message";
                        byte[] outString = null;
                        int outLength = 0;
                        uint bytesConverted = 0;
                        if (CommonServices.ConvertString(objectId, md.StructMQMD.CodedCharacterSetId, characterSet, buffer, dataLength, out outString, ref outLength, 0, out bytesConverted) == 0)
                        {
                            buffer = outString;
                            bufferLength = outLength;
                            dataLength = outLength;
                            compCode = 0;
                            reason = 0;
                        }
                    }
                }
                else
                {
                    bufferLength = buffer.Length;
                    this.MQGET(hConn, hObj, md, gmo, bufferLength, buffer, out dataLength, out compCode, out reason);
                }
                if (compCode != 0)
                {
                    base.throwNewMQException(compCode, reason);
                }
                byte[] b = buffer;
                if (compCode == 0)
                {
                    bool flag2 = false;
                    if (!flag && ((options & 0x8000000) != 0))
                    {
                        flag2 = true;
                    }
                    if (flag2 && (dataLength > 0x24))
                    {
                        b = this.PerformMsgProcessingAfterGet(ref message, buffer, (dataLength > buffer.Length) ? buffer.Length : dataLength);
                        dataLength = b.Length;
                    }
                }
                message.totalMessageLength = dataLength;
                if (dataLength > 0)
                {
                    message.Write(b, 0, (dataLength < bufferLength) ? dataLength : bufferLength);
                    message.Seek(0);
                }
                if (compCode != 0)
                {
                    base.throwNewMQException(compCode, reason);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQGET(Hconn hconn, Hobj hobj, MQMessageDescriptor pMsgDesc, MQGetMessageOptions pGetMsgOpts, int BufferLength, byte[] pBuffer, out int pDataLength, out int pCompCode, out int pReason)
        {
            uint method = 0x306;
            this.TrEntry(method, new object[] { hconn, hobj, pMsgDesc, pGetMsgOpts, BufferLength, pBuffer, "pDataLength : out", "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            pDataLength = -1;
            if (BufferLength > 0)
            {
                if (pBuffer == null)
                {
                    pCompCode = 2;
                    pReason = 0x7d5;
                }
                else if (BufferLength > pBuffer.Length)
                {
                    pCompCode = 2;
                    pReason = 0x7d5;
                }
            }
            try
            {
                if (pReason == 0)
                {
                    UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(hconn);
                    UnmanagedHobj localHobj = this.GetLocalHobj(hobj);
                    int hConn = unmanagedHconn.Value;
                    int hObj = localHobj.Value;
                    MQBase.MQMD structMQMD = pMsgDesc.StructMQMD;
                    MQBase.MQGMO structMQGMO = pGetMsgOpts.StructMQGMO;
                    nativeManager.MQGET(hConn, hObj, ref structMQMD, ref structMQGMO, BufferLength, pBuffer, out pDataLength, out pCompCode, out pReason);
                    pMsgDesc.StructMQMD = structMQMD;
                    pGetMsgOpts.StructMQGMO = structMQGMO;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQINQ(Hconn hconn, Hobj hobj, int SelectorCount, int[] pSelectors, int IntAttrCount, int[] pIntAttrs, int CharAttrLength, byte[] pCharAttrs, out int pCompCode, out int pReason)
        {
            uint method = 0x343;
            this.TrEntry(method, new object[] { hconn, hobj, SelectorCount, pSelectors, IntAttrCount, pIntAttrs, CharAttrLength, pCharAttrs, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(hconn);
                UnmanagedHobj localHobj = this.GetLocalHobj(hobj);
                int num2 = unmanagedHconn.Value;
                int num3 = localHobj.Value;
                nativeManager.MQINQ(num2, num3, SelectorCount, pSelectors, IntAttrCount, pIntAttrs, CharAttrLength, pCharAttrs, out pCompCode, out pReason);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQOPEN(Hconn phConn, ref MQObjectDescriptor pObjDesc, int Options, Phobj phobj, out int pCompCode, out int pReason)
        {
            uint method = 0x304;
            this.TrEntry(method, new object[] { phConn, pObjDesc, Options, phobj, "pCompCode : out", "pReason : out" });
            IntPtr zero = IntPtr.Zero;
            byte[] b = new byte[pObjDesc.GetRequiredBufferSize()];
            try
            {
                if (((Options & 0x100000) != 0) && ((Options & 0x80000) != 0))
                {
                    MQException exception = new MQException(2, 0x7fe);
                    throw exception;
                }
                UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(phConn);
                UnmanagedHobj localHobj = this.GetLocalHobj(phobj.HOBJ);
                int hObj = localHobj.Value;
                pObjDesc.TraceFields();
                if ((pObjDesc.Version == 1) || (pObjDesc.Version == 2))
                {
                    MQBase.MQOD structMQOD = pObjDesc.StructMQOD;
                    nativeManager.MQOPEN(unmanagedHconn.Value, ref structMQOD, Options, out hObj, out pCompCode, out pReason);
                    pObjDesc.StructMQOD = structMQOD;
                }
                else
                {
                    pObjDesc.WriteStruct(b, 0);
                    zero = Marshal.AllocCoTaskMem(b.Length);
                    Marshal.Copy(b, 0, zero, b.Length);
                    nativeManager.MQOPEN_IntPtr(unmanagedHconn.Value, zero, Options, out hObj, out pCompCode, out pReason);
                    Marshal.Copy(zero, b, 0, b.Length);
                    pObjDesc.ReadStruct(b, 0, b.Length);
                }
                if ((pCompCode == 0) || (pCompCode == 1))
                {
                    localHobj.Value = hObj;
                    phobj.HOBJ = localHobj;
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(zero);
                }
                base.TrExit(method);
            }
        }

        public void MQPUT(Hconn hConn, Hobj hObj, MQMessage message, MQPutMessageOptions pmo, out int compCode, out int reason)
        {
            uint method = 0x652;
            this.TrEntry(method, new object[] { hConn, hObj, message, pmo, "pCompCode : out", "pReason : out" });
            try
            {
                if (message == null)
                {
                    throw new MQException(2, 0x7ea);
                }
                if (pmo == null)
                {
                    throw new MQException(2, 0x87d);
                }
                int num2 = pmo.Options & 0x30000;
                if ((num2 & (num2 - 1)) != 0)
                {
                    compCode = 2;
                    reason = 0x7fe;
                    base.throwNewMQException(compCode, reason);
                }
                num2 = pmo.Options & 0x4020;
                if ((num2 & (num2 - 1)) != 0)
                {
                    compCode = 2;
                    reason = 0x7fe;
                    base.throwNewMQException(compCode, reason);
                }
                if ((pmo.Options & 6) == 0)
                {
                    pmo.Options |= 4;
                }
                byte[] src = message.GetBuffer();
                byte[] dst = new byte[src.Length];
                Buffer.BlockCopy(src, 0, dst, 0, src.Length);
                int characterSet = message.CharacterSet;
                int encoding = message.Encoding;
                string format = message.Format;
                this.PerformMsgProcessgingBeforePut(ref message);
                src = message.GetBuffer();
                this.MQPUT(hConn, hObj, message.md, pmo, src.Length, src, out compCode, out reason);
                if (compCode == 0)
                {
                    this.PerformMsgProcessingAfterPut(ref message, dst, characterSet, encoding, format);
                }
                if (compCode != 0)
                {
                    base.throwNewMQException(compCode, reason);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQPUT(Hconn hconn, Hobj hobj, MQMessageDescriptor pMsgDesc, MQPutMessageOptions pPutMsgOpts, int bufferLength, byte[] pBuffer, out int pCompCode, out int pReason)
        {
            uint method = 0x305;
            this.TrEntry(method, new object[] { hconn, hobj, pMsgDesc, pPutMsgOpts, bufferLength, pBuffer, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            if (bufferLength > 0)
            {
                if (pBuffer == null)
                {
                    pCompCode = 2;
                    pReason = 0x7d5;
                }
                else if (bufferLength > pBuffer.Length)
                {
                    pCompCode = 2;
                    pReason = 0x7d5;
                }
            }
            else if (bufferLength < 0)
            {
                pCompCode = 2;
                pReason = 0x7d5;
            }
            try
            {
                if (pReason == 0)
                {
                    UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(hconn);
                    UnmanagedHobj localHobj = this.GetLocalHobj(hobj);
                    int hConn = unmanagedHconn.Value;
                    int hObj = localHobj.Value;
                    MQBase.MQMD structMQMD = pMsgDesc.StructMQMD;
                    MQBase.MQPMO structMQPMO = pPutMsgOpts.StructMQPMO;
                    nativeManager.MQPUT(hConn, hObj, ref structMQMD, ref structMQPMO, bufferLength, pBuffer, out pCompCode, out pReason);
                    pMsgDesc.StructMQMD = structMQMD;
                    pPutMsgOpts.StructMQPMO = structMQPMO;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQPUT1(Hconn hconn, MQObjectDescriptor pObjDesc, MQMessageDescriptor pMsgDesc, MQPutMessageOptions pPutMsgOpts, int bufferLength, byte[] pBuffer, out int pCompCode, out int pReason)
        {
            uint method = 0x342;
            this.TrEntry(method, new object[] { hconn, pObjDesc, pMsgDesc, pPutMsgOpts, bufferLength, pBuffer, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            if (bufferLength > 0)
            {
                if (pBuffer == null)
                {
                    pCompCode = 2;
                    pReason = 0x7d5;
                }
                else if (bufferLength > pBuffer.Length)
                {
                    pCompCode = 2;
                    pReason = 0x7d5;
                }
            }
            else if (bufferLength < 0)
            {
                pCompCode = 2;
                pReason = 0x7d5;
            }
            IntPtr zero = IntPtr.Zero;
            byte[] b = new byte[pObjDesc.GetRequiredBufferSize()];
            try
            {
                if (pReason == 0)
                {
                    int hConn = this.GetUnmanagedHconn(hconn).Value;
                    pObjDesc.WriteStruct(b, 0);
                    zero = Marshal.AllocCoTaskMem(b.Length);
                    Marshal.Copy(b, 0, zero, b.Length);
                    MQBase.MQMD structMQMD = pMsgDesc.StructMQMD;
                    MQBase.MQPMO structMQPMO = pPutMsgOpts.StructMQPMO;
                    pObjDesc.TraceFields();
                    nativeManager.MQPUT1_IntPtr(hConn, zero, ref structMQMD, ref structMQPMO, bufferLength, pBuffer, out pCompCode, out pReason);
                    Marshal.Copy(zero, b, 0, b.Length);
                    pObjDesc.ReadStruct(b, 0, b.Length);
                    pMsgDesc.StructMQMD = structMQMD;
                    pPutMsgOpts.StructMQPMO = structMQPMO;
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(zero);
                }
                base.TrExit(method);
            }
        }

        public void MQSET(Hconn hconn, Hobj hobj, int SelectorCount, int[] pSelectors, int IntAttrCount, int[] pIntAttrs, int CharAttrLength, byte[] pCharAttrs, out int pCompCode, out int pReason)
        {
            uint method = 0x344;
            this.TrEntry(method, new object[] { hconn, hobj, SelectorCount, pSelectors, IntAttrCount, pIntAttrs, CharAttrLength, pCharAttrs, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(hconn);
                UnmanagedHobj localHobj = this.GetLocalHobj(hobj);
                int num2 = unmanagedHconn.Value;
                int num3 = localHobj.Value;
                nativeManager.MQSET(num2, num3, SelectorCount, pSelectors, IntAttrCount, pIntAttrs, CharAttrLength, pCharAttrs, out pCompCode, out pReason);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQSTAT(Hconn hconn, int Type, MQAsyncStatus pStat, out int pCompCode, out int pReason)
        {
            uint method = 0x3db;
            this.TrEntry(method, new object[] { hconn, Type, pStat, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                int num2 = this.GetUnmanagedHconn(hconn).Value;
                MQBase.MQSTS structMQSTS = pStat.StructMQSTS;
                nativeManager.MQSTAT(num2, Type, ref structMQSTS, out pCompCode, out pReason);
                pStat.StructMQSTS = structMQSTS;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQSUB(Hconn hconn, MQSubscriptionDescriptor pSubDesc, Phobj hobj, Phobj hsub, out int pCompCode, out int pReason)
        {
            uint method = 0x3d9;
            this.TrEntry(method, new object[] { hconn, pSubDesc, hobj, hsub, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            byte[] b = new byte[pSubDesc.GetRequiredBufferSize()];
            IntPtr zero = IntPtr.Zero;
            try
            {
                UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(hconn);
                if (hobj == null)
                {
                    hobj = new Phobj(this.env);
                }
                if (hsub == null)
                {
                    hsub = new Phobj(this.env);
                }
                UnmanagedHobj localHobj = this.GetLocalHobj(hobj.HOBJ);
                UnmanagedHobj hobj3 = this.GetLocalHobj(hsub.HOBJ);
                int hConn = unmanagedHconn.Value;
                int hObj = localHobj.Value;
                int hSub = hobj3.Value;
                pSubDesc.WriteStruct(b, 0);
                zero = Marshal.AllocCoTaskMem(b.Length);
                Marshal.Copy(b, 0, zero, b.Length);
                pSubDesc.TraceFields();
                nativeManager.MQSUB_IntPtr(hConn, zero, ref hObj, out hSub, out pCompCode, out pReason);
                if ((pCompCode == 0) || (pCompCode == 1))
                {
                    hobj3.Value = hSub;
                    hsub.HOBJ = hobj3;
                    localHobj.Value = hObj;
                    hobj.HOBJ = localHobj;
                    try
                    {
                        b = new byte[pSubDesc.GetRequiredBufferSize() + pSubDesc.ObjectString.VSString.Length];
                        Marshal.Copy(zero, b, 0, b.Length);
                        pSubDesc.ReadStruct(b, 0, b.Length);
                    }
                    catch (Exception exception)
                    {
                        base.TrException(method, exception);
                        base.TrText("Attempt to read & construct mqsd structure from unmanaged memory has caused an exception");
                        base.TrText("This is an indication and might become potential failure condition further in the MQSUB Call");
                    }
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(zero);
                    zero = IntPtr.Zero;
                }
                base.TrExit(method);
            }
        }

        public void MQSUBRQ(Hconn hconn, Hobj hsub, int action, ref MQSubscriptionRequestOptions pSubReq, out int pCompCode, out int pReason)
        {
            uint method = 0x3da;
            this.TrEntry(method, new object[] { hconn, hsub, action, pSubReq, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(hconn);
                UnmanagedHobj localHobj = this.GetLocalHobj(hsub);
                int hConn = unmanagedHconn.Value;
                int hSub = localHobj.Value;
                MQBase.MQSRO structMQSRO = pSubReq.StructMQSRO;
                nativeManager.MQSUBRQ(hConn, hSub, action, ref structMQSRO, out pCompCode, out pReason);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void NmqiConnect(string name, NmqiConnectOptions pNmqiConnectOpts, MQConnectOptions cno, Hconn parentHconn, Phconn pHconn, out int pCompCode, out int pReason)
        {
            uint method = 0x3e1;
            this.TrEntry(method, new object[] { name, pNmqiConnectOpts, cno, parentHconn, pHconn, "pCompCode : out", "pReason : out" });
            int maxInOutVersion = 0;
            int maxInVersion = 0;
            int maxOutVersion = 0;
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            IntPtr ptr3 = IntPtr.Zero;
            pCompCode = 0;
            pReason = 0;
            if (cno == null)
            {
                pCompCode = 2;
                pReason = 0x85b;
            }
            else if ((parentHconn != null) && !(parentHconn is UnmanagedHconn))
            {
                pCompCode = 2;
                pReason = 0x8f6;
            }
            if (pHconn == null)
            {
                pCompCode = 2;
                pReason = 0x7e2;
            }
            if (((cno.Options & 0x80000) != 0) || ((cno.Options & 0x100000) != 0))
            {
                if (cno.ClientConn == null)
                {
                    base.TrText("MQChannelDefinition passed by caller NULL");
                    pCompCode = 2;
                    pReason = 0x85b;
                }
                else
                {
                    base.TrText("MQChannelDefinition passed by caller in cno.ClientConn:");
                    cno.ClientConn.TraceFields();
                }
            }
            try
            {
                if (cno.SecurityParms != null)
                {
                    if (cno.SecurityParms.UserId != null)
                    {
                        ptr = Marshal.StringToCoTaskMemAnsi(cno.SecurityParms.UserId);
                        cno.SecurityParms.CSPUserIdPtr = ptr;
                        cno.SecurityParms.CSPUserIdLength = cno.SecurityParms.UserId.Length;
                    }
                    if (cno.SecurityParms.Password != null)
                    {
                        ptr3 = Marshal.StringToCoTaskMemAnsi(cno.SecurityParms.Password);
                        cno.SecurityParms.CSPPasswordPtr = ptr3;
                        cno.SecurityParms.CSPPasswordLength = cno.SecurityParms.Password.Length;
                    }
                    zero = Marshal.AllocCoTaskMem(Marshal.SizeOf(cno.SecurityParms.StructMQCSP));
                    Marshal.StructureToPtr(cno.SecurityParms.StructMQCSP, zero, false);
                    cno.SecurityParmsPtr = zero;
                }
                if (pReason == 0)
                {
                    UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(pHconn.HConn);
                    int num5 = 0;
                    cno.TraceFields();
                    MQBase.MQCNO structMQCNO = cno.StructMQCNO;
                    nativeManager.MQCONNX(name, ref structMQCNO, out num5, out pCompCode, out pReason);
                    cno.StructMQCNO = structMQCNO;
                    if ((pCompCode == 0) || (pCompCode == 1))
                    {
                        unmanagedHconn.Value = num5;
                        unmanagedHconn.UpdateHconn(this, pHconn);
                        unmanagedHconn.Parent = parentHconn;
                        unmanagedHconn.PHconn = pHconn;
                        if (((structMQCNO.Options & 0x1000000) == 0x1000000) || ((structMQCNO.Options & 0x4000000) == 0x4000000))
                        {
                            ((UnmanagedHconn) pHconn.HConn).Reconnectable = true;
                        }
                        base.TrText("Copy resolved CD");
                        if ((((cno.Options & 0x80000) != 0) && (structMQCNO.ClientConnPtr != IntPtr.Zero)) && (cno.ClientConn != null))
                        {
                            cno.ClientConn.StructMQCD = (MQBase.MQCD) Marshal.PtrToStructure(structMQCNO.ClientConnPtr, typeof(MQBase.MQCD));
                        }
                        cno.ClientConn.TraceFields();
                        int flags = 0;
                        this.SPIQuerySPI(unmanagedHconn, 0, ref maxInOutVersion, ref maxInVersion, ref maxOutVersion, ref flags, out pCompCode, out pReason);
                        if ((pCompCode == 0) || (pCompCode == 1))
                        {
                            this.SetProductIdentifier(unmanagedHconn, pNmqiConnectOpts.ExternalProductIdentifier);
                        }
                        unmanagedHconn.ConnectionId = cno.ConnectionId;
                    }
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(zero);
                }
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ptr);
                }
                if (ptr3 != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ptr);
                }
                base.TrExit(method);
            }
        }

        public void NmqiConnect(string name, NmqiConnectOptions pNmqiConnectOpts, MQConnectOptions cno, Hconn parentHconn, Phconn pHconn, out int pCompCode, out int pReason, ManagedHconn rcnHconn)
        {
            uint method = 0x4ef;
            this.TrEntry(method, new object[] { name, pNmqiConnectOpts, cno, parentHconn, pHconn, "pCompCode : out", "pReason : out", rcnHconn });
            pCompCode = 2;
            pReason = 0x8fa;
            base.TrExit(method);
        }

        public void NmqiConsumerMethodUM(int hconn, IntPtr structMqmd, IntPtr structMqgmo, IntPtr buffer, IntPtr structMqcbc)
        {
            uint method = 0x43f;
            this.TrEntry(method, new object[] { hconn, structMqmd, structMqgmo, buffer, structMqcbc });
            UnmanagedHobj hobj = null;
            Phconn pHconn = null;
            try
            {
                MQMessageDescriptor mqmd = null;
                if (structMqmd != IntPtr.Zero)
                {
                    mqmd = new MQMessageDescriptor();
                    mqmd.StructMQMD = (MQBase.MQMD) Marshal.PtrToStructure(structMqmd, typeof(MQBase.MQMD));
                }
                MQGetMessageOptions getMsgOpts = null;
                if (structMqgmo != IntPtr.Zero)
                {
                    getMsgOpts = new MQGetMessageOptions();
                    getMsgOpts.StructMQGMO = (MQBase.MQGMO) Marshal.PtrToStructure(structMqgmo, typeof(MQBase.MQGMO));
                }
                byte[] destination = null;
                MQCBC mqcbc = null;
                if (structMqcbc != IntPtr.Zero)
                {
                    mqcbc = new MQCBC();
                    mqcbc.StructMQCBC = (MQBase.structMQCBC) Marshal.PtrToStructure(structMqcbc, typeof(MQBase.structMQCBC));
                    if ((buffer != IntPtr.Zero) && (mqcbc.DataLength != 0))
                    {
                        destination = new byte[mqcbc.DataLength];
                        Marshal.Copy(buffer, destination, 0, mqcbc.DataLength);
                    }
                }
                lock (this.asyncCallbackLock)
                {
                    if (mqcbc != null)
                    {
                        if ((mqcbc.HOBJ == 0) && ((mqcbc.CallType == 3) || (mqcbc.CallType == 5)))
                        {
                            if (this.asyncHconnsUnmanaged.Contains(hconn))
                            {
                                hobj = (UnmanagedHobj) this.asyncHconnsUnmanaged[hconn];
                                base.TrText(string.Concat(new object[] { "PHConn(", hobj.Hconn.Value, "), localhobj(", hobj.Value, ") selected." }));
                            }
                        }
                        else
                        {
                            string key = hconn + "." + mqcbc.HOBJ;
                            if (this.asyncHobjsUnmanaged.Contains(key))
                            {
                                hobj = (UnmanagedHobj) this.asyncHobjsUnmanaged[key];
                                base.TrText(string.Concat(new object[] { "PHConn(", hobj.Hconn.Value, "), localhobj(", hobj.Value, ") selected." }));
                            }
                        }
                    }
                }
                if (hobj == null)
                {
                    NmqiException ex = new NmqiException(this.env, -1, null, mqcbc.CompCode, mqcbc.Reason, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                pHconn = hobj.Hconn.PHconn;
                if ((hobj.Mqcbd != null) && (hobj.Mqcbd.MqConsumer != null))
                {
                    hobj.Mqcbd.MqConsumer.Consumer(pHconn, mqmd, getMsgOpts, destination, mqcbc);
                    base.TrText("Consumer invoked");
                }
                else
                {
                    base.TrText("Mqcbd or MqConsumer is NULL - Consumer not invoked");
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public bool NmqiConvertMessage(Hconn hconn, Hobj hobj, int reqEncoding, int reqCCSID, int appOptions, bool callExitOnLenErr, MQMessageDescriptor md, byte[] buffer, out int dataLength, int availableLength, int bufferLength, out int pCompCode, out int pReason, out int returnedLength)
        {
            uint method = 0x3e7;
            this.TrEntry(method, new object[] { hconn, hobj, reqEncoding, reqCCSID, appOptions, callExitOnLenErr, md, buffer, "dataLength : out", availableLength, bufferLength, "pCompCode : out", "pReason : out", "returnedLength : out" });
            pCompCode = 2;
            pReason = 0x8fa;
            dataLength = 0;
            returnedLength = 0;
            base.TrExit(method, false);
            return false;
        }

        public void NmqiGetMessage(Hconn hconn, Hobj hobj, MQMessageDescriptor md, MQGetMessageOptions gmo, int bufferLength, byte[] buffer, out int dataLength, out int pCompCode, out int pReason)
        {
            uint method = 0x3e8;
            this.TrEntry(method, new object[] { hconn, hobj, md, gmo, bufferLength, buffer, "dataLength : out", "pCompCode : out", "pReason : out" });
            pCompCode = 2;
            pReason = 0x8fa;
            dataLength = 0;
            base.TrExit(method);
        }

        public void NmqiPut(Hconn hconn, Hobj hobj, MQMessageDescriptor msgDesc, MQPutMessageOptions putMsgOpts, MemoryStream[] buffers, out int pCompCode, out int pReason)
        {
            uint method = 0x44c;
            this.TrEntry(method, new object[] { hconn, hobj, msgDesc, putMsgOpts, buffers, "pCompCode : out", "pReason : out" });
            try
            {
                int numBuffs = (buffers == null) ? 0 : buffers.Length;
                this.NmqiPut(hconn, hobj, msgDesc, putMsgOpts, 0, null, buffers, numBuffs, out pCompCode, out pReason);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void NmqiPut(Hconn hconn, Hobj hobj, MQMessageDescriptor msgDesc, MQPutMessageOptions putMsgOpts, int sbLength, byte[] sBuff, MemoryStream[] mBuffs, int numBuffs, out int pCompCode, out int pReason)
        {
            uint method = 0x44d;
            this.TrEntry(method, new object[] { hconn, hobj, msgDesc, putMsgOpts, sbLength, sBuff, mBuffs, numBuffs, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            int length = 0;
            int num7 = 0;
            byte[] buffer = null;
            byte[] buffer2 = null;
            byte[] buffer3 = null;
            byte[] buffer4 = null;
            byte[] buffer5 = null;
            byte[] buffer6 = null;
            try
            {
                switch (numBuffs)
                {
                    case 1:
                        goto Label_0126;

                    case 2:
                        goto Label_0110;

                    case 3:
                        goto Label_00FA;

                    case 4:
                        goto Label_00E3;

                    case 5:
                        break;

                    case 6:
                        buffer6 = mBuffs[5].GetBuffer();
                        num7 = (int) mBuffs[5].Length;
                        break;

                    default:
                        goto Label_013E;
                }
                buffer5 = mBuffs[4].GetBuffer();
                length = (int) mBuffs[4].Length;
            Label_00E3:
                buffer4 = mBuffs[3].GetBuffer();
                num5 = (int) mBuffs[3].Length;
            Label_00FA:
                buffer3 = mBuffs[2].GetBuffer();
                num4 = (int) mBuffs[2].Length;
            Label_0110:
                buffer2 = mBuffs[1].GetBuffer();
                num3 = (int) mBuffs[1].Length;
            Label_0126:
                buffer = mBuffs[0].GetBuffer();
                num2 = (int) mBuffs[0].Length;
                goto Label_014A;
            Label_013E:
                pCompCode = 2;
                pReason = 0x7d4;
            Label_014A:
                if (pReason == 0)
                {
                    UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(hconn);
                    UnmanagedHobj localHobj = this.GetLocalHobj(hobj);
                    int hConn = unmanagedHconn.Value;
                    int hObj = localHobj.Value;
                    MQBase.MQMD structMQMD = msgDesc.StructMQMD;
                    MQBase.MQPMO structMQPMO = putMsgOpts.StructMQPMO;
                    nativeManager.nmqiMultiBufMQPut(hConn, hObj, ref structMQMD, ref structMQPMO, numBuffs, buffer, num2, buffer2, num3, buffer3, num4, buffer4, num5, buffer5, length, buffer6, num7, out pCompCode, out pReason);
                    msgDesc.StructMQMD = structMQMD;
                    putMsgOpts.StructMQPMO = structMQPMO;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void NmqiPut1(Hconn hconn, MQObjectDescriptor objDesc, MQMessageDescriptor msgDesc, MQPutMessageOptions putMsgOpts, MemoryStream[] buffers, out int pCompCode, out int pReason)
        {
            uint method = 0x44e;
            this.TrEntry(method, new object[] { hconn, objDesc, msgDesc, putMsgOpts, buffers, "pCompCode : out", "pReason : out" });
            try
            {
                int numBuffs = (buffers == null) ? 0 : buffers.Length;
                this.NmqiPut1(hconn, objDesc, msgDesc, putMsgOpts, 0, null, buffers, numBuffs, out pCompCode, out pReason);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void NmqiPut1(Hconn hconn, MQObjectDescriptor objDesc, MQMessageDescriptor msgDesc, MQPutMessageOptions putMsgOpts, int sbLength, byte[] sBuff, MemoryStream[] mBuffs, int numBuffs, out int pCompCode, out int pReason)
        {
            uint method = 0x44f;
            this.TrEntry(method, new object[] { hconn, objDesc, msgDesc, putMsgOpts, sbLength, sBuff, mBuffs, numBuffs, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            int length = 0;
            int num7 = 0;
            byte[] buffer = null;
            byte[] buffer2 = null;
            byte[] buffer3 = null;
            byte[] buffer4 = null;
            byte[] buffer5 = null;
            byte[] buffer6 = null;
            try
            {
                switch (numBuffs)
                {
                    case 1:
                        goto Label_0126;

                    case 2:
                        goto Label_0110;

                    case 3:
                        goto Label_00FA;

                    case 4:
                        goto Label_00E3;

                    case 5:
                        break;

                    case 6:
                        buffer6 = mBuffs[5].GetBuffer();
                        num7 = (int) mBuffs[5].Length;
                        break;

                    default:
                        goto Label_013E;
                }
                buffer5 = mBuffs[4].GetBuffer();
                length = (int) mBuffs[4].Length;
            Label_00E3:
                buffer4 = mBuffs[3].GetBuffer();
                num5 = (int) mBuffs[3].Length;
            Label_00FA:
                buffer3 = mBuffs[2].GetBuffer();
                num4 = (int) mBuffs[2].Length;
            Label_0110:
                buffer2 = mBuffs[1].GetBuffer();
                num3 = (int) mBuffs[1].Length;
            Label_0126:
                buffer = mBuffs[0].GetBuffer();
                num2 = (int) mBuffs[0].Length;
                goto Label_014A;
            Label_013E:
                pCompCode = 2;
                pReason = 0x7d4;
            Label_014A:
                if (pReason == 0)
                {
                    int hConn = this.GetUnmanagedHconn(hconn).Value;
                    MQBase.MQOD structMQOD = objDesc.StructMQOD;
                    byte[] bytes = Encoding.UTF8.GetBytes(objDesc.ObjectString.VSString);
                    IntPtr destination = Marshal.AllocCoTaskMem(bytes.Length);
                    Marshal.Copy(bytes, 0, destination, bytes.Length);
                    MQBase.structMQCHARV objectString = structMQOD.ObjectString;
                    objectString.VSPtr = destination;
                    objectString.VSLength = objDesc.ObjectString.VSString.Length;
                    structMQOD.ObjectString = objectString;
                    MQBase.MQMD structMQMD = msgDesc.StructMQMD;
                    MQBase.MQPMO structMQPMO = putMsgOpts.StructMQPMO;
                    nativeManager.nmqiMultiBufMQPut1(hConn, ref structMQOD, ref structMQMD, ref structMQPMO, numBuffs, buffer, num2, buffer2, num3, buffer3, num4, buffer4, num5, buffer5, length, buffer6, num7, out pCompCode, out pReason);
                    objDesc.StructMQOD = structMQOD;
                    msgDesc.StructMQMD = structMQMD;
                    putMsgOpts.StructMQPMO = structMQPMO;
                    if (destination != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(destination);
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void PerformMsgProcessgingBeforePut(ref MQMessage mqMsg)
        {
            uint method = 0x654;
            this.TrEntry(method, new object[] { mqMsg });
            try
            {
                mqMsg = new MQMarshalMessageForPut(mqMsg).ConstructMessageForSend();
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal byte[] PerformMsgProcessingAfterGet(ref MQMessage mqMsg, byte[] data, int length)
        {
            byte[] buffer2;
            uint method = 0x655;
            this.TrEntry(method, new object[] { mqMsg, data, length });
            try
            {
                buffer2 = new MQMarshalMessageForGet(mqMsg, data, length).ProcessMessageForRFH();
            }
            finally
            {
                base.TrExit(method);
            }
            return buffer2;
        }

        internal void PerformMsgProcessingAfterPut(ref MQMessage message, byte[] tempMsgBuffer, int bodyCcsid, int encoding, string format)
        {
            uint method = 0x656;
            this.TrEntry(method, new object[] { message, tempMsgBuffer, bodyCcsid, encoding, format });
            try
            {
                message.ClearMessage();
                message.CharacterSet = bodyCcsid;
                message.Format = format;
                message.Encoding = encoding;
                if (tempMsgBuffer.Length != 0)
                {
                    message.Write(tempMsgBuffer);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void SetProductIdentifier(UnmanagedHconn hconn, string prdIdentifier)
        {
            uint method = 0x471;
            this.TrEntry(method, new object[] { hconn, prdIdentifier });
            if ((prdIdentifier == null) || (prdIdentifier.Length < 1))
            {
                base.TrExit(method, 1);
            }
            else
            {
                try
                {
                    base.TrText("ProductIdentifier = " + prdIdentifier);
                    byte[] bytes = Encoding.ASCII.GetBytes(prdIdentifier);
                    int hConn = hconn.Value;
                    int hObj = 0;
                    int compCode = 0;
                    int reason = 0;
                    MQSPIProdIdIn @in = new MQSPIProdIdIn();
                    MQSPIProdIdInOut @out = new MQSPIProdIdInOut();
                    MQSPIProdIdOut out2 = new MQSPIProdIdOut();
                    @in.ClientProductIdentifier = bytes;
                    nativeManager.zstSPI(hConn, 13, hObj, @out.ToBuffer(), @in.ToBuffer(), out2.ToBuffer(), out compCode, out reason);
                    if ((compCode == 0) && (reason == 0))
                    {
                        base.TrText("Out Parameters for Product Identifier Flow are :");
                        base.TrText("Client Product Identifier : " + out2.ClientProductIdentifier);
                        base.TrText("Server Product Identifier : " + out2.ServerProductIdentifier);
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    base.TrExit(method, 1);
                }
            }
        }

        public void SPIActivateMessage(Hconn hConn, ref MQSPIActivateOpts spiao, out int pCompCode, out int pReason)
        {
            uint method = 0x473;
            this.TrEntry(method, new object[] { hConn, spiao, "pCompCode : out", "pReason : out" });
            byte[] pInOut = null;
            int maxInOutVersion = 0;
            int maxInVersion = 0;
            byte[] pOut = null;
            int maxOutVersion = 0;
            int flags = 0;
            pCompCode = 2;
            pReason = 0x893;
            try
            {
                UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(hConn);
                int num6 = unmanagedHconn.Value;
                this.SPIQuerySPI(unmanagedHconn, 4, ref maxInOutVersion, ref maxInVersion, ref maxOutVersion, ref flags, out pCompCode, out pReason);
                if (pCompCode == 0)
                {
                    MQSPIActivateInOut @out = new MQSPIActivateInOut(maxInOutVersion);
                    MQSPIActivateIn @in = new MQSPIActivateIn(maxInVersion);
                    MQSPIActivateOut out2 = new MQSPIActivateOut(maxOutVersion);
                    if (spiao != null)
                    {
                        @in.Options = spiao.Options;
                        @in.QName = spiao.QName;
                        @in.QMgrName = spiao.QMgrName;
                        @in.MsgId = spiao.MsgId;
                    }
                    pInOut = @out.ToBuffer();
                    pOut = out2.ToBuffer();
                    int hObj = 0;
                    nativeManager.zstSPI(num6, 4, hObj, pInOut, @in.ToBuffer(), pOut, out pCompCode, out pReason);
                    @out.ReadStruct(pInOut, 0);
                    out2.ReadStruct(pOut, 0);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SpiConnect(string pQMgrName, SpiConnectOptions pSpiConnectOpts, MQConnectOptions pConnectOpts, Phconn pHconn, out int pCompCode, out int pReason)
        {
            uint method = 0x3e2;
            this.TrEntry(method, new object[] { pQMgrName, pSpiConnectOpts, pConnectOpts, pHconn, "pCompCode : out", "pReason : out" });
            pCompCode = 2;
            pReason = 0x8fa;
            base.TrExit(method);
        }

        public void SpiGet(Hconn hconn, Hobj hobj, MQMessageDescriptor pMqmd, MQGetMessageOptions pMqgmo, SpiGetOptions spiOptions, int bufferLength, byte[] pBuffer, out int dataLength, out int pCompCode, out int pReason)
        {
            uint method = 0x3e5;
            this.TrEntry(method, new object[] { hconn, hobj, pMqmd, pMqgmo, spiOptions, bufferLength, pBuffer, "dataLength : out", "pCompCode : out", "pReason : out" });
            pCompCode = 2;
            pReason = 0x8fa;
            dataLength = 0;
            base.TrExit(method);
        }

        public void SPIGet(Hconn hConn, Hobj hObj, ref MQMessageDescriptor md, ref MQGetMessageOptions gmo, ref MQSPIGetOpts spigo, int bufferLength, byte[] buffer, out int dataLength, out int pCompCode, out int pReason)
        {
            uint method = 0x476;
            this.TrEntry(method, new object[] { hConn, hObj, md, gmo, spigo, bufferLength, buffer, "dataLength : out", "pCompCode : out", "pReason : out" });
            byte[] pInOut = null;
            int maxInOutVersion = 0;
            int maxInVersion = 0;
            byte[] pOut = null;
            int maxOutVersion = 0;
            int flags = 0;
            pCompCode = 2;
            pReason = 0x893;
            dataLength = 0;
            try
            {
                UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(hConn);
                UnmanagedHobj localHobj = this.GetLocalHobj(hObj);
                int num6 = unmanagedHconn.Value;
                int num7 = localHobj.Value;
                this.SPIQuerySPI(unmanagedHconn, 3, ref maxInOutVersion, ref maxInVersion, ref maxOutVersion, ref flags, out pCompCode, out pReason);
                if (pCompCode == 0)
                {
                    MQSPIGetInOut @out = new MQSPIGetInOut(maxInOutVersion);
                    MQSPIGetIn @in = new MQSPIGetIn(maxInVersion);
                    MQSPIGetOut out2 = new MQSPIGetOut(maxOutVersion);
                    @in.MaxMsgLength = bufferLength;
                    if (spigo != null)
                    {
                        @in.Options = spigo.Options;
                    }
                    @out.MsgDesc = md;
                    @out.GetMsgOpts = gmo;
                    out2.Msg = buffer;
                    out2.MsgLength = bufferLength;
                    out2.SPIGetOpts = spigo;
                    pInOut = @out.ToBuffer();
                    pOut = out2.ToBuffer();
                    nativeManager.zstSPI(num6, 3, num7, pInOut, @in.ToBuffer(), pOut, out pCompCode, out pReason);
                    @out.ReadStruct(pInOut, 0);
                    out2.ReadStruct(pOut, 0, true);
                    dataLength = out2.MsgLength;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void spiNotify(Hconn hconn, ref int options, LpiNotifyDetails notifyDetails, out int pCompCode, out int pReason)
        {
            uint method = 0x3ea;
            this.TrEntry(method, new object[] { hconn, (int) options, notifyDetails, "pCompCode : out", "pReason : out" });
            byte[] pInOut = null;
            int maxInOutVersion = 0;
            int maxInVersion = 0;
            byte[] pOut = null;
            int maxOutVersion = 0;
            int flags = 0;
            int verbId = 11;
            pCompCode = 0;
            pReason = 0;
            try
            {
                UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(hconn);
                int hConn = unmanagedHconn.Value;
                int hObj = 0;
                if (!unmanagedHconn.Spiqo.SPISupported(verbId, ref maxInOutVersion, ref maxInVersion, ref maxOutVersion, ref flags))
                {
                    pCompCode = 2;
                    pReason = 0x8fa;
                }
                else
                {
                    pInOut = new MQSPINotifyInOut(this.env, maxInOutVersion).ToBuffer();
                    MQSPINotifyIn @in = new MQSPINotifyIn(this.env, maxInVersion);
                    @in.Options = options;
                    @in.ConnectionId = notifyDetails.ConnectionId;
                    @in.Reason = notifyDetails.Reason;
                    pOut = new MQSPINotifyOut(this.env, maxOutVersion).ToBuffer();
                    nativeManager.zstSPI(hConn, verbId, hObj, pInOut, @in.ToBuffer(), pOut, out pCompCode, out pReason);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SpiOpen(Hconn hconn, ref MQObjectDescriptor pObjDesc, ref SpiOpenOptions pOptions, Phobj pHobj, out int pCompCode, out int pReason)
        {
            uint method = 0x3ec;
            this.TrEntry(method, new object[] { hconn, pObjDesc, pOptions, pHobj, "pCompCode : out", "pReason : out" });
            byte[] pInOut = null;
            int maxInOutVersion = 0;
            int maxInVersion = 0;
            byte[] pOut = null;
            int maxOutVersion = 0;
            int flags = 0;
            int verbId = 12;
            pCompCode = 0;
            pReason = 0;
            try
            {
                UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(hconn);
                int hConn = unmanagedHconn.Value;
                UnmanagedHobj localHobj = this.GetLocalHobj(pHobj.HOBJ);
                int hObj = localHobj.Value;
                if (!unmanagedHconn.Spiqo.SPISupported(verbId, ref maxInOutVersion, ref maxInVersion, ref maxOutVersion, ref flags))
                {
                    pCompCode = 2;
                    pReason = 0x8fa;
                }
                else
                {
                    if (IntPtr.Size != 4)
                    {
                        pObjDesc.UseNativePtrSz = false;
                    }
                    MQSPIOpenInOut @out = new MQSPIOpenInOut(this.env, maxInOutVersion);
                    pInOut = @out.ToBuffer();
                    MQSPIOpenIn @in = new MQSPIOpenIn(this.env, maxInVersion);
                    @in.OpenOptions = pOptions;
                    @in.MQOpenDescriptor = pObjDesc;
                    MQSPIOpenOut out2 = new MQSPIOpenOut(this.env, maxOutVersion);
                    out2.MQOpenDescriptor = pObjDesc;
                    pOut = out2.ToBuffer();
                    pObjDesc.TraceFields();
                    nativeManager.zstSPINewHObj(hConn, verbId, ref hObj, pInOut, @in.ToBuffer(), pOut, out pCompCode, out pReason);
                    @out.ReadStruct(pInOut, 0);
                    out2.ReadStruct(pOut, 0);
                    localHobj.Value = hObj;
                    pHobj.HOBJ = localHobj;
                    pOptions.DefPersistence = out2.OpenOptions.DefPersistence;
                    pOptions.DefPutResponseType = out2.OpenOptions.DefPutResponseType;
                    pOptions.DefReadAhead = out2.OpenOptions.DefReadAhead;
                    pOptions.PropertyControl = out2.OpenOptions.PropertyControl;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SPIPut(Hconn hConn, Hobj hObj, ref MQMessageDescriptor md, ref MQPutMessageOptions pmo, ref MQSPIPutOpts spipo, int length, byte[] buffer, out int pCompCode, out int pReason)
        {
            uint method = 0x475;
            this.TrEntry(method, new object[] { hConn, hObj, md, pmo, spipo, length, buffer, "pCompCode : out", "pReason : out" });
            byte[] pInOut = null;
            int maxInOutVersion = 0;
            int maxInVersion = 0;
            byte[] pOut = null;
            int maxOutVersion = 0;
            int flags = 0;
            pCompCode = 2;
            pReason = 0x893;
            try
            {
                UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(hConn);
                UnmanagedHobj localHobj = this.GetLocalHobj(hObj);
                int num6 = unmanagedHconn.Value;
                int num7 = localHobj.Value;
                this.SPIQuerySPI(hConn, 2, ref maxInOutVersion, ref maxInVersion, ref maxOutVersion, ref flags, out pCompCode, out pReason);
                if (pCompCode == 0)
                {
                    MQSPIPutInOut @out = new MQSPIPutInOut(maxInOutVersion);
                    MQSPIPutIn @in = new MQSPIPutIn(maxInVersion);
                    MQSPIPutOut out2 = new MQSPIPutOut(maxOutVersion);
                    @in.Msg = buffer;
                    @in.MsgLength = length;
                    if (spipo != null)
                    {
                        @in.Options = spipo.Options;
                    }
                    @out.MsgDesc = md;
                    @out.PutMsgOpts = pmo;
                    pInOut = @out.ToBuffer();
                    pOut = out2.ToBuffer();
                    nativeManager.zstSPI(num6, 2, num7, pInOut, @in.ToBuffer(), pOut, out pCompCode, out pReason);
                    @out.ReadStruct(pInOut, 0);
                    out2.ReadStruct(pOut, 0);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SPIQuerySPI(Hconn hConn, int verbId, ref int maxInOutVersion, ref int maxInVersion, ref int maxOutVersion, ref int flags, out int pCompCode, out int pReason)
        {
            uint method = 0x472;
            this.TrEntry(method, new object[] { hConn, verbId, (int) maxInOutVersion, (int) maxInVersion, (int) maxOutVersion, (int) flags, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(hConn);
                if (unmanagedHconn.Spiqo == null)
                {
                    int num2 = unmanagedHconn.Value;
                    int hObj = 0;
                    MQSPIQueryInOut @out = new MQSPIQueryInOut();
                    byte[] pInOut = null;
                    MQSPIQueryIn @in = new MQSPIQueryIn();
                    byte[] pOut = null;
                    unmanagedHconn.Spiqo = new MQSPIQueryOut();
                    pInOut = @out.ToBuffer();
                    pOut = unmanagedHconn.Spiqo.ToBuffer();
                    nativeManager.zstSPI(num2, 1, hObj, pInOut, @in.ToBuffer(), pOut, out pCompCode, out pReason);
                    if (pCompCode == 2)
                    {
                        if (pReason != 0x8fa)
                        {
                            return;
                        }
                        pCompCode = 0;
                        pReason = 0;
                    }
                    @out.ReadStruct(pInOut, 0);
                    unmanagedHconn.Spiqo.ReadStruct(pOut, 0);
                }
                if (unmanagedHconn.Spiqo.SPISupported(verbId, ref maxInOutVersion, ref maxInVersion, ref maxOutVersion, ref flags))
                {
                    pCompCode = 0;
                    pReason = 0;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void spiSubscribe(Hconn hconn, LpiSD plpiSD, MQSubscriptionDescriptor pSubDesc, Phobj pHobj, Phobj pHsub, out int pCompCode, out int pReason)
        {
            uint method = 0x3eb;
            this.TrEntry(method, new object[] { hconn, plpiSD, pSubDesc, pHobj, pHsub, "pCompCode : out", "pReason : out" });
            byte[] pInOut = null;
            int maxInOutVersion = 0;
            int maxInVersion = 0;
            byte[] pOut = null;
            int maxOutVersion = 0;
            int flags = 0;
            int verbId = 7;
            pCompCode = 0;
            pReason = 0;
            try
            {
                UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(hconn);
                int hConn = unmanagedHconn.Value;
                UnmanagedHobj localHobj = this.GetLocalHobj(pHobj.HOBJ);
                int hObj = localHobj.Value;
                UnmanagedHobj hobj2 = this.GetLocalHobj(pHsub.HOBJ);
                if (!unmanagedHconn.Spiqo.SPISupported(verbId, ref maxInOutVersion, ref maxInVersion, ref maxOutVersion, ref flags))
                {
                    pCompCode = 2;
                    pReason = 0x8fa;
                }
                else
                {
                    if (IntPtr.Size != 4)
                    {
                        pSubDesc.UseNativePtrSz = false;
                        plpiSD.UseNativePtrSz = false;
                    }
                    MQSPISubscribeInOut @out = new MQSPISubscribeInOut(this.env, maxInOutVersion);
                    pInOut = @out.ToBuffer();
                    MQSPISubscribeIn @in = new MQSPISubscribeIn(this.env, maxInVersion);
                    @in.Lpisd = plpiSD;
                    @in.SubDesc = pSubDesc;
                    MQSPISubscribeOut out2 = new MQSPISubscribeOut(this.env, maxOutVersion);
                    out2.SubDesc = pSubDesc;
                    pOut = out2.ToBuffer();
                    pSubDesc.TraceFields();
                    nativeManager.zstSPINewHObj(hConn, verbId, ref hObj, pInOut, @in.ToBuffer(), pOut, out pCompCode, out pReason);
                    @out.ReadStruct(pInOut, 0);
                    out2.ReadStruct(pOut, 0);
                    pSubDesc = out2.SubDesc;
                    plpiSD = out2.Lpisd;
                    hobj2.Value = out2.Hsub;
                    pHsub.HOBJ = hobj2;
                    localHobj.Value = hObj;
                    pHobj.HOBJ = localHobj;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SPISyncpoint(Hconn hConn, ref MQSPISyncpointOpts spispo, out int pCompCode, out int pReason)
        {
            uint method = 0x474;
            this.TrEntry(method, new object[] { hConn, spispo, "pCompCode : out", "pReason : out" });
            int maxInOutVersion = 0;
            int maxInVersion = 0;
            int maxOutVersion = 0;
            int flags = 0;
            pCompCode = 2;
            pReason = 0x893;
            try
            {
                UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(hConn);
                int num6 = unmanagedHconn.Value;
                this.SPIQuerySPI(unmanagedHconn, 5, ref maxInOutVersion, ref maxInVersion, ref maxOutVersion, ref flags, out pCompCode, out pReason);
                if (pCompCode == 0)
                {
                    MQSPISyncpointInOut @out = new MQSPISyncpointInOut(maxInOutVersion);
                    MQSPISyncpointIn @in = new MQSPISyncpointIn(maxInVersion);
                    MQSPISyncpointOut out2 = new MQSPISyncpointOut(maxOutVersion);
                    if (spispo != null)
                    {
                        @in.Options = spispo.Options;
                        @in.Action = spispo.Action;
                    }
                    int hObj = 0;
                    nativeManager.zstSPI(num6, 5, hObj, @out.ToBuffer(), @in.ToBuffer(), out2.ToBuffer(), out pCompCode, out pReason);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void zstMQGET(Hconn hConn, Hobj hObj, ref MQMessageDescriptor md, ref MQGetMessageOptions gmo, int bufferLength, byte[] buffer, out int dataLength, MQLPIGetOpts lpiGetOpts, out int pCompCode, out int pReason)
        {
            uint method = 0x477;
            this.TrEntry(method, new object[] { hConn, hObj, md, gmo, bufferLength, buffer, "dataLength : out", lpiGetOpts, "pCompCode : out", "pReason : out" });
            pCompCode = 2;
            pReason = 0x893;
            dataLength = 0;
            int pSConvs = 0;
            try
            {
                UnmanagedHconn unmanagedHconn = this.GetUnmanagedHconn(hConn);
                UnmanagedHobj localHobj = this.GetLocalHobj(hObj);
                int num3 = unmanagedHconn.Value;
                int num4 = localHobj.Value;
                this.GetSharingConversationsValue(num3, ref pSConvs);
                if (pSConvs > 0)
                {
                    MQBase.MQMD structMQMD = md.StructMQMD;
                    MQBase.MQGMO structMQGMO = gmo.StructMQGMO;
                    MQBase.lpiGETOPT structGETOPT = lpiGetOpts.StructGETOPT;
                    nativeManager.zstMQGET(num3, num4, ref structMQMD, ref structMQGMO, bufferLength, buffer, out dataLength, ref structGETOPT, out pCompCode, out pReason);
                    gmo.StructMQGMO = structMQGMO;
                    md.StructMQMD = structMQMD;
                    lpiGetOpts.StructGETOPT = structGETOPT;
                }
                else
                {
                    MQBase.MQMD mqmd = md.StructMQMD;
                    MQBase.MQGMO mqgmo = gmo.StructMQGMO;
                    nativeManager.MQGET(num3, num4, ref mqmd, ref mqgmo, bufferLength, buffer, out dataLength, out pCompCode, out pReason);
                    md.StructMQMD = mqmd;
                    gmo.StructMQGMO = mqgmo;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }
    }
}

