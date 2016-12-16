namespace IBM.WMQ
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal class MQCommsBufferPool : MQBase
    {
        private const int BUFFER_SIZE_1K = 0x400;
        private const int BUFFER_SIZE_20K = 0x5000;
        private const int BUFFER_SIZE_2K = 0x800;
        private const int BUFFER_SIZE_32K = 0x8000;
        private const int BUFFER_SIZE_5K = 0x1400;
        private IDictionary<int, MQCommsBufferImpl> bufferPool_1k = new Dictionary<int, MQCommsBufferImpl>();
        private IDictionary<int, MQCommsBufferImpl> bufferPool_20k = new Dictionary<int, MQCommsBufferImpl>();
        private IDictionary<int, MQCommsBufferImpl> bufferPool_2k = new Dictionary<int, MQCommsBufferImpl>();
        private IDictionary<int, MQCommsBufferImpl> bufferPool_32k = new Dictionary<int, MQCommsBufferImpl>();
        private IDictionary<int, MQCommsBufferImpl> bufferPool_5k = new Dictionary<int, MQCommsBufferImpl>();
        private volatile int currentAllocation;
        private const int MAX_BUFFER_ALLOCATION = 0x1800000;
        private Random random;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public MQCommsBufferPool()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            lock (this)
            {
                this.random = new Random(this.GetHashCode());
                MQCommsBufferImpl impl = new MQCommsBufferImpl(this, 0x400, this.random.Next());
                impl.Reset(false);
                this.currentAllocation += 0x400;
                MQCommsBufferImpl impl2 = new MQCommsBufferImpl(this, 0x400, this.random.Next());
                impl2.Reset(false);
                this.currentAllocation += 0x400;
                MQCommsBufferImpl impl3 = new MQCommsBufferImpl(this, 0x400, this.random.Next());
                impl3.Reset(false);
                this.currentAllocation += 0x400;
                MQCommsBufferImpl impl4 = new MQCommsBufferImpl(this, 0x400, this.random.Next());
                impl4.Reset(false);
                this.currentAllocation += 0x400;
                MQCommsBufferImpl impl5 = new MQCommsBufferImpl(this, 0x400, this.random.Next());
                impl5.Reset(false);
                this.currentAllocation += 0x400;
                MQCommsBufferImpl impl6 = new MQCommsBufferImpl(this, 0x400, this.random.Next());
                impl6.Reset(false);
                this.currentAllocation += 0x400;
                MQCommsBufferImpl impl7 = new MQCommsBufferImpl(this, 0x400, this.random.Next());
                impl7.Reset(false);
                this.currentAllocation += 0x400;
                MQCommsBufferImpl impl8 = new MQCommsBufferImpl(this, 0x400, this.random.Next());
                impl8.Reset(false);
                this.currentAllocation += 0x400;
                MQCommsBufferImpl impl9 = new MQCommsBufferImpl(this, 0x400, this.random.Next());
                impl9.Reset(false);
                this.currentAllocation += 0x400;
                MQCommsBufferImpl impl10 = new MQCommsBufferImpl(this, 0x400, this.random.Next());
                impl10.Reset(false);
                this.currentAllocation += 0x400;
                MQCommsBufferImpl impl11 = new MQCommsBufferImpl(this, 0x800, this.random.Next());
                impl11.Reset(false);
                this.currentAllocation += 0x800;
                MQCommsBufferImpl impl12 = new MQCommsBufferImpl(this, 0x1400, this.random.Next());
                impl12.Reset(false);
                this.currentAllocation += 0x1400;
                MQCommsBufferImpl impl13 = new MQCommsBufferImpl(this, 0x8000, this.random.Next());
                impl13.Reset(false);
                this.currentAllocation += 0x8000;
                MQCommsBufferImpl impl14 = new MQCommsBufferImpl(this, 0x8000, this.random.Next());
                impl14.Reset(false);
                this.currentAllocation += 0x8000;
                MQCommsBufferImpl impl15 = new MQCommsBufferImpl(this, 0x8000, this.random.Next());
                impl15.Reset(false);
                this.currentAllocation += 0x8000;
                MQCommsBufferImpl impl16 = new MQCommsBufferImpl(this, 0x8000, this.random.Next());
                impl16.Reset(false);
                this.currentAllocation += 0x8000;
                this.bufferPool_1k.Add(impl.HashCode, impl);
                this.bufferPool_1k.Add(impl2.HashCode, impl2);
                this.bufferPool_1k.Add(impl3.HashCode, impl3);
                this.bufferPool_1k.Add(impl4.HashCode, impl4);
                this.bufferPool_1k.Add(impl5.HashCode, impl5);
                this.bufferPool_1k.Add(impl6.HashCode, impl6);
                this.bufferPool_1k.Add(impl7.HashCode, impl7);
                this.bufferPool_1k.Add(impl8.HashCode, impl8);
                this.bufferPool_1k.Add(impl9.HashCode, impl9);
                this.bufferPool_1k.Add(impl10.HashCode, impl10);
                this.bufferPool_2k.Add(impl11.HashCode, impl11);
                this.bufferPool_5k.Add(impl12.HashCode, impl12);
                this.bufferPool_32k.Add(impl13.HashCode, impl13);
                this.bufferPool_32k.Add(impl14.HashCode, impl14);
                this.bufferPool_32k.Add(impl15.HashCode, impl15);
                this.bufferPool_32k.Add(impl16.HashCode, impl16);
            }
        }

        internal IMQCommsBuffer AllocateBuffer(int capacity)
        {
            uint method = 0x6d;
            this.TrEntry(method, new object[] { capacity });
            MQCommsBufferImpl impl = null;
            try
            {
                int num2 = 0;
                IDictionary<int, MQCommsBufferImpl> dictionary = null;
                if (capacity <= 0x400)
                {
                    num2 = 0x400;
                }
                else if (capacity <= 0x800)
                {
                    num2 = 0x800;
                }
                else if (capacity <= 0x1400)
                {
                    num2 = 0x1400;
                }
                else if (capacity <= 0x5000)
                {
                    num2 = 0x5000;
                }
                else
                {
                    num2 = 0x8000;
                }
                switch (num2)
                {
                    case 0x1400:
                        dictionary = this.bufferPool_5k;
                        break;

                    case 0x5000:
                        dictionary = this.bufferPool_20k;
                        break;

                    case 0x8000:
                        dictionary = this.bufferPool_32k;
                        break;

                    case 0x400:
                        dictionary = this.bufferPool_1k;
                        break;

                    case 0x800:
                        dictionary = this.bufferPool_2k;
                        break;

                    default:
                    {
                        MQManagedClientException ex = new MQManagedClientException(string.Concat(new object[] { "AllotedSize=", num2, ", AskedSize=", capacity }), 2, 0x893);
                        base.TrException(method, ex);
                        base.throwNewMQException(2, 0x893);
                        break;
                    }
                }
                lock (dictionary)
                {
                    foreach (MQCommsBufferImpl impl2 in dictionary.Values)
                    {
                        if ((impl2.Capacity >= capacity) && !impl2.InUse)
                        {
                            impl = impl2;
                            base.TrText(method, string.Concat(new object[] { "Reusing buffer of capacity=", impl.Capacity, "BufferHashcode: ", impl.HashCode }));
                            break;
                        }
                    }
                    if (impl == null)
                    {
                        impl = new MQCommsBufferImpl(this, num2, this.random.Next());
                        base.TrText(method, string.Concat(new object[] { "Alloted a new Buffer of capacity=", num2, "Buffer hashcode =", impl.HashCode }));
                        if (this.currentAllocation < 0x1800000)
                        {
                            try
                            {
                                base.TrText(method, "Current Allocation size = " + ((int) this.currentAllocation));
                                this.currentAllocation += num2;
                                dictionary.Add(impl.HashCode, impl);
                                goto Label_0283;
                            }
                            catch (Exception exception2)
                            {
                                base.TrException(method, exception2);
                                throw exception2;
                            }
                        }
                        base.TrText(method, "Threshold buffer allocations reached, now no more new buffers to pool");
                    }
                Label_0283:
                    impl.Reset(true);
                    impl.DataAvailable = 0;
                    impl.DataUsed = 0;
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return impl;
        }

        internal void FreeBuffer(IMQCommsBuffer iBuffer)
        {
            uint method = 0x6c;
            this.TrEntry(method, new object[] { iBuffer });
            try
            {
                MQCommsBufferImpl impl = (MQCommsBufferImpl) iBuffer;
                IDictionary<int, MQCommsBufferImpl> dictionary = null;
                impl.CheckPool(this);
                impl.IsValid();
                switch (iBuffer.Capacity)
                {
                    case 0x1400:
                        if (this.bufferPool_5k.ContainsKey(impl.HashCode))
                        {
                            dictionary = this.bufferPool_5k;
                        }
                        break;

                    case 0x5000:
                        if (this.bufferPool_20k.ContainsKey(impl.HashCode))
                        {
                            dictionary = this.bufferPool_20k;
                        }
                        break;

                    case 0x8000:
                        if (this.bufferPool_32k.ContainsKey(impl.HashCode))
                        {
                            dictionary = this.bufferPool_32k;
                        }
                        break;

                    case 0x400:
                        if (this.bufferPool_1k.ContainsKey(impl.HashCode))
                        {
                            dictionary = this.bufferPool_1k;
                        }
                        break;

                    case 0x800:
                        if (this.bufferPool_2k.ContainsKey(impl.HashCode))
                        {
                            dictionary = this.bufferPool_2k;
                        }
                        break;

                    default:
                    {
                        MQManagedClientException ex = new MQManagedClientException("BufferId=" + impl.HashCode, 2, 0x893);
                        base.TrException(method, ex);
                        base.throwNewMQException(2, 0x893);
                        break;
                    }
                }
                if (dictionary != null)
                {
                    lock (dictionary)
                    {
                        impl.UseCount--;
                        if (impl.UseCount == 0)
                        {
                            base.TrText(method, "UseCount on this buffer is found to be 0,Reseting this buffer of capacity " + impl.Capacity);
                            impl.Reset(false);
                        }
                        return;
                    }
                }
                lock (this)
                {
                    impl.UseCount--;
                    if (impl.UseCount == 0)
                    {
                        impl.Dispose(false);
                        impl = null;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }
    }
}

