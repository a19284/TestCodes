using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
namespace Nxt.RestfulService
{
    public partial class AndroidService
    {
        /// <summary>
        /// 作者：常领峰
        /// 时间：2014年2月25日10:26:55
        /// 功能：获取农业知识实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [WebInvoke(Method = "GET"
              , ResponseFormat = WebMessageFormat.Json
              , BodyStyle = WebMessageBodyStyle.Bare   //不需要任何修饰，否则生成的json无法解析
              , UriTemplate = "/GetModel/?id={id}")]  //只接收string类型，如果是其他类型，需要按照 /?para={parameter}的方式来组织。
        public Nxt.Model.Basic.AgriKnowledge GetModel(Guid id)
        {
            if (id == Guid.Empty) return null;
            Nxt.BLL.Basic.AgriKnowledge bAgriKnowledge = new BLL.Basic.AgriKnowledge();
            return bAgriKnowledge.GetModel(id);
        }
        /// <summary>
        /// 作者：常领峰
        /// 时间：2014年2月25日10:26:55
        /// 功能：获取农业知识查询列表
        /// </summary>
        /// <param name="articletitle">文章标题</param>
        /// <param name="a_keywords">关键字</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "GET"
            , ResponseFormat = WebMessageFormat.Json
            , BodyStyle = WebMessageBodyStyle.Bare
            , UriTemplate = "/GetAgriKnowledgeList/?articletitle={articletitle}&a_keywords={a_keywords}")]
        public List<Nxt.Model.Basic.AgriKnowledge> GetAgriKnowledgeList(string articletitle,string a_keywords)
        {
            StringBuilder strWhere = new StringBuilder(" 1=1 ");
            if (articletitle != "")
                strWhere.AppendFormat(" and articletitle like '%{0}%'", articletitle);
            if (a_keywords != "")
                strWhere.AppendFormat(" and a_keywords like '%{0}%'", a_keywords);
            Nxt.BLL.Basic.AgriKnowledge bAgriKnowledge = new BLL.Basic.AgriKnowledge();
            return bAgriKnowledge.GetModel(strWhere.ToString());
        }

    }
}