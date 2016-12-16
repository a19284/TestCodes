using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using System.IO;

/// <summary>
/// Json帮助类
/// </summary>
public class JsonHelper
{
    /// <summary>
    /// 将对象序列化为JSON格式
    /// </summary>
    /// <param name="o">对象</param>
    /// <returns>json字符串</returns>
    public static string SerializeObject(object o)
    {
        string json = JsonConvert.SerializeObject(o);
        return json;
    }

    /// <summary>
    /// 解析JSON字符串生成对象实体
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="json">json字符串(eg.{"ID":"112","Name":"石子儿"})</param>
    /// <returns>对象实体</returns>
    public static T DeserializeJsonToObject<T>(string json) where T : class
    {
        JsonSerializer serializer = new JsonSerializer();
        StringReader sr = new StringReader(json);
        object o = serializer.Deserialize(new JsonTextReader(sr), typeof(T));
        T t = o as T;
        return t;
    }

    /// <summary>
    /// 解析JSON数组生成对象实体集合
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="json">json数组字符串(eg.[{"ID":"112","Name":"石子儿"}])</param>
    /// <returns>对象实体集合</returns>
    public static List<T> DeserializeJsonToList<T>(string json) where T : class
    {
        JsonSerializer serializer = new JsonSerializer();
        StringReader sr = new StringReader(json);
        object o = serializer.Deserialize(new JsonTextReader(sr), typeof(List<T>));
        List<T> list = o as List<T>;
        return list;
    }

    /// <summary>
    /// 反序列化JSON到给定的匿名对象.
    /// </summary>
    /// <typeparam name="T">匿名对象类型</typeparam>
    /// <param name="json">json字符串</param>
    /// <param name="anonymousTypeObject">匿名对象</param>
    /// <returns>匿名对象</returns>
    public static T DeserializeAnonymousType<T>(string json, T anonymousTypeObject)
    {
        T t = JsonConvert.DeserializeAnonymousType(json, anonymousTypeObject);
        return t;
    }
    /// <summary>
    /// POST json数据
    /// </summary>
    /// <param name="Url"></param>
    /// <param name="postDataStr"></param>
    /// <returns></returns>
    public static string HttpPost(string Url, string postDataStr)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
        request.Method = "POST";
        request.ContentType = "application/json";
        request.ContentLength = Encoding.UTF8.GetByteCount(postDataStr);
        Stream myRequestStream = request.GetRequestStream();
        StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("gb2312"));
        myStreamWriter.Write(postDataStr);
        myStreamWriter.Close();
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        Stream myResponseStream = response.GetResponseStream();
        StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
        string retString = myStreamReader.ReadToEnd();
        myStreamReader.Close();
        myResponseStream.Close();
        return retString;
    }
    /// <summary>
    /// HttpGet对象
    /// </summary>
    /// <param name="Url"></param>
    /// <param name="postDataStr"></param>
    /// <returns></returns>
    public static string HttpGet(string Url, string postDataStr)
    {
        string Ul = Url + (postDataStr == "" ? "" : "?") + postDataStr;
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Ul);
        request.Method = "GET";
        request.ContentType = "text/html;charset=UTF-8";

        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        Stream myResponseStream = response.GetResponseStream();
        StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
        string retString = myStreamReader.ReadToEnd();
        myStreamReader.Close();
        myResponseStream.Close();

        return retString;
    }

    /// <summary>  
    /// 返回JSon数据  
    /// </summary>  
    /// <param name="JSONData">要处理的JSON数据</param>  
    /// <param name="Url">要提交的URL</param>  
    /// <returns>返回的JSON处理字符串</returns>  
    public static string GetResponseData(string JSONData, string Url)  
        {
            byte[] bytes = Encoding.UTF8.GetBytes(JSONData);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);  
            request.Method = "POST";  
            request.ContentLength = bytes.Length;
            request.ContentType = "application/json";  
            Stream reqstream = request.GetRequestStream();  
            reqstream.Write(bytes, 0, bytes.Length);  
  
            //声明一个HttpWebRequest请求  
            request.Timeout = 90000;  
            //设置连接超时时间  
            request.Headers.Set("Pragma", "no-cache");  
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();  
            Stream streamReceive = response.GetResponseStream();  
            Encoding encoding = Encoding.UTF8;  
  
            StreamReader streamReader = new StreamReader(streamReceive, encoding);  
            string  strResult = streamReader.ReadToEnd();  
            streamReceive.Dispose();  
            streamReader.Dispose();  
  
            return strResult;  
        } 
}
