namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;

    public class NmqiException : MQException
    {
        public const int AMQ6047 = 0x179f;
        public const int AMQ6051 = 0x17a3;
        public const int AMQ6052 = 0x17a4;
        public const int AMQ6090 = 0x17ca;
        private const string AMQ6090_ENG_MSG = "&MQ_long was unable to display an error message &6.";
        private const string AMQ6090_ENG_SEVERITY = "0";
        private const string AMQ6090_ENG_URESP = "Use the standard facilities supplied with your system to record the problem identifier, and to save the generated output files. Contact your IBM support center. Do not discard these files until the problem has been resolved.";
        private const string AMQ6090_ENG_XPL = "&MQ_short has attempted to display the message associated with return code hexadecimal '&6'. The return code indicates that there is no message text associated with the message. Associated with the request are inserts &1 &2 &3 &4 &5.";
        public const int AMQ6271 = 0x187f;
        public const int AMQ8562 = 0x2172;
        public const int AMQ8568 = 0x2178;
        public const int AMQ9181 = 0x23dd;
        public const int AMQ9184 = 0x23e0;
        public const int AMQ9204 = 0x23f4;
        public const int AMQ9205 = 0x23f5;
        public const int AMQ9206 = 0x23f6;
        public const int AMQ9208 = 0x23f8;
        public const int AMQ9213 = 0x23fd;
        public const int AMQ9214 = 0x23fe;
        public const int AMQ9231 = 0x240f;
        public const int AMQ9248 = 0x2420;
        public const int AMQ9496 = 0x2518;
        public const int AMQ9503 = 0x251f;
        public const int AMQ9509 = 0x2525;
        public const int AMQ9516 = 0x252c;
        public const int AMQ9520 = 0x2530;
        public const int AMQ9524 = 0x2534;
        public const int AMQ9525 = 0x2535;
        public const int AMQ9528 = 0x2538;
        public const int AMQ9530 = 0x253a;
        public const int AMQ9535 = 0x253f;
        public const int AMQ9536 = 0x2540;
        public const int AMQ9546 = 0x254a;
        public const int AMQ9547 = 0x254b;
        public const int AMQ9555 = 0x2553;
        public const int AMQ9566 = 0x255e;
        public const int AMQ9633 = 0x25a1;
        public const int AMQ9635 = 0x25a3;
        public const int AMQ9636 = 0x25a4;
        public const int AMQ9640 = 0x25a8;
        public const int AMQ9641 = 0x25a9;
        public const int AMQ9643 = 0x25ab;
        public const int AMQ9666 = 0x25c2;
        public const int AMQ9714 = 0x25f2;
        public const int AMQ9736 = 0x2608;
        public const int AMQ9771 = 0x262b;
        public const int AMQ9913 = 0x26b9;
        public const int AMQ9915 = 0x26bb;
        public const int AMQ9999 = 0x270f;
        private int amqXXXX;
        private int compCode;
        private NmqiEnvironment env;
        private string[] inserts;
        public const int NO_AMQ_MESSAGE = -1;
        private int reason;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public NmqiException(NmqiEnvironment env, int amqXXXX, string[] inserts, int compCode, int reason, Exception nestedException) : base(compCode, reason)
        {
            this.env = env;
            this.amqXXXX = amqXXXX;
            if (inserts != null)
            {
                this.inserts = new string[inserts.Length];
                for (int i = 0; i < inserts.Length; i++)
                {
                    this.inserts[i] = inserts[i];
                }
            }
            this.compCode = compCode;
            this.reason = reason;
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
    }
}

