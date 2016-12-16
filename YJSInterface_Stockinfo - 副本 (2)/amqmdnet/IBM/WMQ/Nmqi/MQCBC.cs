namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Runtime.InteropServices;

    public class MQCBC : MQBase
    {
        internal MQBase.structMQCBC mqcbc;
        private const string sccsid = "%Z% %W% %I% %E% %U%";

        public MQCBC()
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%");
            this.mqcbc = new MQBase.structMQCBC();
            this.mqcbc.strucId = new byte[] { 0x43, 0x42, 0x43, 0x20 };
            this.mqcbc.version = 1;
            this.mqcbc.callType = 1;
            this.mqcbc.hobj = -1;
            this.mqcbc.callbackArea = IntPtr.Zero;
            this.mqcbc.connectionArea = IntPtr.Zero;
            this.mqcbc.compCode = 0;
            this.mqcbc.reason = 0;
            this.mqcbc.state = 0;
            this.mqcbc.dataLength = 0;
            this.mqcbc.bufferLength = 0;
            this.mqcbc.flags = 0;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.mqcbc);
        }

        public int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x48e;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            zero = Marshal.AllocCoTaskMem(length);
            if (length > (b.Length - Offset))
            {
                length = b.Length - Offset;
            }
            try
            {
                Marshal.Copy(b, Offset, zero, length);
                this.mqcbc = (MQBase.structMQCBC) Marshal.PtrToStructure(zero, typeof(MQBase.structMQCBC));
            }
            finally
            {
                Marshal.FreeCoTaskMem(zero);
                base.TrExit(method);
            }
            return (Offset + this.GetLength());
        }

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x48d;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            zero = Marshal.AllocCoTaskMem(length);
            try
            {
                Marshal.StructureToPtr(this.mqcbc, zero, false);
                Marshal.Copy(zero, b, Offset, length);
            }
            finally
            {
                Marshal.FreeCoTaskMem(zero);
                base.TrExit(method);
            }
            return length;
        }

        public int BufferLength
        {
            get
            {
                return this.mqcbc.bufferLength;
            }
            set
            {
                this.mqcbc.bufferLength = value;
            }
        }

        public IntPtr CallbackArea
        {
            get
            {
                return this.mqcbc.callbackArea;
            }
            set
            {
                this.mqcbc.callbackArea = value;
            }
        }

        public int CallType
        {
            get
            {
                return this.mqcbc.callType;
            }
            set
            {
                this.mqcbc.callType = value;
            }
        }

        public int CompCode
        {
            get
            {
                return this.mqcbc.compCode;
            }
            set
            {
                this.mqcbc.compCode = value;
            }
        }

        public IntPtr ConnectionArea
        {
            get
            {
                return this.mqcbc.connectionArea;
            }
            set
            {
                this.mqcbc.connectionArea = value;
            }
        }

        public int DataLength
        {
            get
            {
                return this.mqcbc.dataLength;
            }
            set
            {
                this.mqcbc.dataLength = value;
            }
        }

        public int Flags
        {
            get
            {
                return this.mqcbc.flags;
            }
            set
            {
                this.mqcbc.flags = value;
            }
        }

        public int HOBJ
        {
            get
            {
                return this.mqcbc.hobj;
            }
            set
            {
                this.mqcbc.hobj = value;
            }
        }

        public int Reason
        {
            get
            {
                return this.mqcbc.reason;
            }
            set
            {
                this.mqcbc.reason = value;
            }
        }

        public int State
        {
            get
            {
                return this.mqcbc.state;
            }
            set
            {
                this.mqcbc.state = value;
            }
        }

        public MQBase.structMQCBC StructMQCBC
        {
            get
            {
                return this.mqcbc;
            }
            set
            {
                this.mqcbc = value;
            }
        }

        public int Version
        {
            get
            {
                return this.mqcbc.version;
            }
            set
            {
                this.mqcbc.version = value;
            }
        }
    }
}

