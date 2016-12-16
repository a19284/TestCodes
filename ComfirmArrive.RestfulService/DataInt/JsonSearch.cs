using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nxt.RestfulService.DataInt
{
    public class JsonSearch
    {
        public string userid { get; set; }//用户ID
        public string OWNERID { get; set; }//OWNER
        public string OUTSTOCKINDICATIONID { get; set; }//出库单号
        public string STATUS { get; set; }//到货状态
        public string ARRIVALDATE { get; set; }//订单到货日
        public string TOARRIVALDATE { get; set; }//订单到货日(To)
        public string COMPANYID { get; set; }//分公司
        public string CUSTID { get; set; }//客户
        public string SHIPPERID { get; set; }//承运人
        public string FACTOUTSTOCKDATE { get; set; }//实际发货日期
        public string TO_FACTOUTSTOCKDATE { get; set; }//实际发货日期(to)
        public string GOODSID { get; set; }//商品编号
        public string LOADINGDOCID { get; set; }//装车单
        public string SENDDOCID { get; set; }//发货单
        public string INNERCODE1 { get; set; }//机内码1
        public string INNERCODE2 { get; set; }//机内码2

        public string numPerPage { get; set; }//每页数量
        public string pageNum { get; set; }//当前页码
    }

    public class JsonSearchDetail
    {
        public string userid { get; set; }//用户ID
        public string OWNERID { get; set; }//OWNER
        public string OUTSTOCKINDICATIONID { get; set; }//装车单
        public string LOADINGDOCID { get; set; }//装车单
        public string numPerPage { get; set; }//每页数量
        public string pageNum { get; set; }//当前页码
    }
}