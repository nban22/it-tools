namespace it_tools.Data.Services;
public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string message);
}
