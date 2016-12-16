namespace IBM.WMQ.PCF
{
    using IBM.WMQ;
    using System;
    using System.Text;

    public class MQCFST : PCFParameter
    {
        private int codedCharSetId;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private int stringLength;
        private string stringval;

        public MQCFST()
        {
            this.stringval = "";
            base.type = 4;
            base.strucLength = 20;
            base.parameter = 0;
            this.codedCharSetId = 0;
            this.stringLength = 0;
            this.stringval = "";
        }

        public MQCFST(MQMessage message)
        {
            this.stringval = "";
            this.Initialize(message);
        }

        public MQCFST(int param, string str)
        {
            this.stringval = "";
            base.type = 4;
            base.strucLength = 20 + str.Length;
            base.strucLength += (4 - (base.strucLength % 4)) % 4;
            base.parameter = param;
            this.codedCharSetId = 0;
            this.stringLength = str.Length;
            this.stringval = str;
        }

        public override object GetValue()
        {
            return this.stringval;
        }

        public override void Initialize(MQMessage message)
        {
            base.type = message.ReadInt4();
            base.strucLength = message.ReadInt4();
            base.parameter = message.ReadInt4();
            this.codedCharSetId = message.ReadInt4();
            this.stringLength = message.ReadInt4();
            this.stringval = Encoding.ASCII.GetString(message.Read(this.stringLength), 0, this.stringLength);
            int n = (4 - ((20 + this.stringLength) % 4)) % 4;
            message.SkipBytes(n);
        }

        public void SetString(string str)
        {
            this.stringval = str;
            this.stringLength = this.stringval.Length;
            base.strucLength = 20 + this.stringLength;
            base.strucLength += (4 - (base.strucLength % 4)) % 4;
        }

        public override void SetValue(object val)
        {
            this.stringval = (string) val;
            this.stringLength = this.stringval.Length;
            base.strucLength = 20 + this.stringLength;
            base.strucLength += (4 - (base.strucLength % 4)) % 4;
        }

        public override int Write(MQMessage message)
        {
            return Write(message, base.parameter, this.stringval);
        }

        public static int Write(MQMessage message, int param, string val)
        {
            int num = 0;
            if (val != null)
            {
                int num2 = (4 - ((20 + val.Length) % 4)) % 4;
                message.WriteInt4(4);
                message.WriteInt4((20 + val.Length) + num2);
                message.WriteInt4(param);
                message.WriteInt4(0);
                message.WriteInt4(val.Length);
                byte[] bytes = new byte[val.Length];
                Encoding.ASCII.GetBytes(val, 0, val.Length, bytes, 0);
                message.Write(bytes);
                message.Write(new byte[num2]);
                num = (20 + val.Length) + num2;
            }
            return num;
        }
    }
}

