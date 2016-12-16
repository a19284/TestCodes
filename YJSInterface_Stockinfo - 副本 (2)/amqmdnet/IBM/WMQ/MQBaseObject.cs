namespace IBM.WMQ
{
    using System;

    public abstract class MQBaseObject : MQBase
    {
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public MQBaseObject()
        {
            this.ClearErrorCodes();
        }

        public void ClearErrorCodes()
        {
            base.unsafe_compCode = 0;
            base.unsafe_reason = 0;
        }

        protected override void TrEntry(uint method)
        {
            this.ClearErrorCodes();
            base.TrEntry(method);
        }

        public virtual int CompletionCode
        {
            get
            {
                return base.unsafe_compCode;
            }
            set
            {
                base.unsafe_compCode = value;
            }
        }

        public virtual int ReasonCode
        {
            get
            {
                return base.unsafe_reason;
            }
            set
            {
                base.unsafe_reason = value;
            }
        }

        public virtual string ReasonName
        {
            get
            {
                if (this.ReasonCode == 0)
                {
                    return "MQRC_OK";
                }
                return CommonServices.ReasonCodeName(this.ReasonCode);
            }
        }
    }
}

