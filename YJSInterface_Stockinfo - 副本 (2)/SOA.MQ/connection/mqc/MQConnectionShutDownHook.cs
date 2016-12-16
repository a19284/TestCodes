using SOA.log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SOA.connection.mqc
{
    /// <summary>
    /// 连接关闭触发类  
    /// </summary>

class MQConnectionShutDownHook  {


    /// <summary>
    /// Runtime.getRuntime().addShutdownHook(shutdownHook)
    /// 这个方法的意思就是在jvm中增加一个关闭的钩子，当jvm关闭的时候，
    /// 会执行系统中已经设置的所有通过方法addShutdownHook添加的钩子，当系统执行完这些钩子后，jvm才会关闭。
    /// 所以这些钩子可以在jvm关闭的时候进行内存清理、对象销毁等操作
    /// </summary>
	private MQConnectionShutDownHook(){
		
	}
    
    //static {
    //    MQConnectionShutDownHook shutDownHook;
    //    try {
    //        shutDownHook = new MQConnectionShutDownHook();
    //        Runtime.getRuntime().addShutdownHook(shutDownHook);
    //    } catch (EisException e) {			
    //        logUtil.error("Exception:", e);
    //    }
		
    //}


	protected static HashSet<IConnection> connSet = new HashSet<IConnection>();
    private static readonly Object StaticLockObj = new object();
    public static void register(IConnection connection)
    {
        lock (StaticLockObj)
        {
            connSet.Add(connection);
        }
	}

    public void run() {
    	LogUtil.Info(this,"MQConnections are closing ...");
        Object[] conns = connSet.ToArray();
        for (int i = 0; i < conns.Length; i++) {
        	IConnection conn = (IConnection) conns[i];
        	try {
        		conn.release();
			} catch (Exception e) {
                LogUtil.Error("Exception:",e);
			}
        }
    }
}

}
