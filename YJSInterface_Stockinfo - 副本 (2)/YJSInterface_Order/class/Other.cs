using System;
using System.Management;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Text;


/// <summary>
/// Other帮助类
/// </summary>
public class OtherHelper
{
    /// <summary>
    /// 获取当前IP，限定了IPV4的
    /// </summary>
    /// <returns></returns>
    public static string GetIp()
    {
        string ip = "";
        string hostInfo = Dns.GetHostName();
        //IP地址
        //System.Net.IPAddress[] addressList = Dns.GetHostByName(Dns.GetHostName()).AddressList;这个过时
        System.Net.IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
        for (int i = 0; i < addressList.Length; i++)
        {
            if (!addressList[i].IsIPv6LinkLocal)
            {
                if (addressList[i].AddressFamily.ToString() == "InterNetwork")
                {
                    ip = addressList[i].ToString();
                }
            }
        }
        return ip;
    }
    /// <summary>
    /// 获取MAC
    /// </summary>
    /// <returns></returns>
    public static string GetMac()
    {
        string mac = "";
        ManagementClass mc;
        mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
        ManagementObjectCollection moc = mc.GetInstances();
        foreach (ManagementObject mo in moc)
        {
            if (mo["IPEnabled"].ToString() == "True")
            {
                mac = mo["MacAddress"].ToString();
            }
        }
        return mac;
    }
}
