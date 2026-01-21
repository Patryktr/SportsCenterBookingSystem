using System.Threading.RateLimiting;

namespace SportsCenter.API.Extentions.RateLimiterConfig;

public static class RateLimiterConfig
{
    public static IServiceCollection AddCustomRateLimiter(
        this IServiceCollection services,
        int permitLimit = 10,
        TimeSpan? window = null,
        bool enableGlobalLimiter = true)
    {
        var effectiveWindow = window ?? TimeSpan.FromMinutes(1);

        services.AddRateLimiter(options =>
        {
            // ===== POLITYKA PER-CUSTOMER =====
            options.AddPolicy("per-customer", httpContext =>
            {
                var customerId = GetCustomerId(httpContext);
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: customerId,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = effectiveWindow,
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        AutoReplenishment = true
                    });
            });

            // ===== GLOBALNY LIMITER (opcjonalny) =====
            if (enableGlobalLimiter)
            {
                options.GlobalLimiter =
                    PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                    {
                        var customerId = GetCustomerId(ctx);
                        return RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: customerId,
                            factory: _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = permitLimit,
                                Window = effectiveWindow,
                                QueueLimit = 0,
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                AutoReplenishment = true
                            });
                    });
            }

            // ===== 429 response =====
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (ctx, token) =>
            {
                var retryAfterSeconds = Math.Max(1, (int)effectiveWindow.TotalSeconds);
                ctx.HttpContext.Response.Headers["Retry-After"] = retryAfterSeconds.ToString();

                await ctx.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "rate_limited",
                    detail = "Too many requests. Try again later."
                }, cancellationToken: token);
            };
        });

        return services;
    }

    private static string GetCustomerId(HttpContext ctx)
    {
        // route
        if (ctx.Request.RouteValues.TryGetValue("customerId", out var rv) && rv is not null)
        {
            var val = rv.ToString();
            if (!string.IsNullOrWhiteSpace(val))
                return val!;
        }

        // header
        var header = ctx.Request.Headers["X-Customer-Id"].ToString();
        if (!string.IsNullOrWhiteSpace(header))
            return header;

        // IP fallback
        var ip = ctx.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrWhiteSpace(ip))
            return $"ip:{ip}";

        return "anon";
    }
}
