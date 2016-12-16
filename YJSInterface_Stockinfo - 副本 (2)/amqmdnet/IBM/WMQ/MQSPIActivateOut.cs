namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIActivateOut : MQBase
    {
        public const int MAX_SUPPORTED_VERSION = 1;
        private static readonly byte[] rfpVB_ID_ACTIVATE_OUT = new byte[] { 0x53, 80, 0x41, 0x4f };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPIACTIVATEOUT spiActivateOut;

        public MQSPIActivateOut() : this(1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIActivateOut(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiActivateOut = new SPIACTIVATEOUT();
            this.spiActivateOut.ID = rfpVB_ID_ACTIVATE_OUT;
            this.Version = version;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiActivateOut);
        }

        internal int GetVersionLength()
        {
            return this.GetLength();
        }

        public int ReadStruct(byte[] b, int Offset)
        {
            uint method = 640;
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
                this.spiActivateOut = (SPIACTIVATEOUT) Marshal.PtrToStructure(zero, typeof(SPIACTIVATEOUT));
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
            uint method = 0x27e;
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
            uint method = 0x27f;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            this.spiActivateOut.length = this.GetVersionLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiActivateOut, zero, false);
                Marshal.Copy(zero, b, Offset, this.spiActivateOut.length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, this.spiActivateOut.length);
            }
            return this.spiActivateOut.length;
        }

        public int Length
        {
            get
            {
                return this.GetVersionLength();
            }
        }

        public SPIACTIVATEOUT StructSpiActivateOut
        {
            get
            {
                return this.spiActivateOut;
            }
            set
            {
                this.spiActivateOut = value;
            }
        }

        public int Version
        {
            get
            {
                return this.spiActivateOut.version;
            }
            set
            {
                if (this.spiActivateOut.version < value)
                {
                    this.spiActivateOut.version = Math.Min(value, 1);
                }
            }
        }
    }
}

