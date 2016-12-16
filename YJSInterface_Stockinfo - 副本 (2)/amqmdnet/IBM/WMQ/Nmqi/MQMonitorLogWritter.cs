namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;

    internal class MQMonitorLogWritter : NmqiObject, IDisposable
    {
        internal const string ActualTOutcome = "Actual state of the Transaction:";
        internal const string Bqual = "Local Transaction ID: ";
        internal const string CheckingQmgrRecovery = "Checking if transaction is listed as indount with Queuemanager";
        internal const string CompletedRecovery = "WMQ .NET XA Recovery Monitor completed recovering transactions";
        internal const string ConnectionDetails = "QMgr Connection Details: ";
        internal const string DTCOutCome = "Outcome of the transaction(as per DTC):";
        internal const string FoundIncompleteTrans = "Found one Inomplete Transaction from message,(ID):";
        internal const string Gtrid = "Global Transaction ID: ";
        private static string logfileName = "WMQDnetTransactionMonitor";
        internal const string MsgId = "MsgID of current message(recovery message) being processed ";
        private object mutex;
        private static string processId = null;
        private static string processName = null;
        internal const string QmgrlistedIndobt = "Queuemanager too has listed current transaction as indount";
        internal const string QueueDepth = "SYSTEM.DOTNET.XARECOVERY.QUEUE Current Depth : ";
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal const string StartedRecovery = "WMQ .NET XA Recovery Monitor has started running..";
        private static StreamWriter writer_ = null;

        static MQMonitorLogWritter()
        {
            int num = 0;
            string logfileName = MQMonitorLogWritter.logfileName;
            if (writer_ == null)
            {
                while (true)
                {
                    if (!CheckFileExists(logfileName))
                    {
                        break;
                    }
                    num++;
                    logfileName = MQMonitorLogWritter.logfileName + num.ToString();
                }
                writer_ = File.CreateText(logfileName + ".log");
                writer_.AutoFlush = true;
                Process currentProcess = Process.GetCurrentProcess();
                processId = currentProcess.Id.ToString();
                processName = currentProcess.ProcessName;
                currentProcess.Dispose();
                LogHeader();
            }
        }

        internal MQMonitorLogWritter(NmqiEnvironment env) : base(env)
        {
            this.mutex = new object();
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        private static bool CheckFileExists(string name)
        {
            return File.Exists(name + ".log");
        }

        internal void Close()
        {
            uint method = 0x51d;
            this.TrEntry(method);
            this.Dispose();
            base.TrExit(method);
        }

        public void Dispose()
        {
            uint method = 0x51e;
            this.TrEntry(method);
            if (writer_ != null)
            {
                writer_.Flush();
                writer_.Close();
                writer_ = null;
            }
            base.TrExit(method);
        }

        internal string GetHeader()
        {
            uint method = 0x51c;
            this.TrEntry(method);
            StringBuilder builder = new StringBuilder();
            builder.Append(DateTime.UtcNow.ToShortDateString() + ", " + DateTime.UtcNow.ToShortTimeString());
            builder.Append(" | ");
            builder.Append(processId);
            builder.Append(".");
            builder.Append(Thread.CurrentThread.ManagedThreadId);
            builder.Append(" | ");
            string result = builder.ToString();
            base.TrExit(method, result);
            return result;
        }

        private static void LogHeader()
        {
            writer_.WriteLine("************* Start Display Current Environment *************");
            writer_.WriteLine("Process Name       :- " + processName);
            writer_.WriteLine("Process Id         :- " + processId);
            writer_.WriteLine("Machine Name       :- " + Environment.MachineName);
            writer_.WriteLine("OS Version         :- " + Environment.OSVersion);
            writer_.WriteLine(".NET Version       :- " + Environment.Version);
            writer_.WriteLine("Product Long Name  :- WebSphere MQ for Windows");
            writer_.WriteLine("Version            :- " + CommonServices.GetVRMF());
            writer_.WriteLine("Installation Path  :- " + CommonServices.GetInstallationPath());
            writer_.WriteLine("Installation Name  :- " + CommonServices.GetInstallationName());
            writer_.WriteLine("Date/Time          :- " + DateTime.Now.ToString("F"));
            writer_.WriteLine("Date/Time(UTC)     :- " + DateTime.UtcNow.ToString("F"));
            writer_.WriteLine();
            writer_.WriteLine();
            writer_.WriteLine("TimeStamp           PID.TID        Data");
            writer_.WriteLine(new string('=', 100));
            writer_.WriteLine();
        }

        internal void WriteBytes(string caption, byte[] data)
        {
            StringBuilder builder = new StringBuilder();
            if (data != null)
            {
                builder.Append(caption);
            }
            if (data != null)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    builder.Append(data[i]);
                }
            }
            this.WriteLog(builder.ToString());
        }

        internal void WriteLog(string data)
        {
            uint method = 0x51b;
            this.TrEntry(method, new object[] { data });
            lock (this.mutex)
            {
                writer_.WriteLine(this.GetHeader() + data);
            }
            base.TrExit(method);
        }

        internal void WriteLog(string operation, string data)
        {
            uint method = 0x519;
            this.TrEntry(method, new object[] { operation, data });
            lock (this.mutex)
            {
                writer_.WriteLine(this.GetHeader() + operation + data);
            }
            base.TrExit(method);
        }

        internal void WriteTransactionID(Guid GlobalTID, string localTID)
        {
            uint method = 0x51a;
            this.TrEntry(method, new object[] { GlobalTID, localTID });
            string str = "[" + NmqiTools.ArrayToHexString(GlobalTID.ToByteArray()) + "]";
            string str2 = "[" + localTID + "]";
            lock (this.mutex)
            {
                writer_.WriteLine(this.GetHeader() + "Global Transaction ID: " + str);
                writer_.WriteLine(this.GetHeader() + "Local Transaction ID: " + str2);
            }
            base.TrExit(method);
        }

        internal void WriteXid(sbyte[] xid)
        {
            int index = 0;
            StringBuilder builder = new StringBuilder();
            builder.Append("[XID]");
            byte[] dst = new byte[xid.Length];
            Buffer.BlockCopy(xid, 0, dst, 0, xid.Length);
            int num2 = BitConverter.ToInt32(dst, 0);
            index += 4;
            builder.AppendLine(" formatId : " + num2);
            builder.Append("                                   ");
            int length = dst[index];
            index++;
            int num4 = dst[index];
            index++;
            builder.Append("[gtrid]: ");
            string str = NmqiTools.ToString(dst, index, length);
            builder.AppendLine(string.Format("{0,30}", str));
            index += length;
            builder.Append("                                   ");
            builder.Append("[bqual]: ");
            string str2 = NmqiTools.ToString(dst, index, num4);
            builder.Append(string.Format("{0,30}", str2));
            index += num4;
            this.WriteLog(builder.ToString());
        }
    }
}

