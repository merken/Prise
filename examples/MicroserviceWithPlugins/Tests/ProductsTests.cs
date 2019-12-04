using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Contract;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Tests
{
    public class ProductsTests : PluginTestBase
    {
        public ProductsTests(
#if NETCORE2_1
         MyHost2WebApplicationFactory factory
#endif
#if NETCORE3_0
         MyHostWebApplicationFactory factory
#endif
            ) : base(factory) { }

        [Fact]
        public async Task OldSql_Works()
        {
            // Arrange, Act
            var results = await Get<List<Product>>(_client, null, "/products");

            // Assert
            Assert.NotEmpty(results);
        }

        [Fact]
        public async Task Http_Works()
        {
            // Arrange, Act
            var results = await Get<List<Product>>(_client, "Lenovo", "/products", true);

            // Assert
            Assert.NotEmpty(results);
        }

#if NETCORE3_0
        [Fact]
        public async Task NewSql_Works()
        {
            // Arrange, Act
            var results = await Get<List<Product>>(_client, "HP", "/products");

            // Assert
            Assert.NotEmpty(results);
        }

        [Fact]
        public async Task TableStorage_Works()
        {
            // Arrange, Act
            var results = await Get<List<Product>>(_client, "Dell", "/products");

            // Assert
            Assert.NotEmpty(results);
        }

        [Fact]
        public async Task CosmosDb_Works()
        {
            // Arrange, Act
            var results = await Get<List<Product>>(_client, "Apple", "/products");

            // Assert
            Assert.NotEmpty(results);
        }
#endif
    }
}
