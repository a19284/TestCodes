namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIActivateInOut : MQBase
    {
        public const int MAX_SUPPORTED_VERSION = 1;
        private static readonly byte[] rfpVB_ID_ACTIVATE_INOUT = new byte[] { 0x53, 80, 0x41, 0x55 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPIACTIVATEINOUT spiActivateInOut;

        public MQSPIActivateInOut() : this(1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIActivateInOut(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiActivateInOut = new SPIACTIVATEINOUT();
            this.spiActivateInOut.ID = rfpVB_ID_ACTIVATE_INOUT;
            this.Version = version;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiActivateInOut);
        }

        internal int GetVersionLength()
        {
            return this.GetLength();
        }

        public int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x27d;
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
                this.spiActivateInOut = (SPIACTIVATEINOUT) Marshal.PtrToStructure(zero, typeof(SPIACTIVATEINOUT));
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
            uint method = 0x27b;
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
            uint method = 0x27c;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int versionLength = this.GetVersionLength();
            this.spiActivateInOut.length = versionLength;
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiActivateInOut, zero, false);
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

        public SPIACTIVATEINOUT StructSpiActivateInOut
        {
            get
            {
                return this.spiActivateInOut;
            }
            set
            {
                this.spiActivateInOut = value;
            }
        }

        public int Version
        {
            get
            {
                return this.spiActivateInOut.version;
            }
            set
            {
                if (this.spiActivateInOut.version < value)
                {
                    this.spiActivateInOut.version = Math.Min(value, 1);
                }
            }
        }
    }
}

