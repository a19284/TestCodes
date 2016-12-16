namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;

    internal class BindingsNmqiMQ : MQBase, NmqiMQ, NmqiSP
    {
        private object asyncCallbackLock = new object();
        private Hashtable asyncHconnsBindings = new Hashtable();
        private Hashtable asyncHobjsBindings = new Hashtable();
        private int DEFAULT_PAYLOAD_SIZE = 0x1400;
        private NmqiEnvironment env;
        private static readonly object initialiseLock = new object();
        private int mqiOptions;
        public MQBase.CallbackDelegate myDelegate;
        private static NativeManager nativeManager = null;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private object useWorkerThread;

        static BindingsNmqiMQ()
        {
            lock (initialiseLock)
            {
                if (nativeManager == null)
                {
                    nativeManager = new NativeManager();
                    nativeManager.InitializeNativeApis("MQSeries Bindings");
                }
            }
        }

        public BindingsNmqiMQ(NmqiEnvironment env, int options)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { env, options });
            this.env = env;
            this.mqiOptions = options;
            this.myDelegate = new MQBase.CallbackDelegate(this.NmqiConsumerMethodBindings);
        }

        public void CheckCmdLevel(Hconn hconn)
        {
            uint method = 0x379;
            this.TrEntry(method, new object[] { hconn });
            base.TrExit(method);
        }

        ~BindingsNmqiMQ()
        {
            lock (initialiseLock)
            {
                if (nativeManager != null)
                {
                    nativeManager.UnloadSwitchingLibrary();
                }
            }
        }

        private BindingsHconn GetBindingsHconn(Hconn hconn)
        {
            uint method = 790;
            this.TrEntry(method, new object[] { hconn });
            BindingsHconn hconn2 = null;
            try
            {
                hconn2 = BindingsHconn.GetBindingsHconn(this.env, this.useWorkerThread, hconn);
            }
            finally
            {
                base.TrExit(method);
            }
            return hconn2;
        }

        private BindingsHobj GetLocalHobj(Hobj hobj)
        {
            uint method = 0x317;
            this.TrEntry(method, new object[] { hobj });
            BindingsHobj bindingsHobj = null;
            try
            {
                bindingsHobj = BindingsHobj.GetBindingsHobj(this.env, hobj);
            }
            finally
            {
                base.TrExit(method);
            }
            return bindingsHobj;
        }

        public void MQBACK(Hconn hconn, out int pCompCode, out int pReason)
        {
            uint method = 880;
            this.TrEntry(method, new object[] { hconn, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                int num2 = this.GetBindingsHconn(hconn).Value;
                nativeManager.MQBACK(num2, out pCompCode, out pReason);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQBEGIN(Hconn hconn, MQBeginOptions pBeginOptions, out int pCompCode, out int pReason)
        {
            uint method = 0x371;
            this.TrEntry(method, new object[] { hconn, pBeginOptions, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                int num2 = this.GetBindingsHconn(hconn).Value;
                MQBase.MQBO structMQBO = pBeginOptions.StructMQBO;
                nativeManager.MQBEGIN(num2, ref structMQBO, out pCompCode, out pReason);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQCB(Hconn hConn, int operation, MQCBD pCallbackDesc, Hobj hobj, MQMessageDescriptor pMsgDesc, MQGetMessageOptions getMsgOpts, out int compCode, out int reason)
        {
            uint method = 0x372;
            this.TrEntry(method, new object[] { hConn, operation, pCallbackDesc, hobj, pMsgDesc, getMsgOpts, "compCode : out", "reason : out" });
            try
            {
                IntPtr ptr;
                BindingsHconn bindingsHconn = this.GetBindingsHconn(hConn);
                BindingsHobj localHobj = null;
                int key = 0;
                if (hobj != null)
                {
                    localHobj = this.GetLocalHobj(hobj);
                    localHobj.Hconn = bindingsHconn;
                    localHobj.Mqcbd = pCallbackDesc;
                }
                else
                {
                    localHobj = new BindingsHobj(this.env, -1);
                    localHobj.Hconn = bindingsHconn;
                    localHobj.Mqcbd = pCallbackDesc;
                }
                key = localHobj.Value;
                int num3 = bindingsHconn.Value;
                if ((operation & 0x100) != 0)
                {
                    lock (this.asyncCallbackLock)
                    {
                        if (pCallbackDesc.CallbackType == 1)
                        {
                            if (!this.asyncHobjsBindings.Contains(key))
                            {
                                this.asyncHobjsBindings.Add(key, localHobj);
                                base.TrText("Added hObj(" + key + ") to the HOBJ hashtable");
                            }
                        }
                        else if (!this.asyncHconnsBindings.Contains(num3))
                        {
                            this.asyncHconnsBindings.Add(num3, localHobj);
                            base.TrText(string.Concat(new object[] { "Added hConn(", num3, "), localhobj)", localHobj.Value, ")to the HCONN hashtable" }));
                        }
                        goto Label_0238;
                    }
                }
                if ((operation & 0x200) != 0)
                {
                    lock (this.asyncCallbackLock)
                    {
                        if (pCallbackDesc.CallbackType == 1)
                        {
                            if (this.asyncHobjsBindings.Contains(key))
                            {
                                this.asyncHobjsBindings.Remove(key);
                                base.TrText("Removed hObj(" + key + ") from the HOBJ hashtable");
                            }
                        }
                        else if (this.asyncHconnsBindings.Contains(num3))
                        {
                            this.asyncHconnsBindings.Remove(num3);
                            base.TrText("Removed hconn(" + num3 + ") from the HCONN hashtable");
                        }
                    }
                }
            Label_0238:
                ptr = IntPtr.Zero;
                ptr = Marshal.AllocCoTaskMem(pCallbackDesc.GetLength());
                Marshal.StructureToPtr(pCallbackDesc.StructMQCBD, ptr, false);
                IntPtr zero = IntPtr.Zero;
                zero = Marshal.AllocCoTaskMem(pMsgDesc.GetLength());
                Marshal.StructureToPtr(pMsgDesc.StructMQMD, zero, false);
                IntPtr ptr3 = IntPtr.Zero;
                ptr3 = Marshal.AllocCoTaskMem(getMsgOpts.GetLength());
                Marshal.StructureToPtr(getMsgOpts.StructMQGMO, ptr3, false);
                nativeManager.NativeACMQCB(num3, operation, key, ptr, zero, ptr3, this.myDelegate, 0, out compCode, out reason);
                if (compCode != 2)
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
            uint method = 0x318;
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
            uint method = 0x36f;
            this.TrEntry(method, new object[] { hconn, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                int num2 = this.GetBindingsHconn(hconn).Value;
                nativeManager.MQCMIT(num2, out pCompCode, out pReason);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQCONN(string pQMgrName, Phconn phconn, out int pCompCode, out int pReason)
        {
            uint method = 0x31a;
            this.TrEntry(method, new object[] { pQMgrName, phconn, "pCompCode : out", "pReason : out" });
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
                pCompCode = 2;
                pReason = 0x8e1;
                if (phconn == null)
                {
                    pCompCode = 2;
                    pReason = 0x7e2;
                }
                else
                {
                    try
                    {
                        BindingsHconn bindingsHconn = this.GetBindingsHconn(phconn.HConn);
                        int pHconn = bindingsHconn.Value;
                        nativeManager.MQCONN(pQMgrName, out pHconn, out pCompCode, out pReason);
                        bindingsHconn.Value = pHconn;
                        bindingsHconn.UpdateHconn(this, phconn);
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
            uint method = 0x31b;
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
            uint method = 0x31c;
            this.TrEntry(method, new object[] { pQMgrName, (MQBase.MQCNO) pConnectOpts, parentHconn, phconn, "pCompCode : out", "pReason : out" });
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
                if ((parentHconn != null) && !(parentHconn is BindingsHconn))
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
                    BindingsHconn bindingsHconn = this.GetBindingsHconn(phconn.HConn);
                    int pHconn = 0;
                    nativeManager.MQCONNX(pQMgrName, ref pConnectOpts, out pHconn, out pCompCode, out pReason);
                    if ((pCompCode == 0) || (pCompCode == 1))
                    {
                        bindingsHconn.Value = pHconn;
                        bindingsHconn.UpdateHconn(this, phconn);
                        bindingsHconn.Parent = parentHconn;
                        bindingsHconn.ConnectionId = pConnectOpts.ConnectionId;
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

        public void MQCTL(Hconn hConn, int operation, MQCTLO pControlOpts, out int compCode, out int reason)
        {
            uint method = 0x373;
            this.TrEntry(method, new object[] { hConn, operation, pControlOpts, "compCode : out", "reason : out" });
            try
            {
                int num2 = this.GetBindingsHconn(hConn).Value;
                IntPtr zero = IntPtr.Zero;
                zero = Marshal.AllocCoTaskMem(pControlOpts.GetLength());
                Marshal.StructureToPtr(pControlOpts.StructMQCTLO, zero, false);
                nativeManager.MQCTL(num2, operation, zero, out compCode, out reason);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQDISC(Phconn phconn, out int pCompCode, out int pReason)
        {
            uint method = 0x321;
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
                    BindingsHconn bindingsHconn = this.GetBindingsHconn(phconn.HConn);
                    int hConn = bindingsHconn.Value;
                    nativeManager.MQDISC(out hConn, out pCompCode, out pReason);
                    bindingsHconn.Value = hConn;
                    bindingsHconn.UpdateHconn(this, phconn);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQGET(Hconn hConn, Hobj hObj, MQGetMessageOptions gmo, ref MQMessage message, out int compCode, out int reason)
        {
            uint method = 0x64e;
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
                        MQLPIGetOpts getData = new MQLPIGetOpts();
                        getData.SetOptions(getData.GetOptions() | MQLPIGetOpts.lpiGETOPT_FULL_MESSAGE);
                        this.zstMQGET(hConn, hObj, ref md, ref gmo, bufferLength, buffer, out dataLength, getData, out compCode, out reason);
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
            uint method = 800;
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
                    BindingsHconn bindingsHconn = this.GetBindingsHconn(hconn);
                    BindingsHobj localHobj = this.GetLocalHobj(hobj);
                    int hConn = bindingsHconn.Value;
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
            uint method = 0x322;
            this.TrEntry(method, new object[] { hconn, hobj, SelectorCount, pSelectors, IntAttrCount, pIntAttrs, CharAttrLength, pCharAttrs, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                BindingsHconn bindingsHconn = this.GetBindingsHconn(hconn);
                BindingsHobj localHobj = this.GetLocalHobj(hobj);
                int num2 = bindingsHconn.Value;
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
            uint method = 0x31d;
            this.TrEntry(method, new object[] { phConn, pObjDesc, Options, phobj, "pCompCode : out", "pReason : out" });
            IntPtr zero = IntPtr.Zero;
            byte[] b = new byte[pObjDesc.GetRequiredBufferSize() + 50];
            try
            {
                if (((Options & 0x100000) != 0) && ((Options & 0x80000) != 0))
                {
                    MQException exception = new MQException(2, 0x7fe);
                    throw exception;
                }
                BindingsHconn bindingsHconn = this.GetBindingsHconn(phConn);
                BindingsHobj localHobj = this.GetLocalHobj(phobj.HOBJ);
                int hObj = localHobj.Value;
                pObjDesc.WriteStruct(b, 0);
                zero = Marshal.AllocCoTaskMem(b.Length);
                Marshal.Copy(b, 0, zero, b.Length);
                pObjDesc.TraceFields();
                nativeManager.MQOPEN_IntPtr(bindingsHconn.Value, zero, Options, out hObj, out pCompCode, out pReason);
                if ((pCompCode == 0) || (pCompCode == 1))
                {
                    localHobj.Value = hObj;
                    phobj.HOBJ = localHobj;
                    Marshal.Copy(zero, b, 0, b.Length);
                    pObjDesc.ReadStruct(b, 0, b.Length);
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
            uint method = 0x64d;
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
            uint method = 0x31e;
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
                    BindingsHconn bindingsHconn = this.GetBindingsHconn(hconn);
                    BindingsHobj localHobj = this.GetLocalHobj(hobj);
                    int hConn = bindingsHconn.Value;
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
            uint method = 0x31f;
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
                    int hConn = this.GetBindingsHconn(hconn).Value;
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
            uint method = 0x323;
            this.TrEntry(method, new object[] { hconn, hobj, SelectorCount, pSelectors, IntAttrCount, pIntAttrs, CharAttrLength, pCharAttrs, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                BindingsHconn bindingsHconn = this.GetBindingsHconn(hconn);
                BindingsHobj localHobj = this.GetLocalHobj(hobj);
                int num2 = bindingsHconn.Value;
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
            uint method = 0x36e;
            this.TrEntry(method, new object[] { hconn, Type, pStat, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                int num2 = this.GetBindingsHconn(hconn).Value;
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
            uint method = 0x36c;
            this.TrEntry(method, new object[] { hconn, pSubDesc, hobj, hsub, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            byte[] b = new byte[pSubDesc.GetRequiredBufferSize()];
            IntPtr zero = IntPtr.Zero;
            try
            {
                BindingsHconn bindingsHconn = this.GetBindingsHconn(hconn);
                if (hobj == null)
                {
                    hobj = new Phobj(this.env);
                }
                if (hsub == null)
                {
                    hsub = new Phobj(this.env);
                }
                BindingsHobj localHobj = this.GetLocalHobj(hobj.HOBJ);
                BindingsHobj hobj3 = this.GetLocalHobj(hsub.HOBJ);
                int hConn = bindingsHconn.Value;
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
            uint method = 0x36d;
            this.TrEntry(method, new object[] { hconn, hsub, action, pSubReq, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                BindingsHconn bindingsHconn = this.GetBindingsHconn(hconn);
                BindingsHobj localHobj = this.GetLocalHobj(hsub);
                int hConn = bindingsHconn.Value;
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
            uint method = 0x374;
            this.TrEntry(method, new object[] { name, pNmqiConnectOpts, cno, parentHconn, pHconn, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            IntPtr ptr3 = IntPtr.Zero;
            try
            {
                if (cno == null)
                {
                    pCompCode = 2;
                    pReason = 0x85b;
                }
                else if ((parentHconn != null) && !(parentHconn is BindingsHconn))
                {
                    pCompCode = 2;
                    pReason = 0x8f6;
                }
                if (pHconn == null)
                {
                    pCompCode = 2;
                    pReason = 0x7e2;
                }
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
                    BindingsHconn bindingsHconn = this.GetBindingsHconn(pHconn.HConn);
                    int num2 = 0;
                    cno.TraceFields();
                    MQBase.MQCNO structMQCNO = cno.StructMQCNO;
                    nativeManager.MQCONNX(name, ref structMQCNO, out num2, out pCompCode, out pReason);
                    cno.StructMQCNO = structMQCNO;
                    if ((pCompCode == 0) || (pCompCode == 1))
                    {
                        bindingsHconn.Value = num2;
                        bindingsHconn.UpdateHconn(this, pHconn);
                        bindingsHconn.Parent = parentHconn;
                        bindingsHconn.PHconn = pHconn;
                        bindingsHconn.ConnectionId = structMQCNO.ConnectionId;
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
            uint method = 0x4cd;
            this.TrEntry(method, new object[] { name, pNmqiConnectOpts, cno, parentHconn, pHconn, "pCompCode : out", "pReason : out", rcnHconn });
            pCompCode = 2;
            pReason = 0x8fa;
            base.TrExit(method);
        }

        public void NmqiConsumerMethodBindings(int hconn, IntPtr structMqmd, IntPtr structMqgmo, IntPtr buffer, IntPtr structMqcbc)
        {
            uint method = 0x400;
            this.TrEntry(method, new object[] { hconn, structMqmd, structMqgmo, buffer, structMqcbc });
            BindingsHobj hobj = null;
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
                            if (this.asyncHconnsBindings.Contains(hconn))
                            {
                                hobj = (BindingsHobj) this.asyncHconnsBindings[hconn];
                                base.TrText(string.Concat(new object[] { "PHConn(", hobj.Hconn.Value, "), localhobj(", hobj.Value, ") selected." }));
                            }
                        }
                        else
                        {
                            int hOBJ = mqcbc.HOBJ;
                            if (this.asyncHobjsBindings.Contains(hOBJ))
                            {
                                hobj = (BindingsHobj) this.asyncHobjsBindings[hOBJ];
                                base.TrText(string.Concat(new object[] { "PHConn(", hobj.Hconn.Value, "), localhobj(", hobj.Value, ") selected." }));
                            }
                        }
                    }
                }
                pHconn = hobj.Hconn.PHconn;
                hobj.Mqcbd.MqConsumer.Consumer(pHconn, mqmd, getMsgOpts, destination, mqcbc);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public bool NmqiConvertMessage(Hconn hconn, Hobj hobj, int reqEncoding, int reqCCSID, int appOptions, bool callExitOnLenErr, MQMessageDescriptor md, byte[] buffer, out int dataLength, int availableLength, int bufferLength, out int compCode, out int reason, out int returnedLength)
        {
            uint method = 890;
            this.TrEntry(method, new object[] { hconn, hobj, reqEncoding, reqCCSID, appOptions, callExitOnLenErr, md, buffer, "dataLength : out", availableLength, bufferLength, "compCode : out", "reason : out", "returnedLength : out" });
            compCode = 2;
            reason = 0x8fa;
            dataLength = 0;
            returnedLength = 0;
            base.TrExit(method, false);
            return false;
        }

        public void NmqiGetMessage(Hconn hconn, Hobj hobj, MQMessageDescriptor md, MQGetMessageOptions gmo, int bufferLength, byte[] buffer, out int dataLength, out int compCode, out int reason)
        {
            uint method = 0x37b;
            this.TrEntry(method, new object[] { hconn, hobj, md, gmo, bufferLength, buffer, "dataLength : out", "compCode : out", "reason : out" });
            compCode = 2;
            reason = 0x8fa;
            dataLength = 0;
            base.TrExit(method);
        }

        public void NmqiPut(Hconn hconn, Hobj hobj, MQMessageDescriptor msgDesc, MQPutMessageOptions putMsgOpts, MemoryStream[] buffers, out int compCode, out int reason)
        {
            uint method = 0x444;
            this.TrEntry(method, new object[] { hconn, hobj, msgDesc, putMsgOpts, buffers, "compCode : out", "reason : out" });
            int numBuffs = (buffers == null) ? 0 : buffers.Length;
            try
            {
                this.NmqiPut(hconn, hobj, msgDesc, putMsgOpts, 0, null, buffers, numBuffs, out compCode, out reason);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void NmqiPut(Hconn hconn, Hobj hobj, MQMessageDescriptor msgDesc, MQPutMessageOptions putMsgOpts, int sbLength, byte[] sBuff, MemoryStream[] mBuffs, int numBuffs, out int compCode, out int reason)
        {
            uint method = 0x445;
            this.TrEntry(method, new object[] { hconn, hobj, msgDesc, putMsgOpts, sbLength, sBuff, mBuffs, numBuffs, "compCode : out", "reason : out" });
            try
            {
                if ((sBuff != null) && (mBuffs != null))
                {
                    compCode = 2;
                    reason = 0x7d4;
                }
                else
                {
                    byte[] dst = null;
                    int num2 = 0;
                    if (sBuff != null)
                    {
                        dst = sBuff;
                    }
                    else if (mBuffs != null)
                    {
                        int dstOffset = 0;
                        foreach (MemoryStream stream in mBuffs)
                        {
                            if (stream == null)
                            {
                                compCode = 2;
                                reason = 0x7d4;
                                return;
                            }
                            num2 += (int) stream.Length;
                        }
                        dst = new byte[num2];
                        foreach (MemoryStream stream2 in mBuffs)
                        {
                            Buffer.BlockCopy(stream2.GetBuffer(), 0, dst, dstOffset, (int) stream2.Length);
                            dstOffset += (int) stream2.Length;
                        }
                    }
                    this.MQPUT(hconn, hobj, msgDesc, putMsgOpts, dst.Length, dst, out compCode, out reason);
                }
            }
            catch (Exception exception)
            {
                base.TrException(method, exception);
                if ((exception is NmqiException) || (exception is MQException))
                {
                    throw;
                }
                MQException exception2 = new MQException(2, 0x893, exception);
                throw exception2;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void NmqiPut1(Hconn hconn, MQObjectDescriptor objDesc, MQMessageDescriptor msgDesc, MQPutMessageOptions putMsgOpts, MemoryStream[] buffers, out int compCode, out int reason)
        {
            uint method = 0x446;
            this.TrEntry(method, new object[] { hconn, objDesc, msgDesc, putMsgOpts, buffers, "compCode : out", "reason : out" });
            int numBuffs = (buffers == null) ? 0 : buffers.Length;
            try
            {
                this.NmqiPut1(hconn, objDesc, msgDesc, putMsgOpts, 0, null, buffers, numBuffs, out compCode, out reason);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void NmqiPut1(Hconn hconn, MQObjectDescriptor objDesc, MQMessageDescriptor msgDesc, MQPutMessageOptions putMsgOpts, int sbLength, byte[] sBuff, MemoryStream[] mBuffs, int numBuffs, out int compCode, out int reason)
        {
            uint method = 0x447;
            this.TrEntry(method, new object[] { hconn, objDesc, msgDesc, putMsgOpts, sbLength, sBuff, mBuffs, numBuffs, "compCode : out", "reason : out" });
            try
            {
                if ((sBuff != null) && (mBuffs != null))
                {
                    compCode = 2;
                    reason = 0x7d4;
                }
                else
                {
                    byte[] dst = null;
                    int num2 = 0;
                    if (sBuff != null)
                    {
                        dst = sBuff;
                    }
                    else if (mBuffs != null)
                    {
                        int dstOffset = 0;
                        foreach (MemoryStream stream in mBuffs)
                        {
                            if (stream == null)
                            {
                                compCode = 2;
                                reason = 0x7d4;
                                return;
                            }
                            num2 += (int) stream.Length;
                        }
                        dst = new byte[num2];
                        foreach (MemoryStream stream2 in mBuffs)
                        {
                            Buffer.BlockCopy(stream2.GetBuffer(), 0, dst, dstOffset, (int) stream2.Length);
                            dstOffset += (int) stream2.Length;
                        }
                    }
                    this.MQPUT1(hconn, objDesc, msgDesc, putMsgOpts, dst.Length, dst, out compCode, out reason);
                }
            }
            catch (Exception exception)
            {
                base.TrException(method, exception);
                if ((exception is NmqiException) || (exception is MQException))
                {
                    throw;
                }
                MQException exception2 = new MQException(2, 0x893, exception);
                throw exception2;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void PerformMsgProcessgingBeforePut(ref MQMessage mqMsg)
        {
            uint method = 0x64f;
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
            uint method = 0x650;
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
            uint method = 0x651;
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

        public void SPIActivateMessage(Hconn hConn, ref MQSPIActivateOpts spiao, out int compCode, out int reason)
        {
            uint method = 0x456;
            this.TrEntry(method, new object[] { hConn, spiao, "compCode : out", "reason : out" });
            compCode = 2;
            reason = 0x893;
            try
            {
                int num2 = this.GetBindingsHconn(hConn).Value;
                nativeManager.lpiSPIActivateMessage(num2, ref spiao.lpiActivateOpts, out compCode, out reason);
            }
            catch (NmqiException exception)
            {
                base.TrException(method, exception, 1);
                this.env.LastException = exception;
            }
            catch (MQException exception2)
            {
                base.TrException(method, exception2, 2);
                this.env.LastException = exception2;
            }
            catch (Exception exception3)
            {
                base.TrException(method, exception3, 3);
                this.env.LastException = exception3;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SpiConnect(string pQMgrName, SpiConnectOptions pSpiConnectOpts, MQConnectOptions pConnectOpts, Phconn pHconn, out int pCompCode, out int pReason)
        {
            uint method = 0x375;
            this.TrEntry(method, new object[] { pQMgrName, pSpiConnectOpts, pConnectOpts, pHconn, "pCompCode : out", "pReason : out" });
            pCompCode = 2;
            pReason = 0x8fa;
            base.TrExit(method);
        }

        public void SpiGet(Hconn hconn, Hobj hobj, MQMessageDescriptor pMqmd, MQGetMessageOptions pMqgmo, SpiGetOptions spiOptions, int bufferLength, byte[] pBuffer, out int dataLength, out int pCompCode, out int pReason)
        {
            uint method = 0x378;
            this.TrEntry(method, new object[] { hconn, hobj, pMqmd, pMqgmo, spiOptions, bufferLength, pBuffer, "dataLength : out", "pCompCode : out", "pReason : out" });
            pCompCode = 2;
            pReason = 0x8fa;
            dataLength = 0;
            base.TrExit(method);
        }

        public void SPIGet(Hconn hConn, Hobj hObj, ref MQMessageDescriptor md, ref MQGetMessageOptions gmo, ref MQSPIGetOpts spigo, int bufferLength, byte[] buffer, out int dataLength, out int compCode, out int reason)
        {
            uint method = 0x459;
            this.TrEntry(method, new object[] { hConn, hObj, md, gmo, spigo, bufferLength, buffer, "dataLength : out", "compCode : out", "reason : out" });
            compCode = 2;
            reason = 0x893;
            dataLength = 0;
            try
            {
                BindingsHconn bindingsHconn = this.GetBindingsHconn(hConn);
                BindingsHobj localHobj = this.GetLocalHobj(hObj);
                int num2 = bindingsHconn.Value;
                int num3 = localHobj.Value;
                MQBase.MQMD structMQMD = md.StructMQMD;
                MQBase.MQGMO structMQGMO = gmo.StructMQGMO;
                nativeManager.lpiSPIGet(num2, num3, ref structMQMD, ref structMQGMO, bufferLength, buffer, out dataLength, ref spigo.lpiGetOpt, out compCode, out reason);
                md.StructMQMD = structMQMD;
                gmo.StructMQGMO = structMQGMO;
            }
            catch (NmqiException exception)
            {
                base.TrException(method, exception, 1);
                this.env.LastException = exception;
            }
            catch (MQException exception2)
            {
                base.TrException(method, exception2, 2);
                this.env.LastException = exception2;
            }
            catch (Exception exception3)
            {
                base.TrException(method, exception3, 3);
                this.env.LastException = exception3;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void spiNotify(Hconn hconn, ref int options, LpiNotifyDetails notifyDetails, out int pCompCode, out int pReason)
        {
            uint method = 0x37c;
            this.TrEntry(method, new object[] { hconn, (int) options, notifyDetails, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                int num2 = this.GetBindingsHconn(hconn).Value;
                MQBase.structLPINOTIFYDETAILS structLpiNotifyDetails = notifyDetails.StructLpiNotifyDetails;
                nativeManager.lpiSPINotify(num2, options, ref structLpiNotifyDetails, out pCompCode, out pReason);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SpiOpen(Hconn hconn, ref MQObjectDescriptor pObjDesc, ref SpiOpenOptions pOptions, Phobj pHobj, out int pCompCode, out int pReason)
        {
            uint method = 0x37e;
            this.TrEntry(method, new object[] { hconn, pObjDesc, pOptions, pHobj, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            byte[] b = new byte[pObjDesc.GetRequiredBufferSize()];
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            try
            {
                BindingsHconn bindingsHconn = this.GetBindingsHconn(hconn);
                BindingsHobj localHobj = this.GetLocalHobj(pHobj.HOBJ);
                int hConn = bindingsHconn.Value;
                int num3 = localHobj.Value;
                pObjDesc.WriteStruct(b, 0);
                zero = Marshal.AllocCoTaskMem(b.Length);
                Marshal.Copy(b, 0, zero, b.Length);
                ptr = Marshal.AllocCoTaskMem(pOptions.GetLength());
                Marshal.StructureToPtr(pOptions.StructSPIOPENOPTIONS, ptr, false);
                pObjDesc.TraceFields();
                nativeManager.lpiSPIOpen(hConn, zero, ptr, out num3, out pCompCode, out pReason);
                if ((pCompCode == 0) || (pCompCode == 1))
                {
                    localHobj.Value = num3;
                    pHobj.HOBJ = localHobj;
                    pOptions.StructSPIOPENOPTIONS = (MQBase.structSPIOPENOPTIONS) Marshal.PtrToStructure(ptr, typeof(MQBase.structSPIOPENOPTIONS));
                    Marshal.Copy(zero, b, 0, b.Length);
                    pObjDesc.ReadStruct(b, 0, b.Length);
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
                base.TrExit(method);
            }
        }

        public void SPIPut(Hconn hConn, Hobj hObj, ref MQMessageDescriptor md, ref MQPutMessageOptions pmo, ref MQSPIPutOpts spipo, int length, byte[] buffer, out int compCode, out int reason)
        {
            uint method = 0x458;
            this.TrEntry(method, new object[] { hConn, hObj, md, pmo, spipo, length, buffer, "compCode : out", "reason : out" });
            compCode = 2;
            reason = 0x893;
            try
            {
                BindingsHconn bindingsHconn = this.GetBindingsHconn(hConn);
                BindingsHobj localHobj = this.GetLocalHobj(hObj);
                int num2 = bindingsHconn.Value;
                int num3 = localHobj.Value;
                MQBase.MQMD structMQMD = md.StructMQMD;
                MQBase.MQPMO structMQPMO = pmo.StructMQPMO;
                nativeManager.lpiSPIPut(num2, num3, ref structMQMD, ref structMQPMO, length, buffer, ref spipo.lpiPutOpts, out compCode, out reason);
                md.StructMQMD = structMQMD;
                pmo.StructMQPMO = structMQPMO;
            }
            catch (NmqiException exception)
            {
                base.TrException(method, exception, 1);
                this.env.LastException = exception;
            }
            catch (MQException exception2)
            {
                base.TrException(method, exception2, 2);
                this.env.LastException = exception2;
            }
            catch (Exception exception3)
            {
                base.TrException(method, exception3, 3);
                this.env.LastException = exception3;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SPIQuerySPI(Hconn hConn, int verbId, ref int maxInOutVersion, ref int maxInVersion, ref int maxOutVersion, ref int flags, out int compCode, out int reason)
        {
            uint method = 0x455;
            this.TrEntry(method, new object[] { hConn, verbId, (int) maxInOutVersion, (int) maxInVersion, (int) maxOutVersion, (int) flags, "compCode : out", "reason : out" });
            compCode = 0;
            reason = 0;
            try
            {
                switch (verbId)
                {
                    case 2:
                        maxInOutVersion = 1;
                        maxInVersion = 2;
                        maxOutVersion = 1;
                        flags = 0;
                        return;

                    case 3:
                        maxInOutVersion = 2;
                        maxInVersion = 2;
                        maxOutVersion = 3;
                        flags = 0;
                        return;

                    case 4:
                        maxInOutVersion = 1;
                        maxInVersion = 1;
                        maxOutVersion = 1;
                        flags = 0;
                        return;

                    case 5:
                        maxInOutVersion = 1;
                        maxInVersion = 1;
                        maxOutVersion = 1;
                        flags = 0;
                        return;
                }
                compCode = 2;
                reason = 0x8fa;
            }
            catch (NmqiException exception)
            {
                base.TrException(method, exception, 1);
                this.env.LastException = exception;
            }
            catch (MQException exception2)
            {
                base.TrException(method, exception2, 2);
                this.env.LastException = exception2;
            }
            catch (Exception exception3)
            {
                base.TrException(method, exception3, 3);
                this.env.LastException = exception3;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void spiSubscribe(Hconn hconn, LpiSD plpiSD, MQSubscriptionDescriptor pSubDesc, Phobj pHobj, Phobj pHsub, out int pCompCode, out int pReason)
        {
            uint method = 0x37d;
            this.TrEntry(method, new object[] { hconn, plpiSD, pSubDesc, pHobj, pHsub, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            byte[] b = new byte[plpiSD.GetRequiredBufferSize()];
            IntPtr zero = IntPtr.Zero;
            byte[] buffer2 = new byte[pSubDesc.GetRequiredBufferSize()];
            IntPtr destination = IntPtr.Zero;
            try
            {
                BindingsHconn bindingsHconn = this.GetBindingsHconn(hconn);
                BindingsHobj localHobj = this.GetLocalHobj(pHobj.HOBJ);
                BindingsHobj hobj2 = this.GetLocalHobj(pHsub.HOBJ);
                int hConn = bindingsHconn.Value;
                int num3 = localHobj.Value;
                int num4 = hobj2.Value;
                plpiSD.WriteStruct(b, 0);
                zero = Marshal.AllocCoTaskMem(b.Length);
                Marshal.Copy(b, 0, zero, b.Length);
                pSubDesc.WriteStruct(buffer2, 0);
                destination = Marshal.AllocCoTaskMem(buffer2.Length);
                Marshal.Copy(buffer2, 0, destination, buffer2.Length);
                pSubDesc.TraceFields();
                nativeManager.lpiSPISubscribe(hConn, zero, destination, ref num3, out num4, out pCompCode, out pReason);
                if ((pCompCode == 0) || (pCompCode == 1))
                {
                    hobj2.Value = num4;
                    pHsub.HOBJ = hobj2;
                    localHobj.Value = num3;
                    pHobj.HOBJ = localHobj;
                    Marshal.Copy(destination, buffer2, 0, buffer2.Length);
                    pSubDesc.ReadStruct(buffer2, 0, buffer2.Length);
                    Marshal.Copy(zero, b, 0, b.Length);
                    plpiSD.ReadStruct(b, 0);
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(zero);
                }
                if (destination != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(destination);
                }
                base.TrExit(method);
            }
        }

        public void SPISyncpoint(Hconn hConn, ref MQSPISyncpointOpts spispo, out int compCode, out int reason)
        {
            uint method = 0x457;
            this.TrEntry(method, new object[] { hConn, spispo, "compCode : out", "reason : out" });
            compCode = 2;
            reason = 0x893;
            try
            {
                int num2 = this.GetBindingsHconn(hConn).Value;
                nativeManager.lpiSPISyncPoint(num2, ref spispo.lpiSyncpointOpts, out compCode, out reason);
            }
            catch (NmqiException exception)
            {
                base.TrException(method, exception, 1);
                this.env.LastException = exception;
            }
            catch (MQException exception2)
            {
                base.TrException(method, exception2, 2);
                this.env.LastException = exception2;
            }
            catch (Exception exception3)
            {
                base.TrException(method, exception3, 3);
                this.env.LastException = exception3;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void zstMQGET(Hconn hConn, Hobj hObj, ref MQMessageDescriptor md, ref MQGetMessageOptions gmo, int bufferLength, byte[] buffer, out int dataLength, MQLPIGetOpts GetData, out int compCode, out int reason)
        {
            uint method = 0x45a;
            this.TrEntry(method, new object[] { hConn, hObj, md, gmo, bufferLength, buffer, "dataLength : out", GetData, "compCode : out", "reason : out" });
            compCode = 2;
            reason = 0x893;
            dataLength = 0;
            try
            {
                BindingsHconn bindingsHconn = this.GetBindingsHconn(hConn);
                BindingsHobj localHobj = this.GetLocalHobj(hObj);
                int num2 = bindingsHconn.Value;
                int num3 = localHobj.Value;
                MQBase.MQMD structMQMD = md.StructMQMD;
                MQBase.MQGMO structMQGMO = gmo.StructMQGMO;
                MQBase.lpiGETOPT structGETOPT = GetData.StructGETOPT;
                nativeManager.zstMQGET_Bindings(num2, num3, ref structMQMD, ref structMQGMO, bufferLength, buffer, out dataLength, ref structGETOPT, out compCode, out reason);
                md.StructMQMD = structMQMD;
                gmo.StructMQGMO = structMQGMO;
                GetData.StructGETOPT = structGETOPT;
            }
            catch (NmqiException exception)
            {
                base.TrException(method, exception, 1);
                this.env.LastException = exception;
            }
            catch (MQException exception2)
            {
                base.TrException(method, exception2, 2);
                this.env.LastException = exception2;
            }
            catch (Exception exception3)
            {
                base.TrException(method, exception3, 3);
                this.env.LastException = exception3;
            }
            finally
            {
                base.TrExit(method);
            }
        }
    }
}

