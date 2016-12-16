using System;
using System.Data;
using Microsoft.ApplicationBlocks.Data;
using EZCALib;
using System.Security.Cryptography.X509Certificates;
using System.Data;

public class CaHelper
{

    //初始数据
    BaseCA ca = new BaseCA();
    public CaHelper()
    {
    }

    public string GetDigest(string dat)
    {
        
        string member = "";
        string digitalDigest = "";
        DateTime dt = new DateTime();
        String p7b = "";//P7签名值
        try
        {
            //初始数据
            var MyFilter = ca.MyFilter;
            var Collection = ca.MyCollection;
            var certs = ca.MyCertificates;
            var CertSign = ca.MyCertificate;
            MyFilter.CertType = "DIGITAL_SIGNATURE";
            MyFilter.Issuer = "Chongqing  CA,East-Zhongxun CA";
            certs.NewEnum(MyFilter);

            //判断证书库是否存在证书

            if (certs.Count == 0)
            {
                //MessageBox.Show("请确认插入UDBKEY或安装了USBKEY驱动!");
                throw new Exception("请确认插入UDBKEY或安装了USBKEY驱动!");
            }

            //选择证书
            CertSign = certs.GetTheChooseCert();//获取ukey中的失效时间

            //校验ukey是否过期
            var enddate = CertSign.NotAfter;
            var beforedate = DateTime.Now.AddDays(-30);
            var nowdate = DateTime.Now;

            TimeSpan beforeTS = new TimeSpan(beforedate.Ticks);
            TimeSpan nowTS = new TimeSpan(nowdate.Ticks);
            TimeSpan endTS = new TimeSpan(DateTime.Parse(enddate).Ticks);

            if (beforeTS > endTS && endTS > nowTS)
            {
                throw new Exception("证书即将到期，请及时到东方中讯公司证书更新!");
            }
            //dat = HashList("zzllyy_7788");
            p7b = CertSign.SignDataByP7(dat, 1);
            digitalDigest = p7b;

            EZCALib.Certificate cert = new Certificate();
            String certcode = cert.GetP7SignDataInfo(p7b, 2);//证书base64编码
            String indate = dat;
            String rev = cert.VerifySignedDataByP7(indate, certcode, p7b, 1);//原文随机数从session获取。


            if (rev == "" || rev == "-1")
            {
                throw new Exception(rev + "CA验证失败!");
            }
            else
            {
                byte[] certb = Convert.FromBase64String(certcode);
                X509Certificate2 x509cert = new X509Certificate2(certb);
                String Subject = x509cert.Subject;
                int findex = Subject.IndexOf("SERIALNUMBER=") + 13;
                int ss = Subject.IndexOf(',', findex);
                member = Subject.Substring(findex, ss - findex);
                return digitalDigest;
            }

        }
        catch (Exception ex)
        {
            //MessageBox.Show("CA控件未安装，请下载后才能正常使用数字证书！");
            //throw (ex);
            return "";
        }
    }

    //public string HashList(string code)
    //{
    //    System.Security.Cryptography.SHA1 sha = System.Security.Cryptography.SHA1.Create();
    //    byte[] bytResult = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(code));
    //    string strResult = "";
    //    //字节类型的数组转换为字符串

    //    for (int i = 0; i < bytResult.Length; i++)
    //    {
    //        //16进制转换 
    //        string resulti = bytResult[i].ToString("X");
    //        if (resulti.Length == 1)
    //        {
    //            resulti = "0" + resulti;
    //        }
    //        strResult = strResult + resulti;
    //    }

    //    return strResult;
    //}
 }

