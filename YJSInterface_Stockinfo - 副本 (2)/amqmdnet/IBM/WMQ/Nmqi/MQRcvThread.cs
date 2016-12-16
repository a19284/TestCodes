namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    internal class MQRcvThread : NmqiObject
    {
        private IMQCommsBuffer commsBuffer;
        private MQCommsBufferPool commsBufferPool;
        private bool disconnecting;
        private const int RECONNECT_CLIENTS_NO = 0;
        private const int RECONNECT_CLIENTS_YES = 1;
        private MQFAPConnection remoteConnection;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        public int threadId;

        public MQRcvThread(NmqiEnvironment nmqiEnv, MQFAPConnection remoteConn) : base(nmqiEnv)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv, remoteConn });
            this.remoteConnection = remoteConn;
            this.commsBufferPool = this.remoteConnection.CommsBufferPool;
        }

        private int ReceiveBuffer()
        {
            uint method = 0x610;
            this.TrEntry(method);
            int dataAvailable = this.commsBuffer.DataAvailable;
            int offset = dataAvailable + this.commsBuffer.DataUsed;
            int result = 0;
            byte[] buffer = this.commsBuffer.Buffer;
            try
            {
                try
                {
                    result = this.remoteConnection.Receive(buffer, offset, buffer.Length - offset, SocketFlags.None);
                    this.commsBuffer.Buffer = buffer;
                    base.TrText(method, "Number of bytes read from Socket by RcvThread = " + result);
                }
                catch (NmqiException exception)
                {
                    base.TrException(method, exception);
                    if (!this.disconnecting)
                    {
                        throw exception;
                    }
                    result = -1;
                }
                if (!this.disconnecting && (result <= 0))
                {
                    NmqiException exception2 = new NmqiException(base.env, 0x23f8, null, 2, 0x7d9, null);
                    throw exception2;
                }
                if (result > 0)
                {
                    this.commsBuffer.DataAvailable = dataAvailable + result;
                }
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        private MQTSH ReceiveOneTSH()
        {
            uint method = 0x60f;
            this.TrEntry(method);
            int count = 0;
            int capacity = 0;
            MQTSH result = null;
            IMQCommsBuffer buffer = null;
            try
            {
                do
                {
                    if ((this.commsBuffer != null) && ((count = this.commsBuffer.DataAvailable) >= 8))
                    {
                        capacity = IPAddress.HostToNetworkOrder(BitConverter.ToInt32(this.commsBuffer.Buffer, this.commsBuffer.DataUsed + 4));
                        if (capacity <= count)
                        {
                            buffer = this.commsBufferPool.AllocateBuffer(capacity);
                            try
                            {
                                Buffer.BlockCopy(this.commsBuffer.Buffer, this.commsBuffer.DataUsed, buffer.Buffer, 0, capacity);
                            }
                            catch (Exception exception)
                            {
                                base.TrException(method, exception);
                                throw exception;
                            }
                            buffer.DataAvailable = capacity;
                            buffer.DataUsed = 0;
                            byte differentiator = buffer.Buffer[3];
                            result = new MQTSH(MQTSH.GetTshType(differentiator), buffer, true);
                            result.Offset = result.ReadStruct(result.TshBuffer, 0);
                            this.commsBuffer.DataUsed += capacity;
                            this.commsBuffer.DataAvailable = count - capacity;
                            base.TrData(method, 0, "Current TSH", 0, buffer.DataAvailable, buffer.Buffer);
                            return result;
                        }
                    }
                    IMQCommsBuffer buffer2 = this.commsBufferPool.AllocateBuffer(this.remoteConnection.MaxTransmissionSize + 8);
                    if (count != 0)
                    {
                        Buffer.BlockCopy(this.commsBuffer.Buffer, this.commsBuffer.DataUsed, buffer2.Buffer, 0, count);
                        buffer2.DataAvailable = count;
                        base.TrData(method, 0, "Following bytes are left from last read", 0, buffer2.DataAvailable, buffer2.Buffer);
                    }
                    if (this.commsBuffer != null)
                    {
                        this.commsBuffer.Free();
                    }
                    this.commsBuffer = buffer2;
                }
                while (this.ReceiveBuffer() > 0);
                result = null;
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        public void Run()
        {
            uint method = 0x60e;
            this.TrEntry(method);
            this.threadId = Thread.CurrentThread.GetHashCode();
            string str = "RcvThread: " + this.remoteConnection.ToString();
            Thread.CurrentThread.Name = str;
            try
            {
                int conversationID;
                int requestID;
                MQSession sessionByConvId;
                int segmentType;
                MQSOCKACT mqsockact;
                MQTSH rTSH = null;
            Label_0040:
                rTSH = this.ReceiveOneTSH();
                if (rTSH == null)
                {
                    base.TrText(method, "Null TSH is received.. breaking and disconnecting now");
                    return;
                }
                if (this.remoteConnection.RemoteEncoding != rTSH.Encoding)
                {
                    this.remoteConnection.RemoteEncoding = rTSH.Encoding;
                }
                switch (rTSH.TSHType)
                {
                    case 1:
                        base.TrText(method, "TSHM data has been received");
                        conversationID = rTSH.ConversationID;
                        requestID = rTSH.RequestID;
                        base.TrText(method, "ConversationId = " + conversationID);
                        base.TrText(method, "RequestId = " + requestID);
                        sessionByConvId = this.remoteConnection.GetSessionByConvId(conversationID);
                        if ((sessionByConvId == null) || !sessionByConvId.IsEndRequested)
                        {
                            break;
                        }
                        base.TrText(method, "Current Sessions " + sessionByConvId.ToString() + " ending..");
                        goto Label_0040;

                    case 2:
                    {
                        if (rTSH.SegmentType != 12)
                        {
                            goto Label_0515;
                        }
                        mqsockact = new MQSOCKACT();
                        if ((rTSH.Encoding & 15) == 2)
                        {
                            goto Label_037D;
                        }
                        base.TrText(method, "We received BigEndian data, read it into our encoding");
                        byte[] dst = new byte[mqsockact.GetLength()];
                        Buffer.BlockCopy(rTSH.TshBuffer, rTSH.Offset, dst, 0, mqsockact.GetLength());
                        Array.Reverse(dst);
                        mqsockact.ReadStruct(dst, 0);
                        goto Label_0391;
                    }
                    default:
                    {
                        CommonServices.SetValidInserts();
                        CommonServices.ArithInsert1 = 1;
                        CommonServices.CommentInsert1 = this.remoteConnection.NegotiatedChannel.ChannelName;
                        CommonServices.CommentInsert2 = "Invalid TSH flow is received on the receiver thread";
                        base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                        NmqiException exception7 = new NmqiException(base.env, -1, null, 2, 0x7d9, null);
                        base.TrException(method, exception7);
                        throw exception7;
                    }
                }
                if (requestID == 0)
                {
                    if (sessionByConvId != null)
                    {
                        sessionByConvId.DeliverTSH(rTSH);
                    }
                    else if (conversationID == 1)
                    {
                        this.remoteConnection.DeliverTSH(rTSH);
                    }
                    else
                    {
                        base.TrText(method, "Unknown Conversation & RequestId");
                    }
                }
                else
                {
                    if (requestID != 1)
                    {
                        goto Label_0285;
                    }
                    if (sessionByConvId == null)
                    {
                        base.TrText(method, "Unknown Conversation and no existing Session could be found for it.");
                    }
                    else
                    {
                        MQProxyQueueManager proxyQueueManager;
                        rTSH = sessionByConvId.ProcessReceivedData(rTSH);
                        segmentType = rTSH.SegmentType;
                        switch (segmentType)
                        {
                            case 13:
                            case 15:
                                proxyQueueManager = sessionByConvId.Hconn.ProxyQueueManager;
                                if (proxyQueueManager == null)
                                {
                                    NmqiException exception = new NmqiException(base.env, -1, null, 2, 0x7d9, null);
                                    CommonServices.SetValidInserts();
                                    CommonServices.ArithInsert1 = 1;
                                    CommonServices.ArithInsert2 = (uint) segmentType;
                                    CommonServices.CommentInsert1 = "PQM couldn't be found on obtained Session for TSH flow";
                                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                                    throw exception;
                                }
                                break;

                            default:
                                goto Label_0237;
                        }
                        switch (segmentType)
                        {
                            case 13:
                                proxyQueueManager.AddMessage(rTSH);
                                break;

                            case 15:
                                proxyQueueManager.ReceiveNotification(rTSH);
                                break;
                        }
                        sessionByConvId.ReleaseReceivedTSH(rTSH);
                    }
                }
                goto Label_0040;
            Label_0237:
                CommonServices.SetValidInserts();
                CommonServices.ArithInsert1 = 1;
                CommonServices.ArithInsert2 = (uint) segmentType;
                CommonServices.CommentInsert1 = "Incorrect Segment type received on multiplexing flow";
                base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x7d9, null);
                throw exception2;
            Label_0285:
                if ((requestID % 2) != 0)
                {
                    if (sessionByConvId == null)
                    {
                        base.TrText(method, "Unknown Conversation and no existing Session could be found for it.");
                    }
                    else
                    {
                        sessionByConvId.DeliverExchangeReply(requestID, rTSH);
                    }
                    goto Label_0040;
                }
                NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x893, null);
                CommonServices.SetValidInserts();
                CommonServices.ArithInsert1 = 1;
                CommonServices.CommentInsert1 = this.remoteConnection.NegotiatedChannel.ChannelName;
                CommonServices.CommentInsert2 = "Unknown request id flow on the multiplexed connection";
                base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                base.TrException(method, ex);
                throw ex;
            Label_037D:
                mqsockact.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
            Label_0391:
                switch (mqsockact.Type)
                {
                    case 1:
                    case 4:
                    case 5:
                    case 6:
                        goto Label_0040;

                    case 2:
                    {
                        bool informQmgr = false;
                        if ((rTSH.ControlFlags1 & 1) != 0)
                        {
                            base.TrText(method, "rfpSAT_END_CONV request with rfpTCF_CONFIRM_REQUEST received");
                            informQmgr = true;
                        }
                        int convId = mqsockact.ConversationID;
                        MQSession session2 = this.remoteConnection.GetSessionByConvId(convId);
                        if (session2 != null)
                        {
                            this.remoteConnection.RemoveSession(session2.ConversationId, informQmgr);
                            NmqiException exception4 = new NmqiException(base.env, -1, null, 2, 0x7d9, null);
                            base.TrException(method, exception4);
                            session2.AsyncFailureNotify(exception4);
                        }
                        else if (convId == 1)
                        {
                            NmqiException exception5 = new NmqiException(base.env, -1, null, 2, 0x7d9, null);
                            base.TrException(method, exception5);
                            this.remoteConnection.AsyncFailureNotify(exception5, false);
                        }
                        else
                        {
                            base.TrData(method, 1, "hConn not recognised by connection,ConversationID ", BitConverter.GetBytes(mqsockact.ConversationID));
                        }
                        goto Label_057D;
                    }
                    case 3:
                    {
                        NmqiException exception6 = new NmqiException(base.env, -1, null, 2, 0x89a, null);
                        base.TrException(method, exception6);
                        this.remoteConnection.AsyncFailureNotify(exception6, true);
                        goto Label_0040;
                    }
                    case 7:
                        this.remoteConnection.QmQuiescing();
                        goto Label_057D;

                    case 9:
                        switch (mqsockact.Parm1)
                        {
                            case 1:
                                goto Label_0507;
                        }
                        goto Label_057D;

                    default:
                        goto Label_057D;
                }
                this.remoteConnection.NotifyReconnect(false);
                goto Label_057D;
            Label_0507:
                this.remoteConnection.NotifyReconnect(true);
                goto Label_057D;
            Label_0515:
                if ((rTSH.ControlFlags1 & 8) != 0)
                {
                    this.remoteConnection.AnalyseErrorSegment(rTSH);
                    goto Label_057D;
                }
                if (rTSH.SegmentType != 9)
                {
                    goto Label_0570;
                }
                if (this.remoteConnection.FapLevel >= 10)
                {
                    if ((rTSH.ControlFlags1 & 1) != 0)
                    {
                        goto Label_0562;
                    }
                    goto Label_0040;
                }
                if ((rTSH.ControlFlags1 & 1) != 0)
                {
                    goto Label_0040;
                }
            Label_0562:
                this.remoteConnection.SendHeartbeat(2);
                goto Label_057D;
            Label_0570:
                if (rTSH.SegmentType == 11)
                {
                    goto Label_0040;
                }
            Label_057D:
                this.remoteConnection.ReleaseReceivedTSH(rTSH);
                goto Label_0040;
            }
            catch (Exception exception8)
            {
                base.TrException(method, exception8, 1);
                NmqiException e = null;
                if (exception8 is NmqiException)
                {
                    e = (NmqiException) exception8;
                }
                else if (exception8 is MQException)
                {
                    e = new NmqiException(base.env, -1, null, (exception8 as MQException).CompCode, (exception8 as MQException).Reason, null);
                }
                else
                {
                    int compCode = 2;
                    int reason = 0x893;
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = 1;
                    CommonServices.CommentInsert1 = this.remoteConnection.NegotiatedChannel.ChannelName.Trim();
                    CommonServices.CommentInsert2 = "System Exception generated on Receiver thread - " + exception8.Message;
                    CommonServices.CommentInsert3 = "Exception Stack - " + exception8.StackTrace;
                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                    e = new NmqiException(base.env, -1, null, compCode, reason, exception8);
                }
                try
                {
                    if (this.remoteConnection != null)
                    {
                        this.remoteConnection.AsyncFailureNotify(e, false);
                    }
                }
                catch (NmqiException exception10)
                {
                    base.TrException(method, exception10, 2);
                }
                catch (Exception exception11)
                {
                    base.TrException(method, exception11, 3);
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = 1;
                    CommonServices.CommentInsert1 = this.remoteConnection.NegotiatedChannel.ChannelName;
                    CommonServices.CommentInsert2 = "System Exception generated on AsyncFailureNotify - " + exception11.Message;
                    CommonServices.CommentInsert3 = "Exception Stack - " + exception11.StackTrace;
                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                }
                if (!this.disconnecting)
                {
                    try
                    {
                        if (this.remoteConnection != null)
                        {
                            this.remoteConnection.Disconnect();
                        }
                    }
                    catch (NmqiException exception12)
                    {
                        base.TrException(method, exception12, 4);
                    }
                    catch (Exception exception13)
                    {
                        base.TrException(method, exception13, 5);
                        CommonServices.SetValidInserts();
                        CommonServices.ArithInsert1 = 1;
                        CommonServices.CommentInsert1 = this.remoteConnection.NegotiatedChannel.ChannelName;
                        CommonServices.CommentInsert2 = "System Exception generated while Disconnecting - " + exception13.Message;
                        CommonServices.CommentInsert3 = "Exception Stack - " + exception13.StackTrace;
                        base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                    }
                }
            }
            finally
            {
                if (this.commsBuffer != null)
                {
                    this.commsBuffer.Free();
                }
                base.TrExit(method);
            }
        }

        internal void SetDisconnecting()
        {
            uint method = 0x611;
            this.TrEntry(method);
            this.disconnecting = true;
            base.TrExit(method);
        }
    }
}

