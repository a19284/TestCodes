namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Collections;
    using System.Threading;

    public class MQProxyQueue : NmqiObject
    {
        private byte[] appBuffer;
        private int appBufferLength;
        private int appBufferUsed;
        private int appCompCode;
        private int appDataLength;
        private MQGetMessageOptions appGmo;
        private MQMessageDescriptor appMD;
        private int appReason;
        private int asyncSelectionIndex;
        private long backlog;
        private MQCBC callbackCBC;
        private bool cbAlreadyRegistered;
        private int codedCharSetId;
        private Exception connectionFailure;
        private int currentBytes;
        private byte[] currentCorrelId;
        private bool currentCorrelIdSet;
        private byte[] currentGroupId;
        private bool currentGroupIdSet;
        private byte[] currentMsgId;
        private bool currentMsgIdSet;
        private byte[] currentMsgToken;
        private bool currentMsgTokenSet;
        private int encoding;
        private int eventsRaised;
        private int getMsgOptions;
        private Lock getterEventMonitor;
        private ManagedHconn hconn;
        private int highwaterMark;
        private ManagedHobj hobj;
        private int immutableGmoOpts;
        internal int inprocess;
        private MQProxyMessage lastBrowsed;
        private long lastCheckPurge;
        private int lastCompCode;
        private int lastReason;
        private int lastReceivedIndex;
        private string lastResolvedQName;
        private int lowwaterMark;
        private int matchOptions;
        private int maxMsgLength;
        private ArrayList mqCBDList;
        private MQGetMessageOptions mqcbGmo;
        private MQMessageDescriptor mqcbMD;
        private short mqmdVersion;
        private int msgSeqNumber;
        private long msgsPurged;
        private long noMsgTime;
        private int offset;
        private int processedBytes;
        private MQProxyMessage progressMsg;
        private int progressMsgIndex;
        private int progressMsgOffset;
        private Lock proxyQueueLock;
        private int purgeTime;
        private MQProxyMessage queueNewest;
        private MQProxyMessage queueOldest;
        private int queueStatus;
        private MQProxyMessage queueTop;
        private int receivedBytes;
        private int requestedBytes;
        private const int rpqGMO_BROWSE_OPTS = 0x830;
        private const int rpqIMMUTABLE_GMO_OPTS = 0x1ff7d044;
        private const int rpqNONSTREAMING_GMO_OPTS = 0x78a;
        private const string sccsid = "@(#) lib/dotnet/pc/winnt/nmqi/managed/MQProxyQueue.cs, dotnet, p000, p000-L091124  1.6 09/11/19 13:02:00";
        private int selectionIndex;
        private DateTime startTime;
        private int status;
        private Lock statusSync;
        private NmqiEnvironment sysenv;

        internal MQProxyQueue(NmqiEnvironment env, ManagedHconn hconn) : base(env)
        {
            this.statusSync = new Lock();
            this.proxyQueueLock = new Lock();
            this.getterEventMonitor = new Lock();
            this.currentMsgId = new byte[0x18];
            this.currentCorrelId = new byte[0x18];
            this.currentGroupId = new byte[0x18];
            this.currentMsgToken = new byte[0x10];
            this.highwaterMark = 10;
            this.lowwaterMark = 20;
            this.mqCBDList = new ArrayList();
            this.startTime = new DateTime(0x7b2, 1, 1, 0, 0, 0, 0);
            base.TrConstructor("@(#) lib/dotnet/pc/winnt/nmqi/managed/MQProxyQueue.cs, dotnet, p000, p000-L091124  1.6 09/11/19 13:02:00", new object[] { env, hconn });
            this.hconn = hconn;
            this.sysenv = env;
            this.mqmdVersion = 1;
            this.status = 0x800;
            MQClientCfg cfg = this.sysenv.Cfg;
            this.highwaterMark = cfg.GetIntValue(MQClientCfg.MESSAGEBUFFER_MAXIMUMSIZE);
            this.highwaterMark *= 0x400;
            this.lowwaterMark = cfg.GetIntValue(MQClientCfg.MESSAGEBUFFER_UPDATEPERCENTAGE);
            this.lowwaterMark = (this.lowwaterMark * this.highwaterMark) / 100;
            this.purgeTime = cfg.GetIntValue(MQClientCfg.MESSAGEBUFFER_PURGETIME);
            this.backlog = 0L;
            this.lastCheckPurge = this.GetTimeInSeconds();
            if (this.purgeTime > 0)
            {
                base.TrText("@(#) lib/dotnet/pc/winnt/nmqi/managed/MQProxyQueue.cs, dotnet, p000, p000-L091124  1.6 09/11/19 13:02:00 Last Purge check was at : " + this.lastCheckPurge);
            }
            this.queueStatus = 0;
            this.msgsPurged = 0L;
        }

        public void AddCbd(MQCBD callbackDesc)
        {
            uint method = 0x600;
            this.TrEntry(method, new object[] { callbackDesc });
            if (this.mqCBDList != null)
            {
                this.mqCBDList.Add(callbackDesc);
            }
            base.TrExit(method);
        }

        internal void AddMessage(MQTSH tsh, MQASYNC_MESSAGE async)
        {
            uint method = 0x215;
            this.TrEntry(method, new object[] { tsh, async });
            bool flag = false;
            try
            {
                MQSession session = this.hconn.Session;
                this.RequestMutex();
                flag = true;
                byte[] tshBuffer = tsh.TshBuffer;
                int offset = tsh.Offset;
                int length = tsh.Length;
                if ((this.status & 0x1000) != 0)
                {
                    this.progressMsgIndex = 0;
                    if (this.progressMsg != null)
                    {
                        this.FreeMessage(this.progressMsg);
                        this.progressMsg = null;
                    }
                    return;
                }
                int segmentIndex = async.asyncMsg.segmentIndex;
                int segmentLength = async.asyncMsg.segmentLength;
                if ((tsh.ControlFlags1 & 0x10) != 0)
                {
                    this.lastResolvedQName = async.resolvedQName;
                }
                if (segmentIndex != this.progressMsgIndex)
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x893, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                this.progressMsgIndex++;
                try
                {
                    this.statusSync.Acquire();
                    if ((((this.status & 0x4000000) != 0) && ((this.status & 0x400) == 0)) && (((tsh.ControlFlags1 & 0x10) != 0) && (async.asyncMsg.totalMsgLength <= this.appBufferLength)))
                    {
                        this.status |= 0x8000000;
                    }
                }
                finally
                {
                    this.statusSync.Release();
                }
                if ((this.status & 0x8000000) != 0)
                {
                    int num5 = 0;
                    if ((tsh.ControlFlags1 & 0x10) != 0)
                    {
                        num5 = async.asyncMsg.totalMsgLength + async.msgDescriptor.GetVersionLength();
                        this.receivedBytes += num5;
                        this.currentBytes += num5;
                        this.CopyMDToUserMD(this.appMD, async.msgDescriptor);
                        if (segmentLength > 0)
                        {
                            num5 = Math.Min(this.appBufferLength, segmentLength);
                        }
                        else
                        {
                            num5 = 0;
                        }
                        if (async.asyncMsg.reasonCode != 0)
                        {
                            this.appCompCode = 1;
                        }
                        else
                        {
                            this.appCompCode = 0;
                        }
                        this.appReason = async.asyncMsg.reasonCode;
                        this.appDataLength = async.asyncMsg.actualMsgLength;
                        this.appGmo.ResolvedQueueName = this.lastResolvedQName;
                        if (this.appGmo.Version >= 2)
                        {
                            this.appGmo.GroupStatus = 0x20;
                            if ((async.asyncMsg.status & 0x10) != 0)
                            {
                                this.appGmo.GroupStatus = 0x47;
                            }
                            if ((async.asyncMsg.status & 0x20) != 0)
                            {
                                this.appGmo.GroupStatus = 0x4c;
                            }
                            if ((async.asyncMsg.status & 4) != 0)
                            {
                                this.appGmo.SegmentStatus = 0x53;
                            }
                            if ((async.asyncMsg.status & 8) != 0)
                            {
                                this.appGmo.SegmentStatus = 0x4c;
                            }
                            if ((async.asyncMsg.status & 2) != 0)
                            {
                                this.appGmo.Segmentation = 0x41;
                            }
                            if (this.appGmo.Version >= 3)
                            {
                                Buffer.BlockCopy(async.asyncMsg.msgToken, 0, this.appGmo.MsgToken, 0, 0x10);
                                this.appGmo.ReturnedLength = async.asyncMsg.totalMsgLength;
                            }
                        }
                    }
                    else
                    {
                        num5 = this.appBufferLength - this.appBufferUsed;
                        if ((num5 > 0) && (segmentLength > 0))
                        {
                            num5 = Math.Min(num5, segmentLength);
                        }
                        else
                        {
                            num5 = 0;
                        }
                    }
                    Buffer.BlockCopy(tshBuffer, offset, this.appBuffer, this.appBufferUsed, num5);
                    this.appBufferUsed += num5;
                    if ((tsh.ControlFlags1 & 0x20) == 0)
                    {
                        return;
                    }
                    this.hconn.GlobalMessageIndex = async.asyncMsg.globalMessageIndex;
                    this.progressMsgIndex = 0;
                    this.lastReceivedIndex = async.asyncMsg.messageIndex;
                    if (((this.status & 2) == 0) || ((this.status & 4) != 0))
                    {
                        return;
                    }
                    lock (this.getterEventMonitor)
                    {
                        this.statusSync.Acquire();
                        this.status |= 4;
                        this.statusSync.Release();
                        Monitor.PulseAll(this.getterEventMonitor);
                        return;
                    }
                }
                if ((tsh.ControlFlags1 & 0x10) == 0)
                {
                    goto Label_0767;
                }
                if (segmentIndex != 0)
                {
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = tsh.SegmentType;
                    CommonServices.CommentInsert1 = this.hconn.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                    CommonServices.CommentInsert2 = "Unexpected Segmentindex received as AsyncMessage(PQ)";
                    base.FFST("@(#) lib/dotnet/pc/winnt/nmqi/managed/MQProxyQueue.cs, dotnet, p000, p000-L091124  1.6 09/11/19 13:02:00", "%C%", method, 1, 0x20009213, 0);
                    NmqiException exception2 = new NmqiException(base.env, -1, new string[] { "Segment index non-zero." }, 2, 0x893, null);
                    base.TrException(method, exception2);
                    throw exception2;
                }
                MQProxyMessage message = this.AllocMessage(async.asyncMsg.totalMsgLength);
                Buffer.BlockCopy(async.asyncMsg.msgToken, 0, message.MsgToken, 0, 0x10);
                message.MsgDesc = async.msgDescriptor;
                message.MsgDescByteSize = async.msgDescriptor.GetVersionLength();
                Buffer.BlockCopy(tshBuffer, offset, message.MsgData, 0, segmentLength);
                message.ActualMsgLength = async.asyncMsg.actualMsgLength;
                message.MsgLength = async.asyncMsg.totalMsgLength;
                message.SelectionIndex = async.asyncMsg.segmentIndex;
                message.Reason = async.asyncMsg.reasonCode;
                message.Status = async.asyncMsg.status;
                message.ResolvedQName = async.resolvedQName;
                if (this.purgeTime > 0)
                {
                    message.AddedTime = this.GetTimeInSeconds();
                    if ((this.queueOldest != null) && (message.AddedTime >= (this.lastCheckPurge + this.purgeTime)))
                    {
                        this.CheckPurge(message.AddedTime);
                    }
                }
                else
                {
                    message.AddedTime = 0L;
                }
                if (message.Reason != 0)
                {
                    message.CompCode = 1;
                }
                else
                {
                    message.CompCode = 0;
                }
                message.Type = 1;
                if ((this.status & 0x10000000) == 0x10000000)
                {
                    switch ((this.getMsgOptions & 0x1006))
                    {
                        case 0:
                        {
                            string productId = this.hconn.ParentConnection.ProductId;
                            if ((productId != null) && productId.StartsWith("MQMV"))
                            {
                                message.SetTransactional();
                            }
                            break;
                        }
                        case 2:
                            message.SetTransactional();
                            break;

                        case 0x1000:
                            goto Label_0656;
                    }
                }
                goto Label_066C;
            Label_0656:
                if (message.MsgDesc.Persistence == 1)
                {
                    message.SetTransactional();
                }
            Label_066C:
                this.progressMsg = message;
                this.progressMsgOffset = segmentLength;
                if ((this.status & 0x40000000) != 0)
                {
                    this.statusSync.Acquire();
                    this.status &= -1073741825;
                    this.statusSync.Release();
                }
                if ((async.asyncMsg.status & 1) != 0)
                {
                    this.statusSync.Acquire();
                    this.status |= 0x2000;
                    this.statusSync.Release();
                    if ((message.Reason != 0) && (message.Reason != 0x81f))
                    {
                        this.statusSync.Acquire();
                        this.status |= 0x8000;
                        this.statusSync.Release();
                    }
                }
                else
                {
                    this.statusSync.Acquire();
                    this.status &= -40961;
                    this.statusSync.Release();
                    this.lastCompCode = 0;
                }
                goto Label_0824;
            Label_0767:
                if (this.progressMsg == null)
                {
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = tsh.SegmentType;
                    CommonServices.CommentInsert1 = this.hconn.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                    CommonServices.CommentInsert2 = "No message in progress.(MQ)";
                    base.FFST("@(#) lib/dotnet/pc/winnt/nmqi/managed/MQProxyQueue.cs, dotnet, p000, p000-L091124  1.6 09/11/19 13:02:00", "%C%", method, 1, 0x20009527, 0);
                    int num8 = 2;
                    int num9 = 0x893;
                    MQManagedClientException exception3 = new MQManagedClientException("No message in progress. CompCode : " + num8.ToString() + " Reason : " + num9.ToString(), 2, 0x893);
                    throw exception3;
                }
                Buffer.BlockCopy(tshBuffer, offset, this.progressMsg.MsgData, this.progressMsgOffset, segmentLength);
                this.progressMsgOffset += segmentLength;
            Label_0824:
                if ((tsh.ControlFlags1 & 0x20) != 0)
                {
                    MQProxyMessage progressMsg = this.progressMsg;
                    int num6 = progressMsg.MsgLength + progressMsg.MsgDescByteSize;
                    this.progressMsg = null;
                    this.receivedBytes += num6;
                    this.currentBytes += num6;
                    this.progressMsgIndex = 0;
                    this.AddPhysicalMessage(progressMsg);
                    this.hconn.GlobalMessageIndex = async.asyncMsg.globalMessageIndex;
                    this.lastReceivedIndex = async.asyncMsg.messageIndex;
                }
            }
            finally
            {
                if (flag)
                {
                    this.ReleaseMutex();
                }
                base.TrExit(method);
            }
        }

        private void AddPhysicalMessage(MQProxyMessage message)
        {
            uint method = 0x211;
            this.TrEntry(method, new object[] { message });
            try
            {
                this.proxyQueueLock.AssertOnCurrentThreadHoldsLock();
                bool flag = false;
                message.Newer = null;
                message.Older = this.queueNewest;
                if (this.queueNewest != null)
                {
                    this.queueNewest.Newer = message;
                }
                this.queueNewest = message;
                if (this.queueTop == null)
                {
                    this.queueTop = message;
                    flag = true;
                }
                if (this.queueOldest == null)
                {
                    this.queueOldest = message;
                }
                if (((flag && (this.MqcbCBD != null)) && ((this.MqcbCBD != null) && (this.MqcbCBD.MqConsumer != null))) && ((this.status & 0x40000) == 0))
                {
                    base.TrText(method, "Checking for Dispatchable status on the PQ");
                    this.hconn.CheckDispatchable(this);
                }
                if (((this.status & 2) != 0) && ((this.status & 4) == 0))
                {
                    lock (this.getterEventMonitor)
                    {
                        base.TrText(method, "Pulsing getter on this PQ");
                        this.statusSync.Acquire();
                        this.status |= 4;
                        this.statusSync.Release();
                        Monitor.PulseAll(this.getterEventMonitor);
                    }
                }
                base.TrText(method, "Message added to Queue");
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private MQProxyMessage AllocMessage(int msgLength)
        {
            uint method = 0x204;
            this.TrEntry(method, new object[] { msgLength });
            MQProxyMessage message = null;
            try
            {
                message = new MQProxyMessage(msgLength);
            }
            finally
            {
                base.TrExit(method);
            }
            return message;
        }

        private bool BitSet(int value, int bit)
        {
            uint method = 0x218;
            this.TrEntry(method, new object[] { value, bit });
            bool result = (value & bit) == bit;
            base.TrExit(method, result);
            return result;
        }

        internal bool CallbackOnEmpty()
        {
            uint method = 0x5ff;
            this.TrEntry(method);
            bool result = (this.status & 0x1000000) != 0;
            base.TrExit(method, result);
            return result;
        }

        internal void CallConsumer(int callType, int compCode, int reasonP)
        {
            uint method = 0x5eb;
            this.TrEntry(method, new object[] { callType, compCode, reasonP });
            try
            {
                int num2 = reasonP;
                this.callbackCBC.HOBJ = this.Hobj;
                this.callbackCBC.CallbackArea = this.MqcbCBD.CallbackArea;
                switch (callType)
                {
                    case 1:
                        this.statusSync.Acquire();
                        this.status |= 0x400000;
                        this.status &= -8388609;
                        this.statusSync.Release();
                        if ((this.MqcbCBD.Options & 1) != 0)
                        {
                            break;
                        }
                        return;

                    case 2:
                        this.statusSync.Acquire();
                        this.status |= 0x800000;
                        this.statusSync.Release();
                        break;

                    case 3:
                    case 4:
                    case 5:
                        break;

                    default:
                        return;
                }
                this.callbackCBC.Flags = 0;
                this.callbackCBC.CallType = callType;
                this.callbackCBC.State = 0;
                if ((this.status & 0x80) != 0)
                {
                    if (this.IsEmpty())
                    {
                        this.callbackCBC.Flags |= 1;
                    }
                    if (num2 == 0x7f1)
                    {
                        if (!this.IsEmpty())
                        {
                            num2 = 0x9d5;
                        }
                        else
                        {
                            num2 = 0x9d6;
                        }
                    }
                }
                this.callbackCBC.CompCode = compCode;
                this.callbackCBC.Reason = num2;
                switch (num2)
                {
                    case 0x7ef:
                    case 0x7f1:
                    case 0x81f:
                    case 0x85f:
                    case 0x860:
                    case 0x861:
                    case 0x862:
                    case 0x866:
                    case 0x83d:
                    case 0x83e:
                    case 0x83f:
                    case 0x840:
                    case 0x841:
                    case 0x842:
                    case 0x843:
                    case 0x844:
                    case 0x845:
                    case 0x846:
                    case 0x847:
                    case 0x848:
                    case 0x871:
                    case 0x88e:
                    case 0x8c1:
                    case 0x8c2:
                    case 0x8c3:
                    case 0x8c4:
                    case 0x8c5:
                    case 0x89a:
                    case 0x8a1:
                    case 0x8e0:
                    case 0x9d5:
                    case 0x9d6:
                    case 0x9be:
                        this.callbackCBC.State = 0;
                        break;

                    case 0x7f2:
                    case 0x7e8:
                    case 0x820:
                    case 0x80e:
                    case 0x8c6:
                    case 0x8cf:
                    case 0x8e2:
                    case 0x8f9:
                    case 0x92f:
                    case 0x930:
                    case 0x931:
                    case 0x932:
                    case 0x933:
                    case 0x9fd:
                    case 0x946:
                        this.statusSync.Acquire();
                        this.status |= 0x40000;
                        this.callbackCBC.State = 2;
                        this.statusSync.Release();
                        break;

                    case 0x7f9:
                    case 0x804:
                    case 0x817:
                    case 0x855:
                    case 0x835:
                    case 0x836:
                    case 0x852:
                    case 0x886:
                    case 0x887:
                    case 0x890:
                    case 0x891:
                    case 0x8a0:
                    case 0x8b8:
                    case 0x926:
                    case 0x929:
                    case 0x92a:
                    case 0x92b:
                    case 0x92c:
                    case 0x92d:
                    case 0x945:
                        this.statusSync.Acquire();
                        this.status |= 0x40000;
                        this.callbackCBC.State = 3;
                        this.statusSync.Release();
                        break;

                    case 0x7d9:
                    case 0x872:
                    case 0x893:
                    case 0x89b:
                    case 0x89c:
                        this.statusSync.Acquire();
                        this.status |= 0x40000;
                        this.callbackCBC.State = 4;
                        this.statusSync.Release();
                        break;

                    case 0x7e0:
                        this.callbackCBC.State = 1;
                        break;

                    case 0:
                        break;

                    default:
                        if (compCode == 2)
                        {
                            this.statusSync.Acquire();
                            this.status |= 0x40000;
                            this.callbackCBC.State = 3;
                            this.statusSync.Release();
                        }
                        break;
                }
                MQConsumer mqConsumer = this.MqcbCBD.MqConsumer;
                try
                {
                    mqConsumer.Consumer(this.hconn.ParentPhconn, null, null, null, this.callbackCBC);
                }
                catch (Exception exception)
                {
                    base.TrException(method, exception);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void CheckGetMsgOptions(bool asyncConsumer, MQMessageDescriptor msgDesc, MQGetMessageOptions getMsgOptions, ref int compCode, ref int reason)
        {
            uint method = 0x20b;
            this.TrEntry(method, new object[] { asyncConsumer, msgDesc, getMsgOptions, (int) compCode, (int) reason });
            try
            {
                this.proxyQueueLock.AssertOnCurrentThreadHoldsLock();
                bool flag = false;
                bool flag2 = false;
                this.CheckGmoOptions(getMsgOptions, asyncConsumer, msgDesc.Version);
                int options = getMsgOptions.Options;
                if ((options & 0x830) != 0)
                {
                    if ((this.hobj.OpenOptions & 8) == 0)
                    {
                        compCode = 2;
                        reason = 0x7f4;
                        NmqiException ex = new NmqiException(base.env, -1, null, compCode, reason, null);
                        base.TrException(method, ex);
                        throw ex;
                    }
                }
                else if ((this.hobj.OpenOptions & 7) == 0)
                {
                    compCode = 2;
                    reason = 0x7f5;
                    NmqiException exception2 = new NmqiException(base.env, -1, null, compCode, reason, null);
                    base.TrException(method, exception2);
                    throw exception2;
                }
                flag = ((this.status & 0x4000) != 0) && ((options & 0x78a) == 0);
                if ((asyncConsumer && ((options & 0x10) != 0)) && ((options & 0x1300020) == 0))
                {
                    flag2 = true;
                }
                if (((options & 0x830) != 0) || ((options & 4) != 0))
                {
                    this.statusSync.Acquire();
                    this.status &= -268435457;
                    this.statusSync.Release();
                }
                else
                {
                    this.statusSync.Acquire();
                    this.status |= 0x10000000;
                    this.statusSync.Release();
                }
                if ((this.status & 0x200) == 0)
                {
                    goto Label_041F;
                }
                if ((this.status & 0x400) != 0)
                {
                    goto Label_0288;
                }
                this.getMsgOptions = options;
                if ((options & 0x830) != 0)
                {
                    try
                    {
                        this.statusSync.Acquire();
                        this.status |= 0x20;
                        this.status &= -131073;
                        goto Label_0228;
                    }
                    finally
                    {
                        this.statusSync.Release();
                    }
                }
                try
                {
                    this.statusSync.Acquire();
                    this.status |= 0x20000;
                    this.status &= -33;
                }
                finally
                {
                    this.statusSync.Release();
                }
            Label_0228:
                if (this.encoding != msgDesc.Encoding)
                {
                    this.status |= 0x10000;
                    this.encoding = msgDesc.Encoding;
                }
                if (this.codedCharSetId != msgDesc.CodedCharacterSetId)
                {
                    this.status |= 0x10000;
                    this.codedCharSetId = msgDesc.CodedCharacterSetId;
                }
                return;
            Label_0288:
                if (!flag)
                {
                    compCode = 2;
                    reason = 0x7fe;
                    base.TrText(method, "Bad GMO option. Read Ahead enabled for Hobj");
                    NmqiException exception3 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                    base.TrException(method, exception3);
                    throw exception3;
                }
                if ((options & 0x830) != 0)
                {
                    if ((this.status & 0x20000) != 0)
                    {
                        compCode = 2;
                        reason = 0x999;
                        NmqiException exception4 = new NmqiException(base.env, -1, null, compCode, reason, null);
                        base.TrException(method, exception4);
                        throw exception4;
                    }
                }
                else if ((this.status & 0x20) != 0)
                {
                    compCode = 2;
                    reason = 0x999;
                    NmqiException exception5 = new NmqiException(base.env, -1, null, compCode, reason, null);
                    base.TrException(method, exception5);
                    throw exception5;
                }
                if ((options & 0x1ff7d044) != this.immutableGmoOpts)
                {
                    compCode = 2;
                    reason = 0x7fe;
                    base.TrText(method, "GMO option changed. Read-ahead enabled for Hobj");
                    NmqiException exception6 = new NmqiException(base.env, -1, null, compCode, reason, null);
                    base.TrException(method, exception6);
                    throw exception6;
                }
                if (((options & 0x4000) != 0) && ((msgDesc.Encoding != this.encoding) || (msgDesc.CodedCharacterSetId != this.codedCharSetId)))
                {
                    compCode = 2;
                    reason = 0x999;
                    base.TrText(method, "Encoding or CCSID changed. Read-ahead enabled for Hobj");
                    NmqiException exception7 = new NmqiException(base.env, -1, null, compCode, reason, null);
                    base.TrException(method, exception7);
                    throw exception7;
                }
                if (!flag2)
                {
                    goto Label_05BD;
                }
                try
                {
                    this.statusSync.Acquire();
                    this.status &= -1025;
                    goto Label_05BD;
                }
                finally
                {
                    this.statusSync.Release();
                }
            Label_041F:
                if ((getMsgOptions.Options & 0x830) != 0)
                {
                    try
                    {
                        this.statusSync.Acquire();
                        this.status |= 0x20;
                        this.status &= -131073;
                        goto Label_04A1;
                    }
                    finally
                    {
                        this.statusSync.Release();
                    }
                }
                try
                {
                    this.statusSync.Acquire();
                    this.status |= 0x20000;
                    this.status &= -33;
                }
                finally
                {
                    this.statusSync.Release();
                }
            Label_04A1:
                if (flag && !flag2)
                {
                    try
                    {
                        this.statusSync.Acquire();
                        this.status |= 0x400;
                        goto Label_04FD;
                    }
                    finally
                    {
                        this.statusSync.Release();
                    }
                }
                try
                {
                    this.statusSync.Acquire();
                    this.status &= -1025;
                }
                finally
                {
                    this.statusSync.Release();
                }
            Label_04FD:
                if (((getMsgOptions.Options & 0x4000) != 0) && ((this.encoding != msgDesc.Encoding) || (this.codedCharSetId != msgDesc.CodedCharacterSetId)))
                {
                    this.status |= 0x10000;
                }
                this.encoding = msgDesc.Encoding;
                this.codedCharSetId = msgDesc.CodedCharacterSetId;
                this.immutableGmoOpts = options & 0x1ff7d044;
                if ((this.status & 0x4000) == 0x4000)
                {
                    if (flag)
                    {
                        this.queueStatus |= 1;
                    }
                    else
                    {
                        this.queueStatus |= 4;
                    }
                }
                base.TrData(method, 0, "Streaming status", BitConverter.GetBytes(flag));
                base.TrData(method, 0, "Immutable GMO Options", BitConverter.GetBytes(this.immutableGmoOpts));
            Label_05BD:
                this.getMsgOptions = options;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void CheckGmoOptions(MQGetMessageOptions getMsgOpts, bool asyncConsumer, int mdVersion)
        {
            uint method = 0x20a;
            this.TrEntry(method, new object[] { getMsgOpts, asyncConsumer, mdVersion });
            int options = getMsgOpts.Options;
            int version = getMsgOpts.Version;
            int matchOptions = getMsgOpts.MatchOptions;
            int waitInterval = getMsgOpts.WaitInterval;
            try
            {
                if ((version < 1) || (version > 4))
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x8d0, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                if ((options & -536346488) != 0)
                {
                    NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                    base.TrException(method, exception2);
                    throw exception2;
                }
                if ((version > 1) && ((matchOptions & -64) != 0))
                {
                    NmqiException exception3 = new NmqiException(base.env, -1, null, 2, 0x8c7, null);
                    base.TrException(method, exception3);
                    throw exception3;
                }
                if (((version > 1) && ((matchOptions & 0x20) != 0)) && (version < 3))
                {
                    NmqiException exception4 = new NmqiException(base.env, -1, null, 2, 0x8d0, null);
                    base.TrException(method, exception4);
                    throw exception4;
                }
                if (options != 0)
                {
                    if (((options & 2) != 0) && ((options & 0x1004) != 0))
                    {
                        NmqiException exception5 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                        base.TrException(method, exception5);
                        throw exception5;
                    }
                    if (((options & 4) != 0) && ((options & 0x1000) != 0))
                    {
                        NmqiException exception6 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                        base.TrException(method, exception6);
                        throw exception6;
                    }
                    if (((options & 0x1000) != 0) && ((options & 0x10000) != 0))
                    {
                        NmqiException exception7 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                        base.TrException(method, exception7);
                        throw exception7;
                    }
                    int num2 = 0;
                    if ((options & 0x830) != 0)
                    {
                        if ((getMsgOpts.Options & 0x800) != 0)
                        {
                            num2++;
                        }
                        if ((getMsgOpts.Options & 0x20) != 0)
                        {
                            num2++;
                        }
                        if ((getMsgOpts.Options & 0x10) != 0)
                        {
                            num2++;
                        }
                    }
                    if (asyncConsumer)
                    {
                        if ((options & 0x800) == 0)
                        {
                            if (((options & 0x30) == 0x30) && ((options & 0x300000) != 0))
                            {
                                NmqiException exception9 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                                base.TrException(method, exception9);
                                throw exception9;
                            }
                        }
                        else if ((options & 0x30) != 0)
                        {
                            NmqiException exception8 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                            base.TrException(method, exception8);
                            throw exception8;
                        }
                    }
                    else if (num2 > 1)
                    {
                        NmqiException exception10 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                        base.TrException(method, exception10);
                        throw exception10;
                    }
                    if (((options & 0x100) > 0) && (num2 > 0))
                    {
                        NmqiException exception11 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                        base.TrException(method, exception11);
                        throw exception11;
                    }
                    if (((options & 0x1002) != 0) && (num2 > 0))
                    {
                        NmqiException exception12 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                        base.TrException(method, exception12);
                        throw exception12;
                    }
                    if (((options & 0x200) > 0) && (num2 == 0))
                    {
                        NmqiException exception13 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                        base.TrException(method, exception13);
                        throw exception13;
                    }
                    if (((options & 0x400) > 0) && ((options & -1029) > 0))
                    {
                        NmqiException exception14 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                        base.TrException(method, exception14);
                        throw exception14;
                    }
                    if (((options & 0x1f00000) != 0) && ((options & 0x78200) != 0))
                    {
                        NmqiException exception15 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                        base.TrException(method, exception15);
                        throw exception15;
                    }
                    if (((getMsgOpts.Options & 0xf00000) != 0) && (num2 == 0))
                    {
                        NmqiException exception16 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                        base.TrException(method, exception16);
                        throw exception16;
                    }
                    if (((options & 0x800000) != 0) && ((options & 0x1100000) != 0))
                    {
                        NmqiException exception17 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                        base.TrException(method, exception17);
                        throw exception17;
                    }
                    if (((options & 0x400000) != 0) && ((options & 0x1200000) != 0))
                    {
                        NmqiException exception18 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                        base.TrException(method, exception18);
                        throw exception18;
                    }
                    if ((options & 0x8000) != 0)
                    {
                        if (version < 2)
                        {
                            NmqiException exception19 = new NmqiException(base.env, -1, null, 2, 0x8d0, null);
                            base.TrException(method, exception19);
                            throw exception19;
                        }
                        if (mdVersion < 2)
                        {
                            NmqiException exception20 = new NmqiException(base.env, -1, null, 2, 0x8d1, null);
                            base.TrException(method, exception20);
                            throw exception20;
                        }
                    }
                    if (((version > 1) && ((options & 0x409) != 0)) && ((matchOptions & 0x20) != 0))
                    {
                        NmqiException exception21 = new NmqiException(base.env, -1, null, 2, 0x91b, null);
                        base.TrException(method, exception21);
                        throw exception21;
                    }
                    if (((getMsgOpts.Options & 0x8000000) != 0) && ((getMsgOpts.Options & 0x6000000) != 0))
                    {
                        NmqiException exception22 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                        base.TrException(method, exception22);
                        throw exception22;
                    }
                    if (((getMsgOpts.Options & 0x4000000) != 0) && ((getMsgOpts.Options & 0x12000000) != 0))
                    {
                        NmqiException exception23 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                        base.TrException(method, exception23);
                        throw exception23;
                    }
                    if (((getMsgOpts.Options & 0x2000000) != 0) && ((getMsgOpts.Options & 0x10000000) != 0))
                    {
                        NmqiException exception24 = new NmqiException(base.env, -1, null, 2, 0x7fe, null);
                        base.TrException(method, exception24);
                        throw exception24;
                    }
                    if (((getMsgOpts.Options & 0x8000000) != 0) && (getMsgOpts.Version < 4))
                    {
                        NmqiException exception25 = new NmqiException(base.env, -1, null, 2, 0x8d0, null);
                        base.TrException(method, exception25);
                        throw exception25;
                    }
                }
                if ((getMsgOpts.WaitInterval < 0) && (getMsgOpts.WaitInterval != -1))
                {
                    NmqiException exception26 = new NmqiException(base.env, -1, null, 2, 0x82a, null);
                    base.TrException(method, exception26);
                    throw exception26;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void CheckPurge(long timeNow)
        {
            uint method = 0x216;
            this.TrEntry(method, new object[] { timeNow });
            MQProxyMessage queueOldest = this.queueOldest;
            long num2 = timeNow - this.purgeTime;
            this.lastCheckPurge = timeNow;
            try
            {
            Label_0039:
                if (queueOldest.AddedTime <= num2)
                {
                    MQProxyMessage newer = queueOldest.Newer;
                    this.backlog -= queueOldest.MsgLength + queueOldest.MsgDescByteSize;
                    this.RemoveMessage(false, false, true, queueOldest);
                    this.msgsPurged += 1L;
                    queueOldest = newer;
                    if (queueOldest != null)
                    {
                        goto Label_0039;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public bool CheckSelectionCriteria(MQMessageDescriptor msgDesc, MQGetMessageOptions getMsgOptions, int maxMsgLength, ref int compCode, ref int reason)
        {
            uint method = 0x206;
            this.TrEntry(method, new object[] { msgDesc, getMsgOptions, maxMsgLength, (int) compCode, (int) reason });
            bool flag = false;
            try
            {
                int matchOptions;
                this.proxyQueueLock.AssertOnCurrentThreadHoldsLock();
                if (this.mqmdVersion != msgDesc.Version)
                {
                    if (((this.status & 0x200) != 0) && ((this.status & 0x400) != 0))
                    {
                        NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x7ea, null);
                        base.TrException(method, ex);
                        throw ex;
                    }
                    this.mqmdVersion = (short) msgDesc.Version;
                    flag = true;
                }
                if (getMsgOptions.Version >= 2)
                {
                    matchOptions = getMsgOptions.MatchOptions;
                    if (((matchOptions & 0x20) != 0) && (getMsgOptions.Version < 3))
                    {
                        matchOptions &= -33;
                    }
                }
                else
                {
                    matchOptions = 0;
                    if (!this.Compare(msgDesc.CorrelId, MQC.MQCI_NONE))
                    {
                        matchOptions |= 2;
                    }
                    if (!this.Compare(msgDesc.MsgId, MQC.MQMI_NONE))
                    {
                        matchOptions |= 1;
                    }
                }
                if (matchOptions != this.matchOptions)
                {
                    this.matchOptions = matchOptions;
                    flag = true;
                }
                if ((getMsgOptions.Options & 0x4000) != 0)
                {
                    if (msgDesc.CodedCharacterSetId != this.codedCharSetId)
                    {
                        this.codedCharSetId = msgDesc.CodedCharacterSetId;
                        flag = true;
                    }
                    if (msgDesc.Encoding != this.encoding)
                    {
                        this.encoding = msgDesc.Encoding;
                        flag = true;
                    }
                }
                if ((this.matchOptions & 2) != 0)
                {
                    if (!this.Compare(msgDesc.CorrelId, this.currentCorrelId))
                    {
                        Buffer.BlockCopy(msgDesc.CorrelId, 0, this.currentCorrelId, 0, 0x18);
                        if (this.Compare(this.currentCorrelId, MQC.MQCI_NONE))
                        {
                            this.currentCorrelIdSet = false;
                        }
                        else
                        {
                            this.currentCorrelIdSet = true;
                        }
                        flag = true;
                    }
                }
                else
                {
                    this.currentCorrelIdSet = false;
                }
                if ((this.matchOptions & 1) != 0)
                {
                    if (!this.Compare(msgDesc.MsgId, this.currentMsgId))
                    {
                        Buffer.BlockCopy(msgDesc.MsgId, 0, this.currentMsgId, 0, 0x18);
                        if (this.Compare(this.currentMsgId, MQC.MQCI_NONE))
                        {
                            this.currentMsgIdSet = false;
                        }
                        else
                        {
                            this.currentMsgIdSet = true;
                        }
                        flag = true;
                    }
                }
                else
                {
                    this.currentMsgIdSet = false;
                }
                if ((matchOptions & 0x20) != 0)
                {
                    if (!this.Compare(getMsgOptions.MsgToken, this.currentMsgToken))
                    {
                        Buffer.BlockCopy(getMsgOptions.MsgToken, 0, this.currentMsgToken, 0, 0x10);
                        if (this.Compare(this.currentMsgToken, MQC.MQMTOK_NONE))
                        {
                            this.currentMsgTokenSet = false;
                        }
                        else
                        {
                            this.currentMsgTokenSet = true;
                        }
                        flag = true;
                    }
                }
                else
                {
                    this.currentMsgTokenSet = false;
                }
                if (msgDesc.Version != this.mqmdVersion)
                {
                    this.mqmdVersion = (short) msgDesc.Version;
                    flag = true;
                }
                if (msgDesc.Version >= 2)
                {
                    if ((this.matchOptions & 4) != 0)
                    {
                        if (!this.Compare(msgDesc.GroupID, this.currentGroupId))
                        {
                            Buffer.BlockCopy(msgDesc.GroupID, 0, this.currentGroupId, 0, 0x18);
                            if (this.Compare(this.currentGroupId, MQC.MQGI_NONE))
                            {
                                this.currentGroupIdSet = false;
                            }
                            else
                            {
                                this.currentGroupIdSet = true;
                            }
                            flag = true;
                        }
                    }
                    else
                    {
                        this.currentGroupIdSet = false;
                    }
                    if (((this.matchOptions & 8) != 0) && (msgDesc.MsgSequenceNumber != this.msgSeqNumber))
                    {
                        this.msgSeqNumber = msgDesc.MsgSequenceNumber;
                        flag = true;
                    }
                    if (((this.matchOptions & 0x10) != 0) && (msgDesc.Offset != this.offset))
                    {
                        this.offset = msgDesc.Offset;
                        flag = true;
                    }
                }
                else
                {
                    this.currentGroupIdSet = false;
                }
                this.maxMsgLength = maxMsgLength;
                try
                {
                    this.statusSync.Acquire();
                    this.status |= 0x200;
                }
                finally
                {
                    this.statusSync.Release();
                }
                if (flag)
                {
                    this.selectionIndex++;
                    this.queueTop = this.queueOldest;
                    try
                    {
                        this.statusSync.Acquire();
                        this.status |= 0x10000;
                    }
                    finally
                    {
                        this.statusSync.Release();
                    }
                    this.backlog = 0L;
                }
                base.TrText(method, "Did the SelectionCriteria changed = " + flag);
            }
            finally
            {
                base.TrExit(method);
            }
            return flag;
        }

        internal void CheckTxnMessage()
        {
            uint method = 0x5f2;
            this.TrEntry(method);
            try
            {
                if (this.MqcbCBD != null)
                {
                    IntPtr callbackFunction = this.MqcbCBD.CallbackFunction;
                    if ((((this.status & 0x40000) == 0) && ((this.status & 0x10000000) != 0)) && !this.IsEmpty())
                    {
                        bool flag;
                        int options = this.MqcbGmo.Options;
                        if ((options & 2) != 0)
                        {
                            flag = true;
                        }
                        else if ((options & 4) != 0)
                        {
                            flag = true;
                        }
                        else if ((options & 0x1000) != 0)
                        {
                            flag = this.queueTop.MsgDesc.Persistence == 1;
                        }
                        else
                        {
                            flag = true;
                        }
                        if (flag)
                        {
                            this.RequestMutex();
                            this.DriveConsumer(ref this.inprocess);
                            this.ReleaseMutex();
                        }
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private bool Compare(byte[] arr1, byte[] arr2)
        {
            uint method = 0x217;
            this.TrEntry(method, new object[] { arr1, arr2 });
            bool result = true;
            if ((arr1 == null) && (arr2 == null))
            {
                result = true;
            }
            else if ((arr1 == null) && (arr2 != null))
            {
                result = false;
            }
            else if ((arr1 != null) && (arr2 == null))
            {
                result = false;
            }
            else if (arr1.Length != arr2.Length)
            {
                result = false;
            }
            else
            {
                for (int i = 0; i < arr1.Length; i++)
                {
                    if (arr1[i] != arr2[i])
                    {
                        result = false;
                        break;
                    }
                }
            }
            base.TrExit(method, result);
            return result;
        }

        private bool CompareCBD(MQCBD versionA, MQCBD versionB)
        {
            uint method = 0x602;
            this.TrEntry(method, new object[] { versionA, versionB });
            bool result = false;
            result = versionA.CbdId.Equals(versionB.CbdId);
            base.TrExit(method, result);
            return result;
        }

        private void CopyMDToUserMD(MQMessageDescriptor userMd, MQMessageDescriptor rcvdMd)
        {
            uint method = 0x41c;
            this.TrEntry(method, new object[] { userMd, rcvdMd });
            userMd.Report = rcvdMd.Report;
            userMd.MsgType = rcvdMd.MsgType;
            userMd.Expiry = rcvdMd.Expiry;
            userMd.Feedback = rcvdMd.Feedback;
            userMd.Encoding = rcvdMd.Encoding;
            userMd.CodedCharacterSetId = rcvdMd.CodedCharacterSetId;
            userMd.Format = rcvdMd.Format;
            userMd.Priority = rcvdMd.Priority;
            userMd.Persistence = rcvdMd.Persistence;
            userMd.MsgId = rcvdMd.MsgId;
            userMd.CorrelId = rcvdMd.CorrelId;
            userMd.BackoutCount = rcvdMd.BackoutCount;
            userMd.ReplyToQueue = rcvdMd.ReplyToQueue;
            userMd.ReplyToQueueMgr = rcvdMd.ReplyToQueueMgr;
            userMd.UserID = rcvdMd.UserID;
            userMd.AccountingToken = rcvdMd.AccountingToken;
            userMd.ApplIdentityData = rcvdMd.ApplIdentityData;
            userMd.PutApplType = rcvdMd.PutApplType;
            userMd.PutApplName = rcvdMd.PutApplName;
            userMd.PutDate = rcvdMd.PutDate;
            userMd.PutTime = rcvdMd.PutTime;
            userMd.ApplOriginData = rcvdMd.ApplOriginData;
            if (userMd.Version >= 2)
            {
                userMd.GroupID = rcvdMd.GroupID;
                userMd.MsgSequenceNumber = rcvdMd.MsgSequenceNumber;
                userMd.Offset = rcvdMd.Offset;
                userMd.MsgFlags = rcvdMd.MsgFlags;
                userMd.OriginalLength = rcvdMd.OriginalLength;
            }
            base.TrExit(method);
        }

        private void CopyOutMessage(MQProxyMessage message, MQMessageDescriptor userMd, MQGetMessageOptions userGmo, int bufferLength, byte[] buffer, ref int dataLength, ref int compCode, ref int reason)
        {
            uint method = 0x210;
            this.TrEntry(method, new object[] { message, userMd, userGmo, bufferLength, buffer, (int) dataLength, (int) compCode, (int) reason });
            try
            {
                this.proxyQueueLock.AssertOnCurrentThreadHoldsLock();
                bool flag = false;
                bool keepBrowse = false;
                if ((message.Type & 1) != 0)
                {
                    int msgLength;
                    bool flag3;
                    keepBrowse = ((this.status & 0x20) != 0) && ((this.status & 0x400) != 0);
                    MQMessageDescriptor msgDesc = message.MsgDesc;
                    this.CopyMDToUserMD(userMd, msgDesc);
                    if (message.MsgLength > bufferLength)
                    {
                        msgLength = bufferLength;
                        flag3 = true;
                        if ((userGmo.Options & 0x40) != 0)
                        {
                            flag = true;
                        }
                        else if (message.ActualMsgLength > message.MsgLength)
                        {
                            flag = true;
                        }
                        else
                        {
                            flag = false;
                        }
                    }
                    else
                    {
                        msgLength = message.MsgLength;
                        flag3 = false;
                        flag = true;
                    }
                    Buffer.BlockCopy(message.MsgData, 0, buffer, 0, msgLength);
                    if (flag3)
                    {
                        if ((userGmo.Options & 0x40) != 0)
                        {
                            reason = 0x81f;
                        }
                        else
                        {
                            reason = 0x820;
                        }
                    }
                    else
                    {
                        reason = message.Reason;
                    }
                    if (reason != 0)
                    {
                        compCode = 1;
                    }
                    else
                    {
                        compCode = 0;
                    }
                    dataLength = message.ActualMsgLength;
                    if ((message.ResolvedQName != null) && (message.ResolvedQName.Length != 0))
                    {
                        userGmo.ResolvedQueueName = message.ResolvedQName;
                    }
                    else
                    {
                        userGmo.ResolvedQueueName = null;
                    }
                    if (userGmo.Version >= 2)
                    {
                        userGmo.GroupStatus = 0x20;
                        if ((message.Status & 0x3e) != 0)
                        {
                            if ((message.Status & 0x10) != 0)
                            {
                                userGmo.GroupStatus = 0x47;
                            }
                            if ((message.Status & 0x20) != 0)
                            {
                                userGmo.GroupStatus = 0x4c;
                            }
                            if ((message.Status & 4) != 0)
                            {
                                userGmo.GroupStatus = 0x53;
                            }
                            if ((message.Status & 8) != 0)
                            {
                                userGmo.GroupStatus = 0x4c;
                            }
                            if ((message.Status & 2) != 0)
                            {
                                userGmo.GroupStatus = 0x41;
                            }
                        }
                        if (userGmo.Version >= 3)
                        {
                            message.MsgToken.CopyTo(userGmo.MsgToken, 0);
                            userGmo.ReturnedLength = msgLength;
                        }
                    }
                }
                else
                {
                    compCode = message.CompCode;
                    reason = message.Reason;
                    flag = true;
                    keepBrowse = false;
                }
                if (flag && (message != this.lastBrowsed))
                {
                    this.RemoveMessage(true, keepBrowse, true, message);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void DeliverMsgs(ref int processed)
        {
            uint method = 0x5fa;
            this.TrEntry(method, new object[] { (int) processed });
            try
            {
                this.RequestMutex();
                if ((this.mqcbGmo.WaitInterval == 0) && ((this.mqcbGmo.Options & 1) != 0))
                {
                    try
                    {
                        this.statusSync.Acquire();
                        this.status |= 0x1000000;
                    }
                    finally
                    {
                        this.statusSync.Release();
                    }
                }
                this.DriveConsumer(ref processed);
            }
            finally
            {
                this.ReleaseMutex();
                base.TrExit(method);
            }
        }

        public void DriveConsumer(ref int processed)
        {
            uint method = 0x5f3;
            this.TrEntry(method, new object[] { (int) processed });
            try
            {
                MQProxyMessage message = null;
                bool keepBrowse = (this.status & 0x20) != 0;
            Label_0034:
                int num1 = this.mqcbGmo.Options;
                message = this.FindNextMessage(this.MqcbCBD.MaxMsgLength, false);
                if (message == null)
                {
                    return;
                }
                this.RemoveMessage(true, keepBrowse, false, message);
                this.ReleaseMutex();
                try
                {
                    if ((message.Type & 1) == 0)
                    {
                        goto Label_0236;
                    }
                    this.callbackCBC.Version = 2;
                    this.callbackCBC.HOBJ = this.Hobj;
                    if (keepBrowse)
                    {
                        this.callbackCBC.CallType = 7;
                    }
                    else
                    {
                        this.callbackCBC.CallType = 6;
                        if ((message.Reason != 0) && (message.Reason == 0x820))
                        {
                            this.callbackCBC.CallType = 7;
                        }
                    }
                    this.callbackCBC.CompCode = message.CompCode;
                    this.callbackCBC.Reason = message.Reason;
                    this.callbackCBC.DataLength = message.ActualMsgLength;
                    this.callbackCBC.BufferLength = message.MsgLength;
                    this.callbackCBC.Flags = 0;
                    if (((this.status & 0x80) != 0) && this.IsEmpty())
                    {
                        this.callbackCBC.Flags |= 1;
                    }
                    if (((this.status & 0x10000000) != 0) && !this.hconn.InTransaction())
                    {
                        switch ((this.mqcbGmo.Options & 0x1006))
                        {
                            case 2:
                                this.hconn.SetInTransaction();
                                break;

                            case 0x1000:
                                goto Label_01A9;
                        }
                    }
                    goto Label_01C2;
                Label_01A9:
                    if (this.mqcbMD.Persistence == 1)
                    {
                        this.hconn.SetInTransaction();
                    }
                Label_01C2:
                    Buffer.BlockCopy(message.MsgToken, 0, this.MqcbGmo.MsgToken, 0, message.MsgToken.Length);
                    this.mqcbGmo.Version = 4;
                    this.mqcbGmo.ReturnedLength = message.MsgLength;
                    this.MqcbCBD.MqConsumer.Consumer(this.hconn.ParentPhconn, message.MsgDesc, this.MqcbGmo, message.MsgData, this.callbackCBC);
                    goto Label_0252;
                Label_0236:
                    this.CallConsumer(5, message.CompCode, message.Reason);
                }
                finally
                {
                    this.RequestMutex();
                }
            Label_0252:
                processed++;
                if (!keepBrowse || ((message.Type & 1) == 0))
                {
                    this.FreeMessage(message);
                }
                if (this.hconn.ConsumersChanged())
                {
                    bool flag2 = (this.status & 0x80000) != 0;
                    this.hconn.DriveOutstanding();
                    if (flag2)
                    {
                        return;
                    }
                }
                if (!this.IsCbAlreadyRegistered)
                {
                    this.hconn.CheckTxnAllowed();
                }
                else if (this.hconn.IsStarted() && !this.hconn.IsSuspended())
                {
                    if (this.asyncSelectionIndex != this.selectionIndex)
                    {
                        this.SetAsyncSelection();
                    }
                    if ((this.status & 0x40000) != 0)
                    {
                        this.hconn.CheckTxnAllowed();
                    }
                    else
                    {
                        if ((this.status & 0x80) != 0)
                        {
                            this.hconn.CheckTxnAllowed();
                        }
                        else if ((this.status & 8) == 0)
                        {
                            bool flag3;
                            if ((this.status & 0x400) != 0)
                            {
                                flag3 = (this.status & 0x10000) != 0;
                                if (this.IsEmpty())
                                {
                                    if (!flag3)
                                    {
                                        flag3 = (this.status & 0xa000) != 0;
                                    }
                                    if (!flag3)
                                    {
                                        this.ReleaseMutex();
                                        Thread.Sleep(0);
                                        this.RequestMutex();
                                        if (this.IsEmpty() && ((this.status & 8) == 0))
                                        {
                                            flag3 = this.hconn.CheckClientEmpty();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                flag3 = (this.status & 8) == 0;
                            }
                            if (flag3)
                            {
                                this.RequestMessages(false, 0, false, true);
                            }
                        }
                        if (processed < 5)
                        {
                            goto Label_0034;
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

        public void DriveEventsMC()
        {
            uint method = 0x5f8;
            this.TrEntry(method);
            try
            {
                for (int i = 0; i < NmqiConstants.Events.Length; i++)
                {
                    if (((this.hconn.EventsHad & NmqiConstants.Events[i]) != 0) && ((this.eventsRaised & NmqiConstants.Events[i]) == 0))
                    {
                        this.eventsRaised |= NmqiConstants.Events[i];
                        this.CallConsumer(5, 2, NmqiConstants.EventReasons[i]);
                        if (this.eventsRaised == this.hconn.EventsHad)
                        {
                            return;
                        }
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void DriveStop()
        {
            uint method = 0x5fd;
            this.TrEntry(method);
            try
            {
                this.statusSync.Acquire();
                this.status &= -4194305;
            }
            finally
            {
                this.statusSync.Release();
                base.TrExit(method);
            }
        }

        public MQProxyMessage FindNextMessage(int maxMsgLen, bool msgOnly)
        {
            uint method = 0x209;
            this.TrEntry(method, new object[] { maxMsgLen, msgOnly });
            MQProxyMessage queueTop = this.queueTop;
            MQProxyMessage newer = null;
            try
            {
                this.proxyQueueLock.AssertOnCurrentThreadHoldsLock();
                while (queueTop != null)
                {
                    if ((queueTop.Type & 1) == 0)
                    {
                        if (!msgOnly)
                        {
                            break;
                        }
                        newer = queueTop.Newer;
                        this.RemoveMessage(true, false, true, queueTop);
                        queueTop = newer;
                    }
                    else
                    {
                        if ((queueTop.Reason == 0x820) && ((maxMsgLen == -1) || (maxMsgLen > queueTop.MsgLength)))
                        {
                            newer = queueTop.Newer;
                            this.RemoveMessage(true, false, true, queueTop);
                            queueTop = newer;
                            continue;
                        }
                        if ((queueTop.SelectionIndex == this.selectionIndex) || this.MatchSelection(queueTop))
                        {
                            break;
                        }
                        this.backlog += queueTop.MsgLength + queueTop.MsgDescByteSize;
                        queueTop = queueTop.Newer;
                    }
                }
                this.queueTop = queueTop;
            }
            finally
            {
                base.TrExit(method);
            }
            return queueTop;
        }

        private void FlushQueue(bool mqgetWaiting)
        {
            uint method = 0x20c;
            this.TrEntry(method, new object[] { mqgetWaiting });
            try
            {
                this.proxyQueueLock.AssertOnCurrentThreadHoldsLock();
                this.RequestMessages(true, 0, mqgetWaiting, false);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void FreeMessage(MQProxyMessage message)
        {
            uint method = 0x205;
            this.TrEntry(method, new object[] { message });
            base.TrExit(method);
        }

        public int GetAvailableConsumers()
        {
            int count = this.mqCBDList.Count;
            base.TrText("Current number of Consumers on the PQ = " + count);
            return count;
        }

        private long GetTimeInSeconds()
        {
            TimeSpan span = (TimeSpan) (DateTime.UtcNow - this.startTime);
            return Convert.ToInt64(Math.Abs(span.TotalSeconds));
        }

        public bool IsDispatchableQ()
        {
            uint method = 0x5f9;
            this.TrEntry(method);
            try
            {
                if ((this.status & 0x40000) == 0)
                {
                    if (this.asyncSelectionIndex != this.selectionIndex)
                    {
                        this.SetAsyncSelection();
                    }
                    if ((this.IsEmpty() && ((this.status & 0x800) != 0)) && ((this.status & 0x80) == 0))
                    {
                        try
                        {
                            this.RequestMutex();
                            this.RequestMessages(false, 0, false, true);
                        }
                        finally
                        {
                            this.ReleaseMutex();
                        }
                    }
                    if (this.eventsRaised != this.hconn.EventsHad)
                    {
                        return true;
                    }
                    if (((this.mqcbGmo.Options & 1) != 0) && (this.mqcbGmo.WaitInterval >= 0))
                    {
                        if (this.mqcbGmo.WaitInterval == 0)
                        {
                            try
                            {
                                this.statusSync.Acquire();
                                this.status |= 0x1000000;
                            }
                            finally
                            {
                                this.statusSync.Release();
                            }
                        }
                        return true;
                    }
                    if (!this.IsEmpty())
                    {
                        return true;
                    }
                }
            }
            finally
            {
                base.TrExit(method, 4);
            }
            return false;
        }

        internal bool IsEmpty()
        {
            uint method = 0x5ee;
            this.TrEntry(method);
            bool result = this.queueTop == null;
            base.TrExit(method, result);
            return result;
        }

        public bool IsStreamingRequested()
        {
            return ((this.status & 0x4000) != 0);
        }

        private bool MatchSelection(MQProxyMessage message)
        {
            uint method = 520;
            this.TrEntry(method, new object[] { message });
            bool flag = true;
            try
            {
                this.proxyQueueLock.AssertOnCurrentThreadHoldsLock();
                MQMessageDescriptor msgDesc = message.MsgDesc;
                if ((flag && ((this.matchOptions & 1) != 0)) && this.currentMsgIdSet)
                {
                    flag = this.Compare(this.currentMsgId, msgDesc.MsgId);
                }
                if ((flag && ((this.matchOptions & 2) != 0)) && this.currentCorrelIdSet)
                {
                    flag = this.Compare(this.currentCorrelId, msgDesc.CorrelId);
                }
                if ((flag && ((this.matchOptions & 4) != 0)) && this.currentGroupIdSet)
                {
                    flag = this.Compare(this.currentGroupId, msgDesc.GroupID);
                }
                if ((flag && ((this.matchOptions & 0x20) != 0)) && this.currentMsgTokenSet)
                {
                    flag = this.Compare(this.currentMsgToken, message.MsgToken);
                }
                if (flag && ((this.matchOptions & 0x10) != 0))
                {
                    flag = this.offset == msgDesc.Offset;
                }
                if (flag && ((this.matchOptions & 8) != 0))
                {
                    flag = this.msgSeqNumber == msgDesc.MsgSequenceNumber;
                }
                base.TrText(method, "Did have a Selection =" + flag);
            }
            finally
            {
                base.TrExit(method);
            }
            return flag;
        }

        public void MqcbDeRegisterMC(bool immediate, MQCBD callbackDesc)
        {
            uint method = 0x5f7;
            this.TrEntry(method, new object[] { immediate, callbackDesc });
            try
            {
                int availableConsumers = this.GetAvailableConsumers();
                if (availableConsumers == 0)
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 1, 0x990, null);
                    base.env.LastException = ex;
                    base.TrException(method, ex);
                    throw ex;
                }
                MQCBD mqcbCBD = callbackDesc;
                if (mqcbCBD == null)
                {
                    mqcbCBD = this.MqcbCBD;
                }
                if (mqcbCBD.MqConsumer == null)
                {
                    NmqiException exception2 = new NmqiException(base.env, -1, null, 1, 0x990, null);
                    base.env.LastException = exception2;
                    base.TrException(method, exception2);
                    throw exception2;
                }
                if (availableConsumers == 1)
                {
                    this.IsCbAlreadyRegistered = false;
                    if (immediate)
                    {
                        this.hconn.RemoveFromDispatchList(this);
                        if ((this.MqcbCBD.Options & 0x200) != 0)
                        {
                            this.CallConsumer(4, 0, 0);
                        }
                        try
                        {
                            this.statusSync.Acquire();
                            this.status &= -2097153;
                        }
                        finally
                        {
                            this.statusSync.Release();
                        }
                        this.SendConsumerState(false);
                    }
                    else
                    {
                        this.hconn.ConsumersChanged();
                        try
                        {
                            this.statusSync.Acquire();
                            this.status |= 0x200000;
                        }
                        finally
                        {
                            this.statusSync.Release();
                        }
                        this.SendConsumerState(false);
                    }
                    if (immediate)
                    {
                        this.RemoveCbd(mqcbCBD);
                    }
                }
                else
                {
                    this.RemoveCbd(mqcbCBD);
                }
            }
            catch (NullReferenceException exception3)
            {
                base.TrException(method, exception3);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MqcbRegisterMC(MQCBD callbackDesc, MQMessageDescriptor msgDesc, MQGetMessageOptions gmo)
        {
            uint method = 0x5f6;
            this.TrEntry(method, new object[] { callbackDesc, msgDesc, gmo });
            try
            {
                int compCode = 0;
                int reason = 0;
                this.ValidateMQMD(msgDesc);
                this.ValidateMQGMO(gmo);
                this.RequestMutex();
                this.CheckGetMsgOptions(true, msgDesc, gmo, ref compCode, ref reason);
                this.mqcbGmo = gmo;
                this.mqcbMD = msgDesc;
                if (!this.IsCbAlreadyRegistered)
                {
                    this.CallbackCBC = new MQCBC();
                    this.AddCbd(callbackDesc.CopyMqcbd());
                    if ((this.MqcbCBD.Options & 0x100) != 0)
                    {
                        this.CallConsumer(3, 0, 0);
                    }
                    this.IsCbAlreadyRegistered = true;
                    this.SendConsumerState(false);
                    this.NoMsgTime = 0L;
                    this.hconn.AddToDispatchList(this);
                    this.SetAsyncSelection();
                }
                else
                {
                    try
                    {
                        this.statusSync.Acquire();
                        this.status &= -2097153;
                    }
                    finally
                    {
                        this.statusSync.Release();
                    }
                    this.AddCbd(callbackDesc);
                }
            }
            finally
            {
                this.ReleaseMutex();
                base.TrExit(method);
            }
        }

        public void MqcbResumeMC()
        {
            uint method = 0x5f5;
            this.TrEntry(method);
            try
            {
                if (this.MqcbCBD.MqConsumer == null)
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x990, null);
                    base.env.LastException = ex;
                    base.TrException(method, ex);
                    throw ex;
                }
                if ((this.status & 0x40000) != 0)
                {
                    try
                    {
                        this.statusSync.Acquire();
                        this.status &= -262145;
                    }
                    finally
                    {
                        this.statusSync.Release();
                    }
                    this.SendConsumerState(false);
                }
                this.hconn.AddToDispatchList(this);
                this.hconn.CheckDispatchable(this);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MqcbSuspendMC()
        {
            uint method = 0x5f4;
            this.TrEntry(method);
            try
            {
                if (this.MqcbCBD.MqConsumer == null)
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x990, null);
                    base.env.LastException = ex;
                    base.TrException(method, ex);
                    throw ex;
                }
                if ((this.status & 0x40000) == 0)
                {
                    try
                    {
                        this.statusSync.Acquire();
                        this.status |= 0x40000;
                    }
                    finally
                    {
                        this.statusSync.Release();
                    }
                    this.hconn.ConsumersChanged();
                    this.SendConsumerState(false);
                }
                if ((this.MqcbCBD.Options & 4) == 0)
                {
                    this.hconn.RemoveFromDispatchList(this);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void NotifyConnectionFailure(Exception e)
        {
            uint method = 0x5ef;
            this.TrEntry(method, new object[] { e });
            try
            {
                lock (this.getterEventMonitor)
                {
                    this.connectionFailure = e;
                    Monitor.PulseAll(this.getterEventMonitor);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void ProxyMQCLOSE(int closeOptions, ref int compCode, ref int reason)
        {
            uint method = 0x213;
            this.TrEntry(method, new object[] { closeOptions, (int) compCode, (int) reason });
            compCode = 0;
            reason = 0;
            MQSession session = this.hconn.Session;
            bool flag = (closeOptions & 0x20) != 0;
            try
            {
                if (this.hobj != null)
                {
                    if (flag)
                    {
                        if (((this.status & 0x80) != 0) && !this.IsEmpty())
                        {
                            compCode = 2;
                            reason = 0x99a;
                            return;
                        }
                        this.statusSync.Acquire();
                        this.status |= 0x80;
                        this.status |= 0x1000000;
                        this.statusSync.Release();
                    }
                    if (compCode != 2)
                    {
                        MQAPI mqapi = new MQAPI();
                        int translength = (0x24 + mqapi.GetLength()) + 4;
                        MQTSH tsh = session.AllocateTSH(0x84, 0, true, translength);
                        MQTSH rTSH = null;
                        mqapi.Initialize(translength, this.Hobj);
                        if (flag && !this.IsEmpty())
                        {
                            mqapi.mqapi.CompCode = 0;
                        }
                        else
                        {
                            mqapi.mqapi.CompCode = this.lastReceivedIndex;
                        }
                        tsh.Offset = tsh.WriteStruct(tsh.TshBuffer, 0);
                        tsh.Offset += mqapi.WriteStruct(tsh.TshBuffer, tsh.Offset);
                        BitConverter.GetBytes(closeOptions).CopyTo(tsh.TshBuffer, tsh.Offset);
                        tsh.Offset = 0;
                        try
                        {
                            session.SendTSH(tsh);
                            rTSH = session.ReceiveTSH(null);
                            if (rTSH.SegmentType != 0x94)
                            {
                                compCode = 2;
                                reason = 0x893;
                                new NmqiException(base.env, -1, null, 2, 0x893, null);
                                this.hconn.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                                CommonServices.SetValidInserts();
                                CommonServices.ArithInsert1 = rTSH.SegmentType;
                                CommonServices.CommentInsert1 = "Expected SegmentType = " + ((byte) 0x94);
                                CommonServices.CommentInsert2 = "Unexpected reply received during MQCLOSE(PQ)";
                                base.FFST("@(#) lib/dotnet/pc/winnt/nmqi/managed/MQProxyQueue.cs, dotnet, p000, p000-L091124  1.6 09/11/19 13:02:00", "%C%", method, 1, 0x20009213, 0);
                                return;
                            }
                            rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            compCode = mqapi.mqapi.CompCode;
                            reason = mqapi.mqapi.Reason;
                        }
                        finally
                        {
                            session.ReleaseReceivedTSH(rTSH);
                        }
                        if (compCode == 2)
                        {
                            return;
                        }
                        if (flag && !this.IsEmpty())
                        {
                            compCode = 1;
                            reason = 0x99a;
                            return;
                        }
                    }
                }
                try
                {
                    this.statusSync.Acquire();
                    this.status |= 0x20000000;
                }
                finally
                {
                    this.statusSync.Release();
                }
                this.hconn.ProxyQueueManager.DeleteProxyQueue(this);
                if (this.cbAlreadyRegistered)
                {
                    bool immediate = NmqiTools.IsDispatcherThread();
                    this.MqcbDeRegisterMC(immediate, null);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void ProxyMQGET(MQMessageDescriptor msgDesc, MQGetMessageOptions getMsgOpts, int bufferLength, byte[] buffer, ref int dataLength, MQLPIGetOpts lpiGetOpts, ref int compCode, ref int reason)
        {
            uint method = 530;
            this.TrEntry(method, new object[] { msgDesc, getMsgOpts, bufferLength, buffer, (int) dataLength, lpiGetOpts, (int) compCode, (int) reason });
            bool flag = false;
            bool flag2 = false;
            bool flag3 = true;
            bool flag4 = false;
            bool flag5 = false;
            int maxMsgLength = bufferLength;
            int waitInterval = getMsgOpts.WaitInterval;
            compCode = 0;
            reason = 0;
            try
            {
                if (lpiGetOpts != null)
                {
                    int num4 = MQLPIGetOpts.lpiGETOPT_FULL_MESSAGE;
                    if ((lpiGetOpts.GetOptions() & ~num4) != 0)
                    {
                        compCode = 2;
                        reason = 0x8fa;
                        return;
                    }
                    if ((lpiGetOpts.GetOptions() & MQLPIGetOpts.lpiGETOPT_FULL_MESSAGE) > 0)
                    {
                        maxMsgLength = -1;
                    }
                }
                if ((this.status & 0x400) != 0)
                {
                    if (waitInterval == 0)
                    {
                        waitInterval = 10;
                    }
                }
                else
                {
                    this.appMD = msgDesc;
                    this.appGmo = getMsgOpts;
                    this.appBufferLength = bufferLength;
                    this.appBuffer = buffer;
                    this.appDataLength = dataLength;
                    this.appCompCode = compCode;
                    this.appReason = reason;
                    this.appBufferUsed = 0;
                }
                if ((getMsgOpts.Options & 1) != 0)
                {
                    if (waitInterval < -1)
                    {
                        compCode = 2;
                        reason = 0x82a;
                        return;
                    }
                    flag3 = waitInterval != 0;
                }
                else
                {
                    flag3 = false;
                }
                if (bufferLength < 0)
                {
                    compCode = 2;
                    reason = 0x7d5;
                    return;
                }
                this.RequestMutex();
                flag5 = true;
                try
                {
                    this.statusSync.Acquire();
                    this.status |= 0x6000000;
                }
                finally
                {
                    this.statusSync.Release();
                }
                this.CheckGetMsgOptions(false, msgDesc, getMsgOpts, ref compCode, ref reason);
                if ((this.status & 1) != 0)
                {
                    compCode = 2;
                    reason = 0x835;
                    base.TrText(method, "Proxy queue marked as unhealthy.CompCode : " + 2.ToString() + " Reason : " + 0x835.ToString());
                    return;
                }
                if ((getMsgOpts.Options & 0x2000) != 0)
                {
                    if ((this.status & 0x100) != 0)
                    {
                        compCode = 2;
                        reason = 0x871;
                        base.TrText(method, "Queue manager is quiescing. CompCode : " + 2.ToString() + " Reason : " + 0x871.ToString());
                        return;
                    }
                    if (this.hconn.IsQuiescing())
                    {
                        compCode = 2;
                        reason = 0x89a;
                        base.TrText(method, "Connection is quiescing. CompCode : " + 2.ToString() + " Reason : " + 0x89a.ToString());
                        base.TrException(method, new MQException(2, 0x89a));
                        return;
                    }
                }
                if (((this.status & 0x10000000) != 0) && !this.hconn.InTransaction())
                {
                    switch ((getMsgOpts.Options & 0x1006))
                    {
                        case 2:
                            this.hconn.SetInTransaction();
                            break;

                        case 0x1000:
                            goto Label_02EB;
                    }
                }
                goto Label_02FF;
            Label_02EB:
                if (msgDesc.Persistence == 1)
                {
                    this.hconn.SetInTransaction();
                }
            Label_02FF:
                this.CheckSelectionCriteria(msgDesc, getMsgOpts, maxMsgLength, ref compCode, ref reason);
                if (((getMsgOpts.Options & 0x800) != 0) && ((this.status & 0x400) != 0))
                {
                    if (this.lastBrowsed == null)
                    {
                        compCode = 2;
                        reason = 0x7f2;
                    }
                    else
                    {
                        this.CopyOutMessage(this.lastBrowsed, msgDesc, getMsgOpts, bufferLength, buffer, ref dataLength, ref compCode, ref reason);
                        if ((compCode != 0) && (reason != 0))
                        {
                            base.TrException(method, new Exception(string.Concat(new object[] { "CompCode : ", (int) compCode, ", Reason : ", (int) reason })));
                        }
                    }
                    return;
                }
                if (((getMsgOpts.Options & 0x10) != 0) && ((getMsgOpts.Options & 0x300000) == 0))
                {
                    this.getMsgOptions |= 0x20;
                    this.ResetBrowseFirst();
                }
            Label_03E8:
                if (this.queueTop != null)
                {
                    MQProxyMessage message = this.FindNextMessage(maxMsgLength, true);
                    if (message != null)
                    {
                        this.CopyOutMessage(message, msgDesc, getMsgOpts, bufferLength, buffer, ref dataLength, ref compCode, ref reason);
                        return;
                    }
                }
                if ((this.status & 0x80) != 0)
                {
                    compCode = 2;
                    if (!this.IsEmpty())
                    {
                        reason = 0x9d5;
                    }
                    else
                    {
                        reason = 0x9d6;
                    }
                    return;
                }
                if ((this.status & 0x10000) != 0)
                {
                    flag = true;
                }
                if (!flag4 && flag3)
                {
                    if ((this.status & 0x800) != 0)
                    {
                        flag = true;
                    }
                    if ((this.status & 0x400) == 0)
                    {
                        flag = true;
                    }
                    if ((this.status & 0x2000) != 0)
                    {
                        flag = true;
                    }
                    if ((this.status & 0x40000000) != 0)
                    {
                        flag = true;
                    }
                    if ((this.status & 8) != 0)
                    {
                        flag = true;
                    }
                    if (flag)
                    {
                        this.RequestMessages(false, getMsgOpts.WaitInterval, true, false);
                        flag4 = true;
                        this.lastCompCode = 0;
                    }
                }
                if (((this.status & 0x2000) != 0) && (this.lastCompCode != 0))
                {
                    compCode = this.lastCompCode;
                    reason = this.lastReason;
                    base.TrText(method, "Streaming currently paused, due to server notification. CompCode : " + this.lastCompCode.ToString() + " Reason : " + this.lastReason.ToString());
                    return;
                }
                if (flag2)
                {
                    compCode = 2;
                    reason = 0x7f1;
                    return;
                }
                if (((this.status & 0x8000000) != 0) && (this.progressMsgIndex == 0))
                {
                    return;
                }
                if (flag3)
                {
                    try
                    {
                        this.statusSync.Acquire();
                        this.status &= -5;
                        this.status |= 2;
                    }
                    finally
                    {
                        this.statusSync.Release();
                    }
                    this.ReleaseMutex();
                    flag5 = false;
                    this.connectionFailure = null;
                    try
                    {
                        lock (this.getterEventMonitor)
                        {
                            if ((this.status & 4) == 0)
                            {
                                if (waitInterval > 0)
                                {
                                    Monitor.Wait(this.getterEventMonitor, waitInterval, true);
                                }
                                else
                                {
                                    Monitor.Wait(this.getterEventMonitor, -1, true);
                                }
                            }
                        }
                    }
                    catch (ThreadInterruptedException exception)
                    {
                        base.TrException(method, exception);
                        CommonServices.SetValidInserts();
                        CommonServices.ArithInsert1 = 1;
                        CommonServices.CommentInsert1 = this.hconn.Session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                        base.FFST("@(#) lib/dotnet/pc/winnt/nmqi/managed/MQProxyQueue.cs, dotnet, p000, p000-L091124  1.6 09/11/19 13:02:00", "%C%", method, 1, 0x20009546, 0);
                        base.TrText(method, "Thread interrupted while waiting for lock. CompCode : " + 2.ToString() + " Reason : " + 0x893.ToString());
                        throw MQERD.ErrorException(13, 0, this.hconn.Session.ParentConnection.NegotiatedChannel.ChannelName.Trim());
                    }
                    if (this.connectionFailure != null)
                    {
                        NmqiException exception2 = new NmqiException(base.env, 0x23fd, null, 2, 0x7d9, this.connectionFailure);
                        this.connectionFailure = null;
                        throw exception2;
                    }
                    this.RequestMutex();
                    flag5 = true;
                    if ((this.status & 4) != 0)
                    {
                        try
                        {
                            this.statusSync.Acquire();
                            this.status &= -5;
                            goto Label_0702;
                        }
                        finally
                        {
                            this.statusSync.Release();
                        }
                    }
                    flag2 = true;
                }
                else
                {
                    flag2 = true;
                }
            Label_0702:
                if ((getMsgOpts.Options & 0x2000) != 0)
                {
                    if ((this.status & 0x100) != 0)
                    {
                        base.TrText(method, "Queue manager is quiescing. CompCode : " + 2.ToString() + " Reason : " + 0x871.ToString());
                        compCode = 2;
                        reason = 0x871;
                        return;
                    }
                    if (this.hconn.IsQuiescing())
                    {
                        compCode = 2;
                        reason = 0x89a;
                        base.TrException(method, new MQException(2, 0x89a));
                        return;
                    }
                }
                if (flag2)
                {
                    this.FlushQueue(true);
                }
                if ((this.status & 8) != 0)
                {
                    compCode = 2;
                    reason = 0x7e0;
                }
                else if ((this.status & 0x8000000) != 0)
                {
                    compCode = this.appCompCode;
                    reason = this.appReason;
                    dataLength = this.appDataLength;
                }
                else
                {
                    try
                    {
                        this.statusSync.Acquire();
                        this.status &= -3;
                        this.status &= -5;
                    }
                    finally
                    {
                        this.statusSync.Release();
                    }
                    if ((this.status & 0x40000000) != 0)
                    {
                        flag4 = false;
                    }
                    goto Label_03E8;
                }
            }
            finally
            {
                this.status &= -201326593;
                if (flag5)
                {
                    this.ReleaseMutex();
                }
                base.TrExit(method);
            }
        }

        internal void RaiseEvent(int reason)
        {
            uint method = 0x5f1;
            this.TrEntry(method, new object[] { reason });
            if (!this.IsCbAlreadyRegistered)
            {
                base.TrExit(method, 1);
            }
            else if ((this.eventsRaised & reason) != 0)
            {
                base.TrExit(method, 2);
            }
            else
            {
                try
                {
                    this.RequestMutex();
                    this.eventsRaised |= reason;
                    MQProxyMessage message = this.AllocMessage(MQASYNC_MESSAGE.SIZE_TO_MSG_SEG1);
                    message.Type = 0;
                    message.CompCode = 2;
                    message.Reason = reason;
                    this.AddPhysicalMessage(message);
                }
                catch (NmqiException exception)
                {
                    base.TrException(method, exception);
                }
                finally
                {
                    this.ReleaseMutex();
                    base.TrExit(method, 3);
                }
                base.TrExit(method, 4);
            }
        }

        internal void ReceiveNotification(MQNOTIFICATION notification)
        {
            uint method = 0x214;
            this.TrEntry(method, new object[] { notification });
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            MQSession failedSession = this.hconn.Session;
            try
            {
                this.RequestMutex();
                flag = true;
                switch (notification.notify.notificationCode)
                {
                    case 1:
                        this.statusSync.Acquire();
                        this.status |= 8;
                        this.statusSync.Release();
                        flag2 = true;
                        flag3 = true;
                        goto Label_0325;

                    case 2:
                        this.statusSync.Acquire();
                        this.status |= -9;
                        this.statusSync.Release();
                        flag2 = true;
                        flag3 = true;
                        goto Label_0325;

                    case 7:
                        this.statusSync.Acquire();
                        this.status |= 0x100;
                        this.statusSync.Release();
                        flag2 = true;
                        goto Label_0325;

                    case 12:
                        this.statusSync.Acquire();
                        this.status &= -4097;
                        this.statusSync.Release();
                        goto Label_0325;

                    case 14:
                    {
                        this.statusSync.Acquire();
                        this.status |= 0x2000;
                        this.statusSync.Release();
                        this.lastCompCode = 2;
                        this.lastReason = notification.notify.notificationValue;
                        bool isReconnecting = false;
                        if (this.hconn.IsReconnectable)
                        {
                            if (ManagedHconn.IsReconnectableReasonCode(this.lastReason))
                            {
                                this.hconn.EligibleForReconnect(failedSession, true);
                            }
                            isReconnecting = this.hconn.IsReconnecting;
                            if (this.hconn.HasFailed())
                            {
                                this.lastCompCode = this.hconn.ReconnectionFailureCompCode;
                                this.lastReason = this.hconn.ReconnectionFailureReason;
                            }
                        }
                        if (!isReconnecting)
                        {
                            switch (this.lastReason)
                            {
                                case 0x871:
                                case 0x89a:
                                    this.hconn.RaiseEvent(this.lastReason);
                                    break;

                                case 0x83b:
                                    goto Label_0245;
                            }
                        }
                        goto Label_0275;
                    }
                    case 0x10:
                        this.statusSync.Acquire();
                        this.status |= 0x40000000;
                        this.statusSync.Release();
                        flag2 = true;
                        goto Label_0325;

                    default:
                    {
                        byte[] b = new byte[notification.GetLength()];
                        notification.WriteStruct(b, 0);
                        base.TrData(method, 0, "Notification", b);
                        CommonServices.SetValidInserts();
                        CommonServices.ArithInsert1 = (uint) notification.notify.notificationCode;
                        CommonServices.CommentInsert1 = this.hconn.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                        CommonServices.CommentInsert2 = "Unexpected flow received with Notification flow(PQ)";
                        base.FFST("@(#) lib/dotnet/pc/winnt/nmqi/managed/MQProxyQueue.cs, dotnet, p000, p000-L091124  1.6 09/11/19 13:02:00", "%C%", method, 1, 0x20009213, 0);
                        goto Label_0325;
                    }
                }
                this.hconn.RaiseEvent(this.lastReason);
                goto Label_0275;
            Label_0245:
                if (this.hconn.IsQuiescing())
                {
                    this.hconn.RaiseEvent(0x89a);
                }
                else
                {
                    this.hconn.RaiseEvent(this.lastReason);
                }
            Label_0275:
                flag2 = true;
            Label_0325:
                if (flag3)
                {
                    MQProxyMessage message = this.AllocMessage(MQASYNC_MESSAGE.SIZE_TO_MSG_SEG1);
                    message.Type = 0;
                    message.CompCode = 2;
                    message.Reason = notification.notify.notificationValue;
                    this.AddPhysicalMessage(message);
                }
                else if ((flag2 && ((this.status & 2) != 0)) && ((this.status & 4) == 0))
                {
                    lock (this.getterEventMonitor)
                    {
                        this.statusSync.Acquire();
                        this.status |= 4;
                        this.statusSync.Release();
                        Monitor.PulseAll(this.getterEventMonitor);
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    this.ReleaseMutex();
                }
                base.TrExit(method);
            }
        }

        public void ReleaseMutex()
        {
            uint method = 0x202;
            this.TrEntry(method);
            try
            {
                this.proxyQueueLock.Release();
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void ReleaseWaitingGetters()
        {
            uint method = 0x495;
            this.TrEntry(method);
            try
            {
                lock (this.getterEventMonitor)
                {
                    Monitor.PulseAll(this.getterEventMonitor);
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

        private void RemoveAllMessages()
        {
            uint method = 0x20e;
            this.TrEntry(method);
            try
            {
                this.proxyQueueLock.AssertOnCurrentThreadHoldsLock();
                while (this.queueTop != null)
                {
                    this.RemoveMessage(false, false, true, this.queueTop);
                }
                if (this.lastBrowsed != null)
                {
                    this.FreeMessage(this.lastBrowsed);
                }
                if (this.lastBrowsed != null)
                {
                    this.lastBrowsed = null;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void RemoveCbd(MQCBD callbackDesc)
        {
            uint method = 0x601;
            this.TrEntry(method, new object[] { callbackDesc });
            for (int i = 0; i < this.mqCBDList.Count; i++)
            {
                string cbdId = callbackDesc.CbdId;
                string str2 = ((MQCBD) this.mqCBDList[i]).CbdId;
                if (cbdId.Equals(str2))
                {
                    this.mqCBDList.RemoveAt(i);
                    break;
                }
            }
            base.TrExit(method);
        }

        public void RemoveMessage(bool refill, bool keepBrowse, bool freeMsg, MQProxyMessage message)
        {
            uint method = 0x20d;
            this.TrEntry(method, new object[] { refill, keepBrowse, freeMsg, message });
            try
            {
                this.proxyQueueLock.AssertOnCurrentThreadHoldsLock();
                int num2 = message.MsgDescByteSize + message.MsgLength;
                this.currentBytes -= num2;
                this.processedBytes += num2;
                MQProxyMessage older = message.Older;
                MQProxyMessage newer = message.Newer;
                if (newer != null)
                {
                    newer.Older = older;
                }
                if (older != null)
                {
                    older.Newer = newer;
                }
                if (this.queueTop == message)
                {
                    this.queueTop = newer;
                }
                if (this.queueNewest == message)
                {
                    this.queueNewest = older;
                }
                if (this.queueOldest == message)
                {
                    this.queueOldest = newer;
                }
                if (((refill && ((this.status & 0x400) != 0)) && (((this.status & 0x10) == 0) && ((this.status & 0x80) == 0))) && ((((this.status & 0x8000) == 0) && (this.currentBytes < this.highwaterMark)) && (this.processedBytes >= (this.highwaterMark - this.lowwaterMark))))
                {
                    this.RequestMessages(false, 0, false, false);
                }
                if (keepBrowse && ((message.Type & 1) != 0))
                {
                    this.lastBrowsed = message;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void RequestMessages(bool check, int waitInterval, bool mqgetCallWaiting, bool asyncConsumer)
        {
            uint method = 0x207;
            this.TrEntry(method, new object[] { check, waitInterval, mqgetCallWaiting, asyncConsumer });
            try
            {
                this.proxyQueueLock.AssertOnCurrentThreadHoldsLock();
                int compCode = 0;
                int reason = 0;
                MQSession session = this.hconn.Session;
                base.TrText(method, string.Concat(new object[] { "Hconn value = ", this.hconn.Value, ",` Session = ", this.hconn.Session.ConnectionIdAsString }));
                if (this.hconn.IsDisconnected)
                {
                    NmqiException exception = null;
                    if (session.ConnectionBroken)
                    {
                        base.TrText(method, "Session is disconnected, indicated CONN_BROKEN");
                        exception = new NmqiException(base.env, -1, null, 2, 0x7d9, null);
                    }
                    else
                    {
                        exception = new NmqiException(base.env, -1, null, 2, 0x7e2, null);
                    }
                    throw exception;
                }
                if ((this.status & 8) != 0)
                {
                    this.status &= -9;
                }
                if (asyncConsumer && ((this.status & 0x2000000) != 0))
                {
                    this.CheckGetMsgOptions(asyncConsumer, this.mqcbMD, this.mqcbGmo, ref compCode, ref reason);
                    this.CheckSelectionCriteria(this.mqcbMD, this.mqcbGmo, this.MqcbCBD.MaxMsgLength, ref compCode, ref reason);
                    try
                    {
                        this.statusSync.Acquire();
                        this.status &= -33554433;
                    }
                    finally
                    {
                        this.statusSync.Release();
                    }
                }
                MQREQUEST_MSGS mqrequest_msgs = new MQREQUEST_MSGS();
                try
                {
                    this.RequestMutex();
                    base.TrText(method, "Mutex acquired");
                    mqrequest_msgs.reqMsg.version = 1;
                    mqrequest_msgs.reqMsg.hObj = this.Hobj;
                    mqrequest_msgs.reqMsg.receivedBytes = this.receivedBytes;
                    this.receivedBytes = 0;
                    this.processedBytes = 0;
                    mqrequest_msgs.reqMsg.globalMessageIndex = this.hconn.GlobalMessageIndex;
                }
                finally
                {
                    this.ReleaseMutex();
                    base.TrText(method, "Mutex released");
                }
                if ((this.status & 0x400) != 0)
                {
                    this.requestedBytes = this.highwaterMark - this.currentBytes;
                    if (this.requestedBytes < 1)
                    {
                        this.requestedBytes = 0;
                    }
                    if ((this.highwaterMark > 0) && (((10L * this.backlog) / ((long) this.highwaterMark)) > 8L))
                    {
                        this.queueStatus |= 2;
                    }
                    else
                    {
                        this.queueStatus &= -3;
                    }
                    try
                    {
                        this.statusSync.Acquire();
                        this.status &= -2049;
                    }
                    finally
                    {
                        this.statusSync.Release();
                    }
                    base.TrText(method, "Queue Status = " + this.queueStatus);
                }
                else
                {
                    this.requestedBytes = 1;
                    try
                    {
                        this.statusSync.Acquire();
                        this.status |= 0x800;
                    }
                    finally
                    {
                        this.statusSync.Release();
                    }
                }
                mqrequest_msgs.reqMsg.requestedBytes = this.requestedBytes;
                mqrequest_msgs.reqMsg.maxMessageLength = this.maxMsgLength;
                mqrequest_msgs.reqMsg.waitInterval = waitInterval;
                mqrequest_msgs.reqMsg.getMessageOptions = this.getMsgOptions;
                mqrequest_msgs.reqMsg.requestFlags = 0;
                mqrequest_msgs.reqMsg.queueStatus = this.queueStatus;
                if (((this.getMsgOptions & 0x10) != 0) && ((this.getMsgOptions & 0x20) != 0))
                {
                    this.getMsgOptions &= -17;
                }
                int num4 = 0;
                if (mqgetCallWaiting)
                {
                    num4 |= 8;
                }
                else if ((this.hconn.IsStarted() && !this.hconn.IsSuspended()) && ((this.queueTop == null) && this.hconn.CheckClientEmpty()))
                {
                    num4 |= 1;
                }
                if ((mqrequest_msgs.reqMsg.codedCharSetId != this.codedCharSetId) && (this.maxMsgLength == -1))
                {
                    mqrequest_msgs.reqMsg.codedCharSetId = this.codedCharSetId;
                    mqrequest_msgs.reqMsg.encoding = this.encoding;
                }
                if ((this.status & 0x10000) != 0)
                {
                    num4 |= 0x10;
                    mqrequest_msgs.reqMsg.requestFlags = num4;
                    mqrequest_msgs.reqMsg.selectionIndex = (short) this.selectionIndex;
                    mqrequest_msgs.reqMsg.mqmdVersion = this.mqmdVersion;
                    mqrequest_msgs.reqMsg.codedCharSetId = this.codedCharSetId;
                    mqrequest_msgs.reqMsg.encoding = this.encoding;
                    mqrequest_msgs.reqMsg.msgSequenceNumber = this.msgSeqNumber;
                    mqrequest_msgs.reqMsg.offset = this.offset;
                    mqrequest_msgs.reqMsg.matchOptions = this.matchOptions;
                    if ((this.matchOptions & 1) != 0)
                    {
                        this.currentMsgId.CopyTo(mqrequest_msgs.reqMsg.msgId, 0);
                    }
                    if ((this.matchOptions & 2) != 0)
                    {
                        this.currentCorrelId.CopyTo(mqrequest_msgs.reqMsg.correlId, 0);
                    }
                    if ((this.matchOptions & 4) != 0)
                    {
                        this.currentGroupId.CopyTo(mqrequest_msgs.reqMsg.groupId, 0);
                    }
                    if ((this.matchOptions & 0x20) != 0)
                    {
                        this.currentMsgToken.CopyTo(mqrequest_msgs.reqMsg.msgToken, 0);
                    }
                }
                try
                {
                    this.statusSync.Acquire();
                    this.status &= -73729;
                }
                finally
                {
                    this.statusSync.Release();
                }
                if (check)
                {
                    MQTSH mqtsh2;
                    num4 |= 2;
                    if ((this.status & 0x400) == 0)
                    {
                        num4 |= 4;
                    }
                    mqrequest_msgs.reqMsg.requestFlags = num4;
                    int size = 0x24 + mqrequest_msgs.GetLength();
                    MQTSH requestTsh = session.AllocateTshForPQReqMsgs(size);
                    requestTsh.ControlFlags1 = (byte) (requestTsh.ControlFlags1 | 1);
                    requestTsh.Offset = requestTsh.WriteStruct(requestTsh.TshBuffer, 0);
                    requestTsh.Offset = mqrequest_msgs.WriteStruct(requestTsh.TshBuffer, requestTsh.Offset);
                    requestTsh.Offset = 0;
                    this.ReleaseMutex();
                    try
                    {
                        mqtsh2 = session.ExchangeTSH(requestTsh);
                    }
                    finally
                    {
                        this.RequestMutex();
                    }
                    if (mqtsh2.SegmentType != 15)
                    {
                        base.TrData(method, 0, "Unexpected reply received", 0, mqtsh2.Length, mqtsh2.TshBuffer);
                        CommonServices.SetValidInserts();
                        CommonServices.ArithInsert1 = mqtsh2.SegmentType;
                        CommonServices.CommentInsert1 = mqtsh2.ConversationID.ToString();
                        CommonServices.CommentInsert2 = "Unexpected reply received on proxy queue's requst messages";
                        base.FFST("@(#) lib/dotnet/pc/winnt/nmqi/managed/MQProxyQueue.cs, dotnet, p000, p000-L091124  1.6 09/11/19 13:02:00", "%C%", method, 1, 0x20009213, 0);
                        NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x893, null);
                        throw exception2;
                    }
                    session.ReleaseReceivedTSH(mqtsh2);
                }
                else
                {
                    mqrequest_msgs.reqMsg.requestFlags = num4;
                    int num6 = 0x24 + mqrequest_msgs.GetLength();
                    MQTSH mqtsh3 = session.AllocateTshForPQReqMsgs(num6);
                    mqtsh3.Offset = mqtsh3.WriteStruct(mqtsh3.TshBuffer, mqtsh3.Offset);
                    mqtsh3.Offset = mqrequest_msgs.WriteStruct(mqtsh3.TshBuffer, mqtsh3.Offset);
                    mqtsh3.Offset = 0;
                    session.SendData(mqtsh3.TshBuffer, 0, mqtsh3.Length, mqtsh3.SegmentType, 1);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void RequestMutex()
        {
            uint method = 0x201;
            this.TrEntry(method);
            try
            {
                this.proxyQueueLock.Acquire();
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void ResetBrowseFirst()
        {
            uint method = 0x20f;
            this.TrEntry(method);
            try
            {
                this.proxyQueueLock.AssertOnCurrentThreadHoldsLock();
                this.RemoveAllMessages();
                try
                {
                    this.statusSync.Acquire();
                    this.status |= 0x1000;
                }
                finally
                {
                    this.statusSync.Release();
                }
                try
                {
                    this.statusSync.Acquire();
                    this.status |= 0x800;
                }
                finally
                {
                    this.statusSync.Release();
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void ResetForReconnect()
        {
            uint method = 0x5f0;
            this.TrEntry(method);
            try
            {
                this.RequestMutex();
                if (this.connectionFailure != null)
                {
                    this.connectionFailure = null;
                }
                if (this.progressMsg != null)
                {
                    this.progressMsg = null;
                }
                this.progressMsgIndex = 0;
                this.progressMsgOffset = 0;
                try
                {
                    this.statusSync.Acquire();
                    this.status |= 0x10000;
                    this.status |= 0x800;
                }
                finally
                {
                    this.statusSync.Release();
                }
                this.receivedBytes = 0;
                this.processedBytes = 0;
                this.requestedBytes = 0;
                this.cbAlreadyRegistered = false;
                if ((this.status & 0x10000000) == 0x10000000)
                {
                    for (MQProxyMessage message = this.queueTop; message != null; message = message.Newer)
                    {
                        if (((message.Type & 1) == 1) && message.Transactional)
                        {
                            this.RemoveMessage(false, false, true, message);
                            return;
                        }
                    }
                }
            }
            finally
            {
                this.ReleaseMutex();
                base.TrExit(method);
            }
        }

        public void SendConsumerState(bool reply)
        {
            uint method = 0x5ec;
            this.TrEntry(method, new object[] { reply });
            try
            {
                if ((this.status & 0x20000000) == 0)
                {
                    int num2;
                    if (this.IsCbAlreadyRegistered || ((this.status & 0x200000) != 0))
                    {
                        num2 = 0;
                    }
                    else if ((this.status & 0x40000) != 0)
                    {
                        num2 = 0x11;
                    }
                    else
                    {
                        num2 = 0x10;
                    }
                    this.statusSync.Acquire();
                    this.status |= 0x800;
                    this.statusSync.Release();
                    this.hconn.SendNotification(this.hobj.Value, 5, num2, reply);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetAsyncSelection()
        {
            uint method = 0x5ed;
            this.TrEntry(method);
            int compCode = 0;
            int reason = 0;
            try
            {
                this.CheckSelectionCriteria(this.mqcbMD, this.mqcbGmo, this.MqcbCBD.MaxMsgLength, ref compCode, ref reason);
                this.asyncSelectionIndex = this.selectionIndex;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void SetIdentifier(IBM.WMQ.Nmqi.Hobj hObj, int openOptions, int defReadAhead)
        {
            uint method = 0x203;
            this.TrEntry(method, new object[] { hObj, openOptions, defReadAhead });
            int num2 = 0;
            try
            {
                this.hobj = (ManagedHobj) hObj;
                num2 = openOptions & 0x180000;
                if (defReadAhead == 2)
                {
                    num2 = 0x80000;
                    base.TrData(method, 0, "Read-ahead disabled at server. Default Read Ahead", BitConverter.GetBytes(defReadAhead));
                }
                if (num2 == 0)
                {
                    num2 = (defReadAhead == 1) ? 0x100000 : 0x80000;
                    base.TrData(method, 0, "Default Read Ahead", BitConverter.GetBytes(defReadAhead));
                }
                base.TrData(method, 0, "Read Ahead Options", BitConverter.GetBytes(num2));
                try
                {
                    this.statusSync.Acquire();
                    if ((num2 == 0x100000) && (this.highwaterMark != 0))
                    {
                        this.status |= 0x4000;
                    }
                }
                finally
                {
                    this.statusSync.Release();
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void UnsetCallbackOnEmpty()
        {
            uint method = 0x5fe;
            this.TrEntry(method);
            try
            {
                this.statusSync.Acquire();
                this.status &= -16777217;
            }
            finally
            {
                this.statusSync.Release();
                base.TrExit(method);
            }
        }

        public void ValidateMQGMO(MQGetMessageOptions gmo)
        {
            uint method = 0x5fb;
            this.TrEntry(method, new object[] { gmo });
            try
            {
                if ((gmo.Version < 1) || (gmo.Version > 4))
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x8d0, null);
                    base.env.LastException = ex;
                    base.TrException(method, ex);
                    throw ex;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void ValidateMQMD(MQMessageDescriptor md)
        {
            uint method = 0x5fc;
            this.TrEntry(method, new object[] { md });
            try
            {
                if ((md.Version < 1) || (md.Version > 2))
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x8d1, null);
                    base.env.LastException = ex;
                    base.TrException(method, ex);
                    throw ex;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public int AsyncSelectionIndex
        {
            get
            {
                return this.asyncSelectionIndex;
            }
            set
            {
                this.asyncSelectionIndex = value;
            }
        }

        public MQCBC CallbackCBC
        {
            get
            {
                return this.callbackCBC;
            }
            set
            {
                this.callbackCBC = value;
            }
        }

        public int EventsRaised
        {
            get
            {
                return this.eventsRaised;
            }
            set
            {
                this.eventsRaised = value;
            }
        }

        internal int Hobj
        {
            get
            {
                return this.hobj.Handle;
            }
        }

        public bool IsCbAlreadyRegistered
        {
            get
            {
                return this.cbAlreadyRegistered;
            }
            set
            {
                this.cbAlreadyRegistered = value;
            }
        }

        public ManagedHconn ManHconn
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

        public ManagedHobj ManHobj
        {
            get
            {
                return this.hobj;
            }
            set
            {
                this.hobj = value;
            }
        }

        public MQCBD MqcbCBD
        {
            get
            {
                MQCBD mqcbd = null;
                if (this.mqCBDList.Count > 0)
                {
                    mqcbd = (MQCBD) this.mqCBDList[0];
                }
                return mqcbd;
            }
        }

        public MQGetMessageOptions MqcbGmo
        {
            get
            {
                return this.mqcbGmo;
            }
            set
            {
                this.mqcbGmo = value;
            }
        }

        public MQMessageDescriptor MqcbMD
        {
            get
            {
                return this.mqcbMD;
            }
            set
            {
                this.mqcbMD = value;
            }
        }

        public long NoMsgTime
        {
            get
            {
                return this.noMsgTime;
            }
            set
            {
                this.noMsgTime = value;
            }
        }

        public MQProxyMessage QueueTop
        {
            get
            {
                return this.queueTop;
            }
            set
            {
                this.queueTop = value;
            }
        }

        public int SelectionIndex
        {
            get
            {
                return this.selectionIndex;
            }
            set
            {
                this.selectionIndex = value;
            }
        }

        public int Status
        {
            get
            {
                return this.status;
            }
            set
            {
                this.status = value;
            }
        }
    }
}

