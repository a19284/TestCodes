using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SOA.message.Dynamic
{
    /// <summary>
    /// 动态类型 xml字符串 互转换
    /// </summary>
    public class DynamicHelper
    {
        public static string ToXml(dynamic dynamicObject)
        {
            DynamicXElement xmlNode = dynamicObject;
            string str = xmlNode.XContent.ToString();         
            return str;
        }
        public static string ToXml(dynamic dynamicObject,string Rnode)
        {
            DynamicXElement xmlNode = dynamicObject;
            return xmlNode.XContent.ToString().Replace("<" + Rnode + ">", "").Replace("</" + Rnode + ">", "").Replace("<" + Rnode + "/>", "");
        }

        public static dynamic ToObject(string xml, dynamic dynamicResult) 
        {
            XElement element = XElement.Parse(xml);
            dynamicResult = new DynamicXElement(element);
            return dynamicResult;
        }

        public static dynamic ToObject(string xml)
        {
            XElement element = XElement.Parse(xml);
            dynamic dynamicResult = new DynamicXElement(element);
            return dynamicResult;
        }
    }
}
