using System;
using System.Threading.Tasks;
using RustdisSDK.Factory;
using RustdisSDK.Extensions;

namespace RustdisSDK.Examples
{
    /// <summary>
    /// Exemplos de uso do SDK Rustdis
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("🚀 Exemplos do SDK Rustdis");
            Console.WriteLine("============================");

            try
            {
                // Criar cliente (ajustar caminho conforme necessário)
                using var client = RustdisClientFactory.CreateClient(@"..\..\..\..\target\release\rustdis.exe");

                // Exemplo 1: Operações básicas
                await BasicOperationsExample(client);

                // Exemplo 2: Trabalhar com objetos
                await ObjectOperationsExample(client);

                // Exemplo 3: Operações numéricas
                await NumericOperationsExample(client);

                // Exemplo 4: Operações em lote
                await BatchOperationsExample(client);

                // Exemplo 5: Get-or-Set pattern
                await GetOrSetExample(client);

                Console.WriteLine("\n✅ Todos os exemplos executados com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro: {ex.Message}");
            }
        }

        static async Task BasicOperationsExample(IRustdisClient client)
        {
            Console.WriteLine("\n📝 Exemplo 1: Operações Básicas");
            Console.WriteLine("--------------------------------");

            // Test ping
            var pingResult = await client.PingAsync();
            Console.WriteLine($"PING: {(pingResult ? "PONG" : "Failed")}");

            // Set e Get
            await client.SetAsync("nome", "Lucas");
            await client.SetAsync("idade", "25");
            await client.SetAsync("profissao", "Desenvolvedor");

            var nome = await client.GetAsync("nome");
            var idade = await client.GetAsync("idade");
            var profissao = await client.GetAsync("profissao");

            Console.WriteLine($"Nome: {nome}");
            Console.WriteLine($"Idade: {idade}");
            Console.WriteLine($"Profissão: {profissao}");

            // Verificar existência
            var existeNome = await client.ExistsAsync("nome");
            var existeEmail = await client.ExistsAsync("email");
            Console.WriteLine($"Nome existe: {existeNome}");
            Console.WriteLine($"Email existe: {existeEmail}");

            // Listar chaves
            var keys = await client.GetKeysAsync();
            Console.WriteLine($"Chaves ({keys.Length}): [{string.Join(", ", keys)}]");

            // Tamanho do cache
            var size = await client.GetSizeAsync();
            Console.WriteLine($"Tamanho do cache: {size}");
        }

        static async Task ObjectOperationsExample(IRustdisClient client)
        {
            Console.WriteLine("\n🎯 Exemplo 2: Operações com Objetos");
            Console.WriteLine("------------------------------------");

            // Criar objeto de exemplo
            var usuario = new
            {
                Id = 1,
                Nome = "João Silva",
                Email = "joao@exemplo.com",
                Ativo = true,
                DataCriacao = DateTime.Now
            };

            // Salvar objeto
            await client.SetObjectAsync("usuario:1", usuario);
            Console.WriteLine("✅ Usuário salvo como JSON");

            // Recuperar objeto
            var usuarioRecuperado = await client.GetObjectAsync<dynamic>("usuario:1");
            Console.WriteLine($"👤 Usuário recuperado: {usuarioRecuperado}");

            // Configurações do sistema
            var config = new Dictionary<string, object>
            {
                ["timeout"] = 30,
                ["max_connections"] = 100,
                ["debug_mode"] = true
            };

            await client.SetObjectAsync("config:system", config);
            var configRecuperada = await client.GetObjectAsync<Dictionary<string, object>>("config:system");
            
            Console.WriteLine("⚙️ Configurações:");
            foreach (var item in configRecuperada ?? new Dictionary<string, object>())
            {
                Console.WriteLine($"  {item.Key}: {item.Value}");
            }
        }

        static async Task NumericOperationsExample(IRustdisClient client)
        {
            Console.WriteLine("\n🔢 Exemplo 3: Operações Numéricas");
            Console.WriteLine("----------------------------------");

            // Contador de visitantes
            var visitors = await client.IncrementAsync("visitors");
            Console.WriteLine($"Visitantes: {visitors}");

            // Incrementar por 10
            visitors = await client.IncrementAsync("visitors", 10);
            Console.WriteLine($"Visitantes após +10: {visitors}");

            // Decrementar por 3
            visitors = await client.DecrementAsync("visitors", 3);
            Console.WriteLine($"Visitantes após -3: {visitors}");

            // Contador de erros
            for (int i = 0; i < 5; i++)
            {
                await client.IncrementAsync("errors");
            }
            
            var errors = long.Parse(await client.GetAsync("errors") ?? "0");
            Console.WriteLine($"Total de erros: {errors}");
        }

        static async Task BatchOperationsExample(IRustdisClient client)
        {
            Console.WriteLine("\n📦 Exemplo 4: Operações em Lote");
            Console.WriteLine("--------------------------------");

            // Criar várias chaves
            var keys = new[] { "batch:1", "batch:2", "batch:3", "batch:4" };
            
            for (int i = 0; i < keys.Length; i++)
            {
                await client.SetAsync(keys[i], $"valor_{i + 1}");
            }

            // Verificar existência de múltiplas chaves
            var existsResults = await client.ExistsMultipleAsync(keys);
            Console.WriteLine("Verificação de existência:");
            foreach (var result in existsResults)
            {
                Console.WriteLine($"  {result.Key}: {(result.Value ? "✅" : "❌")}");
            }

            // Remover múltiplas chaves
            var deletedCount = await client.DeleteMultipleAsync(keys);
            Console.WriteLine($"🗑️ Removidas {deletedCount} chaves");
        }

        static async Task GetOrSetExample(IRustdisClient client)
        {
            Console.WriteLine("\n🔄 Exemplo 5: Get-or-Set Pattern");
            Console.WriteLine("---------------------------------");

            // Simular cache de dados pesados
            var expensiveData = await client.GetOrSetAsync("expensive_calculation", async () =>
            {
                Console.WriteLine("🔄 Calculando dados pesados...");
                await Task.Delay(1000); // Simular operação demorada
                return $"Resultado calculado em {DateTime.Now:HH:mm:ss}";
            });

            Console.WriteLine($"📊 Dados: {expensiveData}");

            // Segunda chamada deve ser do cache (instantânea)
            var cachedData = await client.GetOrSetAsync("expensive_calculation", async () =>
            {
                Console.WriteLine("🔄 Esta mensagem NÃO deve aparecer!");
                return "Novo valor";
            });

            Console.WriteLine($"💨 Dados do cache: {cachedData}");

            // Get-or-Set com objetos
            var userProfile = await client.GetOrSetObjectAsync("profile:admin", async () =>
            {
                Console.WriteLine("🔄 Criando perfil de administrador...");
                return new
                {
                    Username = "admin",
                    Role = "Administrator",
                    LastLogin = DateTime.Now,
                    Permissions = new[] { "read", "write", "delete", "admin" }
                };
            });

            Console.WriteLine($"👑 Perfil Admin: {userProfile}");
        }
    }
}
