namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;

    public interface MQConsumer
    {
        void Consumer(Phconn hconn, MQMessageDescriptor mqmd, MQGetMessageOptions getMsgOpts, byte[] buffer, MQCBC mqcbc);
    }
}

