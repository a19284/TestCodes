namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    internal class MQCONN : MQBase
    {
        internal structMQCONN mqconn;
        internal const byte OPT_MQCONNX = 2;
        internal const byte OPT_RECONNECTING = 4;
        internal const byte OPT_VERSIONS_SUPPORTED = 1;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        internal MQCONN()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.mqconn = new structMQCONN();
            this.mqconn.QMgrName = new byte[0x30];
            this.mqconn.ApplName = new byte[0x1c];
            this.mqconn.AcctToken = new byte[0x20];
        }

        private void CheckArrayLength()
        {
            uint method = 0x461;
            this.TrEntry(method);
            try
            {
                if ((this.mqconn.AcctToken != null) && (this.mqconn.AcctToken.Length != 0x20))
                {
                    this.mqconn.AcctToken = this.ResizeArrayToCorrectLength(this.mqconn.AcctToken, 0x20);
                }
                if ((this.mqconn.ApplName != null) && (this.mqconn.ApplName.Length != 0x1c))
                {
                    this.mqconn.ApplName = this.ResizeArrayToCorrectLength(this.mqconn.ApplName, 0x1c);
                }
                if ((this.mqconn.QMgrName != null) && (this.mqconn.QMgrName.Length != 0x30))
                {
                    this.mqconn.QMgrName = this.ResizeArrayToCorrectLength(this.mqconn.QMgrName, 0x30);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.mqconn);
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x6f;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            int result = 0;
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                Marshal.Copy(b, Offset, zero, length);
                this.mqconn = (structMQCONN) Marshal.PtrToStructure(zero, typeof(structMQCONN));
                Marshal.FreeCoTaskMem(zero);
                result = Offset + length;
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 110;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                this.CheckArrayLength();
                Marshal.StructureToPtr(this.mqconn, zero, false);
                Marshal.Copy(zero, b, Offset, length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, length);
            }
            return length;
        }

        internal string ApplName
        {
            set
            {
                if (value != string.Empty)
                {
                    if (value.Length > 0x1c)
                    {
                        base.GetBytesRightPad(value.Substring(value.Length - 0x1c, 0x1c), ref this.mqconn.ApplName);
                    }
                    else
                    {
                        base.GetBytesRightPad(value, ref this.mqconn.ApplName);
                    }
                }
                else
                {
                    base.GetBytesRightPad("C# Client", ref this.mqconn.ApplName);
                }
            }
        }

        internal string QmgrName
        {
            set
            {
                base.GetBytesRightPad(value, ref this.mqconn.QMgrName);
            }
        }
    }
}

