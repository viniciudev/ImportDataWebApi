using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace ImportDataWebApi
{
   
        public class DataProtectionConverter : ValueConverter<string, string>
        {
            private static byte[] _chave = Encoding.UTF8.GetBytes("#lgpd+ef");

            public DataProtectionConverter()
                : base(_convertTo, _convertFrom, default)
            {
            }

            static Expression<Func<string, string>> _convertTo = x => LockView(x);
            static Expression<Func<string, string>> _convertFrom = x => UnLockView(x);

            static string LockView(string texto)
            {
                using var hashProvider = new MD5CryptoServiceProvider();
                var encriptar = new TripleDESCryptoServiceProvider
                {
                    Mode = CipherMode.ECB,
                    Key = hashProvider.ComputeHash(_chave),
                    Padding = PaddingMode.PKCS7
                };

                using var transforme = encriptar.CreateEncryptor();
                var dados = Encoding.UTF8.GetBytes(texto);
                return Convert.ToBase64String(transforme.TransformFinalBlock(dados, 0, dados.Length));
            }

            static string UnLockView(string texto)
            {
                using var hashProvider = new MD5CryptoServiceProvider();
                var descriptografar = new TripleDESCryptoServiceProvider
                {
                    Mode = CipherMode.ECB,
                    Key = hashProvider.ComputeHash(_chave),
                    Padding = PaddingMode.PKCS7
                };

                using var transforme = descriptografar.CreateDecryptor();
                var dados = Convert.FromBase64String(texto.Replace(" ", "+"));
                return Encoding.UTF8.GetString(transforme.TransformFinalBlock(dados, 0, dados.Length));
            }
        }
    
}
