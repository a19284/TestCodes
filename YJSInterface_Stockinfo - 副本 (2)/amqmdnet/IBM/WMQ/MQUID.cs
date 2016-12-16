namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Principal;

    internal class MQUID : MQBase
    {
        private int fapLevel;
        private static readonly byte[] rfpUID_ID = new byte[] { 0x55, 0x49, 0x44, 0x20 };
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal structMQUID uid;

        internal MQUID(byte FapLevel)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { FapLevel });
            this.fapLevel = FapLevel;
            this.uid = new structMQUID();
            this.uid.Id = rfpUID_ID;
            this.uid.UserIdentifier = new byte[12];
            this.uid.Password = new byte[12];
            this.uid.LongUserId = new byte[0x40];
            this.uid.UserSecurityId = new byte[40];
        }

        internal int GetLength()
        {
            uint method = 0x2dd;
            this.TrEntry(method);
            if (this.fapLevel < 5)
            {
                base.TrExit(method, 0x1c, 1);
                return 0x1c;
            }
            int result = Marshal.SizeOf(this.uid);
            base.TrExit(method, result, 2);
            return result;
        }

        internal void SetCurrentUser()
        {
            uint method = 0x2de;
            this.TrEntry(method);
            string userName = Environment.UserName;
            string userDomainName = Environment.UserDomainName;
            try
            {
                WindowsIdentity current = WindowsIdentity.GetCurrent();
                string[] strArray = current.Name.Split(new char[] { '\\' });
                current.Dispose();
                if ((strArray.Length == 2) && (string.Compare(userName, strArray[1], true) == 0))
                {
                    userDomainName = strArray[0];
                }
            }
            catch
            {
            }
            finally
            {
                base.TrExit(method);
            }
            this.SetUser(userName, userDomainName);
        }

        internal void SetUser(string user, string domain)
        {
            uint method = 0x2df;
            this.TrEntry(method, new object[] { user, domain });
            byte[] userSID = CommonServices.GetUserSID(user, domain);
            base.GetBytesRightPad(user.ToUpper(), ref this.uid.UserIdentifier);
            base.GetBytesRightPad(user, ref this.uid.LongUserId);
            base.GetBytesRightPad(string.Empty, ref this.uid.Password);
            if ((userSID != null) && (userSID.Length <= 0x26))
            {
                this.uid.UserSecurityId[0] = Convert.ToByte((int) (userSID.Length + 1));
                this.uid.UserSecurityId[1] = 1;
                Buffer.BlockCopy(userSID, 0, this.uid.UserSecurityId, 2, userSID.Length);
            }
            base.TrExit(method);
        }

        internal int WriteStruct(byte[] b, int Offset)
        {
            uint method = 0x2e0;
            this.TrEntry(method, new object[] { b, Offset });
            IntPtr zero = IntPtr.Zero;
            int length = this.GetLength();
            try
            {
                zero = Marshal.AllocCoTaskMem(length);
                Marshal.StructureToPtr(this.uid, zero, false);
                Marshal.Copy(zero, b, Offset, length);
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

