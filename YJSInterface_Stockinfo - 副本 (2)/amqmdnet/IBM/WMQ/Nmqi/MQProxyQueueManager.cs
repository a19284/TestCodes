namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Collections.Generic;

    internal class MQProxyQueueManager : NmqiObject
    {
        internal MQCommsBufferPool commsBufferPool;
        private Lock deletePQueue;
        private Lock findingPQueue;
        private ManagedHconn hconn;
        private Dictionary<int, MQProxyQueue> proxyQueues;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private Lock synchornize;

        public MQProxyQueueManager(NmqiEnvironment env, ManagedHconn hconn) : base(env)
        {
            this.proxyQueues = new Dictionary<int, MQProxyQueue>();
            this.findingPQueue = new Lock();
            this.deletePQueue = new Lock();
            this.synchornize = new Lock();
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { env, hconn });
            this.hconn = hconn;
        }

        internal void AddMessage(MQTSH tsh)
        {
            uint method = 0x607;
            this.TrEntry(method, new object[] { tsh });
            try
            {
                MQASYNC_MESSAGE async = new MQASYNC_MESSAGE();
                tsh.Offset = async.ReadStruct(tsh.TshBuffer, tsh.Offset);
                base.TrData(method, 0, "Hobj to be removed", BitConverter.GetBytes(async.asyncMsg.hObj));
                MQProxyQueue queue = this.FindProxyQueue(async.asyncMsg.hObj);
                if (queue == null)
                {
                    NmqiException exception = new NmqiException(base.env, -1, null, 2, 0x893, null);
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = 1;
                    CommonServices.CommentInsert1 = "Unable to find proxy queue for Hobj";
                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009546, 0);
                    throw exception;
                }
                queue.AddMessage(tsh, async);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public bool CheckClientEmpty()
        {
            uint method = 0x609;
            this.TrEntry(method);
            bool result = false;
            foreach (object obj2 in this.proxyQueues.Values)
            {
                MQProxyQueue queue = (MQProxyQueue) obj2;
                if (((queue != null) && queue.IsCbAlreadyRegistered) && ((queue.Status & 0x40000) == 0))
                {
                    if (!queue.IsEmpty())
                    {
                        base.TrExit(method, false, 1);
                        return false;
                    }
                    if ((queue.Status & 0x10000000) != 0)
                    {
                        result = true;
                    }
                }
            }
            base.TrExit(method, result, 2);
            return result;
        }

        public void CheckTxnMessage()
        {
            uint method = 0x60c;
            this.TrEntry(method);
            try
            {
                foreach (MQProxyQueue queue in this.proxyQueues.Values)
                {
                    queue.CheckTxnMessage();
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public MQProxyQueue CreateProxyQueue()
        {
            uint method = 0x603;
            this.TrEntry(method);
            MQProxyQueue result = null;
            try
            {
                result = new MQProxyQueue(base.env, this.hconn);
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        public void DeleteProxyQueue(MQProxyQueue proxyQueue)
        {
            uint method = 0x606;
            this.TrEntry(method, new object[] { proxyQueue });
            try
            {
                this.deletePQueue.Acquire();
                ManagedHobj manHobj = proxyQueue.ManHobj;
                if (manHobj != null)
                {
                    int handle = manHobj.Handle;
                    base.TrData(method, 0, "Hobj to be removed", BitConverter.GetBytes(handle));
                    MQProxyQueue queue = this.proxyQueues[handle];
                    if (queue != proxyQueue)
                    {
                        CommonServices.SetValidInserts();
                        CommonServices.ArithInsert1 = 1;
                        CommonServices.CommentInsert1 = "Couldn't find the ProxyQueue..";
                        base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009546, 0);
                        NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x893, null);
                        base.TrException(method, ex);
                        throw ex;
                    }
                    this.proxyQueues.Remove(handle);
                }
            }
            finally
            {
                this.deletePQueue.Release();
                base.TrExit(method);
            }
        }

        private MQProxyQueue FindProxyQueue(int clientId)
        {
            uint method = 0x604;
            this.TrEntry(method, new object[] { clientId });
            MQProxyQueue queue = null;
            try
            {
                this.findingPQueue.Acquire();
                try
                {
                    queue = this.proxyQueues[clientId];
                }
                catch (KeyNotFoundException)
                {
                }
                if (queue == null)
                {
                    NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x893, null);
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = 1;
                    CommonServices.CommentInsert1 = this.hconn.Session.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                    CommonServices.CommentInsert2 = "No proxy queue with ClientId ";
                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009546, 0);
                    base.TrException(method, ex);
                    throw ex;
                }
            }
            finally
            {
                this.findingPQueue.Release();
                base.TrExit(method);
            }
            return queue;
        }

        public void NotifyConnectionFailure(Exception asyncFailure)
        {
            uint method = 0x60a;
            this.TrEntry(method, new object[] { asyncFailure });
            try
            {
                foreach (MQProxyQueue queue in this.proxyQueues.Values)
                {
                    queue.NotifyConnectionFailure(asyncFailure);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void RaiseEvent(int reason)
        {
            uint method = 0x60d;
            this.TrEntry(method, new object[] { reason });
            try
            {
                foreach (MQProxyQueue queue in this.proxyQueues.Values)
                {
                    queue.RaiseEvent(reason);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void ReceiveNotification(MQTSH tsh)
        {
            uint method = 0x608;
            this.TrEntry(method, new object[] { tsh });
            try
            {
                MQNOTIFICATION notification = new MQNOTIFICATION();
                notification.ReadStruct(tsh.TshBuffer, tsh.Offset);
                MQProxyQueue queue = this.FindProxyQueue(notification.notify.hObj);
                if (queue == null)
                {
                    NmqiException exception = new NmqiException(base.env, -1, null, 2, 0x893, null);
                    CommonServices.SetValidInserts();
                    CommonServices.ArithInsert1 = 1;
                    CommonServices.CommentInsert1 = "Unable to find proxy queue for Hobj";
                    base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009546, 0);
                    throw exception;
                }
                queue.ReceiveNotification(notification);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetIdentifier(MQProxyQueue proxyQueue, ManagedHobj hobj, int opOpts, int defRA)
        {
            uint method = 0x605;
            this.TrEntry(method, new object[] { proxyQueue, hobj.Value, opOpts, defRA });
            try
            {
                this.synchornize.Acquire();
                proxyQueue.SetIdentifier(hobj, opOpts, defRA);
                if (this.proxyQueues.ContainsKey(hobj.Handle))
                {
                    this.proxyQueues[hobj.Handle] = proxyQueue;
                }
                else
                {
                    this.proxyQueues.Add(hobj.Handle, proxyQueue);
                }
            }
            finally
            {
                this.synchornize.Release();
                base.TrExit(method);
            }
        }

        public void WakeGetters()
        {
            uint method = 0x60b;
            this.TrEntry(method);
            try
            {
                foreach (MQProxyQueue queue in this.proxyQueues.Values)
                {
                    queue.ReleaseWaitingGetters();
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }
    }
}

