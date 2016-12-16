using SOA.connection;
using SOA.connection.mqc;
using SOA.exception;
using SOA.handler;
using SOA.log;
using SOA.message.implcom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SOA.provider.impl
{
    public class BaseServiceProvider : AbstractServiceProvider, IServiceProvider
    {

            public static IServiceProvider provider =null;
            private static readonly object syncRoot = new object();

            //构造方法1，使用配置文件的ServiceHandler
            public BaseServiceProvider():base() {
                //super();
            }
            public  static IServiceProvider getInstance()  {
                lock (syncRoot)
                {
                    if (null == provider)
                    {
                        provider = new BaseServiceProvider();
                    }
                }
                return provider;
            }

            //构造方法2，接收参数的ServiceHandler
            public BaseServiceProvider(IServiceHandler serviceHandler): base()
            {
                //super();
                setServiceHandler(serviceHandler);
            }

          
           
            public  static IServiceProvider getInstance(IServiceHandler serviceHandler)
            {
                lock (syncRoot)
                {
                    if (null == provider)
                    {
                        provider = new BaseServiceProvider(serviceHandler);
                    }
                }
               return provider;
              
            }

            //构造方法3，接收参数的ServiceHandler，启动备用ESB_OUT监听
            public BaseServiceProvider(IServiceHandler serviceHandler, String alt)
                : base(alt)
            {
                //super(alt);
                setServiceHandler(serviceHandler);
            }
            public  static IServiceProvider getInstance(IServiceHandler serviceHandler, String alt)  {
                lock (syncRoot)
                {
                    if (null == provider)
                    {
                        provider = new BaseServiceProvider(serviceHandler, alt);
                    }
                }
                return provider;
            }


            protected override void process()
            {
                IConnection connection = null;
                MQMsgRef reqMsgRef = null;
                try {
                    MQMsgRef sendMsg = new MQMsgRef();
              
                    connection = getConnectionPoolManager().getConnection();            
             
                    reqMsgRef = connection.receive(getProviderRecvTimeOut());
                    sendMsg.MQMsgId = reqMsgRef.MQMsgId;

                     string xml = System.Text.Encoding.UTF8.GetString(reqMsgRef.MQMsgBody);
                     XmlDocument reqMo = new XmlDocument();
                     reqMo.LoadXml(xml);
                   
                     //SOA.message.Response.Service reqMo = (SOA.message.Response.Service)SOA.message.XmlUtil.Deserialize(typeof(SOA.message.Response.Service), xml);

                   // Service reqMo = new MsgObject(reqMsgRef.MQMsgBody, IMsgObject.MOType.initSP);
                    // sendMsg.ReplyToQMgr = reqMo.getProReplyQmgr();MsgConstants.REPLY_QMGR
                     sendMsg.ReplyToQMgr = MsgConstants.REPLY_QMGR;
                    try{
                       // IMsgObject resMo = (IMsgObject)handler.execute(reqMo);
                        XmlDocument resMo = (XmlDocument)handler.execute(reqMo);
                        SOA.message.XmlHelper xh = new message.XmlHelper(resMo);
                        //String status = resMo.getServResStatus();
                        //String status = resMo.ServiceResponse.Status;
                        String status = xh.GetValue("/Service/Route/ServiceResponse/Status");                   


                        if (status != null && status.Equals(MsgConstants.STATUS_INPROCESS))
                        {
                            //resMo.setServResStatus(MsgConstants.STATUS_COMPLETE);
                            // resMo.ServiceResponse.Status=MsgConstants.STATUS_COMPLETE;
                            xh.SetValue("/Service/Route/ServiceResponse/Status", MsgConstants.STATUS_COMPLETE);
                        }

                       // String code = resMo.getServResCode();
                       // String code = resMo.ServiceResponse.Code;
                        String code = xh.GetValue("/Service/Route/ServiceResponse/Code");

                        if (code == null)
                        {
                            // resMo.setServResCode(MsgConstants.SUCCEED_CODE);
                            // resMo.ServiceResponse.Code= MsgConstants.SUCCEED_CODE;
                            xh.SetValue("/Service/Route/ServiceResponse/Code", MsgConstants.SUCCEED_CODE);
                        } 
                 
                        //sendMsg.MQMsgBody = resMo.getBytes();
                        //sendMsg.MQMsgBody = SOA.message.XmlHelper.getBytes(resMo);
                        sendMsg.MQMsgBody = SOA.message.XmlHelper.getBytes(xh._xml);
                   
                        connection.send(sendMsg);
                    }catch(Exception e){

                        //reqMo.setServResStatus(MsgConstants.STATUS_FAIL);
                        //reqMo.setServResCode(MsgConstants.API_ERROR_CODE);
                        //reqMo.setServResDesc(e.Message);
                        //sendMsg.MQMsgBody = reqMo.getBytes();

                        //reqMo.ServiceResponse.Status = MsgConstants.STATUS_FAIL;
                        //reqMo.ServiceResponse.Code = MsgConstants.API_ERROR_CODE;
                        //reqMo.ServiceResponse.Desc = e.Message;
                        //sendMsg.MQMsgBody = SOA.message.XmlHelper.getBytes(reqMo);

                        //connection.send(sendMsg);
                        throw new EisException(e);
                    }

                } catch (EisException e) {
                    if (!MQCException.MQ_MSG_RECEIVE_GETMSG_TIMEOUT_EXCEPTION_CODE.Equals(e.getCode())){
                        if (reqMsgRef != null) {
                            throw e;
                        }
                        LogUtil.Error("Error in Basescribe Service Requester:", e);
                        throw e;
                    }
                }catch (Exception e) {
                   LogUtil.Error("Error in Basescribe Service Requester:",e);
        //			if (reqMsgRef != null) {
        //				throw new EisException(e);
        //			}
                    throw new EisException(e);
                }finally {
                    if (connection != null)
                    {
                        connection.release();
                        //将连接返回到对象池        
                        getConnectionPoolManager().releaseConnection(connection);
                       
                    }
                }

            }


            public void closeConnection()
            {
                try
                {
                    this.getConnectionPoolManager().close();
                    provider = null;
                }
                catch (Exception e)
                {
                   LogUtil.Error("closePool:", e);
                }
            }
    }
}
