namespace IBM.WMQ
{
    using System;

    public interface IMQCommsBuffer
    {
        void AddUseCount(int usecount);
        void Free();

        byte[] Buffer { get; set; }

        int Capacity { get; }

        int DataAvailable { get; set; }

        int DataPosition { get; set; }

        int DataUsed { get; set; }

        int UseCount { get; set; }
    }
}

