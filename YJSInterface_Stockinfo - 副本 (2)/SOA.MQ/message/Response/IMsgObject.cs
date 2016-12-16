using System;
using System.Collections.Generic;
using System.Text;

namespace SOA.message.Response
{
    /// <summary>
    ///  相应报文实体类
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
        private ServiceResponse serviceResponse=new ServiceResponse();
        /// <summary>
        /// 响应信息域
        /// </summary>
        public ServiceResponse ServiceResponse
        {
            get { return serviceResponse; }
            set { serviceResponse = value; }
        }
    }
    /// <summary>
    /// 响应信息域
    /// </summary>
    public class ServiceResponse
    {
        private string status;
        /// <summary>
        /// 服务状态
        /// </summary>
        public string Status
        {
            get { return status; }
            set { status = value; }
        }
        private string code;
        /// <summary>
        /// 状态码
        /// </summary>
        public string Code
        {
            get { return code; }
            set { code = value; }
        }
        private string desc;
        /// <summary>
        /// 状态描述
        /// </summary>
        public string Desc
        {
            get { return desc; }
            set { desc = value; }
        }
    }
    /// <summary>
    /// 服务数据域
    /// </summary>
    public class Data
    {
        private string control;
        /// <summary>
        /// 控制数据域
        /// </summary>
        public string Control
        {
            get { return control; }
            set { control = value; }
        }
        private string response;
        /// <summary>
        /// 响应数据域
        /// </summary>
        public string Response
        {
            get { return response; }
            set { response = value; }
        }
    }
}

