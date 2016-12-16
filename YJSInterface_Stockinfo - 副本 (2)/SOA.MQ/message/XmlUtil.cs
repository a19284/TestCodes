using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace SOA.message
{
    /// <summary>
    /// Xml序列化与反序列化
    /// </summary>
    public class XmlUtil
    {
        #region 反序列化
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="xml">XML字符串</param>
        /// <returns></returns>
        public static object Deserialize(Type type, string xml)
        {
            try
            {
                using (StringReader sr = new StringReader(xml))
                {
                    XmlSerializer xmldes = new XmlSerializer(type);
                    return xmldes.Deserialize(sr);
                }
            }
            catch (Exception e)
            {

                return null;
            }
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="type"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static object Deserialize(Type type, Stream stream)
        {
            XmlSerializer xmldes = new XmlSerializer(type);
            return xmldes.Deserialize(stream);
        }
        #endregion

        #region 序列化
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static string Serializer(Type type, object obj)
        {
            MemoryStream Stream = new MemoryStream();
            XmlSerializer xml = new XmlSerializer(type);
      
           // StreamWriter writer = new StreamWriter(filename, false, Encoding.GetEncoding("GBK"));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
        
            ns.Add(string.Empty, string.Empty); 
            

            try
            {
                //序列化对象
                xml.Serialize(Stream, obj, ns);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            Stream.Position = 0;
            StreamReader sr = new StreamReader(Stream, Encoding.GetEncoding("UTF-8"));
            string str = sr.ReadToEnd();

            sr.Dispose();
            Stream.Dispose();
       
        
            return FormatXml(str);
        }

        #endregion

        public static string FormatXml(string sUnformattedXml)
        {
            sUnformattedXml = sUnformattedXml.Replace("&lt;", "<");
            sUnformattedXml = sUnformattedXml.Replace("&gt;", ">");
            sUnformattedXml = sUnformattedXml.Replace("&amp;lt;", "<");
            sUnformattedXml = sUnformattedXml.Replace("&amp;gt;", ">");
            if (GetValue(sUnformattedXml, "<Request", ">") != "")
            sUnformattedXml = sUnformattedXml.Replace(GetValue(sUnformattedXml, "<Request", ">"), "");
            if (GetValue(sUnformattedXml, "<Control", ">")!="")
            sUnformattedXml = sUnformattedXml.Replace(GetValue(sUnformattedXml, "<Control", ">"), "");

            XmlDocument xd = new XmlDocument();
            xd.LoadXml(sUnformattedXml);
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            XmlTextWriter xtw = null;
            try
            {
                xtw = new XmlTextWriter(sw);
                xtw.Formatting = Formatting.Indented;
                xtw.Indentation = 1;
                xtw.IndentChar = '\t';
                xd.WriteTo(xtw);
            }
            finally
            {
                if (xtw != null)
                    xtw.Close();
            }
            return sb.ToString();
        }
        /// <summary>
        /// 获得字符串中开始和结束字符串中间得值
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="s">开始</param>
        /// <param name="e">结束</param>
        /// <returns></returns> 
        public static string GetValue(string str, string s, string e)
        {
            Regex rg = new Regex("(?<=(" + s + "))[.\\s\\S]*?(?=(" + e + "))", RegexOptions.Multiline | RegexOptions.Singleline);
            return rg.Match(str).Value;
        }

        //public string ConvertDataTableToXml(DataTable dt)
        //{
        //    StringBuilder strXml = new StringBuilder();
        //    strXml.AppendLine("<MonitorData>");
        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        strXml.AppendLine("<rows>");
        //        for (int j = 0; j < dt.Columns.Count; j++)
        //        {
        //            strXml.AppendLine("<" + dt.Columns[j].ColumnName + ">" + dt.Rows[i][j] + "</" + dt.Columns[j].ColumnName + ">");
        //        }
        //        strXml.AppendLine("</rows>");
        //    }
        //    strXml.AppendLine("</MonitorData>");

        //    return strXml.ToString();
        //}
    }
}
