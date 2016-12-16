namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public abstract class CommonServicesInterface
    {
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private const string SCCSID_IDENTIFIER = "@(#) ";

        protected CommonServicesInterface()
        {
        }

        internal abstract uint ConvertString(string objectId, int fromCCSID, int toCCSID, byte[] inString, int inLength, ref byte[] outString, ref int outLength, int options, ref uint bytesConverted);
        internal abstract void DisplayCopyright();
        public abstract uint DisplayMessage(string objectId, string qmgrName, uint returncode, uint mtype);
        protected string ExtractSCCSIDInfo(string sccsid, string formatString)
        {
            string str = formatString;
            try
            {
                if (sccsid.StartsWith("@(#) "))
                {
                    string[] strArray = sccsid.Substring("@(#) ".Length).Split(new char[] { ',' });
                    str = str.Replace("#N", strArray[0]);
                    if (strArray.Length >= 4)
                    {
                        strArray = strArray[3].Trim().Split(new char[] { ' ' });
                        str = str.Replace("#L", strArray[0]);
                    }
                    else if (strArray.Length >= 3)
                    {
                        strArray = strArray[2].Trim().Split(new char[] { ' ' });
                    }
                    else
                    {
                        strArray = null;
                    }
                    if (strArray != null)
                    {
                        int num = 0;
                        foreach (string str2 in strArray)
                        {
                            if (!(str2 != string.Empty))
                            {
                                continue;
                            }
                            switch (num)
                            {
                                case 1:
                                    str = str.Replace("#V", str2);
                                    break;

                                case 2:
                                    str = str.Replace("#D", str2);
                                    break;

                                case 3:
                                    str = str.Replace("#T", str2);
                                    break;
                            }
                            num++;
                        }
                    }
                }
                return str;
            }
            catch
            {
            }
            finally
            {
                str = str.Replace("#N", string.Empty).Replace("#L", string.Empty).Replace("#V", string.Empty).Replace("#D", string.Empty).Replace("#T", string.Empty);
            }
            return str;
        }

        public abstract uint FFST(string objectId, string sccsid, string lineno, uint component, string method, uint probeid, uint tag, ushort alert_num);
        public abstract uint FFST(string objectId, string sccsid, string lineno, uint component, uint method, uint probeid, uint tag, ushort alert_num);
        public abstract uint GetArithInsert1();
        public abstract uint GetArithInsert2();
        public abstract string GetCommentInsert1();
        public abstract string GetCommentInsert2();
        public abstract string GetCommentInsert3();
        protected void GetCurrentContext(out string method, out int line)
        {
            method = ".NET";
            line = 0;
            try
            {
                string stackTrace = Environment.StackTrace;
                if (stackTrace != null)
                {
                    int index;
                    while ((index = stackTrace.IndexOf("   at ")) != -1)
                    {
                        string str2 = stackTrace = stackTrace.Substring(index + 6);
                        index = str2.IndexOf("   at ");
                        if (index != -1)
                        {
                            str2 = str2.Substring(0, index);
                        }
                        if ((!str2.StartsWith("System.Environment") && !str2.StartsWith("IBM.WMQ.MQManagedClientException")) && (!str2.StartsWith("IBM.WMQ.MQBase") && (str2.IndexOf("CommonServices") == -1)))
                        {
                            index = str2.IndexOf(" in ");
                            if (index != -1)
                            {
                                str2 = str2.Substring(index + 4);
                                index = str2.IndexOf(":line ");
                                if (index != -1)
                                {
                                    method = str2.Substring(0, index);
                                    line = Convert.ToInt32(str2.Substring(index + 6));
                                }
                            }
                            else
                            {
                                index = str2.IndexOf("(");
                                if (index != -1)
                                {
                                    method = str2.Substring(0, index);
                                }
                            }
                            if (method == null)
                            {
                                method = str2;
                            }
                            return;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public abstract string GetDataLib();
        internal abstract string GetEnvironmentValue(string name);
        public abstract string GetFunctionName(uint component, uint method);
        public abstract uint GetMessage(string objectId, uint returncode, uint control, out string basicmessage, out string extendedmessage, out string replymessage, int basicLength, int extendedLength, int replyLength);
        public abstract int GetMQCSDApplied();
        public abstract int GetMQRelease();
        public abstract int GetMQVersion();
        public abstract string GetProgramName();
        public abstract string ReasonCodeName(int reason);
        public abstract void SetArithInsert1(uint insert);
        public abstract void SetArithInsert2(uint insert);
        public abstract void SetCommentInsert1(string insert);
        public abstract void SetCommentInsert2(string insert);
        public abstract void SetCommentInsert3(string insert);
        internal abstract bool SetEnvironmentValue(string name, string value);
        public abstract void SetValidInserts();
        public abstract void TraceConstructor(string objectId, string sccsid);
        public abstract void TraceData(string objectId, uint component, uint method, ushort level, string caption, uint offset, int length, byte[] buf);
        public abstract void TraceDetail(string objectId, uint component, uint method, ushort level, string text);
        public abstract bool TraceEnabled();
        public abstract void TraceEntry(string objectId, uint component, uint method, object[] parameters);
        public abstract void TraceEnvironment();
        public abstract void TraceException(string objectId, uint component, uint method, Exception ex);
        public abstract void TraceExit(string objectId, uint component, uint method, object result, int index, int returnCode);
        public abstract bool TraceStatus();
        public abstract void TraceUserData(string objectId, uint component, uint method, ushort level, string caption, uint offset, int length, byte[] buf);
    }
}

