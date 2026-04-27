namespace ConfigurationLogic.DTOs;

// Evaluation response payload
public class ProfileEvaluationResultDto
{
    public ProfileRequirementsDto Requirements { get; set; } = new();
    public List<ComponentOutputDto> EligibleComponents { get; set; } = new();
    public List<EvaluationIssueDto> Issues { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

