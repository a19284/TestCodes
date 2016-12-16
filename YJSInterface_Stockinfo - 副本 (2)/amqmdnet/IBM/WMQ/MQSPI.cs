namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPI : MQBase
    {
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal structMQSPI spi;

        public MQSPI() : this(0, 0, 0)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        internal MQSPI(int VerbId, int OutStructVersion, int OutStructLength)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { VerbId, OutStructVersion, OutStructLength });
            this.spi = new structMQSPI();
            this.spi.verbId = VerbId;
            this.spi.outStructVersion = OutStructVersion;
            this.spi.outStructLength = OutStructLength;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spi);
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x278;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int result = 0;
            int length = this.GetLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                Marshal.Copy(b, Offset, zero, length);
                this.spi = (structMQSPI) Marshal.PtrToStructure(zero, typeof(structMQSPI));
                Marshal.FreeCoTaskMem(zero);
                result = Offset + length;
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x277;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                Marshal.StructureToPtr(this.spi, zero, false);
                Marshal.Copy(zero, b, Offset, length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, length);
            }
            return length;
        }

        internal int OutStructLength
        {
            get
            {
                return this.spi.outStructLength;
            }
            set
            {
                this.spi.outStructLength = value;
            }
        }

        internal int OutStructVersion
        {
            get
            {
                return this.spi.outStructVersion;
            }
            set
            {
                this.spi.outStructVersion = value;
            }
        }

        public structMQSPI Spi
        {
            get
            {
                return this.spi;
            }
            set
            {
                this.spi = value;
            }
        }

        internal int VerbId
        {
            get
            {
                return this.spi.verbId;
            }
            set
            {
                this.spi.verbId = value;
            }
        }
    }
}

