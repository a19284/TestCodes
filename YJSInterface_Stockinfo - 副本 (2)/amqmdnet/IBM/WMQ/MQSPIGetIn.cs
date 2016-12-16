namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIGetIn : MQBase
    {
        public const int MAX_SUPPORTED_VERSION = 2;
        private static readonly byte[] rfpVB_ID_GET_IN = new byte[] { 0x53, 80, 0x47, 0x49 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPIGETIN spiGetIn;

        public MQSPIGetIn() : this(2)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIGetIn(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiGetIn = new SPIGETIN();
            this.spiGetIn.ID = rfpVB_ID_GET_IN;
            this.Version = version;
            this.spiGetIn.options = 0;
            this.spiGetIn.batchSize = 0;
            this.spiGetIn.batchInterval = 0;
            this.spiGetIn.maxMsgLength = 0;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiGetIn);
        }

        internal int GetVersionLength()
        {
            uint method = 0x281;
            this.TrEntry(method);
            switch (this.Version)
            {
                case 1:
                    base.TrExit(method, 0x18, 1);
                    return 0x18;

                case 2:
                    base.TrExit(method, 0x1c, 2);
                    return 0x1c;
            }
            base.TrExit(method, 0, 3);
            return 0;
        }

        public byte[] ToBuffer()
        {
            uint method = 0x282;
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
            uint method = 0x283;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            this.spiGetIn.length = this.GetVersionLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiGetIn, zero, false);
                Marshal.Copy(zero, b, Offset, this.spiGetIn.length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, this.spiGetIn.length);
            }
            return this.spiGetIn.length;
        }

        public int BatchInterval
        {
            get
            {
                return this.spiGetIn.batchInterval;
            }
            set
            {
                this.spiGetIn.batchInterval = value;
            }
        }

        public int BatchSize
        {
            get
            {
                return this.spiGetIn.batchSize;
            }
            set
            {
                this.spiGetIn.batchSize = value;
            }
        }

        public int Length
        {
            get
            {
                return this.GetVersionLength();
            }
        }

        public int MaxMsgLength
        {
            get
            {
                return this.spiGetIn.maxMsgLength;
            }
            set
            {
                this.spiGetIn.maxMsgLength = value;
            }
        }

        public int Options
        {
            get
            {
                if (this.Version >= 2)
                {
                    return this.spiGetIn.options;
                }
                return 0;
            }
            set
            {
                this.Version = 2;
                this.spiGetIn.options = value;
            }
        }

        public SPIGETIN StructSpiGetIn
        {
            get
            {
                return this.spiGetIn;
            }
            set
            {
                this.spiGetIn = value;
            }
        }

        public int Version
        {
            get
            {
                return this.spiGetIn.version;
            }
            set
            {
                if (this.spiGetIn.version < value)
                {
                    this.spiGetIn.version = Math.Min(value, 2);
                }
            }
        }
    }
}

