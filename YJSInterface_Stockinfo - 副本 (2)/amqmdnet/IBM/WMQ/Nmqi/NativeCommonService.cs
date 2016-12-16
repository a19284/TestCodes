namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Runtime.InteropServices;

    public class NativeCommonService
    {
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        [DllImport("mqecs.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern uint xcsConvertOOString_e(int fromCCSID, int toCCSID, [In] byte[] inString, int inLength, [Out] byte[] outString, ref int outLength, int options, ref uint bytesConverted);
        [DllImport("mqecs.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern uint xcsDisplayCopyright_e();
        [DllImport("mqecs.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern uint xcsDisplayDotNetMessage_e([In] byte[] qmgrName, uint returncode, MQCommonServices.xcsINSERTS inserts, uint mtype, [In] byte[] file, int line);
        [DllImport("mqecs.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern unsafe uint xcsDNInitTerm_e(uint flags, MQCommonServices.xcsStdCallDump_t function, ref int* ppTraceStatus);
        [DllImport("mqecs.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern unsafe uint xcsFFSTFn_e(ushort component, ushort function, uint probeid, uint tag, MQCommonServices.xcsINSERTS inserts, void* ptr_dump_data, ushort alert_num, byte[] message_key);
        [DllImport("mqecs.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern unsafe uint xcsGetEnvironmentString_e(byte* var, byte* buffer, ref ushort length);
        [DllImport("mqecs.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern uint xcsGetMessageParts_e(uint returncode, uint control, MQCommonServices.xcsINSERTS inserts, ref int ccsid, [Out] byte[] basicbuffer, ref int basicbufferlen, [Out] byte[] extendedbuffer, ref int extendedbufferlen, [Out] byte[] replybuffer, ref int replybufferlen);
        [DllImport("mqecs.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern unsafe int xcsPrintLines_e(ref MQCommonServices.xcsDumpCtrl DumpCtrl, byte* pString);
        [DllImport("mqecs.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern unsafe uint xcsSaveFileContextSccsId_e(byte* filename, int lineNo);
        [DllImport("mqecs.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern unsafe void xtr_data_e(ushort comp, ushort func, void* ptr, uint length);
        [DllImport("mqecs.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern void xtr_fnc_entryFn_e(uint component, uint module, int init);
        [DllImport("mqecs.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern void xtr_fnc_retcodeFn_e(uint component, uint module, uint rc);
        [DllImport("mqecs.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern unsafe uint xtr_printfFn_e(uint component, uint module, ushort detail, byte* pFormatString);
        [DllImport("mqecs.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern unsafe void xtrFormatRC_e(byte* pBuffer, int reason);
        [DllImport("mqecs.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern unsafe uint xtrGetFunction_e(uint comp, ushort func, out byte* pBuffer);
    }
}

