namespace ShahdCooperative.NotificationService.Infrastructure.Configuration;

public class RabbitMQSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string Exchange { get; set; } = string.Empty;
    public string ExchangeType { get; set; } = "topic";
    public string QueueName { get; set; } = string.Empty;
    public List<string> RoutingKeys { get; set; } = new();
    public ushort PrefetchCount { get; set; } = 10;
    public bool AutomaticRecoveryEnabled { get; set; } = true;
    public int NetworkRecoveryInterval { get; set; } = 5;
}
