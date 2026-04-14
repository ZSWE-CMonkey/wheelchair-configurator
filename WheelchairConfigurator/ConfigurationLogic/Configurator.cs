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

		var rulesByCategoryId = BuildCategoryRulesByCategoryId(components, specsByComponentId);

		foreach (var component in components)
		{
			specsByComponentId.TryGetValue(component.Id, out var specs);
			var output = outputByComponentId[component.Id];
			var rules = rulesByCategoryId.GetValueOrDefault(
				component.CategoryId,
				new CategoryRuleSet(CheckWeightCapacity: false, CheckSeatWidth: false));
			var issues = new List<EvaluationIssueDto>();

			EvaluateDeterministicConstraints(profile, result.Requirements, component, rules, specs, issues);
			AddUnsupportedRuleInfoIssues(profile, component, issues);

			result.Issues.AddRange(issues);

			if (!issues.Any(i => i.Severity == EvaluationIssueSeverity.Critical || i.Severity == EvaluationIssueSeverity.Warning))
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

		if (profile.WeightKg >= 120)
		{
			AddRecommendation(result.Recommendations, "Vyšší hmotnost: Preferuj pevnější podvozek a komponenty s vyšší rezervou nosnosti.");
		}

		if (profile.TrunkStability == TrunkStabilityLevel.Poor)
		{
			AddRecommendation(result.Recommendations, "Špatná stabilita trupu: Doporučují se laterální opory, funkce tilt a vyšší opěradlo.");
		}
		else if (profile.TrunkStability == TrunkStabilityLevel.Medium)
		{
			AddRecommendation(result.Recommendations, "Střední stabilita trupu: Zvaž středně vysoké opěradlo a lehčí posturální podporu.");
		}

		if (profile.HeadControl == HeadControlLevel.No)
		{
			AddRecommendation(result.Recommendations, "Bez kontroly hlavy: Je nutná opěrka hlavy a spíše vyšší opěradlo.");
		}

		if (profile.PressureInjuryRisk == PressureInjuryRiskLevel.High)
		{
			AddRecommendation(result.Recommendations, "Vysoké riziko dekubitů: Doporučuje se antidekubitní polštář, tilt a případně recline.");
		}

		if (profile.HandFunction == HandFunctionLevel.None)
		{
			AddRecommendation(result.Recommendations, "Bez funkce rukou: Zvaž alternativní ovládání, například head control nebo sip and puff.");
		}
		else if (profile.HandFunction == HandFunctionLevel.Limited)
		{
			AddRecommendation(result.Recommendations, "Omezená funkce rukou: Preferuj jednodušší a méně náročné ovládání.");
		}

		if (profile.Environment == UsageEnvironment.Indoor)
		{
			AddRecommendation(result.Recommendations, "Indoor použití: Doporučuje se kompaktní podvozek s vyšší manévrovatelností, například mid-wheel.");
		}
		else if (profile.Environment == UsageEnvironment.Outdoor)
		{
			AddRecommendation(result.Recommendations, "Outdoor použití: Doporučuje se robustní podvozek, větší kola a lepší odpružení.");
		}
		else
		{
			AddRecommendation(result.Recommendations, "Kombinované použití: Hledej vyvážený podvozek mezi obratností a stabilitou.");
		}

		if (profile.LowerLimbCondition != LowerLimbConditionLevel.None)
		{
			AddRecommendation(result.Recommendations, "Dolní končetiny: Doporučují se nastavitelné footplates nebo elevating leg rests.");
		}

		if (profile.Pain >= SymptomSeverityLevel.Medium || profile.Fatigue >= SymptomSeverityLevel.Medium)
		{
			AddRecommendation(result.Recommendations, "Bolest nebo únava: Upřednostni komfortnější seating, lepší oporu a možnost polohování.");
		}
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

	// Validate seat width fit
	private static bool IsSeatWidthWithinRange(int seatWidthCm, ProfileRequirementsDto requirements)
	{
		if (!requirements.MinimumSeatWidthCm.HasValue || !requirements.MaximumSeatWidthCm.HasValue)
		{
			return true;
		}

		return seatWidthCm >= requirements.MinimumSeatWidthCm.Value && seatWidthCm <= requirements.MaximumSeatWidthCm.Value;
	}

	// Deterministic checks based on structured data only.
	private static void EvaluateDeterministicConstraints(
		UserProfileDto profile,
		ProfileRequirementsDto requirements,
		Component component,
		CategoryRuleSet rules,
		ComponentSpecsDto? specs,
		List<EvaluationIssueDto> issues)
	{
		if (specs is null)
		{
			var severity = rules.CheckWeightCapacity || rules.CheckSeatWidth
				? EvaluationIssueSeverity.Warning
				: EvaluationIssueSeverity.Info;
			issues.Add(CreateIssue(component, "specs_missing", "Pro tuto komponentu chybí technické specifikace potřebné pro vyhodnocení.", severity));
			return;
		}

		if (rules.CheckWeightCapacity && profile.WeightKg > 0)
		{
			if (specs.WeightCapacityKg <= 0)
			{
				issues.Add(CreateIssue(component, "weight_capacity_missing", "Komponenta nemá vyplněnou nosnost pro kontrolu hmotnosti uživatele.", EvaluationIssueSeverity.Warning));
			}
			else if (profile.WeightKg > specs.WeightCapacityKg)
			{
				issues.Add(CreateIssue(component, "weight_capacity", "Hmotnost uživatele překročila nosnost komponenty.", EvaluationIssueSeverity.Critical));
			}
		}

		if (rules.CheckSeatWidth
			&& requirements.MinimumSeatWidthCm.HasValue
			&& requirements.MaximumSeatWidthCm.HasValue
			&& specs.SeatWidthCm <= 0)
		{
			issues.Add(CreateIssue(component, "seat_width_missing", "Komponenta nemá vyplněnou šířku sedu pro antropometrickou kontrolu.", EvaluationIssueSeverity.Warning));
		}

		if (rules.CheckSeatWidth
			&& requirements.MinimumSeatWidthCm.HasValue
			&& requirements.MaximumSeatWidthCm.HasValue
			&& specs.SeatWidthCm > 0
			&& !IsSeatWidthWithinRange(specs.SeatWidthCm, requirements))
		{
			issues.Add(CreateIssue(component, "seat_width", "Šířka sedu neodpovídá požadovanému rozsahu podle šířky pánve.", EvaluationIssueSeverity.Warning));
		}
	}

	// Add info issues for rules that cannot be evaluated with current DataLayer schema.
	private static void AddUnsupportedRuleInfoIssues(UserProfileDto profile, Component component, List<EvaluationIssueDto> issues)
	{
		if (profile.ThighLengthCm > 0)
		{
			AddInfoIssueOnce(issues, component, "seat_depth_unavailable", "Kontrola hloubky sedu zatím není dostupná, protože chybí potřebná datová pole komponent.");
		}

		if (profile.TrunkHeightCm > 0)
		{
			AddInfoIssueOnce(issues, component, "backrest_height_unavailable", "Kontrola výšky opěradla zatím není dostupná, protože chybí potřebná datová pole komponent.");
		}

		if (profile.TrunkStability == TrunkStabilityLevel.Poor)
		{
			AddInfoIssueOnce(issues, component, "trunk_support_unavailable", "Kontrola posturální podpory trupu zatím není dostupná, protože chybí potřebná datová pole komponent.");
		}

		if (profile.HeadControl == HeadControlLevel.No)
		{
			AddInfoIssueOnce(issues, component, "head_control_unavailable", "Kontrola požadavku na opěrku hlavy zatím není dostupná, protože chybí potřebná datová pole komponent.");
		}

		if (profile.PressureInjuryRisk == PressureInjuryRiskLevel.High)
		{
			AddInfoIssueOnce(issues, component, "pressure_relief_unavailable", "Kontrola prevence dekubitů zatím není dostupná, protože chybí potřebná datová pole komponent.");
		}

		if (profile.HandFunction == HandFunctionLevel.None)
		{
			AddInfoIssueOnce(issues, component, "alternative_control_unavailable", "Kontrola alternativního ovládání zatím není dostupná, protože chybí potřebná datová pole komponent.");
		}

		if (profile.Environment != UsageEnvironment.Mixed)
		{
			AddInfoIssueOnce(issues, component, "environment_unavailable", "Detailní kontrola prostředí použití zatím není dostupná, protože chybí potřebná datová pole komponent.");
		}

		if (profile.LowerLimbCondition != LowerLimbConditionLevel.None)
		{
			AddInfoIssueOnce(issues, component, "leg_support_unavailable", "Kontrola podpory dolních končetin zatím není dostupná, protože chybí potřebná datová pole komponent.");
		}

		if (profile.Pain >= SymptomSeverityLevel.Medium || profile.Fatigue >= SymptomSeverityLevel.Medium)
		{
			AddInfoIssueOnce(issues, component, "comfort_unavailable", "Kontrola komfortních prvků zatím není dostupná, protože chybí potřebná datová pole komponent.");
		}
	}

	private static void AddInfoIssueOnce(List<EvaluationIssueDto> issues, Component component, string category, string message)
	{
		if (issues.Any(i => i.Category == category))
		{
			return;
		}

		issues.Add(CreateIssue(component, category, message, EvaluationIssueSeverity.Info));
	}

	// Build category rules directly from DB-backed component specs.
	private static Dictionary<int, CategoryRuleSet> BuildCategoryRulesByCategoryId(
		List<Component> components,
		Dictionary<int, ComponentSpecsDto?> specsByComponentId)
	{
		var rulesByCategoryId = new Dictionary<int, CategoryRuleSet>();

		foreach (var categoryGroup in components.GroupBy(c => c.CategoryId))
		{
			var checkWeightCapacity = categoryGroup.Any(c =>
				specsByComponentId.TryGetValue(c.Id, out var specs) && specs is not null && specs.WeightCapacityKg > 0);

			var checkSeatWidth = categoryGroup.Any(c =>
				specsByComponentId.TryGetValue(c.Id, out var specs) && specs is not null && specs.SeatWidthCm > 0);

			rulesByCategoryId[categoryGroup.Key] = new CategoryRuleSet(
				CheckWeightCapacity: checkWeightCapacity,
				CheckSeatWidth: checkSeatWidth);
		}

		return rulesByCategoryId;
	}

	private readonly record struct CategoryRuleSet(bool CheckWeightCapacity, bool CheckSeatWidth);

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

