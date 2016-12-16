namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIPutIn : MQBase
    {
        public const int MAX_SUPPORTED_VERSION = 2;
        internal byte[] msg;
        private static readonly byte[] rfpVB_ID_PUT_IN = new byte[] { 0x53, 80, 80, 0x49 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPIPUTIN spiPutIn;

        public MQSPIPutIn() : this(2)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIPutIn(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiPutIn = new SPIPUTIN();
            this.spiPutIn.ID = rfpVB_ID_PUT_IN;
            this.Version = version;
            this.spiPutIn.options = 0;
            this.spiPutIn.msgLength = 0;
            this.spiPutIn.msgIdReservation = 0;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiPutIn);
        }

        internal int GetVersionLength()
        {
            uint method = 0x28d;
            this.TrEntry(method);
            switch (this.Version)
            {
                case 1:
                    base.TrExit(method, 20, 1);
                    return 20;

                case 2:
                    base.TrExit(method, 0x18, 2);
                    return 0x18;
            }
            base.TrExit(method, 0, 3);
            return 0;
        }

        public byte[] ToBuffer()
        {
            uint method = 0x28e;
            this.TrEntry(method);
            byte[] b = new byte[this.GetVersionLength() + this.spiPutIn.msgLength];
            try
            {
                if (b != null)
                {
                    int dstOffset = this.WriteStruct(b, 0);
                    Buffer.BlockCopy(this.Msg, 0, b, dstOffset, this.spiPutIn.msgLength);
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
            uint method = 0x28f;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int versionLength = this.GetVersionLength();
            this.spiPutIn.length = this.spiPutIn.msgLength + versionLength;
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiPutIn, zero, false);
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

        public byte[] Msg
        {
            get
            {
                return this.msg;
            }
            set
            {
                this.msg = value;
            }
        }

        public int MsgIdReservation
        {
            get
            {
                if (this.Version >= 2)
                {
                    return this.spiPutIn.msgIdReservation;
                }
                return 0;
            }
            set
            {
                this.Version = 2;
                this.spiPutIn.msgIdReservation = value;
            }
        }

        public int MsgLength
        {
            get
            {
                return this.spiPutIn.msgLength;
            }
            set
            {
                this.spiPutIn.msgLength = value;
            }
        }

        public int Options
        {
            get
            {
                return this.spiPutIn.options;
            }
            set
            {
                this.spiPutIn.options = value;
            }
        }

        public int Version
        {
            get
            {
                return this.spiPutIn.version;
            }
            set
            {
                if (this.spiPutIn.version < value)
                {
                    this.spiPutIn.version = Math.Min(value, 2);
                }
            }
        }
    }
}

