namespace ThreeTP.Payment.Application.Options
{
    public class AwsSecretManagerOptions
    {
        public int CacheDurationMinutes { get; set; } = 10;
        public string DefaultRegion { get; set; } = "us-east-1";
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
    }
}
