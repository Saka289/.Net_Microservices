using Azure.Core;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Web.Services.RewardAPI.Data;
using Web.Services.RewardAPI.Message;
using Web.Services.RewardAPI.Models;

namespace Web.Service.RewardAPI.Services
{
    public class RewardService : IRewardService
    {
        private DbContextOptions<AppDbContext> _contextOptions;

        public RewardService(DbContextOptions<AppDbContext> contextOptions)
        {
            _contextOptions = contextOptions;
        }


        public async Task<bool> UpdateRewards(RewardsMessage rewardsMessage)
        {
            try
            {
                Rewards rewards = new()
                {
                    OrderId = rewardsMessage.OrderId,
                    RewardsActivity = rewardsMessage.RewardsActivity,
                    UserId = rewardsMessage.UserId,
                    RewardsDate = DateTime.Now,
                };
                await using var _context = new AppDbContext(_contextOptions);
                await _context.Rewards.AddAsync(rewards);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<bool> SendEmail(string htmlMessage, string email, string subject)
        {
            try
            {
                var emailToSend = new MimeMessage();
                emailToSend.From.Add(MailboxAddress.Parse("Hello"));
                emailToSend.To.Add(MailboxAddress.Parse(email));
                emailToSend.Subject = subject;
                emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = htmlMessage
                };

                using (var emailClient = new SmtpClient())
                {
                    await emailClient.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    await emailClient.AuthenticateAsync("sakanatsuma289@gmail.com", "icni xdjv imww gshb");
                    await emailClient.SendAsync(emailToSend);
                    await emailClient.DisconnectAsync(true);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
