using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Mappers;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Mappers
{
    public class AuditLogMapperTests
    {
        [Fact]
        public void ToNewEntity_Should_Map_Basic_Fields_And_AdditionalInfo_When_Provided()
        {
            var dto = new AuditLogDto
            {
                UserId = 10,
                ScreenId = 20,
                ActionType = "CREATE",
                AdditionalInfo = "Criado via teste"
            };

            var entity = AuditLogMapper.ToNewEntity(dto);

            Assert.NotNull(entity);
            Assert.Equal(10, entity.UserId);
            Assert.Equal(20, entity.ScreenId);
            Assert.Equal("CREATE", entity.ActionType);
            Assert.Equal("Criado via teste", entity.AdditionalInfo);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ToNewEntity_ShouldThrow_When_ActionType_Is_NullOrWhitespace(string? actionType)
        {
            // Arrange
            var dto = new AuditLogDto
            {
                UserId = null,
                ScreenId = null,
                ActionType = actionType,
                AdditionalInfo = null
            };

            // Act & Assert
            Assert.Throws<DomainException>(() => AuditLogMapper.ToNewEntity(dto));
        }

        [Fact]
        public void CopyToExisting_Should_Overwrite_All_Mutable_Fields()
        {
            var entity = new AuditLog("OLD", 1, 2);
            entity.SetAdditionalInfo("old-info");

            var dto = new AuditLogDto
            {
                UserId = 100,
                ScreenId = 200,
                ActionType = "UPDATE",
                AdditionalInfo = "nova-info"
            };

            AuditLogMapper.CopyToExisting(entity, dto);

            Assert.Equal(100, entity.UserId);
            Assert.Equal(200, entity.ScreenId);
            Assert.Equal("UPDATE", entity.ActionType);
            Assert.Equal("nova-info", entity.AdditionalInfo);
        }

        [Fact]
        public void ToDto_Should_Map_All_Fields_From_Entity()
        {
            var entity = new AuditLog("CREATE", 11, 22);
            entity.SetAdditionalInfo("info-x");

            var expectedId = entity.Id;
            var expectedAt = entity.ActionTimestamp;

            var dto = AuditLogMapper.ToDto(entity);

            Assert.NotNull(dto);
            Assert.Equal(expectedId, dto.AuditId);
            Assert.Equal(11, dto.UserId);
            Assert.Equal(22, dto.ScreenId);
            Assert.Equal("CREATE", dto.ActionType);
            Assert.Equal(expectedAt, dto.ActionTimestamp);
            Assert.Equal("info-x", dto.AdditionalInfo);
        }
    }
}