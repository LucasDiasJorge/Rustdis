# Rustdis 🚀
Redis clone implementado em Rust

Um clone completo do Redis escrito em Rust, com cache baseado em HashMap e interface API para operações básicas.

## Características

- ✅ Cache em memória usando HashMap thread-safe
- ✅ Interface CLI interativa
- ✅ API programática para integração
- ✅ Suporte a comandos JSON
- ✅ Operações básicas: GET, SET, DEL, EXISTS, KEYS, FLUSH, SIZE
- ✅ Suporte a multi-threading
- ✅ Testes unitários completos

## Instalação

```bash
# Clone o repositório
git clone https://github.com/LucasDiasJorge/Rustdis.git
cd Rustdis

# Compile o projeto
cargo build --release

# Execute os testes
cargo test
```

## Uso

### CLI Interativo

```bash
# Inicia o modo interativo
cargo run

# Ou use comandos diretos
cargo run -- get mykey
cargo run -- set mykey myvalue
cargo run -- del mykey
```

### Comandos CLI

```
rustdis> SET nome Lucas
OK
rustdis> GET nome
"Lucas"
rustdis> EXISTS nome
1
rustdis> KEYS
1) "nome"
rustdis> DEL nome
1
rustdis> SIZE
0
```

### API Programática

```rust
use rustdis::cache::RustdisCache;

let cache = RustdisCache::new();

// SET
cache.set("chave".to_string(), "valor".to_string()).unwrap();

// GET - aqui está o que você pediu!
let valor = cache.get("chave").unwrap();
println!("Valor: {:?}", valor); // Some("valor")

// EXISTS
let existe = cache.exists("chave").unwrap();
println!("Existe: {}", existe); // true
```

### Interface JSON

```bash
# Comando JSON
rustdis> {"command": "SET", "args": {"key": "test", "value": "json_value"}}
OK

rustdis> {"command": "GET", "args": {"key": "test"}}
"json_value"
```

## Comandos Disponíveis

| Comando | Descrição | Exemplo |
|---------|-----------|---------|
| `GET <key>` | Obtém valor pela chave | `GET usuario:1` |
| `SET <key> <value>` | Define par chave-valor | `SET usuario:1 "João"` |
| `DEL <key>` | Remove chave | `DEL usuario:1` |
| `EXISTS <key>` | Verifica se chave existe | `EXISTS usuario:1` |
| `KEYS` | Lista todas as chaves | `KEYS` |
| `FLUSH` | Limpa todos os dados | `FLUSH` |
| `SIZE` | Retorna número de chaves | `SIZE` |
| `PING` | Testa conexão | `PING` |

## Estrutura do Projeto

```
src/
├── main.rs          # Ponto de entrada e CLI
├── cache.rs         # Core do cache (HashMap)
├── protocol.rs      # Protocolo de comandos e respostas
├── cli.rs           # Interface de linha de comando
└── api.rs           # Interface API programática

examples/
└── basic_usage.rs   # Exemplos de uso
```

## Exemplos

Veja o arquivo `examples/basic_usage.rs` para exemplos completos de uso, incluindo:
- Operações básicas
- Uso multi-thread
- Interface API
- Comandos JSON
- Cache com TTL simulado

## Desenvolvimento

```bash
# Executar testes
cargo test

# Executar com logs detalhados
RUST_LOG=debug cargo run

# Verificar código
cargo clippy

# Formatar código
cargo fmt
```

## Arquitetura

O Rustdis é construído em camadas:

1. **Cache Core** (`cache.rs`) - HashMap thread-safe com RwLock
2. **Protocol** (`protocol.rs`) - Definição de comandos e respostas
3. **CLI** (`cli.rs`) - Interface interativa de linha de comando  
4. **API** (`api.rs`) - Interface programática para integração
5. **Main** (`main.rs`) - Orquestração e pontos de entrada

## Performance

O cache utiliza `Arc<RwLock<HashMap>>` para:
- ✅ Acesso concorrente seguro
- ✅ Múltiplos leitores simultâneos
- ✅ Escritas exclusivas
- ✅ Zero-copy para leituras quando possível

## Licença

MIT License - veja LICENSE para detalhes.

---

**Exemplo de uso da interface GET que você pediu:**

```rust
let cache = RustdisCache::new();
cache.set("minha_chave".to_string(), "meu_valor".to_string()).unwrap();

// Esta é a interface que você pediu - GET passando a chave
let resultado = cache.get("minha_chave").unwrap();
match resultado {
    Some(valor) => println!("Valor encontrado: {}", valor),
    None => println!("Chave não encontrada"),
}
```
