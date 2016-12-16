namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;

    public abstract class MQManagedObject : MQBaseObject
    {
        private string alternateUserId = "";
        private int closeOptions;
        private Phobj handleInq;
        private Phobj handleSet;
        internal Hobj hObj;
        protected bool isClosed = true;
        protected string name;
        internal Phobj objectHandle;
        protected string objectName;
        protected int objectType;
        private int openOptions;
        protected MQQueueManager qMgr;
        private const string sccsid = "@(#) lib/dotnet/pc/winnt/baseclasses/MQManagedObject.cs, dotnet, p000  1.5 10/04/08 06:22:56";

        public MQManagedObject()
        {
            base.TrConstructor("@(#) lib/dotnet/pc/winnt/baseclasses/MQManagedObject.cs, dotnet, p000  1.5 10/04/08 06:22:56");
            this.objectType = 0;
            this.qMgr = null;
            this.objectHandle = null;
            this.handleInq = null;
            this.handleSet = null;
            this.alternateUserId = "";
            this.objectName = "";
            this.openOptions = 0;
            this.closeOptions = 0;
        }

        public virtual void Close()
        {
            uint method = 0xeb;
            this.TrEntry(method);
            int pCompCode = 0;
            int pReason = 0;
            try
            {
                if (this.qMgr != null)
                {
                    if (((this.qMgr.IsConnected && (this.handleInq != null)) && ((this.handleInq.HOBJ != null) && (this.handleInq.HOBJ.Handle != 0))) && (-1 != this.handleInq.HOBJ.Handle))
                    {
                        this.qMgr.nmqiConnector.MQCLOSE(this.qMgr.hConn, this.handleInq, this.CloseOptions, out pCompCode, out pReason);
                        this.handleInq = null;
                        if (pCompCode != 0)
                        {
                            this.qMgr.CheckHConnHealth(pReason);
                            base.throwNewMQException(pCompCode, pReason);
                        }
                    }
                    if (((this.qMgr.IsConnected && (this.handleSet != null)) && ((this.handleSet.HOBJ != null) && (this.handleSet.HOBJ.Handle != 0))) && (-1 != this.handleSet.HOBJ.Handle))
                    {
                        this.qMgr.nmqiConnector.MQCLOSE(this.qMgr.hConn, this.handleSet, this.CloseOptions, out pCompCode, out pReason);
                        this.handleSet = null;
                        if (pCompCode != 0)
                        {
                            this.qMgr.CheckHConnHealth(pReason);
                            base.throwNewMQException(pCompCode, pReason);
                        }
                    }
                    this.isClosed = true;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Inquire(int[] selectors, int[] intAttrs, byte[] charAttrs)
        {
            uint method = 0xed;
            this.TrEntry(method, new object[] { selectors, intAttrs, charAttrs });
            Hobj hOBJ = null;
            int pCompCode = 0;
            int pReason = 0;
            try
            {
                int length;
                int num3;
                if ((selectors == null) || (selectors.Length == 0))
                {
                    base.throwNewMQException(2, 0x813);
                }
                if (this.objectType == 5)
                {
                    if ((this.qMgr == null) || !this.qMgr.IsConnected)
                    {
                        base.throwNewMQException(2, 0x7e2);
                    }
                    if (this.isClosed)
                    {
                        base.throwNewMQException(2, 0x7e3);
                    }
                    if (((this.handleInq == null) || (this.handleInq.HOBJ == null)) || (this.handleInq.HOBJ.Handle == 0))
                    {
                        this.handleInq = MQQueueManager.nmqiEnv.NewPhobj();
                        this.OpenForInquire();
                    }
                    hOBJ = this.handleInq.HOBJ;
                }
                if (this.objectType == 3)
                {
                    if ((this.qMgr == null) || !this.qMgr.IsConnected)
                    {
                        base.throwNewMQException(2, 0x7e2);
                    }
                    if (this.isClosed)
                    {
                        base.throwNewMQException(2, 0x7e3);
                    }
                    hOBJ = this.objectHandle.HOBJ;
                }
                if (this.objectType == 1)
                {
                    if (!this.IsOpen)
                    {
                        base.throwNewMQException(2, 0x7e3);
                    }
                    hOBJ = this.objectHandle.HOBJ;
                }
                if (this.objectType == 8)
                {
                    if (!this.IsOpen)
                    {
                        base.throwNewMQException(2, 0x7e3);
                    }
                    hOBJ = this.objectHandle.HOBJ;
                }
                if (intAttrs == null)
                {
                    length = 0;
                }
                else
                {
                    length = intAttrs.Length;
                }
                if (charAttrs == null)
                {
                    num3 = 0;
                }
                else
                {
                    num3 = charAttrs.Length;
                }
                this.qMgr.nmqiConnector.MQINQ(this.qMgr.hConn, hOBJ, selectors.Length, selectors, length, intAttrs, num3, charAttrs, out pCompCode, out pReason);
                if (pCompCode != 0)
                {
                    this.qMgr.CheckHConnHealth(pReason);
                    base.throwNewMQException(pCompCode, pReason);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void OpenForInquire()
        {
            uint method = 0xf2;
            this.TrEntry(method);
            int pCompCode = 0;
            int pReason = 0;
            MQObjectDescriptor pObjDesc = new MQObjectDescriptor();
            pObjDesc.ObjectType = this.objectType;
            if (5 != this.objectType)
            {
                pObjDesc.ObjectName = this.objectName;
            }
            try
            {
                this.qMgr.nmqiConnector.MQOPEN(this.qMgr.hConn, ref pObjDesc, 0x2020, this.handleInq, out pCompCode, out pReason);
                if (pCompCode != 0)
                {
                    this.qMgr.CheckHConnHealth(pReason);
                    base.throwNewMQException(pCompCode, pReason);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void OpenForSet()
        {
            uint method = 0xf3;
            this.TrEntry(method);
            int pCompCode = 0;
            int pReason = 0;
            MQObjectDescriptor pObjDesc = new MQObjectDescriptor();
            pObjDesc.ObjectType = this.objectType;
            pObjDesc.ObjectName = this.objectName;
            try
            {
                this.qMgr.nmqiConnector.MQOPEN(this.qMgr.hConn, ref pObjDesc, 0x2040, this.handleSet, out pCompCode, out pReason);
                if (pCompCode != 0)
                {
                    this.qMgr.CheckHConnHealth(pReason);
                    base.throwNewMQException(pCompCode, pReason);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        protected int QueryAttribute(int attributeType)
        {
            uint method = 0xee;
            this.TrEntry(method, new object[] { attributeType });
            int[] selectors = new int[] { attributeType };
            int[] intAttrs = new int[1];
            try
            {
                base.TrText(method, " Attribute Type: " + attributeType.ToString());
                this.Inquire(selectors, intAttrs, null);
            }
            finally
            {
                base.TrExit(method);
            }
            return intAttrs[0];
        }

        protected string QueryAttribute(int attributeType, int stringMaxLength)
        {
            string str2;
            uint method = 0xef;
            this.TrEntry(method, new object[] { attributeType, stringMaxLength });
            int[] selectors = new int[] { attributeType };
            byte[] charAttrs = new byte[stringMaxLength];
            try
            {
                base.TrText(method, string.Concat(new object[] { " Attribute Type: ", attributeType, " Length: ", stringMaxLength }));
                this.Inquire(selectors, null, charAttrs);
                str2 = base.GetString(charAttrs);
            }
            finally
            {
                base.TrExit(method);
            }
            return str2;
        }

        public void Set(int[] selectors, int[] intAttrs, byte[] charAttrs)
        {
            uint method = 0xec;
            this.TrEntry(method, new object[] { selectors, intAttrs, charAttrs });
            Hobj hOBJ = null;
            int selectorCount = 0;
            int intAttrCount = 0;
            int charAttrLength = 0;
            int pCompCode = 0;
            int pReason = 0;
            try
            {
                if ((selectors == null) || (selectors.Length == 0))
                {
                    base.throwNewMQException(2, 0x813);
                }
                if (5 == this.objectType)
                {
                    if (this.qMgr == null)
                    {
                        base.throwNewMQException(2, 0x7e2);
                    }
                    if (this.isClosed)
                    {
                        base.throwNewMQException(2, 0x7e3);
                    }
                    if (((this.handleSet == null) || (this.handleSet.HOBJ == null)) || (this.handleSet.HOBJ.Handle == 0))
                    {
                        this.handleSet = MQQueueManager.nmqiEnv.NewPhobj();
                        this.OpenForSet();
                    }
                    hOBJ = this.handleSet.HOBJ;
                }
                if (this.objectHandle == null)
                {
                    this.objectHandle = MQQueueManager.nmqiEnv.NewPhobj();
                }
                if (1 == this.objectType)
                {
                    if (!this.IsOpen)
                    {
                        base.throwNewMQException(2, 0x7e3);
                    }
                    hOBJ = this.objectHandle.HOBJ;
                }
                if (8 == this.objectType)
                {
                    if (!this.IsOpen)
                    {
                        base.throwNewMQException(2, 0x7e3);
                    }
                    hOBJ = this.objectHandle.HOBJ;
                }
                if (3 == this.objectType)
                {
                    if (!this.IsOpen)
                    {
                        base.throwNewMQException(2, 0x7e3);
                    }
                    hOBJ = this.objectHandle.HOBJ;
                }
                selectorCount = selectors.Length;
                if (intAttrs != null)
                {
                    intAttrCount = intAttrs.Length;
                }
                if (charAttrs != null)
                {
                    charAttrLength = charAttrs.Length;
                }
                this.qMgr.nmqiConnector.MQSET(this.qMgr.hConn, hOBJ, selectorCount, selectors, intAttrCount, intAttrs, charAttrLength, charAttrs, out pCompCode, out pReason);
                if (pCompCode != 0)
                {
                    this.qMgr.CheckHConnHealth(pReason);
                    base.throwNewMQException(pCompCode, pReason);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        protected void SetAttribute(int attributeType, int attributeValue)
        {
            uint method = 240;
            this.TrEntry(method, new object[] { attributeType, attributeValue });
            int[] selectors = new int[] { attributeType };
            int[] intAttrs = new int[] { attributeValue };
            try
            {
                base.TrText(method, " Attribute Type: " + attributeType.ToString() + " Attribute Value: " + attributeValue.ToString());
                this.Set(selectors, intAttrs, null);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        protected void SetAttribute(int attributeType, string attributeValue, int attributeMaxLength)
        {
            uint method = 0xf1;
            this.TrEntry(method, new object[] { attributeType, attributeValue, attributeMaxLength });
            int[] selectors = new int[] { attributeType };
            byte[] buffer = new byte[attributeMaxLength];
            try
            {
                base.TrText(method, " Attribute Type: " + attributeType.ToString() + " AttributeValue: " + attributeValue + " Attribute MaxLength: " + attributeMaxLength.ToString());
                base.GetBytes(attributeValue, ref buffer);
                this.Set(selectors, null, buffer);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public string AlternateUserId
        {
            get
            {
                return this.alternateUserId;
            }
            set
            {
                this.alternateUserId = value;
            }
        }

        public int CloseOptions
        {
            get
            {
                return this.closeOptions;
            }
            set
            {
                this.closeOptions = value;
            }
        }

        public MQQueueManager ConnectionReference
        {
            get
            {
                return this.qMgr;
            }
        }

        public bool IsOpen
        {
            get
            {
                return (((this.objectHandle != null) && (this.objectHandle.HOBJ != null)) && ((this.objectHandle.HOBJ.Handle != 0) && (-1 != this.objectHandle.HOBJ.Handle)));
            }
        }

        public string Name
        {
            get
            {
                if (this.name == null)
                {
                    if (this.objectType == 5)
                    {
                        this.name = this.QueryAttribute(0x7df, 0x30);
                    }
                    else if (this.objectType == 1)
                    {
                        this.name = this.QueryAttribute(0x7e0, 0x30);
                    }
                    else if (this.objectType == 8)
                    {
                        this.name = this.QueryAttribute(0x82c, 0x30);
                    }
                    else if (this.objectType == 3)
                    {
                        this.name = this.QueryAttribute(0x7dc, 0x30);
                    }
                }
                return this.name;
            }
        }

        internal int ObjectHandle
        {
            get
            {
                if ((this.objectHandle != null) && (this.objectHandle.HOBJ != null))
                {
                    return this.objectHandle.HOBJ.Handle;
                }
                return -1;
            }
        }

        public int OpenOptions
        {
            get
            {
                return this.openOptions;
            }
            set
            {
                this.openOptions = value;
            }
        }

        public bool OpenStatus
        {
            get
            {
                bool flag = false;
                if ((this.qMgr != null) && this.qMgr.IsConnected)
                {
                    flag = !this.isClosed;
                }
                return flag;
            }
        }

        public string ProcessDescription
        {
            get
            {
                return this.QueryAttribute(0x7db, 0x40);
            }
        }
    }
}

