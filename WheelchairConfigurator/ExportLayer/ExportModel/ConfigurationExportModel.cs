namespace WheelchairConfigurator.Export.ExportModel;

/// <summary>
/// Represents the complete data needed to generate an export document.
/// Populated by ExportService from the database and passed to the builder.
/// </summary>
public class ConfigurationExportModel
{
    /// <summary>Display name of the configuration.</summary>
    public string ConfigurationName { get; set; } = string.Empty;

    /// <summary>Full name of the specialist who created the configuration.</summary>
    public string SpecialistName { get; set; } = string.Empty;

    /// <summary>Patient name embedded at save time.</summary>
    public string PatientName { get; set; } = string.Empty;

    /// <summary>Patient birth number embedded at save time.</summary>
    public string PatientBirthNumber { get; set; } = string.Empty;

    /// <summary>Date and time when the configuration was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>List of selected components grouped by category.</summary>
    public List<ConfigurationExportItem> Items { get; set; } = new();

    /// <summary>Total price of all selected components.</summary>
    public decimal TotalPrice { get; set; }
}

/// <summary>
/// Represents a single component entry in the export document.
/// </summary>
public class ConfigurationExportItem
{
    /// <summary>Name of the component category (e.g. Wheels, Frame).</summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>Name of the selected component.</summary>
    public string ComponentName { get; set; } = string.Empty;

    /// <summary>Price of the component.</summary>
    public decimal Price { get; set; }

    /// <summary>Quantity of the component (default is 1).</summary>
    public int Quantity { get; set; } = 1;

    public string ItemCode { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;
    public string ManufacturerCode { get; set; } = string.Empty;
}