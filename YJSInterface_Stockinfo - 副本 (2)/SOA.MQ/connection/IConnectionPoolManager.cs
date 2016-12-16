using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.connection
{
    /// <summary>
    /// 连接池管理接口
    /// </summary>
  
      public  interface IConnectionPoolManager : IConnectionManager
        {
            /**
             * close pool
             */
             void close();
        }
   
}

