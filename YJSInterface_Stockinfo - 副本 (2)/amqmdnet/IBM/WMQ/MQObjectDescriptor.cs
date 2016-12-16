namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Runtime.InteropServices;

    public class MQObjectDescriptor : MQBaseObject
    {
        private MQBase.MQOD mqOD = new MQBase.MQOD();
        private MQBase.MQOD32 mqOD32 = new MQBase.MQOD32();
        private int mqODSize;
        private MQCHARV objectString;
        private int OBJSTRING_OFFSET;
        private MQCHARV resolvedObjectString;
        private int ROBJSTRING_OFFSET;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private MQCHARV selectionString;
        private int SELSTRING_OFFSET;
        private bool useNativePtrSz;

        public MQObjectDescriptor()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.objectString = new MQCHARV(true, false);
            this.selectionString = new MQCHARV(true, false);
            this.resolvedObjectString = new MQCHARV(false, true);
            this.ClearInvalidFields(0);
            this.useNativePtrSz = true;
        }

        public override void AddFieldsToFormatter(NmqiStructureFormatter fmt)
        {
            base.AddFieldsToFormatter(fmt);
            fmt.Add("version", this.Version);
            fmt.Add("objectType", this.ObjectType);
            fmt.Add("objectName", this.ObjectName);
            fmt.Add("objectQMgrName", this.ObjectQMgrName);
            fmt.Add("dynamicQName", this.DynamicQName);
            fmt.Add("alternateUserId", this.AlternateUserId);
            fmt.Add("recsPresent", this.RecordsPresent);
            fmt.Add("knownDestCount", this.KnownDestCount);
            fmt.Add("unknownDestCount", this.UnknownDestCount);
            fmt.Add("invalidDestCount", this.InvalidDestCount);
            fmt.Add("objectRecordPtr", this.ObjectRecordPtr);
            fmt.Add("objectRecordPtr32", this.ObjectRecordPtr32);
            fmt.Add("responseRecordPtr", this.ResponseRecordPtr);
            fmt.Add("responseRecordPtr32", this.ResponseRecordPtr32);
            fmt.Add("alternateSecurityId", this.AlternateSecurityId);
            fmt.Add("resolvedQName", this.ResolvedQueueName);
            fmt.Add("resolvedQMgrName", this.ResolvedQueueManagerName);
            fmt.Add("objectString", this.ObjectString);
            fmt.Add("selectionString", this.SelectionString);
            fmt.Add("resolvedObjectString", this.ResolvedObjectString);
            fmt.Add("resolvedType", this.ResolvedType);
        }

        private void calculateOffsets()
        {
            uint method = 0x418;
            this.TrEntry(method);
            this.OBJSTRING_OFFSET = this.GetVersionLength(3);
            this.SELSTRING_OFFSET = this.OBJSTRING_OFFSET + this.objectString.GetLength();
            this.ROBJSTRING_OFFSET = this.SELSTRING_OFFSET + this.selectionString.GetLength();
            base.TrExit(method);
        }

        private void CheckArrayLength()
        {
            uint method = 0x466;
            this.TrEntry(method);
            if ((this.mqOD.alternateSecurityId != null) && (this.mqOD.alternateSecurityId.Length != 40))
            {
                this.mqOD.alternateSecurityId = this.ResizeArrayToCorrectLength(this.mqOD.alternateSecurityId, 40);
            }
            if ((this.mqOD.AlternateUserId != null) && (this.mqOD.AlternateUserId.Length != 12))
            {
                this.mqOD.AlternateUserId = this.ResizeArrayToCorrectLength(this.mqOD.AlternateUserId, 12);
            }
            if ((this.mqOD.DynamicQName != null) && (this.mqOD.DynamicQName.Length != 0x30))
            {
                this.mqOD.DynamicQName = this.ResizeArrayToCorrectLength(this.mqOD.DynamicQName, 0x30);
            }
            if ((this.mqOD.ObjectName != null) && (this.mqOD.ObjectName.Length != 0x30))
            {
                this.mqOD.ObjectName = this.ResizeArrayToCorrectLength(this.mqOD.ObjectName, 0x30);
            }
            if ((this.mqOD.ObjectQMgrName != null) && (this.mqOD.ObjectQMgrName.Length != 0x30))
            {
                this.mqOD.ObjectQMgrName = this.ResizeArrayToCorrectLength(this.mqOD.ObjectQMgrName, 0x30);
            }
            base.TrExit(method);
        }

        private void ClearInvalidFields(int ver)
        {
            uint method = 500;
            this.TrEntry(method, new object[] { ver });
            switch (ver)
            {
                case 0:
                    this.StrucId = new byte[] { 0x4f, 0x44, 0x20, 0x20 };
                    this.ObjectType = 1;
                    this.ObjectName = "";
                    this.ObjectQMgrName = "";
                    this.DynamicQName = "AMQ.*";
                    this.AlternateUserId = "";
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_00F5;

                case 3:
                    goto Label_011C;

                default:
                    goto Label_0282;
            }
            this.RecordsPresent = 0;
            this.KnownDestCount = 0;
            this.UnknownDestCount = 0;
            this.InvalidDestCount = 0;
            this.ObjectRecordOffset = 0;
            this.ResponseRecordOffset = 0;
            this.ObjectRecordPtr = IntPtr.Zero;
            this.ResponseRecordPtr = IntPtr.Zero;
            this.ObjectRecordPtr32 = 0;
            this.ResponseRecordPtr32 = 0;
        Label_00F5:
            this.AlternateSecurityId = new byte[40];
            this.ResolvedQueueName = new byte[0x30];
            this.ResolvedQueueManagerName = new byte[0x30];
        Label_011C:
            this.ResolvedType = 0;
            MQBase.structMQCHARV tmqcharv = new MQBase.structMQCHARV();
            tmqcharv.VSPtr = IntPtr.Zero;
            tmqcharv.VSOffset = 0;
            tmqcharv.VSLength = 0;
            tmqcharv.VSCCSID = 0x4b8;
            this.mqOD.ObjectString = tmqcharv;
            MQBase.structMQCHARV tmqcharv2 = new MQBase.structMQCHARV();
            tmqcharv2.VSPtr = IntPtr.Zero;
            tmqcharv2.VSOffset = 0;
            tmqcharv2.VSLength = 0;
            tmqcharv2.VSCCSID = 0x4b8;
            this.mqOD.SelectionString = tmqcharv2;
            MQBase.structMQCHARV tmqcharv3 = new MQBase.structMQCHARV();
            tmqcharv3.VSPtr = IntPtr.Zero;
            tmqcharv3.VSOffset = 0;
            tmqcharv3.VSLength = 0;
            tmqcharv3.VSCCSID = 0x4b8;
            this.mqOD.ResolvedObjectString = tmqcharv3;
            MQBase.structMQCHARV32 tmqcharv4 = new MQBase.structMQCHARV32();
            tmqcharv4.VSPtr = 0;
            tmqcharv4.VSOffset = 0;
            tmqcharv4.VSLength = 0;
            tmqcharv4.VSCCSID = 0x4b8;
            this.mqOD32.ObjectString = tmqcharv4;
            MQBase.structMQCHARV32 tmqcharv5 = new MQBase.structMQCHARV32();
            tmqcharv5.VSPtr = 0;
            tmqcharv5.VSOffset = 0;
            tmqcharv5.VSLength = 0;
            tmqcharv5.VSCCSID = 0x4b8;
            this.mqOD32.SelectionString = tmqcharv5;
            MQBase.structMQCHARV32 tmqcharv6 = new MQBase.structMQCHARV32();
            tmqcharv6.VSPtr = 0;
            tmqcharv6.VSOffset = 0;
            tmqcharv6.VSLength = 0;
            tmqcharv6.VSCCSID = 0x4b8;
            this.mqOD32.ResolvedObjectString = tmqcharv6;
        Label_0282:
            if (ver == 0)
            {
                this.Version = 1;
            }
            this.calculateOffsets();
            base.TrExit(method);
        }

        internal void CopyCHARVIntoOD()
        {
            uint method = 0x1f8;
            this.TrEntry(method);
            try
            {
                this.mqOD.ObjectString = this.objectString.mqcharv;
                this.mqOD.SelectionString = this.selectionString.mqcharv;
                if ((this.mqOD.ObjectString.VSPtr != IntPtr.Zero) && (this.objectString.VSString != null))
                {
                    base.throwNewMQException(2, 0x979);
                }
                if ((this.mqOD.SelectionString.VSPtr != IntPtr.Zero) && (this.selectionString.VSString != null))
                {
                    base.throwNewMQException(2, 0x979);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void copyToMQOD()
        {
            uint method = 0x41a;
            this.TrEntry(method);
            this.mqOD.AlternateSecurityId = this.mqOD32.AlternateSecurityId;
            this.mqOD.AlternateUserId = this.mqOD32.AlternateUserId;
            this.mqOD.DynamicQName = this.mqOD32.DynamicQName;
            this.mqOD.InvalidDestCount = this.mqOD32.InvalidDestCount;
            this.mqOD.KnownDestCount = this.mqOD32.KnownDestCount;
            this.mqOD.ObjectName = this.mqOD32.ObjectName;
            this.mqOD.ObjectQMgrName = this.mqOD32.ObjectQMgrName;
            this.mqOD.ObjectRecOffset = this.mqOD32.ObjectRecOffset;
            this.mqOD.ObjectType = this.mqOD32.ObjectType;
            this.mqOD.RecsPresent = this.mqOD32.RecsPresent;
            this.mqOD.ResolvedQMgrName = this.mqOD32.ResolvedQMgrName;
            this.mqOD.ResolvedQName = this.mqOD32.ResolvedQName;
            this.mqOD.ResolvedType = this.mqOD32.ResolvedType;
            this.mqOD.ResponseRecOffset = this.mqOD32.ResponseRecOffset;
            this.mqOD.UnknownDestCount = this.mqOD32.UnknownDestCount;
            this.mqOD.Version = this.mqOD32.Version;
            base.TrExit(method);
        }

        private void copyToMQOD32()
        {
            uint method = 0x419;
            this.TrEntry(method);
            this.mqOD32.AlternateSecurityId = this.mqOD.AlternateSecurityId;
            this.mqOD32.AlternateUserId = this.mqOD.AlternateUserId;
            this.mqOD32.DynamicQName = this.mqOD.DynamicQName;
            this.mqOD32.InvalidDestCount = this.mqOD.InvalidDestCount;
            this.mqOD32.KnownDestCount = this.mqOD.KnownDestCount;
            this.mqOD32.ObjectName = this.mqOD.ObjectName;
            this.mqOD32.ObjectQMgrName = this.mqOD.ObjectQMgrName;
            this.mqOD32.ObjectRecOffset = this.mqOD.ObjectRecOffset;
            this.mqOD32.ObjectType = this.mqOD.ObjectType;
            this.mqOD32.RecsPresent = this.mqOD.RecsPresent;
            this.mqOD32.ResolvedQMgrName = this.mqOD.ResolvedQMgrName;
            this.mqOD32.ResolvedQName = this.mqOD.ResolvedQName;
            this.mqOD32.ResolvedType = this.mqOD.ResolvedType;
            this.mqOD32.ResponseRecOffset = this.mqOD.ResponseRecOffset;
            this.mqOD32.UnknownDestCount = this.mqOD.UnknownDestCount;
            this.mqOD32.Version = this.mqOD.Version;
            base.TrExit(method);
        }

        public int GetLength()
        {
            uint method = 0x417;
            this.TrEntry(method);
            if (this.useNativePtrSz)
            {
                int num2 = Marshal.SizeOf(this.mqOD);
                base.TrExit(method, num2, 1);
                return num2;
            }
            int result = Marshal.SizeOf(this.mqOD32);
            base.TrExit(method, result, 2);
            return result;
        }

        private int GetPaddedLength(int len)
        {
            uint method = 0x465;
            this.TrEntry(method, new object[] { len });
            int num2 = 0;
            num2 = (4 - (len & 3)) & 3;
            int result = len + num2;
            base.TrExit(method, result);
            return result;
        }

        private int GetPaddedLength(string str)
        {
            uint method = 0x1f6;
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
            uint method = 0x1f7;
            this.TrEntry(method);
            int versionLength = this.GetVersionLength();
            if (this.mqOD.Version >= 4)
            {
                versionLength += this.GetPaddedLength(this.objectString.VSString);
                versionLength += this.GetPaddedLength(this.selectionString.VSString);
                versionLength += this.GetPaddedLength(this.resolvedObjectString.VSBufSize);
            }
            base.TrExit(method, versionLength);
            return versionLength;
        }

        internal int GetVersionLength()
        {
            return this.GetVersionLength(this.Version);
        }

        internal int GetVersionLength(int ver)
        {
            int size;
            uint method = 0x1f5;
            this.TrEntry(method, new object[] { ver });
            int result = 0;
            int num3 = 4;
            int num5 = 0;
            if (this.useNativePtrSz)
            {
                size = IntPtr.Size;
            }
            else
            {
                size = 4;
            }
            if (size == 8)
            {
                num5 = 4;
            }
            result += (((((4 + num3) + num3) + 0x30) + 0x30) + 0x30) + 12;
            if (ver == 1)
            {
                base.TrExit(method, result, 1);
                return result;
            }
            result += ((((((num3 + num3) + num3) + num3) + num3) + num3) + size) + size;
            if (ver == 2)
            {
                base.TrExit(method, result, 2);
                return result;
            }
            result += 0x88;
            if (ver == 3)
            {
                base.TrExit(method, result, 3);
                return result;
            }
            result += (((this.objectString.GetLength() + this.selectionString.GetLength()) + this.resolvedObjectString.GetLength()) + num3) + num5;
            base.TrExit(method, result, 4);
            return result;
        }

        public int ReadStruct(byte[] b, int Offset, int Length)
        {
            uint method = 0x1fa;
            this.TrEntry(method, new object[] { b, Offset, Length });
            int versionLength = this.GetVersionLength();
            int num3 = Offset;
            int length = this.GetLength();
            int requiredBufferSize = this.GetRequiredBufferSize();
            IntPtr zero = IntPtr.Zero;
            try
            {
                if (zero == IntPtr.Zero)
                {
                    zero = Marshal.AllocCoTaskMem(length);
                    this.mqODSize = versionLength;
                }
                Marshal.Copy(b, Offset, zero, versionLength);
                if (this.useNativePtrSz)
                {
                    this.mqOD = (MQBase.MQOD) Marshal.PtrToStructure(zero, typeof(MQBase.MQOD));
                    this.copyToMQOD32();
                }
                else
                {
                    this.mqOD32 = (MQBase.MQOD32) Marshal.PtrToStructure(zero, typeof(MQBase.MQOD32));
                    this.copyToMQOD();
                }
                num3 = Offset + requiredBufferSize;
                if (this.mqOD.Version >= 4)
                {
                    this.objectString.ReadStruct(b, Offset, Offset + this.OBJSTRING_OFFSET);
                    this.objectString.GetEndPosAligned(Offset);
                    this.selectionString.ReadStruct(b, Offset, Offset + this.SELSTRING_OFFSET);
                    this.selectionString.GetEndPosAligned(Offset);
                    this.resolvedObjectString.ReadStruct(b, Offset, Offset + this.ROBJSTRING_OFFSET);
                    this.resolvedObjectString.GetEndPosAligned(Offset);
                }
                this.ClearInvalidFields(this.mqOD.Version);
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
            return num3;
        }

        public int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x1f9;
            this.TrEntry(method, new object[] { b, Offset });
            int stringPos = Offset;
            int versionLength = this.GetVersionLength();
            int length = this.GetLength();
            int requiredBufferSize = this.GetRequiredBufferSize();
            IntPtr zero = IntPtr.Zero;
            try
            {
                this.CheckArrayLength();
                if (zero == IntPtr.Zero)
                {
                    zero = Marshal.AllocCoTaskMem(length);
                    this.mqODSize = length;
                }
                if (this.useNativePtrSz)
                {
                    Marshal.StructureToPtr(this.mqOD, zero, false);
                }
                else
                {
                    Marshal.StructureToPtr(this.mqOD32, zero, false);
                }
                Marshal.Copy(zero, b, Offset, versionLength);
                stringPos += length;
                if (this.mqOD.Version < 4)
                {
                    return requiredBufferSize;
                }
                this.calculateOffsets();
                if ((Offset + this.OBJSTRING_OFFSET) > 0)
                {
                    stringPos = this.objectString.WriteStruct(b, Offset, Offset + this.OBJSTRING_OFFSET, stringPos);
                }
                if ((Offset + this.SELSTRING_OFFSET) > 0)
                {
                    stringPos = this.selectionString.WriteStruct(b, Offset, Offset + this.SELSTRING_OFFSET, stringPos);
                }
                if ((Offset + this.ROBJSTRING_OFFSET) > 0)
                {
                    stringPos = this.resolvedObjectString.WriteStruct(b, Offset, Offset + this.ROBJSTRING_OFFSET, stringPos);
                }
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

        public byte[] AlternateSecurityId
        {
            get
            {
                return this.mqOD.AlternateSecurityId;
            }
            set
            {
                this.Version = (this.Version < 2) ? 2 : this.Version;
                this.mqOD.AlternateSecurityId = value;
                this.mqOD32.AlternateSecurityId = value;
            }
        }

        public string AlternateUserId
        {
            get
            {
                return base.GetString(this.mqOD.AlternateUserId);
            }
            set
            {
                byte[] buffer = new byte[12];
                base.GetBytes(value, ref buffer);
                this.mqOD.AlternateUserId = buffer;
                this.mqOD32.AlternateUserId = buffer;
            }
        }

        public string DynamicQName
        {
            get
            {
                return base.GetString(this.mqOD.DynamicQName);
            }
            set
            {
                byte[] buffer = new byte[0x30];
                base.GetBytesRightPad(value, ref buffer);
                this.mqOD.DynamicQName = buffer;
                this.mqOD32.DynamicQName = buffer;
            }
        }

        public int InvalidDestCount
        {
            get
            {
                return this.mqOD.InvalidDestCount;
            }
            set
            {
                this.mqOD.InvalidDestCount = value;
                this.mqOD32.InvalidDestCount = value;
            }
        }

        public int KnownDestCount
        {
            get
            {
                return this.mqOD.KnownDestCount;
            }
            set
            {
                this.mqOD.KnownDestCount = value;
                this.mqOD32.KnownDestCount = value;
            }
        }

        public string ObjectName
        {
            get
            {
                return base.GetString(this.mqOD.ObjectName);
            }
            set
            {
                byte[] buffer = new byte[0x30];
                base.GetBytes(value, ref buffer);
                this.mqOD.ObjectName = buffer;
                this.mqOD32.ObjectName = buffer;
            }
        }

        public string ObjectQMgrName
        {
            get
            {
                return base.GetString(this.mqOD.ObjectQMgrName);
            }
            set
            {
                byte[] buffer = new byte[0x30];
                base.GetBytes(value, ref buffer);
                this.mqOD.ObjectQMgrName = buffer;
                this.mqOD32.ObjectQMgrName = buffer;
            }
        }

        public int ObjectRecordOffset
        {
            get
            {
                return this.mqOD.ObjectRecOffset;
            }
            set
            {
                this.mqOD.ObjectRecOffset = value;
                this.mqOD32.ObjectRecOffset = value;
            }
        }

        public IntPtr ObjectRecordPtr
        {
            get
            {
                return this.mqOD.ObjectRecPtr;
            }
            set
            {
                this.mqOD.ObjectRecPtr = value;
            }
        }

        public int ObjectRecordPtr32
        {
            get
            {
                return this.mqOD32.ObjectRecPtr;
            }
            set
            {
                this.mqOD32.ObjectRecPtr = value;
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

        public int ObjectType
        {
            get
            {
                return this.mqOD.ObjectType;
            }
            set
            {
                this.mqOD.ObjectType = value;
                this.mqOD32.ObjectType = value;
            }
        }

        public int RecordsPresent
        {
            get
            {
                return this.mqOD.RecsPresent;
            }
            set
            {
                this.mqOD.RecsPresent = value;
                this.mqOD32.RecsPresent = value;
            }
        }

        public MQCHARV ResolvedObjectString
        {
            get
            {
                return this.resolvedObjectString;
            }
            set
            {
                this.resolvedObjectString = value;
            }
        }

        public byte[] ResolvedQueueManagerName
        {
            get
            {
                return this.mqOD.ResolvedQMgrName;
            }
            set
            {
                this.Version = (this.Version < 2) ? 2 : this.Version;
                this.mqOD.ResolvedQMgrName = value;
                this.mqOD32.ResolvedQMgrName = value;
            }
        }

        public byte[] ResolvedQueueName
        {
            get
            {
                return this.mqOD.ResolvedQName;
            }
            set
            {
                this.Version = (this.Version < 2) ? 2 : this.Version;
                this.mqOD.ResolvedQName = value;
                this.mqOD32.ResolvedQName = value;
            }
        }

        public int ResolvedType
        {
            get
            {
                return this.mqOD.ResolvedType;
            }
            set
            {
                this.Version = (this.Version < 3) ? 3 : this.Version;
                this.mqOD.ResolvedType = value;
            }
        }

        public int ResponseRecordOffset
        {
            get
            {
                return this.mqOD.ResponseRecOffset;
            }
            set
            {
                this.mqOD.ResponseRecOffset = value;
                this.mqOD32.ResponseRecOffset = value;
            }
        }

        public IntPtr ResponseRecordPtr
        {
            get
            {
                return this.mqOD.ResponseRecPtr;
            }
            set
            {
                this.mqOD.ResponseRecPtr = value;
            }
        }

        public int ResponseRecordPtr32
        {
            get
            {
                return this.mqOD32.ResponseRecPtr;
            }
            set
            {
                this.mqOD32.ResponseRecPtr = value;
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
                return this.mqOD.StrucId;
            }
            set
            {
                this.mqOD.StrucId = value;
                this.mqOD32.StrucId = value;
            }
        }

        public MQBase.MQOD StructMQOD
        {
            get
            {
                this.CheckArrayLength();
                return this.mqOD;
            }
            set
            {
                this.mqOD = value;
            }
        }

        public int UnknownDestCount
        {
            get
            {
                return this.mqOD.UnknownDestCount;
            }
            set
            {
                this.mqOD.UnknownDestCount = value;
                this.mqOD32.UnknownDestCount = value;
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
                this.selectionString.UseNativePtrSz = value;
                this.resolvedObjectString.UseNativePtrSz = value;
                this.calculateOffsets();
            }
        }

        public int Version
        {
            get
            {
                return this.mqOD.Version;
            }
            set
            {
                this.mqOD.Version = value;
                this.mqOD32.Version = value;
            }
        }
    }
}

