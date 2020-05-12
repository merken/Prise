using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Prise.IntegrationTests
{
    public abstract class SerializationWithCollectableTestsBase : PluginTestBase
    {
        protected SerializationWithCollectableTestsBase(AppHostWebApplicationFactory factory)
            : base(factory) { }

        protected async Task<string> GetRaw(HttpClient client, string accept, string endpoint)
        {
            client.DefaultRequestHeaders.Add("Accept", accept);
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
                throw new Exception("Result was not success!");

            return await response.Content.ReadAsStringAsync();
        }
    }

    public class SerializationWithCollectableTests : SerializationWithCollectableTestsBase
    {
        public SerializationWithCollectableTests()
            : base(new AppHostWebApplicationFactory(new CommandLineArgumentsLazy { UseCollectibleAssemblies = true, UseLazyService = false })) { }

        [Fact]
        public async Task PluginF_Works_WithJson()
        {
#if NETCORE2_1
            // Arrange
            var expectedJson = "\"{\u005C\"StringProperty\u005C\":\u005C\"Some string\u005C\",\u005C\"IntProperty\u005C\":999,\u005C\"DoubleProperty\u005C\":4336.99}\"";
#else
            // Arrange
            var expectedJson = "\"{\u005C\"StringProperty\u005C\":\u005C\"Some string\u005C\",\u005C\"IntProperty\u005C\":999,\u005C\"DoubleProperty\u005C\":4336.9899999999998}\"";
#endif
            //Act
            var result = await GetRaw(_client, "application/json", "/serialization");

            // Assert
            Assert.Equal(expectedJson, result);
        }

#if NETCORE3_0 || NETCORE3_1
        [Fact]
        public async Task PluginF_DOES_NOT_Work_WithXml()
        {
            // Arrange
            //Act
            var result = await GetRaw(_client, "application/xml", "/serialization");

            // Assert
            Assert.Equal("A non-collectible assembly may not reference a collectible assembly.", result);
        }
#endif


#if NETCORE2_1
        [Fact]
        public async Task PluginF_Works_WithXml()
        {
            // Arrange
            //Act
            var result = await GetRaw(_client, "application/xml", "/serialization");

            // Assert
            Assert.Equal(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ObjectToSerialize>
  <StringProperty>Some string</StringProperty>
  <IntProperty>999</IntProperty>
  <DoubleProperty>4336.99</DoubleProperty>
</ObjectToSerialize>", result);
        }
#endif
    }

    public class SerializationWithoutCollectableTests : SerializationWithCollectableTestsBase
    {
        public SerializationWithoutCollectableTests()
            : base(new AppHostWebApplicationFactory(new CommandLineArgumentsLazy { UseCollectibleAssemblies = false, UseLazyService = false })) { }

        [Fact]
        public async Task PluginF_Works_WithJson()
        {
#if NETCORE2_1
            // Arrange
            var expectedJson = "\"{\u005C\"StringProperty\u005C\":\u005C\"Some string\u005C\",\u005C\"IntProperty\u005C\":999,\u005C\"DoubleProperty\u005C\":4336.99}\"";
#else
            // Arrange
            var expectedJson = "\"{\u005C\"StringProperty\u005C\":\u005C\"Some string\u005C\",\u005C\"IntProperty\u005C\":999,\u005C\"DoubleProperty\u005C\":4336.9899999999998}\"";
#endif
            //Act
            var result = await GetRaw(_client, "application/json", "/serialization");

            // Assert
            Assert.Equal(expectedJson, result);
        }

        [Fact]
        public async Task PluginF_Works_WithXml()
        {
            // Arrange
            //Act
            var result = await GetRaw(_client, "application/xml", "/serialization");

            // Assert
            Assert.Equal(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ObjectToSerialize>
  <StringProperty>Some string</StringProperty>
  <IntProperty>999</IntProperty>
  <DoubleProperty>4336.99</DoubleProperty>
</ObjectToSerialize>", result);
        }
    }
}
