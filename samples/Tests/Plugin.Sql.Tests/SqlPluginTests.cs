using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using Plugin.Sql;
using System.Collections.Generic;
using Example.Contract;
using Microsoft.EntityFrameworkCore.InMemory;
using System.Threading.Tasks;
using System.Linq;

namespace Plugin.Sql.Tests
{
    [TestClass]
    public class SqlPluginTests
    {
        [TestMethod]
        public async Task Works()
        {
            var text = "This is data";
            using (var context = await CreateContextWithTestData(new[] { new MyDto { Text = text } }))
            {
                var plugin = Prise.Testing.CreateTestPluginInstance<SqlPlugin>(context);
                var results = await plugin.GetAll();
                Assert.AreEqual(text, results.First().Text);
            }
        }

        private async Task<SqlDbContext> CreateContextWithTestData(IEnumerable<MyDto> data)
        {
            var options = new DbContextOptionsBuilder<SqlDbContext>()
                        .UseInMemoryDatabase(databaseName: "myTestDb")
                        .Options;
            var context = new SqlDbContext(options);
            await context.Data.AddRangeAsync(data);
            await context.SaveChangesAsync();

            return context;
        }
    }
}
