namespace ConfigurationLogic.DTOs;

// Frontend component state
public class ComponentStateDto
{
    public ComponentOutputDto Component { get; set; } = new();
    public bool IsSelected { get; set; }
    public bool IsEnabled { get; set; }
    public List<string> DisableReasons { get; set; } = new();
}
