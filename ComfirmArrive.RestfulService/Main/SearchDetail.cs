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
    public class SearchDetail
    {
        private JsonSearchDetail SearchPar;//传入参数

        public SearchDetail(string Par)
        {
            // 解析
            JsonSearchDetail tmp = JsonHelper.DeserializeJsonToObject<JsonSearchDetail>(Par);
            SearchPar = tmp;
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
                JsonDetail ResultJson = CreateStockinfo(dt);
                string strRet = JsonHelper.SerializeObject(ResultJson);
                return strRet;
            }
            catch (Exception)
            {
                return "{\"Err\":\"查询失败\"}";
            }

        }

        /// <summary>
        /// 构造Json
        /// </summary>
        /// <returns></returns>
        private JsonDetail CreateStockinfo(DataTable dt)
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

            JsonDetail JsInfo = new JsonDetail();
            List<JsonDetailRows> body = new List<JsonDetailRows>();

            int endno=pageno * Perpage<dt.Rows.Count?pageno * Perpage:dt.Rows.Count;
            for (int i = (pageno * Perpage - Perpage); i < endno; i++)
			{
                JsonDetailRows StockBodyRow = new JsonDetailRows();
                StockBodyRow.GOODSID = dt.Rows[i]["GOODSID"].ToString();
                StockBodyRow.NAME = dt.Rows[i]["NAME"].ToString();
                StockBodyRow.MODEL = dt.Rows[i]["MODEL"].ToString();
                StockBodyRow.TRADEMARK =  dt.Rows[i]["TRADEMARK"].ToString();
                StockBodyRow.MID1 = dt.Rows[i]["MID1"].ToString();
                StockBodyRow.Goodsbatch = dt.Rows[i]["Goodsbatch"].ToString();
                StockBodyRow.QUALITYFLG = dt.Rows[i]["QUALITYFLG"].ToString();
                StockBodyRow.SHIPPEDNUM = dt.Rows[i]["SHIPPEDNUM"].ToString();
                StockBodyRow.ARRIVENUM = dt.Rows[i]["ARRIVENUM"].ToString();
                StockBodyRow.BACKNUM = dt.Rows[i]["BACKNUM"].ToString();
                StockBodyRow.BACKREASON = dt.Rows[i]["BACKREASON"].ToString();
                StockBodyRow.LOADINGDOCID = dt.Rows[i]["LOADINGDOCID"].ToString();
                StockBodyRow.LOADINGDOCDTID = dt.Rows[i]["LOADINGDOCDTID"].ToString();
                StockBodyRow.INNERCODE1 = dt.Rows[i]["INNERCODE1"].ToString();
                StockBodyRow.INNERCODE2 = dt.Rows[i]["INNERCODE2"].ToString();
                StockBodyRow.SNAME = dt.Rows[i]["SNAME"].ToString();
                body.Add(StockBodyRow);
			}
            JsInfo.body = body;

            return JsInfo;
        }

        private DataTable GetListD()
        { 
            DataTable dtDetail=null;
            String strSQL;

            //检索数据

            strSQL = "SELECT LD.GOODSID, G.NAME, G.MODEL, G.TRADEMARK, O.SNAME MID1, LD.Goodsbatch,";
            strSQL += "       Q.SNAME QUALITYFLG, LD.SHIPPEDNUM, (LD.SHIPPEDNUM - NVL(LD.BACKNUM,0)) ARRIVENUM, LD.BACKNUM ,LD.BACKREASON, LD.LOADINGDOCID ,LD.LOADINGDOCDTID";
            strSQL += "      , LD.INNERCODE1,LD.INNERCODE2,NVL(CP.SNAME,'-') AS SNAME";
            strSQL += "  FROM LOADEDORDERTBL LD, GOODSMST G,COMPANYMST CP";
            strSQL += "     ,(SELECT CODEVALUE,SNAME FROM BASEDETAILMST";
            strSQL += "         WHERE CODEID = 'SYS_QUALITYFLAG'";
            strSQL += "             AND ACTIONFLG = '1') Q";
            strSQL += "     ,OWNERMST O";
            strSQL += " WHERE LD.GOODSID = G.GOODSID";
            strSQL += "   AND LD.QUALITYFLG = Q.CODEVALUE(+)";
            strSQL += "   AND O.OWNERID(+) = LD.MID1 ";
            strSQL += "   AND LD.OUTSTOCKINDICATIONID = '" + SearchPar.OUTSTOCKINDICATIONID + "'";
            strSQL += "   AND LD.LOADINGDOCID = '" + SearchPar.LOADINGDOCID + "'";
            strSQL += "   AND LD.SENDDOCID IS NOT NULL";
            strSQL += "   AND LD.COMPANYID=CP.COMPANYID(+)";
            strSQL = strSQL + " ORDER BY LD.GOODSID, LD.Goodsbatch";

            OracleConnection conn = OracleHelper.GetOracleConnectionAndOpen;
            if (conn.State == ConnectionState.Open)  
            {
                dtDetail = OracleHelper.ExecuteDataTable(strSQL);
                OracleHelper.CloseOracleConnection(conn);
            }
            return dtDetail;
        }
    }
}