using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SOA.connection.mqc;
using SOA.connection;
using SOA.handler;
using SOA.exception;
using SOA.config;
using SOA.common;
using SOA.log;
using SOA.connection.mqcm;
using System.Threading;

namespace SOA.provider.impl
{
    /// <summary>
    /// 抽象 服务提供方接口
    /// </summary>
  public abstract class AbstractServiceProvider : IServiceProvider
  {

      private static readonly int STATUS_PAUSE = 0;
      private static readonly int STATUS_START = 1;
      private static readonly int STATUS_STOP = 2;

      protected IServiceHandler handler = null;
      private IConnectionPoolManager connectionPoolManager;  
      private int status = STATUS_PAUSE;
      private Properties config;
      private Boolean isT = true;
      protected List<MQParameter> receiveList = new List<MQParameter>();
      protected List<MQParameter> sendList = new List<MQParameter>();

      public void closeConnection()
      { }
	
    /**
     * 加载主MQ配置
     * @throws EisException
     */
    public AbstractServiceProvider(){
     
        this.config = CacheManager.getInstance().getConfig();
		
//		String AltConfig = config.getProperty(ConfigConstants.ALTERNATIVE_PROVIDER_CONFIG);
//		if (AltConfig == null){
//			AltConfig = "";
//		}
//		String[] AltConfigArr = AltConfig.split(ConfigConstants.ALTERNATIVE_CONFIG_SPLIT);
		
		
//		List<MQParameter> receiveList = new ArrayList<MQParameter>();
		
        MQParameter receive = new MQParameter(config.getProperty(ConfigConstants.MQ_PROVIDER_RECV_IP),
                int.Parse(config.getProperty(ConfigConstants.MQ_PROVIDER_RECV_PORT)),
                config.getProperty(ConfigConstants.MQ_PROVIDER_RECV_CHANNEL),
                int.Parse(config.getProperty(ConfigConstants.MQ_PROVIDER_RECV_CCSID)),
                config.getProperty(ConfigConstants.MQ_PROVIDER_RECV_QMANAGER),
                config.getProperty(ConfigConstants.MQ_PROVIDER_RECV_QUEUE));
        receiveList.Add(receive);
		
//		for (int i = 0; i < AltConfigArr.length; i++) {
//			String receiveAlt = AltConfigArr[i].trim();
//			MQParameter receive_t = new MQParameter(config.getProperty(receiveAlt + ConfigConstants.MQ_PROVIDER_RECV_IP),
//					Integer.parseInt(config.getProperty(receiveAlt + ConfigConstants.MQ_PROVIDER_RECV_PORT)),
//					config.getProperty(receiveAlt + ConfigConstants.MQ_PROVIDER_RECV_CHANNEL),
//					Integer.parseInt(config.getProperty(receiveAlt + ConfigConstants.MQ_PROVIDER_RECV_CCSID)),
//					config.getProperty(receiveAlt + ConfigConstants.MQ_PROVIDER_RECV_QMANAGER),
//					config.getProperty(receiveAlt + ConfigConstants.MQ_PROVIDER_RECV_QUEUE));
//			
//			receiveList.add(receive_t);
//		}
		
		
//		List<MQParameter> sendList = new ArrayList<MQParameter>();
		
        MQParameter send = new MQParameter(config.getProperty(ConfigConstants.MQ_PROVIDER_SEND_IP),
                int.Parse(config.getProperty(ConfigConstants.MQ_PROVIDER_SEND_PORT)),
                config.getProperty(ConfigConstants.MQ_PROVIDER_SEND_CHANNEL),
                int.Parse(config.getProperty(ConfigConstants.MQ_PROVIDER_SEND_CCSID)),
                config.getProperty(ConfigConstants.MQ_PROVIDER_SEND_QMANAGER),
                config.getProperty(ConfigConstants.MQ_PROVIDER_SEND_QUEUE));
        sendList.Add(send);
		
//		for (int i = 0; i < AltConfigArr.length; i++) {
//			String sendAlt = AltConfigArr[i].trim();
//			MQParameter send_t = new MQParameter(config.getProperty(sendAlt + ConfigConstants.MQ_PROVIDER_SEND_IP),
//					Integer.parseInt(config.getProperty(sendAlt + ConfigConstants.MQ_PROVIDER_SEND_PORT)),
//					config.getProperty(sendAlt + ConfigConstants.MQ_PROVIDER_SEND_CHANNEL),
//					Integer.parseInt(config.getProperty(sendAlt + ConfigConstants.MQ_PROVIDER_SEND_CCSID)),
//					config.getProperty(sendAlt + ConfigConstants.MQ_PROVIDER_SEND_QMANAGER),
//					config.getProperty(sendAlt + ConfigConstants.MQ_PROVIDER_SEND_QUEUE));
//			sendList.add(send_t);
//		}
		
		
        connectionPoolManager = new PoolableMQConnectionManager(sendList, receiveList,
                int.Parse(config.getProperty(ConfigConstants.MQ_PROVIDER_POOL_MAXNUM)),
                int.Parse(config.getProperty(ConfigConstants.MQ_PROVIDER_GETCONN_TIMEOUT)));
		
        ProviderShutDownHook.register(this);
    }
	
    /**
     * 加载备份MQ配置
     * @throws EisException
     */
    public AbstractServiceProvider(String alt){
     
        this.config = CacheManager.getInstance().getConfig();
        alt = alt + ".";
		
        MQParameter receive = new MQParameter(config.getProperty(alt + ConfigConstants.MQ_PROVIDER_RECV_IP),
                int.Parse(config.getProperty(alt + ConfigConstants.MQ_PROVIDER_RECV_PORT)),
                config.getProperty(alt + ConfigConstants.MQ_PROVIDER_RECV_CHANNEL),
                int.Parse(config.getProperty(alt + ConfigConstants.MQ_PROVIDER_RECV_CCSID)),
                config.getProperty(alt + ConfigConstants.MQ_PROVIDER_RECV_QMANAGER),
                config.getProperty(alt + ConfigConstants.MQ_PROVIDER_RECV_QUEUE));
        receiveList.Add(receive);
		
        MQParameter send = new MQParameter(config.getProperty(alt + ConfigConstants.MQ_PROVIDER_SEND_IP),
                int.Parse(config.getProperty(alt + ConfigConstants.MQ_PROVIDER_SEND_PORT)),
                config.getProperty(alt + ConfigConstants.MQ_PROVIDER_SEND_CHANNEL),
               int.Parse(config.getProperty(alt + ConfigConstants.MQ_PROVIDER_SEND_CCSID)),
                config.getProperty(alt + ConfigConstants.MQ_PROVIDER_SEND_QMANAGER),
                config.getProperty(alt + ConfigConstants.MQ_PROVIDER_SEND_QUEUE));
        sendList.Add(send);
		
        connectionPoolManager = new PoolableMQConnectionManager(sendList, receiveList,
               int.Parse(config.getProperty(ConfigConstants.MQ_PROVIDER_POOL_MAXNUM)),
               int.Parse(config.getProperty(ConfigConstants.MQ_PROVIDER_GETCONN_TIMEOUT)));
		
        ProviderShutDownHook.register(this);
    }



    public static int MQ_PROVIDER_RECV_TIMEOUT = 180;

    /**
     * 返回获取数据超时时间
     * @return
     */
    public int getProviderRecvTimeOut()
    {
        String TimeOut;
        try
        {
            TimeOut = CacheManager.getInstance().getConfig().getProperty(ConfigConstants.MQ_PROVIDER_RECV_TIMEOUT);
        }
        catch (EisException e)
        {
            LogUtil.Error("Exception", e);
            return MQ_PROVIDER_RECV_TIMEOUT;
        }
        if (null == TimeOut)
            return MQ_PROVIDER_RECV_TIMEOUT;
        return int.Parse(TimeOut);
    }


      public void pause()
      {
          status = STATUS_PAUSE;
      }


      public void setServiceHandler(IServiceHandler serviceHandler)
      {
          if (this.handler == null)
          {
              this.handler = serviceHandler;
          }
      }


    public void start() {
        initHandler();
        isT = true;
        status = STATUS_START;

        run();
    }
	
    /**
     * 初始化Handler实例
     * @throws EisException
     */
    private void initHandler(){
        String className = config.getProperty(ConfigConstants.PROVIDER_HANDLER_CLASSNAME);
        try {
            
            if (handler == null){
                //动态创建此类对象下面这样写就行了
                //Type t = Type.GetType(“TestSpace.TestClass”);
                //Object[] constructParms = new object[] {“hello”}; //构造器参数
                //TestClass obj = (TestClass)Activator.CreateInstance(t,constructParms);

                //如果类的构造器是无参数的，就调用这个
                //TestClass obj = (TestClass)Activator.CreateInstance(t);


                Type t = Type.GetType(className);
                this.handler = (IServiceHandler)Activator.CreateInstance(t);


                //this.handler = (IServiceHandler)Class.forName(className).newInstance();
            }
        } catch (Exception e) {
            StringBuilder sb = new StringBuilder().Append("class(").Append(className).Append(") initialization failed!");
            LogUtil.Error(sb.ToString(),e);
            sb = null;
            throw new EisException(e);
        }
    }

//    @Override
    public void stop()
    {
        status = STATUS_STOP;
        isT = false;
    }


    public void run()
    {
        bool loop_out = false;    //增加循环跳出标示位
        while (isT)
        {
            switch (status)
            {
                case 0://暂停
                    try
                    {
                        Thread.Sleep(1000 * 1);
                    }
                    catch (Exception e)
                    {
                        LogUtil.Error("Error in pause status.", e);
                    }
                    break;
                case 1://开始
                    try
                    {
                        process();
                       // loop_out = true;
                    }
                    catch (EisException e)
                    {
                        LogUtil.Error("Error in Service Provider:\n", e);
                        handler.handleException(e);
                        resetConnectionPoolManager();
                    }
                    break;
                case 2://停止
                    StringBuilder sb = new StringBuilder().Append("Provider[").Append(this).Append("] is stopped.");
                    LogUtil.Info(this, sb.ToString());
                    sb = null;
                    loop_out = true;
                    break;
                default:
                    break;
            }
            if (loop_out)
            {
                break;
            }
        }
    }

    /**
     * 重置连接池
     */
    protected void resetConnectionPoolManager()
    {
        try
        {
            connectionPoolManager.close();
            //			receiveList.remove(0);
            if (receiveList.Count > 0)
            {
                connectionPoolManager = new PoolableMQConnectionManager(sendList, receiveList,
                        int.Parse(config.getProperty(ConfigConstants.MQ_PROVIDER_POOL_MAXNUM)),
                        int.Parse(config.getProperty(ConfigConstants.MQ_PROVIDER_GETCONN_TIMEOUT)));
                LogUtil.Info(this, "已重置网络连接！");
            }
            else
            {
                stop();
            }
        }
        catch (Exception e)
        {
            LogUtil.Error(this, e.Message); 
        }
    }

    protected abstract void process();

	
    public IConnectionPoolManager getConnectionPoolManager() {
        return connectionPoolManager;
    }

    public Properties getConfig() {
        return config;
    }
	
}

}
