using WheelchairConfigurator.Helpers;


namespace WheelchairConfigurator.Pages;

public partial class SummaryPage : ContentPage
{
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

    // Mock vybraných komponent - jedna z každé kategorie
    private readonly List<ComponentMock> _selectedComponents =
    [
        new ComponentMock { Id = "RAM-001", Name = "Rám Standard",      Category = "Rám",     IsAvailable = true },
        new ComponentMock { Id = "MOT-001", Name = "Motor 250W",        Category = "Motor",   IsAvailable = true },
        new ComponentMock { Id = "BAT-002", Name = "Baterie 20Ah",      Category = "Baterie", IsAvailable = true },
        new ComponentMock { Id = "POH-001", Name = "Pohon Pøímý",       Category = "Pohon",   IsAvailable = true },
        new ComponentMock { Id = "SED-002", Name = "Sedák Ortopedický", Category = "Sedák",   IsAvailable = true },
        new ComponentMock { Id = "OPE-002", Name = "Opìrka Sklopná",    Category = "Opìrka",  IsAvailable = true },
    ];

    public SummaryPage()
    {
        InitializeComponent();
        LoadPatientData();
        BuildComponentsList();

        VulkanHelper vulkan = new VulkanHelper("app", 800, 600);

        vulkan.AddObject("models/test");

        MyImage.Source = vulkan.GetRenderedImageSource();
    }

    /*
     * LoadPatientData - naplní labels daty pacienta
     */
    private void LoadPatientData()
    {
        PatientIdLabel.Text = _patient.PatientIdentificator;
        DateLabel.Text = $"{_patient.Date:dd.MM.yyyy}";
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
    }

    /*
     * BuildComponentsList - sestaví seznam vybraných komponent podle kategorií
     */
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

    /*
     * OnMainMenuClicked - navigace na hlavní menu
     */
    private async void OnMainMenuClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("mainPage");

    }

    /*
     * OnBackClicked - navigace zpìt na konfigurátor
     */
    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("wheelchairConfiguratorPage");
    }

    /*
     * OnExportClicked - export rekapitulace
     */
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
                System.Diagnostics.Debug.WriteLine($"Delta: {delta.X:F1}, {delta.Y:F1}");
                break;
        }
    }
}