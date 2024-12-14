using Microsoft.Extensions.Configuration;

namespace BaseSKLearn.Utils;

public static class ConfigExtensions
{
    public static T FromSecretsConfig<T>(string sectionName) =>
        new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build()
            .GetSection(sectionName)
            .Get<T>()
        ?? throw new InvalidDataException("Invalid semantic kernel configuration is empty");

    public static T FromJsonConfig<T>(string jsonPath, string sectionName) =>
        new ConfigurationBuilder().AddJsonFile(jsonPath).Build().GetSection(sectionName).Get<T>()
        ?? throw new InvalidDataException("Invalid semantic kernel configuration is empty");
}
