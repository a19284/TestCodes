namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    internal class MQSOCKACT : MQBase
    {
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal structMQSockat sockatFlow;

        internal MQSOCKACT() : this(0, 0, 0, 0, 0)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        internal MQSOCKACT(int convId, int requestId, int type, int parm1, int parm2)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { convId, requestId, type, parm1, parm2 });
            this.sockatFlow = new structMQSockat();
            this.sockatFlow.conversationId = convId;
            this.sockatFlow.requestId = requestId;
            this.sockatFlow.type = type;
            this.sockatFlow.parm1 = parm1;
            this.sockatFlow.parm2 = parm2;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.sockatFlow);
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 630;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            int result = 0;
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                Marshal.Copy(b, Offset, zero, length);
                this.sockatFlow = (structMQSockat) Marshal.PtrToStructure(zero, typeof(structMQSockat));
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
            uint method = 0x275;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                Marshal.StructureToPtr(this.sockatFlow, zero, false);
                Marshal.Copy(zero, b, Offset, length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, length);
            }
            return length;
        }

        internal int ConversationID
        {
            get
            {
                return this.sockatFlow.conversationId;
            }
            set
            {
                this.sockatFlow.conversationId = value;
            }
        }

        internal int Parm1
        {
            get
            {
                return this.sockatFlow.parm1;
            }
            set
            {
                this.sockatFlow.parm1 = value;
            }
        }

        internal int Parm2
        {
            get
            {
                return this.sockatFlow.parm2;
            }
            set
            {
                this.sockatFlow.parm2 = value;
            }
        }

        internal int RequestID
        {
            get
            {
                return this.sockatFlow.requestId;
            }
            set
            {
                this.sockatFlow.requestId = value;
            }
        }

        internal int Type
        {
            get
            {
                return this.sockatFlow.type;
            }
            set
            {
                this.sockatFlow.type = value;
            }
        }
    }
}

