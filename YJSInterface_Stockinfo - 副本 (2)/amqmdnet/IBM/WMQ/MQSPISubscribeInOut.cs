namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Runtime.InteropServices;

    public class MQSPISubscribeInOut : NmqiObject
    {
        internal const int MAX_SUPPORTED_VERSION = 1;
        private static readonly byte[] rfpVB_ID_SUBSCRIBE_INOUT = new byte[] { 0x53, 80, 0x42, 0x55 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPISUBSCRIBEINOUTHDR spiInOut;

        public MQSPISubscribeInOut(NmqiEnvironment nmqiEnv) : this(nmqiEnv, 1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv });
        }

        public MQSPISubscribeInOut(NmqiEnvironment nmqiEnv, int version) : base(nmqiEnv)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { nmqiEnv, version });
            this.spiInOut = new SPISUBSCRIBEINOUTHDR();
            this.spiInOut.ID = rfpVB_ID_SUBSCRIBE_INOUT;
            this.spiInOut.length = this.GetHdrLength();
            this.Version = version;
        }

        private int GetHdrLength()
        {
            return Marshal.SizeOf(this.spiInOut);
        }

        private int GetHdrVersionLength()
        {
            uint method = 0x433;
            this.TrEntry(method);
            if (this.Version == 1)
            {
                int hdrLength = this.GetHdrLength();
                base.TrExit(method, hdrLength, 1);
                return hdrLength;
            }
            base.TrExit(method, 0, 2);
            return 0;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiInOut);
        }

        internal int GetVersionLength()
        {
            return this.GetHdrVersionLength();
        }

        public int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x436;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int hdrLength = this.GetHdrLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                if (hdrLength > (b.Length - Offset))
                {
                    hdrLength = b.Length - Offset;
                }
                Marshal.Copy(b, Offset, zero, hdrLength);
                this.spiInOut = (SPISUBSCRIBEINOUTHDR) Marshal.PtrToStructure(zero, typeof(SPISUBSCRIBEINOUTHDR));
                Offset += this.GetHdrVersionLength();
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, Offset);
            }
            return Offset;
        }

        public byte[] ToBuffer()
        {
            uint method = 0x434;
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

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x435;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int hdrVersionLength = this.GetHdrVersionLength();
            this.spiInOut.length = this.GetVersionLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiInOut, zero, false);
                Marshal.Copy(zero, b, Offset, hdrVersionLength);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, hdrVersionLength);
            }
            return hdrVersionLength;
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
                return this.spiInOut.version;
            }
            set
            {
                if (this.spiInOut.version < value)
                {
                    this.spiInOut.version = Math.Min(value, 1);
                }
            }
        }
    }
}

