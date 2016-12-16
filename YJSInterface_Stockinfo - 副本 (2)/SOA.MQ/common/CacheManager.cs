using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.common
{
    /// <summary>
    /// 配置缓存管理类
    /// </summary>
    public  class CacheManager
    {
        private static CacheManager cm = null;
        private Properties config = new Properties();
 

        private CacheManager() {

        }
        public static CacheManager getInstance()
        {
            if (cm == null)
            {
                cm = new CacheManager();
            }
            return cm;
        }

        public Properties getConfig()
        {
            return config;
        }
    }
}
