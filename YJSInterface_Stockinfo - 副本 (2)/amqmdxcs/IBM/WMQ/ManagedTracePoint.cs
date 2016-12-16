namespace IBM.WMQ
{
    using System;
    using System.IO;

    internal class ManagedTracePoint
    {
        internal uint component;
        internal uint function;
        internal string parms;
        internal object result;
        internal int retCode;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private const string TRC_DATA_FORMAT = "    {0} data={1}";
        private const string TRC_ENTRY_FORMAT = "{{  {0}";
        private const string TRC_ENTRY_FORMAT_PARMS = "{{  {0} inputs {1}";
        private const string TRC_EXIT_FORMAT_ERROR = "}}! {0} rc=(Unknown({1}))";
        private const string TRC_EXIT_FORMAT_OK = "}}  {0} rc=OK";
        private const string TRC_EXIT_FORMAT_RESULT = "}}  {0} rc=OK returns [{1}]";
        internal PointType trcType;

        internal ManagedTracePoint()
        {
        }

        internal ManagedTracePoint(uint comp, uint func, object[] parameters)
        {
            this.SetEntry(comp, func, parameters);
        }

        internal ManagedTracePoint(uint comp, uint func, object res, int rc)
        {
            this.SetExit(comp, func, res, rc);
        }

        internal static int CalculateHistoryIndent(ManagedTracePoint[] traceHistory, int traceHistoryPos, int startIndent)
        {
            int num = startIndent;
            int num2 = startIndent;
            int index = traceHistoryPos - 1;
            do
            {
                if (index == -1)
                {
                    index = traceHistory.Length - 1;
                }
                if (traceHistory[index] == null)
                {
                    break;
                }
                switch (traceHistory[index].trcType)
                {
                    case PointType.Entry:
                        num--;
                        break;

                    case PointType.Exit:
                        num++;
                        break;
                }
                if (num < num2)
                {
                    num2 = num;
                }
            }
            while (index-- != traceHistoryPos);
            return (num - num2);
        }

        internal string Format()
        {
            return this.Format(this.trcType);
        }

        internal string Format(PointType formatType)
        {
            string functionName = FUNCID.GetFunctionName(this.component, this.function);
            if (formatType == PointType.NameOnly)
            {
                return functionName;
            }
            StringWriter writer = new StringWriter();
            switch (formatType)
            {
                case PointType.Entry:
                    if (this.parms == null)
                    {
                        writer.Write("{{  {0}", functionName);
                    }
                    else
                    {
                        writer.Write("{{  {0} inputs {1}", functionName, this.parms);
                    }
                    break;

                case PointType.Exit:
                    if (this.result == null)
                    {
                        if (this.retCode == 0)
                        {
                            writer.Write("}}  {0} rc=OK", functionName);
                        }
                        else
                        {
                            writer.Write("}}! {0} rc=(Unknown({1}))", functionName, this.retCode);
                        }
                    }
                    else
                    {
                        writer.Write("}}  {0} rc=OK returns [{1}]", functionName, this.result);
                    }
                    break;

                default:
                    writer.Write("    {0} data={1}", functionName, this.retCode);
                    break;
            }
            return writer.ToString();
        }

        internal void SetEntry(uint comp, uint func, object[] parameters)
        {
            this.component = comp;
            this.function = func;
            if (parameters != null)
            {
                this.parms = "";
                for (int i = 0; i < parameters.Length; i++)
                {
                    this.parms = this.parms + " [";
                    if (parameters[i] != null)
                    {
                        this.parms = this.parms + parameters[i];
                    }
                    else
                    {
                        this.parms = this.parms + "null";
                    }
                    this.parms = this.parms + "]";
                }
            }
            this.trcType = PointType.Entry;
        }

        internal void SetExit(uint comp, uint func, object res, int rc)
        {
            this.component = comp;
            this.function = func;
            this.retCode = rc;
            this.result = res;
            this.trcType = PointType.Exit;
        }

        internal enum PointType
        {
            None,
            Entry,
            Exit,
            Data,
            NameOnly
        }
    }
}

