namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIShareCnvOut : MQBase
    {
        internal const int MAX_SUPPORTED_VERSION = 1;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPISHARECNVOUTHDR spiShareCnvOut;

        public MQSPIShareCnvOut() : this(1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIShareCnvOut(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiShareCnvOut = new SPISHARECNVOUTHDR();
            this.spiShareCnvOut.ID = new byte[] { 0x53, 80, 0x43, 0x4f };
            this.Version = version;
            this.spiShareCnvOut.convPerSocket = 0;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiShareCnvOut);
        }

        internal int GetVersionLength()
        {
            return this.GetLength();
        }

        public int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x53c;
            this.TrEntry(method, new object[] { b, Offset });
            int length = this.GetLength();
            IntPtr zero = IntPtr.Zero;
            int result = 0;
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                if (length > (b.Length - Offset))
                {
                    length = b.Length - Offset;
                }
                Marshal.Copy(b, Offset, zero, length);
                this.spiShareCnvOut = (SPISHARECNVOUTHDR) Marshal.PtrToStructure(zero, typeof(SPISHARECNVOUTHDR));
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
            uint method = 0x53a;
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
            uint method = 0x53b;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            this.spiShareCnvOut.length = this.GetVersionLength();
            int length = this.spiShareCnvOut.length;
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiShareCnvOut, zero, false);
                Marshal.Copy(zero, b, Offset, length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, length);
            }
            return length;
        }

        public int ConvPerSocket
        {
            get
            {
                return this.spiShareCnvOut.convPerSocket;
            }
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
                return this.spiShareCnvOut.version;
            }
            set
            {
                if (this.spiShareCnvOut.version < value)
                {
                    this.spiShareCnvOut.version = Math.Min(value, 1);
                }
            }
        }
    }
}

