namespace Web.Service.EmailAPI.Messaging
{
    public interface IAzureServiceBusCosumer
    {
        Task Start();
        Task Stop();
    }
}
