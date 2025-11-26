using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace EDU.Services;

public class EmailService(HttpClient httpClient, IConfiguration configuration) : IEmailService
{
    public async Task SendAnalysisEmailAsync(IEnumerable<(string Name, float Confidence)> tags, string imageName)
    {
        var apiKey = configuration["SENDGRID_APIKEY"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("SendGrid API key not configured.");
        }

        var topTags = tags.Take(3).Select(t => t.Name).ToArray();
        var analysisSummary = $"The image '{imageName}' contains {string.Join(", ", topTags)}";

        var emailMessage = new
        {
            personalizations = new[]
            {
                new
                {
                    to = new[]
                    {
                        new { email = "<receiver email>" }
                    }
                }
            },
            from = new { email = "<sender email>" },
            subject = "Image Analysis Result",
            content = new[]
            {
                new
                {
                    type = "text/plain",
                    value = analysisSummary
                }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send")
        {
            Headers = { { "Authorization", $"Bearer {apiKey}" } },
            Content = JsonContent.Create(emailMessage)
        };

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}