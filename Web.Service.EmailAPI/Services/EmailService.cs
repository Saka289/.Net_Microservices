using Azure.Core;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Web.Service.EmailAPI.Models;
using Web.Service.EmailAPI.Models.Dto;
using Web.Services.EmailAPI.Data;
using Web.Services.EmailAPI.Message;

namespace Web.Service.EmailAPI.Services
{
    public class EmailService : IEmailService
    {
        private DbContextOptions<AppDbContext> _contextOptions;

        public EmailService(DbContextOptions<AppDbContext> contextOptions)
        {
            _contextOptions = contextOptions;
        }

        public async Task EmailCartAndLog(CartDto cartDto)
        {
            StringBuilder message = new StringBuilder();

            message.AppendLine("<br/>Cart Email Requested ");
            message.AppendLine("<br/>Total " + cartDto.CartHeader.CartTotal);
            message.Append("<br/>");
            message.Append("<ul>");
            foreach (var item in cartDto.CartDetails)
            {
                message.Append("<li>");
                message.Append(item.Product.Name + " x " + item.Count);
                message.Append("</li>");
            }
            message.Append("</ul>");

            await LogAndEmail(message.ToString(), cartDto.CartHeader.Email);
            await SendEmail(message.ToString(), cartDto.CartHeader.Email, "Shopping Cart");
        }

        public async Task LogOrderPlaced(RewardsMessage rewardsDto)
        {
            string message = "New Order Placed. <br/> Order ID : " + rewardsDto.OrderId;
            await LogAndEmail(message, rewardsDto.Email);
            await SendEmail(message, rewardsDto.Email, "New Order Placed");
        }

        public async Task RegisterUserEmailAndLog(string email)
        {
            string message = "User Registeration Successful. <br/> Email : " + email;
            await LogAndEmail(message, email);
            await SendEmail(message, email, "User Registeration Successful");
        }

        private async Task<bool> LogAndEmail(string message, string email)
        {
            try
            {
                EmailLogger emailLog = new()
                {
                    Email = email,
                    EmailSent = DateTime.Now,
                    Message = message
                };
                await using var _context = new AppDbContext(_contextOptions);
                await _context.EmailLoggers.AddAsync(emailLog);
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
