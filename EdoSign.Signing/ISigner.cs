namespace EdoSign.Signing
{
    public interface ISigner
    {
        byte[] Sign(byte[] data);
        bool Verify(byte[] data, byte[] signature);
        string GetPublicKeyPem();
    }
}

