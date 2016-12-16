using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Net;
using System.Data.SqlClient;
using YJSInterface.DataInt;
using System.Xml;
using SOA.common;
using SOA.config;
using SOA.connection.mqc;
using SOA.connection.mqcm;
using SOA.log;
using SOA.message;
using SOA.message.Request;
using SOA.requester;
using SOA.requester.impl;
using System.IO;
using System.Web;
using EZCALib;
using System.Security.Cryptography.X509Certificates;
using System.Threading;


namespace YJSInterface
{
    public partial class FMain : Form
    {
        public string pam = "";
       // private bool IsLogin = false;
        private int LoginTimes = 0;
        //private CaHelper ca = new CaHelper();
        private string Ticket = "";
        BaseCA ca = new BaseCA();
        CertFilter MyFilter ;
        Collection Collection  ;
        Certificates certs    ;
        Certificate CertSign    ;
        string member = "";


        public string GetDigestFirst(string dat)
        {

            string member = "";
            string digitalDigest = "";
            DateTime dt = new DateTime();
            String p7b = "";//P7签名值
            try
            {
                Thread.Sleep(1000);
                Application.DoEvents();

                //初始数据
                 MyFilter = ca.MyFilter;
                 Collection = ca.MyCollection;
                 certs = ca.MyCertificates;
                 CertSign = ca.MyCertificate;
                MyFilter.CertType = "DIGITAL_SIGNATURE";
                MyFilter.Issuer = "Chongqing  CA,East-Zhongxun CA";
                certs.NewEnum(MyFilter);

                //判断证书库是否存在证书

                if (certs.Count == 0)
                {
                    //MessageBox.Show("请确认插入UDBKEY或安装了USBKEY驱动!");
                    throw new Exception("请确认插入UDBKEY或安装了USBKEY驱动!");
                }

                //选择证书
                CertSign = certs.GetTheChooseCert();//获取ukey中的失效时间

                //校验ukey是否过期
                var enddate = CertSign.NotAfter;
                var beforedate = DateTime.Now.AddDays(-30);
                var nowdate = DateTime.Now;

                TimeSpan beforeTS = new TimeSpan(beforedate.Ticks);
                TimeSpan nowTS = new TimeSpan(nowdate.Ticks);
                TimeSpan endTS = new TimeSpan(DateTime.Parse(enddate).Ticks);

                if (beforeTS > endTS && endTS > nowTS)
                {
                    throw new Exception("证书即将到期，请及时到东方中讯公司证书更新!");
                }
                //dat = HashList("zzllyy_7788");
                p7b = CertSign.SignDataByP7(dat, 1);
                digitalDigest = p7b;

                EZCALib.Certificate cert = new Certificate();
                String certcode = cert.GetP7SignDataInfo(p7b, 2);//证书base64编码
                String indate = dat;
                String rev = cert.VerifySignedDataByP7(indate, certcode, p7b, 1);//原文随机数从session获取。


                if (rev == "" || rev == "-1")
                {
                    throw new Exception(rev + "CA验证失败!");
                }
                else
                {
                    byte[] certb = Convert.FromBase64String(certcode);
                    X509Certificate2 x509cert = new X509Certificate2(certb);
                    String Subject = x509cert.Subject;
                    int findex = Subject.IndexOf("SERIALNUMBER=") + 13;
                    int ss = Subject.IndexOf(',', findex);
                    member = Subject.Substring(findex, ss - findex);
                    return digitalDigest;
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show("CA控件未安装，请下载后才能正常使用数字证书！");
                //throw (ex);
                return "";
            }
        }


        public string GetDigest(string dat)
        {
            //
            

            string digitalDigest = "";
            String p7b = "";//P7签名值
            try
            {
                p7b = CertSign.SignDataByP7(dat, 1);
                digitalDigest = p7b;

                EZCALib.Certificate cert = new Certificate();
                String certcode = cert.GetP7SignDataInfo(p7b, 2);//证书base64编码
                String indate = dat;
                String rev = cert.VerifySignedDataByP7(indate, certcode, p7b, 1);//原文随机数从session获取。


                if (rev == "" || rev == "-1")
                {
                    throw new Exception(rev + "CA验证失败!");
                }
                else
                {
                    byte[] certb = Convert.FromBase64String(certcode);
                    X509Certificate2 x509cert = new X509Certificate2(certb);
                    String Subject = x509cert.Subject;
                    int findex = Subject.IndexOf("SERIALNUMBER=") + 13;
                    int ss = Subject.IndexOf(',', findex);
                    member = Subject.Substring(findex, ss - findex);
                    return digitalDigest;
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show("CA控件未安装，请下载后才能正常使用数字证书！");
                //throw (ex);
                return "";
            }
        }

        public FMain()
        {
            InitializeComponent();

            //初始化消息队列
            MqHelper.InitGetMessage();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy == true)
            {
                MessageBox.Show("正在进行数据交互，不能停止！");
                return;
            }
            dataGridView2.Rows.Insert(0, Program.member, "结束交互", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            timer2.Enabled = false;
            timer2.AutoReset = false;
        }

        private void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //if (timer2.Enabled == false)
            //{
            //    return;
            //}
            //if (backgroundWorker1.IsBusy == true)
            //{
            //    return;
            //}
            //if(DateTime.Now.Hour.ToString()=="06")
                backgroundWorker1.RunWorkerAsync();

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //pam = "dataGridView1&库存上传&开始上传&" + DateTime.Now.ToString();
            //backgroundWorker1.ReportProgress(11, 2);

            try
            {
                //bool isCont = true;
                //long cout = 0;
                //Ticket = "";
                //while (isCont)
                //{
                //    if (Ticket == "")
                //    {

                //        JsonLoginBack LoginBack = Login();
                //        if (LoginBack != null)
                //        {
                //            Ticket = LoginBack.header.ticket[0];
                //        }
                //        else
                //        {
                //            Ticket = "";
                //            throw new Exception("票据获取失败！");
                //            break;
                //        }
                //    }

                 CreateStockinfo();
                //}
                //Ticket = "";
                //pam = "dataGridView1&库存上传&结束上传（共" + (cout * 4 - 1).ToString() + "条）&" + DateTime.Now.ToString();
                //backgroundWorker1.ReportProgress(11, 2);
            }
            catch (Exception ex)
            {
                //string strSQL = "insert into tb_CLLog(CL_Time,CL_Type,CL_Log,CL_Operator,CL_fgs) values(@CL_Time,@CL_Type,@CL_Log,@CL_Operator,@CL_fgs)";
                //SqlParameter[] param = { 
                //                        new SqlParameter("@CL_Time",SqlDbType.DateTime),                     
                //                        new SqlParameter("@CL_Type",SqlDbType.VarChar,20),
                //                        new SqlParameter("@CL_Log",SqlDbType.VarChar,200),
                //                        new SqlParameter("@CL_Operator",SqlDbType.VarChar,50),
                //                        new SqlParameter("@CL_fgs",SqlDbType.VarChar,10)
                //                    };
                //param[0].Value = DateTime.Now.ToString();
                //param[1].Value = "Error";
                //param[2].Value = DateTime.Now.ToString() + "获取库存失败" + ex.Message;
                //param[3].Value = "0";
                //param[4].Value = "";

                //DBComm.ExecuteNonQuery(strSQL, param);
            }
        }

        /// <summary>
        /// 构建库存信息
        /// zhangyy 161010
        /// </summary>
        /// <returns>库存的JSON对象</returns>
        private bool CreateStockinfo()
        {
            bool isCont=true;
            try
            {
                //JsonStockInfo JsInfo = new JsonStockInfo();
                //JsonInfoHead StockHeader = new JsonInfoHead();
                //List<JsonInforows> body = new List<JsonInforows>();
                //string Donelist = "'";

                //StockHeader.ticket = new List<string>();
                //StockHeader.serailNumber = Ticket;
                //StockHeader.clientMAC = OtherHelper.GetMac();
                //StockHeader.clientIP = OtherHelper.GetIp();
                //StockHeader.digitalDigest = GetDigest(Ticket); ;
                //StockHeader.partyMemberId = "user10048";
                //StockHeader.loginAccountID = "135";

                //JsInfo.header = StockHeader;

                //DataTable dtR = DBComm.ExecuteDataTable_Simple("SELECT TOP 1000 [id],[type],[logisticsCode],[productCode],[firstStockNumber],[secondStockNumber],[dispathID],[model],[Flag],[productType] FROM [MergeData].[dbo].[Yjs_StockInfo_temp] where Flag>0");
                //if (dtR == null) return false;
                //if (dtR.Rows.Count < 1) return false;
                //for (int i = 0; i < dtR.Rows.Count; i++)
                //{
                //    JsonInforows StockBodyRow = new JsonInforows();
                //    StockBodyRow.id = i;
                //    StockBodyRow.type = (dtR.Rows[i]["type"] == null ? "" : dtR.Rows[i]["type"]).ToString().Trim();
                //    StockBodyRow.productType = (dtR.Rows[i]["productType"] == null ? "" : dtR.Rows[i]["productType"]).ToString().Trim();
                //    StockBodyRow.productCode = (dtR.Rows[i]["productCode"] == null ? "" : dtR.Rows[i]["productCode"]).ToString();
                //    StockBodyRow.logisticsCode = (dtR.Rows[i]["logisticsCode"] == null ? "" : dtR.Rows[i]["logisticsCode"]).ToString();
                //    StockBodyRow.dispathID = (dtR.Rows[i]["dispathID"] == null ? "" : dtR.Rows[i]["dispathID"]).ToString();
                //    StockBodyRow.model = (dtR.Rows[i]["model"] == null ? "" : dtR.Rows[i]["model"]).ToString();
                //    StockBodyRow.firstStockNumber = Math.Ceiling(Convert.ToDouble(dtR.Rows[i]["firstStockNumber"].ToString()));
                //    StockBodyRow.secondStockNumber = Math.Ceiling(Convert.ToDouble(dtR.Rows[i]["secondStockNumber"].ToString()));
                //    StockBodyRow.stockRemark = "共享库存";
                //    body.Add(StockBodyRow);

                //    Donelist = Donelist + (dtR.Rows[i]["id"] == null ? "" : dtR.Rows[i]["ID"]).ToString() + "','";
                //}
                //Donelist = Donelist.Substring(0, Donelist.Length - 2);
                //JsInfo.body = body;
                ////库存上传
                //JsonStockInfo Stockinfo = JsInfo;
                //if (Stockinfo == null)
                //{
                //    Ticket = "";
                //    DBComm.ExecuteNonQuery_Simple("update [MergeData].[dbo].[Yjs_StockInfo_temp] set flag=flag-1 where id in (" + Donelist + ")");
                //}
                //else
                //{
                //    //实体序列化和反序列化 
                    //string jsonData = "jsonData=" + HttpUtility.UrlEncode( JsonHelper.SerializeObject(Stockinfo));
                    ////string url = "http://wl.yao1.cn:8899/scm/do/dispatcher/erpStock/submitErpStock";
                    ////string LoginRst = JsonHelper.HttpGet(url, jsonData);

                string url = "http://192.168.6.38/CofirmArrive/Save/Update";
                    WebClient wc = new WebClient();
                    string LoginRst = wc.Post(url,"3" );

                    return isCont;

                //    JsonLoginBack StockinfoBack = JsonHelper.DeserializeJsonToObject<JsonLoginBack>(LoginRst);
                //    if (StockinfoBack != null)
                //    {
                //        if (StockinfoBack.header.ticket == null || StockinfoBack.header.respCode != "001")
                //        {
                //            //DBComm.ExecuteNonQuery_Simple("DELETE  FROM  [MergeData].[dbo].[YJS_Ticket] WHERE TICKET='" + Ticket + "'");
                //            //throw new Exception("Failed! ID=" + Donelist);
                //            Ticket = "";
                //            DBComm.ExecuteNonQuery_Simple("update [MergeData].[dbo].[Yjs_StockInfo_temp] set flag=flag-1 where id in (" + Donelist + ")");
                //        }
                //        else
                //        {
                //            //string NewTicket = StockinfoBack.header.ticket[0] == null ? "" : StockinfoBack.header.ticket[0];
                //            //DBComm.ExecuteNonQuery_Simple("insert into [MergeData].[dbo].[YJS_Ticket] (Ticket,CreateDate)values('" +
                //            //NewTicket + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "')");

                //            //DBComm.ExecuteNonQuery_Simple("DELETE  FROM  [MergeData].[dbo].[YJS_Ticket] WHERE TICKET='" + Ticket + "'");
                //            Ticket = StockinfoBack.header.ticket[0] == null ? "" : StockinfoBack.header.ticket[0];
                //            DBComm.ExecuteNonQuery_Simple("update [MergeData].[dbo].[Yjs_StockInfo_temp] set flag=-1 where id in (" + Donelist + ")");
                //        }
                //    }
                //    else
                //    {
                //        Ticket = "";
                //        DBComm.ExecuteNonQuery_Simple("update [MergeData].[dbo].[Yjs_StockInfo_temp] set flag=flag-1 where id in (" + Donelist + ")");
                //        //throw new Exception("Failed! ID=" + Donelist);
                //    }
                //}
                //isCont = true;
                //return isCont;
            }
            catch (Exception ex)
            {
            //    string strSQL = "insert into tb_CLLog(CL_Time,CL_Type,CL_Log,CL_Operator,CL_fgs) values(@CL_Time,@CL_Type,@CL_Log,@CL_Operator,@CL_fgs)";
            //    SqlParameter[] param = { 
            //                            new SqlParameter("@CL_Time",SqlDbType.DateTime),                     
            //                            new SqlParameter("@CL_Type",SqlDbType.VarChar,20),
            //                            new SqlParameter("@CL_Log",SqlDbType.VarChar,200),
            //                            new SqlParameter("@CL_Operator",SqlDbType.VarChar,50),
            //                            new SqlParameter("@CL_fgs",SqlDbType.VarChar,10)
            //                        };
            //    param[0].Value = DateTime.Now.ToString();
            //    param[1].Value = "Error";
            //    param[2].Value = DateTime.Now.ToString() + "获取库存失败" + ex.Message;
            //    param[3].Value = "0";
            //    param[4].Value = "";
            //    DBComm.ExecuteNonQuery(strSQL, param);
            //    pam = "dataGridView1&库存上传&上传出错&" + DateTime.Now.ToString();
                return true;
            }


        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string[] pams = pam.Split('&');
            if (pams[0] == "dataGridView2")
            {
                dataGridView2.Rows.Insert(0, Program.member, pams[1], DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else if (pams[0] == "dataGridView1")
            {
                dataGridView1.Rows.Insert(0, pams[1], DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), pams[2], pams[3]);
            }
        }
        public string HashList(string code)
        {
            System.Security.Cryptography.SHA1 sha = System.Security.Cryptography.SHA1.Create();
            byte[] bytResult = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(code));
            string strResult = "";
            //字节类型的数组转换为字符串

            for (int i = 0; i < bytResult.Length; i++)
            {
                //16进制转换 
                string resulti = bytResult[i].ToString("X");
                if (resulti.Length == 1)
                {
                    resulti = "0" + resulti;
                }
                strResult = strResult + resulti;
            }

            return strResult;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //timer2.Enabled = true;
            //timer2.AutoReset = true;
            //if (timer2.Enabled == false)
            //{
            //    return;
            //}
            //if (backgroundWorker1.IsBusy == true)
            //{
            //    return;
            //}
            //string s = GetDigestFirst("123");
            //if (s == "")
            //{
            //    MessageBox.Show("初始化失败");
            //    return;
            //}
            //if (DateTime.Now.Hour.ToString() == "15")
                backgroundWorker1.RunWorkerAsync();
        }
        /// <summary>
        /// 发送到MQ
        /// </summary>
        /// <param name="i"></param>
        private void Send2MQ(string i)
        {
            SOA.message.Request.Service Service = new SOA.message.Request.Service();
            //基本属性
            Service.Route.SerialNO = "服务流水码";
            Service.Route.ServiceID = "服务码";
            Service.Route.SourecSysID = "源系统代码";
            Service.Route.ServiceTime = System.DateTime.Now.ToString("yyyyMMddHHmmss");
            //复杂对象
            List<Node> dicData = new List<Node>();
            Node n = new Node();
            n.NodeName = "id";
            n.NodeValue = i;
            // n.NodeValue = this.TextBox1.Text.Trim();
            dicData.Add(n);
            XmlDocument m_xml = SOA.message.XmlHelper.InsertNode_xml(Service, "Request", dicData);

            // 获取服务调用者实例
            BaseServiceRequester Requester = new BaseServiceRequester();
            XmlDocument g_xml = Requester.execute(m_xml, 500);


            SOA.message.XmlHelper xh = new XmlHelper(g_xml);

            string SerialNO = xh.GetValue("/Service/Route/SerialNO");
            string ServiceID = xh.GetValue("/Service/Route/ServiceID");
            string SourecSysID = xh.GetValue("/Service/Route/SourecSysID");
            string ServiceTime = xh.GetValue("/Service/Route/ServiceTime");

            List<Node> nodelist = xh.GetNodeObj("/Service/Data/Response");
            // List<MQParameter>  sendQueueLists = (List<MQParameter>)ob.RentObject();
        }


        /// <summary>
        /// 登录
        /// </summary>
        /// <returns>登录成功返回信息</returns>
        private JsonLoginBack Login()
        {
            try
            {
                JsonLogin Loginfo = CreateLogininfo();
                //实体序列化和反序列化 
                string jsonData = "jsonData=" + JsonHelper.SerializeObject(Loginfo);
                //string url = "http://wl.yao1.cn:8899/scm/do/supply-chain/userlogin";
                string url = "http://wl.yao1.cn:9999/scm/do/supply-chain/userlogin";
                string LoginRst = JsonHelper.HttpGet(url, jsonData);
                JsonLoginBack LoginBack = JsonHelper.DeserializeJsonToObject<JsonLoginBack>(LoginRst);
                if (LoginBack.header.respCode == "001")
                {
                    return LoginBack;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception)
            {
                return null;
                throw;
            }
        }
        /// <summary>
        /// 构建登录信息
        /// </summary>
        /// <returns></returns>
        private JsonLogin CreateLogininfo()
        {
            //登录
            JsonLogin Loginfo = new JsonLogin();
            JsonLoginHead LogHead = new JsonLoginHead();
            LogHead.serailNumber = HashList("zzllyy_7788");
            LogHead.funCode = "011";
            LogHead.digitalDigest = "";// GetDigest(HashList("zzllyy_7788"));
            LogHead.respCode = "";
            LogHead.clientMAC = OtherHelper.GetMac();
            LogHead.clientIP = OtherHelper.GetIp();
            JsonLoginBody LogBody = new JsonLoginBody();
            LogBody.loginName = "cqyyypk";
            //LogBody.password = HashList("zzllyy_7788");
            LogBody.password = HashList("123456");
            Loginfo.header = LogHead;
            List<JsonLoginBody> listbody = new List<JsonLoginBody>();
            listbody.Add(LogBody);
            JsonLoginrows LoginRows = new JsonLoginrows();
            LoginRows.rows = listbody;
            Loginfo.body = LoginRows;

            return Loginfo;
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }
    }
}
