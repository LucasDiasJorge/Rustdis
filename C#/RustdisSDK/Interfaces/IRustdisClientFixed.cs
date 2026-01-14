using System;
using System.Threading.Tasks;
using RustdisSDK.Models;

namespace RustdisSDK.Interfaces
{
    /// <summary>
    /// Interface para cliente Rustdis
    /// </summary>
    public interface IRustdisClient : IDisposable
    {
        /// <summary>
        /// Obtém um valor pela chave
        /// </summary>
        /// <param name="key">Chave a ser buscada</param>
        /// <returns>Valor associado à chave ou null se não encontrado</returns>
        Task<string?> GetAsync(string key);

        /// <summary>
        /// Define um par chave-valor
        /// </summary>
        /// <param name="key">Chave</param>
        /// <param name="value">Valor</param>
        /// <returns>True se operação foi bem-sucedida</returns>
        Task<bool> SetAsync(string key, string value);

        /// <summary>
        /// Remove uma chave
        /// </summary>
        /// <param name="key">Chave a ser removida</param>
        /// <returns>True se a chave foi removida</returns>
        Task<bool> DeleteAsync(string key);

        /// <summary>
        /// Verifica se uma chave existe
        /// </summary>
        /// <param name="key">Chave a ser verificada</param>
        /// <returns>True se a chave existe</returns>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// Obtém todas as chaves
        /// </summary>
        /// <returns>Lista de todas as chaves</returns>
        Task<string[]> GetKeysAsync();

        /// <summary>
        /// Limpa todos os dados
        /// </summary>
        /// <returns>True se operação foi bem-sucedida</returns>
        Task<bool> FlushAsync();

        /// <summary>
        /// Obtém o número de chaves
        /// </summary>
        /// <returns>Número de chaves no cache</returns>
        Task<int> GetSizeAsync();

        /// <summary>
        /// Testa a conexão
        /// </summary>
        /// <returns>True se conexão está ativa</returns>
        Task<bool> PingAsync();

        /// <summary>
        /// Executa um comando JSON personalizado
        /// </summary>
        /// <param name="jsonCommand">Comando em formato JSON</param>
        /// <returns>Resposta do comando</returns>
        Task<string> ExecuteCommandAsync(string jsonCommand);
    }
}
