namespace Karve.Invoicing.Application.Email;

/// <summary>
/// SMTP configuration values for outbound email.
/// </summary>
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromAddress { get; set; } = string.Empty;

    public bool EnableSsl { get; set; } = true;
}
