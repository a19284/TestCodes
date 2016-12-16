namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Collections;
    using System.Transactions;

    internal class MQXARecoveryBridge : NmqiObject, IDisposable
    {
        private string hostAndUser;
        private MQMarshalMessageForPut marshal;
        private MQGetMessageOptions recoveryGetGmo;
        private ManagedHconn recoveryHconn;
        private Phobj recoveryHobj;
        private Hashtable recoveryProperties;
        private MQPutMessageOptions recoveryPutPmo;
        private int recoveryQueueOpenOpts;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private Hashtable TransactionLogList;

        public MQXARecoveryBridge(ManagedHconn hconn, NmqiEnvironment env) : base(env)
        {
            this.recoveryProperties = new Hashtable(6);
            this.TransactionLogList = new Hashtable(10);
            this.recoveryQueueOpenOpts = 0x12;
            uint method = 0x529;
            this.TrEntry(method, new object[] { hconn, env });
            try
            {
                base.TrConstructor("%Z% %W%  %I% %E% %U%");
                this.recoveryHconn = hconn;
                this.recoveryPutPmo = new MQPutMessageOptions();
                this.recoveryPutPmo.Options = 0xc0;
                this.recoveryGetGmo = new MQGetMessageOptions();
                this.recoveryGetGmo.MatchOptions = 1;
                this.recoveryGetGmo.Options = 1;
                this.recoveryGetGmo.WaitInterval = 0x3e8;
                this.InitializeTransactionRecovery();
            }
            catch (MQException exception)
            {
                base.TrException(method, exception, 1);
                throw exception;
            }
            catch (Exception exception2)
            {
                base.TrException(method, exception2, 2);
                NmqiException exception3 = new NmqiException(env, -1, null, 2, 0x893, exception2);
                throw exception3;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public void Dispose(bool fromDispose)
        {
            int compCode = 0;
            int reason = 0;
            if (((this.recoveryHobj != null) && (this.recoveryHobj.HOBJ != null)) && (this.recoveryHobj.HOBJ.Handle != -1))
            {
                base.TrText("Hobj = " + this.recoveryHobj.HOBJ.Handle);
                MQProxyQueue proxyQueue = ((ManagedHobj) this.recoveryHobj.HOBJ).ProxyQueue;
                if (proxyQueue != null)
                {
                    proxyQueue.ProxyMQCLOSE(0, ref compCode, ref reason);
                }
                else
                {
                    this.recoveryHconn.GetMQFAP.MQCLOSE(this.recoveryHconn, this.recoveryHobj, 0, out compCode, out reason);
                }
            }
            this.recoveryHconn = null;
            this.recoveryHobj = null;
            this.TransactionLogList = null;
            this.recoveryProperties = null;
            if (!fromDispose)
            {
                GC.SuppressFinalize(this);
            }
        }

        private void InitializeTransactionRecovery()
        {
            uint method = 0x52a;
            this.TrEntry(method);
            try
            {
                if (this.recoveryHconn.XARecoveryBridge == null)
                {
                    MQObjectDescriptor mqod = new MQObjectDescriptor();
                    mqod.ObjectType = 1;
                    mqod.ObjectName = "SYSTEM.DOTNET.XARECOVERY.QUEUE";
                    if (this.recoveryHconn.Name != null)
                    {
                        mqod.ObjectQMgrName = this.recoveryHconn.Name;
                    }
                    int compCode = 0;
                    int reason = 0;
                    this.recoveryHobj = new Phobj(base.env);
                    this.recoveryHconn.Session.MQFap.MQOPEN(this.recoveryHconn, ref mqod, this.recoveryQueueOpenOpts, ref this.recoveryHobj, out compCode, out reason, null);
                    if (compCode != 0)
                    {
                        base.throwNewMQException(compCode, reason);
                    }
                    this.recoveryHconn.XARecoveryBridge = this;
                    this.hostAndUser = Environment.MachineName + ", " + Environment.UserName;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void LogCurrentTransactionEnd(MQXid xid, int rmid)
        {
            uint method = 0x52c;
            this.TrEntry(method, new object[] { xid, rmid });
            MQMessageDescriptor msgDesc = new MQMessageDescriptor();
            msgDesc.Version = 2;
            try
            {
                if (this.TransactionLogList.ContainsKey(xid))
                {
                    MQMessageDescriptor descriptor2 = (MQMessageDescriptor) this.TransactionLogList[xid];
                    msgDesc.MsgId = descriptor2.MsgId;
                    base.TrText(method, "MessageId-" + NmqiTools.ArrayToHexString(msgDesc.MsgId));
                    int dataLength = 0;
                    int bufferLength = 0x1000;
                    byte[] buffer = new byte[bufferLength];
                    int compCode = 0;
                    int reason = 0;
                    MQProxyQueue proxyQueue = ((ManagedHobj) this.recoveryHobj.HOBJ).ProxyQueue;
                    if (proxyQueue != null)
                    {
                        proxyQueue.ProxyMQGET(msgDesc, this.recoveryGetGmo, bufferLength, buffer, ref dataLength, null, ref compCode, ref reason);
                    }
                    else
                    {
                        this.recoveryHconn.GetMQFAP.MQGET(this.recoveryHconn, this.recoveryHobj.HOBJ, msgDesc, this.recoveryGetGmo, bufferLength, buffer, out dataLength, out compCode, out reason);
                    }
                    if (compCode != 0)
                    {
                        base.throwNewMQException(compCode, reason);
                    }
                    this.TransactionLogList.Remove(xid);
                    buffer = null;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void LogCurrentTransactionStart(byte[] xidBytes, MQXid xid, Guid queueManagerUid, int rmid, byte[] recinfo)
        {
            uint method = 0x52b;
            this.TrEntry(method, new object[] { xidBytes, queueManagerUid, rmid, recinfo });
            try
            {
                MQMessage mqMsg = new MQMessage();
                mqMsg.Persistence = 1;
                int compCode = 0;
                int reason = 0;
                sbyte[] dst = new sbyte[queueManagerUid.ToByteArray().Length];
                Buffer.BlockCopy(queueManagerUid.ToByteArray(), 0, dst, 0, dst.Length);
                base.TrText(method, "Qmid=" + NmqiTools.ArrayToHexString(queueManagerUid.ToByteArray()));
                sbyte[] numArray2 = new sbyte[recinfo.Length];
                Buffer.BlockCopy(recinfo, 0, numArray2, 0, numArray2.Length);
                base.TrText(method, "recordinfo=" + NmqiTools.ArrayToHexString(recinfo));
                sbyte[] numArray3 = new sbyte[xidBytes.Length];
                Buffer.BlockCopy(xidBytes, 0, numArray3, 0, numArray3.Length);
                base.TrText(method, "recordinfo=" + NmqiTools.ArrayToHexString(xidBytes));
                mqMsg.SetBytesProperty("dnet.XARECOVERY_QMID", dst);
                mqMsg.SetBytesProperty("dnet.XARECOVERY_RECINFO", numArray2);
                mqMsg.SetBytesProperty("dnet.XARECOVERY_XID", numArray3);
                mqMsg.SetIntProperty("dnet.XARECOVERY_RMID", rmid);
                mqMsg.SetDoubleProperty("dnet.XARECOVERY_TTIMEOUT", TransactionManager.MaximumTimeout.TotalMilliseconds);
                mqMsg.SetStringProperty("dnet.XARECOVERY_HOSTANDUSER", this.hostAndUser);
                this.marshal = new MQMarshalMessageForPut(mqMsg);
                mqMsg = this.marshal.ConstructMessageForSend();
                byte[] sBuff = mqMsg.GetBuffer();
                MQMessageDescriptor md = mqMsg.md;
                this.recoveryHconn.Session.MQFap.MQPUT(this.recoveryHconn, this.recoveryHobj.HOBJ, ref md, ref this.recoveryPutPmo, sBuff.Length, sBuff, null, 1, out compCode, out reason);
                base.TrText(method, "MessageId-" + NmqiTools.ArrayToHexString(md.MsgId));
                if (compCode != 0)
                {
                    base.throwNewMQException(compCode, reason);
                }
                this.TransactionLogList.Add(xid, mqMsg.md);
                this.marshal.Dispose(false);
                this.marshal = null;
                mqMsg = null;
                sBuff = null;
            }
            finally
            {
                base.TrExit(method);
            }
        }
    }
}

