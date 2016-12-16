namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    public class MQSession : NmqiObject
    {
        private byte[] accountingToken;
        internal MQAPI apiSend;
        private volatile Exception asyncFailure;
        private Lock asyncTshLock;
        private Queue<MQTSH> asyncTshQueue;
        private MQRequestEntry cachedExchangeRequest;
        private bool connectionBroken;
        private byte[] connectionId;
        private string connectionIdString;
        private int conversationId;
        private bool disconnected;
        private bool endRequested;
        private MQRequestEntry exchangeRequests;
        internal MQChannelExitHandler exits;
        internal bool exitsActive;
        private ManagedHconn hconn;
        private QueueManagerInfo info;
        private MQSSLConfigOptions mqsco;
        private string originalQueueManagerName;
        private MQFAPConnection parentConnection;
        private Lock rcvExitLock;
        private Lock rcvExitLockCheck;
        private MQTSH rcvExitLockTSH;
        private bool reconnectionBegun;
        private Lock requestEntryMutex;
        private int rmid;
        private int rmtReqEntMaxPollTime;
        public const string sccsid = "%Z% %W% %I% %E% %U%";
        private int sessionMultiplexed;
        internal MQSPIQueryOut SpiQueryOut;
        private bool terminatedByExit;
        private int traceIdentifier;
        internal MQTSH tshNotify;
        internal MQTSH tshReqMsgs;
        internal MQTSH tshSend;
        private string userIdentifier;
        private bool wasReconnectable;
        private int xaState;

        internal MQSession(NmqiEnvironment env, MQFAPConnection connection) : base(env)
        {
            this.rcvExitLockCheck = new Lock();
            this.rcvExitLock = new Lock();
            this.asyncTshLock = new Lock();
            this.asyncTshQueue = new Queue<MQTSH>();
            this.rmid = -1;
            this.requestEntryMutex = new Lock();
            this.rmtReqEntMaxPollTime = 0x7d0;
            this.sessionMultiplexed = -1;
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { env, connection });
            MQClientCfg cfg = env.Cfg;
            this.parentConnection = connection;
        }

        internal MQAPI AllocateAPI(int maxlen, int handle)
        {
            uint method = 0x627;
            this.TrEntry(method, new object[] { maxlen, handle });
            try
            {
                if (this.apiSend == null)
                {
                    this.apiSend = new MQAPI();
                    base.TrText(method, "Allocated a fresh TSH for this call");
                }
                this.apiSend.Initialize(maxlen, handle);
            }
            finally
            {
                base.TrExit(method, this.apiSend);
            }
            return this.apiSend;
        }

        private MQRequestEntry AllocateRequestEntry()
        {
            MQRequestEntry entry3;
            uint method = 0x61f;
            this.TrEntry(method);
            try
            {
                lock (this.requestEntryMutex)
                {
                    MQRequestEntry cachedExchangeRequest;
                    if (this.cachedExchangeRequest != null)
                    {
                        cachedExchangeRequest = this.cachedExchangeRequest;
                        this.cachedExchangeRequest = null;
                        cachedExchangeRequest.Reply = null;
                    }
                    else
                    {
                        cachedExchangeRequest = new MQRequestEntry();
                    }
                    int num2 = 3;
                    for (MQRequestEntry entry2 = this.exchangeRequests; entry2 != null; entry2 = entry2.Next)
                    {
                        if (entry2.RequestID >= num2)
                        {
                            num2 = entry2.RequestID + 2;
                        }
                    }
                    cachedExchangeRequest.RequestID = num2;
                    if (this.exchangeRequests != null)
                    {
                        this.exchangeRequests.Previous = cachedExchangeRequest;
                    }
                    cachedExchangeRequest.Next = this.exchangeRequests;
                    cachedExchangeRequest.Previous = null;
                    this.exchangeRequests = cachedExchangeRequest;
                    base.TrData(method, 0, "Current RequestId allocated = ", BitConverter.GetBytes(cachedExchangeRequest.RequestID));
                    entry3 = cachedExchangeRequest;
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return entry3;
        }

        internal MQTSH AllocateTSH(int segmentType, int requestId, MQTSH tshP)
        {
            uint method = 0x616;
            this.TrEntry(method, new object[] { segmentType, requestId, tshP });
            MQTSH tshHeaderP = tshP;
            try
            {
                MQFAPConnection connection = this.Connection;
                if (this.parentConnection.IsMultiplexingEnabled)
                {
                    tshHeaderP = connection.AllocateTSH(1, segmentType, tshHeaderP);
                    tshHeaderP.SetConversationId(this.conversationId);
                    tshHeaderP.SetRequestId(requestId);
                    return tshHeaderP;
                }
                tshHeaderP = connection.AllocateTSH(0, segmentType, tshHeaderP);
            }
            finally
            {
                base.TrExit(method, tshHeaderP);
            }
            return tshHeaderP;
        }

        internal MQTSH AllocateTSH(byte segmentType, int requestId, bool initialize, int translength)
        {
            uint method = 0x617;
            this.TrEntry(method, new object[] { segmentType, requestId, initialize, translength });
            MQTSH tshHeaderP = null;
            try
            {
                MQFAPConnection connection = this.Connection;
                if (this.parentConnection.IsMultiplexingEnabled)
                {
                    tshHeaderP = connection.AllocateTSH(1, segmentType, tshHeaderP, translength);
                    tshHeaderP.SetConversationId(this.conversationId);
                    tshHeaderP.SetRequestId(requestId);
                    return tshHeaderP;
                }
                tshHeaderP = connection.AllocateTSH(0, segmentType, tshHeaderP, translength);
            }
            finally
            {
                base.TrExit(method, tshHeaderP);
            }
            return tshHeaderP;
        }

        internal MQTSH AllocateTSH(int tshType, byte segmentType, int requestId, bool initialize, int translength)
        {
            uint method = 0x618;
            this.TrEntry(method, new object[] { tshType, segmentType, requestId, initialize, translength });
            MQTSH tshHeaderP = null;
            try
            {
                MQFAPConnection connection = this.Connection;
                if (this.parentConnection.IsMultiplexingEnabled)
                {
                    tshHeaderP = connection.AllocateTSH(tshType, segmentType, tshHeaderP, translength);
                    tshHeaderP.SetConversationId(this.conversationId);
                    tshHeaderP.SetRequestId(requestId);
                    return tshHeaderP;
                }
                tshHeaderP = connection.AllocateTSH(0, segmentType, tshHeaderP, translength);
            }
            finally
            {
                base.TrExit(method, tshHeaderP);
            }
            return tshHeaderP;
        }

        internal MQTSH AllocateTshForNotification(int size)
        {
            uint method = 0x626;
            this.TrEntry(method, new object[] { size });
            try
            {
                if (this.tshNotify == null)
                {
                    this.tshNotify = this.AllocateTSH(15, 1, true, size);
                    base.TrText(method, "Allocated a fresh TSH for this call");
                }
                else
                {
                    if (size > this.tshNotify.ParentBuffer.Capacity)
                    {
                        this.tshNotify.ParentBuffer.Free();
                        this.tshNotify.ParentBuffer = null;
                        IMQCommsBuffer buffer = this.parentConnection.CommsBufferPool.AllocateBuffer(size);
                        this.tshNotify.ParentBuffer = buffer;
                        base.TrText(method, "Allocated cached TSH with new buffer for this call");
                    }
                    this.tshNotify.Offset = 0;
                    this.tshNotify.SetRequestId(1);
                    this.tshNotify.SetTransLength(size);
                    base.TrText(method, "Allocated a cached TSH for this call");
                }
            }
            finally
            {
                base.TrExit(method, this.tshNotify);
            }
            return this.tshNotify;
        }

        internal MQTSH AllocateTshForPQReqMsgs(int size)
        {
            uint method = 0x625;
            this.TrEntry(method, new object[] { size });
            try
            {
                if (this.tshReqMsgs == null)
                {
                    this.tshReqMsgs = this.AllocateTSH(14, 1, true, size);
                    base.TrText(method, "Allocated a fresh TSH for this call");
                }
                else
                {
                    if (size > this.tshReqMsgs.ParentBuffer.Capacity)
                    {
                        this.tshReqMsgs.ParentBuffer.Free();
                        this.tshReqMsgs.ParentBuffer = null;
                        IMQCommsBuffer buffer = this.parentConnection.CommsBufferPool.AllocateBuffer(size);
                        this.tshReqMsgs.ParentBuffer = buffer;
                        base.TrText(method, "Allocated cached TSH with new buffer for this call");
                    }
                    this.tshReqMsgs.Offset = 0;
                    this.tshReqMsgs.SetTransLength(size);
                    base.TrText(method, "Allocated a cached TSH for this call");
                }
            }
            finally
            {
                base.TrExit(method, this.tshReqMsgs);
            }
            return this.tshReqMsgs;
        }

        internal MQTSH AllocateTshForPut(int size)
        {
            uint method = 0x624;
            this.TrEntry(method, new object[] { size });
            try
            {
                if (this.tshSend == null)
                {
                    this.tshSend = this.AllocateTSH(0x86, 0, true, size);
                    base.TrText(method, "Allocated a fresh TSH for this call");
                }
                else
                {
                    if (size > this.tshSend.ParentBuffer.Capacity)
                    {
                        this.tshSend.ParentBuffer.Free();
                        this.tshSend.ParentBuffer = null;
                        IMQCommsBuffer buffer = this.parentConnection.CommsBufferPool.AllocateBuffer(size);
                        this.tshSend.ParentBuffer = buffer;
                        base.TrText(method, "Allocated cached TSH with new buffer for this call");
                    }
                    base.TrText(method, "Allocated a cached TSH for this call");
                    this.tshSend.SetTransLength(size);
                }
            }
            finally
            {
                base.TrExit(method, this.tshSend);
            }
            return this.tshSend;
        }

        internal void AsyncFailureNotify(Exception e)
        {
            uint method = 0x61b;
            this.TrEntry(method, new object[] { e });
            try
            {
                base.TrText(method, "We received an asyncfailure exception");
                MQException ex = null;
                if (e is MQException)
                {
                    ex = (MQException) e;
                }
                else
                {
                    ex = new MQException(2, 0x7d9, e);
                }
                base.TrException(method, ex);
                if ((ex.ReasonCode == 0x89a) || (ex.ReasonCode == 0x871))
                {
                    base.TrText(method, "This is a " + ex.Reason + " exception, setting the flag on hconn");
                    this.Hconn.SetQuiescing();
                }
                else
                {
                    base.TrText(method, "Pulsing all the data waiters");
                    lock (this.asyncTshLock)
                    {
                        this.asyncFailure = ex;
                        this.asyncTshLock.PulseAll();
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void CheckIfDisconnected()
        {
            uint method = 0x612;
            this.TrEntry(method);
            try
            {
                ManagedHconn reconnectableParent = this.ReconnectableParent;
                if (this.disconnected && ((reconnectableParent == null) || ((reconnectableParent != null) && reconnectableParent.HasFailed())))
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x7d9, this.asyncFailure);
                    base.TrException(method, ex);
                    throw ex;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void DeliverException(NmqiException e, int flag, int _event)
        {
            uint method = 0x62d;
            this.TrEntry(method, new object[] { e, flag, _event });
            try
            {
                if (this.hconn != null)
                {
                    this.hconn.DeliverException(e, flag, _event);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void DeliverExchangeReply(int requestId, MQTSH tsh)
        {
            uint method = 0x61e;
            this.TrEntry(method, new object[] { requestId, tsh });
            try
            {
                tsh = this.ProcessReceivedData(tsh);
                lock (this.requestEntryMutex)
                {
                    MQRequestEntry exchangeRequests = this.exchangeRequests;
                    while ((exchangeRequests != null) && (exchangeRequests.RequestID != requestId))
                    {
                        exchangeRequests = exchangeRequests.Next;
                    }
                    if (exchangeRequests == null)
                    {
                        CommonServices.SetValidInserts();
                        CommonServices.ArithInsert1 = (uint) requestId;
                        CommonServices.ArithInsert2 = tsh.SegmentType;
                        CommonServices.CommentInsert1 = "Unexpected flow received in DeliverExchangeReply";
                        CommonServices.CommentInsert2 = "Cached RequestEntry is null for given RequestId";
                        base.FFST("%Z% %W% %I% %E% %U%", "%C%", method, 1, 0x20009546, 0);
                        NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x893, null);
                        base.TrException(method, ex);
                        throw ex;
                    }
                    lock (exchangeRequests)
                    {
                        exchangeRequests.Reply = tsh;
                        Monitor.Pulse(exchangeRequests);
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void DeliverTSH(MQTSH rTSH)
        {
            uint method = 0x619;
            this.TrEntry(method, new object[] { rTSH });
            try
            {
                lock (this.asyncTshLock)
                {
                    this.asyncTshQueue.Enqueue(rTSH);
                    Monitor.Pulse(this.asyncTshLock);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void DisableReconnect()
        {
            uint method = 0x62c;
            this.TrEntry(method);
            try
            {
                this.hconn.EligibleForReconnect(this, false);
            }
            catch (MQException)
            {
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void Disconnect()
        {
            uint method = 0x629;
            this.TrEntry(method);
            try
            {
                if (!this.disconnected && (this.parentConnection != null))
                {
                    MQChannelDefinition negotiatedChannel = this.parentConnection.NegotiatedChannel;
                    int fapLevel = this.parentConnection.FapLevel;
                    if (this.exitsActive)
                    {
                        this.exits.TermExits();
                    }
                    this.parentConnection.RemoveSession(this.conversationId, true);
                    this.ReleaseOwnedTsh();
                    this.SetDisconnected();
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal MQTSH ExchangeTSH(MQTSH requestTsh)
        {
            uint method = 0x622;
            this.TrEntry(method, new object[] { requestTsh });
            MQTSH result = null;
            try
            {
                MQRequestEntry entry2;
                if (this.disconnected)
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x7d9, this.asyncFailure);
                    base.TrException(method, ex);
                    this.connectionBroken = true;
                    this.hconn.RaiseEvent(0x7d9);
                }
                MQRequestEntry entry = this.AllocateRequestEntry();
                requestTsh.SetRequestId(entry.RequestID);
                byte[] bytes = BitConverter.GetBytes(IPAddress.NetworkToHostOrder(entry.RequestID));
                for (int i = 12; i < 0x10; i++)
                {
                    requestTsh.TshBuffer[i] = bytes[i - 12];
                }
                requestTsh.ControlFlags1 = (byte) (requestTsh.ControlFlags1 | 1);
                this.SendData(requestTsh.TshBuffer, 0, requestTsh.Length, requestTsh.SegmentType, 1);
                Monitor.Enter(entry2 = entry);
                try
                {
                    while (entry.Reply == null)
                    {
                        Monitor.Wait(entry, this.rmtReqEntMaxPollTime, true);
                        if ((this.asyncFailure != null) || !this.Connection.IsConnected)
                        {
                            NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x7d9, this.asyncFailure);
                            base.TrException(method, exception2);
                            throw exception2;
                        }
                    }
                }
                catch (ThreadInterruptedException exception3)
                {
                    string str = "Interrupted while waiting for exchange reply";
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = (uint) this.conversationId;
                    CommonServices.ArithInsert2 = (uint) entry.RequestID;
                    CommonServices.CommentInsert1 = str;
                    base.FFST("%Z% %W% %I% %E% %U%", "%C%", method, 1, 0x8e1, 0);
                    NmqiException exception4 = new NmqiException(base.env, -1, null, 2, 0x893, exception3);
                    base.TrException(method, exception4);
                    throw exception4;
                }
                finally
                {
                    Monitor.Exit(entry2);
                }
                result = entry.Reply;
                this.ReleaseRequestEntry(entry);
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        internal Exception GetAsyncFailure()
        {
            return this.asyncFailure;
        }

        internal QueueManagerInfo GetQmgrInfo()
        {
            return this.info;
        }

        internal void InitOAMFlow(MQConnectionSecurityParameters mqcsp)
        {
            uint method = 0x62b;
            this.TrEntry(method, new object[] { mqcsp });
            try
            {
                this.parentConnection.InitOAMFlow(mqcsp, this);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void InitSendReceiveExits(bool firstConv)
        {
            uint method = 0x628;
            this.TrEntry(method, new object[] { firstConv });
            try
            {
                MQFAPConnection conn = this.Connection;
                MQChannelDefinition clientConn = conn.ClientConn;
                MQChannelDefinition negotiatedChannel = conn.NegotiatedChannel;
                if (this.exits == null)
                {
                    this.exits = new MQChannelExitHandler(conn);
                }
                if ((clientConn.SendExit != null) && (clientConn.SendExit != ""))
                {
                    this.exits.LoadExits(13);
                    this.exits.InitializeExits(13, firstConv);
                    clientConn.SendExitsDefined = 1;
                    negotiatedChannel.SendExitsDefined = 1;
                    this.exitsActive = true;
                }
                if ((clientConn.ReceiveExit != null) && (clientConn.ReceiveExit != ""))
                {
                    this.exits.LoadExits(14);
                    this.exits.InitializeExits(14, firstConv);
                    clientConn.ReceiveExitsDefined = 1;
                    negotiatedChannel.ReceiveExitsDefined = 1;
                    this.exitsActive = true;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void LoadInfo()
        {
            uint method = 0x630;
            this.TrEntry(method);
            try
            {
                if (this.info == null)
                {
                    this.info = NmqiTools.GetQueueManagerInfo(base.env, this.MQFap, this.hconn);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void LockRcvExitForTSH(MQTSH rTSH)
        {
            uint method = 0x614;
            this.TrEntry(method, new object[] { rTSH });
            try
            {
                while (this.rcvExitLockTSH != null)
                {
                    try
                    {
                        this.rcvExitLock.Acquire();
                        continue;
                    }
                    catch (ThreadInterruptedException)
                    {
                        continue;
                    }
                }
                this.rcvExitLockTSH = rTSH;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal MQTSH ProcessReceivedData(MQTSH tsh)
        {
            uint method = 0x613;
            this.TrEntry(method, new object[] { tsh });
            try
            {
                MQFAPConnection connection = this.Connection;
                if ((!this.exitsActive || (connection.NegotiatedChannel.ReceiveExitsDefined == 0)) || (this.exits == null))
                {
                    return tsh;
                }
                try
                {
                    try
                    {
                        byte[] tshBuffer = tsh.TshBuffer;
                        int offset = 0;
                        int length = tsh.Length;
                        int maxlength = length;
                        this.LockRcvExitForTSH(tsh);
                        byte[] buf = this.exits.ProcessReceiveExits(ref tshBuffer, ref offset, ref length, ref maxlength);
                        base.TrData(method, 2, "Data after ReceiveExit call", 0, length, buf);
                        tsh.Offset = tsh.ReadStruct(buf, offset);
                        tsh.TshBuffer = buf;
                        tsh.Length = length;
                    }
                    catch (NmqiException exception)
                    {
                        base.TrException(method, exception);
                        if ((exception.Reason == 0x80b) || (exception.Reason == 0x80f))
                        {
                            this.Disconnect();
                        }
                        return tsh;
                    }
                    catch (MQException exception2)
                    {
                        base.TrException(method, exception2);
                        if ((exception2.Reason == 0x80b) || (exception2.Reason == 0x80f))
                        {
                            this.Disconnect();
                        }
                        return tsh;
                    }
                    return tsh;
                }
                finally
                {
                    this.UnlockRcvExitForTSH(tsh);
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return tsh;
        }

        public void QmQuiescing()
        {
            uint method = 0x62e;
            this.TrEntry(method);
            try
            {
                this.hconn.QmQuiescing();
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal MQTSH ReceiveAsyncTSH()
        {
            uint method = 0x61a;
            this.TrEntry(method);
            MQTSH result = null;
            try
            {
                lock (this.asyncTshLock)
                {
                    while ((this.asyncTshQueue.Count == 0) && (this.asyncFailure == null))
                    {
                        try
                        {
                            long currentTimeInMs = NmqiTools.GetCurrentTimeInMs();
                            int millisecondsTimeout = 10;
                            Monitor.Wait(this.asyncTshLock, millisecondsTimeout, true);
                            if ((NmqiTools.GetCurrentTimeInMs() >= (currentTimeInMs + millisecondsTimeout)) && !this.Connection.IsConnected)
                            {
                                NmqiException ex = new NmqiException(base.env, 0x23fd, null, 2, 0x7d9, null);
                                base.TrException(method, ex);
                                throw ex;
                            }
                            continue;
                        }
                        catch (ThreadInterruptedException exception2)
                        {
                            base.TrException(method, exception2);
                            continue;
                        }
                    }
                    if (this.asyncFailure != null)
                    {
                        this.connectionBroken = true;
                        this.hconn.RaiseEvent(0x7d9);
                        NmqiException exception3 = new NmqiException(base.env, -1, null, 2, 0x7d9, this.asyncFailure);
                        base.TrException(method, exception3);
                        throw exception3;
                    }
                }
                result = this.asyncTshQueue.Dequeue();
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        internal MQTSH ReceiveTSH(MQTSH partialTSH)
        {
            uint method = 0x61d;
            this.TrEntry(method, new object[] { partialTSH });
            MQTSH tsh = null;
            try
            {
                MQFAPConnection connection = this.Connection;
                if (connection.IsMultiplexingEnabled)
                {
                    tsh = this.ReceiveAsyncTSH();
                }
                else
                {
                    tsh = connection.ReceiveTSH(partialTSH);
                }
                this.ProcessReceivedData(tsh);
            }
            finally
            {
                base.TrExit(method, tsh);
            }
            return tsh;
        }

        internal void ReleaseOwnedTsh()
        {
            try
            {
                if (this.tshSend != null)
                {
                    this.Connection.ReleaseReceivedTSH(this.tshSend);
                    this.tshSend = null;
                }
                if (this.tshReqMsgs != null)
                {
                    this.Connection.ReleaseReceivedTSH(this.tshReqMsgs);
                    this.tshReqMsgs = null;
                }
                if (this.tshNotify != null)
                {
                    this.Connection.ReleaseReceivedTSH(this.tshNotify);
                    this.tshNotify = null;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal void ReleaseReceivedTSH(MQTSH rTSH)
        {
            uint method = 0x623;
            this.TrEntry(method, new object[] { rTSH });
            try
            {
                MQFAPConnection connection = null;
                try
                {
                    connection = this.Connection;
                }
                finally
                {
                    this.UnlockRcvExitForTSH(rTSH);
                }
                if (connection != null)
                {
                    connection.ReleaseReceivedTSH(rTSH);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void ReleaseRequestEntry(MQRequestEntry entry)
        {
            uint method = 0x620;
            this.TrEntry(method, new object[] { entry });
            try
            {
                lock (this.requestEntryMutex)
                {
                    base.TrData(method, 0, "Current RequestId being released = ", BitConverter.GetBytes(entry.RequestID));
                    MQRequestEntry previous = entry.Previous;
                    MQRequestEntry next = entry.Next;
                    if (previous == null)
                    {
                        this.exchangeRequests = next;
                    }
                    else
                    {
                        previous.Next = next;
                    }
                    if (next != null)
                    {
                        next.Previous = previous;
                    }
                    if (this.cachedExchangeRequest == null)
                    {
                        this.cachedExchangeRequest = entry;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void SendData(byte[] bytes, int offset, int length, byte segmentType, int tshType)
        {
            uint method = 0xb1;
            this.TrEntry(method, new object[] { bytes, offset, length });
            MQFAPConnection connection = this.Connection;
            byte[] buffer = null;
            int maxlength = length;
            try
            {
                if (this.IsEndRequested || (this.asyncFailure != null))
                {
                    NmqiException exception = new NmqiException(base.env, -1, null, 2, 0x7d9, this.asyncFailure);
                    throw exception;
                }
                base.TrData(method, 0, "Data before Processed by SendExits(if no exits are defined, this is final)", bytes);
                if ((this.exitsActive && (this.exits != null)) && this.exits.sendExitDefined)
                {
                    buffer = this.exits.ProcessSendExits(ref bytes, ref offset, ref length, ref maxlength);
                    base.TrData(method, 0, "Data after processed by SendExits", bytes);
                }
                else
                {
                    buffer = bytes;
                }
                try
                {
                    this.parentConnection.RequestSendLock();
                    connection.Send(buffer, offset, length, segmentType, tshType);
                    buffer = null;
                }
                finally
                {
                    this.parentConnection.ReleaseSendLock();
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void SendTSH(MQTSH tsh)
        {
            uint method = 0x61c;
            this.TrEntry(method, new object[] { tsh });
            MQFAPConnection connection = this.Connection;
            try
            {
                if (this.IsEndRequested || (this.asyncFailure != null))
                {
                    NmqiException exception = new NmqiException(base.env, -1, null, 2, 0x7d9, this.asyncFailure);
                    throw exception;
                }
                byte[] tshBuffer = tsh.TshBuffer;
                byte[] buffer = null;
                int offset = tsh.Offset;
                int length = tsh.Length;
                int maxlength = length;
                base.TrData(method, 0, "Data before Processed by SendExits(if no exits are defined, this is final)", tshBuffer);
                if ((this.exitsActive && (this.exits != null)) && this.exits.sendExitDefined)
                {
                    buffer = this.exits.ProcessSendExits(ref tshBuffer, ref offset, ref length, ref maxlength);
                    base.TrData(method, 0, "Data after processed by SendExits", tshBuffer);
                }
                else
                {
                    buffer = tshBuffer;
                }
                try
                {
                    this.parentConnection.RequestSendLock();
                    connection.Send(buffer, offset, length, tsh.SegmentType, tsh.TSHType);
                }
                finally
                {
                    this.parentConnection.ReleaseSendLock();
                }
            }
            finally
            {
                tsh.ParentBuffer.Free();
                tsh.ParentBuffer = null;
                base.TrExit(method);
            }
        }

        internal void SetDisconnected()
        {
            uint method = 0x62a;
            this.TrEntry(method);
            this.disconnected = true;
            try
            {
                if (this.hconn != null)
                {
                    this.hconn.SetDisconnected();
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void StartConversation()
        {
            uint method = 0x621;
            this.TrEntry(method);
            MQSOCKACT mqsockact = new MQSOCKACT();
            int translength = 0x1c + mqsockact.GetLength();
            bool flag = CommonServices.TraceEnabled();
            MQTSH rTSH = null;
            try
            {
                MQTSH tsh = this.AllocateTSH(2, 12, 0, true, translength);
                mqsockact.Type = 1;
                mqsockact.ConversationID = this.conversationId;
                mqsockact.RequestID = 0;
                mqsockact.Parm1 = Thread.CurrentThread.ManagedThreadId;
                mqsockact.Parm2 = this.traceIdentifier;
                tsh.Offset = tsh.WriteStruct(tsh.TshBuffer, tsh.Offset);
                tsh.Offset = mqsockact.WriteStruct(tsh.TshBuffer, tsh.Offset);
                tsh.Offset = 0;
                if (flag)
                {
                    base.TrAPI(method, "__________");
                    base.TrAPI(method, "START CONVERSATION FLOW >>");
                    base.TrData(method, 0, "Conversation Id", BitConverter.GetBytes(this.ConversationId));
                    base.TrData(method, 0, "Buffer", tsh.Offset, tsh.Length, tsh.TshBuffer);
                    base.TrAPIOutput(method, "CompCode");
                    base.TrAPIOutput(method, "Reason");
                }
                try
                {
                    try
                    {
                        this.parentConnection.RequestSendLock();
                        this.Connection.SendTSH(tsh);
                    }
                    finally
                    {
                        this.parentConnection.ReleaseSendLock();
                    }
                    rTSH = this.ReceiveTSH(null);
                    if ((rTSH.ControlFlags1 & 8) != 0)
                    {
                        NmqiException exception = new NmqiException(base.env, -1, null, 2, 0x80b, null);
                        base.TrException(method, exception);
                        throw exception;
                    }
                    switch (rTSH.SegmentType)
                    {
                        case 5:
                            if ((rTSH.ControlFlags1 & 2) != 0)
                            {
                                NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x8e1, null);
                                base.TrException(method, exception2);
                                throw exception2;
                            }
                            return;

                        case 12:
                            mqsockact.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            return;
                    }
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = (uint) this.conversationId;
                    CommonServices.ArithInsert2 = (uint) mqsockact.Type;
                    CommonServices.CommentInsert1 = "Start Conversation Request has been refused by QMgr";
                    CommonServices.CommentInsert2 = "Unknown SockACT from server for START_CONV";
                    base.FFST("%Z% %W% %I% %E% %U%", "%C%", method, 1, 0x8e1, 0);
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x8e1, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                finally
                {
                    if (flag)
                    {
                        base.TrAPI(method, "__________");
                        base.TrAPI(method, "START CONVERSATION FLOW <<");
                        base.TrData(method, 0, "Name", BitConverter.GetBytes(this.ConversationId));
                    }
                    this.ReleaseReceivedTSH(rTSH);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public override string ToString()
        {
            uint method = 0x62f;
            this.TrEntry(method);
            StringBuilder builder = new StringBuilder();
            builder.Append("[ConnectionId=");
            if (this.connectionIdString == null)
            {
                builder.Append("xxxxxxx");
            }
            else
            {
                builder.Append(this.connectionIdString);
            }
            builder.Append("]");
            string result = builder.ToString();
            base.TrExit(method, result);
            return result;
        }

        private void UnlockRcvExitForTSH(MQTSH rTSH)
        {
            uint method = 0x615;
            this.TrEntry(method, new object[] { rTSH });
            try
            {
                this.rcvExitLock.Acquire();
                if (this.rcvExitLockTSH == rTSH)
                {
                    this.rcvExitLockTSH = null;
                }
            }
            finally
            {
                this.rcvExitLock.Release();
                base.TrExit(method);
            }
        }

        public byte[] AccountingToken
        {
            get
            {
                return this.accountingToken;
            }
            set
            {
                this.accountingToken = value;
            }
        }

        public Exception AsyncFailure
        {
            get
            {
                return this.asyncFailure;
            }
            set
            {
                this.asyncFailure = value;
            }
        }

        internal int Ccsid
        {
            get
            {
                return 0;
            }
        }

        internal int CmdLevel
        {
            get
            {
                int cmdLevel = this.Connection.CmdLevel;
                if (cmdLevel < 0)
                {
                    if (this.info == null)
                    {
                        this.LoadInfo();
                    }
                    cmdLevel = this.info.CommandLevel;
                }
                base.TrText("CommandLevel = " + cmdLevel);
                return cmdLevel;
            }
        }

        internal MQFAPConnection Connection
        {
            get
            {
                MQFAPConnection parentConnection = this.parentConnection;
                if (!this.disconnected && (parentConnection != null))
                {
                    return parentConnection;
                }
                NmqiException exception = new NmqiException(base.env, -1, null, 2, 0x7d9, this.asyncFailure);
                throw exception;
            }
        }

        internal bool ConnectionBroken
        {
            get
            {
                return this.connectionBroken;
            }
        }

        public byte[] ConnectionId
        {
            get
            {
                return this.connectionId;
            }
            set
            {
                this.connectionId = value;
                if (value != null)
                {
                    this.connectionIdString = NmqiTools.ArrayToHexString(this.connectionId);
                }
            }
        }

        public string ConnectionIdAsString
        {
            get
            {
                return this.connectionIdString;
            }
        }

        internal int ConversationId
        {
            get
            {
                base.TrText("Coversation Id = " + this.conversationId);
                return this.conversationId;
            }
            set
            {
                this.conversationId = value;
                base.TrText("Set Coversation Id = " + this.conversationId);
            }
        }

        public int FapLevel
        {
            get
            {
                return this.parentConnection.FapLevel;
            }
        }

        internal ManagedHconn Hconn
        {
            get
            {
                return this.hconn;
            }
            set
            {
                this.hconn = value;
            }
        }

        internal MQTagPool IdTagPool
        {
            get
            {
                return this.Connection.IdTagPool;
            }
        }

        internal bool IsDisconnected
        {
            get
            {
                return this.disconnected;
            }
        }

        internal bool IsEndRequested
        {
            get
            {
                return this.endRequested;
            }
            set
            {
                this.endRequested = value;
            }
        }

        internal bool IsMultiplexingEnabled
        {
            get
            {
                if (this.sessionMultiplexed == -1)
                {
                    if (this.Connection.IsMultiplexingEnabled)
                    {
                        this.sessionMultiplexed = 1;
                    }
                    else
                    {
                        this.sessionMultiplexed = 0;
                    }
                }
                return (this.sessionMultiplexed == 1);
            }
        }

        internal bool IsReconnectable
        {
            get
            {
                return (this.hconn != null);
            }
        }

        internal bool IsReconnectionBegun
        {
            get
            {
                return this.reconnectionBegun;
            }
            set
            {
                this.reconnectionBegun = value;
            }
        }

        public long MaximumMessageLength
        {
            get
            {
                return this.Connection.MaximumMessageLength;
            }
        }

        internal int MQEncoding
        {
            get
            {
                return this.Connection.RemoteMQEncoding;
            }
        }

        internal MQFAP MQFap
        {
            get
            {
                return this.parentConnection.MQFap;
            }
        }

        internal string Name
        {
            get
            {
                if (this.info == null)
                {
                    this.LoadInfo();
                }
                return this.info.Name;
            }
        }

        internal int NumberOfSharingConversations
        {
            get
            {
                int sharingConversations = -1;
                if (this.parentConnection != null)
                {
                    MQChannelDefinition negotiatedChannel = this.parentConnection.NegotiatedChannel;
                    if (negotiatedChannel != null)
                    {
                        sharingConversations = negotiatedChannel.SharingConversations;
                    }
                }
                base.TrText("Sharing Coversastion on the channel associated with connection are " + sharingConversations);
                return sharingConversations;
            }
        }

        internal string OriginalQueueManagerName
        {
            get
            {
                return this.originalQueueManagerName;
            }
            set
            {
                this.originalQueueManagerName = value;
            }
        }

        internal MQFAPConnection ParentConnection
        {
            get
            {
                return this.parentConnection;
            }
            set
            {
                this.parentConnection = value;
            }
        }

        internal int Platform
        {
            get
            {
                int platform = this.parentConnection.Platform;
                if (platform < 0)
                {
                    if (this.info == null)
                    {
                        this.LoadInfo();
                    }
                    platform = this.info.Platform;
                }
                base.TrText("Platform = " + platform);
                return platform;
            }
        }

        internal ManagedHconn ReconnectableParent
        {
            get
            {
                return this.hconn;
            }
        }

        internal string RemoteQmgrName
        {
            get
            {
                return this.Connection.RemoteQmgrName;
            }
        }

        public string Uid
        {
            get
            {
                if (this.FapLevel < 10)
                {
                    if (this.info == null)
                    {
                        this.LoadInfo();
                    }
                    return this.info.Uid;
                }
                return this.parentConnection.RemoteQMID;
            }
        }

        public string UserIdentifier
        {
            get
            {
                return this.userIdentifier;
            }
            set
            {
                this.userIdentifier = value;
            }
        }

        internal bool WasReconnectable
        {
            get
            {
                return this.wasReconnectable;
            }
        }
    }
}

