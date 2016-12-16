namespace IBM.WMQ
{
    using System;
    using System.Net;

    public class MQTSH : MQBase
    {
        public static readonly byte[] BLANK_LUWID = new byte[8];
        internal int Length;
        internal bool multiplexing;
        internal int Offset;
        private IMQCommsBuffer parentBuffer;
        public static readonly byte[] rfpTSH_C_ID = new byte[] { 0x54, 0x53, 0x48, 0x43 };
        public static readonly byte[] rfpTSH_C_ID_EBCDIC = new byte[] { 0xe3, 0xe2, 200, 0xc3 };
        public static readonly byte[] rfpTSH_ID = new byte[] { 0x54, 0x53, 0x48, 0x20 };
        public static readonly byte[] rfpTSH_ID_EBCDIC = new byte[] { 0xe3, 0xe2, 200, 0x40 };
        public static readonly byte[] rfpTSH_M_ID = new byte[] { 0x54, 0x53, 0x48, 0x4d };
        public static readonly byte[] rfpTSH_M_ID_EBCDIC = new byte[] { 0xe3, 0xe2, 200, 0xd4 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private structMQTSH tsh;
        internal const int TSH = 0;
        private byte[] tshBuffer;
        internal const int TSHC = 2;
        internal const int tshlen = 0x1c;
        private structMQTSHM tshM;
        internal const int TSHM = 1;
        internal const int tshMlen = 0x24;

        internal MQTSH(bool IsMultiplexingEnabled, IMQCommsBuffer buffer)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { IsMultiplexingEnabled });
            this.multiplexing = IsMultiplexingEnabled;
            if (!this.multiplexing)
            {
                this.tsh = new structMQTSH();
                this.tsh._Id = new byte[4];
                Buffer.BlockCopy(rfpTSH_ID, 0, this.tsh._Id, 0, 4);
                this.tsh._LUWID = BLANK_LUWID;
            }
            else
            {
                this.tshM = new structMQTSHM();
                this.tshM._Id = new byte[4];
                Buffer.BlockCopy(rfpTSH_M_ID, 0, this.tshM._Id, 0, 4);
                this.tshM._LUWID = BLANK_LUWID;
            }
            if (buffer != null)
            {
                this.parentBuffer = buffer;
                this.tshBuffer = this.parentBuffer.Buffer;
                this.Offset = this.parentBuffer.DataPosition;
                this.Length = 0;
            }
        }

        internal MQTSH(int tshType, IMQCommsBuffer buffer)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.multiplexing = false;
            if (!this.multiplexing)
            {
                this.tsh = new structMQTSH();
                this.tsh._Id = new byte[4];
                this.tsh._LUWID = BLANK_LUWID;
                if (tshType == 2)
                {
                    Buffer.BlockCopy(rfpTSH_C_ID, 0, this.tsh._Id, 0, 4);
                }
                else
                {
                    this.tsh._Id = rfpTSH_ID;
                    Buffer.BlockCopy(rfpTSH_ID, 0, this.tsh._Id, 0, 4);
                }
            }
            else
            {
                this.tshM = new structMQTSHM();
                this.tshM._Id = new byte[4];
                Buffer.BlockCopy(rfpTSH_M_ID, 0, this.tshM._Id, 0, 4);
                this.tshM._LUWID = BLANK_LUWID;
            }
            if (buffer != null)
            {
                this.parentBuffer = buffer;
                this.tshBuffer = this.parentBuffer.Buffer;
                this.Offset = this.parentBuffer.DataPosition;
                this.Length = 0;
            }
        }

        internal MQTSH(int tshType, IMQCommsBuffer buffer, bool isMultiplexing)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.multiplexing = isMultiplexing;
            if (!this.multiplexing)
            {
                this.tsh = new structMQTSH();
                this.tsh._Id = new byte[4];
                this.tsh._LUWID = BLANK_LUWID;
                if (tshType == 2)
                {
                    Buffer.BlockCopy(rfpTSH_C_ID, 0, this.tsh._Id, 0, 4);
                }
                else
                {
                    this.tsh._Id = rfpTSH_ID;
                    Buffer.BlockCopy(rfpTSH_ID, 0, this.tsh._Id, 0, 4);
                }
            }
            else if (tshType == 2)
            {
                this.tsh = new structMQTSH();
                this.tsh._Id = new byte[4];
                this.tsh._LUWID = BLANK_LUWID;
                Buffer.BlockCopy(rfpTSH_C_ID, 0, this.tsh._Id, 0, 4);
            }
            else
            {
                this.tshM = new structMQTSHM();
                this.tshM._Id = new byte[4];
                Buffer.BlockCopy(rfpTSH_M_ID, 0, this.tshM._Id, 0, 4);
                this.tshM._LUWID = BLANK_LUWID;
            }
            if (buffer != null)
            {
                this.parentBuffer = buffer;
                this.tshBuffer = this.parentBuffer.Buffer;
                this.Offset = this.parentBuffer.DataPosition;
                this.Length = 0;
            }
        }

        internal bool CheckTSH(byte[] obtainedTSH)
        {
            uint method = 0x4ee;
            this.TrEntry(method, new object[] { obtainedTSH });
            bool result = false;
            if (((obtainedTSH[0] == rfpTSH_ID[0]) && (obtainedTSH[1] == rfpTSH_ID[1])) && (obtainedTSH[2] == rfpTSH_ID[2]))
            {
                result = true;
            }
            else if (((obtainedTSH[0] == rfpTSH_ID_EBCDIC[0]) && (obtainedTSH[1] == rfpTSH_ID_EBCDIC[1])) && (obtainedTSH[2] == rfpTSH_ID_EBCDIC[2]))
            {
                result = true;
            }
            else
            {
                result = false;
            }
            base.TrExit(method, result);
            return result;
        }

        internal int GetLength()
        {
            uint method = 730;
            this.TrEntry(method);
            int result = 0;
            if (!this.multiplexing)
            {
                result = 0x1c;
            }
            else
            {
                result = 0x24;
            }
            base.TrExit(method, result);
            return result;
        }

        internal static int GetTshType(int differentiator)
        {
            if ((differentiator == rfpTSH_M_ID[3]) || (differentiator == rfpTSH_M_ID_EBCDIC[3]))
            {
                return 1;
            }
            if ((differentiator != rfpTSH_C_ID[3]) && (differentiator != rfpTSH_C_ID_EBCDIC[3]))
            {
                return -1;
            }
            return 2;
        }

        internal void Initialize(int transLength, byte segmentType, byte controlFlags1, ushort ccsid)
        {
            uint method = 0x2d9;
            this.TrEntry(method, new object[] { transLength, segmentType, controlFlags1, ccsid });
            if (this.multiplexing)
            {
                Buffer.BlockCopy(rfpTSH_M_ID, 0, this.tshM._Id, 0, 4);
                this.SetTransLength(transLength);
                this.tshM._ConversationId = 0;
                this.tshM._RequestId = 0;
                this.tshM._Encoding = 2;
                this.tshM._SegmentType = segmentType;
                this.tshM._ControlFlags1 = controlFlags1;
                this.tshM._ControlFlags2 = 0;
                this.tshM._LUWID = BLANK_LUWID;
                this.tshM._MQEncoding = 0x222;
                this.tshM._Ccsid = ccsid;
                this.tshM._Reserved = 0;
            }
            else
            {
                Buffer.BlockCopy(rfpTSH_ID, 0, this.tsh._Id, 0, 4);
                this.SetTransLength(transLength);
                this.tsh._Encoding = 2;
                this.tsh._SegmentType = segmentType;
                this.tsh._ControlFlags1 = controlFlags1;
                this.tsh._ControlFlags2 = 0;
                this.tsh._LUWID = BLANK_LUWID;
                this.tsh._MQEncoding = 0x222;
                this.tsh._Ccsid = ccsid;
                this.tsh._Reserved = 0;
            }
            base.TrExit(method);
        }

        internal int ReadStruct(byte[] b, int Offset)
        {
            uint method = 0x2dc;
            this.TrEntry(method, new object[] { b, Offset });
            if (!this.multiplexing || (this.TSHType == 2))
            {
                this.multiplexing = false;
                Buffer.BlockCopy(b, Offset, this.tsh._Id, 0, 4);
                Offset += 4;
                this.tsh._TransLength = BitConverter.ToUInt32(b, Offset);
                Offset += 4;
                this.Length = this.TransLength;
                this.tsh._Encoding = b[Offset];
                Offset++;
                this.tsh._SegmentType = b[Offset];
                Offset++;
                this.tsh._ControlFlags1 = b[Offset];
                Offset++;
                this.tsh._ControlFlags2 = b[Offset];
                Offset++;
                Buffer.BlockCopy(b, Offset, this.tsh._LUWID, 0, 8);
                Offset += 8;
                this.tsh._MQEncoding = BitConverter.ToInt32(b, Offset);
                Offset += 4;
                this.tsh._Ccsid = BitConverter.ToUInt16(b, Offset);
                Offset += 2;
                this.tsh._Reserved = BitConverter.ToUInt16(b, Offset);
                Offset += 2;
                base.TrExit(method, 0x1c, 1);
                return 0x1c;
            }
            Buffer.BlockCopy(b, Offset, this.tshM._Id, 0, 4);
            Offset += 4;
            this.tshM._TransLength = BitConverter.ToUInt32(b, Offset);
            Offset += 4;
            this.Length = this.TransLength;
            this.tshM._ConversationId = BitConverter.ToInt32(b, Offset);
            Offset += 4;
            this.tshM._RequestId = BitConverter.ToInt32(b, Offset);
            Offset += 4;
            this.tshM._Encoding = b[Offset];
            Offset++;
            this.tshM._SegmentType = b[Offset];
            Offset++;
            this.tshM._ControlFlags1 = b[Offset];
            Offset++;
            this.tshM._ControlFlags2 = b[Offset];
            Offset++;
            Buffer.BlockCopy(b, Offset, this.tshM._LUWID, 0, 8);
            Offset += 8;
            this.tshM._MQEncoding = BitConverter.ToInt32(b, Offset);
            Offset += 4;
            this.tshM._Ccsid = BitConverter.ToUInt16(b, Offset);
            Offset += 2;
            this.tshM._Reserved = BitConverter.ToUInt16(b, Offset);
            Offset += 2;
            base.TrExit(method, 0x24, 3);
            return 0x24;
        }

        internal void SetConversationId(int convId)
        {
            this.tshM._ConversationId = IPAddress.NetworkToHostOrder(convId);
        }

        internal void SetRequestId(int reqId)
        {
            this.tshM._RequestId = IPAddress.NetworkToHostOrder(reqId);
        }

        internal void SetTransLength(int translen)
        {
            if (this.multiplexing)
            {
                if ((translen % 4) != 0)
                {
                    base.TrText(0x2d9, "Tsh not padded to a 4-byte boundary:" + translen);
                }
                this.tshM._TransLength = (uint) IPAddress.NetworkToHostOrder(translen);
            }
            else
            {
                this.tsh._TransLength = (uint) IPAddress.NetworkToHostOrder(translen);
            }
        }

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x2db;
            this.TrEntry(method, new object[] { b, Offset });
            if (!this.multiplexing)
            {
                Buffer.BlockCopy(this.tsh._Id, 0, b, Offset, 4);
                Offset += 4;
                Buffer.BlockCopy(BitConverter.GetBytes(this.tsh._TransLength), 0, b, Offset, 4);
                Offset += 4;
                b[Offset] = this.tsh._Encoding;
                Offset++;
                b[Offset] = this.tsh._SegmentType;
                Offset++;
                b[Offset] = this.tsh._ControlFlags1;
                Offset++;
                b[Offset] = this.tsh._ControlFlags2;
                Offset++;
                Buffer.BlockCopy(this.tsh._LUWID, 0, b, Offset, 8);
                Offset += 8;
                Buffer.BlockCopy(BitConverter.GetBytes(this.tsh._MQEncoding), 0, b, Offset, 4);
                Offset += 4;
                Buffer.BlockCopy(BitConverter.GetBytes(this.tsh._Ccsid), 0, b, Offset, 2);
                Offset += 2;
                Buffer.BlockCopy(BitConverter.GetBytes(this.tsh._Reserved), 0, b, Offset, 2);
                Offset += 2;
                this.Length = this.TransLength;
                base.TrText(method, 0x1c + "bytes written");
                base.TrExit(method, 0x1c, 1);
                return 0x1c;
            }
            Buffer.BlockCopy(this.tshM._Id, 0, b, Offset, 4);
            Offset += 4;
            Buffer.BlockCopy(BitConverter.GetBytes(this.tshM._TransLength), 0, b, Offset, 4);
            Offset += 4;
            Buffer.BlockCopy(BitConverter.GetBytes(this.tshM._ConversationId), 0, b, Offset, 4);
            Offset += 4;
            Buffer.BlockCopy(BitConverter.GetBytes(this.tshM._RequestId), 0, b, Offset, 4);
            Offset += 4;
            b[Offset] = this.tshM._Encoding;
            Offset++;
            b[Offset] = this.tshM._SegmentType;
            Offset++;
            b[Offset] = this.tshM._ControlFlags1;
            Offset++;
            b[Offset] = this.tshM._ControlFlags2;
            Offset++;
            Buffer.BlockCopy(this.tshM._LUWID, 0, b, Offset, 8);
            Offset += 8;
            Buffer.BlockCopy(BitConverter.GetBytes(this.tshM._MQEncoding), 0, b, Offset, 4);
            Offset += 4;
            Buffer.BlockCopy(BitConverter.GetBytes(this.tshM._Ccsid), 0, b, Offset, 2);
            Offset += 2;
            Buffer.BlockCopy(BitConverter.GetBytes(this.tshM._Reserved), 0, b, Offset, 2);
            Offset += 2;
            base.TrText(method, 0x24 + "bytes written");
            this.Length = this.TransLength;
            base.TrExit(method, 0x24, 2);
            return 0x24;
        }

        internal ushort Ccsid
        {
            get
            {
                if (!this.multiplexing)
                {
                    return this.tsh._Ccsid;
                }
                return this.tshM._Ccsid;
            }
            set
            {
                if (!this.multiplexing)
                {
                    this.tsh._Ccsid = value;
                }
                else
                {
                    this.tshM._Ccsid = value;
                }
            }
        }

        internal byte ControlFlags1
        {
            get
            {
                if (!this.multiplexing)
                {
                    return this.tsh._ControlFlags1;
                }
                return this.tshM._ControlFlags1;
            }
            set
            {
                if (!this.multiplexing)
                {
                    this.tsh._ControlFlags1 = value;
                }
                else
                {
                    this.tshM._ControlFlags1 = value;
                }
            }
        }

        internal byte ControlFlags2
        {
            get
            {
                if (this.multiplexing)
                {
                    return this.tsh._ControlFlags2;
                }
                return this.tshM._ControlFlags2;
            }
            set
            {
                if (this.multiplexing)
                {
                    this.tsh._ControlFlags2 = value;
                }
                else
                {
                    this.tshM._ControlFlags2 = value;
                }
            }
        }

        internal int ConversationID
        {
            get
            {
                if (!this.multiplexing)
                {
                    return -1;
                }
                return IPAddress.HostToNetworkOrder(this.tshM._ConversationId);
            }
        }

        internal byte Encoding
        {
            get
            {
                if (!this.multiplexing)
                {
                    return this.tsh._Encoding;
                }
                return this.tshM._Encoding;
            }
            set
            {
                if (!this.multiplexing)
                {
                    this.tsh._Encoding = value;
                }
                else
                {
                    this.tshM._Encoding = value;
                }
            }
        }

        internal byte[] Id
        {
            get
            {
                if (this.multiplexing && (!this.multiplexing || (this.tshM._Id != null)))
                {
                    return this.tshM._Id;
                }
                return this.tsh._Id;
            }
            set
            {
                if (!this.multiplexing)
                {
                    this.tsh._Id = value;
                }
                else if (this.multiplexing && (this.tshM._Id == null))
                {
                    this.tsh._Id = value;
                }
                else
                {
                    this.tshM._Id = value;
                }
            }
        }

        internal byte[] LUWID
        {
            get
            {
                if (!this.multiplexing)
                {
                    return this.tsh._LUWID;
                }
                return this.tshM._LUWID;
            }
            set
            {
                if (!this.multiplexing)
                {
                    this.tsh._LUWID = value;
                }
                else
                {
                    this.tsh._LUWID = value;
                }
            }
        }

        internal int MQEncoding
        {
            get
            {
                if (!this.multiplexing)
                {
                    return this.tsh._MQEncoding;
                }
                return this.tshM._MQEncoding;
            }
            set
            {
                if (!this.multiplexing)
                {
                    this.tsh._MQEncoding = value;
                }
                else
                {
                    this.tshM._MQEncoding = value;
                }
            }
        }

        internal IMQCommsBuffer ParentBuffer
        {
            get
            {
                return this.parentBuffer;
            }
            set
            {
                this.parentBuffer = value;
                if (this.parentBuffer != null)
                {
                    this.tshBuffer = this.parentBuffer.Buffer;
                }
            }
        }

        internal int RequestID
        {
            get
            {
                if (!this.multiplexing)
                {
                    return -1;
                }
                return IPAddress.HostToNetworkOrder(this.tshM._RequestId);
            }
        }

        internal ushort Reserved
        {
            get
            {
                if (!this.multiplexing)
                {
                    return this.tsh._Reserved;
                }
                return this.tshM._Reserved;
            }
            set
            {
                if (!this.multiplexing)
                {
                    this.tsh._Reserved = value;
                }
                else
                {
                    this.tshM._Reserved = value;
                }
            }
        }

        internal byte SegmentType
        {
            get
            {
                if (!this.multiplexing)
                {
                    return this.tsh._SegmentType;
                }
                return this.tshM._SegmentType;
            }
            set
            {
                if (!this.multiplexing)
                {
                    this.tsh._SegmentType = value;
                }
                else
                {
                    this.tshM._SegmentType = value;
                }
            }
        }

        internal int TransLength
        {
            get
            {
                if (!this.multiplexing)
                {
                    return IPAddress.HostToNetworkOrder((int) this.tsh._TransLength);
                }
                return IPAddress.HostToNetworkOrder((int) this.tshM._TransLength);
            }
            set
            {
                if (!this.multiplexing)
                {
                    this.tsh._TransLength = (uint) IPAddress.NetworkToHostOrder(value);
                }
                else
                {
                    this.tshM._TransLength = (uint) IPAddress.NetworkToHostOrder(value);
                }
            }
        }

        internal byte[] TshBuffer
        {
            get
            {
                return this.tshBuffer;
            }
            set
            {
                this.tshBuffer = value;
            }
        }

        internal int TSHType
        {
            get
            {
                if ((this.Id[3] == rfpTSH_M_ID[3]) || (this.Id[3] == rfpTSH_M_ID_EBCDIC[3]))
                {
                    return 1;
                }
                if ((this.Id[3] == rfpTSH_C_ID[3]) || (this.Id[3] == rfpTSH_C_ID_EBCDIC[3]))
                {
                    return 2;
                }
                if ((this.Id[3] != rfpTSH_ID[3]) && (this.Id[3] != rfpTSH_ID_EBCDIC[3]))
                {
                    return -1;
                }
                return 0;
            }
        }
    }
}

