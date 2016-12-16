namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Collections;
    using System.Globalization;

    public class MQQueue : MQDestination
    {
        private string dynamicQueueName;
        private string queueManagerName;
        private const string sccsid = "@(#) lib/dotnet/pc/winnt/baseclasses/MQQueue.cs, dotnet, p000  1.8 11/04/07 10:34:41";
        private bool spiQueueEmpty;

        protected MQQueue() : base(1)
        {
            base.TrConstructor("@(#) lib/dotnet/pc/winnt/baseclasses/MQQueue.cs, dotnet, p000  1.8 11/04/07 10:34:41");
        }

        public MQQueue(MQQueueManager qMgr, string queueName, int openOptions, string queueManagerName, string dynamicQueueName, string alternateUserId) : base(1, qMgr, queueName, openOptions, alternateUserId)
        {
            base.TrConstructor("@(#) lib/dotnet/pc/winnt/baseclasses/MQQueue.cs, dotnet, p000  1.8 11/04/07 10:34:41", new object[] { qMgr, queueName, openOptions, queueManagerName, dynamicQueueName, alternateUserId });
            if (queueManagerName == null)
            {
                this.queueManagerName = "";
            }
            else
            {
                this.queueManagerName = queueManagerName;
            }
            if ((dynamicQueueName == null) || (dynamicQueueName == string.Empty))
            {
                this.dynamicQueueName = "AMQ.*";
            }
            else
            {
                this.dynamicQueueName = dynamicQueueName;
            }
            MQObjectDescriptor od = base.CreateMQObjectDescriptor();
            od.ObjectQMgrName = this.queueManagerName;
            od.DynamicQName = this.dynamicQueueName;
            base.Open(ref od);
            base.name = od.ObjectName.PadRight(0x30);
        }

        public override void Close()
        {
            uint method = 0x225;
            this.TrEntry(method);
            try
            {
                base.Close();
                base.qMgr = null;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private MQMessage CopyMDFromOldMsgIntoFwdMsg(MQMessage oldMsg, MQMessage newMsg)
        {
            MQMessage message;
            uint method = 0x22f;
            this.TrEntry(method, new object[] { oldMsg, newMsg });
            try
            {
                newMsg.AccountingToken = oldMsg.AccountingToken;
                newMsg.ApplicationIdData = oldMsg.ApplicationIdData;
                newMsg.ApplicationOriginData = oldMsg.ApplicationOriginData;
                newMsg.CharacterSet = oldMsg.CharacterSet;
                newMsg.CorrelationId = oldMsg.CorrelationId;
                newMsg.Encoding = oldMsg.Encoding;
                newMsg.Expiry = oldMsg.Expiry;
                newMsg.Feedback = oldMsg.Feedback;
                newMsg.Format = oldMsg.Format;
                newMsg.GroupId = oldMsg.GroupId;
                newMsg.MessageFlags = oldMsg.MessageFlags;
                newMsg.MessageId = oldMsg.MessageId;
                newMsg.MessageSequenceNumber = oldMsg.MessageSequenceNumber;
                newMsg.MessageType = oldMsg.MessageType;
                newMsg.Offset = oldMsg.Offset;
                newMsg.OriginalLength = oldMsg.OriginalLength;
                newMsg.Persistence = oldMsg.Persistence;
                newMsg.Priority = oldMsg.Priority;
                newMsg.PutApplicationName = oldMsg.PutApplicationName;
                newMsg.PutApplicationType = oldMsg.PutApplicationType;
                newMsg.PutDateTime = oldMsg.PutDateTime;
                newMsg.ReplyToQueueManagerName = oldMsg.ReplyToQueueManagerName;
                newMsg.ReplyToQueueName = oldMsg.ReplyToQueueName;
                newMsg.Report = oldMsg.Report;
                newMsg.UserId = oldMsg.UserId;
                byte[] b = new byte[oldMsg.MessageLength];
                oldMsg.Seek(0);
                oldMsg.ReadFully(ref b);
                newMsg.Write(b);
                message = newMsg;
            }
            finally
            {
                base.TrExit(method);
            }
            return message;
        }

        private MQMessage CopyMDFromOldMsgIntoReplyMsg(MQMessage oldMsg, MQMessage newMsg, int putOpts)
        {
            MQMessage message;
            uint method = 560;
            this.TrEntry(method, new object[] { oldMsg, newMsg, putOpts });
            try
            {
                if (((oldMsg.Report & 0x4000) != 0) && ((oldMsg.Report & 0x8000000) != 0))
                {
                    newMsg.Report = 0x8000000;
                }
                else
                {
                    newMsg.Report = 0;
                }
                newMsg.MessageType = 2;
                if ((oldMsg.Report & 0x4000) != 0)
                {
                    newMsg.Expiry = oldMsg.Expiry;
                }
                else
                {
                    newMsg.Expiry = -1;
                }
                newMsg.Feedback = 0;
                if ((oldMsg.Report & 0x80) != 0)
                {
                    newMsg.MessageId = oldMsg.MessageId;
                }
                else if ((putOpts & 0x40) == 0)
                {
                    newMsg.MessageId = MQC.MQMI_NONE;
                }
                if ((oldMsg.Report & 0x40) != 0)
                {
                    newMsg.CorrelationId = oldMsg.CorrelationId;
                }
                else if ((putOpts & 0x80) == 0)
                {
                    newMsg.CorrelationId = oldMsg.CorrelationId;
                }
                newMsg.ReplyToQueueName = "";
                newMsg.ReplyToQueueManagerName = "";
                newMsg.GroupId = MQC.MQGI_NONE;
                newMsg.MessageSequenceNumber = 1;
                newMsg.Offset = 0;
                newMsg.MessageFlags = 0;
                newMsg.OriginalLength = -1;
                byte[] b = new byte[oldMsg.MessageLength];
                oldMsg.Seek(0);
                oldMsg.ReadFully(ref b);
                newMsg.Write(b);
                message = newMsg;
            }
            finally
            {
                base.TrExit(method);
            }
            return message;
        }

        private MQMessage CopyMDFromOldMsgIntoReportMsg(MQMessage oldMsg, MQMessage newMsg, int putOpts)
        {
            MQMessage message;
            uint method = 0x231;
            this.TrEntry(method, new object[] { oldMsg, newMsg, putOpts });
            try
            {
                if (((oldMsg.Report & 0x4000) != 0) && ((oldMsg.Report & 0x8000000) != 0))
                {
                    newMsg.Report = 0x8000000;
                }
                else
                {
                    newMsg.Report = 0;
                }
                newMsg.MessageType = 4;
                if ((oldMsg.Report & 0x4000) != 0)
                {
                    newMsg.Expiry = oldMsg.Expiry;
                }
                else
                {
                    newMsg.Expiry = -1;
                }
                if ((oldMsg.Report & 0x40) != 0)
                {
                    newMsg.CorrelationId = oldMsg.CorrelationId;
                }
                else if ((putOpts & 0x80) == 0)
                {
                    newMsg.CorrelationId = oldMsg.CorrelationId;
                }
                newMsg.ReplyToQueueName = "";
                newMsg.ReplyToQueueManagerName = "";
                byte[] b = new byte[oldMsg.MessageLength];
                oldMsg.Seek(0);
                oldMsg.ReadFully(ref b);
                newMsg.Write(b);
                message = newMsg;
            }
            finally
            {
                base.TrExit(method);
            }
            return message;
        }

        public void Get(MQMessage message, MQGetMessageOptions gmo, int maxMsgSize, int spigmo)
        {
            uint method = 550;
            this.TrEntry(method, new object[] { message, gmo, maxMsgSize, spigmo });
            int num2 = 0;
            bool flag = false;
            int dataLength = 0;
            byte[] buffer = null;
            int compCode = 0;
            int reason = 0;
            try
            {
                NmqiSP nmqiConnector = (NmqiSP) base.qMgr.nmqiConnector;
                if (message == null)
                {
                    base.throwNewMQException(2, 0x7ea);
                }
                if (gmo == null)
                {
                    base.throwNewMQException(2, 0x88a);
                }
                if (maxMsgSize < 0)
                {
                    base.throwNewMQException(2, 0x7d5);
                }
                MQSPIGetOpts spigo = new MQSPIGetOpts(spigmo);
                int options = gmo.Options;
                int characterSet = message.CharacterSet;
                int num7 = gmo.Options;
                if ((num7 & 0x8000000) != 0)
                {
                    gmo.Options &= -134217729;
                    gmo.Options |= 0x2000000;
                }
                else if ((((num7 & 0x10000000) == 0) && ((num7 & 0x2000000) == 0)) && ((num7 & 0x4000000) == 0))
                {
                    gmo.Options |= 0x2000000;
                }
                message.ClearMessage();
                num2 = gmo.Options;
                flag = true;
                if ((gmo.Options & 0x1006) == 0)
                {
                    gmo.Options |= 4;
                }
                if (maxMsgSize == 0)
                {
                    int num8 = gmo.Options;
                    MQBase.MQMD targetMqmd = new MQBase.MQMD();
                    base.CopyMQMD(message.md.StructMQMD, ref targetMqmd);
                    maxMsgSize = MQDestination.defaultMaxMsgSize;
                    buffer = new byte[maxMsgSize];
                    gmo.Options &= -65;
                    nmqiConnector.SPIGet(base.qMgr.hConn, base.objectHandle.HOBJ, ref message.md, ref gmo, ref spigo, maxMsgSize, buffer, out dataLength, out compCode, out reason);
                    while ((1 == compCode) && (0x820 == reason))
                    {
                        gmo.Options = num8;
                        MQBase.MQMD structMQMD = message.md.StructMQMD;
                        base.CopyMQMD(targetMqmd, ref structMQMD);
                        message.md.StructMQMD = structMQMD;
                        maxMsgSize = dataLength;
                        buffer = new byte[maxMsgSize];
                        gmo.Options &= -65;
                        nmqiConnector.SPIGet(base.qMgr.hConn, base.objectHandle.HOBJ, ref message.md, ref gmo, ref spigo, maxMsgSize, buffer, out dataLength, out compCode, out reason);
                    }
                    gmo.Options = num8;
                }
                else
                {
                    buffer = new byte[maxMsgSize];
                    nmqiConnector.SPIGet(base.qMgr.hConn, base.objectHandle.HOBJ, ref message.md, ref gmo, ref spigo, maxMsgSize, buffer, out dataLength, out compCode, out reason);
                }
                if (compCode != 2)
                {
                    this.spiQueueEmpty = spigo.QueueEmpty != 0;
                    message.spiInherited = spigo.Inherited != 0;
                    message.spiQTime = spigo.QTime;
                }
                else
                {
                    this.spiQueueEmpty = false;
                    message.spiInherited = false;
                    message.spiQTime = 0L;
                }
                byte[] b = null;
                if (compCode != 2)
                {
                    if ((options & 0x2000000) == 0)
                    {
                        b = base.PerformMsgProcessingAfterGet(ref message, buffer, (dataLength > buffer.Length) ? buffer.Length : dataLength);
                    }
                    else
                    {
                        b = buffer;
                    }
                    dataLength = b.Length;
                }
                gmo.Options = options;
                message.totalMessageLength = dataLength;
                if (dataLength > 0)
                {
                    message.Write(b, 0, (dataLength < maxMsgSize) ? dataLength : maxMsgSize);
                    message.Seek(0);
                }
                if (compCode != 0)
                {
                    base.qMgr.CheckHConnHealth(reason);
                    base.throwNewMQException(compCode, reason);
                }
            }
            catch (MQException exception)
            {
                compCode = exception.CompCode;
                reason = exception.Reason;
                throw exception;
            }
            finally
            {
                if (flag)
                {
                    gmo.Options = num2;
                }
                base.unsafe_compCode = compCode;
                base.unsafe_reason = reason;
                base.TrExit(method);
            }
        }

        protected void Put(MQMessage message, MQPutMessageOptions pmo, int spiOptions)
        {
            uint method = 0x227;
            this.TrEntry(method, new object[] { message, pmo, spiOptions });
            int options = 0;
            bool flag = false;
            byte[] src = null;
            int compCode = 0;
            int reason = 0;
            try
            {
                MQSPIPutOpts spipo = null;
                if (message == null)
                {
                    base.throwNewMQException(2, 0x7ea);
                }
                if (pmo == null)
                {
                    base.throwNewMQException(2, 0x87d);
                }
                spipo = new MQSPIPutOpts(spiOptions);
                options = pmo.Options;
                flag = true;
                if ((pmo.Options & 6) == 0)
                {
                    pmo.Options |= 4;
                }
                src = message.GetBuffer();
                base.tempMsgBuffer = new byte[src.Length];
                Buffer.BlockCopy(src, 0, base.tempMsgBuffer, 0, src.Length);
                int characterSet = message.CharacterSet;
                int encoding = message.Encoding;
                string format = message.Format;
                base.PerformMsgProcessgingBeforePut(ref message);
                src = message.GetBuffer();
                ((NmqiSP) base.qMgr.nmqiConnector).SPIPut(base.qMgr.hConn, base.objectHandle.HOBJ, ref message.md, ref pmo, ref spipo, src.Length, src, out compCode, out reason);
                if (compCode == 0)
                {
                    base.PerformMsgProcessingAfterPut(ref message, base.tempMsgBuffer, characterSet, encoding, format);
                }
                if (compCode != 0)
                {
                    base.qMgr.CheckHConnHealth(reason);
                    base.throwNewMQException(compCode, reason);
                }
            }
            catch (MQException exception)
            {
                compCode = exception.CompCode;
                reason = exception.Reason;
                throw exception;
            }
            finally
            {
                if (flag)
                {
                    pmo.Options = options;
                }
                base.unsafe_compCode = compCode;
                base.unsafe_reason = reason;
                base.TrExit(method);
            }
        }

        public void PutForwardMessage(MQMessage message)
        {
            uint method = 0x228;
            this.TrEntry(method, new object[] { message });
            try
            {
                this.PutForwardMessage(message, new MQPutMessageOptions());
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void PutForwardMessage(MQMessage message, MQPutMessageOptions putMessageOptions)
        {
            uint method = 0x229;
            this.TrEntry(method, new object[] { message, putMessageOptions });
            int compCode = 0;
            int reason = 0;
            try
            {
                MQMessage newMsg = new MQMessage();
                if (message == null)
                {
                    base.throwNewMQException(2, 0x7ea);
                }
                if (putMessageOptions == null)
                {
                    base.throwNewMQException(2, 0x87d);
                }
                if ((putMessageOptions.Options & 0x800000) != 0)
                {
                    newMsg = this.CopyMDFromOldMsgIntoFwdMsg(message, newMsg);
                    putMessageOptions.Options &= -8388609;
                }
                IEnumerator propertyNames = message.GetPropertyNames("%");
                MQPropertyDescriptor pd = new MQPropertyDescriptor();
                while (propertyNames.MoveNext())
                {
                    string name = propertyNames.Current.ToString();
                    object objectProperty = message.GetObjectProperty(name, pd);
                    if (this.ValidToCopy(pd.CopyOptions, 2))
                    {
                        newMsg.SetObjectProperty(name, pd, objectProperty);
                    }
                }
                base.Put(newMsg, putMessageOptions);
            }
            catch (MQException exception)
            {
                compCode = exception.CompCode;
                reason = exception.Reason;
                throw exception;
            }
            finally
            {
                base.unsafe_compCode = compCode;
                base.unsafe_reason = reason;
                base.TrExit(method);
            }
        }

        public void PutReplyMessage(MQMessage message)
        {
            uint method = 0x22a;
            this.TrEntry(method, new object[] { message });
            try
            {
                this.PutReplyMessage(message, new MQPutMessageOptions());
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void PutReplyMessage(MQMessage message, MQPutMessageOptions putMessageOptions)
        {
            uint method = 0x22b;
            this.TrEntry(method, new object[] { message, putMessageOptions });
            int compCode = 0;
            int reason = 0;
            try
            {
                if (message == null)
                {
                    base.throwNewMQException(2, 0x7ea);
                }
                if (putMessageOptions == null)
                {
                    base.throwNewMQException(2, 0x87d);
                }
                MQMessage newMsg = new MQMessage();
                if ((putMessageOptions.Options & 0x800000) != 0)
                {
                    newMsg = this.CopyMDFromOldMsgIntoReplyMsg(message, newMsg, putMessageOptions.Options);
                    putMessageOptions.Options &= -8388609;
                }
                IEnumerator propertyNames = message.GetPropertyNames("%");
                MQPropertyDescriptor pd = new MQPropertyDescriptor();
                while (propertyNames.MoveNext())
                {
                    string name = propertyNames.Current.ToString();
                    object objectProperty = message.GetObjectProperty(name, pd);
                    if (this.ValidToCopy(pd.CopyOptions, 8))
                    {
                        newMsg.SetObjectProperty(name, pd, objectProperty);
                    }
                }
                base.Put(newMsg, putMessageOptions);
            }
            catch (MQException exception)
            {
                compCode = exception.CompCode;
                reason = exception.Reason;
                throw exception;
            }
            finally
            {
                base.unsafe_compCode = compCode;
                base.unsafe_reason = reason;
                base.TrExit(method);
            }
        }

        public void PutReportMessage(MQMessage message)
        {
            uint method = 0x22c;
            this.TrEntry(method, new object[] { message });
            try
            {
                this.PutReportMessage(message, new MQPutMessageOptions());
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void PutReportMessage(MQMessage message, MQPutMessageOptions putMessageOptions)
        {
            uint method = 0x22d;
            this.TrEntry(method, new object[] { message, putMessageOptions });
            int compCode = 0;
            int reason = 0;
            try
            {
                if (message == null)
                {
                    base.throwNewMQException(2, 0x7ea);
                }
                if (putMessageOptions == null)
                {
                    base.throwNewMQException(2, 0x87d);
                }
                MQMessage newMsg = new MQMessage();
                if ((putMessageOptions.Options & 0x800000) != 0)
                {
                    newMsg = this.CopyMDFromOldMsgIntoReportMsg(message, newMsg, putMessageOptions.Options);
                    putMessageOptions.Options &= -8388609;
                }
                IEnumerator propertyNames = message.GetPropertyNames("%");
                MQPropertyDescriptor pd = new MQPropertyDescriptor();
                while (propertyNames.MoveNext())
                {
                    string name = propertyNames.Current.ToString();
                    object objectProperty = message.GetObjectProperty(name, pd);
                    if (this.ValidToCopy(pd.CopyOptions, 0x10))
                    {
                        newMsg.SetObjectProperty(name, pd, objectProperty);
                    }
                }
                base.Put(newMsg, putMessageOptions);
            }
            catch (MQException exception)
            {
                compCode = exception.CompCode;
                reason = exception.Reason;
                throw exception;
            }
            finally
            {
                base.unsafe_compCode = compCode;
                base.unsafe_reason = reason;
                base.TrExit(method);
            }
        }

        private bool ValidToCopy(int oldMsgCopyOpts, int newMsgCopyOpts)
        {
            uint method = 0x22e;
            this.TrEntry(method, new object[] { oldMsgCopyOpts, newMsgCopyOpts });
            bool flag = false;
            try
            {
                if ((oldMsgCopyOpts & 1) != 0)
                {
                    return true;
                }
                if ((oldMsgCopyOpts & 0x16) == newMsgCopyOpts)
                {
                    return true;
                }
                if ((oldMsgCopyOpts & newMsgCopyOpts) == newMsgCopyOpts)
                {
                    flag = true;
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return flag;
        }

        public DateTime AlterationDateTime
        {
            get
            {
                string str = base.QueryAttribute(0x7eb, 12);
                string str2 = base.QueryAttribute(0x7ec, 8);
                return DateTime.ParseExact(str.TrimEnd(new char[0]) + str2, "yyyy-MM-ddHH.mm.ss", new CultureInfo("en-US"));
            }
        }

        public string BackoutRequeueName
        {
            get
            {
                return base.QueryAttribute(0x7e3, 0x30);
            }
        }

        public int BackoutThreshold
        {
            get
            {
                return base.QueryAttribute(0x16);
            }
        }

        public string BaseQueueName
        {
            get
            {
                return base.QueryAttribute(0x7d2, 0x30);
            }
        }

        public string ClusterName
        {
            get
            {
                return base.QueryAttribute(0x7ed, 0x30);
            }
        }

        public string ClusterNamelistName
        {
            get
            {
                return base.QueryAttribute(0x7ee, 0x30);
            }
        }

        public int ClusterWorkLoadPriority
        {
            get
            {
                return base.QueryAttribute(0x60);
            }
        }

        public int ClusterWorkLoadRank
        {
            get
            {
                return base.QueryAttribute(0x5f);
            }
        }

        public int ClusterWorkLoadUseQ
        {
            get
            {
                return base.QueryAttribute(0x62);
            }
        }

        public int CurrentDepth
        {
            get
            {
                return base.QueryAttribute(3);
            }
        }

        public int DefaultBind
        {
            get
            {
                return base.QueryAttribute(0x3d);
            }
        }

        public int DefaultInputOpenOption
        {
            get
            {
                return base.QueryAttribute(4);
            }
        }

        public int DefaultPersistence
        {
            get
            {
                return base.QueryAttribute(5);
            }
        }

        public int DefaultPriority
        {
            get
            {
                return base.QueryAttribute(6);
            }
        }

        public int DefinitionType
        {
            get
            {
                return base.QueryAttribute(7);
            }
        }

        public int DepthHighEvent
        {
            get
            {
                return base.QueryAttribute(0x2b);
            }
        }

        public int DepthHighLimit
        {
            get
            {
                return base.QueryAttribute(40);
            }
        }

        public int DepthLowEvent
        {
            get
            {
                return base.QueryAttribute(0x2c);
            }
        }

        public int DepthLowLimit
        {
            get
            {
                return base.QueryAttribute(0x29);
            }
        }

        public int DepthMaximumEvent
        {
            get
            {
                return base.QueryAttribute(0x2a);
            }
        }

        public string Description
        {
            get
            {
                return base.QueryAttribute(0x7dd, 0x40);
            }
        }

        public int DistributionLists
        {
            get
            {
                return base.QueryAttribute(0x22);
            }
        }

        public string DynamicQueueName
        {
            get
            {
                return this.dynamicQueueName;
            }
        }

        public int HardenGetBackout
        {
            get
            {
                return base.QueryAttribute(8);
            }
        }

        public int IndexType
        {
            get
            {
                return base.QueryAttribute(0x39);
            }
        }

        public int InhibitGet
        {
            get
            {
                return base.QueryAttribute(9);
            }
            set
            {
                base.SetAttribute(9, value);
            }
        }

        public int InhibitPut
        {
            get
            {
                return base.QueryAttribute(10);
            }
            set
            {
                base.SetAttribute(10, value);
            }
        }

        public string InitiationQueueName
        {
            get
            {
                return base.QueryAttribute(0x7d8, 0x30);
            }
        }

        public int MaximumDepth
        {
            get
            {
                return base.QueryAttribute(15);
            }
        }

        public int MaximumMessageLength
        {
            get
            {
                return base.QueryAttribute(13);
            }
        }

        public int MessageDeliverySequence
        {
            get
            {
                return base.QueryAttribute(0x10);
            }
        }

        public int NonPersistentMessageClass
        {
            get
            {
                return base.QueryAttribute(0x4e);
            }
        }

        public int OpenInputCount
        {
            get
            {
                return base.QueryAttribute(0x11);
            }
        }

        public int OpenOutputCount
        {
            get
            {
                return base.QueryAttribute(0x12);
            }
        }

        public string ProcessName
        {
            get
            {
                return base.QueryAttribute(0x7dc, 0x30);
            }
        }

        public int QueueAccounting
        {
            get
            {
                return base.QueryAttribute(0x86);
            }
        }

        public int QueueMonitoring
        {
            get
            {
                return base.QueryAttribute(0x7b);
            }
        }

        public int QueueStatistics
        {
            get
            {
                return base.QueryAttribute(0x80);
            }
        }

        public int QueueType
        {
            get
            {
                return base.QueryAttribute(20);
            }
        }

        public string RemoteQueueManagerName
        {
            get
            {
                return base.QueryAttribute(0x7e1, 0x30);
            }
        }

        public string RemoteQueueName
        {
            get
            {
                return base.QueryAttribute(0x7e2, 0x30);
            }
        }

        public int RetentionInterval
        {
            get
            {
                return base.QueryAttribute(0x15);
            }
        }

        public int Scope
        {
            get
            {
                return base.QueryAttribute(0x2d);
            }
        }

        public int ServiceInterval
        {
            get
            {
                return base.QueryAttribute(0x36);
            }
        }

        public int ServiceIntervalEvent
        {
            get
            {
                return base.QueryAttribute(0x2e);
            }
        }

        public int Shareability
        {
            get
            {
                return base.QueryAttribute(0x17);
            }
        }

        public string StorageClass
        {
            get
            {
                return base.QueryAttribute(0x7e6, 8);
            }
        }

        public string TPIPE
        {
            get
            {
                return base.QueryAttribute(0x825, 8);
            }
        }

        public string TransmissionQueueName
        {
            get
            {
                return base.QueryAttribute(0x7e8, 0x30);
            }
        }

        public int TriggerControl
        {
            get
            {
                return base.QueryAttribute(0x18);
            }
            set
            {
                base.SetAttribute(0x18, value);
            }
        }

        public string TriggerData
        {
            get
            {
                return base.QueryAttribute(0x7e7, 0x40);
            }
            set
            {
                base.SetAttribute(0x7e7, value, 0x40);
            }
        }

        public int TriggerDepth
        {
            get
            {
                return base.QueryAttribute(0x1d);
            }
            set
            {
                base.SetAttribute(0x1d, value);
            }
        }

        public int TriggerMessagePriority
        {
            get
            {
                return base.QueryAttribute(0x1a);
            }
            set
            {
                base.SetAttribute(0x1a, value);
            }
        }

        public int TriggerType
        {
            get
            {
                return base.QueryAttribute(0x1c);
            }
            set
            {
                base.SetAttribute(0x1c, value);
            }
        }

        public int Usage
        {
            get
            {
                return base.QueryAttribute(12);
            }
        }
    }
}

