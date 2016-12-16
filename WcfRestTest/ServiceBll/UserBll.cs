using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IServiceInterface;
using Model;
namespace ServiceBll
{
    public class UserBll:IUser
    {
        public static List<UserModel> GetUserList()
        {
            List<UserModel> list = new List<UserModel>()
            {
                new UserModel(){ID=1,UserName="踏浪帅",PassWord="123456",Age=27},
                new UserModel(){ID=2,UserName="wujunyang",PassWord="345678",Age=30},
                new UserModel(){ID=3,UserName="cnblogs",PassWord="987654",Age=33}
            };
            return list;
        }

        public UserModel GetUserFromID(string ID)
        {
            UserModel item = GetUserList().Where(a => a.ID == int.Parse(ID)).SingleOrDefault();
            if (item != null)
            {
                return item;
            }
            else
            {
                return new UserModel();
            }
        }

        public List<UserModel> GetAllUser()
        {
            return GetUserList();
        }

        public string UpdateUser(UserModel model)
        {
            return model.ToString();
        }

        public String GetUserName(string Name)
        {
            return "您好：" + Name;
        }
    }
}
