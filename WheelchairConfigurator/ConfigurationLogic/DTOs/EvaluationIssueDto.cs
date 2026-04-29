using ConfigurationLogic.Enums;

namespace ConfigurationLogic.DTOs;

// Single evaluation issue
public class EvaluationIssueDto
{
    public int ComponentId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public EvaluationIssueSeverity Severity { get; set; } = EvaluationIssueSeverity.Warning;
}

