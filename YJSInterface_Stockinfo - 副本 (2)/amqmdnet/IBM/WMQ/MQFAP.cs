namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Collections;
    using System.IO;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Text;
    using System.Threading;
    using System.Transactions;

    public class MQFAP : NmqiObject, NmqiMQ, NmqiSP, NmqiXA
    {
        private static MQCommsBufferPool commsBufferPool;
        private MQFAPConnectionPool connectionFactory;
        private int DEFAULT_PAYLOAD_SIZE;
        private static int incrementingHconn = 4;
        private static MQChannelListEntry nameList = null;
        internal object nameListLock;
        private MQManagedReconnectableThread reconnectThread;
        private Lock reconnectThreadLock;
        private static MQChannelListEntry removeNameList = null;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private static int traceIdentifier = 0;

        static MQFAP()
        {
            if (commsBufferPool == null)
            {
                commsBufferPool = new MQCommsBufferPool();
            }
        }

        public MQFAP(NmqiEnvironment env) : base(env)
        {
            this.nameListLock = new object();
            this.reconnectThreadLock = new Lock();
            this.DEFAULT_PAYLOAD_SIZE = 0x1400;
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { env });
            this.connectionFactory = new MQFAPConnectionPool(env, this);
        }

        public void CheckCmdLevel(Hconn hconn)
        {
            uint method = 0x5c7;
            this.TrEntry(method, new object[] { hconn });
            base.TrExit(method);
        }

        private void CheckGetOptions(MQGetMessageOptions getMsgOpts, ManagedHobj remoteHobj)
        {
            uint method = 0x5be;
            this.TrEntry(method, new object[] { getMsgOpts, remoteHobj });
            try
            {
                if ((getMsgOpts.Options & 0x830) != 0)
                {
                    if ((remoteHobj.OpenOptions & 8) == 0)
                    {
                        NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x7f4, null);
                        base.TrException(method, ex);
                        throw ex;
                    }
                }
                else if ((remoteHobj.OpenOptions & 7) == 0)
                {
                    NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x7f5, null);
                    base.TrException(method, exception2);
                    throw exception2;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private MQChannelListEntry CreateNameListEntryFromCCDT(string qMgrName)
        {
            uint method = 0x5a2;
            this.TrEntry(method, new object[] { qMgrName });
            MQChannelListEntry result = null;
            try
            {
                result = this.CreateNameListEntryFromCCDT(qMgrName, null);
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        private MQChannelListEntry CreateNameListEntryFromCCDT(string qMgrName, string ccdtFile)
        {
            uint method = 0x5a3;
            this.TrEntry(method, new object[] { qMgrName, ccdtFile });
            MQChannelListEntry nameList = null;
            try
            {
                MQChannelTable table = new MQChannelTable(base.env);
                if (ccdtFile == null)
                {
                    table = new MQChannelTable(base.env);
                }
                else
                {
                    table = new MQChannelTable(base.env, ccdtFile);
                }
                nameList = new MQChannelListEntry();
                nameList.Name = qMgrName;
                nameList.UseCount = 1;
                nameList.UpdateRequired = false;
                nameList.TotalWeight = 0;
                nameList.ChannelFile = table.CCDTFile;
                string cCDTFile = table.CCDTFile;
                if (cCDTFile.StartsWith("file://"))
                {
                    cCDTFile = table.CCDTFile.Substring("file://".Length);
                }
                FileInfo info = new FileInfo(cCDTFile);
                nameList.ModTime = info.LastWriteTime.ToFileTime();
                table.CreateChannelEntryLists(nameList);
                lock (this.nameListLock)
                {
                    nameList.Next = MQFAP.nameList;
                    MQFAP.nameList = nameList;
                }
                return nameList;
            }
            catch (MQException exception)
            {
                NmqiException ex = new NmqiException(base.env, -1, null, exception.CompCode, exception.Reason, exception);
                base.TrException(method, ex, 1);
                throw ex;
            }
            catch (IOException exception3)
            {
                NmqiException exception4 = new NmqiException(base.env, -1, null, 2, 0x8a0, exception3);
                base.TrException(method, exception4, 2);
                throw exception4;
            }
            catch (Exception exception5)
            {
                NmqiException exception6 = new NmqiException(base.env, -1, null, 2, 0x8e5, exception5);
                base.TrException(method, exception6, 3);
                throw exception6;
            }
            finally
            {
                base.TrExit(method);
            }
            return nameList;
        }

        internal void Disconnect(ManagedHconn remoteHconn, out int pCompCode, out int pReason)
        {
            uint method = 0x5b0;
            this.TrEntry(method, new object[] { remoteHconn, "pCompCode : out", "pReason : out" });
            MQSession failedSession = null;
            MQAPI mqapi = new MQAPI();
            MQTSH tsh = null;
            MQTSH rTSH = null;
            bool flag = CommonServices.TraceEnabled();
            int num2 = 0;
            int num3 = 0x10;
            int translength = 0;
            pCompCode = 0;
            pReason = 0;
            this.InExit();
            try
            {
                failedSession = remoteHconn.Session;
                num2 = failedSession.IsMultiplexingEnabled ? 0x24 : 0x1c;
                translength = num2 + num3;
                if (remoteHconn.IsReconnectable)
                {
                    remoteHconn.EligibleForReconnect(failedSession, false);
                }
                MQFAPConnection parentConnection = remoteHconn.ParentConnection;
                remoteHconn.EnterCall(this.IsDispatchThread(), true);
                try
                {
                    failedSession.CheckIfDisconnected();
                    if (remoteHconn.FapLevel >= 4)
                    {
                        if (flag)
                        {
                            base.TrAPI(method, "__________");
                            base.TrAPI(method, "MQDISC >>");
                            base.TrData(method, 0, "Hconn", BitConverter.GetBytes(remoteHconn.Value));
                            base.TrAPIOutput(method, "CompCode");
                            base.TrAPIOutput(method, "Reason");
                        }
                        tsh = failedSession.AllocateTSH(130, 0, true, translength);
                        mqapi.Initialize(translength, remoteHconn.Value);
                        byte[] tshBuffer = tsh.TshBuffer;
                        int offset = tsh.WriteStruct(tshBuffer, 0);
                        offset += mqapi.WriteStruct(tshBuffer, offset);
                        tsh.TshBuffer = tshBuffer;
                        offset = 0;
                        failedSession.SendTSH(tsh);
                        rTSH = failedSession.ReceiveTSH(null);
                        try
                        {
                            switch (rTSH.SegmentType)
                            {
                                case 5:
                                {
                                    MQERD mqerd = new MQERD();
                                    if (rTSH.Length > num2)
                                    {
                                        mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                    }
                                    throw mqerd.ErrorException(failedSession.ParentConnection.NegotiatedChannel.ChannelName.Trim());
                                }
                                case 0x92:
                                    break;

                                default:
                                    CommonServices.SetValidInserts();
                                    CommonServices.ArithInsert1 = rTSH.SegmentType;
                                    CommonServices.CommentInsert1 = failedSession.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                                    CommonServices.CommentInsert2 = "Unexpected flow received during MQDISC call";
                                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                                    throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, failedSession.ParentConnection.NegotiatedChannel.ChannelName.Trim());
                            }
                            mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            pCompCode = mqapi.mqapi.CompCode;
                            pReason = mqapi.mqapi.Reason;
                        }
                        finally
                        {
                            if (rTSH != null)
                            {
                                failedSession.ReleaseReceivedTSH(rTSH);
                            }
                        }
                    }
                    if (pCompCode != 2)
                    {
                        failedSession.Disconnect();
                        remoteHconn.Value = -1;
                    }
                }
                finally
                {
                    try
                    {
                        if (remoteHconn != null)
                        {
                            remoteHconn.LeaveCall(pReason);
                        }
                    }
                    catch (NmqiException exception)
                    {
                        base.TrException(method, exception, 1);
                        pCompCode = exception.CompCode;
                        pReason = exception.Reason;
                    }
                }
            }
            catch (NmqiException exception2)
            {
                base.TrException(method, exception2, 2);
                pCompCode = exception2.CompCode;
                pReason = exception2.Reason;
            }
            finally
            {
                if (((pReason != 0) && (remoteHconn != null)) && remoteHconn.IsReconnectable)
                {
                    if (ManagedHconn.IsReconnectableReasonCode(pReason) && !remoteHconn.HasFailed())
                    {
                        pCompCode = 0;
                        pReason = 0;
                    }
                    if (remoteHconn.HasFailed())
                    {
                        pCompCode = remoteHconn.ReconnectionFailureCompCode;
                        pReason = remoteHconn.ReconnectionFailureReason;
                    }
                }
                if (flag)
                {
                    base.TrAPI(method, "__________");
                    base.TrAPI(method, "MQDISC <<");
                    base.TrAPIInput(method, "Hconn");
                    base.TrAPICCRC(method, pCompCode, pReason);
                }
                base.TrExit(method);
            }
        }

        private MQChannelListEntry FindNameListEntry(string qMgrName)
        {
            uint method = 0x5a1;
            this.TrEntry(method, new object[] { qMgrName });
            MQChannelListEntry nameList = MQFAP.nameList;
            MQChannelListEntry entry2 = null;
            try
            {
                while (nameList != null)
                {
                    if (nameList.Name.CompareTo(qMgrName) == 0)
                    {
                        break;
                    }
                    entry2 = nameList;
                    nameList = nameList.Next;
                }
                if (nameList != null)
                {
                    nameList.CheckUpdateRequired();
                    if (nameList.UpdateRequired)
                    {
                        if (entry2 != null)
                        {
                            entry2.Next = nameList.Next;
                        }
                        else
                        {
                            nameList = nameList.Next;
                        }
                        if (nameList != null)
                        {
                            nameList.Next = removeNameList;
                            removeNameList = nameList;
                        }
                    }
                    else
                    {
                        nameList.UseCount++;
                    }
                }
                if (removeNameList != null)
                {
                    this.FreeNameEntries();
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return nameList;
        }

        private void FreeNameEntries()
        {
            uint method = 0x5a5;
            this.TrEntry(method);
            try
            {
                MQChannelListEntry entry = null;
                MQChannelListEntry entry2 = null;
                MQChannelListEntry next = null;
                MQChannelEntry alphaEntry = null;
                MQChannelEntry weightedEntry = null;
                MQChannelEntry entry6 = null;
                for (entry = removeNameList; entry != null; entry = next)
                {
                    next = entry.Next;
                    if (entry.UseCount == 0)
                    {
                        alphaEntry = entry.AlphaEntry;
                        while (alphaEntry != null)
                        {
                            entry6 = alphaEntry.Next;
                            alphaEntry.Channel = null;
                            alphaEntry = null;
                            alphaEntry = entry6;
                        }
                        alphaEntry = entry.WeightedEntry;
                        weightedEntry = entry.WeightedEntry;
                        while (alphaEntry != null)
                        {
                            entry6 = alphaEntry.Next;
                            alphaEntry.Channel = null;
                            alphaEntry = null;
                            alphaEntry = entry6;
                            if (alphaEntry.Equals(weightedEntry))
                            {
                                break;
                            }
                        }
                        if (entry.ChannelFile != null)
                        {
                            entry.ChannelFile = null;
                        }
                        if (entry2 != null)
                        {
                            entry2.Next = entry.Next;
                        }
                        else
                        {
                            removeNameList = entry.Next;
                        }
                        entry = null;
                        continue;
                    }
                    entry2 = entry;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private ManagedHconn GetManagedHconn(Hconn hconn)
        {
            uint method = 0x5a9;
            this.TrEntry(method, new object[] { hconn });
            ManagedHconn hconn2 = null;
            try
            {
                hconn2 = (ManagedHconn) hconn;
            }
            finally
            {
                base.TrExit(method);
            }
            return hconn2;
        }

        private ManagedHobj GetManagedHobj(Hobj hobj)
        {
            uint method = 0x5aa;
            this.TrEntry(method, new object[] { hobj });
            ManagedHobj managedHobj = null;
            try
            {
                managedHobj = ManagedHobj.GetManagedHobj(base.env, hobj);
            }
            finally
            {
                base.TrExit(method);
            }
            return managedHobj;
        }

        private MQChannelListEntry GetNameList(string qMgrName)
        {
            uint method = 0x5a0;
            this.TrEntry(method, new object[] { qMgrName });
            MQChannelListEntry entry = null;
            string str = null;
            try
            {
                if (qMgrName != null)
                {
                    int index = qMgrName.IndexOf('*');
                    if (index != -1)
                    {
                        if (index != 0)
                        {
                            throw new MQManagedClientException(2, 0x80a);
                        }
                        if (qMgrName.Length != 1)
                        {
                            str = qMgrName.Substring(1);
                        }
                    }
                    else
                    {
                        str = qMgrName;
                    }
                }
                lock (this.nameListLock)
                {
                    entry = this.FindNameListEntry(str);
                    if ((entry != null) && !entry.UpdateRequired)
                    {
                        return entry;
                    }
                    return this.CreateNameListEntryFromCCDT(str);
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return entry;
        }

        private MQChannelListEntry GetNameListFromMQCD(MQChannelDefinition mqcd, string name)
        {
            uint method = 0x5a4;
            this.TrEntry(method, new object[] { mqcd, name });
            MQChannelListEntry entry = null;
            try
            {
                entry = new MQChannelListEntry();
                entry.Name = name;
                entry.UseCount = 1;
                entry.UpdateRequired = false;
                entry.TotalWeight = 0;
                entry.ModTime = DateTime.Now.ToFileTime();
                entry.Next = nameList;
                nameList = entry;
                ArrayList list = new ArrayList();
                string str = ",";
                string[] strArray = mqcd.ConnectionName.Split(str.ToCharArray());
                for (int i = 0; i < strArray.Length; i++)
                {
                    try
                    {
                        MQChannelDefinition definition = mqcd.Clone();
                        definition.ConnectionName = strArray[i].Trim();
                        list.Add(definition);
                    }
                    catch (Exception)
                    {
                    }
                }
                entry.CreateChannelEntryLists(list.GetEnumerator());
                lock (this.nameListLock)
                {
                    entry.Next = nameList;
                    nameList = entry;
                }
                return entry;
            }
            catch (MQException exception)
            {
                NmqiException ex = new NmqiException(base.env, -1, null, exception.CompCode, exception.Reason, exception);
                base.TrException(method, ex, 1);
                throw ex;
            }
            catch (Exception exception3)
            {
                NmqiException exception4 = new NmqiException(base.env, -1, null, 2, 0x8e5, exception3);
                base.TrException(method, exception4, 3);
                throw exception4;
            }
            finally
            {
                base.TrExit(method);
            }
            return entry;
        }

        private void HandleDoomedTransaction(ManagedHconn hconn)
        {
            uint method = 0x5c9;
            this.TrEntry(method, new object[] { hconn });
            try
            {
                hconn.UnsetInTransaction();
                hconn.UnsetTransactionDoomed();
                int compCode = 0;
                int reason = 0;
                this.MQBACK(hconn, out compCode, out reason);
                NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x7d3, null);
                base.TrException(method, ex);
                throw ex;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private bool InExit()
        {
            uint method = 0x4e8;
            this.TrEntry(method);
            object data = null;
            bool result = false;
            data = CallContext.GetData("inExit");
            if (data != null)
            {
                result = (bool) data;
            }
            base.TrExit(method, result);
            return result;
        }

        private bool IsDispatchThread()
        {
            bool flag2;
            uint method = 0x454;
            this.TrEntry(method);
            try
            {
                bool flag = false;
                object data = Thread.GetData(Thread.GetNamedDataSlot("MQ_CLIENT_THREAD_TYPE"));
                if ((data != null) && (Convert.ToInt32(data) == 2))
                {
                    flag = true;
                }
                flag2 = flag;
            }
            catch (NullReferenceException)
            {
                flag2 = true;
            }
            finally
            {
                base.TrExit(method);
            }
            return flag2;
        }

        public void MQBACK(Hconn hConn, out int compCode, out int reason)
        {
            uint method = 0x9b;
            this.TrEntry(method, new object[] { hConn, "compCode : out", "reason : out" });
            int translength = 0;
            int num3 = 0x10;
            int num4 = 0;
            int offset = 0;
            bool flag = CommonServices.TraceStatus();
            MQERD mqerd = null;
            MQTSH tsh = null;
            MQTSH rTSH = null;
            ManagedHconn managedHconn = null;
            MQSession session = null;
            MQAPI mqapi = new MQAPI();
            compCode = 2;
            reason = 0x7e2;
            this.InExit();
            try
            {
                managedHconn = this.GetManagedHconn(hConn);
                session = managedHconn.Session;
                num4 = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                translength = num4 + num3;
                managedHconn.EnterCall(this.IsDispatchThread(), false);
                try
                {
                    session.CheckIfDisconnected();
                    tsh = session.AllocateTSH(0x8b, 0, true, translength);
                    byte[] tshBuffer = tsh.TshBuffer;
                    offset = tsh.WriteStruct(tshBuffer, 0);
                    mqapi.Initialize(translength, hConn.Value);
                    offset += mqapi.WriteStruct(tshBuffer, offset);
                    if (flag)
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "MQBACK >>");
                        base.TrData(method, 0, "Hconn", BitConverter.GetBytes(hConn.Value));
                        base.TrAPIOutput(method, "CompCode");
                        base.TrAPIOutput(method, "Reason");
                    }
                    tsh.TshBuffer = tshBuffer;
                    session.SendTSH(tsh);
                    tshBuffer = null;
                    rTSH = session.ReceiveTSH(null);
                    switch (rTSH.SegmentType)
                    {
                        case 5:
                            mqerd = new MQERD();
                            if (rTSH.Length > num4)
                            {
                                mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            }
                            throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());

                        case 0x9b:
                            break;

                        default:
                        {
                            string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                            CommonServices.SetValidInserts();
                            CommonServices.ArithInsert1 = rTSH.SegmentType;
                            CommonServices.CommentInsert1 = null;
                            CommonServices.CommentInsert2 = "Unexpected flow received during MQBACK";
                            base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                            throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                        }
                    }
                    rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                    compCode = mqapi.mqapi.CompCode;
                    reason = mqapi.mqapi.Reason;
                }
                finally
                {
                    try
                    {
                        if (managedHconn != null)
                        {
                            if (rTSH != null)
                            {
                                try
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                                catch (NmqiException)
                                {
                                }
                            }
                            managedHconn.LeaveCall(reason);
                        }
                    }
                    catch (NmqiException exception)
                    {
                        base.TrException(method, exception, 1);
                        compCode = exception.CompCode;
                        reason = exception.Reason;
                    }
                }
            }
            catch (NmqiException exception2)
            {
                base.TrException(method, exception2, 2);
                compCode = exception2.CompCode;
                reason = exception2.Reason;
            }
            catch (Exception exception3)
            {
                base.TrException(method, exception3, 3);
                compCode = 2;
                reason = 0x893;
            }
            finally
            {
                if ((reason != 0) && managedHconn.IsReconnectable)
                {
                    if (ManagedHconn.IsReconnectableReasonCode(reason) && !managedHconn.HasFailed())
                    {
                        compCode = 0;
                        reason = 0;
                    }
                    if (managedHconn.HasFailed())
                    {
                        compCode = managedHconn.ReconnectionFailureCompCode;
                        reason = managedHconn.ReconnectionFailureReason;
                    }
                }
                if (flag)
                {
                    base.TrAPI(method, "__________");
                    base.TrAPI(method, "MQBACK <<");
                    base.TrAPIInput(method, "Hconn");
                    base.TrAPICCRC(method, compCode, reason);
                }
                base.TrExit(method);
            }
        }

        public void MQBEGIN(Hconn hconn, MQBeginOptions pBeginOptions, out int pCompCode, out int pReason)
        {
            uint method = 0x5a6;
            this.TrEntry(method, new object[] { hconn, pBeginOptions, "pCompCode : out", "pReason : out" });
            pCompCode = 2;
            pReason = 0x7dc;
            base.TrExit(method);
        }

        public void MQCB(Hconn hConn, int operationP, MQCBD pCallbackDesc, Hobj hobjP, MQMessageDescriptor pMsgDescP, MQGetMessageOptions getMsgOptsP, out int pCompCode, out int pReason)
        {
            uint method = 0x5ab;
            this.TrEntry(method, new object[] { hConn, operationP, pCallbackDesc, hobjP, pMsgDescP, getMsgOptsP, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            int operation = operationP;
            MQMessageDescriptor callbackMsgDesc = pMsgDescP;
            MQGetMessageOptions callbackGetMsgOpts = getMsgOptsP;
            Hobj hobj = hobjP;
            ManagedHobj hobj2 = null;
            MQProxyQueue proxyQueue = null;
            ManagedHconn managedHconn = null;
            MQSession session = null;
            this.InExit();
            try
            {
                if (hobj is ManagedHobj)
                {
                    hobj2 = (ManagedHobj) hobj;
                    if (hobj2.Reconnectable)
                    {
                        hobj2.SetupCallback(pCallbackDesc, operation, callbackMsgDesc, callbackGetMsgOpts);
                    }
                }
                try
                {
                    managedHconn = this.GetManagedHconn(hConn);
                    session = managedHconn.Session;
                    if (session.ParentConnection.FapLevel < 9)
                    {
                        pCompCode = 2;
                        pReason = 0x8fa;
                        NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x8fa, null);
                        base.TrException(method, ex);
                        return;
                    }
                    if (!session.ParentConnection.IsMultiplexingEnabled)
                    {
                        pCompCode = 2;
                        pReason = 0x7dc;
                        NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x7dc, null);
                        base.TrException(method, exception2);
                        return;
                    }
                    managedHconn.EnterCall(this.IsDispatchThread(), true);
                    try
                    {
                        session.CheckIfDisconnected();
                        pCompCode = 0;
                        pReason = 0;
                        switch (operation)
                        {
                            case 0x100:
                            case 0x200:
                            case 0x10000:
                            case 0x20000:
                            case 0x20100:
                            case 0x10100:
                                break;

                            case 0x10200:
                                operation &= -65537;
                                break;

                            default:
                                pCompCode = 2;
                                pReason = 0x9b8;
                                return;
                        }
                        if (pCallbackDesc != null)
                        {
                            this.ValidateMQCBD(pCallbackDesc);
                        }
                        else
                        {
                            pCallbackDesc = base.env.NewMQCBD();
                        }
                        if (((pCallbackDesc.Options & 0x2000) != 0) && managedHconn.IsQuiescing())
                        {
                            pCompCode = 2;
                            pReason = 0x89a;
                            return;
                        }
                        if (pCallbackDesc.CallbackType == 2)
                        {
                            hobj = null;
                            if (managedHconn.IsReconnectable)
                            {
                                managedHconn.SetupEventHandler(pCallbackDesc, operation);
                            }
                        }
                        if (callbackMsgDesc == null)
                        {
                            callbackMsgDesc = base.env.NewMQMD();
                        }
                        if (callbackGetMsgOpts == null)
                        {
                            callbackGetMsgOpts = base.env.NewMQGMO();
                        }
                        if (hobj != null)
                        {
                            proxyQueue = hobj2.ProxyQueue;
                            if (proxyQueue == null)
                            {
                                pCompCode = 2;
                                pReason = 0x7e3;
                                return;
                            }
                            if ((operation & 0x100) != 0)
                            {
                                proxyQueue.MqcbRegisterMC(pCallbackDesc, callbackMsgDesc, callbackGetMsgOpts);
                            }
                            if ((operation & 0x20000) != 0)
                            {
                                proxyQueue.MqcbResumeMC();
                            }
                            if ((operation & 0x10000) != 0)
                            {
                                proxyQueue.MqcbSuspendMC();
                            }
                            if ((operation & 0x200) != 0)
                            {
                                proxyQueue.MqcbDeRegisterMC(!this.IsDispatchThread(), pCallbackDesc);
                            }
                        }
                        else
                        {
                            if ((operation & 0x100) != 0)
                            {
                                managedHconn.MqcbRegisterEH(pCallbackDesc);
                            }
                            if ((operation & 0x20000) != 0)
                            {
                                managedHconn.MqcbResumeEH();
                            }
                            if ((operation & 0x10000) != 0)
                            {
                                managedHconn.MqcbSuspendEH();
                            }
                            if ((operation & 0x200) != 0)
                            {
                                managedHconn.MqcbDeregisterEH();
                            }
                        }
                    }
                    catch (NmqiException exception3)
                    {
                        base.TrException(method, exception3, 1);
                        pCompCode = exception3.CompCode;
                        pReason = exception3.Reason;
                    }
                    finally
                    {
                        if (managedHconn != null)
                        {
                            try
                            {
                                managedHconn.LeaveCall(pReason);
                            }
                            catch (NmqiException exception4)
                            {
                                base.TrException(method, exception4, 2);
                            }
                        }
                    }
                }
                catch (NmqiException exception5)
                {
                    base.TrException(method, exception5, 3);
                    pCompCode = exception5.CompCode;
                    pReason = exception5.Reason;
                }
                if ((pReason != 0) && managedHconn.IsReconnectable)
                {
                    if (ManagedHconn.IsReconnectableReasonCode(pReason) && !managedHconn.HasFailed())
                    {
                        pCompCode = 0;
                        pReason = 0;
                    }
                    if (managedHconn.HasFailed())
                    {
                        pCompCode = managedHconn.ReconnectionFailureCompCode;
                        pReason = managedHconn.ReconnectionFailureReason;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQCLOSE(Hconn hConn, Phobj pHobj, int options, out int pCompCode, out int pReason)
        {
            uint method = 0x5b5;
            this.TrEntry(method, new object[] { hConn, pHobj, options, "pCompCode : out", "pReason : out" });
            int num2 = 0;
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQAPI mqapi = new MQAPI();
            Hobj hOBJ = null;
            MQERD mqerd = null;
            MQProxyQueue proxyQueue = null;
            ManagedHconn managedHconn = null;
            bool flag = CommonServices.TraceStatus();
            bool flag2 = false;
            pCompCode = 2;
            pReason = 0x7e2;
            this.InExit();
            try
            {
                managedHconn = this.GetManagedHconn(hConn);
                MQSession session = managedHconn.Session;
                num2 = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                int translength = (num2 + 0x10) + 4;
                hOBJ = pHobj.HOBJ;
                if ((hOBJ == null) || (!(hOBJ is ManagedHobj) && !(hOBJ is ManagedHsub)))
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x7e3, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                if (managedHconn.IsReconnectable)
                {
                    managedHconn.RemoveHobj(hOBJ);
                }
                if (managedHconn.IsReconnecting || (session.AsyncFailure != null))
                {
                    flag2 = true;
                }
                managedHconn.EnterCall(this.IsDispatchThread(), false);
                try
                {
                    session.CheckIfDisconnected();
                    if (hOBJ is ManagedHobj)
                    {
                        proxyQueue = ((ManagedHobj) hOBJ).ProxyQueue;
                    }
                    if (proxyQueue == null)
                    {
                        tsh = session.AllocateTSH(0x84, 0, true, translength);
                        mqapi.Initialize(translength, hOBJ.Handle);
                        byte[] tshBuffer = tsh.TshBuffer;
                        int offset = tsh.WriteStruct(tshBuffer, 0);
                        offset += mqapi.WriteStruct(tshBuffer, offset);
                        BitConverter.GetBytes(options).CopyTo(tshBuffer, offset);
                        offset += 4;
                        if (flag)
                        {
                            base.TrAPI(method, "__________");
                            base.TrAPI(method, "MQCLOSE >>");
                            base.TrData(method, 0, "Hconn", BitConverter.GetBytes(hConn.Value));
                            base.TrData(method, 0, "Hobj", BitConverter.GetBytes(hOBJ.Handle));
                            base.TrData(method, 0, "Options", BitConverter.GetBytes(options));
                            base.TrAPIOutput(method, "CompCode");
                            base.TrAPIOutput(method, "Reason");
                        }
                        tsh.TshBuffer = tshBuffer;
                        session.SendTSH(tsh);
                        tshBuffer = null;
                        rTSH = session.ReceiveTSH(null);
                        switch (rTSH.SegmentType)
                        {
                            case 5:
                                mqerd = new MQERD();
                                if (rTSH.Length > num2)
                                {
                                    mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                }
                                throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());

                            case 0x94:
                                rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                pCompCode = mqapi.mqapi.CompCode;
                                pReason = mqapi.mqapi.Reason;
                                hOBJ.Handle = mqapi.mqapi.Handle;
                                if ((pCompCode != 1) && (pReason != 0x7fd))
                                {
                                    pHobj.HOBJ.Handle = -1;
                                }
                                return;
                        }
                        string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                        CommonServices.SetValidInserts();
                        CommonServices.ArithInsert1 = rTSH.SegmentType;
                        CommonServices.CommentInsert1 = channelName;
                        CommonServices.CommentInsert2 = "Unexpected flow received during MQCLOSE";
                        base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                        throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                    }
                    int handle = hOBJ.Handle;
                    proxyQueue.ProxyMQCLOSE(options, ref pCompCode, ref pReason);
                }
                finally
                {
                    try
                    {
                        if (managedHconn != null)
                        {
                            if (rTSH != null)
                            {
                                try
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                                catch (NmqiException exception2)
                                {
                                    base.TrException(method, exception2, 1);
                                    pCompCode = exception2.CompCode;
                                    pReason = exception2.Reason;
                                }
                            }
                            managedHconn.LeaveCall(pReason);
                        }
                    }
                    catch (NmqiException exception3)
                    {
                        base.TrException(method, exception3, 2);
                        pCompCode = exception3.CompCode;
                        pReason = exception3.Reason;
                    }
                }
            }
            catch (NmqiException exception4)
            {
                base.TrException(method, exception4, 3);
                pCompCode = exception4.CompCode;
                pReason = exception4.Reason;
            }
            catch (MQException exception5)
            {
                base.TrException(method, exception5, 4);
                pCompCode = exception5.CompCode;
                pReason = exception5.Reason;
            }
            catch (Exception exception6)
            {
                base.TrException(method, exception6, 5);
                pCompCode = 2;
                pReason = 0x893;
            }
            finally
            {
                if (flag2)
                {
                    pCompCode = 0;
                    pReason = 0;
                }
                if ((pReason != 0) && managedHconn.IsReconnectable)
                {
                    ManagedHconn hconn2 = (ManagedHconn) hConn;
                    if (ManagedHconn.IsReconnectableReasonCode(pReason) && !hconn2.HasFailed())
                    {
                        pCompCode = 0;
                        pReason = 0;
                    }
                    if (hconn2.HasFailed())
                    {
                        pCompCode = hconn2.ReconnectionFailureCompCode;
                        pReason = hconn2.ReconnectionFailureReason;
                    }
                }
                if (flag)
                {
                    base.TrAPI(method, "__________");
                    base.TrAPI(method, "MQCLOSE <<");
                    base.TrAPIInput(method, "Hconn");
                    base.TrData(method, 0, "Hobj", BitConverter.GetBytes(hOBJ.Handle));
                    base.TrAPIInput(method, "Options");
                    base.TrAPICCRC(method, pCompCode, pReason);
                }
                base.TrExit(method);
            }
        }

        public void MQCMIT(Hconn hConn, out int compCode, out int reason)
        {
            uint method = 0x9a;
            this.TrEntry(method, new object[] { hConn, "compCode : out", "reason : out" });
            int translength = 0;
            int num3 = 0;
            int num4 = 0x10;
            bool flag = CommonServices.TraceStatus();
            MQERD mqerd = null;
            MQTSH tsh = null;
            MQTSH rTSH = null;
            ManagedHconn managedHconn = null;
            MQSession session = null;
            MQAPI mqapi = new MQAPI();
            compCode = 2;
            reason = 0x7e2;
            this.InExit();
            try
            {
                managedHconn = this.GetManagedHconn(hConn);
                session = managedHconn.Session;
                num3 = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                translength = num3 + num4;
                managedHconn.EnterCall(this.IsDispatchThread(), false);
                try
                {
                    session.CheckIfDisconnected();
                    if (managedHconn.IsTransactionDoomed())
                    {
                        this.HandleDoomedTransaction(managedHconn);
                    }
                    tsh = session.AllocateTSH(0x8a, 0, true, translength);
                    byte[] tshBuffer = tsh.TshBuffer;
                    int offset = tsh.WriteStruct(tshBuffer, 0);
                    mqapi.Initialize(translength, hConn.Value);
                    offset += mqapi.WriteStruct(tshBuffer, offset);
                    if (flag)
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "MQCMIT >>");
                        base.TrData(method, 0, "Hconn", BitConverter.GetBytes(hConn.Value));
                        base.TrAPIOutput(method, "CompCode");
                        base.TrAPIOutput(method, "Reason");
                    }
                    tsh.TshBuffer = tshBuffer;
                    tsh.Offset = 0;
                    offset = 0;
                    session.SendTSH(tsh);
                    tshBuffer = null;
                    rTSH = session.ReceiveTSH(null);
                    switch (rTSH.SegmentType)
                    {
                        case 5:
                            mqerd = new MQERD();
                            if (rTSH.Length > num3)
                            {
                                mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            }
                            throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());

                        case 0x9a:
                            break;

                        default:
                        {
                            string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                            CommonServices.SetValidInserts();
                            CommonServices.ArithInsert1 = rTSH.SegmentType;
                            CommonServices.CommentInsert1 = channelName;
                            CommonServices.CommentInsert2 = "Unexpected flow received during MQCMIT";
                            base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009540, 0);
                            throw MQERD.ErrorException(0x20009540, rTSH.SegmentType, channelName);
                        }
                    }
                    rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                    compCode = mqapi.mqapi.CompCode;
                    reason = mqapi.mqapi.Reason;
                    managedHconn.UnsetInTransaction();
                    managedHconn.UnsetTransactionDoomed();
                }
                finally
                {
                    try
                    {
                        if (managedHconn != null)
                        {
                            if (rTSH != null)
                            {
                                try
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                                catch (NmqiException)
                                {
                                }
                            }
                            managedHconn.LeaveCall(reason);
                        }
                    }
                    catch (NmqiException exception)
                    {
                        base.TrException(method, exception, 1);
                        compCode = exception.CompCode;
                        reason = exception.Reason;
                    }
                }
            }
            catch (NmqiException exception2)
            {
                base.TrException(method, exception2, 2);
                compCode = exception2.CompCode;
                reason = exception2.Reason;
            }
            catch (Exception exception3)
            {
                base.TrException(method, exception3, 3);
                compCode = 2;
                reason = 0x893;
            }
            finally
            {
                if (((reason != 0) && managedHconn.IsReconnectable) && ManagedHconn.IsReconnectableReasonCode(reason))
                {
                    compCode = 2;
                    reason = 0x9f5;
                }
                if (flag)
                {
                    base.TrAPI(method, "__________");
                    base.TrAPI(method, "MQCMIT <<");
                    base.TrAPIInput(method, "Hconn");
                    base.TrAPICCRC(method, compCode, reason);
                }
                base.TrExit(method);
            }
        }

        public void MQCONN(string pQMgrName, Phconn phconn, out int pCompCode, out int pReason)
        {
            uint method = 0x5a7;
            this.TrEntry(method, new object[] { pQMgrName, phconn, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            this.InExit();
            try
            {
                if (this.GetManagedHconn(phconn.HConn).Session != null)
                {
                    pCompCode = 2;
                    pReason = 0x7d2;
                    string str = "Hconn already has a session opened. Its invalid to create a new session while a session is already running";
                    CommonServices.SetValidInserts();
                    CommonServices.CommentInsert1 = str;
                    CommonServices.CommentInsert2 = pQMgrName;
                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009504, 0);
                }
                else
                {
                    this.NmqiConnect(pQMgrName, null, null, null, phconn, out pCompCode, out pReason);
                }
            }
            catch (NmqiException exception)
            {
                base.TrException(method, exception, 1);
                pCompCode = exception.CompCode;
                pReason = exception.Reason;
            }
            catch (SocketException exception2)
            {
                base.TrException(method, exception2, 2);
                pCompCode = 2;
                pReason = 0x80b;
            }
            catch (Exception exception3)
            {
                base.TrException(method, exception3, 3);
                pCompCode = 2;
                pReason = 0x80b;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQCONNX(string pQMgrName, MQConnectOptions pConnectOpts, Phconn phconn, out int pCompCode, out int pReason)
        {
            uint method = 0x5a8;
            this.TrEntry(method, new object[] { pQMgrName, pConnectOpts, phconn, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            this.InExit();
            try
            {
                ManagedHconn managedHconn = this.GetManagedHconn(phconn.HConn);
                if ((managedHconn != null) && (managedHconn.Session != null))
                {
                    pCompCode = 2;
                    pReason = 0x7d2;
                    pCompCode = 2;
                    pReason = 0x7d2;
                    string str = "Hconn already has a session opened. Its invalid to create a new session while a session is already running";
                    CommonServices.SetValidInserts();
                    CommonServices.CommentInsert1 = str;
                    CommonServices.CommentInsert2 = pQMgrName;
                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009504, 0);
                }
                else
                {
                    pConnectOpts.CheckValidity(ref pCompCode, ref pReason);
                    if (pCompCode == 0)
                    {
                        if (!pConnectOpts.ClientConnPtr.Equals(IntPtr.Zero))
                        {
                            MQChannelDefinition cd = pConnectOpts.cd;
                        }
                        NmqiConnectOptions pNmqiConnectOpts = new NmqiConnectOptions();
                        pNmqiConnectOpts.ReconnectionTimeout = base.env.Cfg.GetIntValue(MQClientCfg.CHANNEL_RECONTIMEOUT);
                        if (MQEnvironment.ProductIdentifier != null)
                        {
                            pNmqiConnectOpts.ExternalProductIdentifier = MQEnvironment.ProductIdentifier;
                        }
                        this.NmqiConnect(pQMgrName, pNmqiConnectOpts, pConnectOpts, managedHconn, phconn, out pCompCode, out pReason, null);
                        managedHconn = this.GetManagedHconn(phconn.HConn);
                        if (pCompCode != 2)
                        {
                            managedHconn.Value = phconn.HConn.Value;
                        }
                    }
                }
            }
            catch (NmqiException exception)
            {
                base.TrException(method, exception, 1);
                pCompCode = exception.CompCode;
                pReason = exception.Reason;
            }
            catch (SocketException exception2)
            {
                base.TrException(method, exception2, 2);
                pCompCode = 2;
                pReason = 0x80b;
            }
            catch (Exception exception3)
            {
                base.TrException(method, exception3, 3);
                pCompCode = 2;
                pReason = 0x80b;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQCTL(Hconn hConn, int operation, MQCTLO pControlOpts, out int compCode, out int reason)
        {
            uint method = 0x5af;
            this.TrEntry(method, new object[] { hConn, operation, pControlOpts, "compCode : out", "reason : out" });
            compCode = 0;
            reason = 0;
            ManagedHconn managedHconn = null;
            bool flag = false;
            bool flag2 = false;
            MQSession session = null;
            bool flag3 = false;
            bool flag4 = false;
            this.InExit();
            try
            {
                MQDispatchThread dispatchThread;
                managedHconn = this.GetManagedHconn(hConn);
                session = managedHconn.Session;
                if (operation == 4)
                {
                    if (!managedHconn.IsStarted() && !managedHconn.IsReconnecting)
                    {
                        flag3 = true;
                    }
                    managedHconn.UnsetSuspended();
                    managedHconn.UnsetStarted();
                }
                if (operation == 0x10000)
                {
                    if (!managedHconn.IsStarted())
                    {
                        compCode = 2;
                        reason = 0x9e6;
                        return;
                    }
                    if (managedHconn.IsSuspended() && !managedHconn.IsReconnecting)
                    {
                        flag4 = true;
                    }
                    managedHconn.SetSuspended();
                }
                if (managedHconn.ParentConnection.FapLevel < 9)
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x8fa, null);
                    base.env.LastException = ex;
                    base.TrException(method, ex);
                    throw ex;
                }
                if (!managedHconn.ParentConnection.IsMultiplexingEnabled)
                {
                    NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x7dc, null);
                    base.env.LastException = exception2;
                    base.TrException(method, exception2);
                    throw exception2;
                }
                if (!this.IsDispatchThread())
                {
                    managedHconn.RequestDispatchLock(-1);
                    flag = true;
                }
                managedHconn.EnterCall(this.IsDispatchThread(), true);
                flag2 = true;
                session.CheckIfDisconnected();
                compCode = 0;
                reason = 0;
                this.ValidateMQCTLO(pControlOpts);
                if (((pControlOpts.Options & 0x2000) != 0) && managedHconn.IsQuiescing())
                {
                    compCode = 2;
                    reason = 0x89a;
                    managedHconn.LeaveCall(reason);
                    flag2 = false;
                    return;
                }
                switch (operation)
                {
                    case 1:
                        if (!managedHconn.IsSuspended())
                        {
                            break;
                        }
                        compCode = 2;
                        reason = 0x9e6;
                        goto Label_03BD;

                    case 2:
                        compCode = 2;
                        reason = 0x9e6;
                        goto Label_03BD;

                    case 4:
                        if (!flag3)
                        {
                            managedHconn.UnsetSuspended();
                            managedHconn.UnsetStarted();
                            managedHconn.SendConnState(true);
                            managedHconn.SetConsumersChanged();
                            managedHconn.RequestThreadLock();
                            dispatchThread = managedHconn.DispatchThread;
                            if (dispatchThread != null)
                            {
                                managedHconn.SetStopPending();
                                dispatchThread.IncrementDispatchSeq();
                            }
                            managedHconn.ReleaseThreadLock();
                            if (dispatchThread != null)
                            {
                                if (!this.IsDispatchThread())
                                {
                                    managedHconn.LeaveCall(0);
                                    flag2 = false;
                                    managedHconn.DispatchThreadExchange();
                                    flag = false;
                                }
                            }
                            else
                            {
                                managedHconn.CheckTxnMessage();
                                managedHconn.DriveStops();
                            }
                            managedHconn.UnSetQuiescing();
                        }
                        goto Label_03BD;

                    case 0x10000:
                        if (!flag4)
                        {
                            managedHconn.SetSuspended();
                            managedHconn.SendConnState(true);
                            managedHconn.RequestThreadLock();
                            dispatchThread = managedHconn.DispatchThread;
                            if (dispatchThread != null)
                            {
                                managedHconn.SetSuspendPending();
                                dispatchThread.IncrementDispatchSeq();
                            }
                            managedHconn.ReleaseThreadLock();
                            if (dispatchThread != null)
                            {
                                if (!this.IsDispatchThread())
                                {
                                    managedHconn.LeaveCall(0);
                                    flag2 = false;
                                    managedHconn.DispatchThreadExchange();
                                    flag = false;
                                }
                            }
                            else
                            {
                                managedHconn.CheckTxnMessage();
                            }
                        }
                        goto Label_03BD;

                    case 0x20000:
                        if (!managedHconn.IsStarted())
                        {
                            compCode = 2;
                            reason = 0x9e6;
                        }
                        else if (managedHconn.IsSuspended())
                        {
                            managedHconn.UnsetSuspended();
                            managedHconn.SendConnState(false);
                            managedHconn.CheckTxnAllowed();
                            if (managedHconn.ConsumersChanged() && !this.IsDispatchThread())
                            {
                                managedHconn.DriveOutstanding();
                            }
                            managedHconn.CheckDispatchable(null);
                        }
                        goto Label_03BD;

                    default:
                        compCode = 2;
                        reason = 0x9b8;
                        goto Label_03BD;
                }
                if (managedHconn.IsStarted())
                {
                    compCode = 2;
                    reason = 0x9c4;
                }
                else if (managedHconn.IsXaConnected())
                {
                    compCode = 2;
                    reason = 0x9e2;
                }
                else
                {
                    managedHconn.SetStarted();
                    if (this.IsDispatchThread())
                    {
                        managedHconn.SendConnState(false);
                    }
                    else
                    {
                        managedHconn.StartInit(false);
                        if (!managedHconn.IsStarted() && (compCode == 0))
                        {
                            compCode = 2;
                            reason = 0x9e0;
                        }
                    }
                }
            Label_03BD:
                if (managedHconn.IsReconnectable && (reason == 0))
                {
                    managedHconn.SetupCallback(operation);
                }
            }
            catch (NmqiException exception3)
            {
                base.TrException(method, exception3);
                compCode = exception3.CompCode;
                reason = exception3.Reason;
            }
            finally
            {
                if (flag)
                {
                    managedHconn.ReleaseDispatchLock();
                }
                if (flag2)
                {
                    managedHconn.LeaveCall(reason);
                }
                if ((reason != 0) && managedHconn.IsReconnectable)
                {
                    if (ManagedHconn.IsReconnectableReasonCode(reason) && !managedHconn.HasFailed())
                    {
                        compCode = 0;
                        reason = 0;
                    }
                    if (managedHconn.HasFailed())
                    {
                        compCode = managedHconn.ReconnectionFailureCompCode;
                        reason = managedHconn.ReconnectionFailureReason;
                    }
                }
                base.TrExit(method);
            }
        }

        public void MQDISC(Phconn phconn, out int pCompCode, out int pReason)
        {
            pCompCode = 0;
            pReason = 0;
            ManagedHconn managedHconn = this.GetManagedHconn(phconn.HConn);
            if (managedHconn != null)
            {
                if (managedHconn.IsXAEnabled)
                {
                    managedHconn.XAClose();
                }
                this.Disconnect(managedHconn, out pCompCode, out pReason);
            }
        }

        public void MQGET(Hconn hConn, Hobj hObj, MQGetMessageOptions gmo, ref MQMessage message, out int compCode, out int reason)
        {
            uint method = 0x649;
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
            uint method = 0x5bb;
            this.TrEntry(method, new object[] { hconn, hobj, pMsgDesc, pGetMsgOpts, BufferLength, pBuffer, "pDataLength : out", "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                ManagedHconn managedHconn = this.GetManagedHconn(hconn);
                MQSession failedSession = this.zstMQGET(managedHconn, hobj, ref pMsgDesc, ref pGetMsgOpts, BufferLength, pBuffer, out pDataLength, null, out pCompCode, out pReason);
                if (((pReason != 0) && managedHconn.IsReconnectable) && !this.ThreadIsReconnectThread())
                {
                    if (ManagedHconn.IsReconnectableReasonCode(pReason) && !managedHconn.HasFailed())
                    {
                        try
                        {
                            managedHconn.Reconnect(failedSession);
                            this.zstMQGET(managedHconn, hobj, ref pMsgDesc, ref pGetMsgOpts, BufferLength, pBuffer, out pDataLength, null, out pCompCode, out pReason);
                        }
                        catch (NmqiException)
                        {
                        }
                    }
                    if (managedHconn.HasFailed())
                    {
                        pCompCode = managedHconn.ReconnectionFailureCompCode;
                        pReason = managedHconn.ReconnectionFailureReason;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQINQ(Hconn hConn, Hobj hObj, int selectorCount, int[] selectors, int intAttrCount, int[] intAttrs, int charAttrLength, byte[] charAttrs, out int compCode, out int reason)
        {
            uint method = 0x90;
            this.TrEntry(method, new object[] { hConn, hObj, selectorCount, selectors, intAttrCount, intAttrs, charAttrLength, charAttrs, "compCode : out", "reason : out" });
            int translength = 0;
            int num3 = 0;
            int num4 = 0;
            int offset = 0;
            int num7 = 0;
            int num8 = 0;
            MQERD mqerd = null;
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQAPI mqapi = new MQAPI();
            ManagedHconn managedHconn = null;
            MQSession session = null;
            num4 = 0x10;
            bool flag = CommonServices.TraceStatus();
            compCode = 2;
            reason = 0x7e2;
            this.InExit();
            try
            {
                managedHconn = this.GetManagedHconn(hConn);
                session = managedHconn.Session;
                num3 = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                translength = ((num3 + num4) + charAttrLength) + (((intAttrCount + selectorCount) + 3) * 4);
                if ((hObj == null) || !(hObj is ManagedHobj))
                {
                    compCode = 2;
                    reason = 0x7e3;
                    NmqiException exception = new NmqiException(base.env, -1, null, compCode, reason, null);
                    throw exception;
                }
                ManagedHobj hobj1 = (ManagedHobj) hObj;
                if ((selectors == null) || (selectors.Length < selectorCount))
                {
                    compCode = 2;
                    reason = 0x811;
                    NmqiException exception2 = new NmqiException(base.env, -1, null, compCode, reason, null);
                    throw exception2;
                }
                int num9 = 0;
                for (int i = 0; i < selectorCount; i++)
                {
                    if (selectors[i] <= 0x7d0)
                    {
                        num9++;
                    }
                }
                if ((num9 > 0) && ((intAttrs == null) || (intAttrCount > intAttrs.Length)))
                {
                    compCode = 2;
                    reason = 0x7e5;
                    NmqiException exception3 = new NmqiException(base.env, -1, null, compCode, reason, null);
                    throw exception3;
                }
                int num11 = 0;
                for (int j = 0; j < selectorCount; j++)
                {
                    if ((selectors[j] >= 0x7d1) && (selectors[j] <= 0xfa0))
                    {
                        num11++;
                    }
                }
                if ((num11 > 0) && ((charAttrs == null) || (charAttrLength > charAttrs.Length)))
                {
                    compCode = 2;
                    reason = 0x7d7;
                    NmqiException exception4 = new NmqiException(base.env, -1, null, compCode, reason, null);
                    throw exception4;
                }
                if ((num9 + num11) != selectorCount)
                {
                    compCode = 2;
                    reason = 0x813;
                    NmqiException exception5 = new NmqiException(base.env, -1, null, compCode, reason, null);
                    throw exception5;
                }
                managedHconn.EnterCall(this.IsDispatchThread(), false);
                try
                {
                    session.CheckIfDisconnected();
                    tsh = session.AllocateTSH(0x89, 0, true, translength);
                    byte[] tshBuffer = tsh.TshBuffer;
                    mqapi.Initialize(translength + 1, hObj.Handle);
                    int num5 = tsh.WriteStruct(tshBuffer, 0);
                    num5 += mqapi.WriteStruct(tshBuffer, num5);
                    BitConverter.GetBytes(selectorCount).CopyTo(tshBuffer, num5);
                    num5 += 4;
                    BitConverter.GetBytes(intAttrCount).CopyTo(tshBuffer, num5);
                    num5 += 4;
                    BitConverter.GetBytes(charAttrLength).CopyTo(tshBuffer, num5);
                    offset = num5 += 4;
                    num5 = this.WriteIntArray(tshBuffer, num5, selectors, selectorCount);
                    if (flag)
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "MQINQ >>");
                        base.TrData(method, 0, "Hconn", BitConverter.GetBytes(hConn.Value));
                        base.TrData(method, 0, "Hobj", BitConverter.GetBytes(hObj.Handle));
                        base.TrData(method, 0, "Selectorcount", BitConverter.GetBytes(selectorCount));
                        base.TrData(method, 0, "Selectors", offset, selectorCount * 4, tshBuffer);
                        base.TrData(method, 0, "Intattrcount", BitConverter.GetBytes(intAttrCount));
                        base.TrAPIOutput(method, "Intattrs");
                        base.TrData(method, 0, "Charattrlength", BitConverter.GetBytes(charAttrLength));
                        base.TrAPIOutput(method, "Charattrs");
                        base.TrAPIOutput(method, "CompCode");
                        base.TrAPIOutput(method, "Reason");
                    }
                    tsh.TshBuffer = tshBuffer;
                    num5 = 0;
                    session.SendTSH(tsh);
                    tshBuffer = null;
                    rTSH = session.ReceiveTSH(null);
                    switch (rTSH.SegmentType)
                    {
                        case 5:
                            mqerd = new MQERD();
                            if (rTSH.Length > num3)
                            {
                                mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            }
                            throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());

                        case 0x99:
                            rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            compCode = mqapi.mqapi.CompCode;
                            reason = mqapi.mqapi.Reason;
                            if (compCode != 2)
                            {
                                rTSH.Offset += 12;
                                rTSH.Offset = this.ReadIntArray(rTSH.TshBuffer, rTSH.Offset, selectors, selectorCount);
                                num7 = rTSH.Offset;
                                if (rTSH.Offset < rTSH.Length)
                                {
                                    rTSH.Offset = this.ReadIntArray(rTSH.TshBuffer, rTSH.Offset, intAttrs, intAttrCount);
                                }
                                if (((charAttrLength != 0) && (charAttrs != null)) && ((rTSH.Offset < rTSH.Length) && ((rTSH.Offset + charAttrLength) <= rTSH.Length)))
                                {
                                    num8 = rTSH.Offset;
                                    Buffer.BlockCopy(rTSH.TshBuffer, rTSH.Offset, charAttrs, 0, charAttrLength);
                                    rTSH.Offset += charAttrLength;
                                }
                            }
                            return;
                    }
                    string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = rTSH.SegmentType;
                    CommonServices.CommentInsert1 = channelName;
                    CommonServices.CommentInsert2 = "Unexpected flow received during MQINQ";
                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                    throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                }
                finally
                {
                    if (flag)
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "MQINQ <<");
                        base.TrAPIInput(method, "Hconn");
                        base.TrAPIInput(method, "Hobj");
                        base.TrAPIInput(method, "Selectorcount");
                        base.TrAPIInput(method, "Selectors");
                        base.TrAPIInput(method, "Intattrcount");
                        if (num7 != 0)
                        {
                            base.TrData(method, 0, "Intattrs", num7, intAttrCount * 4, rTSH.TshBuffer);
                        }
                        base.TrAPIInput(method, "Charattrlength");
                        if (num8 != 0)
                        {
                            base.TrData(method, 0, "Charattrs", num8, charAttrLength, rTSH.TshBuffer);
                        }
                        base.TrAPICCRC(method, compCode, reason);
                    }
                    try
                    {
                        if (managedHconn != null)
                        {
                            if (rTSH != null)
                            {
                                try
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                                catch (NmqiException exception6)
                                {
                                    base.TrException(method, exception6, 1);
                                }
                            }
                            managedHconn.LeaveCall(reason);
                        }
                    }
                    catch (NmqiException exception7)
                    {
                        base.TrException(method, exception7, 2);
                        compCode = exception7.CompCode;
                        reason = exception7.Reason;
                    }
                }
            }
            catch (NmqiException exception8)
            {
                base.TrException(method, exception8, 3);
                compCode = exception8.CompCode;
                reason = exception8.Reason;
            }
            catch (Exception exception9)
            {
                base.TrException(method, exception9, 4);
                compCode = 2;
                reason = 0x893;
            }
            finally
            {
                if ((reason != 0) && managedHconn.IsReconnectable)
                {
                    if (ManagedHconn.IsReconnectableReasonCode(reason) && !managedHconn.HasFailed())
                    {
                        this.MQINQ(hConn, hObj, selectorCount, selectors, intAttrCount, intAttrs, charAttrLength, charAttrs, out compCode, out reason);
                    }
                    if (managedHconn.HasFailed())
                    {
                        compCode = managedHconn.ReconnectionFailureCompCode;
                        reason = managedHconn.ReconnectionFailureReason;
                    }
                }
                base.TrExit(method);
            }
        }

        public void MQOPEN(Hconn hConn, ref MQObjectDescriptor pOD, int options, Phobj pHObj, out int pCompCode, out int pReason)
        {
            uint method = 0x5b2;
            this.TrEntry(method, new object[] { hConn, pOD, options, pHObj, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                this.MQOPEN(hConn, ref pOD, options, pHObj, out pCompCode, out pReason, null);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal MQSession MQOPEN(Hconn manHconn, ref MQObjectDescriptor mqod, int options, ref Phobj manHobj, out int compCode, out int reason, ManagedHobj rcnHobj)
        {
            uint method = 0x8b;
            this.TrEntry(method, new object[] { manHconn, mqod, options, manHobj, "compCode : out", "reason : out", rcnHobj });
            MQSession session = null;
            try
            {
                SpiOpenOptions openOptions = new SpiOpenOptions();
                openOptions.Options = options;
                compCode = 0;
                reason = 0;
                session = this.nmqiOpen(manHconn, ref mqod, ref openOptions, options, ref manHobj, out compCode, out reason, false, rcnHobj);
            }
            finally
            {
                base.TrExit(method);
            }
            return session;
        }

        public void MQOPEN(Hconn hConn, ref MQObjectDescriptor pOD, int options, Phobj pHObj, out int pCompCode, out int pReason, ManagedHobj recnHobj)
        {
            uint method = 0x5b1;
            this.TrEntry(method, new object[] { hConn, pOD, options, pHObj, "pCompCode : out", "pReason : out", recnHobj });
            pCompCode = 0;
            pReason = 0;
            try
            {
                MQSession failedSession = this.MQOPEN(hConn, ref pOD, options, ref pHObj, out pCompCode, out pReason, recnHobj);
                ManagedHconn hconn = (ManagedHconn) hConn;
                if (((pReason != 0) && hconn.IsReconnectable) && !this.ThreadIsReconnectThread())
                {
                    if (ManagedHconn.IsReconnectableReasonCode(pReason) && !hconn.HasFailed())
                    {
                        try
                        {
                            hconn.Reconnect(failedSession);
                            this.MQOPEN(hConn, ref pOD, options, pHObj, out pCompCode, out pReason);
                        }
                        catch (NmqiException)
                        {
                        }
                    }
                    if (hconn.HasFailed())
                    {
                        pCompCode = hconn.ReconnectionFailureCompCode;
                        pReason = hconn.ReconnectionFailureReason;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQPUT(Hconn hConn, Hobj hObj, MQMessage message, MQPutMessageOptions pmo, out int compCode, out int reason)
        {
            uint method = 0x648;
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
            uint method = 0x5b6;
            this.TrEntry(method, new object[] { hconn, hobj, pMsgDesc, pPutMsgOpts, bufferLength, pBuffer, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                MQSession failedSession = this.MQPUT(hconn, hobj, ref pMsgDesc, ref pPutMsgOpts, bufferLength, pBuffer, null, 1, out pCompCode, out pReason);
                ManagedHconn managedHconn = this.GetManagedHconn(hconn);
                if (((pReason != 0) && managedHconn.IsReconnectable) && !this.ThreadIsReconnectThread())
                {
                    if (ManagedHconn.IsReconnectableReasonCode(pReason) && !managedHconn.HasFailed())
                    {
                        try
                        {
                            managedHconn.Reconnect(failedSession);
                            this.MQPUT(hconn, hobj, pMsgDesc, pPutMsgOpts, bufferLength, pBuffer, out pCompCode, out pReason);
                        }
                        catch (NmqiException)
                        {
                        }
                    }
                    if (managedHconn.HasFailed())
                    {
                        pCompCode = managedHconn.ReconnectionFailureCompCode;
                        pReason = managedHconn.ReconnectionFailureReason;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal MQSession MQPUT(Hconn hConn, Hobj hObj, ref MQMessageDescriptor mqmd, ref MQPutMessageOptions mqpmo, int sbLength, byte[] sBuff, MemoryStream[] mBuffs, int numBuffs, out int compCode, out int reason)
        {
            int num12;
            int num14;
            uint method = 0x94;
            this.TrEntry(method, new object[] { hConn, hObj, mqmd, mqpmo, sbLength, sBuff, mBuffs, numBuffs, "compCode : out", "reason : out" });
            int versionLength = mqmd.GetVersionLength();
            int num3 = mqpmo.GetVersionLength();
            int num4 = 0;
            int offset = 0;
            int num6 = 0;
            int srcOffset = 0;
            int num9 = 0;
            int callLength = 0;
            byte num16 = 0x10;
            bool flag = CommonServices.TraceStatus();
            bool flag2 = true;
            MQERD mqerd = null;
            MQTSH mqtsh = null;
            MQTSH rTSH = null;
            ManagedHconn managedHconn = null;
            MQSession session = null;
            MQAPI mqapi = new MQAPI();
            num6 = 0x10;
            compCode = 2;
            reason = 0x7e2;
            int num11 = num12 = 0;
            int num13 = num14 = 0;
            try
            {
                managedHconn = this.GetManagedHconn(hConn);
                session = managedHconn.Session;
                if (managedHconn.IsQuiescing() && ((mqpmo.Options & 0x2000) != 0))
                {
                    base.TrText(method, "This hconn is quiescing, notify the caller about it");
                    base.throwNewMQException(2, 0x89a);
                }
                offset = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                num4 = (((offset + num6) + versionLength) + num3) + 4;
                if (managedHconn.IsReconnectable)
                {
                    if ((mqpmo.Options & 0x8000) != 0)
                    {
                        NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x9f3, null);
                        base.TrException(method, ex);
                        throw ex;
                    }
                    if (managedHconn.IsXAEnabled)
                    {
                        NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x7dc, null);
                        base.TrException(method, exception2);
                        throw exception2;
                    }
                }
                this.TransactionCheck(managedHconn, mqpmo.Options, 1);
                if ((hObj == null) || !(hObj is ManagedHobj))
                {
                    NmqiException exception3 = new NmqiException(base.env, -1, null, 2, 0x7e3, null);
                    base.TrException(method, exception3);
                    throw exception3;
                }
                if (mqmd == null)
                {
                    NmqiException exception4 = new NmqiException(base.env, -1, null, 2, 0x7ea, null);
                    base.TrException(method, exception4);
                    throw exception4;
                }
                int num17 = 0;
                if (sBuff != null)
                {
                    if (sBuff.Length < sbLength)
                    {
                        NmqiException exception5 = new NmqiException(base.env, -1, null, 2, 0x7da, null);
                        base.env.LastException = exception5;
                        base.TrException(method, exception5);
                        throw exception5;
                    }
                    num17 = sbLength;
                }
                if (num17 > session.MaximumMessageLength)
                {
                    NmqiException exception6 = new NmqiException(base.env, -1, null, 2, 0x7da, null);
                    base.TrException(method, exception6);
                    throw exception6;
                }
                if (num17 < 0)
                {
                    NmqiException exception7 = new NmqiException(base.env, -1, null, 2, 0x7d5, null);
                    base.TrException(method, exception7);
                    throw exception7;
                }
                num4 += num17;
                managedHconn.EnterCall(this.IsDispatchThread(), false);
                try
                {
                    session.CheckIfDisconnected();
                    int options = mqpmo.Options;
                    if ((session.FapLevel >= 9) && managedHconn.IsSPISupported())
                    {
                        flag2 = this.QuerySyncDelivery(hConn, hObj, ref mqpmo, ref mqmd, out compCode, out reason);
                        base.TrText(method, "Is this a Asynchronous put = " + flag2);
                    }
                    if ((mqpmo.Options & 2) != 0)
                    {
                        managedHconn.SetInTransaction();
                    }
                    callLength = num4;
                    do
                    {
                        int num8;
                        int num19;
                        if (num4 > session.ParentConnection.MaxTransmissionSize)
                        {
                            num8 = num19 = session.ParentConnection.MaxTransmissionSize;
                            num4 = (num4 - session.ParentConnection.MaxTransmissionSize) + offset;
                            num9 = num17 - srcOffset;
                        }
                        else
                        {
                            num8 = num19 = num4;
                            if (session.IsMultiplexingEnabled)
                            {
                                num19 = (num19 + 3) & -4;
                            }
                            num9 = 0;
                        }
                        mqtsh = session.AllocateTshForPut(num19);
                        byte[] tshBuffer = mqtsh.TshBuffer;
                        int num15 = offset;
                        if ((num16 & 0x10) != 0)
                        {
                            mqapi.Initialize(callLength, hObj.Handle);
                            num15 += mqapi.WriteStruct(tshBuffer, offset);
                            num11 = num15;
                            num13 = num12 = num15 += mqmd.WriteStruct(tshBuffer, num15);
                            num14 = num15 += mqpmo.WriteStruct(tshBuffer, num15);
                            BitConverter.GetBytes(num17).CopyTo(tshBuffer, num15);
                            num15 += 4;
                            if (flag)
                            {
                                base.TrAPI(method, "__________");
                                base.TrAPI(method, "MQPUT >>");
                                base.TrData(method, 0, "Hconn", BitConverter.GetBytes(hConn.Value));
                                base.TrData(method, 0, "Hobj", BitConverter.GetBytes(hObj.Handle));
                                base.TrData(method, 0, "Msgdesc", num11, num12 - num11, tshBuffer);
                                base.TrData(method, 0, "Putmsgopts", num13, num14 - num13, tshBuffer);
                                base.TrData(method, 0, "Bufferlength", 0, -1, BitConverter.GetBytes(num17));
                                if (sBuff != null)
                                {
                                    if (num17 > 0)
                                    {
                                        base.TrAPIBuffer(method, sBuff, num17);
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < numBuffs; i++)
                                    {
                                        base.TrAPIBuffer(method, mBuffs[i].GetBuffer(), (int) mBuffs[i].Length);
                                    }
                                }
                                base.TrAPIOutput(method, "CompCode");
                                base.TrAPIOutput(method, "Reason");
                                num11 = num12 = 0;
                                num13 = num14 = 0;
                            }
                        }
                        if (num9 == 0)
                        {
                            num16 = (byte) (num16 | 0x20);
                        }
                        num8 -= num15;
                        Buffer.BlockCopy(sBuff, srcOffset, tshBuffer, num15, num8);
                        num15 += num8;
                        mqtsh.ControlFlags1 = num16;
                        mqtsh.WriteStruct(tshBuffer, 0);
                        num15 = 0;
                        session.SendData(tshBuffer, mqtsh.Offset, mqtsh.Length, mqtsh.SegmentType, mqtsh.TSHType);
                        num16 = (byte) (num16 & 0xef);
                        srcOffset += num8;
                    }
                    while ((num16 & 0x20) == 0);
                    if (flag2)
                    {
                        rTSH = session.ReceiveTSH(null);
                        switch (rTSH.SegmentType)
                        {
                            case 5:
                                mqerd = new MQERD();
                                if (rTSH.Length > offset)
                                {
                                    mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                }
                                throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());

                            case 150:
                                num11 = rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                compCode = mqapi.mqapi.CompCode;
                                reason = mqapi.mqapi.Reason;
                                if (compCode != 0)
                                {
                                    return session;
                                }
                                num13 = num12 = rTSH.Offset = mqmd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                num14 = rTSH.Offset = mqpmo.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                goto Label_070D;
                        }
                        string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                        CommonServices.SetValidInserts();
                        CommonServices.ArithInsert1 = rTSH.SegmentType;
                        CommonServices.CommentInsert1 = channelName;
                        CommonServices.CommentInsert2 = "Unexpected flow received during MQPUT";
                        base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009511, 0);
                        throw MQERD.ErrorException(0x20009511, rTSH.SegmentType, channelName);
                    }
                    compCode = 0;
                    reason = 0;
                    if (flag)
                    {
                        base.TrText(method, string.Concat(new object[] { "ASynchronous Delivery completed successfully, CompCode = ", (int) compCode, " Reason = ", (int) reason }));
                    }
                Label_070D:
                    if (mqpmo != null)
                    {
                        mqpmo.Options = options;
                    }
                    return session;
                }
                finally
                {
                    if (flag && flag2)
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "MQPUT <<");
                        base.TrAPIInput(method, "Hconn");
                        base.TrAPIInput(method, "Hobj");
                        if (num11 != num12)
                        {
                            base.TrData(method, 0, "Msgdesc", num11, num12 - num11, rTSH.TshBuffer);
                        }
                        if (num13 != num14)
                        {
                            base.TrData(method, 0, "Putmsgopts", num13, num14 - num13, rTSH.TshBuffer);
                        }
                        base.TrAPIInput(method, "Bufferlength");
                        base.TrAPIInput(method, "Buffer");
                        base.TrAPICCRC(method, compCode, reason);
                    }
                    else
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "MQPUT <<");
                        base.TrAPIInput(method, "Hconn");
                        base.TrAPIInput(method, "Hobj");
                        base.TrAPICCRC(method, compCode, reason);
                    }
                    try
                    {
                        if (managedHconn != null)
                        {
                            if (rTSH != null)
                            {
                                try
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                                catch (NmqiException exception8)
                                {
                                    base.TrException(method, exception8, 1);
                                    compCode = exception8.CompCode;
                                    reason = exception8.Reason;
                                }
                            }
                            managedHconn.LeaveCall(reason);
                        }
                    }
                    catch (NmqiException exception9)
                    {
                        base.TrException(method, exception9, 2);
                        compCode = exception9.CompCode;
                        reason = exception9.Reason;
                    }
                }
                return session;
            }
            catch (NmqiException exception10)
            {
                base.TrException(method, exception10, 3);
                compCode = exception10.CompCode;
                reason = exception10.Reason;
            }
            catch (MQException exception11)
            {
                base.TrException(method, exception11, 4);
                compCode = exception11.CompCode;
                reason = exception11.Reason;
            }
            catch (Exception exception12)
            {
                base.TrException(method, exception12, 5);
                compCode = 2;
                reason = 0x893;
            }
            finally
            {
                base.TrExit(method);
            }
            return session;
        }

        public void MQPUT1(Hconn hconn, MQObjectDescriptor pObjDesc, MQMessageDescriptor pMsgDesc, MQPutMessageOptions pPutMsgOpts, int bufferLength, byte[] pBuffer, out int pCompCode, out int pReason)
        {
            uint method = 0x5b8;
            this.TrEntry(method, new object[] { hconn, pObjDesc, pMsgDesc, pPutMsgOpts, bufferLength, pBuffer, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                MQSession failedSession = this.MQPUT1(hconn, ref pObjDesc, ref pMsgDesc, ref pPutMsgOpts, bufferLength, pBuffer, null, 0, out pCompCode, out pReason);
                ManagedHconn managedHconn = this.GetManagedHconn(hconn);
                if (((pReason != 0) && managedHconn.IsReconnectable) && !this.ThreadIsReconnectThread())
                {
                    if (ManagedHconn.IsReconnectableReasonCode(pReason) && !managedHconn.HasFailed())
                    {
                        try
                        {
                            managedHconn.Reconnect(failedSession);
                            this.MQPUT1(hconn, ref pObjDesc, ref pMsgDesc, ref pPutMsgOpts, bufferLength, pBuffer, null, 0, out pCompCode, out pReason);
                        }
                        catch (NmqiException)
                        {
                        }
                    }
                    if (managedHconn.HasFailed())
                    {
                        pCompCode = managedHconn.ReconnectionFailureCompCode;
                        pReason = managedHconn.ReconnectionFailureReason;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal MQSession MQPUT1(Hconn hConn, ref MQObjectDescriptor mqod, ref MQMessageDescriptor mqmd, ref MQPutMessageOptions mqpmo, int sbLength, byte[] sBuff, MemoryStream[] mBuffs, int numBuffs, out int compCode, out int reason)
        {
            int num13;
            int num15;
            int num17;
            uint method = 0x95;
            this.TrEntry(method, new object[] { hConn, mqod, mqmd, mqpmo, sbLength, sBuff, mBuffs, numBuffs, "compCode : out", "reason : out" });
            int versionLength = mqmd.GetVersionLength();
            int num4 = mqpmo.GetVersionLength();
            int offset = 0;
            int num7 = 0;
            int srcOffset = 0;
            int num11 = 0;
            int count = 0;
            bool flag = true;
            byte num19 = 0x10;
            bool flag2 = CommonServices.TraceStatus();
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQERD mqerd = null;
            ManagedHconn managedHconn = null;
            MQSession session = null;
            MQAPI mqapi = new MQAPI();
            num7 = 0x10;
            if (IntPtr.Size != 4)
            {
                mqod.UseNativePtrSz = false;
            }
            int requiredBufferSize = mqod.GetRequiredBufferSize();
            compCode = 2;
            reason = 0x7e2;
            int num12 = num13 = 0;
            int num14 = num15 = 0;
            int num16 = num17 = 0;
            this.InExit();
            try
            {
                managedHconn = this.GetManagedHconn(hConn);
                session = managedHconn.Session;
                if (managedHconn.IsQuiescing() && ((mqpmo.Options & 0x2000) != 0))
                {
                    base.TrText(method, "This hconn is quiescing, notify the caller about it");
                    base.throwNewMQException(2, 0x89a);
                }
                offset = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                int num5 = ((((offset + num7) + requiredBufferSize) + versionLength) + num4) + 4;
                if (managedHconn.IsReconnectable)
                {
                    if ((mqpmo.Options & 0x8000) != 0)
                    {
                        NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x9f3, null);
                        base.TrException(method, ex);
                        throw ex;
                    }
                    if (managedHconn.IsXAEnabled)
                    {
                        NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x7dc, null);
                        base.TrException(method, exception2);
                        throw exception2;
                    }
                }
                this.TransactionCheck(managedHconn, mqpmo.Options, 1);
                if (mqod == null)
                {
                    NmqiException exception3 = new NmqiException(base.env, -1, null, 2, 0x7fc, null);
                    base.TrException(method, exception3);
                    throw exception3;
                }
                if (mqmd == null)
                {
                    NmqiException exception4 = new NmqiException(base.env, -1, null, 2, 0x7ea, null);
                    base.TrException(method, exception4);
                    throw exception4;
                }
                int num20 = 0;
                if (sBuff != null)
                {
                    if (sBuff.Length < sbLength)
                    {
                        NmqiException exception5 = new NmqiException(base.env, -1, null, 2, 0x7da, null);
                        base.env.LastException = exception5;
                        base.TrException(method, exception5);
                        throw exception5;
                    }
                    num20 = sbLength;
                }
                else if (mBuffs != null)
                {
                    for (int i = 0; i < numBuffs; i++)
                    {
                        if (mBuffs[i] == null)
                        {
                            NmqiException exception6 = new NmqiException(base.env, -1, null, 2, 0x7d4, null);
                            base.env.LastException = exception6;
                            base.TrException(method, exception6);
                            throw exception6;
                        }
                        num20 += (int) mBuffs[i].Length;
                    }
                    count = (int) mBuffs[0].Length;
                }
                if (num20 > session.MaximumMessageLength)
                {
                    NmqiException exception7 = new NmqiException(base.env, -1, null, 2, 0x7da, null);
                    base.TrException(method, exception7);
                    throw exception7;
                }
                if (num20 < 0)
                {
                    NmqiException exception8 = new NmqiException(base.env, -1, null, 2, 0x7d5, null);
                    base.TrException(method, exception8);
                    throw exception8;
                }
                num11 = num20;
                num5 += num20;
                int options = mqpmo.Options;
                managedHconn.EnterCall(this.IsDispatchThread(), false);
                try
                {
                    int num10;
                    int num25;
                    session.CheckIfDisconnected();
                    if ((session.FapLevel >= 9) && managedHconn.IsSPISupported())
                    {
                        flag = this.QuerySyncDelivery(hConn, null, ref mqpmo, ref mqmd, out compCode, out reason);
                        base.TrText(method, "Is this Asynchronous call - " + flag);
                    }
                    if ((mqpmo.Options & 2) != 0)
                    {
                        managedHconn.SetInTransaction();
                    }
                    int index = 0;
                    int num24 = 0;
                    int callLength = num5;
                Label_0394:
                    if (num5 > session.ParentConnection.MaxTransmissionSize)
                    {
                        num10 = num25 = session.ParentConnection.MaxTransmissionSize;
                        num5 = (num5 - session.ParentConnection.MaxTransmissionSize) + offset;
                        num11 = num20 - srcOffset;
                    }
                    else
                    {
                        num10 = num25 = num5;
                        if (session.IsMultiplexingEnabled)
                        {
                            num25 = (num25 + 3) & -4;
                        }
                        num11 = 0;
                    }
                    tsh = session.AllocateTSH(0x87, 0, true, num25);
                    byte[] tshBuffer = tsh.TshBuffer;
                    int num8 = offset;
                    if ((num19 & 0x10) != 0)
                    {
                        mqapi.Initialize(callLength, 0);
                        num8 += mqapi.WriteStruct(tshBuffer, offset);
                        num12 = num8;
                        num14 = num13 = num8 += mqod.WriteStruct(tshBuffer, num8);
                        num16 = num15 = num8 += mqmd.WriteStruct(tshBuffer, num8);
                        num17 = num8 += mqpmo.WriteStruct(tshBuffer, num8);
                        BitConverter.GetBytes(num20).CopyTo(tshBuffer, num8);
                        num8 += 4;
                        if (flag2)
                        {
                            base.TrAPI(method, "__________");
                            base.TrAPI(method, "MQPUT1 >>");
                            base.TrData(method, 0, "Hconn", BitConverter.GetBytes(hConn.Value));
                            base.TrData(method, 0, "Objdesc", num12, num13 - num12, tshBuffer);
                            base.TrData(method, 0, "Msgdesc", num14, num15 - num14, tshBuffer);
                            base.TrData(method, 0, "Putmsgopts", num16, num17 - num16, tshBuffer);
                            base.TrData(method, 0, "Bufferlength", 0, -1, BitConverter.GetBytes(num20));
                            if (sBuff != null)
                            {
                                if (num20 > 0)
                                {
                                    base.TrAPIBuffer(method, sBuff, num20);
                                }
                            }
                            else
                            {
                                for (int j = 0; j < numBuffs; j++)
                                {
                                    base.TrAPIBuffer(method, mBuffs[j].GetBuffer(), (int) mBuffs[j].Length);
                                }
                            }
                            base.TrAPIOutput(method, "CompCode");
                            base.TrAPIOutput(method, "Reason");
                            num12 = num13 = 0;
                            num14 = num15 = 0;
                            num16 = num17 = 0;
                        }
                    }
                    if (num11 == 0)
                    {
                        num19 = (byte) (num19 | 0x20);
                    }
                    num10 -= num8;
                    if (sBuff != null)
                    {
                        Buffer.BlockCopy(sBuff, srcOffset, tshBuffer, num8, num10);
                        num8 += num10;
                        goto Label_0639;
                    }
                    int num28 = num10;
                Label_05BD:
                    if (num28 < count)
                    {
                        Buffer.BlockCopy(mBuffs[index].GetBuffer(), num24, tshBuffer, num8, num28);
                        num24 += num28;
                        num8 += num28;
                        count -= num28;
                    }
                    else
                    {
                        Buffer.BlockCopy(mBuffs[index].GetBuffer(), num24, tshBuffer, num8, count);
                        num24 = 0;
                        num28 -= count;
                        num8 += count;
                        index++;
                        if ((index < numBuffs) && (num28 > 0))
                        {
                            count = (int) mBuffs[index].Length;
                            goto Label_05BD;
                        }
                    }
                Label_0639:
                    tsh.ControlFlags1 = num19;
                    tsh.WriteStruct(tshBuffer, 0);
                    tsh.TshBuffer = tshBuffer;
                    session.SendTSH(tsh);
                    tshBuffer = null;
                    num19 = (byte) (num19 & 0xef);
                    srcOffset += num10;
                    if ((num19 & 0x20) == 0)
                    {
                        goto Label_0394;
                    }
                    if (flag)
                    {
                        rTSH = session.ReceiveTSH(null);
                        switch (rTSH.SegmentType)
                        {
                            case 5:
                                mqerd = new MQERD();
                                if (rTSH.Length > offset)
                                {
                                    mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                }
                                throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());

                            case 0x97:
                                num12 = rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                compCode = mqapi.mqapi.CompCode;
                                reason = mqapi.mqapi.Reason;
                                if (compCode != 0)
                                {
                                    return session;
                                }
                                num14 = num13 = rTSH.Offset = mqod.ReadStruct(rTSH.TshBuffer, rTSH.Offset, rTSH.Length);
                                num16 = num15 = rTSH.Offset = mqmd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                num17 = rTSH.Offset = mqpmo.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                goto Label_0862;
                        }
                        string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                        CommonServices.SetValidInserts();
                        CommonServices.ArithInsert1 = rTSH.SegmentType;
                        CommonServices.CommentInsert1 = channelName;
                        CommonServices.CommentInsert2 = "Unexpected flow received during MQPUT1";
                        base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009511, 0);
                        throw MQERD.ErrorException(0x20009511, rTSH.SegmentType, channelName);
                    }
                    compCode = 0;
                    reason = 0;
                    base.TrText(method, string.Concat(new object[] { "ASynchronous Delivery completed successfully, CompCode = ", (int) compCode, " Reason = ", (int) reason }));
                Label_0862:
                    if (mqpmo != null)
                    {
                        mqpmo.Options = options;
                    }
                    return session;
                }
                finally
                {
                    mqod.UseNativePtrSz = true;
                    if (flag2 && flag)
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "MQPUT1 <<");
                        if (num12 != num13)
                        {
                            base.TrData(method, 0, "Objdesc", num12, num13 - num12, rTSH.TshBuffer);
                        }
                        if (num14 != num15)
                        {
                            base.TrData(method, 0, "Msgdesc", num14, num15 - num14, rTSH.TshBuffer);
                        }
                        if (num16 != num17)
                        {
                            base.TrData(method, 0, "Putmsgopts", num16, num17 - num16, rTSH.TshBuffer);
                        }
                        base.TrAPIInput(method, "Bufferlength");
                        base.TrAPIInput(method, "Buffer");
                        base.TrAPICCRC(method, compCode, reason);
                    }
                    else
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "MQPUT1 <<");
                        base.TrAPICCRC(method, compCode, reason);
                    }
                    try
                    {
                        if (managedHconn != null)
                        {
                            if (rTSH != null)
                            {
                                try
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                                catch (NmqiException)
                                {
                                }
                            }
                            managedHconn.LeaveCall(reason);
                        }
                    }
                    catch (NmqiException exception9)
                    {
                        base.TrException(method, exception9, 1);
                        compCode = exception9.CompCode;
                        reason = exception9.Reason;
                    }
                }
                return session;
            }
            catch (NmqiException exception10)
            {
                base.TrException(method, exception10, 2);
                compCode = exception10.CompCode;
                reason = exception10.Reason;
            }
            catch (MQException exception11)
            {
                base.TrException(method, exception11, 3);
                compCode = exception11.CompCode;
                reason = exception11.Reason;
            }
            catch (Exception exception12)
            {
                base.TrException(method, exception12, 4);
                compCode = 2;
                reason = 0x893;
            }
            finally
            {
                base.TrExit(method);
            }
            return session;
        }

        public void MQSET(Hconn hConn, Hobj hObj, int selectorCount, int[] selectors, int intAttrCount, int[] intAttrs, int charAttrLength, byte[] charAttrs, out int compCode, out int reason)
        {
            uint method = 0x8f;
            this.TrEntry(method, new object[] { hConn, hObj, selectorCount, selectors, intAttrCount, intAttrs, charAttrLength, charAttrs, "compCode : out", "reason : out" });
            int offset = 0;
            int num3 = 0;
            int num4 = 0;
            int translength = 0;
            int num7 = 0;
            int num8 = 0x10;
            MQERD mqerd = null;
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQAPI mqapi = new MQAPI();
            ManagedHconn managedHconn = null;
            MQSession session = null;
            bool flag = CommonServices.TraceStatus();
            compCode = 2;
            reason = 0x7e2;
            this.InExit();
            try
            {
                if ((selectors == null) || (selectors.Length < selectorCount))
                {
                    compCode = 2;
                    reason = 0x811;
                    NmqiException exception = new NmqiException(base.env, -1, null, compCode, reason, null);
                    throw exception;
                }
                int num9 = 0;
                for (int i = 0; i < selectorCount; i++)
                {
                    if (selectors[i] <= 0x7d0)
                    {
                        num9++;
                    }
                }
                if ((num9 > 0) && ((intAttrs == null) || (intAttrCount > intAttrs.Length)))
                {
                    NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x7e5, null);
                    throw exception2;
                }
                int num11 = 0;
                for (int j = 0; j < selectorCount; j++)
                {
                    if ((selectors[j] >= 0x7d1) && (selectors[j] <= 0xfa0))
                    {
                        num11++;
                    }
                }
                if ((num11 > 0) && ((charAttrs == null) || (charAttrs.Length < num11)))
                {
                    compCode = 2;
                    reason = 0x7d7;
                    NmqiException exception3 = new NmqiException(base.env, -1, null, compCode, reason, null);
                    throw exception3;
                }
                if ((num9 + num11) != selectorCount)
                {
                    compCode = 2;
                    reason = 0x813;
                    NmqiException exception4 = new NmqiException(base.env, -1, null, compCode, reason, null);
                    throw exception4;
                }
                managedHconn = this.GetManagedHconn(hConn);
                session = managedHconn.Session;
                num7 = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                translength = ((num7 + num8) + charAttrLength) + (((intAttrCount + selectorCount) + 3) * 4);
                managedHconn.EnterCall(this.IsDispatchThread(), false);
                try
                {
                    session.CheckIfDisconnected();
                    tsh = session.AllocateTSH(0x88, 0, true, translength);
                    byte[] tshBuffer = tsh.TshBuffer;
                    mqapi.Initialize(translength, hObj.Handle);
                    int num5 = tsh.WriteStruct(tshBuffer, 0);
                    num5 += mqapi.WriteStruct(tshBuffer, num5);
                    BitConverter.GetBytes(selectorCount).CopyTo(tshBuffer, num5);
                    num5 += 4;
                    BitConverter.GetBytes(intAttrCount).CopyTo(tshBuffer, num5);
                    num5 += 4;
                    BitConverter.GetBytes(charAttrLength).CopyTo(tshBuffer, num5);
                    offset = num5 += 4;
                    num5 = this.WriteIntArray(tshBuffer, num5, selectors, selectorCount);
                    if ((intAttrCount != 0) && (intAttrs != null))
                    {
                        num3 = num5;
                        num5 = this.WriteIntArray(tshBuffer, num5, intAttrs, intAttrCount);
                    }
                    if ((charAttrLength != 0) && (charAttrs != null))
                    {
                        num4 = num5;
                        Buffer.BlockCopy(charAttrs, 0, tshBuffer, num5, charAttrLength);
                        num5 += charAttrLength;
                    }
                    if (flag)
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "MQSET >>");
                        base.TrData(method, 0, "Hconn", BitConverter.GetBytes(hConn.Value));
                        base.TrData(method, 0, "Hobj", BitConverter.GetBytes(hObj.Handle));
                        base.TrData(method, 0, "Selectorcount", BitConverter.GetBytes(selectorCount));
                        base.TrData(method, 0, "Selectors", offset, selectorCount * 4, tshBuffer);
                        base.TrData(method, 0, "Intattrcount", BitConverter.GetBytes(intAttrCount));
                        if (num3 != 0)
                        {
                            base.TrData(method, 0, "Intattrs", num3, intAttrCount * 4, tshBuffer);
                        }
                        base.TrData(method, 0, "Charattrlength", BitConverter.GetBytes(charAttrLength));
                        if (num4 != 0)
                        {
                            base.TrData(method, 0, "Charattrs", num4, charAttrLength, tshBuffer);
                        }
                        base.TrAPIOutput(method, "CompCode");
                        base.TrAPIOutput(method, "Reason");
                    }
                    tsh.TshBuffer = tshBuffer;
                    session.SendTSH(tsh);
                    tshBuffer = null;
                    rTSH = session.ReceiveTSH(null);
                    switch (rTSH.SegmentType)
                    {
                        case 5:
                            mqerd = new MQERD();
                            if (rTSH.Length > num7)
                            {
                                mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            }
                            throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());

                        case 0x98:
                            break;

                        default:
                        {
                            string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                            CommonServices.SetValidInserts();
                            CommonServices.ArithInsert1 = rTSH.SegmentType;
                            CommonServices.CommentInsert1 = channelName;
                            CommonServices.CommentInsert2 = "Unexpected flow received during MQSET";
                            base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                            throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                        }
                    }
                    rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                    compCode = mqapi.mqapi.CompCode;
                    reason = mqapi.mqapi.Reason;
                }
                catch (NmqiException exception5)
                {
                    base.TrException(method, exception5, 1);
                    compCode = exception5.CompCode;
                    reason = exception5.Reason;
                }
                finally
                {
                    try
                    {
                        if (managedHconn != null)
                        {
                            if (rTSH != null)
                            {
                                try
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                                catch (NmqiException)
                                {
                                }
                            }
                            managedHconn.LeaveCall(reason);
                        }
                    }
                    catch (NmqiException exception6)
                    {
                        base.TrException(method, exception6, 2);
                        compCode = exception6.CompCode;
                        reason = exception6.Reason;
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    base.TrAPI(method, "__________");
                    base.TrAPI(method, "MQSET <<");
                    base.TrAPICCRC(method, compCode, reason);
                }
                base.TrExit(method);
            }
        }

        public void MQSTAT(Hconn hConn, int type, MQAsyncStatus stat, out int compCode, out int reason)
        {
            int num9;
            uint method = 150;
            this.TrEntry(method, new object[] { hConn, type, stat, "compCode : out", "reason : out" });
            int versionLength = stat.GetVersionLength();
            int num3 = 4;
            int translength = 0;
            int num5 = 0;
            int num6 = 0x10;
            bool flag = CommonServices.TraceStatus();
            MQERD mqerd = null;
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQAPI mqapi = new MQAPI();
            ManagedHconn managedHconn = null;
            compCode = 2;
            reason = 0x7e2;
            int offset = num9 = 0;
            this.InExit();
            try
            {
                managedHconn = this.GetManagedHconn(hConn);
                MQSession session = managedHconn.Session;
                num5 = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                translength = ((num5 + num6) + num3) + versionLength;
                if (session.FapLevel < 9)
                {
                    compCode = 2;
                    reason = 0x8fa;
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x8fa, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                if (stat == null)
                {
                    NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x97a, null);
                    base.TrException(method, exception2);
                    throw exception2;
                }
                managedHconn.EnterCall(this.IsDispatchThread(), false);
                try
                {
                    session.CheckIfDisconnected();
                    tsh = session.AllocateTSH(0x8d, 0, true, translength);
                    byte[] tshBuffer = tsh.TshBuffer;
                    int num7 = tsh.WriteStruct(tshBuffer, 0);
                    mqapi.Initialize(translength, 0);
                    num7 += mqapi.WriteStruct(tshBuffer, num7);
                    BitConverter.GetBytes(type).CopyTo(tshBuffer, num7);
                    num7 += 4;
                    offset = num7;
                    num9 = num7 += stat.WriteStruct(tshBuffer, num7);
                    if (flag)
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "MQSTAT >>");
                        base.TrData(method, 0, "Hconn", BitConverter.GetBytes(hConn.Value));
                        base.TrData(method, 0, "Type", BitConverter.GetBytes(type));
                        base.TrData(method, 0, "Status", offset, num9 - offset, tshBuffer);
                        base.TrAPIOutput(method, "CompCode");
                        base.TrAPIOutput(method, "Reason");
                        offset = num9 = 0;
                    }
                    tsh.TshBuffer = tshBuffer;
                    session.SendTSH(tsh);
                    tshBuffer = null;
                    rTSH = session.ReceiveTSH(null);
                    switch (rTSH.SegmentType)
                    {
                        case 5:
                            mqerd = new MQERD();
                            if (rTSH.Length > num5)
                            {
                                mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            }
                            throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());

                        case 0x9d:
                            break;

                        default:
                        {
                            string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                            CommonServices.SetValidInserts();
                            CommonServices.ArithInsert1 = rTSH.SegmentType;
                            CommonServices.CommentInsert1 = channelName;
                            CommonServices.CommentInsert2 = "Unexpected flow received during MQSTAT";
                            base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                            throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                        }
                    }
                    rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                    compCode = mqapi.mqapi.CompCode;
                    reason = mqapi.mqapi.Reason;
                    if (compCode == 0)
                    {
                        offset = rTSH.Offset += 4;
                        num9 = rTSH.Offset = stat.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                    }
                }
                finally
                {
                    try
                    {
                        if (flag)
                        {
                            base.TrAPI(method, "__________");
                            base.TrAPI(method, "MQSTAT <<");
                            base.TrAPIInput(method, "Hconn");
                            base.TrAPIInput(method, "Type");
                            if (offset != num9)
                            {
                                base.TrData(method, 0, "Status", offset, num9 - offset, rTSH.TshBuffer);
                            }
                        }
                        if (managedHconn != null)
                        {
                            if (rTSH != null)
                            {
                                try
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                                catch (NmqiException)
                                {
                                }
                            }
                            managedHconn.LeaveCall(reason);
                        }
                    }
                    catch (NmqiException exception3)
                    {
                        base.TrException(method, exception3, 1);
                        compCode = exception3.CompCode;
                        reason = exception3.Reason;
                    }
                }
            }
            catch (NmqiException exception4)
            {
                base.TrException(method, exception4, 2);
                compCode = exception4.CompCode;
                reason = exception4.Reason;
            }
            catch (Exception exception5)
            {
                base.TrException(method, exception5);
                compCode = 2;
                reason = 0x893;
            }
            finally
            {
                if ((reason != 0) && managedHconn.IsReconnectable)
                {
                    if (ManagedHconn.IsReconnectableReasonCode(reason) && !managedHconn.HasFailed())
                    {
                        this.MQSTAT(hConn, type, stat, out compCode, out reason);
                    }
                    if (managedHconn.HasFailed())
                    {
                        compCode = managedHconn.ReconnectionFailureCompCode;
                        reason = managedHconn.ReconnectionFailureReason;
                    }
                }
                base.TrExit(method);
            }
        }

        public void MQSUB(Hconn hconn, MQSubscriptionDescriptor pSubDesc, Phobj pHobj, Phobj pHsub, out int pCompCode, out int pReason)
        {
            uint method = 0x5c0;
            this.TrEntry(method, new object[] { hconn, pSubDesc, pHobj, pHsub, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                this.MQSUB(hconn, pSubDesc, pHobj, pHsub, out pCompCode, out pReason, null);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQSUB(Hconn hconn, MQSubscriptionDescriptor pSubDesc, Phobj pHobj, Phobj pHsub, out int pCompCode, out int pReason, ManagedHsub rcnHsub)
        {
            uint method = 0x5bf;
            this.TrEntry(method, new object[] { hconn, pSubDesc, pHobj, pHsub, "pCompCode : out", "pReason : out", rcnHsub });
            pCompCode = 0;
            pReason = 0;
            try
            {
                MQSession failedSession = this.nmqiSubscribe(hconn, pSubDesc, ref pHobj, ref pHsub, out pCompCode, out pReason, null, false, rcnHsub);
                ManagedHconn hconn2 = hconn as ManagedHconn;
                if (((pReason != 0) && hconn2.IsReconnectable) && !this.ThreadIsReconnectThread())
                {
                    if (ManagedHconn.IsReconnectableReasonCode(pReason) && !hconn2.HasFailed())
                    {
                        try
                        {
                            hconn2.Reconnect(failedSession);
                            this.MQSUB(hconn, pSubDesc, pHobj, pHsub, out pCompCode, out pReason);
                        }
                        catch (NmqiException)
                        {
                        }
                    }
                    if (hconn2.HasFailed())
                    {
                        pCompCode = hconn2.ReconnectionFailureCompCode;
                        pReason = hconn2.ReconnectionFailureReason;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQSUBRQ(Hconn hConn, Hobj hSub, int action, ref MQSubscriptionRequestOptions mqsro, out int compCode, out int reason)
        {
            int num8;
            uint method = 0x9d;
            this.TrEntry(method, new object[] { hConn, hSub, action, mqsro, "compCode : out", "reason : out" });
            int length = mqsro.GetLength();
            int translength = 0;
            int num4 = 0x10;
            int num5 = 0;
            bool flag = CommonServices.TraceStatus();
            MQERD mqerd = null;
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQAPI mqapi = new MQAPI();
            ManagedHconn managedHconn = null;
            MQSession session = null;
            compCode = 2;
            reason = 0x7e2;
            int offset = num8 = 0;
            if (session.FapLevel < 9)
            {
                compCode = 2;
                reason = 0x8fa;
                base.TrExit(method);
            }
            else
            {
                this.InExit();
                try
                {
                    managedHconn = this.GetManagedHconn(hConn);
                    session = managedHconn.Session;
                    num5 = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                    translength = ((num5 + num4) + length) + 4;
                    if (hSub == null)
                    {
                        NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x7e3, null);
                        base.TrException(method, ex);
                        throw ex;
                    }
                    if (mqsro == null)
                    {
                        NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x986, null);
                        base.TrException(method, exception2);
                        throw exception2;
                    }
                    if (managedHconn.FapLevel < 9)
                    {
                        NmqiException exception3 = new NmqiException(base.env, -1, null, 2, 0x8fa, null);
                        base.TrException(method, exception3);
                        throw exception3;
                    }
                    managedHconn.EnterCall(this.IsDispatchThread(), false);
                    try
                    {
                        tsh = session.AllocateTSH(0x8f, 0, true, translength);
                        byte[] tshBuffer = tsh.TshBuffer;
                        int num6 = tsh.WriteStruct(tshBuffer, 0);
                        mqapi.Initialize(translength, hSub.Handle);
                        num6 += mqapi.WriteStruct(tshBuffer, num6);
                        BitConverter.GetBytes(action).CopyTo(tshBuffer, num6);
                        num6 += 4;
                        offset = num6;
                        num8 = num6 += mqsro.WriteStruct(tshBuffer, num6);
                        if (flag)
                        {
                            base.TrAPI(method, "__________");
                            base.TrAPI(method, "MQSUBRQ  >>");
                            base.TrData(method, 0, "Hconn", BitConverter.GetBytes(hConn.Value));
                            base.TrData(method, 0, "Objdesc", offset, num8 - offset, tshBuffer);
                            base.TrAPIOutput(method, "CompCode");
                            base.TrAPIOutput(method, "Reason");
                            offset = num8 = 0;
                        }
                        tsh.Offset = 0;
                        num6 = 0;
                        session.SendTSH(tsh);
                        rTSH = session.ReceiveTSH(null);
                        switch (rTSH.SegmentType)
                        {
                            case 5:
                                mqerd = new MQERD();
                                if (rTSH.Length > num5)
                                {
                                    mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                }
                                throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());

                            case 0x9f:
                                break;

                            default:
                            {
                                string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                                CommonServices.SetValidInserts();
                                CommonServices.ArithInsert1 = rTSH.SegmentType;
                                CommonServices.CommentInsert1 = channelName;
                                CommonServices.CommentInsert2 = "Unexpected flow received during MQSUBRQ";
                                base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                                throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                            }
                        }
                        rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                        compCode = mqapi.mqapi.CompCode;
                        reason = mqapi.mqapi.Reason;
                        if (compCode == 0)
                        {
                            offset = num6 += 4;
                            num8 = rTSH.Offset = mqsro.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                        }
                    }
                    finally
                    {
                        try
                        {
                            if (flag)
                            {
                                base.TrAPI(method, "__________");
                                base.TrAPI(method, "MQSUBRQ  <<");
                                base.TrAPIInput(method, "Hconn");
                                if (offset != num8)
                                {
                                    base.TrData(method, 0, "Objdesc", offset, num8 - offset, rTSH.TshBuffer);
                                }
                            }
                            if (managedHconn != null)
                            {
                                if (rTSH != null)
                                {
                                    try
                                    {
                                        session.ReleaseReceivedTSH(rTSH);
                                    }
                                    catch (NmqiException)
                                    {
                                    }
                                }
                                managedHconn.LeaveCall(reason);
                            }
                        }
                        catch (NmqiException)
                        {
                        }
                    }
                }
                catch (NmqiException exception4)
                {
                    compCode = exception4.CompCode;
                    reason = exception4.Reason;
                    base.TrException(method, exception4);
                }
                catch (Exception exception5)
                {
                    compCode = 2;
                    reason = 0x893;
                    base.TrException(method, exception5);
                }
                finally
                {
                    base.TrExit(method);
                }
            }
        }

        public void NmqiConnect(string pQMgrName, NmqiConnectOptions pNmqiConnectOpts, MQConnectOptions pConnectOpts, Hconn parentHconn, Phconn pHconn, out int pCompCode, out int pReason)
        {
            uint method = 0x5ac;
            this.TrEntry(method, new object[] { pQMgrName, pNmqiConnectOpts, pConnectOpts, parentHconn, pHconn, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                this.NmqiConnect(pQMgrName, pNmqiConnectOpts, pConnectOpts, parentHconn, pHconn, out pCompCode, out pReason, null);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void NmqiConnect(string name, NmqiConnectOptions pNmqiConnectOpts, MQConnectOptions cno, Hconn parentHconn, Phconn pHconn, out int compCode, out int reason, ManagedHconn rcnHconn)
        {
            uint method = 0x5ae;
            this.TrEntry(method, new object[] { name, pNmqiConnectOpts, cno, parentHconn, pHconn, "compCode : out", "reason : out", rcnHconn });
            bool flag = rcnHconn != null;
            Exception nestedException = null;
            ManagedHconn hconn = rcnHconn;
            MQSession remoteSession = null;
            MQChannelDefinition mqcd = null;
            int options = 0;
            bool flag2 = false;
            int num3 = 0;
            MQConnectionSecurityParameters mqcsp = null;
            MQSSLConfigOptions sslConfigOptions = null;
            string uidFlowUserid = null;
            string uidFlowPassword = null;
            bool flag3 = CommonServices.TraceEnabled();
            MQThreadChannelEntry threadChlEntry = new MQThreadChannelEntry();
            compCode = 0;
            reason = 0;
            this.InExit();
            Hconn hConn = pHconn.HConn;
            try
            {
                if (hConn is ManagedHconn)
                {
                    ManagedHconn hconn3 = (ManagedHconn) hConn;
                    MQChannelDefinition negotiatedChannel = hconn3.ParentConnection.NegotiatedChannel;
                    if (negotiatedChannel != null)
                    {
                        if (cno != null)
                        {
                            options = cno.Options;
                        }
                        else
                        {
                            cno = base.env.NewMQCNO();
                        }
                        options |= 0x100000;
                        cno.Options = options;
                        cno.ClientConn = negotiatedChannel;
                    }
                }
                if ((parentHconn != null) && !(parentHconn is ManagedHconn))
                {
                    compCode = 2;
                    reason = 0x8f6;
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x8f6, null);
                    base.env.LastException = ex;
                    base.TrException(method, ex);
                    throw ex;
                }
                if (((parentHconn != null) && (parentHconn is ManagedHconn)) && ((ManagedHconn) parentHconn).IsQuiescing())
                {
                    base.TrText(method, "ParentHconn was notified on connection quiescing. At this time we will not allow new connections to be created.");
                    base.throwNewMQException(2, 0x89a);
                }
                bool flag4 = false;
                object data = CallContext.GetData("inExit");
                if (data != null)
                {
                    flag4 = (bool) data;
                }
                if (flag4)
                {
                    compCode = 2;
                    reason = 0x8ab;
                    NmqiException exception3 = new NmqiException(base.env, -1, null, 2, 0x8ab, null);
                    base.env.LastException = exception3;
                    base.TrException(method, exception3);
                    throw exception3;
                }
                if (cno != null)
                {
                    options = cno.Options;
                    if (((options & 0x40000) != 0) && ((options & 0x10000) != 0))
                    {
                        compCode = 2;
                        reason = 0x7fe;
                        NmqiException exception4 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                        base.env.LastException = exception4;
                        base.TrException(method, exception4);
                        throw exception4;
                    }
                    if (((options & 0x80000) != 0) && ((options & 0x100000) != 0))
                    {
                        compCode = 2;
                        reason = 0x7fe;
                        NmqiException exception5 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                        base.env.LastException = exception5;
                        base.TrException(method, exception5);
                        throw exception5;
                    }
                    if ((((options & 0x80000) != 0) || ((options & 0x100000) != 0)) && (cno.ClientConn == null))
                    {
                        compCode = 2;
                        reason = 0x8e5;
                        NmqiException exception6 = new NmqiException(base.env, -1, null, 2, 0x8e5, null);
                        base.env.LastException = exception6;
                        base.TrException(method, exception6);
                        throw exception6;
                    }
                    if ((cno.Options & 0x7000000) == 0)
                    {
                        flag2 = true;
                        base.env.Cfg.GetStringValue(MQClientCfg.CHANNEL_RECON);
                        int num4 = 0;
                        string str3 = base.env.Cfg.GetStringValue(MQClientCfg.CHANNEL_RECON);
                        if (str3.Trim().ToUpper().Equals("YES"))
                        {
                            num4 = 0x1000000;
                        }
                        else if (str3.Trim().ToUpper().Equals("QMGR"))
                        {
                            num4 = 0x4000000;
                        }
                        else if (str3.Trim().ToUpper().Equals("DISABLED"))
                        {
                            num4 = 0x2000000;
                        }
                        cno.Options |= num4;
                    }
                    if ((cno.Options & 0x2000000) != 0)
                    {
                        cno.Options &= -117440513;
                    }
                    if ((options & 0x80000) == 0)
                    {
                        mqcd = cno.ClientConn;
                    }
                    if ((mqcd != null) && (mqcd.ChannelType != 6))
                    {
                        compCode = 2;
                        reason = 0x8e5;
                        NmqiException exception7 = new NmqiException(base.env, -1, null, 2, 0x8e5, null);
                        base.env.LastException = exception7;
                        base.TrException(method, exception7);
                        throw exception7;
                    }
                    num3 = options & 0xe0;
                    if (num3 == 0)
                    {
                        num3 = 0x20;
                    }
                    if (((num3 != 0x20) && (num3 != 0x40)) && (num3 != 0x80))
                    {
                        compCode = 2;
                        reason = 0x7fe;
                        NmqiException exception8 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                        base.env.LastException = exception8;
                        base.TrException(method, exception8);
                        throw exception8;
                    }
                }
                sslConfigOptions = cno.SslConfigOptions;
                if (sslConfigOptions != null)
                {
                    if (MQEnvironment.SSLCertRevocationCheck)
                    {
                        sslConfigOptions.CertRevocationCheck = true;
                    }
                    if (pNmqiConnectOpts.SSLCertRevocationCheck)
                    {
                        sslConfigOptions.CertRevocationCheck = true;
                    }
                }
                if (sslConfigOptions == null)
                {
                    string str4 = base.env.Cfg.GetStringValue(MQClientCfg.SSL_SSLKeyRepository);
                    int intValue = 0;
                    if (str4 != null)
                    {
                        sslConfigOptions = new MQSSLConfigOptions();
                        sslConfigOptions.KeyRepository = str4;
                        intValue = base.env.Cfg.GetIntValue(MQClientCfg.SSL_SSLKeyResetCount);
                        if (intValue > 0)
                        {
                            sslConfigOptions.KeyResetCount = intValue;
                        }
                        cno.SslConfigOptions = sslConfigOptions;
                    }
                }
                if (pNmqiConnectOpts != null)
                {
                    int fapLevel = pNmqiConnectOpts.FapLevel;
                }
                MQChannelListEntry nameListFromMQCD = null;
                if ((mqcd == null) || ((mqcd.ChannelName == "") && (mqcd.ConnectionName == "")))
                {
                    mqcd = this.ReadMQServer();
                }
                if ((mqcd == null) || ((mqcd.ChannelName == "") && (mqcd.ConnectionName == "")))
                {
                    if (pNmqiConnectOpts.CCDTUrl != null)
                    {
                        base.TrText(method, "CCDT URL from XMS :  " + pNmqiConnectOpts.CCDTUrl);
                    }
                    nameListFromMQCD = this.CreateNameListEntryFromCCDT(name, pNmqiConnectOpts.CCDTUrl);
                }
                else if (mqcd.ConnectionName.IndexOf(",") != -1)
                {
                    nameListFromMQCD = this.GetNameListFromMQCD(mqcd, name);
                }
                bool flag5 = true;
                string stringValue = base.env.Cfg.GetStringValue(MQClientCfg.ENV_MQ_LCLADDR);
                traceIdentifier++;
                if (nameListFromMQCD != null)
                {
                    mqcd = nameListFromMQCD.SelectChannelEntry(((options & 0x100000) != 0) ? mqcd : null, threadChlEntry);
                }
                while (flag5)
                {
                    string[] strArray;
                    if (mqcd != null)
                    {
                        mqcd.TraceFields();
                    }
                    if (mqcd == null)
                    {
                        if (nestedException != null)
                        {
                            base.env.LastException = (NmqiException) nestedException;
                            throw nestedException;
                        }
                        compCode = 2;
                        reason = 0x8e6;
                        NmqiException exception9 = new NmqiException(base.env, -1, null, 2, 0x8e6, null);
                        base.env.LastException = exception9;
                        base.TrException(method, exception9);
                        throw exception9;
                    }
                    if ((mqcd.ChannelName == "") && (mqcd.ConnectionName != ""))
                    {
                        compCode = 2;
                        reason = 0x8e5;
                        NmqiException exception10 = new NmqiException(base.env, -1, null, 2, 0x8e5, null);
                        base.env.LastException = exception10;
                        base.TrException(method, exception10);
                        throw exception10;
                    }
                    if (((mqcd.LocalAddress == null) || (mqcd.LocalAddress.Trim().Length == 0)) && (stringValue != null))
                    {
                        mqcd.LocalAddress = stringValue;
                    }
                    if (((nameListFromMQCD != null) && (pNmqiConnectOpts != null)) && ((pNmqiConnectOpts.Flags & 0x40) != 0))
                    {
                        mqcd.SharingConversations = pNmqiConnectOpts.SharingConversations;
                    }
                    try
                    {
                        uidFlowUserid = pNmqiConnectOpts.UserIdentifier;
                        uidFlowPassword = pNmqiConnectOpts.Password;
                        mqcsp = cno.SecurityParms;
                        int connectOptions = cno.Options;
                        flag5 = false;
                        remoteSession = this.connectionFactory.GetSession(connectOptions, sslConfigOptions, mqcsp, mqcd, name, pNmqiConnectOpts.Flags, uidFlowUserid, uidFlowPassword, pNmqiConnectOpts.QueueManagerCCSID, pNmqiConnectOpts.FapLevel);
                        if (pNmqiConnectOpts != null)
                        {
                            pNmqiConnectOpts.FapLevel = remoteSession.FapLevel;
                        }
                        if (flag)
                        {
                            hconn.Session = remoteSession;
                        }
                        else
                        {
                            hconn = new ManagedHconn(base.env, remoteSession, name, cno);
                        }
                        pHconn.HConn = hconn;
                        if (parentHconn != null)
                        {
                            hconn.ParentPhconn = new Phconn(base.env);
                            hconn.ParentPhconn.HConn = parentHconn;
                            hconn.Parent = (ManagedHconn) parentHconn;
                        }
                        remoteSession.Hconn = hconn;
                        pHconn.HConn = hconn;
                        if (((num3 != 0x20) && (num3 != 0x40)) && (num3 != 0x80))
                        {
                            compCode = 2;
                            reason = 0x7fe;
                            NmqiException exception11 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                            base.TrException(method, exception11);
                            throw exception11;
                        }
                        hconn.ShareOption = num3;
                        if (name == null)
                        {
                            compCode = 2;
                            reason = 0x80a;
                            NmqiException exception12 = new NmqiException(base.env, -1, null, 2, 0x80a, null);
                            base.TrException(method, exception12);
                            throw exception12;
                        }
                        if (((name.Length != 0) && (name.ToCharArray()[0] == ' ')) && (name.Trim().Length != 0))
                        {
                            compCode = 2;
                            reason = 0x80a;
                            NmqiException exception13 = new NmqiException(base.env, -1, null, 2, 0x80a, null);
                            base.TrException(method, exception13);
                            throw exception13;
                        }
                        remoteSession.OriginalQueueManagerName = name;
                        IBM.WMQ.MQCONN mqconn = new IBM.WMQ.MQCONN();
                        MQFAPMQCNO mqfapmqcno = new MQFAPMQCNO();
                        MQAPI mqapi = new MQAPI();
                        int translength = 0;
                        int offset = 0;
                        int num9 = 0;
                        MQTSH tsh = null;
                        MQTSH rTSH = null;
                        compCode = 2;
                        reason = 0x80b;
                        int num10 = 0;
                        int length = mqapi.GetLength();
                        try
                        {
                            num10 = remoteSession.IsMultiplexingEnabled ? 0x24 : 0x1c;
                            int num12 = 0;
                            if (remoteSession.FapLevel >= 8)
                            {
                                num12 = mqfapmqcno.GetLength();
                            }
                            if (pNmqiConnectOpts.ReconnectionID != null)
                            {
                                num12 += pNmqiConnectOpts.ReconnectionID.Length;
                            }
                            if (pNmqiConnectOpts.RemoteQMID != null)
                            {
                                num12 += pNmqiConnectOpts.RemoteQMID.Length;
                            }
                            num12 += (num10 + length) + mqconn.GetLength();
                            translength += num12;
                            tsh = remoteSession.AllocateTSH(0x81, 0, true, translength);
                            byte[] tshBuffer = tsh.TshBuffer;
                            offset = tsh.WriteStruct(tshBuffer, offset);
                            mqapi.Initialize(translength, 0);
                            offset += mqapi.WriteStruct(tshBuffer, offset);
                            int num13 = offset;
                            mqconn.QmgrName = hconn.QmName;
                            mqconn.ApplName = CommonServices.ProgramName;
                            mqconn.mqconn.ApplType = 11;
                            mqconn.mqconn.Options = 1;
                            if (cno != null)
                            {
                                mqconn.mqconn.Options |= 2;
                                mqconn.mqconn.XOptions = cno.Options & -1900769;
                                if (hconn.FapLevel >= 8)
                                {
                                    mqfapmqcno.ConnTag = cno.ConnTag;
                                }
                                if ((hconn.FapLevel < 10) && ((cno.Options & 0x5000000) > 0))
                                {
                                    if (!flag2)
                                    {
                                        NmqiException exception14 = new NmqiException(base.env, -1, null, 2, 0x7dc, null);
                                        base.TrException(method, exception14);
                                        throw exception14;
                                    }
                                    cno.Options &= -83886081;
                                    mqconn.mqconn.XOptions &= -83886081;
                                }
                            }
                            offset += mqconn.WriteStruct(tshBuffer, offset);
                            if (remoteSession.FapLevel >= 8)
                            {
                                offset += mqfapmqcno.WriteStruct(tshBuffer, offset);
                            }
                            if ((((hconn.FapLevel >= 10) && hconn.IsReconnectable) && ((rcnHconn != null) && (pNmqiConnectOpts.ReconnectionID != null))) && (pNmqiConnectOpts.RemoteQMID != null))
                            {
                                mqconn.mqconn.Options |= 4;
                                mqconn.WriteStruct(tshBuffer, num13);
                                byte[] reconnectionID = pNmqiConnectOpts.ReconnectionID;
                                reconnectionID.CopyTo(tshBuffer, offset);
                                offset += reconnectionID.Length;
                                pNmqiConnectOpts.RemoteQMID.CopyTo(tshBuffer, offset);
                                offset += 0x30;
                            }
                            if (flag3)
                            {
                                if (cno != null)
                                {
                                    base.TrAPI(method, "MQCONNX >>");
                                }
                                else
                                {
                                    base.TrAPI(method, "MQCONN >>");
                                }
                                base.TrData(method, 0, "Name", mqconn.mqconn.QMgrName);
                                base.TrData(method, 0, "Buffer", 0, tsh.Length, tshBuffer);
                                base.TrAPIOutput(method, "Hconn");
                                base.TrAPIOutput(method, "CompCode");
                                base.TrAPIOutput(method, "Reason");
                            }
                            tsh.TshBuffer = tshBuffer;
                            tsh.Offset = 0;
                            remoteSession.SendTSH(tsh);
                            rTSH = remoteSession.ReceiveTSH(null);
                            byte segmentType = rTSH.SegmentType;
                            if (segmentType == 5)
                            {
                                MQERD mqerd = new MQERD();
                                if (num9 > rTSH.GetLength())
                                {
                                    mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                }
                                throw mqerd.ErrorException(remoteSession.ParentConnection.NegotiatedChannel.ChannelName.Trim());
                            }
                            if (segmentType != 0x91)
                            {
                                CommonServices.SetValidInserts();
                                CommonServices.ArithInsert1 = rTSH.SegmentType;
                                CommonServices.CommentInsert1 = remoteSession.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                                CommonServices.CommentInsert2 = "Unexpected response received during MQCONNX";
                                base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009508, 0);
                                throw MQERD.ErrorException(0x20009508, rTSH.SegmentType, remoteSession.Connection.NegotiatedChannel.ChannelName.Trim());
                            }
                            rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            compCode = mqapi.mqapi.CompCode;
                            reason = mqapi.mqapi.Reason;
                            if (compCode == 2)
                            {
                                base.TrText(method, string.Concat(new object[] { "MQCONNX flow failed with CompCode = ", (int) compCode, ", Reason = ", (int) reason }));
                                remoteSession.Disconnect();
                                remoteSession = null;
                            }
                            if ((compCode != 2) && (compCode == 0))
                            {
                                rTSH.Offset = mqconn.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                if ((remoteSession.FapLevel >= 8) && (cno != null))
                                {
                                    rTSH.Offset = mqfapmqcno.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                    remoteSession.ConnectionId = mqfapmqcno.ConnectionId;
                                    cno.ConnectionId = mqfapmqcno.ConnectionId;
                                }
                                pHconn.HConn.Value = Interlocked.Increment(ref incrementingHconn);
                                base.TrText(method, "Current Hconn value = " + incrementingHconn);
                                if (threadChlEntry.ThisWeightedEntry != null)
                                {
                                    nameListFromMQCD.ThisWeightedEntry = threadChlEntry.ThisWeightedEntry;
                                }
                                if ((cno.Options & 0x80000) != 0)
                                {
                                    if (threadChlEntry.ThisWeightedEntry != null)
                                    {
                                        cno.ClientConn = threadChlEntry.ThisWeightedEntry.Channel;
                                    }
                                    else
                                    {
                                        cno.ClientConn = threadChlEntry.ThisAlphaEntry.Channel;
                                    }
                                }
                                if (nameListFromMQCD != null)
                                {
                                    nameListFromMQCD.UseCount--;
                                }
                                if ((((cno != null) && ((cno.Options & 0x4000000) != 0)) && ((pNmqiConnectOpts != null) && (pNmqiConnectOpts.RemoteQmidAsString != null))) && !hconn.Uid.Equals(pNmqiConnectOpts.RemoteQmidAsString))
                                {
                                    NmqiException exception15 = new NmqiException(base.env, -1, null, 2, 0x9f2, null);
                                    base.TrException(method, exception15);
                                    throw exception15;
                                }
                                hconn.EnterCall(this.IsDispatchThread(), false);
                                try
                                {
                                    remoteSession.CheckIfDisconnected();
                                    if (flag)
                                    {
                                        hconn.IsReconnectable = true;
                                        string uid = hconn.Uid;
                                    }
                                    else if (hconn.ParentConnection.SupportsReconnection && ((cno.Options & 0x5000000) > 0))
                                    {
                                        hconn.IsReconnectable = true;
                                        hconn.NmqiConnectionOptions = pNmqiConnectOpts;
                                        string text2 = hconn.Uid;
                                    }
                                    if (remoteSession.SpiQueryOut == null)
                                    {
                                        this.SPIQuerySPI(remoteSession.Hconn, out compCode, out reason, false);
                                    }
                                }
                                finally
                                {
                                    if (hconn != null)
                                    {
                                        if (rTSH != null)
                                        {
                                            try
                                            {
                                                remoteSession.ReleaseReceivedTSH(rTSH);
                                            }
                                            catch (NmqiException exception16)
                                            {
                                                base.TrException(method, exception16);
                                            }
                                        }
                                        hconn.LeaveCall(reason);
                                    }
                                }
                                return;
                            }
                        }
                        finally
                        {
                            if (flag3)
                            {
                                base.TrAPI(method, "__________");
                                if (cno != null)
                                {
                                    base.TrAPI(method, "MQCONNX <<");
                                }
                                else
                                {
                                    base.TrAPI(method, "MQCONN <<");
                                }
                                base.TrAPIInput(method, "Name");
                                base.TrData(method, 0, "Hconn", BitConverter.GetBytes(pHconn.HConn.Value));
                                base.TrAPICCRC(method, compCode, reason);
                            }
                        }
                        if (((cno.Options & 0x100000) == 0) && (nameListFromMQCD != null))
                        {
                            strArray = new string[3];
                            strArray[2] = mqcd.ConnectionName;
                            NmqiException exception17 = new NmqiException(base.env, 0x23f4, strArray, 2, 0x80b, nestedException);
                            nestedException = exception17;
                            mqcd = nameListFromMQCD.SelectChannelEntry(null, threadChlEntry);
                            if (mqcd != null)
                            {
                                flag5 = true;
                                continue;
                            }
                        }
                        return;
                    }
                    catch (NmqiException exception18)
                    {
                        base.TrException(method, exception18, 1);
                        if (remoteSession != null)
                        {
                            remoteSession.Disconnect();
                            remoteSession = null;
                        }
                        strArray = new string[2];
                        strArray[1] = mqcd.ConnectionName;
                        NmqiException exception19 = new NmqiException(base.env, 0x23f4, strArray, exception18.CompCode, exception18.Reason, (nestedException == null) ? exception18 : nestedException);
                        nestedException = exception19;
                        if (nameListFromMQCD != null)
                        {
                            mqcd = nameListFromMQCD.SelectChannelEntry(null, threadChlEntry);
                            if (mqcd != null)
                            {
                                flag5 = true;
                                continue;
                            }
                        }
                        throw exception19;
                    }
                }
            }
            catch (NmqiException exception20)
            {
                base.TrException(method, exception20, 2);
                compCode = exception20.CompCode;
                reason = exception20.Reason;
                throw exception20;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public bool NmqiConvertMessage(Hconn hconn, Hobj hobj, int reqEncoding, int reqCCSID, int appOptions, bool callExitOnLenErr, MQMessageDescriptor md, byte[] buffer, out int dataLength, int availableLength, int bufferLength, out int compCode, out int reason, out int returnedLength)
        {
            uint method = 0x5c6;
            this.TrEntry(method, new object[] { hconn, hobj, reqEncoding, reqCCSID, appOptions, callExitOnLenErr, md, buffer, "dataLength : out", availableLength, bufferLength, "compCode : out", "reason : out", "returnedLength : out" });
            compCode = 2;
            reason = 0x8fa;
            dataLength = 0;
            returnedLength = 0;
            base.TrExit(method, false);
            return false;
        }

        internal void NmqiGet(Hconn hconn, Hobj hobj, MQMessageDescriptor pMsgDesc, MQGetMessageOptions pGetMsgOpts, int BufferLength, byte[] pBuffer, out int pDataLength, MQLPIGetOpts lpigopts, out int pCompCode, out int pReason)
        {
            uint method = 0x5bc;
            this.TrEntry(method, new object[] { hconn, hobj, pMsgDesc, pGetMsgOpts, BufferLength, pBuffer, "pDataLength : out", lpigopts, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            pDataLength = 0;
            try
            {
                ManagedHconn managedHconn = this.GetManagedHconn(hconn);
                MQSession failedSession = this.zstMQGET(managedHconn, hobj, ref pMsgDesc, ref pGetMsgOpts, BufferLength, pBuffer, out pDataLength, lpigopts, out pCompCode, out pReason);
                if (((pReason != 0) && managedHconn.IsReconnectable) && !this.ThreadIsReconnectThread())
                {
                    if (ManagedHconn.IsReconnectableReasonCode(pReason) && !managedHconn.HasFailed())
                    {
                        try
                        {
                            managedHconn.Reconnect(failedSession);
                            this.zstMQGET(managedHconn, hobj, ref pMsgDesc, ref pGetMsgOpts, BufferLength, pBuffer, out pDataLength, lpigopts, out pCompCode, out pReason);
                        }
                        catch (NmqiException)
                        {
                        }
                    }
                    if (managedHconn.HasFailed())
                    {
                        pCompCode = managedHconn.ReconnectionFailureCompCode;
                        pReason = managedHconn.ReconnectionFailureReason;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void NmqiGetMessage(Hconn hconn, Hobj hobj, MQMessageDescriptor md, MQGetMessageOptions gmo, int bufferLength, byte[] buffer, out int dataLength, out int compCode, out int reason)
        {
            uint method = 0x5c8;
            this.TrEntry(method, new object[] { hconn, hobj, md, gmo, bufferLength, buffer, "dataLength : out", "compCode : out", "reason : out" });
            compCode = 2;
            reason = 0x8fa;
            dataLength = 0;
            base.TrExit(method);
        }

        internal MQSession nmqiOpen(Hconn hConn, ref MQObjectDescriptor mqod, ref SpiOpenOptions OpenOptions, int options, ref Phobj manHobj, out int compCode, out int reason, bool spiCall, ManagedHobj reconnectHobjP)
        {
            uint method = 0x5b4;
            this.TrEntry(method, new object[] { hConn, mqod, OpenOptions, options, manHobj, "compCode : out", "reason : out", spiCall, reconnectHobjP });
            int flags = 0;
            int verbId = 12;
            int num11 = 0x10;
            int num12 = 0;
            int defaultReadAhead = 0;
            int defaultPersistence = 0;
            int defaultPutReponseType = 0;
            int defaultPropertyControl = 0;
            MQOPEN_PRIV mqopen_priv = new MQOPEN_PRIV();
            bool flag = CommonServices.TraceStatus();
            MQProxyQueue proxyQueue = null;
            int num1 = hConn.Value;
            int handle = -1;
            MQSPI mqspi = new MQSPI();
            MQSPIOpenInOut @out = null;
            int maxInOutVersion = 0;
            MQSPIOpenIn @in = null;
            int maxInVersion = 0;
            MQSPIOpenOut out2 = null;
            int maxOutVersion = 0;
            bool flag2 = reconnectHobjP != null;
            ManagedHobj hobj = reconnectHobjP;
            ManagedHconn managedHconn = null;
            MQSession session = null;
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQAPI mqapi = new MQAPI();
            if (IntPtr.Size != 4)
            {
                mqod.UseNativePtrSz = false;
            }
            int requiredBufferSize = mqod.GetRequiredBufferSize();
            int translength = (num11 + requiredBufferSize) + 4;
            try
            {
                int num5;
                int num7;
                int num8;
                this.InExit();
                compCode = 2;
                reason = 0x7e2;
                int offset = num7 = num8 = 0;
                int num4 = num5 = 0;
                string str = (mqod.ObjectString.VSString != null) ? mqod.ObjectString.VSString : "";
                string objectName = mqod.ObjectName;
                MQObjectDescriptor objectDescriptor = new MQObjectDescriptor();
                objectDescriptor = mqod;
                SpiOpenOptions openOpts = new SpiOpenOptions();
                openOpts = OpenOptions;
                if (((options & 0x100000) != 0) && ((options & 0x80000) != 0))
                {
                    compCode = 2;
                    reason = 0x7fe;
                    NmqiException ex = new NmqiException(base.env, -1, null, compCode, reason, null);
                    base.TrException(method, ex);
                }
                managedHconn = this.GetManagedHconn(hConn);
                session = managedHconn.Session;
                if (managedHconn.IsQuiescing() && ((OpenOptions.Options & 0x2000) != 0))
                {
                    base.TrText(method, "This hconn is quiescing, notify the caller about it");
                    base.throwNewMQException(2, 0x89a);
                }
                if (session.FapLevel >= 9)
                {
                    translength += mqopen_priv.GetLength();
                }
                num12 = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                translength += num12;
                objectName = mqod.ObjectName;
                managedHconn.EnterCall(this.IsDispatchThread(), false, false);
                if (spiCall)
                {
                    options = OpenOptions.Options;
                }
                try
                {
                    session.CheckIfDisconnected();
                    if (((session.FapLevel >= 9) && session.IsMultiplexingEnabled) && ((((options & 1) != 0) || ((options & 2) != 0)) || (((options & 4) != 0) || ((options & 8) != 0))))
                    {
                        if (flag2)
                        {
                            if (hobj.ProxyQueue != null)
                            {
                                proxyQueue = hobj.ProxyQueue;
                                proxyQueue.ResetForReconnect();
                            }
                        }
                        else
                        {
                            proxyQueue = managedHconn.ProxyQueueManager.CreateProxyQueue();
                        }
                    }
                    if (spiCall)
                    {
                        if (!session.SpiQueryOut.SPISupported(verbId, ref maxInOutVersion, ref maxInVersion, ref maxOutVersion, ref flags))
                        {
                            compCode = 2;
                            reason = 0x8fa;
                            return session;
                        }
                        @out = new MQSPIOpenInOut(base.env, maxInOutVersion);
                        @in = new MQSPIOpenIn(base.env, maxInVersion);
                        out2 = new MQSPIOpenOut(base.env, maxOutVersion);
                        offset = num12;
                        mqspi.VerbId = verbId;
                        mqspi.OutStructVersion = out2.Version;
                        mqspi.OutStructLength = out2.GetLength();
                        @in.OpenOptions = OpenOptions;
                        @in.MQOpenDescriptor = mqod;
                        if (IntPtr.Size != 4)
                        {
                            out2.UseNativePtrSz = false;
                        }
                        int num21 = (((num12 + 0x10) + mqspi.GetLength()) + @out.GetVersionLength()) + Math.Max(mqspi.OutStructLength, @in.GetRequiredBufferSize());
                        tsh = session.AllocateTSH(140, 0, true, num21);
                        mqapi.Initialize(num21, handle);
                        byte[] tshBuffer = tsh.TshBuffer;
                        offset = tsh.WriteStruct(tshBuffer, 0);
                        offset += mqapi.WriteStruct(tshBuffer, offset);
                        offset += mqspi.WriteStruct(tshBuffer, offset);
                        offset += @out.WriteStruct(tshBuffer, offset);
                        offset += @in.WriteStruct(tshBuffer, offset);
                        tsh.TshBuffer = tshBuffer;
                        offset = 0;
                    }
                    else
                    {
                        tsh = session.AllocateTSH(0x83, 0, true, translength);
                        byte[] b = tsh.TshBuffer;
                        offset = tsh.WriteStruct(b, 0);
                        mqapi.Initialize(translength, 0);
                        offset += mqapi.WriteStruct(b, offset);
                        num7 = offset;
                        num8 = offset += mqod.WriteStruct(b, offset);
                        BitConverter.GetBytes(options).CopyTo(b, offset);
                        offset += 4;
                        if (session.FapLevel >= 9)
                        {
                            num4 = offset;
                            num5 = offset += mqopen_priv.WriteStruct(b, offset);
                        }
                        tsh.TshBuffer = b;
                    }
                    if (flag)
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "MQOPEN >>");
                        base.TrData(method, 0, "Hconn", BitConverter.GetBytes(hConn.Value));
                        base.TrData(method, 0, "Objdesc", num7, num8 - num7, tsh.TshBuffer);
                        base.TrData(method, 0, "Options", BitConverter.GetBytes(options));
                        if (session.FapLevel >= 9)
                        {
                            base.TrData(method, 0, "MQOPEN Private", num4, num5 - num4, tsh.TshBuffer);
                            num4 = num5 = 0;
                        }
                        base.TrAPIOutput(method, "Hobj");
                        base.TrAPIOutput(method, "CompCode");
                        base.TrAPIOutput(method, "Reason");
                        num7 = num8 = 0;
                    }
                    offset = 0;
                    session.SendTSH(tsh);
                    rTSH = session.ReceiveTSH(null);
                    switch (rTSH.SegmentType)
                    {
                        case 5:
                        {
                            MQERD mqerd = new MQERD();
                            if (rTSH.Length > num12)
                            {
                                mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            }
                            throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());
                        }
                        case 0x93:
                        case 0x9c:
                            offset = rTSH.Offset;
                            num7 = offset = mqapi.ReadStruct(rTSH.TshBuffer, offset);
                            compCode = mqapi.mqapi.CompCode;
                            reason = mqapi.mqapi.Reason;
                            handle = mqapi.mqapi.Handle;
                            base.TrText(method, string.Concat(new object[] { "MQOPEN has completed with CompCode = ", (int) compCode, ", Reason= ", (int) reason, ", Hobj= ", handle }));
                            if (compCode == 0)
                            {
                                if (spiCall)
                                {
                                    offset = mqspi.ReadStruct(rTSH.TshBuffer, offset);
                                    offset = @out.ReadStruct(rTSH.TshBuffer, offset);
                                    num8 = out2.ReadStruct(rTSH.TshBuffer, offset);
                                    mqod = out2.MQOpenDescriptor;
                                    OpenOptions.DefPersistence = defaultPersistence = out2.OpenOptions.DefPersistence;
                                    OpenOptions.DefPutResponseType = defaultPutReponseType = out2.OpenOptions.DefPutResponseType;
                                    OpenOptions.DefReadAhead = defaultReadAhead = out2.OpenOptions.DefReadAhead;
                                    OpenOptions.PropertyControl = defaultPropertyControl = out2.OpenOptions.PropertyControl;
                                }
                                else
                                {
                                    num8 = offset = mqod.ReadStruct(rTSH.TshBuffer, offset, rTSH.Length);
                                    offset += 4;
                                    if (session.FapLevel >= 9)
                                    {
                                        num4 = offset;
                                        num5 = offset = mqopen_priv.ReadStruct(rTSH.TshBuffer, offset);
                                        defaultPersistence = mqopen_priv.DefPersistence;
                                        defaultPutReponseType = mqopen_priv.DefPutResponseType;
                                        defaultReadAhead = mqopen_priv.DefReadAhead;
                                        defaultPropertyControl = mqopen_priv.PropertyControl;
                                    }
                                }
                                if (flag2)
                                {
                                    hobj.Handle = handle;
                                    hobj.Mqod = objectDescriptor;
                                    hobj.SpiOpenOpts = openOpts;
                                }
                                else
                                {
                                    mqod.ObjectString.VSString = str;
                                    hobj = new ManagedHobj(base.env, handle, proxyQueue, mqod.ObjectName, mqod.ObjectType, options, defaultPersistence, defaultPutReponseType, defaultReadAhead, defaultPropertyControl, objectDescriptor, openOpts, spiCall);
                                }
                                manHobj.HOBJ = hobj;
                                if (managedHconn.IsReconnectable && !flag2)
                                {
                                    hobj.Reconnectable = true;
                                    hobj.OriginalObjectName = objectName;
                                    managedHconn.AddHobj(hobj);
                                }
                                if (proxyQueue != null)
                                {
                                    managedHconn.ProxyQueueManager.SetIdentifier(proxyQueue, hobj, options, defaultReadAhead);
                                    proxyQueue.ManHconn = managedHconn;
                                }
                            }
                            return session;
                    }
                    string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = rTSH.SegmentType;
                    CommonServices.CommentInsert1 = channelName;
                    CommonServices.CommentInsert2 = "Unexpected response received during MQOPEN";
                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009509, 0);
                    throw MQERD.ErrorException(0x20009509, rTSH.SegmentType, channelName);
                }
                finally
                {
                    if (flag)
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "MQOPEN <<");
                        base.TrAPIInput(method, "Hconn");
                        if (num7 != num8)
                        {
                            base.TrData(method, 0, "Objdesc", num7, num8 - num7, rTSH.TshBuffer);
                        }
                        if ((session.FapLevel >= 9) && (num4 != num5))
                        {
                            base.TrData(method, 0, "MQOPEN Private", num4, num5 - num4, rTSH.TshBuffer);
                        }
                        base.TrAPIInput(method, "Options");
                        base.TrData(method, 0, "Hobj", BitConverter.GetBytes(handle));
                    }
                    try
                    {
                        if (managedHconn != null)
                        {
                            if (rTSH != null)
                            {
                                try
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                                catch (NmqiException exception2)
                                {
                                    base.TrException(method, exception2, 1);
                                    compCode = exception2.CompCode;
                                    reason = exception2.Reason;
                                }
                            }
                            managedHconn.LeaveCall(reason, false);
                            if ((compCode == 2) && (proxyQueue != null))
                            {
                                managedHconn.ProxyQueueManager.DeleteProxyQueue(proxyQueue);
                                if (hobj != null)
                                {
                                    hobj.ProxyQueue = null;
                                }
                            }
                        }
                    }
                    catch (NmqiException exception3)
                    {
                        base.TrException(method, exception3, 2);
                        compCode = exception3.CompCode;
                        reason = exception3.Reason;
                    }
                    mqod.UseNativePtrSz = true;
                }
                return session;
            }
            catch (NmqiException exception4)
            {
                base.TrException(method, exception4, 3);
                compCode = exception4.CompCode;
                reason = exception4.Reason;
            }
            catch (MQException exception5)
            {
                base.TrException(method, exception5, 4);
                compCode = exception5.CompCode;
                reason = exception5.Reason;
            }
            catch (Exception exception6)
            {
                base.TrException(method, exception6, 5);
                compCode = 2;
                reason = 0x893;
            }
            finally
            {
                base.TrExit(method);
            }
            return session;
        }

        public void NmqiPut(Hconn hconn, Hobj hobj, MQMessageDescriptor msgDesc, MQPutMessageOptions putMsgOpts, MemoryStream[] buffers, out int compCode, out int reason)
        {
            uint method = 0x5b7;
            this.TrEntry(method, new object[] { hconn, hobj, msgDesc, putMsgOpts, buffers, "compCode : out", "reason : out" });
            int numBuffs = (buffers == null) ? 0 : buffers.Length;
            try
            {
                MQSession failedSession = this.MQPUT(hconn, hobj, ref msgDesc, ref putMsgOpts, 0, null, buffers, numBuffs, out compCode, out reason);
                ManagedHconn managedHconn = this.GetManagedHconn(hconn);
                if (((reason != 0) && managedHconn.IsReconnectable) && !this.ThreadIsReconnectThread())
                {
                    if (ManagedHconn.IsReconnectableReasonCode(reason) && !managedHconn.HasFailed())
                    {
                        try
                        {
                            managedHconn.Reconnect(failedSession);
                            this.MQPUT(hconn, hobj, ref msgDesc, ref putMsgOpts, 0, null, buffers, numBuffs, out compCode, out reason);
                        }
                        catch (NmqiException)
                        {
                        }
                    }
                    if (managedHconn.HasFailed())
                    {
                        compCode = managedHconn.ReconnectionFailureCompCode;
                        reason = managedHconn.ReconnectionFailureReason;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void NmqiPut1(Hconn hconn, MQObjectDescriptor objDesc, MQMessageDescriptor msgDesc, MQPutMessageOptions putMsgOpts, MemoryStream[] buffers, out int compCode, out int reason)
        {
            uint method = 0x5b9;
            this.TrEntry(method, new object[] { hconn, objDesc, msgDesc, putMsgOpts, buffers, "compCode : out", "reason : out" });
            int numBuffs = (buffers == null) ? 0 : buffers.Length;
            compCode = 0;
            reason = 0;
            try
            {
                MQSession failedSession = this.MQPUT1(hconn, ref objDesc, ref msgDesc, ref putMsgOpts, 0, null, buffers, numBuffs, out compCode, out reason);
                ManagedHconn managedHconn = this.GetManagedHconn(hconn);
                if (((reason != 0) && managedHconn.IsReconnectable) && !this.ThreadIsReconnectThread())
                {
                    if (ManagedHconn.IsReconnectableReasonCode(reason) && !managedHconn.HasFailed())
                    {
                        try
                        {
                            managedHconn.Reconnect(failedSession);
                            this.MQPUT1(hconn, ref objDesc, ref msgDesc, ref putMsgOpts, 0, null, buffers, numBuffs, out compCode, out reason);
                        }
                        catch (NmqiException)
                        {
                        }
                    }
                    if (managedHconn.HasFailed())
                    {
                        compCode = managedHconn.ReconnectionFailureCompCode;
                        reason = managedHconn.ReconnectionFailureReason;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal MQSession nmqiSubscribe(Hconn hconn, MQSubscriptionDescriptor mqsd, ref Phobj pHobj, ref Phobj pHsub, out int compCode, out int reason, LpiSD spiSD, bool spiCall, ManagedHsub rcnHsub)
        {
            int num7;
            uint method = 0x3bf;
            this.TrEntry(method, new object[] { hconn, mqsd, pHobj, pHsub, "compCode : out", "reason : out", spiSD, spiCall, rcnHsub });
            int offset = 0;
            int length = 0;
            int defaultReadAhead = 0;
            int maxInOutVersion = 0;
            int maxInVersion = 0;
            int verbId = 7;
            int vSBufSize = 0;
            int maxOutVersion = 0;
            int flags = 0;
            int options = mqsd.Options;
            int num16 = 0;
            int num17 = 0x10;
            int translength = 0;
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQSPI mqspi = null;
            MQAPI mqapi = new MQAPI();
            MQProxyQueue proxyQueue = null;
            MQSPISubscribeInOut @out = null;
            MQSPISubscribeIn @in = null;
            MQSPISubscribeOut out2 = null;
            ManagedHsub hsub = rcnHsub;
            ManagedHobj hobj = (rcnHsub != null) ? rcnHsub.Hobj : null;
            MQSession session = null;
            bool flag = CommonServices.TraceStatus();
            bool flag2 = rcnHsub != null;
            pHsub.HOBJ.Handle = -1;
            compCode = 2;
            reason = 0x7e2;
            int num6 = num7 = 0;
            this.InExit();
            try
            {
                if (mqsd == null)
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x978, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                if (mqsd.Options == 0)
                {
                    NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                    base.TrException(method, exception2);
                    throw exception2;
                }
                if (pHobj == null)
                {
                    NmqiException exception3 = new NmqiException(base.env, -1, null, 2, 0x7e3, null);
                    base.TrException(method, exception3);
                    throw exception3;
                }
                if (pHsub == null)
                {
                    NmqiException exception4 = new NmqiException(base.env, -1, null, 2, 0x7e3, null);
                    base.TrException(method, exception4);
                    throw exception4;
                }
                MQSubscriptionDescriptor descriptor = mqsd;
                LpiSD isd = spiSD;
                if (IntPtr.Size != 4)
                {
                    mqsd.UseNativePtrSz = false;
                    if (spiSD != null)
                    {
                        spiSD.UseNativePtrSz = false;
                    }
                }
                int requiredBufferSize = mqsd.GetRequiredBufferSize();
                ManagedHconn managedHconn = this.GetManagedHconn(hconn);
                session = managedHconn.Session;
                num16 = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                byte[] b = null;
                if (managedHconn.FapLevel < 9)
                {
                    NmqiException exception5 = new NmqiException(base.env, -1, null, 2, 0x8fa, null);
                    base.TrException(method, exception5);
                    throw exception5;
                }
                int num19 = managedHconn.Value;
                int handle = pHobj.HOBJ.Handle;
                if ((managedHconn.IsReconnectable && ((mqsd.Options & 8) != 0)) && ((managedHconn.ConnectionOptions.Options & 0x4000000) == 0))
                {
                    compCode = 2;
                    reason = 0x9fb;
                    return session;
                }
                bool flag3 = (mqsd.Options & 0x20) != 0;
                if (flag3)
                {
                    if (session.IsMultiplexingEnabled)
                    {
                        if (flag2 && (hsub.Hobj != null))
                        {
                            proxyQueue = hsub.Hobj.ProxyQueue;
                            proxyQueue.ResetForReconnect();
                        }
                        else
                        {
                            proxyQueue = managedHconn.ProxyQueueManager.CreateProxyQueue();
                        }
                    }
                    pHobj.HOBJ.Handle = 0;
                }
                else if (!spiCall && !(pHobj.HOBJ is ManagedHobj))
                {
                    NmqiException exception6 = new NmqiException(base.env, -1, null, 2, 0x7e3, null);
                    base.TrException(method, exception6);
                    throw exception6;
                }
                if (mqsd.ResObjectString.VSBufSize > 0)
                {
                    vSBufSize = mqsd.ResObjectString.VSBufSize;
                }
                managedHconn.EnterCall(this.IsDispatchThread(), false);
                try
                {
                    int num3;
                    session.CheckIfDisconnected();
                    if (spiCall)
                    {
                        if (!session.SpiQueryOut.SPISupported(verbId, ref maxInOutVersion, ref maxInVersion, ref maxOutVersion, ref flags))
                        {
                            compCode = 2;
                            reason = 0x8fa;
                            return session;
                        }
                        mqspi = new MQSPI();
                        @out = new MQSPISubscribeInOut(base.env, maxInOutVersion);
                        @in = new MQSPISubscribeIn(base.env, maxInVersion);
                        out2 = new MQSPISubscribeOut(base.env, maxOutVersion);
                        offset = num16;
                        mqspi.VerbId = verbId;
                        mqspi.OutStructVersion = out2.Version;
                        mqspi.OutStructLength = out2.GetLength();
                        @in.Lpisd = spiSD;
                        @in.SubDesc = mqsd;
                        out2.Lpisd = spiSD;
                        out2.SubDesc = mqsd;
                        num3 = ((((num16 + 0x10) + mqspi.GetLength()) + @out.GetVersionLength()) + Math.Max(mqspi.OutStructLength, @in.GetRequiredBufferSize())) + mqsd.ResObjectString.VSBufSize;
                    }
                    else
                    {
                        num3 = ((num16 + num17) + requiredBufferSize) + 8;
                    }
                    mqapi.Initialize(num3, handle);
                    translength = num3 - vSBufSize;
                    if (spiCall)
                    {
                        tsh = session.AllocateTSH(140, 0, true, translength);
                        b = tsh.TshBuffer;
                        offset = tsh.WriteStruct(b, 0);
                        offset += mqapi.WriteStruct(b, offset);
                        offset += mqspi.WriteStruct(b, offset);
                        offset += @out.WriteStruct(b, offset);
                        offset += @in.WriteStruct(b, offset);
                        num3 = b.Length;
                    }
                    else
                    {
                        if (mqsd.ResObjectString.VSBufSize > 0)
                        {
                            vSBufSize = mqsd.ResObjectString.VSBufSize;
                        }
                        tsh = session.AllocateTSH(0x8e, 0, true, num3);
                        b = tsh.TshBuffer;
                        offset = tsh.WriteStruct(b, 0);
                        offset += mqapi.WriteStruct(b, offset);
                        BitConverter.GetBytes(pHsub.HOBJ.Handle).CopyTo(b, offset);
                        offset += 4;
                        BitConverter.GetBytes(0).CopyTo(b, offset);
                        offset += 4;
                        num6 = offset;
                        num7 = offset += mqsd.WriteStruct(b, offset);
                    }
                    if (flag)
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "MQSUB  >>");
                        base.TrData(method, 0, "Hconn", BitConverter.GetBytes(num19));
                        base.TrData(method, 0, "Objdesc", num6, num7 - num6, b);
                        base.TrAPIOutput(method, "Hobj");
                        base.TrAPIOutput(method, "CompCode");
                        base.TrAPIOutput(method, "Reason");
                        num6 = num7 = 0;
                    }
                    offset = 0;
                    session.SendTSH(tsh);
                    b = null;
                    rTSH = session.ReceiveTSH(null);
                    b = rTSH.TshBuffer;
                    offset = rTSH.Offset;
                    length = rTSH.Length;
                    switch (rTSH.SegmentType)
                    {
                        case 0x9c:
                        case 0x9e:
                            num6 = offset = mqapi.ReadStruct(b, offset);
                            compCode = mqapi.mqapi.CompCode;
                            reason = mqapi.mqapi.Reason;
                            handle = mqapi.mqapi.Handle;
                            pHobj.HOBJ.Handle = handle;
                            if (compCode == 0)
                            {
                                break;
                            }
                            return session;

                        case 5:
                        {
                            MQERD mqerd = new MQERD();
                            if (length > num16)
                            {
                                mqerd.ReadStruct(b, offset);
                            }
                            throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());
                        }
                        default:
                        {
                            string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                            CommonServices.SetValidInserts();
                            CommonServices.ArithInsert1 = rTSH.SegmentType;
                            CommonServices.CommentInsert1 = channelName;
                            base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                            throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                        }
                    }
                    int num21 = 0;
                    if (spiCall)
                    {
                        offset = mqspi.ReadStruct(b, offset);
                        offset = @out.ReadStruct(b, offset);
                        num7 = out2.ReadStruct(b, offset);
                        num21 = out2.Hsub;
                        defaultReadAhead = out2.LpiDestReadAhead;
                    }
                    else
                    {
                        num21 = BitConverter.ToInt32(b, offset);
                        offset += 4;
                        defaultReadAhead = BitConverter.ToInt32(b, offset);
                        offset += 4;
                        num7 = offset = mqsd.ReadStruct(b, offset, length);
                        offset += 4;
                    }
                    ManagedHobj hobj2 = null;
                    if (flag3)
                    {
                        hobj2 = new ManagedHobj(base.env, mqapi.mqapi.Handle, proxyQueue, descriptor.ObjectName, 8, descriptor.Options, 0, 2, defaultReadAhead, 0, null);
                        pHobj.HOBJ = hobj2;
                        if (proxyQueue != null)
                        {
                            int opOpts = 10;
                            if ((mqsd.Options & 0x10000000) != 0)
                            {
                                opOpts |= 0x100000;
                            }
                            else if ((mqsd.Options & 0x8000000) != 0)
                            {
                                opOpts |= 0x80000;
                            }
                            hobj2.OpenOptions = opOpts;
                            managedHconn.ProxyQueueManager.SetIdentifier(proxyQueue, hobj2, opOpts, defaultReadAhead);
                            proxyQueue.ManHconn = managedHconn;
                        }
                        else
                        {
                            hobj2.OpenOptions = 0x8000a;
                        }
                    }
                    else if (!(pHobj.HOBJ is ManagedHobj))
                    {
                        hobj2 = new ManagedHobj(base.env, mqapi.mqapi.Handle, proxyQueue, mqsd.ObjectName, 8, mqsd.Options, 0, 2, defaultReadAhead, 0, null);
                        pHobj.HOBJ = hobj2;
                    }
                    if (flag2)
                    {
                        hsub.Handle = num21;
                        hsub.Hobj = hobj2;
                        if (hobj != null)
                        {
                            hobj.Handle = hobj2.Handle;
                        }
                        descriptor.Options = options;
                        hsub.Mqsd = descriptor;
                    }
                    else
                    {
                        mqsd.Options = options;
                        hsub = new ManagedHsub(base.env, num21, hobj2, mqsd, isd, spiCall);
                    }
                    if (hobj2 != null)
                    {
                        hobj2.ParentHsub = hsub;
                    }
                    pHsub.HOBJ = hsub;
                    if (managedHconn.IsReconnectable && !flag2)
                    {
                        managedHconn.AddHsub(hsub);
                    }
                }
                finally
                {
                    if (flag)
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "MQSUB  <<");
                        base.TrAPIInput(method, "Hconn");
                        if (num6 != num7)
                        {
                            base.TrData(method, 0, "Objdesc", num6, num7 - num6, rTSH.TshBuffer);
                        }
                        base.TrData(method, 0, "Hsub", BitConverter.GetBytes(hsub.Handle));
                        base.TrData(method, 0, "MQSUB ReadAhead", BitConverter.GetBytes(defaultReadAhead));
                        base.TrData(method, 0, "Hobj", BitConverter.GetBytes(pHobj.HOBJ.Handle));
                    }
                    try
                    {
                        if (managedHconn != null)
                        {
                            if (rTSH != null)
                            {
                                try
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                                catch (NmqiException exception7)
                                {
                                    base.TrException(method, exception7, 1);
                                }
                            }
                            managedHconn.LeaveCall(reason);
                        }
                    }
                    catch (NmqiException)
                    {
                    }
                }
                return session;
            }
            catch (NmqiException exception8)
            {
                base.TrException(method, exception8, 2);
                compCode = exception8.CompCode;
                reason = exception8.Reason;
            }
            catch (MQException exception9)
            {
                base.TrException(method, exception9, 3);
                compCode = exception9.CompCode;
                reason = exception9.Reason;
            }
            catch (Exception exception10)
            {
                base.TrException(method, exception10, 4);
                compCode = 2;
                reason = 0x893;
            }
            finally
            {
                mqsd.UseNativePtrSz = true;
                if (spiSD != null)
                {
                    spiSD.UseNativePtrSz = true;
                }
                base.TrExit(method);
            }
            return session;
        }

        internal void PerformMsgProcessgingBeforePut(ref MQMessage mqMsg)
        {
            uint method = 0x64a;
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
            uint method = 0x64b;
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
            uint method = 0x64c;
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

        private bool QuerySyncDelivery(Hconn hconn, Hobj hobj, ref MQPutMessageOptions putMessageOpts, ref MQMessageDescriptor messageDescriptor, out int compCode, out int reason)
        {
            bool flag;
            uint method = 0x92;
            this.TrEntry(method, new object[] { hconn, hobj, putMessageOpts, messageDescriptor, "compCode : out", "reason : out" });
            try
            {
                int num6;
                compCode = 2;
                reason = 0x893;
                int num1 = hconn.Value;
                ManagedHconn hconn2 = (ManagedHconn) hconn;
                if (hobj == null)
                {
                    hobj = new ManagedHobj(base.env, -1);
                }
                ManagedHobj hobj2 = (ManagedHobj) hobj;
                int handle = hobj2.Handle;
                bool flag2 = CommonServices.TraceStatus();
                int options = putMessageOpts.Options;
                int num4 = options & 0x30000;
                int num5 = options & 0xc0;
                int num7 = num5;
                if (((num4 & 0x20000) != 0) || (((num4 == 0) && (handle != -1)) && (hobj2.DefPutResponseType == 1)))
                {
                    num6 = 0x20000;
                }
                else if (((num4 == 0) && base.env.Cfg.GetBoolValue(MQClientCfg.ENV_MQPUT1DEFSYNC)) && (handle == -1))
                {
                    num6 = 0x20000;
                }
                else if (((num4 == 0) && (handle == -1)) && ((options & 2) == 0))
                {
                    num6 = 0x20000;
                }
                else if ((options & 0x8000) != 0)
                {
                    num6 = 0x20000;
                }
                else if (((messageDescriptor.Persistence == 1) || ((messageDescriptor.Persistence == 2) && ((handle == -1) || (hobj2.DefPersistence == 1)))) && ((options & 2) == 0))
                {
                    num6 = 0x20000;
                }
                else
                {
                    num6 = 0x10000;
                    if ((options & 0x800) == 0)
                    {
                        messageDescriptor.PutApplType = 9;
                        messageDescriptor.PutApplName = Encoding.ASCII.GetBytes(this.ToString());
                        int requestedNumber = 0;
                        bool flag4 = false;
                        if (((num5 & 0x40) != 0) || object.Equals(MQC.MQMI_NONE, messageDescriptor.MsgId))
                        {
                            num7 &= -65;
                            flag4 = true;
                            requestedNumber++;
                            if (flag2)
                            {
                                base.TrText(method, "Requested for new Message ID");
                            }
                        }
                        bool flag5 = false;
                        if ((num5 & 0x80) != 0)
                        {
                            num7 &= -129;
                            flag5 = true;
                            requestedNumber++;
                            if (flag2)
                            {
                                base.TrText(method, "Requested for new Correlation ID");
                            }
                        }
                        if (requestedNumber > 0)
                        {
                            int num9 = 0;
                            byte[][] tags = hconn2.IdTagPool.GetTags(requestedNumber, hconn2.Session);
                            if (flag4)
                            {
                                messageDescriptor.MsgId = tags[num9++];
                            }
                            if (flag5)
                            {
                                messageDescriptor.CorrelId = tags[num9++];
                            }
                        }
                    }
                }
                putMessageOpts.Options = (((options - num4) + num6) - num5) + num7;
                flag = num6 == 0x20000;
                if (flag2)
                {
                    base.TrText(method, "Chosen Synchronous Delivery= " + flag.ToString());
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return flag;
        }

        internal int ReadIntArray(byte[] bytes, int offset, int[] array, int arrayCount)
        {
            uint method = 0x8e;
            this.TrEntry(method, new object[] { bytes, offset, array, arrayCount });
            for (int i = 0; i < arrayCount; i++)
            {
                array[i] = BitConverter.ToInt32(bytes, offset);
                offset += 4;
            }
            base.TrExit(method, offset);
            return offset;
        }

        private MQChannelDefinition ReadMQServer()
        {
            uint method = 0x59f;
            this.TrEntry(method);
            string stringValue = null;
            MQChannelDefinition definition = null;
            try
            {
                stringValue = base.env.Cfg.GetStringValue(MQClientCfg.ENV_MQSERVER);
                if (stringValue != null)
                {
                    base.TrText("MQSERVER = " + stringValue);
                }
                else
                {
                    base.TrText("MQSERVER = null");
                }
                if (stringValue != null)
                {
                    int index = stringValue.IndexOf('/', 0);
                    if (index == -1)
                    {
                        throw new Exception();
                    }
                    string str2 = stringValue.Substring(0, index);
                    if (str2.Length > 20)
                    {
                        throw new Exception();
                    }
                    int startIndex = index + 1;
                    index = stringValue.IndexOf('/', startIndex);
                    if (index == -1)
                    {
                        throw new Exception();
                    }
                    if (!stringValue.Substring(startIndex, index - startIndex).ToLower().Equals("tcp"))
                    {
                        throw new Exception();
                    }
                    startIndex = index + 1;
                    string str3 = stringValue.Substring(startIndex);
                    if ((str2.Length == 0) || (str3.Length == 0))
                    {
                        throw new Exception();
                    }
                    definition = new MQChannelDefinition();
                    definition.ChannelName = str2;
                    definition.ConnectionName = str3;
                    definition.MaxMessageLength = 0x6400000;
                }
                return definition;
            }
            catch (MQException exception)
            {
                NmqiException ex = new NmqiException(base.env, -1, null, exception.CompCode, exception.Reason, exception);
                base.TrException(method, ex);
                throw ex;
            }
            catch (Exception exception3)
            {
                NmqiException exception4 = new NmqiException(base.env, -1, null, 2, 0x8e6, exception3);
                base.TrException(method, exception4);
                throw exception4;
            }
            finally
            {
                base.TrExit(method);
            }
            return definition;
        }

        public void SPIActivateMessage(Hconn hConn, ref MQSPIActivateOpts spiao, out int compCode, out int reason)
        {
            uint method = 160;
            this.TrEntry(method, new object[] { hConn, spiao, "compCode : out", "reason : out" });
            int offset = 0;
            int num3 = 0x10;
            int maxInVersion = 0;
            int maxInOutVersion = 0;
            int maxOutVersion = 0;
            int flags = 0;
            int verbId = 4;
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQAPI mqapi = new MQAPI();
            MQERD mqerd = null;
            MQSPI mqspi = null;
            MQSPIActivateInOut @out = null;
            MQSPIActivateIn @in = null;
            MQSPIActivateOut out2 = null;
            ManagedHconn managedHconn = null;
            compCode = 2;
            reason = 0x7e2;
            this.InExit();
            try
            {
                managedHconn = this.GetManagedHconn(hConn);
                MQSession session = managedHconn.Session;
                offset = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                managedHconn.EnterCall(this.IsDispatchThread(), false);
                try
                {
                    session.CheckIfDisconnected();
                    if (spiao == null)
                    {
                        NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x893, null);
                        base.TrException(method, ex);
                        throw ex;
                    }
                    if (!session.SpiQueryOut.SPISupported(verbId, ref maxInOutVersion, ref maxInVersion, ref maxOutVersion, ref flags))
                    {
                        compCode = 2;
                        reason = 0x8fa;
                    }
                    else
                    {
                        mqspi = new MQSPI();
                        @out = new MQSPIActivateInOut(maxInOutVersion);
                        @in = new MQSPIActivateIn(maxInVersion);
                        out2 = new MQSPIActivateOut(maxOutVersion);
                        int num8 = offset;
                        mqspi.VerbId = 4;
                        mqspi.OutStructVersion = out2.Version;
                        mqspi.OutStructLength = out2.GetVersionLength();
                        byte[] msgId = spiao.MsgId;
                        if ((msgId == null) || (msgId.Length != 0x18))
                        {
                            NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x893, null);
                            base.TrException(method, exception2);
                            throw exception2;
                        }
                        if (spiao != null)
                        {
                            @in.Options = spiao.Options;
                            @in.QName = spiao.QName;
                            @in.QMgrName = spiao.QMgrName;
                            @in.MsgId = msgId;
                        }
                        int translength = (((offset + num3) + mqspi.GetLength()) + @out.GetVersionLength()) + Math.Max(mqspi.OutStructLength, @in.GetVersionLength());
                        tsh = session.AllocateTSH(140, 0, true, translength);
                        byte[] tshBuffer = tsh.TshBuffer;
                        mqapi.Initialize(translength, hConn.Value);
                        num8 += mqapi.WriteStruct(tshBuffer, offset);
                        num8 += mqspi.WriteStruct(tshBuffer, num8);
                        num8 += @out.WriteStruct(tshBuffer, num8);
                        num8 += @in.WriteStruct(tshBuffer, num8);
                        tsh.WriteStruct(tshBuffer, 0);
                        tsh.TshBuffer = tshBuffer;
                        session.SendTSH(tsh);
                        rTSH = session.ReceiveTSH(null);
                        switch (rTSH.SegmentType)
                        {
                            case 5:
                                mqerd = new MQERD();
                                if (rTSH.Length > offset)
                                {
                                    mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                }
                                throw mqerd.ErrorException(session.ParentConnection.ChannelName.Trim());

                            case 0x9c:
                                break;

                            default:
                            {
                                string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                                CommonServices.SetValidInserts();
                                CommonServices.ArithInsert1 = rTSH.SegmentType;
                                CommonServices.CommentInsert1 = channelName;
                                CommonServices.CommentInsert2 = "Unexpected flow received during SPIActivateMessage";
                                base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                                throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                            }
                        }
                        rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                        compCode = mqapi.mqapi.CompCode;
                        reason = mqapi.mqapi.Reason;
                        if (compCode != 2)
                        {
                            rTSH.Offset = mqspi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                        }
                    }
                }
                finally
                {
                    try
                    {
                        if (managedHconn != null)
                        {
                            if (rTSH != null)
                            {
                                try
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                                catch (NmqiException)
                                {
                                }
                            }
                            managedHconn.LeaveCall(reason);
                        }
                    }
                    catch (NmqiException)
                    {
                    }
                }
            }
            catch (NmqiException exception3)
            {
                base.TrException(method, exception3, 1);
                compCode = exception3.CompCode;
                reason = exception3.Reason;
            }
            catch (Exception exception4)
            {
                base.TrException(method, exception4);
                compCode = 2;
                reason = 0x893;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SpiConnect(string pQMgrName, SpiConnectOptions pSpiConnectOpts, MQConnectOptions pConnectOpts, Phconn pHconn, out int pCompCode, out int pReason)
        {
            uint method = 0x5ad;
            this.TrEntry(method, new object[] { pQMgrName, pSpiConnectOpts, pConnectOpts, pHconn, "pCompCode : out", "pReason : out" });
            pCompCode = 2;
            pReason = 0x8fa;
            base.TrExit(method);
        }

        public void SpiGet(Hconn hConn, Hobj hObj, MQMessageDescriptor mqmd, MQGetMessageOptions mqgmo, SpiGetOptions spigo, int bufferLength, byte[] buffer, out int dataLength, out int compCode, out int reason)
        {
            uint method = 0x5ba;
            this.TrEntry(method, new object[] { hConn, hObj, mqmd, mqgmo, spigo, bufferLength, buffer, "dataLength : out", "compCode : out", "reason : out" });
            compCode = 2;
            reason = 0;
            MQSPIGetOpts opts = new MQSPIGetOpts(spigo.Options);
            try
            {
                this.SPIGet(hConn, hObj, ref mqmd, ref mqgmo, ref opts, bufferLength, buffer, out dataLength, out compCode, out reason);
                spigo.Options = opts.Options;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SPIGet(Hconn hConn, Hobj hObj, ref MQMessageDescriptor mqmd, ref MQGetMessageOptions mqgmo, ref MQSPIGetOpts spigo, int bufferLength, byte[] buffer, out int dataLength, out int compCode, out int reason)
        {
            uint method = 0x97;
            this.TrEntry(method, new object[] { hConn, hObj, mqmd, mqgmo, spigo, bufferLength, buffer, "dataLength : out", "compCode : out", "reason : out" });
            int dstOffset = 0;
            int verbId = 3;
            int offset = 0;
            int maxOutVersion = 0;
            int flags = 0;
            int maxInVersion = 0;
            int maxInOutVersion = 0;
            MQSPI mqspi = null;
            MQSPIGetInOut @out = null;
            MQSPIGetIn @in = null;
            MQSPIGetOut out2 = null;
            MQERD mqerd = null;
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQAPI mqapi = new MQAPI();
            dataLength = 0;
            compCode = 2;
            reason = 0x7e2;
            this.InExit();
            try
            {
                ManagedHconn managedHconn = this.GetManagedHconn(hConn);
                MQSession session = managedHconn.Session;
                offset = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                if (managedHconn.IsReconnectable && ((mqgmo.Options & 0x8000) != 0))
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x9f3, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                if ((hObj == null) || !(hObj is ManagedHobj))
                {
                    NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x7e3, null);
                    base.TrException(method, exception2);
                    throw exception2;
                }
                ManagedHobj hobj1 = (ManagedHobj) hObj;
                if (mqmd == null)
                {
                    NmqiException exception3 = new NmqiException(base.env, -1, null, 2, 0x7ea, null);
                    base.TrException(method, exception3);
                    throw exception3;
                }
                if (mqgmo == null)
                {
                    NmqiException exception4 = new NmqiException(base.env, -1, null, 2, 0x88a, null);
                    base.TrException(method, exception4);
                    throw exception4;
                }
                try
                {
                    managedHconn.EnterCall(this.IsDispatchThread(), false);
                    session.CheckIfDisconnected();
                    if (!session.SpiQueryOut.SPISupported(verbId, ref maxInOutVersion, ref maxInVersion, ref maxOutVersion, ref flags))
                    {
                        compCode = 2;
                        reason = 0x8fa;
                    }
                    else
                    {
                        mqspi = new MQSPI();
                        @out = new MQSPIGetInOut(maxInOutVersion);
                        @in = new MQSPIGetIn(maxInVersion);
                        out2 = new MQSPIGetOut(maxOutVersion);
                        int num2 = offset;
                        mqspi.VerbId = 3;
                        mqspi.OutStructVersion = out2.Version;
                        mqspi.OutStructLength = out2.GetVersionLength() + bufferLength;
                        @out.MsgDesc = mqmd;
                        @out.GetMsgOpts = mqgmo;
                        @in.MaxMsgLength = bufferLength;
                        if (spigo != null)
                        {
                            @in.Options = spigo.Options;
                        }
                        out2.SPIGetOpts = spigo;
                        int callLength = (((offset + mqapi.GetLength()) + mqspi.GetLength()) + @out.GetVersionLength()) + Math.Max(mqspi.OutStructLength, @in.GetVersionLength());
                        mqapi.Initialize(callLength, hObj.Handle);
                        tsh = session.AllocateTSH(140, 0, true, callLength);
                        byte[] tshBuffer = tsh.TshBuffer;
                        num2 += mqapi.WriteStruct(tshBuffer, offset);
                        num2 += mqspi.WriteStruct(tshBuffer, num2);
                        num2 += @out.WriteStruct(tshBuffer, num2);
                        num2 += @in.WriteStruct(tshBuffer, num2);
                        tsh.WriteStruct(tshBuffer, 0);
                        tsh.TshBuffer = tshBuffer;
                        session.SendTSH(tsh);
                        do
                        {
                            if (rTSH != null)
                            {
                                session.ReleaseReceivedTSH(rTSH);
                                rTSH = null;
                            }
                            rTSH = session.ReceiveTSH(null);
                            switch (rTSH.SegmentType)
                            {
                                case 5:
                                    mqerd = new MQERD();
                                    if (rTSH.Length > offset)
                                    {
                                        mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                    }
                                    throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());

                                case 0x9c:
                                    break;

                                default:
                                {
                                    string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                                    CommonServices.SetValidInserts();
                                    CommonServices.ArithInsert1 = rTSH.SegmentType;
                                    CommonServices.CommentInsert1 = channelName;
                                    CommonServices.CommentInsert2 = "Unexpected response received during SPIGet";
                                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                                    throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                                }
                            }
                            if ((rTSH.ControlFlags1 & 0x10) != 0)
                            {
                                rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                compCode = mqapi.mqapi.CompCode;
                                reason = mqapi.mqapi.Reason;
                                if (compCode == 2)
                                {
                                    return;
                                }
                                rTSH.Offset = mqspi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                rTSH.Offset = @out.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                rTSH.Offset = out2.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                dataLength = out2.MsgLength;
                            }
                            int count = tsh.Length - num2;
                            if (count > buffer.Length)
                            {
                                count = buffer.Length;
                            }
                            Buffer.BlockCopy(rTSH.TshBuffer, rTSH.Offset, buffer, dstOffset, count);
                            dstOffset += count;
                        }
                        while ((rTSH.ControlFlags1 & 0x20) == 0);
                    }
                }
                finally
                {
                    try
                    {
                        if (managedHconn != null)
                        {
                            if (rTSH != null)
                            {
                                try
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                                catch (NmqiException exception5)
                                {
                                    base.TrException(method, exception5, 1);
                                }
                            }
                            managedHconn.LeaveCall(reason);
                        }
                    }
                    catch (NmqiException exception6)
                    {
                        base.TrException(method, exception6, 2);
                        compCode = exception6.CompCode;
                        reason = exception6.Reason;
                    }
                }
            }
            catch (NmqiException exception7)
            {
                base.TrException(method, exception7, 3);
                compCode = exception7.CompCode;
                reason = exception7.Reason;
            }
            catch (Exception exception8)
            {
                base.TrException(method, exception8, 4);
                compCode = 2;
                reason = 0x893;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void spiNotify(Hconn getterHconn, ref int options, LpiNotifyDetails notifyDetails, out int pCompCode, out int pReason)
        {
            uint method = 0x5c3;
            this.TrEntry(method, new object[] { getterHconn, (int) options, notifyDetails, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                MQSession failedSession = this.SPINotify(getterHconn, ref options, notifyDetails, out pCompCode, out pReason);
                ManagedHconn hconn = (ManagedHconn) getterHconn;
                if (((pReason != 0) && hconn.IsReconnectable) && !this.ThreadIsReconnectThread())
                {
                    if (ManagedHconn.IsReconnectableReasonCode(pReason) && !hconn.HasFailed())
                    {
                        try
                        {
                            hconn.Reconnect(failedSession);
                            this.SPINotify(getterHconn, ref options, notifyDetails, out pCompCode, out pReason);
                        }
                        catch (NmqiException)
                        {
                        }
                    }
                    if (hconn.HasFailed())
                    {
                        pCompCode = hconn.ReconnectionFailureCompCode;
                        pReason = hconn.ReconnectionFailureReason;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal MQSession SPINotify(Hconn hConn, ref int options, LpiNotifyDetails notifyDetails, out int compCode, out int reason)
        {
            uint method = 0x3bd;
            this.TrEntry(method, new object[] { hConn, (int) options, notifyDetails, "compCode : out", "reason : out" });
            int verbId = 11;
            int offset = 0;
            int maxInOutVersion = 0;
            int maxInVersion = 0;
            int maxOutVersion = 0;
            int flags = 0;
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQAPI mqapi = new MQAPI();
            ManagedHconn managedHconn = null;
            MQSession session = null;
            compCode = 0;
            reason = 0;
            this.InExit();
            try
            {
                managedHconn = this.GetManagedHconn(hConn);
                session = managedHconn.Session;
                offset = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                managedHconn.EnterNotifyCall(this.IsDispatchThread());
                try
                {
                    session.CheckIfDisconnected();
                    if (!session.SpiQueryOut.SPISupported(verbId, ref maxInOutVersion, ref maxInVersion, ref maxOutVersion, ref flags))
                    {
                        compCode = 2;
                        reason = 0x8fa;
                        return session;
                    }
                    if (managedHconn.FapLevel < 9)
                    {
                        NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x8fa, null);
                        base.TrException(method, ex);
                        throw ex;
                    }
                    MQSPI mqspi = new MQSPI();
                    MQSPINotifyInOut @out = new MQSPINotifyInOut(base.env, maxInOutVersion);
                    MQSPINotifyIn @in = new MQSPINotifyIn(base.env, maxInVersion);
                    MQSPINotifyOut out2 = new MQSPINotifyOut(base.env, maxOutVersion);
                    int num2 = offset;
                    mqspi.VerbId = verbId;
                    mqspi.OutStructVersion = out2.Version;
                    mqspi.OutStructLength = out2.GetVersionLength();
                    @in.Options = options;
                    @in.ConnectionId = notifyDetails.ConnectionId;
                    @in.Reason = notifyDetails.Reason;
                    int translength = (((offset + 0x10) + mqspi.GetLength()) + @out.GetVersionLength()) + Math.Max(mqspi.OutStructLength, @in.GetVersionLength());
                    tsh = session.AllocateTSH(140, 0, true, translength);
                    byte[] tshBuffer = tsh.TshBuffer;
                    mqapi.Initialize(translength, 0);
                    num2 += mqapi.WriteStruct(tshBuffer, offset);
                    num2 += mqspi.WriteStruct(tshBuffer, num2);
                    num2 += @out.WriteStruct(tshBuffer, num2);
                    num2 += @in.WriteStruct(tshBuffer, num2);
                    tsh.WriteStruct(tshBuffer, 0);
                    tsh.TshBuffer = tshBuffer;
                    session.SendTSH(tsh);
                    do
                    {
                        rTSH = session.ReceiveTSH(null);
                        switch (rTSH.SegmentType)
                        {
                            case 5:
                            {
                                MQERD mqerd = new MQERD();
                                if (rTSH.Length > offset)
                                {
                                    mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                }
                                throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());
                            }
                            case 0x9c:
                                break;

                            default:
                            {
                                string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                                CommonServices.SetValidInserts();
                                CommonServices.ArithInsert1 = rTSH.SegmentType;
                                CommonServices.CommentInsert1 = channelName;
                                base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                                throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                            }
                        }
                        if ((rTSH.ControlFlags1 & 0x10) != 0)
                        {
                            rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            compCode = mqapi.mqapi.CompCode;
                            reason = mqapi.mqapi.Reason;
                            if (compCode == 2)
                            {
                                return session;
                            }
                            rTSH.Offset = mqspi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            rTSH.Offset = @out.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            rTSH.Offset = out2.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                        }
                    }
                    while ((rTSH.ControlFlags1 & 0x20) == 0);
                }
                finally
                {
                    try
                    {
                        if (managedHconn != null)
                        {
                            try
                            {
                                if (rTSH != null)
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                            }
                            finally
                            {
                                managedHconn.LeaveNotifyCall(reason);
                            }
                        }
                    }
                    catch (NmqiException exception2)
                    {
                        base.TrException(method, exception2, 1);
                    }
                }
                return session;
            }
            catch (NmqiException exception3)
            {
                base.TrException(method, exception3, 2);
                if ((exception3.CompCode != 2) || (exception3.Reason != 0x7d9))
                {
                    compCode = exception3.CompCode;
                    reason = exception3.Reason;
                }
                return session;
            }
            finally
            {
                base.TrExit(method);
            }
            return session;
        }

        public void SpiOpen(Hconn hconn, ref MQObjectDescriptor od, ref SpiOpenOptions options, Phobj phobj, out int pCompCode, out int pReason)
        {
            uint method = 0x5c5;
            this.TrEntry(method, new object[] { hconn, od, options, phobj, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                MQSession failedSession = this.nmqiOpen(hconn, ref od, ref options, 0, ref phobj, out pCompCode, out pReason, true, null);
                ManagedHconn hconn2 = (ManagedHconn) hconn;
                if (((pReason != 0) && hconn2.IsReconnectable) && !this.ThreadIsReconnectThread())
                {
                    if (ManagedHconn.IsReconnectableReasonCode(pReason) && !hconn2.HasFailed())
                    {
                        try
                        {
                            hconn2.Reconnect(failedSession);
                            this.SpiOpen(hconn, ref od, ref options, phobj, out pCompCode, out pReason);
                        }
                        catch (NmqiException)
                        {
                        }
                    }
                    if (hconn2.HasFailed())
                    {
                        pCompCode = hconn2.ReconnectionFailureCompCode;
                        pReason = hconn2.ReconnectionFailureReason;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SPIPut(Hconn hConn, Hobj hObj, ref MQMessageDescriptor mqmd, ref MQPutMessageOptions mqpmo, ref MQSPIPutOpts spipo, int msglength, byte[] buffer, out int compCode, out int reason)
        {
            uint method = 0x91;
            this.TrEntry(method, new object[] { hConn, hObj, mqmd, mqpmo, spipo, msglength, buffer, "compCode : out", "reason : out" });
            int verbId = 2;
            int offset = 0;
            int flags = 0;
            int maxInOutVersion = 0;
            int maxInVersion = 0;
            int maxOutVersion = 0;
            int srcOffset = 0;
            int num11 = msglength;
            byte num13 = 0x10;
            MQERD mqerd = null;
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQSPI mqspi = null;
            MQSPIPutInOut @out = null;
            MQSPIPutIn @in = null;
            MQSPIPutOut out2 = null;
            MQAPI mqapi = new MQAPI();
            ManagedHconn managedHconn = null;
            MQSession session = null;
            compCode = 2;
            reason = 0x7e2;
            this.InExit();
            try
            {
                managedHconn = this.GetManagedHconn(hConn);
                session = managedHconn.Session;
                offset = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                if (managedHconn.IsReconnectable && ((mqpmo.Options & 0x8000) != 0))
                {
                    compCode = 2;
                    reason = 0x9f3;
                    NmqiException ex = new NmqiException(base.env, -1, null, compCode, reason, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                if ((hObj == null) || !(hObj is ManagedHobj))
                {
                    compCode = 2;
                    reason = 0x7e3;
                    NmqiException exception2 = new NmqiException(base.env, -1, null, compCode, reason, null);
                    base.TrException(method, exception2);
                    throw exception2;
                }
                ManagedHobj hobj1 = (ManagedHobj) hObj;
                if (mqmd == null)
                {
                    compCode = 2;
                    reason = 0x7ea;
                    NmqiException exception3 = new NmqiException(base.env, -1, null, compCode, reason, null);
                    base.TrException(method, exception3);
                    throw exception3;
                }
                if (mqpmo == null)
                {
                    compCode = 2;
                    reason = 0x87d;
                    NmqiException exception4 = new NmqiException(base.env, -1, null, compCode, reason, null);
                    base.TrException(method, exception4);
                    throw exception4;
                }
                if (((mqpmo.Options & 4) == 0) && ((mqpmo.Options & 2) == 0))
                {
                    mqpmo.Options |= 4;
                }
                if (buffer == null)
                {
                    compCode = 2;
                    reason = 0x7d4;
                    NmqiException exception5 = new NmqiException(base.env, -1, null, compCode, reason, null);
                    base.TrException(method, exception5);
                    throw exception5;
                }
                if (msglength > session.MaximumMessageLength)
                {
                    compCode = 2;
                    reason = 0x8aa;
                    NmqiException exception6 = new NmqiException(base.env, -1, null, compCode, reason, null);
                    base.TrException(method, exception6);
                    throw exception6;
                }
                if (msglength > buffer.Length)
                {
                    compCode = 2;
                    reason = 0x7d5;
                    NmqiException exception7 = new NmqiException(base.env, -1, null, compCode, reason, null);
                    base.TrException(method, exception7);
                    throw exception7;
                }
                int options = mqpmo.Options;
                managedHconn.EnterCall(this.IsDispatchThread(), false);
                if ((mqpmo.Options & 2) != 0)
                {
                    managedHconn.SetInTransaction();
                }
                try
                {
                    session.CheckIfDisconnected();
                    if (!session.SpiQueryOut.SPISupported(verbId, ref maxInOutVersion, ref maxInVersion, ref maxOutVersion, ref flags))
                    {
                        compCode = 2;
                        reason = 0x8fa;
                    }
                    else
                    {
                        mqspi = new MQSPI();
                        @out = new MQSPIPutInOut(maxInOutVersion);
                        @in = new MQSPIPutIn(maxInVersion);
                        out2 = new MQSPIPutOut(maxOutVersion);
                        while (true)
                        {
                            int num10;
                            byte[] b = null;
                            int num2 = offset;
                            if ((num13 & 0x10) != 0)
                            {
                                mqspi.VerbId = 2;
                                mqspi.OutStructVersion = out2.Version;
                                mqspi.OutStructLength = out2.Length;
                                @out.MsgDesc = mqmd;
                                @out.PutMsgOpts = mqpmo;
                                @in.MsgLength = msglength;
                                if (spipo != null)
                                {
                                    @in.Options = spipo.Options;
                                }
                                int translength = (((offset + mqapi.GetLength()) + mqspi.GetLength()) + @out.GetVersionLength()) + Math.Max(mqspi.OutStructLength, @in.GetVersionLength() + msglength);
                                tsh = session.AllocateTSH(140, 0, true, translength);
                                b = tsh.TshBuffer;
                                mqapi.Initialize(translength, hObj.Handle);
                                num2 += mqapi.WriteStruct(b, offset);
                                num2 += mqspi.WriteStruct(b, num2);
                                num2 += @out.WriteStruct(b, num2);
                                num2 += @in.WriteStruct(b, num2);
                            }
                            int num12 = session.ParentConnection.MaxTransmissionSize - num2;
                            if (num12 < num11)
                            {
                                num10 = num12;
                            }
                            else
                            {
                                num10 = num11;
                            }
                            num11 -= num10;
                            if (num11 == 0)
                            {
                                num13 = (byte) (num13 | 0x20);
                            }
                            Buffer.BlockCopy(buffer, srcOffset, b, num2, num10);
                            num2 += num10;
                            if (session.IsMultiplexingEnabled)
                            {
                                num2 = (num2 + 3) & -4;
                            }
                            tsh.WriteStruct(b, 0);
                            tsh.TshBuffer = b;
                            session.SendTSH(tsh);
                            num13 = (byte) (num13 & 0xef);
                            srcOffset += num10;
                            if ((num13 & 0x20) != 0)
                            {
                                if ((@in.Options & 8) == 0)
                                {
                                    rTSH = session.ReceiveTSH(null);
                                    switch (rTSH.SegmentType)
                                    {
                                        case 5:
                                            mqerd = new MQERD();
                                            if (rTSH.Length > offset)
                                            {
                                                mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                            }
                                            throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());

                                        case 0x9c:
                                            rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                            compCode = mqapi.mqapi.CompCode;
                                            reason = mqapi.mqapi.Reason;
                                            if (compCode != 2)
                                            {
                                                rTSH.Offset = mqspi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                                rTSH.Offset = @out.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                            }
                                            return;
                                    }
                                    string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                                    CommonServices.SetValidInserts();
                                    CommonServices.ArithInsert1 = rTSH.SegmentType;
                                    CommonServices.CommentInsert1 = channelName;
                                    CommonServices.CommentInsert2 = "Unexpected flow received during SPIPut";
                                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                                    throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                                }
                                compCode = 0;
                                reason = 0;
                                return;
                            }
                        }
                    }
                }
                finally
                {
                    try
                    {
                        if (managedHconn != null)
                        {
                            if (rTSH != null)
                            {
                                try
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                                catch (NmqiException exception8)
                                {
                                    base.TrException(method, exception8, 1);
                                    compCode = exception8.CompCode;
                                    reason = exception8.Reason;
                                }
                            }
                            managedHconn.LeaveCall(reason);
                        }
                    }
                    catch (NmqiException exception9)
                    {
                        base.TrException(method, exception9, 2);
                        compCode = exception9.CompCode;
                        reason = exception9.Reason;
                    }
                }
            }
            catch (NmqiException exception10)
            {
                base.TrException(method, exception10, 3);
                compCode = exception10.CompCode;
                reason = exception10.Reason;
            }
            catch (Exception exception11)
            {
                compCode = 2;
                reason = 0x893;
                base.TrException(method, exception11, 4);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void SPIQuerySPI(Hconn hconn, out int compCode, out int reason, bool enterCall)
        {
            uint method = 0x5c2;
            this.TrEntry(method, new object[] { hconn, "compCode : out", "reason : out", enterCall });
            int offset = 0;
            MQAPI mqapi = new MQAPI();
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQSession session = null;
            ManagedHconn hconn2 = (ManagedHconn) hconn;
            compCode = 2;
            reason = 0x8fa;
            this.InExit();
            try
            {
                session = hconn2.Session;
                offset = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                if (enterCall)
                {
                    hconn2.EnterCall(this.IsDispatchThread(), false);
                    session.CheckIfDisconnected();
                }
                if (session.SpiQueryOut == null)
                {
                    MQSPI mqspi = new MQSPI();
                    MQSPIQueryInOut @out = new MQSPIQueryInOut();
                    MQSPIQueryIn @in = new MQSPIQueryIn();
                    byte[] b = null;
                    session.SpiQueryOut = new MQSPIQueryOut();
                    int num3 = offset;
                    mqspi.VerbId = 1;
                    mqspi.OutStructVersion = session.SpiQueryOut.Version;
                    mqspi.OutStructLength = session.SpiQueryOut.GetVersionLength();
                    int translength = (((offset + 0x10) + mqspi.GetLength()) + @out.GetVersionLength()) + Math.Max(mqspi.OutStructLength, @in.GetVersionLength());
                    tsh = session.AllocateTSH(140, 0, true, translength);
                    b = tsh.TshBuffer;
                    mqapi.Initialize(translength, hconn2.Value);
                    num3 += mqapi.WriteStruct(b, offset);
                    num3 += mqspi.WriteStruct(b, num3);
                    num3 += @out.WriteStruct(b, num3);
                    num3 += @in.WriteStruct(b, num3);
                    tsh.WriteStruct(b, 0);
                    tsh.TshBuffer = b;
                    session.SendTSH(tsh);
                    rTSH = session.ReceiveTSH(null);
                    switch (rTSH.SegmentType)
                    {
                        case 5:
                        {
                            MQERD mqerd = new MQERD();
                            if (rTSH.Length > offset)
                            {
                                mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            }
                            throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());
                        }
                        case 0x9c:
                            rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            compCode = mqapi.mqapi.CompCode;
                            reason = mqapi.mqapi.Reason;
                            if (compCode != 2)
                            {
                                rTSH.Offset = mqspi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                rTSH.Offset = @out.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                rTSH.Offset = session.SpiQueryOut.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            }
                            return;
                    }
                    string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = rTSH.SegmentType;
                    CommonServices.CommentInsert1 = channelName;
                    CommonServices.CommentInsert2 = "Unexpected flow received during SPIQuerySPI";
                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                    throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                }
                compCode = 0;
                reason = 0;
            }
            finally
            {
                try
                {
                    if (rTSH != null)
                    {
                        try
                        {
                            session.ReleaseReceivedTSH(rTSH);
                        }
                        catch (NmqiException)
                        {
                        }
                    }
                    if (enterCall)
                    {
                        hconn2.LeaveCall(reason);
                    }
                }
                catch (NmqiException exception)
                {
                    base.TrException(method, exception, 1);
                    compCode = exception.CompCode;
                    reason = exception.Reason;
                }
                base.TrExit(method);
            }
        }

        public void SPIQuerySPI(Hconn hConn, int verbId, ref int maxInOutVersion, ref int maxInVersion, ref int maxOutVersion, ref int flags, out int compCode, out int reason)
        {
            uint method = 0x5c1;
            this.TrEntry(method, new object[] { hConn, verbId, (int) maxInOutVersion, (int) maxInVersion, (int) maxOutVersion, (int) flags, "compCode : out", "reason : out" });
            compCode = 0;
            reason = 0;
            MQSession session = this.GetManagedHconn(hConn).Session;
            try
            {
                this.SPIQuerySPI(session.Hconn, out compCode, out reason, true);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void spiSubscribe(Hconn hconn, LpiSD plpiSD, MQSubscriptionDescriptor pSubDesc, Phobj pHobj, Phobj pHsub, out int pCompCode, out int pReason)
        {
            uint method = 0x5c4;
            this.TrEntry(method, new object[] { hconn, plpiSD, pSubDesc, pHobj, pHsub, "pCompCode : out", "pReason : out" });
            pCompCode = 0;
            pReason = 0;
            try
            {
                this.spiSubscribe(hconn, plpiSD, pSubDesc, ref pHobj, ref pHsub, out pCompCode, out pReason, null);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void spiSubscribe(Hconn hConn, LpiSD plpiSD, MQSubscriptionDescriptor pSubDesc, ref Phobj pHobj, ref Phobj pHsub, out int compCode, out int reason, ManagedHsub rcnHsub)
        {
            uint method = 0x3be;
            this.TrEntry(method, new object[] { hConn, plpiSD, pSubDesc, pHobj, pHsub, "compCode : out", "reason : out", rcnHsub });
            compCode = 0;
            reason = 0;
            try
            {
                MQSession failedSession = this.nmqiSubscribe(hConn, pSubDesc, ref pHobj, ref pHsub, out compCode, out reason, plpiSD, true, null);
                ManagedHconn hconn = hConn as ManagedHconn;
                if (((reason != 0) && hconn.IsReconnectable) && !this.ThreadIsReconnectThread())
                {
                    if (ManagedHconn.IsReconnectableReasonCode(reason) && !hconn.HasFailed())
                    {
                        try
                        {
                            hconn.Reconnect(failedSession);
                            this.spiSubscribe(hconn, plpiSD, pSubDesc, ref pHobj, ref pHsub, out compCode, out reason, null);
                        }
                        catch (NmqiException)
                        {
                        }
                    }
                    if (hconn.HasFailed())
                    {
                        compCode = hconn.ReconnectionFailureCompCode;
                        reason = hconn.ReconnectionFailureReason;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SPISyncpoint(Hconn hConn, ref MQSPISyncpointOpts spispo, out int compCode, out int reason)
        {
            uint method = 0xa1;
            this.TrEntry(method, new object[] { hConn, spispo, "compCode : out", "reason : out" });
            int maxInOutVersion = 0;
            int maxInVersion = 0;
            int maxOutVersion = 0;
            int flags = 0;
            int verbId = 5;
            int offset = 0;
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQAPI mqapi = new MQAPI();
            MQSPI mqspi = null;
            MQSPISyncpointInOut @out = null;
            MQERD mqerd = null;
            ManagedHconn managedHconn = null;
            MQSession session = null;
            compCode = 2;
            reason = 0x7e2;
            this.InExit();
            try
            {
                managedHconn = this.GetManagedHconn(hConn);
                session = managedHconn.Session;
                offset = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                managedHconn.EnterCall(this.IsDispatchThread(), false);
                try
                {
                    session.CheckIfDisconnected();
                    if (!session.SpiQueryOut.SPISupported(verbId, ref maxInOutVersion, ref maxInVersion, ref maxOutVersion, ref flags))
                    {
                        compCode = 2;
                        reason = 0x8fa;
                    }
                    else
                    {
                        mqspi = new MQSPI();
                        @out = new MQSPISyncpointInOut(maxInOutVersion);
                        MQSPISyncpointIn @in = new MQSPISyncpointIn(maxInVersion);
                        MQSPISyncpointOut out2 = new MQSPISyncpointOut(maxOutVersion);
                        int num6 = offset;
                        mqspi.VerbId = 5;
                        mqspi.OutStructVersion = out2.Version;
                        mqspi.OutStructLength = out2.GetVersionLength();
                        if (spispo != null)
                        {
                            @in.Options = spispo.Options;
                            @in.Action = spispo.Action;
                        }
                        int translength = (((offset + 0x10) + mqspi.GetLength()) + @out.GetVersionLength()) + Math.Max(mqspi.OutStructLength, @in.GetVersionLength());
                        tsh = session.AllocateTSH(140, 0, true, translength);
                        byte[] tshBuffer = tsh.TshBuffer;
                        mqapi.Initialize(translength, hConn.Value);
                        num6 += mqapi.WriteStruct(tshBuffer, offset);
                        num6 += mqspi.WriteStruct(tshBuffer, num6);
                        num6 += @out.WriteStruct(tshBuffer, num6);
                        num6 += @in.WriteStruct(tshBuffer, num6);
                        tsh.WriteStruct(tshBuffer, 0);
                        tsh.TshBuffer = tshBuffer;
                        session.SendTSH(tsh);
                        rTSH = session.ReceiveTSH(null);
                        switch (rTSH.SegmentType)
                        {
                            case 5:
                                mqerd = new MQERD();
                                if (rTSH.Length > offset)
                                {
                                    mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                }
                                throw mqerd.ErrorException(session.ParentConnection.ChannelName.Trim());

                            case 0x9c:
                                break;

                            default:
                            {
                                string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                                CommonServices.SetValidInserts();
                                CommonServices.ArithInsert1 = rTSH.SegmentType;
                                CommonServices.CommentInsert1 = channelName;
                                CommonServices.CommentInsert2 = "Unexpected flow received during SPISyncpoint.";
                                base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                                throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                            }
                        }
                        num6 = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                        compCode = mqapi.mqapi.CompCode;
                        reason = mqapi.mqapi.Reason;
                        if (compCode != 2)
                        {
                            num6 = mqspi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                        }
                    }
                }
                finally
                {
                    try
                    {
                        if (managedHconn != null)
                        {
                            if (rTSH != null)
                            {
                                try
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                                catch (NmqiException)
                                {
                                }
                            }
                            managedHconn.LeaveCall(reason);
                        }
                    }
                    catch (NmqiException)
                    {
                    }
                }
            }
            catch (NmqiException exception)
            {
                base.TrException(method, exception, 1);
                compCode = exception.CompCode;
                reason = exception.Reason;
            }
            catch (Exception exception2)
            {
                base.TrException(method, exception2, 2);
                compCode = 2;
                reason = 0x893;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private bool ThreadIsReconnectThread()
        {
            uint method = 0x5b3;
            this.TrEntry(method);
            bool result = false;
            try
            {
                object data = Thread.GetData(Thread.GetNamedDataSlot("MQ_CLIENT_THREAD_TYPE"));
                if ((data != null) && (Convert.ToInt32(data) == 3))
                {
                    result = true;
                }
            }
            catch (Exception exception)
            {
                base.TrException(method, exception);
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        private void TransactionCheck(ManagedHconn hconn, int options, int operation)
        {
            uint method = 0x5cc;
            this.TrEntry(method, new object[] { hconn, options, operation });
            try
            {
                if (Transaction.Current != null)
                {
                    hconn.CheckUOWNotMixed(options, operation);
                    hconn.ValidateConnectOptionsForXA();
                    hconn.EnlistTransaction(base.env, hconn);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void ValidateMQCBD(MQCBD cbd)
        {
            uint method = 0x5cb;
            this.TrEntry(method, new object[] { cbd });
            try
            {
                int num2;
                if (cbd == null)
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x98c, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                if ((cbd.Version < 1) || (cbd.Version > 1))
                {
                    NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x17f0, null);
                    base.TrException(method, exception2);
                    throw exception2;
                }
                switch (cbd.CallbackType)
                {
                    case 1:
                        num2 = 0x2305;
                        break;

                    case 2:
                        num2 = 0x2300;
                        break;

                    default:
                    {
                        NmqiException exception3 = new NmqiException(base.env, -1, null, 2, 0x9b3, null);
                        base.TrException(method, exception3);
                        throw exception3;
                    }
                }
                if ((cbd.Options & ~num2) != 0)
                {
                    NmqiException exception4 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                    base.TrException(method, exception4);
                    throw exception4;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void ValidateMQCTLO(MQCTLO ctlo)
        {
            uint method = 0x5ca;
            this.TrEntry(method, new object[] { ctlo });
            try
            {
                if (ctlo == null)
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x98d, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                if ((ctlo.Version < 1) || (ctlo.Version > 1))
                {
                    NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x17f0, null);
                    base.TrException(method, exception2);
                    throw exception2;
                }
                int num2 = 0x2000;
                if ((ctlo.Options & ~num2) != 0)
                {
                    NmqiException exception3 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                    base.TrException(method, exception3);
                    throw exception3;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal int WriteIntArray(byte[] bytes, int offset, int[] array, int arrayCount)
        {
            uint method = 0x8d;
            this.TrEntry(method, new object[] { bytes, offset, array, arrayCount });
            for (int i = 0; i < arrayCount; i++)
            {
                BitConverter.GetBytes(array[i]).CopyTo(bytes, offset);
                offset += 4;
            }
            base.TrExit(method, offset);
            return offset;
        }

        public int XA_Close(ManagedHconn remoteHconn, string xaCloseStr, int rmid, int flags)
        {
            uint method = 0x535;
            this.TrEntry(method, new object[] { remoteHconn, xaCloseStr, rmid, flags });
            int translength = 0;
            int reason = 0;
            int num5 = 0x10;
            int num6 = 0;
            bool flag = CommonServices.TraceStatus();
            MQAPI mqapi = new MQAPI();
            MQXAInfo info = new MQXAInfo();
            MQTSH tsh = null;
            MQTSH rTSH = null;
            this.InExit();
            try
            {
                MQSession session = remoteHconn.Session;
                num6 = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                translength = num6 + num5;
                if ((reason == 0) && (xaCloseStr == null))
                {
                    return -5;
                }
                info.SetXaInfo("qmname = " + xaCloseStr.Trim());
                translength += info.GetLength();
                if ((reason == 0) && ((flags & 0x8000000) != 0))
                {
                    return -2;
                }
                if ((reason == 0) && (flags != 0))
                {
                    return -5;
                }
                if ((remoteHconn.XAState & 2) == 0)
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x92f, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                remoteHconn.EnterCall(this.IsDispatchThread(), false);
                try
                {
                    session.CheckIfDisconnected();
                    tsh = session.AllocateTSH(0xa4, 0, true, translength);
                    byte[] tshBuffer = tsh.TshBuffer;
                    int offset = tsh.WriteStruct(tshBuffer, 0);
                    mqapi.InitializeForXA(translength, rmid, flags);
                    offset += mqapi.WriteStruct(tshBuffer, offset);
                    offset += info.WriteStruct(tshBuffer, offset);
                    tsh.TshBuffer = tshBuffer;
                    if (flag)
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "XA_Close  >>");
                        base.TrData(method, 0, "Hconn", BitConverter.GetBytes(remoteHconn.Value));
                        base.TrData(method, 0, "Buffer", 0, offset, tshBuffer);
                        base.TrAPIOutput(method, "Reason");
                    }
                    session.SendTSH(tsh);
                    tshBuffer = null;
                    rTSH = session.ReceiveTSH(null);
                    switch (rTSH.SegmentType)
                    {
                        case 5:
                        {
                            MQERD mqerd = new MQERD();
                            if (rTSH.Length > num6)
                            {
                                mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            }
                            throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());
                        }
                        case 180:
                            break;

                        default:
                        {
                            string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                            CommonServices.SetValidInserts();
                            CommonServices.ArithInsert1 = rTSH.SegmentType;
                            CommonServices.CommentInsert1 = channelName;
                            CommonServices.CommentInsert2 = "Unexpected flow received during xa_close";
                            base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                            throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                        }
                    }
                    rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                    reason = mqapi.mqapi.ReturnCode;
                    remoteHconn.XAState &= -3;
                }
                finally
                {
                    try
                    {
                        if (remoteHconn != null)
                        {
                            if (rTSH != null)
                            {
                                try
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                                catch (NmqiException)
                                {
                                }
                            }
                            remoteHconn.LeaveCall(reason);
                        }
                    }
                    catch (NmqiException exception2)
                    {
                        base.TrException(method, exception2, 1);
                        int compCode = exception2.CompCode;
                        int num9 = exception2.Reason;
                    }
                }
                return reason;
            }
            catch (NmqiException exception3)
            {
                base.TrException(method, exception3, 2);
                int num10 = exception3.CompCode;
                int num11 = exception3.Reason;
            }
            finally
            {
                if (flag)
                {
                    base.TrAPI(method, "__________");
                    base.TrAPI(method, "XA_Close  <<");
                    base.TrAPIInput(method, "Hconn");
                    base.TrData(method, 0, "Reason", BitConverter.GetBytes(reason));
                }
                base.TrExit(method);
            }
            return reason;
        }

        public int XA_Commit(ManagedHconn remoteHconn, MQXid xid, int rmid, int flags)
        {
            uint method = 0x531;
            this.TrEntry(method, new object[] { remoteHconn, xid, rmid, flags });
            int translength = 0;
            int reason = 0;
            int num5 = 0x10;
            int num6 = 0;
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQAPI mqapi = new MQAPI();
            bool flag = CommonServices.TraceStatus();
            this.InExit();
            try
            {
                MQSession session = remoteHconn.Session;
                num6 = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                translength = (num6 + num5) + xid.GetRoundedLength();
                if ((reason == 0) && (xid == null))
                {
                    return -5;
                }
                if ((reason == 0) && ((flags & 0x8000000) != 0))
                {
                    return -2;
                }
                if ((reason == 0) && ((flags & -1342177281) != 0))
                {
                    return -5;
                }
                if ((remoteHconn.XAState & 2) == 0)
                {
                    reason = 0x92f;
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x92f, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                remoteHconn.EnterCall(this.IsDispatchThread(), false);
                try
                {
                    session.CheckIfDisconnected();
                    tsh = session.AllocateTSH(0xa6, 0, true, translength);
                    byte[] tshBuffer = tsh.TshBuffer;
                    int offset = tsh.WriteStruct(tshBuffer, 0);
                    mqapi.InitializeForXA(translength, rmid, flags);
                    offset += mqapi.WriteStruct(tshBuffer, offset);
                    offset += xid.WriteStruct(tshBuffer, offset);
                    tsh.TshBuffer = tshBuffer;
                    tsh.Offset = 0;
                    if (flag)
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "XA_Commit  >>");
                        base.TrData(method, 0, "Hconn", BitConverter.GetBytes(remoteHconn.Value));
                        base.TrData(method, 0, "Buffer", 0, offset, tshBuffer);
                        base.TrAPIOutput(method, "Reason");
                    }
                    session.SendTSH(tsh);
                    tshBuffer = null;
                    rTSH = session.ReceiveTSH(null);
                    switch (rTSH.SegmentType)
                    {
                        case 5:
                        {
                            MQERD mqerd = new MQERD();
                            if (rTSH.Length > num6)
                            {
                                mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            }
                            throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());
                        }
                        case 0xb6:
                            break;

                        default:
                        {
                            string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                            CommonServices.SetValidInserts();
                            CommonServices.ArithInsert1 = rTSH.SegmentType;
                            CommonServices.CommentInsert1 = channelName;
                            CommonServices.CommentInsert2 = "Unexpected flow received during xa_commit";
                            base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                            throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                        }
                    }
                    rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                    reason = mqapi.mqapi.ReturnCode;
                }
                finally
                {
                    try
                    {
                        if (remoteHconn != null)
                        {
                            if (rTSH != null)
                            {
                                try
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                                catch (NmqiException)
                                {
                                }
                            }
                            remoteHconn.LeaveCall(reason);
                        }
                    }
                    catch (NmqiException exception2)
                    {
                        base.TrException(method, exception2, 1);
                        int compCode = exception2.CompCode;
                        int num9 = exception2.Reason;
                    }
                }
                return reason;
            }
            catch (NmqiException exception3)
            {
                base.TrException(method, exception3, 2);
                int num10 = exception3.CompCode;
                int num11 = exception3.Reason;
            }
            catch (Exception exception4)
            {
                base.TrException(method, exception4, 3);
            }
            finally
            {
                if (flag)
                {
                    base.TrAPI(method, "__________");
                    base.TrAPI(method, "XA_Commit  <<");
                    base.TrAPIInput(method, "Hconn");
                    base.TrData(method, 0, "Reason", BitConverter.GetBytes(reason));
                }
                base.TrExit(method, 3);
            }
            return reason;
        }

        public int XA_End(ManagedHconn remoteHconn, MQXid xid, int rmid, int flags)
        {
            uint method = 0x52f;
            this.TrEntry(method, new object[] { remoteHconn, xid, rmid, flags });
            int reason = 0;
            int translength = 0;
            int num5 = 0x10;
            int num6 = 0;
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQAPI mqapi = new MQAPI();
            bool flag = CommonServices.TraceStatus();
            this.InExit();
            try
            {
                MQSession session = remoteHconn.Session;
                num6 = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                translength = (num6 + num5) + xid.GetRoundedLength();
                if ((reason == 0) && (xid == null))
                {
                    return -5;
                }
                if ((reason == 0) && ((flags & 0x8000000) != 0))
                {
                    return -2;
                }
                if ((reason == 0) && ((flags & -637534209) != 0))
                {
                    return -5;
                }
                if ((remoteHconn.XAState & 2) == 0)
                {
                    reason = 0x92f;
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x92f, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                remoteHconn.EnterCall(this.IsDispatchThread(), false);
                try
                {
                    if (reason != 0)
                    {
                        return reason;
                    }
                    session.CheckIfDisconnected();
                    tsh = session.AllocateTSH(0xa2, 0, true, translength);
                    byte[] tshBuffer = tsh.TshBuffer;
                    int offset = tsh.WriteStruct(tshBuffer, 0);
                    mqapi.InitializeForXA(translength, rmid, flags);
                    offset += mqapi.WriteStruct(tshBuffer, offset);
                    offset += xid.WriteStruct(tshBuffer, offset);
                    if (flag)
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "XA_End  >>");
                        base.TrData(method, 0, "Hconn", BitConverter.GetBytes(remoteHconn.Value));
                        base.TrData(method, 0, "Buffer", 0, offset, tshBuffer);
                        base.TrAPIOutput(method, "Reason");
                    }
                    tsh.TshBuffer = tshBuffer;
                    tsh.Offset = offset = 0;
                    session.SendTSH(tsh);
                    rTSH = session.ReceiveTSH(null);
                    switch (rTSH.SegmentType)
                    {
                        case 5:
                        {
                            MQERD mqerd = new MQERD();
                            if (rTSH.Length > num6)
                            {
                                mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            }
                            throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());
                        }
                        case 0xb2:
                            rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            reason = mqapi.mqapi.ReturnCode;
                            if (reason >= 0)
                            {
                                remoteHconn.XAState = -5;
                            }
                            return reason;
                    }
                    string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = rTSH.SegmentType;
                    CommonServices.CommentInsert1 = channelName;
                    CommonServices.CommentInsert2 = "Unexpected flow received during xa_end";
                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                    throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                }
                finally
                {
                    try
                    {
                        if (remoteHconn != null)
                        {
                            if (rTSH != null)
                            {
                                try
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                                catch (NmqiException)
                                {
                                }
                            }
                            remoteHconn.LeaveCall(reason);
                        }
                    }
                    catch (NmqiException exception2)
                    {
                        base.TrException(method, exception2, 1);
                        int compCode = exception2.CompCode;
                        int num9 = exception2.Reason;
                    }
                }
                return reason;
            }
            catch (NmqiException exception3)
            {
                base.TrException(method, exception3, 2);
                int num10 = exception3.CompCode;
                int num11 = exception3.Reason;
            }
            catch (Exception exception4)
            {
                base.TrException(method, exception4, 3);
            }
            finally
            {
                if (flag)
                {
                    base.TrAPI(method, "__________");
                    base.TrAPI(method, "XA_End  <<");
                    base.TrAPIInput(method, "Hconn");
                    base.TrData(method, 0, "Reason", BitConverter.GetBytes(reason));
                }
                base.TrExit(method, 3);
            }
            return reason;
        }

        public int XA_Forget(ManagedHconn remoteHconn, MQXid xid, int rmid, int flags)
        {
            uint method = 0x533;
            this.TrEntry(method, new object[] { remoteHconn, xid, rmid, flags });
            int translength = 0;
            int reason = 0;
            int num5 = 0x10;
            int num6 = 0;
            bool flag = CommonServices.TraceStatus();
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQAPI mqapi = new MQAPI();
            this.InExit();
            try
            {
                MQSession session = remoteHconn.Session;
                num6 = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                translength = (num6 + num5) + xid.GetRoundedLength();
                if ((reason == 0) && (xid == null))
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x92f, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                if ((reason == 0) && ((flags & 0x8000000) != 0))
                {
                    return -2;
                }
                if ((reason == 0) && (flags != 0))
                {
                    return -5;
                }
                if ((remoteHconn.XAState & 2) == 0)
                {
                    return -5;
                }
                remoteHconn.EnterCall(this.IsDispatchThread(), false);
                try
                {
                    session.CheckIfDisconnected();
                    tsh = session.AllocateTSH(0xa8, 0, true, translength);
                    byte[] tshBuffer = tsh.TshBuffer;
                    int offset = tsh.WriteStruct(tshBuffer, 0);
                    mqapi.InitializeForXA(translength, rmid, flags);
                    offset += mqapi.WriteStruct(tshBuffer, offset);
                    offset += xid.WriteStruct(tshBuffer, offset);
                    tsh.TshBuffer = tshBuffer;
                    session.SendTSH(tsh);
                    rTSH = session.ReceiveTSH(null);
                    switch (rTSH.SegmentType)
                    {
                        case 5:
                        {
                            MQERD mqerd = new MQERD();
                            if (rTSH.Length > num6)
                            {
                                mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            }
                            throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());
                        }
                        case 0xb8:
                            break;

                        default:
                        {
                            string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                            CommonServices.SetValidInserts();
                            CommonServices.ArithInsert1 = rTSH.SegmentType;
                            CommonServices.CommentInsert1 = channelName;
                            CommonServices.CommentInsert2 = "Unexpected flow received during xa_forget";
                            base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                            throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                        }
                    }
                    rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                    reason = mqapi.mqapi.ReturnCode;
                }
                finally
                {
                    try
                    {
                        if (remoteHconn != null)
                        {
                            if (rTSH != null)
                            {
                                try
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                                catch (NmqiException)
                                {
                                }
                            }
                            remoteHconn.LeaveCall(reason);
                        }
                    }
                    catch (NmqiException exception2)
                    {
                        base.TrException(method, exception2, 1);
                        int compCode = exception2.CompCode;
                        int num9 = exception2.Reason;
                    }
                }
                return reason;
            }
            catch (NmqiException exception3)
            {
                base.TrException(method, exception3, 2);
                int num10 = exception3.CompCode;
                int num11 = exception3.Reason;
            }
            catch (Exception exception4)
            {
                base.TrException(method, exception4);
            }
            finally
            {
                if (flag)
                {
                    base.TrAPI(method, "__________");
                    base.TrAPI(method, "XA_Forget  <<");
                    base.TrAPIInput(method, "Hconn");
                    base.TrData(method, 0, "Reason", BitConverter.GetBytes(reason));
                }
                base.TrExit(method);
            }
            return reason;
        }

        public int XA_Open(ManagedHconn remoteHconn, string xaOpenStr, int rmid, int flags)
        {
            uint method = 0x52d;
            this.TrEntry(method, new object[] { remoteHconn, xaOpenStr, rmid, flags });
            int translength = 0;
            int reason = 0;
            int num5 = 0;
            int num6 = 0;
            int num7 = 0x10;
            bool flag = CommonServices.TraceStatus();
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQAPI mqapi = new MQAPI();
            MQXAInfo info = new MQXAInfo();
            this.InExit();
            try
            {
                MQSession session = remoteHconn.Session;
                num5 = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                translength = (num5 + num7) + info.GetLength();
                if ((reason == 0) && (xaOpenStr == null))
                {
                    reason = -5;
                }
                if ((reason == 0) && ((flags & 0x8000000) != 0))
                {
                    reason = -2;
                }
                if ((reason == 0) && (flags != 0))
                {
                    reason = -5;
                }
                if (reason == 0)
                {
                    reason = info.SetXaInfo("qmname = " + xaOpenStr.Trim());
                }
                if (reason == 0)
                {
                    remoteHconn.EnterCall(this.IsDispatchThread(), false);
                    try
                    {
                        session.CheckIfDisconnected();
                        bool flag2 = true;
                        if (num6 == 0)
                        {
                            if ((remoteHconn.XAState & 2) != 0)
                            {
                                if (remoteHconn.Rmid != rmid)
                                {
                                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x92f, null);
                                    base.TrException(method, ex);
                                    reason = -5;
                                    throw ex;
                                }
                                flag2 = false;
                            }
                            else if ((remoteHconn.XAState & 4) != 0)
                            {
                                NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x92f, null);
                                base.TrException(method, exception2);
                                reason = -6;
                            }
                        }
                        if ((reason != 0) || !flag2)
                        {
                            return reason;
                        }
                        tsh = session.AllocateTSH(0xa3, 0, true, translength);
                        byte[] tshBuffer = tsh.TshBuffer;
                        int offset = tsh.WriteStruct(tshBuffer, 0);
                        mqapi.InitializeForXA(translength, rmid, flags);
                        offset += mqapi.WriteStruct(tshBuffer, offset);
                        offset += info.WriteStruct(tshBuffer, offset);
                        tsh.TshBuffer = tshBuffer;
                        tsh.Offset = offset = 0;
                        if (flag)
                        {
                            base.TrAPI(method, "__________");
                            base.TrAPI(method, "XA_Open  >>");
                            base.TrData(method, 0, "Hconn", BitConverter.GetBytes(remoteHconn.Value));
                            base.TrData(method, 0, "Buffer", offset, tsh.Length, tshBuffer);
                            base.TrAPIOutput(method, "Reason");
                        }
                        session.SendTSH(tsh);
                        tshBuffer = null;
                        rTSH = session.ReceiveTSH(null);
                        switch (rTSH.SegmentType)
                        {
                            case 5:
                            {
                                MQERD mqerd = new MQERD();
                                if (rTSH.Length > num5)
                                {
                                    mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                }
                                throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());
                            }
                            case 0xb3:
                                rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                reason = mqapi.mqapi.ReturnCode;
                                remoteHconn.XAState |= 2;
                                return reason;
                        }
                        string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                        CommonServices.SetValidInserts();
                        CommonServices.ArithInsert1 = rTSH.SegmentType;
                        CommonServices.CommentInsert1 = channelName;
                        CommonServices.CommentInsert2 = "Unexpected flow received during xa_open";
                        base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                        throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                    }
                    finally
                    {
                        try
                        {
                            if (remoteHconn != null)
                            {
                                if (rTSH != null)
                                {
                                    try
                                    {
                                        session.ReleaseReceivedTSH(rTSH);
                                    }
                                    catch (NmqiException)
                                    {
                                    }
                                }
                                remoteHconn.LeaveCall(reason);
                            }
                        }
                        catch (NmqiException exception3)
                        {
                            base.TrException(method, exception3, 1);
                            int compCode = exception3.CompCode;
                            num6 = exception3.Reason;
                        }
                    }
                }
                return reason;
            }
            catch (NmqiException exception4)
            {
                base.TrException(method, exception4, 2);
                int num10 = exception4.CompCode;
                num6 = exception4.Reason;
            }
            catch (Exception exception5)
            {
                base.TrException(method, exception5, 3);
                num6 = 0x893;
            }
            finally
            {
                if (flag)
                {
                    base.TrAPI(method, "__________");
                    base.TrAPI(method, "XA_Open  <<");
                    base.TrAPIInput(method, "Hconn");
                    base.TrData(method, 0, "Reason", BitConverter.GetBytes(reason));
                }
                base.TrExit(method, 1);
            }
            return reason;
        }

        public int XA_Prepare(ManagedHconn remoteHconn, MQXid xid, int rmid, int flags)
        {
            uint method = 0x530;
            this.TrEntry(method, new object[] { remoteHconn, xid, rmid, flags });
            int translength = 0;
            int reason = 0;
            int num5 = 0x10;
            int num6 = 0;
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQAPI mqapi = new MQAPI();
            bool flag = CommonServices.TraceStatus();
            this.InExit();
            try
            {
                MQSession session = remoteHconn.Session;
                num6 = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                translength = (num6 + num5) + xid.GetRoundedLength();
                if ((reason == 0) && (xid == null))
                {
                    return -5;
                }
                if ((reason == 0) && ((flags & 0x8000000) != 0))
                {
                    return -2;
                }
                if ((reason == 0) && (flags != 0))
                {
                    return -5;
                }
                if ((remoteHconn.XAState & 2) == 0)
                {
                    reason = 0x92f;
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x92f, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                remoteHconn.EnterCall(this.IsDispatchThread(), false);
                try
                {
                    session.CheckIfDisconnected();
                    tsh = session.AllocateTSH(0xa5, 0, true, translength);
                    byte[] tshBuffer = tsh.TshBuffer;
                    int offset = tsh.WriteStruct(tshBuffer, 0);
                    mqapi.InitializeForXA(translength, rmid, flags);
                    offset += mqapi.WriteStruct(tshBuffer, offset);
                    offset += xid.WriteStruct(tshBuffer, offset);
                    if (flag)
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "XA_Prepare  >>");
                        base.TrData(method, 0, "Hconn", BitConverter.GetBytes(remoteHconn.Value));
                        base.TrData(method, 0, "Buffer", 0, offset, tshBuffer);
                        base.TrAPIOutput(method, "Reason");
                    }
                    tsh.TshBuffer = tshBuffer;
                    session.SendTSH(tsh);
                    tshBuffer = null;
                    rTSH = session.ReceiveTSH(null);
                    switch (rTSH.SegmentType)
                    {
                        case 5:
                        {
                            MQERD mqerd = new MQERD();
                            if (rTSH.Length > num6)
                            {
                                mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            }
                            throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());
                        }
                        case 0xb5:
                            break;

                        default:
                        {
                            string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                            CommonServices.SetValidInserts();
                            CommonServices.ArithInsert1 = rTSH.SegmentType;
                            CommonServices.CommentInsert1 = channelName;
                            CommonServices.CommentInsert2 = "Unexpected flow received during xa_prepare";
                            base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                            throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                        }
                    }
                    rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                    reason = mqapi.mqapi.ReturnCode;
                }
                finally
                {
                    try
                    {
                        if (remoteHconn != null)
                        {
                            if (rTSH != null)
                            {
                                try
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                                catch (NmqiException exception2)
                                {
                                    base.TrException(method, exception2, 1);
                                }
                            }
                            remoteHconn.LeaveCall(reason);
                        }
                    }
                    catch (NmqiException exception3)
                    {
                        base.TrException(method, exception3, 2);
                        int compCode = exception3.CompCode;
                        int num9 = exception3.Reason;
                    }
                }
                return reason;
            }
            catch (NmqiException exception4)
            {
                base.TrException(method, exception4, 3);
                int num10 = exception4.CompCode;
                int num11 = exception4.Reason;
            }
            catch (Exception exception5)
            {
                base.TrException(method, exception5, 4);
            }
            finally
            {
                if (flag)
                {
                    base.TrAPI(method, "__________");
                    base.TrAPI(method, "XA_Prepare  <<");
                    base.TrAPIInput(method, "Hconn");
                    base.TrData(method, 0, "Reason", BitConverter.GetBytes(reason));
                }
                base.TrExit(method, 3);
            }
            return reason;
        }

        public int XA_Recover(ManagedHconn remoteHconn, MQXid[] xidList, int rmid, int flags)
        {
            uint method = 0x534;
            this.TrEntry(method, new object[] { remoteHconn, xidList, rmid, flags });
            int translength = 0;
            int reason = 0;
            int num5 = 0x10;
            int num6 = 0;
            bool flag = CommonServices.TraceStatus();
            new MQXid();
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQAPI mqapi = new MQAPI();
            this.InExit();
            try
            {
                MQSession session = remoteHconn.Session;
                num6 = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                translength = (num6 + num5) + 4;
                if ((reason == 0) && (xidList == null))
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x92f, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                if ((reason == 0) && ((flags & 0x8000000) != 0))
                {
                    return -2;
                }
                if ((reason == 0) && ((flags & -25165825) != 0))
                {
                    return -2;
                }
                if ((remoteHconn.XAState & 2) == 0)
                {
                    return -5;
                }
                remoteHconn.EnterCall(this.IsDispatchThread(), false);
                try
                {
                    session.CheckIfDisconnected();
                    tsh = session.AllocateTSH(0xa9, 0, true, translength);
                    byte[] tshBuffer = tsh.TshBuffer;
                    int offset = tsh.WriteStruct(tshBuffer, 0);
                    mqapi.InitializeForXA(translength, rmid, flags);
                    offset += mqapi.WriteStruct(tshBuffer, offset);
                    Buffer.BlockCopy(BitConverter.GetBytes(50), 0, tshBuffer, offset, 4);
                    offset += 4;
                    tsh.TshBuffer = tshBuffer;
                    if (flag)
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "XA_Recover  >>");
                        base.TrData(method, 0, "Hconn", BitConverter.GetBytes(remoteHconn.Value));
                        base.TrData(method, 0, "Buffer", 0, offset, tshBuffer);
                        base.TrAPIOutput(method, "Reason");
                    }
                    session.SendTSH(tsh);
                    tshBuffer = null;
                    rTSH = session.ReceiveTSH(null);
                    switch (rTSH.SegmentType)
                    {
                        case 5:
                        {
                            MQERD mqerd = new MQERD();
                            if (rTSH.Length > num6)
                            {
                                mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            }
                            throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());
                        }
                        case 0xb9:
                            break;

                        default:
                        {
                            string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                            CommonServices.SetValidInserts();
                            CommonServices.ArithInsert1 = rTSH.SegmentType;
                            CommonServices.CommentInsert1 = channelName;
                            CommonServices.CommentInsert2 = "Unexpected flow received during xa_recover";
                            base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                            throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                        }
                    }
                    rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                    int num7 = BitConverter.ToInt32(rTSH.TshBuffer, rTSH.Offset);
                    rTSH.Offset += 4;
                    for (int i = 0; i < num7; i++)
                    {
                        xidList[i] = new MQXid();
                        rTSH.Offset = xidList[i].ReadXidFromBytes(rTSH.TshBuffer, rTSH.Offset);
                    }
                    reason = num7;
                }
                finally
                {
                    try
                    {
                        if (remoteHconn != null)
                        {
                            if (rTSH != null)
                            {
                                try
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                                catch (NmqiException)
                                {
                                }
                            }
                            remoteHconn.LeaveCall(reason);
                        }
                    }
                    catch (NmqiException exception2)
                    {
                        base.TrException(method, exception2, 1);
                        int compCode = exception2.CompCode;
                        int num11 = exception2.Reason;
                    }
                }
                return reason;
            }
            catch (NmqiException exception3)
            {
                base.TrException(method, exception3, 2);
                int num12 = exception3.CompCode;
                int num13 = exception3.Reason;
                throw exception3;
            }
            catch (Exception exception4)
            {
                base.TrException(method, exception4);
                throw exception4;
            }
            finally
            {
                if (flag)
                {
                    base.TrAPI(method, "__________");
                    base.TrAPI(method, "XA_Recover  <<");
                    base.TrAPIInput(method, "Hconn");
                    base.TrData(method, 0, "Reason", BitConverter.GetBytes(reason));
                }
                base.TrExit(method);
            }
            return reason;
        }

        public int XA_Rollback(ManagedHconn remoteHconn, MQXid xid, int rmid, int flags)
        {
            uint method = 0x532;
            this.TrEntry(method, new object[] { remoteHconn, xid, rmid, flags });
            int translength = 0;
            int num4 = 0x10;
            int num5 = 0;
            int reason = 0;
            bool flag = CommonServices.TraceStatus();
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQAPI mqapi = new MQAPI();
            this.InExit();
            try
            {
                MQSession session = remoteHconn.Session;
                num5 = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                translength = (num5 + num4) + xid.GetRoundedLength();
                if ((reason == 0) && (xid == null))
                {
                    return -5;
                }
                if ((reason == 0) && ((flags & 0x8000000) != 0))
                {
                    return -2;
                }
                if ((reason == 0) && (flags != 0))
                {
                    return -5;
                }
                if ((remoteHconn.XAState & 2) == 0)
                {
                    reason = 0x92f;
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x92f, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                remoteHconn.EnterCall(this.IsDispatchThread(), false);
                try
                {
                    session.CheckIfDisconnected();
                    tsh = session.AllocateTSH(0xa7, 0, true, translength);
                    byte[] tshBuffer = tsh.TshBuffer;
                    int offset = tsh.WriteStruct(tshBuffer, 0);
                    mqapi.InitializeForXA(translength, rmid, flags);
                    offset += mqapi.WriteStruct(tshBuffer, offset);
                    offset += xid.WriteStruct(tshBuffer, offset);
                    tsh.TshBuffer = tshBuffer;
                    if (flag)
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "XA_Rollback  >>");
                        base.TrData(method, 0, "Hconn", BitConverter.GetBytes(remoteHconn.Value));
                        base.TrData(method, 0, "Buffer", 0, offset, tshBuffer);
                        base.TrAPIOutput(method, "Reason");
                    }
                    session.SendTSH(tsh);
                    tshBuffer = null;
                    rTSH = session.ReceiveTSH(null);
                    switch (rTSH.SegmentType)
                    {
                        case 5:
                        {
                            MQERD mqerd = new MQERD();
                            if (rTSH.Length > num5)
                            {
                                mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            }
                            throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());
                        }
                        case 0xb7:
                            break;

                        default:
                        {
                            string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                            CommonServices.SetValidInserts();
                            CommonServices.ArithInsert1 = rTSH.SegmentType;
                            CommonServices.CommentInsert1 = channelName;
                            CommonServices.CommentInsert2 = "Unexpected flow received during xa_commit";
                            base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                            throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                        }
                    }
                    rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                    reason = mqapi.mqapi.ReturnCode;
                    if ((reason >= 0) && ((remoteHconn.XAState & 4) != 0))
                    {
                        remoteHconn.XAState &= -5;
                    }
                }
                finally
                {
                    try
                    {
                        if (remoteHconn != null)
                        {
                            if (rTSH != null)
                            {
                                try
                                {
                                    session.ReleaseReceivedTSH(rTSH);
                                }
                                catch (NmqiException exception2)
                                {
                                    base.TrException(method, exception2, 1);
                                }
                            }
                            remoteHconn.LeaveCall(reason);
                        }
                    }
                    catch (NmqiException exception3)
                    {
                        base.TrException(method, exception3, 2);
                        int compCode = exception3.CompCode;
                        int num9 = exception3.Reason;
                    }
                }
                return reason;
            }
            catch (NmqiException exception4)
            {
                base.TrException(method, exception4, 3);
                int num10 = exception4.CompCode;
                int num11 = exception4.Reason;
            }
            catch (Exception exception5)
            {
                base.TrException(method, exception5);
            }
            finally
            {
                base.TrExit(method);
            }
            return reason;
        }

        public int XA_Start(ManagedHconn remoteHconn, MQXid xid, int rmid, int flags)
        {
            uint method = 0x52e;
            this.TrEntry(method, new object[] { remoteHconn, xid, rmid, flags });
            int reason = 0;
            int translength = 0;
            bool flag = CommonServices.TraceStatus();
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQAPI mqapi = new MQAPI();
            int num5 = 0;
            int length = mqapi.GetLength();
            this.InExit();
            try
            {
                MQSession session = remoteHconn.Session;
                num5 = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                translength = (num5 + length) + xid.GetRoundedLength();
                if ((reason == 0) && (xid == null))
                {
                    reason = -5;
                }
                if ((reason == 0) && ((flags & 0x8000000) != 0))
                {
                    reason = -2;
                }
                if ((reason == 0) && ((flags & -404750337) != 0))
                {
                    reason = -5;
                }
                if (reason == 0)
                {
                    if ((remoteHconn.XAState & 2) == 0)
                    {
                        reason = 0x92f;
                        NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x92f, null);
                        base.TrException(method, ex);
                        throw ex;
                    }
                    remoteHconn.EnterCall(this.IsDispatchThread(), false);
                    try
                    {
                        session.CheckIfDisconnected();
                        if (reason != 0)
                        {
                            return reason;
                        }
                        tsh = session.AllocateTSH(0xa1, 0, true, translength);
                        byte[] tshBuffer = tsh.TshBuffer;
                        int offset = tsh.WriteStruct(tshBuffer, 0);
                        mqapi.InitializeForXA(translength, rmid, flags);
                        offset += mqapi.WriteStruct(tshBuffer, offset);
                        offset += xid.WriteStruct(tshBuffer, offset);
                        tsh.TshBuffer = tshBuffer;
                        tsh.Offset = offset = 0;
                        if (flag)
                        {
                            base.TrAPI(method, "__________");
                            base.TrAPI(method, "XA_Start  >>");
                            base.TrData(method, 0, "Hconn", BitConverter.GetBytes(remoteHconn.Value));
                            base.TrData(method, 0, "Buffer", offset, tsh.Length, tshBuffer);
                            base.TrAPIOutput(method, "Reason");
                        }
                        session.SendTSH(tsh);
                        tshBuffer = null;
                        rTSH = session.ReceiveTSH(null);
                        switch (rTSH.SegmentType)
                        {
                            case 5:
                            {
                                MQERD mqerd = new MQERD();
                                if (rTSH.Length > num5)
                                {
                                    mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                }
                                throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());
                            }
                            case 0xb1:
                                rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                reason = mqapi.mqapi.ReturnCode;
                                if (reason == 0)
                                {
                                    remoteHconn.XAState |= 4;
                                }
                                return reason;
                        }
                        string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                        CommonServices.SetValidInserts();
                        CommonServices.ArithInsert1 = rTSH.SegmentType;
                        CommonServices.CommentInsert1 = channelName;
                        CommonServices.CommentInsert2 = "Unexpected flow received during xa_start";
                        base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                        throw MQERD.ErrorException(0x20009213, rTSH.SegmentType, channelName);
                    }
                    finally
                    {
                        try
                        {
                            if (remoteHconn != null)
                            {
                                if (rTSH != null)
                                {
                                    try
                                    {
                                        session.ReleaseReceivedTSH(rTSH);
                                    }
                                    catch (NmqiException)
                                    {
                                    }
                                }
                                remoteHconn.LeaveCall(reason);
                            }
                        }
                        catch (NmqiException exception2)
                        {
                            base.TrException(method, exception2, 1);
                            int compCode = exception2.CompCode;
                            int num9 = exception2.Reason;
                        }
                    }
                }
                return reason;
            }
            catch (NmqiException exception3)
            {
                base.TrException(method, exception3, 2);
                int num10 = exception3.CompCode;
                int num11 = exception3.Reason;
            }
            finally
            {
                if (flag)
                {
                    base.TrAPI(method, "__________");
                    base.TrAPI(method, "XA_Start  <<");
                    base.TrAPIInput(method, "Hconn");
                    base.TrData(method, 0, "Reason", BitConverter.GetBytes(reason));
                }
                base.TrExit(method, 1);
            }
            return reason;
        }

        public void zstMQGET(Hconn hConn, Hobj hObj, ref MQMessageDescriptor md, ref MQGetMessageOptions gmo, int bufferLength, byte[] buffer, out int dataLength, MQLPIGetOpts lpiGetOpts, out int compCode, out int reason)
        {
            uint method = 0x5bd;
            this.TrEntry(method, new object[] { hConn, hObj, md, gmo, bufferLength, buffer, "dataLength : out", lpiGetOpts, "compCode : out", "reason : out" });
            compCode = 0;
            reason = 0;
            try
            {
                ManagedHconn managedHconn = this.GetManagedHconn(hConn);
                ManagedHobj managedHobj = this.GetManagedHobj(hObj);
                this.zstMQGET(managedHconn, managedHobj, ref md, ref gmo, bufferLength, buffer, out dataLength, lpiGetOpts, out compCode, out reason);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal MQSession zstMQGET(ManagedHconn remoteHconn, Hobj hObj, ref MQMessageDescriptor mqmd, ref MQGetMessageOptions mqgmo, int bufferLength, byte[] buffer, out int dataLength, MQLPIGetOpts lpiGetOpts, out int compCode, out int reason)
        {
            int num9;
            int num11;
            uint method = 1;
            this.TrEntry(method, new object[] { remoteHconn, hObj, mqmd, mqgmo, bufferLength, buffer, "dataLength : out", lpiGetOpts, "compCode : out", "reason : out" });
            int versionLength = mqmd.GetVersionLength();
            int num3 = mqgmo.GetVersionLength();
            int callLength = 0;
            int dstOffset = 0;
            int num12 = 0x10;
            int num13 = 0;
            MQERD mqerd = null;
            MQTSH tsh = null;
            MQTSH rTSH = null;
            MQAPI mqapi = new MQAPI();
            MQSession session = null;
            ManagedHobj remoteHobj = null;
            bool flag = CommonServices.TraceStatus();
            dataLength = 0;
            compCode = 2;
            reason = 0x7e2;
            int offset = num9 = 0;
            int num10 = num11 = 0;
            this.InExit();
            try
            {
                session = remoteHconn.Session;
                if (remoteHconn.IsQuiescing() && ((mqgmo.Options & 0x2000) != 0))
                {
                    base.TrText(method, "This hconn is quiescing, notify the caller about it");
                    base.throwNewMQException(2, 0x89a);
                }
                num13 = session.IsMultiplexingEnabled ? 0x24 : 0x1c;
                callLength = ((((num13 + num12) + versionLength) + num3) + bufferLength) + 4;
                if (remoteHconn.IsReconnectable)
                {
                    if ((mqgmo.Options & 0x8000) != 0)
                    {
                        NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x9f3, null);
                        base.TrException(method, ex);
                        throw ex;
                    }
                    if (remoteHconn.IsXAEnabled)
                    {
                        NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x7dc, null);
                        base.TrException(method, exception2);
                        throw exception2;
                    }
                }
                remoteHconn.CheckUOWNotMixed(mqgmo.Options, 2);
                this.TransactionCheck(remoteHconn, mqgmo.Options, 2);
                if ((hObj == null) || !(hObj is ManagedHobj))
                {
                    NmqiException exception3 = new NmqiException(base.env, -1, null, 2, 0x7e3, null);
                    base.TrException(method, exception3);
                    throw exception3;
                }
                remoteHobj = (ManagedHobj) hObj;
                if (bufferLength < 0)
                {
                    NmqiException exception4 = new NmqiException(base.env, -1, null, 2, 0x7d5, null);
                    base.TrException(method, exception4);
                    throw exception4;
                }
                if (bufferLength > buffer.Length)
                {
                    NmqiException exception5 = new NmqiException(base.env, -1, null, 2, 0x7d5, null);
                    base.TrException(method, exception5);
                    throw exception5;
                }
                if (bufferLength > session.MaximumMessageLength)
                {
                    compCode = 2;
                    reason = 0x7da;
                    return session;
                }
                this.CheckGetOptions(mqgmo, remoteHobj);
                SpiGetOptions options = new SpiGetOptions();
                if ((mqgmo.Options & 0x4000) != 0)
                {
                    options.Options = 0x80;
                }
                MQProxyQueue proxyQueue = null;
                if (hObj is ManagedHobj)
                {
                    proxyQueue = ((ManagedHobj) hObj).ProxyQueue;
                }
                remoteHconn.EnterCall(this.IsDispatchThread(), false);
                try
                {
                    session.CheckIfDisconnected();
                    if ((proxyQueue != null) && (!remoteHconn.ParentConnection.IsMultiplexSyncGetCapable() || proxyQueue.IsStreamingRequested()))
                    {
                        proxyQueue.ProxyMQGET(mqmd, mqgmo, bufferLength, buffer, ref dataLength, lpiGetOpts, ref compCode, ref reason);
                        return session;
                    }
                    tsh = session.AllocateTSH(0x85, 0, true, callLength - bufferLength);
                    byte[] tshBuffer = tsh.TshBuffer;
                    int num5 = tsh.WriteStruct(tshBuffer, 0);
                    mqapi.Initialize(callLength, hObj.Handle);
                    num5 += mqapi.WriteStruct(tshBuffer, num5);
                    offset = num5;
                    num10 = num9 = num5 += mqmd.WriteStruct(tshBuffer, num5);
                    num11 = num5 += mqgmo.WriteStruct(tshBuffer, num5);
                    BitConverter.GetBytes(bufferLength).CopyTo(tshBuffer, num5);
                    num5 += 4;
                    if (flag)
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "MQGET >>");
                        base.TrData(method, 0, "Hconn", BitConverter.GetBytes(remoteHconn.Value));
                        base.TrData(method, 0, "Hobj", BitConverter.GetBytes(hObj.Handle));
                        base.TrData(method, 0, "Msgdesc", offset, num9 - offset, tshBuffer);
                        base.TrData(method, 0, "Getmsgopts", num10, num11 - num10, tshBuffer);
                        base.TrData(method, 0, "Bufferlength", 0, -1, BitConverter.GetBytes(bufferLength));
                        base.TrAPIOutput(method, "Buffer");
                        base.TrAPIOutput(method, "Datalength");
                        base.TrAPIOutput(method, "CompCode");
                        base.TrAPIOutput(method, "Reason");
                        offset = num9 = 0;
                        num10 = num11 = 0;
                    }
                    tsh.TshBuffer = tshBuffer;
                    num5 = 0;
                    session.SendTSH(tsh);
                    do
                    {
                        try
                        {
                            rTSH = session.ReceiveTSH(null);
                            switch (rTSH.SegmentType)
                            {
                                case 5:
                                    mqerd = new MQERD();
                                    if (rTSH.Length > num13)
                                    {
                                        mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                    }
                                    throw mqerd.ErrorException(session.ParentConnection.NegotiatedChannel.ChannelName.Trim());

                                case 0x95:
                                    break;

                                default:
                                {
                                    string channelName = session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                                    CommonServices.SetValidInserts();
                                    CommonServices.ArithInsert1 = rTSH.SegmentType;
                                    CommonServices.CommentInsert1 = channelName;
                                    CommonServices.CommentInsert2 = "Unexpected flow received during MQGET";
                                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009510, 0);
                                    throw MQERD.ErrorException(0x20009510, rTSH.SegmentType, channelName);
                                }
                            }
                            if ((rTSH.ControlFlags1 & 0x10) != 0)
                            {
                                rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                compCode = mqapi.mqapi.CompCode;
                                reason = mqapi.mqapi.Reason;
                                if (compCode == 2)
                                {
                                    return session;
                                }
                                offset = num5;
                                num10 = num9 = rTSH.Offset = mqmd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                num11 = rTSH.Offset = mqgmo.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                dataLength = BitConverter.ToInt32(rTSH.TshBuffer, rTSH.Offset);
                                rTSH.Offset += 4;
                            }
                            int count = rTSH.TransLength - rTSH.Offset;
                            if (count > buffer.Length)
                            {
                                count = buffer.Length;
                            }
                            Buffer.BlockCopy(rTSH.TshBuffer, rTSH.Offset, buffer, dstOffset, count);
                            dstOffset += count;
                        }
                        finally
                        {
                            session.ReleaseReceivedTSH(rTSH);
                        }
                    }
                    while ((rTSH.ControlFlags1 & 0x20) == 0);
                    return session;
                }
                finally
                {
                    if (flag)
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "MQGET <<");
                        base.TrAPIInput(method, "Hconn");
                        base.TrAPIInput(method, "Hobj");
                        base.TrData(method, 0, "Bufferlength", BitConverter.GetBytes(bufferLength));
                        if (bufferLength > dataLength)
                        {
                            base.TrAPIBuffer(method, buffer, dataLength);
                        }
                        else
                        {
                            base.TrAPIBuffer(method, buffer, bufferLength);
                        }
                        base.TrData(method, 0, "Datalength", BitConverter.GetBytes(dataLength));
                        base.TrAPICCRC(method, compCode, reason);
                    }
                    if (remoteHconn != null)
                    {
                        remoteHconn.LeaveCall(reason);
                    }
                }
                return session;
            }
            catch (NmqiException exception6)
            {
                base.TrException(method, exception6, 1);
                compCode = exception6.CompCode;
                reason = exception6.Reason;
            }
            catch (MQException exception7)
            {
                base.TrException(method, exception7, 2);
                compCode = exception7.CompCode;
                reason = exception7.Reason;
            }
            catch (Exception exception8)
            {
                base.TrException(method, exception8, 3);
                compCode = 2;
                reason = 0x893;
            }
            finally
            {
                base.TrExit(method);
            }
            return session;
        }

        internal MQCommsBufferPool CommsBufferPool
        {
            get
            {
                return commsBufferPool;
            }
        }

        internal MQFAPConnectionPool ConnectionFactory
        {
            get
            {
                return this.connectionFactory;
            }
        }

        public MQManagedReconnectableThread ReconnectThread
        {
            get
            {
                try
                {
                    this.reconnectThreadLock.Acquire();
                    if (this.reconnectThread == null)
                    {
                        this.reconnectThread = new MQManagedReconnectableThread(base.env, this);
                        Thread thread = new Thread(new ThreadStart(this.reconnectThread.Run));
                        thread.IsBackground = true;
                        thread.Start();
                    }
                }
                finally
                {
                    this.reconnectThreadLock.Release();
                }
                return this.reconnectThread;
            }
        }
    }
}

