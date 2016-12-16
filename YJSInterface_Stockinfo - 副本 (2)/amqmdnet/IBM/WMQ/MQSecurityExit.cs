namespace IBM.WMQ
{
    using System;

    public interface MQSecurityExit
    {
        byte[] SecurityExit(MQChannelExit channelExitParms, MQChannelDefinition channelDefinition, byte[] dataBuffer, ref int dataOffset, ref int dataLength, ref int dataMaxLength);
    }
}

