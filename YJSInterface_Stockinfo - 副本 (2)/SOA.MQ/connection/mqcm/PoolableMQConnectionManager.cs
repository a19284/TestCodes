using SOA.connection.mqc;
using SOA.log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.connection.mqcm
{
    public class PoolableMQConnectionManager : IConnectionPoolManager
    {
       // private MQConnectionPool pool;

        private ObjectPool pool;

        private List<MQParameter> sendQueueList = new List<MQParameter>();
        private List<MQParameter> receiveQueueList = new List<MQParameter>();



        /**
         * @param sendQueue
         * @param receiveQueue
         * @param maxActive
         *            max connection in pool
         * @param maxWait
         *            max time to wait for a connection (second)
         */
        public PoolableMQConnectionManager(MQParameter sendQueue, MQParameter receiveQueue, int maxActive, int maxWait)
        {
            this.sendQueueList.Add(sendQueue);
            this.receiveQueueList.Add(receiveQueue);
            this.pool = new ObjectPool(typeof(SOA.connection.mqcm.MQPool.MQConnectionObject), this, maxActive, maxWait);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sendQueueList"></param>
        /// <param name="receiveQueueList"></param>
        /// <param name="maxActive"></param>
        /// <param name="maxWait"></param>
        public PoolableMQConnectionManager(List<MQParameter> sendQueueList,List<MQParameter> receiveQueueList, int maxActive, int maxWait)
        {
            //this.sendQueueList = sendQueueList;
            //this.receiveQueueList = receiveQueueList;

            //object[] cArgs = new object[1];
            //cArgs.SetValue(new MQConnectionPoolFactory(this), 1);

            //this.pool = new MQConnectionPool(typeof(MQConnectionPoolFactory), cArgs, maxActive, maxWait);

            this.sendQueueList = sendQueueList;
            this.receiveQueueList = receiveQueueList;

            //object[] cArgs = new object[1];
            //cArgs.SetValue(new MQConnectionPoolFactory(this), 0);

            this.pool = new ObjectPool(typeof(MQConnectionPoolFactory), this, maxActive, maxWait);



        }

        /**
         * 从对象池中获取一个对象
         */
        public IConnection getConnection(){
		IConnection connection = null;
		try {
            lock (pool)
            {
				connection = (IConnection) pool.GetOne();
			}
		} catch (Exception e) {
			LogUtil.Error("Fail to get connection.",e);
			//throw new EisException(e);
		}
		return connection;
	}

        /**
         * 将连接返回到对象池
         */
        //public void releaseConnection(IConnection connection)
        //{
        //    try
        //    {
        //        lock (pool)
        //        {
        //            if (null != connection)
        //                pool.returnObject(connection);
        //            connection = null;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        LogUtil.Error("Fail to release connection.", e);
        //        //throw new EisException(e);
        //    }
        //}

        /**
        * 将连接返回到对象池
        */
        public void releaseConnection( IConnection connection)
        {
            try
            {
                lock (pool)
                {
                    if (null != connection)
                        pool.FreeObject(connection);
                    connection = null;
                }
            }
            catch (Exception e)
            {
                LogUtil.Error("Fail to release connection.", e);
                //throw new EisException(e);
            }
        }

        /**
         * 关闭对象池中所有连接
         */
        public void close(){
		try {
            lock (pool)
            {
				if (null != pool)
					pool.Release();
			}
		} catch (Exception e) {
			LogUtil.Error("Fail to close connection pool.",e);
			//throw new EisException(e);
		}
	}


        public List<MQParameter> getSendQueue()
        {
            return sendQueueList;
        }

        public List<MQParameter> getReceiveQueue()
        {
            return receiveQueueList;
        }
    }
}
