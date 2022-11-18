using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace RX.Nyss.Common.Services;

public interface ICryptographyService
{
    public string Encrypt(string encryptedText, string encryptionKey, string supplementaryKey);
    public string Decrypt(string data, string encryptionKey, string supplementaryKey);
}

public class CryptographyService : ICryptographyService
{
    public string Decrypt(string encryptedText, string encryptionKey, string supplementaryKey) => DecryptStringAES(encryptedText, Get128KeyWithSalt(encryptionKey, supplementaryKey));

    public string Encrypt(string data, string encryptionKey, string supplementaryKey) => EncryptStringAES(data, Get128KeyWithSalt(encryptionKey, supplementaryKey));

    private string EncryptStringAES(string plainText, string key128)
    {
        byte[] iv = new byte[16];
        byte[] array;

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key128);
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                    {
                        streamWriter.Write(plainText);
                    }

                    array = memoryStream.ToArray();
                }
            }
        }

        return WebEncoders.Base64UrlEncode(array);
    }

    private string DecryptStringAES(string cipherText, string key128)
    {
        byte[] iv = new byte[16];
        byte[] buffer = WebEncoders.Base64UrlDecode(cipherText);

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key128);
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }
    }

    private string Get128KeyWithSalt(string encryptionKey, string supplementaryKey)
    {
        var key = encryptionKey;
        var salt = supplementaryKey;

        var plainText = Encoding.UTF8.GetBytes(key);
        var saltBytes = Encoding.UTF8.GetBytes(salt);

        var plainTextWithSaltBytes = new byte[plainText.Length + salt.Length];

        for (var i = 0; i < plainText.Length; i++)
        {
            plainTextWithSaltBytes[i] = plainText[i];
        }
        for (var i = 0; i < salt.Length; i++)
        {
            plainTextWithSaltBytes[plainText.Length + i] = saltBytes[i];
        }

        using var md5Hash = MD5.Create();

        var data = md5Hash.ComputeHash(plainTextWithSaltBytes);

        var sBuilder = new StringBuilder();

        for (var i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        return sBuilder.ToString();
    }
}


