using EDU.Models;

namespace EDU.Services;

public interface IEmailService
{
    Task SendAnalysisEmailAsync(IEnumerable<ImageTag> tags, string imageName);
}