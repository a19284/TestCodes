using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.sender
{
    /// <summary>
    /// 服务发送方接口
    /// </summary>
   public interface IServiceSender {
	/**
	 * @param request message
	 * @return group id or message id
	 * @throws AdapterException
	 */
	 byte[] execute(byte[] msg);
	
	/**
	 * @param request message which is read from file
	 * @return group id or message id
	 * @throws AdapterException
	 */
	 byte[] execute(String filename);

}
}
