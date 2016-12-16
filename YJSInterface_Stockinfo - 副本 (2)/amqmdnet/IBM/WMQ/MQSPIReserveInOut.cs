namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIReserveInOut : MQBase
    {
        internal const int MAX_SUPPORTED_VERSION = 1;
        private static readonly byte[] rfpVB_ID_RESERVE_INOUT = new byte[] { 0x53, 80, 0x52, 0x55 };
        private static readonly byte[] rfpVB_ID_RESERVE_INPOUT_EBCDIC = new byte[] { 0xe2, 0xd7, 0xe1, 0xe4 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPIRESERVEINOUTHDR spiInOutHdr;

        public MQSPIReserveInOut() : this(1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIReserveInOut(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiInOutHdr = new SPIRESERVEINOUTHDR();
            this.spiInOutHdr.ID = rfpVB_ID_RESERVE_INOUT;
            this.spiInOutHdr.length = this.GetHdrLength();
            this.Version = version;
        }

        private int GetHdrLength()
        {
            return Marshal.SizeOf(this.spiInOutHdr);
        }

        private int GetHdrVersionLength()
        {
            uint method = 0x2af;
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
            return Marshal.SizeOf(this.spiInOutHdr);
        }

        internal int GetVersionLength()
        {
            return this.GetHdrVersionLength();
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 690;
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
                this.spiInOutHdr = (SPIRESERVEINOUTHDR) Marshal.PtrToStructure(zero, typeof(SPIRESERVEINOUTHDR));
                Offset += this.GetHdrVersionLength();
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, Offset);
            }
            return Offset;
        }

        internal byte[] ToBuffer()
        {
            uint method = 0x2b0;
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
            uint method = 0x2b1;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int hdrVersionLength = this.GetHdrVersionLength();
            this.spiInOutHdr.length = this.GetVersionLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiInOutHdr, zero, false);
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
                return this.spiInOutHdr.version;
            }
            set
            {
                if (this.spiInOutHdr.version < value)
                {
                    this.spiInOutHdr.version = Math.Min(value, 1);
                }
            }
        }
    }
}

