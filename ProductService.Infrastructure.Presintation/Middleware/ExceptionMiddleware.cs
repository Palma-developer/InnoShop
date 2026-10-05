using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ProductService.Infrastructure.Presentation.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var problemDetails = new ProblemDetails
            {
                Title = "Произошла ошибка",
                Detail = exception.Message,
                Instance = context.Request.Path
            };

            switch (exception)
            {
                case KeyNotFoundException:
                    problemDetails.Status = 404;
                    problemDetails.Title = "Не найдено";
                    break;
                case UnauthorizedAccessException:
                    problemDetails.Status = 403;
                    problemDetails.Title = "Доступ запрещен";
                    break;
                case ArgumentException:
                    problemDetails.Status = 400;
                    problemDetails.Title = "Неверные данные";
                    break;
                default:
                    problemDetails.Status = 500;
                    problemDetails.Title = "Внутренняя ошибка сервера";
                    break;
            }

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = problemDetails.Status.Value;

            var json = JsonSerializer.Serialize(problemDetails);
            await context.Response.WriteAsync(json);
        }
    }
}