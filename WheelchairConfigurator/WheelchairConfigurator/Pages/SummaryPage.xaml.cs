using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using WheelchairConfigurator.Helpers;
using WheelchairConfigurator.ServiceLayer.Interfaces;
using WheelchairConfigurator.ServiceLayer.Models;
using WheelchairConfigurator.Services;

namespace WheelchairConfigurator.Pages;

[QueryProperty(nameof(ConfigurationId), "configId")]
public partial class SummaryPage : ContentPage
{
    private readonly IAppService _appService;
    private readonly NavigationState _navState;

    private int _configurationId = 0;
    private ConfigurationModel? _loadedConfig = null;

    public int ConfigurationId
    {
        get => _configurationId;
        set
        {
            _configurationId = value;
            Dispatcher.Dispatch(async () => await LoadData());
        }
    }

    private Bazilišek? _tungTungTungSahur = null;
    private SKBitmap? _skibidiFrame = null;
    private bool _renderUnavailable = false;

    private Border _patientPanel = default!;
    private Border _componentsPanel = default!;
    private Border _renderPanel = default!;
    private Button _mainMenuBtn = default!;
    private Button _backBtn = default!;
    private Button _exportBtn = default!;
    private Button _copyBtn = default!;
    private Button _previewToggleBtn = default!;
    private bool _previewVisible = false;

    private Label PatientIdLabel = default!;
    private Label DateLabel = default!;
    private Label BodyHeightLabel = default!;
    private Label PelvisWidthLabel = default!;
    private Label ThighLengthLabel = default!;
    private Label WeightLabel = default!;
    private Label BodyStabilityLabel = default!;
    private Label HeadStabilityLabel = default!;
    private Label BedsoreRiskLabel = default!;
    private Label ControlLabel = default!;
    private Label EnvironmentLabel = default!;
    private Label LegsLabel = default!;
    private Label PainLabel = default!;
    private VerticalStackLayout ComponentsLayout = default!;
    private SKCanvasView Canvas = default!;

    private bool _isLandscape;

    private static Color ThemeColor(Color light, Color dark) =>
        Application.Current?.RequestedTheme == AppTheme.Dark ? dark : light;

    public SummaryPage(IAppService appService, NavigationState navState)
    {
        _appService = appService;
        _navState = navState;

        InitializeComponent();
        BuildSharedViews();

        if (!IsVulkanSafe())
            _renderUnavailable = true;

        Console.WriteLine("[Summary] Constructor done");

        Dispatcher.Dispatch(async () => await LoadData());
    }

    private async Task LoadData()
    {
        Console.WriteLine("[Summary] LoadData start");

        var settings = await _appService.GetSettingsAsync();
        if (!settings.RenderingEnabled)
        {
            if (_tungTungTungSahur != null)
            {
                await _tungTungTungSahur.ShutdownAsync();
                _tungTungTungSahur = null;
            }
            _renderUnavailable = true;
        }

        List<ComponentModel> components;

        if (_configurationId > 0)
        {
            // Mode B: load saved configuration from DB
            try
            {
                _loadedConfig = await _appService.GetConfigurationAsync(_configurationId);
                components = await _appService.GetConfigurationComponentsAsync(_configurationId);
            }
            catch
            {
                _loadedConfig = null;
                components = new();
            }
            if (_loadedConfig is not null)
            {
                PatientIdLabel.Text = _loadedConfig.PatientName;
                DateLabel.Text = $"Konfigurace #{_loadedConfig.Id}  |  {_loadedConfig.CreatedAt:dd.MM.yyyy}";
                BodyHeightLabel.Text = $"Terapeut: {_loadedConfig.SpecialistName}";
                PelvisWidthLabel.Text = $"Rodné číslo: {_loadedConfig.PatientBirthNumber}";
                ThighLengthLabel.Text = string.Empty;
                WeightLabel.Text = string.Empty;
                BodyStabilityLabel.Text = string.Empty;
                HeadStabilityLabel.Text = string.Empty;
                BedsoreRiskLabel.Text = string.Empty;
                ControlLabel.Text = string.Empty;
                EnvironmentLabel.Text = string.Empty;
                LegsLabel.Text = string.Empty;
                PainLabel.Text = string.Empty;
            }
            else
            {
                PatientIdLabel.Text = $"Konfigurace #{_configurationId}";
                LoadPatientData(null);
            }
            _copyBtn.IsVisible = true;
        }
        else
        {
            // Mode A: fresh selection from NavigationState
            _loadedConfig = null;
            components = _navState.SelectedComponents;
            var patient = _navState.Patient;
            LoadPatientData(patient);
            _copyBtn.IsVisible = false;
        }

        BuildComponentsList(components);

        if (!_renderUnavailable && _tungTungTungSahur == null)
        {
            try
            {
                _tungTungTungSahur = new Bazilišek("summary", 800, 600);
                _tungTungTungSahur.SetHighQualityTextures(settings.HighQualityTextures);
                _tungTungTungSahur.StartRenderLoop(OnFrameReady);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Summary] Bazilisek init failed: " + ex.Message);
                _tungTungTungSahur = null;
                _renderUnavailable = true;
            }
        }

        if (_tungTungTungSahur != null)
        {
            try
            {
                var allModels = await _appService.GetAllModel3DsAsync();
                var modelMap = allModels
                    .Where(m => m.ComponentId > 0)
                    .GroupBy(m => m.ComponentId)
                    .ToDictionary(g => g.Key, g => g.First());
                var modelsDir = Path.Combine(FileSystem.AppDataDirectory, "models");
                var scene = new List<(string id, string geom, string tex, float scale,
                    float ax, float ay, float az, float rx, float ry, float rz)>();
                foreach (var c in components)
                {
                    if (!modelMap.TryGetValue(c.Id, out var m)) continue;
                    if (string.IsNullOrEmpty(m.FilePath)) continue;
                    var geomPath = Path.Combine(modelsDir, m.FilePath);
                    if (!File.Exists(geomPath)) continue;
                    var texPath = string.IsNullOrEmpty(m.TextureId) ? "" : Path.Combine(modelsDir, m.TextureId);
                    if (!string.IsNullOrEmpty(texPath) && !File.Exists(texPath)) texPath = "";
                    scene.Add(($"model_{m.ComponentId}", geomPath, texPath, m.Scale,
                        m.AnchorX, m.AnchorY, m.AnchorZ, m.RotationX, m.RotationY, m.RotationZ));
                }
                Console.WriteLine($"[Summary] LoadData: scene built ({scene.Count} models)");
                await _tungTungTungSahur.RebuildSceneAsync(scene);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Summary] RebuildScene exception: " + ex.Message);
                _renderUnavailable = true;
            }
        }

        Console.WriteLine("[Summary] LoadData done");
    }

    private void OnFrameReady(SKBitmap frame)
    {
        var old = _skibidiFrame;
        _skibidiFrame = frame;
        old?.Dispose();
        MainThread.BeginInvokeOnMainThread(() => Canvas?.InvalidateSurface());
    }

    private void LoadPatientData(UserInput? patient)
    {
        if (patient is null)
        {
            DateLabel.Text = "";
            BodyHeightLabel.Text = "";
            PelvisWidthLabel.Text = "";
            ThighLengthLabel.Text = "";
            WeightLabel.Text = "";
            BodyStabilityLabel.Text = "";
            HeadStabilityLabel.Text = "";
            BedsoreRiskLabel.Text = "";
            ControlLabel.Text = "";
            EnvironmentLabel.Text = "";
            LegsLabel.Text = "";
            PainLabel.Text = "";
            return;
        }

        PatientIdLabel.Text = patient.patientIdentificator;
        DateLabel.Text = $"{patient.Date:dd.MM.yyyy}";
        BodyHeightLabel.Text = $"Výška trupu: {patient.BodyHeight} cm";
        PelvisWidthLabel.Text = $"Šířka pánve: {patient.PelvisWidth} cm";
        ThighLengthLabel.Text = $"Délka stehna: {patient.ThighLength} cm";
        WeightLabel.Text = $"Hmotnost: {patient.Weight} kg";
        BodyStabilityLabel.Text = $"Stabilita trupu: {patient.BodyStability}";
        HeadStabilityLabel.Text = $"Kontrola hlavy: {(patient.HeadStability ? "Ano" : "Ne")}";
        BedsoreRiskLabel.Text = $"Riziko dekubitů: {patient.BedsoreRisk}";
        ControlLabel.Text = $"Ovládání rukou: {patient.Control}";
        EnvironmentLabel.Text = $"Prostředí: {patient.Environment}";
        LegsLabel.Text = $"Dolní končetiny: {(patient.Legs ? "Ano" : "Ne")}";
        PainLabel.Text = $"Bolesti a únava: {patient.Pain}";
    }

    private void BuildComponentsList(List<ComponentModel> components)
    {
        ComponentsLayout.Children.Clear();

        ComponentsLayout.Children.Add(new Label
        {
            Text = "Vybrané komponenty",
            FontAttributes = FontAttributes.Bold,
            FontSize = 16,
            Margin = new Thickness(0, 0, 0, 8)
        });

        if (!components.Any())
        {
            ComponentsLayout.Children.Add(new Label
            {
                Text = "Žádné komponenty",
                TextColor = Colors.Gray,
                FontSize = 13
            });
            return;
        }

        foreach (var c in components)
        {
            var row = new VerticalStackLayout { Spacing = 1, Margin = new Thickness(0, 4, 0, 0) };
            row.Children.Add(new Label { Text = $"[{c.Id}] {c.Name}", FontSize = 13, FontAttributes = FontAttributes.Bold });
            if (!string.IsNullOrEmpty(c.Manufacturer))
            {
                row.Children.Add(new Label
                {
                    Text = $"{c.Manufacturer}  {c.ManufacturerCode}".TrimEnd(),
                    FontSize = 11,
                    TextColor = Colors.Gray
                });
            }
            ComponentsLayout.Children.Add(row);
            ComponentsLayout.Children.Add(new BoxView
            {
                HeightRequest = 1,
                Color = ThemeColor(Color.FromArgb("#E0E0E0"), Color.FromArgb("#3D3D3D")),
                Margin = new Thickness(0, 4)
            });
        }
    }

    private void BuildSharedViews()
    {
        PatientIdLabel = new Label { FontAttributes = FontAttributes.Bold, FontSize = 14 };
        DateLabel = new Label { FontSize = 13, TextColor = Colors.Gray };
        BodyHeightLabel = new Label { FontSize = 13 };
        PelvisWidthLabel = new Label { FontSize = 13 };
        ThighLengthLabel = new Label { FontSize = 13 };
        WeightLabel = new Label { FontSize = 13 };
        BodyStabilityLabel = new Label { FontSize = 13 };
        HeadStabilityLabel = new Label { FontSize = 13 };
        BedsoreRiskLabel = new Label { FontSize = 13 };
        ControlLabel = new Label { FontSize = 13 };
        EnvironmentLabel = new Label { FontSize = 13 };
        LegsLabel = new Label { FontSize = 13 };
        PainLabel = new Label { FontSize = 13 };

        _patientPanel = new Border
        {
            Padding = new Thickness(15),
            StrokeThickness = 1,
            Stroke = ThemeColor(Color.FromArgb("#E0E0E0"), Color.FromArgb("#3D3D3D")),
            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Spacing = 4,
                    Children =
                    {
                        new Label { Text = "Pacient", FontAttributes = FontAttributes.Bold, FontSize = 16, Margin = new Thickness(0,0,0,8) },
                        PatientIdLabel,
                        DateLabel,
                        new BoxView { HeightRequest = 1, Color = ThemeColor(Color.FromArgb("#E0E0E0"), Color.FromArgb("#3D3D3D")), Margin = new Thickness(0,6) },
                        BodyHeightLabel,
                        PelvisWidthLabel,
                        ThighLengthLabel,
                        WeightLabel,
                        new BoxView { HeightRequest = 1, Color = ThemeColor(Color.FromArgb("#E0E0E0"), Color.FromArgb("#3D3D3D")), Margin = new Thickness(0,6) },
                        BodyStabilityLabel,
                        HeadStabilityLabel,
                        BedsoreRiskLabel,
                        ControlLabel,
                        EnvironmentLabel,
                        LegsLabel,
                        PainLabel,
                    }
                }
            }
        };

        ComponentsLayout = new VerticalStackLayout { Spacing = 4 };

        _componentsPanel = new Border
        {
            Padding = new Thickness(15),
            StrokeThickness = 1,
            Stroke = ThemeColor(Color.FromArgb("#E0E0E0"), Color.FromArgb("#3D3D3D")),
            Content = new ScrollView { Content = ComponentsLayout }
        };

        Canvas = new SKCanvasView();
        Canvas.PaintSurface += OnPaintSurface;

        var boxView = new BoxView { Color = ThemeColor(Colors.White, Color.FromArgb("#1E1E1E")) };
        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnPanUpdated;
        boxView.GestureRecognizers.Add(pan);

        var renderGrid = new Grid();
        renderGrid.Children.Add(boxView);
        renderGrid.Children.Add(Canvas);

        _renderPanel = new Border
        {
            Padding = new Thickness(15),
            StrokeThickness = 1,
            Stroke = ThemeColor(Color.FromArgb("#E0E0E0"), Color.FromArgb("#3D3D3D")),
            Content = renderGrid,
            IsVisible = false
        };

        _previewToggleBtn = new Button
        {
            Text = "▶  Zobrazit náhled",
            BackgroundColor = ThemeColor(Color.FromArgb("#F0F0F0"), Color.FromArgb("#2D2D2D")),
            HorizontalOptions = LayoutOptions.Fill,
            FontSize = 13
        };
        _previewToggleBtn.Clicked += OnPreviewToggleClicked;

        _mainMenuBtn = new Button
        {
            Text = "Hlavní menu",
            HorizontalOptions = LayoutOptions.Fill
        };
        _mainMenuBtn.Clicked += OnMainMenuClicked;

        _backBtn = new Button
        {
            Text = "Zpět",
            HorizontalOptions = LayoutOptions.Fill
        };
        _backBtn.Clicked += OnBackClicked;

        _exportBtn = new Button
        {
            Text = "Exportovat",
            HorizontalOptions = LayoutOptions.Fill
        };
        _exportBtn.Clicked += OnExportClicked;

        _copyBtn = new Button
        {
            Text = "Kopírovat a upravit",
            HorizontalOptions = LayoutOptions.Fill,
            IsVisible = false
        };
        _copyBtn.Clicked += OnCopyClicked;
    }

    private void OnPreviewToggleClicked(object? sender, EventArgs e)
    {
        _previewVisible = !_previewVisible;
        UpdatePortraitMode();
    }

    private void UpdatePortraitMode()
    {
        _patientPanel.IsVisible = !_previewVisible;
        _componentsPanel.IsVisible = !_previewVisible;
        _renderPanel.IsVisible = _previewVisible;
        _previewToggleBtn.Text = _previewVisible
            ? "◀  Zpět na přehled"
            : "▶  Zobrazit náhled";
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width <= 0 || height <= 0) return;
        if (_patientPanel == null) return;

        bool landscape = width > height;
        if (landscape == _isLandscape && MainContent.Content != null) return;

        _isLandscape = landscape;
        DetachSharedViews();
        MainContent.Content = landscape ? BuildLandscapeLayout() : BuildPortraitLayout();
    }

    private void DetachSharedViews()
    {
        View[] shared =
        [
            _patientPanel, _componentsPanel, _renderPanel,
            _previewToggleBtn, _mainMenuBtn, _backBtn, _copyBtn, _exportBtn
        ];

        foreach (var view in shared)
        {
            if (view is null) continue;
            if (view.Parent is Layout layout)
                layout.Remove(view);
            else if (view.Parent is Grid grid)
                grid.Children.Remove(view);
            else if (view.Parent is ContentView cv)
                cv.Content = null;
        }
    }

    private View BuildLandscapeLayout()
    {
        _patientPanel.IsVisible = true;
        _componentsPanel.IsVisible = true;
        _renderPanel.IsVisible = true;

        _renderPanel.HeightRequest = -1;
        _renderPanel.WidthRequest = -1;
        _renderPanel.HorizontalOptions = LayoutOptions.Fill;
        _patientPanel.HeightRequest = -1;
        _componentsPanel.HeightRequest = -1;

        var outer = new Grid
        {
            Padding = new Thickness(20),
            RowSpacing = 15,
            ColumnSpacing = 15,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
            },
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(new GridLength(2, GridUnitType.Star)),
            }
        };

        Grid.SetRow(_patientPanel, 0); Grid.SetColumn(_patientPanel, 0);
        Grid.SetRow(_componentsPanel, 0); Grid.SetColumn(_componentsPanel, 1);
        Grid.SetRow(_renderPanel, 0); Grid.SetColumn(_renderPanel, 2);
        outer.Children.Add(_patientPanel);
        outer.Children.Add(_componentsPanel);
        outer.Children.Add(_renderPanel);

        var btnGrid = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
            }
        };
        Grid.SetColumn(_mainMenuBtn, 0);
        Grid.SetColumn(_backBtn, 1);
        Grid.SetColumn(_copyBtn, 2);
        Grid.SetColumn(_exportBtn, 3);
        btnGrid.Children.Add(_mainMenuBtn);
        btnGrid.Children.Add(_backBtn);
        btnGrid.Children.Add(_copyBtn);
        btnGrid.Children.Add(_exportBtn);

        Grid.SetRow(btnGrid, 1);
        Grid.SetColumnSpan(btnGrid, 3);
        outer.Children.Add(btnGrid);

        return outer;
    }

    private View BuildPortraitLayout()
    {
        _renderPanel.HeightRequest = 300;
        _renderPanel.WidthRequest = -1;
        _renderPanel.HorizontalOptions = LayoutOptions.Fill;
        _patientPanel.HeightRequest = 220;
        _componentsPanel.HeightRequest = 400;

        UpdatePortraitMode();

        var outer = new Grid
        {
            Padding = new Thickness(20),
            RowSpacing = 15,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
            }
        };

        Grid.SetRow(_patientPanel, 0);
        Grid.SetRow(_componentsPanel, 1);
        Grid.SetRow(_renderPanel, 0);
        Grid.SetRow(_previewToggleBtn, 2);
        outer.Children.Add(_patientPanel);
        outer.Children.Add(_componentsPanel);
        outer.Children.Add(_renderPanel);
        outer.Children.Add(_previewToggleBtn);

        var btnGrid = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
            }
        };
        Grid.SetColumn(_mainMenuBtn, 0);
        Grid.SetColumn(_backBtn, 1);
        Grid.SetColumn(_copyBtn, 2);
        Grid.SetColumn(_exportBtn, 3);
        btnGrid.Children.Add(_mainMenuBtn);
        btnGrid.Children.Add(_backBtn);
        btnGrid.Children.Add(_copyBtn);
        btnGrid.Children.Add(_exportBtn);

        Grid.SetRow(btnGrid, 3);
        outer.Children.Add(btnGrid);

        return new ScrollView { Content = outer };
    }

    private static bool IsVulkanSafe() => true;

    void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(Application.Current?.RequestedTheme == AppTheme.Dark ? SKColors.Black : SKColors.White);

        if (_renderUnavailable)
        {
            using var paint = new SKPaint { Color = SKColors.Gray, TextSize = 14, IsAntialias = true };
            canvas.DrawText("3D náhled není k dispozici", 20, e.Info.Height / 2f, paint);
            return;
        }

        if (_skibidiFrame == null) return;
        canvas.DrawBitmap(_skibidiFrame, e.Info.Rect);
    }

    private async void OnMainMenuClicked(object sender, EventArgs e)
    {
        Console.WriteLine("[Summary] Navigating away (mainMenu)");
        if (_tungTungTungSahur != null)
        {
            await _tungTungTungSahur.ShutdownAsync();
            _tungTungTungSahur = null;
        }
        await Shell.Current.GoToAsync("mainPage");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        Console.WriteLine("[Summary] Navigating away (back)");
        if (_tungTungTungSahur != null)
        {
            await _tungTungTungSahur.ShutdownAsync();
            _tungTungTungSahur = null;
        }
        await Shell.Current.GoToAsync("..");
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        _exportBtn.IsEnabled = false;
        _exportBtn.Text = "Generuji...";

        try
        {
            int configId = _configurationId != 0 ? _configurationId : (_navState.ConfigurationId ?? 0);
            if (configId == 0)
            {
                await DisplayAlert("Chyba", "Konfigurace nebyla uložena. Vrať se zpět a klikni Pokračovat znovu.", "OK");
                return;
            }

            var pdfBytes = await _appService.ExportConfigurationAsync(configId);

            var filePath = Path.Combine(FileSystem.CacheDirectory, $"konfigurace_{configId}.pdf");
            await File.WriteAllBytesAsync(filePath, pdfBytes);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Konfigurace vozíku",
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Chyba exportu", ex.Message, "OK");
        }
        finally
        {
            _exportBtn.IsEnabled = true;
            _exportBtn.Text = "Exportovat";
        }
    }

    private Point _panStart;
    private bool _panStartSet = false;

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _panStartSet = false;
                break;

            case GestureStatus.Running:
                if (!_panStartSet)
                {
                    _panStart = new Point(e.TotalX, e.TotalY);
                    _panStartSet = true;
                    break;
                }
                var delta = new Point(e.TotalX - _panStart.X, e.TotalY - _panStart.Y);
                _panStart = new Point(e.TotalX, e.TotalY);
                _tungTungTungSahur?.PomaluSanjski(-(float)delta.Y, (float)delta.X);
                break;
        }
    }

    private async void OnCopyClicked(object? sender, EventArgs e)
    {
        if (_configurationId <= 0 || _loadedConfig is null) return;

        try
        {
            var components = await _appService.GetConfigurationComponentsAsync(_configurationId);
            _navState.SelectedComponents = components;

            if (_loadedConfig.PatientMeasurementId > 0)
            {
                var measurement = await _appService.GetMeasurementByIdAsync(_loadedConfig.PatientMeasurementId);
                if (measurement is not null)
                    _navState.ActiveMeasurement = measurement;
            }

            Console.WriteLine("[Summary] Navigating away (copy&edit)");
            if (_tungTungTungSahur != null)
            {
                await _tungTungTungSahur.ShutdownAsync();
                _tungTungTungSahur = null;
            }
            await Shell.Current.GoToAsync("wheelchairConfiguratorPage");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Chyba", ex.Message, "OK");
        }
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        if (_tungTungTungSahur != null)
        {
            await _tungTungTungSahur.ShutdownAsync();
            _tungTungTungSahur = null;
        }
    }
}
