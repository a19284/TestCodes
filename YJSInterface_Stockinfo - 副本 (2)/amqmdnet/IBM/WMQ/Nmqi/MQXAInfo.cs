namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class MQXAInfo : MQBase
    {
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private XAINFO xa_info;
        internal const int XAINFO_STRUCT_LENGTH = 260;

        public MQXAInfo()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.xa_info = new XAINFO();
            this.xa_info.xa_info = new byte[0x100];
        }

        public override void AddFieldsToFormatter(NmqiStructureFormatter fmt)
        {
            base.AddFieldsToFormatter(fmt);
            fmt.Add("Format", this.xa_info.xa_info_length);
            fmt.Add("Xa_Info", Encoding.Unicode.GetString(this.xa_info.xa_info, 0, this.xa_info.xa_info_length));
        }

        private string CovertXAStringToBytes(string xaOpenString)
        {
            uint method = 0x501;
            this.TrEntry(method, new object[] { xaOpenString });
            string result = null;
            string str2 = "qmname";
            string str3 = xaOpenString.ToLower();
            int length = str3.Length;
            int index = str3.IndexOf("qmname");
            if (index >= 0)
            {
                index += str2.Length;
                if (length > index)
                {
                    int num4 = str3.IndexOf('=', index);
                    if (num4 > 0)
                    {
                        do
                        {
                            num4++;
                        }
                        while ((length > num4) && (str3.ToCharArray()[num4] == ' '));
                        if (length > num4)
                        {
                            int num5 = str3.IndexOf(',', num4);
                            if (num5 < 0)
                            {
                                num5 = length;
                            }
                            do
                            {
                                num5--;
                            }
                            while ((num5 > 0) && (str3.ToCharArray()[num5] == ' '));
                            if ((num5 - num4) > 0)
                            {
                                result = str3.Substring(num4, (num5 - num4) + 1);
                            }
                        }
                    }
                }
            }
            base.TrExit(method, result);
            return result;
        }

        public int GetLength()
        {
            return 260;
        }

        public int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x503;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            int result = 0;
            try
            {
                if (zero == IntPtr.Zero)
                {
                    zero = Marshal.AllocCoTaskMem(length);
                }
                if (length > (b.Length - Offset))
                {
                    length = b.Length - Offset;
                }
                Marshal.Copy(b, Offset, zero, length);
                this.xa_info = (XAINFO) Marshal.PtrToStructure(zero, typeof(XAINFO));
                Marshal.FreeCoTaskMem(zero);
                result = Offset + length;
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        internal int SetXaInfo(string xaOpenString)
        {
            uint method = 0x500;
            this.TrEntry(method, new object[] { xaOpenString });
            int index = 0;
            try
            {
                string s = this.CovertXAStringToBytes(xaOpenString);
                if ((s == null) || (s.ToCharArray()[0] == '*'))
                {
                    return -5;
                }
                byte[] bytes = Encoding.ASCII.GetBytes(s);
                Buffer.BlockCopy(bytes, 0, this.xa_info.xa_info, 0, bytes.Length);
                this.xa_info.xa_info_length = (byte) bytes.Length;
            }
            finally
            {
                base.TrExit(method, index);
            }
            return index;
        }

        public int WriteStruct(byte[] b, int offset)
        {
            uint method = 0x502;
            this.TrEntry(method, new object[] { b, offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                Marshal.StructureToPtr(this.xa_info, zero, false);
                Marshal.Copy(zero, b, offset, length);
                Marshal.FreeCoTaskMem(zero);
            }
            finally
            {
                base.TrExit(method, length);
            }
            return length;
        }
    }
}

