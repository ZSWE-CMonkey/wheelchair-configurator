namespace ConfigurationLogic.DTOs;

// Frontend state evaluation request
public class ConfigurationStateRequestDto
{
    public UserProfileDto Profile { get; set; } = new();
    public List<int> SelectedComponentIds { get; set; } = new();
}
