using System;
using System.IO;
using RustdisSDK.Client;
using RustdisSDK.Interfaces;
using System.Threading.Tasks;

namespace RustdisSDK.Factory
{
    /// <summary>
    /// Factory para criar instâncias do cliente Rustdis
    /// </summary>
    public static class RustdisClientFactory
    {
        /// <summary>
        /// Cria cliente com caminho padrão do executável
        /// </summary>
        /// <returns>Nova instância do cliente</returns>
        public static IRustdisClient CreateClient()
        {
            return CreateClient("rustdis.exe");
        }

        /// <summary>
        /// Cria cliente com caminho específico
        /// </summary>
        /// <param name="rustdisPath">Caminho para o executável Rustdis</param>
        /// <returns>Nova instância do cliente</returns>
        public static IRustdisClient CreateClient(string rustdisPath)
        {
            if (string.IsNullOrWhiteSpace(rustdisPath))
                throw new ArgumentException("Rustdis path cannot be null or empty", nameof(rustdisPath));

            if (!File.Exists(rustdisPath))
                throw new FileNotFoundException($"Rustdis executable not found at: {rustdisPath}");

            return new RustdisClient(rustdisPath);
        }

        /// <summary>
        /// Cria cliente procurando o executável no PATH do sistema
        /// </summary>
        /// <returns>Nova instância do cliente</returns>
        public static IRustdisClient CreateClientFromPath()
        {
            var pathVar = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathVar))
                throw new InvalidOperationException("PATH environment variable not found");

            var paths = pathVar.Split(Path.PathSeparator);
            
            foreach (var path in paths)
            {
                var rustdisPath = Path.Combine(path, "rustdis.exe");
                if (File.Exists(rustdisPath))
                {
                    return new RustdisClient(rustdisPath);
                }
                
                // Também tentar sem extensão para sistemas Unix-like
                rustdisPath = Path.Combine(path, "rustdis");
                if (File.Exists(rustdisPath))
                {
                    return new RustdisClient(rustdisPath);
                }
            }

            throw new FileNotFoundException("Rustdis executable not found in PATH");
        }

        /// <summary>
        /// Cria cliente e testa a conexão
        /// </summary>
        /// <param name="rustdisPath">Caminho para o executável</param>
        /// <returns>Cliente testado e pronto para uso</returns>
        public static async Task<IRustdisClient> CreateAndTestClientAsync(string rustdisPath)
        {
            var client = CreateClient(rustdisPath);
            
            var isConnected = await client.PingAsync();
            if (!isConnected)
            {
                client.Dispose();
                throw new InvalidOperationException("Failed to connect to Rustdis");
            }

            return client;
        }
    }
}
