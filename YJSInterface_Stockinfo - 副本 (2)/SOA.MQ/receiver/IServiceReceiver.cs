using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.receiver
{
   /// <summary>
   /// 服务接受方接口
   /// </summary>
public interface IServiceReceiver {
	/**
	 * @param group id or message id
	 * @return received message
	 * @throws AdapterException
	 */
	byte[] execute(byte[] id);
	
	/**
	 * @param group id or message id
	 * @param file name which will be writed
	 * @return write file successfully or failed (true/false)
	 * @throws EisException
	 */
	 Boolean execute(byte[] id, String filename);
}
}
