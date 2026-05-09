using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.Services;

public class NavigationState
{
    public UserInput? Patient { get; set; }
    public List<ComponentModel> SelectedComponents { get; set; } = new();
}
