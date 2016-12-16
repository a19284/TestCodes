namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Xml;

    internal class MQMarshalMessageForGet : MQBaseObject, IDisposable
    {
        private Hashtable _receivedProps;
        public const int CCSID_UTF8 = 0x4b8;
        private int dataLength;
        private const string DT_ATTR_BOOL = "boolean";
        private const string DT_ATTR_BYTE = "i1";
        private const string DT_ATTR_BYTEARRAY = "bin.hex";
        private const string DT_ATTR_CHAR = "char";
        private const string DT_ATTR_DOUBLE = "r8";
        private const string DT_ATTR_FLOAT = "r4";
        private const string DT_ATTR_INT = "i4";
        private const string DT_ATTR_INT_ANY = "int";
        private const string DT_ATTR_LONG = "i8";
        private const string DT_ATTR_SHORT = "i2";
        private const string DT_ATTR_STRING = "string";
        private const string FOLDER_JMS_START = "<jms>";
        private const string FOLDER_MCD_START = "<mcd>";
        private const string FOLDER_MQEXT_START = "<mqext>";
        private const string FOLDER_MQPS_START = "<mqps>";
        private const string FOLDER_MSG_PROPERTY = "content='properties'";
        private const string FOLDER_MSG_PROPERTY_WITH_SPACE = "content = 'properties'";
        private const string FOLDER_PSC_START = "<psc>";
        private const string FOLDER_USR_START = "<usr>";
        private const int INT_SIZEOF = 4;
        private const int MQC_FMT_SIZEOF = 8;
        internal const long MQHEADER_ASCII = 0x4d51480000000000L;
        internal const ulong MQHEADER_EBCDIC = 15337228433335779328L;
        internal const ulong MQHEADER_MASK = 18446742974197923840L;
        private MQMessage mqMessage;
        internal const ulong MQRFH2_ASCII = 0x4d51485246322020L;
        internal const ulong MQRFH2_EBCDIC = 15337229368681447488L;
        internal const long MQSTR_ASCII = 0x4d51535452202020L;
        internal const ulong MQSTR_EBCDIC = 15337257999240544320L;
        private Encoding nonUTF8Rfh2Enc;
        private const int NXT_HDR_CCSID_OFFST = 0x10;
        private const int NXT_HDR_ENCODING_OFFST = 12;
        private const int NXT_HDR_FORMAT_OFFST = 20;
        private int originalCcsid;
        private int originalEncoding;
        private int readCursor;
        private const int RFH2_CCSID_OFFSET = 0x20;
        private const int RFH2_DATA_OFFSET = 0x24;
        private int rfh2HeaderEncoding = 0x222;
        private bool rfh2InUtf8;
        private byte[] rfhBytes;
        private ArrayList rfhHeaderList = new ArrayList(10);
        private ArrayList rfhStrings = new ArrayList(10);
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public MQMarshalMessageForGet(MQMessage mqMsg, byte[] content, int length)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { mqMsg, content, length });
            this.mqMessage = mqMsg;
            this.originalEncoding = this.mqMessage.Encoding;
            this.originalCcsid = this.mqMessage.CharacterSet;
            this.dataLength = length;
            this._receivedProps = new Hashtable(50);
            if (length > 0)
            {
                this.rfhBytes = new byte[length];
                System.Buffer.BlockCopy(content, 0, this.rfhBytes, 0, length);
            }
        }

        private void CheckForPropertyDescriptorAttributes(ref MQPropertyDescriptor pd, ref XmlTextReader xmlReader)
        {
            uint method = 0xfd;
            this.TrEntry(method, new object[] { pd, xmlReader });
            try
            {
                switch (xmlReader.GetAttribute("context"))
                {
                    case "none":
                        pd.Context = 0;
                        break;

                    case "user":
                        pd.Context = 1;
                        break;
                }
                string attribute = xmlReader.GetAttribute("support");
                if (attribute != null)
                {
                    StringBuilder hex = new StringBuilder(attribute);
                    byte[] bin = new byte[attribute.Length / 2];
                    MQMessage.BinToHex(bin, 0, bin.Length, hex);
                    Array.Reverse(bin);
                    pd.Support = BitConverter.ToInt32(bin, 0);
                }
                string str3 = xmlReader.GetAttribute("copy");
                if (str3 != null)
                {
                    foreach (string str4 in str3.Split(new char[] { ',' }))
                    {
                        if (str4 == "none")
                        {
                            pd.CopyOptions = 0;
                        }
                        else if (str4 == "All")
                        {
                            pd.CopyOptions = 1;
                        }
                        else
                        {
                            if (pd.CopyOptions == 0x16)
                            {
                                pd.CopyOptions = 0;
                            }
                            if (str4 == "forward")
                            {
                                pd.CopyOptions += 2;
                            }
                            if (str4 == "reply")
                            {
                                pd.CopyOptions += 8;
                            }
                            if (str4 == "report")
                            {
                                pd.CopyOptions += 0x10;
                            }
                        }
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void ConvertBytesContentToString()
        {
            uint method = 0xf5;
            this.TrEntry(method);
            try
            {
                int count = 0;
                int offset = 0;
                int length = 0;
                string str = null;
                foreach (RFHHeader header in this.rfhHeaderList)
                {
                    if ((header.hdrFormatAsLong == 0x4d51485246322020L) || (header.hdrFormatAsLong == 15337229368681447488L))
                    {
                        base.TrText(method, "Found MQRFH2 header,Processing it now");
                        this.rfh2HeaderEncoding = header.hdrEncoding;
                        int mqCcsid = this.ReadInt(this.rfh2HeaderEncoding, header.hdrData, 0x20);
                        if (mqCcsid == 0x4b8)
                        {
                            this.rfh2InUtf8 = true;
                        }
                        else
                        {
                            this.rfh2InUtf8 = false;
                            this.nonUTF8Rfh2Enc = MQCcsidTable.GetDotnetEncoding(mqCcsid);
                        }
                        int num6 = header.hdrLength - 0x24;
                        base.TrText(method, "Length of MQRFH2 Variable part = " + num6);
                        byte[] dst = new byte[num6];
                        System.Buffer.BlockCopy(header.hdrData, 0x24, dst, 0, num6);
                        count = 0;
                        offset = 0;
                        length = 0;
                        str = null;
                        if ((dst != null) && (dst.Length != 0))
                        {
                            length = dst.Length;
                            ArrayList list = new ArrayList(10);
                            do
                            {
                                count = this.ReadInt(this.rfh2HeaderEncoding, dst, offset);
                                offset += 4;
                                if (this.rfh2InUtf8)
                                {
                                    str = Encoding.UTF8.GetString(dst, offset, count);
                                }
                                else
                                {
                                    str = this.nonUTF8Rfh2Enc.GetString(dst, offset, count);
                                }
                                if (((str.Contains("<mcd>") || str.Contains("<jms>")) || (str.Contains("<usr>") || str.Contains("<mqext>"))) || ((str.Contains("<psc>") || str.Contains("<mqps>")) || (str.Contains("content='properties'") || str.Contains("content = 'properties'"))))
                                {
                                    header.includeInMsg = false;
                                }
                                base.TrText(method, "Adding xmlcontent to list: " + str);
                                list.Add(str);
                                offset += count;
                            }
                            while (offset < length);
                            if (!header.includeInMsg)
                            {
                                base.TrText(method, "NOT including this RFH2 header as part of message data and this header will be used as message properties");
                                foreach (object obj2 in list)
                                {
                                    this.rfhStrings.Add(obj2);
                                }
                            }
                            else
                            {
                                base.TrText(method, "Including this RFH2 header as part of message data and this header will NOT be used as message properties");
                            }
                        }
                    }
                    else
                    {
                        base.TrText(method, "Found a non MQRFH2 header, skipping it");
                    }
                }
            }
            catch (Exception exception)
            {
                base.TrException(method, exception);
                base.throwNewMQException(2, 0x17da);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public void Dispose(bool fromDispose)
        {
            this.mqMessage = null;
            this._receivedProps = null;
            this.rfhBytes = null;
            this.rfhStrings = null;
            if (!fromDispose)
            {
                GC.SuppressFinalize(this);
            }
        }

        private ulong GetFormatAsLong(Encoding encoding)
        {
            uint method = 0xf8;
            this.TrEntry(method, new object[] { encoding });
            ulong num2 = 0L;
            byte[] bytes = null;
            try
            {
                if (encoding != null)
                {
                    base.TrText(method, "Encoding used = " + encoding.EncodingName);
                    bytes = encoding.GetBytes(this.mqMessage.Format);
                }
                else
                {
                    bytes = Encoding.UTF8.GetBytes(this.mqMessage.Format);
                }
                for (int i = 0; i < 8; i++)
                {
                    num2 = num2 << 8;
                    ulong num4 = bytes[i];
                    if (num4 < 0L)
                    {
                        num4 += (ulong) 0x100L;
                    }
                    num2 += num4;
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return num2;
        }

        private ulong GetFormatAsLongFromBytes(Encoding encoding)
        {
            uint method = 0xf9;
            this.TrEntry(method, new object[] { encoding });
            ulong num2 = 0L;
            byte[] buffer = new byte[8];
            try
            {
                base.TrText(method, "Encoding used = " + encoding.EncodingName);
                for (int i = 0; i < 8; i++)
                {
                    buffer[i] = this.rfhBytes[this.readCursor + i];
                    this.readCursor += i;
                }
                for (int j = 0; j < 8; j++)
                {
                    num2 = num2 << 8;
                    ulong num5 = buffer[j];
                    if (num5 < 0L)
                    {
                        num5 += (ulong) 0x100L;
                    }
                    num2 += num5;
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return num2;
        }

        private string GetFormatAsString(Encoding encoding)
        {
            string str2;
            uint method = 0xf7;
            this.TrEntry(method, new object[] { encoding });
            byte[] dst = new byte[8];
            try
            {
                base.TrText(method, "Encoding used = " + encoding.EncodingName);
                System.Buffer.BlockCopy(this.rfhBytes, this.readCursor, dst, 0, 8);
                str2 = encoding.GetString(dst, 0, dst.Length).TrimEnd(new char[1]);
            }
            catch (Exception exception)
            {
                base.TrException(method, exception);
                throw;
            }
            finally
            {
                base.TrExit(method);
            }
            return str2;
        }

        private object GetValueAsObject(string dt, string propValue)
        {
            uint method = 250;
            this.TrEntry(method, new object[] { dt, propValue });
            try
            {
                switch (dt)
                {
                    case "string":
                        return propValue;

                    case "i4":
                        return Convert.ToInt32(propValue);

                    case "i2":
                        return Convert.ToInt16(propValue);

                    case "i1":
                        return Convert.ToSByte(propValue);

                    case "i8":
                    case "int":
                        return Convert.ToInt64(propValue);

                    case "r4":
                        return Convert.ToSingle(propValue);

                    case "r8":
                        return Convert.ToDouble(propValue);

                    case "bin.hex":
                        return NmqiTools.HexToBin(propValue);

                    case "boolean":
                        if (!(propValue == "1"))
                        {
                            break;
                        }
                        return true;

                    default:
                        return propValue;
                }
                if (propValue == "0")
                {
                    return false;
                }
                return propValue;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                base.TrExit(method);
            }
            return null;
        }

        private void ProcessAllAvailableRFHs()
        {
            uint method = 0xf6;
            this.TrEntry(method);
            XmlTextReader xmlReader = null;
            string key = null;
            string dt = null;
            string name = null;
            string propValue = null;
            bool flag = true;
            string attribute = null;
            MQPropertyDescriptor pd = null;
            Queue queue = null;
            Queue queue2 = null;
            try
            {
                foreach (string str6 in this.rfhStrings)
                {
                    string str7 = str6;
                    str7 = str7.Replace("xsi:nil", "xsi_ns:nil");
                    xmlReader = new XmlTextReader(new StringReader("<xsiwrapper xmlns:xsi_ns=\"xsi_explicitns\">" + str7 + "</xsiwrapper>"));
                    key = null;
                    dt = null;
                    name = null;
                    propValue = null;
                    flag = true;
                    attribute = null;
                    xmlReader.WhitespaceHandling = WhitespaceHandling.Significant;
                    try
                    {
                        while (xmlReader.Read())
                        {
                            if ((xmlReader.NodeType != XmlNodeType.Element) || (xmlReader.Name != "Root"))
                            {
                                if (xmlReader.NodeType == XmlNodeType.Element)
                                {
                                    if (!flag && (name != null))
                                    {
                                        name = name + '.' + xmlReader.Name;
                                    }
                                    if (flag && (xmlReader.Name != "xsiwrapper"))
                                    {
                                        name = xmlReader.Name;
                                        flag = false;
                                    }
                                }
                                if (xmlReader.HasAttributes)
                                {
                                    dt = xmlReader.GetAttribute("dt");
                                    attribute = xmlReader.GetAttribute("nil", "xsi_explicitns");
                                    if ((attribute != null) && (attribute.ToLower().Trim() == "true"))
                                    {
                                        key = name;
                                        if (!this._receivedProps.Contains(key))
                                        {
                                            this._receivedProps[key] = null;
                                        }
                                        continue;
                                    }
                                    pd = new MQPropertyDescriptor();
                                    this.CheckForPropertyDescriptorAttributes(ref pd, ref xmlReader);
                                    if (key == null)
                                    {
                                        key = name;
                                    }
                                    if (key != null)
                                    {
                                        if (this.mqMessage.propertiesPDTable.ContainsKey(key))
                                        {
                                            queue = (Queue) this.mqMessage.propertiesPDTable[key];
                                            if (queue != null)
                                            {
                                                queue.Enqueue(pd);
                                            }
                                            this.mqMessage.propertiesPDTable[key] = queue;
                                        }
                                        else
                                        {
                                            queue = new Queue();
                                            queue.Enqueue(pd);
                                            this.mqMessage.propertiesPDTable.Add(key, queue);
                                        }
                                    }
                                    switch (dt)
                                    {
                                        case null:
                                        case "":
                                            dt = "string";
                                            break;
                                    }
                                }
                                if (xmlReader.HasValue)
                                {
                                    key = name;
                                    propValue = xmlReader.Value;
                                    if (!this._receivedProps.Contains(key))
                                    {
                                        queue2 = new Queue();
                                        queue2.Enqueue(this.GetValueAsObject(dt, propValue));
                                        this._receivedProps.Add(key, queue2);
                                    }
                                    else
                                    {
                                        queue2 = (Queue) this._receivedProps[key];
                                        if (queue2 != null)
                                        {
                                            queue2.Enqueue(this.GetValueAsObject(dt, propValue));
                                        }
                                        this._receivedProps[key] = queue2;
                                    }
                                    key = null;
                                    dt = null;
                                }
                                if ((xmlReader.NodeType == XmlNodeType.EndElement) && (xmlReader.Name != "xsiwrapper"))
                                {
                                    int num2 = name.LastIndexOf('.');
                                    if (num2 == -1)
                                    {
                                        if (name != xmlReader.Name)
                                        {
                                            MQException exception = new MQException(2, 0x975);
                                            throw exception;
                                        }
                                        flag = true;
                                        name = null;
                                    }
                                    else
                                    {
                                        if (xmlReader.Name != name.Substring(num2 + 1))
                                        {
                                            MQException exception2 = new MQException(2, 0x975);
                                            throw exception2;
                                        }
                                        name = name.Substring(0, name.LastIndexOf('.'));
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exception3)
                    {
                        base.TrException(method, exception3);
                        base.TrText(method, "We are not sucessful in parsing one of theRFH2Header.Raise the RFH_FORMAT exception and breakfurther processing in loop");
                        base.throwNewMQException(2, 0x975);
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal byte[] ProcessMessageForRFH()
        {
            byte[] buffer;
            uint method = 0xf4;
            this.TrEntry(method);
            if (this.rfhBytes == null)
            {
                base.TrExit(method, this.rfhBytes, 1);
                return this.rfhBytes;
            }
            if ((this.rfhBytes != null) && (this.rfhBytes.Length < 0x24))
            {
                base.TrExit(method, this.rfhBytes, 2);
                return this.rfhBytes;
            }
            int count = 0;
            string format = null;
            try
            {
                int encoding = this.mqMessage.Encoding;
                int characterSet = this.mqMessage.CharacterSet;
                ulong formatAsLong = this.GetFormatAsLong(MQCcsidTable.GetDotnetEncoding(characterSet));
                format = this.mqMessage.Format;
                while (((formatAsLong & 18446742974197923840L) == 0x4d51480000000000L) || ((formatAsLong & 18446742974197923840L) == 15337228433335779328L))
                {
                    int num8 = 0;
                    int readCursor = this.readCursor;
                    this.readCursor += 8;
                    num8 += 8;
                    count = this.ReadInt(encoding);
                    this.readCursor += 4;
                    num8 += 4;
                    int num3 = this.ReadInt(encoding);
                    this.readCursor += 4;
                    num8 += 4;
                    int num4 = this.ReadInt(encoding);
                    this.readCursor += 4;
                    num8 += 4;
                    RFHHeader header = new RFHHeader();
                    header.hdrCcsid = characterSet;
                    header.hdrEncoding = encoding;
                    header.hdrFormatAsLong = formatAsLong;
                    header.hdrFormatAsString = format;
                    header.hdrLength = count;
                    header.hdrData = new byte[count];
                    System.Buffer.BlockCopy(this.rfhBytes, readCursor, header.hdrData, 0, count);
                    this.rfhHeaderList.Add(header);
                    format = this.GetFormatAsString(MQCcsidTable.GetDotnetEncoding(characterSet));
                    base.TrText(method, "Format of Next Header " + format);
                    this.mqMessage.Format = format;
                    num8 += 8;
                    this.readCursor += 8;
                    formatAsLong = this.GetFormatAsLong(MQCcsidTable.GetDotnetEncoding(characterSet));
                    int num10 = count - num8;
                    this.readCursor += num10;
                    encoding = num3;
                    this.mqMessage.Encoding = encoding;
                    characterSet = num4;
                    this.mqMessage.CharacterSet = num4;
                }
                int i = this.mqMessage.Encoding;
                int num12 = this.mqMessage.CharacterSet;
                string src = this.mqMessage.Format;
                if (this.rfhHeaderList.Count > 0)
                {
                    this.ConvertBytesContentToString();
                }
                if (this.rfhStrings.Count > 0)
                {
                    this.ProcessAllAvailableRFHs();
                    this.mqMessage.properties = this._receivedProps;
                    this._receivedProps = null;
                }
                int num13 = this.dataLength - this.readCursor;
                if (num13 < 0)
                {
                    num13 = 0;
                }
                int num14 = 0;
                for (int j = 0; j < this.rfhHeaderList.Count; j++)
                {
                    RFHHeader header2 = (RFHHeader) this.rfhHeaderList[j];
                    if (header2.includeInMsg)
                    {
                        num14 += header2.hdrLength;
                    }
                }
                num14 += num13;
                buffer = new byte[num14];
                int dstOffset = 0;
                bool flag = true;
                for (int k = 0; k < this.rfhHeaderList.Count; k++)
                {
                    RFHHeader header3 = (RFHHeader) this.rfhHeaderList[k];
                    if (header3.includeInMsg)
                    {
                        base.TrText(method, "Including rfh header in message which is at position : " + k);
                        base.TrText(method, "Header Length : " + header3.hdrLength);
                        if (flag)
                        {
                            this.mqMessage.Encoding = header3.hdrEncoding;
                            this.mqMessage.CharacterSet = header3.hdrCcsid;
                            this.mqMessage.Format = header3.hdrFormatAsString;
                            flag = false;
                        }
                        System.Buffer.BlockCopy(header3.hdrData, 0, buffer, dstOffset, header3.hdrLength);
                        base.TrText(method, string.Concat(new object[] { "Header ", k, " of length ", header3.hdrLength, ", included in message starting position : ", dstOffset }));
                        int num18 = k + 1;
                        while (num18 < this.rfhHeaderList.Count)
                        {
                            RFHHeader header4 = (RFHHeader) this.rfhHeaderList[num18];
                            if (header4.includeInMsg)
                            {
                                base.TrText(method, "Updating next header encoding, ccsid and format to this header");
                                RFH2.InsertIntIntoByteArray(header4.hdrEncoding, buffer, dstOffset + 12, header3.hdrEncoding);
                                RFH2.InsertIntIntoByteArray(header4.hdrCcsid, buffer, dstOffset + 0x10, header3.hdrEncoding);
                                RFH2.InsertStrIntoByteArray(header4.hdrFormatAsString, 8, buffer, dstOffset + 20, header3.hdrCcsid, header3.hdrEncoding);
                                break;
                            }
                            num18++;
                        }
                        if (num18 == this.rfhHeaderList.Count)
                        {
                            base.TrText(method, "No more header present. Updating encoding, ccsid and format of data to this header");
                            RFH2.InsertIntIntoByteArray(i, buffer, dstOffset + 12, header3.hdrEncoding);
                            RFH2.InsertIntIntoByteArray(num12, buffer, dstOffset + 0x10, header3.hdrEncoding);
                            RFH2.InsertStrIntoByteArray(src, 8, buffer, dstOffset + 20, header3.hdrCcsid, header3.hdrEncoding);
                        }
                        dstOffset += header3.hdrLength;
                        continue;
                    }
                    base.TrText(method, "The header processed. Not Including rfh header in message which is at position : " + k);
                    base.TrText(method, "Header Length which is not included : " + header3.hdrLength);
                }
                System.Buffer.BlockCopy(this.rfhBytes, this.readCursor, buffer, dstOffset, this.dataLength - this.readCursor);
                base.TrText(method, string.Concat(new object[] { "Message data copied from position: ", this.readCursor, ", to position : ", dstOffset, ", length : ", this.dataLength - this.readCursor }));
            }
            finally
            {
                base.TrExit(method, 2);
            }
            return buffer;
        }

        public int ReadInt(int encoding)
        {
            int num3;
            uint method = 0xfb;
            this.TrEntry(method, new object[] { encoding });
            int num2 = 0;
            byte[] buffer = new byte[4];
            try
            {
                buffer[0] = this.rfhBytes[this.readCursor];
                buffer[1] = this.rfhBytes[this.readCursor + 1];
                buffer[2] = this.rfhBytes[this.readCursor + 2];
                buffer[3] = this.rfhBytes[this.readCursor + 3];
                switch ((encoding & 15))
                {
                    case 0:
                    case 2:
                        num2 = BitConverter.ToInt32(buffer, 0);
                        break;

                    case 1:
                        Array.Reverse(buffer);
                        num2 = BitConverter.ToInt32(buffer, 0);
                        break;

                    default:
                        base.TrText(method, "Invalid encoding : " + (encoding & 15));
                        base.throwNewMQException(2, 0x17da);
                        break;
                }
                num3 = num2;
            }
            finally
            {
                base.TrExit(method);
            }
            return num3;
        }

        public int ReadInt(int encoding, byte[] bytes, int offset)
        {
            int num3;
            uint method = 0xfc;
            this.TrEntry(method, new object[] { encoding, bytes, offset });
            int num2 = 0;
            byte[] buffer = new byte[4];
            try
            {
                buffer[0] = bytes[offset];
                buffer[1] = bytes[offset + 1];
                buffer[2] = bytes[offset + 2];
                buffer[3] = bytes[offset + 3];
                switch ((encoding & 15))
                {
                    case 0:
                    case 2:
                        num2 = BitConverter.ToInt32(buffer, 0);
                        break;

                    case 1:
                        Array.Reverse(buffer);
                        num2 = BitConverter.ToInt32(buffer, 0);
                        break;

                    default:
                        base.TrText(method, "Invalid encoding : " + (encoding & 15));
                        base.throwNewMQException(2, 0x17da);
                        break;
                }
                num3 = num2;
            }
            finally
            {
                base.TrExit(method);
            }
            return num3;
        }

        internal class RFHHeader
        {
            internal int hdrCcsid;
            internal byte[] hdrData;
            internal int hdrEncoding;
            internal ulong hdrFormatAsLong;
            internal string hdrFormatAsString;
            internal int hdrLength;
            internal bool includeInMsg = true;
        }
    }
}

