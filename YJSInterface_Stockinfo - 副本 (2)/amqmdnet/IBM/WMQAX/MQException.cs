namespace IBM.WMQAX
{
    using IBM.WMQ;
    using System;

    public class MQException : IBM.WMQ.MQException
    {
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        private MQException()
        {
        }
    }
}

