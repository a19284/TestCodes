namespace IBM.WMQ.Nmqi
{
    using System;
    using System.Security.Authentication;

    internal class MQCipherMapplingList
    {
        internal CipherAlgorithmType cipherAlgoType;
        internal string CSCipherName;
        internal HashAlgorithmType hashType;
        internal ExchangeAlgorithmType keyExAlgoType;
        internal string MQCipherName;
        internal string SSLVersion;

        public MQCipherMapplingList(string MQName, string CSName, string sslVersion)
        {
            this.MQCipherName = MQName;
            this.CSCipherName = CSName;
            this.SSLVersion = sslVersion;
        }
    }
}

