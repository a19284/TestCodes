namespace IBM.WMQ.PCF
{
    using IBM.WMQ;
    using System;

    public abstract class PCFParameter : PCFHeader
    {
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        protected PCFParameter()
        {
        }

        public abstract object GetValue();
        public static PCFParameter NextParameter(MQMessage message)
        {
            int num = message.ReadInt4();
            message.DataOffset -= 4;
            switch (num)
            {
                case 3:
                    return new MQCFIN(message);

                case 4:
                    return new MQCFST(message);

                case 5:
                    return new MQCFIL(message);

                case 6:
                    return new MQCFSL(message);
            }
            throw new Exception("Unknown type");
        }

        public abstract void SetValue(object value);
    }
}

