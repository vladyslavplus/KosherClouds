using KosherClouds.ServiceDefaults.Middleware;
using Microsoft.AspNetCore.Builder;

namespace KosherClouds.ServiceDefaults.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
