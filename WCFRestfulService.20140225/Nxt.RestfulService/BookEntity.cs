using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nxt.RestfulService
{
    public class BookEntity
    {
        public int BookID { get; set; }

        public string BookName { get; set; }

        public decimal BookPrice { get; set; }

        public string BookPublish { get; set; }
    }
}