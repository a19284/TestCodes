using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nxt.RestfulService.DataInt
{
    class JsonConfirm
    {
        public string OUTSTOCKINDICATIONID { get; set; }//出库指示单号
        public string LOADINGDOCID { get; set; }//装车单号
        public string SignMan { get; set; }//签收人
        public string ArrvivalRemark { get; set; }//备注
        public string Address { get; set; }//地理位置
        public string Photo { get; set; }//照片位置
        public string UERID { get; set; }//用户ID
    }

    class JsonResult
    {
        public string OUTSTOCKINDICATIONID { get; set; }//出库指示单号
        public string LOADINGDOCID { get; set; }//装车单号
        public string FLAG { get; set; }//签收人
    }
}