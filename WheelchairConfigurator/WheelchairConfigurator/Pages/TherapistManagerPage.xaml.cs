using WheelchairConfigurator.ServiceLayer.Interfaces;
using WheelchairConfigurator.ServiceLayer.Models;
using WheelchairConfigurator.Services;

namespace WheelchairConfigurator.Pages;

public class TherapistListItem
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Clinic { get; set; }
    public string? Email { get; set; }
    public bool IsSelected { get; set; }
}

public partial class TherapistManagerPage : ContentPage
{
    private readonly IAppService _appService;
    private readonly NavigationState _navState;
    private TherapistListItem? _selectedItem;
    private int _editingId = 0;

    public TherapistManagerPage(IAppService appService, NavigationState navState)
    {
        _appService = appService;
        _navState = navState;
        InitializeComponent();
        Dispatcher.Dispatch(async () => await LoadTherapists());
    }

    private async Task LoadTherapists()
    {
        var specialists = await _appService.GetSpecialistsAsync();
        var activeId = _navState.ActiveSpecialist?.Id ?? 0;

        TherapistList.ItemsSource = specialists.Select(s => new TherapistListItem
        {
            Id = s.Id,
            FullName = s.FullName,
            Clinic = s.Clinic,
            Email = s.Email,
            IsSelected = s.Id == activeId,
        }).ToList();
    }

    private void OnTherapistSelected(object sender, SelectionChangedEventArgs e)
    {
        _selectedItem = e.CurrentSelection.FirstOrDefault() as TherapistListItem;
        bool hasSelection = _selectedItem is not null;
        SelectBtn.IsEnabled = hasSelection;
        DeactivateBtn.IsEnabled = hasSelection;

        if (_selectedItem is not null)
        {
            _editingId = _selectedItem.Id;
            var nameParts = _selectedItem.FullName.Split(' ', 2);
            FirstNameEntry.Text = nameParts.Length > 0 ? nameParts[0] : string.Empty;
            LastNameEntry.Text = nameParts.Length > 1 ? nameParts[1] : string.Empty;
            EmailEntry.Text = _selectedItem.Email ?? string.Empty;
            ClinicEntry.Text = _selectedItem.Clinic ?? string.Empty;
            FormTitle.Text = "Upravit terapeuta";
        }
    }

    private async void OnSaveFormClicked(object sender, EventArgs e)
    {
        var firstName = FirstNameEntry.Text?.Trim() ?? string.Empty;
        var lastName = LastNameEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            await DisplayAlert("Chyba", "Vyplňte jméno a příjmení.", "OK");
            return;
        }

        await _appService.SaveSpecialistAsync(new SpecialistModel
        {
            Id = _editingId,
            FirstName = firstName,
            LastName = lastName,
            Email = string.IsNullOrWhiteSpace(EmailEntry.Text) ? null : EmailEntry.Text.Trim(),
            Clinic = string.IsNullOrWhiteSpace(ClinicEntry.Text) ? null : ClinicEntry.Text.Trim(),
        });

        OnClearFormClicked(sender, e);
        await LoadTherapists();
    }

    private void OnClearFormClicked(object sender, EventArgs e)
    {
        _editingId = 0;
        FirstNameEntry.Text = string.Empty;
        LastNameEntry.Text = string.Empty;
        EmailEntry.Text = string.Empty;
        ClinicEntry.Text = string.Empty;
        FormTitle.Text = "Přidat terapeuta";
        TherapistList.SelectedItem = null;
        _selectedItem = null;
        SelectBtn.IsEnabled = false;
        DeactivateBtn.IsEnabled = false;
    }

    private async void OnDeactivateClicked(object sender, EventArgs e)
    {
        if (_selectedItem is null) return;

        bool confirm = await DisplayAlert("Deaktivovat",
            $"Opravdu chcete deaktivovat terapeuta {_selectedItem.FullName}?", "Ano", "Ne");
        if (!confirm) return;

        if (_navState.ActiveSpecialist?.Id == _selectedItem.Id)
            _navState.ActiveSpecialist = null;

        await _appService.DeactivateSpecialistAsync(_selectedItem.Id);
        OnClearFormClicked(sender, e);
        await LoadTherapists();
    }

    private async void OnSelectClicked(object sender, EventArgs e)
    {
        if (_selectedItem is null) return;

        var model = await _appService.GetSpecialistByIdAsync(_selectedItem.Id);
        if (model is not null)
        {
            _navState.ActiveSpecialist = model;
            await DisplayAlert("Přihlášení", $"Přihlášen jako: {model.FullName}", "OK");
        }

        await LoadTherapists();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("mainPage");
    }
}
