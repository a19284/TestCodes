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
    public class SaveB
    {
        private JsonDetail SearchPar;//传入参数
        private string LoadDTlist="";

        public SaveB(string Par)
        {
            // 解析
            JsonDetail tmp = JsonHelper.DeserializeJsonToObject<JsonDetail>(Par);
            SearchPar = tmp;
            LoadDTlist = "";
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public string Save()
        {
            try
            {
            //更新
            Update();
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

        private void Update()
        {
            String strSQL;
            LoadDTlist = "";

            OracleConnection conn = OracleHelper.GetOracleConnectionAndOpen;

            for (int i = 0; i < SearchPar.body.Count; i++)
            {
                double Backnum = double.Parse(SearchPar.body[i].BACKNUM);
                strSQL = "UPDATE LOADEDORDERTBL   SET BACKNUM = '" + Backnum.ToString() + "',BACKREASON = '" + SearchPar.body[i].BACKREASON + "'";
                strSQL += "  ,BACKFLAG = '1', ARRIVENUM = SHIPPEDNUM - " + Backnum.ToString() + " WHERE LOADINGDOCID = '" + SearchPar.body[i].LOADINGDOCID + "' AND LOADINGDOCDTID = '" + SearchPar.body[i].LOADINGDOCDTID + "'";
                if (conn.State == ConnectionState.Open)
                {
                     OracleHelper.ExecuteNonQuery(strSQL);
                     LoadDTlist += "'" + SearchPar.body[i].LOADINGDOCDTID + "',";
                     OracleHelper.CloseOracleConnection(conn);
                }
            }
        }


        /// <summary>
        /// 构造Json
        /// </summary>
        /// <returns></returns>
        private JsonDetail CreateStockinfo(DataTable dt)
        {

            JsonDetail JsInfo = new JsonDetail();
            List<JsonDetailRows> body = new List<JsonDetailRows>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                JsonDetailRows StockBodyRow = new JsonDetailRows();
                StockBodyRow.GOODSID = dt.Rows[i]["GOODSID"].ToString();
                StockBodyRow.NAME = dt.Rows[i]["NAME"].ToString();
                StockBodyRow.MODEL = dt.Rows[i]["MODEL"].ToString();
                StockBodyRow.TRADEMARK = dt.Rows[i]["TRADEMARK"].ToString();
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
            DataTable dtDetail = null;
            String strSQL;
            string loaddt = LoadDTlist.Substring(0, LoadDTlist.Length - 1);
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
            strSQL += "   AND LD.LOADINGDOCID = '" + SearchPar.body[0].LOADINGDOCID + "'";
            strSQL += "   AND LD.LOADINGDOCDTID IN (" + loaddt + ")";
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