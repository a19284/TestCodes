namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPIQueryOut : MQBase
    {
        internal const int MAX_SUPPORTED_VERSION = 2;
        private static readonly byte[] rfpVB_ID_QUERY_OUT = new byte[] { 0x53, 80, 0x51, 0x4f };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPIQUERYOUTHDR spiQueryOutHdr;
        internal SPIQUERYVERB[] verbArray;

        public MQSPIQueryOut() : this(1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPIQueryOut(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiQueryOutHdr = new SPIQUERYOUTHDR();
            this.spiQueryOutHdr.ID = rfpVB_ID_QUERY_OUT;
            this.Version = version;
            this.spiQueryOutHdr.arraySize = 12;
            this.verbArray = new SPIQUERYVERB[this.spiQueryOutHdr.arraySize];
            for (int i = 0; i < this.verbArray.Length; i++)
            {
                SPIQUERYVERB spiqueryverb1 = this.verbArray[i];
            }
        }

        private int GetHdrLength()
        {
            return Marshal.SizeOf(this.spiQueryOutHdr);
        }

        private int GetHdrVersionLength()
        {
            return this.GetHdrLength();
        }

        internal int GetLength()
        {
            return (Marshal.SizeOf(this.spiQueryOutHdr) + (Marshal.SizeOf(typeof(SPIQUERYVERB)) * this.verbArray.Length));
        }

        internal int GetVersionLength()
        {
            return this.GetLength();
        }

        public int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x29f;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int hdrLength = this.GetHdrLength();
            int result = 0;
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                if (hdrLength > (b.Length - Offset))
                {
                    hdrLength = b.Length - Offset;
                }
                Marshal.Copy(b, Offset, zero, hdrLength);
                this.spiQueryOutHdr = (SPIQUERYOUTHDR) Marshal.PtrToStructure(zero, typeof(SPIQUERYOUTHDR));
                hdrLength = this.GetHdrVersionLength();
                for (int i = 0; i < this.spiQueryOutHdr.arraySize; i++)
                {
                    int length = Marshal.SizeOf(this.verbArray[i]);
                    Marshal.Copy(b, Offset + hdrLength, zero, length);
                    this.verbArray[i] = (SPIQUERYVERB) Marshal.PtrToStructure(zero, typeof(SPIQUERYVERB));
                    hdrLength += length;
                }
                Marshal.FreeCoTaskMem(zero);
                result = Offset + hdrLength;
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        public bool SPISupported(int verbId, ref int maxInOutVersion, ref int maxInVersion, ref int maxOutVersion, ref int flags)
        {
            uint method = 0x29c;
            this.TrEntry(method, new object[] { verbId, (int) maxInOutVersion, (int) maxInVersion, (int) maxOutVersion, (int) flags });
            int num2 = 0;
            bool result = false;
            do
            {
                SPIQUERYVERB spiqueryverb = this.verbArray[num2++];
                if (spiqueryverb.verbId == verbId)
                {
                    maxInOutVersion = spiqueryverb.maxInOutVersion;
                    maxInVersion = spiqueryverb.maxInVersion;
                    maxOutVersion = spiqueryverb.maxOutVersion;
                    flags = spiqueryverb.flags;
                    result = true;
                    break;
                }
            }
            while (!result && (num2 < this.verbArray.Length));
            base.TrExit(method, result);
            return result;
        }

        public byte[] ToBuffer()
        {
            uint method = 0x29d;
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
            uint method = 670;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int hdrVersionLength = this.GetHdrVersionLength();
            this.spiQueryOutHdr.length = this.GetVersionLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiQueryOutHdr, zero, false);
                Marshal.Copy(zero, b, Offset, hdrVersionLength);
                for (int i = 0; i < this.verbArray.Length; i++)
                {
                    int length = Marshal.SizeOf(this.verbArray[i]);
                    Marshal.StructureToPtr(this.verbArray[i], zero, false);
                    Marshal.Copy(zero, b, Offset + hdrVersionLength, length);
                    hdrVersionLength += length;
                }
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
                return this.spiQueryOutHdr.version;
            }
            set
            {
                this.spiQueryOutHdr.version = Math.Min(value, 2);
            }
        }
    }
}

