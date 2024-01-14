﻿
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;
using Web.Service.EmailAPI.Models.Dto;
using Web.Service.EmailAPI.Services;

namespace Web.Services.EmailAPI.Messaging
{
    public class RabbitMQCartConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private IConnection _connection;
        private IModel _channel;
        public RabbitMQCartConsumer(IConfiguration configuration, EmailService emailSerice)
        {
            _configuration = configuration;
            _emailService = emailSerice;
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Password = "guest",
                UserName = "guest"
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(_configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue"), false, false, false, null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(content);
                HandlerMessage(cartDto).GetAwaiter().GetResult();

                _channel.BasicAck(ea.DeliveryTag, false);
            };
            _channel.BasicConsume(_configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue"), false, consumer);

            return Task.CompletedTask;
        }

        private async Task HandlerMessage(CartDto cartDto)
        {
            _emailService.EmailCartAndLog(cartDto).GetAwaiter().GetResult();
        }
    }
}
