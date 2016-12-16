namespace IBM.WMQ
{
    using System;

    internal class MQExternalSourceTracer : MQBase
    {
        public void TrData(uint fid, string caption, byte[] buffer)
        {
            base.TrData(fid, 0, caption, buffer);
        }

        public void TrEnter(uint fid)
        {
            this.TrEntry(fid);
        }

        public void TrException(string msg, string detailedMsg)
        {
            uint fid = 0x641;
            try
            {
                this.TrEnter(fid);
                Exception ex = new Exception(msg, (detailedMsg != null) ? new Exception(detailedMsg) : null);
                base.TrException(fid, ex);
            }
            finally
            {
                this.TrOut(fid);
            }
        }

        public void TrOut(uint fid)
        {
            base.TrExit(fid);
        }

        public void TrTextMsg(uint fid, string msg)
        {
            base.TrText(fid, msg);
        }
    }
}

