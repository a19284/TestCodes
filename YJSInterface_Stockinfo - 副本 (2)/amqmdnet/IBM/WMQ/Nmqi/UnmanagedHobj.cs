namespace IBM.WMQ.Nmqi
{
    using System;

    public class UnmanagedHobj : NmqiObject, IBM.WMQ.Nmqi.Hobj
    {
        private UnmanagedHconn hconn;
        public UnmanagedHobj localHobj;
        private MQCBD mqcbd;
        public const string sccsid = "%Z% %W% %I% %E% %U%";
        public int value_;

        public UnmanagedHobj(NmqiEnvironment env) : base(env)
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { env });
            this.value_ = 0;
        }

        public UnmanagedHobj(NmqiEnvironment env, int value) : base(env)
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { env, value });
            this.value_ = value;
        }

        public bool Equals(IBM.WMQ.Nmqi.Hobj obj)
        {
            uint method = 0x2fc;
            this.TrEntry(method, new object[] { obj });
            bool result = obj.Handle.Equals(obj.Handle);
            base.TrExit(method, result);
            return result;
        }

        public static IBM.WMQ.Nmqi.Hobj GetHobj(NmqiEnvironment env, UnmanagedHobj localhobj)
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

        public static UnmanagedHobj GetUnmanagedHobj(NmqiEnvironment env, IBM.WMQ.Nmqi.Hobj hobj)
        {
            if (hobj is UnmanagedHobj)
            {
                return (UnmanagedHobj) hobj;
            }
            if (hobj.Equals(0))
            {
                return new UnmanagedHobj(env, 0);
            }
            return new UnmanagedHobj(env, -1);
        }

        public void SetHobj(Phobj phobj)
        {
            uint method = 0x2fb;
            this.TrEntry(method, new object[] { phobj });
            IBM.WMQ.Nmqi.Hobj hobj = null;
            try
            {
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
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public override string ToString()
        {
            uint method = 0x2fa;
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

        public UnmanagedHconn Hconn
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

        public UnmanagedHobj Hobj
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

