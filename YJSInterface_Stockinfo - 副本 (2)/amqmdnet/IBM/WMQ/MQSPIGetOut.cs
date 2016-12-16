namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIGetOut : MQBase
    {
        public const int MAX_SUPPORTED_VERSION = 3;
        internal byte[] msg;
        private static readonly byte[] rfpVB_ID_GET_OUT = new byte[] { 0x53, 80, 0x47, 0x4f };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPIGETOUTHDR spiGetOutHdr;
        internal MQSPIGetOpts spigo;

        public MQSPIGetOut() : this(3)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIGetOut(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiGetOutHdr = new SPIGETOUTHDR();
            this.spiGetOutHdr.ID = rfpVB_ID_GET_OUT;
            this.Version = version;
            this.spiGetOutHdr.getStatus = 0;
            this.spiGetOutHdr.msgLength = 0;
            this.spiGetOutHdr.reserved = 0;
            this.spiGetOutHdr.inherited = 0;
            this.spiGetOutHdr.qTimeHigh = 0;
            this.spiGetOutHdr.qTimeLow = 0;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiGetOutHdr);
        }

        internal int GetVersionLength()
        {
            uint method = 0x288;
            this.TrEntry(method);
            switch (this.Version)
            {
                case 1:
                    base.TrExit(method, 20, 1);
                    return 20;

                case 2:
                    base.TrExit(method, 0x1c, 2);
                    return 0x1c;

                case 3:
                {
                    int length = this.GetLength();
                    base.TrExit(method, length, 3);
                    return length;
                }
            }
            base.TrExit(method, 0, 4);
            return 0;
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x28c;
            this.TrEntry(method, new object[] { b, Offset });
            int result = 0;
            try
            {
                result = this.ReadStruct(b, Offset, false);
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        public int ReadStruct(byte[] b, int Offset, bool readMessage)
        {
            uint method = 0x28b;
            this.TrEntry(method, new object[] { b, Offset, readMessage });
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
                this.spiGetOutHdr = (SPIGETOUTHDR) Marshal.PtrToStructure(zero, typeof(SPIGETOUTHDR));
                length = this.GetVersionLength();
                if (this.spigo != null)
                {
                    this.spigo.QueueEmpty = this.GetStatus & 1;
                    if (this.Version >= 2)
                    {
                        this.spigo.Inherited = this.Inherited;
                    }
                    if (this.Version >= 3)
                    {
                        this.spigo.QTime =(ulong)(0xffffffffL * this.QTimeHigh) + this.QTimeLow;
                    }
                }
                if (readMessage)
                {
                    Offset += length;
                    length = Math.Min(this.Msg.Length, b.Length - Offset);
                    Buffer.BlockCopy(b, Offset, this.Msg, 0, length);
                }
                Marshal.FreeCoTaskMem(zero);
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
            uint method = 0x289;
            this.TrEntry(method);
            byte[] b = new byte[this.GetVersionLength() + this.spiGetOutHdr.msgLength];
            try
            {
                if (b != null)
                {
                    int dstOffset = this.WriteStruct(b, 0);
                    Buffer.BlockCopy(this.Msg, 0, b, dstOffset, this.spiGetOutHdr.msgLength);
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
            uint method = 650;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int versionLength = this.GetVersionLength();
            this.spiGetOutHdr.length = this.spiGetOutHdr.msgLength + versionLength;
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiGetOutHdr, zero, false);
                Marshal.Copy(zero, b, Offset, versionLength);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, versionLength);
            }
            return versionLength;
        }

        public int GetStatus
        {
            get
            {
                return this.spiGetOutHdr.getStatus;
            }
        }

        public int Inherited
        {
            get
            {
                if (this.Version >= 2)
                {
                    return this.spiGetOutHdr.inherited;
                }
                return 0;
            }
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

        public int MsgLength
        {
            get
            {
                return this.spiGetOutHdr.msgLength;
            }
            set
            {
                this.spiGetOutHdr.msgLength = value;
            }
        }

        public uint QTimeHigh
        {
            get
            {
                if (this.Version >= 3)
                {
                    return this.spiGetOutHdr.qTimeHigh;
                }
                return 0;
            }
        }

        public uint QTimeLow
        {
            get
            {
                if (this.Version >= 3)
                {
                    return this.spiGetOutHdr.qTimeLow;
                }
                return 0;
            }
        }

        public int Reserved
        {
            get
            {
                if (this.Version >= 2)
                {
                    return this.spiGetOutHdr.reserved;
                }
                return 0;
            }
            set
            {
                this.Version = 2;
                this.spiGetOutHdr.reserved = value;
            }
        }

        public MQSPIGetOpts SPIGetOpts
        {
            get
            {
                return this.spigo;
            }
            set
            {
                this.spigo = value;
            }
        }

        public SPIGETOUTHDR StructSpiGetOutHdr
        {
            get
            {
                return this.spiGetOutHdr;
            }
            set
            {
                this.spiGetOutHdr = value;
            }
        }

        public int Version
        {
            get
            {
                return this.spiGetOutHdr.version;
            }
            set
            {
                this.spiGetOutHdr.version = Math.Min(value, 3);
            }
        }
    }
}

