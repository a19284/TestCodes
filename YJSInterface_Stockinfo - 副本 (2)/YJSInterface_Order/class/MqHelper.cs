using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IBM.WMQ; 
using System.Collections;
using System.Xml;

//namespace YJSInterface
//{
	class MqHelper
	{
           //发送方 
        private static String sQmName = "IB9QMGR";   //队列管理器名称   
        //private static String sQmName = "YJS";   //队列管理器名称
        private static String sQName = "APP_IN1";   //本地远程队列            
        private static Hashtable sConnProperties = new Hashtable();         
        //读取方 
        private static String gQmName = "QM_GET";   //队列管理器名称         
        private static String gQName = "GET.LQ";   //本地队列            
        private static Hashtable gConnProperties = new Hashtable(); 

        //public static MqHelper ()
        //{
        //    InitSendMessage();
        //    //初始化接收方的相关配置信息  
        //    InitGetMessage();        
        //}  
        //初始化发送方的相关配置信息        
        public static void InitSendMessage()        
        { 
            //发送 
            sConnProperties.Add(MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_CLIENT);
            sConnProperties.Add(MQC.HOST_NAME_PROPERTY, "127.0.0.1");  //发送方服务器IP 
            sConnProperties.Add(MQC.CHANNEL_PROPERTY, "TT.SVRCONN");  //发送方本地服务器连接通道 
            sConnProperties.Add(MQC.PORT_PROPERTY, 1414);  //发送方端口号        
        } 
        //初始化接收方的相关配置信息 
        public static void InitGetMessage()        
        { 
            //读取 
            gConnProperties.Add(MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_CLIENT); 
            gConnProperties.Add(MQC.HOST_NAME_PROPERTY, "127.0.0.1");  //接收方服务器IP 
            gConnProperties.Add(MQC.CHANNEL_PROPERTY, "T.SVRCONN");  //接收方本地服务器连接通道 
            gConnProperties.Add(MQC.PORT_PROPERTY, 1501);  //接收方端口号
            gConnProperties.Add(MQC.USER_ID_PROPERTY, "zhangyiyan");
            
        }
        //发送消息         
        public static bool SendMessages(string Mesg)        
        {
            bool result = false; 
            MQQueueManager sQMgr = null;  //发送消息的队列管理器  
            MQQueue remoteQ = null;     //发送消息的队列                 
            string sendMessages = Mesg; //发送的消息  
            try
            {
                
                // Create a connection to the queue manager   
                sQMgr = new MQQueueManager(sQmName, sConnProperties);
                // Now specify the queue that we wish to open,          
                // and the open options...     
                remoteQ = sQMgr.AccessQueue(sQName, MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_OUTPUT | MQC.MQOO_FAIL_IF_QUIESCING | MQC.MQOO_INQUIRE );
                // Define a simple WebSphere MQ message, and write some text in UTF format..  
             
                MQMessage putMessage = new MQMessage();
                putMessage.Format = MQC.MQFMT_STRING;
                putMessage.CharacterSet = MQC.CODESET_UTF;
                putMessage.WriteString(sendMessages);
                // specify the message options...                   
                MQPutMessageOptions pmo = new MQPutMessageOptions();
                // accept the defaults, same as MQPMO_DEFAULT               
                // put the message on the queue                
                remoteQ.Put(putMessage, pmo);
                remoteQ.Close();  //关闭队列             
                sQMgr.Disconnect(); //关闭队列管理器 
                result = true;
            }
            catch (MQException MqEx)
            {
                result = false;
                throw MqEx;
            }
            return result;
        }

        


	}
//}
