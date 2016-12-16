namespace IBM.WMQ
{
    using System;

    internal class MQDisposer
    {
        private IDisposable disposableObject;
        private const string sccsid = "%Z% %W% %I% %E% %U%";

        internal MQDisposer(IDisposable obj)
        {
            this.disposableObject = obj;
        }

        ~MQDisposer()
        {
            try
            {
                if (this.disposableObject != null)
                {
                    this.disposableObject.Dispose();
                }
            }
            catch
            {
            }
        }
    }
}

