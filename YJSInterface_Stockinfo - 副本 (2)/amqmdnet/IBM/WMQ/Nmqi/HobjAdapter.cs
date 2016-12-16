namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;

    public class HobjAdapter : MQBase, Hobj
    {
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private int value;

        public HobjAdapter()
        {
            this.value = -1;
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.value = -1;
        }

        public HobjAdapter(int initHobjValue)
        {
            this.value = -1;
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { initHobjValue });
            this.value = initHobjValue;
        }

        public override string ToString()
        {
            uint method = 1;
            this.TrEntry(method);
            if (this.value == -1)
            {
                base.TrExit(method, "MQHO_UNUSABLE_HOBJ", 1);
                return "MQHO_UNUSABLE_HOBJ";
            }
            if (this.value == 0)
            {
                base.TrExit(method, "MQHO_NONE", 2);
                return "MQHO_NONE";
            }
            string result = this.value.ToString();
            base.TrExit(method, result, 3);
            return result;
        }

        public int Handle
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

