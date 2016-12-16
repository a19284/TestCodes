namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQPutMessageOptions : MQBaseObject
    {
        internal MQBase.MQPMO mqPMO = new MQBase.MQPMO();
        private MQQueue queueContextReference;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public MQPutMessageOptions()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.ClearInvalidFields(0);
        }

        private void ClearInvalidFields(int Version)
        {
            uint method = 0x220;
            this.TrEntry(method, new object[] { Version });
            switch (Version)
            {
                case 0:
                    this.mqPMO.StrucId = new byte[] { 80, 0x4d, 0x4f, 0x20 };
                    this.mqPMO.Options = 0;
                    this.mqPMO.Timeout = -1;
                    this.mqPMO.Context = 0;
                    this.mqPMO.KnownDestCount = 0;
                    this.mqPMO.UnknownDestCount = 0;
                    this.mqPMO.InvalidDestCount = 0;
                    this.mqPMO.ResolvedQName = new byte[0x30];
                    this.mqPMO.ResolvedQMgrName = new byte[0x30];
                    break;

                case 1:
                    break;

                default:
                    goto Label_0143;
            }
            this.mqPMO.RecsPresent = 0;
            this.mqPMO.PutMsgRecFields = 0;
            this.mqPMO.PutMsgRecOffset = 0;
            this.mqPMO.ResponseRecOffset = 0;
            this.mqPMO.PutMsgRecPtr = IntPtr.Zero;
            this.mqPMO.ResponseRecPtr = IntPtr.Zero;
            this.queueContextReference = null;
        Label_0143:
            if (Version == 0)
            {
                this.mqPMO.Version = 1;
            }
            base.TrExit(method);
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.mqPMO);
        }

        internal int GetVersionLength()
        {
            uint method = 0x221;
            this.TrEntry(method);
            switch (this.mqPMO.Version)
            {
                case 1:
                    base.TrExit(method, 0x80, 1);
                    return 0x80;

                case 2:
                    base.TrExit(method, 0x98, 2);
                    return 0x98;
            }
            base.TrExit(method, 0, 3);
            return 0;
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x223;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            int result = 0;
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                if (length > (b.Length - Offset))
                {
                    length = b.Length - Offset;
                }
                Marshal.Copy(b, Offset, zero, length);
                this.mqPMO = (MQBase.MQPMO) Marshal.PtrToStructure(zero, typeof(MQBase.MQPMO));
                this.ClearInvalidFields(this.mqPMO.Version);
                Marshal.FreeCoTaskMem(zero);
                result = Offset + this.GetVersionLength();
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        internal void ValidateOptions()
        {
            uint method = 0x224;
            this.TrEntry(method);
            int num2 = this.Options & 0x30000;
            int cc = 0;
            int rc = 0;
            try
            {
                if ((num2 & (num2 - 1)) != 0)
                {
                    cc = 2;
                    rc = 0x7fe;
                    base.throwNewMQException(cc, rc);
                }
                num2 = this.Options & 0x4020;
                if ((num2 & (num2 - 1)) != 0)
                {
                    cc = 2;
                    rc = 0x7fe;
                    base.throwNewMQException(cc, rc);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x222;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int versionLength = this.GetVersionLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.mqPMO, zero, false);
                Marshal.Copy(zero, b, Offset, versionLength);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, versionLength);
            }
            return versionLength;
        }

        public int Context
        {
            get
            {
                return this.mqPMO.Context;
            }
            set
            {
                this.mqPMO.Context = value;
            }
        }

        public MQQueue ContextReference
        {
            get
            {
                return this.queueContextReference;
            }
            set
            {
                this.queueContextReference = value;
                if (this.queueContextReference != null)
                {
                    this.mqPMO.Context = this.queueContextReference.objectHandle.HOBJ.Handle;
                }
                else
                {
                    this.mqPMO.Context = 0;
                }
            }
        }

        public int InvalidDestCount
        {
            get
            {
                return this.mqPMO.InvalidDestCount;
            }
        }

        public int KnownDestCount
        {
            get
            {
                return this.mqPMO.KnownDestCount;
            }
        }

        public int Options
        {
            get
            {
                return this.mqPMO.Options;
            }
            set
            {
                this.mqPMO.Options = value;
            }
        }

        public int PutMsgRecOffset
        {
            get
            {
                return this.mqPMO.PutMsgRecOffset;
            }
            set
            {
                this.mqPMO.PutMsgRecOffset = value;
            }
        }

        public IntPtr PutMsgRecPtr
        {
            get
            {
                return this.mqPMO.PutMsgRecPtr;
            }
            set
            {
                this.mqPMO.PutMsgRecPtr = value;
            }
        }

        public int RecordFields
        {
            get
            {
                return this.mqPMO.PutMsgRecFields;
            }
            set
            {
                this.mqPMO.PutMsgRecFields = value;
            }
        }

        public int RecordsPresent
        {
            get
            {
                return this.mqPMO.RecsPresent;
            }
            set
            {
                this.mqPMO.RecsPresent = value;
            }
        }

        public string ResolvedQueueManagerName
        {
            get
            {
                return base.GetString(this.mqPMO.ResolvedQMgrName);
            }
        }

        public string ResolvedQueueName
        {
            get
            {
                return base.GetString(this.mqPMO.ResolvedQName);
            }
        }

        public int ResponseRecordOffset
        {
            get
            {
                return this.mqPMO.ResponseRecOffset;
            }
            set
            {
                this.mqPMO.ResponseRecOffset = value;
            }
        }

        public IntPtr ResponseRecordPtr
        {
            get
            {
                return this.mqPMO.ResponseRecPtr;
            }
            set
            {
                this.mqPMO.ResponseRecPtr = value;
            }
        }

        public MQBase.MQPMO StructMQPMO
        {
            get
            {
                return this.mqPMO;
            }
            set
            {
                this.mqPMO = value;
            }
        }

        public int Timeout
        {
            get
            {
                return this.mqPMO.Timeout;
            }
            set
            {
                this.mqPMO.Timeout = value;
            }
        }

        public int UnknownDestCount
        {
            get
            {
                return this.mqPMO.UnknownDestCount;
            }
        }

        public int Version
        {
            get
            {
                return this.mqPMO.Version;
            }
            set
            {
                this.mqPMO.Version = value;
            }
        }
    }
}

