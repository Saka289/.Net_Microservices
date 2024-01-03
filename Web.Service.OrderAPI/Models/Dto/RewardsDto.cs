namespace Web.Service.OrderAPI.Models.Dto
{
    public class RewardsDto
    {
        public string UserId { get; set; }
        public int RewardsActivity { get; set; }
        public int OrderId { get; set; }
        public string Email { get; set; }
    }
}
