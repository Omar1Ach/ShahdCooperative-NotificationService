using Dapper;
using Hangfire;
using Hangfire.SqlServer;
using ShahdCooperative.NotificationService.API.BackgroundServices;
using ShahdCooperative.NotificationService.API.Hubs;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using ShahdCooperative.NotificationService.Infrastructure.Configuration;
using ShahdCooperative.NotificationService.Infrastructure.Jobs;
using ShahdCooperative.NotificationService.Infrastructure.Repositories;
using ShahdCooperative.NotificationService.Infrastructure.Services;
using ShahdCooperative.NotificationService.Infrastructure.Services.Email;
using ShahdCooperative.NotificationService.Infrastructure.Services.Sms;
using ShahdCooperative.NotificationService.Infrastructure.Services.Push;
using ShahdCooperative.NotificationService.Infrastructure.Services.InApp;
using ShahdCooperative.NotificationService.Infrastructure.TypeHandlers;

var builder = WebApplication.CreateBuilder(args);

// Add configuration settings
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<NotificationSettings>(builder.Configuration.GetSection("NotificationSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<SmsSettings>(builder.Configuration.GetSection("SmsSettings"));
builder.Services.Configure<PushNotificationSettings>(builder.Configuration.GetSection("PushNotificationSettings"));

// Get connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configure Dapper to use string enum type handlers
SqlMapper.AddTypeHandler(new StringEnumTypeHandler<NotificationType>());
SqlMapper.AddTypeHandler(new StringEnumTypeHandler<NotificationStatus>());
SqlMapper.AddTypeHandler(new StringEnumTypeHandler<NotificationPriority>());
SqlMapper.AddTypeHandler(new StringEnumTypeHandler<InAppNotificationType>());

// Register repositories
builder.Services.AddScoped<INotificationTemplateRepository>(sp =>
    new NotificationTemplateRepository(connectionString));
builder.Services.AddScoped<INotificationQueueRepository>(sp =>
    new NotificationQueueRepository(connectionString));
builder.Services.AddScoped<IInAppNotificationRepository>(sp =>
    new InAppNotificationRepository(connectionString));
builder.Services.AddScoped<INotificationPreferenceRepository>(sp =>
    new NotificationPreferenceRepository(connectionString));
builder.Services.AddScoped<INotificationLogRepository>(sp =>
    new NotificationLogRepository(connectionString));

// Register MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ShahdCooperative.NotificationService.Application.AssemblyReference).Assembly);
});

// Register template engine
builder.Services.AddScoped<ITemplateEngine, TemplateEngine>();

// Register email sender based on provider
var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>();
if (emailSettings?.Provider?.ToLower() == "sendgrid")
{
    builder.Services.AddSingleton<INotificationSender, SendGridEmailSender>();
}
else if (emailSettings?.Provider?.ToLower() == "awsses")
{
    builder.Services.AddSingleton<INotificationSender, AwsSesEmailSender>();
}
else if (emailSettings?.Provider?.ToLower() == "smtp")
{
    builder.Services.AddSingleton<INotificationSender, SmtpEmailSender>();
}
else
{
    // Default to mock for email if not configured
    builder.Services.AddSingleton<INotificationSender>(sp =>
        new MockNotificationSender(sp.GetRequiredService<ILogger<MockNotificationSender>>(), NotificationType.Email));
}

// Register SMS sender based on provider
var smsSettings = builder.Configuration.GetSection("SmsSettings").Get<SmsSettings>();
if (smsSettings?.Provider?.ToLower() == "twilio")
{
    builder.Services.AddSingleton<INotificationSender, TwilioSmsSender>();
}
else if (smsSettings?.Provider?.ToLower() == "vonage")
{
    builder.Services.AddSingleton<INotificationSender, VonageSmsSender>();
}
else
{
    // Default to mock for SMS if not configured
    builder.Services.AddSingleton<INotificationSender>(sp =>
        new MockNotificationSender(sp.GetRequiredService<ILogger<MockNotificationSender>>(), NotificationType.SMS));
}

// Register Push notification sender based on configuration
var pushSettings = builder.Configuration.GetSection("PushNotificationSettings").Get<PushNotificationSettings>();
if (!string.IsNullOrWhiteSpace(pushSettings?.FirebaseCredentialsPath) &&
    File.Exists(pushSettings.FirebaseCredentialsPath))
{
    builder.Services.AddSingleton<INotificationSender, FirebasePushNotificationSender>();
}
else
{
    // Default to mock for Push if not configured
    builder.Services.AddSingleton<INotificationSender>(sp =>
        new MockNotificationSender(sp.GetRequiredService<ILogger<MockNotificationSender>>(), NotificationType.Push));
}

// Add SignalR
builder.Services.AddSignalR();

// Register SignalR hub client wrapper
builder.Services.AddSingleton<INotificationHubClient>(sp =>
{
    var hubContext = sp.GetRequiredService<Microsoft.AspNetCore.SignalR.IHubContext<NotificationHub>>();
    return new NotificationHubClient(hubContext);
});

// Register In-App notification sender with SignalR
builder.Services.AddSingleton<INotificationSender, InAppNotificationSender>();

// Register Hangfire jobs
builder.Services.AddScoped<ScheduledNotificationJob>();

// Add Hangfire only if not in Testing environment
if (builder.Environment.EnvironmentName != "Testing")
{
    builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

    builder.Services.AddHangfireServer();
}

// Register background services
builder.Services.AddHostedService<RabbitMQEventConsumer>();
builder.Services.AddHostedService<NotificationQueueProcessor>();

// Add controllers
builder.Services.AddControllers();

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS (configured for SignalR)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

// Use Hangfire Dashboard only if not in Testing environment
if (app.Environment.EnvironmentName != "Testing")
{
    app.UseHangfireDashboard("/hangfire");

    // Configure recurring jobs
    RecurringJob.AddOrUpdate<ScheduledNotificationJob>(
        "process-scheduled-notifications",
        job => job.ProcessScheduledNotificationsAsync(),
        "*/5 * * * *"); // Every 5 minutes

    RecurringJob.AddOrUpdate<ScheduledNotificationJob>(
        "cleanup-expired-notifications",
        job => job.CleanupExpiredNotificationsAsync(),
        Cron.Daily(2)); // Daily at 2 AM UTC

    RecurringJob.AddOrUpdate<ScheduledNotificationJob>(
        "retry-failed-notifications",
        job => job.RetryFailedNotificationsAsync(),
        Cron.Hourly()); // Every hour
}

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
