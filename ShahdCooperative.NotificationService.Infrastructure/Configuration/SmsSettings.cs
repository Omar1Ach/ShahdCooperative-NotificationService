namespace ShahdCooperative.NotificationService.Infrastructure.Configuration;

public class SmsSettings
{
    public string Provider { get; set; } = "Twilio";
    public string TwilioAccountSid { get; set; } = string.Empty;
    public string TwilioAuthToken { get; set; } = string.Empty;
    public string TwilioPhoneNumber { get; set; } = string.Empty;
    public string VonageApiKey { get; set; } = string.Empty;
    public string VonageApiSecret { get; set; } = string.Empty;
    public string VonageFromNumber { get; set; } = string.Empty;
}
