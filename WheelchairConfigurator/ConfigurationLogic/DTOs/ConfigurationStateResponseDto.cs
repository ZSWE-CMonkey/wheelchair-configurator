namespace ConfigurationLogic.DTOs;

// Full frontend configuration state
public class ConfigurationStateResponseDto
{
	public ProfileRequirementsDto Requirements { get; set; } = new();
	public List<EvaluationIssueDto> Issues { get; set; } = new();
	public List<string> Recommendations { get; set; } = new();
	public List<int> EligibleComponentIds { get; set; } = new();
	public List<int> SelectedComponentIds { get; set; } = new();
	public List<ComponentStateDto> Components { get; set; } = new();
}
