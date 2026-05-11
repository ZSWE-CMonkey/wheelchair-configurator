using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using WheelchairConfigurator.Helpers;
using WheelchairConfigurator.ServiceLayer.Interfaces;
using WheelchairConfigurator.ServiceLayer.Models;
using WheelchairConfigurator.Services;

namespace WheelchairConfigurator.Pages;

public partial class WheelchairConfiguratorPage : ContentPage
{
    private readonly IAppService _appService;
    private readonly NavigationState _navState;

    private List<CategoryModel> _categories = new();
    private readonly Dictionary<int, ComponentModel?> _selectedComponents = [];
    private readonly Dictionary<int, Border?> _selectedBorders = [];

    private Bazilišek? _tungTungTungSahur = null;
    private CancellationTokenSource _cts = default!;
    private SKBitmap? _skibidiFrame = null;
    private readonly object _mutex = new();
    private bool _renderUnavailable = false;

    private Border _patientPanel = default!;
    private Border _componentsPanel = default!;
    private Border _renderPanel = default!;
    private Button _backBtn = default!;
    private Button _continueBtn = default!;
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

    public WheelchairConfiguratorPage(IAppService appService, NavigationState navState)
    {
        _appService = appService;
        _navState = navState;

        InitializeComponent();

        if (!IsVulkanSafe())
            _renderUnavailable = true;

        BuildSharedViews();
        StartRenderLoop();

        Dispatcher.Dispatch(async () => await LoadRealData());
    }

    ~WheelchairConfiguratorPage()
    {
        StopRenderLoop();
    }

    private async Task LoadRealData()
    {
        var settings = await _appService.GetSettingsAsync();
        if (!settings.RenderingEnabled)
        {
            _tungTungTungSahur?.ZabijBaziliška();
            _tungTungTungSahur = null;
            _renderUnavailable = true;
        }

        var patient = _navState.Patient;

        PatientIdLabel.Text = patient?.patientIdentificator ?? "Nový pacient";
        DateLabel.Text = patient is not null ? $"Datum: {patient.Date:dd.MM.yyyy}" : "";
        BodyHeightLabel.Text = patient is not null ? $"Výška trupu: {patient.BodyHeight} cm" : "";
        PelvisWidthLabel.Text = patient is not null ? $"Šířka pánve: {patient.PelvisWidth} cm" : "";
        ThighLengthLabel.Text = patient is not null ? $"Délka stehna: {patient.ThighLength} cm" : "";
        WeightLabel.Text = patient is not null ? $"Hmotnost: {patient.Weight} kg" : "";
        BodyStabilityLabel.Text = patient is not null ? $"Stabilita trupu: {patient.BodyStability}" : "";
        HeadStabilityLabel.Text = patient is not null ? $"Kontrola hlavy: {(patient.HeadStability ? "Ano" : "Ne")}" : "";
        BedsoreRiskLabel.Text = patient is not null ? $"Riziko dekubitů: {patient.BedsoreRisk}" : "";
        ControlLabel.Text = patient is not null ? $"Ovládání rukou: {patient.Control}" : "";
        EnvironmentLabel.Text = patient is not null ? $"Prostředí: {patient.Environment}" : "";
        LegsLabel.Text = patient is not null ? $"Dolní končetiny: {(patient.Legs ? "Ano" : "Ne")}" : "";
        PainLabel.Text = patient is not null ? $"Bolesti a únava: {patient.Pain}" : "";

        try
        {
            _categories = await _appService.GetCategoriesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("[WheelchairConfiguratorPage] Failed to load categories: " + ex.Message);
            _categories = new();
        }

        _selectedComponents.Clear();
        foreach (var cat in _categories)
            _selectedComponents[cat.Id] = null;

        await BuildComponentPanels();

        if (!_renderUnavailable && _tungTungTungSahur == null)
        {
            List<Model3DModel> models3D;
            try { models3D = await _appService.GetAllModel3DsAsync(); }
            catch { models3D = new(); }
            await Task.Run(() => InitBazilisek(models3D));
        }
    }

    private void InitBazilisek(List<Model3DModel> models3D)
    {
        var modelsDir = Path.Combine(FileSystem.AppDataDirectory, "models");
        try
        {
            var baz = new Bazilišek("app", 800, 600);
            bool hasModels = false;

            foreach (var m in models3D)
            {
                if (string.IsNullOrEmpty(m.FilePath) || string.IsNullOrEmpty(m.TextureId)) continue;
                var daePath = Path.Combine(modelsDir, m.FilePath);
                var ktxPath = Path.Combine(modelsDir, m.TextureId);
                if (!File.Exists(daePath) || !File.Exists(ktxPath)) continue;
                baz.BrmBrmPatatimZesouboru($"model_{m.ComponentId}", daePath, ktxPath);
                hasModels = true;
            }

            if (!hasModels)
                baz.BrmBrmPatatim("models/test");

            baz.OtevřítKomnatu();
            _tungTungTungSahur = baz;
        }
        catch
        {
            _renderUnavailable = true;
        }
    }

    private async Task BuildComponentPanels()
    {
        ComponentsLayout.Children.Clear();
        var patientProfile = BuildPatientProfile();

        foreach (var category in _categories)
        {
            ComponentsLayout.Children.Add(new Label
            {
                Text = category.Name,
                FontAttributes = FontAttributes.Bold,
                FontSize = 15,
                Margin = new Thickness(0, 10, 0, 4)
            });

            List<ComponentModel> components;
            try
            {
                components = await _appService.GetComponentsAsync(category.Id, patientProfile);
            }
            catch
            {
                components = new();
            }

            if (!components.Any())
            {
                ComponentsLayout.Children.Add(new Label
                {
                    Text = "Žádné komponenty",
                    FontSize = 12,
                    TextColor = Colors.Gray
                });
                continue;
            }

            foreach (var component in components)
            {
                var bgColor = component.IsIncompatible
                    ? ThemeColor(Color.FromArgb("#F5F5F5"), Color.FromArgb("#3A3A3A"))
                    : component.IsRecommended
                        ? ThemeColor(Color.FromArgb("#E8F5E9"), Color.FromArgb("#1B3A1F"))
                        : ThemeColor(Colors.White, Color.FromArgb("#2D2D2D"));

                var textColor = component.IsIncompatible ? Colors.Gray : ThemeColor(Colors.Black, Colors.White);
                var subColor = component.IsIncompatible ? Colors.Gray : ThemeColor(Color.FromArgb("#555555"), Color.FromArgb("#AAAAAA"));

                var nameLabel = new Label
                {
                    Text = $"[{component.Id}] {component.Name}",
                    FontSize = 13,
                    TextColor = textColor
                };
                var manufacturerLabel = new Label
                {
                    Text = string.IsNullOrEmpty(component.Manufacturer)
                        ? string.Empty
                        : $"{component.Manufacturer}  {component.ManufacturerCode}",
                    FontSize = 11,
                    TextColor = subColor,
                    IsVisible = !string.IsNullOrEmpty(component.Manufacturer)
                };
                var componentContent = new VerticalStackLayout { Spacing = 2 };
                componentContent.Children.Add(nameLabel);
                componentContent.Children.Add(manufacturerLabel);

                var border = new Border
                {
                    Padding = new Thickness(10, 8),
                    Margin = new Thickness(0, 2),
                    StrokeThickness = 1,
                    Stroke = Colors.LightGray,
                    BackgroundColor = bgColor,
                    Content = componentContent
                };

                if (!component.IsIncompatible)
                {
                    var capturedComponent = component;
                    var capturedBorder = border;
                    var capturedCategoryId = category.Id;
                    var tap = new TapGestureRecognizer();
                    tap.Tapped += (s, e) => OnComponentTapped(capturedComponent, capturedBorder, capturedCategoryId);
                    border.GestureRecognizers.Add(tap);
                }

                ComponentsLayout.Children.Add(border);
            }
        }
    }

    private void OnComponentTapped(ComponentModel component, Border tappedBorder, int categoryId)
    {
        if (_selectedBorders.TryGetValue(categoryId, out var prev) && prev is not null)
        {
            prev.Stroke = Colors.LightGray;
            prev.BackgroundColor = ThemeColor(Colors.White, Color.FromArgb("#2D2D2D"));
        }

        tappedBorder.Stroke = Color.FromArgb("#512BD4");
        tappedBorder.BackgroundColor = Color.FromArgb("#EDE8FC");

        _selectedBorders[categoryId] = tappedBorder;
        _selectedComponents[categoryId] = component;

        _continueBtn.IsEnabled = _categories.Count > 0
            && _selectedComponents.Count == _categories.Count
            && _selectedComponents.Values.All(c => c is not null);
    }

    private PatientProfileModel? BuildPatientProfile()
    {
        var p = _navState.Patient;
        if (p is null) return null;
        return new PatientProfileModel
        {
            PelvisWidthCm = (int)p.PelvisWidth,
            ThighLengthCm = (int)p.ThighLength,
            LowerLegLengthCm = 0,
            WeightKg = (int)p.Weight,
            TrunkStability = p.BodyStability switch
            {
                "Dobrá" => TrunkStabilityLevel.Good,
                "Střední" => TrunkStabilityLevel.Fair,
                _ => TrunkStabilityLevel.Poor
            },
            HasPressureSoresRisk = p.BedsoreRisk == "Vysoké"
        };
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

        ComponentsLayout = new VerticalStackLayout { Spacing = 2 };

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

        _backBtn = new Button
        {
            Text = "Zpět",
            BackgroundColor = Colors.Red,
            HorizontalOptions = LayoutOptions.Fill
        };
        _backBtn.Clicked += OnBackClicked;

        _continueBtn = new Button
        {
            Text = "Pokračovat",
            IsEnabled = false,
            HorizontalOptions = LayoutOptions.Fill
        };
        _continueBtn.Clicked += OnContinueClicked;
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
            ? "◀  Zpět na výběr"
            : "▶  Zobrazit náhled";
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width <= 0 || height <= 0) return;

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
            _previewToggleBtn, _backBtn, _continueBtn
        ];

        foreach (var view in shared)
        {
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
            },
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
            }
        };
        Grid.SetColumn(_backBtn, 0);
        Grid.SetColumn(_continueBtn, 1);
        btnGrid.Children.Add(_backBtn);
        btnGrid.Children.Add(_continueBtn);

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
        }
        };
        Grid.SetColumn(_backBtn, 0);
        Grid.SetColumn(_continueBtn, 1);
        btnGrid.Children.Add(_backBtn);
        btnGrid.Children.Add(_continueBtn);

        Grid.SetRow(btnGrid, 3);
        outer.Children.Add(btnGrid);

        return new ScrollView { Content = outer };
    }

    private static bool IsVulkanSafe() => true;

    private void StartRenderLoop()
    {
        _cts = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                if (_tungTungTungSahur == null) { await Task.Yield(); continue; }

                try
                {
                    _tungTungTungSahur.ToJáJsemVypustilBaziliška();
                    SKBitmap? frame = _tungTungTungSahur.JaJsemHagrid();
                    if (frame != null)
                    {
                        _skibidiFrame = frame;
                        MainThread.BeginInvokeOnMainThread(() => Canvas.InvalidateSurface());
                    }
                }
                catch
                {
                    _tungTungTungSahur = null;
                    _renderUnavailable = true;
                    MainThread.BeginInvokeOnMainThread(() => Canvas.InvalidateSurface());
                    break;
                }
                await Task.Delay(6);
            }
        });
    }

    private void StopRenderLoop()
    {
        _cts?.Cancel();
        _tungTungTungSahur = null;
    }

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
        lock (_mutex)
            canvas.DrawBitmap(_skibidiFrame, e.Info.Rect);
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        _tungTungTungSahur?.ZabijBaziliška();
        await Shell.Current.GoToAsync("..");
    }

    private async void OnContinueClicked(object sender, EventArgs e)
    {
        _navState.SelectedComponents = _selectedComponents.Values
            .Where(c => c is not null)
            .Cast<ComponentModel>()
            .ToList();

        _tungTungTungSahur?.ZabijBaziliška();
        await Shell.Current.GoToAsync("summaryPage");
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
}
