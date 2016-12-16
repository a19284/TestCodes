namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;

    public class Phobj : MQBase
    {
        private Hobj hobj;
        public const string sccsid = "%Z% %W% %I% %E% %U%";

        public Phobj(NmqiEnvironment env)
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { env });
            this.hobj = new ManagedHobj(env, -1);
        }

        public override string ToString()
        {
            uint method = 750;
            this.TrEntry(method);
            string result = (this.hobj != null) ? this.hobj.ToString() : "<null>";
            base.TrExit(method, result);
            return result;
        }

        public Hobj HOBJ
        {
            get
            {
                return this.hobj;
            }
            set
            {
                this.hobj = value;
            }
        }
    }
}

