using WheelchairConfigurator.ServiceLayer.Interfaces;
using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.Pages;

public partial class ActivityLogPage : ContentPage
{
    private readonly IAppService _appService;
    private List<ActivityLogModel> _allLogs = [];

    public ActivityLogPage(IAppService appService)
    {
        _appService = appService;
        InitializeComponent();
        Dispatcher.Dispatch(async () => await LoadLog());
    }

    private async Task LoadLog()
    {
        _allLogs = await _appService.GetActivityLogAsync(200);

        var entityTypes = _allLogs.Select(l => l.EntityType).Distinct().OrderBy(t => t).ToList();
        FilterPicker.Items.Clear();
        foreach (var t in entityTypes)
            FilterPicker.Items.Add(t);

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (FilterPicker.SelectedIndex < 0)
        {
            LogList.ItemsSource = _allLogs;
            return;
        }
        var selected = FilterPicker.Items[FilterPicker.SelectedIndex];
        LogList.ItemsSource = _allLogs.Where(l => l.EntityType == selected).ToList();
    }

    private void OnFilterChanged(object sender, EventArgs e) => ApplyFilter();

    private void OnShowAllClicked(object sender, EventArgs e)
    {
        FilterPicker.SelectedIndex = -1;
        ApplyFilter();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
