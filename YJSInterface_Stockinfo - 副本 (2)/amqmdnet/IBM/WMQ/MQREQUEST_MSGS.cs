namespace IBM.WMQ
{
    using System;

    internal class MQREQUEST_MSGS : MQBase
    {
        internal structMQREQUEST_MSGS reqMsg;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal const int SIZE_V1_NO_SELECTION = 40;
        internal const int SIZE_V1_SELECTION_FIXED_PART = 0x40;

        internal MQREQUEST_MSGS()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.reqMsg = new structMQREQUEST_MSGS();
            this.reqMsg.version = 1;
            this.reqMsg.msgId = new byte[0x18];
            this.reqMsg.correlId = new byte[0x18];
            this.reqMsg.groupId = new byte[0x18];
            this.reqMsg.msgToken = new byte[0x10];
        }

        internal int GetLength()
        {
            uint method = 0x25b;
            this.TrEntry(method);
            int result = 0;
            if ((this.reqMsg.requestFlags & 0x10) == 0)
            {
                result = 40;
            }
            else
            {
                result = 0x40;
                if ((this.reqMsg.matchOptions & 1) != 0)
                {
                    result += 0x18;
                }
                if ((this.reqMsg.matchOptions & 2) != 0)
                {
                    result += 0x18;
                }
                if ((this.reqMsg.matchOptions & 4) != 0)
                {
                    result += 0x18;
                }
                if ((this.reqMsg.matchOptions & 0x20) != 0)
                {
                    result += 0x10;
                }
            }
            base.TrExit(method, result);
            return result;
        }

        internal void Initialize()
        {
            uint method = 0x25a;
            this.TrEntry(method);
            this.reqMsg.version = 1;
            this.reqMsg.hObj = 0;
            this.reqMsg.receivedBytes = 0;
            this.reqMsg.requestedBytes = 0;
            this.reqMsg.maxMessageLength = 0;
            this.reqMsg.getMessageOptions = 0;
            this.reqMsg.waitInterval = 0;
            this.reqMsg.queueStatus = 0;
            this.reqMsg.requestFlags = 0;
            this.reqMsg.globalMessageIndex = 0;
            this.reqMsg.selectionIndex = 0;
            this.reqMsg.mqmdVersion = 0;
            this.reqMsg.codedCharSetId = 0;
            this.reqMsg.encoding = 0;
            this.reqMsg.msgSequenceNumber = 0;
            this.reqMsg.offset = 0;
            this.reqMsg.matchOptions = 0;
            this.reqMsg.msgId = new byte[0x18];
            this.reqMsg.correlId = new byte[0x18];
            this.reqMsg.groupId = new byte[0x18];
            this.reqMsg.msgToken = new byte[0x10];
            base.TrExit(method);
        }

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x25c;
            this.TrEntry(method, new object[] { b, Offset });
            int length = this.GetLength();
            try
            {
                Buffer.BlockCopy(BitConverter.GetBytes(this.reqMsg.version), 0, b, Offset, 4);
                Offset += 4;
                Buffer.BlockCopy(BitConverter.GetBytes(this.reqMsg.hObj), 0, b, Offset, 4);
                Offset += 4;
                Buffer.BlockCopy(BitConverter.GetBytes(this.reqMsg.receivedBytes), 0, b, Offset, 4);
                Offset += 4;
                Buffer.BlockCopy(BitConverter.GetBytes(this.reqMsg.requestedBytes), 0, b, Offset, 4);
                Offset += 4;
                Buffer.BlockCopy(BitConverter.GetBytes(this.reqMsg.maxMessageLength), 0, b, Offset, 4);
                Offset += 4;
                Buffer.BlockCopy(BitConverter.GetBytes(this.reqMsg.getMessageOptions), 0, b, Offset, 4);
                Offset += 4;
                Buffer.BlockCopy(BitConverter.GetBytes(this.reqMsg.waitInterval), 0, b, Offset, 4);
                Offset += 4;
                Buffer.BlockCopy(BitConverter.GetBytes(this.reqMsg.queueStatus), 0, b, Offset, 4);
                Offset += 4;
                Buffer.BlockCopy(BitConverter.GetBytes(this.reqMsg.requestFlags), 0, b, Offset, 4);
                Offset += 4;
                Buffer.BlockCopy(BitConverter.GetBytes(this.reqMsg.globalMessageIndex), 0, b, Offset, 4);
                Offset += 4;
                if ((this.reqMsg.requestFlags & 0x10) != 0)
                {
                    Buffer.BlockCopy(BitConverter.GetBytes(this.reqMsg.selectionIndex), 0, b, Offset, 2);
                    Offset += 2;
                    Buffer.BlockCopy(BitConverter.GetBytes(this.reqMsg.mqmdVersion), 0, b, Offset, 2);
                    Offset += 2;
                    Buffer.BlockCopy(BitConverter.GetBytes(this.reqMsg.codedCharSetId), 0, b, Offset, 4);
                    Offset += 4;
                    Buffer.BlockCopy(BitConverter.GetBytes(this.reqMsg.encoding), 0, b, Offset, 4);
                    Offset += 4;
                    Buffer.BlockCopy(BitConverter.GetBytes(this.reqMsg.msgSequenceNumber), 0, b, Offset, 4);
                    Offset += 4;
                    Buffer.BlockCopy(BitConverter.GetBytes(this.reqMsg.offset), 0, b, Offset, 4);
                    Offset += 4;
                    Buffer.BlockCopy(BitConverter.GetBytes(this.reqMsg.matchOptions), 0, b, Offset, 4);
                    Offset += 4;
                }
                if ((this.reqMsg.requestFlags & 0x10) == 0)
                {
                    return length;
                }
                if ((this.reqMsg.matchOptions & 1) != 0)
                {
                    this.reqMsg.msgId.CopyTo(b, Offset);
                    Offset += 0x18;
                }
                if ((this.reqMsg.matchOptions & 2) != 0)
                {
                    this.reqMsg.correlId.CopyTo(b, Offset);
                    Offset += 0x18;
                }
                if ((this.reqMsg.matchOptions & 4) != 0)
                {
                    this.reqMsg.groupId.CopyTo(b, Offset);
                    Offset += 0x18;
                }
                if ((this.reqMsg.matchOptions & 0x20) != 0)
                {
                    this.reqMsg.msgToken.CopyTo(b, Offset);
                    Offset += 0x10;
                }
            }
            finally
            {
                base.TrExit(method, length);
            }
            return length;
        }
    }
}

