using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Services;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using FSI.BusinessProcessManagement.UnitTests.Application.Tests; // Para TestHelpers.SetId
using Moq;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Services
{
    public class UsuarioAppServiceTests
    {
        private readonly Mock<IRepository<User>> _repoMock = new();
        private readonly Mock<IUnitOfWork> _uowMock = new();

        private UsuarioAppService CreateSut()
            => new UsuarioAppService(_repoMock.Object, _uowMock.Object);

        [Fact]
        public async Task GetAllAsync_ShouldReturnDtos_FromRepositoryEntities()
        {
            // Arrange
            var u1 = new User(username: "rodrigo", passwordHash: "123", departmentId: 1, email: "r@gru.com", isActive: true);
            var u2 = new User(username: "ana", passwordHash: "abc", departmentId: 2, email: "a@gru.com", isActive: false);
            TestHelpers.SetId(u1, 1);
            TestHelpers.SetId(u2, 2);

            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(new[] { u1, u2 }.AsEnumerable());

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, d => d.UserId == 1 && d.Username == "rodrigo");
            Assert.Contains(list, d => d.UserId == 2 && d.Username == "ana");

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _uowMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(99))
                     .ReturnsAsync((User)null!);

            var sut = CreateSut();

            // Act
            var dto = await sut.GetByIdAsync(99);

            // Assert
            Assert.Null(dto);
            _repoMock.Verify(r => r.GetByIdAsync(99), Times.Once);
            _uowMock.VerifyNoOtherCalls();
        }
        
        [Fact]
        public async Task InsertAsync_ShouldInsertAndCommit_AndReturnGeneratedId()
        {
            // Arrange
            var dto = new UsuarioDto
            {
                UserId = 0,
                Username = "joao",
                PasswordHash = "pwd",
                DepartmentId = 1,
                Email = "joao@gru.com",
                IsActive = true
            };

            _repoMock.Setup(r => r.InsertAsync(It.IsAny<User>()))
                     .Callback<User>(e => TestHelpers.SetId(e, 99))
                     .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var id = await sut.InsertAsync(dto);

            // Assert
            Assert.Equal(99, id);
            _repoMock.Verify(r => r.InsertAsync(It.IsAny<User>()), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenUserNotFound()
        {
            // Arrange
            var dto = new UsuarioDto
            {
                UserId = 77,
                Username = "teste",
                Email = "teste@gru.com"
            };

            _repoMock.Setup(r => r.GetByIdAsync(77))
                     .ReturnsAsync((User)null!);

            var sut = CreateSut();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.UpdateAsync(dto));
            Assert.Equal("User not found.", ex.Message);

            _repoMock.Verify(r => r.GetByIdAsync(77), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
            _uowMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateAndCommit_WhenEntityFound()
        {
            // Arrange
            var entity = new User(username: "lucas", passwordHash: "123", departmentId: 1, email: "lucas@gru.com", isActive: true);
            TestHelpers.SetId(entity, 50);

            var dto = new UsuarioDto
            {
                UserId = 50,
                Username = "lucas.new",
                Email = "lucas.new@gru.com",
                IsActive = false
            };

            _repoMock.Setup(r => r.GetByIdAsync(50))
                     .ReturnsAsync(entity);

            _repoMock.Setup(r => r.UpdateAsync(entity))
                     .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.UpdateAsync(dto);

            // Assert
            _repoMock.Verify(r => r.GetByIdAsync(50), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.Is<User>(e => e == entity)), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteAndCommit()
        {
            // Arrange
            _repoMock.Setup(r => r.DeleteAsync(9)).Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.DeleteAsync(9);

            // Assert
            _repoMock.Verify(r => r.DeleteAsync(9), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }
    }
}
