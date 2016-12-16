namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public class MQXid : MQBase
    {
        private byte[] bqual;
        private static int BQUAL_LENGTH_OFFSET = (GTRID_LENGTH_OFFSET + 1);
        private static int DATA_OFFSET = (BQUAL_LENGTH_OFFSET + 1);
        private static int FORMAT_ID_OFFSET = 0;
        private Guid globalTransId;
        private byte[] gtrid;
        private static int GTRID_LENGTH_OFFSET = (FORMAT_ID_OFFSET + 4);
        private string localTransBranchId = string.Empty;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private Xid xid;

        public MQXid()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.xid = new Xid();
            this.xid.bqual_length = 0;
            this.xid.gtrid_length = 0;
            this.xid.data = new byte[0x80];
            this.xid.formatID = 12;
        }

        public override void AddFieldsToFormatter(NmqiStructureFormatter fmt)
        {
            base.AddFieldsToFormatter(fmt);
            fmt.Add("Format", this.xid.formatID);
            fmt.Add("DistributedTransIdLength", (int) this.xid.gtrid_length);
            fmt.Add("DistributedTransId", Encoding.Unicode.GetString(this.GlobalTransactionId.ToByteArray()));
            fmt.Add("LocalTransIdLength", (int) this.xid.bqual_length);
            fmt.Add("LocalTransId", this.LocalTransactionId);
        }

        public override bool Equals(object obj)
        {
            uint method = 0x50a;
            this.TrEntry(method, new object[] { obj });
            Xid xid = this.xid;
            Xid xid2 = ((MQXid) obj).xid;
            bool result = true;
            if (xid.formatID != xid2.formatID)
            {
                result = false;
                base.TrExit(method, result, 1);
                return result;
            }
            if (xid.gtrid_length != xid2.gtrid_length)
            {
                result = false;
                base.TrExit(method, result, 2);
                return result;
            }
            if (xid.bqual_length != xid2.bqual_length)
            {
                result = false;
                base.TrExit(method, result, 3);
                return result;
            }
            try
            {
                for (int i = 0; i < (xid.bqual_length + xid.gtrid_length); i++)
                {
                    if (xid.data[i] != xid2.data[i])
                    {
                        return false;
                    }
                }
                return result;
            }
            catch (Exception exception)
            {
                base.TrException(method, exception);
            }
            finally
            {
                base.TrExit(method, result, 4);
            }
            return result;
        }

        public void GenerateGtrid(ref Guid Gtrid, int val)
        {
            uint method = 0x508;
            this.TrEntry(method, new object[] { (Guid) Gtrid, val });
            Random random = new Random(val);
            byte[] buffer = new byte[8];
            random.NextBytes(buffer);
            Gtrid = new Guid(val, (short) random.Next(), (short) random.Next(), buffer);
            base.TrExit(method);
        }

        public int GetRoundedLength()
        {
            int num = this.gtrid.Length & 0xff;
            int num2 = this.bqual.Length & 0xff;
            int num3 = num + num2;
            num3 += 4;
            num3 += 2;
            num3 += 3;
            num3 &= 0xffffffc;
            base.TrText("Xid roundedoff length = " + num3);
            return num3;
        }

        public int ReadBytes(byte[] b, int Offset)
        {
            uint method = 0x504;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int roundedLength = this.GetRoundedLength();
            int result = 0;
            try
            {
                if (zero == IntPtr.Zero)
                {
                    zero = Marshal.AllocCoTaskMem(roundedLength);
                }
                if (roundedLength > (b.Length - Offset))
                {
                    roundedLength = b.Length - Offset;
                }
                Marshal.Copy(b, Offset, zero, roundedLength);
                this.xid = (Xid) Marshal.PtrToStructure(zero, typeof(Xid));
                Marshal.FreeCoTaskMem(zero);
                result = Offset + roundedLength;
                byte[] destinationArray = new byte[this.xid.gtrid_length];
                Array.Copy(this.xid.data, destinationArray, (int) this.xid.gtrid_length);
                this.globalTransId = new Guid(destinationArray);
                destinationArray = new byte[this.xid.bqual_length];
                Array.Copy(this.xid.data, this.xid.gtrid_length, destinationArray, 0, this.xid.bqual_length);
                this.localTransBranchId = Encoding.ASCII.GetString(destinationArray);
                destinationArray = null;
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        public int ReadXidFromBytes(byte[] buffer, int offset)
        {
            uint method = 0x507;
            this.TrEntry(method, new object[] { buffer, offset });
            int result = 0;
            try
            {
                this.xid.formatID = BitConverter.ToInt32(buffer, offset + FORMAT_ID_OFFSET);
                this.xid.gtrid_length = buffer[offset + GTRID_LENGTH_OFFSET];
                this.xid.bqual_length = buffer[offset + BQUAL_LENGTH_OFFSET];
                byte[] destinationArray = new byte[this.xid.gtrid_length];
                Array.Copy(buffer, offset + DATA_OFFSET, destinationArray, 0, this.xid.gtrid_length);
                Array.Copy(destinationArray, 0, this.xid.data, 0, this.xid.gtrid_length);
                this.globalTransId = new Guid(destinationArray);
                this.gtrid = new byte[destinationArray.Length];
                Buffer.BlockCopy(destinationArray, 0, this.gtrid, 0, destinationArray.Length);
                destinationArray = new byte[this.xid.bqual_length];
                Array.Copy(buffer, (offset + DATA_OFFSET) + this.xid.gtrid_length, destinationArray, 0, this.xid.bqual_length);
                Array.Copy(destinationArray, 0, this.xid.data, this.xid.gtrid_length, this.xid.bqual_length);
                this.localTransBranchId = Encoding.ASCII.GetString(destinationArray);
                this.bqual = new byte[destinationArray.Length];
                Buffer.BlockCopy(destinationArray, 0, this.bqual, 0, destinationArray.Length);
                destinationArray = null;
                int num3 = buffer[offset + GTRID_LENGTH_OFFSET] & 0xff;
                int num4 = buffer[offset + BQUAL_LENGTH_OFFSET] & 0xff;
                int num5 = num3 + num4;
                num5 += 4;
                num5 += 2;
                num5 += 3;
                num5 &= 0xffffffc;
                result = offset + num5;
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        public void SetXid(Guid Gtrid, string Btrid)
        {
            uint method = 0x505;
            this.TrEntry(method, new object[] { Gtrid, Btrid });
            this.globalTransId = Gtrid;
            this.localTransBranchId = Btrid;
            this.gtrid = this.globalTransId.ToByteArray();
            this.bqual = Encoding.ASCII.GetBytes(this.localTransBranchId);
            base.TrExit(method);
        }

        public override string ToString()
        {
            uint method = 0x509;
            this.TrEntry(method);
            StringBuilder builder = new StringBuilder();
            if (this.gtrid != null)
            {
                builder.Append(Encoding.ASCII.GetString(this.gtrid, 0, this.gtrid.Length));
            }
            if (this.bqual != null)
            {
                builder.Append(Encoding.ASCII.GetString(this.bqual, 0, this.bqual.Length));
            }
            string result = builder.ToString();
            base.TrExit(method, result);
            return result;
        }

        public int WriteStruct(byte[] buffer, int offset)
        {
            uint method = 0x506;
            this.TrEntry(method, new object[] { buffer, offset });
            int result = 0;
            try
            {
                int num3 = this.gtrid.Length & 0xff;
                int num4 = this.bqual.Length & 0xff;
                Array.Copy(BitConverter.GetBytes(this.xid.formatID), 0, buffer, offset, 4);
                buffer[offset + GTRID_LENGTH_OFFSET] = (byte) this.gtrid.Length;
                buffer[offset + BQUAL_LENGTH_OFFSET] = (byte) this.bqual.Length;
                Array.Copy(this.gtrid, 0, buffer, offset + DATA_OFFSET, this.gtrid.Length);
                Array.Copy(this.bqual, 0, buffer, (offset + DATA_OFFSET) + this.gtrid.Length, this.bqual.Length);
                result = num3 + num4;
                result += 4;
                result += 2;
                result += 3;
                result &= 0xffffffc;
            }
            finally
            {
                base.TrExit(method, result);
            }
            return result;
        }

        public Guid GlobalTransactionId
        {
            get
            {
                return this.globalTransId;
            }
        }

        public string LocalTransactionId
        {
            get
            {
                return this.localTransBranchId;
            }
        }
    }
}

