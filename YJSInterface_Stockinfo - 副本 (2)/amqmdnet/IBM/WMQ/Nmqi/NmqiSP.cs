namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    public interface NmqiSP
    {
        void CheckCmdLevel(Hconn hconn);
        void MQGET(Hconn hConn, Hobj objectHandle, MQGetMessageOptions gmo, ref MQMessage message, out int compCode, out int reason);
        void MQPUT(Hconn hConn, Hobj hObj, MQMessage messagehandle, MQPutMessageOptions pmo, out int compCode, out int reason);
        void NmqiConnect(string pQMgrName, NmqiConnectOptions pNmqiConnectOpts, MQConnectOptions pConnectOpts, Hconn parentHconn, Phconn pHconn, out int pCompCode, out int pReason);
        void NmqiConnect(string pQMgrName, NmqiConnectOptions pNmqiConnectOpts, MQConnectOptions pConnectOpts, Hconn parentHconn, Phconn pHconn, out int pCompCode, out int pReason, ManagedHconn rcnHconn);
        bool NmqiConvertMessage(Hconn hconn, Hobj hobj, int reqEncoding, int reqCCSID, int appOptions, bool callExitOnLenErr, MQMessageDescriptor md, byte[] buffer, out int dataLength, int availableLength, int bufferLength, out int compCode, out int reason, out int returnedLength);
        void NmqiGetMessage(Hconn hconn, Hobj hobj, MQMessageDescriptor md, MQGetMessageOptions gmo, int bufferLength, byte[] buffer, out int dataLength, out int compCode, out int reason);
        void NmqiPut(Hconn hconn, Hobj hobj, MQMessageDescriptor msgDesc, MQPutMessageOptions putMsgOpts, MemoryStream[] buffers, out int compCode, out int reason);
        void NmqiPut1(Hconn hconn, MQObjectDescriptor pObjDesc, MQMessageDescriptor msgDesc, MQPutMessageOptions putMsgOpts, MemoryStream[] buffers, out int compCode, out int reason);
        void SPIActivateMessage(Hconn hConn, ref MQSPIActivateOpts spiao, out int compCode, out int reason);
        void SpiConnect(string pQMgrName, SpiConnectOptions pSpiConnectOpts, MQConnectOptions pConnectOpts, Phconn pHconn, out int pCompCode, out int pReason);
        void SpiGet(Hconn hconn, Hobj hobj, MQMessageDescriptor pMqmd, MQGetMessageOptions pMqgmo, SpiGetOptions spiOptions, int bufferLength, byte[] pBuffer, out int dataLength, out int pCompCode, out int pReason);
        void SPIGet(Hconn hConn, Hobj hObj, ref MQMessageDescriptor md, ref MQGetMessageOptions gmo, ref MQSPIGetOpts spigo, int bufferLength, byte[] buffer, out int dataLength, out int compCode, out int reason);
        void spiNotify(Hconn hconn, ref int options, LpiNotifyDetails notifyDetails, out int pCompCode, out int pReason);
        void SpiOpen(Hconn hconn, ref MQObjectDescriptor od, ref SpiOpenOptions options, Phobj pHobj, out int pCompCode, out int pReason);
        void SPIPut(Hconn hConn, Hobj hObj, ref MQMessageDescriptor md, ref MQPutMessageOptions pmo, ref MQSPIPutOpts spipo, int length, byte[] buffer, out int compCode, out int reason);
        void SPIQuerySPI(Hconn hConn, int verbId, ref int maxInOutVersion, ref int maxInVersion, ref int maxOutVersion, ref int flags, out int compCode, out int reason);
        void spiSubscribe(Hconn Hconn, LpiSD plpiSD, MQSubscriptionDescriptor pSubDesc, Phobj pHobj, Phobj pHsub, out int pCompCode, out int pReason);
        void SPISyncpoint(Hconn hConn, ref MQSPISyncpointOpts spispo, out int compCode, out int reason);
        void zstMQGET(Hconn hConn, Hobj hObj, ref MQMessageDescriptor md, ref MQGetMessageOptions gmo, int bufferLength, byte[] buffer, out int dataLength, MQLPIGetOpts lpiGetOpts, out int compCode, out int reason);
    }
}

