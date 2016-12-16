using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nxt.RestfulService.DataInt;
using System.Data;
using Nxt.RestfulService.Helper;
using System.Data.OracleClient;

namespace Nxt.RestfulService.Main
{
    public class SerchList
    {
        private JsonSearch SearchPar;//传入参数
        private string Last;


        public SerchList(string Par)
        {
            // 解析
            JsonSearch tmp = JsonHelper.DeserializeJsonToObject<JsonSearch>(Par);
            SearchPar = tmp;
            Last = "0";
        }
        /// <summary>
        /// 查询头列表
        /// </summary>
        /// <returns></returns>
        public string GetList()
        {
            try
            {
            //根据条件查询
            DataTable dt = GetListD();
            
            //构造返回的Json
            JsonArrStockInfo ResultJson = CreateStockinfo(dt);
            string strRet= JsonHelper.SerializeObject(ResultJson);
            Last = "4";
            return strRet;
            }
            catch (Exception ex)
            {
                return "{\"Err\":\"" + ex.Message + "LastStep:" + Last + "\"}";
            }
        }

        /// <summary>
        /// 构造Json
        /// </summary>
        /// <returns></returns>
        private JsonArrStockInfo CreateStockinfo(DataTable dt)
        {
            int Perpage ;
            if (SearchPar.numPerPage == "") 
                Perpage = 5;
            else
                Perpage = int.Parse(SearchPar.numPerPage);

            int pageno ;
            if (SearchPar.pageNum == "") 
                pageno = 1;
            else
                pageno = int.Parse(SearchPar.pageNum) <= 0 ? 1 : int.Parse(SearchPar.pageNum);


            Last = "2";

            JsonArrStockInfo JsInfo = new JsonArrStockInfo();
            JsonInfoIndHead StockHeader = new JsonInfoIndHead();
            List<JsonInfoIndRows> body = new List<JsonInfoIndRows>();

            
            StockHeader.listcount = dt.Rows.Count.ToString();
            StockHeader.maxPage = (dt.Rows.Count / Perpage + 1).ToString();
            StockHeader.numPerPage = Perpage.ToString();
            StockHeader.pageNum = pageno.ToString();
            JsInfo.header = StockHeader;
            int endno=pageno * Perpage<dt.Rows.Count?pageno * Perpage:dt.Rows.Count;
            for (int i = (pageno * Perpage - Perpage); i < endno; i++)
			{
                JsonInfoIndRows StockBodyRow = new JsonInfoIndRows();
                StockBodyRow.OUTSTOCKINDICATIONID = dt.Rows[i]["OUTSTOCKINDICATIONID"].ToString();
                StockBodyRow.LOADINGDOCID = dt.Rows[i]["LOADINGDOCID"].ToString();
                StockBodyRow.SHIPPEDNUM = double.Parse(dt.Rows[i]["SHIPPEDNUM"].ToString());
                StockBodyRow.ARRIVENUM = double.Parse(dt.Rows[i]["ARRIVENUM"].ToString());
                StockBodyRow.OUTSTOCKTYPE = dt.Rows[i]["OUTSTOCKTYPE"].ToString();
                StockBodyRow.CUSTTYPE = dt.Rows[i]["CUSTTYPE"].ToString();
                StockBodyRow.DELIVERAREAID = dt.Rows[i]["DELIVERAREAID"].ToString();
                StockBodyRow.DELIVERROUTEID = dt.Rows[i]["DELIVERROUTEID"].ToString();
                StockBodyRow.DELIVERID = dt.Rows[i]["DELIVERID"].ToString();
                StockBodyRow.FACTARRIVALDATE = dt.Rows[i]["FACTARRIVALDATE"].ToString();
                StockBodyRow.ARRIVALDATE = dt.Rows[i]["ARRIVALDATE"].ToString();
                StockBodyRow.CUSTID = dt.Rows[i]["CUSTID"].ToString();
                StockBodyRow.OWNERID = dt.Rows[i]["OWNERID"].ToString();
                StockBodyRow.SHIPPERID = dt.Rows[i]["SHIPPERID"].ToString();
                StockBodyRow.TRANSFERTYPE = dt.Rows[i]["TRANSFERTYPE"].ToString();
                StockBodyRow.ISURGENCY = dt.Rows[i]["ISURGENCY"].ToString();
                StockBodyRow.MFLAG = dt.Rows[i]["MFLAG"].ToString();
                StockBodyRow.REMARK = dt.Rows[i]["REMARK"].ToString(); 

                body.Add(StockBodyRow);
			 
			}
            JsInfo.body = body;
            Last = "3";

            return JsInfo;
        }

        private DataTable GetListD()
        { 
            DataTable dtDetail=null;
            String strSQL;
            String strUnArrivalFlg="2";
           // String strArrivalFlg = "1";


            //检索数据


            strSQL = "SELECT   OSI.OUTSTOCKINDICATIONID, LO.LOADINGDOCID,";
            strSQL += "         SUM (LO.SHIPPEDNUM) SHIPPEDNUM, SUM (LO.ARRIVENUM) ARRIVENUM,";
            strSQL += "         OSI.OUTSTOCKTYPE, C.CUSTTYPE, P.SNAME DELIVERAREAID, Q.SNAME DELIVERROUTEID,";
            strSQL += "         D.DELIVERID, LD.SHIPPINGCONFIRMTIME FACTARRIVALDATE, OSI.ARRIVALDATE, C.SNAME CUSTID,";
            strSQL += "         O.SNAME OWNERID, S.SNAME SHIPPERID, D.TRANSFERTYPE, OSI.ISURGENCY, OSI.MFLAG,";
            strSQL += "         A.ARRVIVALREMARK AS REMARK,NVL(CP.SNAME,'-') AS SNAME";
            strSQL += "    FROM OUTSTOCKINDICATIONTBL OSI,";
            strSQL += "         DELIVERTBL D,";
            strSQL += "         OUTSTOCKINDICATIONDTTBL OSID,";
            strSQL += "         LOADEDORDERTBL LO,";
            strSQL += "         CUSTOMERMST C,";
            strSQL += "         LOADINGDOCTBL LD,";
            strSQL += "         ARRIVALINFOTBL A,";
            strSQL += "         SHIPPERMST S,";
            strSQL += "         OWNERMST O,";
            strSQL += "         deliverareamst P,";
            strSQL += "         deliverroutemst Q,";
            strSQL += "         COMPANYMST   CP";
            strSQL += "   WHERE OSI.OUTSTOCKINFORMID = D.OUTSTOCKINFORMID";
            strSQL += "     AND OSI.OUTSTOCKINDICATIONID = OSID.OUTSTOCKINDICATIONID";
            strSQL += "     AND OSID.OUTSTOCKINDICATIONID = LO.OUTSTOCKINDICATIONID";
            strSQL += "     AND OSID.OUTSTOCKINDICATIONDTID = LO.OUTSTOCKINDICATIONDTID";
            strSQL += "     AND C.CUSTID = OSI.CUSTID";
            strSQL += "     AND O.OWNERID = OSI.Ownerid";
            strSQL += "     AND LO.SENDDOCID IS NOT NULL";
            strSQL += "     AND LO.OUTSTOCKINDICATIONID = A.OUTSTOCKINDICATIONID(+)";
            strSQL += "     AND LO.LOADINGDOCID = A.LOADINGDOCID(+)";
            strSQL += "     AND LD.LOADINGDOCID = LO.LOADINGDOCID";
            strSQL += "     AND D.SHIPPERID = S.SHIPPERID(+)";
            strSQL += "     AND D.DELIVERAREAID = P.DELIVERAREAID(+) ";
            strSQL += "     AND D.DELIVERROUTEID = Q.DELIVERROUTEID(+) ";
            strSQL += "     AND D.DELIVERAREAID = Q.DELIVERAREAID(+) ";
            strSQL += "     AND OSI.COMPANYID=CP.COMPANYID(+)  ";
            strSQL += "     AND D.TRANSFERTYPE<>'1' "; //20160118 leijian 改 承运方式为‘自提’的出库指示单不应出现在到货确认界面;

            strSQL += "     AND LD.STATUS = '3'";

            //仓库;
            //If CStr(dt.Rows(0).Item(OutstockIndicationTbl_F.STOCKID_FIELD)).Length <> 0 Then;
            //    strSQL += "   AND OSI.STOCKID = '" + CStr(dt.Rows(0).Item(OutstockIndicationTbl_F.STOCKID_FIELD)) + "'";
            //End If;
            //货主;
            if(SearchPar.OWNERID.Length>0) 
            {
                strSQL += "   AND OSI.OWNERID = '" + SearchPar.OWNERID + "'";
            }
            //出库单号;
            if(SearchPar.OUTSTOCKINDICATIONID.Length>0) 
            {
                    strSQL += "   AND OSI.OUTSTOCKINDICATIONID LIKE  '%" +SearchPar.OUTSTOCKINDICATIONID + "%'";
            }
            //到货状态;

            //if(SearchPar.STATUS.Length > 0)
            //{
                //if(SearchPar.STATUS == strUnArrivalFlg)
                //{
                //    strSQL += "   AND (A.ARRIVALSTATUS IS NULL OR A.ARRIVALSTATUS = '" + SearchPar.STATUS + "')";
                //}
                //else
                //{
                //    strSQL += "   AND A.ARRIVALSTATUS = '" + SearchPar.STATUS + "'";
                //}
                //移动端先写死未到货
            strSQL += "   AND (A.ARRIVALSTATUS IS NULL OR A.ARRIVALSTATUS = '" + strUnArrivalFlg + "')";
                
            //}

            if(SearchPar.ARRIVALDATE.Length > 0){
                strSQL += "  AND OSI.ARRIVALDATE >= '" + SearchPar.ARRIVALDATE + " 00:00:00'";
            }
            //订单到货日(To);
            if (SearchPar.TOARRIVALDATE.Length > 0){
                strSQL += "  AND OSI.ARRIVALDATE <= '" + SearchPar.TOARRIVALDATE + " 23:59:59'";
            }
            //分公司;
            if (SearchPar.COMPANYID.Length > 0){
                strSQL += "   AND OSI.COMPANYID = '" + SearchPar.COMPANYID + "'";
            }
            //客户;
            if (SearchPar.CUSTID.Length > 0){
                strSQL += "   AND OSI.CUSTID = '" + SearchPar.CUSTID + "'";
            }

            //承运人;

            if (SearchPar.SHIPPERID.Length > 0){
                strSQL += "   AND D.SHIPPERID = '" + SearchPar.SHIPPERID + "'";
            }

            //实际发货日期;
            if (SearchPar.FACTOUTSTOCKDATE.Length > 0){
                strSQL += "   AND LD.SHIPPINGCONFIRMTIME >= '" + SearchPar.FACTOUTSTOCKDATE + " 00:00:00'";
            }
            //实际发货日期(to);
            if (SearchPar.TO_FACTOUTSTOCKDATE.Length > 0){
                strSQL += "   AND LD.SHIPPINGCONFIRMTIME <= '" +SearchPar.TO_FACTOUTSTOCKDATE + " 23:59:59'";
            }

            //商品编号;
            if (SearchPar.GOODSID.Length > 0){
                strSQL += "  AND OSID.GOODSID = '" + SearchPar.GOODSID + "'";
            }
            //装车单;
            if(SearchPar.LOADINGDOCID.Length > 0)
            {
                strSQL += "   AND LO.LOADINGDOCID  LIKE '%" + SearchPar.LOADINGDOCID + "%'";
            }
            //装车单;
            if(SearchPar.SENDDOCID.Length > 0 )
            {
                strSQL += "   AND LO.SENDDOCID  LIKE '%" +SearchPar.SENDDOCID + "%'";
            }
            //机内码1;
            if(SearchPar.INNERCODE1.Length > 0 )
            {
                strSQL += "   AND LO.INNERCODE1  LIKE '%" + SearchPar.INNERCODE1 + "%'";
            }
            //机内码2;
            if(SearchPar.INNERCODE2.Length > 0)
            {
                strSQL += "   AND LO.INNERCODE2  LIKE '%" + SearchPar.INNERCODE2 + "%'";
            }

            strSQL += " GROUP BY OSI.OUTSTOCKINDICATIONID,";
            strSQL += "         LO.LOADINGDOCID,";
            strSQL += "         OSI.OUTSTOCKTYPE,";
            strSQL += "         C.CUSTTYPE,";
            strSQL += "         P.SNAME,";
            strSQL += "         Q.SNAME,";
            strSQL += "         D.DELIVERID,";
            strSQL += "		    LD.SHIPPINGCONFIRMTIME,";
            strSQL += "         OSI.ARRIVALDATE,";
            strSQL += "         C.SNAME,";
            strSQL += "         O.SNAME,";
            strSQL += "         S.SNAME,";
            strSQL += "         D.TRANSFERTYPE,";
            strSQL += "         OSI.ISURGENCY,";
            strSQL += "         OSI.MFLAG,";
            strSQL += "         A.ARRVIVALREMARK,";
            strSQL += "         CP.SNAME";
            strSQL += "         ORDER BY OSI.ARRIVALDATE DESC";

            OracleConnection conn = OracleHelper.GetOracleConnectionAndOpen;
            Last = "1";
            if (conn.State == ConnectionState.Open)  
            {
                dtDetail = OracleHelper.ExecuteDataTable(strSQL);
                OracleHelper.CloseOracleConnection(conn);
            }
            Last = "strSQL";
            return dtDetail;
        }
    }
}