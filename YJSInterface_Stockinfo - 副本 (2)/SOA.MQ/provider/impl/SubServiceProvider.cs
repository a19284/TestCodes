using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.provider.impl
{
    public class SubServiceProvider //: AbstractServiceProvider, IServiceProvider
    {

        private static IServiceProvider provider = null;

        //public synchronized static IServiceProvider getInstance() throws EisException {
        //    if (null == provider) {
        //        provider = new SubServiceProvider();
        //    }
        //    return provider;
        //}

        //private SubServiceProvider() throws EisException {
        //    super();
        //}


        //protected void process() throws EisException {
        //    IConnection connection = null;
        //    MQMsgRef reqMsgRef = null;
        //    try {
        //        connection = getConnectionPoolManager().getConnection();
        //        reqMsgRef = connection.receive();

        //        IMsgObject reqMo = new MsgObject(reqMsgRef.MQMsgBody,IMsgObject.MOType.initSP);
        //        handler.execute(reqMo);

        //    } catch (EisException e) {
        //        // reqMsg is null when timeout waiting for request message
        //        logUtil.error("Error in Subscribe Service Requester:",e);
        //        if (reqMsgRef != null) {
        //            throw e;
        //        }
        //    }catch(Exception e){
        //        logUtil.error("Error in Subscribe Service Requester:",e);
        //        if (reqMsgRef != null) {
        //            throw new EisException(e);
        //        }
        //    }finally {
        //        if (connection != null)
        //            getConnectionPoolManager().releaseConnection(connection);
        //    }

        //}

        //public void closeConnection() {
        //    try{
        //        this.getConnectionPoolManager().close();
        //        provider = null;
        //    }catch(Exception e){
        //        logUtil.error("closePool:", e);
        //    }
        //}
    }
}
