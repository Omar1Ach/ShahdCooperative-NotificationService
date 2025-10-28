using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using ShahdCooperative.NotificationService.Infrastructure.Configuration;

namespace ShahdCooperative.NotificationService.Infrastructure.Services;

public class NotificationQueueProcessor : BackgroundService
{
    private readonly ILogger<NotificationQueueProcessor> _logger;
    private readonly NotificationSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<INotificationSender> _senders;

    public NotificationQueueProcessor(
        ILogger<NotificationQueueProcessor> logger,
        IOptions<NotificationSettings> settings,
        IServiceProvider serviceProvider,
        IEnumerable<INotificationSender> senders)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _senders = senders ?? throw new ArgumentNullException(nameof(senders));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Queue Processor starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingNotificationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification queue");
            }

            await Task.Delay(TimeSpan.FromSeconds(_settings.ProcessingIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Notification Queue Processor stopped");
    }

    private async Task ProcessPendingNotificationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var queueRepository = scope.ServiceProvider.GetRequiredService<INotificationQueueRepository>();
        var logRepository = scope.ServiceProvider.GetRequiredService<INotificationLogRepository>();
        var templateEngine = scope.ServiceProvider.GetRequiredService<ITemplateEngine>();

        var pendingNotifications = await queueRepository.GetPendingNotificationsAsync(
            _settings.BatchSize,
            _settings.MaxRetries,
            cancellationToken);

        var notifications = pendingNotifications.ToList();

        if (notifications.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Processing {Count} pending notifications", notifications.Count);

        foreach (var notification in notifications)
        {
            await ProcessNotificationAsync(notification, queueRepository, logRepository, templateEngine, cancellationToken);
        }
    }

    private async Task ProcessNotificationAsync(
        NotificationQueue notification,
        INotificationQueueRepository queueRepository,
        INotificationLogRepository logRepository,
        ITemplateEngine templateEngine,
        CancellationToken cancellationToken)
    {
        try
        {
            await queueRepository.UpdateStatusAsync(notification.Id, NotificationStatus.Processing, cancellationToken);

            var sender = _senders.FirstOrDefault(s => s.NotificationType == notification.NotificationType);

            if (sender == null)
            {
                _logger.LogWarning("No sender found for notification type: {NotificationType}", notification.NotificationType);
                await queueRepository.UpdateStatusAsync(notification.Id, NotificationStatus.Failed, cancellationToken);
                await queueRepository.IncrementAttemptAsync(notification.Id, "No sender available", cancellationToken);
                return;
            }

            // Process template if TemplateKey is provided
            var body = notification.Body;
            if (!string.IsNullOrWhiteSpace(notification.TemplateKey))
            {
                body = await templateEngine.ProcessTemplateAsync(
                    notification.TemplateKey,
                    notification.TemplateData ?? "{}",
                    cancellationToken);

                if (string.IsNullOrEmpty(body))
                {
                    _logger.LogWarning("Template processing failed for notification {Id} with template key {TemplateKey}",
                        notification.Id, notification.TemplateKey);
                    body = notification.Body; // Fallback to original body
                }
            }

            var success = await sender.SendAsync(
                notification.Recipient,
                notification.Subject ?? string.Empty,
                body,
                cancellationToken);

            if (success)
            {
                await queueRepository.UpdateStatusAsync(notification.Id, NotificationStatus.Sent, cancellationToken);

                await logRepository.CreateAsync(new NotificationLog
                {
                    Type = notification.NotificationType,
                    Recipient = notification.Recipient,
                    Subject = notification.Subject,
                    Message = notification.Body,
                    Status = NotificationStatus.Sent,
                    SentAt = DateTime.UtcNow
                }, cancellationToken);

                _logger.LogInformation("Notification {Id} sent successfully to {Recipient}",
                    notification.Id, notification.Recipient);
            }
            else
            {
                await HandleFailedNotificationAsync(notification, queueRepository, "Send operation returned false", cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing notification {Id}", notification.Id);
            await HandleFailedNotificationAsync(notification, queueRepository, ex.Message, cancellationToken);
        }
    }

    private async Task HandleFailedNotificationAsync(
        NotificationQueue notification,
        INotificationQueueRepository queueRepository,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        await queueRepository.IncrementAttemptAsync(notification.Id, errorMessage, cancellationToken);

        var currentAttempt = notification.AttemptCount + 1;

        if (currentAttempt < notification.MaxRetries)
        {
            var retryDelay = CalculateExponentialBackoff(currentAttempt);
            var nextRetryAt = DateTime.UtcNow.Add(retryDelay);

            await queueRepository.SetNextRetryAsync(notification.Id, nextRetryAt, cancellationToken);

            _logger.LogWarning("Notification {Id} failed. Will retry at {NextRetryAt} (Attempt {Attempt}/{MaxRetries})",
                notification.Id, nextRetryAt, currentAttempt, notification.MaxRetries);
        }
        else
        {
            await queueRepository.UpdateStatusAsync(notification.Id, NotificationStatus.Failed, cancellationToken);

            _logger.LogError("Notification {Id} failed permanently after {MaxRetries} attempts: {ErrorMessage}",
                notification.Id, notification.MaxRetries, errorMessage);
        }
    }

    private TimeSpan CalculateExponentialBackoff(int attemptCount)
    {
        var baseDelayMinutes = _settings.RetryDelayMinutes;
        var exponentialDelay = Math.Pow(2, attemptCount - 1) * baseDelayMinutes;
        var maxDelayMinutes = 60; // Cap at 1 hour

        var delayMinutes = Math.Min(exponentialDelay, maxDelayMinutes);

        return TimeSpan.FromMinutes(delayMinutes);
    }
}
