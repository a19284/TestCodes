namespace IBM.WMQ.Nmqi
{
    using System;

    public interface Hconn
    {
        int Ccsid { get; }

        int CmdLevel { get; }

        byte[] ConnectionId { get; set; }

        int FapLevel { get; }

        string Name { get; }

        int Platform { get; }

        int SharingConversations { get; }

        string Uid { get; }

        int Value { get; set; }
    }
}

