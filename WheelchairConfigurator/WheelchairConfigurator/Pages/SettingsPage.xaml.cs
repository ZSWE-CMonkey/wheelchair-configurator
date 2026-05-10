using WheelchairConfigurator.ServiceLayer.Interfaces;
using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly IAppService _appService;
    private bool _suppressToggle;

    public SettingsPage(IAppService appService)
    {
        _appService = appService;
        InitializeComponent();
        Dispatcher.Dispatch(async () => await LoadSettings());
    }

    private async Task LoadSettings()
    {
        var settings = await _appService.GetSettingsAsync();
        _suppressToggle = true;
        RenderingSwitch.IsToggled = settings.RenderingEnabled;
        _suppressToggle = false;
    }

    private async void OnRenderingToggled(object sender, ToggledEventArgs e)
    {
        if (_suppressToggle) return;
        await _appService.SaveSettingsAsync(new AppSettingsModel { RenderingEnabled = e.Value });
    }

    private async void OnExportCatalogClicked(object sender, EventArgs e)
    {
        try
        {
            CatalogStatusLabel.Text = "Exportuji...";
            var bytes = await _appService.ExportComponentCatalogAsync();
            var path = Path.Combine(FileSystem.CacheDirectory, $"katalog_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            await File.WriteAllBytesAsync(path, bytes);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Katalog komponent",
                File = new ShareFile(path)
            });
            CatalogStatusLabel.Text = $"Exportováno: {bytes.Length / 1024} KB";
        }
        catch (Exception ex)
        {
            CatalogStatusLabel.Text = $"Chyba: {ex.Message}";
        }
    }

    private async void OnImportCatalogClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Vyberte JSON katalog",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "application/json" } },
                    { DevicePlatform.WinUI, new[] { ".json" } },
                })
            });

            if (result is null) return;

            CatalogStatusLabel.Text = "Importuji...";
            using var stream = await result.OpenReadAsync();
            var importResult = await _appService.ImportComponentCatalogAsync(stream);
            CatalogStatusLabel.Text = importResult.Message;
        }
        catch (Exception ex)
        {
            CatalogStatusLabel.Text = $"Chyba: {ex.Message}";
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
