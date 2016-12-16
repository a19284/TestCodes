using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nxt.RestfulService.DataInt
{

        class JsonDetail
        {
            public List<JsonDetailRows> body { get; set; }
        }

        class JsonDetailRows
        {
            public string GOODSID { get; set; }//商品编码
            public string NAME { get; set; }//商品名称
            public string MODEL { get; set; }//规格
            public string TRADEMARK { get; set; }//厂牌
            public string MID1 { get; set; }//分公司
            public string Goodsbatch { get; set; }//批号
            public string QUALITYFLG { get; set; }//品质标记
            public string SHIPPEDNUM { get; set; }//发货数
            public string ARRIVENUM { get; set; }//到货数量
            public string BACKNUM { get; set; }//拒收数量
            public string BACKREASON { get; set; }//退回原因
            public string LOADINGDOCID { get; set; }//装车单号（界面无显示）
            public string LOADINGDOCDTID { get; set; }//装车单明细号（界面无显示）
            public string INNERCODE1 { get; set; }//机内码1
            public string INNERCODE2 { get; set; }//机内码2
            public string SNAME { get; set; }//商品简称（界面无显示）
        }
}