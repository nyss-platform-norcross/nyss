using RX.Nyss.Web.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RX.Nyss.Web.Services;

public interface ICryptographyService
{
    public string Encrypt(string data);
    public string Decrypt(string encryptedTex);
}

public class CryptographyService : ICryptographyService
{
    private readonly INyssWebConfig _config;

    public CryptographyService(INyssWebConfig config)
    {
        _config = config;
    }

    public string Decrypt(string encryptedText) => DecryptStringAES(encryptedText, Get128KeyWithSalt());

    public string Encrypt(string data) => EncryptStringAES(data, Get128KeyWithSalt());

    private string EncryptStringAES(string plainText, string key128)
    {
        var iv = new byte[16];

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key128);
        aes.IV = iv;

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        using var streamWriter = new StreamWriter(cryptoStream);
        streamWriter.Write(plainText);

        var array = memoryStream.ToArray();

        return WebEncoders.Base64UrlEncode(array);
    }

    private string DecryptStringAES(string cipherText, string key128)
    {
        var iv = new byte[16];
        var buffer = WebEncoders.Base64UrlDecode(cipherText);

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key128);
        aes.IV = iv;
        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using var memoryStream = new MemoryStream(buffer);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream);
        return streamReader.ReadToEnd();
    }

    private string Get128KeyWithSalt()
    {
        var key = _config.Key;
        var salt = _config.SupplementaryKey;

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


