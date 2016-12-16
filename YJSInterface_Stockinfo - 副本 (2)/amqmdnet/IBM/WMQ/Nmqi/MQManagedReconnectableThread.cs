namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Collections;
    using System.Threading;

    public class MQManagedReconnectableThread : NmqiObject
    {
        private int[] defaultDelayTimes;
        private MQFAP fap;
        private ArrayList hconns;
        private static Random rand = new Random();
        private int[] rcnTimes;
        private object reconnectMutex;
        public const string sccsid = "%Z% %W%  %I% %E% %U%";

        public MQManagedReconnectableThread(NmqiEnvironment env, MQFAP fap) : base(env)
        {
            this.defaultDelayTimes = new int[] { 0x3e8, 0x7d0, 0xfa0, 0x1f40, 0x3e80, 0x61a8 };
            this.reconnectMutex = new object();
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { env, fap });
            this.hconns = new ArrayList();
            this.fap = fap;
            this.BuildReconnectionTimes();
        }

        public ManagedHconn BestHconn()
        {
            uint method = 0x4c2;
            this.TrEntry(method);
            ManagedHconn hconn = null;
            try
            {
                while (hconn == null)
                {
                    long num2 = 0L;
                    long num3 = 0L;
                    lock (this.reconnectMutex)
                    {
                        if (this.hconns.Count == 0)
                        {
                            try
                            {
                                Monitor.Wait(this.reconnectMutex, -1, true);
                            }
                            catch (ThreadInterruptedException exception)
                            {
                                base.TrException(method, exception, 1);
                            }
                        }
                        long currentTimeInMs = NmqiTools.GetCurrentTimeInMs();
                        for (int i = 0; i < this.hconns.Count; i++)
                        {
                            ManagedHconn rcnHconn = (ManagedHconn) this.hconns[i];
                            if (!rcnHconn.IsReconnectable)
                            {
                                base.TrText(method, "Hconn : " + rcnHconn.Value + " is not elgiblefor reconnection any longer..");
                                rcnHconn.SetReconnectionFailure(2, 0x7d9, null);
                                this.ReleaseHconn(rcnHconn);
                            }
                            else if (rcnHconn.ReconnectExpiry < currentTimeInMs)
                            {
                                base.TrText(method, "Hconn : " + rcnHconn.Value + " has reconnection time expired..");
                                rcnHconn.SetReconnectionFailure(2, 0x9fc, null);
                                this.ReleaseHconn(rcnHconn);
                            }
                            else
                            {
                                long num6 = currentTimeInMs - rcnHconn.NextReconnect;
                                if (num2 <= num6)
                                {
                                    num2 = num6;
                                    hconn = rcnHconn;
                                }
                                else if (num3 < -num6)
                                {
                                    num3 = -num6;
                                }
                            }
                        }
                    }
                    if (hconn == null)
                    {
                        try
                        {
                            base.TrText(method, "Reconnection thread sleeping now for until iteration");
                            Thread.Sleep(Convert.ToInt32(num3));
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
                base.TrExit(method);
            }
            return hconn;
        }

        public void BuildReconnectionTimes()
        {
            uint method = 0x4c6;
            this.TrEntry(method);
            string stringValue = base.env.Cfg.GetStringValue(MQClientCfg.CHANNEL_RECONDELAY);
            if (stringValue == null)
            {
                this.rcnTimes = new int[this.defaultDelayTimes.Length];
                for (int i = 0; i < this.defaultDelayTimes.Length; i++)
                {
                    int num3 = rand.Next((i + 1) * 250);
                    this.rcnTimes[i] = this.defaultDelayTimes[i] + num3;
                }
            }
            else
            {
                string[] strArray = stringValue.Split(new char[] { '[', ']' });
                ArrayList list = new ArrayList(strArray.Length);
                foreach (string str2 in strArray)
                {
                    if (str2.Length != 0)
                    {
                        string[] strArray2 = str2.Split(new char[] { ',' });
                        int num4 = int.Parse(strArray2[0]);
                        int num5 = (strArray2.Length > 1) ? int.Parse(strArray2[1]) : 0;
                        list.Add(num4 + num5);
                    }
                    this.rcnTimes = new int[list.Count];
                    int num6 = 0;
                    foreach (int num7 in list)
                    {
                        this.rcnTimes[num6++] = num7;
                    }
                }
            }
            base.TrExit(method);
        }

        internal bool EligibleForReconnect(ManagedHconn remoteHconn, MQSession remoteSession, bool register)
        {
            uint method = 0x4eb;
            this.TrEntry(method, new object[] { remoteHconn, remoteSession, register });
            bool result = true;
            try
            {
                lock (this.reconnectMutex)
                {
                    if (register)
                    {
                        if (remoteHconn.IsReconnectable && !remoteHconn.HasFailed())
                        {
                            MQSession session = remoteHconn.Session;
                            if ((remoteSession == session) && remoteHconn.InitializeForReconnect())
                            {
                                this.hconns.Add(remoteHconn);
                                Monitor.PulseAll(this.reconnectMutex);
                            }
                            return result;
                        }
                        return false;
                    }
                    remoteHconn.IsReconnectable = false;
                    return result;
                }
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        public bool Reconnect(ManagedHconn remoteHconn)
        {
            uint method = 0x4c3;
            this.TrEntry(method, new object[] { remoteHconn });
            int compCode = 0;
            int reason = 0;
            try
            {
                Phconn pHconn = base.env.NewPhconn();
                remoteHconn.UnsetStarted();
                remoteHconn.GlobalMessageIndex = 0;
                base.TrText(method, string.Concat(new object[] { "Hconn : ", remoteHconn.Value, " ReconnectionID : ", remoteHconn.ConnectionId }));
                remoteHconn.NmqiConnectionOptions.ReconnectionID = remoteHconn.ConnectionId;
                try
                {
                    remoteHconn.DriveEventsEH(0, 0x9f0);
                }
                catch (Exception exception)
                {
                    base.TrException(method, exception);
                }
                try
                {
                    if (remoteHconn.NmqiConnectionOptions.RemoteQmidAsString == null)
                    {
                        remoteHconn.NmqiConnectionOptions.RemoteQmidAsString = remoteHconn.Uid;
                    }
                    base.TrText(method, string.Concat(new object[] { "Hconn : ", remoteHconn.Value, " RemoteQmidAsString : ", remoteHconn.NmqiConnectionOptions.RemoteQmidAsString }));
                }
                catch (NmqiException exception2)
                {
                    base.TrException(method, exception2, 1);
                    remoteHconn.SetReconnectionFailure(2, 0x9f4, exception2);
                    return false;
                }
                string name = null;
                try
                {
                    name = remoteHconn.OriginalQueueManagerName;
                    base.TrText(method, string.Concat(new object[] { "Hconn : ", remoteHconn.Value, " OriginalQueueManagerName : ", name }));
                }
                catch (NmqiException exception3)
                {
                    base.TrException(method, exception3, 2);
                }
                MQFAP getMQFAP = remoteHconn.GetMQFAP;
                try
                {
                    getMQFAP.NmqiConnect(name, remoteHconn.NmqiConnectionOptions, remoteHconn.ConnectionOptions, remoteHconn.Parent, pHconn, out compCode, out reason, remoteHconn);
                    base.TrText(method, string.Concat(new object[] { "NmqiConnect during reconnect completed with CompCode = ", compCode, " Reason = ", reason }));
                    if (compCode != 0)
                    {
                        return false;
                    }
                }
                catch (MQException exception4)
                {
                    base.TrException(method, exception4, 3);
                    if ((exception4.CompCode != 0) && (exception4.Reason == 0x9f2))
                    {
                        remoteHconn.SetReconnectionFailure(2, 0x9f2, null);
                    }
                    return false;
                }
                catch (Exception exception5)
                {
                    base.TrException(method, exception5, 4);
                    return false;
                }
                if (remoteHconn.InTransaction())
                {
                    remoteHconn.SetTransactionDoomed();
                }
                remoteHconn.UnSetQuiescing();
                foreach (ManagedHobj hobj in remoteHconn.Hobjs)
                {
                    if (hobj.ParentHsub == null)
                    {
                        Phobj manHobj = base.env.NewPhobj();
                        MQObjectDescriptor mqod = hobj.Mqod;
                        Hobj hOBJ = manHobj.HOBJ;
                        SpiOpenOptions spiOpenOpts = hobj.SpiOpenOpts;
                        base.TrText(method, "Reopening " + mqod.ObjectName);
                        base.TrText(method, "Original name " + hobj.OriginalObjectName);
                        getMQFAP.nmqiOpen(remoteHconn, ref mqod, ref spiOpenOpts, hobj.OpenOptions, ref manHobj, out compCode, out reason, hobj.SpiCall, hobj);
                        base.TrText(method, string.Concat(new object[] { "MQOPEN during reconnect completed with CompCode = ", compCode, " Reason = ", reason }));
                        if (((reason == 0x825) || (reason == 0x827)) && !hobj.OriginalObjectName.Equals(mqod.ObjectName))
                        {
                            string objectName = mqod.ObjectName;
                            string dynamicQName = mqod.DynamicQName;
                            compCode = 0;
                            reason = 0;
                            mqod.ObjectName = hobj.OriginalObjectName;
                            mqod.DynamicQName = objectName;
                            getMQFAP.MQOPEN(remoteHconn, ref mqod, hobj.OpenOptions, manHobj, out compCode, out reason, hobj);
                            base.TrText(method, string.Concat(new object[] { "MQOPEN during reconnect completed with CompCode = ", compCode, " Reason = ", reason }));
                            mqod.ObjectName = objectName;
                            mqod.DynamicQName = dynamicQName;
                        }
                        if (reason != 0)
                        {
                            remoteHconn.SetReconnectionFailure(2, 0x9f4, new NmqiException(base.env, -1, null, compCode, reason, null));
                            return false;
                        }
                        if (hobj.IsCallbackRegistered)
                        {
                            int operationP = 0x100;
                            if (hobj.IsCallbackSuspended)
                            {
                                operationP |= 0x10000;
                            }
                            MQCBD callbackDescriptor = hobj.CallbackDescriptor;
                            Hobj hobjP = hobj;
                            MQMessageDescriptor callbackMessageDescriptor = hobj.CallbackMessageDescriptor;
                            MQGetMessageOptions callbackGetMessageOptions = hobj.CallbackGetMessageOptions;
                            getMQFAP.MQCB(remoteHconn, operationP, callbackDescriptor, hobjP, callbackMessageDescriptor, callbackGetMessageOptions, out compCode, out reason);
                            base.TrText(method, string.Concat(new object[] { "MQCB during reconnect completed with CompCode = ", compCode, " Reason = ", reason }));
                            if (reason != 0)
                            {
                                remoteHconn.SetReconnectionFailure(2, 0x9f4, new NmqiException(base.env, -1, null, compCode, reason, null));
                                return false;
                            }
                        }
                    }
                }
                foreach (ManagedHsub hsub in remoteHconn.Hsbjs)
                {
                    int options = hsub.Mqsd.Options;
                    if ((options & 1) != 0)
                    {
                        options |= 2;
                    }
                    if ((options & 0x20) == 0)
                    {
                        options |= 0x400000;
                    }
                    if (((options & 8) != 0) && ((options & 6) != 0))
                    {
                        options |= 6;
                    }
                    hsub.Mqsd.Options = options;
                    Phobj pHsub = base.env.NewPhobj();
                    Phobj pHobj = base.env.NewPhobj();
                    if (hsub.Mqsd.SubExpiry != -1)
                    {
                        hsub.Mqsd.SubExpiry = hsub.GeExpiryRemainder();
                    }
                    getMQFAP.nmqiSubscribe(remoteHconn, hsub.Mqsd, ref pHobj, ref pHsub, out compCode, out reason, hsub.SpiSD, hsub.SpiCall, hsub);
                    base.TrText(method, string.Concat(new object[] { "MQSUB during reconnect completed with CompCode = ", compCode, " Reason = ", reason }));
                    if (reason != 0)
                    {
                        if (!ManagedHconn.IsReconnectableReasonCode(reason) && (reason != 0x97d))
                        {
                            remoteHconn.SetReconnectionFailure(2, 0x9f4, new NmqiException(base.env, -1, null, compCode, reason, null));
                        }
                        return false;
                    }
                    ManagedHobj hobj3 = hsub.Hobj;
                    if (((hobj3 != null) && (hobj3.ProxyQueue != null)) && hobj3.IsCallbackRegistered)
                    {
                        int num6 = 0x100;
                        if (hobj3.IsCallbackSuspended)
                        {
                            num6 |= 0x10000;
                        }
                        MQCBD pCallbackDesc = hobj3.CallbackDescriptor;
                        Hobj hobj4 = hobj3;
                        MQMessageDescriptor pMsgDescP = hobj3.CallbackMessageDescriptor;
                        MQGetMessageOptions getMsgOptsP = hobj3.CallbackGetMessageOptions;
                        getMQFAP.MQCB(remoteHconn, num6, pCallbackDesc, hobj4, pMsgDescP, getMsgOptsP, out compCode, out reason);
                        base.TrText(method, string.Concat(new object[] { "MQCB during reconnect completed with CompCode = ", compCode, " Reason = ", reason }));
                        if (reason != 0)
                        {
                            remoteHconn.SetReconnectionFailure(2, 0x9f4, new NmqiException(base.env, -1, null, compCode, reason, null));
                            return false;
                        }
                    }
                }
                if (remoteHconn.IsEventRegistered)
                {
                    int num7 = 0x100;
                    if (remoteHconn.IsEventSuspended)
                    {
                        num7 |= 0x10000;
                    }
                    getMQFAP.MQCB(remoteHconn, num7, remoteHconn.GetEventDescriptor(), base.env.NewPhobj().HOBJ, null, null, out compCode, out reason);
                    base.TrText(method, string.Concat(new object[] { "MQCB during reconnect completed with CompCode = ", compCode, " Reason = ", reason }));
                    if (reason != 0)
                    {
                        if (!ManagedHconn.IsReconnectableReasonCode(reason))
                        {
                            remoteHconn.SetReconnectionFailure(2, 0x9f4, new NmqiException(base.env, -1, null, compCode, reason, null));
                        }
                        return false;
                    }
                }
                if (remoteHconn.IsCallbackStarted())
                {
                    MQCTLO pControlOpts = base.env.NewMQCTLO();
                    getMQFAP.MQCTL(remoteHconn, 1, pControlOpts, out compCode, out reason);
                    base.TrText(method, string.Concat(new object[] { "MQCTL during reconnect completed with CompCode = ", compCode, " Reason = ", reason }));
                    if (reason != 0)
                    {
                        if (!ManagedHconn.IsReconnectableReasonCode(reason))
                        {
                            remoteHconn.SetReconnectionFailure(2, 0x9f4, new NmqiException(base.env, -1, null, compCode, reason, null));
                        }
                        return false;
                    }
                    if (remoteHconn.IsCallbackSuspended())
                    {
                        getMQFAP.MQCTL(remoteHconn, 0x10000, pControlOpts, out compCode, out reason);
                        if (reason != 0)
                        {
                            if (!ManagedHconn.IsReconnectableReasonCode(reason))
                            {
                                remoteHconn.SetReconnectionFailure(2, 0x9f4, new NmqiException(base.env, -1, null, compCode, reason, null));
                            }
                            return false;
                        }
                    }
                    remoteHconn.WakeDispatchThread();
                }
            }
            finally
            {
                base.TrExit(method, true, 11);
            }
            return true;
        }

        private void ReconnectionComplete(ManagedHconn remoteHconn)
        {
            uint method = 0x4c7;
            this.TrEntry(method, new object[] { remoteHconn });
            try
            {
                try
                {
                    ManagedHconn.SendAsyncNotification(base.env, remoteHconn, -1, 0x11, 0);
                    base.TrText(method, "A notification has been to post Reconnection Completion event");
                }
                catch (NmqiException)
                {
                    throw;
                }
                try
                {
                    remoteHconn.DriveEventsEH(0, 0x9f1);
                }
                catch (Exception exception)
                {
                    base.TrException(method, exception);
                }
                remoteHconn.ResetReconnectionEvents();
                try
                {
                    remoteHconn.CheckDispatchable(null);
                    base.TrText(method, "An attempt has been made to wake up Dispatcher threads.");
                }
                catch (NmqiException exception2)
                {
                    base.TrException(method, exception2, 1);
                }
                this.ReleaseHconn(remoteHconn);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void ReleaseHconn(ManagedHconn rcnHconn)
        {
            uint method = 0x5e9;
            this.TrEntry(method, new object[] { rcnHconn });
            try
            {
                lock (this.reconnectMutex)
                {
                    this.hconns.Remove(rcnHconn);
                    rcnHconn.Reconnected();
                    base.TrText(method, "Hconn : " + rcnHconn.Value + " has been released now");
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Run()
        {
            uint method = 0x4c1;
            this.TrEntry(method);
            Thread.CurrentThread.Name = "ReconnectThread";
            Thread.SetData(Thread.GetNamedDataSlot("MQ_CLIENT_THREAD_TYPE"), 3);
            try
            {
                while (true)
                {
                    ManagedHconn remoteHconn = this.BestHconn();
                    base.TrText(method, "Hconn found for reconnection = " + remoteHconn.Value);
                    if (this.Reconnect(remoteHconn))
                    {
                        this.ReconnectionComplete(remoteHconn);
                        base.TrText(method, "Reconnection Completed");
                    }
                    else if (remoteHconn.HasFailed())
                    {
                        this.ReleaseHconn(remoteHconn);
                    }
                    else
                    {
                        remoteHconn.ReconnectionAttempts++;
                        int index = remoteHconn.ReconnectionAttempts - 1;
                        if (index >= this.rcnTimes.Length)
                        {
                            index = this.rcnTimes.Length - 1;
                        }
                        remoteHconn.NextReconnect = NmqiTools.GetCurrentTimeInMs() + Convert.ToInt64(this.rcnTimes[index]);
                        base.TrText(method, string.Concat(new object[] { "Reconnection details: Hconn : ", remoteHconn.Value, " Reconnection times : ", remoteHconn.ReconnectionAttempts, "NextReconnectionTime : ", remoteHconn.NextReconnect }));
                    }
                }
            }
            catch (Exception exception)
            {
                base.TrException(method, exception);
                CommonServices.SetValidInserts();
                CommonServices.ArithInsert1 = 1;
                CommonServices.CommentInsert1 = "ReconnectThread::Run";
                CommonServices.CommentInsert2 = exception.Message;
                CommonServices.CommentInsert3 = exception.StackTrace;
                base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009546, 0);
            }
            finally
            {
                Thread.SetData(Thread.GetNamedDataSlot("MQ_CLIENT_THREAD_TYPE"), 0);
                base.TrExit(method);
            }
        }
    }
}

