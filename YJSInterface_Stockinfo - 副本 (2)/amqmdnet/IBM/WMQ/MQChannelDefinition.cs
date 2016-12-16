namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Text;

    public sealed class MQChannelDefinition : MQBase
    {
        public string LongReceiveExit;
        public string LongSecurityExit;
        public string LongSendExit;
        private string[] messageExits;
        private string[] messageUserDatas;
        private MQBase.MQCD mqCD;
        private string[] receiveExits;
        private string[] receiveUserDatas;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal byte SEC_SEPARATOR;
        internal byte SEG_SEPARATOR;
        private string[] sendExits;
        private string[] sendUserDatas;
        internal byte[] sslPeerName;

        public MQChannelDefinition()
        {
            this.mqCD = new MQBase.MQCD();
            this.SEG_SEPARATOR = 1;
            this.SEC_SEPARATOR = 2;
            this.messageExits = new string[0];
            this.messageUserDatas = new string[0];
            this.sendExits = new string[0];
            this.sendUserDatas = new string[0];
            this.receiveExits = new string[0];
            this.receiveUserDatas = new string[0];
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.setDefaultDefinition();
        }

        public MQChannelDefinition(byte[] b, int Offset, int length)
        {
            int num3;
            this.mqCD = new MQBase.MQCD();
            this.SEG_SEPARATOR = 1;
            this.SEC_SEPARATOR = 2;
            this.messageExits = new string[0];
            this.messageUserDatas = new string[0];
            this.sendExits = new string[0];
            this.sendUserDatas = new string[0];
            this.receiveExits = new string[0];
            this.receiveUserDatas = new string[0];
            uint method = 0x310;
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { b, Offset, length });
            base.TrData(method, 0, "Channel Definition", Offset, length, b);
            int num2 = BitConverter.ToInt32(b, Offset + 20);
            if ((num2 <= 0) || (num2 > 30))
            {
                CommonServices.SetValidInserts();
                CommonServices.ArithInsert1 = (uint) num2;
                CommonServices.ArithInsert2 = (uint) Offset;
                base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009498, 0);
                throw new MQManagedClientException("Channel definition version invalid", 2, 0x8e5);
            }
            switch (num2)
            {
                case 1:
                    num3 = 0x3d8;
                    break;

                case 2:
                    num3 = 0x520;
                    break;

                case 3:
                    num3 = 0x5c8;
                    break;

                default:
                    num3 = BitConverter.ToInt32(b, Offset + 0x5d4);
                    break;
            }
            int cb = Marshal.SizeOf(typeof(MQBase.MQCD));
            if (((num3 <= 0) || (num3 > cb)) || (num3 > length))
            {
                CommonServices.SetValidInserts();
                CommonServices.ArithInsert1 = (uint) num3;
                CommonServices.ArithInsert2 = (uint) cb;
                base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 2, 0x20009498, 0);
                throw new MQManagedClientException(string.Concat(new object[] { "Channel definition length invalid (", num3, " vs ", cb, ")" }), 2, 0x8e5);
            }
            this.setDefaultDefinition();
            IntPtr destination = Marshal.AllocCoTaskMem(cb);
            Marshal.Copy(b, Offset, destination, num3);
            if (IntPtr.Size == 4)
            {
                base.TrText("Marshaling MQCD for a 32bit platform");
                this.mqCD = (MQBase.MQCD) Marshal.PtrToStructure(destination, typeof(MQBase.MQCD));
            }
            else
            {
                base.TrText("Marshaling MQCD for a 64bit platform");
                MQBase.MQCD32 fromCD = new MQBase.MQCD32();
                fromCD = (MQBase.MQCD32) Marshal.PtrToStructure(destination, typeof(MQBase.MQCD32));
                this.CopyCD(fromCD);
            }
            Marshal.FreeCoTaskMem(destination);
            int num4 = length - num3;
            if (num4 > 4)
            {
                BitConverter.ToInt32(b, Offset + (length - num4));
                num4 -= 4;
                if (num4 > 4)
                {
                    BitConverter.ToInt32(b, Offset + (length - num4));
                    num4 -= 4;
                    if (num4 > 4)
                    {
                        int num6 = BitConverter.ToInt32(b, Offset + (length - num4));
                        num4 -= 4;
                        if (num2 >= 5)
                        {
                            if (num4 <= 4)
                            {
                                return;
                            }
                            BitConverter.ToInt32(b, Offset + (length - num4));
                            num4 -= 4;
                            if (num4 <= 0x30)
                            {
                                return;
                            }
                            num4 -= 0x30;
                            if (num4 <= 4)
                            {
                                return;
                            }
                            BitConverter.ToInt32(b, Offset + (length - num4));
                            num4 -= 4;
                            if (num4 <= 0x40)
                            {
                                return;
                            }
                            num4 -= 0x40;
                        }
                        if (num4 >= num6)
                        {
                            this.ParseExitsLists(b, Offset + (length - num4), num6);
                            num4 -= num6;
                            if (num4 >= this.mqCD.SSLPeerNameLength)
                            {
                                this.sslPeerName = new byte[this.mqCD.SSLPeerNameLength];
                                Buffer.BlockCopy(b, length - num4, this.sslPeerName, 0, this.mqCD.SSLPeerNameLength);
                            }
                        }
                    }
                }
            }
        }

        public override void AddFieldsToFormatter(NmqiStructureFormatter fmt)
        {
            base.AddFieldsToFormatter(fmt);
            fmt.Add("channelName", this.ChannelName);
            fmt.Add("version", this.Version);
            fmt.Add("channelType", this.ChannelType);
            fmt.Add("transportType", this.TransportType);
            fmt.Add("desc", this.Description);
            fmt.Add("qMgrName", this.QMgrName);
            fmt.Add("connectionName", this.ConnectionName);
            fmt.Add("maxMsgLength", this.MaxMessageLength);
            fmt.Add("userIdentifier", this.UserIdentifier);
            fmt.Add("password", NmqiTools.TracePassword(base.GetString(this.Password)));
            fmt.Add("heartbeatInterval", this.HeartBeatInterval);
            fmt.Add("sslCipherSpec", this.SSLCipherSpec);
            fmt.Add("sslPeerName", this.SSLPeerName);
            fmt.Add("keepAliveInterval", this.KeepAliveInterval);
            fmt.Add("localAddress", this.LocalAddress);
            fmt.Add("hdrCompList", this.HdrCompList);
            fmt.Add("msgCompList", this.MsgCompList);
            fmt.Add("securityExit", this.SecurityExit);
            fmt.Add("securityUserData", this.SecurityUserData);
            fmt.Add("exitNameLength", this.ExitNameLength);
            fmt.Add("exitDataLength", this.ExitDataLength);
            fmt.Add("sendExitsDefined", this.SendExitsDefined);
            fmt.Add("sendExit", this.SendExit);
            fmt.Add("sendUserData", this.SendUserData);
            fmt.Add("sendExitPtr", this.SendExitPtr);
            fmt.Add("sendUserDataPtr", this.SendUserDataPtr);
            fmt.Add("receiveExitsDefined", this.ReceiveExitsDefined);
            fmt.Add("receiveExit", this.ReceiveExit);
            fmt.Add("receiveUserData", this.ReceiveUserData);
            fmt.Add("receiveExitPtr", this.ReceiveExitPtr);
            fmt.Add("receiveUserDataPtr", this.ReceiveUserDataPtr);
            fmt.Add("sharingConversations", this.SharingConversations);
            fmt.Add("clientChannelWeight", this.ClientChannelWeight);
            fmt.Add("connectionAffinity", this.ConnectionAffinity);
        }

        public MQChannelDefinition Clone()
        {
            uint method = 0x4c8;
            this.TrEntry(method);
            MQChannelDefinition result = new MQChannelDefinition();
            result.QMgrName = this.CloneByteArray(this.QMgrName);
            result.ChannelName = this.ChannelName;
            result.Version = this.Version;
            result.ChannelType = this.ChannelType;
            result.TransportType = this.TransportType;
            result.ConnectionName = this.ConnectionName;
            result.SecurityExit = this.SecurityExit;
            result.SecurityUserData = this.SecurityUserData;
            result.MaxMessageLength = this.MaxMessageLength;
            result.HeartBeatInterval = this.HeartBeatInterval;
            result.ExitNameLength = this.ExitNameLength;
            result.ExitDataLength = this.ExitDataLength;
            result.SendExitsDefined = this.SendExitsDefined;
            result.ReceiveExitsDefined = this.ReceiveExitsDefined;
            result.SendExitPtr = this.SendExitPtr;
            result.SendUserDataPtr = this.SendUserDataPtr;
            result.ReceiveExitPtr = this.ReceiveExitPtr;
            result.ReceiveUserDataPtr = this.ReceiveUserDataPtr;
            result.SSLCipherSpec = this.SSLCipherSpec;
            result.SSLPeerName = this.SSLPeerName;
            result.LocalAddress = this.LocalAddress;
            result.HdrCompList = (ArrayList) this.HdrCompList.Clone();
            result.MsgCompList = (ArrayList) this.MsgCompList.Clone();
            result.XmitQName = this.CloneByteArray(this.XmitQName);
            result.MCAName = this.CloneByteArray(this.MCAName);
            result.ModeName = this.CloneByteArray(this.ModeName);
            result.TPName = this.CloneByteArray(this.TPName);
            result.BatchSize = this.BatchSize;
            result.DiscInterval = this.DiscInterval;
            result.ShortRetryCount = this.ShortRetryCount;
            result.ShortRetryInterval = this.ShortRetryInterval;
            result.LongRetryCount = this.LongRetryCount;
            result.LongRetryInterval = this.LongRetryInterval;
            result.SendExit = this.SendExit;
            result.ReceiveExit = this.ReceiveExit;
            result.SeqNumberWrap = this.SeqNumberWrap;
            result.PutAuthority = this.PutAuthority;
            result.DataConversion = this.DataConversion;
            result.SendUserData = this.SendUserData;
            result.ReceiveUserData = this.ReceiveUserData;
            result.MCAUserIdentifier = this.CloneByteArray(this.MCAUserIdentifier);
            result.MCAType = this.MCAType;
            result.RemoteUserIdentifier = this.CloneByteArray(this.RemoteUserIdentifier);
            result.RemotePassword = this.CloneByteArray(this.RemotePassword);
            result.MsgRetryExit = this.CloneByteArray(this.MsgRetryExit);
            result.MsgRetryUserData = this.CloneByteArray(this.MsgRetryUserData);
            result.MsgRetryCount = this.MsgRetryCount;
            result.MsgRetryInterval = this.MsgRetryInterval;
            result.BatchInterval = this.BatchInterval;
            result.NonPersistentMsgSpeed = this.NonPersistentMsgSpeed;
            result.StrucLength = this.StrucLength;
            result.NetworkPriority = this.NetworkPriority;
            result.MCASecurityID = this.CloneByteArray(this.MCASecurityID);
            result.RemoteSecurityID = this.CloneByteArray(this.RemoteSecurityID);
            result.SSLClientAuth = this.SSLClientAuth;
            result.BatchHeartbeat = this.BatchHeartbeat;
            result.CLWLChannelRank = this.CLWLChannelRank;
            result.CLWLChannelPriority = this.CLWLChannelPriority;
            result.CLWLChannelWeight = this.CLWLChannelWeight;
            result.ChannelMonitoring = this.ChannelMonitoring;
            result.ChannelStatistics = this.ChannelStatistics;
            result.SharingConversations = this.SharingConversations;
            result.PropertyControl = this.PropertyControl;
            result.MaxInstances = this.MaxInstances;
            result.MaxInstancesPerClient = this.MaxInstancesPerClient;
            result.ClientChannelWeight = this.ClientChannelWeight;
            result.ConnectionAffinity = this.ConnectionAffinity;
            base.TrExit(method, result);
            return result;
        }

        private byte[] CloneByteArray(byte[] toClone)
        {
            if (toClone != null)
            {
                byte[] dst = new byte[toClone.Length];
                Buffer.BlockCopy(toClone, 0, dst, 0, toClone.Length);
                return dst;
            }
            return null;
        }

        internal bool Compare(byte[] arr1, byte[] arr2)
        {
            uint method = 0x1a;
            this.TrEntry(method, new object[] { arr1, arr2 });
            bool result = true;
            if ((arr1 == null) && (arr2 == null))
            {
                result = true;
            }
            else if ((arr1 == null) && (arr2 != null))
            {
                result = false;
            }
            else if ((arr1 != null) && (arr2 == null))
            {
                result = false;
            }
            else if (arr1.Length != arr2.Length)
            {
                result = false;
            }
            else
            {
                for (int i = 0; i < arr1.Length; i++)
                {
                    if (arr1[i] == arr2[i])
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                        break;
                    }
                }
            }
            base.TrExit(method, result);
            return result;
        }

        private bool CompareIntArr(int[] arr1, int[] arr2)
        {
            uint method = 0x1b;
            this.TrEntry(method, new object[] { arr1, arr2 });
            bool result = true;
            if ((arr1 == null) && (arr2 == null))
            {
                result = true;
            }
            else if ((arr1 == null) && (arr2 != null))
            {
                result = false;
            }
            else if ((arr1 != null) && (arr2 == null))
            {
                result = false;
            }
            else if (arr1.Length != arr2.Length)
            {
                result = false;
            }
            else
            {
                for (int i = 0; i < arr1.Length; i++)
                {
                    if (!arr1[i].Equals(arr2[i]))
                    {
                        result = false;
                        break;
                    }
                }
            }
            base.TrExit(method, result);
            return result;
        }

        private void CopyCD(MQBase.MQCD32 fromCD)
        {
            base.TrText("CopyCD from a 32bit versioned definition to a 64bit version");
            this.mqCD.ChannelName = fromCD.ChannelName;
            this.mqCD.Version = fromCD.Version;
            this.mqCD.ChannelType = fromCD.ChannelType;
            this.mqCD.TransportType = fromCD.TransportType;
            this.mqCD.Desc = fromCD.Desc;
            this.mqCD.QMgrName = fromCD.QMgrName;
            this.mqCD.XmitQName = fromCD.xmitQName;
            this.mqCD.MCAName = fromCD.MCAName;
            this.mqCD.ModeName = fromCD.ModeName;
            this.mqCD.TpName = fromCD.TpName;
            this.mqCD.BatchSize = fromCD.BatchSize;
            this.mqCD.DiscInterval = fromCD.DiscInterval;
            this.mqCD.ShortRetryCount = fromCD.ShortRetryCount;
            this.mqCD.ShortRetryInterval = fromCD.ShortRetryInterval;
            this.mqCD.LongRetryCount = fromCD.LongRetryCount;
            this.mqCD.LongRetryInterval = fromCD.LongRetryInterval;
            this.mqCD.SecurityExit = fromCD.SecurityExit;
            this.mqCD.MsgExit = fromCD.MsgExit;
            this.mqCD.SendExit = fromCD.SendExit;
            this.mqCD.ReceiveExit = fromCD.ReceiveExit;
            this.mqCD.SeqNumberWrap = fromCD.SeqNumberWrap;
            this.mqCD.MaxMsgLength = fromCD.MaxMsgLength;
            this.mqCD.PutAuthority = fromCD.PutAuthority;
            this.mqCD.DataConversion = fromCD.DataConversion;
            this.mqCD.SecurityUserData = fromCD.SecurityUserData;
            this.mqCD.MsgUserData = fromCD.MsgUserData;
            this.mqCD.SendUserData = fromCD.SendUserData;
            this.mqCD.ReceiveUserData = fromCD.ReceiveUserData;
            this.mqCD.UserIdentifier = fromCD.UserIdentifier;
            this.mqCD.Password = fromCD.Password;
            this.mqCD.MCAUserIdentifier = fromCD.MCAUserIdentifier;
            this.mqCD.McaType = fromCD.McaType;
            this.mqCD.ConnectionName = fromCD.ConnectionName;
            this.mqCD.RemoteUserIdentifier = fromCD.RemoteUserIdentifier;
            this.mqCD.RemotePassword = fromCD.RemotePassword;
            this.mqCD.MsgRetryExit = fromCD.MsgRetryExit;
            this.mqCD.MsgRetryUserData = fromCD.MsgRetryUserData;
            this.mqCD.MsgRetryCount = fromCD.MsgRetryCount;
            this.mqCD.MsgRetryInterval = fromCD.MsgRetryInterval;
            this.mqCD.HeartBeatInterval = fromCD.HeartBeatInterval;
            this.mqCD.BatchInterval = fromCD.BatchInterval;
            this.mqCD.NonPersistentMsgSpeed = fromCD.NonPersistentMsgSpeed;
            this.mqCD.StrucLength = fromCD.StrucLength;
            this.mqCD.ExitNameLength = fromCD.ExitNameLength;
            this.mqCD.ExitDataLength = fromCD.ExitDataLength;
            this.mqCD.MsgExitsDefined = fromCD.MsgExitsDefined;
            this.mqCD.SendExitsDefined = fromCD.SendExitsDefined;
            this.mqCD.ReceiveExitsDefined = fromCD.ReceiveExitsDefined;
            this.mqCD.MsgExitPtr = IntPtr.Zero;
            this.mqCD.MsgUserDataPtr = IntPtr.Zero;
            this.mqCD.SendExitPtr = IntPtr.Zero;
            this.mqCD.SendUserDataPtr = IntPtr.Zero;
            this.mqCD.ReceiveExitPtr = IntPtr.Zero;
            this.mqCD.ReceiveUserDataPtr = IntPtr.Zero;
            this.mqCD.ClusterPtr = IntPtr.Zero;
            this.mqCD.ClustersDefined = fromCD.ClustersDefined;
            this.mqCD.NetworkPriority = fromCD.NetworkPriority;
            this.mqCD.LongMCAUserIdLength = fromCD.LongMCAUserIdLength;
            this.mqCD.LongRemoteUserIdLength = fromCD.LongRemoteUserIdLength;
            this.mqCD.LongMCAUserIdPtr = IntPtr.Zero;
            this.mqCD.LongRemoteUserIdPtr = IntPtr.Zero;
            this.mqCD.MCASecurityId = fromCD.MCASecurityId;
            this.mqCD.RemoteSecurityId = fromCD.RemoteSecurityId;
            this.mqCD.SSLCipherSpec = fromCD.SSLCipherSpec;
            this.mqCD.SSLPeerNamePtr = IntPtr.Zero;
            this.mqCD.SSLPeerNameLength = fromCD.SSLPeerNameLength;
            this.mqCD.SSLClientAuth = fromCD.SSLClientAuth;
            this.mqCD.KeepAliveInterval = fromCD.KeepAliveInterval;
            this.mqCD.LocalAddress = fromCD.LocalAddress;
            this.mqCD.BatchHeartbeat = fromCD.BatchHeartbeat;
            this.mqCD.HdrCompList = fromCD.HdrCompList;
            this.mqCD.MsgCompList = fromCD.MsgCompList;
            this.mqCD.CLWLChannelRank = fromCD.CLWLChannelRank;
            this.mqCD.CLWLChannelPriority = fromCD.CLWLChannelPriority;
            this.mqCD.CLWLChannelWeight = fromCD.CLWLChannelWeight;
            this.mqCD.ChannelMonitoring = fromCD.ChannelMonitoring;
            this.mqCD.ChannelStatistics = fromCD.ChannelStatistics;
            this.mqCD.SharingConversations = fromCD.SharingConversations;
            this.mqCD.PropertyControl = fromCD.PropertyControl;
            this.mqCD.MaxInstances = fromCD.MaxInstances;
            this.mqCD.MaxInstancesPerClient = fromCD.MaxInstancesPerClient;
            this.mqCD.ClientChannelWeight = fromCD.ClientChannelWeight;
            this.mqCD.ConnectionAffinity = fromCD.ConnectionAffinity;
            this.mqCD.BatchDataLimit = fromCD.BatchDataLimit;
            this.mqCD.UseDLQ = fromCD.UseDLQ;
            this.mqCD.DefReconnect = fromCD.DefReconnect;
            this.mqCD.CertificateLabel = fromCD.CertificateLabel;
            base.TrText("CopyCD from a 32bit versioned definition to a 64bit version");
        }

        public bool Equals(MQChannelDefinition cd)
        {
            uint method = 0x19;
            this.TrEntry(method, new object[] { cd });
            bool result = true;
            if (cd == null)
            {
                result = false;
            }
            else
            {
                if (this.mqCD.Desc == null)
                {
                    result = result && (cd.mqCD.Desc == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.Desc, cd.mqCD.Desc);
                }
                if (this.mqCD.QMgrName == null)
                {
                    result = result && (cd.mqCD.QMgrName == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.QMgrName, cd.mqCD.QMgrName);
                }
                if (this.mqCD.ChannelName == null)
                {
                    result = result && (cd.mqCD.ChannelName == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.ChannelName, cd.mqCD.ChannelName);
                }
                if (this.mqCD.ConnectionName == null)
                {
                    result = result && (cd.mqCD.ConnectionName == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.ConnectionName, cd.mqCD.ConnectionName);
                }
                if (this.mqCD.SecurityExit == null)
                {
                    result = result && (cd.mqCD.SecurityExit == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.SecurityExit, cd.mqCD.SecurityExit);
                }
                if (this.mqCD.SecurityUserData == null)
                {
                    result = result && (cd.mqCD.SecurityUserData == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.SecurityUserData, cd.mqCD.SecurityUserData);
                }
                if (this.mqCD.UserIdentifier == null)
                {
                    result = result && (cd.mqCD.UserIdentifier == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.UserIdentifier, cd.mqCD.UserIdentifier);
                }
                if (this.mqCD.Password == null)
                {
                    result = result && (cd.mqCD.Password == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.Password, cd.mqCD.Password);
                }
                result = (((((((((((((result && (this.mqCD.Version == cd.mqCD.Version)) && (this.mqCD.ChannelType == cd.mqCD.ChannelType)) && (this.mqCD.TransportType == cd.mqCD.TransportType)) && (this.mqCD.MaxMsgLength == cd.mqCD.MaxMsgLength)) && (this.mqCD.HeartBeatInterval == cd.mqCD.HeartBeatInterval)) && (this.mqCD.KeepAliveInterval == cd.mqCD.KeepAliveInterval)) && (this.mqCD.ExitNameLength == cd.mqCD.ExitNameLength)) && (this.mqCD.ExitDataLength == cd.mqCD.ExitDataLength)) && (this.mqCD.SendExitsDefined == cd.mqCD.SendExitsDefined)) && (this.mqCD.ReceiveExitsDefined == cd.mqCD.ReceiveExitsDefined)) && this.mqCD.SendExitPtr.Equals(cd.mqCD.SendExitPtr)) && this.mqCD.SendUserDataPtr.Equals(cd.mqCD.SendUserDataPtr)) && this.mqCD.ReceiveExitPtr.Equals(cd.mqCD.ReceiveExitPtr)) && this.mqCD.ReceiveUserDataPtr.Equals(cd.mqCD.ReceiveUserDataPtr);
                if (this.mqCD.SSLCipherSpec == null)
                {
                    result = result && (cd.mqCD.SSLCipherSpec == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.SSLCipherSpec, cd.mqCD.SSLCipherSpec);
                }
                if (this.sslPeerName == null)
                {
                    result = result && (cd.sslPeerName == null);
                }
                else
                {
                    result = result && this.Compare(this.sslPeerName, cd.sslPeerName);
                }
                if (this.mqCD.LocalAddress == null)
                {
                    result = result && (cd.mqCD.LocalAddress == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.LocalAddress, cd.mqCD.LocalAddress);
                }
                if (this.mqCD.XmitQName == null)
                {
                    result = result && (cd.mqCD.XmitQName == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.XmitQName, cd.mqCD.XmitQName);
                }
                if (this.mqCD.ShortConnectionName == null)
                {
                    result = result && (cd.mqCD.ShortConnectionName == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.ShortConnectionName, cd.mqCD.ShortConnectionName);
                }
                if (this.mqCD.MCAName == null)
                {
                    result = result && (cd.mqCD.MCAName == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.MCAName, cd.mqCD.MCAName);
                }
                if (this.mqCD.ModeName == null)
                {
                    result = result && (cd.mqCD.ModeName == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.ModeName, cd.mqCD.ModeName);
                }
                if (this.mqCD.TpName == null)
                {
                    result = result && (cd.mqCD.TpName == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.TpName, cd.mqCD.TpName);
                }
                if (this.mqCD.MsgExit == null)
                {
                    result = result && (cd.mqCD.MsgExit == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.MsgExit, cd.mqCD.MsgExit);
                }
                if (this.mqCD.SendExit == null)
                {
                    result = result && (cd.mqCD.SendExit == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.SendExit, cd.mqCD.SendExit);
                }
                if (this.mqCD.ReceiveExit == null)
                {
                    result = result && (cd.mqCD.ReceiveExit == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.ReceiveExit, cd.mqCD.ReceiveExit);
                }
                if (this.mqCD.MsgUserData == null)
                {
                    result = result && (cd.mqCD.MsgUserData == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.MsgUserData, cd.mqCD.MsgUserData);
                }
                if (this.mqCD.SendUserData == null)
                {
                    result = result && (cd.mqCD.SendUserData == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.SendUserData, cd.mqCD.SendUserData);
                }
                if (this.mqCD.MCAUserIdentifier == null)
                {
                    result = result && (cd.mqCD.MCAUserIdentifier == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.MCAUserIdentifier, cd.mqCD.MCAUserIdentifier);
                }
                if (this.mqCD.RemoteUserIdentifier == null)
                {
                    result = result && (cd.mqCD.RemoteUserIdentifier == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.RemoteUserIdentifier, cd.mqCD.RemoteUserIdentifier);
                }
                if (this.mqCD.RemotePassword == null)
                {
                    result = result && (cd.mqCD.RemotePassword == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.RemotePassword, cd.mqCD.RemotePassword);
                }
                if (this.mqCD.MsgRetryExit == null)
                {
                    result = result && (cd.mqCD.MsgRetryExit == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.MsgRetryExit, cd.mqCD.MsgRetryExit);
                }
                if (this.mqCD.MsgRetryUserData == null)
                {
                    result = result && (cd.mqCD.MsgRetryUserData == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.MsgRetryUserData, cd.mqCD.MsgRetryUserData);
                }
                if (this.mqCD.MCASecurityId == null)
                {
                    result = result && (cd.mqCD.MCASecurityId == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.MCASecurityId, cd.mqCD.MCASecurityId);
                }
                if (this.mqCD.RemoteSecurityId == null)
                {
                    result = result && (cd.mqCD.RemoteSecurityId == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.RemoteSecurityId, cd.mqCD.RemoteSecurityId);
                }
                if (this.mqCD.HdrCompList == null)
                {
                    result = result && (cd.mqCD.HdrCompList == null);
                }
                else
                {
                    result = result && this.CompareIntArr(this.mqCD.HdrCompList, cd.mqCD.HdrCompList);
                }
                if (this.mqCD.MsgCompList == null)
                {
                    result = result && (cd.mqCD.MsgCompList == null);
                }
                else
                {
                    result = result && this.CompareIntArr(this.mqCD.MsgCompList, cd.mqCD.MsgCompList);
                }
                result = ((((((((((((((((((((((((((((((((result && (this.mqCD.BatchSize == cd.mqCD.BatchSize)) && (this.mqCD.DiscInterval == cd.mqCD.DiscInterval)) && (this.mqCD.ShortRetryCount == cd.mqCD.ShortRetryCount)) && (this.mqCD.ShortRetryInterval == cd.mqCD.ShortRetryInterval)) && (this.mqCD.LongRetryCount == cd.mqCD.LongRetryCount)) && (this.mqCD.LongRetryInterval == cd.mqCD.LongRetryInterval)) && (this.mqCD.SeqNumberWrap == cd.mqCD.SeqNumberWrap)) && (this.mqCD.PutAuthority == cd.mqCD.PutAuthority)) && (this.mqCD.DataConversion == cd.mqCD.DataConversion)) && (this.mqCD.McaType == cd.mqCD.McaType)) && (this.mqCD.MsgRetryCount == cd.mqCD.MsgRetryCount)) && (this.mqCD.MsgRetryInterval == cd.mqCD.MsgRetryInterval)) && (this.mqCD.BatchInterval == cd.mqCD.BatchInterval)) && (this.mqCD.NonPersistentMsgSpeed == cd.mqCD.NonPersistentMsgSpeed)) && (this.mqCD.StrucLength == cd.mqCD.StrucLength)) && (this.mqCD.MsgExitsDefined == cd.mqCD.MsgExitsDefined)) && (this.mqCD.ClustersDefined == cd.mqCD.ClustersDefined)) && (this.mqCD.NetworkPriority == cd.mqCD.NetworkPriority)) && (this.mqCD.LongMCAUserIdLength == cd.mqCD.LongMCAUserIdLength)) && (this.mqCD.LongRemoteUserIdLength == cd.mqCD.LongRemoteUserIdLength)) && (this.mqCD.SSLClientAuth == cd.mqCD.SSLClientAuth)) && (this.mqCD.BatchHeartbeat == cd.mqCD.BatchHeartbeat)) && (this.mqCD.CLWLChannelRank == cd.mqCD.CLWLChannelRank)) && (this.mqCD.CLWLChannelPriority == cd.mqCD.CLWLChannelPriority)) && (this.mqCD.CLWLChannelWeight == cd.mqCD.CLWLChannelWeight)) && (this.mqCD.ChannelMonitoring == cd.mqCD.ChannelMonitoring)) && (this.mqCD.ChannelStatistics == cd.mqCD.ChannelStatistics)) && (this.mqCD.SharingConversations == cd.mqCD.SharingConversations)) && this.mqCD.MsgExitPtr.Equals(cd.mqCD.MsgExitPtr)) && this.mqCD.MsgUserDataPtr.Equals(cd.mqCD.MsgUserDataPtr)) && this.mqCD.ClusterPtr.Equals(cd.mqCD.ClusterPtr)) && this.mqCD.LongMCAUserIdPtr.Equals(cd.mqCD.LongMCAUserIdPtr)) && this.mqCD.LongRemoteUserIdPtr.Equals(cd.mqCD.LongRemoteUserIdPtr);
                if (this.mqCD.CertificateLabel == null)
                {
                    result = result && (cd.mqCD.CertificateLabel == null);
                }
                else
                {
                    result = result && this.Compare(this.mqCD.CertificateLabel, cd.mqCD.CertificateLabel);
                }
            }
            base.TrExit(method, result);
            return result;
        }

        private int FindByte(byte[] b, int offset, int end, byte fbyte)
        {
            uint method = 0x16;
            this.TrEntry(method, new object[] { b, offset, end, fbyte });
            int result = -1;
            if (offset >= 0)
            {
                while (offset <= end)
                {
                    if (b[offset] == fbyte)
                    {
                        result = offset;
                        break;
                    }
                    offset++;
                }
            }
            base.TrExit(method, result);
            return result;
        }

        private string[] ParseExitsList(byte[] b, ref int offset, int end)
        {
            int num2;
            uint method = 0x17;
            this.TrEntry(method, new object[] { b, (int) offset, end });
            string[] strArray = null;
            try
            {
                num2 = this.FindByte(b, offset, end, this.SEG_SEPARATOR);
                if (num2 != -1)
                {
                    int num4 = offset;
                    int num5 = 0;
                    while (true)
                    {
                        num4 = this.FindByte(b, num4, num2, this.SEC_SEPARATOR);
                        if (num4 == -1)
                        {
                            break;
                        }
                        num4++;
                        num5++;
                    }
                    if (num5 != 0)
                    {
                        strArray = new string[num5];
                        int num3 = offset;
                        num5 = 0;
                        while (true)
                        {
                            num4 = this.FindByte(b, num3, num2, this.SEC_SEPARATOR);
                            if (num4 == -1)
                            {
                                goto Label_00C3;
                            }
                            strArray[num5++] = Encoding.ASCII.GetString(b, num3, num4 - num3);
                            num3 = num4 + 1;
                        }
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        Label_00C3:
            if (num2 == -1)
            {
                offset = -1;
                return strArray;
            }
            offset = num2 + 1;
            return strArray;
        }

        private void ParseExitsLists(byte[] b, int offset, int length)
        {
            uint method = 0x18;
            this.TrEntry(method, new object[] { b, offset, length });
            int end = (offset + length) - 1;
            try
            {
                this.SendExits = this.ParseExitsList(b, ref offset, end);
                this.SendUserDatas = this.ParseExitsList(b, ref offset, end);
                if (((this.SendExits == null) && (this.SendExit != null)) && (this.SendExit.Length != 0))
                {
                    this.SendExits = new string[] { this.SendExit };
                    if ((this.SendUserData != null) && (this.SendUserData.Length != 0))
                    {
                        this.SendUserDatas = new string[] { this.SendUserData };
                    }
                }
                if (this.SendExits == null)
                {
                    this.SendExits = new string[0];
                }
                if (this.SendUserDatas == null)
                {
                    this.SendUserDatas = new string[0];
                }
                this.ReceiveExits = this.ParseExitsList(b, ref offset, end);
                this.ReceiveUserDatas = this.ParseExitsList(b, ref offset, end);
                if (((this.ReceiveExits == null) && (this.ReceiveExit != null)) && (this.ReceiveExit.Length != 0))
                {
                    this.ReceiveExits = new string[] { this.ReceiveExit };
                    if ((this.ReceiveUserData != null) && (this.ReceiveUserData.Length != 0))
                    {
                        this.ReceiveUserDatas = new string[] { this.ReceiveUserData };
                    }
                }
                if (this.ReceiveExits == null)
                {
                    this.ReceiveExits = new string[0];
                }
                if (this.ReceiveUserDatas == null)
                {
                    this.ReceiveUserDatas = new string[0];
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void setDefaultDefinition()
        {
            uint method = 0x15;
            this.TrEntry(method);
            this.mqCD.ChannelName = new byte[20];
            this.mqCD.Version = 1;
            this.mqCD.ChannelType = 6;
            this.mqCD.TransportType = 2;
            this.mqCD.Desc = new byte[0x40];
            this.mqCD.QMgrName = new byte[0x30];
            this.mqCD.XmitQName = new byte[0x30];
            this.mqCD.MCAName = new byte[20];
            this.mqCD.ModeName = new byte[8];
            this.mqCD.TpName = new byte[0x40];
            this.mqCD.BatchSize = 50;
            this.mqCD.DiscInterval = 0x1770;
            this.mqCD.ShortRetryCount = 10;
            this.mqCD.ShortRetryInterval = 60;
            this.mqCD.LongRetryCount = 0x3b9ac9ff;
            this.mqCD.LongRetryInterval = 0x4b0;
            this.mqCD.SecurityExit = new byte[0x80];
            this.mqCD.MsgExit = new byte[0x80];
            this.mqCD.SendExit = new byte[0x80];
            this.mqCD.ReceiveExit = new byte[0x80];
            this.mqCD.SeqNumberWrap = 0x3b9ac9ff;
            this.mqCD.MaxMsgLength = 0x400000;
            this.mqCD.PutAuthority = 1;
            this.mqCD.DataConversion = 0;
            this.mqCD.SecurityUserData = new byte[0x20];
            this.mqCD.MsgUserData = new byte[0x20];
            this.mqCD.SendUserData = new byte[0x20];
            this.mqCD.ReceiveUserData = new byte[0x20];
            this.mqCD.UserIdentifier = new byte[12];
            this.mqCD.Password = new byte[12];
            this.mqCD.MCAUserIdentifier = new byte[12];
            this.mqCD.McaType = 1;
            this.mqCD.ConnectionName = new byte[0x108];
            this.mqCD.RemoteUserIdentifier = new byte[12];
            this.mqCD.RemotePassword = new byte[12];
            this.mqCD.MsgRetryExit = new byte[0x80];
            this.mqCD.MsgRetryUserData = new byte[0x20];
            this.mqCD.MsgRetryCount = 10;
            this.mqCD.MsgRetryInterval = 0x3e8;
            this.mqCD.HeartBeatInterval = 300;
            this.mqCD.BatchInterval = 0;
            this.mqCD.NonPersistentMsgSpeed = 2;
            this.mqCD.StrucLength = 0x794;
            this.mqCD.ExitNameLength = 0x80;
            this.mqCD.ExitDataLength = 0x20;
            this.mqCD.MsgExitsDefined = 0;
            this.mqCD.SendExitsDefined = 0;
            this.mqCD.ReceiveExitsDefined = 0;
            this.mqCD.MsgExitPtr = IntPtr.Zero;
            this.mqCD.MsgUserDataPtr = IntPtr.Zero;
            this.mqCD.SendExitPtr = IntPtr.Zero;
            this.mqCD.SendUserDataPtr = IntPtr.Zero;
            this.mqCD.ReceiveExitPtr = IntPtr.Zero;
            this.mqCD.ReceiveUserDataPtr = IntPtr.Zero;
            this.mqCD.ClusterPtr = IntPtr.Zero;
            this.mqCD.ClustersDefined = 0;
            this.mqCD.NetworkPriority = 0;
            this.mqCD.LongMCAUserIdLength = 0;
            this.mqCD.LongRemoteUserIdLength = 0;
            this.mqCD.LongMCAUserIdPtr = IntPtr.Zero;
            this.mqCD.LongRemoteUserIdPtr = IntPtr.Zero;
            this.mqCD.MCASecurityId = new byte[40];
            this.mqCD.RemoteSecurityId = new byte[40];
            this.mqCD.SSLCipherSpec = new byte[0x20];
            this.mqCD.SSLPeerNamePtr = IntPtr.Zero;
            this.mqCD.SSLPeerNameLength = 0;
            this.mqCD.SSLClientAuth = 0;
            this.mqCD.KeepAliveInterval = -1;
            this.mqCD.LocalAddress = new byte[0x30];
            this.mqCD.BatchHeartbeat = 0;
            this.mqCD.HdrCompList = new int[2];
            this.mqCD.MsgCompList = new int[0x10];
            this.mqCD.CLWLChannelRank = 0;
            this.mqCD.CLWLChannelPriority = 0;
            this.mqCD.CLWLChannelWeight = 50;
            this.mqCD.ChannelMonitoring = 0;
            this.mqCD.ChannelStatistics = 0;
            this.mqCD.SharingConversations = 10;
            this.mqCD.PropertyControl = 0;
            this.mqCD.MaxInstances = 0x3b9ac9ff;
            this.mqCD.MaxInstancesPerClient = 0x3b9ac9ff;
            this.mqCD.ClientChannelWeight = 0;
            this.mqCD.ConnectionAffinity = 1;
            this.mqCD.BatchDataLimit = 0;
            this.mqCD.UseDLQ = 2;
            this.mqCD.DefReconnect = 0;
            this.mqCD.CertificateLabel = new byte[0x40];
            base.TrExit(method);
        }

        public int BatchDataLimit
        {
            get
            {
                return this.mqCD.BatchDataLimit;
            }
            set
            {
                this.Version = (this.Version < 10) ? 10 : this.Version;
                this.mqCD.BatchDataLimit = value;
            }
        }

        public int BatchHeartbeat
        {
            get
            {
                return this.mqCD.BatchHeartbeat;
            }
            set
            {
                this.Version = (this.Version < 7) ? 7 : this.Version;
                this.mqCD.BatchHeartbeat = value;
            }
        }

        public int BatchInterval
        {
            get
            {
                return this.mqCD.BatchInterval;
            }
            set
            {
                this.Version = (this.Version < 4) ? 4 : this.Version;
                this.mqCD.BatchInterval = value;
            }
        }

        public int BatchSize
        {
            get
            {
                return this.mqCD.BatchSize;
            }
            set
            {
                this.mqCD.BatchSize = value;
            }
        }

        public byte[] CertificateLabel
        {
            get
            {
                return this.mqCD.CertificateLabel;
            }
            set
            {
                this.Version = (this.Version < 11) ? 11 : this.Version;
                this.mqCD.CertificateLabel = value;
            }
        }

        public int ChannelMonitoring
        {
            get
            {
                return this.mqCD.ChannelMonitoring;
            }
            set
            {
                this.Version = (this.Version < 8) ? 8 : this.Version;
                this.mqCD.ChannelMonitoring = value;
            }
        }

        public string ChannelName
        {
            get
            {
                return base.GetString(this.mqCD.ChannelName).TrimEnd(new char[0]);
            }
            set
            {
                byte[] channelName = this.mqCD.ChannelName;
                base.GetBytes(value, ref channelName);
                this.mqCD.ChannelName = channelName;
            }
        }

        public int ChannelStatistics
        {
            get
            {
                return this.mqCD.ChannelStatistics;
            }
            set
            {
                this.Version = (this.Version < 8) ? 8 : this.Version;
                this.mqCD.ChannelStatistics = value;
            }
        }

        public int ChannelType
        {
            get
            {
                return this.mqCD.ChannelType;
            }
            set
            {
                this.mqCD.ChannelType = value;
            }
        }

        public int ClientChannelWeight
        {
            get
            {
                return this.mqCD.ClientChannelWeight;
            }
            set
            {
                this.Version = (this.Version < 9) ? 9 : this.Version;
                this.mqCD.ClientChannelWeight = value;
            }
        }

        public IntPtr ClusterPtr
        {
            get
            {
                return this.mqCD.ClusterPtr;
            }
            set
            {
                this.Version = (this.Version < 5) ? 5 : this.Version;
                this.mqCD.ClusterPtr = value;
            }
        }

        public int ClustersDefined
        {
            get
            {
                return this.mqCD.ClustersDefined;
            }
        }

        public int CLWLChannelPriority
        {
            get
            {
                return this.mqCD.CLWLChannelPriority;
            }
            set
            {
                this.Version = (this.Version < 8) ? 8 : this.Version;
                this.mqCD.CLWLChannelPriority = value;
            }
        }

        public int CLWLChannelRank
        {
            get
            {
                return this.mqCD.CLWLChannelRank;
            }
            set
            {
                this.Version = (this.Version < 8) ? 8 : this.Version;
                this.mqCD.CLWLChannelRank = value;
            }
        }

        public int CLWLChannelWeight
        {
            get
            {
                return this.mqCD.CLWLChannelWeight;
            }
            set
            {
                this.Version = (this.Version < 8) ? 8 : this.Version;
                this.mqCD.CLWLChannelWeight = value;
            }
        }

        public int ConnectionAffinity
        {
            get
            {
                return this.mqCD.ConnectionAffinity;
            }
            set
            {
                this.Version = (this.Version < 9) ? 9 : this.Version;
                this.mqCD.ConnectionAffinity = value;
            }
        }

        public string ConnectionName
        {
            get
            {
                return base.GetString(this.mqCD.ConnectionName).TrimEnd(new char[0]);
            }
            set
            {
                this.Version = (this.Version < 2) ? 2 : this.Version;
                byte[] connectionName = this.mqCD.ConnectionName;
                base.GetBytes(value, ref connectionName);
                this.mqCD.ConnectionName = connectionName;
            }
        }

        public int DataConversion
        {
            get
            {
                return this.mqCD.DataConversion;
            }
            set
            {
                this.mqCD.DataConversion = value;
            }
        }

        public int DefaultReconnect
        {
            get
            {
                return this.mqCD.DefReconnect;
            }
            set
            {
                this.Version = (this.Version < 10) ? 10 : this.Version;
                this.mqCD.DefReconnect = value;
            }
        }

        public byte[] Description
        {
            get
            {
                return this.mqCD.Desc;
            }
            set
            {
                this.mqCD.Desc = value;
            }
        }

        public int DiscInterval
        {
            get
            {
                return this.mqCD.DiscInterval;
            }
            set
            {
                this.mqCD.DiscInterval = value;
            }
        }

        public int ExitDataLength
        {
            get
            {
                return this.mqCD.ExitDataLength;
            }
            set
            {
                this.Version = (this.Version < 4) ? 4 : this.Version;
                this.mqCD.ExitDataLength = value;
            }
        }

        public int ExitNameLength
        {
            get
            {
                return this.mqCD.ExitNameLength;
            }
            set
            {
                this.Version = (this.Version < 4) ? 4 : this.Version;
                this.mqCD.ExitNameLength = value;
            }
        }

        public ArrayList HdrCompList
        {
            get
            {
                ArrayList list = new ArrayList();
                for (int i = 0; i < this.mqCD.HdrCompList.Length; i++)
                {
                    list.Add(this.mqCD.HdrCompList[i]);
                }
                return list;
            }
            set
            {
                this.Version = (this.Version < 8) ? 8 : this.Version;
                for (int i = 0; i < 2; i++)
                {
                    if ((i < value.Count) && (((int) value[i]) == 8))
                    {
                        this.mqCD.HdrCompList[i] = (int) value[i];
                        base.TrText("Added header compression: " + this.mqCD.HdrCompList[i].ToString());
                    }
                    else
                    {
                        this.mqCD.HdrCompList[i] = 0;
                        base.TrText("Added header compression MQCOMPRESS_NONE");
                    }
                }
            }
        }

        public int HeartBeatInterval
        {
            get
            {
                return this.mqCD.HeartBeatInterval;
            }
            set
            {
                this.Version = (this.Version < 4) ? 4 : this.Version;
                this.mqCD.HeartBeatInterval = value;
            }
        }

        public int KeepAliveInterval
        {
            get
            {
                return this.mqCD.KeepAliveInterval;
            }
            set
            {
                this.Version = (this.Version < 7) ? 7 : this.Version;
                this.mqCD.KeepAliveInterval = value;
            }
        }

        public string LocalAddress
        {
            get
            {
                return base.GetString(this.mqCD.LocalAddress).TrimEnd(new char[0]);
            }
            set
            {
                this.Version = (this.Version < 7) ? 7 : this.Version;
                byte[] localAddress = this.mqCD.LocalAddress;
                base.GetBytes(value, ref localAddress);
                this.mqCD.LocalAddress = localAddress;
            }
        }

        public int LongMCAUserIdLength
        {
            get
            {
                return this.mqCD.LongMCAUserIdLength;
            }
            set
            {
                this.Version = (this.Version < 6) ? 6 : this.Version;
                this.mqCD.LongMCAUserIdLength = value;
            }
        }

        public IntPtr LongMCAUserIdPtr
        {
            get
            {
                return this.mqCD.LongMCAUserIdPtr;
            }
            set
            {
                this.Version = (this.Version < 6) ? 6 : this.Version;
                this.mqCD.LongMCAUserIdPtr = value;
            }
        }

        public int LongRemoteUserIdLength
        {
            get
            {
                return this.mqCD.LongRemoteUserIdLength;
            }
            set
            {
                this.Version = (this.Version < 6) ? 6 : this.Version;
                this.mqCD.LongRemoteUserIdLength = value;
            }
        }

        public IntPtr LongRemoteUserIdPtr
        {
            get
            {
                return this.mqCD.LongRemoteUserIdPtr;
            }
            set
            {
                this.Version = (this.Version < 6) ? 6 : this.Version;
                this.mqCD.LongRemoteUserIdPtr = value;
            }
        }

        public int LongRetryCount
        {
            get
            {
                return this.mqCD.LongRetryCount;
            }
            set
            {
                this.mqCD.LongRetryCount = value;
            }
        }

        public int LongRetryInterval
        {
            get
            {
                return this.mqCD.LongRetryInterval;
            }
            set
            {
                this.mqCD.LongRetryInterval = value;
            }
        }

        public int MaxInstances
        {
            get
            {
                return this.mqCD.MaxInstances;
            }
            set
            {
                this.Version = (this.Version < 9) ? 9 : this.Version;
                this.mqCD.MaxInstances = value;
            }
        }

        public int MaxInstancesPerClient
        {
            get
            {
                return this.mqCD.MaxInstancesPerClient;
            }
            set
            {
                this.Version = (this.Version < 9) ? 9 : this.Version;
                this.mqCD.MaxInstancesPerClient = value;
            }
        }

        public int MaxMessageLength
        {
            get
            {
                return this.mqCD.MaxMsgLength;
            }
            set
            {
                this.mqCD.MaxMsgLength = value;
            }
        }

        public byte[] MCAName
        {
            get
            {
                return this.mqCD.MCAName;
            }
            set
            {
                this.mqCD.MCAName = value;
            }
        }

        public byte[] MCASecurityID
        {
            get
            {
                return this.mqCD.MCASecurityId;
            }
            set
            {
                this.Version = (this.Version < 6) ? 6 : this.Version;
                this.mqCD.MCASecurityId = value;
            }
        }

        public int MCAType
        {
            get
            {
                return this.mqCD.McaType;
            }
            set
            {
                this.Version = (this.Version < 2) ? 2 : this.Version;
                this.mqCD.McaType = value;
            }
        }

        public byte[] MCAUserIdentifier
        {
            get
            {
                return this.mqCD.MCAUserIdentifier;
            }
            set
            {
                this.Version = (this.Version < 2) ? 2 : this.Version;
                this.mqCD.MCAUserIdentifier = value;
            }
        }

        public byte[] ModeName
        {
            get
            {
                return this.mqCD.ModeName;
            }
            set
            {
                this.mqCD.ModeName = value;
            }
        }

        public ArrayList MsgCompList
        {
            get
            {
                ArrayList list = new ArrayList();
                for (int i = 0; i < this.mqCD.MsgCompList.Length; i++)
                {
                    list.Add(this.mqCD.MsgCompList[i]);
                }
                return list;
            }
            set
            {
                this.Version = (this.Version < 8) ? 8 : this.Version;
                for (int i = 0; i < 0x10; i++)
                {
                    if ((i < value.Count) && (((((int) value[i]) == 1) || (((int) value[i]) == 2)) || (((int) value[i]) == 4)))
                    {
                        this.mqCD.MsgCompList[i] = (int) value[i];
                        base.TrText("Added message compression: " + this.mqCD.MsgCompList[i].ToString());
                    }
                    else
                    {
                        this.mqCD.MsgCompList[i] = 0;
                        base.TrText("Added message compression MQCOMPRESS_NONE");
                    }
                }
            }
        }

        public int MsgRetryCount
        {
            get
            {
                return this.mqCD.MsgRetryCount;
            }
            set
            {
                this.Version = (this.Version < 3) ? 3 : this.Version;
                this.mqCD.MsgRetryCount = value;
            }
        }

        public byte[] MsgRetryExit
        {
            get
            {
                return this.mqCD.MsgRetryExit;
            }
            set
            {
                this.Version = (this.Version < 3) ? 3 : this.Version;
                this.mqCD.MsgRetryExit = value;
            }
        }

        public int MsgRetryInterval
        {
            get
            {
                return this.mqCD.MsgRetryInterval;
            }
            set
            {
                this.Version = (this.Version < 3) ? 3 : this.Version;
                this.mqCD.MsgRetryInterval = value;
            }
        }

        public byte[] MsgRetryUserData
        {
            get
            {
                return this.mqCD.MsgRetryUserData;
            }
            set
            {
                this.Version = (this.Version < 3) ? 3 : this.Version;
                this.mqCD.MsgRetryUserData = value;
            }
        }

        public int NetworkPriority
        {
            get
            {
                return this.mqCD.NetworkPriority;
            }
            set
            {
                this.Version = (this.Version < 5) ? 5 : this.Version;
                this.mqCD.NetworkPriority = value;
            }
        }

        public int NonPersistentMsgSpeed
        {
            get
            {
                return this.mqCD.NonPersistentMsgSpeed;
            }
            set
            {
                this.Version = (this.Version < 4) ? 4 : this.Version;
                this.mqCD.NonPersistentMsgSpeed = value;
            }
        }

        public byte[] Password
        {
            get
            {
                return this.mqCD.Password;
            }
            set
            {
                this.Version = (this.Version < 2) ? 2 : this.Version;
                this.mqCD.Password = value;
            }
        }

        public int PropertyControl
        {
            get
            {
                return this.mqCD.PropertyControl;
            }
            set
            {
                this.Version = (this.Version < 9) ? 9 : this.Version;
                this.mqCD.PropertyControl = value;
            }
        }

        public int PutAuthority
        {
            get
            {
                return this.mqCD.PutAuthority;
            }
            set
            {
                this.mqCD.PutAuthority = value;
            }
        }

        public byte[] QMgrName
        {
            get
            {
                return this.mqCD.QMgrName;
            }
            set
            {
                this.mqCD.QMgrName = value;
            }
        }

        public string ReceiveExit
        {
            get
            {
                if (this.LongReceiveExit != null)
                {
                    return this.LongReceiveExit;
                }
                return base.GetString(this.mqCD.ReceiveExit).TrimEnd(new char[0]);
            }
            set
            {
                if (value.Length > 0x80)
                {
                    this.LongReceiveExit = value;
                }
                if (value != "")
                {
                    this.ReceiveExits = new string[] { value };
                }
                byte[] receiveExit = this.mqCD.ReceiveExit;
                base.GetBytes(value, ref receiveExit);
                this.mqCD.ReceiveExit = receiveExit;
            }
        }

        public IntPtr ReceiveExitPtr
        {
            get
            {
                return this.mqCD.ReceiveExitPtr;
            }
            set
            {
                this.Version = (this.Version < 4) ? 4 : this.Version;
                this.mqCD.ReceiveExitPtr = value;
            }
        }

        public string[] ReceiveExits
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

        public int ReceiveExitsDefined
        {
            get
            {
                return this.mqCD.ReceiveExitsDefined;
            }
            set
            {
                this.mqCD.ReceiveExitsDefined = value;
            }
        }

        internal string ReceiveUserData
        {
            get
            {
                return base.GetString(this.mqCD.ReceiveUserData).TrimEnd(new char[0]);
            }
            set
            {
                this.ReceiveUserDatas = new string[] { value };
                byte[] receiveUserData = this.mqCD.ReceiveUserData;
                base.GetBytes(value, ref receiveUserData);
                this.mqCD.ReceiveUserData = receiveUserData;
            }
        }

        public IntPtr ReceiveUserDataPtr
        {
            get
            {
                return this.mqCD.ReceiveUserDataPtr;
            }
            set
            {
                this.Version = (this.Version < 4) ? 4 : this.Version;
                this.mqCD.ReceiveUserDataPtr = value;
            }
        }

        public string[] ReceiveUserDatas
        {
            get
            {
                return this.receiveUserDatas;
            }
            set
            {
                this.receiveUserDatas = value;
            }
        }

        public byte[] RemotePassword
        {
            get
            {
                return this.mqCD.RemotePassword;
            }
            set
            {
                this.Version = (this.Version < 2) ? 2 : this.Version;
                this.mqCD.RemotePassword = value;
            }
        }

        public byte[] RemoteSecurityID
        {
            get
            {
                return this.mqCD.RemoteSecurityId;
            }
            set
            {
                this.Version = (this.Version < 6) ? 6 : this.Version;
                this.mqCD.RemoteSecurityId = value;
            }
        }

        public byte[] RemoteUserIdentifier
        {
            get
            {
                return this.mqCD.RemoteUserIdentifier;
            }
            set
            {
                this.Version = (this.Version < 2) ? 2 : this.Version;
                this.mqCD.RemoteUserIdentifier = value;
            }
        }

        public string SecurityExit
        {
            get
            {
                if (this.LongSecurityExit != null)
                {
                    return this.LongSecurityExit;
                }
                return base.GetString(this.mqCD.SecurityExit).TrimEnd(new char[0]);
            }
            set
            {
                if (value.Length > 0x80)
                {
                    this.LongSecurityExit = value;
                }
                byte[] securityExit = this.mqCD.SecurityExit;
                base.GetBytes(value, ref securityExit);
                this.mqCD.SecurityExit = securityExit;
            }
        }

        public string SecurityUserData
        {
            get
            {
                return base.GetString(this.mqCD.SecurityUserData).TrimEnd(new char[0]);
            }
            set
            {
                byte[] securityUserData = this.mqCD.SecurityUserData;
                base.GetBytes(value, ref securityUserData);
                this.mqCD.SecurityUserData = securityUserData;
            }
        }

        public string SendExit
        {
            get
            {
                if (this.LongSendExit != null)
                {
                    return this.LongSendExit;
                }
                return base.GetString(this.mqCD.SendExit).TrimEnd(new char[0]);
            }
            set
            {
                if (value.Length > 0x80)
                {
                    this.LongSendExit = value;
                }
                if (value != "")
                {
                    this.SendExits = new string[] { value };
                }
                byte[] sendExit = this.mqCD.SendExit;
                base.GetBytes(value, ref sendExit);
                this.mqCD.SendExit = sendExit;
            }
        }

        public IntPtr SendExitPtr
        {
            get
            {
                return this.mqCD.SendExitPtr;
            }
            set
            {
                this.Version = (this.Version < 4) ? 4 : this.Version;
                this.mqCD.SendExitPtr = value;
            }
        }

        public string[] SendExits
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

        public int SendExitsDefined
        {
            get
            {
                return this.mqCD.SendExitsDefined;
            }
            set
            {
                this.mqCD.SendExitsDefined = value;
            }
        }

        internal string SendUserData
        {
            get
            {
                return base.GetString(this.mqCD.SendUserData).TrimEnd(new char[0]);
            }
            set
            {
                this.SendUserDatas = new string[] { value };
                byte[] sendUserData = this.mqCD.SendUserData;
                base.GetBytes(value, ref sendUserData);
                this.mqCD.SendUserData = sendUserData;
            }
        }

        public IntPtr SendUserDataPtr
        {
            get
            {
                return this.mqCD.SendUserDataPtr;
            }
            set
            {
                this.Version = (this.Version < 4) ? 4 : this.Version;
                this.mqCD.SendUserDataPtr = value;
            }
        }

        public string[] SendUserDatas
        {
            get
            {
                return this.sendUserDatas;
            }
            set
            {
                this.sendUserDatas = value;
            }
        }

        public int SeqNumberWrap
        {
            get
            {
                return this.mqCD.SeqNumberWrap;
            }
            set
            {
                this.mqCD.SeqNumberWrap = value;
            }
        }

        public int SharingConversations
        {
            get
            {
                return this.mqCD.SharingConversations;
            }
            set
            {
                this.mqCD.SharingConversations = value;
            }
        }

        public int ShortRetryCount
        {
            get
            {
                return this.mqCD.ShortRetryCount;
            }
            set
            {
                this.mqCD.ShortRetryCount = value;
            }
        }

        public int ShortRetryInterval
        {
            get
            {
                return this.mqCD.ShortRetryInterval;
            }
            set
            {
                this.mqCD.ShortRetryInterval = value;
            }
        }

        public string SSLCipherSpec
        {
            get
            {
                return base.GetString(this.mqCD.SSLCipherSpec).TrimEnd(new char[0]);
            }
            set
            {
                this.Version = (this.Version < 7) ? 7 : this.Version;
                byte[] sSLCipherSpec = this.mqCD.SSLCipherSpec;
                base.GetBytes(value, ref sSLCipherSpec);
                this.mqCD.SSLCipherSpec = sSLCipherSpec;
            }
        }

        public int SSLClientAuth
        {
            get
            {
                return this.mqCD.SSLClientAuth;
            }
            set
            {
                this.Version = (this.Version < 7) ? 7 : this.Version;
                this.mqCD.SSLClientAuth = value;
            }
        }

        public string SSLPeerName
        {
            get
            {
                if (this.sslPeerName != null)
                {
                    return base.GetString(this.sslPeerName);
                }
                return string.Empty;
            }
            set
            {
                this.Version = (this.Version < 7) ? 7 : this.Version;
                this.sslPeerName = new byte[0x400];
                base.GetBytes(value, ref this.sslPeerName);
                this.mqCD.SSLPeerNameLength = value.Length;
                this.mqCD.SSLPeerNamePtr = Marshal.AllocCoTaskMem(0x400);
                Marshal.Copy(this.sslPeerName, 0, this.mqCD.SSLPeerNamePtr, 0x400);
            }
        }

        internal int SSLPeerNameLength
        {
            get
            {
                return this.mqCD.SSLPeerNameLength;
            }
            set
            {
                this.mqCD.SSLPeerNameLength = value;
            }
        }

        public IntPtr SSLPeerNamePtr
        {
            get
            {
                return this.mqCD.SSLPeerNamePtr;
            }
            set
            {
                this.mqCD.SSLPeerNamePtr = value;
            }
        }

        public int StrucLength
        {
            get
            {
                return this.mqCD.StrucLength;
            }
            set
            {
                this.Version = (this.Version < 4) ? 4 : this.Version;
                this.mqCD.StrucLength = value;
            }
        }

        public MQBase.MQCD StructMQCD
        {
            get
            {
                return this.mqCD;
            }
            set
            {
                this.mqCD = value;
            }
        }

        public byte[] TPName
        {
            get
            {
                return this.mqCD.TpName;
            }
            set
            {
                this.mqCD.TpName = value;
            }
        }

        public int TransportType
        {
            get
            {
                return this.mqCD.TransportType;
            }
            set
            {
                this.mqCD.TransportType = value;
            }
        }

        public int UseDLQ
        {
            get
            {
                return this.mqCD.UseDLQ;
            }
            set
            {
                this.Version = (this.Version < 10) ? 10 : this.Version;
                this.mqCD.UseDLQ = value;
            }
        }

        public byte[] UserIdentifier
        {
            get
            {
                return this.mqCD.UserIdentifier;
            }
            set
            {
                this.Version = (this.Version < 2) ? 2 : this.Version;
                this.mqCD.UserIdentifier = value;
            }
        }

        public int Version
        {
            get
            {
                return this.mqCD.Version;
            }
            set
            {
                this.mqCD.Version = value;
            }
        }

        public byte[] XmitQName
        {
            get
            {
                return this.mqCD.XmitQName;
            }
            set
            {
                this.mqCD.XmitQName = value;
            }
        }
    }
}

