using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.connection.mqc
{
    /// <summary>
    /// MQ 异常信息定义
    /// </summary>
    public class MQCException
    {
        public static readonly String MQ_MSG_RECEIVE_BUFFER_INSTANCE_EXCEPTION_CODE = "EEE900001";
        public static readonly String MQ_MSG_RECEIVE_BUFFER_INSTANCE_EXCEPTION_DESC = "MQ Msg Receive Buffer Instance Get Exception";

        public static readonly String MQ_MSG_RECEIVE_INIT_EXCEPTION_CODE = "EEE2200143";
        public static readonly String MQ_MSG_RECEIVE_INIT_EXCEPTION_DESC = "MQ Msg Receive Initial Exception";

        public static readonly String MQ_MSG_RECEIVE_REFRESH_EXCEPTION_CODE = "EEE2200151";
        public static readonly String MQ_MSG_RECEIVE_REFRESH_EXCEPTION_DESC = "MQ Msg Receive Refresh Exception";

        public static readonly String MQ_MSG_RECEIVE_GETMSG_TIMEOUT_EXCEPTION_CODE = "EEE2200061";
        public static readonly String MQ_MSG_RECEIVE_GETMSG_TIMEOUT_EXCEPTION_DESC = "MQ Msg Receive Get Msg Timeout Exception";

        public static readonly String MQ_MSG_RECEIVE_GETMSG_ERROR_EXCEPTION_CODE = "EEE2200062";
        public static readonly String MQ_MSG_RECEIVE_GETMSG_ERROR_EXCEPTION_DESC = "MQ Msg Receive Get Msg Error Exception";

        public static readonly String MQ_MSG_SEND_BUFFER_INSTANCE_EXCEPTION_CODE = "EEE900001";
        public static readonly String MQ_MSG_SEND_BUFFER_INSTANCE_EXCEPTION_DESC = "MQ Msg Send Buffer Instance Get Exception";

        public static readonly String MQ_MSG_SEND_INIT_EXCEPTION_CODE = "EEE2200143";
        public static readonly String MQ_MSG_SEND_INIT_EXCEPTION_DESC = "MQ Msg Send Initial Exception";

        public static readonly String MQ_MSG_SEND_REFRESH_EXCEPTION_CODE = "EEE2200151";
        public static readonly String MQ_MSG_SEND_REFRESH_EXCEPTION_DESC = "MQ Msg Send Refresh Exception";

        public static readonly String MQ_MSG_SEND_PUTMSG_ERROR_EXCEPTION_CODE = "EEE2200063";
        public static readonly String MQ_MSG_SEND_PUTMSG_ERROR_EXCEPTION_DESC = "MQ Msg Send Put Msg Error Exception";

        public static readonly String MQ_MQCPARAMETER_INIT_ERROR_EXCEPTION_CODE = "EEE2200064";
        public static readonly String MQ_MQCPARAMETER_INIT_ERROR_EXCEPTION_DESC = "MQ MQC Parameter Initial Error Exception";

        public static readonly String MQ_CONNECT_ERROR_EXCEPTION_CODE = "EEE2200065";
        public static readonly String MQ_CONNECT_ERROR_EXCEPTION_DESC = "MQ Connection Error Exception";

    }
}
