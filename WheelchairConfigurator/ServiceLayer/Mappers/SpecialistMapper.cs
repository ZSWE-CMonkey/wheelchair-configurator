using WheelchairConfigurator.Domain.Models;
using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.ServiceLayer.Mappers;

public static class SpecialistMapper
{
    public static Specialist Map(SpecialistModel model) => new()
    {
        Id = model.Id,
        FirstName = model.FirstName,
        LastName = model.LastName,
        Email = model.Email,
        Clinic = model.Clinic,
        IsActive = model.IsActive,
        CreatedAt = model.CreatedAt == default ? DateTime.Now : model.CreatedAt,
    };

    public static SpecialistModel Map(Specialist entity) => new()
    {
        Id = entity.Id,
        FirstName = entity.FirstName,
        LastName = entity.LastName,
        Email = entity.Email,
        Clinic = entity.Clinic,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
    };
}
