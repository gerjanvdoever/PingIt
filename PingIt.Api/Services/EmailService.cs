using MailKit.Net.Smtp;
using MimeKit;
using DotNetEnv;

namespace PingIt.Api.Services
{
    public class EmailService
    {
        private readonly string _server;
        private readonly int _port;
        private readonly string _email;
        private readonly string _password;
        private readonly bool _useSsl;

        public EmailService()
        {
            Env.Load();

            _server = Env.GetString("SMTP_SERVER") ?? throw new Exception("SMTP_SERVER missing");
            _port = int.Parse(Env.GetString("SMTP_PORT") ?? throw new Exception("SMTP_PORT missing"));
            _email = Env.GetString("SMTP_EMAIL") ?? throw new Exception("SMTP_EMAIL missing");
            _password = Env.GetString("SMTP_PASSWORD") ?? throw new Exception("SMTP_PASSWORD missing");
            _useSsl = bool.Parse(Env.GetString("SMTP_SSL") ?? "true");
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("PingIt", _email));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            message.Body = new TextPart("plain")
            {
                Text = body
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(_server, _port, _useSsl);
            await client.AuthenticateAsync(_email, _password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
