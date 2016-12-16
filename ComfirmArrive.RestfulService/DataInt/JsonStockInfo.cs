using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nxt.RestfulService.DataInt
{
    class JsonArrStockInfo
    {
        public JsonInfoIndHead header { get; set; }
        public List<JsonInfoIndRows> body { get; set; }
    }

    class JsonInfoIndRows
    {
        public string OUTSTOCKINDICATIONID { get; set; }//出库指示单号
        public string LOADINGDOCID { get; set; }//装车单号
        public Double SHIPPEDNUM { get; set; }//发货数量
        public Double ARRIVENUM { get; set; }//到货数量
        public string OUTSTOCKTYPE { get; set; }//出库类型
        public string CUSTTYPE { get; set; }//客户类型
        public string DELIVERAREAID { get; set; }//配送区域
        public string DELIVERROUTEID { get; set; }//配送线路
        public string DELIVERID { get; set; }//配送序号
        public string FACTARRIVALDATE { get; set; }//实际到货日期
        public string ARRIVALDATE { get; set; }//要求到货日期
        public string CUSTID { get; set; }//客户分公司
        public string OWNERID { get; set; }//货主（界面无显示）
        public string SHIPPERID { get; set; }//承运单位
        public string TRANSFERTYPE { get; set; }//承运方式
        public string ISURGENCY { get; set; }//是否加急
        public string MFLAG { get; set; }//是否麻精
        public string REMARK { get; set; }//备注
        //public string SNAME { get; set; }//
    }

    class JsonInfoIndHead 
    {
        //public int id { get; set; }
        public string listcount { get; set; } //记录总数
        public string numPerPage { get; set; }//每页显示
        public string pageNum { get; set; }//当前页码
        public string maxPage { get; set; }//最大页数        
    }
}
