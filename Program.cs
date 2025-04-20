using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationSystem.Application.Services;
using NotificationSystem.Consumers;
using NotificationSystem.Domain.Repositories;
using NotificationSystem.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<NotificationDbContext>(options => {
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
    options.EnableSensitiveDataLogging();
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddHostedService<OutboxProcessorService>();
builder.Services.AddHostedService<NotificationSchedulerService>();

builder.Services.AddMassTransit(x => {
    x.AddConsumer<PushNotificationConsumer>();
    x.AddConsumer<EmailNotificationConsumer>();
    x.AddConsumer<NotificationDeliveryResultConsumer>();
    x.AddConsumer<ScheduleNotificationConsumer>();

    x.UsingRabbitMq((context, cfg) => {
        cfg.Host(configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(configuration["RabbitMQ:Username"]);
            h.Password(configuration["RabbitMQ:Password"]);
        });
        
        cfg.ReceiveEndpoint("push-notification-queue", e =>{
            e.PrefetchCount = 1;
            e.ConfigureConsumer<PushNotificationConsumer>(context);
        });

        cfg.ReceiveEndpoint("email-notification-queue", e =>
        {
            e.PrefetchCount = 1; // Process only one message at a time
            e.ConfigureConsumer<EmailNotificationConsumer>(context);
        });

        cfg.ReceiveEndpoint("notification-delivery-result-queue", e =>
        {
            e.ConfigureConsumer<NotificationDeliveryResultConsumer>(context);
        });

        cfg.ReceiveEndpoint("schedule-notification-queue", e =>
        {
            e.ConfigureConsumer<ScheduleNotificationConsumer>(context);
        });
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
