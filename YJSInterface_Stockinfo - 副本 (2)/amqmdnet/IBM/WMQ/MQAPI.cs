namespace IBM.WMQ
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;

    internal class MQAPI : MQBase
    {
        internal structMQAPI mqapi;
        internal const int MQAPI_STRUCT_SIZE = 0x10;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        internal MQAPI()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.mqapi = new structMQAPI();
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.mqapi);
        }

        internal void Initialize(int callLength, int handle)
        {
            uint method = 2;
            this.TrEntry(method, new object[] { callLength, handle });
            try
            {
                this.SetCallLength(callLength);
                this.mqapi.CompCode = 0;
                this.mqapi.Reason = 0;
                this.mqapi.Handle = handle;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void InitializeForXA(int callLength, int rmid, int flags)
        {
            uint method = 2;
            this.TrEntry(method);
            try
            {
                this.SetCallLength(callLength);
                this.mqapi.ReturnCode = 0;
                this.mqapi.Rmid = rmid;
                this.mqapi.Flags = flags;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 4;
            this.TrEntry(method);
            int num2 = 0;
            try
            {
                IntPtr zero = IntPtr.Zero;
                int length = this.GetLength();
                zero = Marshal.AllocCoTaskMem(length);
                Marshal.Copy(b, Offset, zero, length);
                this.mqapi = (structMQAPI) Marshal.PtrToStructure(zero, typeof(structMQAPI));
                Marshal.FreeCoTaskMem(zero);
                num2 = Offset + length;
            }
            finally
            {
                base.TrExit(method);
            }
            return num2;
        }

        internal void SetCallLength(int calllen)
        {
            this.mqapi.CallLength = IPAddress.NetworkToHostOrder(calllen);
        }

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 3;
            this.TrEntry(method);
            int cb = 0;
            try
            {
                IntPtr zero = IntPtr.Zero;
                cb = this.GetLength();
                zero = Marshal.AllocCoTaskMem(cb);
                Marshal.StructureToPtr(this.mqapi, zero, false);
                Marshal.Copy(zero, b, Offset, cb);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method);
            }
            return cb;
        }
    }
}

