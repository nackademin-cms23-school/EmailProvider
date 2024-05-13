using Azure;
using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using EmailProvider.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EmailProvider.Services;

public class EmailSenderService(ILogger<EmailSenderService> logger, EmailClient emailClient) : IEmailSenderService
{
    private readonly ILogger<EmailSenderService> _logger = logger;
    private readonly EmailClient _emailClient = emailClient;

    public EmailRequest UnpackEmailRequest(ServiceBusReceivedMessage message)
    {
        try
        {
            var emailRequest = JsonConvert.DeserializeObject<EmailRequest>(message.Body.ToString());
            if (emailRequest != null)
            {
                return emailRequest;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailSenderService.UnpackEmailRequest() :: {ex.Message}");
        }
        return null!;
    }

    public bool SendEmail(EmailRequest emailRequest)
    {
        try
        {
            var result = _emailClient.Send(
                WaitUntil.Completed,
                senderAddress: Environment.GetEnvironmentVariable("SenderAddress"),
                recipientAddress: emailRequest.To,
                subject: emailRequest.Subject,
                htmlContent: emailRequest.Body,
                plainTextContent: emailRequest.PlainText);

            if (result.HasCompleted)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailSenderService.UnpackEmailRequest() :: {ex.Message}");
        }
        return false;
    }
}
