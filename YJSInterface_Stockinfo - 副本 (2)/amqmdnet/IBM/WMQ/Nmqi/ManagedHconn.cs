namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Transactions;

    public class ManagedHconn : NmqiObject, Hconn
    {
        private int acFlags;
        private const int activeTransactionTimeout = 0xea60;
        internal int associatedTransactionStyle;
        internal int branchCount;
        private bool callbackStarted;
        private bool callbackSuspended;
        private Lock callLock;
        private object connectionArea;
        private MQConnectOptions connectOptions;
        internal Transaction currentTransaction;
        private const int DEFAULT_RECONNECT_TIMEOUT = 0x708;
        private bool dispatchEventPosted;
        private Lock dispatchEventSync;
        private Lock dispatchLock;
        private Lock dispatchQMutex;
        private List<MQProxyQueue> dispatchQueueList;
        private MQDispatchThread dispatchThread;
        private MQConsumer ehObject;
        private int ehOptions;
        private MQCBD eventDesc;
        private bool eventRegistered;
        private int eventsHad;
        private int eventsRaised;
        private bool eventSuspended;
        private int globalMessageIndex;
        private int hconnAsInteger;
        private ArrayList hobjs;
        private ArrayList hsubs;
        internal bool isXaEnabled;
        public const int LOCAL_TRAN_ACTIVE = 3;
        public const int LOCAL_UOW_NONE = 0;
        public int localUowState;
        private long nextReconnect;
        private NmqiConnectOptions nmqiConnectOptions;
        private Lock notifyLock;
        private volatile bool openForXA;
        private ManagedHconn parent;
        private Phconn parentPhconn;
        internal volatile bool previousTransactionCompleted;
        private MQProxyQueueManager proxyQueueManager;
        private string qmName;
        private bool reconnectable;
        private int reconnectAttempts;
        private long reconnectExpiry;
        private bool reconnectionFailed;
        private int reconnectionFailureCompCode;
        private NmqiException reconnectionFailureException;
        private int reconnectionFailureReason;
        private bool reconnectionInProgress;
        private Lock reconnectLock;
        private static MQManagedReconnectableThread reconnectThread;
        private int rmid;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private MQSession session;
        private int shareOption;
        private int sleepingTimeout;
        private Lock threadLock;
        internal int TRANSACTION_LOCAL;
        internal int TRANSACTION_XA;
        private object waitForCommitComplete;
        private MQXARecoveryBridge xaRecoveryBridge;
        private MQDTCCallbackObserver xaResource;
        private object XAResourceCache;
        private int xaState;

        internal ManagedHconn(NmqiEnvironment env, MQSession remoteSession, string qmName, MQConnectOptions connectOptions) : base(env)
        {
            this.reconnectLock = new Lock();
            this.callLock = new Lock();
            this.notifyLock = new Lock();
            this.dispatchEventSync = new Lock();
            this.dispatchLock = new Lock();
            this.threadLock = new Lock();
            this.dispatchQueueList = new List<MQProxyQueue>();
            this.dispatchQMutex = new Lock();
            this.sleepingTimeout = 100;
            this.TRANSACTION_XA = 1;
            this.TRANSACTION_LOCAL = 2;
            this.waitForCommitComplete = new object();
            this.XAResourceCache = new object();
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { env, remoteSession, qmName, connectOptions });
            this.qmName = qmName;
            this.connectOptions = env.NewMQCNO();
            this.connectOptions.ClientConn = connectOptions.ClientConn;
            this.connectOptions.Options = connectOptions.Options;
            this.connectOptions.SecurityParms = connectOptions.SecurityParms;
            this.connectOptions.SslConfigOptions = connectOptions.SslConfigOptions;
            if (remoteSession.IsMultiplexingEnabled)
            {
                this.proxyQueueManager = new MQProxyQueueManager(env, this);
            }
            this.hobjs = new ArrayList();
            this.hsubs = new ArrayList();
            this.Session = remoteSession;
        }

        internal int AddHobj(ManagedHobj hobj)
        {
            uint method = 0x556;
            this.TrEntry(method, new object[] { hobj });
            int result = this.hobjs.Add(hobj);
            base.TrExit(method, result);
            return result;
        }

        internal int AddHsub(ManagedHsub hsub)
        {
            uint method = 0x555;
            this.TrEntry(method, new object[] { hsub });
            int result = this.hsubs.Add(hsub);
            base.TrExit(method, result);
            return result;
        }

        public void AddToDispatchList(MQProxyQueue pq)
        {
            uint method = 0x572;
            this.TrEntry(method, new object[] { pq });
            try
            {
                this.dispatchQMutex.Acquire();
                this.dispatchQueueList.Add(pq);
            }
            finally
            {
                this.dispatchQMutex.Release();
                base.TrExit(method);
            }
        }

        private void CallEventHandler(int callType, int compCode, int reason)
        {
            uint method = 0x56d;
            this.TrEntry(method, new object[] { callType, compCode, reason });
            if (this.IsReconnectable && IsReconnectableReasonCode(reason))
            {
                base.TrExit(method, 1);
            }
            else if (this.ehObject == null)
            {
                base.TrExit(method, 2);
            }
            else
            {
                MQCBC mqcbc = base.env.NewMQCBC();
                switch (callType)
                {
                    case 3:
                    case 4:
                        break;

                    case 5:
                        if ((this.acFlags & 0x400) == 0)
                        {
                            break;
                        }
                        base.TrExit(method, 3);
                        return;

                    default:
                        base.TrExit(method, 4);
                        return;
                }
                mqcbc.CallType = callType;
                mqcbc.CompCode = compCode;
                mqcbc.Reason = reason;
                MQConsumer ehObject = this.ehObject;
                try
                {
                    ehObject.Consumer(this.ParentPhconn, null, null, null, mqcbc);
                }
                catch (Exception)
                {
                }
                finally
                {
                    base.TrExit(method, 5);
                }
            }
        }

        public bool CheckClientEmpty()
        {
            uint method = 0x53e;
            this.TrEntry(method);
            bool result = this.proxyQueueManager.CheckClientEmpty();
            base.TrExit(method, result);
            return result;
        }

        public void CheckDispatchable(MQProxyQueue pq)
        {
            uint method = 0x53f;
            this.TrEntry(method, new object[] { pq });
            MQDispatchThread dispatchThread = null;
            bool flag = false;
            if (((this.EventsHad != this.eventsRaised) && (this.ehObject != null)) && ((this.acFlags & 0x400) == 0))
            {
                flag = true;
            }
            if (!flag)
            {
                if (((this.acFlags & 0x40) == 0) || ((this.acFlags & 0x80) != 0))
                {
                    base.TrExit(method, 1);
                    return;
                }
                if (pq != null)
                {
                    flag = pq.IsDispatchableQ();
                }
                else
                {
                    try
                    {
                        this.dispatchQMutex.Acquire();
                        foreach (MQProxyQueue queue in this.dispatchQueueList)
                        {
                            if (queue.IsDispatchableQ())
                            {
                                flag = true;
                            }
                        }
                    }
                    finally
                    {
                        this.dispatchQMutex.Release();
                    }
                }
                if (!flag)
                {
                    base.TrExit(method, 2);
                    return;
                }
            }
            this.RequestThreadLock();
            try
            {
                dispatchThread = this.DispatchThread;
                dispatchThread.IncrementDispatchSeq();
                if ((dispatchThread.Status & 1) != 0)
                {
                    dispatchThread.PostSleepingEvent();
                    dispatchThread.Status &= -2;
                }
            }
            finally
            {
                this.ReleaseThreadLock();
                base.TrExit(method, 2);
            }
        }

        public void CheckTxnAllowed()
        {
            uint method = 0x540;
            this.TrEntry(method);
            try
            {
                int globalMessageIndex = this.globalMessageIndex;
                if (this.CheckClientEmpty())
                {
                    this.SendNotification(-1, 8, globalMessageIndex, false);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void CheckTxnMessage()
        {
            uint method = 0x578;
            this.TrEntry(method);
            this.proxyQueueManager.CheckTxnMessage();
            base.TrExit(method);
        }

        internal void CheckUOWNotMixed(int Opts, int mqOp)
        {
            uint method = 0x4f2;
            this.TrEntry(method, new object[] { Opts, mqOp });
            int num2 = 0;
            try
            {
                bool flag = Transaction.Current != null;
                if (mqOp == 1)
                {
                    num2 = 2;
                }
                else
                {
                    num2 = 2;
                }
                if (flag && ((this.associatedTransactionStyle & this.TRANSACTION_LOCAL) != 0))
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x933, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                if ((!flag && ((this.associatedTransactionStyle & this.TRANSACTION_XA) != 0)) && ((num2 & Opts) != 0))
                {
                    NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x933, null);
                    base.TrException(method, exception2);
                    throw exception2;
                }
                if ((this.xaState == 4) && (Transaction.Current == null))
                {
                    NmqiException exception3 = new NmqiException(base.env, -1, null, 2, 0x933, null);
                    base.TrException(method, exception3);
                    throw exception3;
                }
                if (flag && ((num2 & Opts) == 0))
                {
                    NmqiException exception4 = new NmqiException(base.env, -1, null, 2, 0x8b8, null);
                    base.TrException(method, exception4);
                    throw exception4;
                }
                if (this.associatedTransactionStyle == 0)
                {
                    if (flag && ((num2 & Opts) != 0))
                    {
                        this.associatedTransactionStyle = this.TRANSACTION_XA;
                    }
                    else if (!flag && ((num2 & Opts) != 0))
                    {
                        this.associatedTransactionStyle = this.TRANSACTION_LOCAL;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void CheckUsable()
        {
            uint method = 0x568;
            this.TrEntry(method);
            if (!this.IsReconnectable)
            {
                base.TrExit(method, 1);
            }
            else
            {
                object data = Thread.GetData(Thread.GetNamedDataSlot("MQ_CLIENT_THREAD_TYPE"));
                if ((data != null) && (Convert.ToInt32(data) == 3))
                {
                    base.TrExit(method, 2);
                }
                else
                {
                    try
                    {
                        lock (this.reconnectLock)
                        {
                            while (this.reconnectionInProgress)
                            {
                                try
                                {
                                    int depth = this.FullyReleaseDispatchLock();
                                    this.WaitForReconnection();
                                    this.ReacquireDispatchLock(depth);
                                    continue;
                                }
                                catch (ThreadInterruptedException)
                                {
                                    continue;
                                }
                            }
                        }
                    }
                    finally
                    {
                        base.TrExit(method, 3);
                    }
                }
            }
        }

        internal void CleanupTxState()
        {
            uint method = 0x645;
            this.TrEntry(method);
            try
            {
                if ((this.currentTransaction != null) && ((((this.xaState & 4) != 0) || !this.xaResource.rmiTxComplete) || !this.xaResource.dtcTxComplete))
                {
                    this.xaResource.asyncFailureNotify = this.session.AsyncFailure;
                    base.TrText(method, "Issuing Rollback on the current Transaction");
                    this.xaResource.MQRMIXARollback();
                    base.TrText(method, "Current Transaction has been set to null");
                    this.currentTransaction = null;
                    base.TrText(method, "Pulsing any waiters for current transaction to complete");
                    this.PulseCommitWait();
                }
                if (this.xaResource != null)
                {
                    this.xaResource = null;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public bool ConsumersChanged()
        {
            uint method = 0x58e;
            this.TrEntry(method);
            bool result = (this.acFlags & 0x200) != 0;
            base.TrExit(method, result);
            return result;
        }

        public void DeliverException(NmqiException e, int flag, int eventS)
        {
            uint method = 0x576;
            this.TrEntry(method, new object[] { e, flag, eventS });
            if (!((this.IsReconnectable && IsReconnectableReasonCode(e.Reason)) && this.EligibleForReconnect(this.session, true)))
            {
                this.acFlags |= flag;
                this.session.AsyncFailureNotify(e);
                this.RaiseEvent(eventS);
                this.WakeGetters();
                this.CleanupTxState();
            }
            else
            {
                this.session.AsyncFailure = e;
                if (this.proxyQueueManager != null)
                {
                    this.proxyQueueManager.NotifyConnectionFailure(this.session.AsyncFailure);
                }
            }
            base.TrExit(method);
        }

        internal void DispatchThreadExchange()
        {
            uint method = 0x543;
            this.TrEntry(method);
            this.dispatchThread.PostSleepingEvent();
            int num2 = this.FullyReleaseDispatchLock();
            this.WaitOnDispatchEvent(-1);
            int num3 = num2 - 1;
            for (int i = 0; i < num3; i++)
            {
                this.RequestDispatchLock(-1);
            }
            base.TrExit(method);
        }

        public void DoStop()
        {
            uint method = 0x579;
            this.TrEntry(method);
            this.CheckTxnMessage();
            this.DriveStops();
            this.acFlags &= -131073;
            this.DispatchThread = null;
            this.ReleaseDispatchLock();
            this.PostDispatchThreadEvent();
            base.TrExit(method);
        }

        public void DoSuspend()
        {
            uint method = 0x577;
            this.TrEntry(method);
            this.CheckTxnMessage();
            this.acFlags &= -1048577;
            this.PostDispatchThreadEvent();
            base.TrExit(method);
        }

        public void DriveEventsEH()
        {
            uint method = 0x544;
            this.TrEntry(method);
            try
            {
                for (int i = 0; i < NmqiConstants.Events.Length; i++)
                {
                    if (((this.EventsHad & NmqiConstants.Events[i]) != 0) && ((this.eventsRaised & NmqiConstants.Events[i]) == 0))
                    {
                        this.eventsRaised |= NmqiConstants.Events[i];
                        this.CallEventHandler(5, 2, NmqiConstants.EventReasons[i]);
                        if (this.eventsRaised == this.EventsHad)
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

        public void DriveEventsEH(int cc, int reason)
        {
            uint method = 0x544;
            this.TrEntry(method);
            try
            {
                this.CallEventHandler(5, cc, reason);
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

        public void DriveOutstanding()
        {
            uint method = 0x545;
            this.TrEntry(method);
            this.acFlags |= 0x200;
            try
            {
                while ((this.acFlags & 0x200) != 0)
                {
                    this.acFlags &= -513;
                    try
                    {
                        this.dispatchQMutex.Acquire();
                        foreach (MQProxyQueue queue in this.dispatchQueueList.ToArray())
                        {
                            if ((this.acFlags & 0x40) != 0)
                            {
                                if (((queue.MqcbCBD.Options & 1) != 0) && ((queue.Status & 0x400000) == 0))
                                {
                                    queue.CallConsumer(1, 0, 0);
                                }
                            }
                            else if ((((queue.MqcbCBD.Options & 4) != 0) && ((queue.Status & 0x400000) != 0)) && ((queue.Status & 0x800000) == 0))
                            {
                                queue.CallConsumer(2, 0, 0);
                            }
                            if ((queue.Status & 0x200000) != 0)
                            {
                                queue.MqcbDeRegisterMC(true, null);
                            }
                        }
                        continue;
                    }
                    finally
                    {
                        this.dispatchQMutex.Release();
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void DriveStops()
        {
            uint method = 0x575;
            this.TrEntry(method);
            this.DriveOutstanding();
            try
            {
                this.dispatchQMutex.Acquire();
                foreach (MQProxyQueue queue in this.dispatchQueueList)
                {
                    queue.DriveStop();
                }
            }
            finally
            {
                this.dispatchQMutex.Release();
                base.TrExit(method);
            }
        }

        internal bool EligibleForReconnect(MQSession failedSession, bool register)
        {
            uint method = 0x55d;
            this.TrEntry(method, new object[] { failedSession, register });
            bool result = false;
            try
            {
                lock (this.reconnectLock)
                {
                    if (reconnectThread == null)
                    {
                        reconnectThread = this.Session.MQFap.ReconnectThread;
                    }
                }
                result = reconnectThread.EligibleForReconnect(this, failedSession, register);
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        internal bool EnlistmentRequired()
        {
            uint method = 0x4f4;
            this.TrEntry(method);
            if (this.currentTransaction == null)
            {
                base.TrText(method, "Current Transaction reference on this hconn found to be null. Needs enlistment");
                if (this.xaResource != null)
                {
                    this.xaResource = null;
                }
                base.TrExit(method, true, 1);
                return true;
            }
            if (this.currentTransaction.Equals(Transaction.Current))
            {
                base.TrText(method, "Current work is part of already enlisted Transaction. No need of enlistment again");
                base.TrExit(method, false, 2);
                return false;
            }
            try
            {
                if ((this.currentTransaction != null) && ((this.xaState & 4) != 0))
                {
                    base.TrText(method, "A new Transaction request has come while eixsting is in progress. This is not right, throw exception");
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x92f, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                if (!this.xaResource.rmiTxComplete && !this.xaResource.dtcTxComplete)
                {
                    base.TrText(method, "Transaction has prepared, but due to the own management of recovery, we want to wait until its committed");
                    this.WaitForInProgressTx();
                    base.TrText(method, "Wait completed");
                }
                else if (!this.xaResource.rmiTxComplete && this.xaResource.dtcTxComplete)
                {
                    base.TrText(method, "This will never happen, where nmqi resource did not commit, but dtc resource committed. But if we get in, throw exception");
                    base.throwNewMQException(2, 0x932);
                }
                if (this.xaResource != null)
                {
                    this.xaResource = null;
                }
            }
            finally
            {
                base.TrExit(method, true, 3);
            }
            return true;
        }

        public void EnlistTransaction(NmqiEnvironment env, ManagedHconn hConn)
        {
            uint method = 0x642;
            this.TrEntry(method);
            try
            {
                lock (this.XAResourceCache)
                {
                    try
                    {
                        if (!this.ParentConnection.IsXASupported())
                        {
                            base.TrText(method, "XA is not supported by the connected Queue manager");
                            base.throwNewMQException(2, 0x8fa);
                        }
                        if (hConn.EnlistmentRequired())
                        {
                            base.TrText(method, "Enlistment is required");
                            this.xaResource = new MQDTCCallbackObserver(env, hConn);
                            if (!this.openForXA)
                            {
                                base.TrText(method, "This is first XA call on this hconn, issue xa_open");
                                this.rmid = env.NextAvailableRmid;
                                base.TrText(method, "Rmid on this hconn is set to - " + this.rmid);
                                this.MQRMIXAOpen();
                                this.xaRecoveryBridge = new MQXARecoveryBridge(this, env);
                                base.TrText(method, "XA Recovery Bridge has been initialized");
                                this.openForXA = true;
                            }
                            base.TrText(method, "Starting the Transaction branch now..");
                            this.xaResource.EnlistTransactionIntoDtc();
                        }
                    }
                    catch (Exception exception)
                    {
                        base.TrException(method, exception);
                        if ((exception is MQException) || (exception is NmqiException))
                        {
                            throw exception;
                        }
                        MQException exception2 = new MQException(2, 0x932, exception);
                        throw exception2;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void EnterCall(bool isDispatchThread, bool callAllowedWhenAsyncStarted)
        {
            uint method = 0x55e;
            this.TrEntry(method, new object[] { isDispatchThread, callAllowedWhenAsyncStarted });
            try
            {
                this.EnterCall(isDispatchThread, callAllowedWhenAsyncStarted, true);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void EnterCall(Lock mutex, bool isDispatchThread, bool callAllowedWhenAsyncStarted)
        {
            uint method = 0x561;
            this.TrEntry(method, new object[] { mutex, isDispatchThread, callAllowedWhenAsyncStarted });
            int timeout = 0;
            try
            {
                if (this.shareOption == 0x40)
                {
                    timeout = -1;
                }
                if (!mutex.Acquire(timeout))
                {
                    int compCode = 2;
                    int reason = 0x7e2;
                    if (this.shareOption == 0x80)
                    {
                        reason = 0x8ab;
                    }
                    NmqiException ex = new NmqiException(base.env, -1, null, compCode, reason, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                if ((!isDispatchThread && ((this.acFlags & 0x40) != 0)) && (((this.acFlags & 0x80) == 0) && !callAllowedWhenAsyncStarted))
                {
                    mutex.Release();
                    NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x9c4, null);
                    base.TrException(method, exception2);
                    throw exception2;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void EnterCall(bool isDispatchThread, bool callAllowedWhenAsyncStarted, bool doCheck)
        {
            uint method = 0x55f;
            this.TrEntry(method, new object[] { isDispatchThread, callAllowedWhenAsyncStarted, doCheck });
            try
            {
                if (doCheck)
                {
                    this.CheckUsable();
                }
                this.EnterCall(this.callLock, isDispatchThread, callAllowedWhenAsyncStarted);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void EnterNotifyCall(bool isDispatchThread)
        {
            uint method = 0x560;
            this.TrEntry(method, new object[] { isDispatchThread });
            try
            {
                this.EnterCall(this.notifyLock, isDispatchThread, true);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public int FullyReleaseDispatchLock()
        {
            uint method = 0x54e;
            this.TrEntry(method);
            int result = 0;
            try
            {
                result = this.dispatchLock.FullyRelease();
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        public MQCBD GetEventDescriptor()
        {
            return this.eventDesc;
        }

        private int GetReconnectionTimeout()
        {
            uint method = 0x56a;
            this.TrEntry(method);
            if (this.nmqiConnectOptions != null)
            {
                int reconnectionTimeout = this.nmqiConnectOptions.ReconnectionTimeout;
                base.TrExit(method, reconnectionTimeout, 1);
                return reconnectionTimeout;
            }
            base.TrExit(method, 0x708, 2);
            return 0x708;
        }

        public bool HasEventsOutstanding()
        {
            uint method = 0x546;
            this.TrEntry(method);
            bool result = this.eventsRaised != this.EventsHad;
            base.TrExit(method, result);
            return result;
        }

        public bool HasFailed()
        {
            bool reconnectionFailed;
            uint method = 0x562;
            this.TrEntry(method);
            lock (this.reconnectLock)
            {
                reconnectionFailed = this.reconnectionFailed;
            }
            base.TrExit(method, reconnectionFailed);
            return reconnectionFailed;
        }

        public bool InitializeForReconnect()
        {
            bool flag;
            uint method = 0x547;
            this.TrEntry(method);
            try
            {
                lock (this.reconnectLock)
                {
                    if (!this.IsReconnecting && !this.session.IsReconnectionBegun)
                    {
                        this.reconnectExpiry = NmqiTools.GetCurrentTimeInMs();
                        this.reconnectExpiry += this.GetReconnectionTimeout() * 0x3e8;
                        this.reconnectAttempts = 0;
                        this.nextReconnect = NmqiTools.GetCurrentTimeInMs();
                        this.reconnectionInProgress = true;
                        this.session.IsReconnectionBegun = true;
                        return true;
                    }
                    flag = false;
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return flag;
        }

        public bool InTransaction()
        {
            uint method = 0x589;
            this.TrEntry(method);
            bool result = (this.acFlags & 0x4000000) != 0;
            base.TrExit(method, result);
            return result;
        }

        public bool IsCallbackStarted()
        {
            uint method = 0x566;
            this.TrEntry(method);
            base.TrExit(method, this.callbackStarted);
            return this.callbackStarted;
        }

        public bool IsCallbackSuspended()
        {
            uint method = 0x567;
            this.TrEntry(method);
            base.TrExit(method, this.callbackSuspended);
            return this.callbackSuspended;
        }

        public bool IsQuiescing()
        {
            uint method = 0x57a;
            this.TrEntry(method);
            bool result = (this.acFlags & 0x100) != 0;
            base.TrExit(method, result);
            return result;
        }

        public static bool IsReconnectableReasonCode(int reason)
        {
            switch (reason)
            {
                case 0x871:
                case 0x872:
                case 0x893:
                case 0x89a:
                case 0x7d9:
                case 0x80b:
                    return true;
            }
            return false;
        }

        public bool IsSPISupported()
        {
            return this.ParentConnection.IsSPISupported();
        }

        public bool IsStarted()
        {
            uint method = 0x582;
            this.TrEntry(method);
            bool result = (this.acFlags & 0x40) != 0;
            base.TrExit(method, result);
            return result;
        }

        public bool IsSuspended()
        {
            uint method = 0x57e;
            this.TrEntry(method);
            bool result = (this.acFlags & 0x80) != 0;
            base.TrExit(method, result);
            return result;
        }

        internal bool IsTransactionDoomed()
        {
            uint method = 0x57d;
            this.TrEntry(method);
            bool result = (this.acFlags & 0x8000000) != 0;
            base.TrExit(method, result);
            return result;
        }

        internal bool IsXaConnected()
        {
            uint method = 0x586;
            this.TrEntry(method);
            bool result = (this.acFlags & 8) != 0;
            base.TrExit(method, result);
            return result;
        }

        public void LeaveCall(int reason)
        {
            uint method = 0x548;
            this.TrEntry(method, new object[] { reason });
            try
            {
                this.LeaveCall(reason, true);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void LeaveCall(Lock mutex, int reason)
        {
            uint method = 0x54b;
            this.TrEntry(method, new object[] { mutex, reason });
            try
            {
                if ((reason != 0) && (reason != 0x9f0))
                {
                    this.RaiseEvent(reason);
                }
            }
            finally
            {
                mutex.Release();
                base.TrExit(method);
            }
        }

        public void LeaveCall(int reason, bool ignored)
        {
            uint method = 0x549;
            this.TrEntry(method, new object[] { reason, ignored });
            try
            {
                this.LeaveCall(this.callLock, reason);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void LeaveNotifyCall(int reason)
        {
            uint method = 0x54a;
            this.TrEntry(method, new object[] { reason });
            try
            {
                this.CheckUsable();
                this.LeaveCall(this.notifyLock, reason);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void MqcbDeregisterEH()
        {
            uint method = 0x56f;
            this.TrEntry(method);
            try
            {
                if (this.ehObject == null)
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 1, 0x990, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                if ((this.ehOptions & 0x200) != 0)
                {
                    this.CallEventHandler(4, 0, 0);
                }
                this.ehObject = null;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void MqcbRegisterEH(MQCBD callbackDesc)
        {
            uint method = 0x56e;
            this.TrEntry(method, new object[] { callbackDesc });
            try
            {
                this.ehObject = callbackDesc.mqConsumer;
                this.ehOptions = callbackDesc.Options;
                if ((this.ehOptions & 0x100) != 0)
                {
                    this.CallEventHandler(3, 0, 0);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void MqcbResumeEH()
        {
            uint method = 0x571;
            this.TrEntry(method);
            this.acFlags &= -1025;
            base.TrExit(method);
        }

        internal void MqcbSuspendEH()
        {
            uint method = 0x570;
            this.TrEntry(method);
            this.acFlags |= 0x400;
            base.TrExit(method);
        }

        private void MQRMIXAOpen()
        {
            uint method = 0x643;
            this.TrEntry(method);
            try
            {
                if (!this.IsXASupported)
                {
                    TransactionException innerException = new TransactionException(NmqiTools.GetTranslatedExceptionMessage(this.ToString(), 0x20008385, 0x7dc));
                    MQException exception2 = new MQException(2, 0x893, innerException);
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = (uint) this.rmid;
                    CommonServices.CommentInsert1 = "Connection do not support Distrubuted Transactions";
                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                    throw exception2;
                }
                base.TrText(method, "Hconn value = " + this.Value + " issuing xa_open");
                int num2 = this.Session.MQFap.XA_Open(this, this.Name, this.rmid, 0);
                if ((num2 != 0) && (num2 != 3))
                {
                    TransactionException exception3 = new TransactionException(NmqiTools.GetTranslatedExceptionMessage(this.ToString(), 0x20008385, (uint) num2));
                    MQException exception4 = new MQException(2, 0x932, exception3);
                    throw exception4;
                }
                this.IsXAEnabled = true;
                base.TrText(method, "Hconn has been opened for XA now");
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void PostDispatchThreadEvent()
        {
            uint method = 0x541;
            this.TrEntry(method);
            try
            {
                lock (this.dispatchEventSync)
                {
                    this.dispatchEventPosted = true;
                    Monitor.Pulse(this.dispatchEventSync);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void PulseCommitWait()
        {
            uint method = 0x647;
            this.TrEntry(method);
            try
            {
                lock (this.waitForCommitComplete)
                {
                    Monitor.Pulse(this.waitForCommitComplete);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void QmQuiescing()
        {
            uint method = 0x56c;
            this.TrEntry(method);
            this.acFlags |= 0x100;
            if (!this.IsReconnectable)
            {
                this.RaiseEvent(0x871);
                this.WakeGetters();
            }
            base.TrExit(method);
        }

        public void RaiseEvent(int reason)
        {
            uint method = 0x557;
            this.TrEntry(method, new object[] { reason });
            int index = 0;
            index = 0;
            while (index < NmqiConstants.Events.Length)
            {
                if (NmqiConstants.EventReasons[index] == reason)
                {
                    break;
                }
                index++;
            }
            if (index >= NmqiConstants.Events.Length)
            {
                base.TrExit(method, 1);
            }
            else if ((this.EventsHad & NmqiConstants.Events[index]) != 0)
            {
                base.TrExit(method, 2);
            }
            else if (this.IsReconnectable && !NmqiConstants.RaiseReconnecting[index])
            {
                base.TrExit(method, 3);
            }
            else
            {
                try
                {
                    this.proxyQueueManager.RaiseEvent(reason);
                    this.eventsHad |= NmqiConstants.Events[index];
                    if (this.ehObject != null)
                    {
                        this.CheckDispatchable(null);
                    }
                }
                finally
                {
                    base.TrExit(method, 4);
                }
            }
        }

        public bool ReacquireDispatchLock(int depth)
        {
            return this.dispatchLock.Acquire(-1, depth);
        }

        internal void Reconnect(MQSession failedSession)
        {
            uint method = 0x559;
            this.TrEntry(method, new object[] { failedSession });
            try
            {
                lock (this.reconnectLock)
                {
                    if (this.EligibleForReconnect(failedSession, true) || this.IsReconnecting)
                    {
                        goto Label_0061;
                    }
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x9f4, null);
                    base.TrException(method, ex);
                    throw ex;
                Label_005B:
                    this.WaitForReconnection();
                Label_0061:
                    if (!this.IsReconnecting)
                    {
                        if (this.reconnectionFailed)
                        {
                            if (this.reconnectionFailureException != null)
                            {
                                base.TrException(method, this.reconnectionFailureException);
                                throw this.reconnectionFailureException;
                            }
                            NmqiException exception2 = new NmqiException(base.env, -1, null, this.reconnectionFailureCompCode, this.reconnectionFailureReason, null);
                            base.TrException(method, exception2);
                            throw exception2;
                        }
                    }
                    else
                    {
                        goto Label_005B;
                    }
                }
            }
            catch (NmqiException exception3)
            {
                base.TrException(method, exception3);
                throw exception3;
            }
            catch (MQException exception4)
            {
                base.TrException(method, exception4);
                throw exception4;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Reconnected()
        {
            uint method = 0x563;
            this.TrEntry(method);
            this.reconnectionInProgress = false;
            this.WakeUpAllAfterReconnection();
            base.TrExit(method);
        }

        public void ReleaseDispatchLock()
        {
            uint method = 0x54d;
            this.TrEntry(method);
            try
            {
                this.dispatchLock.Release();
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void ReleaseThreadLock()
        {
            uint method = 0x550;
            this.TrEntry(method);
            try
            {
                this.threadLock.Release();
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void RemoveFromDispatchList(MQProxyQueue pq)
        {
            uint method = 0x573;
            this.TrEntry(method, new object[] { pq });
            try
            {
                this.dispatchQMutex.Acquire();
                if (!this.dispatchQueueList.Contains(pq))
                {
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = (uint) pq.Hobj;
                    CommonServices.ArithInsert2 = (uint) pq.ManHconn.Value;
                    CommonServices.CommentInsert1 = "PQ does not exist coinnections list of PQS";
                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x893, null);
                    base.TrException(method, ex);
                    throw ex;
                }
                this.dispatchQueueList.Remove(pq);
            }
            finally
            {
                this.dispatchQMutex.Release();
                base.TrExit(method);
            }
        }

        internal bool RemoveHobj(Hobj hobj)
        {
            uint method = 0x558;
            this.TrEntry(method, new object[] { hobj });
            bool result = false;
            if (hobj is ManagedHobj)
            {
                ManagedHobj hobj2 = (ManagedHobj) hobj;
                hobj2.LogicallyClosed = true;
                ManagedHsub parentHsub = hobj2.ParentHsub;
                if (parentHsub == null)
                {
                    this.hobjs.Remove(hobj);
                    result = true;
                }
                else if (parentHsub.LogicallyClosed)
                {
                    this.hsubs.Remove(parentHsub);
                    this.hobjs.Remove(hobj);
                    result = true;
                }
                else
                {
                    result = true;
                }
            }
            else if (hobj is ManagedHsub)
            {
                ManagedHsub hsub2 = (ManagedHsub) hobj;
                hsub2.LogicallyClosed = true;
                ManagedHobj hobj3 = hsub2.Hobj;
                if (hobj3 == null)
                {
                    this.hsubs.Remove(hobj);
                    result = true;
                }
                else if (hobj3.LogicallyClosed)
                {
                    this.hobjs.Remove(hobj3);
                    this.hsubs.Remove(hobj);
                    result = true;
                }
                else
                {
                    result = true;
                }
            }
            base.TrExit(method, result);
            return result;
        }

        public bool RequestDispatchLock(int waitInterval)
        {
            uint method = 0x54c;
            this.TrEntry(method, new object[] { waitInterval });
            bool result = false;
            try
            {
                result = this.dispatchLock.Acquire(waitInterval);
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        public void RequestThreadLock()
        {
            uint method = 0x54f;
            this.TrEntry(method);
            try
            {
                this.threadLock.Acquire();
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void ResetReconnectionEvents()
        {
            uint method = 0x551;
            this.TrEntry(method);
            this.eventsHad &= -32;
            this.eventsRaised &= -32;
            base.TrExit(method);
        }

        public static void SendAsyncNotification(NmqiEnvironment env, ManagedHconn hConn, int hobj, int code, int value)
        {
            MQSession session = hConn.Session;
            MQNOTIFICATION mqnotification = new MQNOTIFICATION();
            int translength = 0x24 + mqnotification.GetLength();
            MQTSH tsh = session.AllocateTSH(15, 1, true, translength);
            mqnotification.notify.notificationCode = code;
            mqnotification.notify.hObj = hobj;
            mqnotification.notify.notificationValue = value;
            mqnotification.notify.version = 1;
            tsh.Offset = tsh.WriteStruct(tsh.TshBuffer, tsh.Offset);
            tsh.Offset = mqnotification.WriteStruct(tsh.TshBuffer, tsh.Offset);
            tsh.Offset = 0;
            session.SendTSH(tsh);
        }

        public void SendConnState(bool reply)
        {
            uint method = 0x552;
            this.TrEntry(method, new object[] { reply });
            try
            {
                int num2;
                if ((this.acFlags & 0x40) != 0)
                {
                    num2 = 2;
                    if ((this.acFlags & 0x80) != 0)
                    {
                        num2 |= 1;
                    }
                }
                else
                {
                    num2 = 0;
                }
                this.SendNotification(-1, 3, num2, reply);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SendNotification(int hobj, int code, int value, bool reply)
        {
            uint method = 0x553;
            this.TrEntry(method, new object[] { hobj, code, value, reply });
            MQSession session = this.Session;
            MQNOTIFICATION mqnotification = new MQNOTIFICATION();
            int translength = 0x24 + mqnotification.GetLength();
            MQTSH requestTsh = session.AllocateTSH(15, 1, true, translength);
            try
            {
                if (reply)
                {
                    requestTsh.ControlFlags1 = (byte) (requestTsh.ControlFlags1 | 1);
                }
                mqnotification.notify.notificationCode = code;
                mqnotification.notify.notificationValue = value;
                mqnotification.notify.hObj = hobj;
                mqnotification.notify.version = 1;
                requestTsh.Offset = requestTsh.WriteStruct(requestTsh.TshBuffer, requestTsh.Offset);
                requestTsh.Offset = mqnotification.WriteStruct(requestTsh.TshBuffer, requestTsh.Offset);
                requestTsh.Offset = 0;
                if (reply)
                {
                    MQTSH rTSH = session.ExchangeTSH(requestTsh);
                    if (rTSH.SegmentType != 15)
                    {
                        base.TrData(method, 0, "Unexpected reply received", 0, rTSH.Length, rTSH.TshBuffer);
                        CommonServices.SetValidInserts();
                        CommonServices.ArithInsert1 = rTSH.SegmentType;
                        CommonServices.CommentInsert1 = rTSH.ConversationID.ToString();
                        CommonServices.CommentInsert2 = "Unexpected reply received on proxy queue's requst messages";
                        base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                        NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x893, null);
                        base.TrException(method, ex);
                        throw ex;
                    }
                    session.ReleaseReceivedTSH(rTSH);
                }
                else
                {
                    this.Session.SendData(requestTsh.TshBuffer, requestTsh.Offset, requestTsh.Length, requestTsh.SegmentType, 1);
                }
            }
            finally
            {
                requestTsh.ParentBuffer.Free();
                requestTsh.ParentBuffer = null;
                base.TrExit(method);
            }
        }

        public void SetConsumersChanged()
        {
            uint method = 0x58f;
            this.TrEntry(method);
            this.acFlags |= 0x200;
            base.TrExit(method);
        }

        public void SetDisconnected()
        {
            uint method = 0x554;
            this.TrEntry(method);
            try
            {
                if (this.proxyQueueManager != null)
                {
                    this.proxyQueueManager.NotifyConnectionFailure(this.session.AsyncFailure);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetInTransaction()
        {
            uint method = 0x58a;
            this.TrEntry(method);
            this.acFlags |= 0x4000000;
            base.TrExit(method);
        }

        public void SetQuiescing()
        {
            uint method = 0x57b;
            this.TrEntry(method);
            this.acFlags |= 0x100;
            base.TrExit(method);
        }

        public void SetReconnectionFailure(int compcode, int reason, NmqiException e)
        {
            uint method = 0x569;
            this.TrEntry(method, new object[] { compcode, reason, e });
            lock (this.reconnectLock)
            {
                this.reconnectable = false;
                this.session.Hconn = null;
                this.reconnectionFailed = true;
                this.reconnectionFailureCompCode = compcode;
                this.reconnectionFailureReason = reason;
                this.reconnectionFailureException = e;
            }
            try
            {
                this.CallEventHandler(5, 2, reason);
            }
            catch (NmqiException)
            {
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void SetStarted()
        {
            uint method = 0x583;
            this.TrEntry(method);
            this.acFlags |= 0x40;
            base.TrExit(method);
        }

        internal void SetStopPending()
        {
            uint method = 0x585;
            this.TrEntry(method);
            this.acFlags |= 0x20000;
            base.TrExit(method);
        }

        internal void SetSuspended()
        {
            uint method = 0x580;
            this.TrEntry(method);
            this.acFlags |= 0x80;
            base.TrExit(method);
        }

        public void SetSuspending()
        {
            uint method = 0x58d;
            this.TrEntry(method);
            this.acFlags |= 0x100000;
            base.TrExit(method);
        }

        public bool SetSuspendPending()
        {
            uint method = 0x58c;
            this.TrEntry(method);
            bool result = (this.acFlags & 0x100000) != 0;
            base.TrExit(method, result);
            return result;
        }

        public void SetTransactionDoomed()
        {
            uint method = 0x590;
            this.TrEntry(method);
            this.acFlags |= 0x8000000;
            base.TrExit(method);
        }

        internal void SetupCallback(int operation)
        {
            uint method = 0x564;
            this.TrEntry(method, new object[] { operation });
            if (this.reconnectionInProgress)
            {
                goto Label_006D;
            }
            int num2 = operation;
            if (num2 <= 4)
            {
                switch (num2)
                {
                    case 1:
                        goto Label_0046;

                    case 4:
                        this.callbackStarted = false;
                        this.callbackSuspended = false;
                        break;
                }
                goto Label_006D;
            }
            if (num2 == 0x10000)
            {
                this.callbackSuspended = true;
                goto Label_006D;
            }
            if (num2 != 0x20000)
            {
                goto Label_006D;
            }
        Label_0046:
            this.callbackStarted = true;
            this.callbackSuspended = false;
        Label_006D:
            base.TrExit(method);
        }

        internal void SetupEventHandler(MQCBD eventDesc, int operation)
        {
            uint method = 0x565;
            this.TrEntry(method, new object[] { eventDesc, operation });
            this.eventDesc = eventDesc;
            this.eventRegistered = (operation & 0x100) != 0;
            this.eventSuspended = (operation & 0x10000) != 0;
            base.TrExit(method);
        }

        internal void StartInit(bool resume)
        {
            uint method = 0x574;
            this.TrEntry(method, new object[] { resume });
            try
            {
                try
                {
                    this.dispatchQMutex.Acquire();
                    try
                    {
                        foreach (MQProxyQueue queue in this.dispatchQueueList)
                        {
                            queue.NoMsgTime = 0L;
                        }
                    }
                    catch (Exception exception)
                    {
                        CommonServices.SetValidInserts();
                        CommonServices.ArithInsert1 = (uint) this.Value;
                        CommonServices.CommentInsert1 = "Exception caught while initializing AsyncConsume.";
                        base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                        NmqiException ex = new NmqiException(base.env, -1, null, 0, 0x893, exception);
                        base.TrException(method, ex);
                        throw ex;
                    }
                }
                finally
                {
                    this.dispatchQMutex.Release();
                }
                this.DriveOutstanding();
                if ((this.acFlags & 0x40) != 0)
                {
                    this.SendConnState(false);
                    this.CheckDispatchable(null);
                    this.CheckTxnAllowed();
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public bool StopPending()
        {
            uint method = 0x588;
            this.TrEntry(method);
            bool result = (this.acFlags & 0x20000) != 0;
            base.TrExit(method, result);
            return result;
        }

        public bool SuspendPending()
        {
            uint method = 0x57f;
            this.TrEntry(method);
            bool result = (this.acFlags & 0x100000) != 0;
            base.TrExit(method, result);
            return result;
        }

        internal void UnsetInTransaction()
        {
            uint method = 0x58b;
            this.TrEntry(method);
            this.acFlags &= -67108865;
            base.TrExit(method);
        }

        public void UnSetQuiescing()
        {
            uint method = 0x57c;
            this.TrEntry(method);
            this.acFlags &= -257;
            base.TrExit(method);
        }

        internal void UnsetStarted()
        {
            uint method = 0x584;
            this.TrEntry(method);
            this.acFlags &= -65;
            base.TrExit(method);
        }

        internal void UnsetSuspended()
        {
            uint method = 0x581;
            this.TrEntry(method);
            this.acFlags &= -129;
            base.TrExit(method);
        }

        internal void UnsetSuspendPending()
        {
            uint method = 0x587;
            this.TrEntry(method);
            this.acFlags &= -1048577;
            base.TrExit(method);
        }

        internal void UnsetTransactionDoomed()
        {
            uint method = 0x591;
            this.TrEntry(method);
            this.acFlags &= -134217729;
            base.TrExit(method);
        }

        internal void ValidateConnectOptionsForXA()
        {
            uint method = 0x4f3;
            this.TrEntry(method);
            try
            {
                if (((this.ConnectionOptions.Options & 0x5000000) != 0) && this.IsReconnectable)
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x7dc, null);
                    base.TrException(method, ex);
                    throw ex;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void WaitForInProgressTx()
        {
            uint method = 0x646;
            this.TrEntry(method);
            try
            {
                if (this.session.AsyncFailure != null)
                {
                    base.TrText(method, "There's an async failure noted on this hconn. Existing now");
                    throw this.session.AsyncFailure;
                }
                int millisecondsTimeout = 10;
                int num3 = 0;
                while (!this.xaResource.rmiTxComplete && !this.xaResource.dtcTxComplete)
                {
                    lock (this.waitForCommitComplete)
                    {
                        Monitor.Wait(this.waitForCommitComplete, millisecondsTimeout, true);
                    }
                    num3 += millisecondsTimeout;
                    if (num3 >= 0xea60)
                    {
                        MQException exception = new MQException(2, 0x932);
                        base.TrText("Previous Tx is still running and a new Tx can't enlisted at this time");
                        throw exception;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void WaitForReconnection()
        {
            uint method = 0x55a;
            this.TrEntry(method);
            lock (this.reconnectLock)
            {
                if (this.reconnectionInProgress)
                {
                    Monitor.Wait(this.reconnectLock);
                }
            }
            base.TrExit(method);
        }

        private bool WaitOnDispatchEvent(int timeout)
        {
            uint method = 0x542;
            this.TrEntry(method, new object[] { timeout });
            bool flag = false;
            try
            {
                lock (this.dispatchEventSync)
                {
                    if (this.dispatchEventPosted)
                    {
                        flag = true;
                        this.dispatchEventPosted = false;
                        return flag;
                    }
                    bool flag2 = false;
                    while (!flag2)
                    {
                        try
                        {
                            if (timeout >= 0)
                            {
                                Monitor.Wait(this.dispatchEventSync, timeout);
                                flag2 = true;
                            }
                            else if (!this.dispatchEventPosted)
                            {
                                Monitor.Wait(this.dispatchEventSync, this.sleepingTimeout);
                            }
                            flag2 = true;
                            if (this.dispatchEventPosted)
                            {
                                flag = true;
                                this.dispatchEventPosted = false;
                                return flag;
                            }
                            continue;
                        }
                        catch (ThreadInterruptedException)
                        {
                            continue;
                        }
                    }
                    return flag;
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return flag;
        }

        public void WakeDispatchThread()
        {
            if (this.dispatchThread != null)
            {
                this.dispatchThread.PostSleepingEvent();
            }
            else
            {
                base.TrText("No dispatch thread has been created for this RemoteHconn so there is nothing to do.");
            }
        }

        private void WakeGetters()
        {
            uint method = 0x56b;
            this.TrEntry(method);
            if (this.proxyQueueManager != null)
            {
                this.proxyQueueManager.WakeGetters();
            }
            base.TrExit(method);
        }

        private void WakeUpAfterReconnection()
        {
            uint method = 0x55c;
            this.TrEntry(method);
            lock (this.reconnectLock)
            {
                Monitor.Pulse(this.reconnectLock);
            }
            base.TrExit(method);
        }

        private void WakeUpAllAfterReconnection()
        {
            uint method = 0x55b;
            this.TrEntry(method);
            lock (this.reconnectLock)
            {
                Monitor.PulseAll(this.reconnectLock);
            }
            base.TrExit(method);
        }

        internal void XAClose()
        {
            uint method = 0x644;
            this.TrEntry(method);
            int num2 = 0;
            try
            {
                if (this.IsXAEnabled)
                {
                    base.TrText(method, "End the RecoveryBridge on this hconn");
                    if (this.xaRecoveryBridge != null)
                    {
                        this.xaRecoveryBridge.Dispose();
                    }
                    base.TrText(method, "Hconn value = " + this.Value + " issuing xa_close");
                    num2 = this.Session.MQFap.XA_Close(this, this.Name, this.rmid, 0);
                    if (num2 != 0)
                    {
                        TransactionException innerException = new TransactionException(NmqiTools.GetTranslatedExceptionMessage(this.ToString(), 0x20008385, (uint) num2));
                        MQException exception2 = new MQException(2, 0x893, innerException);
                        CommonServices.SetValidInserts();
                        CommonServices.ArithInsert1 = (uint) this.rmid;
                        CommonServices.CommentInsert1 = "xa_close call has failed with return code = " + num2;
                        base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009213, 0);
                        throw exception2;
                    }
                }
                else
                {
                    num2 = -6;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public int Ccsid
        {
            get
            {
                return this.session.Ccsid;
            }
        }

        public int CmdLevel
        {
            get
            {
                return this.session.CmdLevel;
            }
        }

        public byte[] ConnectionId
        {
            get
            {
                return this.session.ConnectionId;
            }
            set
            {
            }
        }

        public string ConnectionIdAsString
        {
            get
            {
                return this.session.ConnectionIdAsString;
            }
        }

        public MQConnectOptions ConnectionOptions
        {
            get
            {
                return this.connectOptions;
            }
        }

        public MQDispatchThread DispatchThread
        {
            get
            {
                if (this.dispatchThread == null)
                {
                    this.dispatchThread = new MQDispatchThread(base.env, this.ParentConnection.MQFap, this);
                    Thread thread = new Thread(new ThreadStart(this.dispatchThread.Run));
                    thread.IsBackground = true;
                    thread.Start();
                }
                else
                {
                    this.dispatchThread.HconnActive = true;
                }
                return this.dispatchThread;
            }
            set
            {
                this.dispatchThread = value;
            }
        }

        public int EventsHad
        {
            get
            {
                return this.eventsHad;
            }
        }

        public int FapLevel
        {
            get
            {
                return this.session.FapLevel;
            }
        }

        public MQProxyQueue[] GetDispatchQueueList
        {
            get
            {
                return this.dispatchQueueList.ToArray();
            }
        }

        public MQFAP GetMQFAP
        {
            get
            {
                return this.session.MQFap;
            }
        }

        public int GlobalMessageIndex
        {
            get
            {
                return this.globalMessageIndex;
            }
            set
            {
                this.globalMessageIndex = value;
            }
        }

        internal ArrayList Hobjs
        {
            get
            {
                return this.hobjs;
            }
        }

        internal ArrayList Hsbjs
        {
            get
            {
                return this.hsubs;
            }
        }

        internal MQTagPool IdTagPool
        {
            get
            {
                return this.session.IdTagPool;
            }
        }

        public bool IsDisconnected
        {
            get
            {
                return this.session.IsDisconnected;
            }
        }

        public bool IsEventRegistered
        {
            get
            {
                return this.eventRegistered;
            }
        }

        public bool IsEventSuspended
        {
            get
            {
                return this.eventSuspended;
            }
        }

        public bool IsReconnectable
        {
            get
            {
                return this.reconnectable;
            }
            set
            {
                this.reconnectable = value;
            }
        }

        public bool IsReconnecting
        {
            get
            {
                return this.reconnectionInProgress;
            }
        }

        internal bool IsXAEnabled
        {
            get
            {
                return this.isXaEnabled;
            }
            set
            {
                this.isXaEnabled = value;
            }
        }

        public bool IsXASupported
        {
            get
            {
                return this.ParentConnection.IsXASupported();
            }
        }

        public int LocalUowState
        {
            get
            {
                return this.localUowState;
            }
            set
            {
                this.localUowState = value;
            }
        }

        public string Name
        {
            get
            {
                return this.session.Name;
            }
        }

        public long NextReconnect
        {
            get
            {
                return this.nextReconnect;
            }
            set
            {
                this.nextReconnect = value;
            }
        }

        public NmqiConnectOptions NmqiConnectionOptions
        {
            get
            {
                return this.nmqiConnectOptions;
            }
            set
            {
                this.nmqiConnectOptions = value;
            }
        }

        public int NumberOfSharingConversations
        {
            get
            {
                return this.session.NumberOfSharingConversations;
            }
        }

        internal bool OpenedForXA
        {
            get
            {
                return this.openForXA;
            }
            set
            {
                if (!value)
                {
                    throw new ArgumentException();
                }
                this.openForXA = value;
            }
        }

        public string OriginalQueueManagerName
        {
            get
            {
                return this.session.OriginalQueueManagerName;
            }
        }

        internal ManagedHconn Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                this.parent = value;
            }
        }

        internal MQFAPConnection ParentConnection
        {
            get
            {
                return this.session.ParentConnection;
            }
        }

        internal Phconn ParentPhconn
        {
            get
            {
                return this.parentPhconn;
            }
            set
            {
                this.parentPhconn = value;
            }
        }

        public int Platform
        {
            get
            {
                return this.session.Platform;
            }
        }

        internal MQProxyQueueManager ProxyQueueManager
        {
            get
            {
                return this.proxyQueueManager;
            }
        }

        public string QmName
        {
            get
            {
                return this.qmName;
            }
        }

        public long ReconnectExpiry
        {
            get
            {
                return this.reconnectExpiry;
            }
        }

        public int ReconnectionAttempts
        {
            get
            {
                return this.reconnectAttempts;
            }
            set
            {
                this.reconnectAttempts = value;
            }
        }

        public int ReconnectionFailureCompCode
        {
            get
            {
                return this.reconnectionFailureCompCode;
            }
        }

        public NmqiException ReconnectionFailureException
        {
            get
            {
                return this.reconnectionFailureException;
            }
        }

        public int ReconnectionFailureReason
        {
            get
            {
                return this.reconnectionFailureReason;
            }
        }

        public int Rmid
        {
            get
            {
                return this.rmid;
            }
        }

        internal MQSession Session
        {
            get
            {
                return this.session;
            }
            set
            {
                this.session = value;
                this.session.Hconn = this;
            }
        }

        public int ShareOption
        {
            get
            {
                return this.shareOption;
            }
            set
            {
                this.shareOption = value;
            }
        }

        public int SharingConversations
        {
            get
            {
                return this.session.NumberOfSharingConversations;
            }
        }

        public string Uid
        {
            get
            {
                return this.session.Uid;
            }
        }

        public int Value
        {
            get
            {
                return this.hconnAsInteger;
            }
            set
            {
                if (value == -1)
                {
                    if (this.ConnectionOptions != null)
                    {
                        this.ConnectionOptions.ClientConn = null;
                    }
                    this.connectOptions = null;
                }
                this.hconnAsInteger = value;
            }
        }

        internal MQXARecoveryBridge XARecoveryBridge
        {
            get
            {
                return this.xaRecoveryBridge;
            }
            set
            {
                this.xaRecoveryBridge = value;
            }
        }

        public int XAState
        {
            get
            {
                return this.xaState;
            }
            set
            {
                this.xaState = value;
            }
        }
    }
}

