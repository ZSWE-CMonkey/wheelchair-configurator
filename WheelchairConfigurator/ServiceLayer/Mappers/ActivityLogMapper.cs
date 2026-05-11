using WheelchairConfigurator.Domain.Models;
using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.ServiceLayer.Mappers;

public static class ActivityLogMapper
{
    public static ActivityLogModel Map(ActivityLog entity) => new()
    {
        Id = entity.Id,
        OccurredAt = entity.OccurredAt,
        SpecialistId = entity.SpecialistId,
        SpecialistName = entity.SpecialistName,
        Action = entity.Action,
        EntityType = entity.EntityType,
        EntityId = entity.EntityId,
        Detail = entity.Detail,
    };
}
