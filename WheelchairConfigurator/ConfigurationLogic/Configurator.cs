using WheelchairConfigurator.Data.DTOs;
using WheelchairConfigurator.Data.Repositories;
using WheelchairConfigurator.Domain.Models;
using ConfigurationLogic.DTOs;
using ConfigurationLogic.Enums;

namespace ConfigurationLogic;

// Profile evaluation logic
public class Configurator
{
	private readonly Catalog _catalog;
	private readonly CompatibilityRuleRepository _compatibilityRuleRepository;

	// Stable category roles from database RoleKey.
	private enum ComponentCategoryRole
	{
		Unknown,
		Chassis,
		Wheel,
		Drive,
		Battery,
		Seat,
		Backrest,
		HeadSupport,
		Control,
		LegSupport
	}

	// Initialize evaluator dependencies
	public Configurator(Catalog catalog, CompatibilityRuleRepository compatibilityRuleRepository)
	{
		_catalog = catalog;
		_compatibilityRuleRepository = compatibilityRuleRepository;
	}

	// Evaluate profile against components
	public async Task<ProfileEvaluationResultDto> EvaluateProfileAsync(UserProfileDto profile)
	{
		var result = new ProfileEvaluationResultDto
		{
			Requirements = BuildRequirements(profile)
		};

		AddGeneralRecommendations(result, profile);

		var components = await _catalog.GetAllComponentEntitiesAsync();
		var specsByComponentId = new Dictionary<int, ComponentSpecsDto?>(components.Count);
		var outputByComponentId = new Dictionary<int, ComponentOutputDto>(components.Count);

		foreach (var component in components)
		{
			specsByComponentId[component.Id] = await _catalog.GetComponentDetailAsync(component.Id);
			outputByComponentId[component.Id] = await _catalog.ToComponentOutputDtoAsync(component);
		}

		foreach (var component in components)
		{
			specsByComponentId.TryGetValue(component.Id, out var specs);
			var output = outputByComponentId[component.Id];
			var role = ParseRole(output.CategoryRoleKey);
			var issues = new List<EvaluationIssueDto>();

			EvaluateDeterministicConstraints(profile, result.Requirements, component, role, specs, issues);
			result.Issues.AddRange(issues);

			// Hard fail only on critical rules; warnings are informational.
			if (!issues.Any(i => i.Severity == EvaluationIssueSeverity.Critical))
			{
				result.EligibleComponents.Add(output);
			}
		}

		return result;
	}

	// Check pair compatibility
	public Task<bool?> CheckCompatibilityAsync(int componentAId, int componentBId)
	{
		return _compatibilityRuleRepository.AreCompatibleAsync(componentAId, componentBId);
	}

	// Build requirement model
	private static ProfileRequirementsDto BuildRequirements(UserProfileDto profile)
	{
		var requirements = new ProfileRequirementsDto
		{
			MinimumWeightCapacityKg = Math.Max(profile.WeightKg, 0),
			MinimumSeatWidthCm = profile.PelvisWidthCm > 0 ? profile.PelvisWidthCm : null,
			MaximumSeatWidthCm = profile.PelvisWidthCm > 0 ? profile.PelvisWidthCm + 3 : null,
			MinimumSeatDepthCm = profile.ThighLengthCm > 0 ? Math.Max(profile.ThighLengthCm - 3, 0) : null,
			MaximumSeatDepthCm = profile.ThighLengthCm > 0 ? profile.ThighLengthCm + 2 : null,
			MinimumDrivePowerLevel = GetRequiredDrivePowerLevel(profile.WeightKg),
			MinimumPressureReliefLevel = GetRequiredPressureReliefLevel(profile),
			MinimumComfortLevel = GetRequiredComfortLevel(profile),
			BackrestHeightRecommendation = GetBackrestHeightRecommendation(profile),
			ChassisRecommendation = GetChassisRecommendation(profile),
			NeedsHeadrest = profile.HeadControl == HeadControlLevel.No,
			NeedsTilt = profile.TrunkStability == TrunkStabilityLevel.Poor || profile.PressureInjuryRisk == PressureInjuryRiskLevel.High,
			NeedsLateralSupports = profile.TrunkStability == TrunkStabilityLevel.Poor,
			NeedsAlternativeControl = profile.HandFunction == HandFunctionLevel.None,
			NeedsPressureRelief = profile.PressureInjuryRisk == PressureInjuryRiskLevel.High || profile.Pain == SymptomSeverityLevel.High || profile.Fatigue == SymptomSeverityLevel.High,
			NeedsLegSupportAdaptation = profile.LowerLimbCondition != LowerLimbConditionLevel.None
		};

		return requirements;
	}

	// Add human-readable tips
	private static void AddGeneralRecommendations(ProfileEvaluationResultDto result, UserProfileDto profile)
	{
		AddRecommendation(result.Recommendations, $"Doporučená šířka sedu: {DescribeSeatWidth(profile)}.");
		AddRecommendation(result.Recommendations, $"Doporučená hloubka sedu: {DescribeSeatDepth(profile)}.");
		AddRecommendation(result.Recommendations, profile.TrunkHeightCm > 0
			? $"Výška trupu {profile.TrunkHeightCm} cm: {GetBackrestHeightRecommendation(profile)}."
			: $"Doporučení pro opěradlo: {GetBackrestHeightRecommendation(profile)}.");
		AddRecommendation(result.Recommendations, $"Doporučení podvozku: {GetChassisRecommendation(profile)}.");
	}

	// Compute seat width range
	private static string DescribeSeatWidth(UserProfileDto profile)
	{
		if (profile.PelvisWidthCm <= 0)
		{
			return "Nelze určit bez šířky pánve";
		}

		return $"{profile.PelvisWidthCm}-{profile.PelvisWidthCm + 3} cm";
	}

	// Compute seat depth range
	private static string DescribeSeatDepth(UserProfileDto profile)
	{
		if (profile.ThighLengthCm <= 0)
		{
			return "Nelze určit bez délky stehna";
		}

		var min = Math.Max(profile.ThighLengthCm - 3, 0);
		var max = profile.ThighLengthCm + 2;
		return $"{min}-{max} cm";
	}

	// Pick backrest height
	private static string GetBackrestHeightRecommendation(UserProfileDto profile)
	{
		if (profile.HeadControl == HeadControlLevel.No || profile.TrunkStability == TrunkStabilityLevel.Poor)
		{
			return "Vyšší opěradlo";
		}

		if (profile.TrunkHeightCm >= 60)
		{
			return "Vyšší opěradlo";
		}

		if (profile.TrunkHeightCm >= 50)
		{
			return "Střední výška opěradla";
		}

		if (profile.TrunkStability == TrunkStabilityLevel.Medium)
		{
			return "Střední výška opěradla";
		}

		return "Nižší až střední opěradlo";
	}

	// Pick chassis type
	private static string GetChassisRecommendation(UserProfileDto profile)
	{
		return profile.Environment switch
		{
			UsageEnvironment.Indoor => "Kompaktní podvozek s vysokou manévrovatelností",
			UsageEnvironment.Outdoor => "Robustní podvozek s většími koly a lepším odpružením",
			_ => "Vyvážený podvozek pro indoor i outdoor použití"
		};
	}

	// Deterministic checks based on profile, role and structured specs.
	private static void EvaluateDeterministicConstraints(
		UserProfileDto profile,
		ProfileRequirementsDto requirements,
		Component component,
		ComponentCategoryRole role,
		ComponentSpecsDto? specs,
		List<EvaluationIssueDto> issues)
	{
		if (specs is null)
		{
			issues.Add(CreateIssue(component, "specs_missing", "Komponenta nemá technické specifikace potřebné pro vyhodnocení.", EvaluationIssueSeverity.Warning));
			return;
		}

		switch (role)
		{
			case ComponentCategoryRole.Chassis:
				CheckWeightCapacity(profile, component, specs, issues);
				CheckSeatWidth(requirements, component, specs, issues);
				CheckSeatDepth(requirements, component, specs, issues);
				CheckComfort(requirements, component, specs, issues);
				CheckEnvironment(profile, component, specs, issues);
				break;

			case ComponentCategoryRole.Wheel:
				// Wheel rows in current dataset usually do not carry explicit capacity.
				CheckWeightCapacity(profile, component, specs, issues, requireValue: false);
				CheckEnvironment(profile, component, specs, issues);
				break;

			case ComponentCategoryRole.Drive:
			case ComponentCategoryRole.Battery:
				CheckDrivePower(requirements, component, specs, issues);
				CheckWeightCapacity(profile, component, specs, issues, requireValue: false);
				CheckEnvironment(profile, component, specs, issues);
				break;

			case ComponentCategoryRole.Seat:
				CheckSeatWidth(requirements, component, specs, issues);
				CheckSeatDepth(requirements, component, specs, issues);
				CheckPressureRelief(requirements, component, specs, issues);
				CheckComfort(requirements, component, specs, issues);
				break;

			case ComponentCategoryRole.Backrest:
				CheckBackrestHeight(profile, component, specs, issues);
				CheckTrunkSupport(profile, component, specs, issues);
				CheckComfort(requirements, component, specs, issues);
				break;

			case ComponentCategoryRole.HeadSupport:
				CheckHeadSupport(requirements, component, specs, issues);
				break;

			case ComponentCategoryRole.Control:
				CheckControl(profile, component, specs, issues);
				break;

			case ComponentCategoryRole.LegSupport:
				CheckLegSupport(profile, component, specs, issues);
				break;

			default:
				// Fallback safety check for unknown role.
				CheckWeightCapacity(profile, component, specs, issues);
				break;
		}
	}

	// Check patient weight vs component capacity.
	private static void CheckWeightCapacity(
		UserProfileDto profile,
		Component component,
		ComponentSpecsDto specs,
		List<EvaluationIssueDto> issues,
		bool requireValue = true)
	{
		if (profile.WeightKg <= 0)
		{
			return;
		}

		if (!specs.WeightCapacityKg.HasValue || specs.WeightCapacityKg.Value <= 0)
		{
			if (requireValue)
			{
				issues.Add(CreateIssue(component, "weight_capacity_missing", "Komponenta nemá vyplněnou nosnost pro kontrolu hmotnosti uživatele.", EvaluationIssueSeverity.Warning));
			}
			return;
		}

		if (profile.WeightKg > specs.WeightCapacityKg.Value)
		{
			issues.Add(CreateIssue(component, "weight_capacity", "Hmotnost uživatele překročila nosnost komponenty.", EvaluationIssueSeverity.Critical));
		}
	}

	// Check seat width range.
	private static void CheckSeatWidth(ProfileRequirementsDto requirements, Component component, ComponentSpecsDto specs, List<EvaluationIssueDto> issues)
	{
		if (!requirements.MinimumSeatWidthCm.HasValue || !requirements.MaximumSeatWidthCm.HasValue)
		{
			return;
		}

		if (!specs.SeatWidthCm.HasValue || specs.SeatWidthCm.Value <= 0)
		{
			issues.Add(CreateIssue(component, "seat_width_missing", "Komponenta nemá vyplněnou šířku sedu pro antropometrickou kontrolu.", EvaluationIssueSeverity.Warning));
			return;
		}

		if (specs.SeatWidthCm.Value < requirements.MinimumSeatWidthCm.Value || specs.SeatWidthCm.Value > requirements.MaximumSeatWidthCm.Value)
		{
			issues.Add(CreateIssue(component, "seat_width", "Šířka sedu neodpovídá požadovanému rozsahu podle šířky pánve.", EvaluationIssueSeverity.Critical));
		}
	}

	// Check seat depth range.
	private static void CheckSeatDepth(ProfileRequirementsDto requirements, Component component, ComponentSpecsDto specs, List<EvaluationIssueDto> issues)
	{
		if (!requirements.MinimumSeatDepthCm.HasValue || !requirements.MaximumSeatDepthCm.HasValue)
		{
			return;
		}

		if (!specs.SeatDepthCm.HasValue || specs.SeatDepthCm.Value <= 0)
		{
			issues.Add(CreateIssue(component, "seat_depth_missing", "Komponenta nemá vyplněnou hloubku sedu pro antropometrickou kontrolu.", EvaluationIssueSeverity.Warning));
			return;
		}

		if (specs.SeatDepthCm.Value < requirements.MinimumSeatDepthCm.Value || specs.SeatDepthCm.Value > requirements.MaximumSeatDepthCm.Value)
		{
			issues.Add(CreateIssue(component, "seat_depth", "Hloubka sedu neodpovídá požadovanému rozsahu podle délky stehna.", EvaluationIssueSeverity.Critical));
		}
	}

	// Check backrest height level.
	private static void CheckBackrestHeight(UserProfileDto profile, Component component, ComponentSpecsDto specs, List<EvaluationIssueDto> issues)
	{
		var requiredLevel = GetRequiredBackrestHeightLevel(profile);
		if (requiredLevel <= 0)
		{
			return;
		}

		if (!specs.BackrestHeightLevel.HasValue || specs.BackrestHeightLevel.Value <= 0)
		{
			issues.Add(CreateIssue(component, "backrest_height_missing", "Komponenta nemá vyplněnou úroveň výšky opěradla.", EvaluationIssueSeverity.Warning));
			return;
		}

		if (specs.BackrestHeightLevel.Value < requiredLevel)
		{
			issues.Add(CreateIssue(component, "backrest_height", "Výška opěradla není dostatečná pro zadaný profil.", EvaluationIssueSeverity.Critical));
		}
	}

	// Check trunk support features.
	private static void CheckTrunkSupport(UserProfileDto profile, Component component, ComponentSpecsDto specs, List<EvaluationIssueDto> issues)
	{
		if (profile.TrunkStability != TrunkStabilityLevel.Poor)
		{
			return;
		}

		if (!specs.SupportsTilt.HasValue)
		{
			issues.Add(CreateIssue(component, "tilt_missing", "Komponenta nemá vyplněno, zda podporuje funkci tilt.", EvaluationIssueSeverity.Warning));
		}
		else if (!specs.SupportsTilt.Value)
		{
			issues.Add(CreateIssue(component, "tilt_required", "Profil vyžaduje funkci tilt kvůli nízké stabilitě trupu.", EvaluationIssueSeverity.Critical));
		}

		if (!specs.SupportsLateralSupport.HasValue)
		{
			issues.Add(CreateIssue(component, "lateral_support_missing", "Komponenta nemá vyplněno, zda podporuje laterální opory.", EvaluationIssueSeverity.Warning));
		}
		else if (!specs.SupportsLateralSupport.Value)
		{
			issues.Add(CreateIssue(component, "lateral_support_required", "Profil vyžaduje laterální opory kvůli nízké stabilitě trupu.", EvaluationIssueSeverity.Critical));
		}
	}

	// Check head support requirement.
	private static void CheckHeadSupport(ProfileRequirementsDto requirements, Component component, ComponentSpecsDto specs, List<EvaluationIssueDto> issues)
	{
		if (!requirements.NeedsHeadrest)
		{
			return;
		}

		if (!specs.HasHeadSupport.HasValue)
		{
			issues.Add(CreateIssue(component, "head_support_missing", "Komponenta nemá vyplněno, zda obsahuje opěrku hlavy.", EvaluationIssueSeverity.Warning));
		}
		else if (!specs.HasHeadSupport.Value)
		{
			issues.Add(CreateIssue(component, "head_support_required", "Profil bez kontroly hlavy vyžaduje opěrku hlavy.", EvaluationIssueSeverity.Critical));
		}
	}

	// Check pressure relief and positioning.
	private static void CheckPressureRelief(ProfileRequirementsDto requirements, Component component, ComponentSpecsDto specs, List<EvaluationIssueDto> issues)
	{
		if (requirements.MinimumPressureReliefLevel > 0 && (!specs.PressureReliefLevel.HasValue || specs.PressureReliefLevel.Value <= 0))
		{
			issues.Add(CreateIssue(component, "pressure_relief_missing", "Komponenta nemá vyplněnou úroveň antidekubitní ochrany.", EvaluationIssueSeverity.Warning));
			return;
		}

		if (specs.PressureReliefLevel.HasValue && specs.PressureReliefLevel.Value < requirements.MinimumPressureReliefLevel)
		{
			issues.Add(CreateIssue(component, "pressure_relief", "Antidekubitní ochrana komponenty je pro profil nedostatečná.", EvaluationIssueSeverity.Critical));
		}

		if (requirements.NeedsTilt)
		{
			var tiltKnown = specs.SupportsTilt.HasValue;
			var reclineKnown = specs.SupportsRecline.HasValue;
			var hasTiltOrRecline = specs.SupportsTilt == true || specs.SupportsRecline == true;

			if (!hasTiltOrRecline && !tiltKnown && !reclineKnown)
			{
				issues.Add(CreateIssue(component, "positioning_missing", "Komponenta nemá vyplněno, zda podporuje polohování tilt nebo recline.", EvaluationIssueSeverity.Warning));
			}
			else if (!hasTiltOrRecline)
			{
				issues.Add(CreateIssue(component, "positioning_required", "Profil vyžaduje polohování, ale komponenta nepodporuje tilt ani recline.", EvaluationIssueSeverity.Critical));
			}
		}
	}

	// Check control mode suitability.
	private static void CheckControl(UserProfileDto profile, Component component, ComponentSpecsDto specs, List<EvaluationIssueDto> issues)
	{
		var mode = NormalizeKey(specs.ControlMode);
		if (string.IsNullOrWhiteSpace(mode))
		{
			issues.Add(CreateIssue(component, "control_mode_missing", "Komponenta nemá vyplněný typ ovládání.", EvaluationIssueSeverity.Warning));
			return;
		}

		if (profile.HandFunction == HandFunctionLevel.None)
		{
			var supported = mode is "head" or "sip_puff" or "switch" or "voice";
			if (!supported)
			{
				issues.Add(CreateIssue(component, "alternative_control", "Bez funkce rukou je nutné alternativní ovládání.", EvaluationIssueSeverity.Critical));
			}
			return;
		}

		if (profile.HandFunction == HandFunctionLevel.Limited)
		{
			var tooDemanding = mode is "manual_only" or "joystick_advanced";
			if (tooDemanding)
			{
				issues.Add(CreateIssue(component, "control_demanding", "Zvolený typ ovládání je příliš náročný pro omezenou funkci rukou.", EvaluationIssueSeverity.Critical));
			}
		}
	}

	// Check environment suitability.
	private static void CheckEnvironment(UserProfileDto profile, Component component, ComponentSpecsDto specs, List<EvaluationIssueDto> issues)
	{
		var environmentType = NormalizeKey(specs.EnvironmentType);
		if (string.IsNullOrWhiteSpace(environmentType))
		{
			issues.Add(CreateIssue(component, "environment_missing", "Komponenta nemá vyplněné prostředí použití.", EvaluationIssueSeverity.Warning));
			return;
		}

		if (profile.Environment == UsageEnvironment.Indoor && environmentType == "outdoor")
		{
			issues.Add(CreateIssue(component, "environment_indoor", "Komponenta je určená pouze pro outdoor prostředí.", EvaluationIssueSeverity.Critical));
		}
		else if (profile.Environment == UsageEnvironment.Outdoor && environmentType == "indoor")
		{
			issues.Add(CreateIssue(component, "environment_outdoor", "Komponenta je určená pouze pro indoor prostředí.", EvaluationIssueSeverity.Critical));
		}
	}

	// Check lower limb adaptation features.
	private static void CheckLegSupport(UserProfileDto profile, Component component, ComponentSpecsDto specs, List<EvaluationIssueDto> issues)
	{
		if (profile.LowerLimbCondition == LowerLimbConditionLevel.None)
		{
			return;
		}

		if (!specs.SupportsLegRestAdjustment.HasValue)
		{
			issues.Add(CreateIssue(component, "leg_support_missing", "Komponenta nemá vyplněno, zda podporuje nastavitelné opory dolních končetin.", EvaluationIssueSeverity.Warning));
		}
		else if (!specs.SupportsLegRestAdjustment.Value)
		{
			issues.Add(CreateIssue(component, "leg_support", "Profil vyžaduje nastavitelné opory dolních končetin.", EvaluationIssueSeverity.Critical));
		}
	}

	// Check comfort level.
	private static void CheckComfort(ProfileRequirementsDto requirements, Component component, ComponentSpecsDto specs, List<EvaluationIssueDto> issues)
	{
		if (requirements.MinimumComfortLevel > 0 && (!specs.ComfortLevel.HasValue || specs.ComfortLevel.Value <= 0))
		{
			issues.Add(CreateIssue(component, "comfort_missing", "Komponenta nemá vyplněnou úroveň komfortu.", EvaluationIssueSeverity.Warning));
			return;
		}

		if (specs.ComfortLevel.HasValue && specs.ComfortLevel.Value < requirements.MinimumComfortLevel)
		{
			issues.Add(CreateIssue(component, "comfort", "Úroveň komfortu komponenty je pro profil nedostatečná.", EvaluationIssueSeverity.Critical));
		}
	}

	// Check drive performance level.
	private static void CheckDrivePower(ProfileRequirementsDto requirements, Component component, ComponentSpecsDto specs, List<EvaluationIssueDto> issues)
	{
		if (requirements.MinimumDrivePowerLevel <= 0)
		{
			return;
		}

		if (!specs.DrivePowerLevel.HasValue || specs.DrivePowerLevel.Value <= 0)
		{
			issues.Add(CreateIssue(component, "drive_power_missing", "Komponenta nemá vyplněnou úroveň výkonu.", EvaluationIssueSeverity.Warning));
			return;
		}

		if (specs.DrivePowerLevel.Value < requirements.MinimumDrivePowerLevel)
		{
			issues.Add(CreateIssue(component, "drive_power", "Výkon komponenty je nedostatečný vzhledem k hmotnosti uživatele.", EvaluationIssueSeverity.Critical));
		}
	}

	// Map role key from database to internal enum.
	private static ComponentCategoryRole ParseRole(string? roleKey)
	{
		return NormalizeKey(roleKey) switch
		{
			"chassis" => ComponentCategoryRole.Chassis,
			"wheel" => ComponentCategoryRole.Wheel,
			"drive" => ComponentCategoryRole.Drive,
			"battery" => ComponentCategoryRole.Battery,
			"seat" => ComponentCategoryRole.Seat,
			"backrest" => ComponentCategoryRole.Backrest,
			"head_support" => ComponentCategoryRole.HeadSupport,
			"control" => ComponentCategoryRole.Control,
			"leg_support" => ComponentCategoryRole.LegSupport,
			_ => ComponentCategoryRole.Unknown
		};
	}

	// Normalize database key values.
	private static string NormalizeKey(string? value)
	{
		return (value ?? string.Empty).Trim().ToLowerInvariant();
	}

	// Derive required backrest level (1 low, 2 medium, 3 high).
	private static int GetRequiredBackrestHeightLevel(UserProfileDto profile)
	{
		if (profile.HeadControl == HeadControlLevel.No || profile.TrunkStability == TrunkStabilityLevel.Poor || profile.TrunkHeightCm >= 60)
		{
			return 3;
		}

		if (profile.TrunkHeightCm >= 50 || profile.TrunkStability == TrunkStabilityLevel.Medium)
		{
			return 2;
		}

		return 1;
	}

	// Derive required drive power level by weight.
	private static int GetRequiredDrivePowerLevel(int weightKg)
	{
		if (weightKg >= 120)
		{
			return 3;
		}

		if (weightKg >= 90)
		{
			return 2;
		}

		return weightKg > 0 ? 1 : 0;
	}

	// Derive required pressure relief level (0-3).
	private static int GetRequiredPressureReliefLevel(UserProfileDto profile)
	{
		if (profile.PressureInjuryRisk == PressureInjuryRiskLevel.High)
		{
			return 3;
		}

		if (profile.PressureInjuryRisk == PressureInjuryRiskLevel.Medium)
		{
			return 2;
		}

		if (profile.PressureInjuryRisk == PressureInjuryRiskLevel.Low)
		{
			return 1;
		}

		return 0;
	}

	// Derive required comfort level (0-3).
	private static int GetRequiredComfortLevel(UserProfileDto profile)
	{
		var maxSymptom = Math.Max((int)profile.Pain, (int)profile.Fatigue);
		return Math.Clamp(maxSymptom, 0, 3);
	}

	// Build issue DTO
	private static EvaluationIssueDto CreateIssue(Component component, string category, string message, EvaluationIssueSeverity severity)
	{
		return new EvaluationIssueDto
		{
			ComponentId = component.Id,
			ComponentName = component.Name,
			Category = category,
			Message = message,
			Severity = severity
		};
	}

	// Add unique recommendation
	private static void AddRecommendation(List<string> recommendations, string recommendation)
	{
		if (string.IsNullOrWhiteSpace(recommendation) || recommendations.Contains(recommendation))
		{
			return;
		}

		recommendations.Add(recommendation);
	}
}

