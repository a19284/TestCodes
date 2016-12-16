namespace IBM.WMQ
{
    using System;

    internal class ExitInstance : MQBase
    {
        public int exitID;
        public string exitName;
        public byte[] exitUserArea;
        public bool initialised;
        public bool loaded;
        public object Method;
        public bool suppressed;
        public string userData;

        public ExitInstance(string exitName, string userData, int exitID)
        {
            this.exitName = exitName;
            this.userData = userData;
            this.exitID = exitID;
        }
    }
}

