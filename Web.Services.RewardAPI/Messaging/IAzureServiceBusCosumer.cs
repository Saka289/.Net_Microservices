namespace Web.Service.RewardAPI.Messaging
{
    public interface IAzureServiceBusCosumer
    {
        Task Start();
        Task Stop();
    }
}
