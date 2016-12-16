namespace IBM.WMQ.PCF
{
    using IBM.WMQ;
    using System;

    public class MQCFIL : PCFParameter
    {
        private int count;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private int[] values;

        public MQCFIL()
        {
            base.type = 5;
            base.strucLength = 0x10;
            base.parameter = 0;
        }

        public MQCFIL(MQMessage message)
        {
            this.Initialize(message);
        }

        public MQCFIL(int param, int[] vals)
        {
            base.type = 5;
            base.strucLength = 0x10 + (vals.Length * 4);
            base.strucLength += (4 - (base.strucLength % 4)) % 4;
            base.parameter = param;
            this.values = (int[]) vals.Clone();
        }

        public override object GetValue()
        {
            return this.values;
        }

        public override void Initialize(MQMessage message)
        {
            base.type = message.ReadInt4();
            base.strucLength = message.ReadInt4();
            base.parameter = message.ReadInt4();
            this.count = message.ReadInt4();
            this.values = new int[this.count];
            for (int i = 0; i < this.count; i++)
            {
                this.values[i] = message.ReadInt4();
            }
            int n = (4 - (base.strucLength % 4)) % 4;
            message.SkipBytes(n);
        }

        public override void SetValue(object val)
        {
            this.values = (int[]) ((int[]) val).Clone();
            base.strucLength = 0x10;
            base.strucLength += this.values.Length * 4;
            base.strucLength += (4 - (base.strucLength % 4)) % 4;
            this.count = this.values.Length;
        }

        public void SetValues(int[] vals)
        {
            base.strucLength = 0x10 + (vals.Length * 4);
            this.count = vals.Length;
            this.values = (int[]) vals.Clone();
        }

        public override int Write(MQMessage message)
        {
            return Write(message, base.parameter, this.values);
        }

        public static int Write(MQMessage message, int param, int[] vals)
        {
            int num = (0x10 + (vals.Length * 4)) % 4;
            message.WriteInt4(5);
            message.WriteInt4((0x10 + (vals.Length * 4)) + num);
            message.WriteInt4(param);
            message.WriteInt4(vals.Length);
            for (int i = 0; i < vals.Length; i++)
            {
                message.WriteInt4(vals[i]);
            }
            message.Write(new byte[num]);
            return ((0x10 + (vals.Length * 4)) + num);
        }

        public string StringValue
        {
            get
            {
                string str = "";
                for (int i = 0; i < this.values.Length; i++)
                {
                    if (i > 0)
                    {
                        str = str + "\n";
                    }
                    str = str + this.values[i].ToString();
                }
                return str;
            }
        }
    }
}

