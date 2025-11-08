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
    public class RoleScreenPermissionMapperTests
    {
        [Fact]
        public void ToNewEntity_Should_Map_All_Fields()
        {
            // Arrange
            var dto = new RoleScreenPermissionDto
            {
                RoleId = 10,
                ScreenId = 20,
                CanView = true,
                CanCreate = false,
                CanEdit = true,
                CanDelete = false
            };

            // Act
            var entity = RoleScreenPermissionMapper.ToNewEntity(dto);

            // Assert
            Assert.NotNull(entity);
            Assert.Equal(10, entity.RoleId);
            Assert.Equal(20, entity.ScreenId);
            Assert.True(entity.CanView);
            Assert.False(entity.CanCreate);
            Assert.True(entity.CanEdit);
            Assert.False(entity.CanDelete);
        }

        [Theory]
        // RoleId, ScreenId, View, Create, Edit, Delete
        [InlineData(1, 2, true, true, true, true)]
        [InlineData(1, 2, false, false, false, false)]
        [InlineData(5, 9, true, false, true, false)]
        [InlineData(7, 3, false, true, false, true)]
        public void ToNewEntity_Should_Work_For_Multiple_Permission_Combinations(
            long roleId, long screenId, bool v, bool c, bool e, bool d)
        {
            // Arrange
            var dto = new RoleScreenPermissionDto
            {
                RoleId = roleId,
                ScreenId = screenId,
                CanView = v,
                CanCreate = c,
                CanEdit = e,
                CanDelete = d
            };

            // Act
            var entity = RoleScreenPermissionMapper.ToNewEntity(dto);

            // Assert
            Assert.Equal(roleId, entity.RoleId);
            Assert.Equal(screenId, entity.ScreenId);
            Assert.Equal(v, entity.CanView);
            Assert.Equal(c, entity.CanCreate);
            Assert.Equal(e, entity.CanEdit);
            Assert.Equal(d, entity.CanDelete);
        }

        [Fact]
        public void CopyToExisting_Should_Update_All_Permissions()
        {
            // Arrange: entidade com um conjunto inicial
            var entity = new RoleScreenPermission(roleId: 1, screenId: 2, canView: false, canCreate: false, canEdit: false, canDelete: false);

            // DTO com outro conjunto de permissões
            var dto = new RoleScreenPermissionDto
            {
                RoleId = 1,     // não é alterado em CopyToExisting
                ScreenId = 2,   // não é alterado em CopyToExisting
                CanView = true,
                CanCreate = true,
                CanEdit = false,
                CanDelete = true
            };

            // Act
            RoleScreenPermissionMapper.CopyToExisting(entity, dto);

            // Assert: apenas flags mudam; roleId/screenId permanecem
            Assert.Equal(1, entity.RoleId);
            Assert.Equal(2, entity.ScreenId);
            Assert.True(entity.CanView);
            Assert.True(entity.CanCreate);
            Assert.False(entity.CanEdit);
            Assert.True(entity.CanDelete);
        }

        [Theory]
        [InlineData(false, false, false, false)]
        [InlineData(true, false, false, false)]
        [InlineData(false, true, false, false)]
        [InlineData(false, false, true, false)]
        [InlineData(false, false, false, true)]
        public void CopyToExisting_Should_Handle_All_Boolean_Edges(bool v, bool c, bool e, bool d)
        {
            // Arrange
            var entity = new RoleScreenPermission(10, 20, !v, !c, !e, !d); // começa invertido
            var dto = new RoleScreenPermissionDto
            {
                RoleId = 10,
                ScreenId = 20,
                CanView = v,
                CanCreate = c,
                CanEdit = e,
                CanDelete = d
            };

            // Act
            RoleScreenPermissionMapper.CopyToExisting(entity, dto);

            // Assert
            Assert.Equal(v, entity.CanView);
            Assert.Equal(c, entity.CanCreate);
            Assert.Equal(e, entity.CanEdit);
            Assert.Equal(d, entity.CanDelete);
        }

        [Fact]
        public void ToDto_Should_Map_All_Fields_From_Entity()
        {
            // Arrange
            var entity = new RoleScreenPermission(99, 77, true, false, true, false);
            var expectedId = entity.Id;

            // Act
            var dto = RoleScreenPermissionMapper.ToDto(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(expectedId, dto.RoleScreenPermissionId);
            Assert.Equal(99, dto.RoleId);
            Assert.Equal(77, dto.ScreenId);
            Assert.True(dto.CanView);
            Assert.False(dto.CanCreate);
            Assert.True(dto.CanEdit);
            Assert.False(dto.CanDelete);
        }
    }
}