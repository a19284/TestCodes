namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public class MQDLH : MQBase
    {
        internal MQBase.structMQDLH mqdlh;
        private const int MQDLH_SIZEV1 = 0xac;
        private const string sccsid = "%Z% %W% %I% %E% %U%";

        public MQDLH()
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%");
            this.mqdlh = new MQBase.structMQDLH();
            this.mqdlh.StrucID = new byte[4];
            this.mqdlh.StrucID = new byte[] { 0x44, 0x4c, 0x48, 0x20 };
            this.mqdlh.Version = 1;
            this.mqdlh.Reason = 0;
            this.mqdlh.DestQMgrName = new byte[0x30];
            this.mqdlh.DestQName = new byte[0x30];
            this.mqdlh.Encoding = 0x222;
            this.mqdlh.CodedCharSetId = 0;
            this.mqdlh.Format = new byte[8];
            this.mqdlh.PutApplType = 0;
            this.mqdlh.PutApplName = new byte[0x1c];
            this.mqdlh.PutDate = new byte[8];
            this.mqdlh.PutTime = new byte[8];
        }

        private void CheckArrayLength()
        {
            uint method = 0x462;
            this.TrEntry(method);
            if ((this.mqdlh.DestQMgrName != null) && (this.mqdlh.DestQMgrName.Length != 0x30))
            {
                this.mqdlh.DestQMgrName = this.ResizeArrayToCorrectLength(this.mqdlh.DestQMgrName, 0x30);
            }
            if ((this.mqdlh.DestQName != null) && (this.mqdlh.DestQName.Length != 0x30))
            {
                this.mqdlh.DestQName = this.ResizeArrayToCorrectLength(this.mqdlh.DestQName, 0x30);
            }
            if ((this.mqdlh.PutApplName != null) && (this.mqdlh.PutApplName.Length != 0x1c))
            {
                this.mqdlh.PutApplName = this.ResizeArrayToCorrectLength(this.mqdlh.PutApplName, 0x1c);
            }
            if ((this.mqdlh.PutDate != null) && (this.mqdlh.PutDate.Length != 8))
            {
                this.mqdlh.PutDate = this.ResizeArrayToCorrectLength(this.mqdlh.PutDate, 8);
            }
            if ((this.mqdlh.PutTime != null) && (this.mqdlh.PutTime.Length != 8))
            {
                this.mqdlh.PutTime = this.ResizeArrayToCorrectLength(this.mqdlh.PutTime, 8);
            }
            base.TrExit(method);
        }

        public int GetLength()
        {
            return Marshal.SizeOf(this.mqdlh);
        }

        public int GetSize(int version)
        {
            int num2;
            uint method = 0x412;
            this.TrEntry(method, new object[] { version });
            try
            {
                if (version != 1)
                {
                    throw new MQException(2, 0x893);
                }
                num2 = 0xac;
            }
            finally
            {
                base.TrExit(method, 0xac);
            }
            return num2;
        }

        public int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x414;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            zero = Marshal.AllocCoTaskMem(length);
            if (length > (b.Length - Offset))
            {
                length = b.Length - Offset;
            }
            try
            {
                Marshal.Copy(b, Offset, zero, length);
                this.mqdlh = (MQBase.structMQDLH) Marshal.PtrToStructure(zero, typeof(MQBase.structMQDLH));
            }
            finally
            {
                Marshal.FreeCoTaskMem(zero);
                base.TrExit(method);
            }
            return (Offset + this.GetLength());
        }

        public void SetPutDateTime(int year, int month, int day, int hour, int minute, int second, int millis)
        {
            uint method = 0x415;
            this.TrEntry(method, new object[] { year, month, day, hour, minute, second, millis });
            StringBuilder builder = new StringBuilder();
            builder.Append(NmqiTools.Fix(year, 4));
            builder.Append(NmqiTools.Fix(month, 2));
            builder.Append(NmqiTools.Fix(day, 2));
            this.PutDate = builder.ToString();
            builder = new StringBuilder();
            builder.Append(NmqiTools.Fix(hour, 2));
            builder.Append(NmqiTools.Fix(minute, 2));
            builder.Append(NmqiTools.Fix(second, 2));
            builder.Append(NmqiTools.Fix(millis, 2));
            this.PutTime = builder.ToString();
            base.TrExit(method);
        }

        public int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x413;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            this.CheckArrayLength();
            zero = Marshal.AllocCoTaskMem(length);
            try
            {
                Marshal.StructureToPtr(this.mqdlh, zero, false);
                Marshal.Copy(zero, b, Offset, length);
            }
            finally
            {
                Marshal.FreeCoTaskMem(zero);
                base.TrExit(method);
            }
            return length;
        }

        public int CodedCharSetId
        {
            get
            {
                return this.mqdlh.CodedCharSetId;
            }
            set
            {
                this.mqdlh.CodedCharSetId = value;
            }
        }

        public string DestQMgrName
        {
            get
            {
                return System.Text.Encoding.ASCII.GetString(this.mqdlh.DestQMgrName);
            }
            set
            {
                this.mqdlh.DestQMgrName = System.Text.Encoding.ASCII.GetBytes(value);
            }
        }

        public string DestQName
        {
            get
            {
                return System.Text.Encoding.ASCII.GetString(this.mqdlh.DestQName);
            }
            set
            {
                this.mqdlh.DestQName = System.Text.Encoding.ASCII.GetBytes(value);
            }
        }

        public int Encoding
        {
            get
            {
                return this.mqdlh.Encoding;
            }
            set
            {
                this.mqdlh.Encoding = value;
            }
        }

        public string Format
        {
            get
            {
                return System.Text.Encoding.ASCII.GetString(this.mqdlh.Format);
            }
            set
            {
                this.mqdlh.Format = System.Text.Encoding.ASCII.GetBytes(value);
            }
        }

        public string PutApplName
        {
            get
            {
                return System.Text.Encoding.ASCII.GetString(this.mqdlh.PutApplName);
            }
            set
            {
                this.mqdlh.PutApplName = System.Text.Encoding.ASCII.GetBytes(value);
            }
        }

        public int PutApplType
        {
            get
            {
                return this.mqdlh.PutApplType;
            }
            set
            {
                this.mqdlh.PutApplType = value;
            }
        }

        public string PutDate
        {
            get
            {
                return System.Text.Encoding.ASCII.GetString(this.mqdlh.PutDate);
            }
            set
            {
                this.mqdlh.PutDate = System.Text.Encoding.ASCII.GetBytes(value);
            }
        }

        public string PutTime
        {
            get
            {
                return System.Text.Encoding.ASCII.GetString(this.mqdlh.PutTime);
            }
            set
            {
                this.mqdlh.PutTime = System.Text.Encoding.ASCII.GetBytes(value);
            }
        }

        public int Reason
        {
            get
            {
                return this.mqdlh.Reason;
            }
            set
            {
                this.mqdlh.Reason = value;
            }
        }

        public int Version
        {
            get
            {
                return this.mqdlh.Version;
            }
            set
            {
                this.mqdlh.Version = value;
            }
        }
    }
}

