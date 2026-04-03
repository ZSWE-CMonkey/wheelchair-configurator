using System.Collections.Generic;

namespace WheelchairConfigurator.Data.DTOs;

public class SeedDataDto
{
    public List<CategoryDto> Categories { get; set; } = new();
    public List<ComponentDto> Components { get; set; } = new();
    public List<CompatibilityRuleDto> Rules { get; set; } = new();
    public List<ComponentSpecsDto> Specs { get; set; } = new();
    public List<Model3DDto> Models3D { get; set; } = new();
}