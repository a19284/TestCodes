using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Web;
using Nxt.Model.Basic;
using System.Data;
using Nxt.Model.Bus;
using System.Text;
namespace Nxt.RestfulService
{
    public partial class AndroidService
    {
        [WebInvoke(Method = "GET"
         , ResponseFormat = WebMessageFormat.Json
         , BodyStyle = WebMessageBodyStyle.Bare   //不需要任何修饰，否则生成的json无法解析
         , UriTemplate = "/getLogin/?userName={userName}&userPassword={pwd}")]
        public bool Login(string userName, string pwd)
        {
            BLL.Sys.Account account = new BLL.Sys.Account();
            return account.Login(userName, pwd);
        }

        //[WebInvoke(Method = "GET"
        // , ResponseFormat = WebMessageFormat.Json
        // , BodyStyle = WebMessageBodyStyle.Bare   //不需要任何修饰，否则生成的json无法解析
        // , UriTemplate = "/?provinceParameter={provinceParameter}&cityParameter={cityParameter}&countyParameter={countyParameter}")]
        //public List<Company> GetEnterpriseList(string provinceParameter, string cityParameter, string countyParameter)
        //{
        //    BLL.Basic.Company company = new BLL.Basic.Company();
        //    string whereSQL = " codename='"+provinceParameter+"' and  ";
        //    company.GetList("  ");
        //}

        [WebInvoke(Method = "GET"
         , ResponseFormat = WebMessageFormat.Json
         , BodyStyle = WebMessageBodyStyle.Bare   //不需要任何修饰，否则生成的json无法解析
         , UriTemplate = "/GetCurrentRegion/{enterpriseID}")]
        public List<Scene> GetCurrentRegion(string enterpriseID)
        {
            BLL.Basic.Scene scene = new BLL.Basic.Scene();
            DataTable dt = scene.GetModel(" id='" + enterpriseID + "' ");
            List<Scene> list = new List<Scene>();
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        Nxt.Model.Basic.Scene sceneModel = new Scene();
                        sceneModel.id = Guid.Parse(dr["id"].ToString());
                        sceneModel.scenename = dr["scenename"].ToString();
                        list.Add(sceneModel);
                    }
                }
            }
            return list;
        }

        [WebInvoke(Method = "GET"
         , ResponseFormat = WebMessageFormat.Json
         , BodyStyle = WebMessageBodyStyle.Bare   //不需要任何修饰，否则生成的json无法解析
         , UriTemplate = "/GetRealtimeData/?enterpriseID={enterpriseID}&sceneID={sceneID}")]
        public List<RealTimeData> GetRealtimeDataBy(string enterpriseID,string sceneID)
        {
            BLL.BasicStorage storage = new BLL.BasicStorage();
            DataSet ds = storage.GetCollectingParam(enterpriseID, sceneID);
             List<RealTimeData> list = new List<RealTimeData>();
            if (ds != null)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    foreach (DataRow dr in dt.Rows)
                    {
                        RealTimeData realTimeData = new RealTimeData();
                        realTimeData.ID = Guid.Parse(dr["ID"].ToString());
                        realTimeData.EnterpriseID = Guid.Parse(dr["EnterpriseID"].ToString());
                        realTimeData.Name = dr["Name"].ToString();
                        realTimeData.Phone = dr["Phone"].ToString();
                        realTimeData.EName = dr["EName"].ToString();
                        realTimeData.Params = dr["Params"].ToString();
                        realTimeData.AirTemp = Double.Parse(dr["AirTemp"].ToString());

                        realTimeData.AirHi = Double.Parse(dr["AirHi"].ToString());
                        realTimeData.SoilTemp = Double.Parse(dr["SoilTemp"].ToString());
                        realTimeData.SoilHi = Double.Parse(dr["SoilHi"].ToString());
                        realTimeData.CO = Double.Parse(dr["CO"].ToString());
                        realTimeData.Shine = Double.Parse(dr["Shine"].ToString());
                        realTimeData.PickTime = string.IsNullOrEmpty(dr["PickTime"].ToString()) ? DateTime.Now : DateTime.Parse(dr["PickTime"].ToString());

                        list.Add(realTimeData);
                    }
                }
            }
            return list;
        }

         [WebInvoke(Method = "GET"
         , ResponseFormat = WebMessageFormat.Json
         , BodyStyle = WebMessageBodyStyle.Bare   //不需要任何修饰，否则生成的json无法解析
         , UriTemplate = "/GetLeftMenuJsonData/?codeId={codeId}&companyId={companyId}&rating={rating}")]
        public string GetLeftMenuJsonData(string codeId, string companyId, string rating)
        {
            DataTable dt = new DataTable();
            if (codeId.Length != 6) return null;//表示请求不符合规范
            if (rating != "企业")
            {
                if (codeId.Trim() == "000000") rating = "国家级";
                else if (codeId.Substring(2, 4) == "0000")//省级
                    rating = "省级";
                else if (codeId.Substring(4, 2) == "00")
                    rating = "市级";
                else if (companyId != "")
                    rating = "企业";
                else
                    rating = "县级";
            }
            StringBuilder strJson = new StringBuilder();
            DataSet ds = null;
            try
            {
                string cacheKey = (codeId + companyId).Trim();
                #region "缓存读取区"
                object cacheData = Nxt.Utility.CacheHelper.GetCache(cacheKey);
                if (cacheData != null) ds = cacheData as DataSet;
                #endregion
                #region "获取数据区"
                if (ds == null || ds.Tables[0].Rows.Count == 0) 
                {
                    switch (rating)
                    {
                        case "国家级":
                            ds = Nxt.DBUtility.DbHelperSQL.Query(
                                  @"select codeId,codeName,'' as companyid,'' as companyname,0 as isCompany from t_basic_code where substring(codeId,3,4)='0000' and codeId<>'000000'
                                    union
                                    select codeId,codeName, convert(nvarchar(36),id) as companyid,companyname,1 from t_basic_company where CodeId='000000'"
                                  , null);
                            break;
                        case "省级":
                            ds = Nxt.DBUtility.DbHelperSQL.Query(@"select codeId,codeName,'' as companyid,'' as companyname,0 as isCompany from t_basic_code where substring(codeId,5,2)='00' and substring(codeId,3,4)<>'0000' and substring(codeId,1,2)='" + codeId.Substring(0, 2) + "'"+
                            @" union
                               select codeId,codeName, convert(nvarchar(36),id) as companyid,companyname,1 from t_basic_company where substring(codeId,5,2)='00' and substring(codeId,3,4)<>'0000' and substring(codeId,1,2)='" + codeId.Substring(0, 2) + "'"                                ,null);
                            break;
                        case "市级":
                            ds = Nxt.DBUtility.DbHelperSQL.Query(@"select codeId,codeName from t_basic_code where substring(codeId,5,2)<>'00' and substring(CodeId,1,4)='" + codeId.Substring(0, 4) + "'"+
                            @" union
                               select codeId,codeName, convert(nvarchar(36),id) as companyid,companyname,1 from t_basic_company where substring(codeId,5,2)<>'00' and substring(CodeId,1,4)='" + codeId.Substring(0, 4) + "'", null);
                            break;
                        case "县级":
                            ds = Nxt.DBUtility.DbHelperSQL.Query("select codeId, id as companyid,companyname from t_basic_company where CodeId='" + codeId + "';", null);
                            break;
                        case "企业":
                            ds = Nxt.DBUtility.DbHelperSQL.Query("select codeId,id as sceneid,scenename from t_basic_scene where CodeId='" + codeId + "' and companyId='" + companyId + "';", null);
                            break;
                    }
                    Nxt.Utility.CacheHelper.Insert(cacheKey, ds, 24 * 60 * 60);
                }
                #endregion
                //json字符串格式:[{},{},{}]
                if (ds == null || ds.Tables.Count == 0) return null;//请求数据为空
                if (ds.Tables[0].Rows.Count == 0) return null;//请求数据为空
                dt = ds.Tables[0];
            }
            catch (Exception ex)
            {
                return null;//请求出错
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(dt);
        }
    }
}