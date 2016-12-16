namespace IBM.WMQ.Nmqi
{
    using System;

    public class Guard : IDisposable
    {
        private Lock lockObj_;

        public Guard(Lock lockObj)
        {
            if (lockObj == null)
            {
                throw new ArgumentNullException("lockObj");
            }
            this.lockObj_ = lockObj;
            this.lockObj_.Acquire();
        }

        public void Dispose()
        {
            this.lockObj_.Release();
        }
    }
}

