using System.Diagnostics;
using System.Security.Claims;

namespace Janz.Api.Middlewares
{
    // Hər API sorğusunu bir sətirlə loglayır: metod, ünvan, status, müddət, istifadəçi və IP.
    // Sorğunun gövdəsi, ünvandakı sorğu parametrləri və Authorization başlığı loga YAZILMIR (parol, token, e-poçt sızmasın)
    public class RequestLoggingMiddleware
    {
        // Bundan yavaş sorğular Warning ilə yazılır
        private const int SlowMs = 1500;

        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            // Şəkillər, swagger və digər statik fayllar loga düşmür
            if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) || Path.HasExtension(path))
            {
                await _next(context);
                return;
            }

            var watch = Stopwatch.StartNew();
            try
            {
                await _next(context);
            }
            finally
            {
                watch.Stop();
                var status = context.Response.StatusCode;
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "-";

                var level = status >= 500 ? LogLevel.Error
                    : status >= 400 || watch.ElapsedMilliseconds >= SlowMs ? LogLevel.Warning
                    : LogLevel.Information;

                _logger.Log(level, "{Method} {Path} -> {Status} in {Elapsed} ms (user {UserId}, ip {Ip}, trace {TraceId})",
                    context.Request.Method, path, status, watch.ElapsedMilliseconds, userId, ip, context.TraceIdentifier);
            }
        }
    }
}
