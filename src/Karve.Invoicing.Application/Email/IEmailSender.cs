namespace Karve.Invoicing.Application.Email;

/// <summary>
/// Contract for sending outbound email messages.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email message.
    /// </summary>
    /// <param name="message">Message to send.</param>
    Task SendAsync(EmailMessage message);
}
