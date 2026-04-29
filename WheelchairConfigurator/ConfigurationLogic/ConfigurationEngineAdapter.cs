using ConfigurationLogic.DTOs;
using ConfigurationLogic.Enums;
using WheelchairConfigurator.ServiceLayer.Interfaces;
using WheelchairConfigurator.ServiceLayer.Models;

using ServiceTrunkStabilityLevel = WheelchairConfigurator.ServiceLayer.Models.TrunkStabilityLevel;

namespace ConfigurationLogic;

/// <summary>
/// Bridges ServiceLayer engine calls to MainServices.
/// </summary>
public sealed class ConfigurationEngineAdapter : IConfigurationEngine
{
    private readonly MainServices _mainServices;

    public ConfigurationEngineAdapter(MainServices mainServices)
    {
        _mainServices = mainServices;
    }

    public async Task<List<int>> GetRecommendedComponentIdsAsync(PatientProfileModel patient, List<ComponentModel> availableComponents)
    {
        ArgumentNullException.ThrowIfNull(patient);
        ArgumentNullException.ThrowIfNull(availableComponents);

        var profile = MapPatientProfile(patient);
        var evaluation = await _mainServices.EvaluateProfileAsync(profile);
        var eligibleIds = evaluation.EligibleComponents.Select(c => c.Id).ToHashSet();

        return availableComponents
            .Where(component => eligibleIds.Contains(component.Id))
            .Select(component => component.Id)
            .ToList();
    }

    public async Task<ConfigurationResult> ValidateAsync(ConfigurationRequest request, List<ComponentModel> selectedComponentsFullData)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(selectedComponentsFullData);

        var profile = MapPatientProfile(request.Patient);
        var selectedIds = request.SelectedComponentIds
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        var selectedById = selectedComponentsFullData
            .Where(component => component.Id > 0)
            .DistinctBy(component => component.Id)
            .ToDictionary(component => component.Id, component => component.Name);

        var state = await _mainServices.RefreshConfigurationAsync(profile, selectedIds);
        var acceptedIds = state.SelectedComponentIds.ToHashSet();
        var requestedIds = selectedIds.ToHashSet();

        if (requestedIds.SetEquals(acceptedIds))
        {
            return new ConfigurationResult
            {
                IsSuccess = true,
                Message = "Konfigurace je platná."
            };
        }

        var problems = new List<string>();
        foreach (var missingId in selectedIds.Where(id => !acceptedIds.Contains(id)))
        {
            var componentState = state.Components.FirstOrDefault(component => component.Component.Id == missingId);
            if (componentState is not null && componentState.DisableReasons.Count > 0)
            {
                problems.AddRange(componentState.DisableReasons);
                continue;
            }

            if (selectedById.TryGetValue(missingId, out var name))
            {
                problems.Add($"Komponenta '{name}' není kompatibilní s aktuální konfigurací.");
            }
            else
            {
                problems.Add($"Komponenta s ID {missingId} není kompatibilní s aktuální konfigurací.");
            }
        }

        if (problems.Count == 0)
        {
            problems.Add("Vybrané komponenty nejsou kompatibilní s aktuální konfigurací.");
        }

        return new ConfigurationResult
        {
            IsSuccess = false,
            Message = string.Join(" ", problems.Distinct())
        };
    }

    // Short profile mapping.
    private static UserProfileDto MapPatientProfile(PatientProfileModel? patient)
    {
        return new UserProfileDto
        {
            TrunkHeightCm = 0,
            WeightKg = patient?.WeightKg ?? 0,
            PelvisWidthCm = patient?.PelvisWidthCm ?? 0,
            ThighLengthCm = patient?.ThighLengthCm ?? 0,
            TrunkStability = MapTrunkStability(patient?.TrunkStability ?? ServiceTrunkStabilityLevel.Good),
            HeadControl = HeadControlLevel.Yes,
            PressureInjuryRisk = patient?.HasPressureSoresRisk == true ? PressureInjuryRiskLevel.High : PressureInjuryRiskLevel.Low,
            Pain = SymptomSeverityLevel.None,
            Fatigue = SymptomSeverityLevel.None,
            LowerLimbCondition = LowerLimbConditionLevel.None,
            HandFunction = HandFunctionLevel.Full,
            Environment = UsageEnvironment.Mixed
        };
    }

    private static Enums.TrunkStabilityLevel MapTrunkStability(ServiceTrunkStabilityLevel trunkStability)
    {
        return trunkStability switch
        {
            ServiceTrunkStabilityLevel.Good => Enums.TrunkStabilityLevel.Good,
            ServiceTrunkStabilityLevel.Fair => Enums.TrunkStabilityLevel.Medium,
            ServiceTrunkStabilityLevel.Poor => Enums.TrunkStabilityLevel.Poor,
            _ => Enums.TrunkStabilityLevel.Medium
        };
    }
}




