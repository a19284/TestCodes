using SOA.connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.requester
{
    /// <summary>
    /// 服务请求方接口
    /// </summary>
    public interface IServiceRequester
    {
        /**
         * @param reqMo
         *            request message object
         * @return response message object
         * @throws Exception 
         * @throws AdapterException
         */
        System.Xml.XmlDocument execute(System.Xml.XmlDocument reqMo);

        /**
         * @param reqMo
         *            request message object
         * @param timeout
         *            time to wait for the response (seconds)
         * @return response message object
         * @throws AdapterException
         */
        System.Xml.XmlDocument execute(System.Xml.XmlDocument reqMo, int timeout);

        /**
         * 关闭连接
         */
        void closeConnection();

        /**
         * 获取连接池管理器List[用于Requester转换MQ队列管理器重发]
         * @return
         */
       // List<IConnectionPoolManager> getConnectionPoolManagerList();
    }
}
