namespace IBM.WMQ.Nmqi
{
    using System;
    using System.Collections;

    public class NmqiFactory
    {
        public const int LOCAL_CLIENT = 1;
        public const int LOCAL_SERVER = 0;
        private static Hashtable mqiCache = new Hashtable();
        public const int REMOTE = 2;
        public const string sccsid = "%Z% %W% %I% %E% %U%";
        private static readonly object synchronizer = new object();

        public static NmqiEnvironment GetInstance(NmqiPropertyHandler properties)
        {
            NmqiEnvironment environment = null;
            lock (synchronizer)
            {
                environment = (NmqiEnvironment) mqiCache[Type.GetType("IBM.WMQ.Nmqi.NmqiEnvironment").ToString()];
                if (environment == null)
                {
                    environment = new NmqiEnvironment(properties);
                    string key = Type.GetType("IBM.WMQ.Nmqi.NmqiEnvironment").ToString();
                    mqiCache.Add(key, environment);
                }
            }
            return environment;
        }
    }
}

