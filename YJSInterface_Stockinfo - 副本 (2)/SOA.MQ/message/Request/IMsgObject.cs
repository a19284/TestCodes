using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SOA.message.Request
{
    /// <summary>
    /// 请求报文实体类
    /// </summary>

   public class Service
   {
       private Route route=new Route();

       public Route Route
       {
           get { return route; }
           set { route = value; }
       }
       private string data = " ";

       public string Data
       {
           get { return data; }
           set { data = value; }
       }
   }
   /// <summary>
   /// 服务路由域
   /// </summary>
   public class Route
   {
       private string serviceID;
       /// <summary>
       /// 服务码
       /// </summary>
       public string ServiceID
       {
           get { return serviceID; }
           set { serviceID = value; }
       }
       private string serialNO;
       /// <summary>
       /// 服务流水码
       /// </summary>
       public string SerialNO
       {
           get { return serialNO; }
           set { serialNO = value; }
       }
       private string sourecSysID;
       /// <summary>
       /// 源系统代码
       /// </summary>
       public string SourecSysID
       {
           get { return sourecSysID; }
           set { sourecSysID = value; }
       }
       private string serviceTime;
       /// <summary>
       /// 服务时间
       /// </summary>
       public string ServiceTime
       {
           get { return serviceTime; }
           set { serviceTime = value; }
       }
 
   }
    /// <summary>
    /// 服务数据域
    /// </summary>
   public class Data 
   {
       private dynamic control;
       /// <summary>
       /// 控制数据域
       /// </summary>
       public dynamic Control
       {
           //get { return control; }
           get {
               if (control != null)
               {
                   return control;
               }
               else
               { return control; }
           }
           set { control = value; }
       }
       private dynamic request;
       /// <summary>
       /// 请求数据域
       /// </summary>
       public dynamic Request
       {
           get {
               if (control != null)
               {
                   return Request;
               }
               else
               { return request; }
               
           }
          // get { return request; }
           set { request = value; }
       }

    

   }

   public class Node
   {
       private string nodeName;
       /// <summary>
       /// 节点名称
       /// </summary>
       public string NodeName
       {
           get { return nodeName; }
           set { nodeName = value; }
       }

       private object nodeValue;
       /// <summary>
       /// 节点值
       /// </summary>
       public object NodeValue
       {
           get { return nodeValue; }
           set { nodeValue = value; }
       }

    
   }
}
