namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    public class MQGetMessageOptions : MQBaseObject
    {
        private MQBase.MQGMO mqGMO;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public MQGetMessageOptions()
        {
            this.mqGMO = new MQBase.MQGMO();
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.ClearInvalidFields(0);
        }

        public MQGetMessageOptions(MQGetMessageOptions mqgmo)
        {
            this.mqGMO = new MQBase.MQGMO();
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { mqgmo });
            this.mqGMO.StrucId = (byte[]) mqgmo.mqGMO.StrucId.Clone();
            this.mqGMO.Version = mqgmo.mqGMO.Version;
            this.mqGMO.Options = mqgmo.mqGMO.Options;
            this.mqGMO.WaitInterval = mqgmo.mqGMO.WaitInterval;
            this.mqGMO.Signal1 = mqgmo.mqGMO.Signal1;
            this.mqGMO.Signal2 = mqgmo.mqGMO.Signal2;
            this.mqGMO.ResolvedQName = new byte[0x30];
            Buffer.BlockCopy(mqgmo.mqGMO.ResolvedQName, 0, this.mqGMO.ResolvedQName, 0, mqgmo.mqGMO.ResolvedQName.Length);
            this.mqGMO.MatchOptions = mqgmo.mqGMO.MatchOptions;
            this.mqGMO.GroupStatus = mqgmo.mqGMO.GroupStatus;
            this.mqGMO.SegmentStatus = mqgmo.mqGMO.SegmentStatus;
            this.mqGMO.Segmentation = mqgmo.mqGMO.Segmentation;
            this.mqGMO.Reserved1 = mqgmo.mqGMO.Reserved1;
            this.mqGMO.MsgToken = new byte[0x10];
            Buffer.BlockCopy(mqgmo.mqGMO.MsgToken, 0, this.mqGMO.MsgToken, 0, mqgmo.mqGMO.MsgToken.Length);
            this.mqGMO.ReturnedLength = mqgmo.mqGMO.ReturnedLength;
        }

        private void CheckArrayLength()
        {
            uint method = 0x463;
            this.TrEntry(method);
            try
            {
                if ((this.mqGMO.msgToken != null) && (this.mqGMO.msgToken.Length != 0x10))
                {
                    this.mqGMO.msgToken = this.ResizeArrayToCorrectLength(this.mqGMO.msgToken, 0x10);
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        private void ClearInvalidFields(int Version)
        {
            uint method = 0xc3;
            this.TrEntry(method, new object[] { Version });
            switch (Version)
            {
                case 0:
                    this.mqGMO.StrucId = new byte[] { 0x47, 0x4d, 0x4f, 0x20 };
                    this.mqGMO.Options = 0;
                    this.mqGMO.WaitInterval = 0;
                    this.mqGMO.Signal1 = 0;
                    this.mqGMO.Signal2 = 0;
                    this.mqGMO.ResolvedQName = new byte[0x30];
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_0106;

                default:
                    goto Label_0124;
            }
            this.mqGMO.MatchOptions = 3;
            this.mqGMO.GroupStatus = 0x20;
            this.mqGMO.SegmentStatus = 0x20;
            this.mqGMO.Segmentation = 0x20;
            this.mqGMO.Reserved1 = 0x20;
        Label_0106:
            this.mqGMO.MsgToken = new byte[0x10];
            this.mqGMO.ReturnedLength = -1;
        Label_0124:
            if (Version == 0)
            {
                this.mqGMO.Version = 1;
            }
            base.TrExit(method);
        }

        public int GetLength()
        {
            return Marshal.SizeOf(this.mqGMO);
        }

        public int GetVersionLength()
        {
            uint method = 0xc4;
            this.TrEntry(method);
            switch (this.mqGMO.Version)
            {
                case 1:
                    base.TrExit(method, 0x48, 1);
                    return 0x48;

                case 2:
                    base.TrExit(method, 80, 2);
                    return 80;

                case 3:
                    base.TrExit(method, 100, 3);
                    return 100;
            }
            base.TrExit(method, 0, 4);
            return 0;
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0xc6;
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
                this.mqGMO = (MQBase.MQGMO) Marshal.PtrToStructure(zero, typeof(MQBase.MQGMO));
                this.ClearInvalidFields(this.mqGMO.Version);
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
            uint method = 0xc5;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int versionLength = this.GetVersionLength();
            try
            {
                this.CheckArrayLength();
                zero = Marshal.AllocCoTaskMem(this.GetLength());
                Marshal.StructureToPtr(this.mqGMO, zero, false);
                Marshal.Copy(zero, b, Offset, versionLength);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, versionLength);
            }
            return versionLength;
        }

        public byte GroupStatus
        {
            get
            {
                return this.mqGMO.GroupStatus;
            }
            set
            {
                this.mqGMO.GroupStatus = value;
            }
        }

        public int MatchOptions
        {
            get
            {
                return this.mqGMO.MatchOptions;
            }
            set
            {
                this.mqGMO.Version = (this.mqGMO.Version == 3) ? 3 : 2;
                this.mqGMO.MatchOptions = value;
            }
        }

        public byte[] MsgToken
        {
            get
            {
                return this.mqGMO.MsgToken;
            }
            set
            {
                this.mqGMO.MsgToken = value;
            }
        }

        public int Options
        {
            get
            {
                return this.mqGMO.Options;
            }
            set
            {
                this.mqGMO.Options = value;
            }
        }

        public string ResolvedQueueName
        {
            get
            {
                return base.GetString(this.mqGMO.ResolvedQName, this.mqGMO.ResolvedQName.Length);
            }
            set
            {
                if (value == null)
                {
                    this.mqGMO.ResolvedQName = null;
                }
                else
                {
                    byte[] buffer = new byte[0x30];
                    base.GetBytes(value, ref buffer);
                    this.mqGMO.ResolvedQName = buffer;
                }
            }
        }

        public int ReturnedLength
        {
            get
            {
                return this.mqGMO.ReturnedLength;
            }
            set
            {
                this.mqGMO.ReturnedLength = value;
            }
        }

        public byte Segmentation
        {
            get
            {
                return this.mqGMO.Segmentation;
            }
            set
            {
                this.mqGMO.Segmentation = value;
            }
        }

        public byte SegmentStatus
        {
            get
            {
                return this.mqGMO.SegmentStatus;
            }
            set
            {
                this.mqGMO.SegmentStatus = value;
            }
        }

        public int Signal1
        {
            get
            {
                return this.mqGMO.Signal1;
            }
            set
            {
                this.mqGMO.Signal1 = value;
            }
        }

        public int Signal2
        {
            get
            {
                return this.mqGMO.Signal2;
            }
            set
            {
                this.mqGMO.Signal2 = value;
            }
        }

        public MQBase.MQGMO StructMQGMO
        {
            get
            {
                this.CheckArrayLength();
                return this.mqGMO;
            }
            set
            {
                this.mqGMO = value;
            }
        }

        public int Version
        {
            get
            {
                return this.mqGMO.Version;
            }
            set
            {
                this.mqGMO.Version = value;
            }
        }

        public int WaitInterval
        {
            get
            {
                return this.mqGMO.WaitInterval;
            }
            set
            {
                this.mqGMO.WaitInterval = value;
            }
        }
    }
}

