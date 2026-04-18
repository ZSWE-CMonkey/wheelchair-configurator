namespace WheelchairConfigurator.Pages;

public class PatientData
{
    public string PatientIdentificator { get; set; } = string.Empty;
    public double BodyHeight { get; set; } = 0;
    public double PelvisWidth { get; set; } = 0;
    public double ThighLength { get; set; } = 0;
    public double Weight { get; set; } = 0;
    public string BodyStability { get; set; } = string.Empty;
    public bool HeadStability { get; set; } = true;
    public string BedsoreRisk { get; set; } = string.Empty;
    public string Control { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public bool Legs { get; set; } = true;
    public string Pain { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Today;
}

public class ComponentMock
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
}

// Kategorie - přidej sem další podle potřeby
public static class ComponentCategories
{
    public static readonly List<string> All =
    [
        "Rám",
        "Motor",
        "Baterie",
        "Pohon",
        "Sedák",
        "Opěrka",
    ];
}