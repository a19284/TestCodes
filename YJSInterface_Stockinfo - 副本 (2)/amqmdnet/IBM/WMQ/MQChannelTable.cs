namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    internal class MQChannelTable : NmqiObject
    {
        private const uint AMQR_EYECATCHER = 0x52514d41;
        private string mqfile;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private const int TABLE_BUFFER_CHUNK = 0x8000;
        private Stream tableStream;
        private byte[] uintarray;

        internal MQChannelTable(NmqiEnvironment nmqiEnv) : base(nmqiEnv)
        {
            string str2;
            this.uintarray = new byte[4];
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv });
            MQClientCfg cfg = nmqiEnv.Cfg;
            this.mqfile = cfg.GetStringValue(MQClientCfg.ENV_MQCHLLIB);
            if (this.mqfile != null)
            {
                base.TrText("MQCHLLIB=" + this.mqfile);
            }
            else
            {
                base.TrText("MQCHLLIB=null");
                this.mqfile = CommonServices.DataLib;
            }
            if ((this.mqfile.Length == 0) || (this.mqfile == "."))
            {
                this.mqfile = Environment.CurrentDirectory;
            }
            if (this.mqfile.ToLower().IndexOf("http://") != -1)
            {
                str2 = "/";
            }
            else
            {
                str2 = @"\";
            }
            if (!this.mqfile.EndsWith(str2))
            {
                this.mqfile = this.mqfile + str2;
            }
            string stringValue = cfg.GetStringValue(MQClientCfg.ENV_MQCHLTAB);
            if (stringValue != null)
            {
                base.TrText("MQCHLTAB=" + stringValue);
                this.mqfile = this.mqfile + stringValue;
            }
            else
            {
                base.TrText("MQCHLTAB=null");
                this.mqfile = this.mqfile + "AMQCLCHL.TAB";
            }
            if (this.mqfile != null)
            {
                base.TrText("MQFILE=" + this.mqfile);
            }
        }

        internal MQChannelTable(NmqiEnvironment nmqiEnv, string ccdtFile) : base(nmqiEnv)
        {
            this.uintarray = new byte[4];
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv, ccdtFile });
            this.mqfile = ccdtFile;
        }

        public void CreateChannelEntryLists(MQChannelListEntry nameList)
        {
            uint method = 0x31;
            this.TrEntry(method, new object[] { nameList });
            try
            {
                if (this.tableStream == null)
                {
                    this.GetStream();
                }
                else
                {
                    this.tableStream.Seek(0L, SeekOrigin.Begin);
                }
                if (this.Readuint(this.tableStream) != 0x52514d41)
                {
                    base.throwNewMQException(2, 0x8e6);
                }
                MQChannelDefinition channel = null;
                bool flag = false;
                byte[] buffer = null;
                int num6 = this.FindFirstChannel(this.tableStream);
                while (!flag)
                {
                    this.tableStream.Seek((long) num6, SeekOrigin.Begin);
                    if (this.Readuint(this.tableStream) == 0)
                    {
                        flag = true;
                        break;
                    }
                    uint num4 = this.Readuint(this.tableStream);
                    this.Readuint(this.tableStream);
                    this.Readuint(this.tableStream);
                    uint num5 = this.Readuint(this.tableStream);
                    if (num5 != 0)
                    {
                        num6 = (int) num5;
                    }
                    else
                    {
                        flag = true;
                    }
                    if ((buffer == null) || (num4 > buffer.Length))
                    {
                        buffer = new byte[num4];
                    }
                    int length = this.tableStream.Read(buffer, 0, (int) num4);
                    channel = new MQChannelDefinition(buffer, 0, length);
                    bool flag2 = true;
                    string name = null;
                    base.TrText(method, "Qmgr name suppled " + nameList.Name);
                    if (((nameList.Name != null) && (nameList.Name != "*")) && (nameList.Name != string.Empty))
                    {
                        base.TrText(method, "Qmgr name suppled " + nameList.Name);
                        name = nameList.Name;
                    }
                    if (name != null)
                    {
                        Encoding.ASCII.GetString(channel.QMgrName).Trim();
                        if (name.StartsWith("*") && (name.Length > 1))
                        {
                            name = name.Substring(1);
                            base.TrText(method, "Qmgr name being used by nmqi : " + name);
                        }
                        byte[] buffer2 = new byte[0x30];
                        base.GetBytesRightPad(name, ref buffer2);
                        for (int i = 0; i < 0x30; i++)
                        {
                            if (buffer2[i] != channel.QMgrName[i])
                            {
                                flag2 = false;
                                break;
                            }
                        }
                    }
                    base.TrText(method, "Did we find a matching channel definition from ccdt ? " + flag2);
                    if (flag2)
                    {
                        if ((nameList.Name != null) && nameList.Name.StartsWith("*"))
                        {
                            base.TrText(method, "Qmgr name suppled has * in it.. Found the cd having it. Setting qmgr name as blank in it.");
                            byte[] qMgrName = channel.QMgrName;
                            base.GetBytesRightPad(string.Empty, ref qMgrName);
                            channel.QMgrName = qMgrName;
                        }
                        if (channel.Version < 9)
                        {
                            channel.SharingConversations = 0;
                            channel.PropertyControl = 0;
                            channel.MaxInstances = 0x3b9ac9ff;
                            channel.MaxInstancesPerClient = 0x3b9ac9ff;
                            channel.ClientChannelWeight = 0;
                            channel.ConnectionAffinity = 0;
                            nameList.AddChannelEntry(channel);
                            break;
                        }
                        nameList.AddChannelEntry(channel);
                    }
                }
                if (nameList.WeightedEntry != null)
                {
                    nameList.OrderWeightedChannelEntry();
                }
                nameList.ThisAlphaEntry = nameList.AlphaEntry;
                nameList.ThisWeightedEntry = nameList.WeightedEntry;
            }
            catch (MQException)
            {
                throw;
            }
            catch (IOException exception)
            {
                base.TrException(method, exception, 1);
                throw exception;
            }
            catch (Exception exception2)
            {
                base.TrException(method, exception2, 2);
                base.TrException(method, exception2);
                base.throwNewMQException(2, 0x8e6);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private MQChannelDefinition FindChannel(Stream f, string qmname, ref int pos)
        {
            uint method = 0x2e;
            this.TrEntry(method, new object[] { f, qmname, (int) pos });
            MQChannelDefinition definition = null;
            byte[] bytes = new byte[0x30];
            byte[] buffer = null;
            Encoding.ASCII.GetBytes(qmname, 0, qmname.Length, bytes, 0);
            try
            {
                if (pos == 0)
                {
                    pos = this.FindFirstChannel(f);
                }
                while (pos >= 0)
                {
                    f.Seek((long) pos, SeekOrigin.Begin);
                    if (this.Readuint(f) == 0)
                    {
                        return definition;
                    }
                    uint num3 = this.Readuint(f);
                    this.Readuint(f);
                    this.Readuint(f);
                    uint num4 = this.Readuint(f);
                    if (num4 != 0)
                    {
                        pos = (int) num4;
                    }
                    else
                    {
                        pos = -1;
                    }
                    if ((buffer == null) || (num3 > buffer.Length))
                    {
                        buffer = new byte[num3];
                    }
                    int length = f.Read(buffer, 0, (int) num3);
                    bool flag = true;
                    for (int i = 0; i < 0x30; i++)
                    {
                        if (bytes[i] != buffer[0x60 + i])
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        return new MQChannelDefinition(buffer, 0, length);
                    }
                }
                return definition;
            }
            catch (MQException)
            {
                throw;
            }
            catch (Exception exception)
            {
                base.TrException(method, exception, 1);
                throw new IOException();
            }
            finally
            {
                base.TrExit(method);
            }
            return definition;
        }

        private int FindFirstChannel(Stream f)
        {
            uint method = 0x2d;
            this.TrEntry(method, new object[] { f });
            long position = -1L;
            try
            {
                position = f.Position;
                uint num2 = this.Readuint(f);
                while (num2 != 0)
                {
                    if (this.Readuint(f) != 0)
                    {
                        break;
                    }
                    f.Seek(position + num2, SeekOrigin.Begin);
                    position = f.Position;
                    num2 = this.Readuint(f);
                }
                if (num2 == 0)
                {
                    position = 0L;
                }
                else
                {
                    this.Readuint(f);
                    for (uint i = this.Readuint(f); i != 0; i = this.Readuint(f))
                    {
                        f.Seek((long) i, SeekOrigin.Begin);
                        position = i;
                        num2 = this.Readuint(f);
                        uint num3 = this.Readuint(f);
                        this.Readuint(f);
                    }
                }
            }
            catch (MQException)
            {
                throw;
            }
            catch (Exception exception)
            {
                base.TrException(method, exception, 1);
                throw new IOException();
            }
            finally
            {
                base.TrExit(method);
            }
            return (int) position;
        }

        public MQChannelDefinition GetChannelDefinition(string qmname, ref int pos)
        {
            uint method = 0x2f;
            this.TrEntry(method, new object[] { qmname, (int) pos });
            MQChannelDefinition definition = null;
            try
            {
                if (this.tableStream == null)
                {
                    this.GetStream();
                }
                else
                {
                    this.tableStream.Seek(0L, SeekOrigin.Begin);
                }
                if (this.Readuint(this.tableStream) != 0x52514d41)
                {
                    throw new IOException();
                }
                qmname = qmname.PadRight(0x30);
                definition = this.FindChannel(this.tableStream, qmname, ref pos);
            }
            catch (MQException)
            {
                throw;
            }
            catch (IOException exception)
            {
                base.TrException(method, exception, 1);
                MQManagedClientException exception2 = new MQManagedClientException(0x20009555, 0, 0, this.mqfile, null, null, 2, 0x8a0);
                throw exception2;
            }
            finally
            {
                base.TrExit(method);
            }
            return definition;
        }

        private void GetStream()
        {
            uint method = 0x30;
            this.TrEntry(method);
            string mqfile = this.mqfile;
            try
            {
                if (this.mqfile.IndexOf("://") == -1)
                {
                    mqfile = "file://" + this.mqfile;
                }
                base.TrText("Channel table URI: '" + mqfile + "'");
                WebResponse response = WebRequest.Create(mqfile).GetResponse();
                if (mqfile.ToLower().StartsWith("http://"))
                {
                    using (response)
                    {
                        byte[] buffer;
                        int num2 = 0;
                        int index = 0;
                        BinaryReader reader = new BinaryReader(response.GetResponseStream());
                        byte[] buffer2 = new byte[0x8000];
                        while ((num2 = reader.Read(buffer2, index, 0x8000)) != 0)
                        {
                            if ((index == 0) && (BitConverter.ToUInt32(buffer2, 0) != 0x52514d41))
                            {
                                throw new IOException();
                            }
                            index += num2;
                            buffer = new byte[index + 0x8000];
                            Buffer.BlockCopy(buffer2, 0, buffer, 0, index);
                            buffer2 = buffer;
                        }
                        buffer = new byte[index];
                        Buffer.BlockCopy(buffer2, 0, buffer, 0, index);
                        this.tableStream = new MemoryStream(buffer);
                        return;
                    }
                }
                this.tableStream = response.GetResponseStream();
            }
            catch (MQException)
            {
                throw;
            }
            catch (WebException exception)
            {
                base.TrException(method, exception, 1);
                MQManagedClientException exception2 = new MQManagedClientException(0x20009518, 0, 0, this.mqfile, null, null, 2, 0x80a);
                throw exception2;
            }
            catch (UriFormatException exception3)
            {
                base.TrException(method, exception3, 2);
                MQManagedClientException exception4 = new MQManagedClientException(0x20009518, 0, 0, mqfile, null, null, 2, 0x80a);
                throw exception4;
            }
            catch (IOException exception5)
            {
                base.TrException(method, exception5, 3);
                MQManagedClientException exception6 = new MQManagedClientException(0x20009555, 0, 0, this.mqfile, null, null, 2, 0x80a);
                throw exception6;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private uint Readuint(Stream f)
        {
            uint method = 0x2c;
            this.TrEntry(method, new object[] { f });
            uint result = 0;
            try
            {
                if (f.Read(this.uintarray, 0, 4) < 4)
                {
                    throw new IOException();
                }
                result = BitConverter.ToUInt32(this.uintarray, 0);
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        internal string CCDTFile
        {
            get
            {
                return this.mqfile;
            }
        }
    }
}

