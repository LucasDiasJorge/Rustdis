use crate::cache::RustdisCache;
use anyhow::Result;
use serde::{Deserialize, Serialize};

/// Command types supported by Rustdis
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(tag = "command", content = "args", rename_all = "UPPERCASE")]
pub enum Command {
    Get { key: String },
    Set { key: String, value: String },
    Del { key: String },
    Exists { key: String },
    Keys,
    Flush,
    Size,
    Ping,
}

/// Response types from Rustdis operations
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(untagged)]
pub enum Response {
    String(String),
    StringOption(Option<String>),
    Boolean(bool),
    Number(usize),
    StringArray(Vec<String>),
    Ok,
    Error { error: String },
}

/// Protocol handler for processing commands
#[derive(Debug, Clone)]
pub struct RustdisProtocol {
    cache: RustdisCache,
}

impl RustdisProtocol {
    pub fn new(cache: RustdisCache) -> Self {
        Self { cache }
    }

    /// Process a command and return a response
    pub fn execute(&self, command: Command) -> Response {
        match command {
            Command::Get { key } => {
                match self.cache.get(&key) {
                    Ok(value) => Response::StringOption(value),
                    Err(e) => Response::Error { error: e.to_string() },
                }
            }
            Command::Set { key, value } => {
                match self.cache.set(key, value) {
                    Ok(()) => Response::Ok,
                    Err(e) => Response::Error { error: e.to_string() },
                }
            }
            Command::Del { key } => {
                match self.cache.del(&key) {
                    Ok(deleted) => Response::Boolean(deleted),
                    Err(e) => Response::Error { error: e.to_string() },
                }
            }
            Command::Exists { key } => {
                match self.cache.exists(&key) {
                    Ok(exists) => Response::Boolean(exists),
                    Err(e) => Response::Error { error: e.to_string() },
                }
            }
            Command::Keys => {
                match self.cache.keys() {
                    Ok(keys) => Response::StringArray(keys),
                    Err(e) => Response::Error { error: e.to_string() },
                }
            }
            Command::Flush => {
                match self.cache.flush() {
                    Ok(()) => Response::Ok,
                    Err(e) => Response::Error { error: e.to_string() },
                }
            }
            Command::Size => {
                match self.cache.size() {
                    Ok(size) => Response::Number(size),
                    Err(e) => Response::Error { error: e.to_string() },
                }
            }
            Command::Ping => Response::String("PONG".to_string()),
        }
    }

    /// Parse a JSON string into a command
    pub fn parse_command(input: &str) -> Result<Command> {
        let command: Command = serde_json::from_str(input)?;
        Ok(command)
    }

    /// Convert a response to JSON string
    pub fn response_to_json(response: &Response) -> Result<String> {
        let json = serde_json::to_string(response)?;
        Ok(json)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_protocol_operations() {
        let cache = RustdisCache::new();
        let protocol = RustdisProtocol::new(cache);

        // Test SET command
        let set_cmd = Command::Set {
            key: "test_key".to_string(),
            value: "test_value".to_string(),
        };
        let response = protocol.execute(set_cmd);
        assert!(matches!(response, Response::Ok));

        // Test GET command
        let get_cmd = Command::Get {
            key: "test_key".to_string(),
        };
        let response = protocol.execute(get_cmd);
        assert!(matches!(response, Response::StringOption(Some(_))));

        // Test PING command
        let ping_cmd = Command::Ping;
        let response = protocol.execute(ping_cmd);
        assert!(matches!(response, Response::String(ref s) if s == "PONG"));
    }

    #[test]
    fn test_json_parsing() {
        let json_cmd = r#"{"command": "GET", "args": {"key": "test"}}"#;
        let command = RustdisProtocol::parse_command(json_cmd).unwrap();
        assert!(matches!(command, Command::Get { .. }));

        let response = Response::String("PONG".to_string());
        let json = RustdisProtocol::response_to_json(&response).unwrap();
        assert_eq!(json, r#""PONG""#);
    }
}
