namespace Web.Services.OrderAPI.RabbitMQSender
{
    public interface IRabbitMQOrderMessageSender
    {
        void SendMessage(Object message, string exchangeName);
    }
}
