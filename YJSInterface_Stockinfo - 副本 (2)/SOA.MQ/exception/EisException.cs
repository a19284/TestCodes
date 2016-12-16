using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.exception
{
    /// <summary>
    /// EIS 自定义异常
    /// </summary>
  public class EisException : Exception {
	private String code;
	
	public String getCode(){
		return code;
	}
	/**
	 * 
	 */
    private static readonly long serialVersionUID = 2613005614399603444L;
    private Exception cause = null;
	
	public EisException(): base(){
		//super();
	}
	
	public EisException(String message):base(message){
		//super(message);
	}

    public EisException(String code, String message):base(code + ":" + message)
    {
		//super(code + ":" + message);
		this.code = code;
	}

    public EisException(Exception cause) : base(cause.ToString())
    {
		//super(cause.getMessage());
		this.cause=cause;
	}
	
    //public string printStackTrace() {
    //    if (cause != null)
    //      return cause.StackTrace;          
    //    else
    //        super.printStackTrace();
    //}

    //public void printStackTrace(StreamWriter s)
    //{
    //    if (cause != null)
    //        cause.printStackTrace(s);
    //    else
    //        super.printStackTrace(s);
    //}

    //public void printStackTrace(PrintWriter s) {
    //    if (cause != null)
    //        cause.printStackTrace(s);
    //    else
    //        super.printStackTrace(s);
    //}
	
	/**
	 * 打印Trace信息
	 * @param e
	 * @return
	 */	
    //public static String getTrace(Exception e){
    //    if(null==e) return null;
    //    ByteArrayOutputStream out = null;
    //    PrintStream print = null;
    //    try {
    //        out = new ByteArrayOutputStream();
    //        print = new PrintStream(out);
    //        e.printStackTrace(print);
    //        print.flush();
    //        out.flush();
    //        return out.toString();
    //    }catch(Exception e1){
    //        LogUtil.getInstance().error("In Eis exception get trace error",e1);
    //        return null;
    //    }
    //}

}
}
