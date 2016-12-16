namespace IBM.WMQ.Nmqi
{
    using System;

    internal abstract class MQXAConstants
    {
        internal const int GUID_BYTE_ARRAY_LENGTH = 0x10;
        internal const int MAXBQUALSIZE = 0x40;
        internal const int MAXGTRIDSIZE = 0x40;
        internal const int MAXINFOSIZE = 0x100;
        internal const int MQOP_XA_GET = 2;
        internal const int MQOP_XA_PUT = 1;
        internal const int MQXIDS_COUNT = 50;
        internal const int RMNAMESZ = 0x20;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal const int TM_JOIN = 2;
        internal const int TM_OK = 0;
        internal const int TM_RESUME = 1;
        internal const int TMASYNC = 0x8000000;
        internal const int TMENDRSCAN = 0x800000;
        internal const int TMER_INVAL = -2;
        internal const int TMER_PROTO = -3;
        internal const int TMER_TMERR = -1;
        internal const int TMFAIL = 0x20000000;
        internal const int TMJOIN = 0x200000;
        internal const int TMMIGRATE = 0x100000;
        internal const int TMMULTIPLE = 0x400000;
        internal const int TMNOFLAGS = 0;
        internal const int TMNOMIGRATE = 2;
        internal const int TMNOWAIT = 0x10000000;
        internal const int TMONEPHASE = 0x40000000;
        internal const int TMREGISTER = 1;
        internal const int TMRESUME = 0x8000000;
        internal const int TMSTARTRSCAN = 0x1000000;
        internal const int TMSUCCESS = 0x4000000;
        internal const int TMSUSPEND = 0x2000000;
        internal const int TMUSEASYNC = 4;
        internal const string WMQ_DNET_XAMSGPROP_HOSTANDUSER_NAME = "dnet.XARECOVERY_HOSTANDUSER";
        internal const string WMQ_DNET_XAMSGPROP_QMID_NAME = "dnet.XARECOVERY_QMID";
        internal const string WMQ_DNET_XAMSGPROP_RECINFO_NAME = "dnet.XARECOVERY_RECINFO";
        internal const string WMQ_DNET_XAMSGPROP_RMID_NAME = "dnet.XARECOVERY_RMID";
        internal const string WMQ_DNET_XAMSGPROP_TMADDR_NAME = "dnet.XARECOVERY_TMWHERE";
        internal const string WMQ_DNET_XAMSGPROP_TTIMEOUT_NAME = "dnet.XARECOVERY_TTIMEOUT";
        internal const string WMQ_DNET_XAMSGPROP_XID_NAME = "dnet.XARECOVERY_XID";
        internal const string WMQ_DOTNET_XA_RECOVERY_QUEUE = "SYSTEM.DOTNET.XARECOVERY.QUEUE";
        internal const int XA_HEURCOM = 7;
        internal const int XA_HEURHAZ = 8;
        internal const int XA_HEURMIX = 5;
        internal const int XA_HEURRB = 6;
        internal const int XA_NOMIGRATE = 9;
        internal const int XA_OK = 0;
        internal const int XA_RBBASE = 100;
        internal const int XA_RBCOMMFAIL = 0x65;
        internal const int XA_RBDEADLOCK = 0x66;
        internal const int XA_RBEND = 0x6b;
        internal const int XA_RBINTEGRITY = 0x67;
        internal const int XA_RBOTHER = 0x68;
        internal const int XA_RBPROTO = 0x69;
        internal const int XA_RBROLLBACK = 100;
        internal const int XA_RBTIMEOUT = 0x6a;
        internal const int XA_RBTRANSIENT = 0x6b;
        internal const int XA_RDONLY = 3;
        internal const int XA_RETRY = 4;
        public const uint xadmRC_UNEXPECTED_ERROR = 0x20008385;
        internal const int XAER_ASYNC = -2;
        internal const int XAER_DUPID = -8;
        internal const int XAER_INVAL = -5;
        internal const int XAER_NOTA = -4;
        internal const int XAER_OUTSIDE = -9;
        internal const int XAER_PROTO = -6;
        internal const int XAER_RMERR = -3;
        internal const int XAER_RMFAIL = -7;
        internal const int XIDDATASIZE = 0x80;

        protected MQXAConstants()
        {
        }
    }
}

