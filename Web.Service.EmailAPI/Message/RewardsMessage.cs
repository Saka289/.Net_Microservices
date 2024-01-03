namespace Web.Services.EmailAPI.Message
{
    public class RewardsMessage
    {
        public string UserId { get; set; }
        public int RewardsActivity { get; set; }
        public int OrderId { get; set; }
        public string Email { get; set; }
    }
}
