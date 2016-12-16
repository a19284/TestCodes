namespace IBM.WMQ
{
    using System;

    public interface MQReceiveExit
    {
        byte[] ReceiveExit(MQChannelExit channelExitParms, MQChannelDefinition channelDefinition, byte[] dataBuffer, ref int dataOffset, ref int dataLength, ref int dataMaxLength);
    }
}

