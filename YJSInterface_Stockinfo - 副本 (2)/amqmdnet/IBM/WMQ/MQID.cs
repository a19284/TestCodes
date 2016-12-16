namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    internal class MQID : MQBase
    {
        internal static readonly int FIXED_LENGTH = 0x66;
        internal structMQID id;
        private static readonly byte[] rfpID_ID = new byte[] { 0x49, 0x44, 0x20, 0x20 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        internal MQID()
        {
            int num;
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.id = new structMQID();
            this.id.Id = rfpID_ID;
            this.id.ChannelName = new byte[20];
            this.id.QueueManagerName = new byte[0x30];
            this.id.HdrCompList = new byte[2];
            for (num = 0; num < 2; num++)
            {
                this.id.HdrCompList[num] = 0xff;
            }
            this.id.MsgCompList = new byte[0x10];
            for (num = 0; num < 0x10; num++)
            {
                this.id.MsgCompList[num] = 0xff;
            }
            this.id.ProductIdentifier = new byte[12];
            this.id.QueueManagerId = new byte[0x30];
            this.id.Pal = new ushort[10];
            this.id.R = new byte[12];
        }

        internal static MQManagedClientException ErrorException(uint ErrorCode, uint ErrorData, string ChannelName)
        {
            switch (ErrorCode)
            {
                case 0xf2:
                    return new MQManagedClientException(0x20009551, ErrorCode, ErrorData, ChannelName, null, null, 2, 0x893);
            }
            return new MQManagedClientException(0x20009503, ErrorCode, ErrorData, ChannelName, null, null, 2, 0x893);
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.id);
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 200;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            int result = 0;
            try
            {
                if (zero == IntPtr.Zero)
                {
                    zero = Marshal.AllocCoTaskMem(length);
                }
                if (length > (b.Length - Offset))
                {
                    length = b.Length - Offset;
                }
                Marshal.Copy(b, Offset, zero, length);
                this.id = (structMQID) Marshal.PtrToStructure(zero, typeof(structMQID));
                Marshal.FreeCoTaskMem(zero);
                result = Offset + length;
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        internal void SetPal(ushort[] val)
        {
            Array.Copy(val, this.id.Pal, val.Length);
            if (val.Length < 10)
            {
                for (int i = val.Length; i < 10; i++)
                {
                    this.id.Pal[i] = 0xffff;
                }
            }
        }

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0xc7;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                Marshal.StructureToPtr(this.id, zero, false);
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

