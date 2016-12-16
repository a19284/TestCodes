namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIShareCnvIn : MQBase
    {
        internal const int MAX_SUPPORTED_VERSION = 1;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPISHARECNVINHDR spiShareCnvIn;

        public MQSPIShareCnvIn() : this(1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIShareCnvIn(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiShareCnvIn = new SPISHARECNVINHDR();
            this.spiShareCnvIn.ID = new byte[] { 0x53, 80, 0x43, 0x49 };
            this.Version = version;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiShareCnvIn);
        }

        internal int GetVersionLength()
        {
            return this.GetLength();
        }

        public byte[] ToBuffer()
        {
            uint method = 0x536;
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
            uint method = 0x537;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            this.spiShareCnvIn.length = this.GetVersionLength();
            int length = this.spiShareCnvIn.length;
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiShareCnvIn, zero, false);
                Marshal.Copy(zero, b, Offset, length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, length);
            }
            return length;
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
                return this.spiShareCnvIn.version;
            }
            set
            {
                if (this.spiShareCnvIn.version < value)
                {
                    this.spiShareCnvIn.version = Math.Min(value, 1);
                }
            }
        }
    }
}

