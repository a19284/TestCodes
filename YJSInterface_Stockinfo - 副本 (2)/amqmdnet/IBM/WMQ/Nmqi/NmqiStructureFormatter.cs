namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Collections;
    using System.Text;

    public class NmqiStructureFormatter : MQBase
    {
        private ArrayList fields;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private int width1;
        private int width2;

        public NmqiStructureFormatter(int width1, int width2)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { width1, width2 });
            this.fields = new ArrayList();
            this.width1 = width1;
            this.width2 = width2;
        }

        public void Add(string name, bool value)
        {
            this.AddField("", "", name, value.ToString());
        }

        public void Add(string name, ArrayList list)
        {
            string str = "";
            IEnumerator enumerator = list.GetEnumerator();
            bool flag = enumerator.MoveNext();
            while (flag)
            {
                str = str + enumerator.Current.ToString();
                if (enumerator.MoveNext())
                {
                    str = str + ",";
                }
            }
            this.AddField("'", "'", name, str);
        }

        public void Add(string name, int value)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(NmqiTools.Left(Convert.ToString(value), 1));
            builder.Append(NmqiTools.Left(" (hex ", 1));
            builder.Append(value.ToString("X"));
            builder.Append(NmqiTools.Left(")", 1));
            this.AddField("", "", name, builder.ToString());
        }

        public void Add(string name, long value)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(NmqiTools.Left(Convert.ToString(value), 1));
            builder.Append(NmqiTools.Left(" (hex ", 1));
            builder.Append(value.ToString("X"));
            builder.Append(NmqiTools.Left(")", 1));
            this.AddField("", "", name, builder.ToString());
        }

        public void Add(string name, string value)
        {
            this.AddField("'", "'", name, value);
        }

        public void Add(string name, byte[] bytes)
        {
            bool flag = true;
            StringBuilder builder = new StringBuilder(bytes.Length);
            for (int i = 0; i < bytes.Length; i++)
            {
                byte num1 = bytes[i];
                if (bytes[i] != 0)
                {
                    flag = false;
                }
                builder.Append(bytes[i]);
            }
            if (flag)
            {
                this.AddField("", "", name, "0");
            }
            else
            {
                this.AddField("", "", name, builder.ToString());
            }
        }

        public void Add(string name, int[] value)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(NmqiTools.Left(name, this.width1));
            builder.Append(NmqiTools.Left(":", this.width2));
            builder.Append("(");
            for (int i = 0; i < value.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(',');
                }
                builder.Append(value[i]);
            }
            builder.Append(")");
            this.fields.Add(builder.ToString());
        }

        public void Add(string name, sbyte[] value)
        {
            this.AddField("", "", name, NmqiTools.ArrayToHexString(value));
        }

        public void Add(string name, string[] value)
        {
            if (value != null)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(NmqiTools.Left(name, this.width1));
                builder.Append(":");
                for (int i = 0; i < value.Length; i++)
                {
                    builder.Append("(" + value + ")");
                }
                this.fields.Add(builder.ToString());
            }
        }

        public void Add(string name, object value)
        {
            this.AddField("[", "]", name, value.ToString());
        }

        private void AddField(string lquote, string rquote, string name, string value)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(NmqiTools.Left(name, this.width1));
            builder.Append(NmqiTools.Left(":", this.width2));
            builder.Append(lquote);
            builder.Append(value);
            builder.Append(rquote);
            this.fields.Add(builder.ToString());
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            IEnumerator enumerator = this.fields.GetEnumerator();
            while (enumerator.MoveNext())
            {
                builder.Append(enumerator.Current.ToString());
            }
            return builder.ToString();
        }

        public void Trace()
        {
            IEnumerator enumerator = this.fields.GetEnumerator();
            while (enumerator.MoveNext())
            {
                base.TrText(enumerator.Current.ToString());
            }
        }
    }
}

