using Web.Service.RewardAPI.Messaging;

namespace Web.Service.RewardAPI.Extension
{
    public static class ApplicationBuilderExtensions
    {
        private static IAzureServiceBusCosumer ServiceBusCosumer { get; set; }  

        public static IApplicationBuilder UseAzureServiceBusCosumer(this IApplicationBuilder app)
        {
            ServiceBusCosumer = app.ApplicationServices.GetService<IAzureServiceBusCosumer>();
            var hostApplicationLife = app.ApplicationServices.GetService<IHostApplicationLifetime>();

            hostApplicationLife.ApplicationStarted.Register(OnStart);
            hostApplicationLife.ApplicationStopping.Register(OnStop);

            return app;
        }

        private static void OnStop()
        {
            ServiceBusCosumer.Stop();
        }

        private static void OnStart()
        {
            ServiceBusCosumer.Start();
        }
    }
}
