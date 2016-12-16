namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;

    public class Phconn : MQBase
    {
        private Hconn hconn;
        public const string sccsid = "%Z% %W% %I% %E% %U%";

        public Phconn(NmqiEnvironment env)
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { env });
            this.hconn = null;
        }

        public override string ToString()
        {
            uint method = 0x2ed;
            this.TrEntry(method);
            string result = (this.hconn != null) ? this.hconn.ToString() : "<null>";
            base.TrExit(method, result);
            return result;
        }

        public Hconn HConn
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
    }
}

