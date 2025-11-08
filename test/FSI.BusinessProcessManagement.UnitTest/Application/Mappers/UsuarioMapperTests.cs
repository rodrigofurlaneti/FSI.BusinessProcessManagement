using System;
using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Mappers;
using FSI.BusinessProcessManagement.Domain.Entities;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Mappers
{
    public class UsuarioMapperTests
    {
        [Fact]
        public void ToNewEntity_Should_Copy_Provided_Values_Directly()
        {
            var dto = new UsuarioDto
            {
                DepartmentId = 99,
                Username = "rodrigo",
                PasswordHash = "hash-123",
                Email = "r@acme.com",
                IsActive = false
            };

            var entity = UsuarioMapper.ToNewEntity(dto);

            Assert.Equal(99, entity.DepartmentId);
            Assert.Equal("rodrigo", entity.Username);
            Assert.Equal("r@acme.com", entity.Email);
            Assert.False(entity.IsActive);
        }

        [Fact]
        public void CopyToExisting_Should_Update_All_Fields_And_Activate_User()
        {
            var entity = new User("old", "old-hash", 1, "old@acme.com", false);

            var dto = new UsuarioDto
            {
                DepartmentId = 20,
                Username = "novo",
                PasswordHash = "nova-hash", 
                Email = "novo@acme.com",
                IsActive = true
            };

            UsuarioMapper.CopyToExisting(entity, dto);

            Assert.Equal(20, entity.DepartmentId);
            Assert.Equal("novo", entity.Username);
            Assert.Equal("novo@acme.com", entity.Email);
            Assert.True(entity.IsActive);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void CopyToExisting_Should_Not_Change_PasswordHash_When_Null_Or_Whitespace(string? passwordFromDto)
        {
            var entity = new User("name", "hash-antiga", 10, "x@acme.com", true);

            var dto = new UsuarioDto
            {
                DepartmentId = 30,
                Username = "name-atualizado",
                PasswordHash = passwordFromDto,
                Email = "y@acme.com",
                IsActive = false
            };

            UsuarioMapper.CopyToExisting(entity, dto);

            Assert.Equal(30, entity.DepartmentId);
            Assert.Equal("name-atualizado", entity.Username);
            Assert.Equal("y@acme.com", entity.Email);
            Assert.False(entity.IsActive);
        }

        [Fact]
        public void ToDto_Should_Map_All_Fields_From_Entity()
        {
            var entity = new User("rfurlaneti", "hash-x", 7, "r@acme.com", true);
            var expectedId = entity.Id;

            var dto = UsuarioMapper.ToDto(entity);

            Assert.NotNull(dto);
            Assert.Equal(expectedId, dto.UserId);
            Assert.Equal(7, dto.DepartmentId);
            Assert.Equal("rfurlaneti", dto.Username);
            Assert.Equal("r@acme.com", dto.Email);
            Assert.True(dto.IsActive);
        }
    }
}
