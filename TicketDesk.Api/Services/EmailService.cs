
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace TicketDesk.Api.Services;

public class EmailSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "TicketDesk 2.0";
}

public interface IEmailService
{
    Task EnviarAsync(
        string destinatario,
        string asunto,
        string mensajeHtml,
        CancellationToken cancellationToken = default);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task EnviarAsync(
        string destinatario,
        string asunto,
        string mensajeHtml,
        CancellationToken cancellationToken = default)
    {
        var mensaje = new MimeMessage();

        mensaje.From.Add(
            new MailboxAddress(
                _settings.FromName,
                _settings.FromEmail));

        mensaje.To.Add(
            MailboxAddress.Parse(destinatario));

        mensaje.Subject = asunto;

        mensaje.Body = new BodyBuilder
        {
            HtmlBody = mensajeHtml
        }.ToMessageBody();

        using var cliente = new SmtpClient();

        await cliente.ConnectAsync(
            _settings.Host,
            _settings.Port,
            SecureSocketOptions.StartTls,
            cancellationToken);

        await cliente.AuthenticateAsync(
            _settings.Username,
            _settings.Password,
            cancellationToken);

        await cliente.SendAsync(mensaje, cancellationToken);

        await cliente.DisconnectAsync(
            true,
            cancellationToken);
    }
}
