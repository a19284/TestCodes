namespace IBM.WMQ
{
    using System;
    using System.Collections;
    using System.Text;

    internal class RFH2 : MQBase, IDisposable
    {
        internal ArrayList folders;
        private const uint MQRFH_ASCII_STRUC_ID_INT = 0x52464820;
        private const uint MQRFH_EBCDIC_STRUC_ID_INT = 0xd9c6c840;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private byte[] SPACES = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

        internal RFH2()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.folders = new ArrayList();
        }

        internal void AddFolder(RFH2Folder folder)
        {
            uint method = 0x2ef;
            this.TrEntry(method, new object[] { folder });
            try
            {
                int length = folder.Render().Length;
                this.folders.Add(folder);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal bool Contains(string foldername)
        {
            uint method = 0x2f1;
            this.TrEntry(method, new object[] { foldername });
            bool flag = false;
            try
            {
                foreach (RFH2Folder folder in this.folders)
                {
                    if (folder.name == foldername)
                    {
                        flag = true;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return flag;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public void Dispose(bool fromDispose)
        {
            this.folders = null;
            if (!fromDispose)
            {
                GC.SuppressFinalize(this);
            }
        }

        internal RFH2Folder GetFolder(string foldername)
        {
            uint method = 0x2f0;
            this.TrEntry(method, new object[] { foldername });
            RFH2Folder folder = null;
            try
            {
                foreach (RFH2Folder folder2 in this.folders)
                {
                    if (folder2.name == foldername)
                    {
                        return folder2;
                    }
                }
                return folder;
            }
            finally
            {
                base.TrExit(method);
            }
            return folder;
        }

        internal static void InsertIntIntoByteArray(int i, byte[] array, int off, int encoding)
        {
            byte[] bytes;
            switch ((encoding & 15))
            {
                case 0:
                case 2:
                    bytes = BitConverter.GetBytes(i);
                    array[off] = bytes[0];
                    array[off + 1] = bytes[1];
                    array[off + 2] = bytes[2];
                    array[off + 3] = bytes[3];
                    return;

                case 1:
                    bytes = BitConverter.GetBytes(i);
                    array[off] = bytes[3];
                    array[off + 1] = bytes[2];
                    array[off + 2] = bytes[1];
                    array[off + 3] = bytes[0];
                    return;
            }
        }

        internal static void InsertStrIntoByteArray(string src, int dstLen, byte[] dst, int offset, int ccsid, int enc)
        {
            try
            {
                Encoding dotnetEncoding = MQCcsidTable.GetDotnetEncoding(ccsid);
                if (dotnetEncoding == null)
                {
                    dotnetEncoding = Encoding.UTF8;
                }
                int length = 0;
                if (src != null)
                {
                    byte[] bytes = dotnetEncoding.GetBytes(src);
                    Buffer.BlockCopy(bytes, 0, dst, offset, bytes.Length);
                    length = bytes.Length;
                }
                else
                {
                    length = 0;
                }
                if (dstLen > length)
                {
                    byte[] buffer2 = dotnetEncoding.GetBytes(" ");
                    for (int i = length; i < dstLen; i++)
                    {
                        dst[offset + i] = buffer2[0];
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
    }
}

