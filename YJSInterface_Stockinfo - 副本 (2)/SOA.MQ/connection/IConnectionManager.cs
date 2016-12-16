using SOA.exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.connection
{
    /// <summary>
    /// 连接管理服务接口
    /// </summary>
    public interface IConnectionManager
    {
        /**
	 * For service requester, get connection to service provider.
	 *
	 * @return connection instance
	 * @throws EisException
	 */
       IConnection getConnection();
	//public IConnection getConnection() throws EisException;

	/**
	 * 
	 * @param connection
	 * @throws EisException
	 */
	//public void releaseConnection(IConnection connection) throws EisException;
       void releaseConnection(IConnection connection);

   
    

    }
}
