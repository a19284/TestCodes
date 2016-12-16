namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;

    internal class MQManagedPPA : MQBase
    {
        private static int bs = 8;
        private static byte[] E = new byte[] { 
            0x20, 1, 2, 3, 4, 5, 4, 5, 6, 7, 8, 9, 8, 9, 10, 11, 
            12, 13, 12, 13, 14, 15, 0x10, 0x11, 0x10, 0x11, 0x12, 0x13, 20, 0x15, 20, 0x15, 
            0x16, 0x17, 0x18, 0x19, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1c, 0x1d, 30, 0x1f, 0x20, 1
         };
        private static byte[] IP = new byte[] { 
            0x3a, 50, 0x2a, 0x22, 0x1a, 0x12, 10, 2, 60, 0x34, 0x2c, 0x24, 0x1c, 20, 12, 4, 
            0x3e, 0x36, 0x2e, 0x26, 30, 0x16, 14, 6, 0x40, 0x38, 0x30, 40, 0x20, 0x18, 0x10, 8, 
            0x39, 0x31, 0x29, 0x21, 0x19, 0x11, 9, 1, 0x3b, 0x33, 0x2b, 0x23, 0x1b, 0x13, 11, 3, 
            0x3d, 0x35, 0x2d, 0x25, 0x1d, 0x15, 13, 5, 0x3f, 0x37, 0x2f, 0x27, 0x1f, 0x17, 15, 7
         };
        private static byte[] IP_1 = new byte[] { 
            40, 8, 0x30, 0x10, 0x38, 0x18, 0x40, 0x20, 0x27, 7, 0x2f, 15, 0x37, 0x17, 0x3f, 0x1f, 
            0x26, 6, 0x2e, 14, 0x36, 0x16, 0x3e, 30, 0x25, 5, 0x2d, 13, 0x35, 0x15, 0x3d, 0x1d, 
            0x24, 4, 0x2c, 12, 0x34, 20, 60, 0x1c, 0x23, 3, 0x2b, 11, 0x33, 0x13, 0x3b, 0x1b, 
            0x22, 2, 0x2a, 10, 50, 0x12, 0x3a, 0x1a, 0x21, 1, 0x29, 9, 0x31, 0x11, 0x39, 0x19
         };
        private static int kl = 8;
        private static byte[] P = new byte[] { 
            0x10, 7, 20, 0x15, 0x1d, 12, 0x1c, 0x11, 1, 15, 0x17, 0x1a, 5, 0x12, 0x1f, 10, 
            2, 8, 0x18, 14, 0x20, 0x1b, 3, 9, 0x13, 13, 30, 6, 0x16, 11, 4, 0x19
         };
        private static byte[] PC_1C = new byte[] { 
            0x39, 0x31, 0x29, 0x21, 0x19, 0x11, 9, 1, 0x3a, 50, 0x2a, 0x22, 0x1a, 0x12, 10, 2, 
            0x3b, 0x33, 0x2b, 0x23, 0x1b, 0x13, 11, 3, 60, 0x34, 0x2c, 0x24
         };
        private static byte[] PC_1D = new byte[] { 
            0x3e, 0x37, 0x2f, 0x27, 0x1f, 0x17, 15, 7, 0x3e, 0x36, 0x2e, 0x26, 30, 0x16, 14, 6, 
            0x3d, 0x35, 0x2d, 0x25, 0x1d, 0x15, 13, 5, 0x1c, 20, 12, 4
         };
        private static byte[] PC_2C = new byte[] { 
            14, 0x11, 11, 0x18, 1, 5, 3, 0x1c, 15, 6, 0x15, 10, 0x17, 0x13, 12, 4, 
            0x1a, 8, 0x10, 7, 0x1b, 20, 13, 2
         };
        private static byte[] PC_2D = new byte[] { 
            0x29, 0x34, 0x1f, 0x25, 0x2f, 0x37, 30, 40, 0x33, 0x2d, 0x21, 0x30, 0x2c, 0x31, 0x27, 0x38, 
            0x22, 0x35, 0x2e, 0x2a, 50, 0x24, 0x1d, 0x20
         };
        private static byte[][][] S = new byte[][][] { new byte[][] { new byte[] { 14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7 }, new byte[] { 0, 15, 7, 4, 14, 2, 13, 1, 10, 6, 12, 11, 9, 5, 3, 8 }, new byte[] { 4, 1, 14, 8, 13, 6, 2, 11, 15, 12, 9, 7, 3, 10, 5, 0 }, new byte[] { 15, 12, 8, 2, 4, 9, 1, 7, 5, 11, 3, 14, 10, 0, 6, 13 } }, new byte[][] { new byte[] { 15, 1, 8, 14, 6, 11, 3, 4, 9, 7, 2, 13, 12, 0, 5, 10 }, new byte[] { 3, 13, 4, 7, 15, 2, 8, 14, 12, 0, 1, 10, 6, 9, 11, 5 }, new byte[] { 0, 14, 7, 11, 10, 4, 13, 1, 5, 8, 12, 6, 9, 3, 2, 15 }, new byte[] { 13, 8, 10, 1, 3, 15, 4, 2, 11, 6, 7, 12, 0, 5, 14, 9 } }, new byte[][] { new byte[] { 10, 0, 9, 14, 6, 3, 15, 5, 1, 13, 12, 7, 11, 4, 2, 8 }, new byte[] { 13, 7, 0, 9, 3, 4, 6, 10, 2, 8, 5, 14, 12, 11, 15, 1 }, new byte[] { 13, 6, 4, 9, 8, 15, 3, 0, 11, 1, 2, 12, 5, 10, 14, 7 }, new byte[] { 1, 10, 13, 0, 6, 9, 8, 7, 4, 15, 14, 3, 11, 5, 2, 12 } }, new byte[][] { new byte[] { 7, 13, 14, 3, 0, 6, 9, 10, 1, 2, 8, 5, 11, 12, 4, 15 }, new byte[] { 13, 8, 11, 5, 6, 15, 0, 3, 4, 7, 2, 12, 1, 10, 14, 9 }, new byte[] { 10, 6, 9, 0, 12, 11, 7, 13, 15, 1, 3, 14, 5, 2, 8, 4 }, new byte[] { 3, 15, 0, 6, 10, 1, 13, 8, 9, 4, 5, 11, 12, 7, 2, 14 } }, new byte[][] { new byte[] { 2, 12, 4, 1, 7, 10, 11, 6, 8, 5, 3, 15, 13, 0, 14, 9 }, new byte[] { 14, 11, 2, 12, 4, 7, 13, 1, 5, 0, 15, 10, 3, 9, 8, 6 }, new byte[] { 4, 2, 1, 11, 10, 13, 7, 8, 15, 9, 12, 5, 6, 3, 0, 14 }, new byte[] { 11, 8, 12, 7, 1, 14, 2, 13, 6, 15, 0, 9, 10, 4, 5, 3 } }, new byte[][] { new byte[] { 12, 1, 10, 15, 9, 2, 6, 8, 0, 13, 3, 4, 14, 7, 5, 11 }, new byte[] { 10, 15, 4, 2, 7, 12, 9, 5, 6, 1, 13, 14, 0, 11, 3, 8 }, new byte[] { 9, 14, 15, 5, 2, 8, 12, 3, 7, 0, 4, 10, 1, 13, 11, 6 }, new byte[] { 4, 3, 2, 12, 9, 5, 15, 10, 11, 14, 1, 7, 6, 0, 8, 13 } }, new byte[][] { new byte[] { 4, 11, 2, 14, 15, 0, 8, 13, 3, 12, 9, 7, 5, 10, 6, 1 }, new byte[] { 13, 0, 11, 7, 4, 9, 1, 10, 14, 3, 5, 12, 2, 15, 8, 6 }, new byte[] { 1, 4, 11, 13, 12, 3, 7, 14, 10, 15, 6, 8, 0, 5, 9, 2 }, new byte[] { 6, 11, 13, 8, 1, 4, 10, 7, 9, 5, 0, 15, 14, 2, 3, 12 } }, new byte[][] { new byte[] { 13, 2, 8, 4, 6, 15, 11, 1, 10, 9, 3, 14, 5, 0, 12, 7 }, new byte[] { 1, 15, 13, 8, 10, 3, 7, 4, 12, 5, 6, 11, 0, 14, 9, 2 }, new byte[] { 7, 11, 4, 1, 9, 12, 14, 2, 0, 6, 10, 13, 15, 3, 5, 8 }, new byte[] { 2, 1, 14, 7, 4, 10, 8, 13, 15, 12, 9, 0, 3, 5, 6, 11 } } };
        private static byte[] Shifts = new byte[] { 1, 1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 1 };
        private static byte[] yfx = new byte[] { 
            ((byte) (0xd7 ^ kl)), ((byte) (1 ^ kl)), ((byte) (0x1d ^ kl)), ((byte) (140 ^ kl)), ((byte) (0x81 ^ kl)), ((byte) (0x73 ^ kl)), ((byte) (0x76 ^ kl)), ((byte) (0xde ^ kl)), ((byte) (0xbf ^ kl)), ((byte) (0x3a ^ kl)), ((byte) (0xc9 ^ kl)), ((byte) (0x1f ^ kl)), ((byte) (0xbd ^ kl)), ((byte) (240 ^ kl)), ((byte) (0xa3 ^ kl)), ((byte) (0xb0 ^ kl)), 
            ((byte) (0xdd ^ kl)), ((byte) (0x49 ^ kl)), ((byte) (0xeb ^ kl)), ((byte) (0x13 ^ kl)), ((byte) (0xd3 ^ kl)), ((byte) (0x5c ^ kl)), ((byte) (0xa2 ^ kl)), ((byte) (0x6a ^ kl))
         };

        private static void abc(byte[] input, byte[] output, byte[][] ks)
        {
            int num;
            byte[] buffer = new byte[0x40];
            byte[] buffer2 = new byte[0x20];
            byte[] r = new byte[0x20];
            byte[] retF = new byte[0x20];
            for (num = 0; num < 0x40; num++)
            {
                buffer[num] = input[IP[num] - 1];
            }
            for (num = 0; num < 0x10; num++)
            {
                int index = 0;
                while (index < 0x20)
                {
                    buffer2[index] = buffer[index];
                    r[index] = buffer[index + 0x20];
                    index++;
                }
                byte[] buffer4 = xcsF(r, ks[num], retF);
                int num3 = 0;
                if (num < 15)
                {
                    index = 0;
                    while (index < 0x20)
                    {
                        buffer[index] = r[index];
                        buffer[index + 0x20] = (byte) (buffer2[index] ^ buffer4[num3++]);
                        index++;
                    }
                }
                else
                {
                    for (index = 0; index < 0x20; index++)
                    {
                        buffer[index] = (byte) (buffer2[index] ^ buffer4[num3++]);
                        buffer[index + 0x20] = r[index];
                    }
                }
            }
            for (num = 0; num < 0x40; num++)
            {
                output[num] = buffer[IP_1[num] - 1];
            }
        }

        private static void cba(byte[] bitData, byte[] byteData)
        {
            for (int i = 0; i < 8; i++)
            {
                int index = i * 8;
                byteData[i] = (byte) ((((((((bitData[index] << 7) | (bitData[index + 1] << 6)) | (bitData[index + 2] << 5)) | (bitData[index + 3] << 4)) | (bitData[index + 4] << 3)) | (bitData[index + 5] << 2)) | (bitData[index + 6] << 1)) | bitData[index + 7]);
            }
        }

        private static void efg(byte[] input, byte[] output, byte[][] ks)
        {
            int num;
            byte[] buffer = new byte[0x40];
            byte[] r = new byte[0x20];
            byte[] buffer3 = new byte[0x20];
            byte[] retF = new byte[0x20];
            for (num = 0; num < 0x40; num++)
            {
                buffer[num] = input[IP[num] - 1];
            }
            int index = 0;
            while (index < 0x20)
            {
                r[index] = buffer[index + 0x20];
                buffer3[index] = buffer[index];
                index++;
            }
            for (num = 15; num >= 0; num--)
            {
                byte[] buffer4 = xcsF(r, ks[num], retF);
                int num3 = 0;
                index = 0;
                while (index < 0x20)
                {
                    buffer[index] = (byte) (buffer3[index] ^ buffer4[num3++]);
                    buffer[index + 0x20] = r[index];
                    index++;
                }
                index = 0;
                while (index < 0x20)
                {
                    r[index] = buffer[index];
                    buffer3[index] = buffer[index + 0x20];
                    index++;
                }
                for (index = 0; index < 0x20; index++)
                {
                    buffer[index] = r[index];
                    buffer[index + 0x20] = buffer3[index];
                }
            }
            for (num = 0; num < 0x40; num++)
            {
                output[num] = buffer[IP_1[num] - 1];
            }
        }

        public static int FinishWritingAuthFlow(byte[] buffer, int pwlo, int pwo, byte[] r, int ppa)
        {
            int pwl = BitConverter.ToInt32(buffer, pwlo);
            if (ppa == 0)
            {
                return pwl;
            }
            if (ppa != 1)
            {
                return -1;
            }
            byte[] w = qrs(buffer, pwo, pwl);
            tuv(r, w);
            Array.Copy(w, 0, buffer, pwo, w.Length);
            return w.Length;
        }

        private static void hij(byte[] key, byte[] input, byte[] output)
        {
            byte[] buffer = new byte[0x40];
            byte[] bitData = new byte[0x40];
            byte[][] ks = new byte[0x10][];
            for (int i = 0; i < 0x10; i++)
            {
                ks[i] = new byte[0x30];
            }
            wxy(key, bitData);
            nop(bitData, ks);
            wxy(input, bitData);
            abc(bitData, buffer, ks);
            cba(buffer, output);
        }

        private static void klm(byte[] key, byte[] input, byte[] output)
        {
            byte[] buffer = new byte[0x40];
            byte[] bitData = new byte[0x40];
            byte[][] ks = new byte[0x10][];
            for (int i = 0; i < 0x10; i++)
            {
                ks[i] = new byte[0x30];
            }
            wxy(key, bitData);
            nop(bitData, ks);
            wxy(input, bitData);
            efg(bitData, buffer, ks);
            cba(buffer, output);
        }

        private static void nop(byte[] BitKey, byte[][] ks)
        {
            int num;
            byte[] buffer = new byte[0x1c];
            byte[] buffer2 = new byte[0x1c];
            for (num = 0; num < 0x1c; num++)
            {
                buffer[num] = BitKey[PC_1C[num] - 1];
                buffer2[num] = BitKey[PC_1D[num] - 1];
            }
            for (num = 0; num < 0x10; num++)
            {
                int index = 0;
                while (index < Shifts[num])
                {
                    byte num4 = buffer[0];
                    byte num5 = buffer2[0];
                    for (int i = 0; i < 0x1b; i++)
                    {
                        buffer[i] = buffer[i + 1];
                        buffer2[i] = buffer2[i + 1];
                    }
                    buffer[0x1b] = num4;
                    buffer2[0x1b] = num5;
                    index++;
                }
                for (index = 0; index < 0x18; index++)
                {
                    ks[num][index] = buffer[PC_2C[index] - 1];
                    ks[num][index + 0x18] = buffer2[(PC_2D[index] - 0x1c) - 1];
                }
            }
        }

        private static byte[] qrs(byte[] buffer, int pwo, int pwl)
        {
            int num = pwl;
            if ((num % bs) != 0)
            {
                num += bs - (num % bs);
            }
            byte[] destinationArray = new byte[num];
            Array.Copy(buffer, pwo, destinationArray, 0, pwl);
            return destinationArray;
        }

        private static void tuv(byte[] r, byte[] w)
        {
            byte[] buffer = new byte[kl];
            byte[] buffer2 = new byte[kl];
            byte[] buffer3 = new byte[kl];
            byte[] destinationArray = new byte[kl];
            byte[] buffer5 = new byte[kl];
            byte[] buffer6 = new byte[kl];
            Array.Copy(r, 0, destinationArray, 0, kl);
            Array.Copy(r, kl, buffer5, 0, kl);
            Array.Copy(r, kl * 2, buffer6, 0, kl);
            for (int i = 0; i < kl; i++)
            {
                buffer[i] = (byte) (yfx[i] ^ kl);
                if (i == 0)
                {
                    buffer[i] = (byte) ((buffer[i] & 240) | ((buffer[i] ^ destinationArray[i]) & 15));
                }
                else
                {
                    buffer[i] = (byte) (buffer[i] ^ destinationArray[i]);
                }
            }
            for (int j = 0; j < kl; j++)
            {
                buffer2[j] = (byte) (yfx[8 + j] ^ kl);
                if (j == 0)
                {
                    buffer2[j] = (byte) ((buffer2[j] & 240) | ((buffer2[j] ^ buffer5[j]) & 15));
                }
                else
                {
                    buffer2[j] = (byte) (buffer2[j] ^ buffer5[j]);
                }
            }
            for (int k = 0; k < kl; k++)
            {
                buffer3[k] = (byte) (yfx[0x10 + k] ^ kl);
                if (k == 0)
                {
                    buffer3[k] = (byte) ((buffer3[k] & 240) | ((buffer3[k] ^ buffer6[k]) & 15));
                }
                else
                {
                    buffer3[k] = (byte) (buffer3[k] ^ buffer6[k]);
                }
            }
            int num4 = w.Length / bs;
            byte[] buffer7 = new byte[bs];
            byte[] array = new byte[bs];
            byte[] key = null;
            for (int m = 0; m < 3; m++)
            {
                switch (m)
                {
                    case 0:
                        key = buffer;
                        break;

                    case 1:
                        key = buffer2;
                        break;

                    case 2:
                        key = buffer3;
                        break;
                }
                for (int n = 0; n < num4; n++)
                {
                    Array.Copy(w, n * bs, buffer7, 0, bs);
                    Array.Clear(array, 0, array.Length);
                    hij(key, buffer7, array);
                    Array.Copy(array, 0, w, n * bs, bs);
                }
            }
        }

        public static int URShift(int number, int bits)
        {
            if (number >= 0)
            {
                return (number >> bits);
            }
            return ((number >> bits) + (((int) 2) << ~bits));
        }

        public static int URShift(int number, long bits)
        {
            return URShift(number, (int) bits);
        }

        public static long URShift(long number, int bits)
        {
            if (number >= 0L)
            {
                return (number >> bits);
            }
            return ((number >> bits) + (((long) 2L) << ~bits));
        }

        public static long URShift(long number, long bits)
        {
            return URShift(number, (int) bits);
        }

        private static void wxy(byte[] sbyteData, byte[] bitData)
        {
            Array.Clear(bitData, 0, bitData.Length);
            for (int i = 0; i < 8; i++)
            {
                byte num2 = sbyteData[i];
                for (int j = 0; j < 8; j++)
                {
                    bitData[(i * 8) + j] = (byte) (URShift((int) num2, (int) (7 - j)) & 1);
                }
            }
        }

        private static byte[] xcsF(byte[] R, byte[] K, byte[] retF)
        {
            int num;
            byte[] buffer = new byte[0x30];
            byte[] buffer2 = new byte[0x20];
            for (num = 0; num < 0x30; num++)
            {
                buffer[num] = (byte) (R[E[num] - 1] ^ K[num]);
            }
            for (num = 0; num < 8; num++)
            {
                int index = num * 6;
                int num4 = (buffer[index] * 2) + buffer[index + 5];
                int num5 = (((buffer[index + 1] * 8) + (buffer[index + 2] * 4)) + (buffer[index + 3] * 2)) + buffer[index + 4];
                int num3 = num * 4;
                buffer2[num3] = (byte) (URShift((int) S[num][num4][num5], 3) & 1);
                buffer2[num3 + 1] = (byte) (URShift((int) S[num][num4][num5], 2) & 1);
                buffer2[num3 + 2] = (byte) (URShift((int) S[num][num4][num5], 1) & 1);
                buffer2[num3 + 3] = (byte) (S[num][num4][num5] & 1);
            }
            for (num = 0; num < 0x20; num++)
            {
                retF[num] = buffer2[P[num] - 1];
            }
            return retF;
        }
    }
}

