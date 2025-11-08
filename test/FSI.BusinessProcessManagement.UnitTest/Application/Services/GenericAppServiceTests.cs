using FSI.BusinessProcessManagement.Application.Services;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Application.Services;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using Moq;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Services
{
    public class GenericAppServiceTests
    {
        private readonly Mock<IRepository<FakeEntity>> _repoMock = new();
        private readonly Mock<IUnitOfWork> _uowMock = new();

        private FakeGenericAppService CreateSut()
            => new FakeGenericAppService(_uowMock.Object, _repoMock.Object);

        [Fact]
        public async Task GetAllAsync_ShouldReturnMappedDtos()
        {
            // Arrange
            var entities = new List<FakeEntity>
            {
                new FakeEntity { Id = 1, Name = "A" },
                new FakeEntity { Id = 2, Name = "B" }
            };

            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);
            var sut = CreateSut();

            // Act
            var result = await sut.GetAllAsync();

            // Assert
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.Equal("A", list[0].Name);
            Assert.Equal(1, list[0].Id);
            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnMappedDto_WhenEntityExists()
        {
            // Arrange
            var entity = new FakeEntity { Id = 5, Name = "Test" };
            _repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(entity);
            var sut = CreateSut();

            // Act
            var result = await sut.GetByIdAsync(5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result!.Id);
            Assert.Equal("Test", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenEntityDoesNotExist()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((FakeEntity)null!);
            var sut = CreateSut();

            // Act
            var result = await sut.GetByIdAsync(10);

            // Assert
            Assert.Null(result);
            _repoMock.Verify(r => r.GetByIdAsync(10), Times.Once);
        }

        [Fact]
        public async Task InsertAsync_ShouldMapDtoAndReturnEntityId()
        {
            // Arrange
            var dto = new FakeDto { Id = 0, Name = "Inserted" };

            _repoMock.Setup(r => r.InsertAsync(It.IsAny<FakeEntity>()))
                     .Callback<FakeEntity>(e => e.Id = 123)
                     .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var id = await sut.InsertAsync(dto);

            // Assert
            Assert.Equal(123, id);
            _repoMock.Verify(r => r.InsertAsync(It.Is<FakeEntity>(e => e.Name == "Inserted")), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task InsertAsync_ShouldReturnZero_WhenEntityHasNoIdProperty()
        {
            // Arrange
            var repoMock = new Mock<IRepository<object>>();
            var uowMock = new Mock<IUnitOfWork>();

            // Cria um serviço derivado temporário sem "Id"
            var sut = new NoIdGenericService(uowMock.Object, repoMock.Object);

            var dto = new object();

            // Act
            var id = await sut.InsertAsync(dto);

            // Assert
            Assert.Equal(0L, id);
        }

        private class NoIdGenericService : GenericAppService<object, object>
        {
            public NoIdGenericService(IUnitOfWork uow, IRepository<object> repo)
                : base(uow, repo) { }

            protected override object MapToDto(object entity) => entity;
            protected override object MapToEntity(object dto) => dto;
            public override Task UpdateAsync(object dto) => Task.CompletedTask;
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDeleteAndCommit()
        {
            // Arrange
            _repoMock.Setup(r => r.DeleteAsync(99)).Returns(Task.CompletedTask);
            var sut = CreateSut();

            // Act
            await sut.DeleteAsync(99);

            // Assert
            _repoMock.Verify(r => r.DeleteAsync(99), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallRepositoryAndCommit()
        {
            // Arrange
            var dto = new FakeDto { Id = 7, Name = "Old" };
            var sut = CreateSut();

            // Act
            await sut.UpdateAsync(dto);

            // Assert
            _repoMock.Verify(r => r.UpdateAsync(It.Is<FakeEntity>(e => e.Id == 7 && e.Name == "Old_updated")), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }
    }
}