namespace ThreeTP.Payment.Application.Interfaces
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
        string Hash(string input); //Actualiza en terminal update el Hash cuando cambia el key 
    }
}
