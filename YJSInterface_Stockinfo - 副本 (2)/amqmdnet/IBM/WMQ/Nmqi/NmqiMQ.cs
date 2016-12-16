namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Runtime.InteropServices;

    public interface NmqiMQ
    {
        void MQBACK(Hconn hconn, out int pCompCode, out int pReason);
        void MQBEGIN(Hconn hconn, MQBeginOptions pBeginOptions, out int pCompCode, out int pReason);
        void MQCB(Hconn hconn, int operation, MQCBD pCallbackDesc, Hobj hobj, MQMessageDescriptor pMsgDesc, MQGetMessageOptions getMsgOpts, out int compCode, out int reason);
        void MQCLOSE(Hconn Hconn, Phobj pHobj, int Options, out int pCompCode, out int pReason);
        void MQCMIT(Hconn hconn, out int pCompCode, out int pReason);
        void MQCONN(string pQMgrName, Phconn pHconn, out int pCompCode, out int pReason);
        void MQCONNX(string pQMgrName, MQConnectOptions pConnectOpts, Phconn pHconn, out int pCompCode, out int pReason);
        void MQCTL(Hconn Hconn, int Operation, MQCTLO pControlOpts, out int compCode, out int reason);
        void MQDISC(Phconn pHconn, out int pCompCode, out int pReason);
        void MQGET(Hconn Hconn, Hobj Hobj, MQMessageDescriptor pMsgDesc, MQGetMessageOptions pGetMsgOpts, int BufferLength, byte[] pBuffer, out int pDataLength, out int pCompCode, out int pReason);
        void MQINQ(Hconn Hconn, Hobj Hobj, int SelectorCount, int[] pSelectors, int IntAttrCount, int[] pIntAttrs, int CharAttrLength, byte[] pCharAttrs, out int pCompCode, out int pReason);
        void MQOPEN(Hconn Hconn, ref MQObjectDescriptor pObjDesc, int Options, Phobj pHobj, out int pCompCode, out int pReason);
        void MQPUT(Hconn Hconn, Hobj Hobj, MQMessageDescriptor pMsgDesc, MQPutMessageOptions pPutMsgOpts, int BufferLength, byte[] pBuffer, out int pCompCode, out int pReason);
        void MQPUT1(Hconn Hconn, MQObjectDescriptor pObjDesc, MQMessageDescriptor pMsgDesc, MQPutMessageOptions pPutMsgOpts, int BufferLength, byte[] pBuffer, out int pCompCode, out int pReason);
        void MQSET(Hconn Hconn, Hobj Hobj, int SelectorCount, int[] pSelectors, int IntAttrCount, int[] pIntAttrs, int CharAttrLength, byte[] pCharAttrs, out int pCompCode, out int pReason);
        void MQSTAT(Hconn hconn, int Type, MQAsyncStatus pStat, out int pCompCode, out int pReason);
        void MQSUB(Hconn Hconn, MQSubscriptionDescriptor pSubDesc, Phobj Hobj, Phobj Hsub, out int pCompCode, out int pReason);
        void MQSUBRQ(Hconn Hconn, Hobj Hsub, int action, ref MQSubscriptionRequestOptions mqsro, out int pCompCode, out int pReason);
    }
}

