namespace IBM.WMQ
{
    using System;

    internal class MQChannelEntry : MQBase
    {
        private MQChannelDefinition channel;
        private MQChannelEntry next;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        internal MQChannelEntry(MQChannelDefinition cd, MQChannelEntry chlEntry)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { cd, chlEntry });
            this.channel = cd;
            this.next = chlEntry;
        }

        public MQChannelDefinition Channel
        {
            get
            {
                return this.channel;
            }
            set
            {
                this.channel = value;
            }
        }

        public MQChannelEntry Next
        {
            get
            {
                return this.next;
            }
            set
            {
                this.next = value;
            }
        }
    }
}

