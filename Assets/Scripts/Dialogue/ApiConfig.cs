using System;

public class ApiConfig
{
    public const string ApiKeyEnvironmentVariable = "OPENAI_API_KEY";

    public string ApiKey { get; }
    public bool HasApiKey => !string.IsNullOrWhiteSpace(ApiKey);

    public ApiConfig()
    {
        ApiKey = Environment.GetEnvironmentVariable(ApiKeyEnvironmentVariable);
    }
}
