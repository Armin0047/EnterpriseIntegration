using Microsoft.Extensions.Caching.Memory;

namespace EnterpriseIntegration.Infrastructure.Middleware
{
    public class IdempotencyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        public IdempotencyMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // فقط برای متدهایی مثل POST یا در صورت وجود هدر مربوطه اعمال شود
            if (context.Request.Headers.TryGetValue("X-Idempotency-Key", out var keyValue))
            {
                string key = $"idemp_{keyValue}";

                if (_cache.TryGetValue(key, out string? cachedResponse))
                {
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    await context.Response.WriteAsync(cachedResponse!);
                    return;
                }

                // خواندن و ذخیره پاسخ در کش با یک MemoryStream Wrapper
                var originalBody = context.Response.Body;
                using var memStream = new MemoryStream();
                context.Response.Body = memStream;

                await _next(context);

                memStream.Position = 0;
                string responseText = await new StreamReader(memStream).ReadToEndAsync();
                memStream.Position = 0;

                if (context.Response.StatusCode == StatusCodes.Status200OK)
                {
                    _cache.Set(key, responseText, TimeSpan.FromMinutes(10));
                }

                await memStream.CopyToAsync(originalBody);
                return;
            }

            await _next(context);
        }
    }
}
