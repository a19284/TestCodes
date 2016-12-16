namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Runtime.InteropServices;

    public class LpiSDSubProps : MQBase
    {
        private MQBase.structLPISDSUBPROPS lpiSDSubProps;
        private const string sccsid = "%Z% %W% %I% %E% %U%";

        public LpiSDSubProps()
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%");
            this.lpiSDSubProps = new MQBase.structLPISDSUBPROPS();
            this.lpiSDSubProps.DestinationQMgr = new byte[0x30];
            this.lpiSDSubProps.DestinationQName = new byte[0x30];
            this.lpiSDSubProps.SubCorrelId = new byte[0x18];
            this.lpiSDSubProps.PsPropertyStyle = 3;
            this.lpiSDSubProps.SubScope = 0;
            this.lpiSDSubProps.SubType = 1;
        }

        private void CheckArrayLength()
        {
            uint method = 0x469;
            this.TrEntry(method);
            try
            {
                if ((this.lpiSDSubProps.DestinationQMgr != null) && (this.lpiSDSubProps.DestinationQMgr.Length != 0x30))
                {
                    this.lpiSDSubProps.DestinationQMgr = this.ResizeArrayToCorrectLength(this.lpiSDSubProps.DestinationQMgr, 0x30);
                }
                if ((this.lpiSDSubProps.DestinationQName != null) && (this.lpiSDSubProps.DestinationQName.Length != 0x30))
                {
                    this.lpiSDSubProps.DestinationQName = this.ResizeArrayToCorrectLength(this.lpiSDSubProps.DestinationQName, 0x30);
                }
                if ((this.lpiSDSubProps.SubCorrelId != null) && (this.lpiSDSubProps.SubCorrelId.Length != 0x18))
                {
                    this.lpiSDSubProps.SubCorrelId = this.ResizeArrayToCorrectLength(this.lpiSDSubProps.SubCorrelId, 0x18);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        ~LpiSDSubProps()
        {
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.lpiSDSubProps);
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x358;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                try
                {
                    Marshal.Copy(b, Offset, zero, length);
                    this.lpiSDSubProps = (MQBase.structLPISDSUBPROPS) Marshal.PtrToStructure(zero, typeof(MQBase.structLPISDSUBPROPS));
                }
                finally
                {
                    Marshal.FreeCoTaskMem(zero);
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return (Offset + this.GetLength());
        }

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x357;
            this.TrEntry(method, new object[] { b, Offset });
            int length = this.GetLength();
            IntPtr zero = IntPtr.Zero;
            try
            {
                this.CheckArrayLength();
                zero = Marshal.AllocCoTaskMem(length);
                try
                {
                    Marshal.StructureToPtr(this.lpiSDSubProps, zero, false);
                    Marshal.Copy(zero, b, Offset, length);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(zero);
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return length;
        }

        public byte[] DestinationQMgr
        {
            get
            {
                return this.lpiSDSubProps.DestinationQMgr;
            }
            set
            {
                this.lpiSDSubProps.DestinationQMgr = value;
            }
        }

        public byte[] DestinationQName
        {
            get
            {
                return this.lpiSDSubProps.DestinationQName;
            }
            set
            {
                this.lpiSDSubProps.DestinationQName = value;
            }
        }

        public int PSPropertyStyle
        {
            get
            {
                return this.lpiSDSubProps.PsPropertyStyle;
            }
            set
            {
                this.lpiSDSubProps.PsPropertyStyle = value;
            }
        }

        public MQBase.structLPISDSUBPROPS StructLpiSDSubProps
        {
            get
            {
                this.CheckArrayLength();
                return this.lpiSDSubProps;
            }
            set
            {
                this.lpiSDSubProps = value;
            }
        }

        public byte[] SubCorrelId
        {
            get
            {
                return this.lpiSDSubProps.SubCorrelId;
            }
            set
            {
                this.lpiSDSubProps.SubCorrelId = value;
            }
        }

        public int SubScope
        {
            get
            {
                return this.lpiSDSubProps.SubScope;
            }
            set
            {
                this.lpiSDSubProps.SubScope = value;
            }
        }

        public int SubType
        {
            get
            {
                return this.lpiSDSubProps.SubType;
            }
            set
            {
                this.lpiSDSubProps.SubType = value;
            }
        }
    }
}

