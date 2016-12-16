namespace IBM.WMQ
{
    using System;

    public class MQProxyMessage : MQBaseObject
    {
        private int actualMsgLength;
        private long addedTime;
        private int compCode;
        private byte[] msgData;
        private MQMessageDescriptor msgDesc;
        private int msgDescByteSize;
        private int msgLength;
        private byte[] msgToken = new byte[0x10];
        private MQProxyMessage newer;
        private MQProxyMessage older;
        private int reason;
        private string resolvedQName;
        public const int rpqMS_MESSAGE = 1;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private int selectionIndex;
        private short status;
        private int type;

        internal MQProxyMessage(int msgLength)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { msgLength });
            this.msgLength = msgLength;
            this.msgData = new byte[msgLength];
        }

        internal void SetTransactional()
        {
            uint method = 0x5ea;
            this.TrEntry(method);
            this.type |= 2;
            base.TrExit(method);
        }

        internal int ActualMsgLength
        {
            get
            {
                return this.actualMsgLength;
            }
            set
            {
                this.actualMsgLength = value;
            }
        }

        internal long AddedTime
        {
            get
            {
                return this.addedTime;
            }
            set
            {
                this.addedTime = value;
            }
        }

        internal int CompCode
        {
            get
            {
                return this.compCode;
            }
            set
            {
                this.compCode = value;
            }
        }

        internal byte[] MsgData
        {
            get
            {
                return this.msgData;
            }
            set
            {
                Array.Copy(value, this.msgData, this.msgData.Length);
            }
        }

        internal MQMessageDescriptor MsgDesc
        {
            get
            {
                return this.msgDesc;
            }
            set
            {
                this.msgDesc = value;
            }
        }

        internal int MsgDescByteSize
        {
            get
            {
                return this.msgDescByteSize;
            }
            set
            {
                this.msgDescByteSize = value;
            }
        }

        internal int MsgLength
        {
            get
            {
                return this.msgLength;
            }
            set
            {
                this.msgLength = value;
            }
        }

        internal byte[] MsgToken
        {
            get
            {
                return this.msgToken;
            }
            set
            {
                this.msgToken = value;
            }
        }

        internal MQProxyMessage Newer
        {
            get
            {
                return this.newer;
            }
            set
            {
                this.newer = value;
            }
        }

        internal MQProxyMessage Older
        {
            get
            {
                return this.older;
            }
            set
            {
                this.older = value;
            }
        }

        internal int Reason
        {
            get
            {
                return this.reason;
            }
            set
            {
                this.reason = value;
            }
        }

        internal string ResolvedQName
        {
            get
            {
                return this.resolvedQName;
            }
            set
            {
                this.resolvedQName = value;
            }
        }

        internal int SelectionIndex
        {
            get
            {
                return this.selectionIndex;
            }
            set
            {
                this.selectionIndex = value;
            }
        }

        internal short Status
        {
            get
            {
                return this.status;
            }
            set
            {
                this.status = value;
            }
        }

        internal bool Transactional
        {
            get
            {
                bool flag = (this.type & 1) == 2;
                base.TrText("Is this message a transactional = " + flag);
                return flag;
            }
        }

        internal int Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }
    }
}

