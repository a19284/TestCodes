namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Globalization;
    using System.Transactions;

    public abstract class MQDestination : MQManagedObject, IDisposable
    {
        protected static int defaultMaxMsgSize = 0x1000;
        private int destinationType;
        private bool disposed;
        private int propControl;
        private const string sccsid = "@(#) lib/dotnet/pc/winnt/baseclasses/MQDestination.cs, dotnet, p000  1.53 11/04/18 05:57:37";
        protected byte[] tempMsgBuffer;

        protected MQDestination(int destType)
        {
            this.propControl = -1;
            base.TrConstructor("@(#) lib/dotnet/pc/winnt/baseclasses/MQDestination.cs, dotnet, p000  1.53 11/04/18 05:57:37", new object[] { destType });
            this.destinationType = destType;
        }

        protected MQDestination(int destType, MQQueueManager qMgr, string objectName, int options, string altUserId) : this(destType)
        {
            base.TrConstructor("@(#) lib/dotnet/pc/winnt/baseclasses/MQDestination.cs, dotnet, p000  1.53 11/04/18 05:57:37", new object[] { destType, qMgr, objectName, options, altUserId });
            if (qMgr == null)
            {
                base.throwNewMQException(2, 0x7e2);
            }
            if (!qMgr.IsConnected)
            {
                base.throwNewMQException(2, 0x7e2);
            }
            base.qMgr = qMgr;
            base.objectType = destType;
            base.OpenOptions = options;
            if (objectName != null)
            {
                base.objectName = objectName;
            }
            if (altUserId != null)
            {
                base.AlternateUserId = altUserId;
            }
        }

        public override void Close()
        {
            uint method = 0x77;
            this.TrEntry(method);
            int pCompCode = 0;
            int pReason = 0;
            try
            {
                if (base.qMgr != null)
                {
                    base.Close();
                    if (base.qMgr.IsConnected && base.IsOpen)
                    {
                        base.qMgr.nmqiConnector.MQCLOSE(base.qMgr.hConn, base.objectHandle, base.CloseOptions, out pCompCode, out pReason);
                        base.objectHandle = null;
                        base.objectName = "";
                        if (pCompCode != 0)
                        {
                            base.qMgr.CheckHConnHealth(pReason);
                            base.throwNewMQException(pCompCode, pReason);
                        }
                        if (!this.disposed)
                        {
                            GC.SuppressFinalize(this);
                            this.disposed = true;
                        }
                    }
                }
            }
            catch (MQException exception)
            {
                pCompCode = exception.CompCode;
                pReason = exception.Reason;
                throw exception;
            }
            finally
            {
                base.unsafe_compCode = pCompCode;
                base.unsafe_reason = pReason;
                base.TrExit(method);
            }
        }

        internal void CopyMQMD(MQBase.MQMD sourceMqmd, ref MQBase.MQMD targetMqmd)
        {
            uint method = 0x7b;
            this.TrEntry(method, new object[] { sourceMqmd, (MQBase.MQMD) targetMqmd });
            try
            {
                targetMqmd.StrucId = (byte[]) sourceMqmd.StrucId.Clone();
                targetMqmd.Version = sourceMqmd.Version;
                targetMqmd.Report = sourceMqmd.Report;
                targetMqmd.MsgType = sourceMqmd.MsgType;
                targetMqmd.Expiry = sourceMqmd.Expiry;
                targetMqmd.Feedback = sourceMqmd.Feedback;
                targetMqmd.Encoding = sourceMqmd.Encoding;
                targetMqmd.CodedCharacterSetId = sourceMqmd.CodedCharacterSetId;
                targetMqmd.Format = (byte[]) sourceMqmd.Format.Clone();
                targetMqmd.Priority = sourceMqmd.Priority;
                targetMqmd.Persistence = sourceMqmd.Persistence;
                targetMqmd.MsgId = (byte[]) sourceMqmd.MsgId.Clone();
                targetMqmd.CorrelId = (byte[]) sourceMqmd.CorrelId.Clone();
                targetMqmd.BackoutCount = sourceMqmd.BackoutCount;
                targetMqmd.ReplyToQ = (byte[]) sourceMqmd.ReplyToQ.Clone();
                targetMqmd.ReplyToQMgr = (byte[]) sourceMqmd.ReplyToQMgr.Clone();
                targetMqmd.UserId = (byte[]) sourceMqmd.UserId.Clone();
                targetMqmd.AccountingToken = (byte[]) sourceMqmd.AccountingToken.Clone();
                targetMqmd.ApplIdentityData = (byte[]) sourceMqmd.ApplIdentityData.Clone();
                targetMqmd.PutApplType = sourceMqmd.PutApplType;
                targetMqmd.PutApplName = (byte[]) sourceMqmd.PutApplName.Clone();
                targetMqmd.PutDate = (byte[]) sourceMqmd.PutDate.Clone();
                targetMqmd.PutTime = (byte[]) sourceMqmd.PutTime.Clone();
                targetMqmd.ApplOriginData = (byte[]) sourceMqmd.ApplOriginData.Clone();
                targetMqmd.GroupId = (byte[]) sourceMqmd.GroupId.Clone();
                targetMqmd.MsgSequenceNumber = sourceMqmd.MsgSequenceNumber;
                targetMqmd.Offset = sourceMqmd.Offset;
                targetMqmd.MsgFlags = sourceMqmd.MsgFlags;
                targetMqmd.OriginalLength = sourceMqmd.OriginalLength;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        protected MQObjectDescriptor CreateMQObjectDescriptor()
        {
            uint method = 0x74;
            this.TrEntry(method);
            MQObjectDescriptor descriptor = new MQObjectDescriptor();
            try
            {
                descriptor.ObjectType = this.destinationType;
                descriptor.ObjectName = base.objectName;
                if (base.AlternateUserId != null)
                {
                    descriptor.AlternateUserId = base.AlternateUserId;
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return descriptor;
        }

        internal void Dispose(bool disposing)
        {
            uint method = 0x73;
            this.TrEntry(method, new object[] { disposing });
            try
            {
                this.Close();
            }
            catch (Exception exception)
            {
                base.TrException(method, exception);
            }
            finally
            {
                if (disposing && !this.disposed)
                {
                    GC.SuppressFinalize(this);
                    this.disposed = true;
                }
                base.TrExit(method);
            }
        }

        ~MQDestination()
        {
            this.Dispose(false);
        }

        public void Get(MQMessage message)
        {
            uint method = 120;
            this.TrEntry(method, new object[] { message });
            try
            {
                this.Get(message, new MQGetMessageOptions());
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Get(MQMessage message, MQGetMessageOptions gmo)
        {
            uint method = 0x79;
            this.TrEntry(method, new object[] { message, gmo });
            try
            {
                this.Get(message, gmo, 0);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Get(MQMessage message, MQGetMessageOptions gmo, int maxMsgSize)
        {
            uint method = 0x7a;
            this.TrEntry(method, new object[] { message, gmo, maxMsgSize });
            int dataLength = 0;
            byte[] buffer = null;
            int options = -1;
            bool flag = true;
            int compCode = 0;
            int reason = 0;
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
                if (maxMsgSize < 0)
                {
                    base.throwNewMQException(2, 0x7d5);
                }
                options = gmo.Options;
                if ((gmo.Options & 0x1006) == 0)
                {
                    base.TrText(method, "Setting explicit NO_SYNCPOINT");
                    gmo.Options |= 4;
                }
                if (base.qMgr.CommandLevel >= 700)
                {
                    flag = (options & 0x1e000000) == 0;
                    if (((options & 0x8000000) != 0) || flag)
                    {
                        gmo.Options &= -134217729;
                        gmo.Options |= 0x2000000;
                    }
                }
                if ((Transaction.Current != null) && !base.qMgr.IsXAEnabled)
                {
                    base.qMgr.IsXAEnabled = true;
                }
                int characterSet = message.CharacterSet;
                if (!base.qMgr.IsHconnValid)
                {
                    base.throwNewMQException(2, 0x7e2);
                }
                message.ClearMessage();
                if (maxMsgSize == 0)
                {
                    int num7 = gmo.Options;
                    MQBase.MQMD targetMqmd = new MQBase.MQMD();
                    this.CopyMQMD(message.md.StructMQMD, ref targetMqmd);
                    maxMsgSize = defaultMaxMsgSize;
                    buffer = new byte[maxMsgSize];
                    gmo.Options &= -65;
                    if ((num7 & 0x4000) == 0x4000)
                    {
                        MQLPIGetOpts lpiGetOpts = new MQLPIGetOpts();
                        lpiGetOpts.SetOptions(lpiGetOpts.GetOptions() | MQLPIGetOpts.lpiGETOPT_FULL_MESSAGE);
                        ((NmqiSP) base.qMgr.nmqiConnector).zstMQGET(base.qMgr.hConn, base.objectHandle.HOBJ, ref message.md, ref gmo, maxMsgSize, buffer, out dataLength, lpiGetOpts, out compCode, out reason);
                    }
                    else
                    {
                        base.qMgr.nmqiConnector.MQGET(base.qMgr.hConn, base.hObj, message.md, gmo, maxMsgSize, buffer, out dataLength, out compCode, out reason);
                    }
                    if (0x7da == reason)
                    {
                        maxMsgSize = dataLength;
                        buffer = new byte[maxMsgSize];
                        base.qMgr.nmqiConnector.MQGET(base.qMgr.hConn, base.hObj, message.md, gmo, maxMsgSize, buffer, out dataLength, out compCode, out reason);
                    }
                    while ((compCode != 0) && (0x820 == reason))
                    {
                        gmo.Options = num7;
                        MQBase.MQMD structMQMD = message.md.StructMQMD;
                        this.CopyMQMD(targetMqmd, ref structMQMD);
                        message.md.StructMQMD = structMQMD;
                        maxMsgSize = dataLength;
                        buffer = new byte[maxMsgSize];
                        gmo.Options &= -65;
                        base.qMgr.nmqiConnector.MQGET(base.qMgr.hConn, base.hObj, message.md, gmo, maxMsgSize, buffer, out dataLength, out compCode, out reason);
                    }
                    if ((0x848 == reason) || (0x88e == reason))
                    {
                        string objectId = "Server Binding convert message";
                        byte[] outString = null;
                        int outLength = 0;
                        uint bytesConverted = 0;
                        if (CommonServices.ConvertString(objectId, message.md.StructMQMD.CodedCharacterSetId, characterSet, buffer, dataLength, out outString, ref outLength, 0, out bytesConverted) == 0)
                        {
                            buffer = outString;
                            maxMsgSize = outLength;
                            dataLength = outLength;
                            compCode = 0;
                            reason = 0;
                        }
                    }
                }
                else
                {
                    buffer = new byte[maxMsgSize];
                    base.qMgr.nmqiConnector.MQGET(base.qMgr.hConn, base.hObj, message.md, gmo, maxMsgSize, buffer, out dataLength, out compCode, out reason);
                }
                byte[] b = buffer;
                if (compCode == 0)
                {
                    bool flag2 = false;
                    if (base.qMgr.CommandLevel >= 700)
                    {
                        if (flag)
                        {
                            if (this.propControl != 3)
                            {
                                flag2 = true;
                            }
                        }
                        else if ((options & 0x8000000) != 0)
                        {
                            flag2 = true;
                        }
                        if (flag2 && (dataLength > 0x24))
                        {
                            b = this.PerformMsgProcessingAfterGet(ref message, buffer, (dataLength > buffer.Length) ? buffer.Length : dataLength);
                            dataLength = b.Length;
                        }
                    }
                }
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
            catch (Exception exception2)
            {
                base.TrException(method, exception2);
                compCode = 2;
                reason = 0x893;
                throw exception2;
            }
            finally
            {
                base.unsafe_compCode = compCode;
                base.unsafe_reason = reason;
                gmo.Options = options;
                base.TrExit(method);
            }
        }

        protected void Open(ref MQObjectDescriptor od)
        {
            uint method = 0x75;
            this.TrEntry(method, new object[] { od });
            int pCompCode = 0;
            int pReason = 0;
            try
            {
                od.CopyCHARVIntoOD();
                if (base.qMgr.CommandLevel < 700)
                {
                    int openOptions = base.OpenOptions;
                    if (base.objectHandle == null)
                    {
                        base.objectHandle = MQQueueManager.nmqiEnv.NewPhobj();
                    }
                    base.qMgr.nmqiConnector.MQOPEN(base.qMgr.hConn, ref od, openOptions, base.objectHandle, out pCompCode, out pReason);
                    base.hObj = base.objectHandle.HOBJ;
                }
                else
                {
                    SpiOpenOptions options = new SpiOpenOptions();
                    options.Options = base.OpenOptions;
                    NmqiSP nmqiConnector = (NmqiSP) base.qMgr.nmqiConnector;
                    if (base.objectHandle == null)
                    {
                        base.objectHandle = MQQueueManager.nmqiEnv.NewPhobj();
                    }
                    nmqiConnector.SpiOpen(base.qMgr.hConn, ref od, ref options, base.objectHandle, out pCompCode, out pReason);
                    base.hObj = base.objectHandle.HOBJ;
                    if (pCompCode != 2)
                    {
                        this.propControl = options.PropertyControl;
                    }
                }
                if (pCompCode != 0)
                {
                    base.qMgr.CheckHConnHealth(pReason);
                    base.throwNewMQException(pCompCode, pReason);
                }
                base.isClosed = false;
            }
            catch (MQException exception)
            {
                pCompCode = exception.CompCode;
                pReason = exception.Reason;
                throw exception;
            }
            finally
            {
                base.unsafe_compCode = pCompCode;
                base.unsafe_reason = pReason;
                base.TrExit(method);
            }
        }

        internal void PerformMsgProcessgingBeforePut(ref MQMessage mqMsg)
        {
            uint method = 0x7e;
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
            uint method = 0x7f;
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
            uint method = 0x80;
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

        public void Put(MQMessage message)
        {
            uint method = 0x7c;
            this.TrEntry(method, new object[] { message });
            try
            {
                this.Put(message, new MQPutMessageOptions());
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Put(MQMessage message, MQPutMessageOptions pmo)
        {
            uint method = 0x7d;
            this.TrEntry(method, new object[] { message, pmo });
            int options = 0;
            int pCompCode = 0;
            int pReason = 0;
            try
            {
                if (message == null)
                {
                    base.throwNewMQException(2, 0x7ea);
                }
                if (pmo == null)
                {
                    base.throwNewMQException(2, 0x87d);
                }
                pmo.ValidateOptions();
                options = pmo.Options;
                if ((pmo.Options & 6) == 0)
                {
                    base.TrText(method, "Setting explicit NO_SYNCPOINT");
                    pmo.Options |= 4;
                }
                if ((Transaction.Current != null) && !base.qMgr.IsXAEnabled)
                {
                    base.qMgr.IsXAEnabled = true;
                }
                byte[] src = message.GetBuffer();
                this.tempMsgBuffer = new byte[src.Length];
                Buffer.BlockCopy(src, 0, this.tempMsgBuffer, 0, src.Length);
                int characterSet = message.CharacterSet;
                int encoding = message.Encoding;
                string format = message.Format;
                this.PerformMsgProcessgingBeforePut(ref message);
                src = message.GetBuffer();
                if (!base.qMgr.IsHconnValid)
                {
                    base.throwNewMQException(2, 0x7e2);
                }
                base.qMgr.nmqiConnector.MQPUT(base.qMgr.hConn, base.hObj, message.md, pmo, src.Length, src, out pCompCode, out pReason);
                if (pCompCode == 0)
                {
                    this.PerformMsgProcessingAfterPut(ref message, this.tempMsgBuffer, characterSet, encoding, format);
                }
                if (pCompCode != 0)
                {
                    base.qMgr.CheckHConnHealth(pReason);
                    base.throwNewMQException(pCompCode, pReason);
                }
            }
            catch (MQException exception)
            {
                pCompCode = exception.CompCode;
                pReason = exception.Reason;
                throw exception;
            }
            catch (Exception exception2)
            {
                base.TrException(method, exception2);
                pCompCode = 2;
                pReason = 0x893;
                throw exception2;
            }
            finally
            {
                pmo.Options = options;
                base.unsafe_compCode = pCompCode;
                base.unsafe_reason = pReason;
                base.TrExit(method);
            }
        }

        void IDisposable.Dispose()
        {
            uint method = 0x411;
            this.TrEntry(method);
            try
            {
                this.Dispose(true);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        protected void ValidateDestination()
        {
            uint method = 0x76;
            this.TrEntry(method);
            try
            {
                if (!base.IsOpen)
                {
                    base.throwNewMQException(2, 0x7e3);
                }
                if ((base.qMgr == null) || !base.qMgr.IsConnected)
                {
                    base.throwNewMQException(2, 0x7e2);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public DateTime CreationDateTime
        {
            get
            {
                string str = base.QueryAttribute(0x7d4, 12);
                string str2 = base.QueryAttribute(0x7d5, 8);
                return DateTime.ParseExact(str.TrimEnd(new char[0]) + str2, "yyyy-MM-ddHH.mm.ss", new CultureInfo("en-US"));
            }
        }

        public int DestinationType
        {
            get
            {
                return this.destinationType;
            }
        }
    }
}

