namespace WheelchairConfigurator.Data.DTOs;

public class CompatibilityRuleDto
{
    public string ComponentAName { get; set; } = string.Empty;
    public string ComponentBName { get; set; } = string.Empty;
    public bool IsCompatible { get; set; } = true;
}