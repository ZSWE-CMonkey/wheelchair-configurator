using WheelchairConfigurator.ServiceLayer.Interfaces;
using WheelchairConfigurator.ServiceLayer.Models;
using WheelchairConfigurator.Services;

namespace WheelchairConfigurator.Pages;

public partial class PatientManagerPage : ContentPage
{
    private readonly IAppService _appService;
    private readonly NavigationState _navState;
    private PatientModel? _selectedPatient;

    public PatientManagerPage(IAppService appService, NavigationState navState)
    {
        _appService = appService;
        _navState = navState;
        InitializeComponent();
        Dispatcher.Dispatch(async () => await LoadPatients());
    }

    private async Task LoadPatients()
    {
        PatientList.ItemsSource = await _appService.GetPatientsAsync();
    }

    private async void OnPatientSelected(object sender, SelectionChangedEventArgs e)
    {
        _selectedPatient = e.CurrentSelection.FirstOrDefault() as PatientModel;
        bool hasSelection = _selectedPatient is not null;
        DeactivatePatientBtn.IsEnabled = hasSelection;

        if (hasSelection)
        {
            RightPanelTitle.Text = $"Měření — {_selectedPatient!.FullName}";
            NewPatientForm.IsVisible = false;
            MeasurementList.IsVisible = true;
            ActionBtn.Text = "Přidat měření";

            var measurements = await _appService.GetMeasurementsForPatientAsync(_selectedPatient.Id);
            MeasurementList.ItemsSource = measurements;
        }
        else
        {
            RightPanelTitle.Text = "Vyberte pacienta";
            NewPatientForm.IsVisible = true;
            MeasurementList.IsVisible = false;
            ActionBtn.Text = "Přidat pacienta";
        }
    }

    private async void OnDeactivatePatientClicked(object sender, EventArgs e)
    {
        if (_selectedPatient is null) return;

        bool confirm = await DisplayAlert("Deaktivovat",
            $"Opravdu chcete deaktivovat pacienta {_selectedPatient.FullName}?", "Ano", "Ne");
        if (!confirm) return;

        await _appService.DeactivatePatientAsync(_selectedPatient.Id);
        _selectedPatient = null;
        PatientList.SelectedItem = null;
        RightPanelTitle.Text = "Vyberte pacienta";
        NewPatientForm.IsVisible = true;
        MeasurementList.IsVisible = false;
        ActionBtn.Text = "Přidat pacienta";
        DeactivatePatientBtn.IsEnabled = false;
        await LoadPatients();
    }

    private async void OnActionClicked(object sender, EventArgs e)
    {
        if (_selectedPatient is null)
        {
            await AddNewPatient();
        }
        else
        {
            await AddMeasurementForPatient();
        }
    }

    private async Task AddNewPatient()
    {
        var birthNumber = BirthNumberEntry.Text?.Trim() ?? string.Empty;
        var firstName = FirstNameEntry.Text?.Trim() ?? string.Empty;
        var lastName = LastNameEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(birthNumber) || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            await DisplayAlert("Chyba", "Vyplňte rodné číslo, jméno a příjmení.", "OK");
            return;
        }

        var specialist = _navState.ActiveSpecialist;
        var saved = await _appService.SavePatientAsync(new PatientModel
        {
            BirthNumber = birthNumber,
            FirstName = firstName,
            LastName = lastName,
            CreatedBySpecialistId = specialist?.Id ?? 1,
            CreatedBySpecialistName = specialist?.FullName ?? "",
        });

        BirthNumberEntry.Text = string.Empty;
        FirstNameEntry.Text = string.Empty;
        LastNameEntry.Text = string.Empty;

        await LoadPatients();

        var items = PatientList.ItemsSource as List<PatientModel>;
        var match = items?.FirstOrDefault(p => p.Id == saved.Id);
        if (match is not null)
            PatientList.SelectedItem = match;
    }

    private async Task AddMeasurementForPatient()
    {
        if (_selectedPatient is null) return;

        await Shell.Current.GoToAsync($"newMeasurementPage?patientId={_selectedPatient.Id}");
    }

    private async void OnMeasurementSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not PatientMeasurementModel measurement) return;

        MeasurementList.SelectedItem = null;

        bool configure = await DisplayAlert(
            "Konfigurovat vozík",
            $"Konfigurovat vozík z měření ze dne {measurement.MeasuredAt:dd.MM.yyyy HH:mm}?",
            "Konfigurovat", "Zrušit");

        if (!configure) return;

        _navState.ActiveMeasurement = measurement;
        _navState.SelectedComponents = [];
        await Shell.Current.GoToAsync("wheelchairConfiguratorPage");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
