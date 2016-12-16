namespace IBM.WMQ.Nmqi
{
    using System;

    internal interface IMQNetworkModule : IDisposable
    {
        void Close();
        int Read(byte[] buffer, int offset, int length);
        int Write(byte[] buffer, int offset, int length);
    }
}

