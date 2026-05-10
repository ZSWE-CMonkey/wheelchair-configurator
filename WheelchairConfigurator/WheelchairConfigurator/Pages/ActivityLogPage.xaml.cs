using WheelchairConfigurator.ServiceLayer.Interfaces;

namespace WheelchairConfigurator.Pages;

public partial class ActivityLogPage : ContentPage
{
    private readonly IAppService _appService;

    public ActivityLogPage(IAppService appService)
    {
        _appService = appService;
        InitializeComponent();
        Dispatcher.Dispatch(async () => await LoadLog());
    }

    private async Task LoadLog()
    {
        LogList.ItemsSource = await _appService.GetActivityLogAsync(200);
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("mainPage");
    }
}
