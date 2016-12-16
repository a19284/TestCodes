using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using Nxt.RestfulService;
using System.Collections.Specialized;
using System.Text;

namespace Nxt.Tests
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        private void GetBookByID(string id)
        {
            WebClient proxy = new WebClient();
            string serviceURL = string.Empty;
            DataContractJsonSerializer obj ;
            if (string.IsNullOrEmpty(id))
            {
                serviceURL = string.Format("http://localhost:45345/BookService/");
                obj = new DataContractJsonSerializer(typeof(List<BookEntity>));
            }
            else
            {
                serviceURL = string.Format("http://localhost:45345/BookService/?bookID=" + id);
                obj = new DataContractJsonSerializer(typeof(BookEntity));
            }
            byte[] data = proxy.DownloadData(serviceURL);
            Stream stream = new MemoryStream(data);
            var result = obj.ReadObject(stream);
            List<BookEntity> list=new List<BookEntity>();
            if (result is BookEntity)
                list.Add(result as BookEntity);
            else if (result is List<BookEntity>)
                list = result as List<BookEntity>;
            GridView1.DataSource = list;
            GridView1.DataBind();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            GetBookByID(TextBox1.Text);
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            #region commet
            //string serviceURL = string.Format("http://localhost:45345/BookService");
            //HttpWebRequest webRequest = new HttpWebRequest();

            //BookEntity bookEntity = new BookEntity();
            //bookEntity.BookID = Int32.Parse(txtBookID.Text);
            //bookEntity.BookName = txtBookName.Text;
            //bookEntity.BookPrice = decimal.Parse(txtBookPrice.Text);
            //bookEntity.BookPublish = txtBookPublish.Text;

            //DataContractJsonSerializer obj = new DataContractJsonSerializer(typeof(BookEntity));
            //WebClient proxy = new WebClient();
            //MemoryStream ms = new MemoryStream();

            //obj.WriteObject(ms,bookEntity);
            //byte[] byteSend = ms.ToArray();
            //ms.Close();

            //proxy.Headers.Add("Content-Type", "application/xml; charset=utf-8");
            //byte[] bytesReturn = proxy.UploadData(serviceURL, "PUT", byteSend);
            //var result = bool.Parse(obj.ReadObject(new MemoryStream(bytesReturn)).ToString());

            //if (result)
            //{
            //    lblLog.Text = "数据添加成功";
            //}
            //else
            //{
            //    lblLog.Text = "数据添加失败";
            //}
            #endregion

            #region comment 1

            //BookEntity bookEntity = new BookEntity();
            //bookEntity.BookID = Int32.Parse(txtBookID.Text);
            //bookEntity.BookName = txtBookName.Text;
            //bookEntity.BookPrice = decimal.Parse(txtBookPrice.Text);
            //bookEntity.BookPublish = txtBookPublish.Text;

            
            //string serviceURL = string.Format("http://localhost:45345/BookService");
            //System.Net.ServicePointManager.FindServicePoint(new Uri(serviceURL)).Expect100Continue = false;
            //DataContractJsonSerializer obj = new DataContractJsonSerializer(typeof(BookEntity));
            //MemoryStream ms = new MemoryStream();
            //obj.WriteObject(ms, bookEntity);
            //byte[] byteSend = ms.ToArray();
            //ms.Close();


            //HttpWebRequest httpRequest =(HttpWebRequest)WebRequest.Create(serviceURL);
            //httpRequest.Method = "POST";
            //httpRequest.ContentType = "application/xml; charset=utf-8";
            //httpRequest.ContentLength = byteSend.Length;

            //Stream requestStream = httpRequest.GetRequestStream();
            //requestStream.Write(byteSend, 0, byteSend.Length);
            //requestStream.Close();

            //HttpWebResponse httpWebResponse =(HttpWebResponse)httpRequest.GetResponse();
            //Stream responseStream = httpWebResponse.GetResponseStream();

            //StringBuilder sb = new StringBuilder();

            //using (StreamReader reader =new StreamReader(responseStream, System.Text.Encoding.UTF8))
            //{
            //    string line;
            //    while ((line = reader.ReadLine()) != null)
            //    {
            //        sb.Append(line);
            //    }
            //}

            //lblLog.Text = sb.ToString();
            #endregion

            BookEntity bookEntity = new BookEntity();
            bookEntity.BookID = Int32.Parse(txtBookID.Text);
            bookEntity.BookName = txtBookName.Text;
            bookEntity.BookPrice = decimal.Parse(txtBookPrice.Text);
            bookEntity.BookPublish = txtBookPublish.Text;

            DataContractJsonSerializer obj = new DataContractJsonSerializer(typeof(BookEntity));
            MemoryStream ms = new MemoryStream();
            obj.WriteObject(ms, bookEntity);
            byte[] byteSend = ms.ToArray();
            ms.Close();

            string serviceURL = string.Format("http://localhost:45345/BookService");

            WebClient test = new WebClient();
            test.Headers.Add("Content-Type", "application/json");
            test.Headers.Add("ContentLength", byteSend.Length.ToString());
            

            byte[] responseData = test.UploadData(serviceURL, "PUT", byteSend);

            string result = Encoding.GetEncoding("UTF-8").GetString(responseData);
            lblLog.Text = result;
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            BookEntity bookEntity = new BookEntity();
            bookEntity.BookID = Int32.Parse(txtBookID.Text);
            bookEntity.BookName = txtBookName.Text;
            bookEntity.BookPrice = decimal.Parse(txtBookPrice.Text);
            bookEntity.BookPublish = txtBookPublish.Text;

            DataContractJsonSerializer obj = new DataContractJsonSerializer(typeof(BookEntity));
            MemoryStream ms = new MemoryStream();
            obj.WriteObject(ms, bookEntity);
            byte[] byteSend = ms.ToArray();
            ms.Close();

            string serviceURL = string.Format("http://localhost:45345/BookService");

            WebClient test = new WebClient();
            test.Headers.Add("Content-Type", "application/json");
            test.Headers.Add("ContentLength", byteSend.Length.ToString());
            
            byte[] responseData = test.UploadData(serviceURL, "POST", byteSend);

            string result = Encoding.GetEncoding("UTF-8").GetString(responseData);
            lblLog.Text = result;
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            BookEntity bookEntity = new BookEntity();
            bookEntity.BookID = Int32.Parse(txtBookID.Text);

            DataContractJsonSerializer obj = new DataContractJsonSerializer(typeof(BookEntity));
            MemoryStream ms = new MemoryStream();
            obj.WriteObject(ms, bookEntity);
            byte[] byteSend = ms.ToArray();
            ms.Close();

            string serviceURL = string.Format("http://localhost:45345/BookService");

            WebClient test = new WebClient();
            test.Headers.Add("Content-Type", "application/json");
            test.Headers.Add("ContentLength", byteSend.Length.ToString());
            

            byte[] responseData = test.UploadData(serviceURL, "DELETE", byteSend);

            string result = Encoding.GetEncoding("UTF-8").GetString(responseData);
            lblLog.Text = result;
        }
    }
}
