using SOA.connection.mqc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.connection
{
    /// <summary>
    /// 服务连接接口类
    /// </summary>
    public interface IConnection
    {
     /** 
	 * Default timeout is 5 minutes.<br>
	 * 默认超时时间 : 2分钟.
	 **/
      //  public static readonly int DEFAULT_TIMEOUT = 2 * 60;    

      byte[] request(byte[] msg, int timeout);

     /**
	 * 发送方法.
	 *
	 * @param request message object
	 * @return message id
	 * 
	 */
      MQMsgRef send(MQMsgRef msg);


    /**
	 * 接收方法
	 *
	 * @return response message object
	 * 
	 */
      MQMsgRef receive();

     /**
	 * 接收方法(自定义超时时间参数)
	 *
	 * @param timeout time to wait for the response message (unit : second)
	 * @return response message object
	 * 
	 */
      MQMsgRef receive(int timeout);

    /**
	 * 接收方法
	 *
	 * @param timeout time to wait for the response message (unit : second)
	 * @param message id
	 * @return response message object
	 * 
	 */
    MQMsgRef receive(int timeout, MQMsgRef mqMsgRef);


    /**
	 * release connection.
	 *
	 * 
	 */
	 void release();
	
    }
}
