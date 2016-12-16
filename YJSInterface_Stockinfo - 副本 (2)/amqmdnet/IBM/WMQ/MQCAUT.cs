namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class MQCAUT : MQBase
    {
        internal structMQCAUT caut;
        private string password;
        private readonly int PASSWORD_OFFSET = 20;
        private static int pwo;
        private static readonly byte[] rfpCAUT_ID = new byte[] { 0x43, 0x41, 0x55, 0x54 };
        private const string sccsid = "%Z% %W% %I% %E% %U%";
        private string userId;

        internal MQCAUT()
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%");
            this.caut = new structMQCAUT();
            this.caut.Id = rfpCAUT_ID;
        }

        internal int GetHdrLength()
        {
            return Marshal.SizeOf(this.caut);
        }

        internal int GetLength()
        {
            int introduced1 = Marshal.SizeOf(this.caut);
            return ((introduced1 + this.caut.CAUTUserIdLen) + this.caut.CAUTPasswordLen);
        }

        internal int GetPasswordOffset()
        {
            return this.PASSWORD_OFFSET;
        }

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 20;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int hdrLength = this.GetHdrLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(hdrLength);
                Marshal.StructureToPtr(this.caut, zero, false);
                Marshal.Copy(zero, b, Offset, hdrLength);
                if (this.userId != null)
                {
                    Encoding.ASCII.GetBytes(this.userId, 0, this.caut.CAUTUserIdLen, b, Offset + hdrLength);
                    hdrLength += this.caut.CAUTUserIdLen;
                }
                if (this.password != null)
                {
                    Encoding.ASCII.GetBytes(this.password, 0, this.caut.CAUTPasswordLen, b, Offset + hdrLength);
                    hdrLength += this.caut.CAUTPasswordLen;
                }
                Marshal.FreeCoTaskMem(zero);
                base.TrText(method, hdrLength + "bytes written");
            }
            finally
            {
                base.TrExit(method, hdrLength);
            }
            return hdrLength;
        }

        internal int AuthType
        {
            get
            {
                return this.caut.AuthType;
            }
            set
            {
                this.caut.AuthType = value;
            }
        }

        internal string Password
        {
            get
            {
                return this.password;
            }
            set
            {
                this.password = value;
                if (this.password != null)
                {
                    this.caut.CAUTPasswordLen = this.caut.CAUTPasswdMaxLen = this.password.Length;
                }
                else
                {
                    this.caut.CAUTPasswordLen = this.caut.CAUTPasswdMaxLen = 0;
                }
            }
        }

        internal string UserId
        {
            get
            {
                return this.userId;
            }
            set
            {
                this.userId = value;
                if (this.userId != null)
                {
                    this.caut.CAUTUserIdLen = this.caut.CAUTUserIdMaxLen = this.userId.Length;
                }
                else
                {
                    this.caut.CAUTUserIdLen = this.caut.CAUTUserIdMaxLen = 0;
                }
            }
        }
    }
}

