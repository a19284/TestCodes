using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.connection.mqc
{
    public class MQParameter
    {

        private String hostName;

        private int port = 0;

        private String channel;

        private int ccsid = 0;

        private String qManagerName;

        private String queueName;

        /**
         * @param hostName
         * @param port
         * @param channel
         * @param ccsid
         * @param managerName
         * @param queueName
         */
        public MQParameter(String hostName, int port, String channel, int ccsid, String managerName, String queueName)
        {
            //super();
            this.hostName = hostName;
            this.port = port;
            this.channel = channel;
            this.ccsid = ccsid;
            this.qManagerName = managerName;
            this.queueName = queueName;
        }

        /**
         * @return the hostName
         */
        public String getHostName()
        {
            return hostName;
        }

        /**
         * @return the port
         */
        public int getPort()
        {
            return port;
        }

        /**
         * @return the channel
         */
        public String getChannel()
        {
            return channel;
        }

        /**
         * @return the ccsid
         */
        public int getCcsid()
        {
            return ccsid;
        }

        /**
         * @return the queueName
         */
        public String getQueueName()
        {
            return queueName;
        }

        /**
         * @return the qManagerName
         */
        public String getQManagerName()
        {
            return qManagerName;
        }

        public String toString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("MQParameter[hostname=").Append(this.hostName)
            .Append(",port=").Append(this.port)
            .Append(",ccsid=").Append(this.ccsid)
            .Append(",channel=").Append(this.channel)
            .Append(",qmanager=").Append(this.qManagerName)
            .Append(",queue=").Append(this.queueName).Append("]");
            return sb.ToString();
        }

    }
}
