using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using RustdisSDK.Interfaces;

namespace RustdisSDK.Extensions
{
    /// <summary>
    /// Extensões para o cliente Rustdis
    /// </summary>
    public static class RustdisClientExtensions
    {
        /// <summary>
        /// Obtém e deserializa um objeto
        /// </summary>
        /// <typeparam name="T">Tipo do objeto</typeparam>
        /// <param name="client">Cliente Rustdis</param>
        /// <param name="key">Chave</param>
        /// <returns>Objeto deserializado ou default(T)</returns>
        public static async Task<T?> GetObjectAsync<T>(this IRustdisClient client, string key)
        {
            var json = await client.GetAsync(key);
            if (string.IsNullOrEmpty(json))
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Serializa e armazena um objeto
        /// </summary>
        /// <typeparam name="T">Tipo do objeto</typeparam>
        /// <param name="client">Cliente Rustdis</param>
        /// <param name="key">Chave</param>
        /// <param name="obj">Objeto a ser armazenado</param>
        /// <returns>True se operação foi bem-sucedida</returns>
        public static async Task<bool> SetObjectAsync<T>(this IRustdisClient client, string key, T obj)
        {
            if (obj == null)
                return await client.DeleteAsync(key);

            try
            {
                var json = JsonSerializer.Serialize(obj);
                return await client.SetAsync(key, json);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtém ou define um valor (get-or-set pattern)
        /// </summary>
        /// <param name="client">Cliente Rustdis</param>
        /// <param name="key">Chave</param>
        /// <param name="valueFactory">Função para gerar valor se não existir</param>
        /// <returns>Valor existente ou novo valor criado</returns>
        public static async Task<string> GetOrSetAsync(this IRustdisClient client, string key, Func<Task<string>> valueFactory)
        {
            var existingValue = await client.GetAsync(key);
            if (existingValue != null)
                return existingValue;

            var newValue = await valueFactory();
            await client.SetAsync(key, newValue);
            return newValue;
        }

        /// <summary>
        /// Obtém ou define um objeto (get-or-set pattern)
        /// </summary>
        /// <typeparam name="T">Tipo do objeto</typeparam>
        /// <param name="client">Cliente Rustdis</param>
        /// <param name="key">Chave</param>
        /// <param name="objectFactory">Função para gerar objeto se não existir</param>
        /// <returns>Objeto existente ou novo objeto criado</returns>
        public static async Task<T> GetOrSetObjectAsync<T>(this IRustdisClient client, string key, Func<Task<T>> objectFactory)
        {
            var existingObject = await client.GetObjectAsync<T>(key);
            if (existingObject != null && !existingObject.Equals(default(T)))
                return existingObject;

            var newObject = await objectFactory();
            await client.SetObjectAsync(key, newObject);
            return newObject;
        }

        /// <summary>
        /// Incrementa um valor numérico
        /// </summary>
        /// <param name="client">Cliente Rustdis</param>
        /// <param name="key">Chave</param>
        /// <param name="increment">Valor a incrementar (padrão: 1)</param>
        /// <returns>Novo valor após incremento</returns>
        public static async Task<long> IncrementAsync(this IRustdisClient client, string key, long increment = 1)
        {
            var currentValue = await client.GetAsync(key);
            var numericValue = string.IsNullOrEmpty(currentValue) ? 0 : long.Parse(currentValue);
            var newValue = numericValue + increment;
            
            await client.SetAsync(key, newValue.ToString());
            return newValue;
        }

        /// <summary>
        /// Decrementa um valor numérico
        /// </summary>
        /// <param name="client">Cliente Rustdis</param>
        /// <param name="key">Chave</param>
        /// <param name="decrement">Valor a decrementar (padrão: 1)</param>
        /// <returns>Novo valor após decremento</returns>
        public static async Task<long> DecrementAsync(this IRustdisClient client, string key, long decrement = 1)
        {
            return await client.IncrementAsync(key, -decrement);
        }

        /// <summary>
        /// Verifica se múltiplas chaves existem
        /// </summary>
        /// <param name="client">Cliente Rustdis</param>
        /// <param name="keys">Chaves a verificar</param>
        /// <returns>Dicionário com resultado para cada chave</returns>
        public static async Task<Dictionary<string, bool>> ExistsMultipleAsync(this IRustdisClient client, params string[] keys)
        {
            var result = new Dictionary<string, bool>();
            var tasks = keys.Select(async key => new { Key = key, Exists = await client.ExistsAsync(key) });
            var results = await Task.WhenAll(tasks);
            
            foreach (var item in results)
            {
                result[item.Key] = item.Exists;
            }
            
            return result;
        }

        /// <summary>
        /// Remove múltiplas chaves
        /// </summary>
        /// <param name="client">Cliente Rustdis</param>
        /// <param name="keys">Chaves a remover</param>
        /// <returns>Número de chaves removidas</returns>
        public static async Task<int> DeleteMultipleAsync(this IRustdisClient client, params string[] keys)
        {
            var tasks = keys.Select(client.DeleteAsync);
            var results = await Task.WhenAll(tasks);
            return results.Count(deleted => deleted);
        }
    }
}
