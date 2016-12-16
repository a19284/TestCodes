namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    internal class MQNOTIFICATION : MQBase
    {
        internal structMQNOTIFICATION notify;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        internal MQNOTIFICATION()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.notify = new structMQNOTIFICATION();
            this.notify.version = 1;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.notify);
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x1f3;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                Marshal.Copy(b, Offset, zero, length);
                this.notify = (structMQNOTIFICATION) Marshal.PtrToStructure(zero, typeof(structMQNOTIFICATION));
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, length);
            }
            return length;
        }

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x1f2;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                Marshal.StructureToPtr(this.notify, zero, false);
                Marshal.Copy(zero, b, Offset, length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, length);
            }
            return length;
        }
    }
}

