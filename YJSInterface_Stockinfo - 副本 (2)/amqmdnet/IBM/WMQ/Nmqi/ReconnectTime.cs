namespace IBM.WMQ.Nmqi
{
    using System;

    internal class ReconnectTime
    {
        public int delayTime;
        public int randTime;

        internal ReconnectTime(int delayTime, int randTime)
        {
            this.delayTime = delayTime;
            this.randTime = randTime;
        }
    }
}

