namespace IBM.WMQ
{
    using System;

    public class MQProcess : MQManagedObject, IDisposable
    {
        private string qMgrName;
        private const string sccsid = "@(#) lib/dotnet/pc/winnt/baseclasses/MQProcess.cs, dotnet, p000  1.6 11/04/18 05:57:36";

        public MQProcess(MQQueueManager qMgr, string processName, int openOptions) : this(qMgr, processName, openOptions, null, null)
        {
            base.TrConstructor("@(#) lib/dotnet/pc/winnt/baseclasses/MQProcess.cs, dotnet, p000  1.6 11/04/18 05:57:36", new object[] { qMgr, processName, openOptions });
        }

        public MQProcess(MQQueueManager qMgr, string processName, int openOptions, string queueManagerName, string alternateUserId)
        {
            base.TrConstructor("@(#) lib/dotnet/pc/winnt/baseclasses/MQProcess.cs, dotnet, p000  1.6 11/04/18 05:57:36", new object[] { qMgr, processName, openOptions, queueManagerName, alternateUserId });
            base.qMgr = qMgr;
            if ((processName != null) && (processName.Length > 0))
            {
                base.objectName = processName;
            }
            base.objectType = 3;
            base.OpenOptions = openOptions | 0x20;
            this.qMgrName = queueManagerName;
            if ((alternateUserId != null) && (alternateUserId.Length > 0))
            {
                base.AlternateUserId = alternateUserId;
            }
            this.OpenProcessForInquire(openOptions);
        }

        public override void Close()
        {
            uint method = 0x1ff;
            this.TrEntry(method);
            int pCompCode = 0;
            int pReason = 0;
            try
            {
                if (base.qMgr != null)
                {
                    base.Close();
                    if (((base.qMgr.IsConnected && base.IsOpen) && ((base.objectHandle != null) && (base.objectHandle.HOBJ != null))) && ((base.objectHandle.HOBJ.Handle != 0) && (-1 != base.objectHandle.HOBJ.Handle)))
                    {
                        base.qMgr.nmqiConnector.MQCLOSE(base.qMgr.hConn, base.objectHandle, base.CloseOptions, out pCompCode, out pReason);
                        base.objectHandle = null;
                        base.objectName = "";
                        if (pCompCode != 0)
                        {
                            base.qMgr.CheckHConnHealth(pReason);
                            base.throwNewMQException(pCompCode, pReason);
                        }
                    }
                    GC.SuppressFinalize(this);
                }
            }
            catch (MQException exception)
            {
                pCompCode = exception.CompCode;
                pReason = exception.Reason;
                throw exception;
            }
            finally
            {
                base.unsafe_compCode = pCompCode;
                base.unsafe_reason = pReason;
                base.TrExit(method);
            }
        }

        internal void Dispose(bool disposing)
        {
            uint method = 510;
            this.TrEntry(method, new object[] { disposing });
            try
            {
                this.Close();
            }
            catch (Exception exception)
            {
                base.TrException(method, exception);
            }
            finally
            {
                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }
                base.TrExit(method);
            }
        }

        ~MQProcess()
        {
            this.Dispose(false);
        }

        private void OpenProcessForInquire(int openOptions)
        {
            uint method = 0x1fd;
            this.TrEntry(method, new object[] { openOptions });
            int pCompCode = 0;
            int pReason = 0;
            MQObjectDescriptor pObjDesc = new MQObjectDescriptor();
            pObjDesc.ObjectType = 3;
            pObjDesc.ObjectName = base.objectName;
            if ((this.qMgrName != null) && (this.qMgrName.Length > 0))
            {
                pObjDesc.ObjectQMgrName = this.qMgrName;
            }
            else
            {
                pObjDesc.ObjectQMgrName = base.qMgr.Name;
            }
            if ((base.AlternateUserId != null) && (base.AlternateUserId.Length > 0))
            {
                pObjDesc.AlternateUserId = base.AlternateUserId;
            }
            try
            {
                if (base.objectHandle == null)
                {
                    base.objectHandle = MQQueueManager.nmqiEnv.NewPhobj();
                }
                base.qMgr.nmqiConnector.MQOPEN(base.qMgr.hConn, ref pObjDesc, base.OpenOptions, base.objectHandle, out pCompCode, out pReason);
                if (pCompCode == 0)
                {
                    base.hObj = base.objectHandle.HOBJ;
                }
                if (pCompCode != 0)
                {
                    base.qMgr.CheckHConnHealth(pReason);
                    base.throwNewMQException(pCompCode, pReason);
                }
                base.isClosed = false;
            }
            catch (MQException exception)
            {
                pCompCode = exception.CompCode;
                pReason = exception.Reason;
                throw exception;
            }
            finally
            {
                base.unsafe_compCode = pCompCode;
                base.unsafe_reason = pReason;
                base.TrExit(method);
            }
        }

        void IDisposable.Dispose()
        {
            uint method = 0x41b;
            this.TrEntry(method);
            try
            {
                this.Dispose(true);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public string ApplicationId
        {
            get
            {
                return base.QueryAttribute(0x7d1, 0x100);
            }
        }

        public int ApplicationType
        {
            get
            {
                return base.QueryAttribute(1);
            }
        }

        public string EnvironmentData
        {
            get
            {
                return base.QueryAttribute(0x7d7, 0x80);
            }
        }

        public string UserData
        {
            get
            {
                return base.QueryAttribute(0x7e5, 0x80);
            }
        }
    }
}

