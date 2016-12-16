namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIQueryInOut : MQBase
    {
        internal const int MAX_SUPPORTED_VERSION = 2;
        private static readonly byte[] rfpVB_ID_QUERY_INOUT = new byte[] { 0x53, 80, 0x51, 0x55 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPIQUERYINOUT spiQueryInOut;

        public MQSPIQueryInOut() : this(2)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIQueryInOut(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiQueryInOut = new SPIQUERYINOUT();
            this.spiQueryInOut.ID = rfpVB_ID_QUERY_INOUT;
            this.Version = version;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiQueryInOut);
        }

        internal int GetVersionLength()
        {
            return this.GetLength();
        }

        public int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x29b;
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
                this.spiQueryInOut = (SPIQUERYINOUT) Marshal.PtrToStructure(zero, typeof(SPIQUERYINOUT));
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
            uint method = 0x299;
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
            uint method = 0x29a;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int versionLength = this.GetVersionLength();
            this.spiQueryInOut.length = versionLength;
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiQueryInOut, zero, false);
                Marshal.Copy(zero, b, Offset, versionLength);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, versionLength);
            }
            return versionLength;
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
                return this.spiQueryInOut.version;
            }
            set
            {
                if (this.spiQueryInOut.version < value)
                {
                    this.spiQueryInOut.version = Math.Min(value, 2);
                }
            }
        }
    }
}

