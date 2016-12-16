namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class MQASYNC_MESSAGE : MQBase
    {
        internal structMQASYNC_MESSAGE asyncMsg;
        internal MQMessageDescriptor msgDescriptor;
        internal string resolvedQName;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal static int SIZE_TO_MSG_SEG1 = 0x18;
        internal const int SIZE_TO_QNAME_SEG0 = 0x37;

        internal MQASYNC_MESSAGE()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.asyncMsg = new structMQASYNC_MESSAGE();
            this.asyncMsg.msgToken = new byte[0x10];
            this.msgDescriptor = new MQMessageDescriptor();
        }

        internal int GetSegmentLength()
        {
            uint method = 9;
            this.TrEntry(method);
            int result = 0;
            try
            {
                if (this.asyncMsg.segmentIndex == 0)
                {
                    return (Marshal.SizeOf(this.asyncMsg) + this.msgDescriptor.GetVersionLength());
                }
                result = SIZE_TO_MSG_SEG1;
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 10;
            this.TrEntry(method, new object[] { b, Offset });
            short num2 = BitConverter.ToInt16(b, Offset + 20);
            int cb = 0;
            IntPtr zero = IntPtr.Zero;
            int result = 0;
            try
            {
                if (num2 != 0)
                {
                    cb = Marshal.SizeOf(new structMQASYNC_MESSAGE_SEG1());
                    zero = Marshal.AllocCoTaskMem(cb);
                    Marshal.Copy(b, Offset, zero, cb);
                    structMQASYNC_MESSAGE_SEG1 tmqasync_message_seg = (structMQASYNC_MESSAGE_SEG1) Marshal.PtrToStructure(zero, typeof(structMQASYNC_MESSAGE_SEG1));
                    this.asyncMsg.version = tmqasync_message_seg.version;
                    this.asyncMsg.hObj = tmqasync_message_seg.hObj;
                    this.asyncMsg.messageIndex = tmqasync_message_seg.messageIndex;
                    this.asyncMsg.globalMessageIndex = tmqasync_message_seg.globalMessageIndex;
                    this.asyncMsg.segmentLength = tmqasync_message_seg.segmentLength;
                    this.asyncMsg.segmentIndex = tmqasync_message_seg.segmentIndex;
                    this.asyncMsg.selectionIndex = tmqasync_message_seg.selectionIndex;
                    this.asyncMsg.reasonCode = 0;
                    this.asyncMsg.status = 0;
                    this.asyncMsg.totalMsgLength = 0;
                    this.asyncMsg.actualMsgLength = 0;
                    this.asyncMsg.msgToken = null;
                    this.asyncMsg.resolvedQNameLen = 0;
                    Marshal.FreeCoTaskMem(zero);
                    return (cb + Offset);
                }
                cb = Marshal.SizeOf(this.asyncMsg);
                zero = Marshal.AllocCoTaskMem(cb);
                Marshal.Copy(b, Offset, zero, cb);
                this.asyncMsg = (structMQASYNC_MESSAGE) Marshal.PtrToStructure(zero, typeof(structMQASYNC_MESSAGE));
                int resolvedQNameLen = this.asyncMsg.resolvedQNameLen;
                int index = Offset + 0x37;
                if (resolvedQNameLen != 0)
                {
                    if (resolvedQNameLen < 0x30)
                    {
                        this.resolvedQName = Encoding.ASCII.GetString(b, index, resolvedQNameLen - 1);
                    }
                    else
                    {
                        this.resolvedQName = Encoding.ASCII.GetString(b, index, resolvedQNameLen);
                    }
                }
                index += ((resolvedQNameLen + 2) & -4) + 1;
                result = this.msgDescriptor.ReadStruct(b, index);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        internal struct structMQASYNC_MESSAGE
        {
            public int version;
            public int hObj;
            public int messageIndex;
            public int globalMessageIndex;
            public int segmentLength;
            public short segmentIndex;
            public short selectionIndex;
            public int reasonCode;
            public int totalMsgLength;
            public int actualMsgLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
            public byte[] msgToken;
            public short status;
            public byte resolvedQNameLen;
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        internal struct structMQASYNC_MESSAGE_SEG1
        {
            public int version;
            public int hObj;
            public int messageIndex;
            public int globalMessageIndex;
            public int segmentLength;
            public short segmentIndex;
            public short selectionIndex;
        }
    }
}

