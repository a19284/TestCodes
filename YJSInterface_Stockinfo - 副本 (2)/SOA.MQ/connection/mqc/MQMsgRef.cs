using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.connection.mqc
{
    /// <summary>
    /// MQ对象
    /// </summary>
    public class MQMsgRef : ICloneable
    {
	public byte[] MQMsgId = null;
	public byte[] MQMsgBody = null;
	public String ReplyToQMgr = null;
	
	public MQMsgRef() {
	}
	
	public MQMsgRef(byte[] MQMsgId, byte[] msgBody) {
		this.MQMsgId = MQMsgId;
		this.MQMsgBody = msgBody;
	}

	public Object Clone() {
		try {
			return MemberwiseClone();
		} catch(Exception  e){
			/* Never reach here.*/
			return new MQMsgRef();
		}
	}
  }

}
