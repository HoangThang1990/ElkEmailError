using System;
using MimeKit;
using MailKit.Net.Smtp;
using System.IO;
using System.Threading.Tasks;

public class EmailService
{
    private static string smtpServer = "smtp.gmail.com";
    private static int smtpPort = 587;
    public static string smtpUsername = "dangkythanhvien90@gmail.com";
    private static string smtpPassword = "pkts edpi tjft fewc";

    public static async Task SendEmailWithAttachmentAsync(string toEmail, string ccEmail, string subject, string body, MemoryStream fileStream = null)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("From", EmailService.smtpUsername));
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
            await client.ConnectAsync(smtpServer, smtpPort);
            await client.AuthenticateAsync(smtpUsername, smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
