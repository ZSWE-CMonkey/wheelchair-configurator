using WheelchairConfigurator.Domain.Models;
using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.ServiceLayer.Mappers;

public static class PatientMapper
{
    public static Patient Map(PatientModel model) => new()
    {
        SpecialistId = model.SpecialistId,
        PatientIdentificator = model.PatientIdentificator,
        BodyHeight = model.BodyHeight,
        PelvisWidth = model.PelvisWidth,
        ThighLength = model.ThighLength,
        Weight = model.Weight,
        BodyStability = model.BodyStability,
        HeadStability = model.HeadStability,
        BedsoreRisk = model.BedsoreRisk,
        Control = model.Control,
        Environment = model.Environment,
        Legs = model.Legs,
        Pain = model.Pain,
    };

    public static PatientModel Map(Patient entity) => new()
    {
        SpecialistId = entity.SpecialistId,
        PatientIdentificator = entity.PatientIdentificator,
        BodyHeight = entity.BodyHeight,
        PelvisWidth = entity.PelvisWidth,
        ThighLength = entity.ThighLength,
        Weight = entity.Weight,
        BodyStability = entity.BodyStability,
        HeadStability = entity.HeadStability,
        BedsoreRisk = entity.BedsoreRisk,
        Control = entity.Control,
        Environment = entity.Environment,
        Legs = entity.Legs,
        Pain = entity.Pain,
    };
}
