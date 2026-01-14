mod cache;
mod protocol;
mod cli;
mod api;

use cache::RustdisCache;
use cli::RustdisCli;
use api::RustdisApi;
use clap::{Parser, Subcommand};
use anyhow::Result;

#[derive(Parser)]
#[command(name = "rustdis")]
#[command(about = "A Redis clone written in Rust")]
#[command(version = "0.1.0")]
struct Cli {
    #[command(subcommand)]
    command: Option<Commands>,
}

#[derive(Subcommand)]
enum Commands {
    /// Start interactive CLI mode
    Cli,
    /// Run a single command and exit
    Get { key: String },
    /// Set a key-value pair and exit
    Set { key: String, value: String },
    /// Delete a key and exit
    Del { key: String },
    /// Check if key exists and exit
    Exists { key: String },
    /// List all keys and exit
    Keys,
    /// Clear all data and exit
    Flush,
    /// Get cache size and exit
    Size,
    /// Test connection and exit
    Ping,
    /// Show API documentation
    ApiDocs,
}

fn main() -> Result<()> {
    let cli = Cli::parse();
    let cache = RustdisCache::new();

    match cli.command {
        Some(Commands::Cli) | None => {
            // Start interactive CLI
            let cli_interface = RustdisCli::new(cache);
            cli_interface.run()?;
        }
        Some(Commands::Get { key }) => {
            let api = RustdisApi::new(cache);
            let result = api.api_get(&key)?;
            println!("{}", result);
        }
        Some(Commands::Set { key, value }) => {
            let api = RustdisApi::new(cache);
            let result = api.api_set(key, value)?;
            println!("{}", result);
        }
        Some(Commands::Del { key }) => {
            let api = RustdisApi::new(cache);
            let result = api.api_del(&key)?;
            println!("{}", result);
        }
        Some(Commands::Exists { key }) => {
            let api = RustdisApi::new(cache);
            let result = api.api_exists(&key)?;
            println!("{}", result);
        }
        Some(Commands::Keys) => {
            let api = RustdisApi::new(cache);
            let result = api.api_keys()?;
            println!("{}", result);
        }
        Some(Commands::Flush) => {
            let api = RustdisApi::new(cache);
            let result = api.api_flush()?;
            println!("{}", result);
        }
        Some(Commands::Size) => {
            let api = RustdisApi::new(cache);
            let result = api.api_size()?;
            println!("{}", result);
        }
        Some(Commands::Ping) => {
            let api = RustdisApi::new(cache);
            let result = api.api_ping()?;
            println!("{}", result);
        }
        Some(Commands::ApiDocs) => {
            let api = RustdisApi::new(cache);
            println!("{}", api.api_docs());
        }
    }

    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_cache_creation() {
        let cache = RustdisCache::new();
        assert_eq!(cache.size().unwrap(), 0);
    }

    #[test]
    fn test_integration() {
        let cache = RustdisCache::new();
        
        // Test through API
        let api = RustdisApi::new(cache.clone());
        api.api_set("integration_test".to_string(), "test_value".to_string()).unwrap();
        
        let result = api.api_get("integration_test").unwrap();
        assert!(result.contains("test_value"));
        
        // Test direct cache access
        let value = cache.get("integration_test").unwrap();
        assert_eq!(value, Some("test_value".to_string()));
    }
}
