using back_end.Settings;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Threading.RateLimiting;

namespace back_end.Configurations
{
    public static class RateLimitingConfiguration
    {
        public static IServiceCollection AddRateLimitingConfiguration(
            this IServiceCollection services,
            SecuritySetting securitySetting
        )
        {
            services.AddRateLimiter(
                options =>
                {
                    ConfigureRejectionStatusCode(options);
                    ConfigureGlobalLimiter(options, securitySetting.GlobalRateLimitPermitLimit, 1);
                    ConfigureLoginPolicy(options, securitySetting.LoginRateLimitPermitLimit, 60);
                    ConfigureOtpPolicy(options, securitySetting.OtpRateLimitPermitLimit, securitySetting.OtpExpirySeconds);
                    ConfigureOnRejected(options);
                }
            );
            return services;
        }

        // Response status code khi vượt giới hạn
        private static void ConfigureRejectionStatusCode(RateLimiterOptions options)
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        }


        // Global limit: 10 request/giây cho mỗi IP
        private static void ConfigureGlobalLimiter(RateLimiterOptions options, int limits, int seconds)
        {
            options.GlobalLimiter =
                PartitionedRateLimiter.Create<HttpContext, string>(
                    context =>
                    {
                        var ip = context.Connection.RemoteIpAddress?
                            .ToString() ?? "unknown";

                        return RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: ip,
                            factory: _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = limits,
                                Window = TimeSpan.FromSeconds(seconds),
                                AutoReplenishment = true,
                                QueueLimit = 0
                            });
                    });
        }

        // Policy dành cho đăng nhập
        private static void ConfigureLoginPolicy(RateLimiterOptions options, int limits, int seconds)
        {
            options.AddPolicy("LoginPolicy", context =>
            {
                var ip = context.Connection.RemoteIpAddress?
                    .ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ip,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = limits,
                        Window = TimeSpan.FromSeconds(seconds),
                        AutoReplenishment = true,
                        QueueLimit = 0
                    });
            });
        }

        // Policy dành cho gửi OTP
        private static void ConfigureOtpPolicy(RateLimiterOptions options, int limits, int seconds)
        {
            options.AddPolicy("OtpPolicy", context =>
            {
                var ip = context.Connection.RemoteIpAddress?
                    .ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ip,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = limits,
                        Window = TimeSpan.FromSeconds(seconds),
                        AutoReplenishment = true,
                        QueueLimit = 0
                    });
            });
        }

        // Xử lý request vượt giới hạn
        private static void ConfigureOnRejected(RateLimiterOptions options)
        {
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType =
                    "application/json";

                if (context.Lease.TryGetMetadata(
                        MetadataName.RetryAfter,
                        out var retryAfter))
                {
                    context.HttpContext.Response.Headers["Retry-After"] =
                        Math.Ceiling(retryAfter.TotalSeconds)
                            .ToString(CultureInfo.InvariantCulture);
                }

                await context.HttpContext.Response.WriteAsJsonAsync(
                    new
                    {
                        status = 429,
                        message = "Too many requests. Please try again later."
                    },
                    cancellationToken);
            };
        }
    }
}
