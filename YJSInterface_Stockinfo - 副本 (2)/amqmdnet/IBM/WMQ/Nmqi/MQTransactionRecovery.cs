namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Transactions;

    public class MQTransactionRecovery : NmqiObject
    {
        private string channel_;
        private string ChannelName;
        private static List<Guid> CompletedQMId = new List<Guid>();
        private string ConnectionName;
        private string connname_;
        private MQGetMessageOptions gmoB;
        private MQGetMessageOptions gmoG;
        internal static MQMonitorLogWritter log = null;
        private MQMessageDescriptor mqmd;
        internal static NmqiEnvironment nmqiEnv = null;
        private int openOpts;
        private Hashtable properties;
        private MQQueueManager Qmgr;
        private string qmgrName_;
        private Guid QMId;
        private MQQueue Queue;
        private string QueueManagerName;
        private bool reenter;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private string secExit;
        private string secudata;
        private string SecurityExit;
        private string SecurityExitUserData;
        private static StringBuilder tmAddrAsString = new StringBuilder();
        private string userId;
        private string UserId;
        internal static bool userMode = false;
        private string UserMode;

        static MQTransactionRecovery()
        {
            nmqiEnv = NmqiFactory.GetInstance(null);
            log = new MQMonitorLogWritter(nmqiEnv);
            log.WriteLog("WMQ .NET XA Recovery Monitor has started running..");
            log.WriteLog(tmAddrAsString.ToString());
        }

        public MQTransactionRecovery() : base(nmqiEnv)
        {
            this.properties = new Hashtable(6);
            this.ConnectionName = "connectionName";
            this.ChannelName = "channelName";
            this.QueueManagerName = "queueManagerName";
            this.SecurityExit = "securityExit";
            this.UserId = "userId";
            this.SecurityExitUserData = "securityExitUserData";
            this.UserMode = "UserMode";
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public void InitializeForRecovery(Hashtable properties)
        {
            uint method = 0x51f;
            this.TrEntry(method, new object[] { properties });
            try
            {
                this.properties = properties;
                if (properties.ContainsKey(this.ConnectionName))
                {
                    this.connname_ = (string) properties["connectionName"];
                }
                if (properties.ContainsKey(this.ChannelName))
                {
                    this.channel_ = (string) properties["channelName"];
                }
                if (properties.ContainsKey(this.QueueManagerName))
                {
                    this.qmgrName_ = (string) properties["queueManagerName"];
                }
                if (properties.ContainsKey(this.SecurityExit))
                {
                    this.secExit = (string) properties["securityExit"];
                }
                if (properties.ContainsKey(this.SecurityExitUserData))
                {
                    this.secudata = (string) properties["securityExitUserData"];
                }
                if (properties.ContainsKey(this.UserId))
                {
                    this.userId = (string) properties["userId"];
                }
                if (properties.ContainsKey(this.UserMode))
                {
                    userMode = (bool) properties["UserMode"];
                }
                this.gmoB = new MQGetMessageOptions();
                this.gmoG = new MQGetMessageOptions();
                this.gmoB.Options = 0x2021;
                this.gmoG.Options = 3;
                this.gmoG.MatchOptions = 1;
                this.gmoG.WaitInterval = 0x7d0;
                this.gmoB.WaitInterval = 0x7d0;
                this.mqmd = new MQMessageDescriptor();
                this.openOpts = 0x202a;
                properties.Clear();
                this.properties.Add("channel", this.channel_);
                this.properties.Add("connectionName", this.connname_);
                this.properties.Add("transport", "MQSeries Managed Client");
                if (this.secExit != null)
                {
                    properties.Add("securityExit", this.secExit);
                }
                if (this.userId != null)
                {
                    properties.Add("userID", this.userId);
                }
                if (this.secudata != null)
                {
                    properties.Add("securityExitUserData", this.secudata);
                }
                this.InitiateForRecovery();
            }
            finally
            {
                log.WriteLog("WMQ .NET XA Recovery Monitor completed recovering transactions");
                base.TrExit(method);
            }
        }

        internal void InitiateForRecovery()
        {
            uint method = 0x520;
            this.TrEntry(method);
            try
            {
                this.Qmgr = new MQQueueManager(this.qmgrName_, this.properties);
                log.WriteLog("QMgr Connection Details: ", this.connname_ + ";" + this.channel_);
                if ((this.Qmgr != null) && this.Qmgr.IsConnected)
                {
                    this.Queue = this.Qmgr.AccessQueue("SYSTEM.DOTNET.XARECOVERY.QUEUE", this.openOpts);
                    this.RunRecovery();
                    if (this.reenter)
                    {
                        this.RunRecovery();
                        this.reenter = false;
                    }
                    this.Qmgr.Disconnect();
                }
            }
            catch (MQException exception)
            {
                base.TrException(method, exception);
                throw exception;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal bool RecoverTransaction(Guid QMID, sbyte[] recinfo, sbyte[] xid, int rmid)
        {
            bool success;
            uint method = 0x522;
            this.TrEntry(method, new object[] { QMID, recinfo, xid, rmid });
            MQQueueManager manager = null;
            ManagedHconn manHconn = null;
            try
            {
                try
                {
                    manager = new MQQueueManager(this.qmgrName_, this.properties);
                    if (((manager == null) || (manager.hConn == null)) || (manager.hConn.Value <= 0))
                    {
                        base.throwNewMQException(2, 0x80b);
                    }
                    manHconn = (ManagedHconn) manager.hConn;
                    if (((manHconn == null) || (manHconn.Name == null)) || (manHconn.Value <= 0))
                    {
                        base.throwNewMQException(2, 0x80b);
                    }
                }
                catch (NullReferenceException)
                {
                    base.throwNewMQException(2, 0x80b);
                }
                catch (Exception exception)
                {
                    base.TrException(method, exception, 1);
                    throw exception;
                }
                log.WriteLog("ReEnlisting now with Transaction Manager to complete transaction");
                MQRecoveryEnlistment enlistmentNotification = new MQRecoveryEnlistment(manHconn, nmqiEnv, xid, rmid);
                byte[] dst = new byte[recinfo.Length];
                Buffer.BlockCopy(recinfo, 0, dst, 0, dst.Length);
                try
                {
                    TransactionManager.Reenlist(QMID, dst, enlistmentNotification);
                    success = enlistmentNotification.Success;
                }
                catch (ArgumentException)
                {
                    log.WriteLog("Current messages's recoveryinformation doesn't belong to local Transaction co-ordinator. Skipping it");
                    success = false;
                }
            }
            finally
            {
                if ((manager != null) && manager.IsConnected)
                {
                    manager.Disconnect();
                }
                base.TrExit(method);
            }
            return success;
        }

        internal void RunRecovery()
        {
            uint method = 0x521;
            this.TrEntry(method);
            int num2 = 0;
            bool flag = false;
            while ((num2 = this.Queue.CurrentDepth) > 0)
            {
                flag = true;
                log.WriteLog("SYSTEM.DOTNET.XARECOVERY.QUEUE Current Depth : ", num2.ToString());
                MQMessage message = new MQMessage();
                try
                {
                    this.Queue.Get(message, this.gmoB);
                }
                catch (MQException exception)
                {
                    base.TrException(method, exception, 1);
                    if (exception.Reason == 0x7f1)
                    {
                        break;
                    }
                }
                log.WriteLog("MsgID of current message(recovery message) being processed ", NmqiTools.ArrayToHexString(message.MessageId));
                if (message == null)
                {
                    break;
                }
                sbyte[] xid = null;
                double doubleProperty = 0.0;
                string stringProperty = null;
                try
                {
                    xid = message.GetBytesProperty("dnet.XARECOVERY_XID");
                    doubleProperty = message.GetDoubleProperty("dnet.XARECOVERY_TTIMEOUT");
                    stringProperty = message.GetStringProperty("dnet.XARECOVERY_HOSTANDUSER");
                }
                catch (MQException exception2)
                {
                    base.TrException(method, exception2, 2);
                    if ((((exception2.Reason != 0x9a7) && (exception2.Reason != 0x98a)) && ((exception2.Reason != 0x9a2) && (exception2.Reason != 0x9a3))) && ((exception2.Reason != 0x9a6) && (exception2.Reason != 0x9a5)))
                    {
                        throw exception2;
                    }
                    continue;
                }
                log.WriteLog("Host,UserId under which the Transaction Recovery message has been put : " + stringProperty);
                log.WriteLog("Current Xid from the indoubt message");
                log.WriteXid(xid);
                DateTime putDateTime = message.PutDateTime;
                DateTime time2 = putDateTime.AddMilliseconds(doubleProperty);
                if (time2 > DateTime.Now)
                {
                    log.WriteLog("TransactionTimeout - " + time2.ToLongTimeString());
                    log.WriteLog("Message arrival time - " + putDateTime.ToLongTimeString());
                    log.WriteLog("Monitor skiping the current messages as transaction did not timeout yet..");
                }
                else
                {
                    try
                    {
                        this.Queue.Get(message, this.gmoG);
                        log.WriteLog("Found one Inomplete Transaction from message,(ID):", NmqiTools.ArrayToHexString(message.MessageId));
                    }
                    catch (MQException exception3)
                    {
                        base.TrException(method, exception3, 2);
                        throw exception3;
                    }
                    try
                    {
                        int intProperty = message.GetIntProperty("dnet.XARECOVERY_RMID");
                        sbyte[] bytesProperty = message.GetBytesProperty("dnet.XARECOVERY_QMID");
                        sbyte[] recinfo = message.GetBytesProperty("dnet.XARECOVERY_RECINFO");
                        sbyte[] numArray4 = message.GetBytesProperty("dnet.XARECOVERY_XID");
                        byte[] dst = new byte[bytesProperty.Length];
                        Buffer.BlockCopy(bytesProperty, 0, dst, 0, dst.Length);
                        this.QMId = new Guid(dst);
                        if (this.RecoverTransaction(this.QMId, recinfo, numArray4, intProperty))
                        {
                            this.Qmgr.Commit();
                            if (!CompletedQMId.Contains(this.QMId))
                            {
                                CompletedQMId.Add(this.QMId);
                            }
                        }
                        else
                        {
                            if (CompletedQMId.Contains(this.QMId))
                            {
                                CompletedQMId.Remove(this.QMId);
                            }
                            this.Qmgr.Backout();
                        }
                        continue;
                    }
                    catch (MQException exception4)
                    {
                        base.TrException(method, exception4, 3);
                        throw exception4;
                    }
                }
            }
            foreach (Guid guid in CompletedQMId)
            {
                TransactionManager.RecoveryComplete(guid);
            }
            if (!flag)
            {
                Thread.Sleep(0x1d4c0);
                this.reenter = true;
            }
            base.TrExit(method);
        }
    }
}

