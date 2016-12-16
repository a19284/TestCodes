namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Collections;
    using System.Net;
    using System.Net.Sockets;

    internal class MQTCPConnection : MQFAPConnection
    {
        private static MQClientCfg cfg = null;
        private string conname;
        private bool connected;
        private int heartbeatInterval;
        private string localAddr;
        private static Hashtable localAddrPorts = Hashtable.Synchronized(new Hashtable());
        private readonly object Lock;
        internal int maxTransmissionSize;
        protected MQChannelDefinition mqcd;
        private int mqFapLevel;
        private int mqgetWaitInterval;
        private bool multiplexingEnabled;
        private IMQNetworkModule network;
        private DateTime nextHeartBeatSendTime;
        internal MQRcvThread rcvThread;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private Socket socket;
        private MQSSLConfigOptions sslConfigOptions;
        internal int timeout;
        private bool timeoutChanged;

        internal MQTCPConnection(NmqiEnvironment nmqiEnv, MQConnectionSpecification spec, MQFAP fap, MQChannelDefinition mqcd, MQSSLConfigOptions sslConfigOptions, int options) : base(nmqiEnv, spec.MQChannelDef.Clone())
        {
            this.timeout = 0x1d4c0;
            this.timeoutChanged = true;
            this.maxTransmissionSize = 0x7ff6;
            this.Lock = new object();
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.conname = mqcd.ConnectionName;
            this.nextHeartBeatSendTime = DateTime.Now.AddSeconds((double) this.timeout);
            this.localAddr = mqcd.LocalAddress;
            cfg = base.env.Cfg;
            this.mqcd = mqcd;
            this.sslConfigOptions = sslConfigOptions;
            base.fap = fap;
            base.remoteConnectionSpec = spec;
            base.connectOptions = options;
            base.nmqiFlags = spec.NmqiFlags;
            base.commsBufferPool = fap.CommsBufferPool;
        }

        public void CloseSocket()
        {
            uint method = 0x2cb;
            this.TrEntry(method);
            try
            {
                base.RequestSendLock();
                lock (this.Lock)
                {
                    if (this.network != null)
                    {
                        this.network.Close();
                        this.network = null;
                    }
                    this.connected = false;
                    if ((this.socket != null) && this.socket.Connected)
                    {
                        this.socket.Shutdown(SocketShutdown.Both);
                        this.socket.Close();
                    }
                    this.socket = null;
                    base.TrText("Socket closed ");
                }
            }
            finally
            {
                base.ReleaseSendLock();
                base.TrExit(method);
            }
        }

        private Socket ConnectSocket(string localAddr, string connectionName, int options)
        {
            uint method = 0x2c8;
            this.TrEntry(method, new object[] { localAddr, connectionName, options });
            Socket socket = null;
            IPAddress[] hostAddresses = null;
            try
            {
                string str;
                int num2;
                int length = connectionName.Length;
                int index = connectionName.IndexOf('(', 0);
                if (index >= 0)
                {
                    string str2;
                    str = connectionName.Substring(0, index);
                    int num4 = connectionName.IndexOf(')', index);
                    if (num4 < 0)
                    {
                        str2 = connectionName.Substring(index + 1);
                    }
                    else
                    {
                        str2 = connectionName.Substring(index + 1, (num4 - index) - 1);
                    }
                    num2 = Convert.ToInt32(str2);
                    if ((num2 < 0) || (num2 > 0xffff))
                    {
                        throw new MQManagedClientException(0x20009203, (uint) num2, (uint) num2, "'" + connectionName + "'", "TCP/IP", " (Convert.ToInt32)", 2, 0x893);
                    }
                }
                else
                {
                    str = connectionName.Trim();
                    num2 = 0x586;
                }
                try
                {
                    ParsedLocalAddr addr = this.ParseLocalAddress(localAddr);
                    hostAddresses = Dns.GetHostAddresses(str);
                    Exception exception = null;
                    bool flag = "MQIPADDR_IPV6".Equals(Convert.ToString(base.env.Cfg.GetStringValue(MQClientCfg.ENV_MQIPADDRV)).ToUpper());
                    IPAddress address = null;
                    foreach (IPAddress address2 in hostAddresses)
                    {
                        if ((flag && address2.AddressFamily.Equals(AddressFamily.InterNetworkV6)) || (!flag && address2.AddressFamily.Equals(AddressFamily.InterNetwork)))
                        {
                            address = address2;
                            try
                            {
                                socket = this.ConnectUsingLocalAddr(addr, address2, num2);
                            }
                            catch (Exception exception2)
                            {
                                base.TrException(method, exception2, 1);
                                exception = exception2;
                            }
                            break;
                        }
                    }
                    foreach (IPAddress address3 in hostAddresses)
                    {
                        if (address3 != address)
                        {
                            try
                            {
                                socket = this.ConnectUsingLocalAddr(addr, address3, num2);
                                break;
                            }
                            catch (Exception exception3)
                            {
                                base.TrException(method, exception3, 2);
                                exception = exception3;
                            }
                        }
                    }
                    if (socket == null)
                    {
                        throw exception;
                    }
                    if ((base.clientConn.SSLCipherSpec != null) && (base.clientConn.SSLCipherSpec.Length > 0))
                    {
                        this.network = new MQEncryptedSocket(base.env, this, socket, this.mqcd, this.sslConfigOptions);
                    }
                    else
                    {
                        this.network = new MQPlainSocket(base.env, this, socket);
                    }
                    this.connected = true;
                }
                catch (Exception exception4)
                {
                    uint errorCode;
                    base.TrException(method, exception4, 3);
                    if (exception4 is SocketException)
                    {
                        errorCode = (uint) ((SocketException) exception4).ErrorCode;
                    }
                    else
                    {
                        errorCode = 0;
                    }
                    throw new MQManagedClientException(0x20009202, errorCode, errorCode, connectionName, "TCP/IP", " (Exception)", 2, 0x9ea);
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return socket;
        }

        private Socket ConnectUsingLocalAddr(ParsedLocalAddr localAddr, IPAddress ipad, int port)
        {
            uint method = 0x2ca;
            this.TrEntry(method, new object[] { localAddr, ipad, port });
            Socket socket = null;
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(ipad, port);
                base.TrText("Remote Address:" + remoteEP.ToString());
                IPAddress any = null;
                if (localAddr.hostname != null)
                {
                    foreach (IPAddress address2 in Dns.GetHostEntry(localAddr.hostname).AddressList)
                    {
                        if (address2.AddressFamily == ipad.AddressFamily)
                        {
                            any = address2;
                        }
                    }
                    if (any == null)
                    {
                        NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x80b, null);
                        base.TrException(method, ex);
                        throw ex;
                    }
                }
                else if (remoteEP.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    any = IPAddress.IPv6Any;
                }
                else
                {
                    any = IPAddress.Any;
                }
                bool flag = false;
                try
                {
                    int startPort = localAddr.startPort;
                    if (startPort != 0)
                    {
                        startPort = GetFirstPortToTry(localAddr);
                    }
                    bool connected = false;
                    int num3 = localAddr.endPort - localAddr.startPort;
                    for (int i = 0; (i <= num3) && !connected; i++)
                    {
                        try
                        {
                            socket = new Socket(remoteEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                            IPEndPoint localEP = new IPEndPoint(any, startPort);
                            base.TrText("Local Address:" + localEP.ToString());
                            base.TrText(method, "Bind");
                            socket.Bind(localEP);
                            base.TrText(method, "Bind returned " + socket.LocalEndPoint.ToString());
                            base.TrText(method, "Connect");
                            socket.Connect(remoteEP);
                            base.TrText(method, "Connect returned " + socket.Connected);
                            connected = socket.Connected;
                        }
                        catch (SocketException exception2)
                        {
                            base.TrException(method, exception2);
                            if (socket != null)
                            {
                                socket.Close();
                            }
                            socket = null;
                            if (exception2.ErrorCode != 0x2740)
                            {
                                break;
                            }
                        }
                        startPort++;
                        if (startPort > localAddr.endPort)
                        {
                            startPort = localAddr.startPort;
                        }
                        SetFirstPortToTry(localAddr, startPort);
                    }
                    if ((socket != null) && socket.Connected)
                    {
                        if ((base.connectOptions & 0x10) != 0)
                        {
                            LingerOption option = new LingerOption(true, 10);
                            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, option);
                            base.TrText(method, "TCP/IP LINGER active");
                        }
                        if (((base.connectOptions & 0x20) != 0) && ((base.connectOptions & 0x40) == 0))
                        {
                            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);
                            base.TrText(method, "TCP/IP KEEPALIVE active");
                        }
                        int optionValue = 0x8000;
                        int num6 = 0x8000;
                        string environmentVariable = Environment.GetEnvironmentVariable("MQ_COMMS_IP_SDRBUF");
                        if (environmentVariable != null)
                        {
                            base.TrText(method, "MQ_COMMS_IP_SDRBUF set on evironment");
                            optionValue = Convert.ToInt32(environmentVariable.Trim());
                            if (optionValue <= 0)
                            {
                                base.TrText(method, "MQ_COMMS_IP_SDRBUF set with invalid value " + optionValue);
                            }
                        }
                        string str2 = Environment.GetEnvironmentVariable("MQ_COMMS_IP_RCVBUF");
                        if (str2 != null)
                        {
                            base.TrText(method, "MQ_COMMS_IP_RCVBUF set on evironment");
                            num6 = Convert.ToInt32(str2.Trim());
                            if (num6 <= 0)
                            {
                                base.TrText(method, "MQ_COMMS_IP_RCVBUF set with invalid value " + num6);
                            }
                        }
                        if ((optionValue <= 0) || (num6 <= 0))
                        {
                            base.TrText(method, "Not valid values on Send/Receive buffer. making connection to fail");
                            flag = false;
                        }
                        else
                        {
                            base.TrText(method, "Using socket send buffer size " + optionValue);
                            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, optionValue);
                            base.TrText(method, "Using socket receive buffer size " + num6);
                            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, num6);
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        NmqiException exception3 = new NmqiException(base.env, -1, null, 2, 0x80b, null);
                        base.TrException(method, exception3);
                        throw exception3;
                    }
                }
                finally
                {
                    if ((socket != null) && !flag)
                    {
                        socket.Close();
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return socket;
        }

        private void EnableNonBlockingSocket()
        {
            uint method = 0x2ce;
            this.TrEntry(method);
            this.socket.Blocking = false;
            base.TrExit(method);
        }

        private static int GetFirstPortToTry(ParsedLocalAddr la)
        {
            if (la.key == null)
            {
                la.key = la.startPort.ToString() + ":" + la.endPort.ToString();
                if (la.hostname != null)
                {
                    la.key = la.key + ":" + la.hostname;
                }
            }
            int startPort = la.startPort;
            object obj2 = localAddrPorts[la.key];
            if (obj2 == null)
            {
                return startPort;
            }
            startPort = (int) obj2;
            if ((startPort >= la.startPort) && (startPort <= la.endPort))
            {
                return startPort;
            }
            return la.startPort;
        }

        internal bool IsSocketConnected()
        {
            uint method = 0x497;
            this.TrEntry(method);
            bool result = false;
            try
            {
                result = this.socket.Connected;
            }
            catch (Exception)
            {
                result = false;
            }
            base.TrExit(method, result);
            return result;
        }

        private ParsedLocalAddr ParseLocalAddress(string localAddr)
        {
            uint method = 0x2c9;
            this.TrEntry(method, new object[] { localAddr });
            ParsedLocalAddr result = null;
            try
            {
                int intValue = base.env.Cfg.GetIntValue(MQClientCfg.TCP_STRPORT);
                int num3 = base.env.Cfg.GetIntValue(MQClientCfg.TCP_ENDPORT);
                string str = null;
                if ((localAddr == null) || (localAddr.Length == 0))
                {
                    localAddr = base.env.Cfg.GetStringValue(MQClientCfg.ENV_MQ_LCLADDR);
                }
                if ((localAddr != null) && (localAddr.Length > 0))
                {
                    localAddr = localAddr.Trim();
                    int length = localAddr.LastIndexOf('(');
                    int num5 = localAddr.LastIndexOf(')');
                    if ((length == -1) && (num5 == -1))
                    {
                        str = localAddr;
                    }
                    else
                    {
                        if (((length == -1) || (num5 == -1)) || (num5 != (localAddr.Length - 1)))
                        {
                            NmqiException ex = new NmqiException(base.env, -1, null, 2, 0x80b, null);
                            base.TrException(method, ex);
                            throw ex;
                        }
                        str = localAddr.Substring(0, length);
                        if ((str.IndexOf('(') != -1) || (str.IndexOf(')') != -1))
                        {
                            NmqiException exception2 = new NmqiException(base.env, -1, null, 2, 0x80b, null);
                            base.TrException(method, exception2);
                            throw exception2;
                        }
                        int index = localAddr.IndexOf(',', length + 1);
                        int num7 = ((index >= 0) ? index : num5) - 1;
                        try
                        {
                            int num8 = num7 - length;
                            intValue = int.Parse(localAddr.Substring(length + 1, num8));
                            if (intValue < 0)
                            {
                                NmqiException exception3 = new NmqiException(base.env, -1, null, 2, 0x80b, null);
                                base.TrException(method, exception3);
                                throw exception3;
                            }
                        }
                        catch (Exception)
                        {
                            NmqiException exception4 = new NmqiException(base.env, -1, null, 2, 0x80b, null);
                            base.TrException(method, exception4);
                            throw exception4;
                        }
                        if (index >= 0)
                        {
                            try
                            {
                                int num9 = (num5 - index) - 1;
                                num3 = int.Parse(localAddr.Substring(index + 1, num9));
                                if (num3 < 0)
                                {
                                    NmqiException exception5 = new NmqiException(base.env, -1, null, 2, 0x80b, null);
                                    base.TrException(method, exception5);
                                    throw exception5;
                                }
                                goto Label_021C;
                            }
                            catch (Exception exception6)
                            {
                                NmqiException exception7 = new NmqiException(base.env, -1, null, 2, 0x80b, exception6);
                                base.TrException(method, exception7);
                                throw exception7;
                            }
                        }
                        num3 = intValue;
                    }
                }
            Label_021C:
                result = new ParsedLocalAddr();
                result.hostname = str;
                result.startPort = intValue;
                result.endPort = num3;
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        internal override void ProtocolConnect()
        {
            this.socket = this.ConnectSocket(this.localAddr, this.conname, base.connectOptions);
        }

        internal override void ProtocolDisconnect()
        {
            this.CloseSocket();
        }

        protected override void ProtocolSetHeartbeatInterval(int hbInt)
        {
            this.HeartbeatInterval = hbInt;
        }

        protected override void ProtocolSetupAsyncMode()
        {
            this.SetUpAsyncMode();
        }

        protected override bool ProtocolSupportsAsyncMode()
        {
            return true;
        }

        internal override int Receive(byte[] cBuffer, int offset, int length, SocketFlags flags)
        {
            uint method = 0x2cd;
            this.TrEntry(method, new object[] { cBuffer, offset, length });
            bool flag = false;
            int num2 = offset;
            int num3 = length;
            int num4 = 0;
            try
            {
                if (this.socket == null)
                {
                    return -1;
                }
                if ((this.socket == null) || !this.connected)
                {
                    return -1;
                }
                if (this.timeoutChanged && this.connected)
                {
                    this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, (int) (this.timeout + 0x3e8));
                    base.TrText(method, "ReceiveTimeout set to " + this.timeout);
                    this.timeoutChanged = false;
                }
                if (this.socket.Blocking)
                {
                    try
                    {
                        num4 = this.network.Read(cBuffer, num2, num3);
                        goto Label_030E;
                    }
                    catch (SocketException exception)
                    {
                        throw exception;
                    }
                }
            Label_00DC:
                if ((this.socket == null) || !this.connected)
                {
                    return num4;
                }
                try
                {
                    num4 = this.network.Read(cBuffer, num2, num3);
                }
                catch (NmqiException exception2)
                {
                    base.TrException(method, exception2);
                    throw exception2;
                }
                catch (SocketException exception3)
                {
                    base.TrException(method, exception3, 1);
                    if (exception3.ErrorCode == 0x2733)
                    {
                        base.TrException(method, exception3);
                        base.TrText(method, "Exception Received - Not bubling the exception as this is found to be common when socket is quite busy with loads of sends");
                        goto Label_00DC;
                    }
                    if (exception3.ErrorCode == 0x274a)
                    {
                        return -1;
                    }
                    if (exception3.ErrorCode != 0x2745)
                    {
                        throw exception3;
                    }
                    if ((flag && (num4 <= 0)) || !this.multiplexingEnabled)
                    {
                        num4 = -1;
                    }
                    else
                    {
                        if (DateTime.Compare(DateTime.UtcNow, this.nextHeartBeatSendTime) > 0)
                        {
                            if ((this.socket == null) || !this.connected)
                            {
                                goto Label_0238;
                            }
                            try
                            {
                                base.SendHeartbeat(1);
                            }
                            catch (NmqiException exception4)
                            {
                                base.TrException(method, exception4);
                                num4 = -1;
                                goto Label_030E;
                            }
                            catch (SocketException exception5)
                            {
                                base.TrException(method, exception5);
                                num4 = -1;
                                goto Label_030E;
                            }
                            flag = true;
                            this.timeout = Math.Min(0xea60, this.timeout);
                            this.timeoutChanged = true;
                            this.nextHeartBeatSendTime = DateTime.UtcNow;
                            this.nextHeartBeatSendTime = this.nextHeartBeatSendTime.AddSeconds((double) this.heartbeatInterval);
                            goto Label_00DC;
                        }
                        num4 = -1;
                    }
                    goto Label_030E;
                }
            Label_0238:
                if (num4 > 0)
                {
                    if (flag)
                    {
                        flag = false;
                        this.timeout = this.heartbeatInterval;
                        this.timeoutChanged = true;
                    }
                }
                else if ((flag && (num4 <= 0)) || !this.multiplexingEnabled)
                {
                    num4 = -1;
                }
                else
                {
                    if ((num4 > 0) || !this.connected)
                    {
                        goto Label_00DC;
                    }
                    if (DateTime.Compare(DateTime.UtcNow, this.nextHeartBeatSendTime) > 0)
                    {
                        try
                        {
                            base.SendHeartbeat(1);
                        }
                        catch (NmqiException exception6)
                        {
                            base.TrException(method, exception6);
                            num4 = -1;
                            goto Label_030E;
                        }
                        catch (SocketException exception7)
                        {
                            base.TrException(method, exception7);
                            num4 = -1;
                            goto Label_030E;
                        }
                        flag = true;
                        this.timeout = Math.Min(0xea60, this.timeout);
                        this.timeoutChanged = true;
                        this.nextHeartBeatSendTime = DateTime.UtcNow;
                        this.nextHeartBeatSendTime = this.nextHeartBeatSendTime.AddSeconds((double) this.heartbeatInterval);
                        goto Label_00DC;
                    }
                    num4 = -1;
                }
            Label_030E:
                length = num3;
                offset = num2;
                return num4;
            }
            catch (SocketException exception8)
            {
                base.TrException(method, exception8, 2);
                CommonServices.SetValidInserts();
                CommonServices.ArithInsert1 = (uint) exception8.ErrorCode;
                CommonServices.ArithInsert2 = (uint) exception8.ErrorCode;
                CommonServices.CommentInsert2 = "TCP/IP";
                CommonServices.CommentInsert3 = " (socket.Receive)";
                base.DisplayMessage(0x20009208, 0xf0000010);
                NmqiException exception9 = new NmqiException(base.env, -1, null, 2, 0x7d9, exception8);
                throw exception9;
            }
            catch (ObjectDisposedException exception10)
            {
                base.TrException(method, exception10, 3);
            }
            catch (NmqiException exception11)
            {
                base.TrException(method, exception11);
                throw exception11;
            }
            catch (Exception exception12)
            {
                base.TrException(method, exception12, 5);
                NmqiException exception13 = new NmqiException(base.env, -1, null, 2, 0x7d9, exception12);
                throw exception13;
            }
            finally
            {
                base.TrExit(method);
            }
            return num4;
        }

        internal override int Send(byte[] bytes, int offset, int length, int segmentType, int tshType)
        {
            uint method = 0x2cc;
            bool flag = CommonServices.TraceEnabled();
            if (flag)
            {
                this.TrEntry(method, new object[] { bytes, offset, length });
            }
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            try
            {
                if (this.socket == null)
                {
                    throw new MQManagedClientException("Socket null", 2, 0x893);
                }
                if (flag)
                {
                    base.TrText("----------------");
                    base.TrCommsData(method, 2, "Send Buffer", offset, length, bytes);
                    base.TrText(method, " Data Length --> " + length);
                }
                num3 = offset;
                goto Label_021B;
            Label_0095:
                while (!this.socket.Poll(0x186a0, SelectMode.SelectWrite))
                {
                }
                try
                {
                    num2 = 0;
                    base.TrText(method, "Send >>");
                    num2 = this.network.Write(bytes, num3, length);
                    if (flag)
                    {
                        base.TrText(method, "Send returned " + num2);
                    }
                    if (num2 <= 0)
                    {
                        return num4;
                    }
                    length -= num2;
                    num3 += num2;
                }
                catch (SocketException exception)
                {
                    base.TrException(method, exception, 1);
                    if (exception.ErrorCode == 0x274a)
                    {
                        NmqiException ex = new NmqiException(base.env, -1, new string[] { exception.Message, exception.StackTrace, exception.SocketErrorCode.ToString() }, 2, 0x7d9, exception);
                        base.TrException(method, ex);
                        throw ex;
                    }
                    if (exception.ErrorCode != 0x2733)
                    {
                        base.TrText(method, "e.ErrorCode = " + exception.ErrorCode);
                        NmqiException exception3 = new NmqiException(base.env, -1, new string[] { exception.Message, exception.StackTrace, exception.SocketErrorCode.ToString() }, 2, 0x7d9, exception);
                        base.TrException(method, exception3);
                        throw exception3;
                    }
                    base.TrException(method, exception);
                    base.TrText(method, "Exception Received - Not bubling the exception as this is found to be common when socket is quite busy with loads of sends");
                    goto Label_0095;
                }
                finally
                {
                    base.TrText(method, "Send << - n = " + num2);
                }
            Label_021B:
                if (length > 0)
                {
                    goto Label_0095;
                }
                return 1;
            }
            catch (SocketException exception4)
            {
                base.TrException(method, exception4, 2);
                CommonServices.SetValidInserts();
                CommonServices.ArithInsert1 = (uint) exception4.ErrorCode;
                CommonServices.ArithInsert2 = (uint) exception4.ErrorCode;
                CommonServices.CommentInsert1 = this.conname;
                CommonServices.CommentInsert2 = "TCP/IP";
                CommonServices.CommentInsert3 = " (socket.Send)";
                base.DisplayMessage(0x20009206, 0xf0000010);
                NmqiException exception5 = new NmqiException(base.env, -1, null, 2, 0x7d9, exception4);
                throw exception5;
            }
            catch (MQException exception6)
            {
                base.TrException(method, exception6);
                throw exception6;
            }
            catch (Exception exception7)
            {
                base.TrException(method, exception7);
                NmqiException exception8 = new NmqiException(base.env, -1, null, 2, 0x893, exception7);
                throw exception8;
            }
            finally
            {
                base.TrExit(method);
            }
            return num4;
        }

        private static void SetFirstPortToTry(ParsedLocalAddr la, int lastPort)
        {
            if (la.key == null)
            {
                la.key = la.startPort.ToString() + ":" + la.endPort.ToString();
                if (la.hostname != null)
                {
                    la.key = la.key + ":" + la.hostname;
                }
            }
            localAddrPorts[la.key] = lastPort;
        }

        internal void SetUpAsyncMode()
        {
            uint method = 0x2cf;
            this.TrEntry(method);
            this.multiplexingEnabled = true;
            this.EnableNonBlockingSocket();
            base.TrExit(method);
        }

        public static MQClientCfg Cfg
        {
            get
            {
                return cfg;
            }
        }

        public int HeartbeatInterval
        {
            get
            {
                return this.heartbeatInterval;
            }
            set
            {
                this.heartbeatInterval = value;
                if (value > 0)
                {
                    if (value < 60)
                    {
                        value *= 2;
                    }
                    else
                    {
                        value += 60;
                    }
                }
                else
                {
                    value = 0;
                }
                value *= 0x3e8;
                if (this.timeout != value)
                {
                    this.timeout = value;
                    this.timeoutChanged = true;
                }
                this.nextHeartBeatSendTime = DateTime.UtcNow;
                this.nextHeartBeatSendTime = this.nextHeartBeatSendTime.AddSeconds((double) value);
            }
        }

        public int MQGetWaitInterval
        {
            get
            {
                return this.mqgetWaitInterval;
            }
            set
            {
                if (value <= 0)
                {
                    value = this.timeout;
                }
                this.mqgetWaitInterval = value;
            }
        }

        private class ParsedLocalAddr
        {
            public int endPort;
            public string hostname;
            public string key;
            public int startPort;
        }
    }
}

