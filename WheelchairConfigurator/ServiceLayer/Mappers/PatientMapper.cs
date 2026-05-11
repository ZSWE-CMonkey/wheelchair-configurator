using WheelchairConfigurator.Domain.Models;
using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.ServiceLayer.Mappers;

public static class PatientMapper
{
    public static Patient Map(PatientModel model) => new()
    {
        Id = model.Id,
        BirthNumber = model.BirthNumber,
        FirstName = model.FirstName,
        LastName = model.LastName,
        IsActive = model.IsActive,
        CreatedAt = model.CreatedAt == default ? DateTime.Now : model.CreatedAt,
        CreatedBySpecialistId = model.CreatedBySpecialistId,
        CreatedBySpecialistName = model.CreatedBySpecialistName,
    };

    public static PatientModel Map(Patient entity) => new()
    {
        Id = entity.Id,
        BirthNumber = entity.BirthNumber,
        FirstName = entity.FirstName,
        LastName = entity.LastName,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        CreatedBySpecialistId = entity.CreatedBySpecialistId,
        CreatedBySpecialistName = entity.CreatedBySpecialistName,
    };
}
