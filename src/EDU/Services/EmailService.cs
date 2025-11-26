using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using EDU.Models;

namespace EDU.Services;

public class EmailService(IConfiguration configuration) : IEmailService
{
    public async Task SendAnalysisEmailAsync(IEnumerable<ImageTag> tags, string imageName)
    {
        var apiKey = configuration["SENDGRID_APIKEY"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("SendGrid API key not configured.");
        }

        var client = new SendGridClient(apiKey);

        var topTags = tags.Take(3).Select(t => t.Name).ToArray();
        var analysisSummary = $"The image '{imageName}' contains {string.Join(", ", topTags)}";

        var message = new SendGridMessage
        {
            From = new EmailAddress("<sender email>"),
            Subject = "Image Analysis Result",
            PlainTextContent = analysisSummary
        };
        message.AddTo("<receiver email>");

        var response = await client.SendEmailAsync(message);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to send email: {response.StatusCode}");
        }
    }
}