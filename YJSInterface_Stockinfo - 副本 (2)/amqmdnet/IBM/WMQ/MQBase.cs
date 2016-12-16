namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public abstract class MQBase
    {
        protected int errorCode;
        protected string objectId = string.Empty;
        protected static int s_ExceptionThreshold;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private bool traceEnabled = CommonServices.TraceEnabled();
        protected int unsafe_compCode;
        protected int unsafe_reason;

        protected MQBase()
        {
            this.objectId = base.GetType() + "#" + this.GetHashCode().ToString("X8");
        }

        public virtual void AddFieldsToFormatter(NmqiStructureFormatter fmt)
        {
            fmt.Add("Object ID", this.ToString());
        }

        protected void BlankPad(byte[] value)
        {
            int index = Array.LastIndexOf<byte>(value, 0);
            while (index > -1)
            {
                if (value[index] != 0)
                {
                    break;
                }
                value[index--] = 0x20;
            }
        }

        protected uint DisplayMessage(uint returncode, uint mtype)
        {
            return CommonServices.DisplayMessage(this.objectId, null, returncode, mtype);
        }

        protected uint FFST(string sccsid, string lineno, uint method, uint probeid, uint tag, ushort alert_num)
        {
            return CommonServices.FFST(this.objectId, sccsid, lineno, 0x48, method, probeid, tag, alert_num);
        }

        protected void GetBytes(string value, ref byte[] buffer)
        {
            Encoding.ASCII.GetBytes(value.PadRight(buffer.Length, '\0'), 0, buffer.Length, buffer, 0);
        }

        protected void GetBytesRightPad(string value, ref byte[] buffer)
        {
            Encoding.ASCII.GetBytes(value.PadRight(buffer.Length), 0, buffer.Length, buffer, 0);
        }

        protected string GetString(byte[] value)
        {
            return Encoding.ASCII.GetString(value, 0, value.Length).TrimEnd(new char[1]);
        }

        protected string GetString(byte[] value, int nBytes)
        {
            return Encoding.ASCII.GetString(value, 0, nBytes).TrimEnd(new char[1]);
        }

        public virtual byte[] ResizeArrayToCorrectLength(byte[] input, int actualLength)
        {
            if (input != null)
            {
                if (input.Length == actualLength)
                {
                    return input;
                }
                int length = input.Length;
                if (length < actualLength)
                {
                    byte[] buffer = new byte[length];
                    Buffer.BlockCopy(input, 0, buffer, 0, length);
                    input = new byte[actualLength];
                    Buffer.BlockCopy(buffer, 0, input, 0, length);
                    int num2 = actualLength - length;
                    for (int i = 0; i < num2; i++)
                    {
                        input[length + i] = 0;
                    }
                    return input;
                }
                byte[] dst = new byte[actualLength];
                Buffer.BlockCopy(input, 0, dst, 0, dst.Length);
                input = dst;
            }
            return input;
        }

        protected void throwNewMQException(int cc, int rc)
        {
            this.unsafe_compCode = cc;
            this.unsafe_reason = rc;
            if (cc >= s_ExceptionThreshold)
            {
                throw new MQException(cc, rc);
            }
        }

        public override string ToString()
        {
            return this.objectId;
        }

        public void TraceFields()
        {
            if (this.traceEnabled)
            {
                NmqiStructureFormatter fmt = new NmqiStructureFormatter(15, 2);
                this.AddFieldsToFormatter(fmt);
                fmt.Trace();
            }
        }

        protected void TrAPI(uint method, string traceText)
        {
            if (this.traceEnabled)
            {
                CommonServices.TraceText(this.objectId, 0x48, method, "!! - " + traceText);
            }
        }

        protected void TrAPIBuffer(uint method, byte[] buffer, int length)
        {
            if (this.traceEnabled)
            {
                if (length < 0x80)
                {
                    CommonServices.TraceUserData(this.objectId, 0x48, method, 0, "Buffer", 0, length, buffer);
                }
                else
                {
                    CommonServices.TraceUserData(this.objectId, 0x48, method, 0, "Buffer", 0, 0x40, buffer);
                    this.TrText(method, " .............");
                    CommonServices.TraceUserData(this.objectId, 0x48, method, 0, null, (uint) (length - 0x40), 0x40, buffer);
                }
            }
        }

        protected void TrAPICCRC(uint method, int compCode, int reason)
        {
            if (this.traceEnabled)
            {
                this.TrData(method, 0, "CompCode", BitConverter.GetBytes(compCode));
                this.TrData(method, 0, "Reason", BitConverter.GetBytes(reason));
            }
        }

        protected void TrAPIInput(uint method, string traceParm)
        {
            if (this.traceEnabled)
            {
                this.TrAPI(method, traceParm.PadRight(15) + ": Input Parm");
            }
        }

        protected void TrAPIOutput(uint method, string traceParm)
        {
            if (this.traceEnabled)
            {
                this.TrAPI(method, traceParm.PadRight(15) + ": Output Parm");
            }
        }

        protected void TrCommsData(uint method, ushort level, string caption, int offset, int length, byte[] buf)
        {
            if (this.traceEnabled)
            {
                CommonServices.TraceUserData(this.objectId, 0x48, method, level, caption, (uint) offset, length, buf);
            }
        }

        protected void TrConstructor(string sccsid)
        {
            if (this.traceEnabled)
            {
                CommonServices.TraceConstructor(this.objectId, sccsid);
            }
        }

        protected void TrConstructor(string sccsid, object[] parms)
        {
            if (this.traceEnabled)
            {
                CommonServices.TraceConstructor(this.objectId, sccsid);
            }
        }

        protected void TrData(uint method, ushort level, string caption, byte[] buf)
        {
            if (this.traceEnabled)
            {
                this.TrData(method, level, caption, 0, -1, buf);
            }
        }

        protected void TrData(uint method, ushort level, string caption, int offset, int length, byte[] buf)
        {
            if (this.traceEnabled)
            {
                CommonServices.TraceData(this.objectId, 0x48, method, level, caption, (uint) offset, length, buf);
            }
        }

        protected virtual void TrEntry(uint method)
        {
            if (this.traceEnabled)
            {
                CommonServices.TraceEntry(this.objectId, 0x48, method);
            }
        }

        protected virtual void TrEntry(uint method, object[] parms)
        {
            if (this.traceEnabled)
            {
                CommonServices.TraceEntry(this.objectId, 0x48, method, parms);
            }
        }

        protected void TrException(uint method, Exception ex)
        {
            if (this.traceEnabled)
            {
                CommonServices.TraceException(this.objectId, 0x48, method, ex);
            }
        }

        protected void TrException(uint method, Exception ex, int count)
        {
            if (this.traceEnabled)
            {
                CommonServices.TraceException(this.objectId, 0x48, method, ex);
            }
        }

        protected void TrExit(uint method)
        {
            if (this.traceEnabled)
            {
                CommonServices.TraceExit(this.objectId, 0x48, method, this.unsafe_reason);
            }
        }

        protected void TrExit(uint method, int index)
        {
            if (this.traceEnabled)
            {
                CommonServices.TraceExit(this.objectId, 0x48, method, index, this.unsafe_reason);
            }
        }

        protected void TrExit(uint method, object result)
        {
            if (this.traceEnabled)
            {
                CommonServices.TraceExit(this.objectId, 0x48, method, result, this.unsafe_reason);
            }
        }

        protected void TrExit(uint method, object result, int index)
        {
            if (this.traceEnabled)
            {
                CommonServices.TraceExit(this.objectId, 0x48, method, result, index, this.unsafe_reason);
            }
        }

        protected void TrText(string traceText)
        {
            if (this.traceEnabled)
            {
                CommonServices.TraceText(this.objectId, traceText);
            }
        }

        protected void TrText(uint method, string traceText)
        {
            if (this.traceEnabled)
            {
                CommonServices.TraceText(this.objectId, 0x48, method, traceText);
            }
        }

        public bool TraceEnabled
        {
            get
            {
                return this.traceEnabled;
            }
        }

        public delegate void CallbackDelegate(int hconn, IntPtr mqmd, IntPtr mqgmo, IntPtr buffer, IntPtr mqcbc);

        [StructLayout(LayoutKind.Sequential)]
        public struct lpiGETOPT
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            private byte[] strucId;
            private int version;
            private int options;
            private int queueEmpty;
            private ulong qTime;
            private int inherited;
            private short publishFlags;
            private byte propFlags;
            private byte padding;
            private IntPtr newMsgBuffer;
            private int newMsgBufferLen;
            private int maxMsgBufferLen;
            private int spare1;
            public byte[] StrucId
            {
                get
                {
                    return this.strucId;
                }
                set
                {
                    this.strucId = value;
                }
            }
            public int Version
            {
                get
                {
                    return this.version;
                }
                set
                {
                    this.version = value;
                }
            }
            public int Options
            {
                get
                {
                    return this.options;
                }
                set
                {
                    this.options = value;
                }
            }
            public int QueueEmpty
            {
                get
                {
                    return this.queueEmpty;
                }
                set
                {
                    this.queueEmpty = value;
                }
            }
            public ulong QTime
            {
                get
                {
                    return this.qTime;
                }
                set
                {
                    this.qTime = value;
                }
            }
            public int Inherited
            {
                get
                {
                    return this.inherited;
                }
                set
                {
                    this.inherited = value;
                }
            }
            public short PublishFlags
            {
                get
                {
                    return this.publishFlags;
                }
                set
                {
                    this.publishFlags = value;
                }
            }
            public byte PropFlags
            {
                get
                {
                    return this.propFlags;
                }
                set
                {
                    this.propFlags = value;
                }
            }
            public byte Padding
            {
                get
                {
                    return this.padding;
                }
                set
                {
                    this.padding = value;
                }
            }
            public IntPtr NewMsgBuffer
            {
                get
                {
                    return this.newMsgBuffer;
                }
                set
                {
                    this.newMsgBuffer = value;
                }
            }
            public int NewMsgBufferLen
            {
                get
                {
                    return this.newMsgBufferLen;
                }
                set
                {
                    this.newMsgBufferLen = value;
                }
            }
            public int MaxMsgBufferLen
            {
                get
                {
                    return this.maxMsgBufferLen;
                }
                set
                {
                    this.maxMsgBufferLen = value;
                }
            }
            public int Spare1
            {
                get
                {
                    return this.spare1;
                }
                set
                {
                    this.spare1 = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MQAIR
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            public byte[] strucId;
            private int version;
            private int authInfoType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x108)]
            public byte[] authInfoConnName;
            private IntPtr _LDAPUserNamePtr;
            private int _LDAPUserNameOffset;
            private int _LDAPUserNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            public byte[] _LDAPPassword;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x100)]
            public byte[] _OCSPResponderURL;
            public byte[] StrucId
            {
                get
                {
                    return this.strucId;
                }
                set
                {
                    this.strucId = value;
                }
            }
            public int Version
            {
                get
                {
                    return this.version;
                }
                set
                {
                    this.version = value;
                }
            }
            public int AuthInfoType
            {
                get
                {
                    return this.authInfoType;
                }
                set
                {
                    this.authInfoType = value;
                }
            }
            public byte[] AuthInfoConnName
            {
                get
                {
                    return this.authInfoConnName;
                }
                set
                {
                    this.authInfoConnName = value;
                }
            }
            public IntPtr LDAPUserNamePtr
            {
                get
                {
                    return this._LDAPUserNamePtr;
                }
                set
                {
                    this._LDAPUserNamePtr = value;
                }
            }
            public int LDAPUserNameOffset
            {
                get
                {
                    return this._LDAPUserNameOffset;
                }
                set
                {
                    this._LDAPUserNameOffset = value;
                }
            }
            public int LDAPUserNameLength
            {
                get
                {
                    return this._LDAPUserNameLength;
                }
                set
                {
                    this._LDAPUserNameLength = value;
                }
            }
            public byte[] LDAPPassword
            {
                get
                {
                    return this._LDAPPassword;
                }
                set
                {
                    this._LDAPPassword = value;
                }
            }
            public byte[] OCSPResponderURL
            {
                get
                {
                    return this._OCSPResponderURL;
                }
                set
                {
                    this._OCSPResponderURL = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MQBO
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            private byte[] strucId;
            private int version;
            private int options;
            public byte[] StrucID
            {
                get
                {
                    return this.strucId;
                }
                set
                {
                    this.strucId = value;
                }
            }
            public int Version
            {
                get
                {
                    return this.version;
                }
                set
                {
                    this.version = value;
                }
            }
            public int Options
            {
                get
                {
                    return this.options;
                }
                set
                {
                    this.options = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MQCD
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=20)]
            private byte[] channelName;
            private int version;
            private int channelType;
            private int transportType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x40)]
            public byte[] desc;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            public byte[] qMgrName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            public byte[] xmitQName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=20)]
            public byte[] shortConnectionName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=20)]
            private byte[] _MCAName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
            private byte[] modeName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x40)]
            private byte[] tpName;
            private int batchSize;
            private int discInterval;
            private int shortRetryCount;
            private int shortRetryInterval;
            private int longRetryCount;
            private int longRetryInterval;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x80)]
            private byte[] securityExit;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x80)]
            private byte[] msgExit;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x80)]
            private byte[] sendExit;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x80)]
            private byte[] receiveExit;
            private int seqNumberWrap;
            private int maxMsgLength;
            private int putAuthority;
            private int dataConversion;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            private byte[] securityUserData;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            private byte[] msgUserData;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            private byte[] sendUserData;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            private byte[] receiveUserData;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            public byte[] userIdentifier;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            public byte[] password;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            public byte[] _MCAUserIdentifier;
            private int mcaType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x108)]
            private byte[] connectionName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            public byte[] remoteUserIdentifier;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            public byte[] remotePassword;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x80)]
            public byte[] msgRetryExit;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            public byte[] msgRetryUserData;
            private int msgRetryCount;
            private int msgRetryInterval;
            private int heartBeatInterval;
            private int batchInterval;
            private int nonPersistentMsgSpeed;
            private int strucLength;
            private int exitNameLength;
            private int exitDataLength;
            private int msgExitsDefined;
            private int sendExitsDefined;
            private int receiveExitsDefined;
            private IntPtr msgExitPtr;
            private IntPtr msgUserDataPtr;
            private IntPtr sendExitPtr;
            private IntPtr sendUserDataPtr;
            private IntPtr receiveExitPtr;
            private IntPtr receiveUserDataPtr;
            private IntPtr clusterPtr;
            private int clustersDefined;
            private int networkPriority;
            private int longMCAUserIdLength;
            private int longRemoteUserIdLength;
            private IntPtr longMCAUserIdPtr;
            private IntPtr longRemoteUserIdPtr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=40)]
            public byte[] _MCASecurityId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=40)]
            public byte[] remoteSecurityId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            public byte[] _SSLCipherSpec;
            private IntPtr _SSLPeerNamePtr;
            private int _SSLPeerNameLength;
            private int _SSLClientAuth;
            private int keepAliveInterval;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            private byte[] localAddress;
            private int batchHeartbeat;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
            public int[] hdrCompList;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
            public int[] msgCompList;
            private int _CLWLChannelRank;
            private int _CLWLChannelPriority;
            private int _CLWLChannelWeight;
            private int channelMonitoring;
            private int channelStatistics;
            private int sharingConversations;
            private int propertyControl;
            private int maxInstances;
            private int maxInstancesPerClient;
            private int clientChannelWeight;
            private int connectionAffinity;
            private int batchDataLimit;
            private int useDLQ;
            private int defReconnect;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x40)]
            private byte[] certificateLabel;
            public byte[] ChannelName
            {
                get
                {
                    return this.channelName;
                }
                set
                {
                    this.channelName = value;
                }
            }
            public int Version
            {
                get
                {
                    return this.version;
                }
                set
                {
                    this.version = value;
                }
            }
            public int ChannelType
            {
                get
                {
                    return this.channelType;
                }
                set
                {
                    this.channelType = value;
                }
            }
            public int TransportType
            {
                get
                {
                    return this.transportType;
                }
                set
                {
                    this.transportType = value;
                }
            }
            public byte[] Desc
            {
                get
                {
                    return this.desc;
                }
                set
                {
                    this.desc = value;
                }
            }
            public byte[] QMgrName
            {
                get
                {
                    return this.qMgrName;
                }
                set
                {
                    this.qMgrName = value;
                }
            }
            public byte[] XmitQName
            {
                get
                {
                    return this.xmitQName;
                }
                set
                {
                    this.xmitQName = value;
                }
            }
            public byte[] ShortConnectionName
            {
                get
                {
                    return this.shortConnectionName;
                }
                set
                {
                    this.shortConnectionName = value;
                }
            }
            public byte[] MCAName
            {
                get
                {
                    return this._MCAName;
                }
                set
                {
                    this._MCAName = value;
                }
            }
            public byte[] ModeName
            {
                get
                {
                    return this.modeName;
                }
                set
                {
                    this.modeName = value;
                }
            }
            public byte[] TpName
            {
                get
                {
                    return this.tpName;
                }
                set
                {
                    this.tpName = value;
                }
            }
            public int BatchSize
            {
                get
                {
                    return this.batchSize;
                }
                set
                {
                    this.batchSize = value;
                }
            }
            public int DiscInterval
            {
                get
                {
                    return this.discInterval;
                }
                set
                {
                    this.discInterval = value;
                }
            }
            public int ShortRetryCount
            {
                get
                {
                    return this.shortRetryCount;
                }
                set
                {
                    this.shortRetryCount = value;
                }
            }
            public int ShortRetryInterval
            {
                get
                {
                    return this.shortRetryInterval;
                }
                set
                {
                    this.shortRetryInterval = value;
                }
            }
            public int LongRetryCount
            {
                get
                {
                    return this.longRetryCount;
                }
                set
                {
                    this.longRetryCount = value;
                }
            }
            public int LongRetryInterval
            {
                get
                {
                    return this.longRetryInterval;
                }
                set
                {
                    this.longRetryInterval = value;
                }
            }
            public byte[] SecurityExit
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
            public byte[] MsgExit
            {
                get
                {
                    return this.msgExit;
                }
                set
                {
                    this.msgExit = value;
                }
            }
            public byte[] SendExit
            {
                get
                {
                    return this.sendExit;
                }
                set
                {
                    this.sendExit = value;
                }
            }
            public byte[] ReceiveExit
            {
                get
                {
                    return this.receiveExit;
                }
                set
                {
                    this.receiveExit = value;
                }
            }
            public int SeqNumberWrap
            {
                get
                {
                    return this.seqNumberWrap;
                }
                set
                {
                    this.seqNumberWrap = value;
                }
            }
            public int MaxMsgLength
            {
                get
                {
                    return this.maxMsgLength;
                }
                set
                {
                    this.maxMsgLength = value;
                }
            }
            public int PutAuthority
            {
                get
                {
                    return this.putAuthority;
                }
                set
                {
                    this.putAuthority = value;
                }
            }
            public int DataConversion
            {
                get
                {
                    return this.dataConversion;
                }
                set
                {
                    this.dataConversion = value;
                }
            }
            public byte[] SecurityUserData
            {
                get
                {
                    return this.securityUserData;
                }
                set
                {
                    this.securityUserData = value;
                }
            }
            public byte[] MsgUserData
            {
                get
                {
                    return this.msgUserData;
                }
                set
                {
                    this.msgUserData = value;
                }
            }
            public byte[] SendUserData
            {
                get
                {
                    return this.sendUserData;
                }
                set
                {
                    this.sendUserData = value;
                }
            }
            public byte[] ReceiveUserData
            {
                get
                {
                    return this.receiveUserData;
                }
                set
                {
                    this.receiveUserData = value;
                }
            }
            public byte[] UserIdentifier
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
            public byte[] Password
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
            public byte[] MCAUserIdentifier
            {
                get
                {
                    return this._MCAUserIdentifier;
                }
                set
                {
                    this._MCAUserIdentifier = value;
                }
            }
            public int McaType
            {
                get
                {
                    return this.mcaType;
                }
                set
                {
                    this.mcaType = value;
                }
            }
            public byte[] ConnectionName
            {
                get
                {
                    return this.connectionName;
                }
                set
                {
                    this.connectionName = value;
                }
            }
            public byte[] RemoteUserIdentifier
            {
                get
                {
                    return this.remoteUserIdentifier;
                }
                set
                {
                    this.remoteUserIdentifier = value;
                }
            }
            public byte[] RemotePassword
            {
                get
                {
                    return this.remotePassword;
                }
                set
                {
                    this.remotePassword = value;
                }
            }
            public byte[] MsgRetryExit
            {
                get
                {
                    return this.msgRetryExit;
                }
                set
                {
                    this.msgRetryExit = value;
                }
            }
            public byte[] MsgRetryUserData
            {
                get
                {
                    return this.msgRetryUserData;
                }
                set
                {
                    this.msgRetryUserData = value;
                }
            }
            public int MsgRetryCount
            {
                get
                {
                    return this.msgRetryCount;
                }
                set
                {
                    this.msgRetryCount = value;
                }
            }
            public int MsgRetryInterval
            {
                get
                {
                    return this.msgRetryInterval;
                }
                set
                {
                    this.msgRetryInterval = value;
                }
            }
            public int HeartBeatInterval
            {
                get
                {
                    return this.heartBeatInterval;
                }
                set
                {
                    this.heartBeatInterval = value;
                }
            }
            public int BatchInterval
            {
                get
                {
                    return this.batchInterval;
                }
                set
                {
                    this.batchInterval = value;
                }
            }
            public int NonPersistentMsgSpeed
            {
                get
                {
                    return this.nonPersistentMsgSpeed;
                }
                set
                {
                    this.nonPersistentMsgSpeed = value;
                }
            }
            public int StrucLength
            {
                get
                {
                    return this.strucLength;
                }
                set
                {
                    this.strucLength = value;
                }
            }
            public int ExitNameLength
            {
                get
                {
                    return this.exitNameLength;
                }
                set
                {
                    this.exitNameLength = value;
                }
            }
            public int ExitDataLength
            {
                get
                {
                    return this.exitDataLength;
                }
                set
                {
                    this.exitDataLength = value;
                }
            }
            public int MsgExitsDefined
            {
                get
                {
                    return this.msgExitsDefined;
                }
                set
                {
                    this.msgExitsDefined = value;
                }
            }
            public int SendExitsDefined
            {
                get
                {
                    return this.sendExitsDefined;
                }
                set
                {
                    this.sendExitsDefined = value;
                }
            }
            public int ReceiveExitsDefined
            {
                get
                {
                    return this.receiveExitsDefined;
                }
                set
                {
                    this.receiveExitsDefined = value;
                }
            }
            public IntPtr MsgExitPtr
            {
                get
                {
                    return this.msgExitPtr;
                }
                set
                {
                    this.msgExitPtr = value;
                }
            }
            public IntPtr MsgUserDataPtr
            {
                get
                {
                    return this.msgUserDataPtr;
                }
                set
                {
                    this.msgUserDataPtr = value;
                }
            }
            public IntPtr SendExitPtr
            {
                get
                {
                    return this.sendExitPtr;
                }
                set
                {
                    this.sendExitPtr = value;
                }
            }
            public IntPtr SendUserDataPtr
            {
                get
                {
                    return this.sendUserDataPtr;
                }
                set
                {
                    this.sendUserDataPtr = value;
                }
            }
            public IntPtr ReceiveExitPtr
            {
                get
                {
                    return this.receiveExitPtr;
                }
                set
                {
                    this.receiveExitPtr = value;
                }
            }
            public IntPtr ReceiveUserDataPtr
            {
                get
                {
                    return this.receiveUserDataPtr;
                }
                set
                {
                    this.receiveUserDataPtr = value;
                }
            }
            public IntPtr ClusterPtr
            {
                get
                {
                    return this.clusterPtr;
                }
                set
                {
                    this.clusterPtr = value;
                }
            }
            public int ClustersDefined
            {
                get
                {
                    return this.clustersDefined;
                }
                set
                {
                    this.clustersDefined = value;
                }
            }
            public int NetworkPriority
            {
                get
                {
                    return this.networkPriority;
                }
                set
                {
                    this.networkPriority = value;
                }
            }
            public int LongMCAUserIdLength
            {
                get
                {
                    return this.longMCAUserIdLength;
                }
                set
                {
                    this.longMCAUserIdLength = value;
                }
            }
            public int LongRemoteUserIdLength
            {
                get
                {
                    return this.longRemoteUserIdLength;
                }
                set
                {
                    this.longRemoteUserIdLength = value;
                }
            }
            public IntPtr LongMCAUserIdPtr
            {
                get
                {
                    return this.longMCAUserIdPtr;
                }
                set
                {
                    this.longMCAUserIdPtr = value;
                }
            }
            public IntPtr LongRemoteUserIdPtr
            {
                get
                {
                    return this.longRemoteUserIdPtr;
                }
                set
                {
                    this.longRemoteUserIdPtr = value;
                }
            }
            public byte[] MCASecurityId
            {
                get
                {
                    return this._MCASecurityId;
                }
                set
                {
                    this._MCASecurityId = value;
                }
            }
            public byte[] RemoteSecurityId
            {
                get
                {
                    return this.remoteSecurityId;
                }
                set
                {
                    this.remoteSecurityId = value;
                }
            }
            public byte[] SSLCipherSpec
            {
                get
                {
                    return this._SSLCipherSpec;
                }
                set
                {
                    this._SSLCipherSpec = value;
                }
            }
            public IntPtr SSLPeerNamePtr
            {
                get
                {
                    return this._SSLPeerNamePtr;
                }
                set
                {
                    this._SSLPeerNamePtr = value;
                }
            }
            public int SSLPeerNameLength
            {
                get
                {
                    return this._SSLPeerNameLength;
                }
                set
                {
                    this._SSLPeerNameLength = value;
                }
            }
            public int SSLClientAuth
            {
                get
                {
                    return this._SSLClientAuth;
                }
                set
                {
                    this._SSLClientAuth = value;
                }
            }
            public int KeepAliveInterval
            {
                get
                {
                    return this.keepAliveInterval;
                }
                set
                {
                    this.keepAliveInterval = value;
                }
            }
            public byte[] LocalAddress
            {
                get
                {
                    return this.localAddress;
                }
                set
                {
                    this.localAddress = value;
                }
            }
            public int BatchHeartbeat
            {
                get
                {
                    return this.batchHeartbeat;
                }
                set
                {
                    this.batchHeartbeat = value;
                }
            }
            public int[] HdrCompList
            {
                get
                {
                    return this.hdrCompList;
                }
                set
                {
                    this.hdrCompList = value;
                }
            }
            public int[] MsgCompList
            {
                get
                {
                    return this.msgCompList;
                }
                set
                {
                    this.msgCompList = value;
                }
            }
            public int CLWLChannelRank
            {
                get
                {
                    return this._CLWLChannelRank;
                }
                set
                {
                    this._CLWLChannelRank = value;
                }
            }
            public int CLWLChannelPriority
            {
                get
                {
                    return this._CLWLChannelPriority;
                }
                set
                {
                    this._CLWLChannelPriority = value;
                }
            }
            public int CLWLChannelWeight
            {
                get
                {
                    return this._CLWLChannelWeight;
                }
                set
                {
                    this._CLWLChannelWeight = value;
                }
            }
            public int ChannelMonitoring
            {
                get
                {
                    return this.channelMonitoring;
                }
                set
                {
                    this.channelMonitoring = value;
                }
            }
            public int ChannelStatistics
            {
                get
                {
                    return this.channelStatistics;
                }
                set
                {
                    this.channelStatistics = value;
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
            public int PropertyControl
            {
                get
                {
                    return this.propertyControl;
                }
                set
                {
                    this.propertyControl = value;
                }
            }
            public int MaxInstances
            {
                get
                {
                    return this.maxInstances;
                }
                set
                {
                    this.maxInstances = value;
                }
            }
            public int MaxInstancesPerClient
            {
                get
                {
                    return this.maxInstancesPerClient;
                }
                set
                {
                    this.maxInstancesPerClient = value;
                }
            }
            public int ClientChannelWeight
            {
                get
                {
                    return this.clientChannelWeight;
                }
                set
                {
                    this.clientChannelWeight = value;
                }
            }
            public int ConnectionAffinity
            {
                get
                {
                    return this.connectionAffinity;
                }
                set
                {
                    this.connectionAffinity = value;
                }
            }
            public int BatchDataLimit
            {
                get
                {
                    return this.batchDataLimit;
                }
                set
                {
                    this.batchDataLimit = value;
                }
            }
            public int UseDLQ
            {
                get
                {
                    return this.useDLQ;
                }
                set
                {
                    this.useDLQ = value;
                }
            }
            public int DefReconnect
            {
                get
                {
                    return this.defReconnect;
                }
                set
                {
                    this.defReconnect = value;
                }
            }
            public byte[] CertificateLabel
            {
                get
                {
                    return this.certificateLabel;
                }
                set
                {
                    this.certificateLabel = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MQCD32
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=20)]
            private byte[] channelName;
            private int version;
            private int channelType;
            private int transportType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x40)]
            public byte[] desc;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            public byte[] qMgrName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            public byte[] xmitQName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=20)]
            public byte[] shortConnectionName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=20)]
            private byte[] _MCAName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
            private byte[] modeName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x40)]
            private byte[] tpName;
            private int batchSize;
            private int discInterval;
            private int shortRetryCount;
            private int shortRetryInterval;
            private int longRetryCount;
            private int longRetryInterval;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x80)]
            private byte[] securityExit;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x80)]
            private byte[] msgExit;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x80)]
            private byte[] sendExit;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x80)]
            private byte[] receiveExit;
            private int seqNumberWrap;
            private int maxMsgLength;
            private int putAuthority;
            private int dataConversion;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            private byte[] securityUserData;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            private byte[] msgUserData;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            private byte[] sendUserData;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            private byte[] receiveUserData;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            public byte[] userIdentifier;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            public byte[] password;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            public byte[] _MCAUserIdentifier;
            private int mcaType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x108)]
            private byte[] connectionName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            public byte[] remoteUserIdentifier;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            public byte[] remotePassword;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x80)]
            public byte[] msgRetryExit;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            public byte[] msgRetryUserData;
            private int msgRetryCount;
            private int msgRetryInterval;
            private int heartBeatInterval;
            private int batchInterval;
            private int nonPersistentMsgSpeed;
            private int strucLength;
            private int exitNameLength;
            private int exitDataLength;
            private int msgExitsDefined;
            private int sendExitsDefined;
            private int receiveExitsDefined;
            private int msgExitPtr;
            private int msgUserDataPtr;
            private int sendExitPtr;
            private int sendUserDataPtr;
            private int receiveExitPtr;
            private int receiveUserDataPtr;
            private int clusterPtr;
            private int clustersDefined;
            private int networkPriority;
            private int longMCAUserIdLength;
            private int longRemoteUserIdLength;
            private int longMCAUserIdPtr;
            private int longRemoteUserIdPtr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=40)]
            public byte[] _MCASecurityId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=40)]
            public byte[] remoteSecurityId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            public byte[] _SSLCipherSpec;
            private int _SSLPeerNamePtr;
            private int _SSLPeerNameLength;
            private int _SSLClientAuth;
            private int keepAliveInterval;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            private byte[] localAddress;
            private int batchHeartbeat;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
            public int[] hdrCompList;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
            public int[] msgCompList;
            private int _CLWLChannelRank;
            private int _CLWLChannelPriority;
            private int _CLWLChannelWeight;
            private int channelMonitoring;
            private int channelStatistics;
            private int sharingConversations;
            private int propertyControl;
            private int maxInstances;
            private int maxInstancesPerClient;
            private int clientChannelWeight;
            private int connectionAffinity;
            private int batchDataLimit;
            private int useDLQ;
            private int defReconnect;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x40)]
            private byte[] certificateLabel;
            public byte[] ChannelName
            {
                get
                {
                    return this.channelName;
                }
                set
                {
                    this.channelName = value;
                }
            }
            public int Version
            {
                get
                {
                    return this.version;
                }
                set
                {
                    this.version = value;
                }
            }
            public int ChannelType
            {
                get
                {
                    return this.channelType;
                }
                set
                {
                    this.channelType = value;
                }
            }
            public int TransportType
            {
                get
                {
                    return this.transportType;
                }
                set
                {
                    this.transportType = value;
                }
            }
            public byte[] Desc
            {
                get
                {
                    return this.desc;
                }
                set
                {
                    this.desc = value;
                }
            }
            public byte[] QMgrName
            {
                get
                {
                    return this.qMgrName;
                }
                set
                {
                    this.qMgrName = value;
                }
            }
            public byte[] XmitQName
            {
                get
                {
                    return this.xmitQName;
                }
                set
                {
                    this.xmitQName = value;
                }
            }
            public byte[] ShortConnectionName
            {
                get
                {
                    return this.shortConnectionName;
                }
                set
                {
                    this.shortConnectionName = value;
                }
            }
            public byte[] MCAName
            {
                get
                {
                    return this._MCAName;
                }
                set
                {
                    this._MCAName = value;
                }
            }
            public byte[] ModeName
            {
                get
                {
                    return this.modeName;
                }
                set
                {
                    this.modeName = value;
                }
            }
            public byte[] TpName
            {
                get
                {
                    return this.tpName;
                }
                set
                {
                    this.tpName = value;
                }
            }
            public int BatchSize
            {
                get
                {
                    return this.batchSize;
                }
                set
                {
                    this.batchSize = value;
                }
            }
            public int DiscInterval
            {
                get
                {
                    return this.discInterval;
                }
                set
                {
                    this.discInterval = value;
                }
            }
            public int ShortRetryCount
            {
                get
                {
                    return this.shortRetryCount;
                }
                set
                {
                    this.shortRetryCount = value;
                }
            }
            public int ShortRetryInterval
            {
                get
                {
                    return this.shortRetryInterval;
                }
                set
                {
                    this.shortRetryInterval = value;
                }
            }
            public int LongRetryCount
            {
                get
                {
                    return this.longRetryCount;
                }
                set
                {
                    this.longRetryCount = value;
                }
            }
            public int LongRetryInterval
            {
                get
                {
                    return this.longRetryInterval;
                }
                set
                {
                    this.longRetryInterval = value;
                }
            }
            public byte[] SecurityExit
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
            public byte[] MsgExit
            {
                get
                {
                    return this.msgExit;
                }
                set
                {
                    this.msgExit = value;
                }
            }
            public byte[] SendExit
            {
                get
                {
                    return this.sendExit;
                }
                set
                {
                    this.sendExit = value;
                }
            }
            public byte[] ReceiveExit
            {
                get
                {
                    return this.receiveExit;
                }
                set
                {
                    this.receiveExit = value;
                }
            }
            public int SeqNumberWrap
            {
                get
                {
                    return this.seqNumberWrap;
                }
                set
                {
                    this.seqNumberWrap = value;
                }
            }
            public int MaxMsgLength
            {
                get
                {
                    return this.maxMsgLength;
                }
                set
                {
                    this.maxMsgLength = value;
                }
            }
            public int PutAuthority
            {
                get
                {
                    return this.putAuthority;
                }
                set
                {
                    this.putAuthority = value;
                }
            }
            public int DataConversion
            {
                get
                {
                    return this.dataConversion;
                }
                set
                {
                    this.dataConversion = value;
                }
            }
            public byte[] SecurityUserData
            {
                get
                {
                    return this.securityUserData;
                }
                set
                {
                    this.securityUserData = value;
                }
            }
            public byte[] MsgUserData
            {
                get
                {
                    return this.msgUserData;
                }
                set
                {
                    this.msgUserData = value;
                }
            }
            public byte[] SendUserData
            {
                get
                {
                    return this.sendUserData;
                }
                set
                {
                    this.sendUserData = value;
                }
            }
            public byte[] ReceiveUserData
            {
                get
                {
                    return this.receiveUserData;
                }
                set
                {
                    this.receiveUserData = value;
                }
            }
            public byte[] UserIdentifier
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
            public byte[] Password
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
            public byte[] MCAUserIdentifier
            {
                get
                {
                    return this._MCAUserIdentifier;
                }
                set
                {
                    this._MCAUserIdentifier = value;
                }
            }
            public int McaType
            {
                get
                {
                    return this.mcaType;
                }
                set
                {
                    this.mcaType = value;
                }
            }
            public byte[] ConnectionName
            {
                get
                {
                    return this.connectionName;
                }
                set
                {
                    this.connectionName = value;
                }
            }
            public byte[] RemoteUserIdentifier
            {
                get
                {
                    return this.remoteUserIdentifier;
                }
                set
                {
                    this.remoteUserIdentifier = value;
                }
            }
            public byte[] RemotePassword
            {
                get
                {
                    return this.remotePassword;
                }
                set
                {
                    this.remotePassword = value;
                }
            }
            public byte[] MsgRetryExit
            {
                get
                {
                    return this.msgRetryExit;
                }
                set
                {
                    this.msgRetryExit = value;
                }
            }
            public byte[] MsgRetryUserData
            {
                get
                {
                    return this.msgRetryUserData;
                }
                set
                {
                    this.msgRetryUserData = value;
                }
            }
            public int MsgRetryCount
            {
                get
                {
                    return this.msgRetryCount;
                }
                set
                {
                    this.msgRetryCount = value;
                }
            }
            public int MsgRetryInterval
            {
                get
                {
                    return this.msgRetryInterval;
                }
                set
                {
                    this.msgRetryInterval = value;
                }
            }
            public int HeartBeatInterval
            {
                get
                {
                    return this.heartBeatInterval;
                }
                set
                {
                    this.heartBeatInterval = value;
                }
            }
            public int BatchInterval
            {
                get
                {
                    return this.batchInterval;
                }
                set
                {
                    this.batchInterval = value;
                }
            }
            public int NonPersistentMsgSpeed
            {
                get
                {
                    return this.nonPersistentMsgSpeed;
                }
                set
                {
                    this.nonPersistentMsgSpeed = value;
                }
            }
            public int StrucLength
            {
                get
                {
                    return this.strucLength;
                }
                set
                {
                    this.strucLength = value;
                }
            }
            public int ExitNameLength
            {
                get
                {
                    return this.exitNameLength;
                }
                set
                {
                    this.exitNameLength = value;
                }
            }
            public int ExitDataLength
            {
                get
                {
                    return this.exitDataLength;
                }
                set
                {
                    this.exitDataLength = value;
                }
            }
            public int MsgExitsDefined
            {
                get
                {
                    return this.msgExitsDefined;
                }
                set
                {
                    this.msgExitsDefined = value;
                }
            }
            public int SendExitsDefined
            {
                get
                {
                    return this.sendExitsDefined;
                }
                set
                {
                    this.sendExitsDefined = value;
                }
            }
            public int ReceiveExitsDefined
            {
                get
                {
                    return this.receiveExitsDefined;
                }
                set
                {
                    this.receiveExitsDefined = value;
                }
            }
            public int MsgExitPtr
            {
                get
                {
                    return this.msgExitPtr;
                }
                set
                {
                    this.msgExitPtr = value;
                }
            }
            public int MsgUserDataPtr
            {
                get
                {
                    return this.msgUserDataPtr;
                }
                set
                {
                    this.msgUserDataPtr = value;
                }
            }
            public int SendExitPtr
            {
                get
                {
                    return this.sendExitPtr;
                }
                set
                {
                    this.sendExitPtr = value;
                }
            }
            public int SendUserDataPtr
            {
                get
                {
                    return this.sendUserDataPtr;
                }
                set
                {
                    this.sendUserDataPtr = value;
                }
            }
            public int ReceiveExitPtr
            {
                get
                {
                    return this.receiveExitPtr;
                }
                set
                {
                    this.receiveExitPtr = value;
                }
            }
            public int ReceiveUserDataPtr
            {
                get
                {
                    return this.receiveUserDataPtr;
                }
                set
                {
                    this.receiveUserDataPtr = value;
                }
            }
            public int ClusterPtr
            {
                get
                {
                    return this.clusterPtr;
                }
                set
                {
                    this.clusterPtr = value;
                }
            }
            public int ClustersDefined
            {
                get
                {
                    return this.clustersDefined;
                }
                set
                {
                    this.clustersDefined = value;
                }
            }
            public int NetworkPriority
            {
                get
                {
                    return this.networkPriority;
                }
                set
                {
                    this.networkPriority = value;
                }
            }
            public int LongMCAUserIdLength
            {
                get
                {
                    return this.longMCAUserIdLength;
                }
                set
                {
                    this.longMCAUserIdLength = value;
                }
            }
            public int LongRemoteUserIdLength
            {
                get
                {
                    return this.longRemoteUserIdLength;
                }
                set
                {
                    this.longRemoteUserIdLength = value;
                }
            }
            public int LongMCAUserIdPtr
            {
                get
                {
                    return this.longMCAUserIdPtr;
                }
                set
                {
                    this.longMCAUserIdPtr = value;
                }
            }
            public int LongRemoteUserIdPtr
            {
                get
                {
                    return this.longRemoteUserIdPtr;
                }
                set
                {
                    this.longRemoteUserIdPtr = value;
                }
            }
            public byte[] MCASecurityId
            {
                get
                {
                    return this._MCASecurityId;
                }
                set
                {
                    this._MCASecurityId = value;
                }
            }
            public byte[] RemoteSecurityId
            {
                get
                {
                    return this.remoteSecurityId;
                }
                set
                {
                    this.remoteSecurityId = value;
                }
            }
            public byte[] SSLCipherSpec
            {
                get
                {
                    return this._SSLCipherSpec;
                }
                set
                {
                    this._SSLCipherSpec = value;
                }
            }
            public int SSLPeerNamePtr
            {
                get
                {
                    return this._SSLPeerNamePtr;
                }
                set
                {
                    this._SSLPeerNamePtr = value;
                }
            }
            public int SSLPeerNameLength
            {
                get
                {
                    return this._SSLPeerNameLength;
                }
                set
                {
                    this._SSLPeerNameLength = value;
                }
            }
            public int SSLClientAuth
            {
                get
                {
                    return this._SSLClientAuth;
                }
                set
                {
                    this._SSLClientAuth = value;
                }
            }
            public int KeepAliveInterval
            {
                get
                {
                    return this.keepAliveInterval;
                }
                set
                {
                    this.keepAliveInterval = value;
                }
            }
            public byte[] LocalAddress
            {
                get
                {
                    return this.localAddress;
                }
                set
                {
                    this.localAddress = value;
                }
            }
            public int BatchHeartbeat
            {
                get
                {
                    return this.batchHeartbeat;
                }
                set
                {
                    this.batchHeartbeat = value;
                }
            }
            public int[] HdrCompList
            {
                get
                {
                    return this.hdrCompList;
                }
                set
                {
                    this.hdrCompList = value;
                }
            }
            public int[] MsgCompList
            {
                get
                {
                    return this.msgCompList;
                }
                set
                {
                    this.msgCompList = value;
                }
            }
            public int CLWLChannelRank
            {
                get
                {
                    return this._CLWLChannelRank;
                }
                set
                {
                    this._CLWLChannelRank = value;
                }
            }
            public int CLWLChannelPriority
            {
                get
                {
                    return this._CLWLChannelPriority;
                }
                set
                {
                    this._CLWLChannelPriority = value;
                }
            }
            public int CLWLChannelWeight
            {
                get
                {
                    return this._CLWLChannelWeight;
                }
                set
                {
                    this._CLWLChannelWeight = value;
                }
            }
            public int ChannelMonitoring
            {
                get
                {
                    return this.channelMonitoring;
                }
                set
                {
                    this.channelMonitoring = value;
                }
            }
            public int ChannelStatistics
            {
                get
                {
                    return this.channelStatistics;
                }
                set
                {
                    this.channelStatistics = value;
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
            public int PropertyControl
            {
                get
                {
                    return this.propertyControl;
                }
                set
                {
                    this.propertyControl = value;
                }
            }
            public int MaxInstances
            {
                get
                {
                    return this.maxInstances;
                }
                set
                {
                    this.maxInstances = value;
                }
            }
            public int MaxInstancesPerClient
            {
                get
                {
                    return this.maxInstancesPerClient;
                }
                set
                {
                    this.maxInstancesPerClient = value;
                }
            }
            public int ClientChannelWeight
            {
                get
                {
                    return this.clientChannelWeight;
                }
                set
                {
                    this.clientChannelWeight = value;
                }
            }
            public int ConnectionAffinity
            {
                get
                {
                    return this.connectionAffinity;
                }
                set
                {
                    this.connectionAffinity = value;
                }
            }
            public int BatchDataLimit
            {
                get
                {
                    return this.batchDataLimit;
                }
                set
                {
                    this.batchDataLimit = value;
                }
            }
            public int UseDLQ
            {
                get
                {
                    return this.useDLQ;
                }
                set
                {
                    this.useDLQ = value;
                }
            }
            public int DefReconnect
            {
                get
                {
                    return this.defReconnect;
                }
                set
                {
                    this.defReconnect = value;
                }
            }
            public byte[] CertificateLabel
            {
                get
                {
                    return this.certificateLabel;
                }
                set
                {
                    this.certificateLabel = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MQCNO
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            private byte[] strucId;
            private int version;
            private int options;
            private int clientConnOffset;
            private IntPtr clientConnPtr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x80)]
            public byte[] connTag;
            private IntPtr _SSLConfigPtr;
            private int _SSLConfigOffset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x18)]
            public byte[] connectionId;
            private int securityParmsOffset;
            private IntPtr securityParmsPtr;
            public byte[] StrucId
            {
                get
                {
                    return this.strucId;
                }
                set
                {
                    this.strucId = value;
                }
            }
            public int Version
            {
                get
                {
                    return this.version;
                }
                set
                {
                    this.version = value;
                }
            }
            public int Options
            {
                get
                {
                    return this.options;
                }
                set
                {
                    this.options = value;
                }
            }
            public int ClientConnOffset
            {
                get
                {
                    return this.clientConnOffset;
                }
                set
                {
                    this.clientConnOffset = value;
                }
            }
            public IntPtr ClientConnPtr
            {
                get
                {
                    return this.clientConnPtr;
                }
                set
                {
                    this.clientConnPtr = value;
                }
            }
            public byte[] ConnTag
            {
                get
                {
                    return this.connTag;
                }
                set
                {
                    this.connTag = value;
                }
            }
            public IntPtr SSLConfigPtr
            {
                get
                {
                    return this._SSLConfigPtr;
                }
                set
                {
                    this._SSLConfigPtr = value;
                }
            }
            public int SSLConfigOffset
            {
                get
                {
                    return this._SSLConfigOffset;
                }
                set
                {
                    this._SSLConfigOffset = value;
                }
            }
            public byte[] ConnectionId
            {
                get
                {
                    return this.connectionId;
                }
                set
                {
                    this.connectionId = value;
                }
            }
            public int SecurityParmsOffset
            {
                get
                {
                    return this.securityParmsOffset;
                }
                set
                {
                    this.securityParmsOffset = value;
                }
            }
            public IntPtr SecurityParmsPtr
            {
                get
                {
                    return this.securityParmsPtr;
                }
                set
                {
                    this.securityParmsPtr = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MQCSP
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            private byte[] strucId;
            private int version;
            private int authenticationType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            public byte[] reserved1;
            private IntPtr _CSPUserIdPtr;
            private int _CSPUserIdOffset;
            private int _CSPUserIdLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
            public byte[] reserved2;
            private IntPtr _CSPPasswordPtr;
            private int _CSPPasswordOffset;
            private int _CSPPasswordLength;
            public byte[] StrucId
            {
                get
                {
                    return this.strucId;
                }
                set
                {
                    this.strucId = value;
                }
            }
            public int Version
            {
                get
                {
                    return this.version;
                }
                set
                {
                    this.version = value;
                }
            }
            public int AuthenticationType
            {
                get
                {
                    return this.authenticationType;
                }
                set
                {
                    this.authenticationType = value;
                }
            }
            public byte[] Reserved1
            {
                get
                {
                    return this.reserved1;
                }
                set
                {
                    this.reserved1 = value;
                }
            }
            public IntPtr CSPUserIdPtr
            {
                get
                {
                    return this._CSPUserIdPtr;
                }
                set
                {
                    this._CSPUserIdPtr = value;
                }
            }
            public int CSPUserIdOffset
            {
                get
                {
                    return this._CSPUserIdOffset;
                }
                set
                {
                    this._CSPUserIdOffset = value;
                }
            }
            public int CSPUserIdLength
            {
                get
                {
                    return this._CSPUserIdLength;
                }
                set
                {
                    this._CSPUserIdLength = value;
                }
            }
            public byte[] Reserved2
            {
                get
                {
                    return this.reserved2;
                }
                set
                {
                    this.reserved2 = value;
                }
            }
            public IntPtr CSPPasswordPtr
            {
                get
                {
                    return this._CSPPasswordPtr;
                }
                set
                {
                    this._CSPPasswordPtr = value;
                }
            }
            public int CSPPasswordOffset
            {
                get
                {
                    return this._CSPPasswordOffset;
                }
                set
                {
                    this._CSPPasswordOffset = value;
                }
            }
            public int CSPPasswordLength
            {
                get
                {
                    return this._CSPPasswordLength;
                }
                set
                {
                    this._CSPPasswordLength = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MQGMO
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            private byte[] strucId;
            private int version;
            private int options;
            private int waitInterval;
            private int signal1;
            private int signal2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            private byte[] resolvedQName;
            private int matchOptions;
            private byte groupStatus;
            private byte segmentStatus;
            private byte segmentation;
            private byte reserved1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
            public byte[] msgToken;
            private int returnedLength;
            public byte[] StrucId
            {
                get
                {
                    return this.strucId;
                }
                set
                {
                    this.strucId = value;
                }
            }
            public int Version
            {
                get
                {
                    return this.version;
                }
                set
                {
                    this.version = value;
                }
            }
            public int Options
            {
                get
                {
                    return this.options;
                }
                set
                {
                    this.options = value;
                }
            }
            public int WaitInterval
            {
                get
                {
                    return this.waitInterval;
                }
                set
                {
                    this.waitInterval = value;
                }
            }
            public int Signal1
            {
                get
                {
                    return this.signal1;
                }
                set
                {
                    this.signal1 = value;
                }
            }
            public int Signal2
            {
                get
                {
                    return this.signal2;
                }
                set
                {
                    this.signal2 = value;
                }
            }
            public byte[] ResolvedQName
            {
                get
                {
                    return this.resolvedQName;
                }
                set
                {
                    this.resolvedQName = value;
                }
            }
            public int MatchOptions
            {
                get
                {
                    return this.matchOptions;
                }
                set
                {
                    this.matchOptions = value;
                }
            }
            public byte GroupStatus
            {
                get
                {
                    return this.groupStatus;
                }
                set
                {
                    this.groupStatus = value;
                }
            }
            public byte SegmentStatus
            {
                get
                {
                    return this.segmentStatus;
                }
                set
                {
                    this.segmentStatus = value;
                }
            }
            public byte Segmentation
            {
                get
                {
                    return this.segmentation;
                }
                set
                {
                    this.segmentation = value;
                }
            }
            public byte Reserved1
            {
                get
                {
                    return this.reserved1;
                }
                set
                {
                    this.reserved1 = value;
                }
            }
            public byte[] MsgToken
            {
                get
                {
                    return this.msgToken;
                }
                set
                {
                    this.msgToken = value;
                }
            }
            public int ReturnedLength
            {
                get
                {
                    return this.returnedLength;
                }
                set
                {
                    this.returnedLength = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MQMD
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            private byte[] strucId;
            private int version;
            private int report;
            private int msgType;
            private int expiry;
            private int feedback;
            private int encoding;
            private int codedCharacterSetId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
            private byte[] format;
            private int priority;
            private int persistence;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x18)]
            public byte[] msgId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x18)]
            public byte[] correlId;
            private int backoutCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            public byte[] replyToQ;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            public byte[] replyToQMgr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            public byte[] userId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            public byte[] accountingToken;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            public byte[] applIdentityData;
            private int putApplType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x1c)]
            public byte[] putApplName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
            public byte[] putDate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
            public byte[] putTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            public byte[] applOriginData;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x18)]
            public byte[] groupId;
            private int msgSequenceNumber;
            private int offset;
            private int msgFlags;
            private int originalLength;
            public byte[] StrucId
            {
                get
                {
                    return this.strucId;
                }
                set
                {
                    this.strucId = value;
                }
            }
            public int Version
            {
                get
                {
                    return this.version;
                }
                set
                {
                    this.version = value;
                }
            }
            public int Report
            {
                get
                {
                    return this.report;
                }
                set
                {
                    this.report = value;
                }
            }
            public int MsgType
            {
                get
                {
                    return this.msgType;
                }
                set
                {
                    this.msgType = value;
                }
            }
            public int Expiry
            {
                get
                {
                    return this.expiry;
                }
                set
                {
                    this.expiry = value;
                }
            }
            public int Feedback
            {
                get
                {
                    return this.feedback;
                }
                set
                {
                    this.feedback = value;
                }
            }
            public int Encoding
            {
                get
                {
                    return this.encoding;
                }
                set
                {
                    this.encoding = value;
                }
            }
            public int CodedCharacterSetId
            {
                get
                {
                    return this.codedCharacterSetId;
                }
                set
                {
                    this.codedCharacterSetId = value;
                }
            }
            public byte[] Format
            {
                get
                {
                    return this.format;
                }
                set
                {
                    this.format = value;
                }
            }
            public int Priority
            {
                get
                {
                    return this.priority;
                }
                set
                {
                    this.priority = value;
                }
            }
            public int Persistence
            {
                get
                {
                    return this.persistence;
                }
                set
                {
                    this.persistence = value;
                }
            }
            public byte[] MsgId
            {
                get
                {
                    return this.msgId;
                }
                set
                {
                    this.msgId = value;
                }
            }
            public byte[] CorrelId
            {
                get
                {
                    return this.correlId;
                }
                set
                {
                    this.correlId = value;
                }
            }
            public int BackoutCount
            {
                get
                {
                    return this.backoutCount;
                }
                set
                {
                    this.backoutCount = value;
                }
            }
            public byte[] ReplyToQ
            {
                get
                {
                    return this.replyToQ;
                }
                set
                {
                    this.replyToQ = value;
                }
            }
            public byte[] ReplyToQMgr
            {
                get
                {
                    return this.replyToQMgr;
                }
                set
                {
                    this.replyToQMgr = value;
                }
            }
            public byte[] UserId
            {
                get
                {
                    return this.userId;
                }
                set
                {
                    this.userId = value;
                }
            }
            public byte[] AccountingToken
            {
                get
                {
                    return this.accountingToken;
                }
                set
                {
                    this.accountingToken = value;
                }
            }
            public byte[] ApplIdentityData
            {
                get
                {
                    return this.applIdentityData;
                }
                set
                {
                    this.applIdentityData = value;
                }
            }
            public int PutApplType
            {
                get
                {
                    return this.putApplType;
                }
                set
                {
                    this.putApplType = value;
                }
            }
            public byte[] PutApplName
            {
                get
                {
                    return this.putApplName;
                }
                set
                {
                    this.putApplName = value;
                }
            }
            public byte[] PutDate
            {
                get
                {
                    return this.putDate;
                }
                set
                {
                    this.putDate = value;
                }
            }
            public byte[] PutTime
            {
                get
                {
                    return this.putTime;
                }
                set
                {
                    this.putTime = value;
                }
            }
            public byte[] ApplOriginData
            {
                get
                {
                    return this.applOriginData;
                }
                set
                {
                    this.applOriginData = value;
                }
            }
            public byte[] GroupId
            {
                get
                {
                    return this.groupId;
                }
                set
                {
                    this.groupId = value;
                }
            }
            public int MsgSequenceNumber
            {
                get
                {
                    return this.msgSequenceNumber;
                }
                set
                {
                    this.msgSequenceNumber = value;
                }
            }
            public int Offset
            {
                get
                {
                    return this.offset;
                }
                set
                {
                    this.offset = value;
                }
            }
            public int MsgFlags
            {
                get
                {
                    return this.msgFlags;
                }
                set
                {
                    this.msgFlags = value;
                }
            }
            public int OriginalLength
            {
                get
                {
                    return this.originalLength;
                }
                set
                {
                    this.originalLength = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MQOD
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            private byte[] strucId;
            private int version;
            private int objectType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            private byte[] objectName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            private byte[] objectQMgrName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            private byte[] dynamicQName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            private byte[] alternateUserId;
            private int recsPresent;
            private int knownDestCount;
            private int unknownDestCount;
            private int invalidDestCount;
            private int objectRecOffset;
            private int responseRecOffset;
            private IntPtr objectRecPtr;
            private IntPtr responseRecPtr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=40)]
            public byte[] alternateSecurityId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            public byte[] resolvedQName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            public byte[] resolvedQMgrName;
            private MQBase.structMQCHARV objectString;
            private MQBase.structMQCHARV selectionString;
            private MQBase.structMQCHARV resolvedObjectString;
            private int resolvedType;
            public byte[] StrucId
            {
                get
                {
                    return this.strucId;
                }
                set
                {
                    this.strucId = value;
                }
            }
            public int Version
            {
                get
                {
                    return this.version;
                }
                set
                {
                    this.version = value;
                }
            }
            public int ObjectType
            {
                get
                {
                    return this.objectType;
                }
                set
                {
                    this.objectType = value;
                }
            }
            public byte[] ObjectName
            {
                get
                {
                    return this.objectName;
                }
                set
                {
                    this.objectName = value;
                }
            }
            public byte[] ObjectQMgrName
            {
                get
                {
                    return this.objectQMgrName;
                }
                set
                {
                    this.objectQMgrName = value;
                }
            }
            public byte[] DynamicQName
            {
                get
                {
                    return this.dynamicQName;
                }
                set
                {
                    this.dynamicQName = value;
                }
            }
            public byte[] AlternateUserId
            {
                get
                {
                    return this.alternateUserId;
                }
                set
                {
                    this.alternateUserId = value;
                }
            }
            public int RecsPresent
            {
                get
                {
                    return this.recsPresent;
                }
                set
                {
                    this.recsPresent = value;
                }
            }
            public int KnownDestCount
            {
                get
                {
                    return this.knownDestCount;
                }
                set
                {
                    this.knownDestCount = value;
                }
            }
            public int UnknownDestCount
            {
                get
                {
                    return this.unknownDestCount;
                }
                set
                {
                    this.unknownDestCount = value;
                }
            }
            public int InvalidDestCount
            {
                get
                {
                    return this.invalidDestCount;
                }
                set
                {
                    this.invalidDestCount = value;
                }
            }
            public int ObjectRecOffset
            {
                get
                {
                    return this.objectRecOffset;
                }
                set
                {
                    this.objectRecOffset = value;
                }
            }
            public int ResponseRecOffset
            {
                get
                {
                    return this.responseRecOffset;
                }
                set
                {
                    this.responseRecOffset = value;
                }
            }
            public IntPtr ObjectRecPtr
            {
                get
                {
                    return this.objectRecPtr;
                }
                set
                {
                    this.objectRecPtr = value;
                }
            }
            public IntPtr ResponseRecPtr
            {
                get
                {
                    return this.responseRecPtr;
                }
                set
                {
                    this.responseRecPtr = value;
                }
            }
            public byte[] AlternateSecurityId
            {
                get
                {
                    return this.alternateSecurityId;
                }
                set
                {
                    this.alternateSecurityId = value;
                }
            }
            public byte[] ResolvedQName
            {
                get
                {
                    return this.resolvedQName;
                }
                set
                {
                    this.resolvedQName = value;
                }
            }
            public byte[] ResolvedQMgrName
            {
                get
                {
                    return this.resolvedQMgrName;
                }
                set
                {
                    this.resolvedQMgrName = value;
                }
            }
            public MQBase.structMQCHARV ObjectString
            {
                get
                {
                    return this.objectString;
                }
                set
                {
                    this.objectString = value;
                }
            }
            public MQBase.structMQCHARV SelectionString
            {
                get
                {
                    return this.selectionString;
                }
                set
                {
                    this.selectionString = value;
                }
            }
            public MQBase.structMQCHARV ResolvedObjectString
            {
                get
                {
                    return this.resolvedObjectString;
                }
                set
                {
                    this.resolvedObjectString = value;
                }
            }
            public int ResolvedType
            {
                get
                {
                    return this.resolvedType;
                }
                set
                {
                    this.resolvedType = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MQOD32
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            private byte[] strucId;
            private int version;
            private int objectType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            private byte[] objectName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            private byte[] objectQMgrName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            private byte[] dynamicQName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            private byte[] alternateUserId;
            private int recsPresent;
            private int knownDestCount;
            private int unknownDestCount;
            private int invalidDestCount;
            private int objectRecOffset;
            private int responseRecOffset;
            private int objectRecPtr;
            private int responseRecPtr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=40)]
            public byte[] alternateSecurityId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            public byte[] resolvedQName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            public byte[] resolvedQMgrName;
            private MQBase.structMQCHARV32 objectString;
            private MQBase.structMQCHARV32 selectionString;
            private MQBase.structMQCHARV32 resolvedObjectString;
            private int resolvedType;
            public byte[] StrucId
            {
                get
                {
                    return this.strucId;
                }
                set
                {
                    this.strucId = value;
                }
            }
            public int Version
            {
                get
                {
                    return this.version;
                }
                set
                {
                    this.version = value;
                }
            }
            public int ObjectType
            {
                get
                {
                    return this.objectType;
                }
                set
                {
                    this.objectType = value;
                }
            }
            public byte[] ObjectName
            {
                get
                {
                    return this.objectName;
                }
                set
                {
                    this.objectName = value;
                }
            }
            public byte[] ObjectQMgrName
            {
                get
                {
                    return this.objectQMgrName;
                }
                set
                {
                    this.objectQMgrName = value;
                }
            }
            public byte[] DynamicQName
            {
                get
                {
                    return this.dynamicQName;
                }
                set
                {
                    this.dynamicQName = value;
                }
            }
            public byte[] AlternateUserId
            {
                get
                {
                    return this.alternateUserId;
                }
                set
                {
                    this.alternateUserId = value;
                }
            }
            public int RecsPresent
            {
                get
                {
                    return this.recsPresent;
                }
                set
                {
                    this.recsPresent = value;
                }
            }
            public int KnownDestCount
            {
                get
                {
                    return this.knownDestCount;
                }
                set
                {
                    this.knownDestCount = value;
                }
            }
            public int UnknownDestCount
            {
                get
                {
                    return this.unknownDestCount;
                }
                set
                {
                    this.unknownDestCount = value;
                }
            }
            public int InvalidDestCount
            {
                get
                {
                    return this.invalidDestCount;
                }
                set
                {
                    this.invalidDestCount = value;
                }
            }
            public int ObjectRecOffset
            {
                get
                {
                    return this.objectRecOffset;
                }
                set
                {
                    this.objectRecOffset = value;
                }
            }
            public int ResponseRecOffset
            {
                get
                {
                    return this.responseRecOffset;
                }
                set
                {
                    this.responseRecOffset = value;
                }
            }
            public int ObjectRecPtr
            {
                get
                {
                    return this.objectRecPtr;
                }
                set
                {
                    this.objectRecPtr = value;
                }
            }
            public int ResponseRecPtr
            {
                get
                {
                    return this.responseRecPtr;
                }
                set
                {
                    this.responseRecPtr = value;
                }
            }
            public byte[] AlternateSecurityId
            {
                get
                {
                    return this.alternateSecurityId;
                }
                set
                {
                    this.alternateSecurityId = value;
                }
            }
            public byte[] ResolvedQName
            {
                get
                {
                    return this.resolvedQName;
                }
                set
                {
                    this.resolvedQName = value;
                }
            }
            public byte[] ResolvedQMgrName
            {
                get
                {
                    return this.resolvedQMgrName;
                }
                set
                {
                    this.resolvedQMgrName = value;
                }
            }
            public MQBase.structMQCHARV32 ObjectString
            {
                get
                {
                    return this.objectString;
                }
                set
                {
                    this.objectString = value;
                }
            }
            public MQBase.structMQCHARV32 SelectionString
            {
                get
                {
                    return this.selectionString;
                }
                set
                {
                    this.selectionString = value;
                }
            }
            public MQBase.structMQCHARV32 ResolvedObjectString
            {
                get
                {
                    return this.resolvedObjectString;
                }
                set
                {
                    this.resolvedObjectString = value;
                }
            }
            public int ResolvedType
            {
                get
                {
                    return this.resolvedType;
                }
                set
                {
                    this.resolvedType = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MQPMO
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            private byte[] strucId;
            private int version;
            private int options;
            private int timeout;
            private int context;
            private int knownDestCount;
            private int unknownDestCount;
            private int invalidDestCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            private byte[] resolvedQName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            private byte[] resolvedQMgrName;
            private int recsPresent;
            private int putMsgRecFields;
            private int putMsgRecOffset;
            private int responseRecOffset;
            private IntPtr putMsgRecPtr;
            private IntPtr responseRecPtr;
            public byte[] StrucId
            {
                get
                {
                    return this.strucId;
                }
                set
                {
                    this.strucId = value;
                }
            }
            public int Version
            {
                get
                {
                    return this.version;
                }
                set
                {
                    this.version = value;
                }
            }
            public int Options
            {
                get
                {
                    return this.options;
                }
                set
                {
                    this.options = value;
                }
            }
            public int Timeout
            {
                get
                {
                    return this.timeout;
                }
                set
                {
                    this.timeout = value;
                }
            }
            public int Context
            {
                get
                {
                    return this.context;
                }
                set
                {
                    this.context = value;
                }
            }
            public int KnownDestCount
            {
                get
                {
                    return this.knownDestCount;
                }
                set
                {
                    this.knownDestCount = value;
                }
            }
            public int UnknownDestCount
            {
                get
                {
                    return this.unknownDestCount;
                }
                set
                {
                    this.unknownDestCount = value;
                }
            }
            public int InvalidDestCount
            {
                get
                {
                    return this.invalidDestCount;
                }
                set
                {
                    this.invalidDestCount = value;
                }
            }
            public byte[] ResolvedQName
            {
                get
                {
                    return this.resolvedQName;
                }
                set
                {
                    this.resolvedQName = value;
                }
            }
            public byte[] ResolvedQMgrName
            {
                get
                {
                    return this.resolvedQMgrName;
                }
                set
                {
                    this.resolvedQMgrName = value;
                }
            }
            public int RecsPresent
            {
                get
                {
                    return this.recsPresent;
                }
                set
                {
                    this.recsPresent = value;
                }
            }
            public int PutMsgRecFields
            {
                get
                {
                    return this.putMsgRecFields;
                }
                set
                {
                    this.putMsgRecFields = value;
                }
            }
            public int PutMsgRecOffset
            {
                get
                {
                    return this.putMsgRecOffset;
                }
                set
                {
                    this.putMsgRecOffset = value;
                }
            }
            public int ResponseRecOffset
            {
                get
                {
                    return this.responseRecOffset;
                }
                set
                {
                    this.responseRecOffset = value;
                }
            }
            public IntPtr PutMsgRecPtr
            {
                get
                {
                    return this.putMsgRecPtr;
                }
                set
                {
                    this.putMsgRecPtr = value;
                }
            }
            public IntPtr ResponseRecPtr
            {
                get
                {
                    return this.responseRecPtr;
                }
                set
                {
                    this.responseRecPtr = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MQSCO
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            private byte[] strucId;
            private int version;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x100)]
            private byte[] keyRepository;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x100)]
            private byte[] cryptoHardware;
            private int authInfoRecCount;
            private int authInfoRecOffset;
            private IntPtr authInfoRecPtr;
            private int keyResetCount;
            private int fipsRequired;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            public int[] encryptionPolicySuiteB;
            private int certificateValPolicy;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x40)]
            private byte[] certificateLabel;
            public byte[] StrucId
            {
                get
                {
                    return this.strucId;
                }
                set
                {
                    this.strucId = value;
                }
            }
            public int Version
            {
                get
                {
                    return this.version;
                }
                set
                {
                    this.version = value;
                }
            }
            public byte[] KeyRepository
            {
                get
                {
                    return this.keyRepository;
                }
                set
                {
                    this.keyRepository = value;
                }
            }
            public byte[] CryptoHardware
            {
                get
                {
                    return this.cryptoHardware;
                }
                set
                {
                    this.cryptoHardware = value;
                }
            }
            public int AuthInfoRecCount
            {
                get
                {
                    return this.authInfoRecCount;
                }
                set
                {
                    this.authInfoRecCount = value;
                }
            }
            public int AuthInfoRecOffset
            {
                get
                {
                    return this.authInfoRecOffset;
                }
                set
                {
                    this.authInfoRecOffset = value;
                }
            }
            public IntPtr AuthInfoRecPtr
            {
                get
                {
                    return this.authInfoRecPtr;
                }
                set
                {
                    this.authInfoRecPtr = value;
                }
            }
            public int KeyResetCount
            {
                get
                {
                    return this.keyResetCount;
                }
                set
                {
                    this.keyResetCount = value;
                }
            }
            public int FipsRequired
            {
                get
                {
                    return this.fipsRequired;
                }
                set
                {
                    this.fipsRequired = value;
                }
            }
            public int[] EncryptionPolicySuiteB
            {
                get
                {
                    return this.encryptionPolicySuiteB;
                }
                set
                {
                    this.encryptionPolicySuiteB = value;
                }
            }
            public int CertificateValPolicy
            {
                get
                {
                    return this.certificateValPolicy;
                }
                set
                {
                    this.certificateValPolicy = value;
                }
            }
            public byte[] CertificateLabel
            {
                get
                {
                    return this.certificateLabel;
                }
                set
                {
                    this.certificateLabel = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MQSD
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            private byte[] strucId;
            private int version;
            private int options;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            private byte[] objectName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            private byte[] alternateUserId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=40)]
            private byte[] alternateSecurityId;
            private int subExpiry;
            private MQBase.structMQCHARV objectString;
            private MQBase.structMQCHARV subName;
            private MQBase.structMQCHARV subUserData;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x18)]
            private byte[] subCorrelId;
            private int pubPriority;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            private byte[] pubAccountingToken;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            private byte[] pubApplIdentityData;
            private MQBase.structMQCHARV selectionString;
            private int subLevel;
            private MQBase.structMQCHARV resObjectString;
            public byte[] StrucId
            {
                get
                {
                    return this.strucId;
                }
                set
                {
                    this.strucId = value;
                }
            }
            public int Version
            {
                get
                {
                    return this.version;
                }
                set
                {
                    this.version = value;
                }
            }
            public int Options
            {
                get
                {
                    return this.options;
                }
                set
                {
                    this.options = value;
                }
            }
            public byte[] ObjectName
            {
                get
                {
                    return this.objectName;
                }
                set
                {
                    this.objectName = value;
                }
            }
            public byte[] AlternateUserId
            {
                get
                {
                    return this.alternateUserId;
                }
                set
                {
                    this.alternateUserId = value;
                }
            }
            public byte[] AlternateSecurityId
            {
                get
                {
                    return this.alternateSecurityId;
                }
                set
                {
                    this.alternateSecurityId = value;
                }
            }
            public int SubExpiry
            {
                get
                {
                    return this.subExpiry;
                }
                set
                {
                    this.subExpiry = value;
                }
            }
            public MQBase.structMQCHARV ObjectString
            {
                get
                {
                    return this.objectString;
                }
                set
                {
                    this.objectString = value;
                }
            }
            public MQBase.structMQCHARV SubName
            {
                get
                {
                    return this.subName;
                }
                set
                {
                    this.subName = value;
                }
            }
            public MQBase.structMQCHARV SubUserData
            {
                get
                {
                    return this.subUserData;
                }
                set
                {
                    this.subUserData = value;
                }
            }
            public byte[] SubCorrelId
            {
                get
                {
                    return this.subCorrelId;
                }
                set
                {
                    this.subCorrelId = value;
                }
            }
            public int PubPriority
            {
                get
                {
                    return this.pubPriority;
                }
                set
                {
                    this.pubPriority = value;
                }
            }
            public byte[] PubAccountingToken
            {
                get
                {
                    return this.pubAccountingToken;
                }
                set
                {
                    this.pubAccountingToken = value;
                }
            }
            public byte[] PubApplIdentityData
            {
                get
                {
                    return this.pubApplIdentityData;
                }
                set
                {
                    this.pubApplIdentityData = value;
                }
            }
            public MQBase.structMQCHARV SelectionString
            {
                get
                {
                    return this.selectionString;
                }
                set
                {
                    this.selectionString = value;
                }
            }
            public int SubLevel
            {
                get
                {
                    return this.subLevel;
                }
                set
                {
                    this.subLevel = value;
                }
            }
            public MQBase.structMQCHARV ResObjectString
            {
                get
                {
                    return this.resObjectString;
                }
                set
                {
                    this.resObjectString = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MQSD32
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            private byte[] strucId;
            private int version;
            private int options;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            private byte[] objectName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            private byte[] alternateUserId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=40)]
            private byte[] alternateSecurityId;
            private int subExpiry;
            private MQBase.structMQCHARV32 objectString;
            private MQBase.structMQCHARV32 subName;
            private MQBase.structMQCHARV32 subUserData;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x18)]
            private byte[] subCorrelId;
            private int pubPriority;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            private byte[] pubAccountingToken;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            private byte[] pubApplIdentityData;
            private MQBase.structMQCHARV32 selectionString;
            private int subLevel;
            private MQBase.structMQCHARV32 resObjectString;
            public byte[] StrucId
            {
                get
                {
                    return this.strucId;
                }
                set
                {
                    this.strucId = value;
                }
            }
            public int Version
            {
                get
                {
                    return this.version;
                }
                set
                {
                    this.version = value;
                }
            }
            public int Options
            {
                get
                {
                    return this.options;
                }
                set
                {
                    this.options = value;
                }
            }
            public byte[] ObjectName
            {
                get
                {
                    return this.objectName;
                }
                set
                {
                    this.objectName = value;
                }
            }
            public byte[] AlternateUserId
            {
                get
                {
                    return this.alternateUserId;
                }
                set
                {
                    this.alternateUserId = value;
                }
            }
            public byte[] AlternateSecurityId
            {
                get
                {
                    return this.alternateSecurityId;
                }
                set
                {
                    this.alternateSecurityId = value;
                }
            }
            public int SubExpiry
            {
                get
                {
                    return this.subExpiry;
                }
                set
                {
                    this.subExpiry = value;
                }
            }
            public MQBase.structMQCHARV32 ObjectString
            {
                get
                {
                    return this.objectString;
                }
                set
                {
                    this.objectString = value;
                }
            }
            public MQBase.structMQCHARV32 SubName
            {
                get
                {
                    return this.subName;
                }
                set
                {
                    this.subName = value;
                }
            }
            public MQBase.structMQCHARV32 SubUserData
            {
                get
                {
                    return this.subUserData;
                }
                set
                {
                    this.subUserData = value;
                }
            }
            public byte[] SubCorrelId
            {
                get
                {
                    return this.subCorrelId;
                }
                set
                {
                    this.subCorrelId = value;
                }
            }
            public int PubPriority
            {
                get
                {
                    return this.pubPriority;
                }
                set
                {
                    this.pubPriority = value;
                }
            }
            public byte[] PubAccountingToken
            {
                get
                {
                    return this.pubAccountingToken;
                }
                set
                {
                    this.pubAccountingToken = value;
                }
            }
            public byte[] PubApplIdentityData
            {
                get
                {
                    return this.pubApplIdentityData;
                }
                set
                {
                    this.pubApplIdentityData = value;
                }
            }
            public MQBase.structMQCHARV32 SelectionString
            {
                get
                {
                    return this.selectionString;
                }
                set
                {
                    this.selectionString = value;
                }
            }
            public int SubLevel
            {
                get
                {
                    return this.subLevel;
                }
                set
                {
                    this.subLevel = value;
                }
            }
            public MQBase.structMQCHARV32 ResObjectString
            {
                get
                {
                    return this.resObjectString;
                }
                set
                {
                    this.resObjectString = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MQSRO
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            private byte[] strucId;
            private int version;
            private int options;
            private int numPubs;
            public byte[] StrucId
            {
                get
                {
                    return this.strucId;
                }
                set
                {
                    this.strucId = value;
                }
            }
            public int Version
            {
                get
                {
                    return this.version;
                }
                set
                {
                    this.version = value;
                }
            }
            public int Options
            {
                get
                {
                    return this.options;
                }
                set
                {
                    this.options = value;
                }
            }
            public int NumPubs
            {
                get
                {
                    return this.numPubs;
                }
                set
                {
                    this.numPubs = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MQSTS
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            private byte[] strucID;
            private int version;
            private int compCode;
            private int reason;
            private int putSuccessCount;
            private int putWarningCount;
            private int putFailureCount;
            private int objectType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            public byte[] objectName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            public byte[] objectQMgrName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            public byte[] resolvedObjectName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            public byte[] resolvedObjectQMgrName;
            public byte[] StrucID
            {
                get
                {
                    return this.strucID;
                }
                set
                {
                    this.strucID = value;
                }
            }
            public int Version
            {
                get
                {
                    return this.version;
                }
                set
                {
                    this.version = value;
                }
            }
            public int CompCode
            {
                get
                {
                    return this.compCode;
                }
                set
                {
                    this.compCode = value;
                }
            }
            public int Reason
            {
                get
                {
                    return this.reason;
                }
                set
                {
                    this.reason = value;
                }
            }
            public int PutSuccessCount
            {
                get
                {
                    return this.putSuccessCount;
                }
                set
                {
                    this.putSuccessCount = value;
                }
            }
            public int PutWarningCount
            {
                get
                {
                    return this.putWarningCount;
                }
                set
                {
                    this.putWarningCount = value;
                }
            }
            public int PutFailureCount
            {
                get
                {
                    return this.putFailureCount;
                }
                set
                {
                    this.putFailureCount = value;
                }
            }
            public int ObjectType
            {
                get
                {
                    return this.objectType;
                }
                set
                {
                    this.objectType = value;
                }
            }
            public byte[] ObjectName
            {
                get
                {
                    return this.objectName;
                }
                set
                {
                    this.objectName = value;
                }
            }
            public byte[] ObjectQMgrName
            {
                get
                {
                    return this.objectQMgrName;
                }
                set
                {
                    this.objectQMgrName = value;
                }
            }
            public byte[] ResolvedObjectName
            {
                get
                {
                    return this.resolvedObjectName;
                }
                set
                {
                    this.resolvedObjectName = value;
                }
            }
            public byte[] ResolvedObjectQMgrName
            {
                get
                {
                    return this.resolvedObjectQMgrName;
                }
                set
                {
                    this.resolvedObjectQMgrName = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MQTMC2
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            private byte[] strucId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            private byte[] version;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            private byte[] qName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            private byte[] processName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x40)]
            private byte[] triggerData;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            private byte[] applType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x100)]
            private byte[] applId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x80)]
            private byte[] envData;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x80)]
            private byte[] userData;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            private byte[] qMgrName;
            public byte[] StrucId
            {
                get
                {
                    return this.strucId;
                }
                set
                {
                    this.strucId = value;
                }
            }
            public byte[] Version
            {
                get
                {
                    return this.version;
                }
                set
                {
                    this.version = value;
                }
            }
            public byte[] QName
            {
                get
                {
                    return this.qName;
                }
                set
                {
                    this.qName = value;
                }
            }
            public byte[] ProcessName
            {
                get
                {
                    return this.processName;
                }
                set
                {
                    this.processName = value;
                }
            }
            public byte[] TriggerData
            {
                get
                {
                    return this.triggerData;
                }
                set
                {
                    this.triggerData = value;
                }
            }
            public byte[] ApplType
            {
                get
                {
                    return this.applType;
                }
                set
                {
                    this.applType = value;
                }
            }
            public byte[] ApplId
            {
                get
                {
                    return this.applId;
                }
                set
                {
                    this.applId = value;
                }
            }
            public byte[] EnvData
            {
                get
                {
                    return this.envData;
                }
                set
                {
                    this.envData = value;
                }
            }
            public byte[] UserData
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
            public byte[] QMgrName
            {
                get
                {
                    return this.qMgrName;
                }
                set
                {
                    this.qMgrName = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct structLPINOTIFYDETAILS
        {
            public int version;
            public int reason;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x18)]
            public byte[] connectionId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct structLPISDSUBPROPS
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            private byte[] destinationQMgr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            private byte[] destinationQName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x18)]
            private byte[] subCorrelId;
            private int psPropertyStyle;
            private int subScope;
            private int subType;
            public byte[] DestinationQMgr
            {
                get
                {
                    return this.destinationQMgr;
                }
                set
                {
                    this.destinationQMgr = value;
                }
            }
            public byte[] DestinationQName
            {
                get
                {
                    return this.destinationQName;
                }
                set
                {
                    this.destinationQName = value;
                }
            }
            public byte[] SubCorrelId
            {
                get
                {
                    return this.subCorrelId;
                }
                set
                {
                    this.subCorrelId = value;
                }
            }
            public int PsPropertyStyle
            {
                get
                {
                    return this.psPropertyStyle;
                }
                set
                {
                    this.psPropertyStyle = value;
                }
            }
            public int SubScope
            {
                get
                {
                    return this.subScope;
                }
                set
                {
                    this.subScope = value;
                }
            }
            public int SubType
            {
                get
                {
                    return this.subType;
                }
                set
                {
                    this.subType = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct structMQCBC
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            public byte[] strucId;
            public int version;
            public int callType;
            public int hobj;
            public IntPtr callbackArea;
            public IntPtr connectionArea;
            public int compCode;
            public int reason;
            public int state;
            public int dataLength;
            public int bufferLength;
            public int flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct structMQCBD
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            public byte[] strucId;
            public int version;
            public int callbackType;
            public int options;
            public IntPtr callbackArea;
            public IntPtr callbackFunction;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x80)]
            public byte[] callbackName;
            public int maxMsgLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct structMQCHARV
        {
            public IntPtr VSPtr;
            public int VSOffset;
            public int VSBufSize;
            public int VSLength;
            public int VSCCSID;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct structMQCHARV32
        {
            public int VSPtr;
            public int VSOffset;
            public int VSBufSize;
            public int VSLength;
            public int VSCCSID;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct structMQDLH
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            private byte[] strucId;
            private int version;
            private int reason;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            private byte[] destQName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x30)]
            private byte[] destQMgrName;
            private int encoding;
            private int codedCharSetId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
            private byte[] format;
            private int putApplType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x1c)]
            private byte[] putApplName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
            private byte[] putDate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
            private byte[] putTime;
            public byte[] StrucID
            {
                get
                {
                    return this.strucId;
                }
                set
                {
                    this.strucId = value;
                }
            }
            public int Version
            {
                get
                {
                    return this.version;
                }
                set
                {
                    this.version = value;
                }
            }
            public int Reason
            {
                get
                {
                    return this.reason;
                }
                set
                {
                    this.reason = value;
                }
            }
            public byte[] DestQName
            {
                get
                {
                    return this.destQName;
                }
                set
                {
                    this.destQName = value;
                }
            }
            public byte[] DestQMgrName
            {
                get
                {
                    return this.destQMgrName;
                }
                set
                {
                    this.destQMgrName = value;
                }
            }
            public int Encoding
            {
                get
                {
                    return this.encoding;
                }
                set
                {
                    this.encoding = value;
                }
            }
            public int CodedCharSetId
            {
                get
                {
                    return this.codedCharSetId;
                }
                set
                {
                    this.codedCharSetId = value;
                }
            }
            public byte[] Format
            {
                get
                {
                    return this.format;
                }
                set
                {
                    this.format = value;
                }
            }
            public int PutApplType
            {
                get
                {
                    return this.putApplType;
                }
                set
                {
                    this.putApplType = value;
                }
            }
            public byte[] PutApplName
            {
                get
                {
                    return this.putApplName;
                }
                set
                {
                    this.putApplName = value;
                }
            }
            public byte[] PutDate
            {
                get
                {
                    return this.putDate;
                }
                set
                {
                    this.putDate = value;
                }
            }
            public byte[] PutTime
            {
                get
                {
                    return this.putTime;
                }
                set
                {
                    this.putTime = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct structSPIOPENOPTIONS
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            public byte[] structId;
            public int version;
            public int options;
            public int lpiOpts;
            public int defPersistence;
            public int defPutResponseType;
            public int defReadAhead;
            public int propertyControl;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct xcsHPOOL
        {
            public uint subpoolId;
            public uint extentId;
            public uint offset;
        }
    }
}

