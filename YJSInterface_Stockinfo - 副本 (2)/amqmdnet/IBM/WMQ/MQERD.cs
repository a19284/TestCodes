namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    internal class MQERD : MQBase
    {
        internal structMQERD erd;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        internal MQERD()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.erd = new structMQERD();
            this.erd.ErrorDataLength = 8;
        }

        internal MQManagedClientException ErrorException(string ChannelName)
        {
            uint method = 0x83;
            this.TrEntry(method, new object[] { ChannelName });
            MQManagedClientException result = null;
            try
            {
                result = ErrorException(this.erd.ReturnCode, this.erd.ErrorData, ChannelName);
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        internal static MQManagedClientException ErrorException(uint ReturnCode, uint ErrorData, string ChannelName)
        {
            return new MQManagedClientException(GetRemoteReturnCode(ReturnCode), ReturnCode, ErrorData, ChannelName, null, null, 2, 0x893);
        }

        internal int GetLength()
        {
            return (int) this.erd.ErrorDataLength;
        }

        internal int GetReasonCode()
        {
            int num2;
            uint method = 0x3bb;
            this.TrEntry(method);
            switch (GetRemoteReturnCode(this.erd.ReturnCode))
            {
                case 0x20009202:
                case 0x20009204:
                case 0x20009206:
                case 0x20009208:
                case 0x20009209:
                case 0x20009213:
                case 0x20009271:
                    num2 = 0x9ea;
                    break;

                case 0x20009203:
                case 0x20009181:
                case 0x20009182:
                case 0x20009184:
                case 0x20009185:
                case 0x20009186:
                case 0x20009187:
                case 0x20009188:
                case 0x20009189:
                case 0x20009190:
                case 0x20009195:
                case 0x20009197:
                case 0x20009270:
                case 0x20009503:
                case 0x20009504:
                case 0x20009523:
                case 0x20009541:
                case 0x20009547:
                    num2 = 0x9eb;
                    break;

                case 0x20009233:
                    num2 = 0x836;
                    break;

                case 0x20009496:
                case 0x20009536:
                case 0x20009539:
                case 0x20009573:
                case 0x20009558:
                    num2 = 0x9e9;
                    break;

                case 0x20009520:
                    num2 = 0x9ec;
                    break;

                case 0x20009641:
                case 0x20009642:
                case 0x20009643:
                    num2 = 0x959;
                    break;

                default:
                    num2 = 0x80b;
                    break;
            }
            base.TrExit(method, num2);
            return num2;
        }

        internal static uint GetRemoteReturnCode(uint ReturnCode)
        {
            switch (ReturnCode)
            {
                case 1:
                    return 0x20009520;

                case 2:
                    return 0x20009547;

                case 3:
                    return 0x20009524;

                case 4:
                    return 0x20009526;

                case 5:
                    return 0x20009525;

                case 6:
                case 9:
                    return 0x20009527;

                case 7:
                    return 0x10009528;

                case 8:
                    return 0x9545;

                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                    return 0x20009523;

                case 0x15:
                    return 0x20009505;

                case 0x16:
                    return 0x20009558;

                case 0x17:
                    return 0x20009496;

                case 0x18:
                    return 0x20009641;

                case 0x19:
                    return 0x20009643;

                case 0x1a:
                    return 0x20009642;

                case 0x1b:
                    return 0x20009608;

                case 0x1c:
                    return 0x20009714;
            }
            return 0x20009999;
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 130;
            this.TrEntry(method, new object[] { b, Offset });
            this.erd.ErrorDataLength = BitConverter.ToUInt32(b, Offset);
            Offset += 4;
            this.erd.ReturnCode = BitConverter.ToUInt32(b, Offset);
            Offset += 4;
            if (this.erd.ErrorDataLength > 8)
            {
                this.erd.ErrorData = BitConverter.ToUInt32(b, Offset);
                Offset += 4;
            }
            int length = this.GetLength();
            base.TrExit(method, length);
            return length;
        }

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x81;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int result = 0;
            try
            {
                zero = Marshal.AllocCoTaskMem(Marshal.SizeOf(this.erd));
                Marshal.StructureToPtr(this.erd, zero, false);
                Marshal.Copy(zero, b, Offset, this.GetLength());
                Marshal.FreeCoTaskMem(zero);
                result = this.GetLength();
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }
    }
}

