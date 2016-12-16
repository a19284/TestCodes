namespace IBM.WMQ
{
    using System;
    using System.Collections;
    using System.Text;

    public sealed class MQCcsidTable
    {
        private static Hashtable characterSetTable = new Hashtable(0x10);
        private static Hashtable CS_TO_MQ = new Hashtable(40);
        private static Hashtable MQ_TO_CS = new Hashtable(40);
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        static MQCcsidTable()
        {
            characterSetTable.Add("37", "Cp037");
            characterSetTable.Add("2022", "JIS");
            characterSetTable.Add("932", "SJIS");
            characterSetTable.Add("954", "EUCJIS");
            characterSetTable.Add("1208", "UTF8");
            characterSetTable.Add("1250", "Cp1250");
            characterSetTable.Add("1251", "Cp1251");
            characterSetTable.Add("1252", "Cp1252");
            characterSetTable.Add("1253", "Cp1253");
            characterSetTable.Add("1254", "Cp1254");
            characterSetTable.Add("1256", "Cp1256");
            characterSetTable.Add("1257", "Cp1257");
            characterSetTable.Add("1258", "Cp1258");
            characterSetTable.Add("5601", "KSC5601");
            characterSetTable.Add("819", "ISO8859_1");
            characterSetTable.Add("5488", "GB18030");
            MQ_TO_CS[0] = 0xfde9;
            CS_TO_MQ[0] = 0x4b8;
            MQ_TO_CS[0x4b8] = 0xfde9;
            MQ_TO_CS[0x333] = 0x6faf;
            CS_TO_MQ[0xfde9] = 0x4b8;
            CS_TO_MQ[0x6faf] = 0x333;
            MQ_TO_CS[0x390] = 0x6fb0;
            MQ_TO_CS[0x391] = 0x6fb1;
            MQ_TO_CS[0x393] = 0x6fb3;
            MQ_TO_CS[0x441] = 0x6fb4;
            MQ_TO_CS[0x32d] = 0x6fb5;
            MQ_TO_CS[0x394] = 0x6fb6;
            MQ_TO_CS[920] = 0x6fb7;
            MQ_TO_CS[0x399] = 0x6fbb;
            MQ_TO_CS[0x39b] = 0x6fbd;
            CS_TO_MQ[0x6fb0] = 0x390;
            CS_TO_MQ[0x6fb1] = 0x391;
            CS_TO_MQ[0x6fb3] = 0x393;
            CS_TO_MQ[0x6fb4] = 0x441;
            CS_TO_MQ[0x6fb5] = 0x32d;
            CS_TO_MQ[0x6fb6] = 0x394;
            CS_TO_MQ[0x6fb7] = 920;
            CS_TO_MQ[0x6fbb] = 0x399;
            CS_TO_MQ[0x6fbd] = 0x39b;
            MQ_TO_CS[0x4b0] = 0x4b0;
            MQ_TO_CS[0x4b1] = 0x4b1;
            MQ_TO_CS[0x4b2] = 0x4b0;
            MQ_TO_CS[0x4d0] = 0xfdee;
            MQ_TO_CS[0x4d2] = 0xfded;
            CS_TO_MQ[0x4b0] = 0x4b0;
            CS_TO_MQ[0x4b1] = 0x4b1;
            CS_TO_MQ[0xfdee] = 0x4d0;
            CS_TO_MQ[0xfded] = 0x4d2;
            MQ_TO_CS[0x25] = 0x25;
            CS_TO_MQ[0x25] = 0x25;
            MQ_TO_CS[500] = 500;
            CS_TO_MQ[500] = 500;
            MQ_TO_CS[0x111] = 0x4f31;
            MQ_TO_CS[0x115] = 0x4f35;
            MQ_TO_CS[0x116] = 0x4f36;
            MQ_TO_CS[280] = 0x4f38;
            MQ_TO_CS[0x11c] = 0x4f3c;
            MQ_TO_CS[0x11d] = 0x4f3d;
            MQ_TO_CS[290] = 0x4f42;
            MQ_TO_CS[0x129] = 0x4f49;
            MQ_TO_CS[420] = 0x4fc4;
            MQ_TO_CS[0x1a7] = 0x4fc7;
            MQ_TO_CS[0x1a8] = 0x4fc8;
            MQ_TO_CS[0x346] = 0x5166;
            MQ_TO_CS[0x367] = 0x5187;
            MQ_TO_CS[880] = 0x5190;
            MQ_TO_CS[0x389] = 0x51a9;
            CS_TO_MQ[0x4f31] = 0x111;
            CS_TO_MQ[0x4f35] = 0x115;
            CS_TO_MQ[0x4f36] = 0x116;
            CS_TO_MQ[0x4f38] = 280;
            CS_TO_MQ[0x4f3c] = 0x11c;
            CS_TO_MQ[0x4f3d] = 0x11d;
            CS_TO_MQ[0x4f42] = 290;
            CS_TO_MQ[0x4f49] = 0x129;
            CS_TO_MQ[0x4fc4] = 420;
            CS_TO_MQ[0x4fc7] = 0x1a7;
            CS_TO_MQ[0x4fc8] = 0x1a8;
            CS_TO_MQ[0x5166] = 0x346;
            CS_TO_MQ[0x5187] = 0x367;
            CS_TO_MQ[0x5190] = 880;
            CS_TO_MQ[0x51a9] = 0x389;
            CS_TO_MQ[0x5221] = 0x401;
            MQ_TO_CS[0x3ba] = 0xcadc;
            MQ_TO_CS[0x13ba] = 0xcadc;
            MQ_TO_CS[0x83ba] = 0xcadc;
            MQ_TO_CS[0x3a4] = 0x3a4;
            MQ_TO_CS[0x3af] = 0x3a4;
            CS_TO_MQ[0xcadc] = 0x3ba;
            CS_TO_MQ[0x3a4] = 0x3a4;
            CS_TO_MQ[0xc42c] = 0x7e6;
            MQ_TO_CS[0x565] = 0xd698;
            MQ_TO_CS[0x567] = 0xd698;
            MQ_TO_CS[0x56a] = 0xd698;
            MQ_TO_CS[0x1570] = 0xd698;
            CS_TO_MQ[0xd698] = 0x567;
            MQ_TO_CS[950] = 950;
            MQ_TO_CS[970] = 0xcaed;
            MQ_TO_CS[0x15e1] = 0x3b5;
            MQ_TO_CS[0x36e] = 0x5182;
            CS_TO_MQ[950] = 950;
            CS_TO_MQ[0xcaed] = 970;
            CS_TO_MQ[0x3b5] = 0x15e1;
            CS_TO_MQ[0x5182] = 0x36e;
            MQ_TO_CS[0x1b5] = 0x1b5;
            MQ_TO_CS[850] = 850;
            MQ_TO_CS[0x4e2] = 0x4e2;
            CS_TO_MQ[0x1b5] = 0x1b5;
            CS_TO_MQ[850] = 850;
            CS_TO_MQ[0x4e2] = 0x4e2;
        }

        internal static int GetDefaultEncoding()
        {
            return Encoding.ASCII.WindowsCodePage;
        }

        internal static int GetDotnetCodepage(int mqCcsid)
        {
            if (MQ_TO_CS.Contains(mqCcsid))
            {
                return (int) MQ_TO_CS[mqCcsid];
            }
            return mqCcsid;
        }

        internal static Encoding GetDotnetEncoding(int mqCcsid)
        {
            Encoding aSCII = null;
            try
            {
                aSCII = Encoding.GetEncoding(GetDotnetCodepage(mqCcsid));
                if (aSCII == null)
                {
                    return Encoding.ASCII;
                }
            }
            catch (NotSupportedException)
            {
                aSCII = Encoding.ASCII;
            }
            catch (Exception)
            {
                return null;
            }
            return aSCII;
        }

        internal static int GetMqCcsid(int dotnetCodepage)
        {
            if (CS_TO_MQ.Contains(dotnetCodepage))
            {
                return (int) CS_TO_MQ[dotnetCodepage];
            }
            return dotnetCodepage;
        }

        internal static int GetMqCcsid(Encoding dotnetEncoding)
        {
            return GetMqCcsid(dotnetEncoding.CodePage);
        }

        public static string Lookup(string ccsid)
        {
            return (string) characterSetTable[ccsid];
        }
    }
}

