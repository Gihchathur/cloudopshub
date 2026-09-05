using CloudOpsHub.NotificationService.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<OrderCreatedConsumer>();

var host = builder.Build();

host.Run();