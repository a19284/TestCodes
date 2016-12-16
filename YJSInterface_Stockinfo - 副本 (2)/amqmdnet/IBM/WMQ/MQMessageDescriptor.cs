namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQMessageDescriptor : MQBase
    {
        private MQBase.MQMD mqmd;
        private static readonly byte[] MQMD_STRUC_ID = new byte[] { 0x4d, 0x44, 0x20, 0x20 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public MQMessageDescriptor()
        {
            this.mqmd = new MQBase.MQMD();
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.ClearInvalidFields(0);
        }

        internal MQMessageDescriptor(MQMessageDescriptor md)
        {
            this.mqmd = new MQBase.MQMD();
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { md });
            MQBase.MQMD mqmd = md.mqmd;
            this.mqmd.StrucId = (byte[]) this.mqmd.StrucId.Clone();
            this.mqmd.Version = mqmd.Version;
            this.mqmd.Report = mqmd.Report;
            this.mqmd.MsgType = mqmd.MsgType;
            this.mqmd.Expiry = mqmd.Expiry;
            this.mqmd.Feedback = mqmd.Feedback;
            this.mqmd.Encoding = mqmd.Encoding;
            this.mqmd.CodedCharacterSetId = mqmd.CodedCharacterSetId;
            this.mqmd.Format = (byte[]) mqmd.Format.Clone();
            this.mqmd.Priority = mqmd.Priority;
            this.mqmd.Persistence = mqmd.Persistence;
            this.mqmd.MsgId = (byte[]) mqmd.MsgId.Clone();
            this.mqmd.CorrelId = (byte[]) mqmd.CorrelId.Clone();
            this.mqmd.BackoutCount = mqmd.BackoutCount;
            this.mqmd.ReplyToQ = (byte[]) mqmd.ReplyToQ.Clone();
            this.mqmd.ReplyToQMgr = (byte[]) mqmd.ReplyToQMgr.Clone();
            this.mqmd.UserId = (byte[]) mqmd.UserId.Clone();
            this.mqmd.AccountingToken = (byte[]) mqmd.AccountingToken.Clone();
            this.mqmd.ApplIdentityData = (byte[]) mqmd.ApplIdentityData.Clone();
            this.mqmd.PutApplType = mqmd.PutApplType;
            this.mqmd.PutApplName = (byte[]) mqmd.PutApplName.Clone();
            this.mqmd.PutDate = (byte[]) mqmd.PutDate.Clone();
            this.mqmd.PutTime = (byte[]) mqmd.PutTime.Clone();
            this.mqmd.ApplOriginData = (byte[]) mqmd.ApplOriginData.Clone();
            this.mqmd.GroupId = (byte[]) mqmd.GroupId.Clone();
            this.mqmd.MsgSequenceNumber = mqmd.MsgSequenceNumber;
            this.mqmd.Offset = mqmd.Offset;
            this.mqmd.MsgFlags = mqmd.MsgFlags;
            this.mqmd.OriginalLength = mqmd.OriginalLength;
        }

        private void CheckArrayLength()
        {
            uint method = 0x464;
            this.TrEntry(method);
            if ((this.mqmd.accountingToken != null) && (this.mqmd.accountingToken.Length != 0x20))
            {
                this.mqmd.accountingToken = this.ResizeArrayToCorrectLength(this.mqmd.accountingToken, 0x20);
            }
            if ((this.mqmd.applIdentityData != null) && (this.mqmd.applIdentityData.Length != 0x20))
            {
                this.mqmd.applIdentityData = this.ResizeArrayToCorrectLength(this.mqmd.applIdentityData, 0x20);
            }
            if ((this.mqmd.applOriginData != null) && (this.mqmd.applOriginData.Length != 4))
            {
                this.mqmd.applOriginData = this.ResizeArrayToCorrectLength(this.mqmd.applOriginData, 4);
            }
            if ((this.mqmd.correlId != null) && (this.mqmd.correlId.Length != 0x18))
            {
                this.mqmd.correlId = this.ResizeArrayToCorrectLength(this.mqmd.correlId, 0x18);
            }
            if ((this.mqmd.Format != null) && (this.mqmd.Format.Length != 8))
            {
                this.mqmd.Format = this.ResizeArrayToCorrectLength(this.mqmd.Format, 8);
            }
            if ((this.mqmd.groupId != null) && (this.mqmd.groupId.Length != 0x18))
            {
                this.mqmd.groupId = this.ResizeArrayToCorrectLength(this.mqmd.groupId, 0x18);
            }
            if ((this.mqmd.msgId != null) && (this.mqmd.msgId.Length != 0x18))
            {
                this.mqmd.msgId = this.ResizeArrayToCorrectLength(this.mqmd.msgId, 0x18);
            }
            if ((this.mqmd.putApplName != null) && (this.mqmd.putApplName.Length != 0x1c))
            {
                this.mqmd.putApplName = this.ResizeArrayToCorrectLength(this.mqmd.putApplName, 0x1c);
            }
            if ((this.mqmd.putDate != null) && (this.mqmd.putDate.Length != 8))
            {
                this.mqmd.putDate = this.ResizeArrayToCorrectLength(this.mqmd.putDate, 8);
            }
            if ((this.mqmd.putTime != null) && (this.mqmd.putTime.Length != 8))
            {
                this.mqmd.putTime = this.ResizeArrayToCorrectLength(this.mqmd.putTime, 8);
            }
            if ((this.mqmd.replyToQ != null) && (this.mqmd.replyToQ.Length != 0x30))
            {
                this.mqmd.replyToQ = this.ResizeArrayToCorrectLength(this.mqmd.replyToQ, 0x30);
            }
            if ((this.mqmd.replyToQMgr != null) && (this.mqmd.replyToQMgr.Length != 0x30))
            {
                this.mqmd.replyToQMgr = this.ResizeArrayToCorrectLength(this.mqmd.replyToQMgr, 0x30);
            }
            if ((this.mqmd.userId != null) && (this.mqmd.userId.Length != 12))
            {
                this.mqmd.userId = this.ResizeArrayToCorrectLength(this.mqmd.userId, 12);
            }
            base.TrExit(method);
        }

        private void ClearInvalidFields(int Version)
        {
            uint method = 0x19c;
            this.TrEntry(method, new object[] { Version });
            switch (Version)
            {
                case 0:
                {
                    this.mqmd.StrucId = MQMD_STRUC_ID;
                    this.mqmd.Report = 0;
                    this.mqmd.MsgType = 8;
                    this.mqmd.Expiry = -1;
                    this.mqmd.Feedback = 0;
                    this.mqmd.Encoding = 0x222;
                    this.mqmd.CodedCharacterSetId = 0;
                    byte[] buffer = new byte[8];
                    base.GetBytes("        ", ref buffer);
                    this.mqmd.Format = buffer;
                    this.mqmd.Priority = -1;
                    this.mqmd.Persistence = 2;
                    this.mqmd.MsgId = new byte[0x18];
                    this.mqmd.CorrelId = new byte[0x18];
                    this.mqmd.BackoutCount = 0;
                    this.mqmd.ReplyToQ = new byte[0x30];
                    this.mqmd.ReplyToQMgr = new byte[0x30];
                    this.mqmd.UserId = new byte[12];
                    this.mqmd.AccountingToken = new byte[0x20];
                    this.mqmd.ApplIdentityData = new byte[0x20];
                    this.mqmd.PutApplType = 0;
                    this.mqmd.PutApplName = new byte[0x1c];
                    this.mqmd.PutDate = new byte[8];
                    this.mqmd.PutTime = new byte[8];
                    this.mqmd.ApplOriginData = new byte[4];
                    break;
                }
                case 1:
                    break;

                default:
                    goto Label_01E4;
            }
            this.mqmd.GroupId = new byte[0x18];
            this.mqmd.MsgSequenceNumber = 1;
            this.mqmd.Offset = 0;
            this.mqmd.MsgFlags = 0;
            this.mqmd.OriginalLength = -1;
        Label_01E4:
            if (Version == 0)
            {
                this.mqmd.Version = 1;
            }
            base.TrExit(method);
        }

        public int GetLength()
        {
            return Marshal.SizeOf(this.mqmd);
        }

        public int GetVersionLength()
        {
            uint method = 0x19d;
            this.TrEntry(method);
            switch (this.mqmd.Version)
            {
                case 1:
                    base.TrExit(method, 0x144, 1);
                    return 0x144;

                case 2:
                    base.TrExit(method, 0x16c, 2);
                    return 0x16c;
            }
            base.TrExit(method, 0, 3);
            return 0;
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x19f;
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
                this.mqmd = (MQBase.MQMD) Marshal.PtrToStructure(zero, typeof(MQBase.MQMD));
                this.ClearInvalidFields(this.mqmd.Version);
                Marshal.FreeCoTaskMem(zero);
                result = Offset + this.GetVersionLength();
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        public int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x19e;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int versionLength = this.GetVersionLength();
            try
            {
                this.CheckArrayLength();
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.mqmd, zero, false);
                Marshal.Copy(zero, b, Offset, versionLength);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, versionLength);
            }
            return versionLength;
        }

        public byte[] AccountingToken
        {
            get
            {
                return this.mqmd.AccountingToken;
            }
            set
            {
                this.mqmd.AccountingToken = value;
            }
        }

        public byte[] ApplIdentityData
        {
            get
            {
                return this.mqmd.ApplIdentityData;
            }
            set
            {
                this.mqmd.ApplIdentityData = value;
            }
        }

        public byte[] ApplOriginData
        {
            get
            {
                return this.mqmd.ApplOriginData;
            }
            set
            {
                this.mqmd.ApplOriginData = value;
            }
        }

        public int BackoutCount
        {
            get
            {
                return this.mqmd.BackoutCount;
            }
            set
            {
                this.mqmd.BackoutCount = value;
            }
        }

        public int Ccsid
        {
            get
            {
                return this.mqmd.CodedCharacterSetId;
            }
            set
            {
                this.mqmd.CodedCharacterSetId = value;
            }
        }

        public int CodedCharacterSetId
        {
            get
            {
                return this.mqmd.CodedCharacterSetId;
            }
            set
            {
                this.mqmd.CodedCharacterSetId = value;
            }
        }

        public byte[] CorrelId
        {
            get
            {
                return this.mqmd.CorrelId;
            }
            set
            {
                this.mqmd.CorrelId = value;
            }
        }

        public int Encoding
        {
            get
            {
                return this.mqmd.Encoding;
            }
            set
            {
                this.mqmd.Encoding = value;
            }
        }

        public int Expiry
        {
            get
            {
                return this.mqmd.Expiry;
            }
            set
            {
                this.mqmd.Expiry = value;
            }
        }

        public int Feedback
        {
            get
            {
                return this.mqmd.Feedback;
            }
            set
            {
                this.mqmd.Feedback = value;
            }
        }

        public byte[] Format
        {
            get
            {
                return this.mqmd.Format;
            }
            set
            {
                this.mqmd.Format = value;
            }
        }

        public byte[] GroupID
        {
            get
            {
                return this.mqmd.GroupId;
            }
            set
            {
                this.mqmd.GroupId = value;
            }
        }

        public int MsgFlags
        {
            get
            {
                return this.mqmd.MsgFlags;
            }
            set
            {
                this.mqmd.MsgFlags = value;
            }
        }

        public byte[] MsgId
        {
            get
            {
                return this.mqmd.MsgId;
            }
            set
            {
                this.mqmd.MsgId = value;
            }
        }

        public int MsgSequenceNumber
        {
            get
            {
                return this.mqmd.MsgSequenceNumber;
            }
            set
            {
                this.mqmd.MsgSequenceNumber = value;
            }
        }

        public int MsgType
        {
            get
            {
                return this.mqmd.MsgType;
            }
            set
            {
                this.mqmd.MsgType = value;
            }
        }

        public int Offset
        {
            get
            {
                return this.mqmd.Offset;
            }
            set
            {
                this.mqmd.Offset = value;
            }
        }

        public int OriginalLength
        {
            get
            {
                return this.mqmd.OriginalLength;
            }
            set
            {
                this.mqmd.OriginalLength = value;
            }
        }

        public int Persistence
        {
            get
            {
                return this.mqmd.Persistence;
            }
            set
            {
                this.mqmd.Persistence = value;
            }
        }

        public int Priority
        {
            get
            {
                return this.mqmd.Priority;
            }
            set
            {
                this.mqmd.Priority = value;
            }
        }

        public byte[] PutApplName
        {
            get
            {
                return this.mqmd.PutApplName;
            }
            set
            {
                this.mqmd.PutApplName = value;
            }
        }

        public int PutApplType
        {
            get
            {
                return this.mqmd.PutApplType;
            }
            set
            {
                this.mqmd.PutApplType = value;
            }
        }

        public byte[] PutDate
        {
            get
            {
                return this.mqmd.PutDate;
            }
            set
            {
                this.mqmd.PutDate = value;
            }
        }

        public byte[] PutTime
        {
            get
            {
                return this.mqmd.PutTime;
            }
            set
            {
                this.mqmd.PutTime = value;
            }
        }

        public byte[] ReplyToQueue
        {
            get
            {
                return this.mqmd.ReplyToQ;
            }
            set
            {
                this.mqmd.ReplyToQ = value;
            }
        }

        public byte[] ReplyToQueueMgr
        {
            get
            {
                return this.mqmd.ReplyToQMgr;
            }
            set
            {
                this.mqmd.ReplyToQMgr = value;
            }
        }

        public int Report
        {
            get
            {
                return this.mqmd.Report;
            }
            set
            {
                this.mqmd.Report = value;
            }
        }

        public MQBase.MQMD StructMQMD
        {
            get
            {
                this.CheckArrayLength();
                return this.mqmd;
            }
            set
            {
                this.mqmd = value;
            }
        }

        public byte[] UserID
        {
            get
            {
                return this.mqmd.UserId;
            }
            set
            {
                this.mqmd.UserId = value;
            }
        }

        public int Version
        {
            get
            {
                return this.mqmd.Version;
            }
            set
            {
                this.mqmd.Version = value;
            }
        }
    }
}

