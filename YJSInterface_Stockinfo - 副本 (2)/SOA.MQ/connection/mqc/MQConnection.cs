using IBM.WMQ;
using SOA.common;
using SOA.exception;
using SOA.log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.connection.mqc
{
    /// <summary>
    /// MQ实现服务连接
    /// </summary>
    public class MQConnection : IConnection
    {

        private MQCParameter cfgSendData = null;
        public MQCParameter cfgReceiveData = null;

        private int currentIndex = 0;
        private List<MQParameter> sendQList = new List<MQParameter>();
        private List<MQParameter> receiveQList = new List<MQParameter>();

        private Properties config;
        protected String encoding;

        /**
         * 将currentIndex值进行累加，如果超出范围则返回false否则返回true
         * @return 
         */
        private Boolean doNextCurrIndex()
        {
            if ((currentIndex + 1) >= receiveQList.Count())
            {
                currentIndex = 0;
                return false;
            }
            else
            {
                currentIndex++;
                return true;
            }
        }

        private Boolean doNextCurrMQC()
        {
            Boolean isT = doNextCurrIndex();
            setMQCpara();
            return isT;
        }

        /**
         * 返回当前发送队列的配置
         * @return
         */
        private MQParameter getSendQueuePara()
        {
            return sendQList[currentIndex];
        }

        /**
         * 返回当前接收队列配置
         * @return
         */
        private MQParameter getReceiveQueuePara()
        {
            return receiveQList[currentIndex];
        }

        /**
         * MQ连接配置初始化
         * @param sendQueue
         *            MQ config for send queue
         * @param receiveQueue
         *            MQ config for receive queue
         */
        public MQConnection(List<MQParameter> sendQList, List<MQParameter> receiveQList)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("Open MQConnection : ").Append(this);
            LogUtil.Debug(this,sb.ToString());
            sb = null;

            this.sendQList = sendQList;
            this.receiveQList = receiveQList;
            //		this.config = CacheManager.getInstance().getConfig();
            //		this.encoding = config.getProperty(ConfigConstants.ENCODING);
            this.encoding = "UTF-8";

            MQConnectionShutDownHook.register(this);
        }

        private void setMQCpara()
        {
            cfgSendData = new MQCParameter(getSendQueuePara());
            cfgReceiveData = new MQCParameter(getReceiveQueuePara());
        }

        /**
         * 进行MQ连接初始化
         * @return TRUE:连接成功；FALSE:连接失败
         */
        public Boolean initMQConnection()
        {
            Boolean ret = false;
            currentIndex = -1;
            while (doNextCurrMQC())
            {
                try
                {
                    if (cfgSendData != null)
                    {
                        // 接收队列连接参数
                        ret = MQQueueAccesser.connectMQ(cfgSendData, MQQueueAccesser.MQ_MSGSEND_TYPE);
                    }

                    if (cfgReceiveData != null)
                    {
                        // 发送队列连接参数
                        ret = MQQueueAccesser.connectMQ(cfgReceiveData, MQQueueAccesser.MQ_MSGRECEIVE_TYPE);
                    }

                    return ret;
                }
                catch (Exception e)
                {
                    LogUtil.Error(this, "MQ Client 初始化 Exception:" + e.Message);
                    if (cfgSendData != null)
                    {
                        cfgSendData.release();
                        cfgSendData = null;
                    }

                    if (cfgReceiveData != null)
                    {
                        cfgReceiveData.release();
                        cfgReceiveData = null;
                    }

                    ret = false;
                }
            }
            return ret;
        }

        public MQMsgRef receive(int timeout)
        {
            return receive(timeout, null);
        }

        public MQMsgRef receive()
        {
            return receive(0, null);
        }

            private MQMessage getMQMsg(MQMsgRef mqMsgRef,  int timeout) {
                MQMessage receiveMsg = null;
                try{
                    int i = 0;

                    while(true){
                        try{
                            if (cfgReceiveData.qManager == null || cfgReceiveData.queue == null) {
                                initMQConnection();
                            }

                            if (mqMsgRef == null || mqMsgRef.MQMsgId == null ||  mqMsgRef.MQMsgId.Length <= 0)
                                receiveMsg = MQQueueAccesser.getMsgFromQueue(null, cfgReceiveData, timeout);
                            else
                                receiveMsg = MQQueueAccesser.getMsgFromQueue(mqMsgRef.MQMsgId, cfgReceiveData, timeout);

                            return receiveMsg;
                        }catch(MQException mqe){
                            i ++;
                            int ret = MQQueueAccesser.handleMQException(mqe);
                            if (ret == -1){
                                Boolean isT = initMQConnection();
                                if (!isT)
                                    throw new EisException(MQCException.MQ_CONNECT_ERROR_EXCEPTION_CODE);

                            }else if (ret == 1){
                                throw mqe;
                            }

                            if (i > MQQueueAccesser.getConnGetMsgCount()){
                                throw mqe;
                            }
                        }
                    }
                }catch(MQException mqe){
                    int ret = MQQueueAccesser.handleMQException(mqe);

                    if (ret == 1) {
                        throw new EisException(MQCException.MQ_MSG_RECEIVE_GETMSG_TIMEOUT_EXCEPTION_CODE,
                                                MQCException.MQ_MSG_RECEIVE_GETMSG_TIMEOUT_EXCEPTION_DESC);
                    } else if (ret == -1) {
                        throw new EisException(	MQCException.MQ_CONNECT_ERROR_EXCEPTION_CODE);
                    } else{
                        throw new EisException(	MQCException.MQ_MSG_RECEIVE_GETMSG_ERROR_EXCEPTION_CODE	+ mqe);
                    }
                } catch (Exception e) {
                    throw new EisException(MQCException.MQ_MSG_RECEIVE_GETMSG_ERROR_EXCEPTION_CODE,MQCException.MQ_MSG_RECEIVE_GETMSG_ERROR_EXCEPTION_DESC);
                }


            }

        public MQMsgRef receive(int timeout, MQMsgRef mqMsgRef)
        {
            if (cfgReceiveData == null)
                throw new EisException("Fail to init receive queue.");

            MQMessage receiveMsg = getMQMsg(mqMsgRef, timeout);

            if (receiveMsg == null)
                throw new EisException("Fail to get mq msg.");

            byte[] response = MQQueueAccesser.getBytesFromMQMessage(receiveMsg);

            //		logUtil.info("replyToQueueManagerName::" + receiveMsg.replyToQueueManagerName);
            //		if (logUtil.isInfoEnabled()) {
            //			StringBuffer sb;
            //			try {
            //				sb = new StringBuffer().append("receive message:\n").append(new String(response, encoding));
            //				logUtil.info(sb.toString());
            //			} catch (UnsupportedEncodingException e) {
            //				logUtil.error("Exception:", e);
            //			}	
            //			sb = null;
            //		}

            MQMsgRef mQMsgRef = new MQMsgRef(receiveMsg.MessageId, response);
            return mQMsgRef;
        }


        public MQMsgRef send(MQMsgRef msg)
        {
            if (cfgSendData == null)
                throw new EisException("Fail to init send queue.");

            if (msg == null || msg.MQMsgBody == null)
                throw new EisException("Error in MQConnection : send message is null.");

            //		if (logUtil.isInfoEnabled()) {
            //			StringBuffer sb;
            //			try {
            //				sb = new StringBuffer().append("send message:\n").append(new String(msg.MQMsgBody, encoding));
            //				logUtil.info(sb.toString());
            //			} catch (UnsupportedEncodingException e) {
            //				logUtil.error("Exception:", e);
            //			}
            //			sb = null;
            //		}

            MQMessage sendMsg = MQQueueAccesser.getMQMessageFromBytes(msg.MQMsgBody);

            //设置消息id
            if (msg != null && msg.MQMsgId != null && msg.MQMsgId.Length > 0)
            {
                sendMsg.MessageId = msg.MQMsgId;
                sendMsg.CorrelationId = msg.MQMsgId;
            }

            //设置消息队列管理器
            if (msg != null && msg.ReplyToQMgr != null)
            {
                sendMsg.ReplyToQueueManagerName = msg.ReplyToQMgr;
                sendMsg.MessageType = MQC.MQMT_REPLY;
            }

            //		sendMsg.encoding = cfgSendData.mqParameter.getCcsid();
            sendMsg.CharacterSet = cfgSendData.mqParameter.getCcsid();
            //		sendMsg.format = MQC.MQFMT_STRING;

            Boolean ret = putMQMsg(sendMsg);
            if (!ret)
                throw new EisException("Fail to put mq msg.");

            MQMsgRef mQMsgRef = new MQMsgRef();
            mQMsgRef.MQMsgId = sendMsg.MessageId;

            return mQMsgRef;
        }

        private Boolean putMQMsg(MQMessage sendMsg)
        {
            try
            {
                int i = 0;

                //			while(true){
                //				try{
                //					if (cfgSendData.qManager == null || cfgSendData.queue == null) {
                //						initMQConnection();
                //					}
                //					return MQQueueAccesser.putMsgToQueue(cfgSendData, sendMsg);
                //				}catch(MQException mqe){
                //					i ++;
                //					int retInt = MQQueueAccesser.handleMQException(mqe);
                //					if (retInt == -1){
                //						boolean isT = initMQConnection();
                //						if (!isT)
                //							throw new EisException(MQCException.MQ_CONNECT_ERROR_EXCEPTION_CODE);
                //	
                //					}
                //					
                //					if (i > MQQueueAccesser.getConnPutMsgCount()){
                //						throw mqe;
                //					}
                //				}
                //			}

                while (true)
                {
                    try
                    {
                        if (cfgSendData.qManager == null || cfgSendData.queue == null)
                        {
                            initMQConnection();
                        }
                        return MQQueueAccesser.putMsgToQueue(cfgSendData, sendMsg);
                    }
                    catch (MQException mqe)
                    {
                        LogUtil.Error("putMQMsg error", mqe);
                        int retInt = MQQueueAccesser.handleMQException(mqe);
                        if (retInt == MQQueueAccesser.MQ_CONNECTION_BROKER || retInt == MQQueueAccesser.MQ_UNKNOW_EXCEPTION || retInt == MQQueueAccesser.MQ_CONNECTION_BROKEN_2009)
                        {
                           Boolean isT = initMQConnection();
                            if (!isT)
                                throw new EisException(MQCException.MQ_CONNECT_ERROR_EXCEPTION_CODE);
                        }
                        else
                        {
                            throw mqe;
                        }
                        //对于2009类型的错误因为消息已经发出，所以如果接收到此错误则只是初始化连接，但不重发消息
                        if (retInt == MQQueueAccesser.MQ_CONNECTION_BROKEN_2009)
                        {
                            throw mqe;
                        }
                        i++;
                        if (i > MQQueueAccesser.getConnPutMsgCount())
                        {
                            throw mqe;
                        }
                    }
                }
            }
            catch (MQException mqe)
            {
                int retInt = MQQueueAccesser.handleMQException(mqe);

                if (retInt == 1)
                {
                    throw new EisException(MQCException.MQ_MSG_SEND_PUTMSG_ERROR_EXCEPTION_CODE, MQCException.MQ_MSG_SEND_PUTMSG_ERROR_EXCEPTION_DESC);
                }
                else if (retInt == -1)
                {
                    throw new EisException(MQCException.MQ_CONNECT_ERROR_EXCEPTION_CODE, MQCException.MQ_CONNECT_ERROR_EXCEPTION_DESC);
                }
                else if (retInt == -2)
                {
                    throw new EisException(MQCException.MQ_MSG_SEND_PUTMSG_ERROR_EXCEPTION_CODE, MQCException.MQ_MSG_SEND_PUTMSG_ERROR_EXCEPTION_DESC + mqe);
                }
                else
                {
                    throw new EisException(MQCException.MQ_MSG_SEND_PUTMSG_ERROR_EXCEPTION_CODE,
                            MQCException.MQ_MSG_SEND_PUTMSG_ERROR_EXCEPTION_DESC
                                    + mqe);
                }
            }
            catch (Exception e)
            {
                throw new EisException(MQCException.MQ_MSG_SEND_PUTMSG_ERROR_EXCEPTION_CODE, MQCException.MQ_MSG_SEND_PUTMSG_ERROR_EXCEPTION_DESC);
            }
        }

        /**
         * 释放当前对象的MQ连接
         */
        public void release()
        {
            if (cfgSendData != null)
                cfgSendData.release();
            if (cfgReceiveData != null)
                cfgReceiveData.release();
            StringBuilder sb = new StringBuilder();
            sb.Append("Close MQConnection : ").Append(this);
            LogUtil.Info(this, sb.ToString());
            sb = null;

        }

        //    public void reset() {

        //    }

        /**
         * 验证当前连接是否有效
         * @return TRUE:正常连接；FALSE:连接失败
         */
        public Boolean valid()
        {
            try
            {
                //得到当前发送队列的深度
                int send=cfgSendData.queue.CurrentDepth;//getCurrentDepth
                //得到当前接收队列的深度
                int Receive = cfgReceiveData.queue.CurrentDepth;
               // cfgReceiveData.queue.getCurrentDepth();
                return true;
            }
            catch (Exception e)
            {
                LogUtil.Error(this, "Exception:" + e.Message);
                return false;
            }
        }

        public byte[] request(byte[] msg, int timeout)
        {

           return receive(timeout, send(new MQMsgRef(null, msg))).MQMsgBody;         

        }

        public byte[] request(byte[] msg)
        {
            //return receive(0, send(new MQMsgRef(null, msg))).MQMsgBody;
            return null;
        }


        //    public byte[] receiveBySegment(int timeout, byte[] grpid)
        //             {
        //        if (cfgReceiveData == null)
        //            throw new EisException("Fail to init receive queue.");
        //        if (grpid == null)
        //            throw new EisException("Error in MQConnection : group id is null.");

        //        byte[] msg = MQQueueAccesser.getMsgFromQueueBySegment(grpid, cfgReceiveData, MQQueueAccesser.MQ_MSGRECEIVE_TYPE, timeout);

        //        if (logUtil.isInfoEnabled()) {
        //            StringBuffer sb = new StringBuffer().append("message group id is :\n");	
        //            logUtil.info(sb.toString());
        //            sb = null;
        //            PublicPrint.printBuffer(grpid, grpid.length, PublicPrint.PrintType.DEBUG);				
        //            try {
        //                sb = new StringBuffer().append("receive message:\n").append(new String(msg, encoding));
        //                logUtil.info(sb.toString());
        //            } catch (UnsupportedEncodingException e) {
        //                logUtil.error("Exception:", e);
        //            }				
        //            sb = null;				
        //        }

        //        return msg;
        //    }

        //    public Boolean receiveBySegment(int timeout, byte[] grpid, String filename)  {
        //        if (cfgReceiveData == null)
        //            throw new EisException("Fail to init receive queue.");
        //        if (grpid == null)
        //            throw new EisException("Error in MQConnection : group id is null.");

        //        if (MQQueueAccesser.getMsgFromQueueBySegment(grpid, cfgReceiveData, 
        //                MQQueueAccesser.MQ_MSGRECEIVE_TYPE, timeout, filename)) {			
        //            StringBuffer sb = new StringBuffer();
        //            logUtil.info(sb.append("Write file (").append(filename).append(") successfully."));
        //            sb = null;
        //            return true;
        //        } else {
        //            StringBuffer sb = new StringBuffer();
        //            logUtil.info(sb.append("Fail to write file (").append(filename).append(")."));
        //            sb = null;
        //            return false;
        //        }
        //    }


        //    public byte[] sendBySegment(byte[] msg) throws EisException {
        //        if (cfgSendData == null)
        //            throw new EisException("Fail to init send queue.");

        //        if (msg == null)
        //            throw new EisException("Error in MQConnection : send message is null.");

        //        if (logUtil.isInfoEnabled()) {
        //            StringBuffer sb;
        //            try {
        //                sb = new StringBuffer().append("send message:\n").append(new String(msg, encoding));
        //                logUtil.info(sb.toString());
        //            } catch (UnsupportedEncodingException e) {
        //                logUtil.error("Exception:", e);
        //            }				
        //            sb = null;
        //        }

        //        byte[] grpid = MQQueueAccesser.putMsgToQueueBySegment(cfgSendData, msg,
        //                MQQueueAccesser.MQ_MSGSEND_TYPE);
        //        if (grpid == null)
        //            throw new EisException("Fail to put mq msg.");
        //        return grpid;
        //    }

        //    public byte[] sendBySegment(String filename) throws EisException {
        //        if (cfgSendData == null)
        //            throw new EisException("Fail to init send queue.");

        //        if (filename == null)
        //            throw new EisException("Error in MQConnection : file name is null.");

        //        if (logUtil.isInfoEnabled()) {
        //            StringBuffer sb = new StringBuffer().append("send message:\n").append(filename);
        //            logUtil.info(sb.toString());
        //            sb = null;
        //        }

        //        byte[] grpid = MQQueueAccesser.putMsgToQueueBySegment(cfgSendData, filename,
        //                MQQueueAccesser.MQ_MSGSEND_TYPE);
        //        if (grpid == null)
        //            throw new EisException("Fail to put mq msg.");
        //        return grpid;
        //    }

        //    }
    }
}
