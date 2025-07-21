using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using RustdisSDK.Interfaces;
using RustdisSDK.Models;

namespace RustdisSDK.Client
{
    /// <summary>
    /// Cliente para interagir com Rustdis via processo
    /// </summary>
    public class RustdisClient : IRustdisClient, IDisposable
    {
        private readonly string _rustdisPath;
        private bool _disposed = false;

        /// <summary>
        /// Inicializa novo cliente Rustdis
        /// </summary>
        /// <param name="rustdisPath">Caminho para o executável do Rustdis</param>
        public RustdisClient(string rustdisPath)
        {
            _rustdisPath = rustdisPath ?? throw new ArgumentNullException(nameof(rustdisPath));
        }

        /// <inheritdoc />
        public async Task<string?> GetAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            var result = await ExecuteRustdisCommandAsync("get", key);
            
            // Parse JSON response
            if (result.StartsWith("\"") && result.EndsWith("\""))
            {
                return JsonSerializer.Deserialize<string>(result);
            }
            
            return result == "null" ? null : result;
        }

        /// <inheritdoc />
        public async Task<bool> SetAsync(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var result = await ExecuteRustdisCommandAsync("set", key, value);
            return result.Contains("OK") || result == "\"OK\"";
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            var result = await ExecuteRustdisCommandAsync("del", key);
            return result == "true" || result == "1";
        }

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            var result = await ExecuteRustdisCommandAsync("exists", key);
            return result == "true" || result == "1";
        }

        /// <inheritdoc />
        public async Task<string[]> GetKeysAsync()
        {
            var result = await ExecuteRustdisCommandAsync("keys");
            
            try
            {
                var keys = JsonSerializer.Deserialize<string[]>(result);
                return keys ?? Array.Empty<string>();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        /// <inheritdoc />
        public async Task<bool> FlushAsync()
        {
            var result = await ExecuteRustdisCommandAsync("flush");
            return result.Contains("OK") || result == "\"OK\"";
        }

        /// <inheritdoc />
        public async Task<int> GetSizeAsync()
        {
            var result = await ExecuteRustdisCommandAsync("size");
            return int.TryParse(result, out var size) ? size : 0;
        }

        /// <inheritdoc />
        public async Task<bool> PingAsync()
        {
            try
            {
                var result = await ExecuteRustdisCommandAsync("ping");
                return result.Contains("PONG");
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<string> ExecuteCommandAsync(string jsonCommand)
        {
            if (string.IsNullOrWhiteSpace(jsonCommand))
                throw new ArgumentException("JSON command cannot be null or empty", nameof(jsonCommand));

            // Para comandos JSON, usar stdin do processo
            return await ExecuteRustdisJsonCommandAsync(jsonCommand);
        }

        /// <summary>
        /// Executa comando Rustdis via linha de comando
        /// </summary>
        private async Task<string> ExecuteRustdisCommandAsync(string command, params string[] args)
        {
            var arguments = new StringBuilder($"{command}");
            foreach (var arg in args)
            {
                arguments.Append($" \"{arg}\"");
            }

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _rustdisPath,
                    Arguments = arguments.ToString(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();

            if (process.ExitCode != 0 && !string.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException($"Rustdis command failed: {error}");
            }

            return output.Trim();
        }

        /// <summary>
        /// Executa comando JSON via stdin
        /// </summary>
        private async Task<string> ExecuteRustdisJsonCommandAsync(string jsonCommand)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _rustdisPath,
                    Arguments = "cli", // Modo CLI interativo
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();

            // Enviar comando JSON
            await process.StandardInput.WriteLineAsync(jsonCommand);
            await process.StandardInput.WriteLineAsync("quit");
            await process.StandardInput.FlushAsync();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0 && !string.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException($"Rustdis JSON command failed: {error}");
            }

            // Extrair apenas a resposta do comando (remover prompt, etc.)
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            return lines.Length > 1 ? lines[1].Trim() : output.Trim();
        }

        /// <summary>
        /// Libera recursos
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Libera recursos gerenciados
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // Cleanup if needed
                _disposed = true;
            }
        }
    }
}
