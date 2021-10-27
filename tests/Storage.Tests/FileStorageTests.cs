using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Dms.Storage;
using Storage.Tests.RandomGeneration;
using Xunit;

namespace Storage.Tests
{
    public class FileStorageTests
    {
        [Fact]
        public async Task Read_CleanStorage()
        {
            var tempDir = Path.GetTempPath();

            var randomFileName = Guid.NewGuid().ToString();
            var storage = new FileStorage(Path.Join(tempDir, randomFileName));

            await storage.InitializeAsync();

            var sampleKey = RandomDataGenerator.RandomString(32);
            var sampleValue = RandomDataGenerator.RandomBinary(128);

            await storage.WriteAsync(sampleKey, sampleValue);

            var result = await storage.ReadAsync(sampleKey);

            Assert.Equal(Encoding.UTF8.GetString(sampleValue), Encoding.UTF8.GetString(result.Span));
        }

        [Fact]
        public async Task Read_WithInitialization()
        {
            var tempDir = Path.GetTempPath();

            var randomFileName = Guid.NewGuid().ToString();
            var storage = new FileStorage(Path.Join(tempDir, randomFileName));

            await storage.InitializeAsync();

            var sampleKey = RandomDataGenerator.RandomString(32);
            var sampleValue = RandomDataGenerator.RandomBinary(128);

            await storage.WriteAsync(sampleKey, sampleValue);

            await storage.DisposeAsync();
            
            storage = new FileStorage(Path.Join(tempDir, randomFileName));

            await storage.InitializeAsync();

            var result = await storage.ReadAsync(sampleKey);

            Assert.Equal(Encoding.UTF8.GetString(sampleValue), Encoding.UTF8.GetString(result.Span));
        }
        
        [Fact]
        public async Task Delete_CleanStorage()
        {
            var tempDir = Path.GetTempPath();

            var randomFileName = Guid.NewGuid().ToString();
            var storage = new FileStorage(Path.Join(tempDir, randomFileName));

            await storage.InitializeAsync();

            var sampleKey = RandomDataGenerator.RandomString(32);
            var sampleValue = RandomDataGenerator.RandomBinary(128);

            await storage.WriteAsync(sampleKey, sampleValue);

            await storage.DeleteAsync(sampleKey);
            
            var result = await storage.ReadAsync(sampleKey);

            Assert.True(result.IsEmpty);
        }
        
        [Fact]
        public async Task Delete_WithInitialization()
        {
            var tempDir = Path.GetTempPath();

            var randomFileName = Guid.NewGuid().ToString();
            var storage = new FileStorage(Path.Join(tempDir, randomFileName));

            await storage.InitializeAsync();

            var sampleKey = RandomDataGenerator.RandomString(32);
            var sampleValue = RandomDataGenerator.RandomBinary(128);

            await storage.WriteAsync(sampleKey, sampleValue);

            await storage.DeleteAsync(sampleKey);
            
            await storage.DisposeAsync();
            
            storage = new FileStorage(Path.Join(tempDir, randomFileName));
            
            await storage.InitializeAsync();

            var result = await storage.ReadAsync(sampleKey);

            Assert.True(result.IsEmpty);
        }
        
        [Fact]
        public async Task VacuumStat_MakeSureReturnValueIsOk()
        {
            var tempDir = Path.GetTempPath();

            var randomFileName = Guid.NewGuid().ToString();
            var storage = new FileStorage(Path.Join(tempDir, randomFileName));

            await storage.InitializeAsync();

            var sampleKey = RandomDataGenerator.RandomString(32);
            var sampleValue = RandomDataGenerator.RandomBinary(128);

            await storage.WriteAsync(sampleKey, sampleValue);

            await storage.DeleteAsync(sampleKey);
            
            sampleKey = RandomDataGenerator.RandomString(32);
            sampleValue = RandomDataGenerator.RandomBinary(128);
            
            await storage.WriteAsync(sampleKey, sampleValue);

            var statBeforeVacuum = await storage.VacuumStatAsync();
            
            Assert.Equal(2, statBeforeVacuum.TotalNumberOfRecords);
            Assert.Equal(1, statBeforeVacuum.NumberOfActiveRecords);

            await storage.VacuumAsync();
            
            var statAfterVacuum = await storage.VacuumStatAsync();
            
            Assert.Equal(1, statAfterVacuum.TotalNumberOfRecords);
            Assert.Equal(1, statAfterVacuum.NumberOfActiveRecords);

        }
    }
}