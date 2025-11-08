using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Mappers;
using FSI.BusinessProcessManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Mappers
{
    public class UserRoleMapperTests
    {
        [Fact]
        public void ToNewEntity_Should_Map_UserId_And_RoleId()
        {
            // Arrange
            var dto = new UserRoleDto
            {
                UserId = 10,
                RoleId = 5
            };

            // Act
            var entity = UserRoleMapper.ToNewEntity(dto);

            // Assert
            Assert.NotNull(entity);
            Assert.Equal(10, entity.UserId);
            Assert.Equal(5, entity.RoleId);
        }

        [Fact]
        public void CopyToExisting_Should_Update_User_And_Role()
        {
            // Arrange
            var entity = new UserRole(1, 2);
            var dto = new UserRoleDto
            {
                UserId = 100,
                RoleId = 200
            };

            // Act
            UserRoleMapper.CopyToExisting(entity, dto);

            // Assert
            Assert.Equal(100, entity.UserId);
            Assert.Equal(200, entity.RoleId);
        }

        [Theory]
        [InlineData(10, 20)]
        [InlineData(5, 9)]
        [InlineData(123, 321)]
        public void CopyToExisting_Should_Work_For_Various_Values(long newUserId, long newRoleId)
        {
            // Arrange
            var entity = new UserRole(999, 888);
            var dto = new UserRoleDto
            {
                UserId = newUserId,
                RoleId = newRoleId
            };

            // Act
            UserRoleMapper.CopyToExisting(entity, dto);

            // Assert
            Assert.Equal(newUserId, entity.UserId);
            Assert.Equal(newRoleId, entity.RoleId);
        }

        [Fact]
        public void ToDto_Should_Map_All_Fields_From_Entity()
        {
            // Arrange
            var entity = new UserRole(10, 20);
            var expectedId = entity.Id;

            // Act
            var dto = UserRoleMapper.ToDto(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(expectedId, dto.UserRoleId);
            Assert.Equal(10, dto.UserId);
            Assert.Equal(20, dto.RoleId);
        }
    }
}