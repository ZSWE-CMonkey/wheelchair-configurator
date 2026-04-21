using Microsoft.Maui.Dispatching;
using WheelchairConfigurator.Helpers;

namespace WheelchairConfigurator.Pages;

[QueryProperty(nameof(WheelchairId), "wheelchairId")]
public partial class WheelchairConfiguratorPage : ContentPage
{
    private string _wheelchairId = string.Empty;

    public string WheelchairId
    {
        get => _wheelchairId;
        set
        {
            _wheelchairId = value;
            // TODO: naèíst reálná data podle ID
            // Skibidi zachod lol xddddddddddddddddddddddddddddddddddddddddddddddddddddddddd
            LoadMockData();
        }
    }

    // Mock pacienta
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

    // Mock komponent
    private readonly List<ComponentMock> _components =
    [
        new ComponentMock { Id = "RAM-001", Name = "Rám Standard",   Category = "Rám",     IsAvailable = true  },
        new ComponentMock { Id = "RAM-002", Name = "Rám Sport",      Category = "Rám",     IsAvailable = true  },
        new ComponentMock { Id = "RAM-003", Name = "Rám Lehký",      Category = "Rám",     IsAvailable = false },

        new ComponentMock { Id = "MOT-001", Name = "Motor 250W",     Category = "Motor",   IsAvailable = true  },
        new ComponentMock { Id = "MOT-002", Name = "Motor 500W",     Category = "Motor",   IsAvailable = true  },
        new ComponentMock { Id = "MOT-003", Name = "Motor 750W",     Category = "Motor",   IsAvailable = false },

        new ComponentMock { Id = "BAT-001", Name = "Baterie 10Ah",   Category = "Baterie", IsAvailable = true  },
        new ComponentMock { Id = "BAT-002", Name = "Baterie 20Ah",   Category = "Baterie", IsAvailable = true  },
        new ComponentMock { Id = "BAT-003", Name = "Baterie 30Ah",   Category = "Baterie", IsAvailable = false },

        new ComponentMock { Id = "POH-001", Name = "Pohon Pøímý",    Category = "Pohon",   IsAvailable = true  },
        new ComponentMock { Id = "POH-002", Name = "Pohon Pøevodový",Category = "Pohon",   IsAvailable = false },

        new ComponentMock { Id = "SED-001", Name = "Sedák Základní", Category = "Sedák",   IsAvailable = true  },
        new ComponentMock { Id = "SED-002", Name = "Sedák Ortopedický", Category = "Sedák",IsAvailable = true  },

        new ComponentMock { Id = "OPE-001", Name = "Opìrka Pevná",   Category = "Opìrka", IsAvailable = true  },
        new ComponentMock { Id = "OPE-002", Name = "Opìrka Sklopná", Category = "Opìrka", IsAvailable = true  },
    ];

    private readonly Dictionary<string, ComponentMock?> _selectedComponents = [];

    private VulkanHelper? vulkan = null;
    private CancellationTokenSource _cts = default!;

    public WheelchairConfiguratorPage()
    {
        InitializeComponent();

        foreach (var category in ComponentCategories.All)
            _selectedComponents[category] = null;

        vulkan = new VulkanHelper("app", 800, 600);

        vulkan.AddObject("models/test");
        vulkan.Initialize();
        vulkan.Render();
        MyImage.Source = vulkan.GetRenderedImageSource();

        StartRenderLoop();
        StartTimer();
    }

    ~WheelchairConfiguratorPage()
    {
        StopRenderLoop();//time shares same cts as render loop
    }

    private void StartRenderLoop()
    {
        _cts = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                vulkan?.Render();
                await Task.Yield();
            }
        });
    }

    private void StopRenderLoop()
    {
        _cts?.Cancel();
        vulkan = null;
    }
    private void StartTimer()
    {
        _cts = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));

            while (await timer.WaitForNextTickAsync(_cts.Token))
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (vulkan != null)
                    {
                        MyImage.Source = vulkan.GetRenderedImageSource();
                    }
                });
            }
        });
    }
    private void LoadMockData()
    {
        // Info o pacientovi
        PatientIdLabel.Text = _patient.PatientIdentificator;
        BodyHeightLabel.Text = $"Výška trupu: {_patient.BodyHeight} cm";
        PelvisWidthLabel.Text = $"Šíøka pánve: {_patient.PelvisWidth} cm";
        ThighLengthLabel.Text = $"Délka stehna: {_patient.ThighLength} cm";
        WeightLabel.Text = $"Hmotnost: {_patient.Weight} kg";
        BodyStabilityLabel.Text = $"Stabilita trupu: {_patient.BodyStability}";
        HeadStabilityLabel.Text = $"Kontrola hlavy: {(_patient.HeadStability ? "Ano" : "Ne")}";
        BedsoreRiskLabel.Text = $"Riziko dekubitù: {_patient.BedsoreRisk}";
        ControlLabel.Text = $"Ovládání rukou: {_patient.Control}";
        EnvironmentLabel.Text = $"Prostøedí: {_patient.Environment}";
        LegsLabel.Text = $"Dolní konèetiny: {(_patient.Legs ? "Ano" : "Ne")}";
        PainLabel.Text = $"Bolesti a únava: {_patient.Pain}";
        DateLabel.Text = $"Datum: {_patient.Date:dd.MM.yyyy}";

        BuildComponentPanels();
    }

    /*
     * BuildComponentPanels - dynamicky vytvoøí sekce kategorií s komponentami
     */
    private void BuildComponentPanels()
    {
        ComponentsLayout.Children.Clear();

        foreach (var category in ComponentCategories.All)
        {
            // Název kategorie
            ComponentsLayout.Children.Add(new Label
            {
                Text = category,
                FontAttributes = FontAttributes.Bold,
                FontSize = 15,
                Margin = new Thickness(0, 10, 0, 4)
            });

            var componentsInCategory = _components
                .Where(c => c.Category == category)
                .ToList();

            foreach (var component in componentsInCategory)
            {
                var border = new Border
                {
                    Padding = new Thickness(10, 8),
                    Margin = new Thickness(0, 2),
                    StrokeThickness = 1,
                    Stroke = Colors.LightGray,
                    BackgroundColor = component.IsAvailable ? Colors.White : Color.FromArgb("#E0E0E0")
                };

                var label = new Label
                {
                    Text = component.Name,
                    FontSize = 13,
                    TextColor = component.IsAvailable ? Colors.Black : Colors.Gray
                };

                border.Content = label;

                if (component.IsAvailable)
                {
                    var tapped = new TapGestureRecognizer();
                    tapped.Tapped += (s, e) => OnComponentTapped(component, border);
                    border.GestureRecognizers.Add(tapped);
                }

                ComponentsLayout.Children.Add(border);
            }
        }
    }

    /*
     * OnComponentTapped - výbìr komponenty v kategorii
     */
    private readonly Dictionary<string, Border?> _selectedBorders = [];

    private void OnComponentTapped(ComponentMock component, Border tappedBorder)
    {
        // Odznaè pøedchozí výbìr ve stejné kategorii
        if (_selectedBorders.TryGetValue(component.Category, out var previousBorder)
            && previousBorder is not null)
        {
            previousBorder.Stroke = Colors.LightGray;
            previousBorder.BackgroundColor = Colors.White;
        }

        // Oznaè nový výbìr
        tappedBorder.Stroke = Color.FromArgb("#512BD4");
        tappedBorder.BackgroundColor = Color.FromArgb("#EDE8FC");

        _selectedBorders[component.Category] = tappedBorder;
        _selectedComponents[component.Category] = component;

        // Zkontroluj jestli jsou vybrány všechny kategorie
        ContinueBtn.IsEnabled = _selectedComponents.Values.All(c => c is not null);
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("patientSelectPage");
    }

    private async void OnContinueClicked(object sender, EventArgs e)
    {
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
                System.Diagnostics.Debug.WriteLine($"Delta: {delta.X:F1}, {delta.Y:F1}");

                //TODO: set intensity
                vulkan?.AddRotationXY(-(float)delta.Y, (float)delta.X);

                break;
        }
    }
}