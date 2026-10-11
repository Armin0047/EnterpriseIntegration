using System.Diagnostics;
using System.Security.Claims;

namespace EnterpriseIntegration.Infrastructure.Middleware
{
    public class AuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditLoggingMiddleware> _logger;

        public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            var path = context.Request.Path;
            var method = context.Request.Method;

            try
            {
                await _next(context);
            }
            finally
            {
                sw.Stop();
                var statusCode = context.Response.StatusCode;
                var clientApp = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? "Anonymous";

                _logger.LogInformation(
                    "[AUDIT] Client: {Client} | IP: {IP} | {Method} {Path} | Status: {Status} | Elapsed: {Elapsed}ms",
                    clientApp, clientIp, method, path, statusCode, sw.ElapsedMilliseconds
                );
            }
        }
    }
}
