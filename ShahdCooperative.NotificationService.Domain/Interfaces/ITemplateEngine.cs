namespace ShahdCooperative.NotificationService.Domain.Interfaces;

public interface ITemplateEngine
{
    Task<string> ProcessTemplateAsync(string templateKey, string templateData, CancellationToken cancellationToken = default);
    string ReplaceTokens(string template, Dictionary<string, string> tokens);
}
