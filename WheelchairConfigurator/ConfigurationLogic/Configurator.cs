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

		foreach (var component in components)
		{
			var specs = await _catalog.GetComponentDetailAsync(component.Id);
			if (specs is null)
			{
				result.Issues.Add(CreateIssue(component, "specifikace", "Chybí specifikace komponenty.", EvaluationIssueSeverity.Warning));
				continue;
			}

			if (specs.WeightCapacityKg <= 0)
			{
				result.Issues.Add(CreateIssue(component, "specifikace", "Komponenta nemá vyplněnou nosnost.", EvaluationIssueSeverity.Warning));
				continue;
			}

			if (profile.WeightKg > 0 && profile.WeightKg > specs.WeightCapacityKg)
			{
				result.Issues.Add(CreateIssue(component, "nosnost", "Komponenta nemá dostatečnou nosnost pro zadanou hmotnost.", EvaluationIssueSeverity.Critical));
				continue;
			}

			if (result.Requirements.MinimumSeatWidthCm.HasValue && result.Requirements.MaximumSeatWidthCm.HasValue)
			{
				if (!IsSeatWidthWithinRange(specs.SeatWidthCm, result.Requirements))
				{
					result.Issues.Add(CreateIssue(
						component,
						"šířka sedu",
						$"Šířka sedu {specs.SeatWidthCm} cm neodpovídá požadavku {result.Requirements.MinimumSeatWidthCm}-{result.Requirements.MaximumSeatWidthCm} cm.",
						EvaluationIssueSeverity.Warning));
					continue;
				}
			}

			result.EligibleComponents.Add(await _catalog.ToComponentOutputDtoAsync(component));
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
			AddRecommendation(result.Recommendations, "Vyšší hmotnost: preferuj pevnější podvozek a komponenty s vyšší rezervou nosnosti.");
		}

		if (profile.TrunkStability == TrunkStabilityLevel.Poor)
		{
			AddRecommendation(result.Recommendations, "Špatná stabilita trupu: tilt, laterální opory a vyšší opěradlo.");
		}
		else if (profile.TrunkStability == TrunkStabilityLevel.Medium)
		{
			AddRecommendation(result.Recommendations, "Střední stabilita trupu: zvážit středně vysoké opěradlo a případně lehčí posturální podporu.");
		}

		if (profile.HeadControl == HeadControlLevel.No)
		{
			AddRecommendation(result.Recommendations, "Bez kontroly hlavy: nutná opěrka hlavy a spíše vyšší opěradlo.");
		}

		if (profile.PressureInjuryRisk == PressureInjuryRiskLevel.High)
		{
			AddRecommendation(result.Recommendations, "Vysoké riziko dekubitů: pressure-relief cushion, tilt a případně recline.");
		}

		if (profile.HandFunction == HandFunctionLevel.None)
		{
			AddRecommendation(result.Recommendations, "Bez funkce rukou: zvaž alternativní ovládání jako head control nebo sip & puff.");
		}
		else if (profile.HandFunction == HandFunctionLevel.Limited)
		{
			AddRecommendation(result.Recommendations, "Omezená funkce rukou: preferuj jednodušší a méně náročné ovládání.");
		}

		if (profile.Environment == UsageEnvironment.Indoor)
		{
			AddRecommendation(result.Recommendations, "Indoor použití: kompaktní podvozek a vyšší manévrovatelnost, např. mid-wheel.");
		}
		else if (profile.Environment == UsageEnvironment.Outdoor)
		{
			AddRecommendation(result.Recommendations, "Outdoor použití: robustní podvozek, větší kola a lepší odpružení.");
		}
		else
		{
			AddRecommendation(result.Recommendations, "Kombinované použití: hledej vyvážený podvozek mezi obratností a stabilitou.");
		}

		if (profile.LowerLimbCondition != LowerLimbConditionLevel.None)
		{
			AddRecommendation(result.Recommendations, "Dolní končetiny: nastavitelné footplates nebo elevating leg rests.");
		}

		if (profile.Pain >= SymptomSeverityLevel.Medium || profile.Fatigue >= SymptomSeverityLevel.Medium)
		{
			AddRecommendation(result.Recommendations, "Bolest nebo únava: upřednostni komfortní seating, lepší oporu a možnost polohování.");
		}
	}

	// Compute seat width range
	private static string DescribeSeatWidth(UserProfileDto profile)
	{
		if (profile.PelvisWidthCm <= 0)
		{
			return "nelze určit bez šířky pánve";
		}

		return $"{profile.PelvisWidthCm}-{profile.PelvisWidthCm + 3} cm";
	}

	// Compute seat depth range
	private static string DescribeSeatDepth(UserProfileDto profile)
	{
		if (profile.ThighLengthCm <= 0)
		{
			return "nelze určit bez délky stehna";
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
			return "vyšší opěradlo";
		}

		if (profile.TrunkHeightCm >= 60)
		{
			return "vyšší opěradlo";
		}

		if (profile.TrunkHeightCm >= 50)
		{
			return "střední výška opěradla";
		}

		if (profile.TrunkStability == TrunkStabilityLevel.Medium)
		{
			return "střední výška opěradla";
		}

		return "nižší až střední opěradlo";
	}

	// Pick chassis type
	private static string GetChassisRecommendation(UserProfileDto profile)
	{
		return profile.Environment switch
		{
			UsageEnvironment.Indoor => "kompaktní podvozek s vysokou manévrovatelností",
			UsageEnvironment.Outdoor => "robustní podvozek s většími koly a lepším odpružením",
			_ => "vyvážený podvozek pro indoor i outdoor použití"
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
