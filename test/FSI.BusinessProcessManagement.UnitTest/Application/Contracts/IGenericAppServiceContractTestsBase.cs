using FSI.BusinessProcessManagement.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;        // 👈 AQUI
using System.Threading.Tasks;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Contracts
{
    public abstract class IGenericAppServiceContractTestsBase<TDto> where TDto : class
    {
        protected abstract IGenericAppService<TDto> CreateService();
        protected abstract TDto CreateNewDto();
        protected abstract TDto CreateUpdatedDto(TDto original);
        protected abstract long GetId(TDto dto);

        [Fact]
        public async Task InsertAsync_ShouldReturnNewId_AndBeRetrievableByGetById()
        {
            var svc = CreateService();
            var dto = CreateNewDto();

            var id = await svc.InsertAsync(dto);
            var loaded = await svc.GetByIdAsync(id);

            Assert.True(id > 0);
            Assert.NotNull(loaded);
            Assert.Equal(id, GetId(loaded!));
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            var svc = CreateService();
            var result = await svc.GetByIdAsync(999_999);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInsertedItems()
        {
            var svc = CreateService();
            var a = CreateNewDto();
            var b = CreateNewDto();

            var idA = await svc.InsertAsync(a);
            var idB = await svc.InsertAsync(b);

            var all = (await svc.GetAllAsync()).ToList();

            Assert.True(all.Count >= 2);
            Assert.Contains(all, x => GetId(x) == idA);
            Assert.Contains(all, x => GetId(x) == idB);
        }

        [Fact]
        public async Task UpdateAsync_ShouldPersistChanges()
        {
            var svc = CreateService();
            var dto = CreateNewDto();
            var id = await svc.InsertAsync(dto);

            var existing = await svc.GetByIdAsync(id);
            Assert.NotNull(existing);

            var beforeSnapshot = JsonSerializer.Serialize(existing);
            var updatedDto = CreateUpdatedDto(existing!);

            await svc.UpdateAsync(updatedDto);
            var reloaded = await svc.GetByIdAsync(id);

            Assert.NotNull(reloaded);
            Assert.Equal(id, GetId(reloaded!));
            var afterSnapshot = JsonSerializer.Serialize(reloaded);
            Assert.NotEqual(beforeSnapshot, afterSnapshot);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveItem()
        {
            var svc = CreateService();
            var id = await svc.InsertAsync(CreateNewDto());

            await svc.DeleteAsync(id);
            var after = await svc.GetByIdAsync(id);

            Assert.Null(after);
        }
    }
}