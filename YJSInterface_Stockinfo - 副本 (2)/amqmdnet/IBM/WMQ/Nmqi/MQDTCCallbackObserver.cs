namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Runtime.CompilerServices;
    using System.Transactions;

    internal class MQDTCCallbackObserver : NmqiXAResourceManager, ISinglePhaseNotification, IEnlistmentNotification, IDisposable
    {
        internal volatile bool rmiTxComplete;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public MQDTCCallbackObserver(NmqiEnvironment env, ManagedHconn hConn) : base(env, hConn)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { env, hConn });
            this.GenerateGuid();
        }

        public void Commit(Enlistment enlistment)
        {
            uint method = 0x4fc;
            this.TrEntry(method, new object[] { enlistment });
            bool flag = false;
            try
            {
                if (base.asyncFailureNotify == null)
                {
                    base.TrText(method, "Hconn =" + base.hconn.Value + " has been asked for Commit(2phase) by the Transaction Co-ordinator");
                    base.MQRMIXACommit(false);
                    base.TrText(method, "WMQ .NET has commited now for current transaction.");
                    flag = true;
                }
            }
            catch (Exception exception)
            {
                base.TrException(method, exception);
            }
            finally
            {
                if (base.hconn != null)
                {
                    base.hconn.PulseCommitWait();
                }
                if (flag)
                {
                    enlistment.Done();
                }
                this.rmiTxComplete = true;
                base.TrExit(method);
            }
        }

        public void Dispose()
        {
            base.hconn = null;
        }

        internal void EnlistTransactionIntoDtc()
        {
            uint method = 0x4f8;
            this.TrEntry(method);
            try
            {
                base.TrText(method, "ResourceManager Id:- " + NmqiTools.ArrayToHexString(this.myGuid.ToByteArray()));
                base.TrText(method, "Hconn = " + base.hconn.Value);
                base.TrText(method, "Enlisting as Durable Resource manager into Current Transaction");
                base.InitializeMQRMI();
                Transaction.Current.EnlistDurable(base.myGuid, (ISinglePhaseNotification) this, EnlistmentOptions.EnlistDuringPrepareRequired);
                base.TrText(method, "WMQ .NET Enlisted as Resource Manager with Transaction Co-ordinator");
                base.hconn.currentTransaction = Transaction.Current;
                base.TrText(method, "Current Transaction's Information");
                base.TrText(method, "DistributedIdentifier:- " + NmqiTools.ArrayToHexString(Transaction.Current.TransactionInformation.DistributedIdentifier.ToByteArray()));
                base.TrText(method, "LocalIdentifier:- " + Transaction.Current.TransactionInformation.LocalIdentifier);
            }
            catch (MQException exception)
            {
                base.TrException(method, exception, 1);
                throw exception;
            }
            catch (TransactionException exception2)
            {
                base.TrException(method, exception2, 2);
                MQException exception3 = new MQException(2, 0x932, exception2);
                throw exception3;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void GenerateGuid()
        {
            uint method = 0x4fa;
            this.TrEntry(method);
            try
            {
                base.myGuid = base.env.GetMQResourceManagerIdentifier(base.hconn.Name.Trim());
                base.TrData(method, 0, "ResourceManager ID", this.myGuid.ToByteArray());
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void InDoubt(Enlistment enlistment)
        {
            uint method = 0x4fd;
            this.TrEntry(method, new object[] { enlistment });
            base.TrExit(method);
            throw new Exception("The method or operation is not implemented.");
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            uint method = 0x4fe;
            this.TrEntry(method, new object[] { preparingEnlistment });
            Exception e = null;
            bool flag = false;
            try
            {
                if (base.asyncFailureNotify != null)
                {
                    base.TrText(method, "Rollback the tx as we have an async failure");
                    preparingEnlistment.ForceRollback(base.asyncFailureNotify);
                    flag = true;
                }
                else
                {
                    base.TrText(method, "Hconn =" + base.hconn.Value + " has been asked to Prepare by the Transaction Co-ordinator");
                    base.MQRMIXAEnd();
                    if (base.MQRMIXAPrepare(preparingEnlistment.RecoveryInformation()) == 3)
                    {
                        flag = true;
                        preparingEnlistment.Done();
                    }
                    else
                    {
                        base.TrText(method, "WMQ .NET is now prepared..");
                        preparingEnlistment.Prepared();
                    }
                }
            }
            catch (MQException exception2)
            {
                base.TrException(method, exception2, 1);
                e = exception2;
                preparingEnlistment.ForceRollback(e);
                flag = true;
            }
            catch (Exception exception3)
            {
                base.TrException(method, exception3, 2);
                e = exception3;
                preparingEnlistment.ForceRollback(e);
                flag = true;
            }
            finally
            {
                if (flag)
                {
                    this.rmiTxComplete = true;
                    if (base.hconn != null)
                    {
                        base.hconn.PulseCommitWait();
                    }
                }
                base.TrExit(method);
            }
        }

        public void Rollback(Enlistment enlistment)
        {
            uint method = 0x4ff;
            this.TrEntry(method, new object[] { enlistment });
            try
            {
                if (base.asyncFailureNotify == null)
                {
                    base.TrText(method, "Hconn =" + base.hconn.Value + " has been asked to Rollback by the Transaction Co-ordinator");
                    base.MQRMIXARollback();
                    base.TrText(method, "WMQ .NET has Rolledback the unit of work under the current transaction.");
                }
            }
            catch (Exception exception)
            {
                base.TrException(method, exception);
            }
            finally
            {
                this.rmiTxComplete = true;
                if (base.hconn != null)
                {
                    base.hconn.PulseCommitWait();
                }
                enlistment.Done();
                base.TrExit(method);
            }
        }

        public void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
        {
            uint method = 0x4fb;
            this.TrEntry(method, new object[] { singlePhaseEnlistment });
            bool flag = false;
            Exception e = null;
            try
            {
                base.TrText(method, "Hconn =" + base.hconn.Value + " has been asked for SinglePhaseCommit by the Transaction Co-ordinator");
                base.MQRMIXACommit(true);
                flag = true;
            }
            catch (MQException exception2)
            {
                base.TrException(method, exception2);
                e = exception2;
                throw;
            }
            finally
            {
                if (flag)
                {
                    singlePhaseEnlistment.Done();
                }
                else if (e != null)
                {
                    singlePhaseEnlistment.Aborted(e);
                }
                else
                {
                    singlePhaseEnlistment.Aborted(new Exception("Underlaying MQ XA call has failed"));
                }
                this.rmiTxComplete = true;
                if (base.hconn != null)
                {
                    base.hconn.PulseCommitWait();
                }
                base.TrExit(method);
            }
        }
    }
}

