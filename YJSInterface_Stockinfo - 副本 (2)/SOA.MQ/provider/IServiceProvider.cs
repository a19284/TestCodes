using SOA.connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.provider
{
    /// <summary>
    /// 服务提供方接口
    /// </summary>
   public interface IServiceProvider {
	/**
	 * Pause processing service responses.
	 */
	 void pause();

	/**
	 * A service provider process service requests and generates service
	 * responses via service handler.
	 *
	 * @param serviceHandler
	 */
	 void setServiceHandler(SOA.handler.IServiceHandler serviceHandler);

	/**
	 * Start processing service requests.
	 */
	 void start();

	/**
	 * Stop processing service responses.
	 */
	 void stop();

     void closeConnection();


}
}
