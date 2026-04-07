namespace WheelchairConfigurator.ServiceLayer.Models;

/// <summary>
/// Represents the result of a configuration operation returned to UI.
/// </summary>
public class ConfigurationResult
{
    /// <summary>Whether the operation was successful.</summary>
    public bool IsSuccess { get; set; }

    /// <summary>Human-readable message describing the result.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>ID of the saved configuration — only set on success.</summary>
    public int? ConfigurationId { get; set; }
}