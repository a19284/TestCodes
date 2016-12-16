namespace IBM.WMQ
{
    using System;

    internal class MQCommsBufferImpl : MQBase, IMQCommsBuffer, IDisposable
    {
        private byte[] buffer;
        private int bufferCapacity;
        private readonly object bufferLock = new object();
        private int dataAvailable;
        private int dataPosition;
        private int dataUsed;
        private readonly int hashcode;
        private bool inUse;
        private MQCommsBufferPool pool;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private int useCount;

        public MQCommsBufferImpl(MQCommsBufferPool pool, int capacity, int hash)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { pool, capacity });
            this.pool = pool;
            this.buffer = new byte[capacity];
            this.bufferCapacity = capacity;
            this.hashcode = hash;
        }

        public void AddUseCount(int usecount)
        {
            uint method = 0x68;
            this.TrEntry(method, new object[] { usecount });
            try
            {
                if (!this.inUse)
                {
                    this.ThrowBufferException();
                }
                lock (this.bufferLock)
                {
                    this.useCount += usecount;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void CheckPool(MQCommsBufferPool currentPool)
        {
            uint method = 0x4ca;
            this.TrEntry(method, new object[] { currentPool });
            try
            {
                if (!this.pool.Equals(currentPool))
                {
                    this.ThrowBufferException();
                }
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
            this.buffer = null;
            if (!fromDispose)
            {
                GC.SuppressFinalize(this);
            }
        }

        public void Free()
        {
            uint method = 0x69;
            this.TrEntry(method);
            try
            {
                if (!this.inUse)
                {
                    this.ThrowBufferException();
                }
                this.pool.FreeBuffer(this);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void IsValid()
        {
            uint method = 0x4cc;
            this.TrEntry(method);
            try
            {
                if (!this.inUse || (this.useCount <= 0))
                {
                    this.ThrowBufferException();
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void Reset(bool inUse)
        {
            uint method = 0x6a;
            this.TrEntry(method, new object[] { inUse });
            if (inUse)
            {
                this.useCount = 1;
                this.inUse = true;
            }
            else
            {
                this.useCount = 0;
                this.inUse = false;
                this.dataAvailable = 0;
                this.dataUsed = 0;
            }
            base.TrExit(method);
        }

        private void ThrowBufferException()
        {
            uint method = 0x4cb;
            this.TrEntry(method);
            try
            {
                base.throwNewMQException(2, 0x893);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public byte[] Buffer
        {
            get
            {
                if (!this.inUse)
                {
                    this.ThrowBufferException();
                }
                return this.buffer;
            }
            set
            {
                this.buffer = value;
            }
        }

        public int Capacity
        {
            get
            {
                return this.bufferCapacity;
            }
        }

        public int DataAvailable
        {
            get
            {
                return this.dataAvailable;
            }
            set
            {
                this.dataAvailable = value;
            }
        }

        public int DataPosition
        {
            get
            {
                return this.dataPosition;
            }
            set
            {
                this.dataPosition = value;
            }
        }

        public int DataUsed
        {
            get
            {
                return this.dataUsed;
            }
            set
            {
                this.dataUsed = value;
            }
        }

        internal int HashCode
        {
            get
            {
                return this.hashcode;
            }
        }

        public bool InUse
        {
            get
            {
                return this.inUse;
            }
        }

        public int UseCount
        {
            get
            {
                return this.useCount;
            }
            set
            {
                if (!this.inUse)
                {
                    this.ThrowBufferException();
                }
                lock (this.bufferLock)
                {
                    this.useCount = value;
                }
            }
        }
    }
}

