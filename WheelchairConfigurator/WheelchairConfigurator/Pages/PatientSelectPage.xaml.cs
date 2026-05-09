using WheelchairConfigurator.ServiceLayer.Interfaces;
using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.Pages;

public class PatientEntry
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.MinValue;
}

public class ConfigEntry
{
    public int DbId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public bool IsNew { get; set; }
}

public partial class PatientSelectPage : ContentPage
{
    private readonly IAppService _appService;
    private List<ConfigurationModel> _allConfigs = new();
    private ConfigEntry? _selectedConfig = null;

    public PatientSelectPage(IAppService appService)
    {
        _appService = appService;
        InitializeComponent();
        Dispatcher.Dispatch(async () => await LoadPatients());
    }

    private async Task LoadPatients()
    {
        _allConfigs = await _appService.GetConfigurationsBySpecialistAsync(1);

        var patients = _allConfigs
            .Select(c => c.PatientIdentificator)
            .Distinct()
            .Select(id => new PatientEntry { Id = id })
            .ToList();

        PatientList.ItemsSource = patients;
    }

    private void OnPatientSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not PatientEntry selected)
            return;

        _selectedConfig = null;
        ContinueBtn.IsEnabled = false;

        WheelchairListTitle.Text = $"Konfigurace pacienta {selected.Id}";

        var items = _allConfigs
            .Where(c => c.PatientIdentificator == selected.Id)
            .Select(c => new ConfigEntry
            {
                DbId = c.Id,
                Name = $"Konfigurace {c.Id}",
                CreatedAt = c.CreatedAt,
                PatientId = c.PatientIdentificator,
                IsNew = false
            })
            .ToList<ConfigEntry>();

        items.Add(new ConfigEntry
        {
            DbId = 0,
            Name = "Vytvořit nový vozík",
            CreatedAt = DateTime.MinValue,
            PatientId = selected.Id,
            IsNew = true
        });

        WheelchairList.ItemsSource = items;
    }

    private void OnWheelchairSelected(object sender, SelectionChangedEventArgs e)
    {
        _selectedConfig = e.CurrentSelection.FirstOrDefault() as ConfigEntry;
        ContinueBtn.IsEnabled = _selectedConfig is not null;
    }

    private async void OnContinueClicked(object sender, EventArgs e)
    {
        if (_selectedConfig is null)
            return;

        if (_selectedConfig.IsNew)
            await Shell.Current.GoToAsync("wheelchairConfiguratorPage");
        else
            await Shell.Current.GoToAsync($"summaryPage?configId={_selectedConfig.DbId}");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("mainPage");
    }
}
