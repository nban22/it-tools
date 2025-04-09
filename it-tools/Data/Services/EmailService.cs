// EmailService.cs

using MailKit.Net.Smtp;
using MimeKit;

namespace it_tools.Data.Services;
public class EmailService(IConfiguration configuration) : IEmailService
{
    private readonly IConfiguration _configuration = configuration;

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        using var client = new SmtpClient();
        client.Connect("smtp.gmail.com", 587, false);
        client.Authenticate(_configuration["Email:Username"], _configuration["Email:Password"]);
        
        var mailMessage = new MimeMessage();
        mailMessage.From.Add(new MailboxAddress("Your App", _configuration["Email:Username"]));
        mailMessage.To.Add(new MailboxAddress("", email));
        mailMessage.Subject = subject;
        mailMessage.Body = new TextPart("html") { Text = message };

        await client.SendAsync(mailMessage);
        await client.DisconnectAsync(true);
    }
}
