﻿
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;
using Web.Service.EmailAPI.Models.Dto;
using Web.Service.EmailAPI.Services;
using Web.Services.EmailAPI.Message;

namespace Web.Services.EmailAPI.Messaging
{
    public class RabbitMQOrderConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private IConnection _connection;
        private IModel _channel;
        private string ExchangeName = string.Empty;
        private const string OrderCreated_EmailUpdateQueue = "EmailUpdateQueue";

        public RabbitMQOrderConsumer(IConfiguration configuration, EmailService emailSerice)
        {
            _configuration = configuration;
            _emailService = emailSerice;
            ExchangeName = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Password = "guest",
                UserName = "guest"
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct);
            _channel.QueueDeclare(OrderCreated_EmailUpdateQueue, false, false, false, null);
            _channel.QueueBind(OrderCreated_EmailUpdateQueue, ExchangeName, "EmailUpdate");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                RewardsMessage rewardsMessage = JsonConvert.DeserializeObject<RewardsMessage>(content);
                HandlerMessage(rewardsMessage).GetAwaiter().GetResult();

                _channel.BasicAck(ea.DeliveryTag, false);
            };
            _channel.BasicConsume(OrderCreated_EmailUpdateQueue, false, consumer);

            return Task.CompletedTask;
        }

        private async Task HandlerMessage(RewardsMessage rewardsMessage)
        {
            _emailService.LogOrderPlaced(rewardsMessage).GetAwaiter().GetResult();
        }
    }
}