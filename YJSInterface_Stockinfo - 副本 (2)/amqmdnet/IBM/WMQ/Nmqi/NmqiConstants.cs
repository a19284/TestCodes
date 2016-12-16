namespace IBM.WMQ.Nmqi
{
    using System;

    internal abstract class NmqiConstants
    {
        public static string AMQ_RANDOM_NUMBER_TYPE_FAST;
        public static string AMQ_RANDOM_NUMBER_TYPE_STANDARD;
        public const int asyncRequestId = 1;
        public const int CONN_ASYNC_THREAD_AFFINITY = 0x10000;
        public const int CONN_BROKEN = 2;
        public const int CONN_CLEANUP = 0x8000;
        public const int CONN_CONSUMERS_CHANGED = 0x200;
        public const int CONN_DELETED = 0x20;
        public const int CONN_DISCONNECTING = 0x10;
        public const int CONN_EVENT_HANDLER_SUSPENDED = 0x400;
        public const int CONN_INCALL = 0x2000;
        public const int CONN_INCALL_INTERNAL = 0x4000;
        public const int CONN_LOCAL = 1;
        public const int CONN_MQ_CONNECT_WAITING = 0x1000;
        public const int CONN_MQ_CONNECTED = 4;
        public const int CONN_MQ_CONNECTING = 0x800;
        public const int CONN_QUIESCING = 0x100;
        public const int CONN_START_PENDING = 0x80000;
        public const int CONN_STARTED = 0x40;
        public const int CONN_STOP_PENDING = 0x20000;
        public const int CONN_SUSPEND_PENDING = 0x100000;
        public const int CONN_SUSPENDED = 0x80;
        public const int CONN_TXN = 0x4000000;
        public const int CONN_TXN_DOOMED = 0x8000000;
        public const int CONN_XA_CONNECTED = 8;
        public static readonly int[] EventReasons;
        public static readonly int[] Events = new int[] { 1, 2, 4, 8, 0x10 };
        public const int INDEFINATE_WAIT = -1;
        public const int MaxMsgsPerCB = 5;
        public const int MQRC_PASSWORD_PROTECTION_ERROR = 0xa22;
        public const string PASSWORD_PROTECTION_ALWAYS = "ALWAYS";
        public const string PASSWORD_PROTECTION_COMPATIBLE = "COMPATIBLE";
        public const string PASSWORD_PROTECTION_OPTIONAL = "OPTIONAL";
        public static readonly bool[] RaiseReconnecting;
        public const int reqEV_CONNECTION_BROKEN = 1;
        public const int reqEV_CONNECTION_QUIESCING = 2;
        public const int reqEV_CONNECTION_STOPPING = 4;
        public const int reqEV_Q_MGR_QUIESCING = 8;
        public const int reqEV_Q_MGR_STOPPING = 0x10;
        public const int rfpMS_LAST_MSG_IN_GROUP = 0x20;
        public const int rfpMS_LAST_SEGMENT = 8;
        public const int rfpMS_MSG_IN_GROUP = 0x10;
        public const int rfpMS_MSG_PROPERTIES_RETURNED = 0x40;
        public const int rfpMS_SEGMENT = 4;
        public const int rfpMS_SEGMENTATION_ALLOWED = 2;
        public const int rfpMS_STREAMING_PAUSED = 1;
        public const int rfpQS_STREAMING = 1;
        public const int rfpQS_STREAMING_BACKLOG = 2;
        public const int rfpQS_STREAMING_INHIBITED = 4;
        public const int rfpRMS_MSG_PROPERTIES_REQUESTED = 0x20;
        public const int rfpVS_REGISTERED = 0x10;
        public const int rfpVS_START_WAIT = 4;
        public const int rfpVS_STARTED = 2;
        public const int rfpVS_SUSPENDED = 1;
        public const int rpqMS_MESSAGE = 1;
        public const int rpqMS_TXN = 2;
        public const int rpqST_BROWSE = 0x20;
        public const int rpqST_BROWSE_FIRST_WAIT = 0x1000;
        public const int rpqST_CALLBACK_ON_EMPTY = 0x1000000;
        public const int rpqST_CLOSED = 0x20000000;
        public const int rpqST_DEREGISTER_OUTSTANDING = 0x200000;
        public const int rpqST_FUNCTION_LOADED = 0x100000;
        public const int rpqST_GET_INHIBITED = 8;
        public const int rpqST_GETTER = 2;
        public const int rpqST_GETTER_SIGNALLED = 4;
        public const int rpqST_INPUT = 0x20000;
        public const int rpqST_LOGICALLY_DELETED = 0x80000;
        public const int rpqST_MQGET_CALLED = 0x2000000;
        public const int rpqST_MQGET_WAITING = 0x4000000;
        public const int rpqST_MSG_COPIED = 0x8000000;
        public const int rpqST_MULTICAST = 0x40;
        public const int rpqST_QM_QUIESCING = 0x100;
        public const int rpqST_QUIESCED = 0x80;
        public const int rpqST_REQUEST_MSG_NEEDED = 0x800;
        public const int rpqST_SELECTION_CHANGED = 0x10000;
        public const int rpqST_SELECTION_SET = 0x200;
        public const int rpqST_START_CALLED = 0x400000;
        public const int rpqST_STOP_CALLED = 0x800000;
        public const int rpqST_STREAMING = 0x400;
        public const int rpqST_STREAMING_INHIBITED = 0x8000;
        public const int rpqST_STREAMING_PAUSED = 0x2000;
        public const int rpqST_STREAMING_REQUESTED = 0x4000;
        public const int rpqST_STREAMING_TXN_PAUSED = 0x40000000;
        public const int rpqST_SUSPENDED = 0x40000;
        public const int rpqST_TRANSACTIONAL = 0x10000000;
        public const int rpqST_TRANSACTIONAL_NEXT = 0x10;
        public const int rpqST_UNHEALTHY = 1;
        public const string sccsid = "%Z% %W% %I% %E% %U%";
        public const int syncRequestId = 0;
        public const int XA_NONE = 0;
        public const int XA_OPENED = 2;
        public const int XA_TRAN_ACTIVE = 4;

        static NmqiConstants()
        {
            bool[] flagArray = new bool[5];
            flagArray[3] = true;
            flagArray[4] = true;
            RaiseReconnecting = flagArray;
            EventReasons = new int[] { 0x7d9, 0x89a, 0x89b, 0x871, 0x872 };
            AMQ_RANDOM_NUMBER_TYPE_STANDARD = "STANDARD";
            AMQ_RANDOM_NUMBER_TYPE_FAST = "FAST";
        }

        protected NmqiConstants()
        {
        }
    }
}

