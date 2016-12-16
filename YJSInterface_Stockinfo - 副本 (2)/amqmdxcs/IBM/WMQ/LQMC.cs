namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Size=1)]
    public struct LQMC
    {
        public const int lpiRC_OK = 0;
        public const int lpiRC_CSPRC_INVALID_ARGUMENTS = 0x7024;
        public const int lpiRC_CSPRC_Q_MGR_NOT_AVAILABLE = 0x7028;
        public const int lpiRC_CSPRC_Q_MGR_STOPPING = 0x7031;
        public const int lpiRC_CSPRC_UNEXPECTED_ERROR = 0x7047;
        public const int lpiRC_CSPRC_Q_MGR_NAME_ERROR = 0x7048;
        public const int lpiRC_CSPRC_UNKNOWN_OBJECT_NAME = 0x7085;
        public const int lpiGETOPT_NONE = 0;
        public const int lpiGETOPT_INHERIT = 1;
        public const int lpiGETOPT_COMMIT = 2;
        public const int lpiGETOPT_COMMIT_IF_YOU_LIKE = 4;
        public const int lpiGETOPT_COMMIT_ASYNC = 8;
        public const int lpiGETOPT_SHORT_TXN = 0x10;
        public const int lpiPUTOPTS_NONE = 0;
        public const int lpiPUTOPTS_BLANK_PADDED = 1;
        public const int lpiPUTOPTS_SYNCPOINT_IF_YOU_LIKE = 2;
        public const int lpiPUTOPTS_DEFERRED = 4;
        public const int lpiPUTOPTS_PUT_AND_FORGET = 8;
        public const int lpiPUTOPTS_ASYNC = 0x20;
        public const int lpiACTIVATE_NONE = 0;
        public const int lpiACTIVATE_ACTIVATE = 1;
        public const int lpiACTIVATE_CANCEL = 2;
        public const int lpiSYNCPT_NONE = 0;
        public const int lpiSYNCPT_START = 1;
        public const int lpiSYNCPT_END = 2;
        public const int lpiSYNCPT_PREPARE = 3;
        public const int lpiSYNCPT_COMMIT = 4;
        public const int lpiSYNCPT_ROLLBACK = 5;
        public const int lpiSYNCPT_ASYNC_COMMIT = 6;
    }
}

