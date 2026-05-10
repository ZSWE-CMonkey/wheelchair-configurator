using WheelchairConfigurator.ServiceLayer.Interfaces;
using WheelchairConfigurator.Services;

namespace WheelchairConfigurator.Pages;

public partial class MainPage : ContentPage
{
    private readonly NavigationState _navState;
    private readonly IAppService _appService;

    public MainPage(NavigationState navState, IAppService appService)
    {
        _navState = navState;
        _appService = appService;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateSpecialistUI();
    }

    private void UpdateSpecialistUI()
    {
        var specialist = _navState.ActiveSpecialist;
        bool hasSpecialist = specialist is not null;

        SpecialistLabel.Text = hasSpecialist ? specialist!.FullName : "Není vybrán";
        SelectSpecialistBtn.Text = hasSpecialist ? "Změnit" : "Vybrat";

        ToNewPatientBtn.IsEnabled = hasSpecialist;
        ToPatientSelectBtn.IsEnabled = hasSpecialist;
        ToPatientManagerBtn.IsEnabled = hasSpecialist;
        SpecialistHintLabel.IsVisible = !hasSpecialist;
    }

    private async void OnSelectSpecialistClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("therapistManagerPage");
    }

    private async void OnToNewPatientClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("newPatientPage");
    }

    private async void OnToPatientSelectClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("patientSelectPage");
    }

    private async void OnToPatientManagerClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("patientManagerPage");
    }

    private async void OnToTherapistManagerClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("therapistManagerPage");
    }

    private async void OnToComponentManagerClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("componentManagerPage");
    }

    private async void OnToActivityLogClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("activityLogPage");
    }

    private async void OnToSettingsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("settingsPage");
    }
}
