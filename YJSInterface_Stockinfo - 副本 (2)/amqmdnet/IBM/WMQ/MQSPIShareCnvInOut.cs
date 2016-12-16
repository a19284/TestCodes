namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIShareCnvInOut : MQBase
    {
        internal const int MAX_SUPPORTED_VERSION = 1;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPISHARECNVINOUTHDR spiShareCnvInOut;

        public MQSPIShareCnvInOut() : this(1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIShareCnvInOut(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiShareCnvInOut = new SPISHARECNVINOUTHDR();
            this.spiShareCnvInOut.ID = new byte[] { 0x53, 80, 0x43, 0x55 };
            this.Version = version;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiShareCnvInOut);
        }

        internal int GetVersionLength()
        {
            return this.GetLength();
        }

        public byte[] ToBuffer()
        {
            uint method = 0x538;
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
            uint method = 0x539;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            this.spiShareCnvInOut.length = this.GetVersionLength();
            int length = this.spiShareCnvInOut.length;
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiShareCnvInOut, zero, false);
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
                return this.spiShareCnvInOut.version;
            }
            set
            {
                if (this.spiShareCnvInOut.version < value)
                {
                    this.spiShareCnvInOut.version = Math.Min(value, 1);
                }
            }
        }
    }
}

