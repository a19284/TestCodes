namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPISyncpointOut : MQBase
    {
        internal const int MAX_SUPPORTED_VERSION = 1;
        private static readonly byte[] rfpVB_ID_SYNCPOINT_OUT = new byte[] { 0x53, 80, 0x53, 0x4f };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPISYNCPOINTOUT spiSyncpointOut;

        public MQSPISyncpointOut() : this(1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPISyncpointOut(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiSyncpointOut = new SPISYNCPOINTOUT();
            this.spiSyncpointOut.ID = rfpVB_ID_SYNCPOINT_OUT;
            this.Version = version;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiSyncpointOut);
        }

        internal int GetVersionLength()
        {
            return this.GetLength();
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x2bd;
            this.TrEntry(method, new object[] { b, Offset });
            int length = this.GetLength();
            IntPtr zero = IntPtr.Zero;
            int result = 0;
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                if (length > (b.Length - Offset))
                {
                    length = b.Length - Offset;
                }
                Marshal.Copy(b, Offset, zero, length);
                this.spiSyncpointOut = (SPISYNCPOINTOUT) Marshal.PtrToStructure(zero, typeof(SPISYNCPOINTOUT));
                Marshal.FreeCoTaskMem(zero);
                length = this.GetVersionLength();
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
            uint method = 0x2bb;
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
            uint method = 700;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            this.spiSyncpointOut.length = this.GetVersionLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiSyncpointOut, zero, false);
                Marshal.Copy(zero, b, Offset, this.spiSyncpointOut.length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, this.spiSyncpointOut.length);
            }
            return this.spiSyncpointOut.length;
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
                return this.spiSyncpointOut.version;
            }
            set
            {
                if (this.spiSyncpointOut.version < value)
                {
                    this.spiSyncpointOut.version = Math.Min(value, 1);
                }
            }
        }
    }
}

