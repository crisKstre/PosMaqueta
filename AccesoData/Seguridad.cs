using System;
using System.Security.Cryptography;
using System.Text;

namespace AccesoData
{
    public static class Seguridad
    {
        // Hash SHA256 simple. Para producción se recomienda PBKDF2 con salt por usuario.
        public static string Hash(string texto)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(texto));
                var sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
