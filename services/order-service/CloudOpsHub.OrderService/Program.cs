using CloudOpsHub.OrderService.Application.Services;
using CloudOpsHub.OrderService.Infrastructure.Persistence;
using CloudOpsHub.OrderService.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres"));
});

builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<IRabbitMqPublisher, RabbitMqPublisher>();
builder.Services.AddScoped<CloudOpsHub.OrderService.Application.Services.OutboxService>();

builder.Services.Configure<OutboxOptions>(builder.Configuration.GetSection("Outbox"));

builder.Services.AddHostedService<OutboxPublisherWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();