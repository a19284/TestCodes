using SOA.exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.handler
{
    /// <summary>
    /// 服务响应处理接口
    /// </summary>
  public interface IServiceHandler {
	/**
	 * 服务方处理方法,处理请求方的请求报文,并返回报文
	 * @param reqMo request message object
	 * @return response message object
	 * @throws EisException
	 */
	// IMsgObject execute(IMsgObject reqMo) throws EisException;
      //SOA.message.Response.Service execute(SOA.message.Response.Service reqMo);// throws EisException;
      System.Xml.XmlDocument execute(System.Xml.XmlDocument reqMo);// throws EisException;
      
	/**
	 * @param exception EisException
	 */
	 void handleException(EisException exception);
}
}
