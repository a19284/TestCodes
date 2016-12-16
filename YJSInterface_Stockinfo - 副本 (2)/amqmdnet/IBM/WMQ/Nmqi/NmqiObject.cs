namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;

    public class NmqiObject : MQBase
    {
        public NmqiEnvironment env;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public NmqiObject(NmqiEnvironment nmqiEnv)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv });
            this.env = nmqiEnv;
        }
    }
}

