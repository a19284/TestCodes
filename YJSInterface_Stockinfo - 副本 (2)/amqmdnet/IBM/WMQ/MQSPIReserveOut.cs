namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIReserveOut : MQBase
    {
        internal const int MAX_SUPPORTED_VERSION = 1;
        private static readonly byte[] rfpVB_ID_RESERVE_OUT = new byte[] { 0x53, 80, 0x52, 0x4f };
        private static readonly byte[] rfpVB_ID_RESERVE_OUT_EBCDIC = new byte[] { 0xe2, 0xd7, 0xe1, 0xd6 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPIRESERVEOUT spiReserveOut;

        public MQSPIReserveOut() : this(1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIReserveOut(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiReserveOut = new SPIRESERVEOUT();
            this.spiReserveOut.ID = rfpVB_ID_RESERVE_OUT;
            this.Version = version;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiReserveOut);
        }

        internal int GetVersionLength()
        {
            return this.GetLength();
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x2b5;
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
                this.spiReserveOut = (SPIRESERVEOUT) Marshal.PtrToStructure(zero, typeof(SPIRESERVEOUT));
                length = this.GetVersionLength();
                Marshal.FreeCoTaskMem(zero);
                result = Offset + length;
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        internal byte[] ToBuffer()
        {
            uint method = 0x2b3;
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
            uint method = 0x2b4;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            this.spiReserveOut.length = this.GetVersionLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiReserveOut, zero, false);
                Marshal.Copy(zero, b, Offset, this.spiReserveOut.length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, this.spiReserveOut.length);
            }
            return this.spiReserveOut.length;
        }

        public int ActualReservation
        {
            get
            {
                return this.spiReserveOut.actualReservation;
            }
        }

        public byte[] BaseReservationTag
        {
            get
            {
                return this.spiReserveOut.baseReservationTag;
            }
        }

        public int Length
        {
            get
            {
                return this.GetVersionLength();
            }
        }

        public int TagIncrementOffset
        {
            get
            {
                return this.spiReserveOut.tagIncrementOffset;
            }
        }

        public int Version
        {
            get
            {
                return this.spiReserveOut.version;
            }
            set
            {
                if (this.spiReserveOut.version < value)
                {
                    this.spiReserveOut.version = Math.Min(value, 1);
                }
            }
        }
    }
}

