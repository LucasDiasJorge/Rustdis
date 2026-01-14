using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using RustdisSDK.Factory;
using RustdisSDK.Extensions;
using RustdisSDK.Interfaces;

namespace RustdisSDK.Tests
{
    /// <summary>
    /// Testes de integração para o SDK Rustdis
    /// Requer que o executável Rustdis esteja disponível
    /// </summary>
    public class RustdisIntegrationTests : IDisposable
    {
        private readonly IRustdisClient? _client;
        private const string TestRustdisPath = @"..\..\..\..\target\release\rustdis.exe"; // Ajustar conforme necessário

        public RustdisIntegrationTests()
        {
            // Criar cliente para testes (ajustar caminho conforme necessário)
            if (File.Exists(TestRustdisPath))
            {
                _client = RustdisClientFactory.CreateClient(TestRustdisPath);
            }
        }

        [Fact]
        public async Task PingAsync_ShouldReturnTrue()
        {
            // Skip if client is not initialized
            if (_client == null)
            {
                return; // Skip test if Rustdis executable is not available
            }

            // Act
            var result = await _client.PingAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task SetAndGet_ShouldWorkCorrectly()
        {
            // Skip if client is not initialized
            if (_client == null) return;

            // Arrange
            var key = $"test_key_{Guid.NewGuid()}";
            var value = "test_value";

            try
            {
                // Act
                var setResult = await _client.SetAsync(key, value);
                var getValue = await _client.GetAsync(key);

                // Assert
                Assert.True(setResult);
                Assert.Equal(value, getValue);
            }
            finally
            {
                // Cleanup
                await _client.DeleteAsync(key);
            }
        }

        [Fact]
        public async Task Delete_ShouldRemoveKey()
        {
            // Skip if client is not initialized
            if (_client == null) return;

            // Arrange
            var key = $"delete_test_{Guid.NewGuid()}";
            var value = "to_be_deleted";

            // Act
            await _client.SetAsync(key, value);
            var existsBefore = await _client.ExistsAsync(key);
            var deleteResult = await _client.DeleteAsync(key);
            var existsAfter = await _client.ExistsAsync(key);

            // Assert
            Assert.True(existsBefore);
            Assert.True(deleteResult);
            Assert.False(existsAfter);
        }

        [Fact]
        public async Task GetKeys_ShouldReturnAllKeys()
        {
            // Skip if client is not initialized
            if (_client == null) return;

            // Arrange
            var key1 = $"keys_test_1_{Guid.NewGuid()}";
            var key2 = $"keys_test_2_{Guid.NewGuid()}";

            try
            {
                // Act
                await _client.SetAsync(key1, "value1");
                await _client.SetAsync(key2, "value2");
                var keys = await _client.GetKeysAsync();

                // Assert
                Assert.Contains(key1, keys);
                Assert.Contains(key2, keys);
            }
            finally
            {
                // Cleanup
                await _client.DeleteAsync(key1);
                await _client.DeleteAsync(key2);
            }
        }

        [Fact]
        public async Task GetSize_ShouldReturnCorrectCount()
        {
            // Skip if client is not initialized
            if (_client == null) return;

            // Arrange
            await _client.FlushAsync(); // Limpar cache
            var key1 = $"size_test_1_{Guid.NewGuid()}";
            var key2 = $"size_test_2_{Guid.NewGuid()}";

            try
            {
                // Act
                var initialSize = await _client.GetSizeAsync();
                await _client.SetAsync(key1, "value1");
                await _client.SetAsync(key2, "value2");
                var finalSize = await _client.GetSizeAsync();

                // Assert
                Assert.Equal(initialSize + 2, finalSize);
            }
            finally
            {
                // Cleanup
                await _client.DeleteAsync(key1);
                await _client.DeleteAsync(key2);
            }
        }

        [Fact]
        public async Task GetObjectAsync_Extension_ShouldSerializeAndDeserialize()
        {
            // Skip if client is not initialized
            if (_client == null) return;

            // Arrange
            var key = $"object_test_{Guid.NewGuid()}";
            var testObject = new { Name = "Test", Value = 42, Active = true };

            try
            {
                // Act
                var setResult = await _client.SetObjectAsync(key, testObject);
                var retrievedObject = await _client.GetObjectAsync<dynamic>(key);

                // Assert
                Assert.True(setResult);
                Assert.NotNull(retrievedObject);
            }
            finally
            {
                // Cleanup
                await _client.DeleteAsync(key);
            }
        }

        [Fact]
        public async Task IncrementAsync_Extension_ShouldIncrementCorrectly()
        {
            // Skip if client is not initialized
            if (_client == null) return;

            // Arrange
            var key = $"increment_test_{Guid.NewGuid()}";

            try
            {
                // Act
                var result1 = await _client.IncrementAsync(key); // 0 + 1 = 1
                var result2 = await _client.IncrementAsync(key, 5); // 1 + 5 = 6
                var result3 = await _client.DecrementAsync(key, 2); // 6 - 2 = 4

                // Assert
                Assert.Equal(1, result1);
                Assert.Equal(6, result2);
                Assert.Equal(4, result3);
            }
            finally
            {
                // Cleanup
                await _client.DeleteAsync(key);
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }

    /// <summary>
    /// Testes unitários para componentes que não dependem do Rustdis
    /// </summary>
    public class RustdisUnitTests
    {
        [Fact]
        public void RustdisClientFactory_CreateClient_WithNullPath_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => RustdisClientFactory.CreateClient(null));
            Assert.Throws<ArgumentException>(() => RustdisClientFactory.CreateClient(""));
            Assert.Throws<ArgumentException>(() => RustdisClientFactory.CreateClient("   "));
        }

        [Fact]
        public void RustdisClientFactory_CreateClient_WithNonExistentPath_ShouldThrowException()
        {
            // Arrange
            var nonExistentPath = @"C:\NonExistent\rustdis.exe";

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => RustdisClientFactory.CreateClient(nonExistentPath));
        }
    }
}
