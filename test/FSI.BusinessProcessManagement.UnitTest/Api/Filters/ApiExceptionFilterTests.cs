using System;
using FSI.BusinessProcessManagement.Api.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Api.Filters
{
    public sealed class ApiExceptionFilterTests
    {
        private static ExceptionContext MakeExceptionContext(Exception ex)
        {
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor()
            );

            return new ExceptionContext(actionContext, filters: Array.Empty<IFilterMetadata>())
            {
                Exception = ex
            };
        }

        [Fact]
        public void OnException_ShouldSetObjectResult_WithProblemDetails_AndMarkHandled()
        {
            // Arrange
            var filter = new ApiExceptionFilter();
            var ex = new InvalidOperationException("algo deu errado");
            var ctx = MakeExceptionContext(ex);

            // Act
            filter.OnException(ctx);

            // Assert
            Assert.True(ctx.ExceptionHandled);

            var result = Assert.IsType<ObjectResult>(ctx.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);

            var problem = Assert.IsType<ProblemDetails>(result.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
            Assert.Equal("Erro ao processar a requisição", problem.Title);
            Assert.Equal("algo deu errado", problem.Detail);
        }

        [Fact]
        public void OnException_ShouldNotNullifyExceptionInstance()
        {
            // Arrange
            var filter = new ApiExceptionFilter();
            var ex = new ApplicationException("falhou forte");
            var ctx = MakeExceptionContext(ex);

            // Act
            filter.OnException(ctx);

            // Assert
            Assert.Same(ex, ctx.Exception);
            Assert.True(ctx.ExceptionHandled);

            var objectResult = Assert.IsType<ObjectResult>(ctx.Result);
            var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
            Assert.Equal("falhou forte", problem.Detail);
        }

        [Fact]
        public void OnException_ShouldOverwriteAnyExistingResult()
        {
            // Arrange
            var filter = new ApiExceptionFilter();
            var ex = new Exception("boom");
            var ctx = MakeExceptionContext(ex);

            ctx.Result = new ContentResult { Content = "old", StatusCode = 418 };

            // Act
            filter.OnException(ctx);

            // Assert
            var result = Assert.IsType<ObjectResult>(ctx.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
            var problem = Assert.IsType<ProblemDetails>(result.Value);
            Assert.Equal("boom", problem.Detail);
        }
    }
}
