namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.IO;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;

    internal class MQEncryptedSocket : NmqiObject, IMQNetworkModule, IDisposable
    {
        private string cipherSpec;
        private TcpClient client;
        private string clientCertName;
        private long currentbytescount;
        private NmqiEnvironment env;
        private bool isClosed;
        private string keyStore;
        private long maxkeyresetcount;
        private MQTCPConnection owningConnection;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private const string SERVERNAME = "*";
        private bool sslCertRevocationCheck;
        private string sslPeerName;
        private SslProtocols sslProtocol;
        private SslStream stream;
        private const string TLS1_1 = "Tls11";
        private const string TLS1_2 = "Tls12";

        public MQEncryptedSocket(NmqiEnvironment env, MQTCPConnection conn, Socket socket, MQChannelDefinition mqcd, MQSSLConfigOptions sslConfigOptions) : base(env)
        {
            this.keyStore = "*SYSTEM";
            this.sslProtocol = SslProtocols.Tls;
            try
            {
                base.TrConstructor("%Z% %W%  %I% %E% %U%");
                this.env = env;
                this.owningConnection = conn;
                this.clientCertName = "ibmwebspheremq" + Environment.UserName.ToLower();
                this.RetrieveAndValidateSSLParams(mqcd, sslConfigOptions);
                this.client = new TcpClient();
                this.client.Client = socket;
                this.MakeSecuredConnection();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool CheckSSLPeerNameMatching(X509Certificate peerCertificate)
        {
            MQPeerNameMatching matching = new MQPeerNameMatching(this.env, this.owningConnection, this.sslPeerName, true);
            string subject = peerCertificate.Subject;
            MQPeerNameMatching name = new MQPeerNameMatching(this.env, this.owningConnection, subject, false);
            if (!matching.IsMatchingPeerName(name))
            {
                new MQException(2, 0x95e);
                return false;
            }
            return true;
        }

        public bool ClientValidatingServerCertificate(object sender, X509Certificate peerCertificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool flag;
            uint method = 0x635;
            this.TrEntry(method);
            try
            {
                if ((this.sslPeerName != null) && (this.sslPeerName.Length > 0))
                {
                    if (peerCertificate == null)
                    {
                        MQException exception = new MQException(2, 0x95e);
                        throw exception;
                    }
                    return this.CheckSSLPeerNameMatching(peerCertificate);
                }
                if (sslPolicyErrors != SslPolicyErrors.None)
                {
                    base.TrText(method, "SSL Server Certificate validation failed - " + sslPolicyErrors.ToString());
                }
                base.TrText(method, "Ignoring Server certificate validation. Accepting any certificate as Client did not specify a SSLPEERNAME");
                flag = true;
            }
            finally
            {
                base.TrExit(method);
            }
            return flag;
        }

        public void Close()
        {
            uint method = 0x63a;
            this.TrEntry(method);
            try
            {
                if (!this.isClosed)
                {
                    this.Dispose(true);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Dispose()
        {
            uint method = 0x63b;
            this.TrEntry(method);
            try
            {
                if (!this.isClosed)
                {
                    this.Dispose(false);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Dispose(bool suppressFinalize)
        {
            uint method = 0x63b;
            this.TrEntry(method);
            try
            {
                if (this.stream != null)
                {
                    this.stream.Close();
                }
                if ((this.client != null) && this.client.Connected)
                {
                    this.client.Close();
                }
                this.client = null;
                base.TrText(method, "Stream closed");
                if (suppressFinalize)
                {
                    GC.SuppressFinalize(this);
                }
            }
            catch (Exception exception)
            {
                base.TrException(method, exception);
                throw exception;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public X509Certificate FixClientCertificate(object sender, string targetHost, X509CertificateCollection clientCertificates, X509Certificate remoteServerCertificate, string[] Issuers)
        {
            X509Certificate certificate2;
            uint method = 0x636;
            this.TrEntry(method);
            try
            {
                base.TrText(method, "Client callback has been invoked to find client certificate");
                if (((Issuers != null) && (Issuers.Length > 0)) && ((clientCertificates != null) && (clientCertificates.Count > 0)))
                {
                    base.TrText(method, "Use the first certificate that is from an acceptable issuer.");
                    foreach (X509Certificate certificate in clientCertificates)
                    {
                        string issuer = certificate.Issuer;
                        if (Array.IndexOf<string>(Issuers, issuer) != -1)
                        {
                            return certificate;
                        }
                    }
                }
                if ((clientCertificates != null) && (clientCertificates.Count > 0))
                {
                    return clientCertificates[0];
                }
                certificate2 = null;
            }
            finally
            {
                base.TrExit(method);
            }
            return certificate2;
        }

        private void MakeSecuredConnection()
        {
            uint method = 0x63c;
            this.TrEntry(method);
            try
            {
                StoreLocation localMachine;
                this.stream = new SslStream(this.client.GetStream(), false, new RemoteCertificateValidationCallback(this.ClientValidatingServerCertificate), new LocalCertificateSelectionCallback(this.FixClientCertificate));
                base.TrText(method, "Created an instance of SSLStreams");
                if (this.keyStore == "*SYSTEM")
                {
                    localMachine = StoreLocation.LocalMachine;
                    base.TrText(method, "Setting current certificate store as 'Computer'");
                }
                else
                {
                    localMachine = StoreLocation.CurrentUser;
                    base.TrText(method, "Setting current certificate store as 'User'");
                }
                X509Store store = new X509Store(StoreName.My, localMachine);
                base.TrText(method, "Created store object to access certificates");
                store.Open(OpenFlags.OpenExistingOnly);
                base.TrText(method, "Opened store");
                base.TrText(method, "Accessing certificate - " + this.clientCertName);
                X509Certificate2Collection clientCertificates = new X509Certificate2Collection();
                X509Certificate2Enumerator enumerator = store.Certificates.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    X509Certificate2 current = enumerator.Current;
                    if (current.FriendlyName.ToLower() == this.clientCertName)
                    {
                        clientCertificates.Add(current);
                    }
                }
                Array values = Enum.GetValues(typeof(SslProtocols));
                bool flag = Enum.IsDefined(typeof(SslProtocols), "Tls12");
                base.TrText(method, "TLS12 supported - " + flag);
                if (((this.cipherSpec == null) || (this.cipherSpec == "*NEGOTIATE")) || (this.cipherSpec == string.Empty))
                {
                    if (flag)
                    {
                        foreach (SslProtocols protocols in values)
                        {
                            if (protocols.ToString() == "Tls12")
                            {
                                base.TrText(method, "Setting SslProtcol as TSL12");
                                this.sslProtocol = protocols;
                                break;
                            }
                        }
                    }
                    else
                    {
                        base.TrText(method, "Setting SslProtocol as TLS");
                        this.sslProtocol = SslProtocols.Tls;
                    }
                }
                else
                {
                    values = Enum.GetValues(typeof(SslProtocols));
                    string sSLPrtocolVersionForCipher = MQCipherMappingTable.GetSSLPrtocolVersionForCipher(this.cipherSpec);
                    if (sSLPrtocolVersionForCipher == null)
                    {
                        goto Label_02E8;
                    }
                    if (!(sSLPrtocolVersionForCipher == "SSL 3.0"))
                    {
                        if (sSLPrtocolVersionForCipher == "TLS 1.0")
                        {
                            goto Label_0263;
                        }
                        if (sSLPrtocolVersionForCipher == "TLS 1.2")
                        {
                            goto Label_027F;
                        }
                        goto Label_02E8;
                    }
                    this.sslProtocol = SslProtocols.Ssl3;
                    base.TrText(method, "Setting SslProtol as Ssl3");
                }
                goto Label_02FF;
            Label_0263:
                this.sslProtocol = SslProtocols.Tls;
                base.TrText(method, "Setting SslProtol as Tls");
                goto Label_02FF;
            Label_027F:
                foreach (SslProtocols protocols2 in values)
                {
                    if (protocols2.ToString() == "Tls12")
                    {
                        this.sslProtocol = protocols2;
                        break;
                    }
                }
                base.TrText(method, "Setting SslProtol as Tls12");
                goto Label_02FF;
            Label_02E8:
                this.sslProtocol = SslProtocols.Default;
                base.TrText(method, "Setting SslProtocol as Default");
            Label_02FF:
                base.TrText(method, "Starting SSL Authentication");
                this.stream.AuthenticateAsClient("*", clientCertificates, this.sslProtocol, this.sslCertRevocationCheck);
                if (!this.stream.IsAuthenticated || !this.stream.IsEncrypted)
                {
                    MQException exception = new MQException(2, 0x80b);
                    this.client.Close();
                    throw exception;
                }
                store.Close();
                store = null;
                clientCertificates.Clear();
                clientCertificates = null;
                values = null;
                base.TrText(method, "SSL Authentication completed");
            }
            catch (Exception exception2)
            {
                base.TrException(method, exception2);
                throw exception2;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public int Read(byte[] buffer, int offset, int length)
        {
            uint method = 0x638;
            this.TrEntry(method);
            int num2 = 0;
            try
            {
                if ((this.client == null) || !this.client.Connected)
                {
                    goto Label_00EF;
                }
            Label_002A:
                base.TrText(method, "Reading data from Socket");
                if (this.client.Client.Poll(this.owningConnection.timeout, SelectMode.SelectRead))
                {
                    IAsyncResult asyncResult = this.stream.BeginRead(buffer, offset, length, null, null);
                    asyncResult.AsyncWaitHandle.WaitOne();
                    num2 = this.stream.EndRead(asyncResult);
                    base.TrText(method, "MQEncryptedSocket.Read completed with " + num2 + "bytes");
                    if (num2 > 0)
                    {
                        Interlocked.Add(ref this.currentbytescount, (long) num2);
                    }
                }
                else
                {
                    if (this.client.Connected)
                    {
                        base.TrText(method, "Socket polling returned false,Socket is connected but Data available to read:" + this.client.Available);
                        goto Label_002A;
                    }
                    num2 = -1;
                    base.TrText(method, "Socket status: NOT CONNECTED, returning no data can be read");
                }
            Label_00EF:
                base.TrText(method, "Current total bytes read/write on socket: " + this.currentbytescount);
            }
            catch (IOException exception)
            {
                base.TrException(method, exception);
                throw;
            }
            catch (Exception exception2)
            {
                base.TrException(method, exception2);
                throw;
            }
            finally
            {
                base.TrExit(method);
            }
            return num2;
        }

        private void RetrieveAndValidateSSLParams(MQChannelDefinition mqcd, MQSSLConfigOptions sslConfigOptions)
        {
            uint method = 0x637;
            this.TrEntry(method);
            try
            {
                if (sslConfigOptions != null)
                {
                    this.keyStore = sslConfigOptions.KeyRepository;
                    if ((this.keyStore == null) || ((this.keyStore != "*SYSTEM") && (this.keyStore != "*USER")))
                    {
                        base.throwNewMQException(2, 0x94d);
                    }
                    base.TrText(method, "KeyStore is " + this.keyStore);
                    string str = base.GetString(sslConfigOptions.CerfificateLabel);
                    if ((str != null) & (str != ""))
                    {
                        this.clientCertName = str;
                    }
                    this.maxkeyresetcount = sslConfigOptions.KeyResetCount;
                    base.TrText(method, "KeyResetCount is " + this.maxkeyresetcount);
                    this.sslCertRevocationCheck = sslConfigOptions.CertRevocationCheck;
                    base.TrText(method, "CertificationCheck = " + Convert.ToString(this.sslCertRevocationCheck));
                    if (mqcd != null)
                    {
                        this.cipherSpec = mqcd.SSLCipherSpec;
                        base.TrText(method, "CipherSpec value is " + this.cipherSpec);
                        this.sslPeerName = mqcd.SSLPeerName;
                        base.TrText(method, "SSLPEERNAME value is " + this.sslPeerName);
                        if (this.sslPeerName == null)
                        {
                            base.throwNewMQException(2, 0x95f);
                        }
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public int Write(byte[] buffer, int offset, int length)
        {
            uint method = 0x639;
            this.TrEntry(method);
            try
            {
                base.TrText(method, "Writing " + length + " bytes onto wire");
                if ((this.stream == null) || ((this.client != null) && !this.client.Connected))
                {
                    base.throwNewMQException(2, 0x7d9);
                }
                if ((this.maxkeyresetcount != 0L) && (this.maxkeyresetcount < (this.currentbytescount + length)))
                {
                    this.currentbytescount = 0L;
                    if (this.stream != null)
                    {
                        this.stream.Close();
                    }
                    if (this.client.Connected)
                    {
                        this.client.Close();
                    }
                    this.isClosed = true;
                    Exception nestedException = new Exception("Max keyreset count reached..breaking the connection");
                    NmqiException exception2 = new NmqiException(this.env, -1, new string[] { "KeyResetCount=" + this.maxkeyresetcount, "CurrentCount=" + this.currentbytescount }, 2, 0x7d9, nestedException);
                    throw exception2;
                }
                if ((this.client != null) && this.client.Connected)
                {
                    this.stream.Write(buffer, offset, length);
                    base.TrText(method, "Current total bytes read/write on socket: " + this.currentbytescount);
                    Interlocked.Add(ref this.currentbytescount, (long) length);
                }
                base.TrText(method, "Write onto wire complete");
            }
            catch (Exception exception3)
            {
                base.TrException(method, exception3);
                throw exception3;
            }
            finally
            {
                base.TrExit(method);
            }
            return length;
        }
    }
}

