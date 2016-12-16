namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Runtime.InteropServices;

    public class MQSPINotifyIn : NmqiObject
    {
        internal const int MAX_SUPPORTED_VERSION = 1;
        private static readonly byte[] rfpVB_ID_NOTIFY_IN = new byte[] { 0x53, 80, 0x4e, 0x49 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPINOTIFYIN spiNotifyIn;

        public MQSPINotifyIn(NmqiEnvironment nmqiEnv) : this(nmqiEnv, 1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv });
        }

        public MQSPINotifyIn(NmqiEnvironment nmqiEnv, int version) : base(nmqiEnv)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv, version });
            this.spiNotifyIn = new SPINOTIFYIN();
            this.spiNotifyIn.ID = rfpVB_ID_NOTIFY_IN;
            this.Version = version;
            this.spiNotifyIn.options = 0;
            this.spiNotifyIn.reason = 0;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiNotifyIn);
        }

        internal int GetVersionLength()
        {
            return this.GetLength();
        }

        public byte[] ToBuffer()
        {
            uint method = 0x420;
            this.TrEntry(method);
            byte[] b = new byte[this.GetVersionLength()];
            try
            {
                if (b != null)
                {
                    this.WriteStruct(b, 0);
                }
            }
            finally
            {
                base.TrExit(method, b);
            }
            return b;
        }

        public int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x421;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            this.spiNotifyIn.length = this.GetVersionLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiNotifyIn, zero, false);
                Marshal.Copy(zero, b, Offset, this.spiNotifyIn.length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, this.spiNotifyIn.length);
            }
            return this.spiNotifyIn.length;
        }

        public byte[] ConnectionId
        {
            get
            {
                return this.spiNotifyIn.connectionId;
            }
            set
            {
                this.spiNotifyIn.connectionId = value;
            }
        }

        public int Length
        {
            get
            {
                return this.GetVersionLength();
            }
        }

        public int Options
        {
            get
            {
                return this.spiNotifyIn.options;
            }
            set
            {
                this.spiNotifyIn.options = value;
            }
        }

        public int Reason
        {
            get
            {
                return this.spiNotifyIn.reason;
            }
            set
            {
                this.spiNotifyIn.reason = value;
            }
        }

        public int Version
        {
            get
            {
                return this.spiNotifyIn.version;
            }
            set
            {
                if (this.spiNotifyIn.version < value)
                {
                    this.spiNotifyIn.version = Math.Min(value, 1);
                }
            }
        }
    }
}

