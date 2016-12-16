namespace IBM.WMQ
{
    using System;

    public interface MQSendExit
    {
        byte[] SendExit(MQChannelExit channelExitParms, MQChannelDefinition channelDefinition, byte[] dataBuffer, ref int dataOffset, ref int dataLength, ref int dataMaxLength);
    }
}

