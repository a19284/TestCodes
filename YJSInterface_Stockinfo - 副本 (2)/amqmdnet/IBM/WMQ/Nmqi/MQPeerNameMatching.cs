namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class MQPeerNameMatching : NmqiObject
    {
        private string C;
        private string CN;
        private List<string> DC;
        private static int DNCLEAR = 1;
        private static int DNFINISHED = 5;
        private string DNQ;
        private static int DNQVALUE = 4;
        private static int DNSYMBOL = 2;
        private static int DNVALUE = 3;
        private string E;
        private NmqiEnvironment env;
        private string L;
        private string MAIL;
        private bool matcher;
        private string O;
        private string originalName;
        private List<string> OU;
        private MQTCPConnection owningConnection;
        private string PC;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private string SERIALNUMBER;
        private string ST;
        private string STREET;
        private string T;
        private string UID;
        private string UNSTRUCTUREDADDRESS;
        private string UNSTRUCTUREDNAME;

        internal MQPeerNameMatching(NmqiEnvironment env, MQTCPConnection conn, string DN, bool validate) : base(env)
        {
            this.DC = new List<string>();
            this.OU = new List<string>();
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.env = env;
            this.owningConnection = conn;
            this.originalName = DN;
            this.matcher = validate;
            this.ParseDN(DN);
        }

        public bool IsMatchingPeerName(MQPeerNameMatching name)
        {
            uint method = 0x63f;
            this.TrEntry(method);
            bool flag = true;
            if ((this.CN != null) && !this.Wequals(this.CN, name.CN))
            {
                flag = false;
                base.TrText(method, "CN Matching = false");
            }
            if ((this.T != null) && !this.Wequals(this.T, name.T))
            {
                flag = false;
                base.TrText(method, "T Matching = false");
            }
            if ((this.O != null) && !this.Wequals(this.O, name.O))
            {
                flag = false;
                base.TrText(method, "O Matching = false");
            }
            if ((this.L != null) && !this.Wequals(this.L, name.L))
            {
                flag = false;
                base.TrText(method, "L Matching = false");
            }
            if ((this.ST != null) && !this.Wequals(this.ST, name.ST))
            {
                flag = false;
                base.TrText(method, "ST Matching = false");
            }
            if ((this.C != null) && !this.Wequals(this.C, name.C))
            {
                flag = false;
                base.TrText(method, "C Matching = false");
            }
            if ((this.SERIALNUMBER != null) && !this.Wequals(this.SERIALNUMBER, name.SERIALNUMBER))
            {
                flag = false;
            }
            if ((this.MAIL != null) && !this.Wequals(this.MAIL, name.MAIL))
            {
                flag = false;
            }
            if ((this.E != null) && !this.Wequals(this.E, name.E))
            {
                flag = false;
            }
            if ((this.UID != null) && !this.Wequals(this.UID, name.UID))
            {
                flag = false;
            }
            if ((this.STREET != null) && !this.Wequals(this.STREET, name.STREET))
            {
                flag = false;
            }
            if ((this.PC != null) && !this.Wequals(this.PC, name.PC))
            {
                flag = false;
            }
            if ((this.UNSTRUCTUREDNAME != null) && !this.Wequals(this.UNSTRUCTUREDNAME, name.UNSTRUCTUREDNAME))
            {
                flag = false;
            }
            if ((this.UNSTRUCTUREDADDRESS != null) && !this.Wequals(this.UNSTRUCTUREDADDRESS, name.UNSTRUCTUREDADDRESS))
            {
                flag = false;
            }
            if ((this.DNQ != null) && !this.Wequals(this.DNQ, name.DNQ))
            {
                flag = false;
            }
            if (this.OU.Count > name.OU.Count)
            {
                flag = false;
                base.TrText(method, "OU Matching = false");
            }
            else
            {
                for (int i = 0; i < this.OU.Count; i++)
                {
                    if (!this.Wequals(this.OU[i], name.OU[i]))
                    {
                        flag = false;
                        base.TrText(method, "OU Matching = false");
                    }
                }
            }
            if (this.DC.Count > name.DC.Count)
            {
                flag = false;
            }
            else
            {
                for (int j = 0; j < this.DC.Count; j++)
                {
                    if (!this.Wequals(this.DC[j], name.DC[j]))
                    {
                        flag = false;
                        base.TrText(method, "DC Matching = false");
                    }
                }
            }
            base.TrText(method, "Return value = " + flag);
            base.TrExit(method);
            return flag;
        }

        private void ParseDN(string DN)
        {
            uint method = 0x63d;
            this.TrEntry(method);
            int dNCLEAR = DNCLEAR;
            string rawsymbol = "";
            string str2 = "";
            base.TrText(method, "DN String = " + DN);
            if ((DN != null) && DN.Equals(""))
            {
                dNCLEAR = DNFINISHED;
                base.TrText(method, "State=" + dNCLEAR);
            }
            try
            {
                for (int i = 0; i < DN.Length; i++)
                {
                    char ch = DN.ToCharArray()[i];
                    if (dNCLEAR == DNCLEAR)
                    {
                        switch (ch)
                        {
                            case '"':
                            case ',':
                            case ';':
                            case '=':
                            {
                                string[] inserts = new string[5];
                                inserts[2] = this.owningConnection.ChannelName;
                                inserts[4] = Convert.ToString(ch);
                                NmqiException ex = new NmqiException(this.env, 0x25a8, inserts, 2, 0x95f, null);
                                base.TrException(method, ex);
                                throw ex;
                            }
                        }
                        if ((ch != ' ') && (ch != '\t'))
                        {
                            rawsymbol = rawsymbol + ch;
                            dNCLEAR = DNSYMBOL;
                            base.TrText(method, "State=" + dNCLEAR);
                        }
                    }
                    else if (dNCLEAR == DNSYMBOL)
                    {
                        switch (ch)
                        {
                            case ' ':
                            case '"':
                            {
                                string[] strArray2 = new string[5];
                                strArray2[2] = this.owningConnection.ChannelName;
                                strArray2[4] = Convert.ToString(ch);
                                NmqiException exception2 = new NmqiException(this.env, 0x25a8, strArray2, 2, 0x95f, null);
                                base.TrException(method, exception2);
                                throw exception2;
                            }
                        }
                        if (ch == '=')
                        {
                            str2 = "";
                            if ((i + 1) >= DN.Length)
                            {
                                dNCLEAR = DNVALUE;
                                base.TrText(method, "State=" + dNCLEAR);
                            }
                            else if (DN.ToCharArray()[i + 1] == '"')
                            {
                                i++;
                                dNCLEAR = DNQVALUE;
                                base.TrText(method, "State=" + dNCLEAR);
                            }
                            else
                            {
                                dNCLEAR = DNVALUE;
                                base.TrText(method, "State=" + dNCLEAR);
                            }
                        }
                        else
                        {
                            rawsymbol = rawsymbol + ch;
                        }
                    }
                    else if (dNCLEAR == DNVALUE)
                    {
                        if (((ch == ',') || (ch == ';')) && ((i == 0) || (DN.ToCharArray()[i - 1] != '\\')))
                        {
                            dNCLEAR = DNCLEAR;
                            base.TrText(method, "State=" + dNCLEAR);
                            this.SetValue(rawsymbol, str2.Trim());
                            rawsymbol = "";
                        }
                        else
                        {
                            str2 = str2 + ch;
                            base.TrText(method, "State=" + dNCLEAR);
                        }
                    }
                    else if (dNCLEAR == DNQVALUE)
                    {
                        if ((ch == '"') && ((i == 0) || (DN.ToCharArray()[i - 1] != '\\')))
                        {
                            dNCLEAR = DNFINISHED;
                            base.TrText(method, "State=" + dNCLEAR);
                            this.SetValue(rawsymbol, str2);
                            rawsymbol = "";
                        }
                        else
                        {
                            str2 = str2 + ch;
                        }
                    }
                    else if (dNCLEAR == DNFINISHED)
                    {
                        if ((ch == ',') || (ch == ';'))
                        {
                            dNCLEAR = DNCLEAR;
                            base.TrText(method, "State=" + dNCLEAR);
                        }
                        else if ((ch != ' ') && (ch != '\t'))
                        {
                            string[] strArray3 = new string[5];
                            strArray3[2] = this.owningConnection.ChannelName;
                            strArray3[4] = Convert.ToString(ch);
                            NmqiException exception3 = new NmqiException(this.env, 0x25a8, strArray3, 2, 0x95f, null);
                            base.TrException(method, exception3);
                            throw exception3;
                        }
                    }
                }
                if (dNCLEAR == DNVALUE)
                {
                    base.TrText(method, "State=" + dNCLEAR);
                    this.SetValue(rawsymbol, str2.Trim());
                }
                if ((dNCLEAR == DNSYMBOL) || (dNCLEAR == DNCLEAR))
                {
                    string[] strArray4 = new string[5];
                    strArray4[2] = this.owningConnection.ChannelName;
                    strArray4[4] = rawsymbol;
                    NmqiException exception4 = new NmqiException(this.env, 0x25a8, strArray4, 2, 0x95f, null);
                    base.TrException(method, exception4);
                    throw exception4;
                }
            }
            catch (MQException exception5)
            {
                base.TrException(method, exception5);
                throw exception5;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void SetValue(string rawsymbol, string value)
        {
            uint method = 0x63e;
            this.TrEntry(method);
            try
            {
                string str = rawsymbol.ToUpper();
                base.TrText(method, "RawSymbol =" + str);
                if (str.Equals("CN"))
                {
                    if (this.CN != null)
                    {
                        string[] inserts = new string[5];
                        inserts[2] = this.owningConnection.ChannelName;
                        inserts[4] = "CN (x2)";
                        NmqiException ex = new NmqiException(this.env, 0x25a8, inserts, 2, 0x95f, null);
                        base.TrException(method, ex);
                        throw ex;
                    }
                    this.CN = value.ToUpper();
                    base.TrText(method, "CN=" + this.CN);
                }
                else if (str.Equals("T"))
                {
                    if (this.T != null)
                    {
                        string[] strArray2 = new string[5];
                        strArray2[2] = this.owningConnection.ChannelName;
                        strArray2[4] = "T (x2)";
                        NmqiException exception2 = new NmqiException(this.env, 0x25a8, strArray2, 2, 0x95f, null);
                        base.TrException(method, exception2);
                        throw exception2;
                    }
                    this.T = value.ToUpper();
                    base.TrText(method, "T=" + this.T);
                }
                else if (str.Equals("O"))
                {
                    if (this.O != null)
                    {
                        string[] strArray3 = new string[5];
                        strArray3[2] = this.owningConnection.ChannelName;
                        strArray3[4] = "O (x2)";
                        NmqiException exception3 = new NmqiException(this.env, 0x25a8, strArray3, 2, 0x95f, null);
                        base.TrException(method, exception3);
                        throw exception3;
                    }
                    this.O = value.ToUpper();
                    base.TrText(method, "O=" + this.O);
                }
                else if (str.Equals("L"))
                {
                    if (this.L != null)
                    {
                        string[] strArray4 = new string[5];
                        strArray4[2] = this.owningConnection.ChannelName;
                        strArray4[4] = "L (x2)";
                        NmqiException exception4 = new NmqiException(this.env, 0x25a8, strArray4, 2, 0x95f, null);
                        base.TrException(method, exception4);
                        throw exception4;
                    }
                    this.L = value.ToUpper();
                    base.TrText(method, "L=" + this.L);
                }
                else if ((str.Equals("ST") || str.Equals("SP")) || str.Equals("S"))
                {
                    if (this.ST != null)
                    {
                        string[] strArray5 = new string[5];
                        strArray5[2] = this.owningConnection.ChannelName;
                        strArray5[4] = str + " (x2)";
                        NmqiException exception5 = new NmqiException(this.env, 0x25a8, strArray5, 2, 0x95f, null);
                        base.TrException(method, exception5);
                        throw exception5;
                    }
                    this.ST = value.ToUpper();
                    base.TrText(method, "ST=" + this.ST);
                }
                else if (str.Equals("C"))
                {
                    if (this.C != null)
                    {
                        string[] strArray6 = new string[5];
                        strArray6[2] = this.owningConnection.ChannelName;
                        strArray6[4] = "C (x2)";
                        NmqiException exception6 = new NmqiException(this.env, 0x25a8, strArray6, 2, 0x95f, null);
                        base.TrException(method, exception6);
                        throw exception6;
                    }
                    this.C = value.ToUpper();
                    base.TrText(method, "C=" + this.C);
                }
                else if (str.Equals("SERIALNUMBER"))
                {
                    if (this.SERIALNUMBER != null)
                    {
                        string[] strArray7 = new string[5];
                        strArray7[2] = this.owningConnection.ChannelName;
                        strArray7[4] = str + " (x2)";
                        NmqiException exception7 = new NmqiException(this.env, 0x25a8, strArray7, 2, 0x95f, null);
                        base.TrException(method, exception7);
                        throw exception7;
                    }
                    this.SERIALNUMBER = value.ToUpper();
                    base.TrText(method, "SERIALNUMBER=" + this.SERIALNUMBER);
                }
                else if (str.Equals("MAIL"))
                {
                    if (this.MAIL != null)
                    {
                        string[] strArray8 = new string[5];
                        strArray8[2] = this.owningConnection.ChannelName;
                        strArray8[4] = str + " (x2)";
                        NmqiException exception8 = new NmqiException(this.env, 0x25a8, strArray8, 2, 0x95f, null);
                        base.TrException(method, exception8);
                        throw exception8;
                    }
                    this.MAIL = value.ToUpper();
                    base.TrText(method, "MAIL=" + this.MAIL);
                }
                else if (str.Equals("E"))
                {
                    if (this.E != null)
                    {
                        string[] strArray9 = new string[5];
                        strArray9[2] = this.owningConnection.ChannelName;
                        strArray9[4] = str + " (x2)";
                        NmqiException exception9 = new NmqiException(this.env, 0x25a8, strArray9, 2, 0x95f, null);
                        base.TrException(method, exception9);
                        throw exception9;
                    }
                    this.E = value.ToUpper();
                    base.TrText(method, "E=" + this.E);
                }
                else if (str.Equals("UID") || str.Equals("USERID"))
                {
                    if (this.UID != null)
                    {
                        string[] strArray10 = new string[5];
                        strArray10[2] = this.owningConnection.ChannelName;
                        strArray10[4] = str + " (x2)";
                        NmqiException exception10 = new NmqiException(this.env, 0x25a8, strArray10, 2, 0x95f, null);
                        base.TrException(method, exception10);
                        throw exception10;
                    }
                    this.UID = value.ToUpper();
                    base.TrText(method, "UID=" + this.UID);
                }
                else if (str.Equals("STREET"))
                {
                    if (this.STREET != null)
                    {
                        string[] strArray11 = new string[5];
                        strArray11[2] = this.owningConnection.ChannelName;
                        strArray11[4] = str + " (x2)";
                        NmqiException exception11 = new NmqiException(this.env, 0x25a8, strArray11, 2, 0x95f, null);
                        base.TrException(method, exception11);
                        throw exception11;
                    }
                    this.STREET = value.ToUpper();
                    base.TrText(method, "STREET=" + this.STREET);
                }
                else if (str.Equals("PC") || str.Equals("POSTALCODE"))
                {
                    if (this.PC != null)
                    {
                        string[] strArray12 = new string[5];
                        strArray12[2] = this.owningConnection.ChannelName;
                        strArray12[4] = str + " (x2)";
                        NmqiException exception12 = new NmqiException(this.env, 0x25a8, strArray12, 2, 0x95f, null);
                        base.TrException(method, exception12);
                        throw exception12;
                    }
                    this.PC = value.ToUpper();
                    base.TrText(method, "PC=" + this.PC);
                }
                else if (str.Equals("UNSTRUCTUREDNAME"))
                {
                    if (this.UNSTRUCTUREDNAME != null)
                    {
                        string[] strArray13 = new string[5];
                        strArray13[2] = this.owningConnection.ChannelName;
                        strArray13[4] = str + " (x2)";
                        NmqiException exception13 = new NmqiException(this.env, 0x25a8, strArray13, 2, 0x95f, null);
                        base.TrException(method, exception13);
                        throw exception13;
                    }
                    this.UNSTRUCTUREDNAME = value.ToUpper();
                    base.TrText(method, "UNSTRUCTUREDNAME=" + this.UNSTRUCTUREDNAME);
                }
                else if (str.Equals("UNSTRUCTUREDADDRESS"))
                {
                    if (this.UNSTRUCTUREDADDRESS != null)
                    {
                        string[] strArray14 = new string[5];
                        strArray14[2] = this.owningConnection.ChannelName;
                        strArray14[4] = str + " (x2)";
                        NmqiException exception14 = new NmqiException(this.env, 0x25a8, strArray14, 2, 0x95f, null);
                        base.TrException(method, exception14);
                        throw exception14;
                    }
                    this.UNSTRUCTUREDADDRESS = value.ToUpper();
                    base.TrText(method, "UNSTRUCTUREDADDRESS=" + this.UNSTRUCTUREDADDRESS);
                }
                else if (str.Equals("DNQ"))
                {
                    if (this.DNQ != null)
                    {
                        string[] strArray15 = new string[5];
                        strArray15[2] = this.owningConnection.ChannelName;
                        strArray15[4] = str + " (x2)";
                        NmqiException exception15 = new NmqiException(this.env, 0x25a8, strArray15, 2, 0x95f, null);
                        base.TrException(method, exception15);
                        throw exception15;
                    }
                    this.DNQ = value.ToUpper();
                    base.TrText(method, "DNQ=" + this.DNQ);
                }
                else if (str.Equals("OU"))
                {
                    this.OU.Add(value.ToUpper());
                    base.TrText(method, "OU=" + this.OU);
                }
                else if (str.Equals("DC"))
                {
                    this.DC.Add(value.ToUpper());
                    base.TrText(method, "OU=" + this.OU);
                }
                else if (this.matcher)
                {
                    string[] strArray16 = new string[5];
                    strArray16[2] = this.owningConnection.ChannelName;
                    strArray16[4] = str + " (x2)";
                    NmqiException exception16 = new NmqiException(this.env, 0x25a8, strArray16, 2, 0x95f, null);
                    base.TrException(method, exception16);
                    throw exception16;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public bool Wequals(string pattern, string real)
        {
            bool flag5;
            uint method = 0x640;
            this.TrEntry(method);
            try
            {
                if (real == null)
                {
                    base.TrText(method, "Checking for special case where pattern is '*' but real is null, this matches the pattern");
                    if ((pattern != null) && pattern.Equals("*"))
                    {
                        base.TrText(method, "Returning true");
                        return true;
                    }
                    if (pattern != null)
                    {
                        base.TrText(method, "Returning false");
                        return false;
                    }
                    base.TrText(method, "Returning true");
                    return true;
                }
                string str = real.ToUpper();
                if (pattern.Equals("*"))
                {
                    base.TrText(method, "Returning true");
                    return true;
                }
                if (pattern.ToCharArray()[0] == '*')
                {
                    if (pattern.ToCharArray()[pattern.Length - 1] == '*')
                    {
                        bool flag = str.IndexOf(str.Substring(1, pattern.Length - 2)) != -1;
                        base.TrText(method, "Returning " + flag);
                        return flag;
                    }
                    bool flag2 = str.EndsWith(pattern.Substring(1));
                    base.TrText(method, "Returning " + flag2);
                    return flag2;
                }
                if ((pattern.ToCharArray()[pattern.Length - 1] == '*') && (pattern.ToCharArray()[pattern.Length - 2] != '\\'))
                {
                    bool flag3 = str.StartsWith(pattern.Substring(0, pattern.Length - 1));
                    base.TrText(method, "Returning " + flag3);
                    return flag3;
                }
                StringBuilder builder = new StringBuilder(pattern);
                for (int i = 0; i < builder.Length; i++)
                {
                    if (builder.ToString()[i] == '\\')
                    {
                        builder.Remove(i, 1);
                    }
                }
                string str2 = builder.ToString();
                StringBuilder builder2 = new StringBuilder(str);
                for (int j = 0; j < builder2.Length; j++)
                {
                    if (builder2[j] == '\\')
                    {
                        builder2.Remove(j, 1);
                    }
                }
                string str3 = builder2.ToString();
                bool flag4 = str2.Equals(str3);
                base.TrText(method, "Returning " + flag4);
                flag5 = flag4;
            }
            finally
            {
                base.TrExit(method);
            }
            return flag5;
        }

        public string DN
        {
            get
            {
                return this.originalName;
            }
            set
            {
                this.originalName = value;
            }
        }
    }
}

