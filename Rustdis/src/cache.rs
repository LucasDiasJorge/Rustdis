use std::collections::HashMap;
use std::sync::{Arc, RwLock};
use anyhow::Result;

/// Core cache structure using HashMap
#[derive(Debug, Clone)]
pub struct RustdisCache {
    data: Arc<RwLock<HashMap<String, String>>>,
}

impl RustdisCache {
    /// Creates a new empty cache
    pub fn new() -> Self {
        Self {
            data: Arc::new(RwLock::new(HashMap::new())),
        }
    }

    /// GET operation - retrieves value by key
    pub fn get(&self, key: &str) -> Result<Option<String>> {
        let data = self.data.read().map_err(|_| anyhow::anyhow!("Failed to acquire read lock"))?;
        Ok(data.get(key).cloned())
    }

    /// SET operation - stores key-value pair
    pub fn set(&self, key: String, value: String) -> Result<()> {
        let mut data = self.data.write().map_err(|_| anyhow::anyhow!("Failed to acquire write lock"))?;
        data.insert(key, value);
        Ok(())
    }

    /// DEL operation - deletes a key
    pub fn del(&self, key: &str) -> Result<bool> {
        let mut data = self.data.write().map_err(|_| anyhow::anyhow!("Failed to acquire write lock"))?;
        Ok(data.remove(key).is_some())
    }

    /// EXISTS operation - checks if key exists
    pub fn exists(&self, key: &str) -> Result<bool> {
        let data = self.data.read().map_err(|_| anyhow::anyhow!("Failed to acquire read lock"))?;
        Ok(data.contains_key(key))
    }

    /// KEYS operation - returns all keys (be careful with large datasets)
    pub fn keys(&self) -> Result<Vec<String>> {
        let data = self.data.read().map_err(|_| anyhow::anyhow!("Failed to acquire read lock"))?;
        Ok(data.keys().cloned().collect())
    }

    /// FLUSH operation - clears all data
    pub fn flush(&self) -> Result<()> {
        let mut data = self.data.write().map_err(|_| anyhow::anyhow!("Failed to acquire write lock"))?;
        data.clear();
        Ok(())
    }

    /// SIZE operation - returns number of keys
    pub fn size(&self) -> Result<usize> {
        let data = self.data.read().map_err(|_| anyhow::anyhow!("Failed to acquire read lock"))?;
        Ok(data.len())
    }
}

impl Default for RustdisCache {
    fn default() -> Self {
        Self::new()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_basic_operations() {
        let cache = RustdisCache::new();
        
        // Test SET and GET
        cache.set("key1".to_string(), "value1".to_string()).unwrap();
        assert_eq!(cache.get("key1").unwrap(), Some("value1".to_string()));
        
        // Test non-existent key
        assert_eq!(cache.get("nonexistent").unwrap(), None);
        
        // Test EXISTS
        assert!(cache.exists("key1").unwrap());
        assert!(!cache.exists("nonexistent").unwrap());
        
        // Test DEL
        assert!(cache.del("key1").unwrap());
        assert!(!cache.del("key1").unwrap()); // Second delete should return false
        assert_eq!(cache.get("key1").unwrap(), None);
    }

    #[test]
    fn test_multiple_keys() {
        let cache = RustdisCache::new();
        
        cache.set("key1".to_string(), "value1".to_string()).unwrap();
        cache.set("key2".to_string(), "value2".to_string()).unwrap();
        cache.set("key3".to_string(), "value3".to_string()).unwrap();
        
        assert_eq!(cache.size().unwrap(), 3);
        
        let keys = cache.keys().unwrap();
        assert!(keys.contains(&"key1".to_string()));
        assert!(keys.contains(&"key2".to_string()));
        assert!(keys.contains(&"key3".to_string()));
        
        cache.flush().unwrap();
        assert_eq!(cache.size().unwrap(), 0);
    }
}
