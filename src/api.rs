use crate::cache::RustdisCache;
use crate::protocol::{RustdisProtocol, Response};
use anyhow::Result;
use serde_json;
use std::collections::HashMap;
use std::sync::Arc;

/// HTTP-like API interface for Rustdis
pub struct RustdisApi {
    protocol: RustdisProtocol,
}

impl RustdisApi {
    pub fn new(cache: RustdisCache) -> Self {
        Self {
            protocol: RustdisProtocol::new(cache),
        }
    }

    /// GET /api/get?key=<key>
    /// Get value by key
    pub fn api_get(&self, key: &str) -> Result<String> {
        let command = crate::protocol::Command::Get { key: key.to_string() };
        let response = self.protocol.execute(command);
        Ok(RustdisProtocol::response_to_json(&response)?)
    }

    /// POST /api/set
    /// Body: {"key": "mykey", "value": "myvalue"}
    pub fn api_set(&self, key: String, value: String) -> Result<String> {
        let command = crate::protocol::Command::Set { key, value };
        let response = self.protocol.execute(command);
        Ok(RustdisProtocol::response_to_json(&response)?)
    }

    /// DELETE /api/del?key=<key>
    /// Delete key
    pub fn api_del(&self, key: &str) -> Result<String> {
        let command = crate::protocol::Command::Del { key: key.to_string() };
        let response = self.protocol.execute(command);
        Ok(RustdisProtocol::response_to_json(&response)?)
    }

    /// GET /api/exists?key=<key>
    /// Check if key exists
    pub fn api_exists(&self, key: &str) -> Result<String> {
        let command = crate::protocol::Command::Exists { key: key.to_string() };
        let response = self.protocol.execute(command);
        Ok(RustdisProtocol::response_to_json(&response)?)
    }

    /// GET /api/keys
    /// Get all keys
    pub fn api_keys(&self) -> Result<String> {
        let command = crate::protocol::Command::Keys;
        let response = self.protocol.execute(command);
        Ok(RustdisProtocol::response_to_json(&response)?)
    }

    /// DELETE /api/flush
    /// Clear all data
    pub fn api_flush(&self) -> Result<String> {
        let command = crate::protocol::Command::Flush;
        let response = self.protocol.execute(command);
        Ok(RustdisProtocol::response_to_json(&response)?)
    }

    /// GET /api/size
    /// Get number of keys
    pub fn api_size(&self) -> Result<String> {
        let command = crate::protocol::Command::Size;
        let response = self.protocol.execute(command);
        Ok(RustdisProtocol::response_to_json(&response)?)
    }

    /// GET /api/ping
    /// Test connection
    pub fn api_ping(&self) -> Result<String> {
        let command = crate::protocol::Command::Ping;
        let response = self.protocol.execute(command);
        Ok(RustdisProtocol::response_to_json(&response)?)
    }

    /// POST /api/command
    /// Execute raw JSON command
    pub fn api_execute_command(&self, json_command: &str) -> Result<String> {
        match RustdisProtocol::parse_command(json_command) {
            Ok(command) => {
                let response = self.protocol.execute(command);
                Ok(RustdisProtocol::response_to_json(&response)?)
            }
            Err(e) => {
                let error_response = Response::Error { error: e.to_string() };
                Ok(RustdisProtocol::response_to_json(&error_response)?)
            }
        }
    }

    /// Generate API documentation
    pub fn api_docs(&self) -> String {
        r#"
# Rustdis API Documentation

## Endpoints

### GET /api/get?key=<key>
Get value by key
- **Query Parameter**: `key` - The key to retrieve
- **Response**: JSON with the value or null if not found

### POST /api/set
Set key-value pair
- **Body**: `{"key": "mykey", "value": "myvalue"}`
- **Response**: `"OK"` on success

### DELETE /api/del?key=<key>
Delete key
- **Query Parameter**: `key` - The key to delete
- **Response**: `true` if deleted, `false` if key didn't exist

### GET /api/exists?key=<key>
Check if key exists
- **Query Parameter**: `key` - The key to check
- **Response**: `true` if exists, `false` otherwise

### GET /api/keys
Get all keys
- **Response**: Array of all keys

### DELETE /api/flush
Clear all data
- **Response**: `"OK"` on success

### GET /api/size
Get number of keys
- **Response**: Number of keys in the cache

### GET /api/ping
Test connection
- **Response**: `"PONG"`

### POST /api/command
Execute raw JSON command
- **Body**: JSON command object
- **Response**: JSON response from command execution

## Example Usage

```bash
# Get a value
curl "http://localhost:8080/api/get?key=mykey"

# Set a value
curl -X POST "http://localhost:8080/api/set" \
     -H "Content-Type: application/json" \
     -d '{"key": "mykey", "value": "myvalue"}'

# Check if key exists
curl "http://localhost:8080/api/exists?key=mykey"

# Get all keys
curl "http://localhost:8080/api/keys"

# Execute raw command
curl -X POST "http://localhost:8080/api/command" \
     -H "Content-Type: application/json" \
     -d '{"command": "GET", "args": {"key": "mykey"}}'
```
        "#.trim().to_string()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_api_operations() {
        let cache = RustdisCache::new();
        let api = RustdisApi::new(cache);

        // Test SET
        let result = api.api_set("test_key".to_string(), "test_value".to_string()).unwrap();
        assert!(result.contains("OK") || result == "\"OK\"");

        // Test GET
        let result = api.api_get("test_key").unwrap();
        assert!(result.contains("test_value"));

        // Test EXISTS
        let result = api.api_exists("test_key").unwrap();
        assert!(result.contains("true"));

        // Test PING
        let result = api.api_ping().unwrap();
        assert!(result.contains("PONG"));
    }

    #[test]
    fn test_api_command_execution() {
        let cache = RustdisCache::new();
        let api = RustdisApi::new(cache);

        let json_cmd = r#"{"command": "SET", "args": {"key": "api_test", "value": "api_value"}}"#;
        let result = api.api_execute_command(json_cmd).unwrap();
        assert!(result.contains("OK") || result == "\"OK\"");

        let json_cmd = r#"{"command": "GET", "args": {"key": "api_test"}}"#;
        let result = api.api_execute_command(json_cmd).unwrap();
        assert!(result.contains("api_value"));
    }
}
