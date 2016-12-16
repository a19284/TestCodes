namespace IBM.WMQ.PCF
{
    using IBM.WMQ;
    using System;

    public class MQCFIN : PCFParameter
    {
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private int value;

        public MQCFIN()
        {
            base.type = 3;
            base.strucLength = 0x10;
            base.parameter = 0;
            this.value = 0;
        }

        public MQCFIN(MQMessage message)
        {
            this.Initialize(message);
        }

        public MQCFIN(int param, int val)
        {
            base.type = 3;
            base.strucLength = 0x10;
            base.parameter = param;
            this.value = val;
        }

        public override object GetValue()
        {
            return this.value;
        }

        public override void Initialize(MQMessage message)
        {
            base.type = message.ReadInt4();
            base.strucLength = message.ReadInt4();
            base.parameter = message.ReadInt4();
            this.value = message.ReadInt4();
        }

        public void SetValue(int val)
        {
            this.value = val;
        }

        public override void SetValue(object val)
        {
            this.value = (int) val;
        }

        public override int Write(MQMessage message)
        {
            return Write(message, base.parameter, this.value);
        }

        public static int Write(MQMessage message, int parameter, int val)
        {
            message.WriteInt4(3);
            message.WriteInt4(0x10);
            message.WriteInt4(parameter);
            message.WriteInt4(val);
            return 0x10;
        }

        public string StringValue
        {
            get
            {
                return this.value.ToString();
            }
        }
    }
}

