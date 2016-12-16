namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Runtime.InteropServices;

    public class MQSubscriptionDescriptor : MQBaseObject
    {
        private MQBase.MQSD mqSD = new MQBase.MQSD();
        private MQBase.MQSD32 mqSD32 = new MQBase.MQSD32();
        private int mqSDSize;
        private MQCHARV objectString;
        private int OBJSTRING_OFFSET;
        private MQCHARV resObjectString;
        private int ROBJSTRING_OFFSET;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private MQCHARV selectionString;
        private int SELSTRING_OFFSET;
        private MQCHARV subName;
        private int SUBNAME_OFFSET;
        private MQCHARV subUserData;
        private int SUBUSERDATA_OFFSET;
        private bool useNativePtrSz;

        public MQSubscriptionDescriptor()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.objectString = new MQCHARV(true, false);
            this.subName = new MQCHARV(true, false);
            this.subUserData = new MQCHARV(true, false);
            this.selectionString = new MQCHARV(true, false);
            this.resObjectString = new MQCHARV(false, true);
            this.ClearInvalidFields(0);
            this.useNativePtrSz = true;
            this.calculateOffsets();
        }

        public override void AddFieldsToFormatter(NmqiStructureFormatter fmt)
        {
            base.AddFieldsToFormatter(fmt);
            fmt.Add("version", this.Version);
            fmt.Add("options", this.Options);
            fmt.Add("objectName", this.ObjectName);
            fmt.Add("alternateUserId", this.AlternateUserId);
            fmt.Add("alternateSecurityId", this.AlternateSecurityId);
            fmt.Add("subExpiry", this.SubExpiry);
            fmt.Add("objectString", this.ObjectString.VSString);
            fmt.Add("subName", this.SubName.VSString);
            fmt.Add("subUserData", this.SubUserData);
            fmt.Add("subCorrelId", this.SubCorrelId);
            fmt.Add("pubPriority", this.PubPriority);
            fmt.Add("pubAccountingToken", this.PubAccountingToken);
            fmt.Add("pubApplIdentityData", this.PubApplIdentityData);
            fmt.Add("selectionString", this.SelectionString.VSString);
            fmt.Add("subLevel", this.SubLevel);
            fmt.Add("resolvedObjectString", this.ResObjectString);
        }

        private void calculateOffsets()
        {
            int num2;
            uint method = 0x43b;
            this.TrEntry(method);
            if (this.useNativePtrSz && (IntPtr.Size == 8))
            {
                num2 = 4;
            }
            else
            {
                num2 = 0;
            }
            this.OBJSTRING_OFFSET = 0x74 + num2;
            this.SUBNAME_OFFSET = this.OBJSTRING_OFFSET + this.objectString.GetLength();
            this.SUBUSERDATA_OFFSET = this.SUBNAME_OFFSET + this.subName.GetLength();
            this.SELSTRING_OFFSET = (((((this.SUBUSERDATA_OFFSET + this.subUserData.GetLength()) + 0x18) + 4) + 0x20) + 0x20) + num2;
            this.ROBJSTRING_OFFSET = ((this.SELSTRING_OFFSET + this.selectionString.GetLength()) + 4) + num2;
            base.TrExit(method);
        }

        private void CheckArrayLength()
        {
            uint method = 0x468;
            this.TrEntry(method);
            if ((this.mqSD.AlternateSecurityId != null) && (this.mqSD.AlternateSecurityId.Length != 40))
            {
                this.mqSD.AlternateSecurityId = this.ResizeArrayToCorrectLength(this.mqSD.AlternateSecurityId, 40);
            }
            if ((this.mqSD.AlternateUserId != null) && (this.mqSD.AlternateUserId.Length != 40))
            {
                this.mqSD.AlternateUserId = this.ResizeArrayToCorrectLength(this.mqSD.AlternateUserId, 40);
            }
            if ((this.mqSD.ObjectName != null) && (this.mqSD.ObjectName.Length != 0x30))
            {
                this.mqSD.ObjectName = this.ResizeArrayToCorrectLength(this.mqSD.ObjectName, 0x30);
            }
            if ((this.mqSD.PubAccountingToken != null) && (this.mqSD.PubAccountingToken.Length != 0x20))
            {
                this.mqSD.PubAccountingToken = this.ResizeArrayToCorrectLength(this.mqSD.PubAccountingToken, 0x20);
            }
            if ((this.mqSD.PubApplIdentityData != null) && (this.mqSD.PubApplIdentityData.Length != 0x20))
            {
                this.mqSD.PubApplIdentityData = this.ResizeArrayToCorrectLength(this.mqSD.PubApplIdentityData, 0x20);
            }
            if ((this.mqSD.SubCorrelId != null) && (this.mqSD.SubCorrelId.Length != 0x18))
            {
                this.mqSD.SubCorrelId = this.ResizeArrayToCorrectLength(this.mqSD.SubCorrelId, 0x18);
            }
            base.TrExit(method);
        }

        private void ClearInvalidFields(int ver)
        {
            uint method = 0x33b;
            this.TrEntry(method, new object[] { ver });
            if (ver == 0)
            {
                this.StrucId = new byte[] { 0x53, 0x44, 0x20, 0x20 };
                this.ObjectName = "";
                this.AlternateUserId = "";
                this.AlternateSecurityId = "";
                this.SubExpiry = -1;
                this.SubCorrelId = "";
                this.PubPriority = -3;
                this.PubAccountingToken = "";
                this.PubApplIdentityData = "";
                MQBase.structMQCHARV tmqcharv = new MQBase.structMQCHARV();
                tmqcharv.VSPtr = IntPtr.Zero;
                tmqcharv.VSOffset = 0;
                tmqcharv.VSLength = 0;
                tmqcharv.VSCCSID = 0x4b8;
                this.mqSD.ObjectString = tmqcharv;
                MQBase.structMQCHARV tmqcharv2 = new MQBase.structMQCHARV();
                tmqcharv2.VSPtr = IntPtr.Zero;
                tmqcharv2.VSOffset = 0;
                tmqcharv2.VSLength = 0;
                tmqcharv2.VSCCSID = 0x4b8;
                this.mqSD.SubName = tmqcharv2;
                MQBase.structMQCHARV tmqcharv3 = new MQBase.structMQCHARV();
                tmqcharv3.VSPtr = IntPtr.Zero;
                tmqcharv3.VSOffset = 0;
                tmqcharv3.VSLength = 0;
                tmqcharv3.VSCCSID = 0x4b8;
                this.mqSD.SubUserData = tmqcharv3;
                MQBase.structMQCHARV tmqcharv4 = new MQBase.structMQCHARV();
                tmqcharv4.VSPtr = IntPtr.Zero;
                tmqcharv4.VSOffset = 0;
                tmqcharv4.VSLength = 0;
                tmqcharv4.VSCCSID = 0x4b8;
                this.mqSD.SelectionString = tmqcharv4;
                this.SubLevel = 1;
                MQBase.structMQCHARV tmqcharv5 = new MQBase.structMQCHARV();
                tmqcharv5.VSPtr = IntPtr.Zero;
                tmqcharv5.VSOffset = 0;
                tmqcharv5.VSLength = 0;
                tmqcharv5.VSCCSID = 0x4b8;
                this.mqSD.ResObjectString = tmqcharv5;
                MQBase.structMQCHARV32 tmqcharv6 = new MQBase.structMQCHARV32();
                tmqcharv6.VSPtr = 0;
                tmqcharv6.VSOffset = 0;
                tmqcharv6.VSLength = 0;
                tmqcharv6.VSCCSID = 0x4b8;
                this.mqSD32.ObjectString = tmqcharv6;
                MQBase.structMQCHARV32 tmqcharv7 = new MQBase.structMQCHARV32();
                tmqcharv7.VSPtr = 0;
                tmqcharv7.VSOffset = 0;
                tmqcharv7.VSLength = 0;
                tmqcharv7.VSCCSID = 0x4b8;
                this.mqSD32.SubName = tmqcharv7;
                MQBase.structMQCHARV32 tmqcharv8 = new MQBase.structMQCHARV32();
                tmqcharv8.VSPtr = 0;
                tmqcharv8.VSOffset = 0;
                tmqcharv8.VSLength = 0;
                tmqcharv8.VSCCSID = 0x4b8;
                this.mqSD32.SubUserData = tmqcharv8;
                MQBase.structMQCHARV32 tmqcharv9 = new MQBase.structMQCHARV32();
                tmqcharv9.VSPtr = 0;
                tmqcharv9.VSOffset = 0;
                tmqcharv9.VSLength = 0;
                tmqcharv9.VSCCSID = 0x4b8;
                this.mqSD32.SelectionString = tmqcharv9;
                MQBase.structMQCHARV32 tmqcharv10 = new MQBase.structMQCHARV32();
                tmqcharv10.VSPtr = 0;
                tmqcharv10.VSOffset = 0;
                tmqcharv10.VSLength = 0;
                tmqcharv10.VSCCSID = 0x4b8;
                this.mqSD32.ResObjectString = tmqcharv10;
            }
            if (ver == 0)
            {
                this.Version = 1;
            }
            base.TrExit(method);
        }

        internal void CopyCHARVIntoSD()
        {
            uint method = 0x33f;
            this.TrEntry(method);
            int cc = 0;
            int rc = 0;
            try
            {
                this.mqSD.ObjectString = this.objectString.mqcharv;
                this.mqSD32.ObjectString = this.objectString.mqcharv32;
                this.mqSD.SubName = this.subName.mqcharv;
                this.mqSD32.SubName = this.subName.mqcharv32;
                this.mqSD.SubUserData = this.subUserData.mqcharv;
                this.mqSD32.SubUserData = this.subUserData.mqcharv32;
                this.mqSD.SelectionString = this.selectionString.mqcharv;
                this.mqSD32.SelectionString = this.selectionString.mqcharv32;
                if (((this.mqSD.ObjectString.VSPtr != IntPtr.Zero) && (this.objectString.VSString != null)) || ((this.mqSD32.ObjectString.VSPtr != 0) && (this.objectString.VSString != null)))
                {
                    cc = 2;
                    rc = 0x979;
                    base.throwNewMQException(cc, rc);
                }
                if (((this.mqSD.SelectionString.VSPtr != IntPtr.Zero) && (this.selectionString.VSString != null)) || ((this.mqSD32.SelectionString.VSPtr != 0) && (this.selectionString.VSString != null)))
                {
                    cc = 2;
                    rc = 0x813;
                    base.throwNewMQException(cc, rc);
                }
                if (((this.mqSD.SubName.VSPtr != IntPtr.Zero) && (this.subName.VSString != null)) || ((this.mqSD32.SubName.VSPtr != 0) && (this.subName.VSString != null)))
                {
                    cc = 2;
                    rc = 0x988;
                    base.throwNewMQException(cc, rc);
                }
                if (((this.mqSD.SubUserData.VSPtr != IntPtr.Zero) && (this.subUserData.VSString != null)) || ((this.mqSD32.SubUserData.VSPtr != 0) && (this.subUserData.VSString != null)))
                {
                    cc = 2;
                    rc = 0x988;
                    base.throwNewMQException(cc, rc);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void copyToMQSD()
        {
            uint method = 0x43d;
            this.TrEntry(method);
            this.mqSD.AlternateSecurityId = this.mqSD32.AlternateSecurityId;
            this.mqSD.AlternateUserId = this.mqSD32.AlternateUserId;
            this.mqSD.ObjectName = this.mqSD32.ObjectName;
            this.mqSD.Options = this.mqSD32.Options;
            this.mqSD.PubAccountingToken = this.mqSD32.PubAccountingToken;
            this.mqSD.PubApplIdentityData = this.mqSD32.PubApplIdentityData;
            this.mqSD.PubPriority = this.mqSD32.PubPriority;
            this.mqSD.SubCorrelId = this.mqSD32.SubCorrelId;
            this.mqSD.SubExpiry = this.mqSD32.SubExpiry;
            this.mqSD.SubLevel = this.mqSD32.SubLevel;
            this.mqSD.Version = this.mqSD32.Version;
            base.TrExit(method);
        }

        private void copyToMQSD32()
        {
            uint method = 0x43c;
            this.TrEntry(method);
            this.mqSD32.AlternateSecurityId = this.mqSD.AlternateSecurityId;
            this.mqSD32.AlternateUserId = this.mqSD.AlternateUserId;
            this.mqSD32.ObjectName = this.mqSD.ObjectName;
            this.mqSD32.Options = this.mqSD.Options;
            this.mqSD32.PubAccountingToken = this.mqSD.PubAccountingToken;
            this.mqSD32.PubApplIdentityData = this.mqSD.PubApplIdentityData;
            this.mqSD32.PubPriority = this.mqSD.PubPriority;
            this.mqSD32.SubCorrelId = this.mqSD.SubCorrelId;
            this.mqSD32.SubExpiry = this.mqSD.SubExpiry;
            this.mqSD32.SubLevel = this.mqSD.SubLevel;
            this.mqSD32.Version = this.mqSD.Version;
            base.TrExit(method);
        }

        internal int GetLength()
        {
            uint method = 0x43a;
            this.TrEntry(method);
            if (this.useNativePtrSz)
            {
                int num2 = Marshal.SizeOf(this.mqSD);
                base.TrExit(method, num2, 1);
                return num2;
            }
            int result = Marshal.SizeOf(this.mqSD32);
            base.TrExit(method, result, 2);
            return result;
        }

        private int GetPaddedLength(int len)
        {
            uint method = 0x467;
            this.TrEntry(method, new object[] { len });
            int num2 = 0;
            num2 = (4 - (len & 3)) & 3;
            int result = len + num2;
            base.TrExit(method, result);
            return result;
        }

        private int GetPaddedLength(string str)
        {
            uint method = 0x33d;
            this.TrEntry(method, new object[] { str });
            int len = 0;
            int paddedLength = 0;
            if (str != null)
            {
                len = str.Length;
                paddedLength = this.GetPaddedLength(len);
            }
            else
            {
                len = paddedLength = 0;
            }
            int result = paddedLength;
            base.TrExit(method, result);
            return result;
        }

        public int GetRequiredBufferSize()
        {
            uint method = 830;
            this.TrEntry(method);
            int result = this.GetVersionLength() + this.GetPaddedLength(this.objectString.VSString);
            result += this.GetPaddedLength(this.subName.VSString);
            result += this.GetPaddedLength(this.subUserData.VSString);
            result += this.GetPaddedLength(this.selectionString.VSString);
            result += this.GetPaddedLength(this.resObjectString.VSBufSize);
            base.TrExit(method, result);
            return result;
        }

        public int GetRequiredBufferSizeWOResObj()
        {
            uint method = 0x4ed;
            this.TrEntry(method);
            int result = this.GetVersionLength() + this.GetPaddedLength(this.objectString.VSString);
            result += this.GetPaddedLength(this.subName.VSString);
            result += this.GetPaddedLength(this.subUserData.VSString);
            result += this.GetPaddedLength(this.selectionString.VSString);
            base.TrExit(method, result);
            return result;
        }

        internal int GetVersionLength()
        {
            uint method = 0x33c;
            this.TrEntry(method);
            if (this.mqSD.Version == 1)
            {
                int length = this.GetLength();
                base.TrExit(method, length, 1);
                return length;
            }
            base.TrExit(method, 0, 2);
            return 0;
        }

        public int ReadStruct(byte[] b, int Offset, int Length)
        {
            uint method = 0x341;
            this.TrEntry(method, new object[] { b, Offset, Length });
            int length = this.GetLength();
            IntPtr zero = IntPtr.Zero;
            try
            {
                if (zero == IntPtr.Zero)
                {
                    zero = Marshal.AllocCoTaskMem(length);
                    this.mqSDSize = length;
                }
                Marshal.Copy(b, Offset, zero, length);
                if (this.useNativePtrSz)
                {
                    this.mqSD = (MQBase.MQSD) Marshal.PtrToStructure(zero, typeof(MQBase.MQSD));
                    this.copyToMQSD32();
                }
                else
                {
                    this.mqSD32 = (MQBase.MQSD32) Marshal.PtrToStructure(zero, typeof(MQBase.MQSD32));
                    this.copyToMQSD();
                }
                this.objectString.ReadStruct(b, Offset, Offset + this.OBJSTRING_OFFSET);
                this.objectString.GetEndPosAligned(Offset);
                this.subName.ReadStruct(b, Offset, Offset + this.SUBNAME_OFFSET);
                this.subName.GetEndPosAligned(Offset);
                this.subUserData.ReadStruct(b, Offset, Offset + this.SUBUSERDATA_OFFSET);
                this.subUserData.GetEndPosAligned(Offset);
                this.selectionString.ReadStruct(b, Offset, Offset + this.SELSTRING_OFFSET);
                this.selectionString.GetEndPosAligned(Offset);
                this.resObjectString.ReadStruct(b, Offset, Offset + this.ROBJSTRING_OFFSET);
                this.resObjectString.GetEndPosAligned(Offset);
                this.ClearInvalidFields(this.Version);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(zero);
                    zero = IntPtr.Zero;
                }
                base.TrExit(method);
            }
            return ((((((Offset + this.GetVersionLength()) + this.objectString.VSLength) + this.subName.VSLength) + this.subUserData.VSLength) + this.selectionString.VSLength) + this.resObjectString.VSLength);
        }

        public int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x340;
            this.TrEntry(method, new object[] { b, Offset });
            int stringPos = Offset;
            int requiredBufferSize = this.GetRequiredBufferSize();
            int length = this.GetLength();
            IntPtr zero = IntPtr.Zero;
            try
            {
                this.CheckArrayLength();
                if (zero == IntPtr.Zero)
                {
                    zero = Marshal.AllocCoTaskMem(length);
                    this.mqSDSize = length;
                }
                if (this.useNativePtrSz)
                {
                    Marshal.StructureToPtr(this.mqSD, zero, false);
                }
                else
                {
                    Marshal.StructureToPtr(this.mqSD32, zero, false);
                }
                Marshal.Copy(zero, b, Offset, length);
                stringPos += length;
                stringPos = this.objectString.WriteStruct(b, Offset, Offset + this.OBJSTRING_OFFSET, stringPos);
                stringPos = this.subName.WriteStruct(b, Offset, Offset + this.SUBNAME_OFFSET, stringPos);
                stringPos = this.subUserData.WriteStruct(b, Offset, Offset + this.SUBUSERDATA_OFFSET, stringPos);
                stringPos = this.selectionString.WriteStruct(b, Offset, Offset + this.SELSTRING_OFFSET, stringPos);
                stringPos = this.resObjectString.WriteStruct(b, Offset, Offset + this.ROBJSTRING_OFFSET, stringPos);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(zero);
                    zero = IntPtr.Zero;
                }
                base.TrExit(method);
            }
            return requiredBufferSize;
        }

        public string AlternateSecurityId
        {
            get
            {
                return base.GetString(this.mqSD.AlternateSecurityId);
            }
            set
            {
                byte[] buffer = new byte[40];
                base.GetBytes(value, ref buffer);
                this.mqSD.AlternateSecurityId = buffer;
                this.mqSD32.AlternateSecurityId = buffer;
            }
        }

        public string AlternateUserId
        {
            get
            {
                return base.GetString(this.mqSD.AlternateUserId);
            }
            set
            {
                byte[] buffer = new byte[12];
                base.GetBytes(value, ref buffer);
                this.mqSD.AlternateUserId = buffer;
                this.mqSD32.AlternateUserId = buffer;
            }
        }

        public string ObjectName
        {
            get
            {
                return base.GetString(this.mqSD.ObjectName);
            }
            set
            {
                byte[] buffer = new byte[0x30];
                base.GetBytes(value, ref buffer);
                this.mqSD.ObjectName = buffer;
                this.mqSD32.ObjectName = buffer;
            }
        }

        public MQCHARV ObjectString
        {
            get
            {
                return this.objectString;
            }
            set
            {
                this.objectString = value;
            }
        }

        public int Options
        {
            get
            {
                return this.mqSD.Options;
            }
            set
            {
                this.mqSD.Options = value;
                this.mqSD32.Options = value;
            }
        }

        public string PubAccountingToken
        {
            get
            {
                return base.GetString(this.mqSD.PubAccountingToken);
            }
            set
            {
                byte[] buffer = new byte[0x20];
                base.GetBytes(value, ref buffer);
                this.mqSD.PubAccountingToken = buffer;
                this.mqSD32.PubAccountingToken = buffer;
            }
        }

        public string PubApplIdentityData
        {
            get
            {
                return base.GetString(this.mqSD.PubApplIdentityData);
            }
            set
            {
                byte[] buffer = new byte[0x20];
                base.GetBytes(value, ref buffer);
                this.mqSD.PubApplIdentityData = buffer;
                this.mqSD32.PubApplIdentityData = buffer;
            }
        }

        public int PubPriority
        {
            get
            {
                return this.mqSD.PubPriority;
            }
            set
            {
                this.mqSD.PubPriority = value;
                this.mqSD32.PubPriority = value;
            }
        }

        public MQCHARV ResObjectString
        {
            get
            {
                return this.resObjectString;
            }
            set
            {
                this.resObjectString = value;
            }
        }

        public MQCHARV SelectionString
        {
            get
            {
                return this.selectionString;
            }
            set
            {
                this.selectionString = value;
            }
        }

        public byte[] StrucId
        {
            get
            {
                return this.mqSD.StrucId;
            }
            set
            {
                this.mqSD.StrucId = value;
                this.mqSD32.StrucId = value;
            }
        }

        public string SubCorrelId
        {
            get
            {
                return base.GetString(this.mqSD.SubCorrelId);
            }
            set
            {
                byte[] buffer = new byte[0x18];
                base.GetBytes(value, ref buffer);
                this.mqSD.SubCorrelId = buffer;
                this.mqSD32.SubCorrelId = buffer;
            }
        }

        public int SubExpiry
        {
            get
            {
                return this.mqSD.SubExpiry;
            }
            set
            {
                this.mqSD.SubExpiry = value;
                this.mqSD32.SubExpiry = value;
            }
        }

        public int SubLevel
        {
            get
            {
                return this.mqSD.SubLevel;
            }
            set
            {
                this.mqSD.SubLevel = value;
                this.mqSD32.SubLevel = value;
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

        public MQCHARV SubUserData
        {
            get
            {
                return this.subUserData;
            }
            set
            {
                this.subUserData = value;
            }
        }

        public bool UseNativePtrSz
        {
            get
            {
                return this.useNativePtrSz;
            }
            set
            {
                this.useNativePtrSz = value;
                this.objectString.UseNativePtrSz = value;
                this.subName.UseNativePtrSz = value;
                this.subUserData.UseNativePtrSz = value;
                this.selectionString.UseNativePtrSz = value;
                this.resObjectString.UseNativePtrSz = value;
                this.calculateOffsets();
            }
        }

        public int Version
        {
            get
            {
                return this.mqSD.Version;
            }
            set
            {
                this.mqSD.Version = value;
                this.mqSD32.Version = value;
            }
        }
    }
}

