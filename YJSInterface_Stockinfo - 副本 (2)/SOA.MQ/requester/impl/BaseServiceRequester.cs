using SOA.config;
using SOA.connection;
using SOA.connection.mqcm;
using SOA.log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace SOA.requester.impl
{
    /// <summary>
    /// 服务请求
    /// </summary>
    public class BaseServiceRequester : AbstractServiceRequester, IServiceRequester
    {
       
        //MQ队列管理器数量
        private int connectionPoolManagerSize = 0;
        public System.Xml.XmlDocument execute(System.Xml.XmlDocument reqMo)
        {
            return execute(reqMo,0);
        }

        public override System.Xml.XmlDocument execute(System.Xml.XmlDocument m_xml, int timeout)
        {
            IConnection connection = null;
            SOA.message.Request.Service resMo = null;
            connectionPoolManagerSize = getConnectionPoolManagerList().Count;
            List<SOA.connection.mqcm.MQPool> connectionPoolManager = getConnectionPoolManagerList();
            for (int i = 0; i < connectionPoolManagerSize; i++)
            {
                try
                {

                     byte[] reqMs,resMsg=null;

                    connection = getConnection(i);              
                    if (i == 0)
                    {
                        m_xml = setMAC(m_xml);
                    }
                    //字符编码
                    using (MemoryStream ms = new MemoryStream())
                    {
                        m_xml.Save(ms);
                        reqMs = ms.ToArray();
                    }

                
                    if (((SOA.connection.mqc.MQConnection)(connection)).cfgReceiveData.qManager != null || ((SOA.connection.mqc.MQConnection)(connection)).cfgReceiveData.queue!=null)
                    {
                        resMsg = connection.request(reqMs, timeout);
                    }
                    if (resMsg != null)
                    {
                        string xml = System.Text.Encoding.UTF8.GetString(resMsg);
                        //创建XmlDocument对象
                        XmlDocument xmlDocument = new XmlDocument();
                        //加载XML文件
                         xmlDocument.LoadXml(xml);
                         return xmlDocument;
                    }
                }
                catch (TimeoutException te)
                {
                    LogUtil.Error(this, "第" + (i + 1) + "个MQ连接超时！" + te);
                }
                catch (SocketException se)
                {
                    LogUtil.Error(this,"获取客户端MAC失败！"+se);
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.release();
                        //将连接返回到对象池        
                        getConnectionPoolManagerList()[i].FreeConnection(connection);
                        connection = null;
                    }
                       
                }
            }

            //for (int i = 0; i < connectionPoolManagerSize; i++)
            //{
            //    try
            //    {
            //        connection = getConnection(i);
            //        setMAC(reqMo);
            //        byte[] reqMsg = reqMo.getBytes();
            //        byte[] resMsg = connection.request(reqMsg, timeout);
            //        resMo = new MsgObject(resMsg, IMsgObject.MOType.initSR);
            //    }
            //    catch (TimeoutException te)
            //    {
            //        logUtil.error("第" + (i + 1) + "个MQ连接超时！", te);
            //        if (i < connectionPoolManagerSize - 1)
            //        {
            //            continue;
            //        }
            //        else
            //        {
            //            throw new EisException(te);
            //        }
            //    }
            //    catch (SocketException se)
            //    {
            //        logUtil.error("获取客户端MAC失败！", se);
            //        if (i < connectionPoolManagerSize - 1)
            //        {
            //            continue;
            //        }
            //        else
            //        {
            //            throw new EisException(se);
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        String errorMsg = e.getMessage();
            //        if (errorMsg.contains("Failed to init connection.") || errorMsg.contains("Fail to get connection."))
            //        {
            //            logUtil.error("第" + (i + 1) + "个MQ获取连接失败！", e);
            //        }
            //        else if (errorMsg.contains("MQ Msg Send Put Msg Error Exception"))
            //        {
            //            logUtil.error("第" + (i + 1) + "个MQ发送数据失败！", e);
            //        }
            //        else
            //        {
            //            logUtil.error("Error in Base Service Requester:", e);
            //            throw new EisException(e);
            //        }

            //        if (i < connectionPoolManagerSize - 1)
            //        {
            //            continue;
            //        }
            //        else
            //        {
            //            throw new EisException(e);
            //        }
            //    }
            //    finally
            //    {
            //        if (connection != null)
            //        {
            //            connection.release();
            //            getConnectionPoolManagerList().get(i).releaseConnection(connection);
            //            connection = null;
            //        }
            //    }

            //    if (resMo != null)
            //    {
            //        break;
            //    }
            //}

            //try
            //{
            //    connection = getConnection(connectionPoolManagerIndex);

            //    byte[] reqMsg = ((IMsgObject)reqMo).getBytes();

            //    byte[] resMsg = connection.request(reqMsg, timeout);

            //    resMo = new MsgObject(resMsg, IMsgObject.MOType.initSR);
            //}
            //catch (EisException e)
            //{
            //    logUtil.error("Error in Base Service Requester:", e);
            //    //当放入消息失败时，重置主MQ连接
            //    if ("EEE2200063:MQ Msg Send Put Msg Error Exception".equals(e.getMessage()))
            //    {
            //        resetConnectionPoolManager();
            //    }
            //    throw e;
            //}
            //catch (Exception e)
            //{
            //    logUtil.error("Error in Base Service Requester:", e);
            //    throw new EisException(e);
            //}
            //finally
            //{
            //    if (connection != null)
            //    {
            //        connection.release();
            //        getConnectionPoolManagerList().get(connectionPoolManagerIndex).releaseConnection(connection);
            //        connection = null;
            //    }
            //}

            return null;
        }
        public void closeConnection()
        { }

//获取MQ连接，若超过设定时间强行中断进程
    public IConnection getConnection1(int i){
        IConnection connection = null;
        connection = getConnectionPoolManagerList()[i].GetConnection();

         // ExecutorService executor = Executors.newFixedThreadPool(1);
        //Future<IConnection> future = executor.submit(new Callable<IConnection>(){
           
        //    public IConnection call()  {
        //        return BaseServiceRequester.getInstance().getConnectionPoolManagerList().get(i).getConnection();
        //    }
        //});
		
        try {
           // connection = future.get(Long.valueOf(config.getProperty(ConfigConstants.MQ_REQUESTER_LINK_TIMEOUT)), TimeUnit.MILLISECONDS);
        } catch(TimeoutException te) {
//			System.out.println("连接第" + (connectionPoolManagerIndex + 1) + "个MQ队列管理器失败！");
//			if(connectionPoolManagerIndex + 1 < connectionPoolManagerSize){
//				connectionPoolManagerIndex++;
//				System.out.println("开始连接第"+ (connectionPoolManagerIndex + 1) + "个MQ队列管理器……");
//				connection = getConnection(connectionPoolManagerIndex);
//			}
            throw(te);
        } finally {
           // executor.shutdown();
        }
		
        return connection;
    }


    static void CallWithTimeout(Action action, int timeoutMilliseconds)
    {
        Thread threadToKill = null;
        Action wrappedAction = () =>
        {
            threadToKill = Thread.CurrentThread;
            action();
        };

        IAsyncResult result = wrappedAction.BeginInvoke(null, null);
        if (result.AsyncWaitHandle.WaitOne(timeoutMilliseconds))
        {
            wrappedAction.EndInvoke(result);
        }
        else
        {
            threadToKill.Abort();
            throw new TimeoutException();
        }
    }
        /// <summary>
        /// 启动一个线程，判断是否超时
        /// </summary>
        /// <param name="action">函数</param>
        /// <param name="timeoutMilliseconds">时间</param>
        /// <param name="i">参数</param>
        /// <returns></returns>
    static IConnection CallWithTimeout(Func<int, IConnection> action, int timeoutMilliseconds, int i)
    {
        IConnection connection = null;
        Thread threadToKill = null;
        Func<int, IConnection> wrappedAction = (j) =>
        {
            threadToKill = Thread.CurrentThread;
            return action(j);            
        };

        IAsyncResult result = wrappedAction.BeginInvoke(i,null, null);
        if (result.AsyncWaitHandle.WaitOne(timeoutMilliseconds))
        {
            connection=wrappedAction.EndInvoke(result);
        }
        else
        {
            connection = wrappedAction.EndInvoke(result);
          
            //throw new TimeoutException();
        }
        threadToKill.Abort();
        return connection;
    }

    public IConnection Getcon(int i)
    {
      
       IConnection ic= getConnectionPoolManagerList()[i].GetConnection();
        return ic;
    }

    //public IConnection getConnection2(int i)
    //{
    //    IConnection connection = null;

    //    Thread timingbeginApp = new Thread(new ParameterizedThreadStart(Getcon));
    //    parameter p = new parameter();
    //    p.i = i;
    //    try
    //    {
    //        timingbeginApp.Start(p);
    //        timingbeginApp.Join(int.Parse(config.getProperty(ConfigConstants.MQ_REQUESTER_LINK_TIMEOUT)));


    //        if (((SOA.connection.mqc.MQConnection)(p.returnVaule)).cfgReceiveData.qManager != null || ((SOA.connection.mqc.MQConnection)(p.returnVaule)).cfgReceiveData.queue != null)
    //        {
    //            connection = p.returnVaule;
    //        }

    //    }
    //    catch (TimeoutException te)
    //    {
    //        //			System.out.println("连接第" + (connectionPoolManagerIndex + 1) + "个MQ队列管理器失败！");
    //        //			if(connectionPoolManagerIndex + 1 < connectionPoolManagerSize){
    //        //				connectionPoolManagerIndex++;
    //        //				System.out.println("开始连接第"+ (connectionPoolManagerIndex + 1) + "个MQ队列管理器……");
    //        //				connection = getConnection(connectionPoolManagerIndex);
    //        //			}
    //        throw (te);
    //    }
    //    finally
    //    {
    //        // executor.shutdown();
    //        timingbeginApp.Abort();
    //    }

    //    return connection;
    //}

    public delegate IConnection MethodCaller(int i);//定义个代理 

    public IConnection getConnection(int i)
    {
        IConnection connection = null;
    
        try
        {
           connection = getConnectionPoolManagerList()[i].GetConnection();
           //connection= CallWithTimeout(Getcon, int.Parse(config.getProperty(ConfigConstants.MQ_REQUESTER_LINK_TIMEOUT)), i);
        }
        catch (TimeoutException te)
        {
            //			System.out.println("连接第" + (connectionPoolManagerIndex + 1) + "个MQ队列管理器失败！");
            //			if(connectionPoolManagerIndex + 1 < connectionPoolManagerSize){
            //				connectionPoolManagerIndex++;
            //				System.out.println("开始连接第"+ (connectionPoolManagerIndex + 1) + "个MQ队列管理器……");
            //				connection = getConnection(connectionPoolManagerIndex);
            //			}
            throw (te);
        }
        finally
        {
            // executor.shutdown();
            //timingbeginApp.Abort();

        }

        return connection;
    }


        /**
 * 在报文中设置请求方MAC
 * @param reqMo
 * @throws SocketException 
 */
        //public void setMAC(SOA.message.Request.Service reqMo)
        //{
        //    //由于一台电脑中可能会有多个网卡，故把所有的MAC都统计进来，避免漏掉真实在用的MAC
        //    StringBuilder macs = new StringBuilder();

        //    NetworkInterface[] niEnum = NetworkInterface.GetAllNetworkInterfaces();
        //    foreach (NetworkInterface ni in niEnum)
        //    {

        //        if (ni.GetPhysicalAddress().ToString() != "")
        //        {
        //            macs.Append(ni.GetPhysicalAddress().ToString());
        //        }


        //    }
        //    //reqMo.Route.MAC = macs.ToString();
        //    //((SOA.message.Request.Service)reqMo).setHeaderAttribute("MAC", macs.toString());
        //}
        public System.Xml.XmlDocument setMAC(System.Xml.XmlDocument m_xml)
        {
            //由于一台电脑中可能会有多个网卡，故把所有的MAC都统计进来，避免漏掉真实在用的MAC
            StringBuilder macs = new StringBuilder();

            NetworkInterface[] niEnum = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in niEnum)
            {
                //选出“Up”的网卡，并且排除掉环回接口
                if (ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback && ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                {
                    StringBuilder sbacs = new StringBuilder();
                    string macAddress = ni.GetPhysicalAddress().ToString();

                    char[] charArr2 = new char[macAddress.Length];
                    charArr2 = macAddress.ToString().ToCharArray();
                    for (int i = 0; i < charArr2.Length; i++)
                    {
                        sbacs.Append(charArr2[i]);
                        if (Convert.ToBoolean(i % 2))
                        {
                            sbacs.Append("-");
                        }
                    }
                    if (sbacs.Length > 0)
                    {
                        macs.Append(sbacs.ToString().Substring(0, sbacs.Length - 1));
                        macs.Append(",");
                    }

                }

            }
            string strmac = macs.ToString();
            if (macs.Length > 0)
            {
                 strmac = macs.ToString().Substring(0, macs.Length - 1);
            }
        
            //为XML的根节点赋值      

           return SOA.message.XmlHelper.InsertNode(m_xml, "/Service/Route", "MAC", strmac);
            //reqMo.Route.MAC = macs.ToString();
            //((SOA.message.Request.Service)reqMo).setHeaderAttribute("MAC", macs.toString());
        }



    }
}
