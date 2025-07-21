# RustdisSDK - SDK C# para Rustdis

SDK oficial em C# para interagir com o [Rustdis](https://github.com/LucasDiasJorge/Rustdis) - um clone do Redis implementado em Rust.

## 🚀 Características

- ✅ API assíncrona moderna (async/await)
- ✅ Interface tipada e segura
- ✅ Extensões úteis para operações avançadas
- ✅ Suporte a serialização automática de objetos
- ✅ Pattern Get-or-Set para cache inteligente
- ✅ Operações numéricas (increment/decrement)
- ✅ Operações em lote para melhor performance
- ✅ Testes unitários e de integração
- ✅ Documentação completa com exemplos

## 📦 Instalação

### Via NuGet (quando publicado)
```bash
dotnet add package RustdisSDK
```

### Compilação local
```bash
git clone https://github.com/LucasDiasJorge/Rustdis.git
cd Rustdis/RustdisSDK
dotnet build
```

## 🎯 Uso Básico

### 1. Criar Cliente

```csharp
using RustdisSDK.Factory;

// Opção 1: Caminho específico
var client = RustdisClientFactory.CreateClient(@"C:\caminho\para\rustdis.exe");

// Opção 2: Buscar no PATH do sistema
var client = RustdisClientFactory.CreateClientFromPath();

// Opção 3: Criar e testar conexão
var client = await RustdisClientFactory.CreateAndTestClientAsync(@"caminho\rustdis.exe");
```

### 2. Operações Básicas

```csharp
using var client = RustdisClientFactory.CreateClient("rustdis.exe");

// SET - Definir chave-valor
await client.SetAsync("nome", "Lucas");
await client.SetAsync("idade", "25");

// GET - Obter valor pela chave (exatamente como você pediu!)
var nome = await client.GetAsync("nome");
Console.WriteLine($"Nome: {nome}"); // Output: Nome: Lucas

var idade = await client.GetAsync("idade");
Console.WriteLine($"Idade: {idade}"); // Output: Idade: 25

// EXISTS - Verificar se chave existe
var existe = await client.ExistsAsync("nome");
Console.WriteLine($"Nome existe: {existe}"); // Output: Nome existe: True

// DEL - Remover chave
var removido = await client.DeleteAsync("idade");
Console.WriteLine($"Idade removida: {removido}"); // Output: Idade removida: True

// KEYS - Listar todas as chaves
var chaves = await client.GetKeysAsync();
Console.WriteLine($"Chaves: [{string.Join(", ", chaves)}]");

// SIZE - Obter número de chaves
var tamanho = await client.GetSizeAsync();
Console.WriteLine($"Tamanho: {tamanho}");

// PING - Testar conexão
var conectado = await client.PingAsync();
Console.WriteLine($"Conectado: {conectado}");

// FLUSH - Limpar todos os dados
await client.FlushAsync();
```

## 🎯 Operações Avançadas

### 1. Trabalhar com Objetos

```csharp
using RustdisSDK.Extensions;

// Serializar e armazenar objeto
var usuario = new { 
    Id = 1, 
    Nome = "João", 
    Email = "joao@email.com" 
};
await client.SetObjectAsync("usuario:1", usuario);

// Deserializar objeto
var usuarioRecuperado = await client.GetObjectAsync<dynamic>("usuario:1");
Console.WriteLine($"Usuário: {usuarioRecuperado.Nome}");
```

### 2. Operações Numéricas

```csharp
// Incrementar contador
var visitantes = await client.IncrementAsync("visitantes"); // 1
visitantes = await client.IncrementAsync("visitantes", 5);  // 6

// Decrementar
visitantes = await client.DecrementAsync("visitantes", 2);  // 4
```

### 3. Pattern Get-or-Set

```csharp
// Cache inteligente - só calcula se não existe
var dadosPesados = await client.GetOrSetAsync("calculo_complexo", async () =>
{
    // Esta função só executa se a chave não existir
    await Task.Delay(1000); // Simular operação demorada
    return "Resultado do cálculo complexo";
});

// Com objetos
var config = await client.GetOrSetObjectAsync("config:app", async () =>
{
    return new { Timeout = 30, MaxConnections = 100 };
});
```

### 4. Operações em Lote

```csharp
// Verificar múltiplas chaves
var existencias = await client.ExistsMultipleAsync("key1", "key2", "key3");
foreach (var item in existencias)
{
    Console.WriteLine($"{item.Key}: {item.Value}");
}

// Remover múltiplas chaves
var removidas = await client.DeleteMultipleAsync("key1", "key2", "key3");
Console.WriteLine($"Removidas: {removidas} chaves");
```

## 🏗️ Estrutura do Projeto

```
RustdisSDK/
├── Client/
│   └── RustdisClient.cs          # Implementação principal do cliente
├── Interfaces/
│   └── IRustdisClient.cs         # Interface do cliente
├── Models/
│   └── RustdisModels.cs         # Modelos de dados
├── Factory/
│   └── RustdisClientFactory.cs  # Factory para criar instâncias
├── Extensions/
│   └── RustdisClientExtensions.cs # Extensões úteis
└── RustdisSDK.csproj            # Configuração do projeto

RustdisSDK.Tests/
├── RustdisTests.cs              # Testes unitários e integração
└── RustdisSDK.Tests.csproj     # Configuração dos testes

RustdisSDK.Examples/
├── Program.cs                   # Exemplos de uso
└── RustdisSDK.Examples.csproj  # Configuração dos exemplos
```

## 🧪 Executar Testes

```bash
# Certificar que o Rustdis está compilado
cd ../
cargo build --release

# Executar testes
cd RustdisSDK.Tests
dotnet test
```

## 📖 Executar Exemplos

```bash
cd RustdisSDK.Examples
dotnet run
```

## 🔧 Configuração

### Pré-requisitos

1. **.NET 8.0** ou superior
2. **Rustdis compilado** - O executável do Rustdis deve estar disponível
3. **Cargo** (para compilar o Rustdis se necessário)

### Compilar Rustdis

```bash
# No diretório raiz do projeto Rustdis
cargo build --release
```

O executável estará em `target/release/rustdis.exe` (Windows) ou `target/release/rustdis` (Linux/Mac).

## 📚 API Reference

### Interface Principal

```csharp
public interface IRustdisClient
{
    Task<string?> GetAsync(string key);                    // GET chave
    Task<bool> SetAsync(string key, string value);        // SET chave valor
    Task<bool> DeleteAsync(string key);                   // DEL chave
    Task<bool> ExistsAsync(string key);                   // EXISTS chave
    Task<string[]> GetKeysAsync();                        // KEYS
    Task<bool> FlushAsync();                              // FLUSH
    Task<int> GetSizeAsync();                             // SIZE
    Task<bool> PingAsync();                               // PING
    Task<string> ExecuteCommandAsync(string jsonCommand); // Comando JSON customizado
}
```

### Extensões Disponíveis

```csharp
// Objetos
Task<T?> GetObjectAsync<T>(string key)
Task<bool> SetObjectAsync<T>(string key, T obj)

// Get-or-Set
Task<string> GetOrSetAsync(string key, Func<Task<string>> valueFactory)
Task<T> GetOrSetObjectAsync<T>(string key, Func<Task<T>> objectFactory)

// Numérico
Task<long> IncrementAsync(string key, long increment = 1)
Task<long> DecrementAsync(string key, long decrement = 1)

// Lote
Task<Dictionary<string, bool>> ExistsMultipleAsync(params string[] keys)
Task<int> DeleteMultipleAsync(params string[] keys)
```

## 🤝 Contribuição

1. Fork o projeto
2. Crie sua feature branch (`git checkout -b feature/nova-funcionalidade`)
3. Commit suas mudanças (`git commit -am 'Adiciona nova funcionalidade'`)
4. Push para a branch (`git push origin feature/nova-funcionalidade`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está licenciado sob a Licença MIT - veja o arquivo [LICENSE](LICENSE) para detalhes.

## 🔗 Links Relacionados

- [Rustdis - Redis Clone em Rust](https://github.com/LucasDiasJorge/Rustdis)
- [Documentação do .NET](https://docs.microsoft.com/dotnet/)
- [Redis Documentation](https://redis.io/documentation)

---

**Exemplo da interface GET que você pediu:**

```csharp
// Criar cliente
using var client = RustdisClientFactory.CreateClient("rustdis.exe");

// Definir dados
await client.SetAsync("minha_chave", "meu_valor");

// GET - Interface para acessar valor pela chave (exatamente como solicitado!)
var valor = await client.GetAsync("minha_chave");
if (valor != null)
{
    Console.WriteLine($"Valor encontrado: {valor}");
}
else
{
    Console.WriteLine("Chave não encontrada");
}
```
