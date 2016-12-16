namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Runtime.InteropServices;

    public class LpiUSD : MQBase
    {
        internal structLPIUSD lpiUSD;
        public const int lpiUSD_CURRENT_VERSION = 1;
        public const int lpiUSD_VERSION_1 = 1;
        public const int lpiUSDO_ALTERNATE_USER_AUTHORITY = 0x40000;
        public const int lpiUSDO_LEAVE_ONLY = 0x20;
        public const int lpiUSDO_NONE = 0;
        public const int lpiUSDO_SYNCPOINT = 0x4000000;
        private int lpiUSDStructSize;
        private const string sccsid = "%Z% %W% %I% %E% %U%";
        public MQCHARV subIdentity;
        private const int SUBIDSTRING_OFFSET = 0x10;
        public MQCHARV subName;
        private const int SUBNAMESTRING_OFFSET = 0x24;

        public LpiUSD()
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%");
            this.lpiUSD = new structLPIUSD();
            this.lpiUSD.structId = new byte[] { 0x4c, 0x55, 0x53, 0x44 };
            this.lpiUSD.version = 1;
            this.lpiUSD.options = 0;
            this.subIdentity = new MQCHARV(true, false);
            this.subName = new MQCHARV(true, false);
            this.lpiUSD.subId = new byte[0x18];
            this.lpiUSD.alternateUserId = new byte[12];
            this.lpiUSD.alternateSecurityId = new byte[40];
        }

        private void CheckArrayLength()
        {
            uint method = 0x46a;
            this.TrEntry(method);
            try
            {
                if ((this.lpiUSD.subId != null) && (this.lpiUSD.subId.Length != 0x80))
                {
                    this.lpiUSD.subId = this.ResizeArrayToCorrectLength(this.lpiUSD.subId, 0x80);
                }
                if ((this.lpiUSD.alternateSecurityId != null) && (this.lpiUSD.alternateSecurityId.Length != 40))
                {
                    this.lpiUSD.alternateSecurityId = this.ResizeArrayToCorrectLength(this.lpiUSD.alternateSecurityId, 40);
                }
                if ((this.lpiUSD.alternateUserId != null) && (this.lpiUSD.alternateUserId.Length != 12))
                {
                    this.lpiUSD.alternateUserId = this.ResizeArrayToCorrectLength(this.lpiUSD.alternateUserId, 12);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void CopyCHARVIntoLPIUSD()
        {
            uint method = 0x35a;
            this.TrEntry(method);
            this.lpiUSD.subIdentity = this.subIdentity.mqcharv;
            this.lpiUSD.subName = this.subName.mqcharv;
            base.TrExit(method);
        }

        ~LpiUSD()
        {
        }

        internal int GetLength()
        {
            return Marshal.SizeOf(this.lpiUSD);
        }

        private int GetPaddedLength(string str)
        {
            uint method = 0x359;
            this.TrEntry(method, new object[] { str });
            int length = 0;
            int num3 = 0;
            if (str != null)
            {
                length = str.Length;
                num3 = (4 - (length & 3)) & 3;
            }
            else
            {
                length = num3 = 0;
            }
            int result = length + num3;
            base.TrExit(method, result);
            return result;
        }

        internal int GetRequiredBufferSize()
        {
            int num = this.GetLength() + this.GetPaddedLength(this.subIdentity.VSString);
            return (num + this.GetPaddedLength(this.subName.VSString));
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 860;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int requiredBufferSize = this.GetRequiredBufferSize();
            int num3 = 0;
            try
            {
                zero = Marshal.AllocCoTaskMem(requiredBufferSize);
                this.lpiUSDStructSize = requiredBufferSize;
                try
                {
                    Marshal.Copy(b, Offset, zero, requiredBufferSize);
                    this.lpiUSD = (structLPIUSD) Marshal.PtrToStructure(zero, typeof(structLPIUSD));
                    num3 = Offset + requiredBufferSize;
                    this.subIdentity.ReadStruct(b, Offset, Offset + 0x10);
                    this.subIdentity.GetEndPosAligned(Offset);
                    this.subName.ReadStruct(b, Offset, Offset + 0x24);
                    this.subName.GetEndPosAligned(Offset);
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
            return num3;
        }

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x35b;
            this.TrEntry(method, new object[] { b, Offset });
            int stringPos = Offset;
            int requiredBufferSize = this.GetRequiredBufferSize();
            int length = this.GetLength();
            IntPtr zero = IntPtr.Zero;
            this.CheckArrayLength();
            zero = Marshal.AllocCoTaskMem(length);
            this.lpiUSDStructSize = length;
            try
            {
                Marshal.StructureToPtr(this.lpiUSD, zero, false);
                Marshal.Copy(zero, b, Offset, length);
                stringPos += length;
                stringPos = this.subIdentity.WriteStruct(b, Offset, Offset + 0x10, stringPos);
                stringPos = this.subIdentity.WriteStruct(b, Offset, Offset + 0x24, stringPos);
            }
            finally
            {
                Marshal.FreeCoTaskMem(zero);
                base.TrExit(method);
            }
            zero = IntPtr.Zero;
            return requiredBufferSize;
        }

        public byte[] AlternateSecurityId
        {
            get
            {
                return this.lpiUSD.alternateSecurityId;
            }
            set
            {
                this.lpiUSD.alternateSecurityId = value;
            }
        }

        public byte[] AlternateUserId
        {
            get
            {
                return this.lpiUSD.alternateUserId;
            }
            set
            {
                this.lpiUSD.alternateUserId = value;
            }
        }

        public int Options
        {
            get
            {
                return this.lpiUSD.options;
            }
            set
            {
                this.lpiUSD.options = value;
            }
        }

        public byte[] SubId
        {
            get
            {
                return this.lpiUSD.subId;
            }
            set
            {
                this.lpiUSD.subId = value;
            }
        }

        public MQCHARV SubIdentity
        {
            get
            {
                return this.subIdentity;
            }
            set
            {
                this.subIdentity = value;
            }
        }

        public MQCHARV SubName
        {
            get
            {
                return this.subName;
            }
            set
            {
                this.subName = value;
            }
        }

        public int Version
        {
            get
            {
                return this.lpiUSD.version;
            }
            set
            {
                this.lpiUSD.version = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct structLPIUSD
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
            public byte[] structId;
            public int version;
            public int options;
            public MQBase.structMQCHARV subIdentity;
            public MQBase.structMQCHARV subName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x18)]
            public byte[] subId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            public byte[] alternateUserId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=40)]
            public byte[] alternateSecurityId;
        }
    }
}

