namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Runtime.InteropServices;

    public class MQCBD : MQBase
    {
        private string cbdId;
        internal MQBase.structMQCBD mqcbd;
        public MQConsumer mqConsumer;
        private const string sccsid = "%Z% %W% %I% %E% %U%";

        public MQCBD()
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%");
            this.mqcbd = new MQBase.structMQCBD();
            this.mqcbd.strucId = new byte[] { 0x43, 0x42, 0x44, 0x20 };
            this.mqcbd.version = 1;
            this.mqcbd.callbackType = 1;
            this.mqcbd.options = 0;
            this.mqcbd.callbackArea = IntPtr.Zero;
            this.mqcbd.callbackFunction = IntPtr.Zero;
            this.mqConsumer = null;
            this.mqcbd.callbackName = new byte[0x80];
            this.mqcbd.maxMsgLength = -1;
            this.cbdId = NmqiTools.GetCurrentTimeInMs().ToString() + this.GetHashCode();
        }

        private void CheckArrayLength()
        {
            uint method = 0x490;
            this.TrEntry(method);
            if ((this.mqcbd.callbackName != null) && (this.mqcbd.callbackName.Length != 0x80))
            {
                this.mqcbd.callbackName = this.ResizeArrayToCorrectLength(this.mqcbd.callbackName, 0x80);
            }
            base.TrExit(method);
        }

        public MQCBD CopyMqcbd()
        {
            uint method = 0x492;
            this.TrEntry(method);
            MQCBD result = new MQCBD();
            result.mqcbd.strucId = new byte[] { 0x43, 0x42, 0x44, 0x20 };
            result.mqcbd.version = this.mqcbd.version;
            result.mqcbd.callbackType = this.mqcbd.callbackType;
            result.mqcbd.options = this.mqcbd.options;
            result.mqcbd.callbackArea = this.mqcbd.callbackArea;
            result.mqcbd.callbackFunction = this.mqcbd.callbackFunction;
            result.mqConsumer = this.mqConsumer;
            result.mqcbd.callbackName = (byte[]) this.CallbackName.Clone();
            result.mqcbd.maxMsgLength = this.mqcbd.maxMsgLength;
            result.CbdId = this.CbdId;
            base.TrExit(method, result);
            return result;
        }

        public int GetLength()
        {
            return Marshal.SizeOf(this.mqcbd);
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x491;
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
                this.mqcbd = (MQBase.structMQCBD) Marshal.PtrToStructure(zero, typeof(MQBase.structMQCBD));
            }
            finally
            {
                Marshal.FreeCoTaskMem(zero);
                base.TrExit(method);
            }
            return (Offset + this.GetLength());
        }

        public int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x48f;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            this.CheckArrayLength();
            zero = Marshal.AllocCoTaskMem(length);
            try
            {
                Marshal.StructureToPtr(this.mqcbd, zero, false);
                Marshal.Copy(zero, b, Offset, length);
            }
            finally
            {
                Marshal.FreeCoTaskMem(zero);
                base.TrExit(method);
            }
            return length;
        }

        public IntPtr CallbackArea
        {
            get
            {
                return this.mqcbd.callbackArea;
            }
            set
            {
                this.mqcbd.callbackArea = value;
            }
        }

        public IntPtr CallbackFunction
        {
            get
            {
                return this.mqcbd.callbackFunction;
            }
            set
            {
                this.mqcbd.callbackFunction = value;
            }
        }

        public byte[] CallbackName
        {
            get
            {
                return this.mqcbd.callbackName;
            }
            set
            {
                this.mqcbd.callbackName = value;
            }
        }

        public int CallbackType
        {
            get
            {
                return this.mqcbd.callbackType;
            }
            set
            {
                this.mqcbd.callbackType = value;
            }
        }

        internal string CbdId
        {
            get
            {
                return this.cbdId;
            }
            set
            {
                this.cbdId = value;
            }
        }

        public int MaxMsgLength
        {
            get
            {
                return this.mqcbd.maxMsgLength;
            }
            set
            {
                this.mqcbd.maxMsgLength = value;
            }
        }

        public MQConsumer MqConsumer
        {
            get
            {
                return this.mqConsumer;
            }
            set
            {
                this.mqConsumer = value;
            }
        }

        public int Options
        {
            get
            {
                return this.mqcbd.options;
            }
            set
            {
                this.mqcbd.options = value;
            }
        }

        public MQBase.structMQCBD StructMQCBD
        {
            get
            {
                this.CheckArrayLength();
                return this.mqcbd;
            }
            set
            {
                this.mqcbd = value;
            }
        }

        public int Version
        {
            get
            {
                return this.mqcbd.version;
            }
            set
            {
                this.mqcbd.version = value;
            }
        }
    }
}

