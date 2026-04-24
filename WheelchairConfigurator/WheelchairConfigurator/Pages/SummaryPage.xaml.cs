using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using WheelchairConfigurator.Helpers;

namespace WheelchairConfigurator.Pages;

public partial class SummaryPage : ContentPage
{
    private readonly PatientData _patient = new()
    {
        PatientIdentificator = "PAT-001",
        BodyHeight = 175,
        PelvisWidth = 35,
        ThighLength = 45,
        Weight = 70,
        BodyStability = "Dobrá",
        HeadStability = true,
        BedsoreRisk = "Nízké",
        Control = "Ano",
        Environment = "Kombinace",
        Legs = true,
        Pain = "Nízké",
        Date = new DateTime(2024, 1, 15)
    };

    private readonly List<ComponentMock> _selectedComponents =
    [
        new ComponentMock { Id = "RAM-001", Name = "Rám Standard",      Category = "Rám",     IsAvailable = true },
        new ComponentMock { Id = "MOT-001", Name = "Motor 250W",        Category = "Motor",   IsAvailable = true },
        new ComponentMock { Id = "BAT-002", Name = "Baterie 20Ah",      Category = "Baterie", IsAvailable = true },
        new ComponentMock { Id = "POH-001", Name = "Pohon Přímý",       Category = "Pohon",   IsAvailable = true },
        new ComponentMock { Id = "SED-002", Name = "Sedák Ortopedický", Category = "Sedák",   IsAvailable = true },
        new ComponentMock { Id = "OPE-002", Name = "Opěrka Sklopná",    Category = "Opěrka",  IsAvailable = true },
    ];

    private Bazilišek? _tungTungTungSahur = null;
    private CancellationTokenSource _cts = default!;
    private SKBitmap? _skibidiFrame = null;
    private readonly object _mutex = new();

    private Border _patientPanel = default!;
    private Border _componentsPanel = default!;
    private Border _renderPanel = default!;
    private Button _mainMenuBtn = default!;
    private Button _backBtn = default!;
    private Button _exportBtn = default!;
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

    public SummaryPage()
    {
        InitializeComponent();

        BuildSharedViews();
        LoadPatientData();
        BuildComponentsList();

        _tungTungTungSahur = new Bazilišek("app", 800, 600);
        _tungTungTungSahur.BrmBrmPatatim("models/test");
        _tungTungTungSahur.OtevřítKomnatu();
        _tungTungTungSahur.ToJáJsemVypustilBaziliška();
        _skibidiFrame = _tungTungTungSahur.JaJsemHagrid();

        StartRenderLoop();
    }

    ~SummaryPage()
    {
        StopRenderLoop();
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
            Stroke = Color.FromArgb("#E0E0E0"),
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
                        new BoxView { HeightRequest = 1, Color = Color.FromArgb("#E0E0E0"), Margin = new Thickness(0,6) },
                        BodyHeightLabel,
                        PelvisWidthLabel,
                        ThighLengthLabel,
                        WeightLabel,
                        new BoxView { HeightRequest = 1, Color = Color.FromArgb("#E0E0E0"), Margin = new Thickness(0,6) },
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
            Stroke = Color.FromArgb("#E0E0E0"),
            Content = new ScrollView { Content = ComponentsLayout }
        };

        Canvas = new SKCanvasView();
        Canvas.PaintSurface += OnPaintSurface;

        var boxView = new BoxView { Color = Colors.White };
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
            Stroke = Color.FromArgb("#E0E0E0"),
            Content = renderGrid,
            IsVisible = false
        };

        _previewToggleBtn = new Button
        {
            Text = "▶  Zobrazit náhled",
            BackgroundColor = Color.FromArgb("#F0F0F0"),
            TextColor = Colors.Black,
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
            _previewToggleBtn, _mainMenuBtn, _backBtn, _exportBtn
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
            }
        };
        Grid.SetColumn(_mainMenuBtn, 0);
        Grid.SetColumn(_backBtn, 1);
        Grid.SetColumn(_exportBtn, 2);
        btnGrid.Children.Add(_mainMenuBtn);
        btnGrid.Children.Add(_backBtn);
        btnGrid.Children.Add(_exportBtn);

        Grid.SetRow(btnGrid, 1);
        Grid.SetColumnSpan(btnGrid, 3);
        outer.Children.Add(btnGrid);

        return outer;
    }

    private View BuildPortraitLayout()
    {
        _renderPanel.HeightRequest = 330;
        _renderPanel.WidthRequest = 430;
        _renderPanel.HorizontalOptions = LayoutOptions.Center;
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
            }
        };
        Grid.SetColumn(_mainMenuBtn, 0);
        Grid.SetColumn(_backBtn, 1);
        Grid.SetColumn(_exportBtn, 2);
        btnGrid.Children.Add(_mainMenuBtn);
        btnGrid.Children.Add(_backBtn);
        btnGrid.Children.Add(_exportBtn);

        Grid.SetRow(btnGrid, 3);
        outer.Children.Add(btnGrid);

        return new ScrollView { Content = outer };
    }

    private void StartRenderLoop()
    {
        _cts = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                if (_tungTungTungSahur == null) { await Task.Yield(); continue; }

                _tungTungTungSahur.ToJáJsemVypustilBaziliška();
                SKBitmap? frame = _tungTungTungSahur.JaJsemHagrid();

                if (frame == null) { await Task.Yield(); continue; }

                _skibidiFrame = frame;
                MainThread.BeginInvokeOnMainThread(() => Canvas.InvalidateSurface());
                await Task.Delay(6);
            }
        });
    }

    private void StopRenderLoop()
    {
        _cts?.Cancel();
        _cts = null;
        _tungTungTungSahur = null;
    }

    void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        if (_skibidiFrame == null) return;
        var canvas = e.Surface.Canvas;
        canvas.Clear();
        lock (_mutex)
            canvas.DrawBitmap(_skibidiFrame, e.Info.Rect);
    }

    private void LoadPatientData()
    {
        PatientIdLabel.Text = _patient.PatientIdentificator;
        DateLabel.Text = $"{_patient.Date:dd.MM.yyyy}";
        BodyHeightLabel.Text = $"Výška trupu: {_patient.BodyHeight} cm";
        PelvisWidthLabel.Text = $"Šířka pánve: {_patient.PelvisWidth} cm";
        ThighLengthLabel.Text = $"Délka stehna: {_patient.ThighLength} cm";
        WeightLabel.Text = $"Hmotnost: {_patient.Weight} kg";
        BodyStabilityLabel.Text = $"Stabilita trupu: {_patient.BodyStability}";
        HeadStabilityLabel.Text = $"Kontrola hlavy: {(_patient.HeadStability ? "Ano" : "Ne")}";
        BedsoreRiskLabel.Text = $"Riziko dekubitů: {_patient.BedsoreRisk}";
        ControlLabel.Text = $"Ovládání rukou: {_patient.Control}";
        EnvironmentLabel.Text = $"Prostředí: {_patient.Environment}";
        LegsLabel.Text = $"Dolní končetiny: {(_patient.Legs ? "Ano" : "Ne")}";
        PainLabel.Text = $"Bolesti a únava: {_patient.Pain}";
    }

    private void BuildComponentsList()
    {
        ComponentsLayout.Children.Clear();

        ComponentsLayout.Children.Add(new Label
        {
            Text = "Vybrané komponenty",
            FontAttributes = FontAttributes.Bold,
            FontSize = 16,
            Margin = new Thickness(0, 0, 0, 8)
        });

        foreach (var category in ComponentCategories.All)
        {
            var component = _selectedComponents.FirstOrDefault(c => c.Category == category);

            ComponentsLayout.Children.Add(new Label
            {
                Text = category,
                FontAttributes = FontAttributes.Bold,
                FontSize = 13,
                TextColor = Color.FromArgb("#512BD4"),
                Margin = new Thickness(0, 8, 0, 2)
            });

            ComponentsLayout.Children.Add(new Label
            {
                Text = component?.Name ?? "—",
                FontSize = 13,
                TextColor = component is not null ? Colors.Black : Colors.Gray
            });

            ComponentsLayout.Children.Add(new BoxView
            {
                HeightRequest = 1,
                Color = Color.FromArgb("#E0E0E0"),
                Margin = new Thickness(0, 4)
            });
        }
    }

    private async void OnMainMenuClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("mainPage");

    private async void OnBackClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("wheelchairConfiguratorPage");

    private async void OnExportClicked(object sender, EventArgs e)
    {
        // Here will be exporting to pdf
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

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopRenderLoop();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_cts == null || _cts.IsCancellationRequested)
            StartRenderLoop();
    }

}