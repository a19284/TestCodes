namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Runtime.InteropServices;

    public class MQSPINotifyOut : NmqiObject
    {
        internal const int MAX_SUPPORTED_VERSION = 1;
        private static readonly byte[] rfpVB_ID_NOTIFY_INOUT = new byte[] { 0x53, 80, 0x4e, 0x4f };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPINOTIFYOUT spiNotifyOut;

        public MQSPINotifyOut(NmqiEnvironment nmqiEnv) : this(nmqiEnv, 1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv });
        }

        public MQSPINotifyOut(NmqiEnvironment nmqiEnv, int version) : base(nmqiEnv)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv, version });
            this.spiNotifyOut = new SPINOTIFYOUT();
            this.spiNotifyOut.ID = rfpVB_ID_NOTIFY_INOUT;
            this.Version = version;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiNotifyOut);
        }

        internal int GetVersionLength()
        {
            return this.GetLength();
        }

        public int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x427;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            int result = 0;
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                if (length > (b.Length - Offset))
                {
                    length = b.Length - Offset;
                }
                Marshal.Copy(b, Offset, zero, length);
                this.spiNotifyOut = (SPINOTIFYOUT) Marshal.PtrToStructure(zero, typeof(SPINOTIFYOUT));
                Marshal.FreeCoTaskMem(zero);
                length = this.GetVersionLength();
                result = Offset + length;
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        public byte[] ToBuffer()
        {
            uint method = 0x425;
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
            uint method = 0x426;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            this.spiNotifyOut.length = this.GetVersionLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiNotifyOut, zero, false);
                Marshal.Copy(zero, b, Offset, this.spiNotifyOut.length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, this.spiNotifyOut.length);
            }
            return this.spiNotifyOut.length;
        }

        public int Length
        {
            get
            {
                return this.GetVersionLength();
            }
        }

        public int Version
        {
            get
            {
                return this.spiNotifyOut.version;
            }
            set
            {
                if (this.spiNotifyOut.version < value)
                {
                    this.spiNotifyOut.version = Math.Min(value, 1);
                }
            }
        }
    }
}

