using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.Services;

public class NavigationState
{
    public SpecialistModel? ActiveSpecialist { get; set; }
    public PatientMeasurementModel? ActiveMeasurement { get; set; }
    public List<ComponentModel> SelectedComponents { get; set; } = new();

    public UserInput? Patient
    {
        get => ActiveMeasurement is null ? null : new UserInput
        {
            patientIdentificator = ActiveMeasurement.PatientBirthNumber,
            Date = ActiveMeasurement.MeasuredAt,
            BodyHeight = ActiveMeasurement.BodyHeight,
            PelvisWidth = ActiveMeasurement.PelvisWidth,
            ThighLength = ActiveMeasurement.ThighLength,
            Weight = ActiveMeasurement.Weight,
            BodyStability = ActiveMeasurement.BodyStability,
            HeadStability = ActiveMeasurement.HeadStability,
            BedsoreRisk = ActiveMeasurement.BedsoreRisk,
            Control = ActiveMeasurement.Control,
            Environment = ActiveMeasurement.Environment,
            Legs = ActiveMeasurement.Legs,
            Pain = ActiveMeasurement.Pain,
        };
        set
        {
            if (value is null)
            {
                ActiveMeasurement = null;
                return;
            }
            ActiveMeasurement ??= new PatientMeasurementModel();
            ActiveMeasurement.PatientBirthNumber = value.patientIdentificator;
            ActiveMeasurement.MeasuredAt = value.Date;
            ActiveMeasurement.BodyHeight = value.BodyHeight;
            ActiveMeasurement.PelvisWidth = value.PelvisWidth;
            ActiveMeasurement.ThighLength = value.ThighLength;
            ActiveMeasurement.Weight = value.Weight;
            ActiveMeasurement.BodyStability = value.BodyStability;
            ActiveMeasurement.HeadStability = value.HeadStability;
            ActiveMeasurement.BedsoreRisk = value.BedsoreRisk;
            ActiveMeasurement.Control = value.Control;
            ActiveMeasurement.Environment = value.Environment;
            ActiveMeasurement.Legs = value.Legs;
            ActiveMeasurement.Pain = value.Pain;
        }
    }
}
