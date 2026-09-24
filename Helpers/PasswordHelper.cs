using System.Security.Cryptography;

namespace ControleEstoque.Desktop.Helpers
{
    // Responsável por transformar uma senha em um "hash" seguro para guardar no banco,
    // e por conferir depois se uma senha digitada corresponde a esse hash.
    //
    // Usamos PBKDF2 (Password-Based Key Derivation Function 2), um algoritmo padrão da
    // indústria para senhas — ele é "lento de propósito", o que dificulta muito um ataque
    // de força bruta caso alguém tenha acesso ao arquivo do banco de dados.
    public static class PasswordHelper
    {
        private const int TamanhoSaltEmBytes = 16;
        private const int TamanhoHashEmBytes = 32;
        private const int Iteracoes = 100_000;

        // Gera um "salt" (valor aleatório único) e o hash da senha combinado com esse salt.
        // O salt é necessário guardar junto, pois é preciso dele para conferir a senha depois.
        public static (string Hash, string Salt) CriarHash(string senha)
        {
            var saltBytes = RandomNumberGenerator.GetBytes(TamanhoSaltEmBytes);
            var hashBytes = Rfc2898DeriveBytes.Pbkdf2(senha, saltBytes, Iteracoes, HashAlgorithmName.SHA256, TamanhoHashEmBytes);

            return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
        }

        // Refaz o mesmo cálculo com a senha digitada e o salt guardado, e compara os resultados.
        public static bool Verificar(string senhaDigitada, string hashArmazenado, string saltArmazenado)
        {
            var saltBytes = Convert.FromBase64String(saltArmazenado);
            var hashBytes = Rfc2898DeriveBytes.Pbkdf2(senhaDigitada, saltBytes, Iteracoes, HashAlgorithmName.SHA256, TamanhoHashEmBytes);
            var hashCalculado = Convert.ToBase64String(hashBytes);

            return hashCalculado == hashArmazenado;
        }
    }
}
