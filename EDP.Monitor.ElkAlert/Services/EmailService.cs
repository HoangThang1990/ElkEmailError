using System;
using MimeKit;
using MailKit.Net.Smtp;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class EmailService  : IEmailService
{
    private readonly EmailConfig _emailConfig;
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _logger = logger;
        _configuration = configuration;
        _emailConfig = _configuration.GetSection("EmailConfig").Get<EmailConfig>();
        if (_emailConfig == null) {
            throw new ArgumentNullException("_emailConfig","Chưa thiết lập email config");
        }else {
            _logger.LogInformation($"Email config {JsonConvert.SerializeObject(_emailConfig)}");
        }
    }

    public async Task SendEmailWithAttachmentAsync(string toEmail, string ccEmail, string subject, string body, MemoryStream fileStream = null)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("From", _emailConfig.SmtpUsername));
        if(string.IsNullOrEmpty(toEmail)){
            throw new ArgumentNullException("To email is required!");
        }

        message.To.Add(new MailboxAddress("To",toEmail));
        if (!string.IsNullOrEmpty(ccEmail)){
            message.Cc.Add(new MailboxAddress("To",ccEmail));
        }
        
        message.Subject = subject;

        var multipart = new Multipart("mixed");
        var textPart = new TextPart("html")
        {
            Text = body
        };

        multipart.Add(textPart);

        if(fileStream != null) {
            var attachment = new MimePart("application", "octet-stream")
            {
                Content = new MimeContent(fileStream, ContentEncoding.Default),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Binary,
                FileName = $"Error_{DateTime.Now.ToString(@"ddMMyyyyHHmm")}.csv"
            };
            multipart.Add(attachment);
        }
        
        message.Body = multipart;

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.SmtpPort);
            await client.AuthenticateAsync(_emailConfig.SmtpUsername, _emailConfig.SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
