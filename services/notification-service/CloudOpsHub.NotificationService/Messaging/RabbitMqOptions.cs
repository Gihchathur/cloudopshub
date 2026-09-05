namespace CloudOpsHub.NotificationService.Messaging;

public sealed class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "cloudops";
    public string Password { get; set; } = "cloudops_dev_password";
}