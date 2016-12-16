namespace IBM.WMQ
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Net;

    internal class MQChannelListEntry : MQBase
    {
        private MQChannelEntry alphaEntry;
        private string channelFile;
        private MQChannelEntry lastWeightedEntry;
        private long modTime;
        private string name;
        private MQChannelListEntry next;
        private bool ordered;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private MQChannelEntry thisAlphaEntry;
        private MQChannelEntry thisWeightedEntry;
        private int totalWeight;
        private bool updateRequired;
        private int useCount;
        private MQChannelEntry weightedEntry;

        internal MQChannelListEntry()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.next = null;
            this.name = null;
            this.useCount = 0;
            this.updateRequired = false;
            this.totalWeight = 0;
            this.channelFile = null;
            this.modTime = 0L;
            this.alphaEntry = null;
            this.thisAlphaEntry = null;
            this.weightedEntry = null;
            this.thisWeightedEntry = null;
        }

        internal void AddChannelEntry(MQChannelDefinition channel)
        {
            uint method = 0x27;
            this.TrEntry(method, new object[] { channel });
            try
            {
                MQChannelEntry entry = new MQChannelEntry(channel, null);
                if (entry.Channel.ClientChannelWeight == 0)
                {
                    if (this.thisAlphaEntry != null)
                    {
                        this.thisAlphaEntry.Next = entry;
                    }
                    else
                    {
                        this.alphaEntry = entry;
                    }
                    entry.Next = null;
                    this.thisAlphaEntry = entry;
                }
                else
                {
                    entry.Next = this.weightedEntry;
                    this.weightedEntry = entry;
                    this.totalWeight += channel.ClientChannelWeight;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal void CheckUpdateRequired()
        {
            uint method = 0x26;
            this.TrEntry(method);
            try
            {
                if (this.channelFile != null)
                {
                    FileInfo info = new FileInfo(this.channelFile);
                    long num2 = info.LastWriteTime.ToFileTime();
                    if (this.modTime != num2)
                    {
                        this.updateRequired = true;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void CreateChannelEntryLists(IEnumerator enumerator)
        {
            uint method = 0x4c9;
            this.TrEntry(method, new object[] { enumerator });
            while (enumerator.MoveNext())
            {
                MQChannelDefinition current = (MQChannelDefinition) enumerator.Current;
                this.AddChannelEntry(current);
            }
            if (this.weightedEntry != null)
            {
                this.OrderWeightedChannelEntry();
            }
            this.thisAlphaEntry = this.alphaEntry;
            this.thisWeightedEntry = this.weightedEntry;
            base.TrExit(method);
        }

        internal void OrderWeightedChannelEntry()
        {
            uint method = 40;
            this.TrEntry(method);
            if (!this.ordered)
            {
                int num3 = 0;
                int num4 = 0;
                Random random = null;
                int maxValue = 0;
                MQChannelEntry weightedEntry = null;
                MQChannelEntry entry2 = null;
                MQChannelEntry entry3 = null;
                MQChannelEntry entry4 = null;
                if (this.lastWeightedEntry != null)
                {
                    this.lastWeightedEntry.Next = null;
                }
                try
                {
                    random = new Random(Dns.GetHostName().GetHashCode());
                    maxValue = this.totalWeight;
                    while (this.weightedEntry != null)
                    {
                        num4 = random.Next(maxValue);
                        weightedEntry = this.weightedEntry;
                        entry2 = null;
                        num3 = 0;
                        while (weightedEntry != null)
                        {
                            num3 += weightedEntry.Channel.ClientChannelWeight;
                            if (num3 > num4)
                            {
                                maxValue -= weightedEntry.Channel.ClientChannelWeight;
                                if (entry4 != null)
                                {
                                    entry4.Next = weightedEntry;
                                }
                                else
                                {
                                    entry3 = weightedEntry;
                                }
                                entry4 = weightedEntry;
                                if (entry2 != null)
                                {
                                    entry2.Next = weightedEntry.Next;
                                }
                                else
                                {
                                    this.weightedEntry = weightedEntry.Next;
                                }
                                continue;
                            }
                            entry2 = weightedEntry;
                            weightedEntry = weightedEntry.Next;
                        }
                    }
                    entry4.Next = entry3;
                    this.weightedEntry = entry3;
                    this.thisWeightedEntry = entry3;
                    this.lastWeightedEntry = entry4;
                    this.ordered = true;
                }
                finally
                {
                    base.TrExit(method);
                }
            }
        }

        internal MQChannelDefinition SelectChannelEntry(MQChannelDefinition cd, MQThreadChannelEntry threadChlEntry)
        {
            uint method = 0x29;
            this.TrEntry(method, new object[] { cd, threadChlEntry });
            MQChannelEntry thisWeightedEntry = null;
            try
            {
                if (cd != null)
                {
                    thisWeightedEntry = this.SelectNamedEntry(cd);
                }
                else if (threadChlEntry.ThisWeightedEntry != null)
                {
                    threadChlEntry.ThisWeightedEntry = threadChlEntry.ThisWeightedEntry.Next;
                    if (threadChlEntry.ThisWeightedEntry != threadChlEntry.FirstWeightedEntry)
                    {
                        thisWeightedEntry = threadChlEntry.ThisWeightedEntry;
                    }
                }
                else
                {
                    if (threadChlEntry.ThisAlphaEntry != null)
                    {
                        threadChlEntry.ThisAlphaEntry = threadChlEntry.ThisAlphaEntry.Next;
                    }
                    else
                    {
                        threadChlEntry.ThisAlphaEntry = this.thisAlphaEntry;
                    }
                    if (threadChlEntry.ThisAlphaEntry != null)
                    {
                        thisWeightedEntry = threadChlEntry.ThisAlphaEntry;
                    }
                    else if (this.thisWeightedEntry != null)
                    {
                        if (this.thisWeightedEntry.Channel.ConnectionAffinity == 1)
                        {
                            thisWeightedEntry = this.thisWeightedEntry;
                        }
                        else
                        {
                            thisWeightedEntry = this.SelectRandomEntry(threadChlEntry);
                        }
                        threadChlEntry.FirstWeightedEntry = thisWeightedEntry;
                        threadChlEntry.ThisWeightedEntry = thisWeightedEntry;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return ((thisWeightedEntry == null) ? null : thisWeightedEntry.Channel);
        }

        private MQChannelEntry SelectNamedEntry(MQChannelDefinition cd)
        {
            uint method = 0x2a;
            this.TrEntry(method, new object[] { cd });
            MQChannelEntry alphaEntry = this.alphaEntry;
            MQChannelEntry weightedEntry = null;
            string channelName = cd.ChannelName;
            try
            {
                while (alphaEntry != null)
                {
                    if (alphaEntry.Channel.ChannelName.CompareTo(channelName) == 0)
                    {
                        break;
                    }
                    alphaEntry = alphaEntry.Next;
                }
                if (alphaEntry != null)
                {
                    return alphaEntry;
                }
                alphaEntry = this.weightedEntry;
                weightedEntry = this.weightedEntry;
                while (alphaEntry != null)
                {
                    if (alphaEntry.Channel.ChannelName.CompareTo(channelName) == 0)
                    {
                        return alphaEntry;
                    }
                    alphaEntry = alphaEntry.Next;
                    if (alphaEntry == weightedEntry)
                    {
                        return null;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return alphaEntry;
        }

        private MQChannelEntry SelectRandomEntry(MQThreadChannelEntry threadChlEntry)
        {
            uint method = 0x2b;
            this.TrEntry(method, new object[] { threadChlEntry });
            MQChannelEntry weightedEntry = null;
            Random random = null;
            int num2 = 0;
            int num3 = 0;
            try
            {
                if (threadChlEntry.Seed == 0)
                {
                    DateTime now = DateTime.Now;
                    threadChlEntry.Seed = (int) now.Ticks;
                }
                random = new Random(threadChlEntry.Seed);
                num2 = random.Next() % this.totalWeight;
                weightedEntry = this.weightedEntry;
                weightedEntry = this.weightedEntry;
                while (weightedEntry != null)
                {
                    num3 += weightedEntry.Channel.ClientChannelWeight;
                    if (num3 > num2)
                    {
                        return weightedEntry;
                    }
                    weightedEntry = weightedEntry.Next;
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return weightedEntry;
        }

        public MQChannelEntry AlphaEntry
        {
            get
            {
                return this.alphaEntry;
            }
            set
            {
                this.alphaEntry = value;
            }
        }

        public string ChannelFile
        {
            get
            {
                return this.channelFile;
            }
            set
            {
                this.channelFile = value;
            }
        }

        public long ModTime
        {
            get
            {
                return this.modTime;
            }
            set
            {
                this.modTime = value;
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

        public MQChannelListEntry Next
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

        public int TotalWeight
        {
            get
            {
                return this.totalWeight;
            }
            set
            {
                this.totalWeight = value;
            }
        }

        public bool UpdateRequired
        {
            get
            {
                return this.updateRequired;
            }
            set
            {
                this.updateRequired = value;
            }
        }

        public int UseCount
        {
            get
            {
                return this.useCount;
            }
            set
            {
                this.useCount = value;
            }
        }

        public MQChannelEntry WeightedEntry
        {
            get
            {
                return this.weightedEntry;
            }
            set
            {
                this.weightedEntry = value;
            }
        }
    }
}

