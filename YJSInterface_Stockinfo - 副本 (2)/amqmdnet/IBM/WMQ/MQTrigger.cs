namespace IBM.WMQ
{
    using System;
    using System.IO;

    public class MQTrigger : MQBaseObject
    {
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        private const int SIZEOF_MQTMC2 = 0x2dc;
        private MQBase.MQTMC2 trig;

        public MQTrigger()
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
        }

        public MQTrigger(string mqtmc2)
        {
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { mqtmc2 });
            if ((mqtmc2 == null) || (0x2dc != mqtmc2.Length))
            {
                throw new Exception("Invalid MQTMC2 Structure");
            }
            MemoryStream output = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(output);
            BinaryReader reader = new BinaryReader(output);
            writer.Write(mqtmc2.ToCharArray(0, mqtmc2.Length));
            output.Seek(0L, SeekOrigin.Begin);
            this.trig.StrucId = reader.ReadBytes(4);
            this.trig.Version = reader.ReadBytes(4);
            this.trig.QName = reader.ReadBytes(0x30);
            this.trig.ProcessName = reader.ReadBytes(0x30);
            this.trig.TriggerData = reader.ReadBytes(0x40);
            this.trig.ApplType = reader.ReadBytes(4);
            this.trig.ApplId = reader.ReadBytes(0x100);
            this.trig.EnvData = reader.ReadBytes(0x80);
            this.trig.UserData = reader.ReadBytes(0x80);
            this.trig.QMgrName = reader.ReadBytes(0x30);
        }

        public string ApplicationIdentifier
        {
            get
            {
                return base.GetString(this.trig.ApplId);
            }
        }

        public string ApplicationType
        {
            get
            {
                return base.GetString(this.trig.ApplType);
            }
        }

        public string EnvironmentData
        {
            get
            {
                return base.GetString(this.trig.EnvData);
            }
        }

        public string ProcessName
        {
            get
            {
                return base.GetString(this.trig.ProcessName);
            }
        }

        public string QueueManagerName
        {
            get
            {
                return base.GetString(this.trig.QMgrName);
            }
        }

        public string QueueName
        {
            get
            {
                return base.GetString(this.trig.QName);
            }
        }

        public string StructId
        {
            get
            {
                return base.GetString(this.trig.StrucId);
            }
        }

        public string TriggerData
        {
            get
            {
                return base.GetString(this.trig.TriggerData);
            }
        }

        public string UserData
        {
            get
            {
                return base.GetString(this.trig.UserData);
            }
        }

        public string Version
        {
            get
            {
                return base.GetString(this.trig.Version);
            }
        }
    }
}

