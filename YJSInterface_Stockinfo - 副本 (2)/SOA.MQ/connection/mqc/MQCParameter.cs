using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBM.WMQ;

namespace SOA.connection.mqc
{
    /// <summary>
    /// MQ 连接实例
    /// </summary>
    public class MQCParameter
    {

        /**
         * MQ 连接配置对象
         */
        public MQParameter mqParameter = null;

        /**
         * MQ 队列管理器
         */
        public MQQueueManager qManager = null;

        /**
         * MQ Q队列
         */
        public MQQueue queue = null;

        public MQParameter getMqParameter()
        {
            return mqParameter;
        }

        public MQQueueManager getqManager()
        {
            return qManager;
        }

        public MQQueue getQueue()
        {
            return queue;
        }

        /**
         * @param mqParameter
         */
        public MQCParameter(MQParameter mqParameter): base()
        {
            //super();      
            this.mqParameter = mqParameter;
        }

        /**
         * 断开Q队列和 队列管理的链接，并将MQ对象清空
         */
        public void release()
        {
            /*
             * if (this.resQueue != null) try { this.resQueue.close(); } catch
             * (MQException e) { }
             */
            if (this.queue != null)
            {
                try
                {
                    if (this.queue.IsOpen)
                        this.queue.Close();
                }
                catch (MQException e)
                {
                }
                this.queue = null;
            }
            if (this.qManager != null)
            {
                /*
                 * try { this.qManager.close(); } catch (MQException e) { }
                 */
                try
                {
                    if (this.qManager.IsOpen)
                        this.qManager.Close();

                    if (this.qManager.IsConnected)
                        this.qManager.Disconnect();
                }
                catch (MQException e)
                {
                }

                this.qManager = null;
            }
            /*
             * if (this.token != null)
             * MQEnvironment.removeConnectionPoolToken(this.token);
             */
        }
    } // end of class
}
