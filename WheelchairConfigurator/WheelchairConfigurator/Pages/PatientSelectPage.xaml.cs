using WheelchairConfigurator.ServiceLayer.Interfaces;
using WheelchairConfigurator.ServiceLayer.Models;
using WheelchairConfigurator.Services;

namespace WheelchairConfigurator.Pages;

public class PatientEntry
{
    public int PatientId { get; set; }
    public string BirthNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DisplayText => $"{BirthNumber}  {FullName}";
    public DateTime CreatedAt { get; set; }
}

public class ConfigEntry
{
    public int DbId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string PatientBirthNumber { get; set; } = string.Empty;
    public int MeasurementId { get; set; }
    public bool IsNew { get; set; }
}

public partial class PatientSelectPage : ContentPage
{
    private readonly IAppService _appService;
    private readonly NavigationState _navState;
    private List<ConfigurationModel> _allConfigs = new();
    private PatientEntry? _selectedPatient = null;
    private ConfigEntry? _selectedConfig = null;

    public PatientSelectPage(IAppService appService, NavigationState navState)
    {
        _appService = appService;
        _navState = navState;
        InitializeComponent();
        Dispatcher.Dispatch(async () => await LoadPatients());
    }

    private async Task LoadPatients()
    {
        var patients = await _appService.GetPatientsAsync();

        PatientList.ItemsSource = patients.Select(p => new PatientEntry
        {
            PatientId = p.Id,
            BirthNumber = p.BirthNumber,
            FullName = p.FullName,
            CreatedAt = p.CreatedAt,
        }).ToList();
    }

    private async void OnPatientSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not PatientEntry selected)
            return;

        _selectedPatient = selected;
        _selectedConfig = null;
        ContinueBtn.IsEnabled = false;

        WheelchairListTitle.Text = $"Konfigurace — {selected.DisplayText}";

        var specialist = _navState.ActiveSpecialist;
        _allConfigs = specialist is not null
            ? await _appService.GetConfigurationsBySpecialistAsync(specialist.Id)
            : new List<ConfigurationModel>();

        var measurements = await _appService.GetMeasurementsForPatientAsync(selected.PatientId);
        var latestMeasurement = measurements.FirstOrDefault();

        var items = _allConfigs
            .Where(c => c.PatientBirthNumber == selected.BirthNumber)
            .Select(c => new ConfigEntry
            {
                DbId = c.Id,
                Name = $"Konfigurace #{c.Id} ({c.Hash[..Math.Min(8, c.Hash.Length)]})",
                CreatedAt = c.CreatedAt,
                PatientBirthNumber = selected.BirthNumber,
                MeasurementId = c.PatientMeasurementId,
                IsNew = false
            })
            .ToList<ConfigEntry>();

        items.Add(new ConfigEntry
        {
            DbId = 0,
            Name = "Vytvořit nový vozík",
            CreatedAt = DateTime.MinValue,
            PatientBirthNumber = selected.BirthNumber,
            MeasurementId = latestMeasurement?.Id ?? 0,
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
        if (_selectedConfig is null || _selectedPatient is null)
            return;

        if (_selectedConfig.IsNew)
        {
            var measurements = await _appService.GetMeasurementsForPatientAsync(_selectedPatient.PatientId);
            var measurement = measurements.FirstOrDefault();

            if (measurement is null)
            {
                await DisplayAlert("Chybí měření", "Pro tohoto pacienta nejsou uložena žádná měření. Přidejte měření ve správě pacientů.", "OK");
                return;
            }

            _navState.ActiveMeasurement = measurement;
            _navState.SelectedComponents.Clear();
            await Shell.Current.GoToAsync("wheelchairConfiguratorPage");
        }
        else
            await Shell.Current.GoToAsync($"summaryPage?configId={_selectedConfig.DbId}");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("mainPage");
    }
}
