namespace IBM.WMQ.PCF
{
    using IBM.WMQ;
    using System;

    public abstract class PCFHeader
    {
        protected int parameter;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        protected int strucLength;
        protected int type;

        protected PCFHeader()
        {
        }

        public abstract void Initialize(MQMessage message);
        public abstract int Write(MQMessage message);

        public int Parameter
        {
            get
            {
                return this.parameter;
            }
            set
            {
                this.parameter = value;
            }
        }

        public int Size
        {
            get
            {
                return this.strucLength;
            }
        }

        public int Type
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

