using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;

namespace Nxt.RestfulService
{
    // Start the service and browse to http://<machine_name>:<port>/Service1/help to view the service's generated help page
    // NOTE: By default, a new instance of the service is created for each call; change the InstanceContextMode to Single if you want
    // a single instance of the service to process all calls.	
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class UpdateInfo
    {
        public UpdateInfo()
        {
        }

        [WebInvoke(Method = "GET"
            , ResponseFormat = WebMessageFormat.Json
            , BodyStyle = WebMessageBodyStyle.Bare   //不需要任何修饰，否则生成的json无法解析
            , UriTemplate = "/?Stif={Stif}")]  //只接收string类型，如果是其他类型，需要按照 /?para={parameter}的方式来组织。
        public string Get(string Stif)
        {
            UpdateMain Um = new UpdateMain();
            Um.StockInfo = Stif;
            string Result = Um.MainFlow();
            //string Result = Um.GetCa();
            return Result;
        }

        //[WebInvoke(Method = "GET"
        //    , ResponseFormat = WebMessageFormat.Json
        //    , BodyStyle = WebMessageBodyStyle.Bare
        //    , UriTemplate = "/")]
        //public List<BookEntity> GetALL()
        //{
        //    return bookList;
        //}

          [WebInvoke(Method = "POST"
            , ResponseFormat = WebMessageFormat.Json
            , BodyStyle = WebMessageBodyStyle.Bare)]
        public bool Update(string book)
        {
            return true;
          }

         [WebInvoke(Method = "PUT"
            , ResponseFormat = WebMessageFormat.Json
            , BodyStyle = WebMessageBodyStyle.Bare
            , UriTemplate = "/")]
        public bool Add(string book)
        {

            return true;
        }

        //[WebInvoke(Method = "DELETE"
        //    , ResponseFormat = WebMessageFormat.Json
        //    , BodyStyle = WebMessageBodyStyle.Bare
        //    , UriTemplate = "/")]
        //public bool Delete(string  book)
        //{
        //    return bookList.Remove(bookCurrent);
        //}
    }
}
