namespace IBM.WMQ.Nmqi
{
    using System;

    public class QueueManagerInfo : NmqiObject
    {
        private int ccsid;
        private int commandLevel;
        private string name;
        private int platform;
        public const string sccsid = "%Z% %W% %I% %E% %U%";
        private string uid;

        public QueueManagerInfo(NmqiEnvironment env) : base(env)
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%", new object[] { env });
        }

        public int Ccsid
        {
            get
            {
                return this.ccsid;
            }
            set
            {
                this.ccsid = value;
            }
        }

        public int CommandLevel
        {
            get
            {
                return this.commandLevel;
            }
            set
            {
                this.commandLevel = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public int Platform
        {
            get
            {
                return this.platform;
            }
            set
            {
                this.platform = value;
            }
        }

        public string Uid
        {
            get
            {
                return this.uid;
            }
            set
            {
                this.uid = value;
            }
        }
    }
}

