namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Collections.Generic;

    internal class MQCipherMappingTable : MQBase
    {
        private static List<MQCipherMapplingList> cipherList = new List<MQCipherMapplingList>();

        static MQCipherMappingTable()
        {
            cipherList.Add(new MQCipherMapplingList("TLS_RSA_WITH_AES_128_CBC_SHA", "TLS_RSA_WITH_AES_128_CBC_SHA", "TLS 1.0"));
            cipherList.Add(new MQCipherMapplingList("TLS_RSA_WITH_AES_256_CBC_SHA", "TLS_RSA_WITH_AES_256_CBC_SHA", "TLS 1.0"));
            cipherList.Add(new MQCipherMapplingList("TLS_RSA_WITH_3DES_EDE_CBC_SHA", "TLS_RSA_WITH_3DES_EDE_CBC_SHA", "TLS 1.0"));
            cipherList.Add(new MQCipherMapplingList("RC4_SHA_US", "TLS_RSA_WITH_RC4_128_SHA", "SSL 3.0"));
            cipherList.Add(new MQCipherMapplingList("RC4_MD5_EXPORT", "TLS_RSA_EXPORT_WITH_RC4_40_MD5", "SSL 3.0"));
            cipherList.Add(new MQCipherMapplingList("RC4_56_SHA_EXPORT1024", "TLS_RSA_EXPORT1024_WITH_RC4_56_SHA", "SSL 3.0"));
            cipherList.Add(new MQCipherMapplingList("RC4_MD5_US", "TLS_RSA_WITH_RC4_128_MD5", "SSL 3.0"));
            cipherList.Add(new MQCipherMapplingList("TLS_RSA_WITH_AES_128_CBC_SHA256", "TLS_RSA_WITH_AES_128_CBC_SHA256", "TLS 1.2"));
            cipherList.Add(new MQCipherMapplingList("TLS_RSA_WITH_AES_256_CBC_SHA256", "TLS_RSA_WITH_AES_256_CBC_SHA256", "TLS 1.2"));
            cipherList.Add(new MQCipherMapplingList("ECDHE_RSA_AES_128_CBC_SHA256", "TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256_P256", "TLS 1.2"));
            cipherList.Add(new MQCipherMapplingList("ECDHE_RSA_AES_128_CBC_SHA256", "TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256_P384", "TLS 1.2"));
            cipherList.Add(new MQCipherMapplingList("ECDHE_RSA_AES_128_CBC_SHA256", "TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256_P521", "TLS 1.2"));
            cipherList.Add(new MQCipherMapplingList("ECDHE_RSA_AES_256_CBC_SHA256", "TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA256_P256", "TLS 1.2"));
            cipherList.Add(new MQCipherMapplingList("ECDHE_RSA_AES_256_CBC_SHA256", "TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA256_P384", "TLS 1.2"));
            cipherList.Add(new MQCipherMapplingList("ECDHE_RSA_AES_256_CBC_SHA256", "TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA256_P521", "TLS 1.2"));
            cipherList.Add(new MQCipherMapplingList("ECDHE_ECDSA_AES_128_CBC_SHA256", "TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256_P256", "TLS 1.2"));
            cipherList.Add(new MQCipherMapplingList("ECDHE_ECDSA_AES_128_CBC_SHA256", "TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256_P384", "TLS 1.2"));
            cipherList.Add(new MQCipherMapplingList("ECDHE_ECDSA_AES_128_CBC_SHA256", "TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256_P521", "TLS 1.2"));
            cipherList.Add(new MQCipherMapplingList("ECDHE_ECDSA_AES_256_CBC_SHA384", "TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA384_P384", "TLS 1.2"));
            cipherList.Add(new MQCipherMapplingList("ECDHE_ECDSA_AES_256_CBC_SHA384", "TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA384_P521", "TLS 1.2"));
            cipherList.Add(new MQCipherMapplingList("ECDHE_ECDSA_AES_128_GCM_SHA256", "TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256_P256", "TLS 1.2"));
            cipherList.Add(new MQCipherMapplingList("ECDHE_ECDSA_AES_128_GCM_SHA256", "TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256_P384", "TLS 1.2"));
            cipherList.Add(new MQCipherMapplingList("ECDHE_ECDSA_AES_128_GCM_SHA256", "TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256_P521", "TLS 1.2"));
            cipherList.Add(new MQCipherMapplingList("ECDHE_ECDSA_AES_256_GCM_SHA384", "TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384_P384", "TLS 1.2"));
            cipherList.Add(new MQCipherMapplingList("ECDHE_ECDSA_AES_256_GCM_SHA384", "TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384_P521", "TLS 1.2"));
        }

        public static string GetDotnetNameForCipher(string cipher)
        {
            foreach (MQCipherMapplingList list in cipherList)
            {
                if (list.MQCipherName == cipher)
                {
                    return list.CSCipherName;
                }
            }
            return null;
        }

        public static string GetMQNameForCipher(string cipher)
        {
            foreach (MQCipherMapplingList list in cipherList)
            {
                if (list.CSCipherName == cipher)
                {
                    return list.MQCipherName;
                }
            }
            return null;
        }

        public static string GetSSLPrtocolVersionForCipher(string cipher)
        {
            foreach (MQCipherMapplingList list in cipherList)
            {
                if ((list.CSCipherName == cipher) || (list.MQCipherName == cipher))
                {
                    return list.SSLVersion;
                }
            }
            return null;
        }
    }
}

