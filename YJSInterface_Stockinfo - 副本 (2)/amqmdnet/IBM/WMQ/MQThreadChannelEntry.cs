namespace IBM.WMQ
{
    using System;

    internal class MQThreadChannelEntry : MQBase
    {
        private MQChannelEntry firstWeightedEntry;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private int seed;
        private MQChannelEntry thisAlphaEntry;
        private MQChannelEntry thisWeightedEntry;

        internal MQThreadChannelEntry()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.seed = 0;
            this.thisAlphaEntry = null;
            this.firstWeightedEntry = null;
            this.thisWeightedEntry = null;
        }

        public MQChannelEntry FirstWeightedEntry
        {
            get
            {
                return this.firstWeightedEntry;
            }
            set
            {
                this.firstWeightedEntry = value;
            }
        }

        public int Seed
        {
            get
            {
                return this.seed;
            }
            set
            {
                this.seed = value;
            }
        }

        public MQChannelEntry ThisAlphaEntry
        {
            get
            {
                return this.thisAlphaEntry;
            }
            set
            {
                this.thisAlphaEntry = value;
            }
        }

        public MQChannelEntry ThisWeightedEntry
        {
            get
            {
                return this.thisWeightedEntry;
            }
            set
            {
                this.thisWeightedEntry = value;
            }
        }
    }
}

