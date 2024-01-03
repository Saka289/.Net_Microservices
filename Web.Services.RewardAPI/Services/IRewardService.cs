using Web.Services.RewardAPI.Message;

namespace Web.Service.RewardAPI.Services
{
    public interface IRewardService
    {
        Task<bool> UpdateRewards(RewardsMessage rewardsMessage);
    }
}
