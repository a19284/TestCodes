namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Transactions;

    internal class NmqiXAResourceManager : NmqiObject
    {
        internal volatile Exception asyncFailureNotify;
        private string btrid;
        internal volatile bool dtcTxComplete;
        private int flags;
        private Guid gtrid;
        internal ManagedHconn hconn;
        protected Guid myGuid;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        protected double transTimeout;
        public const int XA_OK = 0;
        public const int XA_RDONLY = 3;
        internal MQXid xid;

        public NmqiXAResourceManager(NmqiEnvironment env, ManagedHconn hConn) : base(env)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { env, hConn });
            this.hconn = hConn;
            base.env = env;
            this.transTimeout = TransactionManager.MaximumTimeout.TotalMilliseconds;
            base.TrText("Transaction Timeout value = " + this.transTimeout);
        }

        internal void Current_TransactionCompleted(object sender, TransactionEventArgs e)
        {
            uint method = 0x50c;
            this.TrEntry(method, new object[] { sender, e });
            try
            {
                this.dtcTxComplete = true;
                if (this.hconn != null)
                {
                    this.hconn.PulseCommitWait();
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private string GetBranchQualifier()
        {
            uint method = 0x50d;
            this.TrEntry(method);
            StringBuilder builder = new StringBuilder();
            string result = "";
            if (Transaction.Current != null)
            {
                try
                {
                    string localIdentifier = Transaction.Current.TransactionInformation.LocalIdentifier;
                    if (localIdentifier != null)
                    {
                        builder.Append(localIdentifier);
                        builder.Append(":");
                        builder.Append(base.env.GetBranchQualifier);
                    }
                    else
                    {
                        builder.Append(":" + Process.GetCurrentProcess().Id);
                        builder.Append(":" + Thread.CurrentThread.ManagedThreadId);
                        builder.Append(":" + this.hconn.branchCount);
                        builder.Append(":" + base.env.GetBranchQualifier);
                    }
                }
                catch (Exception exception)
                {
                    base.TrException(method, exception);
                }
            }
            result = builder.ToString();
            base.TrExit(method, result);
            return result;
        }

        public void InitializeMQRMI()
        {
            uint method = 0x50b;
            this.TrEntry(method);
            try
            {
                this.xid = new MQXid();
                this.gtrid = this.myGuid;
                this.btrid = this.GetBranchQualifier();
                this.xid.SetXid(this.gtrid, this.btrid);
                this.xid.TraceFields();
                this.MQRMIXAStart();
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void MQRMIXACommit(bool onephase)
        {
            uint method = 0x513;
            this.TrEntry(method, new object[] { onephase });
            int flags = onephase ? 0x40000000 : 0;
            try
            {
                base.TrText(method, "Hconn value = " + this.hconn.Value + " issuing xa_commit");
                this.xid.TraceFields();
                int num3 = this.hconn.Session.MQFap.XA_Commit(this.hconn, this.xid, this.hconn.Rmid, flags);
                if ((num3 != 0) && (num3 != 3))
                {
                    TransactionException innerException = new TransactionException(NmqiTools.GetTranslatedExceptionMessage(this.ToString(), 0x20008385, (uint) num3));
                    MQException exception2 = new MQException(2, 0x893, innerException);
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = (uint) this.hconn.Rmid;
                    CommonServices.CommentInsert1 = "xa_commit call has failed with return code = " + num3;
                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                    throw exception2;
                }
                if (!onephase)
                {
                    base.TrText(method, "Hconn value = " + this.hconn.Value + " issuing MQGET to remove recovery message.");
                    this.hconn.XARecoveryBridge.LogCurrentTransactionEnd(this.xid, this.hconn.Rmid);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void MQRMIXAEnd()
        {
            uint method = 0x511;
            this.TrEntry(method);
            int num2 = 0;
            try
            {
                base.TrText(method, "Hconn value = " + this.hconn.Value + " issuing xa_end");
                this.xid.TraceFields();
                num2 = this.hconn.Session.MQFap.XA_End(this.hconn, this.xid, this.hconn.Rmid, 0x4000000);
                if (num2 != 0)
                {
                    TransactionException innerException = new TransactionException(NmqiTools.GetTranslatedExceptionMessage(this.ToString(), 0x20008385, (uint) num2));
                    MQException exception2 = new MQException(2, 0x893, innerException);
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = (uint) this.hconn.Rmid;
                    CommonServices.CommentInsert1 = "xa_end call has failed with return code = " + num2;
                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                    throw exception2;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public int MQRMIXAPrepare(byte[] recoveryInformation)
        {
            uint method = 0x512;
            this.TrEntry(method, new object[] { recoveryInformation });
            int num2 = 0;
            try
            {
                byte[] buffer = null;
                buffer = new byte[this.xid.GetRoundedLength()];
                this.xid.WriteStruct(buffer, 0);
                this.xid.TraceFields();
                base.TrText(method, "Hconn value = " + this.hconn.Value + " issuing xa_prepare");
                num2 = this.hconn.Session.MQFap.XA_Prepare(this.hconn, this.xid, this.hconn.Rmid, this.flags);
                if ((num2 != 0) && (num2 != 3))
                {
                    TransactionException innerException = new TransactionException(NmqiTools.GetTranslatedExceptionMessage(this.ToString(), 0x20008385, (uint) num2));
                    MQException exception2 = new MQException(2, 0x893, innerException);
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = (uint) this.hconn.Rmid;
                    CommonServices.CommentInsert1 = "xa_prepare call has failed with return code = " + num2;
                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                    throw exception2;
                }
                if (num2 == 0)
                {
                    base.TrText(method, "Hconn value = " + this.hconn.Value + " issuing MQPUT for tx recovery.");
                    this.hconn.XARecoveryBridge.LogCurrentTransactionStart(buffer, this.xid, this.myGuid, this.hconn.Rmid, recoveryInformation);
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return num2;
        }

        public void MQRMIXARollback()
        {
            uint method = 0x514;
            this.TrEntry(method);
            int flags = 0;
            try
            {
                base.TrText(method, "Hconn value = " + this.hconn.Value + " issuing xa_rollback");
                this.xid.TraceFields();
                int num2 = this.hconn.Session.MQFap.XA_Rollback(this.hconn, this.xid, this.hconn.Rmid, flags);
                if (num2 != 0)
                {
                    TransactionException innerException = new TransactionException(NmqiTools.GetTranslatedExceptionMessage(this.ToString(), 0x20008385, (uint) num2));
                    MQException exception2 = new MQException(2, 0x893, innerException);
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = (uint) this.hconn.Rmid;
                    CommonServices.CommentInsert1 = "xa_rollback call has failed with return code = " + num2;
                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                    throw exception2;
                }
                base.TrText(method, "Hconn value = " + this.hconn.Value + " issuing MQGET to remove recovery message.");
                this.hconn.XARecoveryBridge.LogCurrentTransactionEnd(this.xid, this.hconn.Rmid);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void MQRMIXAStart()
        {
            uint method = 0x50f;
            this.TrEntry(method);
            int num2 = 0;
            try
            {
                base.TrText(method, "Hconn value = " + this.hconn.Value + " issuing xa_start");
                this.xid.TraceFields();
                num2 = this.hconn.Session.MQFap.XA_Start(this.hconn, this.xid, this.hconn.Rmid, 0);
                if ((num2 != 0) && (num2 != 3))
                {
                    TransactionException innerException = new TransactionException(NmqiTools.GetTranslatedExceptionMessage(this.ToString(), 0x20008385, (uint) num2));
                    MQException exception2 = new MQException(2, 0x893, innerException);
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = (uint) this.hconn.Rmid;
                    CommonServices.CommentInsert1 = "xa_start call has failed with return code = " + num2;
                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                    throw exception2;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }
    }
}

