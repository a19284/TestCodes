using SOA.connection.mqc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.connection.mqcm
{
    public class MQPool
    {
        public class MQConnectionObject : IDynamicObject
        {
            private MQConnection connection;

            public MQConnectionObject()
            {
                connection = null;
            }

            #region IDynamicObject Members
            /// <summary>
            /// 初始化
            /// </summary>
            /// <param name="param"></param>
            public void Create(Object param)
            {
                //String strConn = (String)param;
                //_SqlConn = new MQConnection(strConn);
                //_SqlConn.Open();

                connection = new MQConnection(((MQPool)param).getSendQueue(), ((MQPool)param).getReceiveQueue());
                Boolean ret = connection.initMQConnection();
               // if (!ret)
                  //  throw new Exception("Failed to init connection.");
               // return connection;
            }

            public Object GetInnerObject()
            {
                // TODO: Add SqlConnectionObject.GetInnerObject implementation 
                return connection;
            }
            /// <summary>
            /// 连接是否有效
            /// </summary>
            /// <returns></returns>
            public bool IsValidate()
            {
                return (connection != null
                    && connection.GetHashCode() > 0
                    && connection.valid());
            }
            /// <summary>
            /// 释放当前对象的MQ连接
            /// </summary>
            public void Release()
            {                
                connection.release();
            }

            #endregion
        }

        private ObjectPool _Connections;
        private List<MQParameter> sendQueueList = new List<MQParameter>();
        private List<MQParameter> receiveQueueList = new List<MQParameter>();

        public MQPool(PoolableMQConnectionManager connection, int initcount, int capacity)
        {
           
            if (connection == null ||  initcount < 0 || capacity < 1)
            {
                throw (new Exception("Invalid parameter!"));
            }
            _Connections = new ObjectPool(typeof(MQConnectionObject), connection, initcount, capacity);
        }
        public MQPool(MQParameter sendQueue, MQParameter receiveQueue, int capacity, int initcount)
        {
            this.sendQueueList.Add(sendQueue);
            this.receiveQueueList.Add(receiveQueue);

            if ( initcount < 0 || capacity < 1)
            {
                throw (new Exception("Invalid parameter!"));
            }
            _Connections = new ObjectPool(typeof(MQConnectionObject), this, initcount, capacity);
        }
        public MQPool(List<MQParameter> sendQueueList, List<MQParameter> receiveQueueList, int capacity, int initcount)
        {
            this.sendQueueList = sendQueueList;
            this.receiveQueueList = receiveQueueList;
            if ( initcount < 0 || capacity < 1)
            {
                throw (new Exception("Invalid parameter!"));
            }
            _Connections = new ObjectPool(typeof(MQConnectionObject), this, initcount, capacity);
        }
        public MQConnection GetConnection()
        {
            return (MQConnection)_Connections.GetOne();
        }

        public void FreeConnection(object sqlConn)
        {
            _Connections.FreeObject(sqlConn);
        }

        public void Release()
        {
            _Connections.Release();
        }

        public int Count
        {
            get { return _Connections.CurrentSize; }
        }

        public int UsingCount
        {
            get { return _Connections.ActiveCount; }
        }

        public int DecreaseSize(int size)
        {
            return _Connections.DecreaseSize(size);
        }
        public List<MQParameter> getSendQueue()
        {
            return sendQueueList;
        }

        public List<MQParameter> getReceiveQueue()
        {
            return receiveQueueList;
        }  


    } // DBPool


}
