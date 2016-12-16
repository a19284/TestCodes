namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSPISyncpointInOut : MQBase
    {
        internal const int MAX_SUPPORTED_VERSION = 1;
        private static readonly byte[] rfpVB_ID_SYNCPOINT_INOUT = new byte[] { 0x53, 80, 0x53, 0x55 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal SPISYNCPOINTINOUT spiSyncpointInOut;

        public MQSPISyncpointInOut() : this(1)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQSPISyncpointInOut(int version)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { version });
            this.spiSyncpointInOut = new SPISYNCPOINTINOUT();
            this.spiSyncpointInOut.ID = rfpVB_ID_SYNCPOINT_INOUT;
            this.Version = version;
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.spiSyncpointInOut);
        }

        internal int GetVersionLength()
        {
            return this.GetLength();
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x2ba;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            int result = 0;
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                if (length > (b.Length - Offset))
                {
                    length = b.Length - Offset;
                }
                Marshal.Copy(b, Offset, zero, length);
                this.spiSyncpointInOut = (SPISYNCPOINTINOUT) Marshal.PtrToStructure(zero, typeof(SPISYNCPOINTINOUT));
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
            uint method = 0x2b8;
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
            uint method = 0x2b9;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int versionLength = this.GetVersionLength();
            this.spiSyncpointInOut.length = versionLength;
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.spiSyncpointInOut, zero, false);
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

        public int Version
        {
            get
            {
                return this.spiSyncpointInOut.version;
            }
            set
            {
                if (this.spiSyncpointInOut.version < value)
                {
                    this.spiSyncpointInOut.version = Math.Min(value, 1);
                }
            }
        }
    }
}

