# Exemplos de uso do Rustdis

## Exemplo básico - operações simples
```rust
use rustdis::cache::RustdisCache;

fn main() {
    let cache = RustdisCache::new();
    
    // SET
    cache.set("nome".to_string(), "Lucas".to_string()).unwrap();
    cache.set("idade".to_string(), "25".to_string()).unwrap();
    
    // GET
    let nome = cache.get("nome").unwrap();
    println!("Nome: {:?}", nome); // Some("Lucas")
    
    let idade = cache.get("idade").unwrap();
    println!("Idade: {:?}", idade); // Some("25")
    
    // EXISTS
    let existe = cache.exists("nome").unwrap();
    println!("Nome existe: {}", existe); // true
    
    // DEL
    let deletado = cache.del("idade").unwrap();
    println!("Idade deletada: {}", deletado); // true
    
    // KEYS
    let chaves = cache.keys().unwrap();
    println!("Chaves: {:?}", chaves); // ["nome"]
    
    // SIZE
    let tamanho = cache.size().unwrap();
    println!("Tamanho: {}", tamanho); // 1
}
```

## Exemplo com API
```rust
use rustdis::{cache::RustdisCache, api::RustdisApi};

fn main() {
    let cache = RustdisCache::new();
    let api = RustdisApi::new(cache);
    
    // Usando a interface API
    api.api_set("usuario:1".to_string(), "{'nome': 'João', 'email': 'joao@email.com'}".to_string()).unwrap();
    
    let usuario = api.api_get("usuario:1").unwrap();
    println!("Usuário: {}", usuario);
    
    // Verificar se existe
    let existe = api.api_exists("usuario:1").unwrap();
    println!("Usuário existe: {}", existe);
}
```

## Exemplo com Protocol (JSON)
```rust
use rustdis::{cache::RustdisCache, protocol::RustdisProtocol};

fn main() {
    let cache = RustdisCache::new();
    let protocol = RustdisProtocol::new(cache);
    
    // Comando JSON para SET
    let json_cmd = r#"{"command": "SET", "args": {"key": "config:timeout", "value": "30"}}"#;
    let comando = RustdisProtocol::parse_command(json_cmd).unwrap();
    let resposta = protocol.execute(comando);
    
    println!("Resposta SET: {:?}", resposta);
    
    // Comando JSON para GET
    let json_cmd = r#"{"command": "GET", "args": {"key": "config:timeout"}}"#;
    let comando = RustdisProtocol::parse_command(json_cmd).unwrap();
    let resposta = protocol.execute(comando);
    
    println!("Resposta GET: {:?}", resposta);
}
```

## Exemplo de uso multi-thread
```rust
use rustdis::cache::RustdisCache;
use std::thread;
use std::sync::Arc;

fn main() {
    let cache = Arc::new(RustdisCache::new());
    
    let mut handles = vec![];
    
    // Criar múltiplas threads para escrever
    for i in 0..10 {
        let cache_clone = Arc::clone(&cache);
        let handle = thread::spawn(move || {
            cache_clone.set(
                format!("thread:{}", i),
                format!("valor_{}", i)
            ).unwrap();
        });
        handles.push(handle);
    }
    
    // Aguardar todas as threads terminarem
    for handle in handles {
        handle.join().unwrap();
    }
    
    // Ler todos os valores
    let chaves = cache.keys().unwrap();
    println!("Total de chaves: {}", chaves.len());
    
    for chave in chaves {
        let valor = cache.get(&chave).unwrap();
        println!("{}: {:?}", chave, valor);
    }
}
```

## Exemplo de cache com TTL simulado
```rust
use rustdis::cache::RustdisCache;
use std::time::{SystemTime, UNIX_EPOCH};

struct CacheComTTL {
    cache: RustdisCache,
}

impl CacheComTTL {
    fn new() -> Self {
        Self {
            cache: RustdisCache::new(),
        }
    }
    
    fn set_with_ttl(&self, key: &str, value: &str, ttl_seconds: u64) -> anyhow::Result<()> {
        let expiry = SystemTime::now()
            .duration_since(UNIX_EPOCH)?
            .as_secs() + ttl_seconds;
        
        let value_with_ttl = format!("{}:{}", expiry, value);
        self.cache.set(key.to_string(), value_with_ttl)
    }
    
    fn get_with_ttl(&self, key: &str) -> anyhow::Result<Option<String>> {
        if let Some(stored_value) = self.cache.get(key)? {
            let parts: Vec<&str> = stored_value.splitn(2, ':').collect();
            if parts.len() == 2 {
                let expiry: u64 = parts[0].parse()?;
                let current_time = SystemTime::now()
                    .duration_since(UNIX_EPOCH)?
                    .as_secs();
                
                if current_time <= expiry {
                    return Ok(Some(parts[1].to_string()));
                } else {
                    // Expirado, remover
                    self.cache.del(key)?;
                    return Ok(None);
                }
            }
        }
        Ok(None)
    }
}

fn main() -> anyhow::Result<()> {
    let cache_ttl = CacheComTTL::new();
    
    // Set com TTL de 5 segundos
    cache_ttl.set_with_ttl("temp_data", "dados_temporarios", 5)?;
    
    // Imediato - deve retornar o valor
    let valor = cache_ttl.get_with_ttl("temp_data")?;
    println!("Valor imediato: {:?}", valor);
    
    // Aguardar 6 segundos e tentar novamente
    std::thread::sleep(std::time::Duration::from_secs(6));
    let valor_expirado = cache_ttl.get_with_ttl("temp_data")?;
    println!("Valor após expirar: {:?}", valor_expirado); // None
    
    Ok(())
}
```
