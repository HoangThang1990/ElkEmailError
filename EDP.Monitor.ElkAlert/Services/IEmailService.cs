public interface IEmailService
{
    Task SendEmailWithAttachmentAsync(string toEmail, string ccEmail, string subject, string body, MemoryStream fileStream = null);
}