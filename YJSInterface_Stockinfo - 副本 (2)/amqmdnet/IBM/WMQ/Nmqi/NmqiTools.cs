namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using Microsoft.Win32;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    public class NmqiTools : MQBase
    {
        private const int CONVERT_MESSAGE = 2;
        private static readonly char[] digits = "0123456789ABCDEF".ToCharArray();
        private const int GET_MESSAGE = 1;
        private const int QUIT = 0;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public static string ArrayToHexString(byte[] array)
        {
            if (array != null)
            {
                StringBuilder builder = new StringBuilder(array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    int num = (array[i] & 240) >> 4;
                    char ch = (num > 9) ? ((char) (0x41 + (num - 10))) : ((char) (0x30 + num));
                    builder.Append(ch);
                    num = array[i] & 15;
                    ch = (num > 9) ? ((char) (0x41 + (num - 10))) : ((char) (0x30 + num));
                    builder.Append(ch);
                }
                return builder.ToString();
            }
            return "<null>";
        }

        public static string ArrayToHexString(sbyte[] array)
        {
            if (array != null)
            {
                StringBuilder builder = new StringBuilder(array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    int num = (array[i] & 240) >> 4;
                    char ch = (num > 9) ? ((char) (0x41 + (num - 10))) : ((char) (0x30 + num));
                    builder.Append(ch);
                    num = array[i] & 15;
                    ch = (num > 9) ? ((char) (0x41 + (num - 10))) : ((char) (0x30 + num));
                    builder.Append(ch);
                }
                return builder.ToString();
            }
            return "<null>";
        }

        public static void ByteArrayCopy(byte[] source, int sourceOffset, byte[] target, int targetOffset, int dataLength)
        {
            for (int i = 0; i < dataLength; i++)
            {
                if ((source != null) && (source.Length > (sourceOffset + i)))
                {
                    target[targetOffset + i] = source[sourceOffset + i];
                }
                else
                {
                    target[targetOffset + i] = 0;
                }
            }
        }

        public static bool Compare(byte[] arr1, byte[] arr2)
        {
            if ((arr1 == null) && (arr2 == null))
            {
                return true;
            }
            if ((arr1 == null) && (arr2 != null))
            {
                return false;
            }
            if ((arr1 != null) && (arr2 == null))
            {
                return false;
            }
            if (arr1.Length != arr2.Length)
            {
                return false;
            }
            for (int i = 0; i < arr1.Length; i++)
            {
                if (arr1[i] != arr2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static string Fix(int number, int limit)
        {
            string str = Convert.ToString(number);
            string str2 = str;
            if (str.Length < limit)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = str.Length; i < limit; i++)
                {
                    builder.Append('0');
                }
                builder.Append(str);
                return builder.ToString();
            }
            if (str.Length > limit)
            {
                str2 = str.Substring(0, limit);
            }
            return str2;
        }

        internal static long GetCurrentTimeInMs()
        {
            return (DateTime.Now.ToUniversalTime().Ticks / 0x2710L);
        }

        public static string GetExSumm(Exception e)
        {
            string str = null;
            if (e != null)
            {
                str = e.GetType().FullName + "[" + e.Message + "]";
            }
            return str;
        }

        public static byte[] GetMessage(NmqiEnvironment env, NmqiSP sp, Hconn hconn, Hobj hobj, MQMessageDescriptor md, MQGetMessageOptions gmo, int expectedMsgLength, int maxMsgLength, byte[] buffer, int msgTooSmallForBufferCount, out int dataLength, out int compCode, out int reason)
        {
            int num = 0x1000;
            int num2 = 10;
            int num3 = (expectedMsgLength < 0) ? num : expectedMsgLength;
            num3 = Math.Min(num3, maxMsgLength);
            if ((buffer == null) || (buffer.Length < num3))
            {
                buffer = new byte[num3];
            }
            int encoding = md.Encoding;
            int ccsid = md.Ccsid;
            byte[] msgId = md.MsgId;
            byte[] correlId = md.CorrelId;
            int options = gmo.Options;
            bool callExitOnLenErr = false;
            int returnedLength = 0;
            int bufferLength = 0;
            byte[] buffer4 = null;
            dataLength = 0;
            compCode = 0;
            reason = 0;
            int num10 = 1;
            while (num10 != 0)
            {
                int num12;
                switch (num10)
                {
                    case 1:
                        bufferLength = Math.Min(buffer.Length, maxMsgLength);
                        sp.NmqiGetMessage(hconn, hobj, md, gmo, bufferLength, buffer, out dataLength, out compCode, out reason);
                        num12 = reason;
                        if (num12 > 0x7da)
                        {
                            break;
                        }
                        switch (num12)
                        {
                            case 0:
                                goto Label_0100;

                            case 0x7da:
                                goto Label_0108;
                        }
                        goto Label_01CC;

                    case 2:
                    {
                        byte[] source = buffer;
                        byte[] structure = new byte[bufferLength];
                        IntPtr zero = IntPtr.Zero;
                        Marshal.StructureToPtr(structure, zero, false);
                        Marshal.Copy(source, 0, zero, dataLength);
                        buffer = structure;
                        int availableLength = dataLength;
                        if (!sp.NmqiConvertMessage(hconn, hobj, encoding, ccsid, options, callExitOnLenErr, md, buffer, out dataLength, availableLength, bufferLength, out compCode, out reason, out returnedLength))
                        {
                            goto Label_0232;
                        }
                        num10 = 0;
                        continue;
                    }
                    default:
                    {
                        continue;
                    }
                }
                switch (num12)
                {
                    case 0x820:
                    {
                        msgTooSmallForBufferCount = 0;
                        if (dataLength >= maxMsgLength)
                        {
                            goto Label_019C;
                        }
                        if (bufferLength < dataLength)
                        {
                            bufferLength = Math.Min(dataLength, maxMsgLength);
                        }
                        else
                        {
                            bufferLength = Math.Min(bufferLength * 2, maxMsgLength);
                        }
                        buffer4 = new byte[bufferLength];
                        buffer = buffer4;
                        md.Encoding = encoding;
                        md.Ccsid = ccsid;
                        md.MsgId = msgId;
                        md.CorrelId = correlId;
                        num10 = 1;
                        continue;
                    }
                    case 0x848:
                    case 0x88e:
                    {
                        msgTooSmallForBufferCount = 0;
                        if (dataLength < maxMsgLength)
                        {
                            bufferLength = Math.Min(dataLength * 2, maxMsgLength);
                            num10 = 2;
                        }
                        else
                        {
                            num10 = 0;
                        }
                        continue;
                    }
                    default:
                        goto Label_01CC;
                }
            Label_0100:
                num10 = 0;
                continue;
            Label_0108:
                maxMsgLength = dataLength;
                md.Encoding = encoding;
                md.Ccsid = ccsid;
                md.MsgId = msgId;
                md.CorrelId = correlId;
                num10 = 1;
                continue;
            Label_019C:
                num10 = 0;
                continue;
            Label_01CC:
                num10 = 0;
                continue;
            Label_0232:
                switch (reason)
                {
                    case 0:
                    {
                        num10 = 0;
                        continue;
                    }
                    case 0x848:
                    case 0x88e:
                    {
                        if (dataLength < maxMsgLength)
                        {
                            bufferLength = Math.Min(bufferLength * 2, maxMsgLength);
                            num10 = 2;
                        }
                        else
                        {
                            num10 = 0;
                        }
                        continue;
                    }
                }
                num10 = 0;
            }
            byte[] buffer7 = buffer;
            if ((compCode == 0) || (compCode == 1))
            {
                if ((bufferLength > (dataLength * 2)) && (bufferLength > num))
                {
                    msgTooSmallForBufferCount++;
                }
                else
                {
                    msgTooSmallForBufferCount = 0;
                }
                if (msgTooSmallForBufferCount > num2)
                {
                    buffer = null;
                    msgTooSmallForBufferCount = 0;
                }
            }
            return buffer7;
        }

        public static QueueManagerInfo GetQueueManagerInfo(NmqiEnvironment env, NmqiMQ mq, Hconn hconn)
        {
            QueueManagerInfo info = env.NewQueueManagerInfo();
            MQObjectDescriptor pObjDesc = env.NewMQOD();
            pObjDesc.ObjectType = 5;
            int options = 0x20;
            Phobj pHobj = env.NewPhobj();
            int pCompCode = 0;
            int pReason = 0;
            mq.MQOPEN(hconn, ref pObjDesc, options, pHobj, out pCompCode, out pReason);
            if (pReason == 0)
            {
                Hobj hOBJ = pHobj.HOBJ;
                int[] pSelectors = new int[] { 0x1f, 0x20, 2, 0x7df, 0x7f0 };
                int[] pIntAttrs = new int[3];
                byte[] pCharAttrs = new byte[0x60];
                mq.MQINQ(hconn, hOBJ, pSelectors.Length, pSelectors, pIntAttrs.Length, pIntAttrs, pCharAttrs.Length, pCharAttrs, out pCompCode, out pReason);
                if (pReason == 0)
                {
                    info.CommandLevel = pIntAttrs[0];
                    info.Platform = pIntAttrs[1];
                    info.Ccsid = pIntAttrs[2];
                    Encoding dotnetEncoding = MQCcsidTable.GetDotnetEncoding(info.Ccsid);
                    info.Name = dotnetEncoding.GetString(pCharAttrs, 0, 0x30);
                    info.Uid = dotnetEncoding.GetString(pCharAttrs, 0x30, 0x30);
                }
                else
                {
                    NmqiException exception = new NmqiException(env, 0x253a, null, pCompCode, pReason, null);
                    env.LastException = exception;
                    throw exception;
                }
                mq.MQCLOSE(hconn, pHobj, 0, out pCompCode, out pReason);
                return info;
            }
            NmqiException exception2 = new NmqiException(env, 0x2525, null, pCompCode, pReason, null);
            env.LastException = exception2;
            throw exception2;
        }

        public static object GetRegistryProperty(string name)
        {
            return Registry.CurrentUser.CreateSubKey(name).GetValue(name);
        }

        internal static string GetTranslatedExceptionMessage(string objectId, uint returnCode, uint insert)
        {
            string str;
            string str2;
            string str3;
            DateTime now = DateTime.Now;
            int extendedLength = 1;
            CommonServices.SetValidInserts();
            CommonServices.ArithInsert1 = insert;
            uint control = 2;
            CommonServices.GetMessage(objectId, returnCode, control, out str, out str2, out str3, 1, extendedLength, extendedLength);
            return str;
        }

        public static string GetUserHome()
        {
            return Environment.GetEnvironmentVariable("HOMEPATH");
        }

        public static string GetUsername()
        {
            return Environment.UserName;
        }

        public static byte[] GetUtf8Bytes(NmqiEnvironment env, string str, string name)
        {
            if (str == null)
            {
                return new byte[0];
            }
            try
            {
                return Encoding.GetEncoding("UTF-8").GetBytes(str);
            }
            catch (IOException exception)
            {
                string[] strArray2 = new string[3];
                strArray2[0] = GetExSumm(exception);
                strArray2[2] = exception.Source;
                string[] inserts = strArray2;
                NmqiException exception2 = new NmqiException(env, 0x254a, inserts, 2, 0x893, exception);
                env.LastException = exception2;
                throw exception2;
            }
            return null;
        }

        public static void HexDump(byte[] byteArray, int offset, int length, StringBuilder dumpBuffer)
        {
            if (length > 0x10000)
            {
                length = 0x10000;
            }
            char[][] chArray = new char[2][];
            for (int i = 0; i < 2; i++)
            {
                chArray[i] = new char[0x3f];
            }
            for (int j = 0; j < 2; j++)
            {
                chArray[j][0] = '0';
                chArray[j][1] = 'x';
                chArray[j][6] = ' ';
                chArray[j][7] = ' ';
                chArray[j][0x10] = ' ';
                chArray[j][0x19] = ' ';
                chArray[j][0x22] = ' ';
                chArray[j][0x2b] = ' ';
                chArray[j][0x2c] = ' ';
                chArray[j][0x2d] = '|';
                chArray[j][0x3e] = '|';
            }
            if (((offset >= 0) && (length > 0)) && (byteArray.Length >= (offset + length)))
            {
                int num3 = 0;
                int num5 = offset;
                int num7 = num5 % 0x10;
                int num8 = length;
                int num14 = 0;
                while (num8 > 0)
                {
                    int num12;
                    int num13;
                    int index = num3 % 2;
                    int num4 = (num8 >= 0x10) ? 0x10 : num8;
                    string str = Convert.ToString(num5, 0x10);
                    int num6 = str.Length;
                    int num9 = 0;
                    while (num9 < 4)
                    {
                        if (num9 < num6)
                        {
                            chArray[index][5 - num9] = str[(num6 - num9) - 1];
                        }
                        else
                        {
                            chArray[index][5 - num9] = '0';
                        }
                        num9++;
                    }
                    num9 = 0;
                    while (num9 < num7)
                    {
                        num12 = (8 + (num9 / 4)) + (num9 * 2);
                        num13 = 0x2e + num9;
                        chArray[index][num12] = ' ';
                        chArray[index][num12 + 1] = ' ';
                        chArray[index][num13] = ' ';
                        num9++;
                    }
                    while (num9 < 0x10)
                    {
                        num12 = (8 + (num9 / 4)) + (num9 * 2);
                        num13 = 0x2e + num9;
                        if (num9 < num4)
                        {
                            byte num10 = byteArray[num5 + num9];
                            int num11 = (num10 & 240) >> 4;
                            chArray[index][num12] = (num11 > 9) ? ((char) (0x61 + (num11 - 10))) : ((char) (0x30 + num11));
                            num11 = num10 & 15;
                            chArray[index][num12 + 1] = (num11 > 9) ? ((char) (0x61 + (num11 - 10))) : ((char) (0x30 + num11));
                            num11 = num10 & 0xff;
                            if ((num11 >= 0x20) && (num11 < 0x7f))
                            {
                                chArray[index][num13] = (char) num11;
                            }
                            else
                            {
                                chArray[index][num13] = '.';
                            }
                        }
                        else
                        {
                            chArray[index][num12] = ' ';
                            chArray[index][num12 + 1] = ' ';
                            chArray[index][num13] = ' ';
                        }
                        num9++;
                    }
                    num5 += 0x10;
                    num8 -= num4;
                    int num16 = (num3 + 1) % 2;
                    bool flag = false;
                    if (num3 != 0)
                    {
                        flag = true;
                        for (num9 = 8; flag && (num9 <= 0x2a); num9++)
                        {
                            flag = flag && (chArray[index][num9] == chArray[num16][num9]);
                        }
                    }
                    if (flag)
                    {
                        num14++;
                    }
                    if (!flag || (num8 == 0))
                    {
                        if (num14 == 1)
                        {
                            flag = false;
                        }
                        else if (num14 > 0)
                        {
                            dumpBuffer.Append(Environment.NewLine);
                            dumpBuffer.Append(num14);
                            dumpBuffer.Append(" lines suppressed, same as above");
                        }
                        num14 = 0;
                        if (!flag)
                        {
                            if (num3 != 0)
                            {
                                dumpBuffer.Append(Environment.NewLine);
                            }
                            dumpBuffer.Append(chArray[index]);
                        }
                    }
                    num7 = 0;
                    num3++;
                }
            }
        }

        public static byte[] HexToBin(string hex)
        {
            int length = hex.Length;
            if (length == 0)
            {
                return new byte[0];
            }
            if ((length < 0) && ((length % 2) != 0))
            {
                throw new Exception("Bad hex string.");
            }
            length /= 2;
            byte[] buffer = new byte[length];
            try
            {
                for (int i = 0; i < length; i++)
                {
                    string str = hex.Substring(2 * i, 2);
                    buffer[i] = Convert.ToByte(str, 0x10);
                }
            }
            catch (Exception)
            {
                throw new Exception("Invalid hex string conversion.");
            }
            return buffer;
        }

        internal static bool IsDispatcherThread()
        {
            bool flag = false;
            object data = Thread.GetData(Thread.GetNamedDataSlot("MQ_CLIENT_THREAD_TYPE"));
            if ((data != null) && (Convert.ToInt32(data) == 2))
            {
                flag = true;
            }
            return flag;
        }

        public static string Left(string s, int width)
        {
            return Left(s, width, ' ');
        }

        public static string Left(string s, int width, char fillChar)
        {
            if (s.Length >= width)
            {
                return s;
            }
            StringBuilder builder = new StringBuilder(width);
            builder.Append(s);
            int num = width - s.Length;
            while (--num >= 0)
            {
                builder.Append(fillChar);
            }
            return builder.ToString();
        }

        public static int Padding(int ptrSize, int padding4, int padding8, int padding16)
        {
            switch (ptrSize)
            {
                case 4:
                    return padding4;

                case 8:
                    return padding8;

                case 0x10:
                    return padding16;
            }
            return -1;
        }

        public static string QmgrBytesToString(Hconn hconn, byte[] charAttrs, int offset, int length)
        {
            return MQCcsidTable.GetDotnetEncoding(hconn.Ccsid).GetString(charAttrs, offset, length).Trim();
        }

        public static object ReadStruct(ref byte[] b, Type structType, ref int offset, int size)
        {
            IntPtr zero = IntPtr.Zero;
            object obj2 = null;
            zero = Marshal.AllocCoTaskMem(size);
            Marshal.Copy(b, offset, zero, size);
            obj2 = Marshal.PtrToStructure(zero, structType);
            Marshal.FreeCoTaskMem(zero);
            offset += size;
            return obj2;
        }

        public static string Right(string s, int width)
        {
            return Right(s, width, ' ');
        }

        public static string Right(string s, int width, char fillChar)
        {
            if (s.Length >= width)
            {
                return s;
            }
            StringBuilder builder = new StringBuilder(width);
            int num = width - s.Length;
            while (--num >= 0)
            {
                builder.Append(fillChar);
            }
            builder.Append(s);
            return builder.ToString();
        }

        public static string SafeString(object obj)
        {
            if (obj != null)
            {
                return obj.ToString();
            }
            return "<null>";
        }

        public static byte[] StringToQmgrBytes(NmqiEnvironment env, Hconn hconn, string str, byte[] charAttrs, int offset, int length)
        {
            Encoding dotnetEncoding = MQCcsidTable.GetDotnetEncoding(hconn.Ccsid);
            byte[] bytes = dotnetEncoding.GetBytes(str);
            if (bytes.Length > length)
            {
                NmqiException exception = new NmqiException(env, -1, null, 2, 0x7d5, null);
                env.LastException = exception;
                throw exception;
            }
            if ((offset + length) > charAttrs.Length)
            {
                NmqiException exception2 = new NmqiException(env, -1, null, 2, 0x7d5, null);
                env.LastException = exception2;
                throw exception2;
            }
            for (int i = 0; i < bytes.Length; i++)
            {
                charAttrs[offset + i] = bytes[i];
            }
            byte[] buffer2 = dotnetEncoding.GetBytes(" ");
            for (int j = bytes.Length; j < length; j++)
            {
                charAttrs[offset + j] = buffer2[0];
            }
            return bytes;
        }

        public static string ToString(byte[] frame, int offset, int length)
        {
            if (frame == null)
            {
                return null;
            }
            StringBuilder builder = new StringBuilder();
            int num = offset + length;
            while (offset < num)
            {
                builder.Append(digits[URShift(frame[offset], 4) & 15]);
                builder.Append(digits[frame[offset++] & 15]);
            }
            return builder.ToString();
        }

        public static string TracePassword(string password)
        {
            StringBuilder builder = new StringBuilder();
            if (password == null)
            {
                builder.Append("<null>");
            }
            else
            {
                builder.Append("length:");
                builder.Append(password.Length);
            }
            return builder.ToString();
        }

        public static int URShift(int number, int bits)
        {
            if (number >= 0)
            {
                return (number >> bits);
            }
            return ((number >> bits) + (((int) 2) << ~bits));
        }

        public static void WriteNativeNullPointerToBuffer(ref byte[] buffer, int offset, int sizeofNativePointer)
        {
            for (int i = 0; i < sizeofNativePointer; i++)
            {
                buffer[offset + i] = 0;
            }
        }

        public static int WriteStruct(ref byte[] b, int offset, object structure, int size)
        {
            IntPtr zero = IntPtr.Zero;
            zero = Marshal.AllocCoTaskMem(size);
            Marshal.StructureToPtr(structure, zero, false);
            Marshal.Copy(zero, b, offset, size);
            Marshal.FreeCoTaskMem(zero);
            return (offset + size);
        }
    }
}

