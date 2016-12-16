namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Collections;
    using System.IO;

    public class MQIniFile : NmqiObject
    {
        private string currentLine;
        private int currentLineNumber;
        private string currentStanzaPrefix;
        private TextReader fileReader;
        private Exception firstParseFailure;
        private const int LINE_IS_COMMENT = 3;
        private const int LINE_IS_KEY = 2;
        private const int LINE_IS_STANZA = 1;
        internal const string PROPERTY_PREFIX = "com.ibm.mq.cfg.";
        protected Hashtable propertyHash;
        private const string sccsid = "%Z% %W% %I% %E% %U%";
        private const char STANZA_COMMENT1 = ';';
        private const char STANZA_COMMENT2 = '#';
        private const char STANZA_EQUAL_CHAR = '=';
        private const char STANZA_ESCAPE = '\\';
        private const char STANZA_START_CHAR = ':';

        public MQIniFile(NmqiEnvironment nmqiEnv) : base(nmqiEnv)
        {
            this.propertyHash = new Hashtable();
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { nmqiEnv });
        }

        private void EnterStanza(string stanzaName)
        {
            uint method = 0xcb;
            this.TrEntry(method, new object[] { stanzaName });
            string str = "com.ibm.mq.cfg." + stanzaName.ToLower();
            this.currentStanzaPrefix = str;
            int num2 = 0;
            string s = (string) this.propertyHash[str];
            if (s != null)
            {
                num2 = int.Parse(s);
                this.currentStanzaPrefix = str + s;
            }
            this.propertyHash[str] = Convert.ToString((int) (num2 + 1));
            base.TrText("Stanza:" + this.currentStanzaPrefix);
            base.TrExit(method);
        }

        internal string GetAttributeValue(string attribute)
        {
            uint method = 0xca;
            this.TrEntry(method, new object[] { attribute });
            string str = attribute.ToLower();
            string result = (string) this.propertyHash[str];
            base.TrText(str + "=" + result);
            base.TrExit(method, result);
            return result;
        }

        private int GetCurrentLineType()
        {
            uint method = 0xcd;
            this.TrEntry(method);
            int result = 3;
            bool flag = false;
            for (int i = 0; i < this.currentLine.Length; i++)
            {
                char c = this.currentLine[i];
                if (!char.IsWhiteSpace(c))
                {
                    result = 1;
                    switch (c)
                    {
                        case ';':
                        case '#':
                            if (flag)
                            {
                                result = 1;
                            }
                            else
                            {
                                result = 3;
                            }
                            goto Label_0069;

                        case ':':
                            result = 2;
                            goto Label_0069;

                        case '=':
                            goto Label_0069;
                    }
                    flag = true;
                }
            }
        Label_0069:
            base.TrExit(method, result);
            return result;
        }

        internal void Parse(Stream inStream)
        {
            uint method = 0xc9;
            this.TrEntry(method, new object[] { inStream });
            try
            {
                this.firstParseFailure = null;
                if (inStream != null)
                {
                    try
                    {
                        this.fileReader = new StreamReader(inStream);
                    }
                    catch (Exception exception)
                    {
                        base.TrException(method, exception, 1);
                        this.firstParseFailure = exception;
                    }
                }
                if (this.fileReader != null)
                {
                    try
                    {
                        while ((this.currentLine = this.fileReader.ReadLine()) != null)
                        {
                            this.currentLineNumber++;
                            this.ProcessCurrentLine();
                        }
                    }
                    catch (Exception exception2)
                    {
                        base.TrException(method, exception2, 2);
                        if (this.firstParseFailure == null)
                        {
                            this.firstParseFailure = exception2;
                        }
                    }
                    finally
                    {
                        try
                        {
                            this.fileReader.Close();
                            this.fileReader.Dispose();
                        }
                        catch (IOException exception3)
                        {
                            base.TrException(method, exception3, 3);
                        }
                        this.fileReader = null;
                        this.currentLine = null;
                        this.currentStanzaPrefix = null;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void ProcessCurrentLine()
        {
            uint method = 0xce;
            this.TrEntry(method);
            int startIndex = 0;
            try
            {
                int num5;
                switch (this.GetCurrentLineType())
                {
                    case 1:
                        if (this.currentStanzaPrefix == null)
                        {
                            throw new MQException(2, 0x893);
                        }
                        goto Label_00A5;

                    case 2:
                        goto Label_0037;

                    case 3:
                        return;

                    default:
                        throw new MQException(2, 0x893);
                }
            Label_0033:
                startIndex++;
            Label_0037:
                if (char.IsWhiteSpace(this.currentLine[startIndex]))
                {
                    goto Label_0033;
                }
                int index = this.currentLine.IndexOf(':');
                while ((index > 0) && char.IsWhiteSpace(this.currentLine[index - 1]))
                {
                    index--;
                }
                this.EnterStanza(this.currentLine.Substring(startIndex, index - startIndex));
                return;
            Label_00A5:
                num5 = -1;
                for (int i = 0; i < this.currentLine.Length; i++)
                {
                    if (this.currentLine[i] == '\\')
                    {
                        if ((i < this.currentLine.Length) && (((this.currentLine[i + 1] == ';') || (this.currentLine[i + 1] == '#')) || (this.currentLine[i + 1] == '\\')))
                        {
                            this.currentLine = this.currentLine.Substring(0, i) + this.currentLine.Substring(i + 1);
                        }
                    }
                    else if ((this.currentLine[i] == ';') || (this.currentLine[i] == '#'))
                    {
                        num5 = i;
                        break;
                    }
                }
                if (num5 >= 0)
                {
                    for (index = num5; (index > 0) && char.IsWhiteSpace(this.currentLine[index - 1]); index--)
                    {
                    }
                }
                else
                {
                    index = this.currentLine.Length;
                }
                while (char.IsWhiteSpace(this.currentLine[startIndex]))
                {
                    startIndex++;
                }
                int num7 = this.currentLine.IndexOf('=');
                if (num7 <= 0)
                {
                    goto Label_0270;
                }
                int num8 = num7 + 1;
                while ((num7 > 0) && char.IsWhiteSpace(this.currentLine[num7 - 1]))
                {
                    num7--;
                }
                string attribute = this.currentLine.Substring(startIndex, num7 - startIndex);
                while ((num8 < index) && char.IsWhiteSpace(this.currentLine[num8]))
                {
                    num8++;
                }
                while ((num8 < index) && char.IsWhiteSpace(this.currentLine[index - 1]))
                {
                    index--;
                }
                string str2 = this.currentLine.Substring(num8, index - num8);
                goto Label_02A1;
            Label_026C:
                index--;
            Label_0270:
                if ((startIndex < index) && char.IsWhiteSpace(this.currentLine[index - 1]))
                {
                    goto Label_026C;
                }
                attribute = this.currentLine.Substring(startIndex, index - startIndex);
                str2 = "";
            Label_02A1:
                this.StoreAttributeValuePair(attribute, str2);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void StoreAttributeValuePair(string attribute, string value)
        {
            uint method = 0xcc;
            this.TrEntry(method, new object[] { attribute, value });
            string str = this.currentStanzaPrefix + "." + attribute.ToLower();
            this.propertyHash[str] = value;
            base.TrText(str + "=" + value);
            base.TrExit(method);
        }

        internal Exception FirstParseFailure
        {
            get
            {
                return this.firstParseFailure;
            }
        }
    }
}

