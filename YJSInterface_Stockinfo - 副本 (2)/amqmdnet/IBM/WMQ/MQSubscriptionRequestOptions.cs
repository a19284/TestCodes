namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQSubscriptionRequestOptions : MQBaseObject
    {
        private MQBase.MQSRO mqsro = new MQBase.MQSRO();
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public MQSubscriptionRequestOptions()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.ClearInvalidFields(0);
        }

        private void ClearInvalidFields(int Version)
        {
            uint method = 0x2c0;
            this.TrEntry(method, new object[] { Version });
            switch (Version)
            {
                case 0:
                    this.mqsro.StrucId = new byte[] { 0x53, 0x52, 0x4f, 0x20 };
                    this.mqsro.Options = 0;
                    this.mqsro.NumPubs = 0;
                    break;
            }
            if (Version == 0)
            {
                this.mqsro.Version = 1;
            }
            base.TrExit(method);
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.mqsro);
        }

        internal int GetVersionLength()
        {
            uint method = 0x2c1;
            this.TrEntry(method);
            if (this.mqsro.Version == 1)
            {
                int length = this.GetLength();
                base.TrExit(method, length, 1);
                return length;
            }
            base.TrExit(method, 0, 2);
            return 0;
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x2c3;
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
                this.mqsro = (MQBase.MQSRO) Marshal.PtrToStructure(zero, typeof(MQBase.MQSRO));
                Marshal.FreeCoTaskMem(zero);
                result = Offset + this.GetVersionLength();
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x2c2;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int versionLength = this.GetVersionLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.mqsro, zero, false);
                Marshal.Copy(zero, b, Offset, versionLength);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, versionLength);
            }
            return versionLength;
        }

        public int NumPublications
        {
            get
            {
                return this.mqsro.NumPubs;
            }
            set
            {
                this.mqsro.NumPubs = value;
            }
        }

        public int Options
        {
            get
            {
                return this.mqsro.Options;
            }
            set
            {
                this.mqsro.Options = value;
            }
        }

        public MQBase.MQSRO StructMQSRO
        {
            get
            {
                return this.mqsro;
            }
            set
            {
                this.mqsro = value;
            }
        }

        public int Version
        {
            get
            {
                return this.mqsro.Version;
            }
            set
            {
                this.mqsro.Version = value;
            }
        }
    }
}

