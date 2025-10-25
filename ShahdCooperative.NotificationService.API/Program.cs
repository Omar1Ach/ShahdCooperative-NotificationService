using ShahdCooperative.NotificationService.API.BackgroundServices;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using ShahdCooperative.NotificationService.Infrastructure.Configuration;
using ShahdCooperative.NotificationService.Infrastructure.Repositories;
using ShahdCooperative.NotificationService.Infrastructure.Services;

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

// Register notification senders (mock implementations for now)
builder.Services.AddSingleton<INotificationSender>(sp =>
    new MockNotificationSender(sp.GetRequiredService<ILogger<MockNotificationSender>>(), NotificationType.Email));
builder.Services.AddSingleton<INotificationSender>(sp =>
    new MockNotificationSender(sp.GetRequiredService<ILogger<MockNotificationSender>>(), NotificationType.SMS));
builder.Services.AddSingleton<INotificationSender>(sp =>
    new MockNotificationSender(sp.GetRequiredService<ILogger<MockNotificationSender>>(), NotificationType.Push));
builder.Services.AddSingleton<INotificationSender>(sp =>
    new MockNotificationSender(sp.GetRequiredService<ILogger<MockNotificationSender>>(), NotificationType.InApp));

// Register background services
builder.Services.AddHostedService<RabbitMQEventConsumer>();
builder.Services.AddHostedService<NotificationQueueProcessor>();

// Add controllers
builder.Services.AddControllers();

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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
app.MapControllers();

app.Run();
