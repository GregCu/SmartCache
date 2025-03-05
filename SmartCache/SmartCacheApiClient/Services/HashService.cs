using System.Security.Cryptography;
using System.Text;

namespace SmartCacheApiClient.Services
{
    public class HashService : IHashService
    {
        public string CalculateHash(string email)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(email));
            return Convert.ToHexString(bytes);
        }
    }
}
