namespace IBM.WMQ
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;

    internal class ExternalSourceTrace : TextWriterTraceListener
    {
        private static MQExternalSourceTracer mqTrace;
        private const int TRACE_TEXT_MAX_BUFFER = 0x400;

        static ExternalSourceTrace()
        {
            if (mqTrace == null)
            {
                mqTrace = new MQExternalSourceTracer();
            }
        }

        public override void Fail(string message)
        {
            mqTrace.TrException(message, null);
        }

        public override void Fail(string message, string detailMessage)
        {
            mqTrace.TrException(message, null);
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object datas)
        {
            if (datas is string)
            {
                if (((string) datas).Length < 0x400)
                {
                    this.TraceData(eventCache, source, eventType, id, Convert.ToString(datas));
                }
            }
            else
            {
                this.TraceData(eventCache, source, eventType, id, new object[] { datas });
            }
        }

        public void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string dataS)
        {
            uint fid = 0x641;
            mqTrace.TrEnter(fid);
            try
            {
                string msg = null;
                if (eventCache != null)
                {
                    msg = msg + "ThreadId:" + eventCache.ThreadId;
                }
                else
                {
                    msg = msg + "ThreadId:" + Thread.CurrentThread.ManagedThreadId;
                }
                if (source != null)
                {
                    msg = msg + ", MS_Source:" + source;
                }
                else
                {
                    msg = msg + ", MS_Source: NULL";
                }
                if ((dataS == null) || (dataS.Length == 0))
                {
                    dataS = "NULL";
                }
                msg = msg + "Message: " + dataS;
                mqTrace.TrTextMsg(fid, msg);
            }
            finally
            {
                mqTrace.TrOut(fid);
            }
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] datas)
        {
            uint fid = 0x641;
            mqTrace.TrEnter(fid);
            try
            {
                MemoryStream stream = new MemoryStream();
                string caption = null;
                if (eventCache != null)
                {
                    caption = caption + "ThreadId:" + eventCache.ThreadId;
                }
                else
                {
                    caption = caption + "ThreadId:" + Thread.CurrentThread.ManagedThreadId;
                }
                if (source != null)
                {
                    caption = caption + ", MS_Source:" + source;
                }
                else
                {
                    caption = caption + ", MS_Source: NULL";
                }
                if (datas != null)
                {
                    foreach (object obj2 in datas)
                    {
                        if (obj2 != null)
                        {
                            byte[] bytes;
                            if (obj2 is byte[])
                            {
                                bytes = (byte[]) obj2;
                            }
                            else if (obj2 is byte)
                            {
                                bytes = new byte[] { (byte) obj2 };
                            }
                            else
                            {
                                bytes = Encoding.ASCII.GetBytes(obj2.ToString());
                            }
                            stream.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
                mqTrace.TrData(fid, caption, stream.ToArray());
                stream.Dispose();
            }
            finally
            {
                mqTrace.TrOut(fid);
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            this.TraceData(eventCache, source, eventType, id, string.Empty);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (message.Length < 0x400)
            {
                this.TraceData(eventCache, source, eventType, id, message);
            }
            else
            {
                this.TraceData(eventCache, source, eventType, id, new object[] { message });
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            this.TraceData(eventCache, source, eventType, id, args);
        }

        public override void Write(object o)
        {
            this.Write(o, null);
        }

        public override void Write(string message)
        {
            this.Write(message, null);
        }

        public override void Write(object o, string category)
        {
            this.TraceData(null, category, TraceEventType.Information, 0, o);
        }

        public override void Write(string message, string category)
        {
            this.TraceData(null, category, TraceEventType.Information, 0, message);
        }

        public override void WriteLine(object o)
        {
            this.Write(o);
        }

        public override void WriteLine(string message)
        {
            this.Write(message);
        }

        public override void WriteLine(object o, string category)
        {
            this.Write(o, category);
        }

        public override void WriteLine(string message, string category)
        {
            this.Write(message, category);
        }
    }
}

