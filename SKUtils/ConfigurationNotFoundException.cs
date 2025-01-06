using System;

namespace SKUtils;

public class ConfigurationNotFoundException : Exception
{
    public string? Section { get; set; }
    public string? Key { get; set; }

    public ConfigurationNotFoundException()
        : base() { }

    public ConfigurationNotFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }

    public ConfigurationNotFoundException(string section)
        : base($"Configuration section '{section}' not found")
    {
        this.Section = section;
    }

    public ConfigurationNotFoundException(string section, string key)
        : base($"Configuration key '{section}:{key}' not found")
    {
        this.Section = section;
        this.Key = key;
    }
}
