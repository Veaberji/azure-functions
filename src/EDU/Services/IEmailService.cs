namespace EDU.Services;

public interface IEmailService
{
    Task SendAnalysisEmailAsync(IEnumerable<(string Name, float Confidence)> tags, string imageName);
}