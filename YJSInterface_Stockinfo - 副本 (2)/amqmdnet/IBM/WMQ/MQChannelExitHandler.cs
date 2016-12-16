namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Remoting.Messaging;

    internal class MQChannelExitHandler : MQBase
    {
        private MQChannelDefinition cd;
        private MQFAPConnection fapConnection;
        private MQChannelDefinition negotiateCD;
        private ExitInstance[] ReceiveExit;
        public bool receiveExitDefined;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private ExitInstance[] SecurityExit;
        public bool securityExitDefined;
        private ExitInstance[] SendExit;
        public bool sendExitDefined;

        internal MQChannelExitHandler(MQFAPConnection conn)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { conn });
            this.fapConnection = conn;
            this.cd = this.fapConnection.NegotiatedChannel;
        }

        private string GetQualifiedExitPath(ref string libraryName)
        {
            uint method = 0x25;
            this.TrEntry(method, new object[] { libraryName });
            FileInfo info = null;
            string stringValue = null;
            try
            {
                info = new FileInfo(libraryName);
                if (info.FullName.Equals(libraryName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return libraryName;
                }
                MQClientCfg cfg = MQTCPConnection.Cfg;
                if (CommonServices.Is64bitCLR())
                {
                    stringValue = cfg.GetStringValue(MQClientCfg.CLIENTEXITPATH_EXITSDEFAULTPATH64);
                }
                else
                {
                    stringValue = cfg.GetStringValue(MQClientCfg.CLIENTEXITPATH_EXITSDEFAULTPATH);
                }
                libraryName = stringValue + @"\" + libraryName;
            }
            finally
            {
                base.TrExit(method);
            }
            return libraryName;
        }

        public void InitializeExits(int exitType, bool firstConv)
        {
            uint method = 0x1f;
            this.TrEntry(method, new object[] { exitType, firstConv });
            byte[] inb = null;
            int bufferOffset = 0;
            int bufferLength = 0;
            int bufferMaxLength = 0;
            bool securityRequired = false;
            MQChannelExit ep = new MQChannelExit();
            if (firstConv)
            {
                ep.SharingConversations = false;
            }
            else
            {
                ep.SharingConversations = true;
            }
            try
            {
                string[] sendExits;
                string securityExit;
                int num5;
                if (this.negotiateCD == null)
                {
                    this.negotiateCD = this.fapConnection.NegotiatedChannel;
                }
                switch (exitType)
                {
                    case 11:
                        securityExit = this.negotiateCD.SecurityExit;
                        if ((securityExit != null) && (securityExit.Length != 0))
                        {
                            this.SecurityExit[0].userData = this.negotiateCD.SecurityUserData;
                            this.LoadExit(ref this.SecurityExit[0]);
                            this.InvokeExits(11, ep, this.SecurityExit, inb, ref bufferOffset, ref bufferLength, ref bufferMaxLength, ref securityRequired);
                        }
                        return;

                    case 13:
                        sendExits = this.negotiateCD.SendExits;
                        securityExit = this.negotiateCD.SendExit;
                        if ((sendExits == null) || (sendExits.Length == 0))
                        {
                            goto Label_0160;
                        }
                        num5 = 0;
                        goto Label_0157;

                    case 14:
                        sendExits = this.negotiateCD.ReceiveExits;
                        securityExit = this.negotiateCD.ReceiveExit;
                        if ((sendExits == null) || (sendExits.Length == 0))
                        {
                            goto Label_0256;
                        }
                        num5 = 0;
                        goto Label_024D;

                    default:
                        throw new MQManagedClientException("Unsupported exit type " + exitType, 2, 0x893);
                }
            Label_0118:
                if ((this.negotiateCD.SendUserDatas != null) && (this.negotiateCD.SendUserDatas.Length > 1))
                {
                    this.SendExit[num5].userData = this.negotiateCD.SendUserDatas[num5];
                }
                num5++;
            Label_0157:
                if (num5 < sendExits.Length)
                {
                    goto Label_0118;
                }
                goto Label_01AC;
            Label_0160:
                if (((securityExit != null) && (securityExit != "")) && ((this.negotiateCD.SendUserData != null) && (this.negotiateCD.SendUserData != "")))
                {
                    this.SendExit[0].userData = this.negotiateCD.SendUserData;
                }
            Label_01AC:
                if (((sendExits != null) && (sendExits.Length != 0)) || ((securityExit != null) && (securityExit != "")))
                {
                    this.InvokeExits(11, ep, this.SendExit, inb, ref bufferOffset, ref bufferLength, ref bufferMaxLength, ref securityRequired);
                }
                return;
            Label_020E:
                if ((this.negotiateCD.ReceiveUserDatas != null) && (this.negotiateCD.ReceiveUserDatas.Length > 1))
                {
                    this.ReceiveExit[num5].userData = this.negotiateCD.ReceiveUserDatas[num5];
                }
                num5++;
            Label_024D:
                if (num5 < sendExits.Length)
                {
                    goto Label_020E;
                }
                goto Label_029D;
            Label_0256:
                if (((securityExit != null) && (securityExit.Length != 0)) && ((this.negotiateCD.ReceiveUserData != null) && (this.negotiateCD.ReceiveUserData != "")))
                {
                    this.ReceiveExit[0].userData = this.negotiateCD.ReceiveUserData;
                }
            Label_029D:
                if (((sendExits != null) && (sendExits.Length != 0)) || ((securityExit != null) && (securityExit != "")))
                {
                    this.InvokeExits(11, ep, this.ReceiveExit, inb, ref bufferOffset, ref bufferLength, ref bufferMaxLength, ref securityRequired);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public byte[] InitSecurity(ref int offset, ref int length, ref int maxlength, ref bool securityRequired)
        {
            uint method = 0x20;
            this.TrEntry(method, new object[] { (int) offset, (int) length, (int) maxlength, (bool) securityRequired });
            byte[] inb = null;
            MQChannelExit ep = new MQChannelExit();
            ep.SharingConversations = false;
            try
            {
                if (this.SecurityExit != null)
                {
                    inb = this.InvokeExits(0x10, ep, this.SecurityExit, inb, ref offset, ref length, ref maxlength, ref securityRequired);
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return inb;
        }

        public byte[] InvokeExits(int exitReason, MQChannelExit ep, ExitInstance[] ei, byte[] inb, ref int bufferOffset, ref int bufferLength, ref int bufferMaxLength, ref bool securityRequired)
        {
            uint method = 0x1d;
            this.TrEntry(method, new object[] { exitReason, ep, ei, inb, (int) bufferOffset, (int) bufferLength, (int) bufferMaxLength, (bool) securityRequired });
            byte[] dataBuffer = inb;
            byte[] buffer2 = null;
            int length = ei.Length;
            int index = 0;
            bool flag = false;
            int maxTransmissionSize = this.fapConnection.MaxTransmissionSize;
            CallContext.SetData("inExit", true);
            try
            {
                securityRequired = false;
                ep.ExitReason = exitReason;
                ep.FapLevel = this.fapConnection.FapLevel;
                for (index = 0; index < length; index++)
                {
                    ep.ExitID = ei[index].exitID;
                    ep.ExitResponse = 0;
                    ep.ExitNumber = index;
                    ep.UserData = ei[index].userData;
                    ep.ExitUserArea = ei[index].exitUserArea;
                    try
                    {
                        if (ei[index].loaded && (!ei[index].suppressed || ((exitReason == 12) && ei[index].initialised)))
                        {
                            switch (ei[index].exitID)
                            {
                                case 11:
                                    maxTransmissionSize = this.fapConnection.MaxTransmissionSize - 0x20;
                                    ep.MaxSegmentLength = maxTransmissionSize;
                                    buffer2 = ((MQSecurityExit) ei[index].Method).SecurityExit(ep, this.cd, dataBuffer, ref bufferOffset, ref bufferLength, ref bufferMaxLength);
                                    if (buffer2 != null)
                                    {
                                        switch (ep.ExitResponse)
                                        {
                                        }
                                        buffer2 = null;
                                    }
                                    break;

                                case 13:
                                    maxTransmissionSize = this.fapConnection.MaxTransmissionSize;
                                    ep.MaxSegmentLength = maxTransmissionSize;
                                    buffer2 = ((MQSendExit) ei[index].Method).SendExit(ep, this.cd, dataBuffer, ref bufferOffset, ref bufferLength, ref bufferMaxLength);
                                    break;

                                case 14:
                                    maxTransmissionSize = this.fapConnection.MaxTransmissionSize;
                                    ep.MaxSegmentLength = maxTransmissionSize;
                                    buffer2 = ((MQReceiveExit) ei[index].Method).ReceiveExit(ep, this.cd, dataBuffer, ref bufferOffset, ref bufferLength, ref bufferMaxLength);
                                    break;

                                default:
                                    throw new MQManagedClientException("Unsupported exit type " + ei[index].exitID, 2, 0x893);
                            }
                            ei[index].exitUserArea = ep.ExitUserArea;
                            dataBuffer = buffer2;
                            if (exitReason == 11)
                            {
                                ei[index].initialised = true;
                                if (ep.FapLevel < this.fapConnection.FapLevel)
                                {
                                    this.fapConnection.FapLevel = (byte) ep.FapLevel;
                                }
                            }
                        }
                        switch (ep.ExitResponse)
                        {
                            case -8:
                            case -6:
                            case -2:
                            case -1:
                                if (ep.ExitResponse == -6)
                                {
                                    this.fapConnection.Disconnect();
                                }
                                throw new MQManagedClientException(0x20009536, 0, 0, this.cd.ChannelName, ei[index].exitName, "", 2, 0x9e9);

                            case -5:
                                ei[index].suppressed = true;
                                break;

                            case -4:
                            case 0:
                                break;

                            case -3:
                                securityRequired = true;
                                break;

                            default:
                                CommonServices.SetValidInserts();
                                CommonServices.ArithInsert1 = (uint) ep.ExitResponse;
                                CommonServices.ArithInsert2 = 0;
                                CommonServices.CommentInsert1 = ei[index].exitName;
                                CommonServices.CommentInsert2 = "";
                                CommonServices.CommentInsert3 = "";
                                base.DisplayMessage(0x20009181, 0xf0000010);
                                flag = true;
                                break;
                        }
                        if (((buffer2 != null) && (bufferOffset < 0)) || ((bufferLength < 0) || (bufferMaxLength < 0)))
                        {
                            CommonServices.SetValidInserts();
                            CommonServices.ArithInsert1 = (uint) bufferLength;
                            CommonServices.ArithInsert2 = 0;
                            CommonServices.CommentInsert1 = ei[index].exitName;
                            CommonServices.CommentInsert2 = "";
                            CommonServices.CommentInsert3 = "";
                            base.DisplayMessage(0x20009189, 0xf0000010);
                            flag = true;
                        }
                        if ((buffer2 != null) && ((bufferOffset + bufferLength) > buffer2.Length))
                        {
                            CommonServices.SetValidInserts();
                            CommonServices.ArithInsert1 = (uint) bufferLength;
                            CommonServices.ArithInsert2 = 0;
                            CommonServices.CommentInsert1 = ei[index].exitName;
                            CommonServices.CommentInsert2 = "";
                            CommonServices.CommentInsert3 = "";
                            base.DisplayMessage(0x20009197, 0xf0000010);
                            flag = true;
                        }
                        if ((buffer2 != null) && (bufferLength > maxTransmissionSize))
                        {
                            CommonServices.SetValidInserts();
                            CommonServices.ArithInsert1 = (uint) bufferLength;
                            CommonServices.ArithInsert2 = (uint) maxTransmissionSize;
                            CommonServices.CommentInsert1 = ei[index].exitName;
                            CommonServices.CommentInsert2 = "";
                            CommonServices.CommentInsert3 = "";
                            base.DisplayMessage(0x20009195, 0xf0000010);
                            flag = true;
                        }
                    }
                    catch (MQManagedClientException)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        base.TrException(method, exception, 1);
                        base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009190, 0);
                        flag = true;
                    }
                    if (flag)
                    {
                        throw new MQManagedClientException(0x20009190, (uint) ep.ExitID, (uint) ep.ExitReason, ei[index].exitName, this.cd.ChannelName, "", 2, 0x893);
                    }
                }
            }
            finally
            {
                CallContext.SetData("inExit", false);
                base.TrExit(method);
            }
            return buffer2;
        }

        public void LoadExit(ref ExitInstance ei)
        {
            uint method = 0x1c;
            this.TrEntry(method, new object[] { ei });
            string exitName = ei.exitName;
            int index = exitName.IndexOf('(', 0);
            int num3 = exitName.IndexOf(')', 0);
            Type type = null;
            try
            {
                if (((index <= 0) || (num3 <= 0)) || (num3 < index))
                {
                    throw new MQManagedClientException(0x20009535, 0, 0, this.cd.ChannelName, exitName, "", 2, 0x893);
                }
                string libraryName = exitName.Substring(0, index);
                this.GetQualifiedExitPath(ref libraryName);
                string name = exitName.Substring(index + 1, (num3 - index) - 1);
                try
                {
                    type = Assembly.LoadFrom(libraryName).GetType(name, true);
                    if (type == null)
                    {
                        throw new MQManagedClientException(0x20009535, 0, 0, this.cd.ChannelName, exitName, "", 2, 0x893);
                    }
                    ei.Method = type.InvokeMember(null, BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, null, null);
                    ei.loaded = true;
                }
                catch (Exception exception)
                {
                    base.TrException(method, exception);
                    throw new MQManagedClientException(0x20009535, 0, 0, this.cd.ChannelName, exitName, "", 2, 0x893);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void LoadExits(int exitType)
        {
            uint method = 30;
            this.TrEntry(method, new object[] { exitType });
            new MQChannelExit();
            try
            {
                string securityExit;
                string[] sendExits;
                string securityUserData;
                string[] sendUserDatas;
                int num2;
                switch (exitType)
                {
                    case 11:
                        securityExit = this.cd.SecurityExit;
                        if ((securityExit != null) && (securityExit.Length != 0))
                        {
                            this.securityExitDefined = true;
                            securityUserData = this.cd.SecurityUserData;
                            this.SecurityExit = new ExitInstance[] { new ExitInstance(securityExit, securityUserData, 11) };
                            this.LoadExit(ref this.SecurityExit[0]);
                        }
                        return;

                    case 13:
                        sendExits = this.cd.SendExits;
                        securityExit = this.cd.SendExit;
                        if ((sendExits == null) || (sendExits.Length == 0))
                        {
                            goto Label_0141;
                        }
                        this.sendExitDefined = true;
                        sendUserDatas = this.cd.SendUserDatas;
                        this.SendExit = new ExitInstance[sendExits.Length];
                        num2 = 0;
                        goto Label_0135;

                    case 14:
                        sendExits = this.cd.ReceiveExits;
                        securityExit = this.cd.ReceiveExit;
                        if ((sendExits == null) || (sendExits.Length == 0))
                        {
                            goto Label_022D;
                        }
                        this.receiveExitDefined = true;
                        sendUserDatas = this.cd.ReceiveUserDatas;
                        this.ReceiveExit = new ExitInstance[sendExits.Length];
                        num2 = 0;
                        goto Label_0224;

                    default:
                        throw new MQManagedClientException("Unsupported exit type " + exitType, 2, 0x893);
                }
            Label_00F1:
                if ((sendUserDatas != null) && (sendUserDatas.Length > num2))
                {
                    securityUserData = sendUserDatas[num2];
                }
                else
                {
                    securityUserData = null;
                }
                this.SendExit[num2] = new ExitInstance(sendExits[num2], securityUserData, 13);
                this.LoadExit(ref this.SendExit[num2]);
                num2++;
            Label_0135:
                if (num2 < sendExits.Length)
                {
                    goto Label_00F1;
                }
                return;
            Label_0141:
                if ((securityExit != null) && (securityExit.Length != 0))
                {
                    this.sendExitDefined = true;
                    securityUserData = this.cd.SendUserData;
                    this.SendExit = new ExitInstance[] { new ExitInstance(securityExit, securityUserData, 13) };
                    this.LoadExit(ref this.SendExit[0]);
                }
                return;
            Label_01E0:
                if ((sendUserDatas != null) && (sendUserDatas.Length > num2))
                {
                    securityUserData = sendUserDatas[num2];
                }
                else
                {
                    securityUserData = null;
                }
                this.ReceiveExit[num2] = new ExitInstance(sendExits[num2], securityUserData, 14);
                this.LoadExit(ref this.ReceiveExit[num2]);
                num2++;
            Label_0224:
                if (num2 < sendExits.Length)
                {
                    goto Label_01E0;
                }
                return;
            Label_022D:
                if ((securityExit != null) && (securityExit.Length != 0))
                {
                    this.receiveExitDefined = true;
                    securityUserData = this.cd.ReceiveUserData;
                    this.ReceiveExit = new ExitInstance[] { new ExitInstance(securityExit, securityUserData, 14) };
                    this.LoadExit(ref this.ReceiveExit[0]);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public byte[] ProcessReceiveExits(ref byte[] input, ref int offset, ref int length, ref int maxlength)
        {
            uint method = 0x23;
            this.TrEntry(method, new object[] { input, (int) offset, (int) length, (int) maxlength });
            bool securityRequired = false;
            byte[] buffer = input;
            MQChannelExit ep = new MQChannelExit();
            if (this.fapConnection.IsMultiplexingEnabled)
            {
                ep.SharingConversations = true;
            }
            else
            {
                ep.SharingConversations = false;
            }
            try
            {
                if (this.ReceiveExit != null)
                {
                    buffer = this.InvokeExits(14, ep, this.ReceiveExit, input, ref offset, ref length, ref maxlength, ref securityRequired);
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return buffer;
        }

        public byte[] ProcessSecurity(ref byte[] input, ref int offset, ref int length, ref int maxlength, ref bool securityRequired)
        {
            uint method = 0x21;
            this.TrEntry(method, new object[] { input, (int) offset, (int) length, (int) maxlength, (bool) securityRequired });
            byte[] buffer = null;
            MQChannelExit ep = new MQChannelExit();
            ep.SharingConversations = false;
            try
            {
                if (this.SecurityExit != null)
                {
                    buffer = this.InvokeExits(15, ep, this.SecurityExit, input, ref offset, ref length, ref maxlength, ref securityRequired);
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return buffer;
        }

        public byte[] ProcessSecurityParms(MQConnectionSecurityParameters mqcsp, ref byte[] input, ref int offset, ref int length, ref int maxlength, ref bool securityRequired)
        {
            uint method = 0x496;
            this.TrEntry(method, new object[] { mqcsp, input, (int) offset, (int) length, (int) maxlength, (bool) securityRequired });
            byte[] buffer = null;
            MQChannelExit ep = new MQChannelExit();
            ep.SecurityParms = mqcsp;
            try
            {
                if (this.SecurityExit != null)
                {
                    buffer = this.InvokeExits(0x1d, ep, this.SecurityExit, input, ref offset, ref length, ref maxlength, ref securityRequired);
                    mqcsp = ep.SecurityParms;
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return buffer;
        }

        public byte[] ProcessSendExits(ref byte[] input, ref int offset, ref int length, ref int maxlength)
        {
            uint method = 0x22;
            this.TrEntry(method, new object[] { input, (int) offset, (int) length, (int) maxlength });
            bool securityRequired = false;
            byte[] buffer = input;
            MQChannelExit ep = new MQChannelExit();
            if (this.fapConnection.IsMultiplexingEnabled)
            {
                ep.SharingConversations = true;
            }
            else
            {
                ep.SharingConversations = false;
            }
            try
            {
                if (this.SendExit != null)
                {
                    buffer = this.InvokeExits(14, ep, this.SendExit, input, ref offset, ref length, ref maxlength, ref securityRequired);
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return buffer;
        }

        public void TermExits()
        {
            uint method = 0x24;
            this.TrEntry(method);
            byte[] inb = null;
            int bufferOffset = 0;
            int bufferLength = 0;
            int bufferMaxLength = 0;
            bool securityRequired = false;
            MQChannelExit ep = new MQChannelExit();
            try
            {
                if (this.SecurityExit != null)
                {
                    this.InvokeExits(12, ep, this.SecurityExit, inb, ref bufferOffset, ref bufferLength, ref bufferMaxLength, ref securityRequired);
                }
                if (this.SendExit != null)
                {
                    this.InvokeExits(12, ep, this.SendExit, inb, ref bufferOffset, ref bufferLength, ref bufferMaxLength, ref securityRequired);
                }
                if (this.ReceiveExit != null)
                {
                    this.InvokeExits(12, ep, this.ReceiveExit, inb, ref bufferOffset, ref bufferLength, ref bufferMaxLength, ref securityRequired);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }
    }
}

