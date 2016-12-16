namespace IBM.WMQ.Nmqi
{
    using System;
    using System.Threading;

    public class Lock
    {
        private object apiCallWait = new object();
        private const int DEFAULT_TIMEOUT = 200;
        private int lockCounter;
        private Thread owningThread_;
        private object wait = new object();
        private readonly object waitForPulse = new object();

        public void Acquire()
        {
            this.Acquire(-1);
        }

        public bool Acquire(int timeout)
        {
            return this.Acquire(timeout, 0);
        }

        public bool Acquire(int timeout, int depth)
        {
            bool flag;
            if (this.owningThread_ == Thread.CurrentThread)
            {
                this.lockCounter++;
                return true;
            }
            if (this.owningThread_ == null)
            {
                this.AcquireLock();
                return true;
            }
            if (timeout >= 0)
            {
                try
                {
                    lock (this.wait)
                    {
                        Monitor.Wait(this.wait, (int) (timeout * 0x3e8), true);
                    }
                    if (this.owningThread_ == null)
                    {
                        this.AcquireLock();
                        return true;
                    }
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            try
            {
            Label_0076:
                lock (this.wait)
                {
                    while (this.owningThread_ != null)
                    {
                        Monitor.Wait(this.wait, 200, false);
                    }
                }
                if (this.owningThread_ == null)
                {
                    this.AcquireLock();
                    flag = true;
                }
                else
                {
                    goto Label_0076;
                }
            }
            catch (Exception)
            {
                flag = false;
            }
            return flag;
        }

        private void AcquireLock()
        {
            Monitor.Enter(this.apiCallWait);
            this.owningThread_ = Thread.CurrentThread;
        }

        public void AssertOnCurrentThreadHoldsLock()
        {
            if (this.OwningThread != Thread.CurrentThread)
            {
                throw new SynchronizationLockException("owningTread_ != Thread.CurrentThread, CurrentThreadId - " + Thread.CurrentThread.ManagedThreadId);
            }
        }

        public void ForceClear()
        {
            try
            {
                this.FullyRelease();
            }
            catch (Exception)
            {
            }
            this.lockCounter = 0;
            this.owningThread_ = null;
        }

        public int FullyRelease()
        {
            int lockCounter = 0;
            if (this.owningThread_ == Thread.CurrentThread)
            {
                lockCounter = this.lockCounter;
                if (this.lockCounter >= 0)
                {
                    this.lockCounter = 0;
                    this.Release();
                }
            }
            return lockCounter;
        }

        public void Pulse()
        {
            lock (this.wait)
            {
                Monitor.Pulse(this.wait);
            }
        }

        public void PulseAll()
        {
            lock (this.wait)
            {
                Monitor.PulseAll(this.wait);
            }
        }

        public void Release()
        {
            if (this.owningThread_ != null)
            {
                if (this.owningThread_ != Thread.CurrentThread)
                {
                    throw new SynchronizationLockException();
                }
                if (this.lockCounter > 0)
                {
                    this.lockCounter--;
                }
                else
                {
                    this.owningThread_ = null;
                    Monitor.Exit(this.apiCallWait);
                    lock (this.wait)
                    {
                        Monitor.PulseAll(this.wait);
                    }
                }
            }
        }

        public void Wait()
        {
            this.Wait(-1);
        }

        public void Wait(int timeout)
        {
            this.Release();
            if (timeout == -1)
            {
                timeout = 100;
            }
            lock (this.wait)
            {
                while (this.owningThread_ != null)
                {
                    Monitor.Wait(this.wait, 100, true);
                }
            }
            this.Acquire();
        }

        public bool CurrentThreadHoldsLock
        {
            get
            {
                return (this.OwningThread == Thread.CurrentThread);
            }
        }

        public Thread OwningThread
        {
            get
            {
                return this.owningThread_;
            }
        }
    }
}

