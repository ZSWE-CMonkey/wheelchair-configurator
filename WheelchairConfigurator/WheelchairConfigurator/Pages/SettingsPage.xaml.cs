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
        HighQualityTexturesSwitch.IsToggled = settings.HighQualityTextures;
        _suppressToggle = false;
    }

    private async void OnRenderingToggled(object sender, ToggledEventArgs e)
    {
        if (_suppressToggle) return;
        var settings = await _appService.GetSettingsAsync();
        settings.RenderingEnabled = e.Value;
        await _appService.SaveSettingsAsync(settings);
    }

    private async void OnHighQualityTexturesToggled(object sender, ToggledEventArgs e)
    {
        if (_suppressToggle) return;
        var settings = await _appService.GetSettingsAsync();
        settings.HighQualityTextures = e.Value;
        await _appService.SaveSettingsAsync(settings);
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

    private async void OnLoadTestDataClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(
            "Načtení testovacích dat",
            "Tato akce SMAŽE všechna existující data a načte testovací katalog + terapeuta + pacienta. Pokračovat?",
            "Ano, načti",
            "Zrušit");
        if (!confirm) return;

        await RunDevAction(
            "Načítám...",
            () => _appService.LoadTestDataAsync(),
            "Hotovo. Restartuj aplikaci pro úplný efekt (nové 3D soubory se zkopírují při dalším startu).");
    }

    private async void OnWipeDbClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(
            "Vymazání databáze",
            "Tato akce SMAŽE všechna data včetně katalogu komponent. Databáze bude úplně prázdná. Pokračovat?",
            "Ano, vymaž",
            "Zrušit");
        if (!confirm) return;

        await RunDevAction(
            "Mažu...",
            () => _appService.WipeDatabaseAsync(),
            "Databáze vymazána. Restartuj aplikaci.");
    }

    private async Task RunDevAction(string busyText, Func<Task> action, string okText)
    {
        try
        {
            LoadTestDataBtn.IsEnabled = false;
            WipeDbBtn.IsEnabled = false;
            DevStatusLabel.TextColor = Colors.Gray;
            DevStatusLabel.Text = busyText;
            await action();
            DevStatusLabel.TextColor = Colors.Green;
            DevStatusLabel.Text = okText;
        }
        catch (Exception ex)
        {
            DevStatusLabel.TextColor = Colors.Red;
            DevStatusLabel.Text = $"Chyba: {ex.Message}";
        }
        finally
        {
            LoadTestDataBtn.IsEnabled = true;
            WipeDbBtn.IsEnabled = true;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
