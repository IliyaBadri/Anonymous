using System.Security.Cryptography;

namespace Anonymous.Cryptography
{
    public class RSAManager
    {
        public class KeyPair
        {
            public required byte[] PublicKey { get; set; }
            public required byte[] PrivateKey { get; set; }
        }

        public static KeyPair CreateKeyPair()
        {
            RSA rsa = RSA.Create();
            rsa.KeySize = 4096;
            KeyPair keyPair = new()
            {
                PublicKey = rsa.ExportRSAPublicKey(),
                PrivateKey = rsa.ExportRSAPrivateKey()
            };

            return keyPair;
        }
    }
}
