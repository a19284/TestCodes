namespace IBM.WMQ.Nmqi
{
    using System;

    public class BindingsHobj : NmqiObject, IBM.WMQ.Nmqi.Hobj
    {
        private BindingsHconn hconn;
        public BindingsHobj localHobj;
        private MQCBD mqcbd;
        public const string sccsid = "%Z% %W% %I% %E% %U%";
        public int value_;

        protected BindingsHobj(NmqiEnvironment env) : base(env)
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { env });
            this.value_ = 0;
        }

        public BindingsHobj(NmqiEnvironment env, int value) : base(env)
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { env, value });
            this.value_ = value;
        }

        public bool Equals(IBM.WMQ.Nmqi.Hobj obj)
        {
            uint method = 0x315;
            this.TrEntry(method, new object[] { obj });
            bool result = obj.Handle.Equals(obj.Handle);
            base.TrExit(method, result);
            return result;
        }

        public static BindingsHobj GetBindingsHobj(NmqiEnvironment env, IBM.WMQ.Nmqi.Hobj hobj)
        {
            if (hobj is BindingsHobj)
            {
                return (BindingsHobj) hobj;
            }
            if (hobj.Equals(0))
            {
                return new BindingsHobj(env, 0);
            }
            return new BindingsHobj(env, -1);
        }

        public static IBM.WMQ.Nmqi.Hobj GetHobj(NmqiEnvironment env, BindingsHobj localhobj)
        {
            int initHobjValue = localhobj.value_;
            switch (initHobjValue)
            {
                case -1:
                    return new HobjAdapter(initHobjValue);

                case 0:
                    return new HobjAdapter(initHobjValue);
            }
            return localhobj;
        }

        public void SetHobj(Phobj phobj)
        {
            uint method = 0x314;
            this.TrEntry(method, new object[] { phobj });
            IBM.WMQ.Nmqi.Hobj hobj = null;
            switch (this.value_)
            {
                case -1:
                    hobj = new HobjAdapter();
                    break;

                case 0:
                    hobj = new HobjAdapter();
                    break;

                default:
                    hobj = this;
                    break;
            }
            phobj.HOBJ = hobj;
            base.TrExit(method);
        }

        public override string ToString()
        {
            uint method = 0x313;
            this.TrEntry(method);
            string result = "0x" + Convert.ToString(this.value_);
            base.TrExit(method, result);
            return result;
        }

        public int Handle
        {
            get
            {
                return this.value_;
            }
            set
            {
                this.value_ = value;
            }
        }

        public BindingsHconn Hconn
        {
            get
            {
                return this.hconn;
            }
            set
            {
                this.hconn = value;
            }
        }

        public BindingsHobj Hobj
        {
            get
            {
                return this.localHobj;
            }
            set
            {
                this.localHobj = value;
            }
        }

        public MQCBD Mqcbd
        {
            get
            {
                return this.mqcbd;
            }
            set
            {
                this.mqcbd = value;
            }
        }

        public int Value
        {
            get
            {
                return this.value_;
            }
            set
            {
                this.value_ = value;
            }
        }
    }
}

