using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using Nxt.RestfulService.Main;


namespace Nxt.RestfulService
{
    // Start the service and browse to http://<machine_name>:<port>/Service1/help to view the service's generated help page
    // NOTE: By default, a new instance of the service is created for each call; change the InstanceContextMode to Single if you want
    // a single instance of the service to process all calls.	
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class SerchInd
    {
        public SerchInd()
        {
        }

        [WebInvoke(Method = "GET"
            , ResponseFormat = WebMessageFormat.Json
            , BodyStyle = WebMessageBodyStyle.Bare   //不需要任何修饰，否则生成的json无法解析
            , UriTemplate = "/?par={par}")]  //只接收string类型，如果是其他类型，需要按照 /?para={parameter}的方式来组织。
        public string Get(string par)
        {
            SerchList Um = new SerchList(par);
            string Result = Um.GetList();
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
            , BodyStyle = WebMessageBodyStyle.Bare
            , UriTemplate = "/")]
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

    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class SerchDetail
    {
        public SerchDetail()
        {
        }

        [WebInvoke(Method = "GET"
            , ResponseFormat = WebMessageFormat.Json
            , BodyStyle = WebMessageBodyStyle.Bare   //不需要任何修饰，否则生成的json无法解析
            , UriTemplate = "/?Dpar={Dpar}")]  //只接收string类型，如果是其他类型，需要按照 /?para={parameter}的方式来组织。
        public string Get(string Dpar)
        {
            SearchDetail Um = new SearchDetail(Dpar);
            string Result = Um.GetList();
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
          , BodyStyle = WebMessageBodyStyle.Bare
          , UriTemplate = "/")]
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

    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Save
    {
        public Save()
        {
        }

        [WebInvoke(Method = "GET"
            , ResponseFormat = WebMessageFormat.Json
            , BodyStyle = WebMessageBodyStyle.Bare   //不需要任何修饰，否则生成的json无法解析
            , UriTemplate = "/?info={info}")]  //只接收string类型，如果是其他类型，需要按照 /?para={parameter}的方式来组织。
        public string Get(string info)
        {
            SaveB Um = new SaveB(info);
            string Result = Um.Save();
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
        [WebInvoke(UriTemplate = "/Update", 
            Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json, 
            BodyStyle = WebMessageBodyStyle.Bare)]
        [OperationContract]
        public string Update(string info)
        {
            //SaveB Um = new SaveB(info);
            //string Result = Um.Save();
            //return Result;
            string str = System.Web.HttpContext.Current.Server.ToString();
            return str;
        }

        //[WebInvoke(Method = "PUT"
        //   , ResponseFormat = WebMessageFormat.Json
        //   , BodyStyle = WebMessageBodyStyle.Bare
        //   , UriTemplate = "/")]
        //public bool Add(string book)
        //{

        //    return true;
        //}

        //[WebInvoke(Method = "DELETE"
        //    , ResponseFormat = WebMessageFormat.Json
        //    , BodyStyle = WebMessageBodyStyle.Bare
        //    , UriTemplate = "/")]
        //public bool Delete(string  book)
        //{
        //    return bookList.Remove(bookCurrent);
        //}
    }

    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ConfirmArrive
    {
        public ConfirmArrive()
        {
        }

        [WebInvoke(Method = "GET"
            , ResponseFormat = WebMessageFormat.Json
            , BodyStyle = WebMessageBodyStyle.Bare   //不需要任何修饰，否则生成的json无法解析
            , UriTemplate = "/?info={info}")]  //只接收string类型，如果是其他类型，需要按照 /?para={parameter}的方式来组织。
        public string Get(string info)
        {
            ConfirmA Um = new ConfirmA(info);
            string Result = Um.Confirm();
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
          , BodyStyle = WebMessageBodyStyle.Bare
          , UriTemplate = "/")]
        public string Confirm(string info)
        {
            ConfirmA Um = new ConfirmA(info);
            string Result = Um.Confirm();
            return Result;
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
