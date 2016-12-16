<%@ Page Title="主页" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="Nxt.Tests._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    请输入图书序号：<asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
    <asp:Button ID="Button1" runat="server" onclick="Button1_Click" Text="绑定列表" />
    <p>
        <asp:GridView ID="GridView1" runat="server" BackColor="White" 
            BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="4" 
            ForeColor="Black" GridLines="Horizontal">
            <FooterStyle BackColor="#CCCC99" ForeColor="Black" />
            <HeaderStyle BackColor="#333333" Font-Bold="True" ForeColor="White" />
            <PagerStyle BackColor="White" ForeColor="Black" HorizontalAlign="Right" />
            <SelectedRowStyle BackColor="#CC3333" Font-Bold="True" ForeColor="White" />
            <SortedAscendingCellStyle BackColor="#F7F7F7" />
            <SortedAscendingHeaderStyle BackColor="#4B4B4B" />
            <SortedDescendingCellStyle BackColor="#E5E5E5" />
            <SortedDescendingHeaderStyle BackColor="#242121" />
        </asp:GridView>
    </p>
    <p>
       
        &nbsp;</p>
    <p>
       
        图书序号：<asp:TextBox ID="txtBookID" runat="server"></asp:TextBox>
    </p>
    <p>
       
        图书名称：<asp:TextBox ID="txtBookName" runat="server"></asp:TextBox>
    </p>
    <p>
       
        图书价格：<asp:TextBox ID="txtBookPrice" runat="server"></asp:TextBox>
    </p>
    <p>
       
        出版单位：<asp:TextBox ID="txtBookPublish" runat="server"></asp:TextBox>
    </p>
    <p>
       
        <asp:Button ID="btnAdd" runat="server" onclick="btnAdd_Click" Text="添加" />
        <asp:Button ID="btnUpdate" runat="server" onclick="btnUpdate_Click" Text="更新" />
        <asp:Button ID="btnDelete" runat="server" onclick="btnDelete_Click" Text="删除" />
    </p>
    <p>
       
        <asp:Label ID="lblLog" runat="server"></asp:Label>
    </p>
</asp:Content>
