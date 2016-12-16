namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    internal class MQFAPMQCNO : MQBase
    {
        internal structMQFAPMQCNO mqfapmqcno;
        private static readonly byte[] rfpFAPMQCNO_ID = new byte[] { 70, 0x43, 0x4e, 0x4f };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        internal MQFAPMQCNO()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.mqfapmqcno = new structMQFAPMQCNO();
            this.mqfapmqcno.Id = rfpFAPMQCNO_ID;
            this.mqfapmqcno.Version = 1;
            this.mqfapmqcno.ConnTag = new byte[0x80];
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.mqfapmqcno);
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0xc2;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            int result = 0;
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                Marshal.Copy(b, Offset, zero, length);
                this.mqfapmqcno = (structMQFAPMQCNO) Marshal.PtrToStructure(zero, typeof(structMQFAPMQCNO));
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
            uint method = 0xc1;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                Marshal.StructureToPtr(this.mqfapmqcno, zero, false);
                Marshal.Copy(zero, b, Offset, length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, length);
            }
            return length;
        }

        public byte[] ConnectionId
        {
            get
            {
                byte[] destinationArray = new byte[0x18];
                Array.Copy(this.mqfapmqcno.ConnTag, 0, destinationArray, 0, 0x18);
                return destinationArray;
            }
            set
            {
                value.CopyTo(this.mqfapmqcno.ConnTag, 0);
            }
        }

        public byte[] ConnTag
        {
            get
            {
                return (byte[]) this.mqfapmqcno.ConnTag.Clone();
            }
            set
            {
                value.CopyTo(this.mqfapmqcno.ConnTag, 0);
            }
        }
    }
}

