using System;
using System.Security.Cryptography;
using System.Text;
using AccesoData;
using Xunit;

namespace PosMaqueta.Tests
{
    /// <summary>
    /// Hash de contraseñas PBKDF2-SHA256 con salt, compatible hacia atrás con el
    /// formato antiguo (SHA256 hex) para migrar logins existentes de forma transparente.
    /// </summary>
    public class SeguridadTests
    {
        [Fact]
        public void Hash_tiene_formato_iteraciones_salt_hash()
        {
            var partes = Seguridad.Hash("clave123").Split('$');
            Assert.Equal(3, partes.Length);
            Assert.Equal("100000", partes[0]);
            Assert.Equal(16, Convert.FromBase64String(partes[1]).Length); // salt 128 bits
            Assert.Equal(32, Convert.FromBase64String(partes[2]).Length); // hash 256 bits
        }

        [Fact]
        public void Hash_usa_un_salt_distinto_cada_vez()
            => Assert.NotEqual(Seguridad.Hash("clave123"), Seguridad.Hash("clave123"));

        [Fact]
        public void Verificar_acepta_la_password_correcta()
            => Assert.True(Seguridad.Verificar("clave123", Seguridad.Hash("clave123")));

        [Fact]
        public void Verificar_rechaza_la_password_incorrecta()
            => Assert.False(Seguridad.Verificar("otra", Seguridad.Hash("clave123")));

        [Fact]
        public void Verificar_distingue_mayusculas_de_minusculas()
            => Assert.False(Seguridad.Verificar("Clave123", Seguridad.Hash("clave123")));

        [Fact]
        public void Verificar_funciona_con_password_vacia()
            => Assert.True(Seguridad.Verificar("", Seguridad.Hash("")));

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Verificar_rechaza_hash_nulo_o_vacio(string hash)
            => Assert.False(Seguridad.Verificar("clave123", hash));

        [Theory]
        [InlineData("dos$partes")]                       // recuento de partes != 3
        [InlineData("100000$no-base64$tampoco")]         // base64 inválido -> excepción capturada
        [InlineData("abc$c2FsdA==$aGFzaA==")]            // iteraciones no numéricas
        [InlineData("100000$c2FsdA==$aGFzaA==$extra")]   // más de 3 partes
        [InlineData("$$")]                               // tres partes vacías
        public void Verificar_rechaza_hash_malformado(string hash)
            => Assert.False(Seguridad.Verificar("clave123", hash));

        [Fact]
        public void Verificar_acepta_hash_legacy_sha256()
            => Assert.True(Seguridad.Verificar("clave123", Sha256Hex("clave123")));

        [Fact]
        public void Verificar_legacy_es_insensible_a_mayusculas_del_hex()
            => Assert.True(Seguridad.Verificar("clave123", Sha256Hex("clave123").ToUpperInvariant()));

        [Fact]
        public void Verificar_rechaza_legacy_con_password_incorrecta()
            => Assert.False(Seguridad.Verificar("otra", Sha256Hex("clave123")));

        [Fact]
        public void NecesitaRehash_es_true_para_hash_legacy()
            => Assert.True(Seguridad.NecesitaRehash(Sha256Hex("clave123")));

        [Fact]
        public void NecesitaRehash_es_false_para_hash_pbkdf2()
            => Assert.False(Seguridad.NecesitaRehash(Seguridad.Hash("clave123")));

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void NecesitaRehash_es_true_para_nulo_o_vacio(string hash)
            => Assert.True(Seguridad.NecesitaRehash(hash));

        // Verificar lee las iteraciones DEL hash almacenado, no de una constante fija: un hash
        // generado con otro costo (p.ej. 50000) debe seguir validando. Es el sentido del formato.
        [Fact]
        public void Verificar_acepta_hash_pbkdf2_con_iteraciones_distintas_a_la_constante()
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
            using (var k = new Rfc2898DeriveBytes("clave123", salt, 50000, HashAlgorithmName.SHA256))
            {
                string hash = "50000$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(k.GetBytes(32));
                Assert.True(Seguridad.Verificar("clave123", hash));
                Assert.False(Seguridad.Verificar("mala", hash));
            }
        }

        // El flujo real de migración transparente, de punta a punta (lo que hace UsuarioDao.Login).
        [Fact]
        public void Flujo_de_migracion_legacy_a_pbkdf2()
        {
            var legacy = Sha256Hex("clave123");
            Assert.True(Seguridad.Verificar("clave123", legacy));   // el login antiguo sigue funcionando
            Assert.True(Seguridad.NecesitaRehash(legacy));          // y queda marcado para migrar
            var nuevo = Seguridad.Hash("clave123");                 // se regenera con PBKDF2
            Assert.True(Seguridad.Verificar("clave123", nuevo));    // sigue validando
            Assert.False(Seguridad.NecesitaRehash(nuevo));          // ya no necesita migrar
        }

        // Contraseñas con caracteres no-ASCII (proyecto en español) deben hacer round-trip en ambas rutas.
        [Fact]
        public void Verificar_funciona_con_unicode_en_ambas_rutas()
        {
            string pwd = "Contraseña-Ñoño-áéíóú";
            Assert.True(Seguridad.Verificar(pwd, Seguridad.Hash(pwd)));   // ruta PBKDF2
            Assert.True(Seguridad.Verificar(pwd, Sha256Hex(pwd)));        // ruta legacy
        }

        [Fact]
        public void Verificar_legacy_funciona_con_password_vacia()
            => Assert.True(Seguridad.Verificar("", Sha256Hex("")));

        // Heurística intencional: un hash con '$' se trata como "formato nuevo". Es seguro porque
        // Verificar rechaza igual los malformados, así que el login falla antes de consultar esto.
        [Theory]
        [InlineData("abc$x$y")]
        [InlineData("basura$con$dolar")]
        public void NecesitaRehash_es_false_si_contiene_dolar_aunque_sea_invalido(string hash)
            => Assert.False(Seguridad.NecesitaRehash(hash));

        // Réplica del formato antiguo (SHA256 hex en minúsculas) para probar la compatibilidad.
        private static string Sha256Hex(string texto)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(texto));
                var sb = new StringBuilder();
                foreach (var b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
