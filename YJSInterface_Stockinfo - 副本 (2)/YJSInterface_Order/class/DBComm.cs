using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;

/// <summary>
/// DBComm 的摘要说明

/// </summary>
public class DBComm
{
    public static string ConStr = GetConStr();
    public static string DESKEY = "V";
    public static int icount = 0;

	public DBComm()
	{
		//
		// TODO: 在此处添加构造函数逻辑
		//
	}

    public static string GetConStr()
    {
        //return "Data Source=202.198.0.182;Initial Catalog=MergeData;Persist Security Info=True;User ID=sa;Password=1";
        return @"Data Source=202.198.0.142;Initial Catalog=MergeData;Persist Security Info=True;User ID=yjs_user;Password=123";
    }

    //返回数据表
    public static DataTable ExecuteDataTable(string ASQL, params SqlParameter[] commandParameters)
    {
        return SqlHelper.ExecuteDataset(ConStr, CommandType.Text, ASQL, commandParameters).Tables[0];
    }
    public static DataTable ExecuteDataTable_Simple(string ASQL)
    {
        return SqlHelper.ExecuteDataset(ConStr, CommandType.Text, ASQL).Tables[0];
    }

    //返回单值
    public static object ExecuteScalar(string ASQL)
    {
        return SqlHelper.ExecuteScalar(ConStr, CommandType.Text, ASQL);
    }

    //执行SQL
    public static int ExecuteNonQuery(string ASQL, params SqlParameter[] commandParameters)
    {
        return SqlHelper.ExecuteNonQuery(ConStr, CommandType.Text, ASQL, commandParameters);
    }

    //执行SQL_事务控制
    public static int ExecuteNonQuery_Trans(SqlTransaction transaction, string ASQL, params SqlParameter[] commandParameters)
    {
        return SqlHelper.ExecuteNonQuery(transaction, CommandType.Text, ASQL, commandParameters);
    }

    //执行SQL_事务控制，返回第一行第一列值
    public static object ExecuteScalar_Trans(SqlTransaction transaction, string ASQL, params SqlParameter[] commandParameters)
    {
        return SqlHelper.ExecuteScalar(transaction, CommandType.Text, ASQL, commandParameters);
    }

    //执行SQL_事务控制返回数据表
    public static DataTable ExecuteDataTable_Trans(SqlTransaction transaction, string ASQL, params SqlParameter[] commandParameters)
    {
        return SqlHelper.ExecuteDataset(transaction, CommandType.Text, ASQL, commandParameters).Tables[0];
    }

    //执行SQL
    public static int ExecuteNonQuery_Simple(string ASQL)
    {
        return SqlHelper.ExecuteNonQuery(ConStr, CommandType.Text, ASQL);
    }

    //写日志
    public static void WriteLog(string Operator,string Memo,string ip,string lclass,string page,string elapsed)
    {
        //string strSQL = "insert into sys_LOG (L_Operator,L_Time,L_LOG,L_IP,L_Class,L_Page,L_Elapsed) values (" +
        //                 "'" + Operator + "'," +
        //                 "'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," +
        //                 "'" + Memo + "'," +
        //                  "'" + ip + "'," +
        //                   "'" + lclass + "'," +
        //                    "'" + page + "'," +
        //                 "'" + elapsed + "')";

        //SqlHelper.ExecuteNonQuery(ConStr, CommandType.Text, strSQL);
    }
}
