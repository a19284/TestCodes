using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nxt.RestfulService.DataInt;
using EZCALib;
using System.Security.Cryptography.X509Certificates;
using System.Data;

namespace Nxt.RestfulService
{
    public class UpdateMain
    {
        public string StockInfo { get; set; }
        public string Ticket { get; set; }
        public string Res { get; set; }

        private bool IsLogin = false;

        public UpdateMain()
        {
            Res = "";
        }

        public string MainFlow()
        {

            string instkif = HttpUtility.UrlDecode(StockInfo);
            //修改库存中tickect
            JsonStockInfo Stockinfo = JsonHelper.DeserializeJsonToObject<JsonStockInfo>(instkif);
            if (Stockinfo == null)
            {
                return "Failed! Message format is not correct!";
            }



            object obj = DBComm.ExecuteScalar("SELECT TOP 1 [Ticket] FROM [MergeData].[dbo].[YJS_Ticket]");

            if (obj == null)
            {
                //进行登录
                DataTable dtser = DBComm.ExecuteDataTable("SELECT TOP 1 [s],[d] FROM [MergeData].[dbo].[YSJ_SerNum]");
                string s = dtser.Rows[0]["s"].ToString();
                string d = dtser.Rows[0]["d"].ToString();
                JsonLoginBack LoginBack = Login(s,d);
                if (!IsLogin)
                {
                    if (LoginBack != null)
                    {
                        Ticket = LoginBack.header.ticket[0];
                        IsLogin = true;
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }
            else
            {
                Ticket = obj.ToString();
            }

            Stockinfo.header.serailNumber = Ticket;
            string Inid = "'";
            for (int ctbdy = 0; ctbdy < Stockinfo.body.Count; ctbdy++)
            {
                Inid = Inid + Stockinfo.body[ctbdy].logisticsCode + "','";
                //Stockinfo.body[ctbdy].stockRemark = "共享库存";
            }

            

            //实体序列化和反序列化 
            string jsonData = "jsonData=" + JsonHelper.SerializeObject(Stockinfo);
            string url = "http://wl.yao1.cn:8899/scm/do/dispatcher/erpStock/submitErpStock";
            string LoginRst = JsonHelper.HttpGet(url, jsonData);
            JsonLoginBack StockinfoBack = JsonHelper.DeserializeJsonToObject<JsonLoginBack>(LoginRst);
            if (StockinfoBack != null)
            {
                if (StockinfoBack.header.ticket==null)
                {
                    DBComm.ExecuteNonQuery_Simple("DELETE  FROM  [MergeData].[dbo].[YJS_Ticket] WHERE TICKET='" + Ticket + "'");
                    return "Failed! ID=" + Inid;
                }
                else
                {
                    string NewTicket = StockinfoBack.header.ticket[0] == null ? "" : StockinfoBack.header.ticket[0];
                    DBComm.ExecuteNonQuery_Simple("insert into [MergeData].[dbo].[YJS_Ticket] (Ticket,CreateDate)values('" +
                    NewTicket + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "')");

                    DBComm.ExecuteNonQuery_Simple("DELETE  FROM  [MergeData].[dbo].[YJS_Ticket] WHERE TICKET='" + Ticket + "'");
                    return "Success! ID=" + Inid;
                }
            }
            else
                return "Failed! ID=" + Inid;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns>登录成功返回信息</returns>
        private JsonLoginBack Login(string s,string d)
        {
            try
            {
                JsonLogin Loginfo = CreateLogininfo(s,d);
                //实体序列化和反序列化 
                string jsonData = "jsonData=" + JsonHelper.SerializeObject(Loginfo);
                string url = "http://wl.yao1.cn:8899/scm/do/supply-chain/userlogin";
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
        private JsonLogin CreateLogininfo(string s,string d)
        {
            //登录
            JsonLogin Loginfo = new JsonLogin();
            JsonLoginHead LogHead = new JsonLoginHead();
            LogHead.serailNumber = s;
            LogHead.funCode = "011";
            LogHead.digitalDigest = d;// GetCa();
            LogHead.respCode = "";
            LogHead.clientMAC = OtherHelper.GetMac();
            LogHead.clientIP = OtherHelper.GetIp();
            JsonLoginBody LogBody = new JsonLoginBody();
            LogBody.loginName = "cqyyypk";
            LogBody.password = HashList("zzllyy_7788");
            //LogBody.password = HashList("123456");
            Loginfo.header = LogHead;
            List<JsonLoginBody> listbody = new List<JsonLoginBody>();
            listbody.Add(LogBody);
            JsonLoginrows LoginRows = new JsonLoginrows();
            LoginRows.rows = listbody;
            Loginfo.body = LoginRows;

            return Loginfo;
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
        /// <summary>
        /// 构建库存信息
        /// </summary>
        /// <returns></returns>
        private JsonStockInfo CreateStockinfo()
        {
            JsonStockInfo JsInfo = new JsonStockInfo();
            JsonInfoHead StockHeader = new JsonInfoHead();
            JsonInforows StockBodyRow = new JsonInforows();
            StockHeader.ticket = new List<string>();
            StockHeader.serailNumber =Ticket;
            StockHeader.clientMAC = OtherHelper.GetMac();
            StockHeader.clientIP = OtherHelper.GetIp();
            StockHeader.digitalDigest = "";
            StockHeader.partyMemberId = "user10048";
            StockHeader.loginAccountID = "135";

            JsInfo.header = StockHeader;

            StockBodyRow.id = 0;
            StockBodyRow.type = "00";
            StockBodyRow.productType = "01";
            StockBodyRow.productCode = "1006050";
            StockBodyRow.logisticsCode = "100000";
            //StockBodyRow.number = ">3000";
            //StockBodyRow.stockNumber = "集团仓库>3000 ";
            //StockBodyRow.minNumber = "2000";
            //StockBodyRow.maxNumber = "5000";
            StockBodyRow.dispathID = "user10048";
            StockBodyRow.model = "型号";
            StockBodyRow.firstStockNumber = 888;
            StockBodyRow.secondStockNumber = 999;
            StockBodyRow.stockRemark = "";

            List<JsonInforows> body = new List<JsonInforows>();
            body.Add(StockBodyRow);

            JsInfo.body = body;

            return JsInfo;
        }
    }
}