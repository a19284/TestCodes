using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YJSInterface.DataInt
{


    class JsonLoginHead
    {

        public string serailNumber { get; set; }
        public string funCode { get; set; }
        public string digitalDigest { get; set; }
        public string respCode { get; set; }
        public string respMsg { get; set; }
        public string clientMAC { get; set; }
        public string clientIP { get; set; }
    }

    class JsonLoginBody
    {
        public string loginName { get; set; }
        public string password { get; set; }
    }

    class JsonLogin
    {
        public JsonLoginHead header { get; set; }
        public JsonLoginrows body { get; set; }
    }
    class JsonLoginrows
    {
        public List<JsonLoginBody> rows { get; set; }
    }
}
