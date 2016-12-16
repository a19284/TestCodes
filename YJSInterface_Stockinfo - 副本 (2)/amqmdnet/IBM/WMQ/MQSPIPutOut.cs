namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIPutOut : MQBase
    {
        public const int MAX_SUPPORTED_VERSION = 1;
        private static readonly byte[] rfpVB_ID_PUT_OUT = new byte[] { 0x53, 80, 80, 0x4f };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPIPUTOUT spiPutOut;

        public MQSPIPutOut() : this(1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIPutOut(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiPutOut = new SPIPUTOUT();
            this.spiPutOut.ID = rfpVB_ID_PUT_OUT;
            this.Version = version;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiPutOut);
        }

        internal int GetVersionLength()
        {
            return this.GetLength();
        }

        public int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x296;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            int result = 0;
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                if (length > (b.Length - Offset))
                {
                    length = b.Length - Offset;
                }
                Marshal.Copy(b, Offset, zero, length);
                this.spiPutOut = (SPIPUTOUT) Marshal.PtrToStructure(zero, typeof(SPIPUTOUT));
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
            uint method = 660;
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
            uint method = 0x295;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            this.spiPutOut.length = this.GetVersionLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiPutOut, zero, false);
                Marshal.Copy(zero, b, Offset, this.spiPutOut.length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, this.spiPutOut.length);
            }
            return this.spiPutOut.length;
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
                return this.spiPutOut.version;
            }
            set
            {
                if (this.spiPutOut.version < value)
                {
                    this.spiPutOut.version = Math.Min(value, 1);
                }
            }
        }
    }
}

