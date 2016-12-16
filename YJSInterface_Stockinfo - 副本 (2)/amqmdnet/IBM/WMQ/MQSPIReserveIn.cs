namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIReserveIn : MQBase
    {
        internal const int MAX_SUPPORTED_VERSION = 1;
        private static readonly byte[] rfpVB_ID_RESERVE_IN = new byte[] { 0x53, 80, 0x52, 0x49 };
        private static readonly byte[] rfpVB_ID_RESERVE_IN_EBCDIC = new byte[] { 0xe2, 0xd7, 0xe1, 0xc9 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPIRESERVEIN spiReserveIn;

        public MQSPIReserveIn() : this(1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIReserveIn(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiReserveIn = new SPIRESERVEIN();
            this.spiReserveIn.ID = rfpVB_ID_RESERVE_IN;
            this.Version = version;
            this.TagReservation = 0;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiReserveIn);
        }

        internal int GetVersionLength()
        {
            uint method = 0x2ad;
            this.TrEntry(method);
            if (this.Version == 1)
            {
                int result = Marshal.SizeOf(this.spiReserveIn);
                base.TrExit(method, result, 1);
                return result;
            }
            base.TrExit(method, 0, 2);
            return 0;
        }

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x2ae;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int versionLength = this.GetVersionLength();
            try
            {
                this.spiReserveIn.length = versionLength;
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiReserveIn, zero, false);
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

        public int TagReservation
        {
            get
            {
                return this.spiReserveIn.tagReservation;
            }
            set
            {
                this.spiReserveIn.tagReservation = value;
            }
        }

        public int TagSize
        {
            get
            {
                return this.spiReserveIn.tagSize;
            }
            set
            {
                this.spiReserveIn.tagSize = value;
            }
        }

        public int Version
        {
            get
            {
                return this.spiReserveIn.version;
            }
            set
            {
                if (this.spiReserveIn.version < value)
                {
                    this.spiReserveIn.version = Math.Min(value, 1);
                }
            }
        }
    }
}

