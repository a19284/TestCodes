namespace IBM.WMQ
{
    using System;
    using System.Collections;
    using System.Text;

    internal class MQMarshalMessageForPut : MQBaseObject, IDisposable
    {
        private byte[] body;
        private const int CCSID_UTF8 = 0x4b8;
        private const string DATA_NULL_VALUE = " xsi:nil='true'>";
        private const string DATATYPE_BOOL = " dt='boolean'>";
        private const string DATATYPE_BYTE = " dt='i1'>";
        private const string DATATYPE_BYTEARRAY = " dt='bin.hex'>";
        private const string DATATYPE_CHAR = " dt='char'>";
        private const string DATATYPE_DOUBLE = " dt='r8'>";
        private const string DATATYPE_FLOAT = " dt='r4'>";
        private const string DATATYPE_INT = " dt='i4'>";
        private const string DATATYPE_LONG = " dt='i8'>";
        private const string DATATYPE_SHORT = " dt='i2'>";
        private const string DATATYPE_STRING = " dt='string'>";
        private const string ESC_SEQUENCE_AMP = "&amp;";
        private const string ESC_SEQUENCE_APOS = "&apos;";
        private const string ESC_SEQUENCE_GT = "&gt;";
        private const string ESC_SEQUENCE_LT = "&lt;";
        private const string ESC_SEQUENCE_QUOT = "&quot;";
        private const string FOLDER_JMS_EMPTY = "<jms></jms>";
        private const string FOLDER_JMS_END = "</jms>";
        private const string FOLDER_JMS_NAME = "jms";
        private const string FOLDER_JMS_START = "<jms>";
        private const string FOLDER_MCD_EMPTY = "<mcd></mcd>";
        private const string FOLDER_MCD_END = "</mcd>";
        private const string FOLDER_MCD_NAME = "mcd";
        private const string FOLDER_MCD_START = "<mcd>";
        private const string ID_STRING = "ID:";
        private const int INT_SIZEOF = 4;
        private MQMessage mqMessage;
        private const char MQPD_OPTIONS_SEPARATOR = ',';
        private byte[] msgDataforSend;
        private const string PROP_CORRELATIONID_END = "</Cid>";
        private const string PROP_CORRELATIONID_NAME = "Cid";
        private const string PROP_CORRELATIONID_START = "<Cid>";
        private const string PROP_DELIVERYMODE_END = "</Dlv>";
        private const string PROP_DELIVERYMODE_NAME = "Dlv";
        private const string PROP_DELIVERYMODE_START = "<Dlv>";
        private const string PROP_DESTINATION_END = "</Dst>";
        private const string PROP_DESTINATION_NAME = "Dst";
        private const string PROP_DESTINATION_START = "<Dst>";
        private const string PROP_EXPIRATION_END = "</Exp>";
        private const string PROP_EXPIRATION_NAME = "Exp";
        private const string PROP_EXPIRATION_START = "<Exp>";
        private const string PROP_FMT_END = "</Fmt>";
        private const string PROP_FMT_NAME = "Fmt";
        private const string PROP_FMT_START = "<Fmt>";
        private const string PROP_GROUPID_END = "</Gid>";
        private const string PROP_GROUPID_NAME = "Gid";
        private const string PROP_GROUPID_START = "<Gid>";
        private const string PROP_GROUPSEQ_END = "</Seq>";
        private const string PROP_GROUPSEQ_NAME = "Seq";
        private const string PROP_GROUPSEQ_START = "<Seq>";
        private const string PROP_PRIORITY_END = "</Pri>";
        private const string PROP_PRIORITY_NAME = "Pri";
        private const string PROP_PRIORITY_START = "<Pri>";
        private const string PROP_REPLYTO_END = "</Rto>";
        private const string PROP_REPLYTO_NAME = "Rto";
        private const string PROP_REPLYTO_START = "<Rto>";
        private const string PROP_SET_END = "</Set>";
        private const string PROP_SET_NAME = "Set";
        private const string PROP_SET_START = "<Set>";
        private const string PROP_TIMESTAMP_END = "</Tms>";
        private const string PROP_TIMESTAMP_NAME = "Tms";
        private const string PROP_TIMESTAMP_START = "<Tms>";
        private const string PROP_TYPE_END = "</Type>";
        private const string PROP_TYPE_NAME = "Type";
        private const string PROP_TYPE_START = "<Type>";
        private Hashtable properties;
        private RFH2 rfh = new RFH2();
        private RFH2Folder rfhFolders;
        private ArrayList rfhHeaders = new ArrayList(10);
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private byte[] SPACES = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
        private int writeCursor;
        private const string XML_ELEMENT_END_TAG = "</";
        private const string XML_END_TAG = ">";
        private const string XML_START_TAG = "<";

        internal MQMarshalMessageForPut(MQMessage mqMsg)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { mqMsg });
            this.mqMessage = mqMsg;
            this.body = this.mqMessage.GetBuffer();
            this.msgDataforSend = new byte[this.body.Length];
            if (this.mqMessage.properties.Count > 0)
            {
                this.properties = this.mqMessage.properties;
            }
            this.rfhFolders = new RFH2Folder();
        }

        private void AppendByteArray(byte[] content)
        {
            uint method = 0x101;
            this.TrEntry(method, new object[] { content });
            for (int i = 0; i < content.Length; i++)
            {
                this.msgDataforSend[this.writeCursor + i] = content[i];
            }
            this.writeCursor += content.Length;
            base.TrExit(method);
        }

        private void AppendByteArray(byte[] content, int offset, int length)
        {
            uint method = 0x102;
            this.TrEntry(method, new object[] { content, offset, length });
            for (int i = offset; i < length; i++)
            {
                this.msgDataforSend[this.writeCursor + i] = content[i];
            }
            this.writeCursor += length;
            base.TrExit(method);
        }

        private void AppendInt(int v, int enc)
        {
            uint method = 0x103;
            this.TrEntry(method, new object[] { v, enc });
            byte[] content = null;
            try
            {
                switch ((enc & 15))
                {
                    case 0:
                    case 2:
                        content = BitConverter.GetBytes(v);
                        this.AppendByteArray(content);
                        return;

                    case 1:
                        content = BitConverter.GetBytes(v);
                        Array.Reverse(content);
                        this.AppendByteArray(content);
                        return;
                }
                base.TrText(method, "Unsupported encoding: " + (v & 15));
                base.throwNewMQException(2, 0x17da);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal MQMessage ConstructMessageForSend()
        {
            uint method = 0xfe;
            this.TrEntry(method);
            if (this.properties == null)
            {
                base.TrExit(method, this.mqMessage, 1);
                return this.mqMessage;
            }
            try
            {
                this.mqMessage.ClearMessage();
                this.WriteRFH2Properties();
                if ((this.body != null) & (this.body.Length > 0))
                {
                    this.AppendByteArray(this.body);
                }
                this.writeCursor = this.msgDataforSend.Length;
                this.mqMessage.ClearMessage();
                this.mqMessage.Format = "MQHRF2  ";
                this.mqMessage.CharacterSet = 0x4b8;
                base.TrData(method, 2, "RFH2(fixed & variable) and payload", this.msgDataforSend);
                this.mqMessage.Write(this.msgDataforSend, 0, this.writeCursor);
            }
            finally
            {
                base.TrExit(method, 1);
            }
            this.mqMessage.properties = this.properties;
            return this.mqMessage;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public void Dispose(bool fromDispose)
        {
            this.mqMessage = null;
            this.properties = null;
            this.body = null;
            this.msgDataforSend = null;
            if (this.rfhFolders != null)
            {
                this.rfhFolders.Dispose(false);
            }
            if (this.rfh != null)
            {
                this.rfh.Dispose(false);
            }
            this.rfhHeaders = null;
            if (!fromDispose)
            {
                GC.SuppressFinalize(this);
            }
        }

        private string EscapeXMLChars(StringBuilder output, string input)
        {
            uint method = 0x105;
            this.TrEntry(method, new object[] { output, input });
            try
            {
                char[] chArray = input.ToCharArray();
                for (int i = 0; i < chArray.Length; i++)
                {
                    char number = chArray[i];
                    if ('<' == number)
                    {
                        output.Append("&lt;");
                    }
                    else if ('>' == number)
                    {
                        output.Append("&gt;");
                    }
                    else if ('&' == number)
                    {
                        output.Append("&amp;");
                    }
                    else if ('"' == number)
                    {
                        output.Append("&quot;");
                    }
                    else if ('\'' == number)
                    {
                        output.Append("&apos;");
                    }
                    else if ((0xd800 <= number) && (number < 0xdc00))
                    {
                        int num3 = 0;
                        if ((i + 1) >= chArray.Length)
                        {
                            string text1 = IntToHex(number) + " ?";
                        }
                        else
                        {
                            i++;
                            num3 = chArray[i];
                            if ((0xdc00 > num3) || (num3 >= 0xe000))
                            {
                                string text2 = IntToHex(number) + " " + IntToHex(num3);
                            }
                            num3 = ((((number - 0xd800) << 10) + num3) - 0xdc00) + 0x10000;
                        }
                        output.Append("&#x");
                        output.Append(IntToHex(num3));
                        output.Append(";");
                    }
                    else
                    {
                        output.Append(number);
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return output.ToString();
        }

        private void ExpandMessageData(int newSize)
        {
            uint method = 0x100;
            this.TrEntry(method, new object[] { newSize });
            byte[] buffer = new byte[newSize];
            for (int i = 0; i < this.writeCursor; i++)
            {
                buffer[i] = this.msgDataforSend[i];
            }
            this.msgDataforSend = buffer;
            base.TrExit(method);
        }

        private static string IntToHex(int number)
        {
            byte[] bin = new byte[] { (byte) (number >> 0x18), (byte) (number >> 0x10), (byte) (number >> 8), (byte) number };
            StringBuilder hex = new StringBuilder();
            StringBuilder builder2 = new StringBuilder();
            MQMessage.BinToHex(bin, 0, bin.Length, hex);
            bool flag = false;
            foreach (char ch in hex.ToString().ToCharArray())
            {
                if (flag || (ch != '0'))
                {
                    flag = true;
                    builder2.Append(ch);
                }
            }
            return builder2.ToString();
        }

        private void SetContent(RFH2Folder element, object value)
        {
            uint method = 260;
            this.TrEntry(method, new object[] { element, value });
            try
            {
                StringBuilder output = new StringBuilder(10);
                if (value is string)
                {
                    element.SetContent(this.EscapeXMLChars(output, (string) value), 0);
                }
                else if (value is int)
                {
                    element.SetContent(value.ToString(), 5);
                }
                else if (value is short)
                {
                    element.SetContent(value.ToString(), 4);
                }
                else if (value is sbyte)
                {
                    element.SetContent(value.ToString(), 3);
                }
                else if (value is byte)
                {
                    element.SetContent(((sbyte) ((byte) value)).ToString(), 3);
                }
                else if (value is long)
                {
                    element.SetContent(value.ToString(), 6);
                }
                else if (value is float)
                {
                    element.SetContent(value.ToString(), 8);
                }
                else if (value is double)
                {
                    element.SetContent(value.ToString(), 9);
                }
                else
                {
                    if (value is byte[])
                    {
                        try
                        {
                            StringBuilder hex = new StringBuilder();
                            MQMessage.BinToHex((byte[]) value, 0, ((byte[]) value).Length, hex);
                            element.SetContent(hex.ToString(), 2);
                            return;
                        }
                        catch (Exception exception)
                        {
                            base.TrException(method, exception, 1);
                            throw exception;
                        }
                    }
                    if (value is bool)
                    {
                        element.SetContent(value.ToString(), 1);
                    }
                    else if (value == null)
                    {
                        element.SetContent(null);
                    }
                    else
                    {
                        element.SetContent(this.EscapeXMLChars(output, value.ToString()), 0);
                    }
                }
            }
            catch (Exception)
            {
                MQException ex = new MQException(2, 0x9a2);
                base.TrException(method, ex);
                throw ex;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void SetPropertyDescriptor(RFH2Folder element, MQPropertyDescriptor pd)
        {
            uint method = 0x107;
            this.TrEntry(method, new object[] { element, pd });
            int cc = 0;
            int rc = 0;
            try
            {
                if (element != null)
                {
                    if (element.name.ToLower().Equals("mq"))
                    {
                        byte[] bytes = BitConverter.GetBytes(pd.Support);
                        Array.Reverse(bytes);
                        StringBuilder builder = new StringBuilder(bytes.Length);
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            builder.Append(bytes[i].ToString("X2"));
                        }
                        element.support = builder.ToString();
                    }
                    if (pd.Context == 0)
                    {
                        element.context = "none";
                    }
                    else if (pd.Context == 1)
                    {
                        element.context = "user";
                    }
                    else
                    {
                        cc = 2;
                        rc = 0x9b2;
                        base.throwNewMQException(cc, rc);
                    }
                    int copyOptions = pd.CopyOptions;
                    string str = null;
                    if ((copyOptions & 0x16) != 0x16)
                    {
                        if (copyOptions == 0)
                        {
                            str = "none";
                        }
                        else if ((copyOptions & 1) != 0)
                        {
                            str = "All";
                        }
                        else
                        {
                            if ((copyOptions & 2) != 0)
                            {
                                if (str == null)
                                {
                                    str = "forward";
                                }
                                else
                                {
                                    str = str + ',' + "forward";
                                }
                            }
                            if ((copyOptions & 4) != 0)
                            {
                                if (str == null)
                                {
                                    str = "publish";
                                }
                                else
                                {
                                    str = str + ',' + "publish";
                                }
                            }
                            if ((copyOptions & 0x10) != 0)
                            {
                                if (str == null)
                                {
                                    str = "report";
                                }
                                else
                                {
                                    str = str + ',' + "report";
                                }
                            }
                            if ((copyOptions & 8) != 0)
                            {
                                if (str == null)
                                {
                                    str = "reply";
                                }
                                else
                                {
                                    str = str + ',' + "reply";
                                }
                            }
                        }
                    }
                    element.copyOption = str;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void WriteRFH2Properties()
        {
            uint method = 0xff;
            this.TrEntry(method);
            try
            {
                int num5;
                int length = 0;
                int num3 = 0;
                string foldername = null;
                bool flag = false;
                RFH2Folder folder = null;
                if (this.properties != null)
                {
                    IEnumerator enumerator = this.properties.Keys.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        string current = (string) enumerator.Current;
                        Queue queue = (Queue) ((Queue) this.properties[current]).Clone();
                        Queue queue2 = (Queue) this.mqMessage.propertiesPDTable[current];
                        MQPropertyDescriptor pd = new MQPropertyDescriptor();
                        length = current.IndexOf('.');
                        if (length > 0)
                        {
                            foldername = current.Substring(0, length);
                            if (!this.rfh.Contains(foldername))
                            {
                                RFH2Folder child = new RFH2Folder(foldername);
                                this.rfhFolders.AddFolder(child);
                                this.rfh.AddFolder(child);
                            }
                            flag = true;
                            current = current.Substring(length + 1);
                        }
                        else
                        {
                            if (!this.rfh.Contains("usr"))
                            {
                                RFH2Folder folder3 = new RFH2Folder("usr");
                                this.rfhFolders.AddFolder(folder3);
                                this.rfh.AddFolder(folder3);
                            }
                            RFH2Folder folder4 = this.rfh.GetFolder("usr");
                            if (queue != null)
                            {
                                while (queue.Count > 0)
                                {
                                    RFH2Folder element = new RFH2Folder(current);
                                    if ((queue2 != null) && (queue2.Count > 0))
                                    {
                                        pd = (MQPropertyDescriptor) queue2.Dequeue();
                                        if (pd != null)
                                        {
                                            this.SetPropertyDescriptor(element, pd);
                                        }
                                    }
                                    this.SetContent(element, queue.Dequeue());
                                    folder4.AddFolder(element);
                                }
                                folder4 = null;
                            }
                            else
                            {
                                RFH2Folder folder6 = new RFH2Folder(current);
                                if ((queue2 != null) && (queue2.Count > 0))
                                {
                                    pd = (MQPropertyDescriptor) queue2.Dequeue();
                                    if (pd != null)
                                    {
                                        this.SetPropertyDescriptor(folder6, pd);
                                    }
                                }
                                this.SetContent(folder6, null);
                                folder4.AddFolder(folder6);
                                folder4 = null;
                            }
                        }
                        while (flag)
                        {
                            length = current.IndexOf('.');
                            if (folder == null)
                            {
                                folder = this.rfh.GetFolder(foldername);
                            }
                            if (length == -1)
                            {
                                foldername = current;
                                if (queue != null)
                                {
                                    while (queue.Count > 0)
                                    {
                                        RFH2Folder folder7 = new RFH2Folder(foldername);
                                        if ((queue2 != null) && (queue2.Count > 0))
                                        {
                                            pd = (MQPropertyDescriptor) queue2.Dequeue();
                                            if (pd != null)
                                            {
                                                this.SetPropertyDescriptor(folder7, pd);
                                            }
                                        }
                                        this.SetContent(folder7, queue.Dequeue());
                                        folder.AddFolder(folder7);
                                    }
                                    folder = null;
                                }
                                else
                                {
                                    RFH2Folder folder8 = new RFH2Folder(foldername);
                                    if ((queue2 != null) && (queue2.Count > 0))
                                    {
                                        pd = (MQPropertyDescriptor) queue2.Dequeue();
                                        if (pd != null)
                                        {
                                            this.SetPropertyDescriptor(folder8, pd);
                                        }
                                    }
                                    this.SetContent(folder8, null);
                                    folder.AddFolder(folder8);
                                    folder = null;
                                }
                                continue;
                            }
                            foldername = current.Substring(0, length);
                            current = current.Substring(length + 1);
                            if (!folder.Contains(foldername) && (folder.name != foldername))
                            {
                                RFH2Folder folder9 = new RFH2Folder(foldername);
                                folder.AddFolder(folder9);
                                folder = folder9;
                            }
                            else if (folder.name != foldername)
                            {
                                folder = folder.GetFolder(foldername);
                            }
                        }
                    }
                }
                ArrayList list = new ArrayList(10);
                byte[] buf = null;
                int num6 = 4;
                Encoding dotnetEncoding = MQCcsidTable.GetDotnetEncoding(0x4b8);
                foreach (RFH2Folder folder11 in this.rfh.folders)
                {
                    string s = folder11.Render();
                    buf = dotnetEncoding.GetBytes(s);
                    int num4 = buf.Length;
                    num5 = 3 - ((buf.Length - 1) % num6);
                    num4 += num5;
                    num3 += num4 + num6;
                    base.TrData(method, 2, "RFH2 XML(variable part) Content", buf);
                    list.Add(buf);
                }
                int i = num3 + 0x24;
                int encoding = this.mqMessage.Encoding;
                int characterSet = this.mqMessage.CharacterSet;
                string format = this.mqMessage.Format;
                int off = 0;
                byte[] dst = new byte[0x24];
                RFH2.InsertStrIntoByteArray("RFH ", 4, dst, 0, 0x4b8, encoding);
                off += "RFH ".Length;
                RFH2.InsertIntIntoByteArray(2, dst, off, encoding);
                off += 4;
                RFH2.InsertIntIntoByteArray(i, dst, off, encoding);
                off += 4;
                RFH2.InsertIntIntoByteArray(encoding, dst, off, encoding);
                off += 4;
                RFH2.InsertIntIntoByteArray(characterSet, dst, off, encoding);
                off += 4;
                RFH2.InsertStrIntoByteArray(format, 8, dst, off, 0x4b8, encoding);
                off += 8;
                RFH2.InsertIntIntoByteArray(0, dst, off, encoding);
                off += 4;
                RFH2.InsertIntIntoByteArray(0x4b8, dst, off, encoding);
                this.mqMessage.Format = "MQHRF2  ";
                this.ExpandMessageData((num3 + this.body.Length) + 0x24);
                this.AppendByteArray(dst);
                foreach (object obj2 in list)
                {
                    buf = (byte[]) obj2;
                    num5 = 3 - ((buf.Length - 1) % num6);
                    this.AppendInt(buf.Length, this.mqMessage.Encoding);
                    this.AppendByteArray(buf);
                    this.AppendByteArray(this.SPACES, 0, num5);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void WriteTriplet(StringBuilder output, string name, object value, bool newStyle)
        {
            uint method = 0x106;
            this.TrEntry(method, new object[] { output, name, value, newStyle });
            try
            {
                output.Append("<");
                if (newStyle)
                {
                    output.Append("elt name=\"" + name + "\"");
                }
                else
                {
                    output.Append(name);
                }
                if (value is string)
                {
                    output.Append(">");
                    this.EscapeXMLChars(output, (string) value);
                }
                else if (value is int)
                {
                    output.Append(" dt='i4'>");
                    output.Append((int) value);
                }
                else if (value is short)
                {
                    output.Append(" dt='i2'>");
                    output.Append((short) value);
                }
                else if (value is sbyte)
                {
                    output.Append(" dt='i1'>");
                    output.Append((sbyte) value);
                }
                else if (value is byte)
                {
                    output.Append(" dt='i1'>");
                    output.Append((sbyte) ((byte) value));
                }
                else if (value is long)
                {
                    output.Append(" dt='i8'>");
                    output.Append((long) value);
                }
                else if (value is float)
                {
                    output.Append(" dt='r4'>");
                    output.Append((float) value);
                }
                else if (value is double)
                {
                    output.Append(" dt='r8'>");
                    output.Append((double) value);
                }
                else
                {
                    if (value is byte[])
                    {
                        output.Append(" dt='bin.hex'>");
                        try
                        {
                            StringBuilder hex = new StringBuilder();
                            MQMessage.BinToHex((byte[]) value, 0, ((byte[]) value).Length, hex);
                            output.Append(hex.ToString());
                            goto Label_025A;
                        }
                        catch (Exception exception)
                        {
                            base.TrException(method, exception);
                            throw exception;
                        }
                    }
                    if (value is bool)
                    {
                        output.Append(" dt='boolean'>");
                        output.Append(((bool) value) ? "1" : "0");
                    }
                    else if (value is char)
                    {
                        output.Append(" dt='char'>");
                        this.EscapeXMLChars(output, value.ToString());
                    }
                    else if (value == null)
                    {
                        output.Append(" xsi:nil='true'>");
                    }
                    else
                    {
                        output.Append("<");
                        this.EscapeXMLChars(output, value.ToString());
                    }
                }
            Label_025A:
                output.Append("</");
                if (newStyle)
                {
                    output.Append("elt");
                }
                else
                {
                    output.Append(name);
                }
                output.Append(">");
            }
            finally
            {
                base.TrExit(method);
            }
        }
    }
}

