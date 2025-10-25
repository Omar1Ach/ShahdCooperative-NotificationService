using MediatR;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ShahdCooperative.NotificationService.Application.Commands.ProcessEvent;
using ShahdCooperative.NotificationService.Domain.Events;
using ShahdCooperative.NotificationService.Infrastructure.Configuration;
using System.Text;
using System.Text.Json;

namespace ShahdCooperative.NotificationService.API.BackgroundServices;

public class RabbitMQEventConsumer : BackgroundService
{
    private readonly ILogger<RabbitMQEventConsumer> _logger;
    private readonly RabbitMQSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMQEventConsumer(
        ILogger<RabbitMQEventConsumer> logger,
        IOptions<RabbitMQSettings> settings,
        IServiceProvider serviceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RabbitMQ Event Consumer starting...");

        try
        {
            await InitializeRabbitMQAsync(stoppingToken);
            await ConsumeEvents(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RabbitMQ Event Consumer");
            throw;
        }
    }

    private async Task InitializeRabbitMQAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            Port = _settings.Port,
            UserName = _settings.Username,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost,
            AutomaticRecoveryEnabled = _settings.AutomaticRecoveryEnabled,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(_settings.NetworkRecoveryInterval)
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(
            exchange: _settings.Exchange,
            type: _settings.ExchangeType,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            queue: _settings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        foreach (var routingKey in _settings.RoutingKeys)
        {
            await _channel.QueueBindAsync(
                queue: _settings.QueueName,
                exchange: _settings.Exchange,
                routingKey: routingKey,
                arguments: null,
                cancellationToken: cancellationToken);
        }

        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: _settings.PrefetchCount, global: false, cancellationToken);

        _logger.LogInformation("RabbitMQ connection established. Queue: {QueueName}, Exchange: {Exchange}",
            _settings.QueueName, _settings.Exchange);
    }

    private async Task ConsumeEvents(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;

            _logger.LogInformation("Received message with routing key: {RoutingKey}", routingKey);

            try
            {
                await ProcessMessage(routingKey, message);
                await _channel!.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                _logger.LogInformation("Message processed successfully: {RoutingKey}", routingKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message with routing key: {RoutingKey}", routingKey);
                await _channel!.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        await _channel!.BasicConsumeAsync(queue: _settings.QueueName, autoAck: false, consumer: consumer);

        _logger.LogInformation("Started consuming messages from queue: {QueueName}", _settings.QueueName);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessMessage(string routingKey, string message)
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        switch (routingKey)
        {
            case "user.registered":
                var userRegisteredEvent = JsonSerializer.Deserialize<UserRegisteredEvent>(message);
                if (userRegisteredEvent != null)
                {
                    await mediator.Send(new ProcessUserRegisteredCommand { Event = userRegisteredEvent });
                }
                break;

            case "user.logged-in":
                var userLoggedInEvent = JsonSerializer.Deserialize<UserLoggedInEvent>(message);
                if (userLoggedInEvent != null)
                {
                    await mediator.Send(new ProcessUserLoggedInCommand { Event = userLoggedInEvent });
                }
                break;

            case "password.changed":
                var passwordChangedEvent = JsonSerializer.Deserialize<PasswordChangedEvent>(message);
                if (passwordChangedEvent != null)
                {
                    await mediator.Send(new ProcessPasswordChangedCommand { Event = passwordChangedEvent });
                }
                break;

            case "order.created":
                var orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(message);
                if (orderCreatedEvent != null)
                {
                    await mediator.Send(new ProcessOrderCreatedCommand { Event = orderCreatedEvent });
                }
                break;

            case "order.shipped":
                var orderShippedEvent = JsonSerializer.Deserialize<OrderShippedEvent>(message);
                if (orderShippedEvent != null)
                {
                    await mediator.Send(new ProcessOrderShippedCommand { Event = orderShippedEvent });
                }
                break;

            case "product.out-of-stock":
                var productOutOfStockEvent = JsonSerializer.Deserialize<ProductOutOfStockEvent>(message);
                if (productOutOfStockEvent != null)
                {
                    await mediator.Send(new ProcessProductOutOfStockCommand { Event = productOutOfStockEvent });
                }
                break;

            case "feedback.received":
                var feedbackReceivedEvent = JsonSerializer.Deserialize<FeedbackReceivedEvent>(message);
                if (feedbackReceivedEvent != null)
                {
                    await mediator.Send(new ProcessFeedbackReceivedCommand { Event = feedbackReceivedEvent });
                }
                break;

            default:
                _logger.LogWarning("Unknown routing key: {RoutingKey}", routingKey);
                break;
        }
    }

    public override void Dispose()
    {
        try
        {
            _channel?.CloseAsync().GetAwaiter().GetResult();
            _channel?.Dispose();
        }
        catch { }

        try
        {
            _connection?.CloseAsync().GetAwaiter().GetResult();
            _connection?.Dispose();
        }
        catch { }

        base.Dispose();
    }
}
