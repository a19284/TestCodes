namespace IBM.WMQ.Nmqi
{
    using System;
    using System.Text;

    public class NmqiConnectOptions
    {
        private string ccdtUrl;
        private int cmdLevel = -1;
        private string exitClassPath;
        private string externalProductIdentifier;
        private int fapLevel;
        private int flags;
        public const int OVERRIDE_CCDT_SHARECNV = 0x40;
        private string password;
        private int platform = -1;
        private string qmidAsString;
        private int queueManagerCCSID = -1;
        private object receiveExits;
        private string receiveExitsUserData;
        private byte[] reconnectionID;
        private int reconnectionTimeout = 0x708;
        private byte[] remoteQMID;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private object securityExit;
        private string securityExitUserData;
        private object sendExits;
        private string sendExitsUserData;
        private int sharingConversations;
        private bool sslCertRevocationCheck;
        public const int TRUST_SOCKET_FACTORY = 0x10;
        private string userIdentifier;

        public string CCDTUrl
        {
            get
            {
                return this.ccdtUrl;
            }
            set
            {
                this.ccdtUrl = value;
            }
        }

        public int CmdLevel
        {
            get
            {
                return this.cmdLevel;
            }
            set
            {
                this.cmdLevel = value;
            }
        }

        public string ExitClassPath
        {
            get
            {
                return this.exitClassPath;
            }
            set
            {
                this.exitClassPath = value;
            }
        }

        public string ExternalProductIdentifier
        {
            get
            {
                return this.externalProductIdentifier;
            }
            set
            {
                this.externalProductIdentifier = value;
            }
        }

        public int FapLevel
        {
            get
            {
                return this.fapLevel;
            }
            set
            {
                this.fapLevel = value;
            }
        }

        public int Flags
        {
            get
            {
                return this.flags;
            }
            set
            {
                this.flags = value;
            }
        }

        public string Password
        {
            get
            {
                return this.password;
            }
            set
            {
                this.password = value;
            }
        }

        public int Platform
        {
            get
            {
                return this.platform;
            }
            set
            {
                this.platform = value;
            }
        }

        public int QueueManagerCCSID
        {
            get
            {
                return this.queueManagerCCSID;
            }
            set
            {
                this.queueManagerCCSID = value;
            }
        }

        public object ReceiveExits
        {
            get
            {
                return this.receiveExits;
            }
            set
            {
                this.receiveExits = value;
            }
        }

        public string ReceiveExitsUserData
        {
            get
            {
                return this.receiveExitsUserData;
            }
            set
            {
                this.receiveExitsUserData = value;
            }
        }

        public byte[] ReconnectionID
        {
            get
            {
                return this.reconnectionID;
            }
            set
            {
                this.reconnectionID = value;
            }
        }

        public int ReconnectionTimeout
        {
            get
            {
                return this.reconnectionTimeout;
            }
            set
            {
                this.reconnectionTimeout = value;
            }
        }

        public byte[] RemoteQMID
        {
            get
            {
                return this.remoteQMID;
            }
            set
            {
                this.remoteQMID = value;
                this.qmidAsString = Encoding.ASCII.GetString(this.remoteQMID);
            }
        }

        public string RemoteQmidAsString
        {
            get
            {
                return this.qmidAsString;
            }
            set
            {
                this.qmidAsString = value;
            }
        }

        public object SecurityExits
        {
            get
            {
                return this.securityExit;
            }
            set
            {
                this.securityExit = value;
            }
        }

        public string SecurityExitsUserData
        {
            get
            {
                return this.securityExitUserData;
            }
            set
            {
                this.securityExitUserData = value;
            }
        }

        public object SendExits
        {
            get
            {
                return this.sendExits;
            }
            set
            {
                this.sendExits = value;
            }
        }

        public string SendExitsUserData
        {
            get
            {
                return this.sendExitsUserData;
            }
            set
            {
                this.sendExitsUserData = value;
            }
        }

        public int SharingConversations
        {
            get
            {
                return this.sharingConversations;
            }
            set
            {
                this.sharingConversations = value;
            }
        }

        public bool SSLCertRevocationCheck
        {
            get
            {
                return this.sslCertRevocationCheck;
            }
            set
            {
                this.sslCertRevocationCheck = value;
            }
        }

        public string UserIdentifier
        {
            get
            {
                return this.userIdentifier;
            }
            set
            {
                this.userIdentifier = value;
            }
        }
    }
}

