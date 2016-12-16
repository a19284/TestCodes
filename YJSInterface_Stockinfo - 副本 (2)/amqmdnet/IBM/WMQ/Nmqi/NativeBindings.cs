namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Runtime.InteropServices;

    public abstract class NativeBindings : MQBase
    {
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        protected NativeBindings()
        {
        }

        [DllImport("mqz.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void lpiSPIActivateMessage(int hConn, ref LPIACTIVATE ActivateOpts, out int compCode, out int reason);
        [DllImport("mqz.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void lpiSPIGet(int hConn, int hObj, ref MQBase.MQMD mqmd, ref MQBase.MQGMO mqgmo, int bufferLength, [Out] byte[] buffer, out int dataLength, ref LPIGETOPT GetData, out int compCode, out int reason);
        [DllImport("mqz.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void lpiSPINotify(int hconn, int options, ref MQBase.structLPINOTIFYDETAILS notifyDetails, out int compCode, out int reason);
        [DllImport("mqz.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void lpiSPIOpen(int hConn, IntPtr ObjDesc, IntPtr Options, out int pHobj, out int pCompCode, out int pReason);
        [DllImport("mqz.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void lpiSPIPut(int hConn, int hObj, ref MQBase.MQMD mqmd, ref MQBase.MQPMO mqpmo, int bufferLength, [In] byte[] buffer, ref LPIPUTOPTS PutOptsIn, out int compCode, out int reason);
        [DllImport("mqz.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void lpiSPISubscribe(int hConn, IntPtr lpiSD, IntPtr subDesc, ref int pHobj, out int pHsub, out int pCompCode, out int pReason);
        [DllImport("mqz.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void lpiSPISyncPoint(int hConn, ref LPISPO SyncPointOpts, out int compCode, out int reason);
        [DllImport("mqm.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQBACK(int hconn, out int pCompCode, out int pReason);
        [DllImport("mqm.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQBEGIN(int hconn, ref MQBase.MQBO mqBO, out int pCompCode, out int pReason);
        [DllImport("mqm.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern void MQCLOSE(int hConn, ref int hObj, int options, out int compCode, out int reason);
        [DllImport("mqm.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQCMIT(int hconn, out int pCompCode, out int pReason);
        [DllImport("mqm.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern void MQCONN(string qMgrName, out int pHconn, out int pCompCode, out int pReason);
        [DllImport("mqm.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern void MQCONNX(string qMgrName, ref MQBase.MQCNO mqcno, out int pHconn, out int pCompCode, out int pReason);
        [DllImport("mqm.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQCTL(int hConn, int operation, IntPtr pControlOpts, out int compCode, out int reason);
        [DllImport("mqm.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQDISC(out int hConn, out int compCode, out int reason);
        [DllImport("mqm.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQGET(int hConn, int hObj, ref MQBase.MQMD mqmd, ref MQBase.MQGMO mqgmo, int bufferLength, [Out] byte[] buffer, out int dataLength, out int compCode, out int reason);
        [DllImport("mqm.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQINQ(int hconn, int hobj, int SelectorCount, [In] int[] pSelectors, int IntAttrCount, [Out] int[] pIntAttrs, int CharAttrLength, [Out] byte[] pCharAttrs, out int pCompCode, out int pReason);
        [DllImport("mqm.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQOPEN(int hConn, ref MQBase.MQOD mqod, int options, out int hObj, out int compCode, out int reason);
        [DllImport("mqm.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQOPEN(int hConn, IntPtr odPtr, int options, out int hObj, out int compCode, out int reason);
        [DllImport("mqm.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQPUT(int hConn, int hObj, ref MQBase.MQMD mqmd, ref MQBase.MQPMO mqpmo, int length, [In] byte[] buffer, out int compCode, out int reason);
        [DllImport("mqm.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQPUT1(int hConn, ref MQBase.MQOD mqod, ref MQBase.MQMD mqmd, ref MQBase.MQPMO mqpmo, int length, [In] byte[] buffer, out int compCode, out int reason);
        [DllImport("mqm.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQPUT1(int hConn, IntPtr mqodPtr, ref MQBase.MQMD mqmd, ref MQBase.MQPMO mqpmo, int length, [In] byte[] buffer, out int compCode, out int reason);
        [DllImport("mqm.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQSET(int hconn, int hobj, int SelectorCount, [In] int[] pSelectors, int IntAttrCount, [In] int[] pIntAttrs, int CharAttrLength, [In] byte[] pCharAttrs, out int pCompCode, out int pReason);
        [DllImport("mqm.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQSTAT(int hconn, int Type, ref MQBase.MQSTS Stat, out int pCompCode, out int pReason);
        [DllImport("mqm.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQSUB(int hConn, ref MQBase.MQSD mqsd, ref int hObj, out int hSub, out int compCode, out int reason);
        [DllImport("mqm.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQSUB(int hConn, IntPtr sdPtr, ref int hObj, out int hSub, out int compCode, out int reason);
        [DllImport("mqm.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void MQSUBRQ(int hConn, int hSub, int action, ref MQBase.MQSRO mqsro, out int compCode, out int reason);
        [DllImport("amqmdnac.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void NativeACMQCB(int hConn, int operation, int hObj, IntPtr mqcbd, IntPtr mqmd, IntPtr mqgmo, MQBase.CallbackDelegate myDelegate, int bindingsMode, out int compCode, out int reason);
        [DllImport("mqz.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern void zstMQGET(int hConn, int hObj, ref MQBase.MQMD mqmd, ref MQBase.MQGMO mqgmo, int bufferLength, [Out] byte[] buffer, out int dataLength, ref MQBase.lpiGETOPT GetData, out int compCode, out int reason);
    }
}

