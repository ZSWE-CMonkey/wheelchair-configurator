namespace WheelchairConfigurator.ServiceLayer.Models;

/// <summary>
/// UI model for a component.
/// UI never works directly with domain entities from DataLayer.
/// </summary>
public class ComponentModel
{
    /// <summary>Database ID of the component.</summary>
    public int Id { get; set; }

    /// <summary>Display name of the component.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Price of the component.</summary>
    public decimal Price { get; set; }

    /// <summary>Optional URL to the component catalog page.</summary>
    public string? CatalogUrl { get; set; }

    /// <summary>True if the engine recommends this component based on clinical rules.</summary>
    public bool IsRecommended { get; set; }

    /// <summary>True if this component cannot be selected with the current configuration.</summary>
    public bool IsIncompatible { get; set; }
}