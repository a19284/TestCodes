namespace IBM.WMQ
{
    using IBM.WMQ.Nmqi;
    using System;
    using System.Collections;
    using System.EnterpriseServices;

    [Synchronization(SynchronizationOption.Required)]
    internal class MQTagPool : MQBase
    {
        private static object _hashtableLock = new object();
        private static Hashtable _qmgrToTagPool = new Hashtable();
        internal byte[] baseTag;
        internal int indexIssued;
        internal int maxIndex;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal int tagIncrementIndex;
        internal int tagSize = 0x18;

        public MQTagPool()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        internal static MQTagPool GetInstance(int tagSize, string qmid)
        {
            string key = tagSize.ToString() + qmid;
            MQTagPool pool = null;
            if (pool == null)
            {
                lock (_hashtableLock)
                {
                    pool = (MQTagPool) _qmgrToTagPool[key];
                    if (pool == null)
                    {
                        pool = new MQTagPool();
                        _qmgrToTagPool.Add(key, pool);
                    }
                }
            }
            return pool;
        }

        internal byte[][] GetTags(int requestedNumber, MQSession remoteSession)
        {
            uint method = 0;
            bool flag = CommonServices.TraceStatus();
            byte[][] bufferArray = new byte[requestedNumber][];
            int cc = 0;
            int rc = 0;
            try
            {
                int num10;
                int num11;
                int num12;
                int num13;
                int num14;
                MQSPI mqspi = null;
                MQSPIReserveInOut @out = null;
                int version = 1;
                MQSPIReserveIn @in = null;
                int num5 = 1;
                MQSPIReserveOut out2 = null;
                int num6 = 1;
                MQTSH tsh = null;
                MQTSH rTSH = null;
                int offset = remoteSession.IsMultiplexingEnabled ? 0x24 : 0x1c;
                MQAPI mqapi = new MQAPI();
                mqapi.GetLength();
                int num9 = num10 = num11 = num12 = num13 = num14 = 0;
                cc = 2;
                rc = 0x893;
                for (int i = 0; i < requestedNumber; i++)
                {
                    bufferArray[i] = new byte[this.tagSize];
                    if (this.indexIssued >= this.maxIndex)
                    {
                        base.TrText("Requesting Server for Tag Reservation");
                        int num7 = offset;
                        mqspi = new MQSPI();
                        @out = new MQSPIReserveInOut(version);
                        @in = new MQSPIReserveIn(num5);
                        out2 = new MQSPIReserveOut(num6);
                        mqspi.VerbId = 6;
                        mqspi.OutStructVersion = out2.Version;
                        mqspi.OutStructLength = out2.Length;
                        int translength = (((offset + mqapi.GetLength()) + mqspi.GetLength()) + @out.GetVersionLength()) + Math.Max(mqspi.OutStructLength, @in.GetVersionLength());
                        tsh = remoteSession.AllocateTSH(140, 0, true, translength);
                        byte[] tshBuffer = tsh.TshBuffer;
                        mqapi.Initialize(translength, -1);
                        @out.Version = version;
                        @in.Version = num5;
                        @in.TagSize = this.tagSize;
                        @in.TagReservation = 0x100;
                        num7 += mqapi.WriteStruct(tshBuffer, offset);
                        num9 = num7;
                        num11 = num10 = num7 += mqspi.WriteStruct(tshBuffer, num7);
                        num13 = num12 = num7 += @out.WriteStruct(tshBuffer, num7);
                        num14 = num7 += @in.WriteStruct(tshBuffer, num7);
                        tsh.WriteStruct(tshBuffer, 0);
                        if (flag)
                        {
                            base.TrAPI(method, "__________");
                            base.TrAPI(method, "GetTags >>");
                            base.TrData(method, 0, "Hconn", BitConverter.GetBytes(remoteSession.Hconn.Value));
                            base.TrData(method, 0, "MQSPI", num9, num10 - num9, tshBuffer);
                            base.TrData(method, 0, "SPI_In_Out_Structure", num11, num12 - num11, tshBuffer);
                            base.TrData(method, 0, "SPI_Input_Structure", num13, num14 - num13, tshBuffer);
                            base.TrAPIOutput(method, "CompCode");
                            base.TrAPIOutput(method, "Reason");
                        }
                        tsh.TshBuffer = tshBuffer;
                        num7 = 0;
                        remoteSession.SendTSH(tsh);
                        tshBuffer = null;
                        rTSH = remoteSession.ReceiveTSH(null);
                        byte segmentType = rTSH.SegmentType;
                        if (segmentType == 5)
                        {
                            MQERD mqerd = new MQERD();
                            if (rTSH.Length > offset)
                            {
                                mqerd.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                            }
                            throw mqerd.ErrorException(remoteSession.ParentConnection.NegotiatedChannel.ChannelName.Trim());
                        }
                        if (segmentType != 0x9c)
                        {
                            CommonServices.SetValidInserts();
                            CommonServices.ArithInsert1 = rTSH.SegmentType;
                            CommonServices.CommentInsert1 = remoteSession.ParentConnection.NegotiatedChannel.ChannelName.Trim();
                            base.FFST("%Z% %W%  %I% %E% %U%", "%C%", method, 1, 0x20009504, 0);
                            throw MQERD.ErrorException(13, rTSH.SegmentType, remoteSession.ParentConnection.NegotiatedChannel.ChannelName.Trim().Trim());
                        }
                        rTSH.Offset = mqapi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                        cc = mqapi.mqapi.CompCode;
                        rc = mqapi.mqapi.Reason;
                        if (cc == 2)
                        {
                            return bufferArray;
                        }
                        rTSH.Offset = mqspi.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                        rTSH.Offset = @out.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                        rTSH.Offset = out2.ReadStruct(rTSH.TshBuffer, rTSH.Offset);
                        if (rTSH != null)
                        {
                            remoteSession.ReleaseReceivedTSH(rTSH);
                        }
                        int num17 = 0;
                        int actualReservation = out2.ActualReservation;
                        if (actualReservation <= 0x100)
                        {
                            num17 = 0x100;
                        }
                        else
                        {
                            base.TrText("Unexpected tag reservation allocation: " + actualReservation + "MQRC_UNEXPECTED_ERROR");
                            cc = 2;
                            rc = 0x893;
                            base.throwNewMQException(cc, rc);
                        }
                        this.tagIncrementIndex = out2.TagIncrementOffset - 1;
                        if ((this.tagIncrementIndex < 0) || (this.tagIncrementIndex >= this.tagSize))
                        {
                            cc = 2;
                            rc = 0x893;
                            if (flag)
                            {
                                base.TrException(method, new Exception("Unexpected tag increment offset supplied by server: " + out2.TagIncrementOffset + "MQRC_UNEXPECTED_ERROR"));
                            }
                            base.throwNewMQException(cc, rc);
                        }
                        this.baseTag = new byte[this.tagSize];
                        this.baseTag = out2.BaseReservationTag;
                        this.indexIssued = 0;
                        this.indexIssued = this.baseTag[this.tagIncrementIndex];
                        if ((this.indexIssued + actualReservation) > num17)
                        {
                            cc = 2;
                            rc = 0x893;
                            base.TrException(method, new Exception("Queue manager returned counter which will wrap MQRC_UNEXPECTED_ERROR"));
                            base.throwNewMQException(cc, rc);
                        }
                        Array.Copy(this.baseTag, 0, bufferArray[i], 0, this.tagSize);
                        this.maxIndex = (this.indexIssued + actualReservation) - 1;
                    }
                    else
                    {
                        base.TrText(method, "Alloting the ID's from Reserved Pool");
                        this.indexIssued++;
                        Array.Copy(this.baseTag, 0, bufferArray[i], 0, this.tagSize);
                        bufferArray[i][this.tagIncrementIndex] = (byte) this.indexIssued;
                        cc = 0;
                        rc = 0;
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    base.TrAPI(method, "__________");
                    base.TrAPI(method, "<< GetTags ");
                    base.TrData(method, 0, "Hconn", BitConverter.GetBytes(remoteSession.Hconn.Value));
                    base.TrAPIOutput(method, "CompCode");
                    base.TrAPIOutput(method, "Reason");
                }
            }
            return bufferArray;
        }
    }
}

