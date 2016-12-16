namespace IBM.WMQ.PCF
{
    using IBM.WMQ;
    using System;
    using System.Text;

    public class MQCFSL : PCFParameter
    {
        private int codedCharSetId;
        private int count;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private int stringLength;
        private string[] strings;

        public MQCFSL()
        {
            base.type = 6;
            base.strucLength = 0x18;
            base.parameter = 0;
            this.codedCharSetId = 0;
            this.count = 0;
            this.stringLength = 0;
            this.strings = null;
        }

        public MQCFSL(MQMessage message)
        {
            this.Initialize(message);
        }

        public MQCFSL(int param, string[] strs)
        {
            this.count = strs.Length;
            if (this.count > 0)
            {
                this.stringLength = strs[0].Length;
            }
            base.strucLength = 0x18;
            base.strucLength += this.count * this.stringLength;
            base.strucLength += (4 - (base.strucLength % 4)) % 4;
            this.strings = (string[]) strs.Clone();
            base.parameter = param;
        }

        public override object GetValue()
        {
            return this.strings;
        }

        public override void Initialize(MQMessage message)
        {
            base.type = message.ReadInt4();
            base.strucLength = message.ReadInt4();
            base.parameter = message.ReadInt4();
            this.codedCharSetId = message.ReadInt4();
            this.count = message.ReadInt4();
            this.stringLength = message.ReadInt4();
            this.strings = new string[this.count];
            for (int i = 0; i < this.count; i++)
            {
                this.strings[i] = Encoding.ASCII.GetString(message.Read(this.stringLength), 0, this.stringLength);
            }
            int n = (4 - (base.strucLength % 4)) % 4;
            message.SkipBytes(n);
        }

        public void SetStrings(string[] strs)
        {
            if (strs != null)
            {
                this.strings = (string[]) strs.Clone();
                this.count = strs.Length;
                this.stringLength = strs[0].Length;
                base.strucLength = 0x18;
                base.strucLength += this.count * this.stringLength;
                base.strucLength += (4 - (base.strucLength % 4)) % 4;
            }
        }

        public override void SetValue(object val)
        {
            if (val != null)
            {
                this.strings = (string[]) ((string[]) val).Clone();
                this.count = this.strings.Length;
                this.stringLength = this.strings[0].Length;
                base.strucLength = 0x18;
                base.strucLength += this.count * this.stringLength;
                base.strucLength += (4 - (base.strucLength % 4)) % 4;
            }
        }

        public override int Write(MQMessage message)
        {
            return Write(message, base.parameter, this.strings);
        }

        public static int Write(MQMessage message, int param, string[] strs)
        {
            int num = 0;
            if (strs == null)
            {
                return num;
            }
            int num2 = (0x18 + (strs.Length * strs[0].Length)) % 4;
            message.WriteInt4(6);
            message.WriteInt4((0x18 + (strs.Length * strs[0].Length)) + num2);
            message.WriteInt4(param);
            message.WriteInt4(0);
            message.WriteInt4(strs.Length);
            message.WriteInt4(strs[0].Length);
            for (int i = 0; i < strs.Length; i++)
            {
                byte[] bytes = new byte[strs[0].Length];
                Encoding.ASCII.GetBytes(strs[i], 0, strs[0].Length, bytes, 0);
                message.Write(bytes);
            }
            message.Write(new byte[num2]);
            return ((0x18 + (strs.Length * strs[0].Length)) + num2);
        }

        public object Value
        {
            get
            {
                return this.strings;
            }
            set
            {
                this.strings = (string[]) value;
            }
        }
    }
}

