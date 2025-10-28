using Microsoft.Extensions.Logging;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ShahdCooperative.NotificationService.Infrastructure.Services;

public class TemplateEngine : ITemplateEngine
{
    private readonly ILogger<TemplateEngine> _logger;
    private readonly INotificationTemplateRepository _templateRepository;

    public TemplateEngine(
        ILogger<TemplateEngine> logger,
        INotificationTemplateRepository templateRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
    }

    public async Task<string> ProcessTemplateAsync(string templateKey, string templateData, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _templateRepository.GetByKeyAsync(templateKey, cancellationToken);

            if (template == null)
            {
                _logger.LogWarning("Template with key '{TemplateKey}' not found", templateKey);
                return string.Empty;
            }

            if (!template.IsActive)
            {
                _logger.LogWarning("Template with key '{TemplateKey}' is inactive", templateKey);
                return string.Empty;
            }

            var tokens = ParseTemplateData(templateData);
            var processedBody = ReplaceTokens(template.BodyTemplate, tokens);

            return processedBody;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing template '{TemplateKey}'", templateKey);
            return string.Empty;
        }
    }

    public string ReplaceTokens(string template, Dictionary<string, string> tokens)
    {
        if (string.IsNullOrEmpty(template))
        {
            return string.Empty;
        }

        var result = template;

        foreach (var token in tokens)
        {
            var placeholder = $"{{{{{token.Key}}}}}";
            result = result.Replace(placeholder, token.Value ?? string.Empty);
        }

        // Remove any remaining unreplaced placeholders
        result = Regex.Replace(result, @"\{\{[^}]+\}\}", string.Empty);

        return result;
    }

    private Dictionary<string, string> ParseTemplateData(string templateData)
    {
        var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(templateData))
        {
            return tokens;
        }

        try
        {
            using var document = JsonDocument.Parse(templateData);
            var root = document.RootElement;

            foreach (var property in root.EnumerateObject())
            {
                tokens[property.Name] = property.Value.ToString();
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse template data as JSON: {TemplateData}", templateData);
        }

        return tokens;
    }
}
