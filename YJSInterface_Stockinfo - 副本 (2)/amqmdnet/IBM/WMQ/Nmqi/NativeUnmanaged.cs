namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Runtime.InteropServices;

    public class NativeUnmanaged : MQBase
    {
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        [DllImport("mqdc.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQBACK(int hconn, out int pCompCode, out int pReason);
        [DllImport("mqdc.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern void MQCLOSE(int hConn, ref int hObj, int options, out int compCode, out int reason);
        [DllImport("mqdc.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQCMIT(int hconn, out int pCompCode, out int pReason);
        [DllImport("mqdc.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern void MQCONN(string qMgrName, out int pHconn, out int pCompCode, out int pReason);
        [DllImport("mqdc.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern void MQCONNX(string qMgrName, ref MQBase.MQCNO mqcno, out int pHconn, out int pCompCode, out int pReason);
        [DllImport("mqdc.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQCTL(int hConn, int operation, IntPtr pControlOpts, out int compCode, out int reason);
        [DllImport("mqdc.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQDISC(out int hConn, out int compCode, out int reason);
        [DllImport("mqdc.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQGET(int hConn, int hObj, ref MQBase.MQMD mqmd, ref MQBase.MQGMO mqgmo, int bufferLength, [Out] byte[] buffer, out int dataLength, out int compCode, out int reason);
        [DllImport("mqdc.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQINQ(int hconn, int hobj, int SelectorCount, [In] int[] pSelectors, int IntAttrCount, [Out] int[] pIntAttrs, int CharAttrLength, [Out] byte[] pCharAttrs, out int pCompCode, out int pReason);
        [DllImport("mqdc.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQOPEN(int hConn, ref MQBase.MQOD mqod, int options, out int hObj, out int compCode, out int reason);
        [DllImport("mqdc.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQOPEN(int hConn, IntPtr odPtr, int options, out int hObj, out int compCode, out int reason);
        [DllImport("mqdc.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQPUT(int hConn, int hObj, ref MQBase.MQMD mqmd, ref MQBase.MQPMO mqpmo, int length, [In] byte[] buffer, out int compCode, out int reason);
        [DllImport("mqdc.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQPUT1(int hConn, ref MQBase.MQOD mqod, ref MQBase.MQMD mqmd, ref MQBase.MQPMO mqpmo, int length, [In] byte[] buffer, out int compCode, out int reason);
        [DllImport("mqdc.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQPUT1(int hConn, IntPtr mqodPtr, ref MQBase.MQMD mqmd, ref MQBase.MQPMO mqpmo, int length, [In] byte[] buffer, out int compCode, out int reason);
        [DllImport("mqdc.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQSET(int hconn, int hobj, int SelectorCount, [In] int[] pSelectors, int IntAttrCount, [In] int[] pIntAttrs, int CharAttrLength, [In] byte[] pCharAttrs, out int pCompCode, out int pReason);
        [DllImport("mqdc.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQSTAT(int hconn, int Type, ref MQBase.MQSTS Stat, out int pCompCode, out int pReason);
        [DllImport("mqdc.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQSUB(int hConn, ref MQBase.MQSD mqsd, ref int hObj, out int hSub, out int compCode, out int reason);
        [DllImport("mqdc.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQSUB(int hConn, IntPtr sdPtr, ref int hObj, out int hSub, out int compCode, out int reason);
        [DllImport("mqdc.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQSUBRQ(int hConn, int hSub, int action, ref MQBase.MQSRO mqsro, out int compCode, out int reason);
        [DllImport("amqmdnac.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void NativeACMQCB(int hConn, int operation, int hObj, IntPtr mqcbd, IntPtr mqmd, IntPtr mqgmo, MQBase.CallbackDelegate myDelegate, int bindingsMode, out int compCode, out int reason);
        [DllImport("amqmdnac.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void nmqiMultiBufMQPut(int hConn, int hObj, ref MQBase.MQMD mqmd, ref MQBase.MQPMO mqpmo, int numBuffers, [In] byte[] buffer1, int length1, [In] byte[] buffer2, int length2, [In] byte[] buffer3, int length3, [In] byte[] buffer4, int length4, [In] byte[] buffer5, int length5, [In] byte[] buffer6, int length6, out int compCode, out int reason);
        [DllImport("amqmdnac.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void nmqiMultiBufMQPut1(int hConn, ref MQBase.MQOD mqod, ref MQBase.MQMD mqmd, ref MQBase.MQPMO mqpmo, int numBuffers, [In] byte[] buffer1, int length1, [In] byte[] buffer2, int length2, [In] byte[] buffer3, int length3, [In] byte[] buffer4, int length4, [In] byte[] buffer5, int length5, [In] byte[] buffer6, int length6, out int compCode, out int reason);
        [DllImport("mqz.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void zstMQGET(int hConn, int hObj, ref MQBase.MQMD md, ref MQBase.MQGMO gmo, int bufferLength, byte[] buffer, out int dataLength, ref MQBase.lpiGETOPT lpiGetOpts, out int compCode, out int reason);
        [DllImport("mqz.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void zstSPI(int hConn, int verbId, int hObj, [In, Out] byte[] pInOut, [In] byte[] pIn, [Out] byte[] pOut, out int compCode, out int reason);
        [DllImport("mqz.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void zstSPINewHObj(int hConn, int verbId, ref int hObj, [In, Out] byte[] pInOut, [In] byte[] pIn, [Out] byte[] pOut, out int compCode, out int reason);
    }
}

