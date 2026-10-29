using back_end.Data;
using back_end.Middleware;
using back_end.Settings;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using back_end.Services;
using back_end.Services.Implements;
using back_end.Kafka.Producers.Implements;
using back_end.Kafka.Producers;
using back_end.Kafka.Consumers;
using back_end.Repositories;
using back_end.Repositories.Implements;
using back_end.Profiles;
using back_end.Configurations;
using back_end.Redis.Services;
using back_end.Redis.Services.Implements;
using Microsoft.AspNetCore.Http.Features;
using Amazon.S3;
using Amazon;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//Connect to the database
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Handle validation
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
    options.InvalidModelStateResponseFactory = context =>
        ValidationErrorResponse.Create(context)
);

// Add settings
builder.Services.Configure<SecuritySetting>(builder.Configuration.GetSection("Security"));
builder.Services.Configure<EmailSetting>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<KafkaSetting>(builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<GoogleSetting>(builder.Configuration.GetSection("Authentication:Google"));
builder.Services.Configure<S3Setting>(builder.Configuration.GetSection("AWS:S3"));
builder.Services.Configure<FileStorageSetting>(builder.Configuration.GetSection("FileStorage"));

// Connection redis
var redisConnectionString =
    builder.Configuration.GetSection("Redis:ConnectionString").Value;

if (string.IsNullOrWhiteSpace(redisConnectionString))
{
    throw new InvalidOperationException(
        "Redis ConnectionString was not found in the configuration!"
    );
}

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnectionString)
);

// Kafka
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

// Add Repository
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStoreRepository, StoreRepository>();

// Add service
builder.Services.AddScoped<IRedisService, RedisService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IGoogleService, GoogleService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFileService, FileService>();

// Background Service
builder.Services.AddHostedService<OtpEmailConsumer>();

// Auto mapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// Jwt setting
var securitySetting = builder.Configuration
    .GetSection("Security")
    .Get<SecuritySetting>();
builder.Services.AddJWTAuthentication(securitySetting!);

// Rate Limiting Configuration
builder.Services.AddRateLimitingConfiguration(securitySetting!);

// Register IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

var googleSetting = builder.Configuration
    .GetSection("Authentication:Google")
    .Get<GoogleSetting>();

//builder.Services.Configure<FormOptions>(options =>
//{
//    options.MultipartBodyLengthLimit = (googleSetting!.MaxFileSizeMB + 5) * 1024L * 1024L;
//});

// The AWS SDK uses the default credential provider chain.
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var options = sp.GetRequiredService<
        Microsoft.Extensions.Options.IOptions<S3Setting>>().Value;

    return new AmazonS3Client(
        RegionEndpoint.GetBySystemName(options.Region));
});

var app = builder.Build();

// Add Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Add Rate Limiting Middleware
app.UseRouting();
app.UseRateLimiter();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
