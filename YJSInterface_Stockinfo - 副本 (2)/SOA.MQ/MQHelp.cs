using IBM.WMQ;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SOA
{
   public class MQHelp
    {
       private  String queueManagerName;
 
       private  Hashtable ConnProperties = new Hashtable();

       /// <summary>
       /// 初始化
       /// </summary>
       /// <param name="HOST_NAME">MQ服务器地址</param>
       /// <param name="PORT_PROPERTY"> MQ服务器端口</param>
       /// <param name="CHANNEL">服务器连接通道</param>
       /// <param name="QUERE_MANAGER_NAME">队列管理器名称</param>
       public MQHelp(string HOST_NAME, int PORT_PROPERTY, string CHANNEL,string QUERE_MANAGER_NAME)
        {
           ConnProperties.Add(MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_CLIENT);
           ConnProperties.Add(MQC.HOST_NAME_PROPERTY, HOST_NAME);
           ConnProperties.Add(MQC.CHANNEL_PROPERTY,CHANNEL );
           ConnProperties.Add(MQC.PORT_PROPERTY, PORT_PROPERTY);
           queueManagerName = QUERE_MANAGER_NAME;
        }
       /// <summary>
       /// 发送
       /// </summary>
       /// <param name="queueName">队列名称</param>
       /// <param name="sendMessages">发送内容</param>
       public void SendMessages(string queueName, string sendMessages)
       {
           MQQueueManager sQMgr = null;  //发送消息的队列管理器
           MQQueue remoteQ = null;     //发送消息的队列      
           try
           {
                // 创建一个连接到队列管理器
               sQMgr = new MQQueueManager(queueManagerName, ConnProperties);

               //现在指定要打开的队列，
              //和打开选项…
               remoteQ = sQMgr.AccessQueue(queueName, MQC.MQOO_OUTPUT | MQC.MQOO_FAIL_IF_QUIESCING | MQC.MQOO_INQUIRE);

               // 定义一个简单的WebSphere MQ消息
               MQMessage putMessage = new MQMessage();

               putMessage.WriteString(sendMessages);

               // 指定消息选项…
               MQPutMessageOptions pmo = new MQPutMessageOptions();
            
               // 放入消息到队列
               remoteQ.Put(putMessage, pmo);

               remoteQ.Close();  //关闭队列
               sQMgr.Disconnect(); //关闭队列管理器连接            

           }
           catch
           {
           }
       }

      /// <summary>
      /// 获取队列消息，只获取一条信息
      /// </summary>
      /// <param name="queueName">队列名称</param>
      /// <returns></returns>
       public string GetMessages(string queueName)
       {
           string msg = string.Empty;
           MQQueueManager gQMgr = null;  //接收消息的队列管理器
           MQQueue getQueue = null;  //接收消息的队列

           try
           {
               gQMgr = new MQQueueManager(queueManagerName, ConnProperties);  //连接队列管理器
               getQueue = gQMgr.AccessQueue(queueName, MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_INQUIRE);  //连接队列
               MQGetMessageOptions gmo = new MQGetMessageOptions();
               MQMessage recMsg = new MQMessage();
               getQueue.Get(recMsg, gmo);  //读取信息存入recMsg
               if (recMsg.MessageLength > 0)
               {
                  msg= recMsg.ReadString(recMsg.MessageLength);
               }
               return msg;
               //int depth = getQueue.CurrentDepth;
               //for (int i = 0; i < depth; i++)
               //{
               //    MQMessage recMsg = new MQMessage();
               //    getQueue.Get(recMsg, gmo);  //读取信息存入recMsg
               //    getMsgBox.Text += recMsg.ReadString(recMsg.MessageLength) + "\r\n";
               //}
           }
           catch (MQException mqex)
           {
               Console.WriteLine(mqex.Message);
               return mqex.Message;
           }
           finally
           {
               getQueue.Close();   //关闭接收队列
               gQMgr.Disconnect();  //关闭接收队列管理器连接
           }

       }

    }
}
