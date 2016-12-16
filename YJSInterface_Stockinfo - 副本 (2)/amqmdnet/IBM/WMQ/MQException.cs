namespace IBM.WMQ
{
    using System;

    public class MQException : ApplicationException
    {
        private int compCode;
        private int reason;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        protected MQException()
        {
        }

        public MQException(int compCode, int reason) : base(CommonServices.ReasonCodeName(reason))
        {
            this.compCode = compCode;
            this.reason = reason;
            CommonServices.TraceText("MQException CompCode: " + compCode.ToString() + " Reason: " + reason.ToString());
        }

        public MQException(int compCode, int reason, Exception innerException) : base(CommonServices.ReasonCodeName(reason), innerException)
        {
            this.compCode = compCode;
            this.reason = reason;
            CommonServices.TraceText("MQException CompCode: " + compCode.ToString() + " Reason: " + reason.ToString());
        }

        public override string ToString()
        {
            return ("CompCode: " + this.compCode.ToString() + ", Reason: " + this.reason.ToString());
        }

        public int CompCode
        {
            get
            {
                return this.compCode;
            }
            set
            {
                this.compCode = value;
            }
        }

        public int CompletionCode
        {
            get
            {
                return this.compCode;
            }
            set
            {
                this.compCode = value;
            }
        }

        public override string Message
        {
            get
            {
                return CommonServices.ReasonCodeName(this.reason);
            }
        }

        public int Reason
        {
            get
            {
                return this.reason;
            }
            set
            {
                this.reason = value;
            }
        }

        public int ReasonCode
        {
            get
            {
                return this.reason;
            }
            set
            {
                this.reason = value;
            }
        }
    }
}

