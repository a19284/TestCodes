using SOA.common;
using SOA.config;
using SOA.connection;
using SOA.connection.mqc;
using SOA.connection.mqcm;
using SOA.log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.requester.impl
{
    /// <summary>
    /// 抽象服务接口
    /// </summary>
    public abstract class AbstractServiceRequester : IServiceRequester
    {
         // private IConnectionPoolManager connectionPoolManager;
        //private List<IConnectionPoolManager> connectionPoolManagerList = new List<IConnectionPoolManager>();
        private MQPool connectionPoolManager;
        private List<MQPool> connectionPoolManagerList = new List<MQPool>();
        protected Properties config;
        protected List<MQParameter> reqList = new List<MQParameter>();
        protected List<MQParameter> resList = new List<MQParameter>();

        public System.Xml.XmlDocument execute(System.Xml.XmlDocument reqMo)
        {
            return null;
        }
      public abstract System.Xml.XmlDocument execute(System.Xml.XmlDocument reqMo, int timeout);
     
      public void closeConnection()
        { }

        public AbstractServiceRequester() {

		LogUtil.Info(this,"AbstractServiceRequester init Start()");
		
		this.config = CacheManager.getInstance().getConfig();
		
		//加载主MQ队列管理器
		MQParameter req = new MQParameter(config.getProperty(ConfigConstants.MQ_REQUESTER_REQ_IP), 
				int.Parse(config.getProperty(ConfigConstants.MQ_REQUESTER_REQ_PORT)),
				config.getProperty(ConfigConstants.MQ_REQUESTER_REQ_CHANNEL),
                int.Parse(config.getProperty(ConfigConstants.MQ_REQUESTER_REQ_CCSID)),
				config.getProperty(ConfigConstants.MQ_REQUESTER_REQ_QMANAGER),
				config.getProperty(ConfigConstants.MQ_REQUESTER_REQ_QUEUE));
		reqList.Add(req);
		
		MQParameter res = new MQParameter(config.getProperty(ConfigConstants.MQ_REQUESTER_RES_IP),
                int.Parse(config.getProperty(ConfigConstants.MQ_REQUESTER_RES_PORT)),
				config.getProperty(ConfigConstants.MQ_REQUESTER_RES_CHANNEL),
                int.Parse(config.getProperty(ConfigConstants.MQ_REQUESTER_RES_CCSID)),
				config.getProperty(ConfigConstants.MQ_REQUESTER_RES_QMANAGER),
				config.getProperty(ConfigConstants.MQ_REQUESTER_RES_QUEUE));
		resList.Add(res);
		//加入对象池管理
        connectionPoolManager = new MQPool(
				reqList,
				resList,
                int.Parse(config.getProperty(ConfigConstants.MQ_REQUESTER_POOL_MAXNUM)),
                int.Parse(config.getProperty(ConfigConstants.MQ_REQUESTER_GETCONN_TIMEOUT)));
		
		connectionPoolManagerList.Add(connectionPoolManager);
		
		
		//加载备用MQ队列管理器
		String AltConfig = config.getProperty(ConfigConstants.ALTERNATIVE_REQUESTER_CONFIG);
        if (AltConfig != null && AltConfig.Trim() != "")
        {
            String[] AltConfigArr = AltConfig.Split(char.Parse(ConfigConstants.ALTERNATIVE_CONFIG_SPLIT));


            for (int i = 0; i < AltConfigArr.Length; i++)
            {
                String alt = AltConfigArr[i] + ".";

                MQParameter req_t = new MQParameter(config.getProperty(alt + ConfigConstants.MQ_REQUESTER_REQ_IP),
                        int.Parse(config.getProperty(alt + ConfigConstants.MQ_REQUESTER_REQ_PORT)),
                        config.getProperty(alt + ConfigConstants.MQ_REQUESTER_REQ_CHANNEL),
                        int.Parse(config.getProperty(alt + ConfigConstants.MQ_REQUESTER_REQ_CCSID)),
                        config.getProperty(alt + ConfigConstants.MQ_REQUESTER_REQ_QMANAGER),
                        config.getProperty(alt + ConfigConstants.MQ_REQUESTER_REQ_QUEUE));

                List<MQParameter> reqList_t = new List<MQParameter>();
                reqList_t.Add(req_t);

                MQParameter res_t = new MQParameter(config.getProperty(alt + ConfigConstants.MQ_REQUESTER_RES_IP),
                        int.Parse(config.getProperty(alt + ConfigConstants.MQ_REQUESTER_RES_PORT)),
                        config.getProperty(alt + ConfigConstants.MQ_REQUESTER_RES_CHANNEL),
                        int.Parse(config.getProperty(alt + ConfigConstants.MQ_REQUESTER_RES_CCSID)),
                        config.getProperty(alt + ConfigConstants.MQ_REQUESTER_RES_QMANAGER),
                        config.getProperty(alt + ConfigConstants.MQ_REQUESTER_RES_QUEUE));

                List<MQParameter> resList_t = new List<MQParameter>();
                resList_t.Add(res_t);

                MQPool connectionPoolManager_t = new MQPool(
                        reqList_t,
                        resList_t,
                        int.Parse(config.getProperty(ConfigConstants.MQ_REQUESTER_POOL_MAXNUM)),
                        int.Parse(config.getProperty(ConfigConstants.MQ_REQUESTER_GETCONN_TIMEOUT)));

                connectionPoolManagerList.Add(connectionPoolManager_t);
            }
        }

        LogUtil.Info(this,"AbstractServiceRequester init End()");
	}

        public MQPool getConnectionPoolManager()
        {
            return connectionPoolManager;
        }

        public List<MQPool> getConnectionPoolManagerList()
        {
            return connectionPoolManagerList;
        }
    }
}
