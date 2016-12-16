using IBM.WMQ;
using SOA.common;
using SOA.config;
using SOA.exception;
using SOA.log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;


namespace SOA.connection.mqc
{
    /// <summary>
    ///  针对MQ操作的工具类
    /// </summary>
    public class MQQueueAccesser
    {

    public static readonly int MQ_OK = 0;

	public static readonly int MQ_CONNECTION_BROKER = -1;
	
	public static readonly int MQ_CONNECTION_BROKEN_2009 = -2;

	public static readonly int MQ_TIME_OUT = 1;

	public static readonly int MQ_UNKNOW_EXCEPTION = 2;
	
	/**
	 * MQ消息接收类型
	 */
	public static readonly int MQ_MSGRECEIVE_TYPE = 0;
	
	/**
	 * MQ消息发送类型
	 */
	public static readonly int MQ_MSGSEND_TYPE = 1;
	
	/**
	 * MQ连接异常，重连次数默认值
	 */
	public static readonly int MQ_TRY_COUNT = 3;
	/**
	 * 接收消息异常，重试次数默认值
	 */
	public static readonly int MQ_CONN_GETMSG_COUNT = 3;
	/**
	 * 发送消息异常，重试次数默认值
	 */
	public static readonly int MQ_CONN_PUTMSG_COUNT = 3;
	
	/**
	 * 每次重连的等待时间，默认值(毫秒)
	 */
	public static readonly int MQ_TRY_INTERVAL = 2000;
	
	public static readonly int MQ_SEGMENT_LEN = 5000;
	
	/**
	 * 消息有效时间,(1/10秒)
	 */
	public static readonly int MQ_MSG_EXPIRY = 6000;
	
	public static readonly int MQ_REQ_MSG_TIMEOUT = 5 * 60;
	
	public static readonly int MQ_SUB_MSG_TIMEOUT = 5 * 60;

	public static  MQPutMessageOptions getPMOFromPara() {
		MQPutMessageOptions pmo = new MQPutMessageOptions();
		pmo.Options |= MQC.MQPMO_FAIL_IF_QUIESCING;
		return pmo;
	}


        /**
	 * 连接到MQ队列管理器
	 * @param para 
	 * 			调用类的MQ连接配置参数
	 * @param type
	 * 			连接类型，0－接收该Queue队列的消息，1－向该Queue队列发送消息
	 * @return TRUE:连接成功；
	 * @throws EisException 连接失败则抛出异常
	 */
        public static Boolean connectMQ(MQCParameter para, int type)
        {
            try
            {
                int interval = getInterval();
                int trycount = getCount();
                return connectMQ(para, type, trycount, interval);
            }
            catch (Exception e)
            {
                throw new EisException("不能连接到MQ Server :" + para.mqParameter.toString() + "]");
            }

        }

        public static Boolean connectMQExce(MQCParameter para, int type)
        {
            int interval = getInterval();
            int trycount = getCount();
            return connectMQ(para, type, trycount, interval);
        }

     /**
	 * 连接到MQ队列管理器
	 * @param para 连接对象
	 * @param type 连接类型（发送，接收）
	 * @param trycount 重试连接次数
	 * @param interval 重连等待时间
	 * @return TRUE:连接成功；FALSE:连接失败;
	 * @throws EisException 当重试连接次数都失败，则返回异常
	 */
        public static Boolean connectMQ(MQCParameter para, int type, int trycount, int interval)
        {

            LogUtil.Info("try to connect mq :", para.mqParameter.toString());
            Boolean ret = false;
            MQParameter mqParameter = para.mqParameter;
            Hashtable props = new Hashtable();
            if (mqParameter.getHostName() != null)
                props.Add(MQC.HOST_NAME_PROPERTY, mqParameter.getHostName());
            if (mqParameter.getPort() != 0)
                props.Add(MQC.PORT_PROPERTY, mqParameter.getPort());
            if (mqParameter.getChannel() != null)
                props.Add(MQC.CHANNEL_PROPERTY, mqParameter.getChannel());
            if (mqParameter.getCcsid() != 0)
                props.Add(MQC.CCSID_PROPERTY, mqParameter.getCcsid());
            // MQPoolToken token=MQEnvironment.addConnectionPoolToken();

            int i = 0;

            MQQueue queue = null;
            MQQueueManager qManager = null;
            while (trycount <= 0 || (trycount > 0 && i < trycount))
            {
                i++;
                try
                {
                    para.release();

                    //连接到指定的队列管理器
                    qManager = new MQQueueManager(mqParameter.getQManagerName(), props);

                    //根据参数不同连接到Q队列上
                    if (MQ_MSGRECEIVE_TYPE == type)
                    {
                        //					queue = qManager.accessQueue(mqParameter.getQueueName(),MQC.MQOO_INPUT_AS_Q_DEF);
                        //queue = qManager.AccessQueue(mqParameter.getQueueName(), MQC.MQOO_INQUIRE | MQC.MQOO_FAIL_IF_QUIESCING | MQC.MQOO_INPUT_SHARED);
                        queue = qManager.AccessQueue(mqParameter.getQueueName(), MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_INQUIRE);
                    }
                    else if (MQ_MSGSEND_TYPE == type)
                    {
                        queue = qManager.AccessQueue(mqParameter.getQueueName(), MQC.MQOO_OUTPUT | MQC.MQOO_FAIL_IF_QUIESCING | MQC.MQOO_INQUIRE);
                       // queue = qManager.AccessQueue(mqParameter.getQueueName(), MQC.MQOO_OUTPUT);
                    }

                    para.qManager = qManager;
                    para.queue = queue;

                    ret = true;
                    break;
                }
                catch (MQException mqe)
                {
                    LogUtil.Error("",mqe);
                    if (i == trycount)
                    {
                        LogUtil.Error(
                                    "不能连接到MQ Server :" , para.mqParameter.toString() + "]，" +
                                    "已经做了[" + i + "]次尝试！");
                        throw mqe;
                    }
                    else
                    {
                        try
                        {
                            // 在下一次重试之前等待一定时间
                            Thread.Sleep(interval);
                        }
                        catch (Exception e)
                        {
                            LogUtil.Error("Exception:", e);
                            throw new EisException("interrupted when connect sleeping");
                        }
                    }
                }
            }// end of while loop
            props.Clear();
            return ret;
        }

      /// <summary>
        /// 连接出现异常后，等待时间，单位毫秒 ，默认值：2000
      /// </summary>
      /// <returns></returns>
        private static int getInterval()
        {
            String tryInterval;
            try
            {
                tryInterval = CacheManager.getInstance().getConfig().getProperty(ConfigConstants.MQ_CONN_TRY_WITETIME);
            }
            catch (EisException e)
            {
                LogUtil.Error("Exception:",e);
                return MQ_TRY_INTERVAL;
            }
            if (null == tryInterval)
                return MQ_TRY_INTERVAL;
            return int.Parse(tryInterval);
        }

   /// <summary>
        /// 出现异常后重新连接次数,根据Config配置文件获取，默认值是3
   /// </summary>
   /// <returns></returns>
        private static int getCount()
        {
            String trycount;
            try
            {
                trycount = CacheManager.getInstance().getConfig().getProperty(ConfigConstants.MQ_CONN_TRY_COUNT);
            }
            catch (EisException e)
            {
                LogUtil.Error("Exception:",e);
                return MQ_TRY_COUNT;
            }
            if (null == trycount)
                return MQ_TRY_COUNT;
            return int.Parse(trycount);
        }


        /**
	 * 通过匹配的msgID来从Queue中获取消息
	 * @param msgId Q队列的消息标识
	 * @param para MQ连接实例对象
	 * @param type 设置接收消息
	 * @param timeout < 0:则无限等待;< 0:从MQ队列连接的超时时间，单位秒
	 * @return
	 * @throws EisException
	 */
	public static MQMessage getMsgFromQueue(byte[] msgId, MQCParameter para, int timeout){
		
		try{
				MQGetMessageOptions gmo = getGMOFromPara(timeout);
				if (para.qManager == null || para.queue == null) {
					connectMQExce(para, MQQueueAccesser.MQ_MSGRECEIVE_TYPE);
				}
				MQMessage msg = new MQMessage();
				if (msgId != null){
					msg.MessageId = msgId;
				}

                printMsgId("before get MsgId", msg.MessageId);
				// 从MQ请求队列拿消息
				para.queue.Get(msg, gmo);
                printMsgId("after get MsgId", msg.MessageId);
				return msg;
		}catch (MQException mqe) {
			throw mqe;
		}catch(Exception e){
			throw new EisException(MQCException.MQ_MSG_RECEIVE_GETMSG_ERROR_EXCEPTION_CODE);
		}
		// return null;
	}


 /**
 * 打印msgId
 * @param msg
 * @param msgId
 */
    public static void printMsgId(String msg, byte[] msgId)
    {
        try
        {
            if (msgId == null)
            {
                LogUtil.Info(msg, "null");
            }
            else
            {
                LogUtil.Info(msg, System.Text.Encoding.Default.GetString(msgId));
            }

        }
        catch (Exception e)
        {
            LogUtil.Error(msg, e);
        }
    }


    /**
	 * 设置从MQ队列中获取消息的参数
	 * @param timeout < 0:则无限等待;< 0:从MQ队列连接的超时时间，单位秒
	 * @return
	 */
	private static  MQGetMessageOptions getGMOFromPara(int timeout) {
		MQGetMessageOptions gmo = new MQGetMessageOptions();
		gmo.Options = MQC.MQGMO_WAIT + MQC.MQGMO_FAIL_IF_QUIESCING;
//		gmo.options = MQC.MQGMO_SYNCPOINT  + MQC.MQGMO_WAIT + MQC.MQGMO_FAIL_IF_QUIESCING; ;
		
		//如果是-1则无限等待取值,否则设置超时时间
		if (timeout < 0){
			gmo.WaitInterval = MQC.MQWI_UNLIMITED;
		}else{
            gmo.WaitInterval = timeout * 1000; // 设置timeout的时间，单位毫秒
		}
		return gmo;
	}

    /**
	 * 将byte[]转换为MQMessage
	 *
	 * @param msg
	 * @return
	 */
	public static MQMessage getMQMessageFromBytes(byte[] msg) {
		if (null == msg || msg.Length <= 0)
			return null;

		MQMessage message = null;
		int expiry = getExpiry();
		try {
			// prepare the mq message
			message = new MQMessage();
			message.Expiry = expiry;
			message.Write(msg);
			return message;
		} catch (Exception t) {
			if (message != null) {
				LogUtil.Error("Exception",t);
				throw new EisException("Fail to get message body from MQMessage|msgId="	+ message.MessageId);
			}
		}
		return null;
	}

   	/**
	 * 将MQ消费转换成byte流返回
	 * @param msg
	 * @return
	 * @throws EisException
	 */

	public static byte[] getBytesFromMQMessage(MQMessage msg){
		if (null == msg)
			return null;

		try {
			byte[] msgContent = new byte[msg.MessageLength];
			msg.ReadFully(ref msgContent);
			return msgContent;
		} catch (Exception t) {
			LogUtil.Error("Exception",t);
			if (msg != null)
				throw new EisException("|msgId=" + msg.MessageId);
		}
		return null;
	}

    /**
	 * 返回消息的有效时间
	 * @return
	 */
    private static int getExpiry()
    {
        String expiry;
        try
        {
            expiry = CacheManager.getInstance().getConfig().getProperty(ConfigConstants.MQ_MSG_EXPIRY);
        }
        catch (EisException e)
        {
            LogUtil.Error("Exception", e);
            return MQ_MSG_EXPIRY;
        }
        if (null == expiry)
            return MQ_MSG_EXPIRY;
        return int.Parse(expiry);
    }

    /**
         * 尝试从队列中获取消息次数
         * @return，默认值3次
         */
    public static int getConnGetMsgCount()
    {
        String trycount;
        try
        {
            trycount = CacheManager.getInstance().getConfig().getProperty(ConfigConstants.MQ_CONN_GETMSG_COUNT);
        }
        catch (EisException e)
        {
            LogUtil.Error("Exception", e);
            return MQ_CONN_GETMSG_COUNT;
        }
        if (null == trycount)
            return MQ_CONN_GETMSG_COUNT;
        return int.Parse(trycount);
    }

    /**
     * 尝试从将消息放入到队列次数
     * @return 默认值3
     */
    public static int getConnPutMsgCount()
    {
        String trycount;
        try
        {
            trycount = CacheManager.getInstance().getConfig().getProperty(ConfigConstants.MQ_CONN_PUTMSG_COUNT);
        }
        catch (EisException e)
        {
            LogUtil.Error("Exception", e);
            return MQ_CONN_PUTMSG_COUNT;
        }
        if (null == trycount)
            return MQ_CONN_PUTMSG_COUNT;
        return int.Parse(trycount);
    }

    /**
 * 统一处理MQException
 *
 * @param e
 * @return int return 0 success; return 1 timeout; return -1 connection
 *         broken; return 2 unknown exception.
 */
    public static int handleMQException(MQException e)
    {
        int ret = MQ_UNKNOW_EXCEPTION;
       // if (e.completionCode == MQException.MQCC_OK    && e.reasonCode == MQException.MQRC_NONE)
        if (e.CompletionCode == 0 && e.ReasonCode == 0)
            ret = MQ_OK;
        //else if (e.CompletionCode == MQException.MQCC_FAILED)
        else if (e.CompletionCode == 2)
        {
            switch (e.ReasonCode)
            {
                //case MQException.MQRC_NO_MSG_AVAILABLE:
                //    ret = MQ_TIME_OUT;
                //    break;
                //case MQException.MQRC_CONNECTION_QUIESCING:
                //case MQException.MQRC_Q_MGR_NAME_ERROR:
                //case MQException.MQRC_Q_MGR_NOT_AVAILABLE:
                //case MQException.MQRC_SECURITY_ERROR:
                //case MQException.MQRC_CONNECTION_STOPPING:
                //    ret = MQ_CONNECTION_BROKER;
                //    break;
                //case MQException.MQRC_CONNECTION_BROKEN:
                //    ret = MQ_CONNECTION_BROKEN_2009;
                //    break;
                //default:
                //    ret = MQ_UNKNOW_EXCEPTION;

                case 2033:
                    ret = MQ_TIME_OUT;
                    break;
                case 2202:
                    break;
                case 2058: 
                    break;
                case 2059:
                    break;
                case 2063:
                    break;
                case 2203:
                    ret = MQ_CONNECTION_BROKER;
                    break;
                case 2009:
                    ret = MQ_CONNECTION_BROKEN_2009;
                    break;
                default:
                    ret = MQ_UNKNOW_EXCEPTION;
                    break;
            }
        }
        return ret;
    }// end of method

	/**
	 * 将响应消息放入发送队列
	 * @param para 连接实例
	 * @param message 准备发送的消息
	 * @param type 
	 * @return
	 * @throws EisException
	 */
	public static Boolean putMsgToQueue(MQCParameter para, MQMessage message){
		MQPutMessageOptions pmo = getPMOFromPara();
		if (null == message || null == pmo || null == para) {
			throw new EisException(MQCException.MQ_MSG_SEND_PUTMSG_ERROR_EXCEPTION_CODE);
		}
		try {
				if (para.qManager == null || para.queue == null) {
					connectMQExce(para, MQQueueAccesser.MQ_MSGSEND_TYPE);				}
				
				printMsgId("before put MsgId",message.MessageId);
				LogUtil.Info("put message.replyToQueueManagerName:" , message.ReplyToQueueManagerName);
                LogUtil.Info("put message.messageType:" , message.MessageType);
                LogUtil.Info("put message.report:" , message.Report);
                LogUtil.Info("message.encoding:" , message.Encoding);
                LogUtil.Info("message.characterSet:" , message.CharacterSet);
                LogUtil.Info("message.format:" , message.Format);
				printMsgId("before put correlationId",message.CorrelationId);
				
				para.queue.Put(message, pmo);
				para.qManager.Commit();
				printMsgId("after put MsgId",message.MessageId);
				return  true;
			
		} catch (MQException mqe) {
			throw mqe;
		} finally {
			pmo = null;
		}
	}

    public static byte[] GetByteArray(string shex)
    {
        string[] ssArray = shex.Split(' ');
        List<byte> bytList = new List<byte>();
        foreach (var s in ssArray)
        {
            //将十六进制的字符串转换成数值
            bytList.Add(Convert.ToByte(s, 16));
        }
        //返回字节数组
        return bytList.ToArray();
    }



	
  
    }
}
