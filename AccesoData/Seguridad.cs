using System;
using System.Security.Cryptography;
using System.Text;

namespace AccesoData
{
    /// <summary>
    /// Hash de contraseñas con PBKDF2-SHA256 + salt aleatorio por usuario.
    /// Formato almacenado: "iteraciones$saltBase64$hashBase64".
    /// Mantiene compatibilidad con el formato antiguo (SHA256 hex) para migrar
    /// las contraseñas existentes de forma transparente en el próximo login.
    /// </summary>
    public static class Seguridad
    {
        private const int SaltSize    = 16;      // 128 bits
        private const int HashSize    = 32;      // 256 bits
        private const int Iteraciones = 100_000; // costo PBKDF2

        /// <summary>Genera un hash seguro nuevo (PBKDF2).</summary>
        public static string Hash(string password)
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            byte[] hash = Pbkdf2(password, salt, Iteraciones);
            return Iteraciones + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Verifica una contraseña contra el hash almacenado.
        /// Acepta el formato nuevo (PBKDF2) y el antiguo (SHA256 hex) para compatibilidad.
        /// </summary>
        public static bool Verificar(string password, string hashGuardado)
        {
            if (string.IsNullOrEmpty(hashGuardado)) return false;

            // Formato nuevo: "iteraciones$salt$hash"
            if (hashGuardado.Contains("$"))
            {
                var partes = hashGuardado.Split('$');
                if (partes.Length != 3 || !int.TryParse(partes[0], out int iter)) return false;
                try
                {
                    byte[] salt     = Convert.FromBase64String(partes[1]);
                    byte[] esperado = Convert.FromBase64String(partes[2]);
                    byte[] actual   = Pbkdf2(password, salt, iter);
                    return IgualesEnTiempoConstante(actual, esperado);
                }
                catch { return false; }
            }

            // Formato antiguo: SHA256 hex (BD creadas antes de la migración)
            return string.Equals(HashSha256Hex(password), hashGuardado, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>True si el hash está en el formato antiguo y conviene regenerarlo con PBKDF2.</summary>
        public static bool NecesitaRehash(string hashGuardado)
            => string.IsNullOrEmpty(hashGuardado) || !hashGuardado.Contains("$");

        private static byte[] Pbkdf2(string password, byte[] salt, int iteraciones)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iteraciones, HashAlgorithmName.SHA256))
                return pbkdf2.GetBytes(HashSize);
        }

        private static string HashSha256Hex(string texto)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(texto));
                var sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        // Comparación en tiempo constante (evita timing attacks)
        private static bool IgualesEnTiempoConstante(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int dif = 0;
            for (int i = 0; i < a.Length; i++) dif |= a[i] ^ b[i];
            return dif == 0;
        }
    }
}
