using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Model
{
    [DataContract]
    public class UserModel
    {
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string PassWord { get; set; }

        [DataMember]
        public int Age { get; set; }

        public override string ToString()
        {
            return string.Format("ID：{0};姓名: {1};年龄：{2};密码：{3}",ID, UserName, Age, PassWord);
        }
    }
}
