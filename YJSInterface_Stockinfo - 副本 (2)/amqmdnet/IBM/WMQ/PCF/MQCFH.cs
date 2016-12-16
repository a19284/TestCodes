namespace IBM.WMQ.PCF
{
    using IBM.WMQ;
    using System;

    public class MQCFH : PCFHeader
    {
        private int command;
        public int compCode;
        private int control;
        private int msgSeqNumber;
        public int parameterCount;
        public int reason;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private int version;

        public MQCFH()
        {
            this.version = 1;
            this.msgSeqNumber = 1;
            this.control = 1;
            base.type = 1;
            base.strucLength = 0x24;
            this.command = 0;
            this.parameterCount = 0;
        }

        public MQCFH(MQMessage message)
        {
            this.version = 1;
            this.msgSeqNumber = 1;
            this.control = 1;
            this.Initialize(message);
        }

        public MQCFH(int cmd, int paramCount)
        {
            this.version = 1;
            this.msgSeqNumber = 1;
            this.control = 1;
            base.type = 1;
            base.strucLength = 0x24;
            this.version = 1;
            this.command = cmd;
            this.msgSeqNumber = 1;
            this.control = 1;
            this.compCode = 0;
            this.reason = 0;
            this.parameterCount = paramCount;
        }

        public override void Initialize(MQMessage message)
        {
            base.type = message.ReadInt4();
            base.strucLength = message.ReadInt4();
            this.version = message.ReadInt4();
            this.command = message.ReadInt4();
            this.msgSeqNumber = message.ReadInt4();
            this.control = message.ReadInt4();
            this.compCode = message.ReadInt4();
            this.reason = message.ReadInt4();
            this.parameterCount = message.ReadInt4();
        }

        public override int Write(MQMessage message)
        {
            return Write(message, this.command, this.parameterCount);
        }

        public static int Write(MQMessage message, int command, int paramCount)
        {
            return Write(message, command, paramCount, 1, 1);
        }

        public static int Write(MQMessage message, int command, int paramCount, int type, int version)
        {
            message.WriteInt4(type);
            message.WriteInt4(0x24);
            message.WriteInt4(version);
            message.WriteInt4(command);
            message.WriteInt4(1);
            message.WriteInt4(1);
            message.WriteInt4(0);
            message.WriteInt4(0);
            message.WriteInt4(paramCount);
            return 0x24;
        }

        public int Command
        {
            get
            {
                return this.command;
            }
            set
            {
                this.command = value;
            }
        }

        public int CompCode
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

        public int Control
        {
            get
            {
                return this.control;
            }
            set
            {
                this.control = value;
            }
        }

        public int MsgSeqNumber
        {
            get
            {
                return this.msgSeqNumber;
            }
            set
            {
                this.msgSeqNumber = value;
            }
        }

        public int Reason
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
    }
}

