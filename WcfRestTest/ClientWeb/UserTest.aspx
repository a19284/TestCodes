<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UserTest.aspx.cs" Inherits="ClientWeb.UserTest" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="Js/jquery-1.4.2.js" type="text/javascript"></script>
    <script src="Js/json2.js" type="text/javascript"></script>
    <script type="text/javascript">
        $(function () {
            $("#btnSave").click(
            function () {
                $.ajax({
                    type: "get",
                    contentType: "application/json; charset=utf-8",
                    url: "http://localhost:8089/WcfRestService.svc/1",
                    success: function (userInfo) {
                        alert("用户名：" + userInfo.UserName + " 密码：" + userInfo.PassWord + " 年龄：" + userInfo.Age);
                    },
                    error: function (error) {
                        alert("出错：" + error.responseText);
                    }
                });
            }
            );

            $("#btnAll").click(
            function () {
                $.ajax({
                    type: "get",
                    contentType: "application/json; charset=utf-8",
                    url: "http://localhost:8089/WcfRestService.svc/All",
                    success: function (userlist) {
                        $.each(userlist.GetAllUserResult, function (item, value) {
                            alert(value.ID + "|" + value.UserName + "|" + value.PassWord + "|" + value.Age);
                        })
                    }
                });
            }
            );


            $("#btnUserName").click(
            function () {
                var UserMsg = { 'Name': '踏浪帅' };
                var UserMessage = JSON2.stringify(UserMsg);
                $.ajax({
                    type: "POST",
                    contentType: "application/json",
                    url: "http://localhost:8089/WcfRestService.svc/User/UserName",
                    data: UserMessage,
                    dataType: "json",
                    crossDomain: false,
                    success: function (userNameInfo) {
                        alert(userNameInfo);
                    },
                    error: function (error) {
                        alert(error.responseText);
                    }
                });
            }
            );

            $("#btnPost").click(
            function () {
                var UserMsg = { 'model': { 'ID': '6', 'UserName': '踏浪帅', 'PassWord': '123456', 'Age': '27'} };
                var UserMessage = JSON2.stringify(UserMsg);
                $.ajax({
                    type: "POST",
                    contentType: "application/json",
                    url: "http://localhost:8089/WcfRestService.svc/User/Post",
                    data: UserMessage,
                    dataType: "json",
                    crossDomain: false,
                    success: function (userNameInfo) {
                        alert(userNameInfo);
                    },
                    error: function (error) {
                        alert(error.responseText);
                    }
                });
            }
            );
        })
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <input id="btnSave" type="button" value="获得一条数据" />

    <input id="btnAll" type="button" value="获得所有数据" />

    <input id="btnUserName" type="button" value="Post获得用户名" />

    <input id="btnPost" type="button" value="Post提交" />
    </div>
    </form>
</body>
</html>
