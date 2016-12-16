using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.connection.mqcm
{
    public class  MQConnectionPool:ObjectPool_bak
    {
  
        //public static object[] cArgs=new object[1];
        //new MQConnectionPoolFactory(manager)

        public MQConnectionPool(Type objType, object[] cArgs, int minNum, int maxNum)
            : base(typeof(MQConnectionPoolFactory), cArgs, minNum, maxNum)
        {
                        // cArgs.SetValue(new MQConnectionPoolFactory(manager), 1);
        
                       
            
        }
    }
}
