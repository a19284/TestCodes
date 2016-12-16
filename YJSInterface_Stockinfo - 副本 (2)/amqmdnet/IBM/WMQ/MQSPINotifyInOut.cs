namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Runtime.InteropServices;

    public class MQSPINotifyInOut : NmqiObject
    {
        internal const int MAX_SUPPORTED_VERSION = 1;
        private static readonly byte[] rfpVB_ID_NOTIFY_INOUT = new byte[] { 0x53, 80, 0x4e, 0x55 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPINOTIFYINOUT spiNotifyInOut;

        public MQSPINotifyInOut(NmqiEnvironment nmqiEnv) : this(nmqiEnv, 1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv });
        }

        public MQSPINotifyInOut(NmqiEnvironment nmqiEnv, int version) : base(nmqiEnv)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv, version });
            this.spiNotifyInOut = new SPINOTIFYINOUT();
            this.spiNotifyInOut.ID = rfpVB_ID_NOTIFY_INOUT;
            this.Version = version;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiNotifyInOut);
        }

        internal int GetVersionLength()
        {
            return this.GetLength();
        }

        public int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x424;
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
                this.spiNotifyInOut = (SPINOTIFYINOUT) Marshal.PtrToStructure(zero, typeof(SPINOTIFYINOUT));
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
            uint method = 0x422;
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
            uint method = 0x423;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            this.spiNotifyInOut.length = this.GetVersionLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiNotifyInOut, zero, false);
                Marshal.Copy(zero, b, Offset, this.spiNotifyInOut.length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, this.spiNotifyInOut.length);
            }
            return this.spiNotifyInOut.length;
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
                return this.spiNotifyInOut.version;
            }
            set
            {
                if (this.spiNotifyInOut.version < value)
                {
                    this.spiNotifyInOut.version = Math.Min(value, 1);
                }
            }
        }
    }
}

