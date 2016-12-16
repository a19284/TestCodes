namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class MQDispatchThread : NmqiObject
    {
        private int dispatchSeq;
        private const int dispatchThreadIdleTimeout = 0x493e0;
        private const int dispatchThreadSleepTimeout = 0x7d0;
        private bool exitThread;
        private MQFAP fap;
        private ManagedHconn hconn;
        private bool hconnActive;
        private int noMsgsWait;
        private long now;
        private int processed;
        public const int rpqTS_AFFINITY = 4;
        public const int rpqTS_SLEEPING = 1;
        public const int rpqTS_START_WAIT = 2;
        private int savedDispatchSeq;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private volatile bool sleepingEventPosted;
        private Lock sleepingEventSync;
        private int status;
        private string threadName;
        private bool waitQ;

        public MQDispatchThread(NmqiEnvironment nmqiEnv, MQFAP fap, ManagedHconn hconn) : base(nmqiEnv)
        {
            this.sleepingEventSync = new Lock();
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv, fap, hconn });
            this.fap = fap;
            this.hconn = hconn;
            this.hconnActive = true;
        }

        private void DeliverMsgs(MQProxyQueue pq)
        {
            pq.DeliverMsgs(ref this.processed);
            if (this.waitQ)
            {
                pq.NoMsgTime = this.now + pq.MqcbGmo.WaitInterval;
                if ((this.noMsgsWait > pq.MqcbGmo.WaitInterval) || (this.noMsgsWait == 0))
                {
                    this.noMsgsWait = pq.MqcbGmo.WaitInterval;
                }
            }
        }

        public void IncrementDispatchSeq()
        {
            this.dispatchSeq++;
        }

        public void PostSleepingEvent()
        {
            uint method = 0x3b7;
            this.TrEntry(method);
            try
            {
                lock (this.sleepingEventSync)
                {
                    this.sleepingEventPosted = true;
                    Monitor.Pulse(this.sleepingEventSync);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void ProcessHconn()
        {
            if (this.hconn.SuspendPending())
            {
                this.hconn.DoSuspend();
            }
            if (this.hconn.HasEventsOutstanding())
            {
                this.hconn.DriveEventsEH();
            }
            if (this.hconn.StopPending())
            {
                this.hconn.DoStop();
                this.hconnActive = false;
            }
        }

        public void Run()
        {
            uint method = 0x3b9;
            this.TrEntry(method);
            this.threadName = "DispatchThread: [" + this.hconn + "]";
            Thread.CurrentThread.Name = this.threadName;
            base.TrText(method, "Dispatcher's name = " + this.threadName);
            Thread.SetData(Thread.GetNamedDataSlot("MQ_CLIENT_THREAD_TYPE"), 2);
            int num2 = 0;
            int num3 = 0;
            try
            {
                this.hconn.RequestThreadLock();
                this.hconn.ReleaseThreadLock();
                while (!this.exitThread)
                {
                    num2 = 0;
                    this.savedDispatchSeq = this.dispatchSeq;
                    this.noMsgsWait = 0;
                    if (this.hconnActive)
                    {
                        this.hconn.RequestDispatchLock(-1);
                        try
                        {
                            this.ProcessHconn();
                            if ((this.hconnActive && this.hconn.IsStarted()) && !this.hconn.IsSuspended())
                            {
                                this.processed = 0;
                                num3 = 0;
                                MQProxyQueue[] getDispatchQueueList = this.hconn.GetDispatchQueueList;
                                for (int i = 0; i < getDispatchQueueList.Length; i++)
                                {
                                    MQProxyQueue pq = getDispatchQueueList[i];
                                    this.waitQ = (pq.MqcbGmo.WaitInterval > 0) && ((pq.MqcbGmo.Options & 1) != 0);
                                    if (this.waitQ)
                                    {
                                        this.now = DateTime.Now.Millisecond;
                                    }
                                    IntPtr callbackFunction = pq.MqcbCBD.CallbackFunction;
                                    if ((pq.Status & 0x40000) == 0)
                                    {
                                        if (pq.EventsRaised != this.hconn.EventsHad)
                                        {
                                            pq.DriveEventsMC();
                                        }
                                        if (!pq.IsEmpty())
                                        {
                                            this.DeliverMsgs(pq);
                                        }
                                        else if (this.ShouldDriveNoMsgs(pq) || pq.CallbackOnEmpty())
                                        {
                                            pq.CallConsumer(5, 2, 0x7f1);
                                            pq.UnsetCallbackOnEmpty();
                                        }
                                        if (this.hconn.ConsumersChanged())
                                        {
                                            this.hconn.DriveOutstanding();
                                        }
                                        if ((pq.Status & 0x80) != 0)
                                        {
                                            if (this.processed != 0)
                                            {
                                                num3++;
                                            }
                                        }
                                        else
                                        {
                                            num3++;
                                        }
                                        if ((pq.Status & 0x80000) != 0)
                                        {
                                            this.hconn.RemoveFromDispatchList(pq);
                                            this.hconn.ProxyQueueManager.DeleteProxyQueue(pq);
                                        }
                                    }
                                    num2 += this.processed;
                                    getDispatchQueueList = this.hconn.GetDispatchQueueList;
                                }
                            }
                        }
                        finally
                        {
                            if (this.hconn != null)
                            {
                                try
                                {
                                    this.hconn.ReleaseDispatchLock();
                                }
                                catch (SynchronizationLockException)
                                {
                                }
                            }
                        }
                        Thread.Sleep(0);
                    }
                    if ((num2 == 0) && (this.savedDispatchSeq == this.dispatchSeq))
                    {
                        this.hconn.RequestThreadLock();
                        try
                        {
                            if (this.savedDispatchSeq != this.dispatchSeq)
                            {
                                continue;
                            }
                            this.status |= 1;
                        }
                        finally
                        {
                            this.hconn.ReleaseThreadLock();
                        }
                        this.SleepPhase();
                    }
                }
            }
            catch (Exception exception)
            {
                NmqiException ex = null;
                if ((exception is NmqiException) || (exception is MQException))
                {
                    if (exception is MQException)
                    {
                        ex = new NmqiException(base.env, -1, null, ((MQException) exception).CompCode, ((MQException) exception).Reason, exception);
                    }
                    else
                    {
                        ex = (NmqiException) exception;
                    }
                    if (ex.Reason == 0x893)
                    {
                        CommonServices.SetValidInserts();
                        CommonServices.ArithInsert1 = 1;
                        CommonServices.ArithInsert2 = (uint) this.status;
                        CommonServices.CommentInsert1 = exception.Message;
                        CommonServices.CommentInsert2 = exception.StackTrace;
                        base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009546, 0);
                    }
                    base.TrException(method, ex);
                    throw ex;
                }
                CommonServices.SetValidInserts();
                CommonServices.ArithInsert1 = 1;
                CommonServices.ArithInsert2 = (uint) this.status;
                CommonServices.CommentInsert1 = exception.Message;
                CommonServices.CommentInsert2 = exception.StackTrace;
                base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009546, 0);
                base.TrException(method, exception);
                throw exception;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private bool ShouldDriveNoMsgs(MQProxyQueue pq)
        {
            bool flag = false;
            if (this.waitQ)
            {
                long noMsgTime = pq.NoMsgTime;
                int waitInterval = pq.MqcbGmo.WaitInterval;
                if (noMsgTime == 0L)
                {
                    pq.NoMsgTime = this.now + waitInterval;
                }
                long num3 = noMsgTime - this.now;
                if (num3 <= 0L)
                {
                    flag = true;
                    pq.NoMsgTime = this.now + waitInterval;
                    if ((this.noMsgsWait > waitInterval) || (this.noMsgsWait == 0))
                    {
                        this.noMsgsWait = waitInterval;
                    }
                    return flag;
                }
                if ((this.noMsgsWait <= num3) && (this.noMsgsWait != 0))
                {
                    return flag;
                }
                this.noMsgsWait = (int) num3;
            }
            return flag;
        }

        private void SleepPhase()
        {
            bool flag;
        Label_0000:
            if (this.noMsgsWait != 0)
            {
                flag = this.WaitOnSleepingEvent(this.noMsgsWait);
            }
            else if (this.hconnActive)
            {
                flag = this.WaitOnSleepingEvent(0x7d0);
            }
            else
            {
                flag = this.WaitOnSleepingEvent(0x493e0);
            }
            if (!flag && (this.noMsgsWait == 0))
            {
                this.hconn.RequestThreadLock();
                try
                {
                    if (this.savedDispatchSeq == this.dispatchSeq)
                    {
                        if (!this.hconnActive)
                        {
                            this.hconn.DispatchThread = null;
                            this.exitThread = true;
                        }
                        else
                        {
                            if (!this.hconn.SuspendPending() && !this.hconn.StopPending())
                            {
                                this.hconnActive = false;
                            }
                            goto Label_0000;
                        }
                    }
                }
                finally
                {
                    this.hconn.ReleaseThreadLock();
                }
            }
        }

        private bool WaitOnSleepingEvent(int timeout)
        {
            uint method = 0x3b8;
            this.TrEntry(method, new object[] { timeout });
            bool flag = false;
            int millisecondsTimeout = timeout;
            try
            {
                lock (this.sleepingEventSync)
                {
                    if (this.sleepingEventPosted)
                    {
                        flag = true;
                        this.sleepingEventPosted = false;
                        return flag;
                    }
                    for (bool flag2 = false; !flag2; flag2 = true)
                    {
                        if (millisecondsTimeout >= 0)
                        {
                            if (!this.sleepingEventPosted)
                            {
                                Monitor.Wait(this.sleepingEventSync, millisecondsTimeout, true);
                            }
                        }
                        else if (!this.sleepingEventPosted)
                        {
                            Monitor.Wait(this.sleepingEventSync);
                        }
                    }
                    if (this.sleepingEventPosted)
                    {
                        flag = true;
                        this.sleepingEventPosted = false;
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

        public bool HconnActive
        {
            get
            {
                return this.hconnActive;
            }
            set
            {
                this.hconnActive = true;
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

