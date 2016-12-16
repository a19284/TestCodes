namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Collections;

    internal class MQFAPConnectionPool : NmqiObject
    {
        private Hashtable allConnectionSpecs;
        private Lock callSync;
        private MQFAP fap;
        public const string sccsid = "%Z% %W% %I% %E% %U%";

        internal MQFAPConnectionPool(NmqiEnvironment nmqiEnv, MQFAP fap) : base(nmqiEnv)
        {
            this.allConnectionSpecs = new Hashtable();
            this.callSync = new Lock();
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { nmqiEnv, fap });
            this.fap = fap;
        }

        public MQSession GetSession(int connectOptions, MQSSLConfigOptions sslConfigOptions, MQConnectionSecurityParameters mqcsp, MQChannelDefinition mqcd, string qMgrName, int nmqiFlags, string uidFlowUserid, string uidFlowPassword, int ccsid, int maxFapLevel)
        {
            uint method = 0x5e7;
            this.TrEntry(method, new object[] { connectOptions, mqcsp, mqcd, qMgrName, nmqiFlags, uidFlowUserid, (uidFlowPassword == null) ? uidFlowPassword : "********", ccsid, maxFapLevel });
            MQSession session = null;
            try
            {
                MQConnectionSpecification specification = new MQConnectionSpecification(base.env, this, mqcd, sslConfigOptions, nmqiFlags, uidFlowUserid, uidFlowPassword, ccsid);
                try
                {
                    this.callSync.Acquire();
                    object obj2 = null;
                    foreach (object obj3 in this.allConnectionSpecs.Keys)
                    {
                        MQConnectionSpecification specification2 = (MQConnectionSpecification) obj3;
                        if (specification2.Equals(specification))
                        {
                            specification = specification2;
                            obj2 = specification2;
                            base.TrText(method, "Found a matching connection spec..");
                            break;
                        }
                    }
                    if (obj2 == null)
                    {
                        base.TrText(method, "Couldnt find a matching connection spec. Adding new one into table");
                        this.allConnectionSpecs.Add(specification, specification);
                    }
                }
                finally
                {
                    this.callSync.Release();
                }
                session = specification.GetSession(mqcd, sslConfigOptions, mqcsp, this.fap, qMgrName, connectOptions, maxFapLevel);
                try
                {
                    this.callSync.Acquire();
                    if (!this.allConnectionSpecs.ContainsKey(specification))
                    {
                        this.allConnectionSpecs.Add(specification, specification);
                    }
                }
                finally
                {
                    this.callSync.Release();
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return session;
        }

        internal void RemoveSpec(MQConnectionSpecification connectionSpec)
        {
            uint method = 0x5e8;
            this.TrEntry(method, new object[] { connectionSpec });
            try
            {
                this.callSync.Acquire();
                this.allConnectionSpecs.Remove(connectionSpec);
                base.TrText(method, "Connection spec has been removed from table..");
            }
            finally
            {
                this.callSync.Release();
                base.TrExit(method);
            }
        }
    }
}

