namespace IBM.WMQ
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;

    public class MQMessage : MQBaseObject
    {
        private static ArrayList ALLOWED_HIERARCHY_PROPERTY_NAMES = new ArrayList(new string[] { "Root" + '.' + "MQMD" });
        private static char[] BIN2HEX = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
        internal BinaryReader binaryReader;
        internal BinaryWriter binaryWriter;
        private const int BOOLEAN_SIZEOF = 1;
        private const int BYTE_SIZEOF = 1;
        private const int CHAR_SIZEOF = 2;
        private static CultureInfo dateTimeCulture = new CultureInfo("en-US");
        private const int DOUBLE_SIZEOF = 8;
        private const int EXPAND_SIZE = 0x400;
        private const int FLOAT_SIZEOF = 4;
        internal const string FORWARD_SLASH = "/";
        private const int IEEE_SIGN_BIT = 0x80;
        private const int INT_SIZEOF = 4;
        private const int LONG_SIZEOF = 8;
        private static string mcdURI = "mcd://";
        internal MQMessageDescriptor md;
        internal MemoryStream memoryStream;
        private const string MQ_JMS_CORRELATION_ID = "JMSCorrelationID";
        private const string MQ_JMS_DELIVERY_MODE = "JMSDeliveryMode";
        private const string MQ_JMS_DESTINATION = "JMSDestination";
        private const string MQ_JMS_EXPIRATION = "JMSExpiration";
        private const string MQ_JMS_MESSAGE_ID = "JMSMessageID";
        private const string MQ_JMS_MSG_CLASS_BYTES = "jms_bytes";
        private const string MQ_JMS_MSG_CLASS_TEXT = "jms_text";
        private const string MQ_JMS_PRIORITY = "JMSPriority";
        private const string MQ_JMS_REDELIVERED = "JMSRedelivered";
        private const string MQ_JMS_REPLY_TO = "JMSReplyTo";
        private const string MQ_JMS_TIME_STAMP = "JMSTimestamp";
        private const string MQ_JMS_TYPE = "JMSType";
        private const string MQ_JMSX_APP_ID = "JMSXAppID";
        private const string MQ_JMSX_DELIVERY_COUNT = "JMSXDeliveryCount";
        private const string MQ_JMSX_GROUP_ID = "JMSXGroupID";
        private const string MQ_JMSX_GROUP_SEQ = "JMSXGroupSeq";
        private const string MQ_JMSX_USER_ID = "JMSXUserID";
        internal const string MQ_PD_CONTEXT = "context";
        internal const string MQ_PD_CONTEXT_NONE = "none";
        internal const string MQ_PD_CONTEXT_USER = "user";
        internal const string MQ_PD_COPY = "copy";
        internal const string MQ_PD_COPY_ALL = "All";
        internal const string MQ_PD_COPY_DEFAULT = "default";
        internal const string MQ_PD_COPY_FORWARD = "forward";
        internal const string MQ_PD_COPY_NONE = "none";
        internal const string MQ_PD_COPY_PUBLISH = "publish";
        internal const string MQ_PD_COPY_REPLY = "reply";
        internal const string MQ_PD_COPY_REPORT = "report";
        internal const string MQ_PD_SUPPORT = "support";
        private const string MQMD_PROPERTY_FORMAT = "format";
        private static Hashtable mqPropertyNames_ = new Hashtable();
        private const string MSGPROPS_MQMD_SYNTAX = "Root.MQMD.";
        private static Hashtable namedProperties = null;
        internal Hashtable properties;
        internal static ArrayList PROPERTIES_FOLDER_NAMES = new ArrayList(new string[] { "mq", "mq_usr", "mqext", "jms", "mcd", "usr", "sib", "sib_usr", "sib_context", "mqema", "mqps" });
        internal Hashtable propertiesPDTable;
        internal const string PROPERTY_WILDCARD = "%";
        protected int propertyValidation;
        private static string replyToURI = "queue://";
        private static string[] RESERVED_HIERARCHY_PROPERTY_NAME_PREFIXES = new string[] { ("Body" + '.'), ("Root" + '.') };
        private static string[] reservedKeys = new string[] { "NULL", "TRUE", "FALSE", "NOT", "AND", "OR", "BETWEEN", "LIKE", "IN", "IS", "ESCAPE" };
        internal static ArrayList RESTRICTED_HIERARCHY_FOLDER_NAMES = new ArrayList(new string[] { "mq", "jms", "mcd", "usr", "sib" });
        internal const char RFH2_FOLDER_SEPARATOR = '.';
        private const string RFH2_JMS_CORREL_ID = "jms.Cid";
        private const string RFH2_JMS_DELIVERY_MODE = "jms.Dlv";
        private const string RFH2_JMS_DESTINATION = "jms.Dst";
        private const string RFH2_JMS_EXPIRATION = "jms.Exp";
        internal const string RFH2_JMS_FOLDER = "jms";
        private const string RFH2_JMS_GROUP_ID = "jms.Gid";
        private const string RFH2_JMS_MCD_FMT = "mcd.Fmt";
        private const string RFH2_JMS_MCD_MSD = "mcd.Msd";
        private const string RFH2_JMS_MCD_SET = "mcd.Set";
        private const string RFH2_JMS_MCD_TYPE = "mcd.Type";
        private const string RFH2_JMS_PRIORITY = "jms.Pri";
        internal const string RFH2_JMS_PROPERTY_PREFIX = "JMS";
        private const string RFH2_JMS_REPLY_TO = "jms.Rto";
        private const string RFH2_JMS_TIME_STAMP = "jms.Tms";
        private const string RFH2_JMSX_GROUP_SEQ = "jms.Seq";
        internal const string RFH2_MCD_FOLDER = "mcd";
        internal const string RFH2_MQ_FOLDER = "mq";
        internal const string RFH2_USR_FOLDER = "usr";
        private const int S390_EXP_BIAS = 0x40;
        private const int S390_EXP_BITS = 0x7f;
        private const int S390_MANTISSA = 0xffffff;
        private const int S390_NEGATIVE = 0x80;
        private const int S390_POSITIVE = 0;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private const int SHORT_SIZEOF = 2;
        internal bool spiInherited;
        internal ulong spiQTime;
        internal int totalMessageLength;
        private const int UNBYTE_SIZEOF = 2;
        private const int UNSHORT_SIZEOF = 4;
        private const int UTF_SIZEOF = 2;

        static MQMessage()
        {
            mqPropertyNames_["Root.MQMD.Report"] = true;
            mqPropertyNames_["Root.MQMD.MessageType"] = true;
            mqPropertyNames_["Root.MQMD.Expiry"] = true;
            mqPropertyNames_["Root.MQMD.Feedback"] = true;
            mqPropertyNames_["Root.MQMD.Encoding"] = true;
            mqPropertyNames_["Root.MQMD.CharacterSet"] = true;
            mqPropertyNames_["Root.MQMD.Format"] = true;
            mqPropertyNames_["Root.MQMD.Priority"] = true;
            mqPropertyNames_["Root.MQMD.Persistence"] = true;
            mqPropertyNames_["Root.MQMD.MessageId"] = true;
            mqPropertyNames_["Root.MQMD.CorrelationId"] = true;
            mqPropertyNames_["Root.MQMD.BackoutCount"] = true;
            mqPropertyNames_["Root.MQMD.ReplyToQueueName"] = true;
            mqPropertyNames_["Root.MQMD.ReplyToQueueManagerName"] = true;
            mqPropertyNames_["Root.MQMD.UserId"] = true;
            mqPropertyNames_["Root.MQMD.AccountingToken"] = true;
            mqPropertyNames_["Root.MQMD.ApplicationIdData"] = true;
            mqPropertyNames_["Root.MQMD.PutApplicationType"] = true;
            mqPropertyNames_["Root.MQMD.PutApplicationName"] = true;
            mqPropertyNames_["Root.MQMD.PutDateTime"] = true;
            mqPropertyNames_["Root.MQMD.ApplicationOriginData"] = true;
            mqPropertyNames_["Root.MQMD.GroupId"] = true;
            mqPropertyNames_["Root.MQMD.MessageSequenceNumber"] = true;
            mqPropertyNames_["Root.MQMD.OriginalLength"] = true;
            mqPropertyNames_["Root.MQMD.DataOffset"] = true;
            mqPropertyNames_["Root.MQMD.MessageFlags"] = true;
            mqPropertyNames_["Root.MQMD.MessageLength"] = true;
            mqPropertyNames_["Root.MQMD.Offset"] = true;
            mqPropertyNames_["Root.MQMD.TotalMessageLength"] = true;
            mqPropertyNames_["Root.MQMD.DataLength"] = true;
            mqPropertyNames_["JMSCorrelationID"] = true;
            mqPropertyNames_["JMSDeliveryMode"] = true;
            mqPropertyNames_["JMSDestination"] = true;
            mqPropertyNames_["JMSExpiration"] = true;
            mqPropertyNames_["JMSMessageID"] = true;
            mqPropertyNames_["JMSPriority"] = true;
            mqPropertyNames_["JMSReplyTo"] = true;
            mqPropertyNames_["JMSTimestamp"] = true;
            mqPropertyNames_["JMSType"] = true;
            mqPropertyNames_["JMSXAppID"] = true;
            mqPropertyNames_["JMSXDeliveryCount"] = true;
            mqPropertyNames_["JMSXGroupID"] = true;
            mqPropertyNames_["JMSXGroupSeq"] = true;
            mqPropertyNames_["JMSXUserID"] = true;
            if (namedProperties == null)
            {
                namedProperties = new Hashtable();
                namedProperties.Add("jms.Cid", "JMSCorrelationID");
                namedProperties.Add("jms.Dlv", "JMSDeliveryMode");
                namedProperties.Add("jms.Dst", "JMSDestination");
                namedProperties.Add("jms.Exp", "JMSExpiration");
                namedProperties.Add("jms.Pri", "JMSPriority");
                namedProperties.Add("jms.Rto", "JMSReplyTo");
                namedProperties.Add("jms.Tms", "JMSTimestamp");
                namedProperties.Add("mcd.Type", "JMSType");
                namedProperties.Add("mcd.Set", "JMSType");
                namedProperties.Add("mcd.Fmt", "JMSType");
            }
        }

        public MQMessage()
        {
            this.md = new MQMessageDescriptor();
            this.propertiesPDTable = new Hashtable(10);
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.ClearMessage();
            this.md = new MQMessageDescriptor();
            this.properties = new Hashtable(40);
        }

        protected MQMessage(MQMessage msg)
        {
            this.md = new MQMessageDescriptor();
            this.propertiesPDTable = new Hashtable(10);
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { msg });
            this.ClearMessage();
            this.md = new MQMessageDescriptor(msg.md);
        }

        private void AddPropertyToList(string name, MQPropertyDescriptor pd, object value)
        {
            uint method = 0x183;
            this.TrEntry(method, new object[] { name, pd, value });
            Queue queue = null;
            Queue queue2 = null;
            try
            {
                if (name.StartsWith("Root.MQMD."))
                {
                    name = name.Substring(10);
                    if ((name.ToLower() != "version") && (name.ToLower() != "strucid"))
                    {
                        PropertyInfo property = base.GetType().GetProperty(name);
                        if (property == null)
                        {
                            base.throwNewMQException(2, 0x98a);
                        }
                        property.SetValue(this, value, null);
                    }
                }
                else if (name.ToLower().StartsWith("jms"))
                {
                    int index = name.IndexOf('.');
                    if (index > -1)
                    {
                        name = name.Substring(index + 1);
                    }
                    if (!mqPropertyNames_.ContainsKey(name))
                    {
                        base.throwNewMQException(2, 0x98a);
                    }
                    this.SetJmsProperty(name, value);
                }
                else
                {
                    if (name.ToLower().StartsWith("mq"))
                    {
                        name = this.MakeMQFolderExplicit(name);
                    }
                    else
                    {
                        name = this.MakeUsrFolderExplicit(name);
                    }
                    if (this.properties.ContainsKey(name.Trim()))
                    {
                        queue = (Queue) this.properties[name.Trim()];
                        if (queue != null)
                        {
                            queue.Enqueue(value);
                        }
                        this.properties[name.Trim()] = queue;
                    }
                    else
                    {
                        queue = new Queue();
                        queue.Enqueue(value);
                        this.properties[name.Trim()] = queue;
                    }
                    if (pd != null)
                    {
                        if (this.propertiesPDTable.ContainsKey(name))
                        {
                            queue2 = (Queue) this.propertiesPDTable[name];
                            if (queue2 != null)
                            {
                                queue2.Enqueue(pd);
                            }
                            this.propertiesPDTable[name] = queue2;
                        }
                        else
                        {
                            queue2 = new Queue();
                            queue2.Enqueue(pd);
                            this.propertiesPDTable.Add(name, queue2);
                        }
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private Exception BadConvertException(uint funcid)
        {
            uint method = 390;
            this.TrEntry(method, new object[] { funcid });
            MQException ex = new MQException(1, 0x9a2);
            base.TrException(method, ex);
            base.TrExit(method, ex);
            return ex;
        }

        internal static void BinToHex(byte[] bin, int start, int length, StringBuilder hex)
        {
            int num2 = start + length;
            for (int i = start; i < num2; i++)
            {
                int num = bin[i];
                hex.Append(BIN2HEX[num / 0x10]);
                hex.Append(BIN2HEX[num % 0x10]);
            }
        }

        private void CheckIsValidJavaIdentifier(string propertyName)
        {
            uint method = 0x18a;
            this.TrEntry(method, new object[] { propertyName });
            try
            {
                if (!mqPropertyNames_.ContainsKey(propertyName))
                {
                    if ((propertyName == null) || !IsJavaIdentifierStart(propertyName[0]))
                    {
                        base.throwNewMQException(2, 0x98a);
                    }
                    int num2 = 0;
                    int length = propertyName.Length;
                    while (num2 < length)
                    {
                        if (!IsJavaIdentifierPart(propertyName[num2]))
                        {
                            base.throwNewMQException(2, 0x98a);
                        }
                        num2++;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void CheckMaxSizeOfPropertyName(string name)
        {
            uint method = 0x189;
            this.TrEntry(method, new object[] { name });
            try
            {
                if (name.Length > 0xfff)
                {
                    base.throwNewMQException(2, 0x9a1);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void CheckNotMixedContent(string propertyName)
        {
            uint method = 0x18e;
            this.TrEntry(method, new object[] { propertyName });
            try
            {
                IEnumerator enumerator = this.properties.Keys.GetEnumerator();
                if (enumerator != null)
                {
                    while (enumerator.MoveNext())
                    {
                        string current = (string) enumerator.Current;
                        if (propertyName != current)
                        {
                            string str3;
                            string str2 = propertyName + '.';
                            int length = propertyName.LastIndexOf('.');
                            if (length != -1)
                            {
                                str3 = propertyName.Substring(0, length);
                            }
                            else
                            {
                                str3 = propertyName;
                            }
                            if (current.StartsWith(str2) || current.Equals(str3))
                            {
                                base.throwNewMQException(2, 0x9c2);
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

        private void CheckNotNullPropertyName(string name)
        {
            uint method = 0x188;
            this.TrEntry(method, new object[] { name });
            try
            {
                if ((name == null) || (name == ""))
                {
                    base.throwNewMQException(2, 0x98a);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void ClearMessage()
        {
            uint method = 0x109;
            this.TrEntry(method);
            try
            {
                this.memoryStream = new MemoryStream();
                this.memoryStream.SetLength(0L);
                this.totalMessageLength = 0;
                this.spiInherited = false;
                this.spiQTime = 0L;
                this.binaryReader = new BinaryReader(this.memoryStream);
                this.binaryWriter = new BinaryWriter(this.memoryStream);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void DeleteProperty(string name)
        {
            uint method = 410;
            this.TrEntry(method, new object[] { name });
            try
            {
                if (name != null)
                {
                    if (name.StartsWith("mq"))
                    {
                        name = this.MakeMQFolderExplicit(name);
                    }
                    else
                    {
                        name = this.MakeUsrFolderExplicit(name);
                    }
                    if (this.properties.ContainsKey(name))
                    {
                        this.properties.Remove(name);
                    }
                    else if (name.StartsWith("Root.MQMD."))
                    {
                        this.SetMQMDToDefault(name.Substring(0, name.Length - 1));
                    }
                    else
                    {
                        base.throwNewMQException(2, 0x9a7);
                    }
                }
                else
                {
                    base.throwNewMQException(2, 0x98a);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private double frexp(double x, ref int n)
        {
            uint method = 0x146;
            this.TrEntry(method, new object[] { x, (int) n });
            double num2 = 0.0;
            try
            {
                double intptr = 0.0;
                this.modf(x, ref intptr);
                ulong num4 = Convert.ToUInt64(intptr);
                n = 0;
                while ((num4 >> n) > 0L)
                {
                    n++;
                }
                num2 = x / Math.Pow(2.0, (double) n);
            }
            finally
            {
                base.TrExit(method);
            }
            return num2;
        }

        private unsafe int From370fp(byte* pSource, ref double result, uint ulCount)
        {
            uint num = 0;
            double x = 0.0;
            x = this.ldexpl((double) (((pSource[1] << 0x10) + (pSource[2] << 8)) + pSource[3]), 0x20);
            if (ulCount == 8)
            {
                x += (((pSource[4] << 0x18) + (pSource[5] << 0x10)) + (pSource[6] << 8)) + pSource[7];
            }
            num = (uint) (4 * ((pSource[0] & 0x7f) - 0x40));
            result = this.ldexpl(x, ((int) num) - 0x38);
            if ((pSource[0] & 0x80) != 0)
            {
                result = -result;
            }
            return 0;
        }

        private unsafe int FromPackedDec(byte* pPackedDecIn, long* pValue, long length)
        {
            int num8;
            long num = 0L;
            long num2 = 0L;
            int num7 = 0;
            byte* numPtr = (byte*) ((&num + 1) - (length));
            switch ((this.Encoding & 240))
            {
                case 0:
                case 0x10:
                    for (num8 = 0; num8 < length; num8++)
                    {
                        numPtr[num8] = pPackedDecIn[num8];
                    }
                    break;

                case 0x20:
                    num8 = 0;
                    while (num8 < length)
                    {
                        numPtr[num8] = pPackedDecIn[((int) (length - 1L)) - num8];
                        num8++;
                    }
                    break;

                default:
                    num7 = 0x841;
                    break;
            }
            if (num7 == 0)
            {
                byte num3 = (byte) (numPtr[(int) (length - 1L)] & 15);
                if (num3 < 10)
                {
                    return 0x1774;
                }
                long num6 = 1L;
                for (num8 = ((int) length) - 1; num8 >= 0; num8--)
                {
                    byte num4;
                    byte num5;
                    if (num8 == (length - 1L))
                    {
                        num5 = 0;
                        num4 = (byte) ((numPtr[num8] & 240) >> 4);
                        num2 += num4 * num6;
                        num6 *= 10L;
                    }
                    else
                    {
                        num5 = (byte) (numPtr[num8] & 15);
                        num2 += num5 * num6;
                        num6 *= 10L;
                        num4 = (byte) ((numPtr[num8] & 240) >> 4);
                        num2 += num4 * num6;
                        num6 *= 10L;
                    }
                    if ((num4 > 9) || (num5 > 9))
                    {
                        num7 = 0x1774;
                        num2 = 0L;
                        break;
                    }
                }
                switch (num3)
                {
                    case 11:
                    case 13:
                        num2 = -num2;
                        break;
                }
                pValue[0] = num2;
            }
            return num7;
        }

        public bool GetBooleanProperty(string name)
        {
            bool booleanProperty;
            uint method = 0x14d;
            this.TrEntry(method, new object[] { name });
            try
            {
                booleanProperty = this.GetBooleanProperty(name, null);
            }
            finally
            {
                base.TrExit(method);
            }
            return booleanProperty;
        }

        public bool GetBooleanProperty(string name, MQPropertyDescriptor pd)
        {
            uint method = 0x14c;
            this.TrEntry(method, new object[] { name, pd });
            bool flag = false;
            try
            {
                flag = this.ParseBoolean(this.GetObjectProperty(name, pd));
            }
            finally
            {
                base.TrExit(method);
            }
            return flag;
        }

        internal byte[] GetBuffer()
        {
            byte[] buffer2;
            uint method = 0x108;
            this.TrEntry(method);
            try
            {
                buffer2 = this.memoryStream.ToArray();
            }
            finally
            {
                base.TrExit(method);
            }
            return buffer2;
        }

        public sbyte GetByteProperty(string name)
        {
            sbyte byteProperty;
            uint method = 0x14f;
            this.TrEntry(method, new object[] { name });
            try
            {
                byteProperty = this.GetByteProperty(name, null);
            }
            finally
            {
                base.TrExit(method);
            }
            return byteProperty;
        }

        public sbyte GetByteProperty(string name, MQPropertyDescriptor pd)
        {
            uint method = 0x14e;
            this.TrEntry(method, new object[] { name, pd });
            sbyte num2 = 0;
            try
            {
                num2 = this.ParseByte(this.GetObjectProperty(name, pd));
            }
            finally
            {
                base.TrExit(method);
            }
            return num2;
        }

        public sbyte[] GetBytesProperty(string name)
        {
            sbyte[] bytesProperty;
            uint method = 0x14b;
            this.TrEntry(method, new object[] { name });
            try
            {
                bytesProperty = this.GetBytesProperty(name, null);
            }
            finally
            {
                base.TrExit(method);
            }
            return bytesProperty;
        }

        public sbyte[] GetBytesProperty(string name, MQPropertyDescriptor pd)
        {
            sbyte[] numArray;
            uint method = 330;
            this.TrEntry(method, new object[] { name, pd });
            try
            {
                numArray = this.ParseBytes(this.GetObjectProperty(name, pd));
            }
            finally
            {
                base.TrExit(method);
            }
            return numArray;
        }

        public double GetDoubleProperty(string name)
        {
            double doubleProperty;
            uint method = 0x15f;
            this.TrEntry(method, new object[] { name });
            try
            {
                doubleProperty = this.GetDoubleProperty(name, null);
            }
            finally
            {
                base.TrExit(method);
            }
            return doubleProperty;
        }

        public double GetDoubleProperty(string name, MQPropertyDescriptor pd)
        {
            uint method = 350;
            this.TrEntry(method, new object[] { name, pd });
            double num2 = 0.0;
            try
            {
                num2 = this.ParseDouble(this.GetObjectProperty(name, pd));
            }
            finally
            {
                base.TrExit(method);
            }
            return num2;
        }

        public float GetFloatProperty(string name)
        {
            float floatProperty;
            uint method = 0x15d;
            this.TrEntry(method, new object[] { name });
            try
            {
                floatProperty = this.GetFloatProperty(name, null);
            }
            finally
            {
                base.TrExit(method);
            }
            return floatProperty;
        }

        public float GetFloatProperty(string name, MQPropertyDescriptor pd)
        {
            uint method = 0x15c;
            this.TrEntry(method, new object[] { name, pd });
            float num2 = 0f;
            try
            {
                num2 = this.ParseSingle(this.GetObjectProperty(name, pd));
            }
            finally
            {
                base.TrExit(method);
            }
            return num2;
        }

        public short GetInt2Property(string name)
        {
            short shortProperty;
            uint method = 0x153;
            this.TrEntry(method, new object[] { name });
            try
            {
                shortProperty = this.GetShortProperty(name, null);
            }
            finally
            {
                base.TrExit(method);
            }
            return shortProperty;
        }

        public short GetInt2Property(string name, MQPropertyDescriptor pd)
        {
            uint method = 0x152;
            this.TrEntry(method, new object[] { name, pd });
            short shortProperty = 0;
            try
            {
                shortProperty = this.GetShortProperty(name, pd);
            }
            finally
            {
                base.TrExit(method);
            }
            return shortProperty;
        }

        public int GetInt4Property(string name)
        {
            int intProperty;
            uint method = 0x157;
            this.TrEntry(method, new object[] { name });
            try
            {
                intProperty = this.GetIntProperty(name, null);
            }
            finally
            {
                base.TrExit(method);
            }
            return intProperty;
        }

        public int GetInt4Property(string name, MQPropertyDescriptor pd)
        {
            uint method = 0x156;
            this.TrEntry(method, new object[] { name, pd });
            int intProperty = 0;
            try
            {
                intProperty = this.GetIntProperty(name, pd);
            }
            finally
            {
                base.TrExit(method);
            }
            return intProperty;
        }

        public long GetInt8Property(string name)
        {
            long longProperty;
            uint method = 0x15b;
            this.TrEntry(method, new object[] { name });
            try
            {
                longProperty = this.GetLongProperty(name, null);
            }
            finally
            {
                base.TrExit(method);
            }
            return longProperty;
        }

        public long GetInt8Property(string name, MQPropertyDescriptor pd)
        {
            uint method = 0x15a;
            this.TrEntry(method, new object[] { name, pd });
            long longProperty = 0L;
            try
            {
                longProperty = this.GetLongProperty(name, pd);
            }
            finally
            {
                base.TrExit(method);
            }
            return longProperty;
        }

        public int GetIntProperty(string name)
        {
            int intProperty;
            uint method = 0x155;
            this.TrEntry(method, new object[] { name });
            try
            {
                intProperty = this.GetIntProperty(name, null);
            }
            finally
            {
                base.TrExit(method);
            }
            return intProperty;
        }

        public int GetIntProperty(string name, MQPropertyDescriptor pd)
        {
            uint method = 340;
            this.TrEntry(method, new object[] { name, pd });
            int num2 = 0;
            try
            {
                num2 = this.ParseInt32(this.GetObjectProperty(name, pd));
            }
            finally
            {
                base.TrExit(method);
            }
            return num2;
        }

        private object GetJmsProperty(string name)
        {
            uint method = 0x165;
            this.TrEntry(method, new object[] { name });
            object obj2 = null;
            try
            {
                Queue queue = null;
                if (name.Equals("JMSXAppID"))
                {
                    return this.PutApplicationName;
                }
                if (name.Equals("JMSXDeliveryCount"))
                {
                    return this.BackoutCount;
                }
                if (name.Equals("JMSXUserID"))
                {
                    return this.UserId;
                }
                if (name.Equals("JMSCorrelationID"))
                {
                    if (this.properties.ContainsKey("jms.Cid"))
                    {
                        queue = (Queue) this.properties["jms.Cid"];
                        if ((queue != null) && (queue.Count > 0))
                        {
                            obj2 = queue.Dequeue();
                            queue.Enqueue(obj2);
                            this.properties["jms.Cid"] = queue;
                            return obj2;
                        }
                        base.throwNewMQException(2, 0x9a7);
                        return obj2;
                    }
                    StringBuilder hex = new StringBuilder();
                    BinToHex(this.CorrelationId, 0, this.CorrelationId.Length, hex);
                    hex.Insert(0, "ID:");
                    return hex.ToString();
                }
                if (name.Equals("JMSDeliveryMode"))
                {
                    if (this.properties.ContainsKey("JMSDeliveryMode"))
                    {
                        queue = (Queue) this.properties["JMSDeliveryMode"];
                        if ((queue != null) && (queue.Count > 0))
                        {
                            obj2 = queue.Dequeue();
                            queue.Enqueue(obj2);
                            this.properties["JMSDeliveryMode"] = queue;
                            return obj2;
                        }
                        base.throwNewMQException(2, 0x9a7);
                        return obj2;
                    }
                    return this.Persistence;
                }
                if (name.Equals("JMSDestination"))
                {
                    if (this.properties.ContainsKey("jms.Dst"))
                    {
                        queue = (Queue) this.properties["jms.Dst"];
                        if ((queue != null) && (queue.Count > 0))
                        {
                            obj2 = queue.Dequeue();
                            queue.Enqueue(obj2);
                            this.properties["jms.Dst"] = queue;
                            return obj2;
                        }
                        base.throwNewMQException(2, 0x9a7);
                        return obj2;
                    }
                    base.throwNewMQException(2, 0x9a7);
                    return obj2;
                }
                if (name.Equals("JMSExpiration"))
                {
                    if (this.properties.ContainsKey("jms.Exp"))
                    {
                        queue = (Queue) this.properties["jms.Exp"];
                        if ((queue != null) && (queue.Count > 0))
                        {
                            obj2 = queue.Dequeue();
                            queue.Enqueue(obj2);
                            this.properties["jms.Exp"] = queue;
                            return obj2;
                        }
                        base.throwNewMQException(2, 0x9a7);
                        return obj2;
                    }
                    return this.Expiry;
                }
                if (name.Equals("JMSMessageID"))
                {
                    StringBuilder builder2 = new StringBuilder();
                    BinToHex(this.MessageId, 0, this.MessageId.Length, builder2);
                    builder2.Insert(0, "ID:");
                    return builder2.ToString();
                }
                if (name.Equals("JMSPriority"))
                {
                    if (this.properties.ContainsKey("jms.Pri"))
                    {
                        queue = (Queue) this.properties["jms.Pri"];
                        if ((queue != null) && (queue.Count > 0))
                        {
                            obj2 = queue.Dequeue();
                            queue.Enqueue(obj2);
                            this.properties["jms.Pri"] = queue;
                            return obj2;
                        }
                        base.throwNewMQException(2, 0x9a7);
                        return obj2;
                    }
                    return this.Priority;
                }
                if (name.Equals("JMSReplyTo"))
                {
                    if (this.properties.ContainsKey("jms.Rto"))
                    {
                        queue = (Queue) this.properties["jms.Rto"];
                        if ((queue != null) && (queue.Count > 0))
                        {
                            obj2 = queue.Dequeue();
                            queue.Enqueue(obj2);
                            this.properties["jms.Rto"] = queue;
                            return obj2;
                        }
                        base.throwNewMQException(2, 0x9a7);
                        return obj2;
                    }
                    base.throwNewMQException(2, 0x9a7);
                    return obj2;
                }
                if (name.Equals("JMSTimestamp"))
                {
                    if (this.properties.ContainsKey("jms.Tms"))
                    {
                        queue = (Queue) this.properties["jms.Tms"];
                        if ((queue != null) && (queue.Count > 0))
                        {
                            obj2 = queue.Dequeue();
                            queue.Enqueue(obj2);
                            this.properties["jms.Tms"] = queue;
                            return obj2;
                        }
                        base.throwNewMQException(2, 0x9a7);
                        return obj2;
                    }
                    base.throwNewMQException(2, 0x9a7);
                    return obj2;
                }
                if (name.Equals("JMSType"))
                {
                    string str = null;
                    string str2 = null;
                    string str3 = null;
                    if (this.properties.ContainsKey("mcd.Type"))
                    {
                        queue = (Queue) this.properties["mcd.Type"];
                        if ((queue != null) && (queue.Count > 0))
                        {
                            str = (string) queue.Dequeue();
                            queue.Enqueue(str);
                            this.properties["mcd.Type"] = queue;
                        }
                        else
                        {
                            base.throwNewMQException(2, 0x9a7);
                        }
                    }
                    else
                    {
                        base.throwNewMQException(2, 0x9a7);
                    }
                    if (this.properties.ContainsKey("mcd.Set"))
                    {
                        queue = (Queue) this.properties["mcd.Set"];
                        if ((queue != null) && (queue.Count > 0))
                        {
                            str2 = (string) queue.Dequeue();
                            queue.Enqueue(str2);
                            this.properties["mcd.Set"] = queue;
                        }
                        else
                        {
                            base.throwNewMQException(2, 0x9a7);
                        }
                    }
                    if (this.properties.ContainsKey("mcd.Fmt"))
                    {
                        queue = (Queue) this.properties["mcd.Fmt"];
                        if ((queue != null) && (queue.Count > 0))
                        {
                            str3 = (string) queue.Dequeue();
                            queue.Enqueue(str3);
                            this.properties["mcd.Fmt"] = queue;
                        }
                        else
                        {
                            base.throwNewMQException(2, 0x9a7);
                        }
                    }
                    if ((str2 == null) && (str3 == null))
                    {
                        return obj2;
                    }
                    StringBuilder builder3 = new StringBuilder();
                    builder3.Append("mcd:///");
                    if (str2 != null)
                    {
                        builder3.Append(str2 + "/");
                    }
                    else
                    {
                        builder3.Append("/");
                    }
                    builder3.Append(str + "?");
                    if (str3 != null)
                    {
                        builder3.Append("format=" + str3);
                    }
                    return builder3.ToString();
                }
                if (name.Equals("JMSXGroupID"))
                {
                    if (this.properties.ContainsKey("jms.Gid"))
                    {
                        queue = (Queue) this.properties["jms.Gid"];
                        if ((queue != null) && (queue.Count > 0))
                        {
                            obj2 = queue.Dequeue();
                            queue.Enqueue(obj2);
                            this.properties["jms.Gid"] = queue;
                        }
                        return obj2;
                    }
                    return this.GroupId;
                }
                if (name.Equals("JMSXGroupSeq"))
                {
                    if (this.properties.ContainsKey("jms.Seq"))
                    {
                        queue = (Queue) this.properties["jms.Seq"];
                        if ((queue != null) && (queue.Count > 0))
                        {
                            obj2 = queue.Dequeue();
                            queue.Enqueue(obj2);
                            this.properties["jms.Seq"] = queue;
                        }
                        return obj2;
                    }
                    return this.MessageSequenceNumber;
                }
                base.throwNewMQException(2, 0x9a1);
            }
            finally
            {
                base.TrExit(method);
            }
            return obj2;
        }

        public long GetLongProperty(string name)
        {
            long longProperty;
            uint method = 0x159;
            this.TrEntry(method, new object[] { name });
            try
            {
                longProperty = this.GetLongProperty(name, null);
            }
            finally
            {
                base.TrExit(method);
            }
            return longProperty;
        }

        public long GetLongProperty(string name, MQPropertyDescriptor pd)
        {
            uint method = 0x158;
            this.TrEntry(method, new object[] { name, pd });
            long num2 = 0L;
            try
            {
                num2 = this.ParseInt64(this.GetObjectProperty(name, pd));
            }
            finally
            {
                base.TrExit(method);
            }
            return num2;
        }

        public object GetObjectProperty(string name)
        {
            object objectProperty;
            uint method = 0x162;
            this.TrEntry(method, new object[] { name });
            try
            {
                objectProperty = this.GetObjectProperty(name, null);
            }
            finally
            {
                base.TrExit(method);
            }
            return objectProperty;
        }

        public object GetObjectProperty(string name, MQPropertyDescriptor pd)
        {
            uint method = 0x163;
            this.TrEntry(method, new object[] { name, pd });
            object jmsProperty = null;
            Queue queue = null;
            try
            {
                if (name == null)
                {
                    base.throwNewMQException(2, 0x98a);
                }
                if (name.StartsWith("Root.MQMD."))
                {
                    name = name.Substring(10);
                    if ((name.ToLower() != "version") && (name.ToLower() != "strucid"))
                    {
                        PropertyInfo property = base.GetType().GetProperty(name);
                        if (property == null)
                        {
                            base.throwNewMQException(2, 0x98a);
                        }
                        try
                        {
                            jmsProperty = property.GetValue(this, null);
                            if (jmsProperty == null)
                            {
                                base.throwNewMQException(2, 0x9a7);
                            }
                        }
                        catch (Exception exception)
                        {
                            base.TrException(method, exception);
                            base.throwNewMQException(2, 0x9a7);
                        }
                    }
                    else
                    {
                        base.TrText(method, "Un-Supported MQMD Property : " + name);
                    }
                }
                else
                {
                    if (name.ToLower().StartsWith("jms"))
                    {
                        if (name.IndexOf('.') > -1)
                        {
                            name = name.Substring(name.IndexOf('.') + 1);
                        }
                        if (!mqPropertyNames_.ContainsKey(name))
                        {
                            base.throwNewMQException(2, 0x98a);
                        }
                        try
                        {
                            jmsProperty = this.GetJmsProperty(name);
                            if (jmsProperty == null)
                            {
                                base.throwNewMQException(2, 0x9a7);
                            }
                            goto Label_01CD;
                        }
                        catch (MQException)
                        {
                            throw;
                        }
                        catch (Exception exception2)
                        {
                            base.TrException(method, exception2);
                            base.throwNewMQException(2, 0x9a7);
                            goto Label_01CD;
                        }
                    }
                    if (name.ToLower().StartsWith("mq"))
                    {
                        name = this.MakeMQFolderExplicit(name);
                    }
                    else
                    {
                        name = this.MakeUsrFolderExplicit(name);
                    }
                    if (!this.properties.ContainsKey(name))
                    {
                        base.throwNewMQException(2, 0x9a7);
                    }
                    queue = (Queue) this.properties[name];
                    if ((queue != null) && (queue.Count > 0))
                    {
                        jmsProperty = queue.Dequeue();
                        queue.Enqueue(jmsProperty);
                    }
                }
            Label_01CD:
                if ((pd != null) && this.propertiesPDTable.ContainsKey(name))
                {
                    Queue queue2 = (Queue) this.propertiesPDTable[name];
                    if ((queue2 != null) && (queue2.Count > 0))
                    {
                        MQPropertyDescriptor descriptor = (MQPropertyDescriptor) queue2.Dequeue();
                        pd.pd = descriptor.pd;
                        queue2.Enqueue(descriptor);
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return jmsProperty;
        }

        public IEnumerator GetPropertyNames(string name)
        {
            uint method = 0x199;
            this.TrEntry(method, new object[] { name });
            ArrayList list = new ArrayList();
            try
            {
                if (!name.Equals("%"))
                {
                    name = this.MakeUsrFolderExplicit(name);
                }
                IEnumerator enumerator = this.properties.Keys.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    bool flag = false;
                    string item = enumerator.Current.ToString();
                    if (name.EndsWith("%"))
                    {
                        string str2 = name.Substring(0, name.Length - 1);
                        if (item.StartsWith(str2))
                        {
                            flag = true;
                        }
                    }
                    else if (item.Equals(name))
                    {
                        flag = true;
                    }
                    if (flag)
                    {
                        if (item.StartsWith("usr" + '.'))
                        {
                            item = item.Substring("usr".Length + 1);
                        }
                        if (item.ToLower().StartsWith("jms") || item.ToLower().StartsWith("mcd"))
                        {
                            object obj2 = namedProperties[item];
                            if (obj2 != null)
                            {
                                item = obj2.ToString();
                            }
                        }
                        if (!list.Contains(item))
                        {
                            list.Add(item);
                        }
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return list.GetEnumerator();
        }

        public short GetShortProperty(string name)
        {
            short shortProperty;
            uint method = 0x151;
            this.TrEntry(method, new object[] { name });
            try
            {
                shortProperty = this.GetShortProperty(name, null);
            }
            finally
            {
                base.TrExit(method);
            }
            return shortProperty;
        }

        public short GetShortProperty(string name, MQPropertyDescriptor pd)
        {
            uint method = 0x150;
            this.TrEntry(method, new object[] { name, pd });
            short num2 = 0;
            try
            {
                num2 = this.ParseInt16(this.GetObjectProperty(name, pd));
            }
            finally
            {
                base.TrExit(method);
            }
            return num2;
        }

        public string GetStringProperty(string name)
        {
            string stringProperty;
            uint method = 0x161;
            this.TrEntry(method, new object[] { name });
            try
            {
                stringProperty = this.GetStringProperty(name, null);
            }
            finally
            {
                base.TrExit(method);
            }
            return stringProperty;
        }

        public string GetStringProperty(string name, MQPropertyDescriptor pd)
        {
            uint method = 0x160;
            this.TrEntry(method, new object[] { name, pd });
            string str = null;
            try
            {
                str = this.ParseString(this.GetObjectProperty(name, pd));
            }
            finally
            {
                base.TrExit(method);
            }
            return str;
        }

        internal byte[] HexToBin(string hex, int start)
        {
            uint method = 0x166;
            this.TrEntry(method, new object[] { hex, start });
            byte[] buffer = null;
            int num2 = hex.Length - start;
            if (num2 == 0)
            {
                byte[] result = new byte[0];
                base.TrExit(method, result, 1);
                return result;
            }
            try
            {
                if ((num2 < 0) && ((num2 % 2) != 0))
                {
                    base.throwNewMQException(2, 0x9a2);
                }
                num2 /= 2;
                buffer = new byte[num2];
                try
                {
                    for (int i = 0; i < num2; i++)
                    {
                        string str = hex.Substring((2 * i) + start, 2);
                        buffer[i] = Convert.ToByte(str, 0x10);
                    }
                }
                catch (Exception)
                {
                    base.throwNewMQException(2, 0x9a2);
                }
            }
            finally
            {
                base.TrExit(method, 1);
            }
            return buffer;
        }

        private static bool IsJavaIdentifierPart(char c)
        {
            if (char.IsLetter(c))
            {
                return true;
            }
            UnicodeCategory unicodeCategory = char.GetUnicodeCategory(c);
            switch (unicodeCategory)
            {
                case UnicodeCategory.CurrencySymbol:
                    return true;

                case UnicodeCategory.ConnectorPunctuation:
                    return true;

                case UnicodeCategory.DecimalDigitNumber:
                    return true;

                case UnicodeCategory.LetterNumber:
                    return true;

                case UnicodeCategory.SpacingCombiningMark:
                    return true;

                case UnicodeCategory.NonSpacingMark:
                    return true;
            }
            return (((c >= '\0') && (c <= '\b')) || (((c >= '\x000e') && (c <= '\x001b')) || (((c >= '\x007f') && (c <= '\x009f')) || ((c == '.') || (unicodeCategory == UnicodeCategory.Format)))));
        }

        private static bool IsJavaIdentifierStart(char c)
        {
            if (!char.IsLetter(c))
            {
                UnicodeCategory unicodeCategory = char.GetUnicodeCategory(c);
                if ((unicodeCategory != UnicodeCategory.CurrencySymbol) && (unicodeCategory != UnicodeCategory.ConnectorPunctuation))
                {
                    return false;
                }
            }
            return true;
        }

        private double ldexp(double x, int exp)
        {
            uint method = 0x147;
            this.TrEntry(method, new object[] { x, exp });
            double result = Math.Pow(2.0, (double) exp) * x;
            base.TrExit(method, result);
            return result;
        }

        private double ldexpl(double x, int exp)
        {
            uint method = 0x148;
            this.TrEntry(method, new object[] { x, exp });
            double result = Math.Pow(2.0, (double) exp) * x;
            base.TrExit(method, result);
            return result;
        }

        private string MakeMQFolderExplicit(string name)
        {
            uint method = 0x185;
            this.TrEntry(method, new object[] { name });
            if (((name != null) && (name.Length > 0)) && (name.IndexOf('.') < 0))
            {
                name = "mq" + '.' + name;
            }
            base.TrExit(method, name);
            return name;
        }

        private string MakeUsrFolderExplicit(string name)
        {
            uint method = 0x184;
            this.TrEntry(method, new object[] { name });
            if (((name != null) && (name.Length > 0)) && (name.IndexOf('.') < 0))
            {
                name = "usr" + '.' + name;
            }
            base.TrExit(method, name);
            return name;
        }

        private double modf(double x, ref double intptr)
        {
            uint method = 0x149;
            this.TrEntry(method, new object[] { x, (double) intptr });
            intptr = Math.Round(x, 0);
            double result = x - intptr;
            base.TrExit(method, result);
            return result;
        }

        private bool ParseBoolean(object obj)
        {
            uint method = 0x18f;
            this.TrEntry(method, new object[] { obj });
            bool flag = false;
            try
            {
                if (obj != null)
                {
                    if (obj is bool)
                    {
                        return (bool) obj;
                    }
                    string strA = obj as string;
                    if (strA == null)
                    {
                        throw this.BadConvertException(method);
                    }
                    if ((string.Compare(strA, "true", true) == 0) || (strA == "1"))
                    {
                        strA = bool.TrueString;
                    }
                    else if ((string.Compare(strA, "false", true) == 0) || (strA == "0"))
                    {
                        strA = bool.FalseString;
                    }
                    try
                    {
                        flag = Convert.ToBoolean(strA);
                    }
                    catch (Exception exception)
                    {
                        base.TrException(method, exception);
                        base.throwNewMQException(2, 0x9a9);
                    }
                }
                return flag;
            }
            finally
            {
                base.TrExit(method);
            }
            return flag;
        }

        private sbyte ParseByte(object obj)
        {
            uint method = 400;
            this.TrEntry(method, new object[] { obj });
            sbyte num2 = 0;
            try
            {
                if (obj != null)
                {
                    if (obj is byte)
                    {
                        return (sbyte) obj;
                    }
                    if (obj is sbyte)
                    {
                        return (sbyte) obj;
                    }
                    if (obj is bool)
                    {
                        return Convert.ToSByte((bool) obj);
                    }
                    string str = obj as string;
                    if (str == null)
                    {
                        throw this.BadConvertException(method);
                    }
                    try
                    {
                        num2 = Convert.ToSByte(str, 10);
                    }
                    catch (Exception exception)
                    {
                        base.TrException(method, exception);
                        base.throwNewMQException(2, 0x9a9);
                    }
                }
                return num2;
            }
            finally
            {
                base.TrExit(method);
            }
            return num2;
        }

        private sbyte[] ParseBytes(object obj)
        {
            uint method = 0x197;
            this.TrEntry(method, new object[] { obj });
            sbyte[] numArray = null;
            try
            {
                if (obj == null)
                {
                    return numArray;
                }
                if (!(obj is sbyte[]))
                {
                    throw this.BadConvertException(method);
                }
                return (sbyte[]) obj;
            }
            finally
            {
                base.TrExit(method);
            }
            return numArray;
        }

        private double ParseDouble(object obj)
        {
            uint method = 0x191;
            this.TrEntry(method, new object[] { obj });
            double num2 = 0.0;
            try
            {
                if (obj != null)
                {
                    if (obj is double)
                    {
                        return (double) obj;
                    }
                    if (obj is float)
                    {
                        return (double) ((float) obj);
                    }
                    string str = obj as string;
                    if (str == null)
                    {
                        throw this.BadConvertException(method);
                    }
                    try
                    {
                        num2 = Convert.ToDouble(str);
                    }
                    catch (Exception exception)
                    {
                        base.TrException(method, exception);
                        base.throwNewMQException(2, 0x9a9);
                    }
                }
                return num2;
            }
            finally
            {
                base.TrExit(method);
            }
            return num2;
        }

        private short ParseInt16(object obj)
        {
            uint method = 0x195;
            this.TrEntry(method, new object[] { obj });
            short num2 = 0;
            try
            {
                if (obj == null)
                {
                    return num2;
                }
                if (obj is short)
                {
                    return (short) obj;
                }
                string str = obj as string;
                if (str != null)
                {
                    try
                    {
                        num2 = Convert.ToInt16(str);
                    }
                    catch (Exception exception)
                    {
                        base.TrException(method, exception);
                        base.throwNewMQException(2, 0x9a9);
                    }
                    return num2;
                }
                if (obj is bool)
                {
                    return Convert.ToInt16((bool) obj);
                }
                if (obj is byte)
                {
                    return Convert.ToInt16((byte) obj);
                }
                if (!(obj is sbyte))
                {
                    throw this.BadConvertException(method);
                }
                return Convert.ToInt16((sbyte) obj);
            }
            finally
            {
                base.TrExit(method);
            }
            return num2;
        }

        private int ParseInt32(object obj)
        {
            uint method = 0x193;
            this.TrEntry(method, new object[] { obj });
            int num2 = 0;
            try
            {
                if (obj == null)
                {
                    return num2;
                }
                if (obj is int)
                {
                    return (int) obj;
                }
                string str = obj as string;
                if (str != null)
                {
                    try
                    {
                        num2 = Convert.ToInt32(str);
                    }
                    catch (Exception exception)
                    {
                        base.TrException(method, exception);
                        base.throwNewMQException(2, 0x9a9);
                    }
                    return num2;
                }
                if (obj is bool)
                {
                    return Convert.ToInt32((bool) obj);
                }
                if (obj is byte)
                {
                    return Convert.ToInt32((byte) obj);
                }
                if (obj is sbyte)
                {
                    return Convert.ToInt32((sbyte) obj);
                }
                if (obj is short)
                {
                    return Convert.ToInt32((short) obj);
                }
                if (!(obj is System.Text.Encoding))
                {
                    throw this.BadConvertException(method);
                }
                return MQCcsidTable.GetMqCcsid((System.Text.Encoding) obj);
            }
            finally
            {
                base.TrExit(method);
            }
            return num2;
        }

        private long ParseInt64(object obj)
        {
            uint method = 0x194;
            this.TrEntry(method, new object[] { obj });
            long num2 = 0L;
            try
            {
                if (obj == null)
                {
                    return num2;
                }
                if (obj is long)
                {
                    return (long) obj;
                }
                string str = obj as string;
                if (str != null)
                {
                    try
                    {
                        num2 = Convert.ToInt64(str);
                    }
                    catch (Exception exception)
                    {
                        base.TrException(method, exception);
                        base.throwNewMQException(2, 0x9a9);
                    }
                    return num2;
                }
                if (obj is bool)
                {
                    return Convert.ToInt64((bool) obj);
                }
                if (obj is byte)
                {
                    return Convert.ToInt64((byte) obj);
                }
                if (obj is sbyte)
                {
                    return Convert.ToInt64((sbyte) obj);
                }
                if (obj is short)
                {
                    return Convert.ToInt64((short) obj);
                }
                if (!(obj is int))
                {
                    throw this.BadConvertException(method);
                }
                return Convert.ToInt64((int) obj);
            }
            finally
            {
                base.TrExit(method);
            }
            return num2;
        }

        private object ParseObjectAsType(object obj, Type type)
        {
            uint method = 0x198;
            this.TrEntry(method, new object[] { obj, type });
            object obj2 = null;
            try
            {
                if (obj == null)
                {
                    return obj2;
                }
                if (type == typeof(string))
                {
                    if (obj is byte[])
                    {
                        return System.Text.Encoding.UTF8.GetString((byte[]) obj);
                    }
                    return this.ParseString(obj);
                }
                if (type == typeof(bool))
                {
                    return this.ParseBoolean(obj);
                }
                if (type == typeof(byte))
                {
                    return this.ParseByte(obj);
                }
                if (type == typeof(byte[]))
                {
                    return this.ParseBytes(obj);
                }
                if (type == typeof(short))
                {
                    return this.ParseInt16(obj);
                }
                if (type == typeof(int))
                {
                    return this.ParseInt32(obj);
                }
                if (type == typeof(long))
                {
                    return this.ParseInt64(obj);
                }
                if (type == typeof(float))
                {
                    return this.ParseSingle(obj);
                }
                if (type == typeof(double))
                {
                    return this.ParseDouble(obj);
                }
                obj2 = null;
            }
            finally
            {
                base.TrExit(method);
            }
            return obj2;
        }

        private float ParseSingle(object obj)
        {
            uint method = 0x192;
            this.TrEntry(method, new object[] { obj });
            float num2 = 0f;
            try
            {
                if (obj != null)
                {
                    if (obj is float)
                    {
                        return (float) obj;
                    }
                    string str = obj as string;
                    if (str == null)
                    {
                        throw this.BadConvertException(method);
                    }
                    try
                    {
                        num2 = Convert.ToSingle(str);
                    }
                    catch (Exception exception)
                    {
                        base.TrException(method, exception);
                        base.throwNewMQException(2, 0x9a9);
                    }
                }
                return num2;
            }
            finally
            {
                base.TrExit(method);
            }
            return num2;
        }

        private string ParseString(object obj)
        {
            uint method = 0x196;
            this.TrEntry(method, new object[] { obj });
            string str = null;
            try
            {
                if (obj is byte[])
                {
                    byte[] buffer = (byte[]) obj;
                    if (buffer.Length > 0x3fffffff)
                    {
                        throw this.BadConvertException(method);
                    }
                    StringBuilder builder = new StringBuilder(buffer.Length * 2);
                    foreach (byte num2 in buffer)
                    {
                        builder.Append(BIN2HEX[num2 >> 4]);
                        builder.Append(BIN2HEX[num2 & 15]);
                    }
                    return builder.ToString();
                }
                str = obj as string;
                if ((str == null) && (obj != null))
                {
                    str = obj.ToString();
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return str;
        }

        internal byte[] Read(int count)
        {
            byte[] buffer2;
            uint method = 0x10a;
            this.TrEntry(method, new object[] { count });
            try
            {
                buffer2 = this.binaryReader.ReadBytes(count);
            }
            finally
            {
                base.TrExit(method);
            }
            return buffer2;
        }

        public bool ReadBoolean()
        {
            bool flag2;
            uint method = 0x10b;
            this.TrEntry(method);
            try
            {
                flag2 = this.binaryReader.ReadBoolean();
            }
            finally
            {
                base.TrExit(method);
            }
            return flag2;
        }

        public byte ReadByte()
        {
            byte num3;
            uint method = 0x10c;
            this.TrEntry(method);
            try
            {
                num3 = this.binaryReader.ReadByte();
            }
            finally
            {
                base.TrExit(method);
            }
            return num3;
        }

        public byte[] ReadBytes(int count)
        {
            byte[] buffer2;
            uint method = 0x10d;
            this.TrEntry(method, new object[] { count });
            try
            {
                buffer2 = this.binaryReader.ReadBytes(count);
            }
            finally
            {
                base.TrExit(method);
            }
            return buffer2;
        }

        public char ReadChar()
        {
            char ch;
            uint method = 270;
            this.TrEntry(method);
            try
            {
                ch = System.Text.Encoding.Unicode.GetChars(this.binaryReader.ReadBytes(2))[0];
            }
            finally
            {
                base.TrExit(method);
            }
            return ch;
        }

        private string ReadConvertedLine()
        {
            string str2;
            uint method = 0x143;
            this.TrEntry(method);
            try
            {
                string str = "";
                bool flag = false;
                while (!flag && this.memoryStream.CanRead)
                {
                    int num2;
                    if (-1 == (num2 = this.binaryReader.PeekChar()))
                    {
                        flag = true;
                    }
                    else
                    {
                        this.memoryStream.Seek(1L, SeekOrigin.Current);
                        char ch = Convert.ToChar(num2);
                        switch (ch)
                        {
                            case '\n':
                            {
                                flag = true;
                                continue;
                            }
                            case '\r':
                            {
                                flag = true;
                                if (((this.DataLength - this.DataOffset) >= 1) && (Convert.ToChar(this.memoryStream.ReadByte()) != '\n'))
                                {
                                    this.Seek(this.DataOffset - 1);
                                }
                                continue;
                            }
                        }
                        str = str + ch;
                    }
                }
                str2 = str;
            }
            finally
            {
                base.TrExit(method);
            }
            return str2;
        }

        public unsafe short ReadDecimal2()
        {
            short num6;
            uint method = 0x10f;
            this.TrEntry(method);
            try
            {
                int rc = 0;
                long pValue = 0L;
                short num4 = this.binaryReader.ReadInt16();
                rc = this.FromPackedDec((byte*) &num4, &pValue, 2L);
                if (rc != 0)
                {
                    base.throwNewMQException(2, rc);
                }
                num6 = Convert.ToInt16(pValue);
            }
            finally
            {
                base.TrExit(method);
            }
            return num6;
        }

        public unsafe int ReadDecimal4()
        {
            int num6;
            uint method = 0x110;
            this.TrEntry(method);
            try
            {
                int rc = 0;
                long pValue = 0L;
                int num4 = this.binaryReader.ReadInt32();
                rc = this.FromPackedDec((byte*) &num4, &pValue, 4L);
                if (rc != 0)
                {
                    base.throwNewMQException(2, rc);
                }
                num6 = Convert.ToInt32(pValue);
            }
            finally
            {
                base.TrExit(method);
            }
            return num6;
        }

        public unsafe long ReadDecimal8()
        {
            long num6;
            uint method = 0x111;
            this.TrEntry(method);
            try
            {
                int rc = 0;
                long pValue = 0L;
                long num4 = this.binaryReader.ReadInt64();
                rc = this.FromPackedDec((byte*) &num4, &pValue, 8L);
                if (rc != 0)
                {
                    base.throwNewMQException(2, rc);
                }
                num6 = Convert.ToInt64(pValue);
            }
            finally
            {
                base.TrExit(method);
            }
            return num6;
        }

        public unsafe double ReadDouble()
        {
            double num4;
            uint method = 0x112;
            this.TrEntry(method);
            double result = 0.0;
            try
            {
                switch ((this.Encoding & 0xf00))
                {
                    case 0x200:
                    case 0:
                        result = this.binaryReader.ReadDouble();
                        break;

                    case 0x300:
                    {
                        long num3 = this.binaryReader.ReadInt64();
                        this.From370fp((byte*) &num3, ref result, 8);
                        break;
                    }
                    case 0x100:
                        result = BitConverter.ToDouble(this.ReadReverse(8), 0);
                        break;

                    default:
                        base.TrText(method, "Invalid encoding : " + (this.Encoding & 0xf00));
                        base.throwNewMQException(2, 0x17da);
                        break;
                }
                num4 = result;
            }
            finally
            {
                base.TrExit(method);
            }
            return num4;
        }

        public unsafe float ReadFloat()
        {
            float num5;
            uint method = 0x113;
            this.TrEntry(method);
            float num2 = 0f;
            try
            {
                switch ((this.Encoding & 0xf00))
                {
                    case 0x200:
                    case 0:
                        num2 = this.binaryReader.ReadSingle();
                        break;

                    case 0x300:
                    {
                        long num3 = this.binaryReader.ReadInt64();
                        double result = 0.0;
                        this.From370fp((byte*) &num3, ref result, 8);
                        num2 = Convert.ToSingle(result);
                        break;
                    }
                    case 0x100:
                        num2 = BitConverter.ToSingle(this.ReadReverse(4), 0);
                        break;

                    default:
                        base.TrText(method, "Invalid encoding : " + (this.Encoding & 0xf00));
                        base.throwNewMQException(2, 0x17da);
                        break;
                }
                num5 = num2;
            }
            finally
            {
                base.TrExit(method);
            }
            return num5;
        }

        public void ReadFully(ref byte[] b)
        {
            uint method = 0x114;
            this.TrEntry(method, new object[] { b });
            try
            {
                this.ReadFully(ref b, this.DataOffset, this.DataLength);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void ReadFully(ref sbyte[] b)
        {
            uint method = 0x116;
            this.TrEntry(method, new object[] { b });
            try
            {
                this.ReadFully(ref b, 0, b.Length);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void ReadFully(ref byte[] b, int off, int len)
        {
            uint method = 0x115;
            this.TrEntry(method, new object[] { b, off, len });
            try
            {
                this.Seek(off);
                b = this.ReadBytes(len);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void ReadFully(ref sbyte[] b, int off, int len)
        {
            uint method = 0x117;
            this.TrEntry(method, new object[] { b, off, len });
            try
            {
                this.Seek(off);
                for (int i = 0; i < len; i++)
                {
                    b[i] = this.binaryReader.ReadSByte();
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public int ReadInt()
        {
            int num3;
            uint method = 280;
            this.TrEntry(method);
            int num2 = 0;
            try
            {
                switch ((this.Encoding & 15))
                {
                    case 0:
                    case 2:
                        num2 = this.binaryReader.ReadInt32();
                        break;

                    case 1:
                        num2 = BitConverter.ToInt32(this.ReadReverse(4), 0);
                        break;

                    default:
                        base.TrText(method, "Invalid encoding : " + (this.Encoding & 15));
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

        public short ReadInt2()
        {
            short num3;
            uint method = 0x119;
            this.TrEntry(method);
            try
            {
                num3 = this.ReadShort();
            }
            finally
            {
                base.TrExit(method);
            }
            return num3;
        }

        public int ReadInt4()
        {
            int num3;
            uint method = 0x11a;
            this.TrEntry(method);
            try
            {
                num3 = this.ReadInt();
            }
            finally
            {
                base.TrExit(method);
            }
            return num3;
        }

        public long ReadInt8()
        {
            long num3;
            uint method = 0x11b;
            this.TrEntry(method);
            try
            {
                num3 = this.ReadLong();
            }
            finally
            {
                base.TrExit(method);
            }
            return num3;
        }

        public string ReadLine()
        {
            uint method = 0x11c;
            this.TrEntry(method);
            string str = "";
            try
            {
                if (this.CharacterSet == 0x4b0)
                {
                    return this.ReadUnicodeLine();
                }
                str = this.ReadConvertedLine();
            }
            finally
            {
                base.TrExit(method);
            }
            return str;
        }

        public long ReadLong()
        {
            long num3;
            uint method = 0x11d;
            this.TrEntry(method);
            long num2 = 0L;
            try
            {
                switch ((this.Encoding & 15))
                {
                    case 0:
                    case 2:
                        num2 = this.binaryReader.ReadInt64();
                        break;

                    case 1:
                        num2 = BitConverter.ToInt64(this.ReadReverse(8), 0);
                        break;

                    default:
                        base.TrText(method, "Invalid encoding : " + (this.Encoding & 15));
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

        public object ReadObject()
        {
            uint method = 0x145;
            this.TrEntry(method);
            object obj2 = null;
            try
            {
                IFormatter formatter = new BinaryFormatter();
                obj2 = formatter.Deserialize(this.memoryStream);
            }
            finally
            {
                base.TrExit(method);
            }
            return obj2;
        }

        internal byte[] ReadReverse(int count)
        {
            byte[] buffer2;
            uint method = 0x125;
            this.TrEntry(method, new object[] { count });
            base.TrText(method, "Count = " + count.ToString());
            try
            {
                byte[] array = this.binaryReader.ReadBytes(count);
                Array.Reverse(array);
                buffer2 = array;
            }
            finally
            {
                base.TrExit(method);
            }
            return buffer2;
        }

        public short ReadShort()
        {
            short num3;
            uint method = 0x11e;
            this.TrEntry(method);
            short num2 = 0;
            try
            {
                switch ((this.Encoding & 15))
                {
                    case 0:
                    case 2:
                        num2 = this.binaryReader.ReadInt16();
                        break;

                    case 1:
                        num2 = BitConverter.ToInt16(this.ReadReverse(2), 0);
                        break;

                    default:
                        base.TrText(method, "Invalid encoding : " + (this.Encoding & 0xf00));
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

        public string ReadString(int length)
        {
            uint method = 0x11f;
            this.TrEntry(method, new object[] { length });
            string str = "";
            base.TrText(method, "charCount = " + length.ToString());
            try
            {
                byte[] buffer2;
                uint num3;
                if (this.DataLength == 0)
                {
                    this.binaryReader.ReadString();
                }
                int count = 0;
                int outLength = length * 2;
                if (outLength == 0)
                {
                    return str;
                }
                int options = 0;
                long position = this.memoryStream.Position;
                count = this.DataLength;
                byte[] inString = this.binaryReader.ReadBytes(count);
                this.Seek((int) position);
                if ((this.CharacterSet == 0x4b0) && ((this.Encoding & 15) == 1))
                {
                    options = 0x10000210;
                }
                uint num7 = CommonServices.ConvertString(this.CharacterSet, 0x4b0, inString, count, out buffer2, ref outLength, options, out num3);
                if ((num7 != 0) && (0x10806055 != num7))
                {
                    base.throwNewMQException(2, 0x893);
                }
                this.Seek(((int) position) + ((int) num3));
                str = System.Text.Encoding.Unicode.GetString(buffer2, 0, outLength);
            }
            finally
            {
                base.TrExit(method);
            }
            return str;
        }

        public ushort ReadUInt2()
        {
            ushort num3;
            uint method = 0x120;
            this.TrEntry(method);
            try
            {
                num3 = this.ReadUnsignedShort();
            }
            finally
            {
                base.TrExit(method);
            }
            return num3;
        }

        private string ReadUnicodeLine()
        {
            string str3;
            uint method = 0x142;
            this.TrEntry(method);
            try
            {
                string str = "";
                bool flag = false;
                while (!flag && this.memoryStream.CanRead)
                {
                    if (-1 == this.binaryReader.PeekChar())
                    {
                        flag = true;
                    }
                    else
                    {
                        char ch = this.ReadChar();
                        switch (ch)
                        {
                            case '\n':
                            {
                                flag = true;
                                continue;
                            }
                            case '\r':
                            {
                                flag = true;
                                if (((this.DataLength - this.DataOffset) >= 2) && (this.ReadChar() != '\n'))
                                {
                                    this.Seek(this.DataOffset - 2);
                                }
                                continue;
                            }
                        }
                        str = str + ch;
                    }
                }
                str3 = str.ToString();
            }
            catch (EndOfStreamException)
            {
                base.TrText(method, "hit end of file!");
                throw;
            }
            finally
            {
                base.TrExit(method);
            }
            return str3;
        }

        public byte ReadUnsignedByte()
        {
            byte num3;
            uint method = 0x121;
            this.TrEntry(method);
            try
            {
                num3 = this.binaryReader.ReadByte();
            }
            finally
            {
                base.TrExit(method);
            }
            return num3;
        }

        public ushort ReadUnsignedShort()
        {
            ushort num3;
            uint method = 290;
            this.TrEntry(method);
            ushort num2 = 0;
            try
            {
                switch ((this.Encoding & 15))
                {
                    case 0:
                    case 2:
                        num2 = this.binaryReader.ReadUInt16();
                        break;

                    case 1:
                        num2 = BitConverter.ToUInt16(this.ReadReverse(2), 0);
                        break;

                    default:
                        base.TrText(method, "Invalid encoding : " + (this.Encoding & 15));
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

        public string ReadUTF()
        {
            string str2;
            uint method = 0x123;
            this.TrEntry(method);
            try
            {
                byte[] buffer2;
                uint num4;
                ushort count = BitConverter.ToUInt16(this.ReadReverse(2), 0);
                base.TrText(method, "byteCount = " + count.ToString());
                byte[] inString = this.binaryReader.ReadBytes(count);
                int outLength = 0;
                uint num5 = CommonServices.ConvertString(0x4b8, 0x4b0, inString, count, out buffer2, ref outLength, 0, out num4);
                if ((num5 != 0) && (0x10806055 != num5))
                {
                    base.throwNewMQException(2, 0x893);
                }
                if (num4 != count)
                {
                    base.TrText(method, "ConvertString only converted " + num4.ToString() + " of " + count.ToString() + " bytes supplied");
                }
                str2 = System.Text.Encoding.Unicode.GetString(buffer2, 0, outLength);
            }
            finally
            {
                base.TrExit(method);
            }
            return str2;
        }

        public void ResizeBuffer(int size)
        {
            uint method = 0x124;
            this.TrEntry(method, new object[] { size });
            try
            {
                this.memoryStream.SetLength((long) size);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Seek(int offset)
        {
            uint method = 0x126;
            this.TrEntry(method, new object[] { offset });
            base.TrText(method, "Offset = " + offset.ToString());
            try
            {
                if (offset > this.memoryStream.Length)
                {
                    base.TrText(method, "seek (via exception)");
                    throw new EndOfStreamException();
                }
                this.memoryStream.Seek((long) offset, SeekOrigin.Begin);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetBooleanProperty(string name, bool value)
        {
            uint method = 0x16a;
            this.TrEntry(method, new object[] { name, value });
            try
            {
                this.SetBooleanProperty(name, null, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetBooleanProperty(string name, MQPropertyDescriptor pd, bool value)
        {
            uint method = 0x169;
            this.TrEntry(method, new object[] { name, pd, value });
            try
            {
                this.SetObjectProperty(name, pd, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetByteProperty(string name, sbyte value)
        {
            uint method = 0x16e;
            this.TrEntry(method, new object[] { name, value });
            try
            {
                this.SetByteProperty(name, null, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetByteProperty(string name, MQPropertyDescriptor pd, sbyte value)
        {
            uint method = 0x16d;
            this.TrEntry(method, new object[] { name, pd, value });
            try
            {
                this.SetObjectProperty(name, pd, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetBytesProperty(string name, sbyte[] value)
        {
            uint method = 0x16c;
            this.TrEntry(method, new object[] { name, value });
            try
            {
                this.SetBytesProperty(name, null, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetBytesProperty(string name, MQPropertyDescriptor pd, sbyte[] value)
        {
            uint method = 0x16b;
            this.TrEntry(method, new object[] { name, pd, value });
            try
            {
                this.SetObjectProperty(name, pd, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void SetCorrelationId(byte[] correlId)
        {
            uint method = 360;
            this.TrEntry(method, new object[] { correlId });
            byte[] buffer = new byte[0x18];
            int length = correlId.Length;
            if (length > 0x18)
            {
                length = 0x18;
            }
            for (int i = 0; i < length; i++)
            {
                buffer[i] = correlId[i];
            }
            for (int j = length; j < 0x18; j++)
            {
                buffer[j] = 0;
            }
            this.CorrelationId = buffer;
            base.TrExit(method);
        }

        public void SetDoubleProperty(string name, double value)
        {
            uint method = 0x17e;
            this.TrEntry(method, new object[] { name, value });
            try
            {
                this.SetDoubleProperty(name, null, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetDoubleProperty(string name, MQPropertyDescriptor pd, double value)
        {
            uint method = 0x17d;
            this.TrEntry(method, new object[] { name, pd, value });
            try
            {
                this.SetObjectProperty(name, pd, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetFloatProperty(string name, float value)
        {
            uint method = 380;
            this.TrEntry(method, new object[] { name, value });
            try
            {
                this.SetFloatProperty(name, null, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetFloatProperty(string name, MQPropertyDescriptor pd, float value)
        {
            uint method = 0x17b;
            this.TrEntry(method, new object[] { name, pd, value });
            try
            {
                this.SetObjectProperty(name, pd, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetInt2Property(string name, short value)
        {
            uint method = 370;
            this.TrEntry(method, new object[] { name, value });
            try
            {
                this.SetShortProperty(name, null, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetInt2Property(string name, MQPropertyDescriptor pd, short value)
        {
            uint method = 0x171;
            this.TrEntry(method, new object[] { name, pd, value });
            try
            {
                this.SetShortProperty(name, pd, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetInt4Property(string name, int value)
        {
            uint method = 0x176;
            this.TrEntry(method, new object[] { name, value });
            try
            {
                this.SetIntProperty(name, null, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetInt4Property(string name, MQPropertyDescriptor pd, int value)
        {
            uint method = 0x175;
            this.TrEntry(method, new object[] { name, pd, value });
            try
            {
                this.SetIntProperty(name, pd, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetInt8Property(string name, long value)
        {
            uint method = 0x17a;
            this.TrEntry(method, new object[] { name, value });
            try
            {
                this.SetLongProperty(name, null, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetInt8Property(string name, MQPropertyDescriptor pd, long value)
        {
            uint method = 0x179;
            this.TrEntry(method, new object[] { name, pd, value });
            try
            {
                this.SetLongProperty(name, pd, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetIntProperty(string name, int value)
        {
            uint method = 0x174;
            this.TrEntry(method, new object[] { name, value });
            try
            {
                this.SetIntProperty(name, null, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetIntProperty(string name, MQPropertyDescriptor pd, int value)
        {
            uint method = 0x173;
            this.TrEntry(method, new object[] { name, pd, value });
            try
            {
                this.SetObjectProperty(name, pd, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void SetJmsProperty(string name, object value)
        {
            uint method = 0x164;
            this.TrEntry(method, new object[] { name, value });
            try
            {
                Queue queue = new Queue();
                if (name.Equals("JMSCorrelationID"))
                {
                    try
                    {
                        if (value is string)
                        {
                            string hex = value.ToString();
                            if ((hex.Length >= 3) && hex.Substring(0, 3).ToLower().Equals("id:"))
                            {
                                hex = hex.Substring(3, hex.Length - 3);
                                this.SetCorrelationId(this.HexToBin(hex, 0));
                            }
                            else
                            {
                                this.SetCorrelationId(System.Text.Encoding.UTF8.GetBytes(value.ToString()));
                                queue.Enqueue(value);
                                this.properties.Add("jms.Cid", queue);
                            }
                        }
                        else if (value is byte[])
                        {
                            this.SetCorrelationId((byte[]) value);
                        }
                        else
                        {
                            base.throwNewMQException(2, 0x89f);
                        }
                    }
                    catch (Exception)
                    {
                        base.throwNewMQException(2, 0x9a9);
                    }
                }
                else if (name.Equals("JMSDeliveryMode"))
                {
                    if (value is int)
                    {
                        int num2 = Convert.ToInt32(value);
                        switch (num2)
                        {
                            case 1:
                                this.Persistence = 1;
                                break;

                            case 0:
                                this.Persistence = 0;
                                break;

                            default:
                                base.throwNewMQException(2, 0x7ff);
                                break;
                        }
                        queue.Enqueue(num2);
                        this.properties.Add("jms.Dlv", queue);
                    }
                    else
                    {
                        base.throwNewMQException(2, 0x9a9);
                    }
                }
                else if (name.Equals("JMSDestination"))
                {
                    queue.Enqueue(value);
                    this.properties.Add("jms.Dst", queue);
                }
                else if (name.Equals("JMSExpiration"))
                {
                    try
                    {
                        long num3 = Convert.ToInt64(value);
                        if (num3 == 0L)
                        {
                            this.Expiry = -1;
                        }
                        else
                        {
                            long num4 = num3 - DateTime.Now.Millisecond;
                            if ((num4 < 0L) || (num4 > 0x31ffffff9cL))
                            {
                                this.Expiry = -1;
                            }
                            else
                            {
                                this.Expiry = (int) ((num4 + 100L) / 100L);
                            }
                            queue.Enqueue(num3);
                            this.properties.Add("jms.Exp", queue);
                        }
                    }
                    catch (Exception)
                    {
                        base.throwNewMQException(2, 0x9a9);
                    }
                }
                else if (name.Equals("JMSMessageID"))
                {
                    try
                    {
                        if (value is string)
                        {
                            string str2 = value.ToString();
                            if ((str2.Length >= 3) && str2.Substring(0, 3).ToLower().Equals("id:"))
                            {
                                str2 = str2.Substring(3, str2.Length - 3);
                                this.SetMessageId(this.HexToBin(str2, 0));
                            }
                        }
                        else if (value is byte[])
                        {
                            this.SetMessageId((byte[]) value);
                        }
                        else
                        {
                            base.throwNewMQException(2, 0x9a9);
                        }
                    }
                    catch (Exception)
                    {
                        base.throwNewMQException(2, 0x89e);
                    }
                }
                else if (name.Equals("JMSPriority"))
                {
                    if (value is int)
                    {
                        this.Priority = Convert.ToInt32(value);
                        if (this.Priority != 4)
                        {
                            queue.Enqueue(value);
                            this.properties.Add("jms.Pri", queue);
                        }
                    }
                    else
                    {
                        base.throwNewMQException(2, 0x9a9);
                    }
                }
                else if (name.Equals("JMSReplyTo"))
                {
                    string str3 = "queue:///";
                    string str4 = value.ToString();
                    try
                    {
                        if (str4.StartsWith(str3))
                        {
                            str4 = str4.Substring(str3.Length);
                            if (str4.IndexOf('?') > -1)
                            {
                                str4 = str4.Substring(0, str4.IndexOf('?'));
                            }
                            byte[] replyToQueue = this.md.ReplyToQueue;
                            base.GetBytes(str4.Trim(), ref replyToQueue);
                            this.md.ReplyToQueue = replyToQueue;
                        }
                        else if (str4.StartsWith(replyToURI))
                        {
                            str4 = str4.Substring(replyToURI.Length);
                            if (str4.IndexOf("/") > 0)
                            {
                                byte[] replyToQueueMgr = this.md.ReplyToQueueMgr;
                                base.GetBytes(str4.Substring(0, str4.IndexOf("/")).Trim(), ref replyToQueueMgr);
                                this.md.ReplyToQueueMgr = replyToQueueMgr;
                            }
                            if (str4.Length > 1)
                            {
                                str4 = str4.Substring(this.ReplyToQueueManagerName.Length + 1);
                                if (str4.IndexOf('?') > -1)
                                {
                                    str4 = str4.Substring(0, str4.IndexOf('?'));
                                }
                            }
                            byte[] buffer = this.md.ReplyToQueue;
                            base.GetBytes(str4.Trim(), ref buffer);
                            this.md.ReplyToQueue = buffer;
                        }
                        else
                        {
                            byte[] buffer4 = this.md.ReplyToQueue;
                            base.GetBytes(str4.Trim(), ref buffer4);
                            this.md.ReplyToQueue = buffer4;
                        }
                        queue.Enqueue(value);
                        this.properties.Add("jms.Rto", queue);
                    }
                    catch (Exception)
                    {
                        base.throwNewMQException(2, 0x9a9);
                    }
                }
                else if (name.Equals("JMSType"))
                {
                    string str5 = value.ToString();
                    string str6 = "";
                    Queue queue2 = new Queue();
                    if (!str5.StartsWith(mcdURI))
                    {
                        queue.Enqueue(value);
                        this.properties.Add("mcd.Type", queue);
                        if (this.Format == "MQSTR   ")
                        {
                            str6 = "jms_text";
                        }
                        else
                        {
                            str6 = "jms_bytes";
                        }
                        queue2.Enqueue(str6);
                        this.properties.Add("mcd.Msd", queue2);
                    }
                    else
                    {
                        try
                        {
                            string str7 = "";
                            string str8 = "";
                            string str9 = "";
                            int length = 0;
                            str5 = str5.Substring(mcdURI.Length);
                            str6 = str5.Substring(0, str5.IndexOf("/"));
                            if ((str6 != null) && (str6.Length == 0))
                            {
                                if (str8 == "MQSTR   ")
                                {
                                    str6 = "jms_text";
                                }
                                else
                                {
                                    str6 = "jms_bytes";
                                }
                            }
                            else
                            {
                                length = str6.Length;
                            }
                            str5 = str5.Substring(length + 1);
                            str7 = str5.Substring(0, str5.IndexOf("/"));
                            str5 = str5.Substring(str7.Length + 1);
                            str9 = str5.Substring(0, str5.IndexOf('?'));
                            str5 = str5.Substring(str9.Length + 1);
                            if (str5.Length > 1)
                            {
                                if (str5.StartsWith("format"))
                                {
                                    str8 = str5.Substring(str5.IndexOf('=') + 1);
                                }
                                else
                                {
                                    str8 = str5;
                                }
                            }
                            queue.Enqueue(str9);
                            this.properties.Add("mcd.Type", queue);
                            Queue queue3 = new Queue();
                            queue3.Enqueue(str7);
                            this.properties.Add("mcd.Set", queue3);
                            Queue queue4 = new Queue();
                            queue4.Enqueue(str8);
                            this.properties.Add("mcd.Fmt", queue4);
                            queue2.Enqueue(str6);
                            this.properties.Add("mcd.Msd", queue2);
                        }
                        catch (Exception)
                        {
                            base.throwNewMQException(2, 0x98a);
                        }
                    }
                }
                else if (name.Equals("JMSTimestamp"))
                {
                    if (value is long)
                    {
                        queue.Enqueue(value);
                        this.properties.Add("jms.Tms", queue);
                    }
                    else
                    {
                        base.throwNewMQException(2, 0x9a9);
                    }
                }
                else if (name.Equals("JMSXAppID"))
                {
                    byte[] putApplName = this.md.PutApplName;
                    base.GetBytes(value.ToString(), ref putApplName);
                    this.md.PutApplName = putApplName;
                }
                else if (name.Equals("JMSXDeliveryCount"))
                {
                    if (value is int)
                    {
                        this.md.BackoutCount = Convert.ToInt32(value);
                    }
                    else
                    {
                        base.throwNewMQException(2, 0x9a9);
                    }
                }
                else if (name.Equals("JMSXGroupID"))
                {
                    base.throwNewMQException(2, 0x98a);
                }
                else if (name.Equals("JMSXGroupSeq"))
                {
                    base.throwNewMQException(2, 0x98a);
                }
                else if (name.Equals("JMSXUserID"))
                {
                    byte[] userID = this.md.UserID;
                    base.GetBytes(value.ToString(), ref userID);
                    this.md.UserID = userID;
                }
                else
                {
                    base.throwNewMQException(2, 0x98a);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetLongProperty(string name, long value)
        {
            uint method = 0x178;
            this.TrEntry(method, new object[] { name, value });
            try
            {
                this.SetLongProperty(name, null, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetLongProperty(string name, MQPropertyDescriptor pd, long value)
        {
            uint method = 0x177;
            this.TrEntry(method, new object[] { name, pd, value });
            try
            {
                this.SetObjectProperty(name, pd, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void SetMessageId(byte[] msgId)
        {
            uint method = 0x167;
            this.TrEntry(method, new object[] { msgId });
            byte[] buffer = new byte[0x18];
            int length = msgId.Length;
            if (length > 0x18)
            {
                length = 0x18;
            }
            for (int i = 0; i < length; i++)
            {
                buffer[i] = msgId[i];
            }
            for (int j = length; j < 0x18; j++)
            {
                buffer[j] = 0;
            }
            this.MessageId = buffer;
            base.TrExit(method);
        }

        private void SetMQMDToDefault(string name)
        {
            uint method = 0x19b;
            this.TrEntry(method, new object[] { name });
            try
            {
                switch (name)
                {
                    case "AccountingToken":
                        this.AccountingToken = System.Text.Encoding.ASCII.GetString(MQC.MQACT_NONE);
                        return;

                    case "ApplicationIdData":
                        this.ApplicationIdData = "";
                        return;

                    case "ApplicationOriginData":
                        this.ApplicationOriginData = "";
                        return;

                    case "CharacterSet":
                        this.CharacterSet = 0;
                        return;

                    case "CorrelationId":
                        this.CorrelationId = MQC.MQCI_NONE;
                        return;

                    case "Encoding":
                        this.Encoding = 0x222;
                        return;

                    case "Expiry":
                        this.Expiry = -1;
                        return;

                    case "Feedback":
                        this.Feedback = 0;
                        return;

                    case "Format":
                        this.Format = "        ";
                        return;

                    case "GroupId":
                        this.GroupId = MQC.MQGI_NONE;
                        return;

                    case "MessageFlags":
                        this.MessageFlags = 0;
                        return;

                    case "MessageId":
                        this.MessageId = MQC.MQMI_NONE;
                        return;

                    case "MessageType":
                        this.MessageType = 8;
                        return;

                    case "Persistence":
                        this.Persistence = 2;
                        return;

                    case "Priority":
                        this.Priority = -1;
                        return;

                    case "PutApplicationName":
                        this.PutApplicationName = "";
                        return;

                    case "PutApplicationType":
                        this.PutApplicationType = 0;
                        return;

                    case "ReplyToQueueManagerName":
                        this.ReplyToQueueManagerName = "";
                        return;

                    case "ReplyToQueueName":
                        this.ReplyToQueueName = "";
                        return;

                    case "Report":
                        this.Report = 0;
                        return;

                    case "UserId":
                        this.UserId = "";
                        return;
                }
                base.throwNewMQException(1, 0x9a7);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetObjectProperty(string name, object value)
        {
            uint method = 0x181;
            this.TrEntry(method, new object[] { name, value });
            try
            {
                this.SetObjectProperty(name, null, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetObjectProperty(string name, MQPropertyDescriptor pd, object value)
        {
            uint method = 0x182;
            this.TrEntry(method, new object[] { name, pd, value });
            bool flag = false;
            try
            {
                if (name == null)
                {
                    base.throwNewMQException(2, 0x98a);
                }
                if (this.propertyValidation != 1)
                {
                    this.ValidateProperty(name);
                }
                if (value != null)
                {
                    switch (Type.GetTypeCode(value.GetType()))
                    {
                        case TypeCode.Object:
                        case TypeCode.Boolean:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.String:
                            this.AddPropertyToList(name, pd, value);
                            break;

                        default:
                            flag = true;
                            break;
                    }
                    if (flag)
                    {
                        base.throwNewMQException(2, 0x9a9);
                    }
                }
                else
                {
                    if (name.ToLower().StartsWith("Root.MQMD.") || name.ToLower().StartsWith("jms"))
                    {
                        base.throwNewMQException(2, 0x9a2);
                    }
                    this.AddPropertyToList(name, pd, value);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetShortProperty(string name, short value)
        {
            uint method = 0x170;
            this.TrEntry(method, new object[] { name, value });
            try
            {
                this.SetShortProperty(name, null, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetShortProperty(string name, MQPropertyDescriptor pd, short value)
        {
            uint method = 0x16f;
            this.TrEntry(method, new object[] { name, pd, value });
            try
            {
                this.SetObjectProperty(name, pd, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetStringProperty(string name, string value)
        {
            uint method = 0x180;
            this.TrEntry(method, new object[] { name, value });
            try
            {
                this.SetStringProperty(name, null, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void SetStringProperty(string name, MQPropertyDescriptor pd, string value)
        {
            uint method = 0x17f;
            this.TrEntry(method, new object[] { name, pd, value });
            try
            {
                this.SetObjectProperty(name, pd, value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public int SkipBytes(int n)
        {
            int num2;
            uint method = 0x127;
            this.TrEntry(method, new object[] { n });
            try
            {
                if ((this.memoryStream.Position + n) > this.memoryStream.Length)
                {
                    base.TrText(method, "SkipBytes (via exception)");
                    throw new EndOfStreamException();
                }
                this.memoryStream.Seek((long) n, SeekOrigin.Current);
                num2 = n;
            }
            finally
            {
                base.TrExit(method);
            }
            return num2;
        }

        public unsafe int To370fp(byte* pOutput, uint ulCount, double input)
        {
            byte num;
            byte num2;
            int num7;
            uint num3 = 0;
            uint num4 = 0;
            int n = 0;
            double x = 0.0;
            if (input < 0.0)
            {
                num = 0x80;
                x = this.frexp(-input, ref n);
            }
            else
            {
                num = 0;
                x = this.frexp(input, ref n);
            }
            int num8 = Math.DivRem((n - 1) + 4, 4, out num7);
            x = this.ldexp(x, 0x15 + num7);
            if (ulCount == 8)
            {
                num3 = Convert.ToUInt32(Math.Floor(x));
            }
            else
            {
                num3 = Convert.ToUInt32(x);
            }
            num4 = Convert.ToUInt32(this.ldexp(x - num3, 0x20));
            if ((num3 != 0) || (num4 != 0))
            {
                num2 = (byte) (0x40 + num8);
            }
            else
            {
                num = 0;
                num2 = 0;
            }
            if (((num3 & 0xfffff) != 0) || (num4 != 0))
            {
                while ((num3 & 0xf00000) == 0)
                {
                    num3 = (num3 << 4) | ((num4 >> 0x1c) & 15);
                    num4 = num4 << 4;
                    num2 = (byte) (num2 - 1);
                }
            }
            pOutput[0] = (byte) (num | num2);
            pOutput[1] = (byte) ((num3 & 0xff0000) >> 0x10);
            pOutput[2] = (byte) ((num3 & 0xff00) >> 8);
            pOutput[3] = (byte) (num3 & 0xff);
            if (ulCount == 8)
            {
                pOutput[4] = (byte) ((num4 & -16777216) >> 0x18);
                pOutput[5] = (byte) ((num4 & 0xff0000) >> 0x10);
                pOutput[6] = (byte) ((num4 & 0xff00) >> 8);
                pOutput[7] = (byte) (num4 & 0xff);
            }
            return 0;
        }

        private unsafe int ToPackedDec(long inValue, void* pPackedDecOut, int length)
        {
            byte num4;
            int num8;
            long num = 0L;
            long num2 = 1L;
            int num7 = 0;
            long num3 = inValue;
            byte* numPtr = (byte*) ((&num + 1) - length);
            for (num8 = 0; num8 < ((length * 2) - 1); num8++)
            {
                num2 *= 10L;
            }
            num2 -= 1L;
            if ((num3 > num2) || (num3 < -num2))
            {
                return 0x1773;
            }
            if (num3 < 0L)
            {
                num3 = -num3;
                num4 = 13;
            }
            else
            {
                num4 = 12;
            }
            num8 = length - 1;
            while (num8 >= 0)
            {
                byte num5;
                byte num6;
                if (num8 == (length - 1))
                {
                    num6 = num4;
                    num5 = Convert.ToByte((long) ((num3 - ((num3 / 10L) * 10L)) << 4));
                    num3 /= 10L;
                }
                else
                {
                    num6 = Convert.ToByte((long) (num3 - ((num3 / 10L) * 10L)));
                    num3 /= 10L;
                    num5 = Convert.ToByte((long) ((num3 - ((num3 / 10L) * 10L)) << 4));
                    num3 /= 10L;
                }
                byte* numPtr1 = numPtr + num8;
                numPtr1[0] = (byte) (numPtr1[0] + Convert.ToByte((int) (num6 + num5)));
                num8--;
            }
            switch ((this.Encoding & 240))
            {
                case 0:
                case 0x10:
                    for (num8 = 0; num8 < length; num8++)
                    {
                       // *((sbyte*) (pPackedDecOut + num8)) = numPtr[num8];
                        *((byte*)pPackedDecOut + num8) = numPtr[num8];
                    }
                    return num7;

                case 0x20:
                    for (num8 = 0; num8 < length; num8++)
                    {
                       // *((sbyte*) (pPackedDecOut + num8)) = numPtr[(length - 1) - num8];
                        *((byte*)pPackedDecOut + num8) = numPtr[(length - 1) - num8];
                    }
                    return num7;
            }
            num7 = 0x845;
            num = 0L;
            return num7;
        }

        private void ValidateNameForEmptyDot(string propertyName)
        {
            uint method = 0x18b;
            this.TrEntry(method, new object[] { propertyName });
            try
            {
                string[] strArray = propertyName.Split(new char[] { '.' });
                int index = 0;
                int length = strArray.Length;
                while (index < length)
                {
                    if (strArray[index] == "")
                    {
                        base.throwNewMQException(2, 0x98a);
                    }
                    index++;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void ValidateNameForReservedFolderNames(string propertyName)
        {
            uint method = 0x18d;
            this.TrEntry(method, new object[] { propertyName });
            try
            {
                if (propertyName.StartsWith("jms"))
                {
                    if (!mqPropertyNames_.ContainsKey(propertyName))
                    {
                        base.throwNewMQException(2, 0x98a);
                    }
                }
                else if (propertyName.StartsWith("Root.MQMD."))
                {
                    if (!mqPropertyNames_.ContainsKey(propertyName))
                    {
                        base.throwNewMQException(2, 0x98a);
                    }
                }
                else
                {
                    string str3;
                    IEnumerator enumerator = null;
                    enumerator = RESERVED_HIERARCHY_PROPERTY_NAME_PREFIXES.GetEnumerator();
                    if (!ALLOWED_HIERARCHY_PROPERTY_NAMES.Contains(propertyName))
                    {
                        while (enumerator.MoveNext())
                        {
                            string str = enumerator.Current.ToString();
                            if (propertyName.ToLower().StartsWith(str.ToLower()) && !propertyName.StartsWith("Root.MQMD."))
                            {
                                base.throwNewMQException(2, 0x98a);
                            }
                        }
                    }
                    int index = propertyName.IndexOf('.');
                    string str2 = null;
                    IEnumerator enumerator2 = null;
                    string str4 = null;
                    if (index > -1)
                    {
                        str2 = propertyName.Substring(0, index);
                        index++;
                        str3 = propertyName.Substring(index, propertyName.Length - index);
                    }
                    else
                    {
                        str3 = propertyName;
                    }
                    string[] strArray = str3.Split(new char[] { '.' });
                    if (str2 != null)
                    {
                        enumerator2 = RESTRICTED_HIERARCHY_FOLDER_NAMES.GetEnumerator();
                        while (enumerator2.MoveNext())
                        {
                            str4 = enumerator2.Current.ToString();
                            if (str2.ToLower().StartsWith(str4) && (strArray.Length > 1))
                            {
                                base.throwNewMQException(2, 0x98a);
                            }
                        }
                    }
                    enumerator = strArray.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        string str5 = enumerator.Current.ToString();
                        enumerator2 = RESTRICTED_HIERARCHY_FOLDER_NAMES.GetEnumerator();
                        while (enumerator2.MoveNext())
                        {
                            str4 = enumerator2.Current.ToString();
                            if (str5.ToLower().StartsWith(str4))
                            {
                                base.throwNewMQException(2, 0x98a);
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

        private void ValidateNameForReservedSQLNames(string propertyName)
        {
            uint method = 0x18c;
            this.TrEntry(method, new object[] { propertyName });
            try
            {
                foreach (string str in propertyName.Split(new char[] { '.' }))
                {
                    int index = 0;
                    int length = reservedKeys.Length;
                    while (index < length)
                    {
                        if (str.ToUpper().Equals(reservedKeys[index].ToUpper()))
                        {
                            base.throwNewMQException(2, 0x98a);
                        }
                        index++;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void ValidateProperty(string propertyName)
        {
            uint method = 0x187;
            this.TrEntry(method, new object[] { propertyName });
            try
            {
                if (!mqPropertyNames_.ContainsKey(propertyName))
                {
                    this.CheckNotNullPropertyName(propertyName);
                    this.CheckMaxSizeOfPropertyName(propertyName);
                    this.CheckIsValidJavaIdentifier(propertyName);
                    this.ValidateNameForEmptyDot(propertyName);
                    this.ValidateNameForReservedSQLNames(propertyName);
                    this.ValidateNameForReservedFolderNames(propertyName);
                    this.CheckNotMixedContent(propertyName);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Write(int b)
        {
            uint method = 0x128;
            this.TrEntry(method, new object[] { b });
            try
            {
                this.binaryWriter.Write((byte) b);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Write(byte[] b)
        {
            uint method = 0x129;
            this.TrEntry(method, new object[] { b });
            try
            {
                this.binaryWriter.Write(b);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Write(sbyte[] b)
        {
            uint method = 0x12b;
            this.TrEntry(method, new object[] { b });
            try
            {
                for (int i = 0; i < b.Length; i++)
                {
                    this.binaryWriter.Write(b[i]);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Write(byte[] b, int off, int len)
        {
            uint method = 0x12a;
            this.TrEntry(method, new object[] { b, off, len });
            try
            {
                this.binaryWriter.Write(b, off, len);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Write(sbyte[] b, int off, int len)
        {
            uint method = 300;
            this.TrEntry(method, new object[] { b, off, len });
            try
            {
                for (int i = off; i < len; i++)
                {
                    this.binaryWriter.Write(b[i]);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void WriteBoolean(bool value)
        {
            uint method = 0x12d;
            this.TrEntry(method, new object[] { value });
            try
            {
                this.binaryWriter.Write(value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void WriteByte(byte value)
        {
            uint method = 0x12e;
            this.TrEntry(method, new object[] { value });
            try
            {
                this.binaryWriter.Write(value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void WriteByte(int value)
        {
            uint method = 0x130;
            this.TrEntry(method, new object[] { value });
            try
            {
                this.binaryWriter.Write(Convert.ToByte(value));
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void WriteByte(sbyte value)
        {
            uint method = 0x12f;
            this.TrEntry(method, new object[] { value });
            try
            {
                this.binaryWriter.Write(value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void WriteBytes(string s)
        {
            uint method = 0x131;
            this.TrEntry(method, new object[] { s });
            try
            {
                byte[] bytes = System.Text.Encoding.Unicode.GetBytes(s);
                for (int i = 0; i < bytes.Length; i += 2)
                {
                    this.binaryWriter.Write(bytes[i]);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void WriteChar(int v)
        {
            uint method = 0x132;
            this.TrEntry(method, new object[] { v });
            try
            {
                this.binaryWriter.Write(Convert.ToInt16(v));
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void WriteChars(string s)
        {
            uint method = 0x133;
            this.TrEntry(method, new object[] { s });
            try
            {
                this.binaryWriter.Write(System.Text.Encoding.Unicode.GetBytes(s.ToCharArray()));
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public unsafe void WriteDecimal2(short v)
        {
            uint method = 0x134;
            this.TrEntry(method, new object[] { v });
            try
            {
                int rc = 0;
                short num3 = 0;
                rc = this.ToPackedDec((long) v, (void*) &num3, 2);
                if (rc == 0)
                {
                    this.binaryWriter.Write(num3);
                }
                else
                {
                    base.throwNewMQException(2, rc);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public unsafe void WriteDecimal4(int v)
        {
            uint method = 0x135;
            this.TrEntry(method, new object[] { v });
            try
            {
                int rc = 0;
                int num3 = 0;
                rc = this.ToPackedDec((long) v, (void*) &num3, 4);
                if (rc == 0)
                {
                    this.binaryWriter.Write(num3);
                }
                else
                {
                    base.throwNewMQException(2, rc);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public unsafe void WriteDecimal8(long v)
        {
            uint method = 310;
            this.TrEntry(method, new object[] { v });
            try
            {
                int rc = 0;
                long num3 = 0L;
                rc = this.ToPackedDec(v, (void*) &num3, 8);
                if (rc == 0)
                {
                    this.binaryWriter.Write(num3);
                }
                else
                {
                    base.throwNewMQException(2, rc);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public unsafe void WriteDouble(double v)
        {
            uint method = 0x137;
            this.TrEntry(method, new object[] { v });
            try
            {
                switch ((this.Encoding & 0xf00))
                {
                    case 0x200:
                    case 0:
                        this.binaryWriter.Write(v);
                        return;

                    case 0x300:
                    {
                        long num2 = 0L;
                        this.To370fp((byte*) &num2, 8, Convert.ToDouble(v));
                        this.binaryWriter.Write(num2);
                        return;
                    }
                    case 0x100:
                    {
                        byte[] bytes = BitConverter.GetBytes(v);
                        Array.Reverse(bytes);
                        this.binaryWriter.Write(bytes);
                        return;
                    }
                }
                base.TrText(method, "Unsupported encoding: " + (this.Encoding & 0xf00));
                base.throwNewMQException(2, 0x17da);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public unsafe void WriteFloat(float v)
        {
            uint method = 0x138;
            this.TrEntry(method, new object[] { v });
            try
            {
                switch ((this.Encoding & 0xf00))
                {
                    case 0x200:
                    case 0:
                        this.binaryWriter.Write(v);
                        return;

                    case 0x300:
                    {
                        long num2 = 0L;
                        this.To370fp((byte*) &num2, 8, Convert.ToDouble(v));
                        this.binaryWriter.Write(num2);
                        return;
                    }
                    case 0x100:
                    {
                        byte[] bytes = BitConverter.GetBytes(v);
                        Array.Reverse(bytes);
                        this.binaryWriter.Write(bytes);
                        return;
                    }
                }
                base.TrText(method, "Unsupported encoding: " + (this.Encoding & 0xf00));
                base.throwNewMQException(2, 0x17da);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void WriteInt(int v)
        {
            uint method = 0x139;
            this.TrEntry(method, new object[] { v });
            try
            {
                switch ((this.Encoding & 15))
                {
                    case 0:
                    case 2:
                        this.binaryWriter.Write(v);
                        return;

                    case 1:
                    {
                        byte[] bytes = BitConverter.GetBytes(v);
                        Array.Reverse(bytes);
                        this.binaryWriter.Write(bytes);
                        return;
                    }
                }
                base.TrText(method, "Unsupported encoding: " + (this.Encoding & 15));
                base.throwNewMQException(2, 0x17da);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void WriteInt2(int v)
        {
            uint method = 0x13a;
            this.TrEntry(method, new object[] { v });
            try
            {
                this.WriteShort(v);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void WriteInt4(int value)
        {
            uint method = 0x13b;
            this.TrEntry(method, new object[] { value });
            try
            {
                this.WriteInt(value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void WriteInt8(long value)
        {
            uint method = 0x13c;
            this.TrEntry(method, new object[] { value });
            try
            {
                this.WriteLong(value);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void WriteLong(long v)
        {
            uint method = 0x13d;
            this.TrEntry(method, new object[] { v });
            try
            {
                switch ((this.Encoding & 15))
                {
                    case 0:
                    case 2:
                        this.binaryWriter.Write(v);
                        return;

                    case 1:
                    {
                        byte[] bytes = BitConverter.GetBytes(v);
                        Array.Reverse(bytes);
                        this.binaryWriter.Write(bytes);
                        return;
                    }
                }
                base.TrText(method, "Unsupported encoding: " + (this.Encoding & 15));
                base.throwNewMQException(2, 0x17da);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void WriteObject(object obj)
        {
            uint method = 0x144;
            this.TrEntry(method, new object[] { obj });
            try
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(this.memoryStream, obj);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void WriteShort(int v)
        {
            uint method = 0x13e;
            this.TrEntry(method, new object[] { v });
            try
            {
                switch ((this.Encoding & 15))
                {
                    case 0:
                    case 2:
                        this.binaryWriter.Write(Convert.ToInt16(v));
                        return;

                    case 1:
                    {
                        byte[] bytes = BitConverter.GetBytes(Convert.ToInt16(v));
                        Array.Reverse(bytes);
                        this.binaryWriter.Write(bytes);
                        return;
                    }
                }
                base.TrText(method, "Unsupported encoding: " + (this.Encoding & 15));
                base.throwNewMQException(2, 0x17da);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void WriteString(string s)
        {
            uint method = 320;
            this.TrEntry(method, new object[] { s });
            try
            {
                this.WriteString(s, false);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void WriteString(string s, bool bWriteUTF)
        {
            uint method = 0x13f;
            this.TrEntry(method, new object[] { s, bWriteUTF });
            try
            {
                int num3;
                byte[] buffer2;
                uint num5;
                int options = 0;
                int characterSet = this.CharacterSet;
                if (characterSet != 0)
                {
                    if (characterSet == 0x4b0)
                    {
                        goto Label_004B;
                    }
                    goto Label_006B;
                }
                this.CharacterSet = 0x4b0;
            Label_004B:
                if ((this.Encoding & 15) == 1)
                {
                    options = 0x10000120;
                }
                else
                {
                    this.WriteChars(s);
                    return;
                }
            Label_006B:
                num3 = System.Text.Encoding.Unicode.GetByteCount(s);
                byte[] bytes = new byte[num3];
                System.Text.Encoding.Unicode.GetBytes(s, 0, s.Length, bytes, 0);
                int outLength = 0;
                if (CommonServices.ConvertString(0x4b0, this.CharacterSet, bytes, num3, out buffer2, ref outLength, options, out num5) != 0)
                {
                    base.throwNewMQException(2, 0x893);
                }
                if (bWriteUTF)
                {
                    if (outLength > 0xffff)
                    {
                        base.TrText(method, "String length greater than 65535, (" + outLength.ToString() + ")");
                        base.throwNewMQException(2, 0x88e);
                    }
                    else
                    {
                        ushort num7 = (ushort) outLength;
                        byte[] array = BitConverter.GetBytes(num7);
                        Array.Reverse(array);
                        this.binaryWriter.Write(array);
                    }
                }
                this.binaryWriter.Write(buffer2, 0, outLength);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void WriteUTF(string s)
        {
            uint method = 0x141;
            this.TrEntry(method, new object[] { s });
            int characterSet = this.CharacterSet;
            try
            {
                this.CharacterSet = 0x4b8;
                this.WriteString(s, true);
            }
            finally
            {
                this.CharacterSet = characterSet;
                base.TrExit(method);
            }
        }

        public string AccountingToken
        {
            get
            {
                return base.GetString(this.md.AccountingToken);
            }
            set
            {
                byte[] accountingToken = this.md.AccountingToken;
                base.GetBytes(value, ref accountingToken);
                this.md.AccountingToken = accountingToken;
            }
        }

        public string ApplicationIdData
        {
            get
            {
                return base.GetString(this.md.ApplIdentityData);
            }
            set
            {
                byte[] applIdentityData = this.md.ApplIdentityData;
                base.GetBytes(value, ref applIdentityData);
                this.md.ApplIdentityData = applIdentityData;
            }
        }

        public string ApplicationOriginData
        {
            get
            {
                return base.GetString(this.md.ApplOriginData);
            }
            set
            {
                byte[] applOriginData = this.md.ApplOriginData;
                base.GetBytes(value, ref applOriginData);
                this.md.ApplOriginData = applOriginData;
            }
        }

        public int BackoutCount
        {
            get
            {
                return this.md.BackoutCount;
            }
        }

        public int CharacterSet
        {
            get
            {
                return this.md.CodedCharacterSetId;
            }
            set
            {
                this.md.CodedCharacterSetId = value;
            }
        }

        public byte[] CorrelationId
        {
            get
            {
                return (byte[]) this.md.CorrelId.Clone();
            }
            set
            {
                value.CopyTo(this.md.CorrelId, 0);
            }
        }

        public int DataLength
        {
            get
            {
                return (this.MessageLength - Convert.ToInt32(this.memoryStream.Position));
            }
        }

        public int DataOffset
        {
            get
            {
                return Convert.ToInt32(this.memoryStream.Position);
            }
            set
            {
                this.memoryStream.Position = value;
            }
        }

        public int Encoding
        {
            get
            {
                return this.md.Encoding;
            }
            set
            {
                this.md.Encoding = value;
            }
        }

        public int Expiry
        {
            get
            {
                return this.md.Expiry;
            }
            set
            {
                this.md.Expiry = value;
            }
        }

        public int Feedback
        {
            get
            {
                return this.md.Feedback;
            }
            set
            {
                this.md.Feedback = value;
            }
        }

        public string Format
        {
            get
            {
                return base.GetString(this.md.Format);
            }
            set
            {
                byte[] format = this.md.Format;
                base.GetBytes(value, ref format);
                this.md.Format = format;
            }
        }

        public byte[] GroupId
        {
            get
            {
                return (byte[]) this.md.GroupID.Clone();
            }
            set
            {
                value.CopyTo(this.md.GroupID, 0);
                this.md.Version = 2;
            }
        }

        public int MessageFlags
        {
            get
            {
                return this.md.MsgFlags;
            }
            set
            {
                this.md.MsgFlags = value;
                this.md.Version = 2;
            }
        }

        public byte[] MessageId
        {
            get
            {
                return (byte[]) this.md.MsgId.Clone();
            }
            set
            {
                value.CopyTo(this.md.MsgId, 0);
            }
        }

        public int MessageLength
        {
            get
            {
                return (int) this.memoryStream.Length;
            }
        }

        public int MessageSequenceNumber
        {
            get
            {
                return this.md.MsgSequenceNumber;
            }
            set
            {
                this.md.MsgSequenceNumber = value;
                this.md.Version = 2;
            }
        }

        public int MessageType
        {
            get
            {
                return this.md.MsgType;
            }
            set
            {
                this.md.MsgType = value;
            }
        }

        public MQMessageDescriptor MQMD
        {
            get
            {
                return this.md;
            }
            set
            {
                this.md = value;
            }
        }

        public int Offset
        {
            get
            {
                return this.md.Offset;
            }
            set
            {
                this.md.Offset = value;
                this.md.Version = 2;
            }
        }

        public int OriginalLength
        {
            get
            {
                return this.md.OriginalLength;
            }
            set
            {
                this.md.OriginalLength = value;
                this.md.Version = 2;
            }
        }

        public int Persistence
        {
            get
            {
                return this.md.Persistence;
            }
            set
            {
                this.md.Persistence = value;
            }
        }

        public int Priority
        {
            get
            {
                return this.md.Priority;
            }
            set
            {
                this.md.Priority = value;
            }
        }

        public int PropertyValidation
        {
            get
            {
                return this.propertyValidation;
            }
            set
            {
                this.propertyValidation = value;
            }
        }

        public string PutApplicationName
        {
            get
            {
                return base.GetString(this.md.PutApplName);
            }
            set
            {
                byte[] putApplName = this.md.PutApplName;
                base.GetBytes(value, ref putApplName);
                this.md.PutApplName = putApplName;
            }
        }

        public int PutApplicationType
        {
            get
            {
                return this.md.PutApplType;
            }
            set
            {
                this.md.PutApplType = value;
            }
        }

        public DateTime PutDateTime
        {
            get
            {
                string str = base.GetString(this.md.PutDate);
                string str2 = base.GetString(this.md.PutTime);
                if ((str.Trim().Length == 0) || (str2.Trim().Length == 0))
                {
                    return new DateTime();
                }
                return DateTime.ParseExact(str + str2, "yyyyMMddHHmmssff", dateTimeCulture);
            }
            set
            {
                string s = value.ToString("yyyyMMdd", dateTimeCulture);
                int charCount = (s.Length < 12) ? s.Length : 12;
                System.Text.Encoding.ASCII.GetBytes(s, 0, charCount, this.md.PutDate, 0);
                string str2 = value.ToString("HHmmssff", dateTimeCulture);
                charCount = (str2.Length < 8) ? str2.Length : 8;
                System.Text.Encoding.ASCII.GetBytes(str2, 0, charCount, this.md.PutTime, 0);
            }
        }

        public string ReplyToQueueManagerName
        {
            get
            {
                return base.GetString(this.md.ReplyToQueueMgr);
            }
            set
            {
                byte[] replyToQueueMgr = this.md.ReplyToQueueMgr;
                base.GetBytes(value, ref replyToQueueMgr);
                this.md.ReplyToQueueMgr = replyToQueueMgr;
            }
        }

        public string ReplyToQueueName
        {
            get
            {
                return base.GetString(this.md.ReplyToQueue);
            }
            set
            {
                byte[] replyToQueue = this.md.ReplyToQueue;
                base.GetBytes(value, ref replyToQueue);
                this.md.ReplyToQueue = replyToQueue;
            }
        }

        public int Report
        {
            get
            {
                return this.md.Report;
            }
            set
            {
                this.md.Report = value;
            }
        }

        protected bool SPIInherited
        {
            get
            {
                return this.spiInherited;
            }
        }

        protected ulong SPIQTime
        {
            get
            {
                return this.spiQTime;
            }
        }

        public int TotalMessageLength
        {
            get
            {
                return this.totalMessageLength;
            }
        }

        public string UserId
        {
            get
            {
                return base.GetString(this.md.UserID);
            }
            set
            {
                byte[] userID = this.md.UserID;
                base.GetBytes(value, ref userID);
                this.md.UserID = userID;
            }
        }

        public int Version
        {
            get
            {
                return this.md.Version;
            }
            set
            {
                if ((value > 2) || (value < 1))
                {
                    base.throwNewMQException(2, 0x7ea);
                }
                this.md.Version = value;
            }
        }
    }
}

