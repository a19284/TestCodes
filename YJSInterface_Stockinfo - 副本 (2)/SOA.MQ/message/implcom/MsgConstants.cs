using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.message.implcom
{
    public  class MsgConstants
    {
        public static readonly String Service = "Service";

        public static readonly String PATH_SEPARATOR = "/";

        public static readonly String ROUTE = "Route";
        public static readonly String SERIAL_NO = "SerialNO";
        public static readonly String SERVICE_ID = "ServiceID";
        public static readonly String SERVICE_TIME = "ServiceTime";
        public static readonly String SOURCE_SYSTEM_ID = "SourceSysID";
        public static readonly String PROCESSES = "Processes";
        public static readonly String PROCESS = "Process";
        public static readonly String SUB_SERIAL_NO = "SubSerialNO";
        public static readonly String SERVICE_RESPONSE = "ServiceResponse";
        public static readonly String STATUS = "Status";
        public static readonly String CODE = "Code";
        public static readonly String DESC = "Desc";
        public static readonly String SOURCE_SYSTEM_CODE = "SourceSysCode";
        public static readonly String SOURCE_SYSTEM_DESC = "SourceSysDesc";
        public static readonly String REQ_APP_SERVID = "ReqAppServID";
        public static readonly String MSSAGE_TYPE = "MessageType";
        public static readonly String DATA = "Data";
        public static readonly String DATA_CONTROL = "Control";
        public static readonly String DATA_RESPONSE = "Response";
        public static readonly String DATA_REQUEST = "Request";

        public static readonly String SERV_PROCESSTIME = "process_timestamp";
        public static readonly String SERV_STARTTIME = "start_timestamp";
        public static readonly String SERV_ENDTIME = "end_timestamp";
        public static readonly String SERV_TIMEOUT_START = "timeout_start";

        public static readonly String STATUS_COMPLETE = "COMPLETE";
        public static readonly String STATUS_INITIAL = "INITIAL";
        public static readonly String STATUS_FAIL = "FAIL";
        public static readonly String STATUS_INPROCESS = "INPROCESS";
        public static readonly String STATUS_SKIP = "SKIP";

        public static readonly String REQUEST_URL = "requestURL"; //add for soap message
        public static readonly String MSG_DOMAIN = "XMLNSC";
        public static readonly String MSG_DOMAIN_PATH = "/" + MsgConstants.MSG_DOMAIN;
        public static readonly String ENVELOPE = "Envelope";
        public static readonly String ENVELOPE_PATH = MsgConstants.MSG_DOMAIN_PATH + "/" + MsgConstants.ENVELOPE;
        public static readonly String HEADER_PATH = MsgConstants.ENVELOPE_PATH + "/" + MsgConstants.ROUTE;

        /**
         * 响应输出过期时间
         */
        public static readonly String CONFIG_RESOUT_EXPIRY = "ResOutExpiry";
        public static readonly String IS_SET_REPLYQM = "SetReplyQmgrFlag";
        public static readonly String TX_TIME_OUT_FLAG = "TxTimeOutFlag";
        public static readonly String REPORT_FLAG = "ReportFlag";
        public static readonly String REPORT_Q = "ReportQ";
        public static readonly String REPORT_QMGR = "ReportQmgr";
        public static readonly String REPLY_QMGR = "reply_qmgr";
        public static readonly String REPLY_Q = "reply_q";
        public static readonly String SYS_QM = "sys_qm";

        /**
         * 主动同步冲正
         */
        public static readonly String MSG_INIT_SYNC = "INITSYNC";
        public static readonly String MSG_SERV_PROCESS_AFTER_LOGIC_CLASS = "after_logic_class";
        public static readonly String MSG_SERV_PROCESS_SKIP_TO_PROCESS = "skip_to_process";
        public static readonly String MSG_SERV_PROCESS_IS_END = "is_end";
        /**
         * 原流水号
         */
        public static readonly String MSG_ORI_SUB_SERVICE_SN = "org_sub_service_sn";

        //Transaction for Single
        public static readonly char SERV_SINGLE = '0';

        //Transaction for Multi
        public static readonly char SERV_MULTI = '1';

        //Transaction for Passive Reversal
        public static readonly char SERV_PASV_RL = '4';

        // valid ccsid
        public static String MESSAGE_CCSID = "1208";

        //count rervice number
        public static String MSG_SERV_TOTAL = "total";

        //Form sn
        public static String MSG_FORM_SN = "SN";

        /**
         * 特殊标识，表示该报文是冲正的报文或响应
         */
        public static readonly String IS_REVERSAL_MESSAGE = "is_reversal_message";


        public static readonly String TRACE = "trace";

        public static readonly String RESP_CATALOG_BROKER = "Broker";

        /**
         * 在冲正报文中使用，表示冲正的原服务步骤
         */
        public static readonly String ORG_PROCESS_SEQ = "org_process_seq";

        public static readonly int DEFAULTEXPIRY = 5 * 60 * 10; // 缺省超时时间为5分钟

        public static String KEYWORK_LOOP = "LOOP";
        public static String KEYWORK_REQUEST = "REQ";
        public static String KEYWORK_RESPONSE = "RES";
        public static String KEYWORK_HEADER = "HEADER";
        public static String KEYWORK_MULTIREQUEST = "MREQ";

        public static readonly String Loop_Size = "loop_size";
        public static readonly String Loop_Index = "loop_index";
        public static readonly String Loop_Response = "loop_response";

        public static readonly String DelayTime = "delay_time";
        public static readonly String MEAT_TYPE = "type";
        public static readonly String MEAT_LENTH = "length";
        public static readonly String MEAT_ENCRYPT = "encrypt";
        public static readonly String MSG_ROOT = "Service";
        public static readonly String MSG_SERV_NAME = "name";
        //error catalog
        public static readonly String MSG_SERV_RESP_CATALOG = "catalog";
        public static readonly String MSG_SERV_REVERSAL_SERV_ID = "reversal_service_id";
        public static readonly String MSG_SERV_TYPE = "service_type";
        public static readonly String MSG_SERV_TIMEOUT = "timeout";
        public static readonly String MSG_SERV_LOG = "msglog";
        public static readonly String MSG_SERV_PROCESS_SYNC_REVERSAL_SERV_ID = "sync_reversal_service_id";
        public static readonly String MSG_SERV_PROCESS_ASYNC_REVERSAL_SERV_ID = "async_reversal_service_id";
        public static readonly String MSG_SUB_REVERSAL_SERV_ID = "sub_reversal_service_id";
        public static readonly String MSG_SERV_TARGET = "target_id";
        //Route sub_provider target_id
        public static readonly String MSG_SERV_SUBTARGET = "sub_target_id";
        public static readonly String MSG_SERV_REQ_TARGETQ = "req_target_q";
        public static readonly String MSG_SERV_RESP_TARGETQ = "resp_target_q";
        public static readonly String MSG_SERV_OPERATION = "operation";
        public static readonly String MSG_SERV_CURRENT = "currentprocess";
        public static readonly String MSG_SERV_NEXT_PROCESS = "nextprocess";
        public static readonly String MSG_SERV_PROCESS_ID = "id";
        public static readonly String MSG_ID = "id";

        public static readonly String LOOP_NUM = "loop_num";
        public static readonly String FIELD_TYPE_NAME = "p_type";
        public static readonly String FIELD_TYPE_G = "G";

        //INM_MODE represents atomic or combined service
        public static String INM_MODE = "INM_MODE";

        //Mutil_service request message id
        public static String MSG_REQUEST_MULTI_MSG_ID = "req_multi_msg_id";

        //Mutil_service response message id
        public static String MSG_RESPONSE_MULTI_MSG_ID = "resp_multi_msg_id";

        //Atomic_service request message id
        public static String MSG_SERV_REQUEST_MESSAGE_ID = "req_msg_id";

        //Atomic_service response message id
        public static String MSG_SERV_RESPONSE_MESSAGE_ID = "resp_msg_id";

        //Multi Request
        public static String MSG_MULTI_REQUEST = "multi_request";
        public static String MSG_MULTI_RESPONSE = "multi_response";

        public static String INM_PROCESS_CODE = "INM_PROCESS_CODE";
        public static String MSG_SERV_PROCESS_KEY_SERV = "key_service";
        public static String MSG_SERV_PROCESS_APPS_REVERSAL_DELAY = "apps_reversal_delay";
        public static String MSG_SERV_PROCESS_REVERSAL_SEQ = "reversal_seq";
        public static readonly String MSG_PASSIVE_REVERSAL = "reversal_flag";
        public static readonly String MSG_ORG_SERV_SN = "org_service_sn";
        public static readonly String MSG_ORG_SERV_DATE = "org_service_date";
        public static readonly String MSG_ORG_REQUEST_ID = "org_requester_id";

        //Performance Test Flag
        public static String MSG_PERFORMANCE_TEST = "performance_test";
        //Message Expire
        public static String MSG_EXPIRY = "msg_expiry";

        //Message Interval in millis
        public static String MSG_INTERVAL = "msg_interval";

        public static readonly String MSG_PASV_SYNC = "PASVSYNC";
        public static readonly String MSG_PASV_ASYNC = "PASVASYNC";
        public static readonly String MSG_INIT_ASYNC = "INITASYNC";

        public static readonly String BRANCH_SYS_ID = "sys_id";
        //added end

        public static readonly String MSG_GROUP_PROPERTY = "p_type";
        public static readonly String MSG_FIELD_TYPE_G = "G";
        //	public static readonly String MSG_FIELD_TYPE_E = "E";	
        public static readonly String MSG_SERV_PROCESS_SEND_DATE = "INM_SND_TX_CYCLE";
        public static readonly String MSG_SERV_PROCESS_SEND_SN = "INM_SND_TX_LOG_NO";
        // for ESB service proxy
        public static String MSG_SERV_PROVIDER = "provider";
        public static String MSG_SERV_REQUESTER = "requester";

        public static String BUSINESS_CODE = "BusinessCode";

        //成功响应码
        public static readonly String SUCCEED_CODE = "S000A000";

        //响应失败
        public static readonly String ERROR_CODE = "E000A920";


        public static readonly String MSG_STRUCTURE_ERRCODE = "E000A310";
        public static readonly String MSG_STRUCTURE_ERRDESC = "[报文结构]";

        public static readonly String API_ERROR_CODE = "E000I001";

        public static readonly String VERSION_ID = "version_id";
    }
}
