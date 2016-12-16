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
    public class BookService
    {
        public BookService()
        {
            bookList = new List<BookEntity>();
            BookEntity book = new BookEntity();
            book.BookID = 1;
            book.BookName = "大话设计模式";
            book.BookPrice = (decimal)45.2;
            book.BookPublish = "中国邮电出版社";
            bookList.Add(book);

            BookEntity book1 = new BookEntity();
            book1.BookID = 2;
            book1.BookName = "测试用例";
            book1.BookPrice = (decimal)21.0;
            book1.BookPublish = "清华大学出版社";
            bookList.Add(book1);

            BookEntity book2 = new BookEntity();
            book2.BookID = 3;
            book2.BookName = "Rework";
            book2.BookPrice = (decimal)15.4;
            book2.BookPublish = "Wrox pulishment";
            bookList.Add(book2);
        }

        private static List<BookEntity> bookList;

        [WebInvoke(Method = "GET"
            , ResponseFormat = WebMessageFormat.Json
            , BodyStyle = WebMessageBodyStyle.Bare   //不需要任何修饰，否则生成的json无法解析
            , UriTemplate = "/?bookID={bookID}")]  //只接收string类型，如果是其他类型，需要按照 /?para={parameter}的方式来组织。
        public BookEntity Get(int bookID)
        {
            return bookList.Where(p => p.BookID == bookID).FirstOrDefault();
        }

        [WebInvoke(Method = "GET"
            , ResponseFormat = WebMessageFormat.Json
            , BodyStyle = WebMessageBodyStyle.Bare
            , UriTemplate = "/")]
        public List<BookEntity> GetALL()
        {
            return bookList;
        }

          [WebInvoke(Method = "POST"
            , ResponseFormat = WebMessageFormat.Json
            , BodyStyle = WebMessageBodyStyle.Bare
            , UriTemplate = "/UP/{book}")]
        public bool Update(BookEntity book)
        {
            BookEntity query = (from p in bookList where p.BookID == book.BookID select p).FirstOrDefault();
            bookList.Remove(query);
            bookList.Add(book);
            return true;
          }

         [WebInvoke(Method = "PUT"
            , ResponseFormat = WebMessageFormat.Json
            , BodyStyle = WebMessageBodyStyle.Bare
            , UriTemplate = "/")]
        public bool Add(BookEntity book)
        {
            bookList.Add(book);
            return true;
        }

        [WebInvoke(Method = "DELETE"
            , ResponseFormat = WebMessageFormat.Json
            , BodyStyle = WebMessageBodyStyle.Bare
            , UriTemplate = "/")]
        public bool Delete(BookEntity book)
        {
            BookEntity bookCurrent = (from p in bookList where p.BookID == book.BookID select p).FirstOrDefault();
            return bookList.Remove(bookCurrent);
        }
    }
}
