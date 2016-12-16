namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;

    public class HconnAdapter : MQBase, Hconn
    {
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private int value;

        public HconnAdapter()
        {
            this.value = -1;
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.value = -1;
        }

        public HconnAdapter(int initHconnValue)
        {
            this.value = -1;
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { initHconnValue });
            this.value = initHconnValue;
        }

        public int Ccsid
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public int CmdLevel
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public byte[] ConnectionId
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public int FapLevel
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public string Name
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public int Platform
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public int SharingConversations
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public string Uid
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public int Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }
    }
}

