using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YJSInterface.DataInt
{
    class JsonStockInfo
    {
        public JsonInfoHead header { get; set; }
        public List<JsonInforows> body { get; set; }
    }

    class JsonInfoHead
    {

        public List<string> ticket { get; set; }
        public string version { get; set; }
        public string serailNumber { get; set; }
        public string funCode { get; set; }
        public string txCode { get; set; }
        public string txDate { get; set; }
        public string txTime { get; set; }
        public string clientMAC { get; set; }
        public string clientIP { get; set; }
        public string digitalDigest { get; set; }
        public string respCode { get; set; }
        public string respMsg { get; set; }
        public string partyMemberId { get; set; }
        public string loginAccountID { get; set; }
        public string userName { get; set; }
        public string typeCode { get; set; }
    }

    class JsonInforows
    {
        public int id { get; set; }
        public string type { get; set; }
        public string productType { get; set; }
        public string productCode { get; set; }
        public string logisticsCode { get; set; }
        //public string number { get; set; }
        //public string stockNumber { get; set; }
        //public string minNumber { get; set; }
        //public string maxNumber { get; set; }
        public string loginAccountID { get; set; }
        public string dispathID { get; set; }
        public string model { get; set; }

        public double firstStockNumber { get; set; }
        public double secondStockNumber { get; set; }
        public string stockRemark { get; set; }
        
    }
}
