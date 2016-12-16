namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Runtime.InteropServices;

    public class LpiNotifyDetails : MQBaseObject
    {
        internal MQBase.structLPINOTIFYDETAILS lpiNotifyDetails;
        public const int LPINOTIFYDETAILS_VERSION_1 = 1;
        public const int lpiSPINotify_BREAKCONN = 2;
        public const int lpiSPINotify_CANCELWAITER = 1;
        private const string sccsid = "%Z% %W% %I% %E% %U%";

        public LpiNotifyDetails()
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%");
            this.lpiNotifyDetails = new MQBase.structLPINOTIFYDETAILS();
            this.lpiNotifyDetails.version = 1;
            this.lpiNotifyDetails.reason = 0;
            this.lpiNotifyDetails.connectionId = new byte[0x18];
        }

        private void CheckArrayLength()
        {
            uint method = 0x49a;
            this.TrEntry(method);
            if ((this.lpiNotifyDetails.connectionId != null) && (this.lpiNotifyDetails.connectionId.Length != 0x18))
            {
                this.lpiNotifyDetails.connectionId = this.ResizeArrayToCorrectLength(this.lpiNotifyDetails.connectionId, 0x18);
            }
            base.TrExit(method);
        }

        ~LpiNotifyDetails()
        {
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.lpiNotifyDetails);
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x49b;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                if (length > (b.Length - Offset))
                {
                    length = b.Length - Offset;
                }
                Marshal.Copy(b, Offset, zero, length);
                this.lpiNotifyDetails = (MQBase.structLPINOTIFYDETAILS) Marshal.PtrToStructure(zero, typeof(MQBase.structLPINOTIFYDETAILS));
            }
            finally
            {
                Marshal.FreeCoTaskMem(zero);
                base.TrExit(method);
            }
            return (Offset + this.GetLength());
        }

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x499;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            try
            {
                this.CheckArrayLength();
                zero = Marshal.AllocCoTaskMem(length);
                Marshal.StructureToPtr(this.lpiNotifyDetails, zero, false);
                Marshal.Copy(zero, b, Offset, length);
            }
            finally
            {
                Marshal.FreeCoTaskMem(zero);
                base.TrExit(method);
            }
            return length;
        }

        public byte[] ConnectionId
        {
            get
            {
                return this.lpiNotifyDetails.connectionId;
            }
            set
            {
                this.lpiNotifyDetails.connectionId = value;
            }
        }

        public int Reason
        {
            get
            {
                return this.lpiNotifyDetails.reason;
            }
            set
            {
                this.lpiNotifyDetails.reason = value;
            }
        }

        public MQBase.structLPINOTIFYDETAILS StructLpiNotifyDetails
        {
            get
            {
                this.CheckArrayLength();
                return this.lpiNotifyDetails;
            }
            set
            {
                this.lpiNotifyDetails = value;
            }
        }

        public int Version
        {
            get
            {
                return this.lpiNotifyDetails.version;
            }
            set
            {
                this.lpiNotifyDetails.version = value;
            }
        }
    }
}

