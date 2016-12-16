using SOA.connection.mqc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.connection.mqcm
{
    public class MQConnectionPoolFactory : IDynamicObject
    {
        private MQConnection connection;

        public MQConnectionPoolFactory()
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

                connection = new MQConnection(((PoolableMQConnectionManager)param).getSendQueue(), ((PoolableMQConnectionManager)param).getReceiveQueue());
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
}
