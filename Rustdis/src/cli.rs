use crate::cache::RustdisCache;
use crate::protocol::{RustdisProtocol, Command, Response};
use anyhow::Result;
use std::io::{self, Write, BufRead, BufReader};
use std::sync::Arc;

/// Simple CLI interface for Rustdis
pub struct RustdisCli {
    protocol: RustdisProtocol,
}

impl RustdisCli {
    pub fn new(cache: RustdisCache) -> Self {
        Self {
            protocol: RustdisProtocol::new(cache),
        }
    }

    /// Start the interactive CLI
    pub fn run(&self) -> Result<()> {
        println!("🚀 Welcome to Rustdis - Redis clone in Rust!");
        println!("Type 'help' for available commands or 'quit' to exit.");
        println!();

        let stdin = io::stdin();
        let mut reader = BufReader::new(stdin.lock());
        let mut line = String::new();

        loop {
            print!("rustdis> ");
            io::stdout().flush()?;

            line.clear();
            match reader.read_line(&mut line) {
                Ok(0) => break, // EOF
                Ok(_) => {
                    let input = line.trim();
                    
                    if input.is_empty() {
                        continue;
                    }

                    if input == "quit" || input == "exit" {
                        println!("Goodbye! 👋");
                        break;
                    }

                    if input == "help" {
                        self.show_help();
                        continue;
                    }

                    // Try to parse as JSON first, then as simple commands
                    let response = if input.starts_with('{') {
                        // JSON command
                        match RustdisProtocol::parse_command(input) {
                            Ok(cmd) => self.protocol.execute(cmd),
                            Err(e) => Response::Error { error: format!("Invalid JSON: {}", e) },
                        }
                    } else {
                        // Simple command parsing
                        self.parse_simple_command(input)
                    };

                    self.print_response(&response);
                }
                Err(e) => {
                    eprintln!("Error reading input: {}", e);
                    break;
                }
            }
        }

        Ok(())
    }

    /// Parse simple text commands (non-JSON)
    fn parse_simple_command(&self, input: &str) -> Response {
        let parts: Vec<&str> = input.split_whitespace().collect();
        
        if parts.is_empty() {
            return Response::Error { error: "Empty command".to_string() };
        }

        let command = match parts[0].to_uppercase().as_str() {
            "GET" => {
                if parts.len() != 2 {
                    return Response::Error { error: "GET requires exactly one argument: GET <key>".to_string() };
                }
                Command::Get { key: parts[1].to_string() }
            }
            "SET" => {
                if parts.len() != 3 {
                    return Response::Error { error: "SET requires exactly two arguments: SET <key> <value>".to_string() };
                }
                Command::Set { 
                    key: parts[1].to_string(), 
                    value: parts[2].to_string() 
                }
            }
            "DEL" | "DELETE" => {
                if parts.len() != 2 {
                    return Response::Error { error: "DEL requires exactly one argument: DEL <key>".to_string() };
                }
                Command::Del { key: parts[1].to_string() }
            }
            "EXISTS" => {
                if parts.len() != 2 {
                    return Response::Error { error: "EXISTS requires exactly one argument: EXISTS <key>".to_string() };
                }
                Command::Exists { key: parts[1].to_string() }
            }
            "KEYS" => Command::Keys,
            "FLUSH" | "FLUSHALL" => Command::Flush,
            "SIZE" | "DBSIZE" => Command::Size,
            "PING" => Command::Ping,
            _ => {
                return Response::Error { error: format!("Unknown command: {}", parts[0]) };
            }
        };

        self.protocol.execute(command)
    }

    /// Print response in a user-friendly format
    fn print_response(&self, response: &Response) {
        match response {
            Response::String(s) => println!("{}", s),
            Response::StringOption(Some(s)) => println!("\"{}\"", s),
            Response::StringOption(None) => println!("(nil)"),
            Response::Boolean(b) => println!("{}", if *b { 1 } else { 0 }),
            Response::Number(n) => println!("{}", n),
            Response::StringArray(arr) => {
                for (i, key) in arr.iter().enumerate() {
                    println!("{}) \"{}\"", i + 1, key);
                }
                if arr.is_empty() {
                    println!("(empty array)");
                }
            }
            Response::Ok => println!("OK"),
            Response::Error { error } => println!("Error: {}", error),
        }
    }

    /// Show help information
    fn show_help(&self) {
        println!("Available commands:");
        println!("  GET <key>           - Get value by key");
        println!("  SET <key> <value>   - Set key-value pair");
        println!("  DEL <key>           - Delete key");
        println!("  EXISTS <key>        - Check if key exists");
        println!("  KEYS                - List all keys");
        println!("  FLUSH               - Clear all data");
        println!("  SIZE                - Get number of keys");
        println!("  PING                - Test connection");
        println!("  help                - Show this help");
        println!("  quit/exit           - Exit the program");
        println!();
        println!("You can also use JSON format:");
        println!("  {{\"command\": \"GET\", \"args\": {{\"key\": \"mykey\"}}}}");
        println!();
    }
}
