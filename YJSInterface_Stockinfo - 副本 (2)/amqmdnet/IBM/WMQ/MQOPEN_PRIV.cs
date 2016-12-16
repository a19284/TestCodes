namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    internal class MQOPEN_PRIV : MQBase
    {
        internal const int MQOPEN_PRIV_CURRENT_VERSION = 1;
        internal structMQOPENPRIV mqopenpriv;
        private static readonly byte[] rfpMQOPEN_PRIV = new byte[] { 70, 0x4f, 80, 0x41 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        internal MQOPEN_PRIV()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.mqopenpriv = new structMQOPENPRIV();
            this.mqopenpriv.Id = rfpMQOPEN_PRIV;
            this.mqopenpriv.Version = 1;
            this.mqopenpriv.Length = this.GetLength();
            this.mqopenpriv.DefPersistence = -1;
            this.mqopenpriv.DefPutResponseType = -1;
            this.mqopenpriv.PropertyControl = -1;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.mqopenpriv);
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x1fc;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            int result = 0;
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                Marshal.Copy(b, Offset, zero, length);
                this.mqopenpriv = (structMQOPENPRIV) Marshal.PtrToStructure(zero, typeof(structMQOPENPRIV));
                Marshal.FreeCoTaskMem(zero);
                result = Offset + length;
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x1fb;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                Marshal.StructureToPtr(this.mqopenpriv, zero, false);
                Marshal.Copy(zero, b, Offset, length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, length);
            }
            return length;
        }

        public int DefPersistence
        {
            get
            {
                return this.mqopenpriv.DefPersistence;
            }
            set
            {
                this.mqopenpriv.DefPersistence = value;
            }
        }

        public int DefPutResponseType
        {
            get
            {
                return this.mqopenpriv.DefPutResponseType;
            }
            set
            {
                this.mqopenpriv.DefPutResponseType = value;
            }
        }

        public int DefReadAhead
        {
            get
            {
                return this.mqopenpriv.DefReadAhead;
            }
            set
            {
                this.mqopenpriv.DefReadAhead = value;
            }
        }

        public byte[] Id
        {
            get
            {
                return (byte[]) this.mqopenpriv.Id.Clone();
            }
            set
            {
                value.CopyTo(this.mqopenpriv.Id, 0);
            }
        }

        public int Length
        {
            get
            {
                return this.mqopenpriv.Length;
            }
            set
            {
                this.mqopenpriv.Length = value;
            }
        }

        public int PropertyControl
        {
            get
            {
                return this.mqopenpriv.PropertyControl;
            }
        }

        public int Version
        {
            get
            {
                return this.mqopenpriv.Version;
            }
            set
            {
                this.mqopenpriv.Version = value;
            }
        }
    }
}

