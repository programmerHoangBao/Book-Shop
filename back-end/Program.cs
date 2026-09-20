using back_end.Data;
using back_end.Middleware;
using back_end.Redis.Implements;
using back_end.Redis;
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

// Add service
builder.Services.AddScoped<IRedisService, RedisService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Background Service
builder.Services.AddHostedService<OtpEmailConsumer>();

// Auto mapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

var app = builder.Build();

// Add Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
