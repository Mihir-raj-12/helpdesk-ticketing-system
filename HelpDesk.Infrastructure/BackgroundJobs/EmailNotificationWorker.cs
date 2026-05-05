using HelpDesk.Infrastructure.Repositories.Implementations.Service;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;

namespace HelpDesk.Infrastructure.BackgroundJobs
{
    public class EmailNotificationWorker : BackgroundService
    {
        private readonly IEmailQueue _emailQueue;
        private readonly IConfiguration _config;
        private readonly ILogger<EmailNotificationWorker> _logger;

        // Notice we inject the Queue directly! No IServiceProvider needed here 
        // because the Queue is a Singleton, just like this Worker.
        public EmailNotificationWorker(IEmailQueue emailQueue, ILogger<EmailNotificationWorker> logger , IConfiguration config)
        {
            _emailQueue = emailQueue;
            _logger = logger;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Notification Engine started. Waiting for emails...");

            // This loop runs forever. 
            while (!stoppingToken.IsCancellationRequested)
            {
                // 1. Wait for an email to arrive. 
                // Unlike a PeriodicTimer, this doesn't wake up every minute. 
                // It goes completely to sleep (using 0% CPU) until an email enters the queue!
                var email = await _emailQueue.DequeueEmailAsync(stoppingToken);

                // 2. We got an email! Let's process it.
                _logger.LogWarning($"[SENDING EMAIL] To: {email.To} | Subject: {email.Subject}");

                int maxRetries = 3;
                bool emailSent = false;

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress("HelpDesk System", _config["Smtp:Username"]));
                        message.To.Add(new MailboxAddress("User", email.To));
                        message.Subject = email.Subject;
                        message.Body = new TextPart("plain") { Text = email.Body };

                        using var client = new SmtpClient();
                        await client.ConnectAsync(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]), SecureSocketOptions.StartTls, stoppingToken);
                        await client.AuthenticateAsync(_config["Smtp:Username"], _config["Smtp:Password"], stoppingToken);
                        await client.SendAsync(message, stoppingToken);
                        await client.DisconnectAsync(true, stoppingToken);

                        _logger.LogInformation($"[EMAIL SENT SUCCESSFULLY] To: {email.To} on attempt {attempt}");

                        emailSent = true;
                        break; // SUCCESS! Break out of the retry loop.
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[EMAIL FAILED] Attempt {attempt} of {maxRetries} to {email.To}. Error: {ex.Message}");

                        if (attempt < maxRetries)
                        {
                            _logger.LogInformation($"Waiting 5 seconds before retrying...");
                            await Task.Delay(5000, stoppingToken); // Wait 5 seconds before the next loop
                        }
                    }
                }
                if (!emailSent)
                {
                    // If it failed 3 times, we log a critical error. 
                    // In a massive enterprise app, we might write this failed email to a "Dead Letter" database table here.
                    _logger.LogCritical($"[EMAIL PERMANENTLY FAILED] Could not send to {email.To} after {maxRetries} attempts.");
                }
            }
        }
    }
}
