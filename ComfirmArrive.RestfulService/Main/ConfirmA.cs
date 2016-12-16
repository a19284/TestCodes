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
    public class ConfirmA
    {
        private JsonConfirm SearchPar;//传入参数

        public ConfirmA(string Par)
        {
            // 解析
            JsonConfirm tmp = JsonHelper.DeserializeJsonToObject<JsonConfirm>(Par);
            SearchPar = tmp;
        }
        /// <summary>
        /// 确认
        /// </summary>
        /// <returns></returns>
        public string Confirm()
        {
            try
            {
            //更新
            JsonResult JsR = new JsonResult();
            JsR.OUTSTOCKINDICATIONID = SearchPar.OUTSTOCKINDICATIONID;
            JsR.LOADINGDOCID = SearchPar.LOADINGDOCID;

            if (Update())
                JsR.FLAG = "0";
            else
                JsR.FLAG = "1"; 

            //构造返回的Json
            string strRet = JsonHelper.SerializeObject(JsR);
            return strRet;
            }
            catch (Exception)
            {
                return "{\"Err\":\"查询失败\"}";
            }
        }

        private bool Update()
        {
            try
            {
                String strSQL;
                OracleConnection conn = OracleHelper.GetOracleConnectionAndOpen;

                //保存装车明细里到货数量和到货状态
                strSQL = "UPDATE LOADEDORDERTBL L   SET ARRIVENUM = SHIPPEDNUM - NVL (BACKNUM, 0),STATUS = '1' WHERE OUTSTOCKINDICATIONID = '" + SearchPar.OUTSTOCKINDICATIONID + "' AND LOADINGDOCID = '" + SearchPar.LOADINGDOCID + "'";
                if (conn.State == ConnectionState.Open)
                {
                    OracleHelper.ExecuteNonQuery(strSQL);
                }

                //保存到货信息到到货信息表
                strSQL = "INSERT INTO ARRIVALINFOTBL";
                strSQL += "            (LOADINGDOCID, OUTSTOCKINDICATIONID, SENDDOCID, OUTSTOCKNUM,";
                strSQL += "             ARRVALNUM, ARRIVALSTATUS, ARRIVALDATE, SIGNMAN, ARRVIVALREMARK,UPDATEMAN,UPDATEDATE)";
                strSQL += "   SELECT  LOADINGDOCID, OUTSTOCKINDICATIONID, SENDDOCID, SUM (SHIPPEDNUM),";
                strSQL += "           SUM (ARRIVENUM), STATUS,";
                strSQL += "'" + DateTime.Now.ToShortDateString() + "',";
                strSQL += "'" + SearchPar.SignMan + "',";
                strSQL += "'" + SearchPar.ArrvivalRemark + "|签到地址：" + SearchPar.Address + "|现场记录：" + SearchPar.Photo + "',";
                strSQL += "'" + SearchPar.UERID + "',";
                strSQL += "'" + DateTime.Now.ToShortDateString() + "'";
                strSQL += " FROM LOADEDORDERTBL";
                strSQL += " WHERE OUTSTOCKINDICATIONID = '" + SearchPar.OUTSTOCKINDICATIONID + "'";
                strSQL += "   AND LOADINGDOCID = '" + SearchPar.LOADINGDOCID + "'";
                strSQL += " GROUP BY LOADINGDOCID, OUTSTOCKINDICATIONID, SENDDOCID, STATUS, '', '', ''";

                if (conn.State == ConnectionState.Open)
                {
                    OracleHelper.ExecuteNonQuery(strSQL);
                }

                //设置出库进度明细表的到货数量
                strSQL = "UPDATE OUTSTOCKPROCESSDTTBL OP ";
                strSQL += "   SET ARRIVENUM = ARRIVENUM + ";
                strSQL += "          (SELECT   SUM (ARRIVENUM)";
                strSQL += "             FROM LOADEDORDERTBL";
                strSQL += "            WHERE OP.OUTSTOCKINDICATIONID = OUTSTOCKINDICATIONID";
                strSQL += "              AND OP.OUTSTOCKINDICATIONDTID = OUTSTOCKINDICATIONDTID";
                strSQL += "              AND OUTSTOCKINDICATIONID = '" + SearchPar.OUTSTOCKINDICATIONID + "'";
                strSQL += "              AND LOADINGDOCID = '" + SearchPar.LOADINGDOCID + "'";
                strSQL += "         GROUP BY LOADINGDOCID, OUTSTOCKINDICATIONID,OUTSTOCKINDICATIONDTID)";
                strSQL += " WHERE EXISTS (";
                strSQL += "          SELECT *";
                strSQL += "             FROM LOADEDORDERTBL";
                strSQL += "            WHERE OP.OUTSTOCKINDICATIONID = OUTSTOCKINDICATIONID";
                strSQL += "              AND OP.OUTSTOCKINDICATIONDTID = OUTSTOCKINDICATIONDTID";
                strSQL += "              AND OUTSTOCKINDICATIONID = '" + SearchPar.OUTSTOCKINDICATIONID + "'";
                strSQL += "              AND LOADINGDOCID = '" + SearchPar.LOADINGDOCID + "'";
                strSQL += "         GROUP BY LOADINGDOCID, OUTSTOCKINDICATIONID,OUTSTOCKINDICATIONDTID)";
                if (conn.State == ConnectionState.Open)
                {
                    OracleHelper.ExecuteNonQuery(strSQL);
                }
                //设置出库指示单的到货确认状态
                //20160719出库指示状态跟新状态(指示书=进度的到货数才跟新指示状态为到货)
                strSQL = "      select   a.*,b.*  from   ";
                strSQL += "     (select c.outstockindicationid, sum(c.arrivenum) as arrivenum from outstockprocessdttbl c where  c.OUTSTOCKINDICATIONID = '" + SearchPar.OUTSTOCKINDICATIONID + "'  group by c.outstockindicationid) a";
                strSQL += "      ,OUTSTOCKINDICATIONTBL b";
                strSQL += "    where a.outstockindicationid=b.outstockindicationid     ";
                strSQL += "    and a.arrivenum=b.thisnum";
                strSQL += "    and a.OUTSTOCKINDICATIONID = '" + SearchPar.OUTSTOCKINDICATIONID + "'";
                DataTable dt = OracleHelper.ExecuteDataTable(strSQL);
                if (dt.Rows.Count > 0)
                {
                    strSQL = "UPDATE OUTSTOCKINDICATIONTBL ";
                    strSQL += "   SET STATUS = '8'";
                    strSQL += "   ,UPDATEMAN = '" + SearchPar.UERID + "'";
                    strSQL += "   ,UPDATEDATE = '" + DateTime.Now.ToShortDateString() + "'";
                    strSQL += " WHERE OUTSTOCKINDICATIONID = '" + SearchPar.OUTSTOCKINDICATIONID + "'   ";
                    if (conn.State == ConnectionState.Open)
                    {
                        OracleHelper.ExecuteNonQuery(strSQL);
                        OracleHelper.CloseOracleConnection(conn);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
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
    }
}