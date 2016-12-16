using SOA.message.Request;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SOA.message
{

    /// <summary>
    /// Xml的操作公共类
    /// </summary>    
    public class XmlHelper
    {
        #region 字段定义
        /// <summary>
        /// XML文件的物理路径
        /// </summary>
        private string _xmlstr = string.Empty;
        /// <summary>
        /// Xml文档
        /// </summary>
        public XmlDocument _xml;
        /// <summary>
        /// XML的根节点
        /// </summary>
        private XmlElement _element;

        #endregion

        #region 构造方法
        /// <summary>
        /// 实例化XmlHelper对象
        /// </summary>
        /// <param name="xmlFilePath">Xml文件的相对路径</param>
        public XmlHelper(string xmlstr)
        {
            //获取XML文件的绝对路径
            _xmlstr = xmlstr;

            //创建根对象
           // rootElement = CreateRootElement(xmlstr);

            //创建一个XML对象
            _xml = new XmlDocument();

      
                //加载XML文件
             _xml.LoadXml(this._xmlstr);
          

            //为XML的根节点赋值
            _element = _xml.DocumentElement;

        }
        public XmlHelper(XmlDocument xd)
        {
            _xml = xd;

            //为XML的根节点赋值
            _element = _xml.DocumentElement;
        }
        #endregion

        #region 创建XML的根节点
        /// <summary>
        /// 创建XML的根节点
        /// </summary>

        //private void CreateXMLElement()
        //{

        //    //创建一个XML对象
        //    _xml = new XmlDocument();

        //    if (string.IsNullOrEmpty(_xmlstr))
        //    {
        //        //加载XML文件
        //        _xml.LoadXml(this._xmlstr);
        //    }

        //    //为XML的根节点赋值
        //    _element = _xml.DocumentElement;
        //}
        #endregion

        #region 获取指定XPath表达式的节点对象
        /// <summary>
        /// 获取指定XPath表达式的节点对象
        /// </summary>	
        /// <param name="xPath">XPath表达式,
        /// 范例1: @"Skill/First/SkillItem", 等效于 @"//Skill/First/SkillItem"
        /// 范例2: @"Table[USERNAME='a']" , []表示筛选,USERNAME是Table下的一个子节点.
        /// 范例3: @"ApplyPost/Item[@itemName='岗位编号']",@itemName是Item节点的属性.
        /// </param>
        public XmlNode GetNode(string xPath)
        {
            ////创建XML的根节点
            //CreateXMLElement();

            ////返回XPath节点
            //return _element.SelectSingleNode(xPath);
            return _element.SelectSingleNode(xPath);
        }
        public XmlNodeList GetNodeList(string xPath)
        {
            return _element.SelectSingleNode(xPath).ChildNodes; ;
        }
        #endregion

        #region 获取指定XPath表达式节点的值
        /// <summary>
        /// 获取指定XPath表达式节点的值
        /// </summary>
        /// <param name="xPath">XPath表达式,
        /// 范例1: @"Skill/First/SkillItem", 等效于 @"//Skill/First/SkillItem"
        /// 范例2: @"Table[USERNAME='a']" , []表示筛选,USERNAME是Table下的一个子节点.
        /// 范例3: @"ApplyPost/Item[@itemName='岗位编号']",@itemName是Item节点的属性.
        /// </param>
        //public string GetValue(string xPath)
        //{
        //    //创建XML的根节点
        //    CreateXMLElement();

        //    //返回XPath节点的值
        //    return _element.SelectSingleNode(xPath).InnerText;
        //}
        #endregion

        #region 获取指定XPath表达式节点的属性值
        /// <summary>
        /// 获取指定XPath表达式节点的属性值
        /// </summary>
        /// <param name="xPath">XPath表达式,
        /// 范例1: @"Skill/First/SkillItem", 等效于 @"//Skill/First/SkillItem"
        /// 范例2: @"Table[USERNAME='a']" , []表示筛选,USERNAME是Table下的一个子节点.
        /// 范例3: @"ApplyPost/Item[@itemName='岗位编号']",@itemName是Item节点的属性.
        /// </param>
        /// <param name="attributeName">属性名</param>
        public string GetAttributeValue(string xPath, string attributeName)
        {
            //创建XML的根节点
           // CreateXMLElement();

            //返回XPath节点的属性值
            return _element.SelectSingleNode(xPath).Attributes[attributeName].Value;
        }
        #endregion

        #region 新增节点
        /// <summary>
        /// 1. 功能：新增节点。
        /// 2. 使用条件：将任意节点插入到当前Xml文件中。
        /// </summary>	
        /// <param name="xmlNode">要插入的Xml节点</param>
        public void AppendNode(XmlNode xmlNode)
        {
            //创建XML的根节点
           // CreateXMLElement();

            //导入节点
            XmlNode node = _xml.ImportNode(xmlNode, true);

            //将节点插入到根节点下
            _element.AppendChild(node);
        }

        /// <summary>
        /// 1. 功能：新增节点。
        /// 2. 使用条件：将DataSet中的第一条记录插入Xml文件中。
        /// </summary>	
        /// <param name="ds">DataSet的实例，该DataSet中应该只有一条记录</param>
        //public void AppendNode(DataSet ds)
        //{
        //    //创建XmlDataDocument对象
        //    XmlDataDocument xmlDataDocument = new XmlDataDocument(ds);

        //    //导入节点
        //    XmlNode node = xmlDataDocument.DocumentElement.FirstChild;

        //    //将节点插入到根节点下
        //    AppendNode(node);
        //}
        #endregion

        #region 删除节点
        /// <summary>
        /// 删除指定XPath表达式的节点
        /// </summary>	
        /// <param name="xPath">XPath表达式,
        /// 范例1: @"Skill/First/SkillItem", 等效于 @"//Skill/First/SkillItem"
        /// 范例2: @"Table[USERNAME='a']" , []表示筛选,USERNAME是Table下的一个子节点.
        /// 范例3: @"ApplyPost/Item[@itemName='岗位编号']",@itemName是Item节点的属性.
        /// </param>
        public void RemoveNode(string xPath)
        {
            //创建XML的根节点
           // CreateXMLElement();

            //获取要删除的节点
            XmlNode node = _xml.SelectSingleNode(xPath);

            //删除节点
            _element.RemoveChild(node);
        }
        #endregion //删除节点

        #region 保存XML文件
        /// <summary>
        /// 保存XML文件
        /// </summary>	
        public void Save()
        {
            //创建XML的根节点
           // CreateXMLElement();

            //保存XML文件
            _xml.Save(this._xmlstr);
        }
        #endregion //保存XML文件



        #region 创建根节点对象
        /// <summary>
        /// 创建根节点对象
        /// </summary>
        /// <param name="xmlFilePath">Xml文件的相对路径</param>	
        private static XmlElement CreateRootElement(string xmlstr)
        {

            //创建XmlDocument对象
            XmlDocument xmlDocument = new XmlDocument();
            //加载XML文件
            xmlDocument.LoadXml(xmlstr);

            //返回根节点
            return xmlDocument.DocumentElement;
        }
        #endregion

        #region 修改节点对象
        /// <summary>
        /// 修改节点对象
        /// </summary>
        /// <param name="XPath">节点路径</param>	
        public void  SetValue(string XPath, string NodeText)
        {
             _element.SelectSingleNode("XPath").InnerText = NodeText;  
        }
        #endregion

        #region 获取指定XPath表达式节点的值
        /// <summary>
        /// 获取指定XPath表达式节点的值
        /// </summary>
        /// <param name="xmlFilePath">Xml文件的相对路径</param>
        /// <param name="xPath">XPath表达式,
        /// 范例1: @"Skill/First/SkillItem", 等效于 @"//Skill/First/SkillItem"
        /// 范例2: @"Table[USERNAME='a']" , []表示筛选,USERNAME是Table下的一个子节点.
        /// 范例3: @"ApplyPost/Item[@itemName='岗位编号']",@itemName是Item节点的属性.
        /// </param>
        public string GetValue(string xPath)
        {
            ////创建根对象
            //XmlElement rootElement = CreateRootElement(xmlstr);

            //返回XPath节点的值
            return _element.SelectSingleNode(xPath).InnerText;
        }
        #endregion

        #region 获取指定XPath表达式节点的属性值
        /// <summary>
        /// 获取指定XPath表达式节点的属性值
        /// </summary>
        /// <param name="xmlFilePath">Xml文件的相对路径</param>
        /// <param name="xPath">XPath表达式,
        /// 范例1: @"Skill/First/SkillItem", 等效于 @"//Skill/First/SkillItem"
        /// 范例2: @"Table[USERNAME='a']" , []表示筛选,USERNAME是Table下的一个子节点.
        /// 范例3: @"ApplyPost/Item[@itemName='岗位编号']",@itemName是Item节点的属性.
        /// </param>
        /// <param name="attributeName">属性名</param>
        public static string GetAttributeValue(string xmlstr, string xPath, string attributeName)
        {
            //创建根对象
            XmlElement rootElement = CreateRootElement(xmlstr);

            //返回XPath节点的属性值
            return rootElement.SelectSingleNode(xPath).Attributes[attributeName].Value;
        }
        #endregion

        #region 根据Xml文件的节点路径，返回一个DataSet数据集
        /// <summary>
        /// 根据Xml文件的节点路径，返回一个DataSet数据集
        /// </summary>
        /// <param name="XmlPathNode">Xml文件的某个节点</param>
        /// <returns></returns>
        public DataSet GetDs(string XmlPathNode)
        {
            DataSet ds = new DataSet();
            try
            {
                System.IO.StringReader read = new System.IO.StringReader(_element.SelectSingleNode(XmlPathNode).OuterXml);
                ds.ReadXml(read); //利用DataSet的ReadXml方法读取StringReader文件流
                //ds.ReadXml(xmlSR, XmlReadMode.InferTypedSchema);
                read.Close();
            }
            catch
            { }
            return ds;
        }
    
        #endregion

        #region 节点值查询判断
        /// <summary>
        /// 节点值查询判断
        /// </summary>
        /// <param name="XmlPathNode">父节点</param>
        /// <param name="index">节点索引</param>
        /// <param name="NodeText">节点值</param>
        /// <returns></returns>
        public bool SelectNode(string XmlPathNode, int index, string NodeText)
        {
            try
            {
                XmlNodeList _NodeList = _element.SelectNodes(XmlPathNode);
                //循环遍历节点，查询是否存在该节点
                for (int i = 0; i < _NodeList.Count; i++)
                {
                    if (_NodeList[i].ChildNodes[index].InnerText == NodeText)
                    {
                        return true;
                        break;
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        #endregion

        #region 返回节点对象
        public List<Node> GetNodeObj(string  xPath)
        {
            XmlNode node = GetNode(xPath);
            return GetNodeObjs(node);       
           
           
        }
        public List<Node> GetNodeObjs(XmlNode node)
        {
            List<Node> objNode = new List<Node>();
            XmlNodeList nodeList = node.ChildNodes;
            foreach (XmlNode childnode in nodeList)
            {
                Node n1 = new Node();
                n1.NodeName = childnode.Name;
                //判断是否子节点
                if (childnode.ChildNodes.Count == 1)
                {
                    n1.NodeValue = childnode.InnerText;
                }
                else
                {
                    //递归调用
                    n1.NodeValue = GetNodeObjs(childnode);
                }
                objNode.Add(n1);

            }
            return objNode;

        }    
        #endregion

        #region 插入一个节点，带N个子节点
        /// <summary>
        /// 插入一个节点，带N个子节点
        /// </summary>
        /// <param name="MainNode">插入节点的父节点</param>
        /// <param name="ChildNode">插入节点的元素名</param>
        /// <param name="Element">插入节点的子节点名数组</param>
        /// <param name="Content">插入节点的子节点内容数组</param>
        /// <returns></returns>
        public bool InsertNode(string MainNode, string ChildNode, string[] Element, string[] Content)
        {
            try
            {
                XmlNode objRootNode = _xml.SelectSingleNode(MainNode); //声明XmlNode对象
                XmlElement objChildNode = _xml.CreateElement(ChildNode); //创建XmlElement对象
                objRootNode.AppendChild(objChildNode);
                for (int i = 0; i < Element.Length; i++) //循环插入节点元素
                {
                    XmlElement objElement = _xml.CreateElement(Element[i]);
                    objElement.InnerText = Content[i];
                    objChildNode.AppendChild(objElement);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static XmlDocument InsertNode(XmlDocument _xml, string MainNode, string ChildNode, string Content)
        {
            try
            {
                XmlNode objRootNode = _xml.SelectSingleNode(MainNode); //声明XmlNode对象
                XmlElement objChildNode = _xml.CreateElement(ChildNode); //创建XmlElement对象
                objChildNode.InnerText = Content;
                objRootNode.AppendChild(objChildNode);
                //for (int i = 0; i < Element.Length; i++) //循环插入节点元素
                //{
                //    XmlElement objElement = _xml.CreateElement(Element[i]);
                //    objElement.InnerText = Content[i];
                //    objChildNode.AppendChild(objElement);
                //}
                return _xml;
            }
            catch
            {
                return _xml;
            }
        }

        public static string InsertNode_string(SOA.message.Request.Service Service, string ChildNode, List<SOA.message.Request.Node> Content)
        {
            try
            {
                string xml = XmlUtil.Serializer(typeof(SOA.message.Request.Service), Service);
                string MainNode = "Service/Data";
                //创建一个XML对象
                XmlDocument m_xml = new XmlDocument();

                //加载XML文件
                m_xml.LoadXml(xml);

                return  XmlUtil.FormatXml(InsertNode(m_xml, MainNode, ChildNode,  Content).OuterXml);        
            }
            catch
            {
                return "";
            }
        }
        public static XmlDocument InsertNode_xml(SOA.message.Request.Service Service, string ChildNode, List<SOA.message.Request.Node> Content)
        {
            try
            {
                string xml = XmlUtil.Serializer(typeof(SOA.message.Request.Service), Service);
                string MainNode = "Service/Data";
                //创建一个XML对象
                XmlDocument m_xml = new XmlDocument();

                //加载XML文件
                m_xml.LoadXml(xml);

                return InsertNode(m_xml, MainNode, ChildNode, Content);
            }
            catch
            {
                return null;
            }
        }
        public static XmlDocument InsertNode_xml(SOA.message.Response.Service Service, string ChildNode, List<SOA.message.Request.Node> Content)
        {
            try
            {
                string xml = XmlUtil.Serializer(typeof(SOA.message.Response.Service), Service);
                string MainNode = "Service/Data";
                //创建一个XML对象
                XmlDocument m_xml = new XmlDocument();

                //加载XML文件
                m_xml.LoadXml(xml);

                return InsertNode(m_xml, MainNode, ChildNode, Content);
            }
            catch
            {
                return null;
            }
        }
        public static XmlDocument InsertNode(XmlDocument m_xml, string MainNode, string ChildNode, List<SOA.message.Request.Node> Content)
        {
            try
            {
                 XmlNode objRootNode = getobjRootNode(m_xml, MainNode);
               // XmlNode objRootNode = m_xml.SelectSingleNode(MainNode); //声明XmlNode对象
                XmlElement objChildNode = m_xml.CreateElement(ChildNode); //创建XmlElement对象
                objRootNode.AppendChild(objChildNode);
                for (int i = 0; i < Content.Count; i++) //循环插入节点元素
                {
                    
                   
                    if (Content.ElementAt(i).NodeValue is string)
                    {
                      XmlElement objElement = m_xml.CreateElement(Content.ElementAt(i).NodeName);
                      objElement.InnerText = Content.ElementAt(i).NodeValue.ToString();
                      objChildNode.AppendChild(objElement);
                     // string xxx = XmlUtil.FormatXml(m_xml.OuterXml); 
                    }
                    else
                    {
                        XmlNode mXmlNode2 = getobjRootNode(m_xml, MainNode);
                        //递归调用
                        m_xml = InsertNode(m_xml, MainNode+"/"+ChildNode, Content.ElementAt(i).NodeName, (List<SOA.message.Request.Node>)Content.ElementAt(i).NodeValue);

                        List<SOA.message.Request.Node> nnn= (List<SOA.message.Request.Node>)Content.ElementAt(i).NodeValue;
                       // string xxxx = XmlUtil.FormatXml(m_xml.OuterXml); 
                    }
                }
             
                return m_xml;
            }
            catch
            {
                return null;
            }
        }

        public static XmlNode getobjRootNode(XmlDocument XmlDoc, string XmlPathNode)
        {
            try
            {
               int i = XmlDoc.SelectSingleNode(XmlPathNode).ParentNode.ChildNodes.Count;
               XmlNodeList _NodeList = XmlDoc.SelectSingleNode(XmlPathNode).ParentNode.ChildNodes;
                if(i>0)
               return _NodeList[i-1];
            }
            catch
            {
            }
            return null;
        }
        #endregion

        public static byte[] getBytes(XmlDocument m_xml)
        {

            try
            {
                byte[] reqMs = null;
                //string xml = XmlUtil.Serializer(typeof(SOA.message.Request.Service), Service);
        
                //创建一个XML对象
                //XmlDocument m_xml = new XmlDocument();

                //加载XML文件
                //m_xml.LoadXml(xml);

                //字符编码
                using (MemoryStream ms = new MemoryStream())
                {
                    m_xml.Save(ms);
                    reqMs = ms.ToArray();
                }

                return reqMs;
            }
            catch
            {
                return null;
            }
        }
      
    }
}
