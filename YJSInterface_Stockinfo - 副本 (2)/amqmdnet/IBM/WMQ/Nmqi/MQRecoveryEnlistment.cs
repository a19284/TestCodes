namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Collections;
    using System.Transactions;

    internal class MQRecoveryEnlistment : NmqiObject, IEnlistmentNotification
    {
        private ArrayList incompleteXids;
        private ManagedHconn recoveryHconn;
        private MQXid recoveryXid;
        private int rmid_;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private bool success;
        private byte[] xid_;
        private MQXid[] xidList;
        private static int XIDLIST_COUNT = 500;

        internal MQRecoveryEnlistment(ManagedHconn manHconn, NmqiEnvironment env, sbyte[] xid, int rmid) : base(env)
        {
            this.recoveryXid = new MQXid();
            this.xidList = new MQXid[XIDLIST_COUNT];
            this.incompleteXids = new ArrayList();
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { manHconn, env, xid, rmid });
            this.recoveryHconn = manHconn;
            this.xid_ = new byte[xid.Length];
            Buffer.BlockCopy(xid, 0, this.xid_, 0, this.xid_.Length);
            this.recoveryXid.ReadXidFromBytes(this.xid_, 0);
            this.rmid_ = rmid;
            this.CheckXidListedIncompleteInWMQ();
        }

        private void CheckXidListedIncompleteInWMQ()
        {
            uint method = 0x523;
            this.TrEntry(method);
            this.recoveryHconn.GetMQFAP.XA_Open(this.recoveryHconn, this.recoveryHconn.Name, this.rmid_, 0);
            MQTransactionRecovery.log.WriteLog("Checking if transaction is listed as indount with Queuemanager");
            int num2 = this.recoveryHconn.GetMQFAP.XA_Recover(this.recoveryHconn, this.xidList, this.rmid_, 0x1800000);
            int num3 = 0;
            while (num2 >= this.xidList.Length)
            {
                num3 = this.xidList.Length + 0x3e8;
                this.xidList = new MQXid[num3];
                num2 = this.recoveryHconn.GetMQFAP.XA_Recover(this.recoveryHconn, this.xidList, this.rmid_, 0x1800000);
            }
            foreach (MQXid xid in this.xidList)
            {
                if (xid != null)
                {
                    this.incompleteXids.Add(xid);
                }
            }
            base.TrExit(method);
        }

        public void Commit(Enlistment enlistment)
        {
            uint method = 0x524;
            this.TrEntry(method, new object[] { enlistment });
            try
            {
                int num2 = 0;
                MQTransactionRecovery.log.WriteLog("Outcome of the transaction(as per DTC):", "Commit");
                int num3 = 0;
                num2 = 0;
                while (num2 < this.incompleteXids.Count)
                {
                    if (this.recoveryXid.Equals(this.incompleteXids[num2]))
                    {
                        if (MQTransactionRecovery.userMode)
                        {
                            num3 = this.HeuristicTransactionCompletion();
                        }
                        else
                        {
                            num3 = this.recoveryHconn.GetMQFAP.XA_Commit(this.recoveryHconn, this.recoveryXid, this.rmid_, 0);
                            MQTransactionRecovery.log.WriteLog("Transaction Commit was successful with Returncode:  " + num3);
                        }
                        if (num3 < 0)
                        {
                            TransactionException innerException = new TransactionException(NmqiTools.GetTranslatedExceptionMessage(this.ToString(), 0x20008385, (uint) num3));
                            MQException exception2 = new MQException(2, 0x893, innerException);
                            throw exception2;
                        }
                        enlistment.Done();
                        this.success = true;
                        break;
                    }
                    num2++;
                }
                if (num2 == this.incompleteXids.Count)
                {
                    MQTransactionRecovery.log.WriteLog("This particular Tx is not listed as in-doubt with WMQ. Releasing it now.");
                    enlistment.Done();
                    this.success = true;
                }
            }
            catch (Exception exception3)
            {
                base.TrException(method, exception3);
                MQTransactionRecovery.log.WriteLog("Actual state of the Transaction:", "Unknown/Indoubt");
                throw exception3;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private int HeuristicTransactionCompletion()
        {
            uint method = 0x525;
            this.TrEntry(method);
            Console.WriteLine("DistributedIdentifier :- " + this.recoveryXid.GlobalTransactionId);
            Console.WriteLine("LocalIdentifier :-" + this.recoveryXid.LocalTransactionId);
            Console.Write("Commit[c]/Rollback[r]: ");
            char keyChar = Console.ReadKey(false).KeyChar;
            MQTransactionRecovery.log.WriteLog("User Input :- " + keyChar);
            if (keyChar == 'c')
            {
                int num2 = this.recoveryHconn.GetMQFAP.XA_Commit(this.recoveryHconn, this.recoveryXid, this.rmid_, 0);
                if (num2 != 0)
                {
                    MQTransactionRecovery.log.WriteLog("Transaction Commit was unsuccessful, Returncode:  " + num2);
                }
                MQTransactionRecovery.log.WriteLog("Transaction Commit was successful with Returncode:  " + num2);
                base.TrExit(method, num2, 1);
                return num2;
            }
            if (keyChar != 'r')
            {
                throw new ArgumentException("Invalid input - " + keyChar);
            }
            int result = this.recoveryHconn.GetMQFAP.XA_Rollback(this.recoveryHconn, this.recoveryXid, this.rmid_, 0x20000000);
            if (result != 0)
            {
                MQTransactionRecovery.log.WriteLog("Transaction Rollback was unsuccessful, Returncode:  " + result);
            }
            base.TrExit(method, result, 2);
            return result;
        }

        public void InDoubt(Enlistment enlistment)
        {
            uint method = 0x527;
            this.TrEntry(method, new object[] { enlistment });
            enlistment.Done();
            base.TrExit(method);
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            uint method = 0x528;
            this.TrEntry(method, new object[] { preparingEnlistment });
            preparingEnlistment.ForceRollback();
            base.TrExit(method);
        }

        public void Rollback(Enlistment enlistment)
        {
            uint method = 0x526;
            this.TrEntry(method, new object[] { enlistment });
            MQTransactionRecovery.log.WriteLog("Outcome of the transaction(as per DTC):", "Rollback");
            int num2 = 0;
            int num3 = 0;
            try
            {
                num3 = 0;
                while (num3 < this.incompleteXids.Count)
                {
                    if (this.recoveryXid.Equals(this.incompleteXids[num3]))
                    {
                        if (MQTransactionRecovery.userMode)
                        {
                            num2 = this.HeuristicTransactionCompletion();
                        }
                        else
                        {
                            num2 = this.recoveryHconn.GetMQFAP.XA_Rollback(this.recoveryHconn, this.recoveryXid, this.rmid_, 0);
                            MQTransactionRecovery.log.WriteLog("Transaction Rollback was successful with Returncode:  " + num2);
                        }
                        if (num2 < 0)
                        {
                            TransactionException innerException = new TransactionException(NmqiTools.GetTranslatedExceptionMessage(this.ToString(), 0x20008385, (uint) num2));
                            MQException exception2 = new MQException(2, 0x893, innerException);
                            throw exception2;
                        }
                        this.success = true;
                        enlistment.Done();
                        break;
                    }
                    num3++;
                }
                if (num3 == this.incompleteXids.Count)
                {
                    MQTransactionRecovery.log.WriteLog("This particular Tx is not listed as in-doubt with WMQ. Releasing it now.");
                    enlistment.Done();
                    this.success = true;
                }
            }
            catch (Exception exception3)
            {
                base.TrException(method, exception3);
                MQTransactionRecovery.log.WriteLog("Actual state of the Transaction:", "Unknown/Indoubt");
                throw exception3;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal bool Success
        {
            get
            {
                return this.success;
            }
        }
    }
}

