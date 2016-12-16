namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;

    internal abstract class MQFAPConnection : NmqiObject
    {
        public const string APPNAME = "WebSphere MQ Client for .NET";
        private volatile NmqiException asyncFailure;
        private Lock asyncTshLock;
        private Queue<MQTSH> asyncTshQueue;
        private string channelName;
        private MQChannelExitHandler channelSecurityExit;
        protected MQChannelDefinition clientConn;
        private const int COMMS_COPY_MAX = 0x1000;
        protected MQCommsBufferPool commsBufferPool;
        private bool connected;
        private bool connectionComplete;
        internal int connectOptions;
        internal int CONVERSATION_ALLOCATION_FAILURE;
        internal const int CONVERSATION_ALLOCATION_RETRY = 2;
        internal const int CONVERSATION_ALLOCATION_SUCCESS = 0;
        internal const int CONVERSATION_ALLOCATION_WAIT = 3;
        private bool convIdsWrapped;
        protected MQFAP fap;
        protected int fapLevel;
        public const int HEARTBEAT_REQUEST = 1;
        public const int HEARTBEAT_RESPONSE = 2;
        private MQTagPool idTagPool;
        private int lastAllocatedConvId;
        private int localCcsid;
        private const int MAX_BIND_ATTEMPTS = 10;
        internal uint maxTransmissionSize;
        private int mqgetWaitInterval;
        private bool multiplexingEnabled;
        protected MQChannelDefinition negotiatedChannel;
        protected int nmqiFlags;
        internal const int NUMBER_OF_SHARING_CONVERSTIONS_UNDEFINED = -1;
        private byte[] overflow;
        private static int overflowbytes = 0;
        private int overflowsize;
        private ushort[] pal;
        private ushort ppa;
        private bool ppaRequired;
        private const string productIdentifier = "MQNM07100000";
        protected string qMgrName;
        private byte[] r;
        private MQRcvThread rcvThread;
        private Lock rcvThreadSendMutex;
        private bool reconnectRequested;
        private int remoteCmdLevel;
        protected MQConnectionSpecification remoteConnectionSpec;
        private int remoteEncoding;
        private int remoteMQEncoding;
        private int remotePlatform;
        private int remoteProcessId;
        private string remoteProductId;
        private byte[] remoteQMID;
        private int remoteThreadId;
        private int remoteTraceId;
        private Random rg;
        private RandomNumberGenerator rgs;
        public const string sccsid = "%Z% %W% %I% %E% %U%";
        private Mutex sendMutex;
        private int sessFlags;
        private int sessFlags2;
        private int sessFlags3;
        private readonly Lock sessionMutex;
        private Dictionary<int, MQSession> sessions;
        protected MQSSLConfigOptions sslConfig;
        private int timeout;
        private static int traceIdentifier = 1;
        public const int TSHM_CONVID_PRIMARY = 1;

        protected MQFAPConnection(NmqiEnvironment env, MQChannelDefinition clonedcd) : base(env)
        {
            this.CONVERSATION_ALLOCATION_FAILURE = 1;
            this.sessionMutex = new Lock();
            this.fapLevel = 13;
            this.maxTransmissionSize = 0x7ff6;
            this.localCcsid = Encoding.ASCII.WindowsCodePage;
            this.sendMutex = new Mutex();
            this.rcvThreadSendMutex = new Lock();
            this.remoteQMID = new byte[0x30];
            this.remotePlatform = -1;
            this.remoteCmdLevel = -1;
            this.sessions = new Dictionary<int, MQSession>();
            this.lastAllocatedConvId = -1;
            this.asyncTshLock = new Lock();
            this.asyncTshQueue = new Queue<MQTSH>();
            this.remoteProductId = "";
            this.qMgrName = "";
            this.timeout = 0x1d4c0;
            this.r = new byte[0x18];
            this.ppa = 0xffff;
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { env });
            int intValue = env.Cfg.GetIntValue(MQClientCfg.ENV_MQCCSID);
            this.clientConn = clonedcd;
            if (intValue <= 0)
            {
                this.localCcsid = (ushort) Encoding.ASCII.WindowsCodePage;
            }
            else
            {
                this.localCcsid = (ushort) intValue;
            }
            string choiceValue = env.Cfg.GetChoiceValue(MQClientCfg.PASSWORD_PROTECTION);
            base.TrText("Cfg value for the PASSWORD_PROTECTION: " + choiceValue);
            if ((choiceValue.Equals("ALWAYS") && (this.clientConn != null)) && (((this.clientConn.SSLCipherSpec == null) || (this.clientConn.SSLCipherSpec.Length == 0)) || this.clientConn.SSLCipherSpec.Contains("NULL")))
            {
                base.TrText("Setting PP Option to Required");
                this.ppaRequired = true;
                this.pal = new ushort[] { 1 };
            }
            else
            {
                base.TrText("Setting PP option to default - NULL, this is compatible mode.");
                ushort[] numArray2 = new ushort[2];
                numArray2[0] = 1;
                this.pal = numArray2;
            }
            if (env.Cfg.GetChoiceValue(MQClientCfg.AMQ_RANDOM_NUMBER_TYPE).Equals(NmqiConstants.AMQ_RANDOM_NUMBER_TYPE_FAST))
            {
                this.rg = new Random();
                byte[] buffer = new byte[12];
                this.rg.NextBytes(buffer);
                Array.Copy(buffer, 0, this.r, 0, 12);
            }
            else
            {
                this.rgs = RandomNumberGenerator.Create();
                byte[] data = new byte[12];
                this.rgs.GetNonZeroBytes(data);
                Array.Copy(data, 0, this.r, 0, 12);
            }
        }

        internal MQTSH AllocateTSH(int tshType, int segmentType, MQTSH tshHeaderP)
        {
            uint method = 0x5da;
            this.TrEntry(method, new object[] { tshType, segmentType, tshHeaderP });
            MQTSH result = null;
            try
            {
                result = this.AllocateTSH(tshType, (byte) segmentType, tshHeaderP, 0);
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        internal MQTSH AllocateTSH(int tshType, byte segmentType, MQTSH tshHeaderP, int capacity)
        {
            uint method = 0x5d9;
            this.TrEntry(method, new object[] { tshType, segmentType, tshHeaderP, capacity });
            MQTSH result = tshHeaderP;
            try
            {
                if (this.IsMultiplexingEnabled && (capacity == 0))
                {
                    capacity = ((int) this.maxTransmissionSize) & -4;
                    capacity += 8;
                }
                IMQCommsBuffer buffer = this.commsBufferPool.AllocateBuffer(capacity);
                if (result == null)
                {
                    if ((tshType == 0) || (tshType == 2))
                    {
                        result = new MQTSH(tshType, buffer);
                    }
                    else
                    {
                        result = new MQTSH(this.IsMultiplexingEnabled, buffer);
                    }
                    if (this.IsMultiplexingEnabled && (tshType == 2))
                    {
                        Buffer.BlockCopy(MQTSH.rfpTSH_C_ID, 0, result.Id, 0, 4);
                    }
                }
                else
                {
                    result.ParentBuffer = buffer;
                    result.Offset = 0;
                }
                result.SetTransLength(capacity);
                result.Encoding = 2;
                result.SegmentType = segmentType;
                result.ControlFlags1 = 0x30;
                result.ControlFlags2 = 0;
                result.LUWID = MQTSH.BLANK_LUWID;
                result.MQEncoding = 0x222;
                result.Ccsid = (ushort) this.localCcsid;
                result.Reserved = 0;
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        public MQTSH AllocInitialDataTsh(byte segmentType, int translength)
        {
            uint method = 0x5d7;
            this.TrEntry(method, new object[] { segmentType, translength });
            MQTSH result = null;
            try
            {
                if (this.IsMultiplexingEnabled)
                {
                    result = this.AllocateTSH(1, segmentType, null, translength);
                    result.SetConversationId(1);
                    result.SetRequestId(0);
                    return result;
                }
                result = this.AllocateTSH(0, segmentType, null, translength);
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        public void AnalyseErrorSegment(MQTSH rTSH)
        {
            uint method = 0x5e4;
            this.TrEntry(method, new object[] { rTSH });
            try
            {
                int reason = this.connected ? 0x7d9 : 0x80b;
                int reasonCode = -1;
                int length = rTSH.GetLength();
                MQERD mqerd = new MQERD();
                if (rTSH.TransLength >= (length + mqerd.GetLength()))
                {
                    mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset + length);
                    reasonCode = mqerd.GetReasonCode();
                }
                switch (((uint) reasonCode))
                {
                    case 1:
                    {
                        string[] strArray4 = new string[3];
                        strArray4[1] = this.clientConn.ConnectionName;
                        strArray4[2] = this.ChannelName;
                        NmqiException exception4 = new NmqiException(base.env, 0x2530, strArray4, 2, 0x9ec, null);
                        throw exception4;
                    }
                    case 2:
                    {
                        string[] strArray = new string[3];
                        strArray[2] = this.ChannelName;
                        NmqiException exception = new NmqiException(base.env, 0x254b, strArray, 2, reason, null);
                        throw exception;
                    }
                    case 3:
                    {
                        string[] strArray2 = new string[3];
                        strArray2[2] = this.ChannelName;
                        NmqiException exception2 = new NmqiException(base.env, 0x2534, strArray2, 2, reason, null);
                        throw exception2;
                    }
                    case 5:
                    {
                        string[] strArray3 = new string[3];
                        strArray3[2] = this.ChannelName;
                        NmqiException exception3 = new NmqiException(base.env, 0x2535, strArray3, 2, reason, null);
                        throw exception3;
                    }
                    case 7:
                    {
                        string[] strArray5 = new string[3];
                        strArray5[2] = this.ChannelName;
                        NmqiException exception5 = new NmqiException(base.env, 0x2538, strArray5, 2, reason, null);
                        throw exception5;
                    }
                    case 0x17:
                    {
                        reason = 0x9e9;
                        string[] strArray6 = new string[3];
                        strArray6[2] = this.ChannelName;
                        NmqiException exception6 = new NmqiException(base.env, 0x2518, strArray6, 2, reason, null);
                        throw exception6;
                    }
                    case 0x19:
                    {
                        string[] strArray7 = new string[3];
                        strArray7[2] = this.ChannelName;
                        NmqiException exception7 = new NmqiException(base.env, 0x25ab, strArray7, 2, reason, null);
                        throw exception7;
                    }
                    case 0x1c:
                    {
                        string[] strArray8 = new string[3];
                        strArray8[2] = this.ChannelName;
                        NmqiException exception8 = new NmqiException(base.env, 0x25f2, strArray8, 2, reason, null);
                        throw exception8;
                    }
                }
                if (this.connected)
                {
                    string[] strArray9 = new string[3];
                    strArray9[2] = this.ChannelName;
                    NmqiException exception9 = new NmqiException(base.env, 0x270f, strArray9, 2, reason, null);
                    throw exception9;
                }
                string[] inserts = new string[3];
                inserts[2] = this.ChannelName;
                NmqiException exception10 = new NmqiException(base.env, 0x251f, inserts, 2, reason, null);
                throw exception10;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal MQSession AssignSession()
        {
            uint method = 0x5cd;
            this.TrEntry(method);
            MQSession session = null;
            try
            {
                this.remoteConnectionSpec.ConnectionsLock.AssertOnCurrentThreadHoldsLock();
                session = new MQSession(base.env, this);
                if (this.rcvThread != null)
                {
                    int num2 = this.lastAllocatedConvId + 2;
                    if (num2 < this.lastAllocatedConvId)
                    {
                        this.convIdsWrapped = true;
                    }
                    if (this.convIdsWrapped)
                    {
                        try
                        {
                            while (this.sessions[num2] != null)
                            {
                                num2 += 2;
                            }
                        }
                        catch (KeyNotFoundException)
                        {
                        }
                    }
                    session.ConversationId = num2;
                    base.TrText(method, string.Concat(new object[] { "Allocated ConversationId = ", num2, " for Connection = ", session.ToString() }));
                    this.lastAllocatedConvId = num2;
                }
                this.sessions.Add(session.ConversationId, session);
            }
            finally
            {
                base.TrExit(method, session);
            }
            return session;
        }

        internal void AsyncFailureNotify(NmqiException e, bool quiescing)
        {
            uint method = 0x5e1;
            this.TrEntry(method, new object[] { e, quiescing });
            try
            {
                base.TrText(method, "We received async failure on this connection");
                int flag = 0x100;
                int reason = 0x89a;
                if (e.Reason == 0x7d9)
                {
                    flag = 2;
                    reason = 0x7d9;
                }
                if ((e.Reason == 0x89a) || (e.ReasonCode == 0x871))
                {
                    base.TrText(method, "We receieved QUIESCING error on this connection, notify the connections.");
                    try
                    {
                        this.sessionMutex.Acquire();
                        foreach (MQSession session in this.sessions.Values)
                        {
                            if ((session.Hconn != null) && (session.Hconn.Parent == null))
                            {
                                base.TrText(method, "Hconn = " + session.Hconn.Value + "Setting to quiescing");
                                session.Hconn.SetQuiescing();
                                session.Hconn.RaiseEvent(reason);
                            }
                        }
                        foreach (MQSession session2 in this.sessions.Values)
                        {
                            if ((session2.Hconn != null) && (session2.Hconn.Parent != null))
                            {
                                base.TrText(method, "Hconn = " + session2.Hconn.Value + "Setting to quiescing");
                                session2.Hconn.SetQuiescing();
                                session2.Hconn.RaiseEvent(reason);
                            }
                        }
                    }
                    finally
                    {
                        this.sessionMutex.Release();
                    }
                }
                else
                {
                    base.TrText(method, "Pulse all the waiters on this connection");
                    lock (this.asyncTshLock)
                    {
                        this.asyncFailure = e;
                        Monitor.PulseAll(this.asyncTshLock);
                    }
                    if (!quiescing)
                    {
                        base.TrText(method, "Setting connected to false");
                        this.connected = false;
                    }
                    try
                    {
                        this.sessionMutex.Acquire();
                        foreach (MQSession session3 in this.sessions.Values)
                        {
                            if ((session3.Hconn != null) && (session3.Hconn.Parent == null))
                            {
                                base.TrText(method, "Hconn = " + session3.Hconn.Value + " notified on the error");
                                session3.DeliverException(e, flag, reason);
                            }
                        }
                        foreach (MQSession session4 in this.sessions.Values)
                        {
                            if ((session4.Hconn != null) && (session4.Hconn.Parent != null))
                            {
                                base.TrText(method, "Hconn = " + session4.Hconn.Value + " notified on the error");
                                session4.DeliverException(e, flag, reason);
                            }
                        }
                    }
                    finally
                    {
                        this.sessionMutex.Release();
                    }
                    try
                    {
                        this.remoteConnectionSpec.ConnectionsLock.Acquire();
                        this.remoteConnectionSpec.RemoveConnection(this);
                    }
                    finally
                    {
                        this.remoteConnectionSpec.ConnectionsLock.Release();
                    }
                    if (!quiescing)
                    {
                        base.TrText(method, "Disconnecting following the async failure");
                        this.Disconnect();
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void CleanUp(bool protocolConnected, NmqiException mqe)
        {
            uint method = 0x5d0;
            this.TrEntry(method, new object[] { protocolConnected, mqe });
            try
            {
                if (this.asyncFailure == null)
                {
                    this.asyncFailure = mqe;
                }
                if (protocolConnected)
                {
                    try
                    {
                        this.ProtocolDisconnect();
                    }
                    catch (Exception exception)
                    {
                        base.TrException(method, exception);
                        throw exception;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void Connect(MQConnectionSecurityParameters securityParms)
        {
            uint method = 0x5cf;
            this.TrEntry(method, new object[] { securityParms });
            this.remoteConnectionSpec.ConnectionsLock.AssertOnCurrentThreadHoldsLock();
            bool protocolConnected = false;
            try
            {
                if (!this.connectionComplete)
                {
                    this.ProtocolConnect();
                    protocolConnected = true;
                    base.TrText(method, "Protocol connected..for this connection request.");
                    this.InitialiseSession();
                    base.TrText(method, "Initial Negotiations completed for this connection..");
                    this.ProtocolSetHeartbeatInterval(this.negotiatedChannel.HeartBeatInterval);
                    base.TrText(method, "Heart beat has been set.");
                    this.NegotiateSecurity();
                    base.TrText(method, "Security has been negotiated to Queue Manager.");
                    this.connected = true;
                    base.TrText(method, "Now we are fully connected and ready to use connection..");
                }
            }
            catch (NmqiException exception)
            {
                base.TrException(method, exception, 1);
                this.CleanUp(protocolConnected, exception);
                throw exception;
            }
            catch (MQManagedClientException exception2)
            {
                base.TrException(method, exception2, 1);
                NmqiException mqe = new NmqiException(base.env, -1, null, exception2.CompCode, exception2.Reason, exception2);
                this.CleanUp(protocolConnected, mqe);
                throw mqe;
            }
            catch (MQException exception4)
            {
                base.TrException(method, exception4, 1);
                NmqiException exception5 = new NmqiException(base.env, -1, null, exception4.CompCode, exception4.Reason, exception4);
                this.CleanUp(protocolConnected, exception5);
                throw exception4;
            }
            catch (Exception exception6)
            {
                base.TrException(method, exception6, 2);
                NmqiException exception7 = new NmqiException(base.env, -1, null, 2, 0x893, exception6);
                this.CleanUp(protocolConnected, exception7);
                throw exception7;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void DeliverTSH(MQTSH rTSH)
        {
            uint method = 0x5e0;
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

        internal void Disconnect()
        {
            uint method = 0x5d1;
            this.TrEntry(method);
            try
            {
                this.sessionMutex.Acquire();
                foreach (MQSession session in this.sessions.Values)
                {
                    session.SetDisconnected();
                }
                if (this.channelSecurityExit != null)
                {
                    this.channelSecurityExit.TermExits();
                    this.connected = false;
                }
                if (!this.multiplexingEnabled)
                {
                    try
                    {
                        MQTSH tsh = this.AllocateTSH(0, 5, null, 0x1c);
                        tsh.ControlFlags1 = 0x38;
                        tsh.TransLength = tsh.GetLength();
                        tsh.Offset = tsh.WriteStruct(tsh.TshBuffer, 0);
                        tsh.Offset = 0;
                        this.RequestSendLock();
                        this.SendTSH(tsh);
                        this.ReleaseSendLock();
                    }
                    catch (NmqiException)
                    {
                    }
                }
                if (this.rcvThread != null)
                {
                    this.rcvThread.SetDisconnecting();
                }
                this.ProtocolDisconnect();
            }
            finally
            {
                this.sessionMutex.Release();
                base.TrExit(method);
            }
        }

        internal int GetFreeConversations()
        {
            if (this.negotiatedChannel != null)
            {
                return (this.negotiatedChannel.SharingConversations - this.sessions.Count);
            }
            return 0;
        }

        internal MQSession GetSessionByConvId(int convId)
        {
            uint method = 0x5e6;
            this.TrEntry(method, new object[] { convId });
            MQSession session = null;
            try
            {
                this.sessionMutex.Acquire();
                session = this.sessions[convId];
            }
            catch (KeyNotFoundException exception)
            {
                base.TrException(method, exception);
            }
            finally
            {
                this.sessionMutex.Release();
                base.TrExit(method);
            }
            return session;
        }

        internal bool InitialiseSession()
        {
            uint method = 0x5d2;
            this.TrEntry(method);
            int num2 = 10;
            MQID mqid = new MQID();
            int length = mqid.GetLength();
            MQTSH tsh = null;
            int windowsCodePage = Encoding.ASCII.WindowsCodePage;
            ushort batchSize = (ushort) this.negotiatedChannel.BatchSize;
            uint seqNumberWrap = (uint) this.negotiatedChannel.SeqNumberWrap;
            uint maxMessageLength = (uint) this.negotiatedChannel.MaxMessageLength;
            uint heartBeatInterval = (uint) this.negotiatedChannel.HeartBeatInterval;
            try
            {
                this.sessFlags |= 0x25;
                this.sessFlags2 |= 80;
                this.sessFlags3 |= 8;
                if (this.ProtocolSupportsAsyncMode())
                {
                    if ((this.connectOptions & 0x10000) != 0)
                    {
                        this.negotiatedChannel.SharingConversations = 1;
                    }
                }
                else
                {
                    this.negotiatedChannel.SharingConversations = 0;
                }
                this.channelName = this.negotiatedChannel.ChannelName;
                bool flag2 = true;
                bool flag3 = true;
                while ((num2 > 0) && flag2)
                {
                    if (flag3)
                    {
                        int translength = (this.IsMultiplexingEnabled ? 0x24 : 0x1c) + length;
                        tsh = this.AllocInitialDataTsh(1, translength);
                        tsh.ControlFlags1 = (byte) (tsh.ControlFlags1 | 1);
                        base.GetBytesRightPad(this.negotiatedChannel.ChannelName, ref mqid.id.ChannelName);
                        base.BlankPad(mqid.id.QueueManagerName);
                        mqid.id.FapLevel = (byte) this.fapLevel;
                        mqid.id.IDFlags = (byte) this.sessFlags;
                        mqid.id.IDEFlags = 0;
                        mqid.id.ErrFlags = 0;
                        mqid.id.MaxMessageSize = maxMessageLength;
                        mqid.id.MaxMessagesPerBatch = batchSize;
                        mqid.id.MaxTransmissionSize = this.maxTransmissionSize;
                        mqid.id.MessageSequenceWrapValue = seqNumberWrap;
                        mqid.id.Ccsid = (ushort) this.localCcsid;
                        mqid.id.IDFlags2 = (byte) this.sessFlags2;
                        mqid.id.IDEFlags2 = 0;
                        mqid.id.HeartbeatInterval = heartBeatInterval;
                        mqid.id.EFLLength = (ushort) (mqid.GetLength() - MQID.FIXED_LENGTH);
                        mqid.id.ErrFlags2 = 0;
                        if (this.fapLevel >= 9)
                        {
                            mqid.id.Reserved2 = BitConverter.GetBytes((short) 0);
                            mqid.id.ConvPerSocket = this.negotiatedChannel.SharingConversations;
                            mqid.id.IDFlags3 = (byte) this.sessFlags3;
                            mqid.id.IDEFlags3 = 0;
                            Process currentProcess = Process.GetCurrentProcess();
                            mqid.id.ProcessIdentifier = currentProcess.Id;
                            currentProcess.Dispose();
                            mqid.id.ThreadIdentifier = Thread.CurrentThread.ManagedThreadId;
                            mqid.id.TraceIdentifier = traceIdentifier;
                            traceIdentifier++;
                            Encoding.ASCII.GetBytes("MQNM07100000", 0, 12, mqid.id.ProductIdentifier, 0);
                        }
                        if (this.fapLevel >= 10)
                        {
                            mqid.id.QueueManagerId = this.remoteQMID;
                        }
                        if (this.fapLevel >= 13)
                        {
                            mqid.SetPal(this.pal);
                            Buffer.BlockCopy(this.r, 0, mqid.id.R, 0, 12);
                        }
                        tsh.Offset = tsh.WriteStruct(tsh.TshBuffer, tsh.Offset);
                        tsh.Offset += mqid.WriteStruct(tsh.TshBuffer, tsh.Offset);
                        tsh.Offset = 0;
                        try
                        {
                            this.RequestSendLock();
                            this.SendTSH(tsh);
                        }
                        finally
                        {
                            this.ReleaseSendLock();
                        }
                    }
                    MQTSH rTSH = this.ReceiveTSH(null);
                    ushort ccsid = rTSH.Ccsid;
                    this.remoteEncoding = rTSH.MQEncoding;
                    flag2 = false;
                    flag3 = false;
                    switch (rTSH.SegmentType)
                    {
                        case 5:
                            if ((rTSH.ControlFlags1 & 2) != 0)
                            {
                                MQERD mqerd = new MQERD();
                                if (rTSH.Length > tsh.GetLength())
                                {
                                    mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                    mqerd.GetReasonCode();
                                }
                                string[] inserts = new string[3];
                                inserts[2] = this.channelName;
                                NmqiException exception = new NmqiException(base.env, 0x251f, inserts, 2, 0x80b, null);
                                throw exception;
                            }
                            break;

                        case 6:
                            flag2 = false;
                            num2++;
                            goto Label_0A03;

                        case 1:
                        {
                            MQID mqid2 = new MQID();
                            if (rTSH.Length > rTSH.GetLength())
                            {
                                mqid2.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            }
                            if (mqid2.id.FapLevel >= 3)
                            {
                                ushort num20 = mqid2.id.Ccsid;
                                if ((mqid2.id.IDEFlags2 & 0x10) != 0)
                                {
                                    this.sessFlags2 &= -17;
                                }
                            }
                            if (mqid2.id.FapLevel >= 4)
                            {
                                this.NegotiatedChannel.QMgrName = mqid2.id.QueueManagerName;
                            }
                            if (mqid2.id.FapLevel >= 9)
                            {
                                this.remoteProcessId = mqid2.id.ProcessIdentifier;
                                this.negotiatedChannel.SharingConversations = mqid2.id.ConvPerSocket;
                                this.remoteThreadId = mqid2.id.ThreadIdentifier;
                                this.remoteTraceId = mqid2.id.TraceIdentifier;
                                Array.Copy(mqid2.id.ProductIdentifier, 0, "MQNM07100000".ToCharArray(), 0, 12);
                                this.negotiatedChannel.SharingConversations = mqid2.id.ConvPerSocket;
                                if ((mqid2.id.ConvPerSocket > 0) && !this.IsMultiplexingEnabled)
                                {
                                    this.SetUpAsyncMode();
                                }
                            }
                            else
                            {
                                this.negotiatedChannel.SharingConversations = 0;
                            }
                            if (mqid2.id.FapLevel >= 10)
                            {
                                this.remoteQMID = mqid2.id.QueueManagerId;
                            }
                            if ((rTSH.ControlFlags1 & 2) != 0)
                            {
                                flag2 = true;
                                if ((mqid2.id.ErrFlags & 1) != 0)
                                {
                                    throw MQID.ErrorException(240, (uint) this.localCcsid, this.negotiatedChannel.ChannelName.Trim());
                                }
                                if ((mqid2.id.ErrFlags & 2) != 0)
                                {
                                    throw MQID.ErrorException(0xf1, (uint) tsh.MQEncoding, this.negotiatedChannel.ChannelName.Trim());
                                }
                                if ((mqid2.id.ErrFlags & 4) != 0)
                                {
                                    uint maxTransmissionSize = mqid2.id.MaxTransmissionSize;
                                    if (this.maxTransmissionSize > maxTransmissionSize)
                                    {
                                        this.maxTransmissionSize = maxTransmissionSize;
                                    }
                                }
                                if ((mqid2.id.ErrFlags & 0x10) != 0)
                                {
                                    uint maxMessageSize = mqid2.id.MaxMessageSize;
                                    if (maxMessageLength > maxMessageSize)
                                    {
                                        maxMessageLength = maxMessageSize;
                                    }
                                }
                                if ((mqid2.id.ErrFlags & 0x80) != 0)
                                {
                                    uint heartbeatInterval = mqid2.id.HeartbeatInterval;
                                    if (heartBeatInterval > heartbeatInterval)
                                    {
                                        heartBeatInterval = heartbeatInterval;
                                    }
                                }
                                if ((mqid2.id.IDEFlags & 4) != 0)
                                {
                                    this.sessFlags &= -5;
                                    this.negotiatedChannel.MaxMessageLength = ((int) this.maxTransmissionSize) - tsh.GetLength();
                                }
                                if ((mqid2.id.IDEFlags & 0x40) != 0)
                                {
                                    this.sessFlags |= 0x40;
                                }
                                if ((mqid2.id.ErrFlags & 8) != 0)
                                {
                                    this.fapLevel = mqid2.id.FapLevel;
                                    if (this.fapLevel < 13)
                                    {
                                        this.sessFlags3 &= -9;
                                    }
                                    if (this.fapLevel < 4)
                                    {
                                        heartBeatInterval = 0;
                                    }
                                    if (this.fapLevel < 2)
                                    {
                                        throw MQID.ErrorException(0xf2, mqid2.id.FapLevel, this.negotiatedChannel.ChannelName.Trim());
                                    }
                                }
                                if ((mqid2.id.ErrFlags & 0x10) != 0)
                                {
                                    uint num12 = mqid2.id.MaxMessageSize;
                                    if (this.negotiatedChannel.MaxMessageLength > num12)
                                    {
                                        this.negotiatedChannel.MaxMessageLength = (int) num12;
                                    }
                                }
                                if ((mqid2.id.ErrFlags & 0x20) != 0)
                                {
                                    ushort maxMessagesPerBatch = mqid2.id.MaxMessagesPerBatch;
                                    if (batchSize > maxMessagesPerBatch)
                                    {
                                        batchSize = maxMessagesPerBatch;
                                    }
                                }
                                if ((mqid2.id.ErrFlags & 0x40) != 0)
                                {
                                    throw MQID.ErrorException(0x15, mqid2.id.MessageSequenceWrapValue, this.negotiatedChannel.ChannelName.Trim());
                                }
                                if ((mqid2.id.ErrFlags & 0x40) != 0)
                                {
                                    this.sessFlags2 &= -65;
                                }
                                if ((mqid2.id.FapLevel >= 4) && ((mqid2.id.ErrFlags & 0x80) != 0))
                                {
                                    uint num14 = mqid2.id.HeartbeatInterval;
                                    if ((this.negotiatedChannel.HeartBeatInterval < num14) || (num14 == 0))
                                    {
                                        this.negotiatedChannel.HeartBeatInterval = (int) num14;
                                    }
                                }
                                if (mqid2.id.FapLevel >= 13)
                                {
                                    this.ppa = mqid2.id.Pal[0];
                                    bool flag4 = false;
                                    foreach (ushort num15 in this.pal)
                                    {
                                        if (this.ppa == num15)
                                        {
                                            flag4 = true;
                                            break;
                                        }
                                    }
                                    if (!flag4)
                                    {
                                        NmqiException exception3 = new NmqiException(base.env, 0x251f, new string[] { this.negotiatedChannel.ChannelName }, 2, 0xa22, null);
                                        throw exception3;
                                    }
                                    Array.Copy(mqid2.id.R, 0, this.r, 12, 12);
                                    if ((mqid2.id.IDEFlags3 & 8) != 0)
                                    {
                                        this.sessFlags3 &= -9;
                                    }
                                }
                                flag3 = flag2;
                            }
                            goto Label_0A03;
                        }
                        default:
                            CommonServices.SetValidInserts();
                            CommonServices.ArithInsert1 = tsh.SegmentType;
                            CommonServices.CommentInsert1 = this.negotiatedChannel.ChannelName.Trim();
                            CommonServices.CommentInsert2 = "Initial negotiation has failed..";
                            base.FFST("%Z% %W% %I% %E% %U%", "%C%", method, 1, 0x20009504, 0);
                            throw MQERD.ErrorException(13, tsh.SegmentType, this.negotiatedChannel.ChannelName.Trim());
                    }
                    if ((rTSH.ControlFlags1 & 8) != 0)
                    {
                        string[] strArray2 = new string[3];
                        strArray2[2] = this.channelName;
                        NmqiException exception2 = new NmqiException(base.env, 0x251f, strArray2, 2, 0x80b, null);
                        throw exception2;
                    }
                Label_0A03:
                    if (rTSH != null)
                    {
                        this.ReleaseReceivedTSH(rTSH);
                    }
                    num2--;
                }
                if (num2 == 0)
                {
                    object[] objArray = new object[] { "Negotiation Failed. CompCode : ", 2.ToString(), " Reason : ", 0x80b };
                    MQManagedClientException exception4 = new MQManagedClientException(string.Concat(objArray), 2, 0x80b);
                    throw exception4;
                }
            }
            catch (MQException exception5)
            {
                base.TrException(method, exception5);
                int reasonCode = exception5.ReasonCode;
                int compCode = exception5.CompCode;
                throw exception5;
            }
            finally
            {
                base.TrExit(method);
            }
            return false;
        }

        internal void InitOAMFlow(MQConnectionSecurityParameters mqcsp, MQSession remoteSession)
        {
            uint method = 0x5d5;
            this.TrEntry(method, new object[] { mqcsp, remoteSession });
            int num2 = this.IsMultiplexingEnabled ? 0x24 : 0x1c;
            int translength = 0;
            byte[] input = null;
            int offset = 0;
            int maxlength = 0;
            bool securityRequired = false;
            int length = 0;
            MQTSH tsh = null;
            MQTSH rTSH = null;
            try
            {
                if (this.fapLevel >= 8)
                {
                    if (this.channelSecurityExit.securityExitDefined)
                    {
                        length = 0;
                        offset = 0;
                        this.channelSecurityExit.ProcessSecurityParms(mqcsp, ref input, ref offset, ref length, ref maxlength, ref securityRequired);
                    }
                    if ((mqcsp != null) && (mqcsp.AuthenticationType != 0))
                    {
                        MQCAUT mqcaut = new MQCAUT();
                        mqcaut.AuthType = mqcsp.AuthenticationType;
                        mqcaut.UserId = mqcsp.UserId;
                        mqcaut.Password = mqcsp.Password;
                        remoteSession.CheckIfDisconnected();
                        byte[] b = new byte[0x400];
                        mqcaut.WriteStruct(b, 0);
                        int cAUTPasswordLen = mqcaut.caut.CAUTPasswordLen;
                        int num7 = MQManagedPPA.FinishWritingAuthFlow(b, mqcaut.GetPasswordOffset(), mqcaut.GetHdrLength() + mqcaut.UserId.Length, this.r, this.ppa);
                        int count = mqcaut.GetLength() + (num7 - mqcaut.caut.CAUTPasswordLen);
                        translength = num2 + count;
                        tsh = remoteSession.AllocateTSH(10, 0, true, translength);
                        tsh.Offset = tsh.WriteStruct(tsh.TshBuffer, 0);
                        Buffer.BlockCopy(b, 0, tsh.TshBuffer, tsh.Offset, count);
                        tsh.Offset = 0;
                        this.SendTSH(tsh);
                        rTSH = remoteSession.ReceiveTSH(null);
                        if (rTSH.SegmentType == 5)
                        {
                            if ((rTSH.ControlFlags1 & 2) != 0)
                            {
                                MQERD mqerd = new MQERD();
                                if (rTSH.Length > rTSH.GetLength())
                                {
                                    mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                                }
                                throw mqerd.ErrorException(this.channelName.Trim());
                            }
                        }
                        else
                        {
                            CommonServices.SetValidInserts();
                            CommonServices.ArithInsert1 = rTSH.SegmentType;
                            CommonServices.CommentInsert1 = this.channelName.Trim();
                            CommonServices.CommentInsert2 = "Unexpected flow received during OAM Flow..";
                            base.FFST("%Z% %W% %I% %E% %U%", "%C%", method, 1, 0x20009504, 0);
                            throw MQERD.ErrorException(13, rTSH.SegmentType, this.channelName.Trim());
                        }
                    }
                }
            }
            finally
            {
                if (rTSH != null)
                {
                    this.ReleaseReceivedTSH(rTSH);
                }
                base.TrExit(method);
            }
        }

        internal void InitSecurityExits()
        {
            uint method = 0x5d8;
            this.TrEntry(method);
            try
            {
                this.channelSecurityExit = new MQChannelExitHandler(this);
                this.channelSecurityExit.LoadExits(11);
                this.channelSecurityExit.InitializeExits(11, true);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public bool IsMultiplexSyncGetCapable()
        {
            return ((this.sessFlags3 & 8) != 0);
        }

        public bool IsSPISupported()
        {
            return ((this.sessFlags2 & 0x40) != 0);
        }

        public bool IsXASupported()
        {
            return ((this.sessFlags2 & 0x10) != 0);
        }

        internal bool NegotiateSecurity()
        {
            uint method = 0x5d4;
            this.TrEntry(method);
            bool flag = true;
            bool flag2 = false;
            int length = 0;
            byte[] src = null;
            bool securityRequired = false;
            bool flag4 = (this.sessFlags & 0x40) != 0;
            bool flag5 = true;
            int offset = 0;
            int maxlength = 0;
            int num6 = this.IsMultiplexingEnabled ? 0x24 : 0x1c;
            MQERD mqerd = new MQERD();
            int num7 = mqerd.GetLength();
            byte[] input = null;
            MQTSH tsh = null;
            MQTSH rTSH = null;
            int compCode = 0;
            try
            {
                while (flag)
                {
                    MQUID mquid;
                    if (!this.channelSecurityExit.securityExitDefined && flag5)
                    {
                        goto Label_0254;
                    }
                    flag5 = false;
                    if (this.channelSecurityExit.securityExitDefined)
                    {
                        try
                        {
                            if (flag2)
                            {
                                flag2 = false;
                                src = this.channelSecurityExit.ProcessSecurity(ref input, ref offset, ref length, ref maxlength, ref securityRequired);
                            }
                            else
                            {
                                length = 0;
                                offset = 0;
                                src = this.channelSecurityExit.InitSecurity(ref offset, ref length, ref maxlength, ref securityRequired);
                            }
                            if (src == null)
                            {
                                length = 0;
                                if (!flag4)
                                {
                                    goto Label_04AA;
                                }
                            }
                            goto Label_01A4;
                        }
                        catch (MQManagedClientException exception)
                        {
                            base.TrException(method, exception, 1);
                            compCode = exception.CompCode;
                            int reason = exception.Reason;
                            try
                            {
                                mqerd.erd.ReturnCode = 0x17;
                                MQTSH mqtsh3 = this.AllocateTSH(this.IsMultiplexingEnabled ? 1 : 0, 5, null, num6 + num7);
                                if (this.IsMultiplexingEnabled)
                                {
                                    mqtsh3.SetConversationId(1);
                                    mqtsh3.SetRequestId(0);
                                }
                                mqtsh3.ControlFlags1 = (byte) (mqtsh3.ControlFlags1 | 10);
                                mqtsh3.Offset = mqtsh3.WriteStruct(mqtsh3.TshBuffer, 0);
                                mqtsh3.Offset += mqerd.WriteStruct(mqtsh3.TshBuffer, mqtsh3.Offset);
                                try
                                {
                                    this.RequestSendLock();
                                    this.SendTSH(mqtsh3);
                                }
                                finally
                                {
                                    this.ReleaseSendLock();
                                }
                            }
                            catch (Exception)
                            {
                            }
                            throw exception;
                        }
                    }
                    length = 0;
                Label_01A4:
                    tsh = this.AllocateTSH(this.IsMultiplexingEnabled ? 1 : 0, 6, null, (num6 + 4) + length);
                    if (this.IsMultiplexingEnabled)
                    {
                        tsh.SetConversationId(1);
                        tsh.SetRequestId(0);
                    }
                    tsh.Offset = tsh.WriteStruct(tsh.TshBuffer, 0);
                    BitConverter.GetBytes(length).CopyTo(tsh.TshBuffer, tsh.Offset);
                    tsh.Offset += 4;
                    if ((length != 0) && (src != null))
                    {
                        Buffer.BlockCopy(src, offset, tsh.TshBuffer, tsh.Offset, length);
                        tsh.Offset += length;
                    }
                    goto Label_02D8;
                Label_0254:
                    mquid = new MQUID((byte) this.fapLevel);
                    tsh = this.AllocInitialDataTsh(8, num6 + mquid.GetLength());
                    if (this.IsMultiplexingEnabled)
                    {
                        tsh.SetConversationId(1);
                        tsh.SetRequestId(0);
                    }
                    tsh.ControlFlags1 = 1;
                    tsh.Offset = tsh.WriteStruct(tsh.TshBuffer, 0);
                    mquid.SetCurrentUser();
                    tsh.Offset += mquid.WriteStruct(tsh.TshBuffer, tsh.Offset);
                    flag = flag4;
                Label_02D8:
                    tsh.Offset = 0;
                    try
                    {
                        this.RequestSendLock();
                        this.SendTSH(tsh);
                    }
                    finally
                    {
                        this.ReleaseSendLock();
                    }
                    if (!flag)
                    {
                        continue;
                    }
                    int num2 = 0;
                    rTSH = this.ReceiveTSH(null);
                    input = new byte[rTSH.Length];
                    Buffer.BlockCopy(rTSH.TshBuffer, 0, input, 0, rTSH.Length);
                    switch (rTSH.SegmentType)
                    {
                        case 5:
                            if ((rTSH.ControlFlags1 & 2) != 0)
                            {
                                MQERD mqerd2 = new MQERD();
                                if (rTSH.Length > rTSH.GetLength())
                                {
                                    mqerd2.ReadStruct(input, num2);
                                    mqerd2.GetReasonCode();
                                }
                                compCode = 2;
                                throw mqerd2.ErrorException(this.channelName.Trim());
                            }
                            break;

                        case 6:
                        {
                            flag2 = true;
                            offset = rTSH.Offset + 4;
                            length = (rTSH.Length - rTSH.GetLength()) - 4;
                            maxlength = (input.Length - rTSH.GetLength()) - 4;
                            continue;
                        }
                        default:
                            goto Label_0430;
                    }
                    if ((rTSH.ControlFlags1 & 8) != 0)
                    {
                        compCode = 2;
                    }
                    if (compCode == 2)
                    {
                        throw new MQManagedClientException(0x20009503, rTSH.SegmentType, 0, this.channelName.Trim(), null, null, 2, 0x893);
                    }
                    if (securityRequired)
                    {
                        compCode = 2;
                        throw new MQManagedClientException(0x20009552, 0, 0, this.channelName.Trim(), null, null, 2, 0x893);
                    }
                    flag = false;
                    continue;
                Label_0430:
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = rTSH.SegmentType;
                    CommonServices.CommentInsert1 = this.channelName.Trim();
                    CommonServices.CommentInsert2 = "Unexpected flow received during uid flow..";
                    base.FFST("%Z% %W% %I% %E% %U%", "%C%", method, 1, 0x20009504, 0);
                    throw MQERD.ErrorException(13, rTSH.SegmentType, this.channelName.Trim());
                }
            }
            finally
            {
                if (rTSH != null)
                {
                    this.ReleaseReceivedTSH(rTSH);
                }
                base.TrExit(method);
            }
        Label_04AA:
            return (compCode != 2);
        }

        internal void NotifyReconnect(bool reconnect)
        {
            uint method = 0x5e5;
            this.TrEntry(method, new object[] { reconnect });
            try
            {
                if (reconnect)
                {
                    NmqiException e = new NmqiException(base.env, -1, null, 2, 0x7d9, null);
                    try
                    {
                        this.reconnectRequested = true;
                        this.AsyncFailureNotify(e, false);
                    }
                    catch (NmqiException exception2)
                    {
                        base.TrException(method, exception2);
                    }
                    catch (Exception exception3)
                    {
                        base.TrException(method, exception3);
                    }
                }
                else
                {
                    try
                    {
                        this.sessionMutex.Acquire();
                        foreach (MQSession session in this.sessions.Values)
                        {
                            session.DisableReconnect();
                        }
                    }
                    finally
                    {
                        this.sessionMutex.Release();
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal abstract void ProtocolConnect();
        internal abstract void ProtocolDisconnect();
        protected abstract void ProtocolSetHeartbeatInterval(int hbInt);
        protected abstract void ProtocolSetupAsyncMode();
        protected abstract bool ProtocolSupportsAsyncMode();
        internal void QmQuiescing()
        {
            uint method = 0x5e2;
            this.TrEntry(method);
            try
            {
                this.sessionMutex.Acquire();
                foreach (MQSession session in this.sessions.Values)
                {
                    session.QmQuiescing();
                }
            }
            finally
            {
                this.sessionMutex.Release();
                base.TrExit(method);
            }
        }

        internal abstract int Receive(byte[] buffer, int offset, int length, SocketFlags flags);
        private MQTSH ReceiveAsyncTsh()
        {
            uint method = 0x5de;
            this.TrEntry(method);
            MQTSH mqtsh = null;
            try
            {
                lock (this.asyncTshLock)
                {
                    while ((this.asyncTshQueue.Count == 0) && (this.asyncFailure == null))
                    {
                        try
                        {
                            Monitor.Wait(this.asyncTshLock);
                            continue;
                        }
                        catch (ThreadInterruptedException exception)
                        {
                            base.TrException(method, exception);
                            continue;
                        }
                    }
                    if (this.asyncFailure != null)
                    {
                        throw this.asyncFailure;
                    }
                    mqtsh = this.asyncTshQueue.Dequeue();
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return mqtsh;
        }

        internal MQTSH ReceiveTSH(MQTSH rTSHP)
        {
            uint method = 0x5df;
            this.TrEntry(method, new object[] { rTSHP });
            MQTSH mqtsh = rTSHP;
            try
            {
                if (this.IsMultiplexingEnabled)
                {
                    return this.ReceiveAsyncTsh();
                }
                int maxTransmissionSize = (int) this.maxTransmissionSize;
                if (this.IsMultiplexingEnabled)
                {
                    maxTransmissionSize += 8;
                }
                IMQCommsBuffer buffer = this.commsBufferPool.AllocateBuffer(maxTransmissionSize);
                byte[] dst = buffer.Buffer;
                if (mqtsh == null)
                {
                    mqtsh = new MQTSH(this.IsMultiplexingEnabled, buffer);
                }
                int length = dst.Length;
                int dstOffset = 0;
                int overflowbytes = 0;
                int transLength = dst.Length;
                bool flag = false;
                int num7 = mqtsh.GetLength();
                if (((dstOffset == 0) && (MQFAPConnection.overflowbytes != 0)) && (this.overflow != null))
                {
                    Buffer.BlockCopy(this.overflow, 0, dst, dstOffset, MQFAPConnection.overflowbytes);
                    overflowbytes = MQFAPConnection.overflowbytes;
                    base.TrText("Overflow used " + MQFAPConnection.overflowbytes);
                    MQFAPConnection.overflowbytes = 0;
                    if (overflowbytes >= num7)
                    {
                        flag = true;
                        mqtsh.ReadStruct(dst, dstOffset);
                        transLength = mqtsh.TransLength;
                        if (!mqtsh.CheckTSH(mqtsh.Id))
                        {
                            throw new MQManagedClientException("Invalid/Bad Data received from QueueManager.", 2, 0x8e1);
                        }
                    }
                    base.TrText("We already have " + overflowbytes + "Bytes of data with us");
                }
                int num8 = 0;
                int num9 = 0;
                while (overflowbytes < transLength)
                {
                    length = transLength - overflowbytes;
                    try
                    {
                        num8 = this.Receive(dst, overflowbytes + dstOffset, length, SocketFlags.None);
                    }
                    catch (SocketException exception)
                    {
                        base.TrException(method, exception);
                        if (this.mqgetWaitInterval <= 0)
                        {
                            throw;
                        }
                        if (exception.ErrorCode == 0x274c)
                        {
                            num9 += this.timeout;
                            if (num9 >= this.mqgetWaitInterval)
                            {
                                throw;
                            }
                        }
                        else if (exception.ErrorCode != 0x2733)
                        {
                            throw;
                        }
                        continue;
                    }
                    base.TrText("Bytes Read from Socket = " + num8);
                    if (num8 <= 0)
                    {
                        MQManagedClientException exception2 = new MQManagedClientException("Invalid number of bytes read from Socket. BytesRead = " + num8, 2, 0x7d9);
                        throw exception2;
                    }
                    overflowbytes += num8;
                    num8 = 0;
                    if (!flag && (overflowbytes >= num7))
                    {
                        flag = true;
                        mqtsh.ReadStruct(dst, dstOffset);
                        transLength = mqtsh.TransLength;
                        if (!mqtsh.CheckTSH(mqtsh.Id))
                        {
                            throw new MQManagedClientException("Invalid/Bad Data received from QueueManager.", 2, 0x7d9);
                        }
                    }
                }
                if (overflowbytes > transLength)
                {
                    MQFAPConnection.overflowbytes = overflowbytes - transLength;
                    if (MQFAPConnection.overflowbytes > this.overflowsize)
                    {
                        this.overflow = null;
                    }
                    if (this.overflow == null)
                    {
                        this.overflowsize = overflowbytes - transLength;
                        this.overflow = new byte[this.overflowsize];
                    }
                    Buffer.BlockCopy(dst, transLength + dstOffset, this.overflow, 0, MQFAPConnection.overflowbytes);
                }
                mqtsh.TshBuffer = dst;
                mqtsh.Offset = num7;
                mqtsh.Length = mqtsh.TransLength;
            }
            finally
            {
                base.TrExit(method);
            }
            return mqtsh;
        }

        public void ReleaseReceivedTSH(MQTSH rTSH)
        {
            uint method = 0x5db;
            this.TrEntry(method, new object[] { rTSH });
            try
            {
                if ((rTSH != null) && (rTSH.ParentBuffer != null))
                {
                    rTSH.ParentBuffer.Free();
                    rTSH.ParentBuffer = null;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void ReleaseSendLock()
        {
            uint method = 0x5dd;
            this.TrEntry(method);
            this.sendMutex.ReleaseMutex();
            base.TrExit(method);
        }

        internal void RemoveSession(int convId, bool informQmgr)
        {
            uint method = 0x5ce;
            this.TrEntry(method, new object[] { convId, informQmgr });
            try
            {
                if (!this.sessions.ContainsKey(convId))
                {
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = (uint) convId;
                    CommonServices.CommentInsert1 = "Hconn is not associated with this connection";
                    base.FFST("%Z% %W% %I% %E% %U%", "%C%", method, 1, 0, 0);
                    NmqiException exception = new NmqiException(base.env, -1, null, 2, 0x893, null);
                    throw exception;
                }
                bool flag = false;
                try
                {
                    this.remoteConnectionSpec.ConnectionsLock.Acquire();
                    MQSession session = this.sessions[convId];
                    session.IsEndRequested = true;
                    this.sessions.Remove(convId);
                    if (this.sessions.Count == 0)
                    {
                        this.remoteConnectionSpec.RemoveConnection(this);
                        flag = true;
                        base.TrText(method, "This is last connection and so disconnecting now..");
                        if (this.rcvThread != null)
                        {
                            this.rcvThread.SetDisconnecting();
                        }
                    }
                    else
                    {
                        this.remoteConnectionSpec.AddConnection(this);
                        base.TrText(method, "Adding Connection back to available connection list..");
                    }
                    if (this.multiplexingEnabled && informQmgr)
                    {
                        MQSOCKACT mqsockact = new MQSOCKACT();
                        int capacity = (this.IsMultiplexingEnabled ? 0x24 : 0x1c) + mqsockact.GetLength();
                        MQTSH tsh = this.AllocateTSH(2, 12, null, capacity);
                        tsh.SetConversationId(session.ConversationId);
                        tsh.SetRequestId(0);
                        mqsockact.ConversationID = session.ConversationId;
                        mqsockact.RequestID = 0;
                        mqsockact.Type = 2;
                        if (this.FapLevel >= 10)
                        {
                            mqsockact.Parm1 = 1;
                        }
                        tsh.Offset = tsh.WriteStruct(tsh.TshBuffer, tsh.Offset);
                        tsh.Offset += mqsockact.WriteStruct(tsh.TshBuffer, tsh.Offset);
                        tsh.Offset = 0;
                        try
                        {
                            this.RequestSendLock();
                            this.SendTSH(tsh);
                        }
                        finally
                        {
                            this.ReleaseSendLock();
                        }
                    }
                    if (flag)
                    {
                        this.Disconnect();
                    }
                }
                finally
                {
                    this.remoteConnectionSpec.ConnectionsLock.Release();
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void RequestSendLock()
        {
            uint method = 0x5dc;
            this.TrEntry(method);
            try
            {
                this.sendMutex.WaitOne();
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal abstract int Send(byte[] buffer, int offset, int size, int segmentType, int tshType);
        public void SendHeartbeat(int heartBeatType)
        {
            uint method = 0x5e3;
            this.TrEntry(method, new object[] { heartBeatType });
            int capacity = 0x1c;
            int tshType = this.IsMultiplexingEnabled ? 2 : 0;
            try
            {
                MQTSH mqtsh = this.AllocateTSH(tshType, 9, null, capacity);
                if ((this.fapLevel >= 10) || !this.IsMultiplexingEnabled)
                {
                    switch (heartBeatType)
                    {
                        case 1:
                            mqtsh.ControlFlags1 = (byte) (mqtsh.ControlFlags1 | 1);
                            break;

                        case 2:
                            mqtsh.ControlFlags1 = (byte) (mqtsh.ControlFlags1 & -2);
                            break;
                    }
                }
                else
                {
                    switch (heartBeatType)
                    {
                        case 1:
                            mqtsh.ControlFlags1 = (byte) (mqtsh.ControlFlags1 & -2);
                            break;

                        case 2:
                            mqtsh.ControlFlags1 = (byte) (mqtsh.ControlFlags1 | 1);
                            break;
                    }
                }
                mqtsh.WriteStruct(mqtsh.TshBuffer, mqtsh.Offset);
                this.Send(mqtsh.TshBuffer, mqtsh.Offset, mqtsh.Length, 9, tshType);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SendTSH(MQTSH tsh)
        {
            uint method = 0x5d6;
            this.TrEntry(method, new object[] { tsh });
            try
            {
                switch (tsh.TSHType)
                {
                    case 0:
                        if (this.IsMultiplexingEnabled)
                        {
                            CommonServices.SetValidInserts();
                            CommonServices.ArithInsert1 = (uint) tsh.TSHType;
                            CommonServices.CommentInsert1 = "Incorrect TSH type for communication flow";
                            CommonServices.CommentInsert2 = string.Concat(new object[] { "TSHTYPE = ", tsh.TSHType, " Expected type = ", 2 });
                            base.FFST("%Z% %W% %I% %E% %U%", "%C%", method, 1, 0x20009504, 0);
                            NmqiException exception = new NmqiException(base.env, -1, null, 2, 0x893, null);
                            throw exception;
                        }
                        break;

                    case 1:
                    case 2:
                        if (!this.IsMultiplexingEnabled)
                        {
                            CommonServices.SetValidInserts();
                            CommonServices.ArithInsert1 = (uint) tsh.TSHType;
                            CommonServices.CommentInsert1 = "Incorrect TSH type for communication flow";
                            CommonServices.CommentInsert2 = string.Concat(new object[] { "TSHTYPE = ", tsh.TSHType, " Expected type = ", 0 });
                            base.FFST("%Z% %W% %I% %E% %U%", "%C%", method, 1, 0x20009504, 0);
                            NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x893, null);
                            throw exception2;
                        }
                        break;

                    default:
                    {
                        CommonServices.SetValidInserts();
                        CommonServices.ArithInsert1 = (uint) tsh.TSHType;
                        CommonServices.CommentInsert1 = "Incorrect TSH type for communication flow";
                        CommonServices.CommentInsert2 = "TSHTYPE = " + tsh.TSHType;
                        base.FFST("%Z% %W% %I% %E% %U%", "%C%", method, 1, 0x20009504, 0);
                        NmqiException exception3 = new NmqiException(base.env, -1, null, 2, 0x893, null);
                        throw exception3;
                    }
                }
                this.Send(tsh.TshBuffer, tsh.Offset, tsh.Length, tsh.SegmentType, tsh.TSHType);
            }
            finally
            {
                tsh.ParentBuffer.Free();
                tsh.ParentBuffer = null;
                base.TrExit(method);
            }
        }

        internal void SetUpAsyncMode()
        {
            uint method = 0x5d3;
            this.TrEntry(method);
            try
            {
                this.multiplexingEnabled = true;
                this.ProtocolSetupAsyncMode();
                this.rcvThread = new MQRcvThread(base.env, this);
                Thread thread = new Thread(new ThreadStart(this.rcvThread.Run));
                thread.IsBackground = true;
                thread.Start();
                base.TrText(method, "Receiver thread is now started ..");
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public string ChannelName
        {
            get
            {
                string channelName = this.channelName;
                if ((this.channelName == null) && (this.negotiatedChannel != null))
                {
                    channelName = this.negotiatedChannel.ChannelName;
                }
                return channelName;
            }
        }

        internal MQChannelDefinition ClientConn
        {
            get
            {
                return this.clientConn;
            }
        }

        internal int CmdLevel
        {
            get
            {
                return this.remoteCmdLevel;
            }
        }

        internal MQCommsBufferPool CommsBufferPool
        {
            get
            {
                return this.commsBufferPool;
            }
        }

        public int FapLevel
        {
            get
            {
                return this.fapLevel;
            }
            set
            {
                this.fapLevel = value;
            }
        }

        internal MQTagPool IdTagPool
        {
            get
            {
                if (this.remoteQMID == null)
                {
                    NmqiException exception = new NmqiException(base.env, -1, null, 2, 0x893, null);
                    throw exception;
                }
                if (this.idTagPool == null)
                {
                    this.idTagPool = MQTagPool.GetInstance(0x18, this.RemoteQMID);
                }
                return this.idTagPool;
            }
        }

        internal bool IsConnected
        {
            get
            {
                return this.connected;
            }
        }

        public bool IsMultiplexingEnabled
        {
            get
            {
                return this.multiplexingEnabled;
            }
        }

        internal bool IsReconnectRequested
        {
            get
            {
                return this.reconnectRequested;
            }
        }

        public int LocalCcsid
        {
            get
            {
                return this.localCcsid;
            }
        }

        internal long MaximumMessageLength
        {
            get
            {
                return (long) this.NegotiatedChannel.MaxMessageLength;
            }
        }

        public int MaxTransmissionSize
        {
            get
            {
                if (this.IsMultiplexingEnabled)
                {
                    return (((int) this.maxTransmissionSize) & -4);
                }
                return (int) this.maxTransmissionSize;
            }
        }

        public MQFAP MQFap
        {
            get
            {
                return this.fap;
            }
            set
            {
                this.fap = value;
            }
        }

        public MQChannelDefinition NegotiatedChannel
        {
            get
            {
                return this.negotiatedChannel;
            }
            set
            {
                this.negotiatedChannel = value;
            }
        }

        internal int Platform
        {
            get
            {
                return this.remotePlatform;
            }
        }

        internal string ProductId
        {
            get
            {
                return this.remoteProductId;
            }
        }

        internal int RemoteEncoding
        {
            get
            {
                return this.remoteEncoding;
            }
            set
            {
                this.remoteEncoding = value;
            }
        }

        internal int RemoteMQEncoding
        {
            get
            {
                return this.remoteMQEncoding;
            }
        }

        public string RemoteQmgrName
        {
            get
            {
                return Encoding.ASCII.GetString(this.NegotiatedChannel.QMgrName);
            }
        }

        internal string RemoteQMID
        {
            get
            {
                return Encoding.ASCII.GetString(this.remoteQMID);
            }
        }

        public bool SupportsReconnection
        {
            get
            {
                bool flag = false;
                if ((this.fapLevel >= 10) && this.multiplexingEnabled)
                {
                    flag = true;
                }
                return flag;
            }
        }
    }
}

