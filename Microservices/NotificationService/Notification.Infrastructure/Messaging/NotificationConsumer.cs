using MassTransit;
using Notification.Core.DTOs;
using RecoMind.Contracts.Events;
using Notification.Core.Interfaces;

namespace Notification.Infrastructure.Messaging
{
    public class NotificationConsumer : IConsumer<NotificationEventDto>
    {
        private readonly INotificationService _notificationService;

        public NotificationConsumer(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task Consume(ConsumeContext<NotificationEventDto> context)
        {
            // MassTransit بيعمل Deserialize للرسالة لوحده هنا!
            var msg = context.Message;
            await _notificationService.SendNotificationAsync(msg);
        }
    }
}


//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using Notification.Core.Events;
//using Notification.Core.Interfaces;
//using Notification.Core.Models;
//using RabbitMQ.Client;
//using RabbitMQ.Client.Events;
//using System.Text;
//using System.Text.Json;

//namespace Notification.Infrastructure.Messaging
//{
//    public class RabbitMQConsumer : BackgroundService
//    {
//        private readonly IServiceProvider _services;
//        private readonly ILogger<RabbitMQConsumer> _logger;
//        private readonly RabbitMqSettings _settings;
//        private IConnection? _connection;
//        private IChannel? _channel;

//        public RabbitMQConsumer(
//            IOptions<RabbitMqSettings> options,
//            IServiceProvider services,
//            ILogger<RabbitMQConsumer> logger)
//        {
//            _services = services;
//            _logger = logger;
//            _settings = options.Value;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            await ConnectWithRetryAsync(stoppingToken);

//            if (_channel == null) return;

//            // بنسمع لصف واحد فقط شايل كل أنواع التنبيهات
//            await StartConsumerAsync("notification.queue", HandleNotificationAsync, stoppingToken);

//            while (!stoppingToken.IsCancellationRequested)
//            {
//                await Task.Delay(1000, stoppingToken);
//            }
//        }

//        private async Task ConnectWithRetryAsync(CancellationToken stoppingToken)
//        {
//            var factory = new ConnectionFactory
//            {
//                HostName = _settings.Host ?? "localhost",
//                Port = _settings.Port,
//                UserName = _settings.Username ?? "guest",
//                Password = _settings.Password ?? "guest",
//                VirtualHost = _settings.VirtualHost ?? "/",
//                AutomaticRecoveryEnabled = true
//            };

//            while (!stoppingToken.IsCancellationRequested)
//            {
//                try
//                {
//                    _connection = await factory.CreateConnectionAsync(stoppingToken);
//                    _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

//                    await ConfigureTopologyAsync();

//                    _logger.LogInformation("✅ RabbitMQ Connected: Listening on notification.queue");
//                    break;
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogWarning("❌ RabbitMQ Connection failed. Retrying in 5s...");
//                    await Task.Delay(5000, stoppingToken);
//                }
//            }
//        }

//        private async Task ConfigureTopologyAsync()
//        {
//            if (_channel == null) return;

//            // تعريف الـ Exchange
//            await _channel.ExchangeDeclareAsync(_settings.ExchangeName, ExchangeType.Topic, durable: true);

//            // صف واحد لكل شيء
//            await _channel.QueueDeclareAsync("notification.queue", durable: true, exclusive: false, autoDelete: false);

//            // ربط الصف بكل أنواع رسائل النوتفيكيشن
//            await _channel.QueueBindAsync("notification.queue", _settings.ExchangeName, "notification.*");

//            await _channel.BasicQosAsync(0, 1, false);
//        }

//        private async Task StartConsumerAsync(string queueName, Func<NotificationEventDto, Task> handler, CancellationToken stoppingToken)
//        {
//            var consumer = new AsyncEventingBasicConsumer(_channel!);
//            consumer.ReceivedAsync += async (model, ea) =>
//            {
//                try
//                {
//                    var body = ea.Body.ToArray();
//                    var json = Encoding.UTF8.GetString(body);
//                    var message = JsonSerializer.Deserialize<NotificationEventDto>(json,
//                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

//                    if (message != null)
//                        await handler(message);

//                    await _channel!.BasicAckAsync(ea.DeliveryTag, false);
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "Error processing message");
//                    await _channel!.BasicNackAsync(ea.DeliveryTag, false, requeue: true);
//                }
//            };

//            await _channel!.BasicConsumeAsync(queueName, autoAck: false, consumer: consumer);
//        }

//        private async Task HandleNotificationAsync(NotificationEventDto msg)
//        {
//            using var scope = _services.CreateScope();
//            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

//            var model = new NotificationModel
//            {
//                Id = Guid.NewGuid().ToString(),
//                Title = msg.Title,
//                Message = msg.Message,
//                ReceiverId = msg.ReceiverId,
//                SenderId = msg.SenderId,
//                PlanId = msg.PlanId,
//                CreatedAt = DateTime.UtcNow,
//                IsRead = false
//            };

//            await notificationService.SendNotificationAsync(model);
//        }

//        public override void Dispose()
//        {
//            _channel?.Dispose();
//            _connection?.Dispose();
//            base.Dispose();
//        }
//    }
//}