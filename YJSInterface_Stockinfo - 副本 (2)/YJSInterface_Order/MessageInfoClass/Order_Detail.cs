using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YJSInterface
{
    class Order_Detail
    {
        public string orderDetailCode;
        public string orderCode;
        public DateTime creationDate;
        public string buyerCode;
        public string logisticsCode;
        public string deliveryAddress;
        public int leadTime;
        public string productCode;
        public double price;
        public int quantity;
        public int responceQuantity;
        public string responceTime;
        public int stockQuantity;
        public int receiptQuantity;
        public string rejectReason;
        public string remark;
        public string cancelStatus;
        public string cancelReason;
        public string status;
        public string sellerMemberId;
        public string orderInfoCode;
        public string orderType;
        public string isContract;
        public string supplyId;
        public string businessTypeId;
        public List<Order_Detail_stocks> stocks;
    }
}
