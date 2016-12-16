namespace IBM.WMQ
{
    using System;

    public class MQChannelExit
    {
        private int capabilityFlags = 1;
        private int curHdrCompression;
        private int curMsgCompression;
        private int exitID;
        private int exitNumber;
        private int exitReason;
        private int exitResponse;
        private byte[] exitUserArea;
        private int fapLevel;
        private int Hconn = -1;
        private int maxSegmentLength;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private MQConnectionSecurityParameters securityParms;
        private bool sharingConversations;
        private string userData;

        public int CapabilityFlags
        {
            get
            {
                return this.capabilityFlags;
            }
            set
            {
                this.capabilityFlags = value;
            }
        }

        public int CurHdrCompression
        {
            get
            {
                return this.curHdrCompression;
            }
            set
            {
                this.curHdrCompression = value;
            }
        }

        public int CurMsgCompression
        {
            get
            {
                return this.curMsgCompression;
            }
            set
            {
                this.curMsgCompression = value;
            }
        }

        public int ExitID
        {
            get
            {
                return this.exitID;
            }
            set
            {
                this.exitID = value;
            }
        }

        public int ExitNumber
        {
            get
            {
                return this.exitNumber;
            }
            set
            {
                this.exitNumber = value;
            }
        }

        public int ExitReason
        {
            get
            {
                return this.exitReason;
            }
            set
            {
                this.exitReason = value;
            }
        }

        public int ExitResponse
        {
            get
            {
                return this.exitResponse;
            }
            set
            {
                this.exitResponse = value;
            }
        }

        public byte[] ExitUserArea
        {
            get
            {
                return this.exitUserArea;
            }
            set
            {
                this.exitUserArea = value;
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

        public int HConn
        {
            get
            {
                return this.Hconn;
            }
            set
            {
                this.Hconn = value;
            }
        }

        public int MaxSegmentLength
        {
            get
            {
                return this.maxSegmentLength;
            }
            set
            {
                this.maxSegmentLength = value;
            }
        }

        public MQConnectionSecurityParameters SecurityParms
        {
            get
            {
                return this.securityParms;
            }
            set
            {
                this.securityParms = value;
            }
        }

        public bool SharingConversations
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

        public string UserData
        {
            get
            {
                return this.userData;
            }
            set
            {
                this.userData = value;
            }
        }
    }
}

