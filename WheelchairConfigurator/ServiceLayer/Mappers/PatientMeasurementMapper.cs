using WheelchairConfigurator.Domain.Models;
using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.ServiceLayer.Mappers;

public static class PatientMeasurementMapper
{
    public static PatientMeasurement Map(PatientMeasurementModel model) => new()
    {
        Id = model.Id,
        PatientId = model.PatientId,
        MeasuredAt = model.MeasuredAt == default ? DateTime.Now : model.MeasuredAt,
        CreatedBySpecialistId = model.CreatedBySpecialistId,
        CreatedBySpecialistName = model.CreatedBySpecialistName,
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

    public static PatientMeasurementModel Map(PatientMeasurement entity, Patient? patient = null) => new()
    {
        Id = entity.Id,
        PatientId = entity.PatientId,
        PatientFullName = patient is not null ? $"{patient.LastName} {patient.FirstName}" : string.Empty,
        PatientBirthNumber = patient?.BirthNumber ?? string.Empty,
        MeasuredAt = entity.MeasuredAt,
        CreatedBySpecialistId = entity.CreatedBySpecialistId,
        CreatedBySpecialistName = entity.CreatedBySpecialistName,
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
