using System.Net;
using System.Net.Mail;
using Karve.Invoicing.Application.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Karve.Invoicing.Infrastructure.Services;

/// <summary>
/// Vendor-neutral SMTP email sender implementation.
/// </summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendAsync(EmailMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (string.IsNullOrWhiteSpace(message.To))
        {
            throw new ArgumentException("Recipient address is required.", nameof(message));
        }

        if (string.IsNullOrWhiteSpace(_options.SmtpHost))
        {
            throw new InvalidOperationException("Email SMTP host is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.FromAddress))
        {
            throw new InvalidOperationException("Email from address is not configured.");
        }

        using var smtpClient = new SmtpClient(_options.SmtpHost, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            smtpClient.Credentials = new NetworkCredential(_options.Username, _options.Password);
        }

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_options.FromAddress),
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = message.IsHtml
        };

        mailMessage.To.Add(message.To);

        try
        {
            await smtpClient.SendMailAsync(mailMessage).ConfigureAwait(false);
            _logger.LogInformation("Email sent successfully to {Recipient} with subject {Subject}.", message.To, message.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient} with subject {Subject}.", message.To, message.Subject);
            throw;
        }
    }
}
