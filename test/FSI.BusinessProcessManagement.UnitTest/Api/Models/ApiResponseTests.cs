using System;
using FSI.BusinessProcessManagement.Api.Models;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Api.Models
{
    public sealed class ApiResponseTests
    {
        [Fact]
        public void DefaultConstructor_ShouldSetSuccessTrue_AndNullMessageAndData()
        {
            // Arrange & Act
            var resp = new ApiResponse<string>();

            // Assert
            Assert.True(resp.Success);
            Assert.Null(resp.Message);
            Assert.Null(resp.Data);
        }

        [Fact]
        public void Ok_ShouldSetSuccessTrue_AndData_AssignsMessage_WhenProvided()
        {
            // Arrange
            var data = new { Id = 1, Name = "Alice" };

            // Act
            var resp = ApiResponse<object>.Ok(data, "created");

            // Assert
            Assert.True(resp.Success);
            Assert.Same(data, resp.Data);
            Assert.Equal("created", resp.Message);
        }

        [Fact]
        public void Ok_WithoutMessage_ShouldSetMessageNull()
        {
            // Arrange
            var data = "payload";

            // Act
            var resp = ApiResponse<string>.Ok(data);

            // Assert
            Assert.True(resp.Success);
            Assert.Equal("payload", resp.Data);
            Assert.Null(resp.Message);
        }

        [Fact]
        public void Ok_WithNullData_ShouldBeAllowed()
        {
            // Act
            var resp = ApiResponse<string>.Ok(null);

            // Assert
            Assert.True(resp.Success);
            Assert.Null(resp.Data);
            Assert.Null(resp.Message);
        }

        [Fact]
        public void Fail_ShouldSetSuccessFalse_AndMessage_AndNullData()
        {
            // Act
            var resp = ApiResponse<int>.Fail("boom");

            // Assert
            Assert.False(resp.Success);
            Assert.Equal("boom", resp.Message);
            Assert.Equal(default, resp.Data);
        }

        [Fact]
        public void Fail_WithEmptyMessage_IsAllowed_ButKeepsSuccessFalse_AndNullData()
        {
            // Act
            var resp = ApiResponse<object>.Fail(string.Empty);

            // Assert
            Assert.False(resp.Success);
            Assert.Equal(string.Empty, resp.Message);
            Assert.Null(resp.Data);
        }

        [Fact]
        public void Generic_WithValueTypes_WorksAsExpected()
        {
            // Arrange
            var ok = ApiResponse<int>.Ok(42, "ok");
            var fail = ApiResponse<int>.Fail("err");

            // Assert (OK)
            Assert.True(ok.Success);
            Assert.Equal(42, ok.Data);
            Assert.Equal("ok", ok.Message);

            // Assert (Fail)
            Assert.False(fail.Success);
            Assert.Equal("err", fail.Message);
            Assert.Equal(0, fail.Data); // default(int)
        }
    }
}
