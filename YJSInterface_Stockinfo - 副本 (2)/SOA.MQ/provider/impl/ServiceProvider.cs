using SOA.common;
using SOA.config;
using SOA.handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace SOA.provider.impl
{
    public class ServiceProvider
    {

        private static ServiceProvider sp = null;
        private List<IServiceProvider> ProviderList = new List<IServiceProvider>();

            private ServiceProvider(){
                init();
            }

        	public  static ServiceProvider getInstance(){
        		if (sp == null){
        			sp = new ServiceProvider();
        		}
        		return sp;
        	}

            private ServiceProvider(IServiceHandler serviceHandler){
                //加入主MQ队列管理器监控
                ProviderList.Add(new BaseServiceProvider(serviceHandler));

                //加入备份MQ队列管理器监控
                Properties config = CacheManager.getInstance().getConfig();
                String AltConfig = config.getProperty(ConfigConstants.ALTERNATIVE_PROVIDER_CONFIG);
                if(AltConfig != null && !"".Equals(AltConfig.Trim())){
                    String[] alts = AltConfig.Split(char.Parse(ConfigConstants.ALTERNATIVE_CONFIG_SPLIT));
                    foreach (String alt in alts){
                        ProviderList.Add(new BaseServiceProvider(serviceHandler, alt));
                    }
                }

                init();
            }

            public  static ServiceProvider getInstance(IServiceHandler serviceHandler){
                if (sp == null){
                    sp = new ServiceProvider(serviceHandler);
                }
                return sp;
            }

            private void init(){
               // for(IServiceProvider Provider : ProviderList){
                foreach (IServiceProvider Provider in ProviderList)
                {
                    //获取提供方处理线程数
                    int providerThreadNum = int.Parse(CacheManager.getInstance()
                            .getConfig().getProperty(ConfigConstants.PROVIDER_HANDLER_MAXNUM));

                    //生成处理线程
                    for (int i = 0; i < providerThreadNum; i++) {
                       // new Thread(Provider).start();
                       //new Thread(new ThreadStart(Provider.start)).Start();
                        new Thread(new ThreadStart(Provider.start));
                   
                    }
                }
            }

            public void start() {
                 foreach (IServiceProvider Provider in ProviderList)
                 {
                    //启动处理线程
                    Provider.start();
                }
            }

            public void stop(){
                foreach (IServiceProvider Provider in ProviderList)
                {
                    Provider.closeConnection();
                    Provider.stop();
                }
            }

    }
}
