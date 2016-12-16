using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nxt.RestfulService.DataInt
{
    class JsonLoginBack
    {

        public JsonLoginBackHead header { get; set; }
        public JsonLoginBackBody body { get; set; }
    }

    class JsonLoginBackHead
    {
        public string version { get; set; }
        public string serailNumber { get; set; }
        public string funCode { get; set; }
        public string txCode { get; set; }
        public string txDate { get; set; }

        public string txTime { get; set; }
        public string digitalDigest { get; set; }
        public string respCode { get; set; }
        public string respMsg { get; set; }
        public string partyMemberId { get; set; }
        public string loginAccountID { get; set; }

        public string userName { get; set; }
        public List<string> ticket { get; set; }
        public string clientMAC { get; set; }
        public string typeCode { get; set; }
    }

    class JsonLoginAddress
    {
        public string id { get; set; }
        public string postCode { get; set; }
        public string address { get; set; }
        public string provinceCode { get; set; }
        public string provinceName { get; set; }
        public string memo { get; set; }
        public string delFlag { get; set; }
        public string defFlag { get; set; }
        public string memberID { get; set; }
        public string areaCode { get; set; }
        public string aresName { get; set; }
    }

    class JsonLoginPermissions
    {
        public string reportPermission { get; set; }
        public string PRODUCT_ORDER { get; set; }
        public string IS_ALL_DATA_ACCESS { get; set; }
        public string DRUG_ORDER { get; set; }
        public string DATA_ACCESS { get; set; }
    }

    class JsonLoginBackBody
    {
        public List<JsonLoginAddress> orderAddressList { get; set; }
        public JsonLoginPermissions permissions { get; set; }
        public List<string> SCMPermissions { get; set; }
        public string serverTime { get; set; }
    }
}

