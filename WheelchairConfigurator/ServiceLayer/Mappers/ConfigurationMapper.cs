using WheelchairConfigurator.Domain.Models;
using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.ServiceLayer.Mappers;

public static class ConfigurationMapper
{
    public static ConfigurationModel Map(Configuration entity) => new()
    {
        Id = entity.Id,
        SpecialistId = entity.SpecialistId,
        SpecialistName = entity.SpecialistName,
        CreatedAt = entity.CreatedAt,
        PatientMeasurementId = entity.PatientMeasurementId,
        PatientBirthNumber = entity.PatientBirthNumber,
        PatientName = entity.PatientName,
        Hash = entity.Hash,
    };

    public static Configuration Map(ConfigurationRequest request) => new()
    {
        SpecialistId = request.SpecialistId,
        SpecialistName = request.SpecialistName,
        CreatedAt = DateTime.Now,
        PatientMeasurementId = request.PatientMeasurementId,
        PatientBirthNumber = request.PatientBirthNumber,
        PatientName = request.PatientName,
        Hash = Guid.NewGuid().ToString("N"),
    };
}
