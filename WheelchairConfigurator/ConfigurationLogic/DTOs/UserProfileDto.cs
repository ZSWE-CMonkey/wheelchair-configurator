using ConfigurationLogic.Enums;

namespace ConfigurationLogic.DTOs;

// Profile input data
public class UserProfileDto
{
    public int TrunkHeightCm { get; set; } // vyska trupu
    public int WeightKg { get; set; } // hmotnost
    public int PelvisWidthCm { get; set; } // sirka panve
    public int ThighLengthCm { get; set; } // delka stehna
    public TrunkStabilityLevel TrunkStability { get; set; } = TrunkStabilityLevel.Medium; //stabilita trupu
    public HeadControlLevel HeadControl { get; set; } = HeadControlLevel.Yes; // kontrola hlavy
    public PressureInjuryRiskLevel PressureInjuryRisk { get; set; } = PressureInjuryRiskLevel.Medium; // riziko dekubitu
    public SymptomSeverityLevel Pain { get; set; } = SymptomSeverityLevel.None; // bolest
    public SymptomSeverityLevel Fatigue { get; set; } = SymptomSeverityLevel.None; // unava
    public LowerLimbConditionLevel LowerLimbCondition { get; set; } = LowerLimbConditionLevel.None; // dolni koncetiny
    public HandFunctionLevel HandFunction { get; set; } = HandFunctionLevel.Full; // ovladani rukou
    public UsageEnvironment Environment { get; set; } = UsageEnvironment.Mixed; // prostredi pouziti
}

