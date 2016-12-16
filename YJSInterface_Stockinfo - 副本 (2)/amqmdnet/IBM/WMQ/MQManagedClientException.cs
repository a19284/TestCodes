namespace IBM.WMQ
{
    using System;
    using System.Text;

    internal class MQManagedClientException : MQException
    {
        private string explanation;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        internal MQManagedClientException(int compCode, int reason) : base(compCode, reason)
        {
        }

        internal MQManagedClientException(string explanation, int compCode, int reason) : base(compCode, reason)
        {
            this.explanation = "Explanation: " + explanation;
        }

        internal MQManagedClientException(uint returncode, uint arith1, uint arith2, string comment1, string comment2, string comment3, int compCode, int reason) : base(compCode, reason)
        {
            CommonServices.SetValidInserts();
            CommonServices.ArithInsert1 = arith1;
            CommonServices.ArithInsert2 = arith2;
            if (comment1 != null)
            {
                CommonServices.CommentInsert1 = comment1;
            }
            if (comment2 != null)
            {
                CommonServices.CommentInsert2 = comment2;
            }
            if (comment3 != null)
            {
                CommonServices.CommentInsert3 = comment3;
            }
            CommonServices.GetMessage(returncode, 6, out this.explanation);
            CommonServices.DisplayMessage(string.Empty, null, returncode, 0xf0000010);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(base.ToString());
            builder.Append(this.Explanation);
            return builder.ToString();
        }

        internal string Explanation
        {
            get
            {
                return this.explanation;
            }
            set
            {
                this.explanation = value;
            }
        }
    }
}

