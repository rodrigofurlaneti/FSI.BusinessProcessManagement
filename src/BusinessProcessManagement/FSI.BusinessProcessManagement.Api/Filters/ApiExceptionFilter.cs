using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FSI.BusinessProcessManagement.Api.Filters
{
    public class ApiExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var problem = new ProblemDetails
            {
                Title = "Erro ao processar a requisição",
                Detail = context.Exception.Message,
                Status = StatusCodes.Status400BadRequest
            };
            context.Result = new ObjectResult(problem) { StatusCode = problem.Status };
            context.ExceptionHandled = true;
        }
    }
}
