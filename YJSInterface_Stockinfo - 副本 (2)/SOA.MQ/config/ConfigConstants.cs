using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.config
{
    /// <summary>
    /// 配置参数
    /// </summary>
    public class ConfigConstants
    {
        //public static String LOG_FILE = "config/Log4Net.config";
        public static String CONFIG_FILE = "config/config.properties";

        /**
         *备选配置分隔符
         */
        public static readonly String ALTERNATIVE_CONFIG_SPLIT = ",";
        /**
         * 服务提供方备选配置
         */
        public static readonly String ALTERNATIVE_PROVIDER_CONFIG = "ALTERNATIVE_PROVIDER_CONFIG";

        /**
         * 服务请求方备选配置
         */
        public static readonly String ALTERNATIVE_REQUESTER_CONFIG = "ALTERNATIVE_REQUESTER_CONFIG";

        /* 服务提供方接收/发送消息 */
        public static readonly String MQ_PROVIDER_RECV_IP = "MQ.PROVIDER.RECV.IP";
        public static readonly String MQ_PROVIDER_RECV_PORT = "MQ.PROVIDER.RECV.PORT";
        public static readonly String MQ_PROVIDER_RECV_CHANNEL = "MQ.PROVIDER.RECV.CHANNEL";
        public static readonly String MQ_PROVIDER_RECV_CCSID = "MQ.PROVIDER.RECV.CCSID";
        public static readonly String MQ_PROVIDER_RECV_QMANAGER = "MQ.PROVIDER.RECV.QMANAGER";
        public static readonly String MQ_PROVIDER_RECV_QUEUE = "MQ.PROVIDER.RECV.QUEUE";

        public static readonly String MQ_PROVIDER_SEND_IP = "MQ.PROVIDER.SEND.IP";
        public static readonly String MQ_PROVIDER_SEND_PORT = "MQ.PROVIDER.SEND.PORT";
        public static readonly String MQ_PROVIDER_SEND_CHANNEL = "MQ.PROVIDER.SEND.CHANNEL";
        public static readonly String MQ_PROVIDER_SEND_CCSID = "MQ.PROVIDER.SEND.CCSID";
        public static readonly String MQ_PROVIDER_SEND_QMANAGER = "MQ.PROVIDER.SEND.QMANAGER";
        public static readonly String MQ_PROVIDER_SEND_QUEUE = "MQ.PROVIDER.SEND.QUEUE";

        public static readonly String MQ_PROVIDER_POOL_MAXNUM = "MQ.PROVIDER.POOL.MAXNUM";
        public static readonly String MQ_PROVIDER_GETCONN_TIMEOUT = "MQ.PROVIDER.GETCONN.TIMEOUT";

        /* 服务请求方请求/应答消息 */
        public static readonly String MQ_REQUESTER_REQ_IP = "MQ.REQUESTER.REQ.IP";
        public static readonly String MQ_REQUESTER_REQ_PORT = "MQ.REQUESTER.REQ.PORT";
        public static readonly String MQ_REQUESTER_REQ_CHANNEL = "MQ.REQUESTER.REQ.CHANNEL";
        public static readonly String MQ_REQUESTER_REQ_CCSID = "MQ.REQUESTER.REQ.CCSID";
        public static readonly String MQ_REQUESTER_REQ_QMANAGER = "MQ.REQUESTER.REQ.QMANAGER";
        public static readonly String MQ_REQUESTER_REQ_QUEUE = "MQ.REQUESTER.REQ.QUEUE";

        public static readonly String MQ_REQUESTER_RES_IP = "MQ.REQUESTER.RES.IP";
        public static readonly String MQ_REQUESTER_RES_PORT = "MQ.REQUESTER.RES.PORT";
        public static readonly String MQ_REQUESTER_RES_CHANNEL = "MQ.REQUESTER.RES.CHANNEL";
        public static readonly String MQ_REQUESTER_RES_CCSID = "MQ.REQUESTER.RES.CCSID";
        public static readonly String MQ_REQUESTER_RES_QMANAGER = "MQ.REQUESTER.RES.QMANAGER";
        public static readonly String MQ_REQUESTER_RES_QUEUE = "MQ.REQUESTER.RES.QUEUE";

        public static readonly String MQ_REQUESTER_POOL_MAXNUM = "MQ.REQUESTER.POOL.MAXNUM";
        public static readonly String MQ_REQUESTER_GETCONN_TIMEOUT = "MQ.REQUESTER.GETCONN.TIMEOUT";

        public static readonly String MQ_REQUESTER_REQ_TIMEOUT = "MQ.REQUESTER.REQ.TIMEOUT";
        public static readonly String MQ_PROVIDER_RECV_TIMEOUT = "MQ.PROVIDER.RECV.TIMEOUT";
        

        //MQ组的长度
        public static readonly String MQ_SEGMENT_LEN = "MQ.SEGMENT.LEN";
        /**
         * 消息有效时间,(1/10秒)
         */
        public static readonly String MQ_MSG_EXPIRY = "MQ.MSG.EXPIRY";

        //MQ尝试重新进行连接
        /**
         * 接收消息异常，重试次数
         */
        public static readonly String MQ_CONN_GETMSG_COUNT = "MQ.CONN.GETMSG.COUNT";
        /**
         * 发送消息异常，重试次数
         */
        public static readonly String MQ_CONN_PUTMSG_COUNT = "MQ.CONN.PUTMSG.COUNT";


        /**
         * MQ连接异常，重连次
         */
        public static readonly String MQ_CONN_TRY_COUNT = "MQ.CONN.TRY.COUNT";

        /**
         * 每次重连的等待时间(毫秒)
         */
        public static readonly String MQ_CONN_TRY_WITETIME = "MQ.CONN.TRY.WITETIME";

        /**
         * MQ队列管理器尝试连接超时时间(毫秒)
         */
        public static readonly String MQ_REQUESTER_LINK_TIMEOUT = "MQ.REQUESTER.LINK.TIMEOUT";


        //	/* 接收消息 */
        //	public static readonly String MQ_SUB_IP = "MQ.SUB.IP";
        //	public static readonly String MQ_SUB_PORT = "MQ.SUB.PORT";
        //	public static readonly String MQ_SUB_CHANNEL = "MQ.SUB.CHANNEL";
        //	public static readonly String MQ_SUB_CCSID = "MQ.SUB.CCSID";
        //	public static readonly String MQ_SUB_QMANAGER = "MQ.SUB.QMANAGER";
        //	public static readonly String MQ_SUB_QUEUE = "MQ.SUB.QUEUE";
        //	public static readonly String MQ_SUB_POOL_MAXNUM = "MQ.SUB.POOL.MAXNUM";
        //	public static readonly String MQ_SUB_GETCONN_TIMEOUT = "MQ.SUB.GETCONN.TIMEOUT";
        //	public static readonly String MQ_SUB_MSG_TIMEOUT = "MQ.SUB.MSG.TIMEOUT";
        //	
        //	/* 发送消息 */
        //	public static readonly String MQ_PUB_IP = "MQ.PUB.IP";
        //	public static readonly String MQ_PUB_PORT = "MQ.PUB.PORT";
        //	public static readonly String MQ_PUB_CHANNEL = "MQ.PUB.CHANNEL";
        //	public static readonly String MQ_PUB_CCSID = "MQ.PUB.CCSID";
        //	public static readonly String MQ_PUB_QMANAGER = "MQ.PUB.QMANAGER";
        //	public static readonly String MQ_PUB_QUEUE = "MQ.PUB.QUEUE";
        //	public static readonly String MQ_PUB_POOL_MAXNUM = "MQ.PUB.POOL.MAXNUM";
        //	public static readonly String MQ_PUB_GETCONN_TIMEOUT = "MQ.PUB.GETCONN.TIMEOUT";


        /* 编码 */
        public static readonly String ENCODING = "ENCODING";

        /* 服务提供方处理类名 */
        public static readonly String PROVIDER_HANDLER_CLASSNAME = "PROVIDER.HANDLER.CLASSNAME";

        /* 服务提供方处理线程数 */
        public static readonly String PROVIDER_HANDLER_MAXNUM = "PROVIDER.HANDLER.MAXNUM";

        public static readonly String SYSID = "SYSID";
    }
}

