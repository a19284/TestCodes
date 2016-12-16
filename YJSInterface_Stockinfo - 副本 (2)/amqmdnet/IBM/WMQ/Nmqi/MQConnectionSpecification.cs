namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Collections;
    using System.Threading;

    internal class MQConnectionSpecification : NmqiObject
    {
        private int ccsid;
        private int connectingConnections;
        private MQFAPConnectionPool connectionPool;
        private Lock connectionsLock;
        private ArrayList eligibleConnections;
        private const int MAX_ATTEMPTS = 3;
        private MQChannelDefinition mqcd;
        private MQSSLConfigOptions mqsco;
        private int nmqiFlags;
        public const string sccsid = "%Z% %W% %I% %E% %U%";
        private string uidFlowPassword;
        private string uidFlowUserId;

        internal MQConnectionSpecification(NmqiEnvironment nmqiEnv, MQFAPConnectionPool mqConnectionPool, MQChannelDefinition mqcd, MQSSLConfigOptions sslConfigOptions, int nmqiFlags, string uidFlowUserId, string uidFlowPassword, int ccsid) : base(nmqiEnv)
        {
            this.connectionsLock = new Lock();
            this.eligibleConnections = new ArrayList();
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { nmqiEnv, mqConnectionPool, mqcd, nmqiFlags, uidFlowUserId, (uidFlowPassword == null) ? uidFlowPassword : "********", ccsid });
            this.connectionPool = mqConnectionPool;
            this.mqcd = mqcd;
            this.mqsco = sslConfigOptions;
            this.nmqiFlags = nmqiFlags;
            this.uidFlowUserId = uidFlowUserId;
            this.uidFlowPassword = uidFlowPassword;
            this.ccsid = ccsid;
        }

        internal void AddConnection(MQFAPConnection connection)
        {
            uint method = 0x59e;
            this.TrEntry(method, new object[] { connection });
            try
            {
                this.connectionsLock.AssertOnCurrentThreadHoldsLock();
                if (!this.eligibleConnections.Contains(connection))
                {
                    this.eligibleConnections.Add(connection);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private MQFAPConnection CreateAndConnectConnection(MQChannelDefinition mqcd, MQSSLConfigOptions sslConfigOptions, MQConnectionSecurityParameters mqcsp, MQFAP fap, string qMgrName, int connectOptions, int maxFapLevel)
        {
            MQFAPConnection connection;
            uint method = 0x59a;
            this.TrEntry(method, new object[] { mqcsp, fap, qMgrName, connectOptions, maxFapLevel });
            try
            {
                if (mqcd == null)
                {
                    CommonServices.SetValidInserts();
                    CommonServices.CommentInsert1 = "No Channel Definition to establish connection";
                    base.FFST("%Z% %W% %I% %E% %U%", "%C%", method, 1, 0x20009519, 0);
                    NmqiException exception = new NmqiException(base.env, -1, null, 2, 0x893, null);
                    throw exception;
                }
                if (mqcd.TransportType == 2)
                {
                    connection = new MQTCPConnection(base.env, this, fap, mqcd, sslConfigOptions, this.GetConnectOptions(connectOptions));
                }
                else
                {
                    string[] inserts = new string[3];
                    inserts[2] = mqcd.TransportType.ToString();
                    NmqiException exception2 = new NmqiException(base.env, 0x26bb, inserts, 2, 0x80b, null);
                    throw exception2;
                }
                this.connectionsLock.Acquire();
                this.connectingConnections++;
                try
                {
                    MQChannelDefinition definition = connection.ClientConn.Clone();
                    definition.Version = 11;
                    connection.NegotiatedChannel = definition;
                }
                catch (Exception exception3)
                {
                    base.TrException(method, exception3, 1);
                    NmqiException exception4 = new NmqiException(base.env, -1, null, 2, 0x8e5, null);
                    throw exception4;
                }
                connection.InitSecurityExits();
                try
                {
                    connection.Connect(mqcsp);
                }
                catch (Exception exception5)
                {
                    base.TrException(method, exception5, 2);
                    connection = null;
                    throw exception5;
                }
            }
            finally
            {
                this.connectingConnections--;
                this.connectionsLock.Release();
                this.connectionsLock.PulseAll();
                base.TrExit(method);
            }
            return connection;
        }

        public override bool Equals(object other)
        {
            uint method = 0x596;
            this.TrEntry(method, new object[] { other });
            if (!(other is MQConnectionSpecification))
            {
                base.TrExit(method, false, 1);
                return false;
            }
            MQConnectionSpecification specification = (MQConnectionSpecification) other;
            bool result = specification.mqcd.Equals(this.mqcd) && ((specification.mqsco != null) ? specification.mqsco.Equals(this.mqsco) : (((specification.nmqiFlags.Equals(this.nmqiFlags) && object.Equals(specification.uidFlowUserId, this.uidFlowUserId)) && object.Equals(specification.uidFlowPassword, this.uidFlowPassword)) && (specification.ccsid == this.ccsid)));
            base.TrText(method, "Return value from  Equals - " + result.ToString());
            base.TrExit(method, result, 2);
            return result;
        }

        private int GetConnectOptions(int connectOptions)
        {
            uint method = 0x59c;
            this.TrEntry(method);
            if (base.env.Cfg.GetBoolValue(MQClientCfg.TCP_KEEPALIVE))
            {
                connectOptions |= 0x20;
            }
            else
            {
                connectOptions |= 0x40;
            }
            if (base.env.Cfg.GetBoolValue(MQClientCfg.TCP_LINGER))
            {
                connectOptions |= 0x10;
            }
            base.TrExit(method, connectOptions);
            return connectOptions;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal MQSession GetSession(MQChannelDefinition mqcd, MQSSLConfigOptions sslConfigOptions, MQConnectionSecurityParameters mqcsp, MQFAP fap, string qMgrName, int connectOptions, int maxFAPLevel)
        {
            uint method = 0x598;
            this.TrEntry(method, new object[] { mqcsp, fap, qMgrName, connectOptions, maxFAPLevel });
            MQSession result = null;
            bool flag = CommonServices.TraceEnabled();
            try
            {
                int num2 = 0;
                for (num2 = 0; (num2 < 3) && (result == null); num2++)
                {
                    result = this.GetSessionFromEligibleConnection();
                    if ((result != null) && flag)
                    {
                        base.TrText(method, string.Concat(new object[] { "Current session(existing connection) = ", result.ConversationId, "Connection - ", result.Connection.GetHashCode() }));
                    }
                }
                if (result == null)
                {
                    result = this.GetSessionFromNewConnection(mqcd, sslConfigOptions, mqcsp, fap, qMgrName, connectOptions, maxFAPLevel);
                    if ((result != null) && flag)
                    {
                        base.TrText(method, string.Concat(new object[] { "Current session(existing connection) = ", result.ConversationId, "Connection - ", result.Connection.GetHashCode() }));
                    }
                }
                result.InitOAMFlow(mqcsp);
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        private MQSession GetSessionFromEligibleConnection()
        {
            uint method = 0x59b;
            this.TrEntry(method);
            MQSession session = null;
            bool flag = true;
            try
            {
                this.connectionsLock.Acquire();
                if (this.eligibleConnections.Count != 0)
                {
                    MQFAPConnection connection = (MQFAPConnection) this.eligibleConnections[0];
                    if (connection.GetFreeConversations() == 1)
                    {
                        this.RemoveConnection(connection);
                    }
                    if (connection.IsReconnectRequested)
                    {
                        this.RemoveConnection(connection);
                        base.TrExit(method, null, 1);
                        return null;
                    }
                    if (!connection.IsConnected)
                    {
                        this.RemoveConnection(connection);
                        base.TrExit(method, null, 2);
                        return null;
                    }
                    session = connection.AssignSession();
                    session.InitSendReceiveExits(false);
                    session.StartConversation();
                    flag = false;
                }
                else if (this.connectingConnections > 0)
                {
                    try
                    {
                        flag = false;
                        this.connectionsLock.Wait();
                    }
                    catch (ThreadInterruptedException exception)
                    {
                        base.TrException(method, exception);
                    }
                }
                if (flag)
                {
                    Thread.Sleep(0);
                }
            }
            finally
            {
                this.connectionsLock.Release();
                base.TrExit(method, 3);
            }
            return session;
        }

        private MQSession GetSessionFromNewConnection(MQChannelDefinition mqcd, MQSSLConfigOptions sslConfigOptions, MQConnectionSecurityParameters mqcsp, MQFAP fap, string qMgrName, int connectOptions, int maxFapLevel)
        {
            uint method = 0x599;
            this.TrEntry(method, new object[] { mqcsp, fap, qMgrName, connectOptions, maxFapLevel });
            MQFAPConnection connection = null;
            MQSession session = null;
            try
            {
                this.connectionsLock.Acquire();
                connection = this.CreateAndConnectConnection(mqcd, sslConfigOptions, mqcsp, fap, qMgrName, connectOptions, maxFapLevel);
                session = connection.AssignSession();
                session.ParentConnection = connection;
                session.InitSendReceiveExits(true);
                if (connection.GetFreeConversations() > 0)
                {
                    this.AddConnection(connection);
                }
            }
            finally
            {
                this.connectionsLock.Release();
                base.TrExit(method);
            }
            return session;
        }

        internal void RegisterConnection(MQFAPConnection connection)
        {
            uint method = 0x597;
            this.TrEntry(method, new object[] { connection });
            if (!this.eligibleConnections.Contains(connection))
            {
                base.TrText(method, "ConnectionId of current connection being added - " + connection.GetHashCode());
                this.eligibleConnections.Add(connection);
            }
            base.TrExit(method);
        }

        internal void RemoveConnection(MQFAPConnection connection)
        {
            uint method = 0x59d;
            this.TrEntry(method, new object[] { connection });
            try
            {
                this.connectionsLock.AssertOnCurrentThreadHoldsLock();
                this.eligibleConnections.Remove(connection);
                if (this.eligibleConnections.Count == 0)
                {
                    this.connectionPool.RemoveSpec(this);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal int CCSID
        {
            get
            {
                return this.ccsid;
            }
        }

        internal Lock ConnectionsLock
        {
            get
            {
                return this.connectionsLock;
            }
        }

        internal MQChannelDefinition MQChannelDef
        {
            get
            {
                return this.mqcd;
            }
        }

        internal MQSSLConfigOptions MQSCO
        {
            get
            {
                return this.mqsco;
            }
        }

        internal int NmqiFlags
        {
            get
            {
                return this.nmqiFlags;
            }
        }

        internal string UidFlowPassword
        {
            get
            {
                return this.uidFlowPassword;
            }
        }

        internal string UidFlowUserId
        {
            get
            {
                return this.uidFlowUserId;
            }
        }
    }
}

