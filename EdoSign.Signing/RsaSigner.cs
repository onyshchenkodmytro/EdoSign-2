using System;
using System.Security.Cryptography;
using System.Text;

namespace EdoSign.Signing
{
    public class RsaSigner : ISigner, IDisposable
    {
        private static readonly RSA _sharedRsa = RSA.Create(2048); // спільний ключ
        private bool _disposed;

        public byte[] Sign(byte[] data) =>
            _sharedRsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        public bool Verify(byte[] data, byte[] signature) =>
            _sharedRsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        public string GetPublicKeyPem()
        {
            var pub = _sharedRsa.ExportSubjectPublicKeyInfo();
            var base64 = Convert.ToBase64String(pub);
            var sb = new StringBuilder();
            sb.AppendLine("-----BEGIN PUBLIC KEY-----");
            for (int i = 0; i < base64.Length; i += 64)
                sb.AppendLine(base64.Substring(i, Math.Min(64, base64.Length - i)));
            sb.AppendLine("-----END PUBLIC KEY-----");
            return sb.ToString();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                // нічого не робимо, бо ключ спільний
                _disposed = true;
            }
        }
    }
}


