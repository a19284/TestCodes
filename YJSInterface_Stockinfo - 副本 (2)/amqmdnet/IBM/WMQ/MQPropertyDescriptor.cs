namespace IBM.WMQ
{
    using System;

    public class MQPropertyDescriptor : MQBase
    {
        internal MQPD pd;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public MQPropertyDescriptor()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.pd.strucId = new byte[] { 80, 0x44, 0x20, 0x20 };
            this.pd.Version = 1;
            this.pd.Options = 0;
            this.pd.Support = 1;
            this.pd.Context = 0;
            this.pd.CopyOptions = 0x16;
        }

        internal bool IsDefault()
        {
            uint method = 0x200;
            this.TrEntry(method);
            bool result = true;
            if (this.Context != 0)
            {
                result = false;
            }
            if (this.CopyOptions != 0x16)
            {
                result = false;
            }
            if (this.Options != 0)
            {
                result = false;
            }
            if (this.Support != 1)
            {
                result = false;
            }
            base.TrExit(method, result);
            return result;
        }

        public int Context
        {
            get
            {
                return this.pd.Context;
            }
            set
            {
                this.pd.Context = value;
            }
        }

        public int CopyOptions
        {
            get
            {
                return this.pd.CopyOptions;
            }
            set
            {
                this.pd.CopyOptions = value;
            }
        }

        public int Options
        {
            get
            {
                return this.pd.Options;
            }
            set
            {
                this.pd.Options = value;
            }
        }

        public int Support
        {
            get
            {
                return this.pd.Support;
            }
            set
            {
                this.pd.Support = value;
            }
        }
    }
}

