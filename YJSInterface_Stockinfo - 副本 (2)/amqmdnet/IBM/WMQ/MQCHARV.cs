namespace IBM.WMQ
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public class MQCHARV : MQBase
    {
        private bool isInputCapable;
        private bool isOutputCapable;
        internal MQBase.structMQCHARV mqcharv;
        internal MQBase.structMQCHARV32 mqcharv32;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private bool useNativePtrSz;
        private string vsString;

        internal MQCHARV(bool input, bool output)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { input, output });
            this.isInputCapable = input;
            this.isOutputCapable = output;
            this.useNativePtrSz = true;
            this.mqcharv = new MQBase.structMQCHARV();
            this.mqcharv32 = new MQBase.structMQCHARV32();
            this.vsString = "";
            this.VSCCSID = 0x4b8;
            this.VSLength = 0;
            this.VSBufSize = 0;
            this.VSOffset = 0;
            this.VSPtr = IntPtr.Zero;
            this.VSPtr32 = 0;
        }

        private void copyToMQCHARV()
        {
            uint method = 0x40c;
            this.TrEntry(method);
            this.mqcharv.VSBufSize = this.mqcharv32.VSBufSize;
            this.mqcharv.VSCCSID = this.mqcharv32.VSCCSID;
            this.mqcharv.VSLength = this.mqcharv32.VSLength;
            this.mqcharv.VSOffset = this.mqcharv32.VSOffset;
            this.mqcharv.VSPtr = (IntPtr) this.mqcharv32.VSPtr;
            base.TrExit(method);
        }

        private void copyToMQCHARV32()
        {
            uint method = 0x40b;
            this.TrEntry(method);
            this.mqcharv32.VSBufSize = this.mqcharv.VSBufSize;
            this.mqcharv32.VSCCSID = this.mqcharv.VSCCSID;
            this.mqcharv32.VSLength = this.mqcharv.VSLength;
            this.mqcharv32.VSOffset = this.mqcharv.VSOffset;
            this.mqcharv32.VSPtr = this.mqcharv.VSPtr.ToInt32();
            base.TrExit(method);
        }

        internal int GetEndPosAligned(int structPos)
        {
            uint method = 0x35f;
            this.TrEntry(method, new object[] { structPos });
            int result = (structPos + this.VSOffset) + Math.Min(this.VSLength, this.VSBufSize);
            result += (4 - (result & 3)) & 3;
            base.TrExit(method, result);
            return result;
        }

        public int GetLength()
        {
            uint method = 0x40a;
            this.TrEntry(method);
            if (this.useNativePtrSz)
            {
                int num2 = Marshal.SizeOf(this.mqcharv);
                base.TrExit(method, num2, 1);
                return num2;
            }
            int result = Marshal.SizeOf(this.mqcharv32);
            base.TrExit(method, result, 2);
            return result;
        }

        internal int ReadStruct(byte[] b, int structPos, int offset)
        {
            uint method = 0x35e;
            this.TrEntry(method, new object[] { b, structPos, offset });
            int length = this.GetLength();
            IntPtr zero = IntPtr.Zero;
            try
            {
                if (zero == IntPtr.Zero)
                {
                    zero = Marshal.AllocCoTaskMem(length);
                }
                Marshal.Copy(b, offset, zero, length);
                if (this.useNativePtrSz)
                {
                    this.mqcharv = (MQBase.structMQCHARV) Marshal.PtrToStructure(zero, typeof(MQBase.structMQCHARV));
                }
                else
                {
                    this.mqcharv32 = (MQBase.structMQCHARV32) Marshal.PtrToStructure(zero, typeof(MQBase.structMQCHARV32));
                }
                if (this.VSLength != 0)
                {
                    int index = structPos + this.VSOffset;
                    if (this.VSLength == -1)
                    {
                        this.VSLength = 0;
                        int num4 = index;
                        while (b[num4] != 0)
                        {
                            num4++;
                            this.VSLength++;
                        }
                    }
                    if (this.isOutputCapable)
                    {
                        int count = Math.Min(this.VSLength, this.VSBufSize);
                        if (count != 0)
                        {
                            this.vsString = Encoding.ASCII.GetString(b, index, count);
                            return length;
                        }
                        this.vsString = null;
                    }
                    return length;
                }
                this.VSOffset = 0;
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(zero);
                    zero = IntPtr.Zero;
                }
                base.TrExit(method);
            }
            return length;
        }

        internal int WriteStruct(byte[] b, int structPos, int offset, int stringPos)
        {
            uint method = 0x35d;
            this.TrEntry(method, new object[] { b, structPos, offset, stringPos });
            int num2 = 0;
            int num3 = 0;
            IntPtr zero = IntPtr.Zero;
            if (this.isInputCapable && (this.vsString != null))
            {
                Encoding.ASCII.GetBytes(this.vsString, 0, this.vsString.Length, b, stringPos);
                num2 = (4 - (this.vsString.Length & 3)) & 3;
                for (int i = 0; i < num2; i++)
                {
                    b[(stringPos + this.vsString.Length) + i] = 0;
                }
                this.VSLength = this.vsString.Length;
            }
            else
            {
                this.VSLength = 0;
                num2 = 0;
            }
            num3 = Math.Max((int) (this.VSLength + num2), (int) ((this.VSBufSize + 3) & -4));
            this.mqcharv.VSPtr = IntPtr.Zero;
            this.mqcharv32.VSPtr = 0;
            this.VSOffset = stringPos - structPos;
            int length = this.GetLength();
            try
            {
                if (zero == IntPtr.Zero)
                {
                    zero = Marshal.AllocCoTaskMem(length);
                }
                if (this.useNativePtrSz)
                {
                    Marshal.StructureToPtr(this.mqcharv, zero, false);
                }
                else
                {
                    Marshal.StructureToPtr(this.mqcharv32, zero, false);
                }
                Marshal.Copy(zero, b, offset, length);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(zero);
                    zero = IntPtr.Zero;
                }
                base.TrExit(method);
            }
            return (stringPos + num3);
        }

        public bool UseNativePtrSz
        {
            get
            {
                return this.useNativePtrSz;
            }
            set
            {
                this.useNativePtrSz = value;
            }
        }

        public int VSBufSize
        {
            get
            {
                return this.mqcharv.VSBufSize;
            }
            set
            {
                this.mqcharv.VSBufSize = value;
                this.mqcharv32.VSBufSize = value;
            }
        }

        public int VSCCSID
        {
            get
            {
                return this.mqcharv.VSCCSID;
            }
            set
            {
                this.mqcharv.VSCCSID = value;
                this.mqcharv32.VSCCSID = value;
            }
        }

        public int VSLength
        {
            get
            {
                return this.mqcharv.VSLength;
            }
            set
            {
                this.mqcharv.VSLength = value;
                this.mqcharv32.VSLength = value;
            }
        }

        public int VSOffset
        {
            get
            {
                return this.mqcharv.VSOffset;
            }
            set
            {
                this.mqcharv.VSOffset = value;
                this.mqcharv32.VSOffset = value;
            }
        }

        public IntPtr VSPtr
        {
            get
            {
                return this.mqcharv.VSPtr;
            }
            set
            {
                this.mqcharv.VSPtr = value;
            }
        }

        public int VSPtr32
        {
            get
            {
                return this.mqcharv32.VSPtr;
            }
            set
            {
                this.mqcharv32.VSPtr = value;
            }
        }

        public string VSString
        {
            get
            {
                return this.vsString;
            }
            set
            {
                this.vsString = value;
            }
        }
    }
}

