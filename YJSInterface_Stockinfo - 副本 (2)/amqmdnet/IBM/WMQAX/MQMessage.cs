namespace IBM.WMQAX
{
    using IBM.WMQ;
    using System;
    using System.Text;

    public class MQMessage : IBM.WMQ.MQMessage
    {
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        private byte ConvertHexToByte(byte[] HexChar)
        {
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < 2; i++)
            {
                switch (((char) HexChar[i]))
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        num = HexChar[i] - 0x30;
                        break;

                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                        num = (HexChar[i] - 0x41) + 10;
                        break;

                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                        num = (HexChar[i] - 0x61) + 10;
                        break;

                    default:
                        throw new Exception("Invalid Hex Value");
                }
                if (i == 0)
                {
                    num2 = num * 0x10;
                }
                else
                {
                    num2 += num;
                }
            }
            return Convert.ToByte(num2);
        }

        public short ReadByte()
        {
            return base.binaryReader.ReadByte();
        }

        public int ReadUint2()
        {
            return base.binaryReader.ReadUInt16();
        }

        public void WriteShort(short value)
        {
            base.binaryWriter.Write(value);
        }

        public string AccountingTokenHex
        {
            get
            {
                int length = base.md.StructMQMD.AccountingToken.Length;
                StringBuilder builder = new StringBuilder(length * 2);
                for (int i = 0; i < length; i++)
                {
                    builder.AppendFormat("{0,2:X}", base.md.StructMQMD.AccountingToken[i]);
                }
                return builder.ToString();
            }
            set
            {
                int length = base.md.StructMQMD.AccountingToken.Length;
                byte[] bytes = new byte[2];
                for (int i = 0; i < length; i++)
                {
                    System.Text.Encoding.ASCII.GetBytes(value, i * 2, 2, bytes, 0);
                    base.md.StructMQMD.AccountingToken[i] = this.ConvertHexToByte(bytes);
                }
            }
        }

        public string CorrelationIdHex
        {
            get
            {
                int length = base.md.StructMQMD.CorrelId.Length;
                StringBuilder builder = new StringBuilder(length * 2);
                for (int i = 0; i < length; i++)
                {
                    builder.AppendFormat("{0,2:X}", base.md.StructMQMD.CorrelId[i]);
                }
                return builder.ToString();
            }
            set
            {
                int length = base.md.StructMQMD.CorrelId.Length;
                byte[] bytes = new byte[2];
                for (int i = 0; i < length; i++)
                {
                    System.Text.Encoding.ASCII.GetBytes(value, i * 2, 2, bytes, 0);
                    base.md.StructMQMD.CorrelId[i] = this.ConvertHexToByte(bytes);
                }
            }
        }

        public string CorrelationIdString
        {
            get
            {
                return base.GetString(base.md.StructMQMD.CorrelId);
            }
            set
            {
                byte[] correlId = base.md.CorrelId;
                base.GetBytes(value, ref correlId);
                base.md.CorrelId = correlId;
            }
        }

        public string MessageData
        {
            get
            {
                return base.ReadString(base.MessageLength);
            }
            set
            {
                base.ClearMessage();
                base.WriteString(value);
            }
        }

        public string MessageIdHex
        {
            get
            {
                int length = base.md.StructMQMD.MsgId.Length;
                StringBuilder builder = new StringBuilder(length * 2);
                for (int i = 0; i < length; i++)
                {
                    builder.AppendFormat("{0,2:X}", base.md.StructMQMD.MsgId[i]);
                }
                return builder.ToString();
            }
            set
            {
                int length = base.md.StructMQMD.MsgId.Length;
                byte[] bytes = new byte[2];
                for (int i = 0; i < length; i++)
                {
                    System.Text.Encoding.ASCII.GetBytes(value, i * 2, 2, bytes, 0);
                    base.md.StructMQMD.MsgId[i] = this.ConvertHexToByte(bytes);
                }
            }
        }

        public string MessageIdString
        {
            get
            {
                return base.GetString(base.md.StructMQMD.MsgId);
            }
            set
            {
                byte[] msgId = base.md.MsgId;
                base.GetBytes(value, ref msgId);
                base.md.MsgId = msgId;
            }
        }
    }
}

