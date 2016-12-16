namespace IBM.WMQ.PCF
{
    using IBM.WMQ;
    using System;
    using System.Collections;

    public class PCFMessage : PCFHeader
    {
        private ArrayList list;
        private MQCFH mqCFH;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";

        public PCFMessage(MQMessage message)
        {
            this.list = new ArrayList();
            this.mqCFH = new MQCFH(message);
            for (int i = 0; i < this.mqCFH.parameterCount; i++)
            {
                this.AddParameter(PCFParameter.NextParameter(message));
            }
        }

        public PCFMessage(int command)
        {
            this.Initialize(command);
        }

        public PCFMessage(int type, int command, int msgSeqNumber, bool last)
        {
            this.Initialize(type, command, msgSeqNumber, last);
        }

        public void AddParameter(PCFParameter parameter)
        {
            this.list.Add(parameter);
        }

        public void AddParameter(int parameter, int value)
        {
            MQCFIN mqcfin = new MQCFIN(parameter, value);
            this.list.Add(mqcfin);
        }

        public void AddParameter(int parameter, int[] values)
        {
            MQCFIL mqcfil = new MQCFIL(parameter, values);
            this.list.Add(mqcfil);
        }

        public void AddParameter(int parameter, string value)
        {
            MQCFST mqcfst = new MQCFST(parameter, value);
            this.list.Add(mqcfst);
        }

        public void AddParameter(int parameter, string[] values)
        {
            MQCFSL mqcfsl = new MQCFSL(parameter, values);
            this.list.Add(mqcfsl);
        }

        public int GetCommand()
        {
            return this.mqCFH.Command;
        }

        public int GetCompCode()
        {
            return this.mqCFH.compCode;
        }

        public int GetControl()
        {
            return this.mqCFH.Control;
        }

        public int[] GetIntListParameterValue(int parameter)
        {
            for (int i = 0; i < this.list.Count; i++)
            {
                PCFParameter parameter2 = (PCFParameter) this.list[i];
                if (parameter2.Parameter == parameter)
                {
                    if (parameter2.Type != 5)
                    {
                        throw new PCFException(2, 0xbe7);
                    }
                    return (int[]) parameter2.GetValue();
                }
            }
            throw new PCFException(2, 0xbe7);
        }

        public int GetIntParameterValue(int parameter)
        {
            for (int i = 0; i < this.list.Count; i++)
            {
                PCFParameter parameter2 = (PCFParameter) this.list[i];
                if (parameter2.Parameter == parameter)
                {
                    if (parameter2.Type != 3)
                    {
                        throw new PCFException(2, 0xbc6);
                    }
                    return (int) parameter2.GetValue();
                }
            }
            throw new PCFException(2, 0xbc6);
        }

        public int GetMsgSeqNumber()
        {
            return this.mqCFH.MsgSeqNumber;
        }

        public int GetParameterCount()
        {
            return this.list.Count;
        }

        public PCFParameter[] GetParameters()
        {
            if (this.list.Count == 0)
            {
                return null;
            }
            return (PCFParameter[]) this.list.ToArray(typeof(PCFParameter));
        }

        public object GetParameterValue(int parameter)
        {
            throw new Exception("Not implemented");
        }

        private int GetReason()
        {
            return this.mqCFH.reason;
        }

        public string[] GetStringListParameterValue(int parameter)
        {
            for (int i = 0; i < this.list.Count; i++)
            {
                PCFParameter parameter2 = (PCFParameter) this.list[i];
                if (parameter2.Parameter == parameter)
                {
                    if (parameter2.Type != 6)
                    {
                        throw new PCFException(2, 0xbd9);
                    }
                    return (string[]) parameter2.GetValue();
                }
            }
            throw new PCFException(2, 0xbd9);
        }

        public string GetStringParameterValue(int parameter)
        {
            for (int i = 0; i < this.list.Count; i++)
            {
                PCFParameter parameter2 = (PCFParameter) this.list[i];
                if (parameter2.Parameter == parameter)
                {
                    if (parameter2.Type != 4)
                    {
                        throw new PCFException(2, 0xbc7);
                    }
                    return (string) parameter2.GetValue();
                }
            }
            throw new PCFException(2, 0xbc7);
        }

        public override void Initialize(MQMessage message)
        {
            throw new Exception("Not implemented");
        }

        public void Initialize(int command)
        {
            this.Initialize(1, command, 1, true);
        }

        public void Initialize(int type, int command, int msgSeqNumber, bool last)
        {
            this.list = new ArrayList();
            this.mqCFH = new MQCFH();
            this.mqCFH.CompCode = 0;
            this.mqCFH.Reason = 0;
            this.mqCFH.Command = command;
            this.mqCFH.Type = type;
            this.mqCFH.MsgSeqNumber = msgSeqNumber;
            if (last)
            {
                this.mqCFH.Control = 1;
            }
            else
            {
                this.mqCFH.Control = 0;
            }
        }

        public int Size()
        {
            int size = this.mqCFH.Size;
            for (int i = 0; i < this.list.Count; i++)
            {
                size += ((PCFParameter) this.list[i]).Size;
            }
            return size;
        }

        public string ToString()
        {
            throw new Exception("Not implemented");
        }

        public override int Write(MQMessage message)
        {
            int num = 0;
            message.ClearMessage();
            message.MessageType = 1;
            message.Expiry = 0x3e8;
            message.Format = "MQADMIN ";
            message.Feedback = 0;
            this.mqCFH.parameterCount = this.list.Count;
            num += this.mqCFH.Write(message);
            for (int i = 0; i < this.list.Count; i++)
            {
                PCFParameter parameter = (PCFParameter) this.list[i];
                num += parameter.Write(message);
            }
            return num;
        }
    }
}

