namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public class MQAsyncStatus : MQBaseObject
    {
        private IntPtr buf = IntPtr.Zero;
        private MQBase.MQSTS mqsts = new MQBase.MQSTS();
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        internal MQAsyncStatus()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.ClearInvalidFields(0);
        }

        private void ClearInvalidFields(int Version)
        {
            uint method = 5;
            this.TrEntry(method, new object[] { Version });
            switch (Version)
            {
                case 0:
                    this.mqsts.StrucID = new byte[] { 0x53, 0x54, 0x41, 0x54 };
                    this.mqsts.CompCode = 0;
                    this.mqsts.Reason = 0;
                    this.mqsts.PutSuccessCount = 0;
                    this.mqsts.PutWarningCount = 0;
                    this.mqsts.PutFailureCount = 0;
                    this.mqsts.ObjectType = 0;
                    this.mqsts.ObjectName = new byte[0x30];
                    this.mqsts.ObjectQMgrName = new byte[0x30];
                    this.mqsts.ResolvedObjectName = new byte[0x30];
                    this.mqsts.ResolvedObjectQMgrName = new byte[0x30];
                    break;
            }
            if (Version == 0)
            {
                this.mqsts.Version = 1;
            }
            base.TrExit(method);
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.mqsts);
        }

        internal int GetVersionLength()
        {
            uint method = 6;
            this.TrEntry(method);
            if (this.mqsts.Version == 1)
            {
                int length = this.GetLength();
                base.TrExit(method, length, 1);
                return length;
            }
            base.TrExit(method, 0, 2);
            return 0;
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 8;
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
                this.mqsts = (MQBase.MQSTS) Marshal.PtrToStructure(zero, typeof(MQBase.MQSTS));
                Marshal.FreeCoTaskMem(zero);
                result = Offset + this.GetVersionLength();
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 7;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int versionLength = this.GetVersionLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.mqsts, zero, false);
                Marshal.Copy(zero, b, Offset, versionLength);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, versionLength);
            }
            return versionLength;
        }

        public int CompCode
        {
            get
            {
                return this.mqsts.CompCode;
            }
        }

        public string ObjectName
        {
            get
            {
                return Encoding.ASCII.GetString(this.mqsts.ObjectName);
            }
        }

        public string ObjectQMgrName
        {
            get
            {
                return Encoding.ASCII.GetString(this.mqsts.ObjectQMgrName);
            }
        }

        public int ObjectType
        {
            get
            {
                return this.mqsts.ObjectType;
            }
        }

        public int PutFailureCount
        {
            get
            {
                return this.mqsts.PutFailureCount;
            }
        }

        public int PutSuccessCount
        {
            get
            {
                return this.mqsts.PutSuccessCount;
            }
        }

        public int PutWarningCount
        {
            get
            {
                return this.mqsts.PutWarningCount;
            }
        }

        public int Reason
        {
            get
            {
                return this.mqsts.Reason;
            }
        }

        public string ResolvedObjectName
        {
            get
            {
                return Encoding.ASCII.GetString(this.mqsts.ResolvedObjectName);
            }
        }

        public string ResolvedObjectQMgrName
        {
            get
            {
                return Encoding.ASCII.GetString(this.mqsts.ResolvedObjectQMgrName);
            }
        }

        public MQBase.MQSTS StructMQSTS
        {
            get
            {
                return this.mqsts;
            }
            set
            {
                this.mqsts = value;
            }
        }

        public int Version
        {
            get
            {
                return this.mqsts.Version;
            }
            set
            {
                this.mqsts.Version = value;
            }
        }
    }
}

