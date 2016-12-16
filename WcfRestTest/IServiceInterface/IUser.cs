using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Web;
using System.ServiceModel;
using Model;
namespace IServiceInterface
{
    [ServiceContract]
    public interface IUser
    {
        [WebGet(UriTemplate = "/{ID}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        UserModel GetUserFromID(string ID);

        [WebGet(UriTemplate = "All", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
         BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<UserModel> GetAllUser();

        [WebInvoke(UriTemplate = "/User/UserName", Method = "POST", RequestFormat = WebMessageFormat.Json,
         BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        String GetUserName(string Name);

        [WebInvoke(UriTemplate = "/User/Post", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
         BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string UpdateUser(UserModel model);
    }
}
