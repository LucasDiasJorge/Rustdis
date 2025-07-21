using System.Text.Json.Serialization;

namespace RustdisSDK.Models
{
    /// <summary>
    /// Representa um comando para o Rustdis
    /// </summary>
    public class RustdisCommand
    {
        [JsonPropertyName("command")]
        public string Command { get; set; } = string.Empty;

        [JsonPropertyName("args")]
        public object? Args { get; set; }
    }

    /// <summary>
    /// Argumentos para comando GET
    /// </summary>
    public class GetArgs
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;
    }

    /// <summary>
    /// Argumentos para comando SET
    /// </summary>
    public class SetArgs
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Argumentos para comando DEL
    /// </summary>
    public class DelArgs
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;
    }

    /// <summary>
    /// Argumentos para comando EXISTS
    /// </summary>
    public class ExistsArgs
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resposta do Rustdis
    /// </summary>
    public class RustdisResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }
    }
}
