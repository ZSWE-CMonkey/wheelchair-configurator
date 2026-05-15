using WheelchairConfigurator.Components;
using WheelchairConfigurator.ServiceLayer.Interfaces;
using WheelchairConfigurator.ServiceLayer.Models;
using WheelchairConfigurator.Services;

namespace WheelchairConfigurator.Pages;

[QueryProperty(nameof(PatientId), "patientId")]
public partial class NewMeasurementPage : ContentPage
{
    private readonly IAppService _appService;
    private readonly NavigationState _navState;

    private int _patientId;
    private PatientModel? _patient;

    public int PatientId
    {
        get => _patientId;
        set
        {
            _patientId = value;
            Dispatcher.Dispatch(async () => await LoadPatient());
        }
    }

    public UserInput userInput = new();

    private readonly List<ContentView> _panels;
    private readonly Button _saveBtn;
    private readonly Button _configureBtn;
    private readonly Button _backBtn;
    private readonly Label _titleLabel;
    private bool _isLandscape;

    public NewMeasurementPage(IAppService appService, NavigationState navState)
    {
        _appService = appService;
        _navState = navState;
        InitializeComponent();

        _titleLabel = new Label
        {
            Text = "Nové měření",
            FontAttributes = FontAttributes.Bold,
            FontSize = 20,
            Margin = new Thickness(0, 0, 0, 12)
        };

        _panels =
        [
            new SideBarView("Tělesné parametry",
            [
                new SideBarField { Label = "Výška trupu (cm)",    Type = FieldType.Entry, OnSave = v => userInput.BodyHeight  = double.TryParse(v, out var x) ? x : 0 },
                new SideBarField { Label = "Hmotnost (kg)",       Type = FieldType.Entry, OnSave = v => userInput.Weight      = double.TryParse(v, out var x) ? x : 0 },
                new SideBarField { Label = "Šířka pánve (cm)",    Type = FieldType.Entry, OnSave = v => userInput.PelvisWidth = double.TryParse(v, out var x) ? x : 0 },
                new SideBarField { Label = "Délka stehna (cm)",   Type = FieldType.Entry, OnSave = v => userInput.ThighLength = double.TryParse(v, out var x) ? x : 0 },
            ]),

            new SideBarView("Zdravotní parametry",
            [
                new SideBarField { Label = "Stabilita trupu",              Type = FieldType.Picker, Options = ["Dobrá", "Střední", "Špatná"],   OnSave = v => userInput.BodyStability = v },
                new SideBarField { Label = "Kontrola hlavy",               Type = FieldType.Picker, Options = ["Ano", "Ne"],                    OnSave = v => userInput.HeadStability = v == "Ano" },
                new SideBarField { Label = "Riziko dekubitů (proleženin)", Type = FieldType.Picker, Options = ["Nízké", "Střední", "Vysoké"],   OnSave = v => userInput.BedsoreRisk   = v },
                new SideBarField { Label = "Bolesti a únava",              Type = FieldType.Picker, Options = ["Nízké", "Střední", "Vysoké"],   OnSave = v => userInput.Pain          = v },
                new SideBarField { Label = "Dolní končetiny",              Type = FieldType.Picker, Options = ["Ano", "Ne"],                    OnSave = v => userInput.Legs          = v == "Ano" },
                new SideBarField { Label = "Ovládání",                     Type = FieldType.Picker, Options = ["Hand control", "Head control", "Sip & puff"], OnSave = v => userInput.Control = v },
                new SideBarField { Label = "Terén a prostředí",            Type = FieldType.Picker, Options = ["Indoor", "Outdoor", "Kombinace"],             OnSave = v => userInput.Environment = v },
            ]),
        ];

        _saveBtn = new Button
        {
            Text = "Uložit měření",
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(0, 0, 0, 8)
        };
        _saveBtn.Clicked += OnSaveClicked;

        _configureBtn = new Button
        {
            Text = "Uložit a konfigurovat vozík",
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(0, 0, 0, 8)
        };
        _configureBtn.Clicked += OnSaveAndConfigureClicked;

        _backBtn = new Button
        {
            Text = "Zpět",
            BackgroundColor = Colors.Gray,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Fill
        };
        _backBtn.Clicked += OnBackClicked;
    }

    private async Task LoadPatient()
    {
        if (_patientId <= 0) return;
        var patients = await _appService.GetPatientsAsync();
        _patient = patients.FirstOrDefault(p => p.Id == _patientId);
        if (_patient is not null)
            _titleLabel.Text = $"Nové měření — {_patient.FullName}";
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
        View[] shared = [_panels[0], _panels[1], _saveBtn, _configureBtn, _backBtn, _titleLabel];
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
        var panelsGrid = new Grid
        {
            ColumnSpacing = 20,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
            }
        };
        Grid.SetColumn(_panels[0], 0);
        Grid.SetColumn(_panels[1], 1);
        panelsGrid.Children.Add(_panels[0]);
        panelsGrid.Children.Add(_panels[1]);

        var buttonsRow = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            }
        };
        Grid.SetColumn(_saveBtn, 0);
        Grid.SetColumn(_configureBtn, 1);
        Grid.SetColumn(_backBtn, 2);
        buttonsRow.Children.Add(_saveBtn);
        buttonsRow.Children.Add(_configureBtn);
        buttonsRow.Children.Add(_backBtn);

        var scroll = new ScrollView { Content = panelsGrid };

        var outer = new Grid
        {
            Padding = new Thickness(20),
            RowSpacing = 12,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
            }
        };
        Grid.SetRow(_titleLabel, 0);
        Grid.SetRow(scroll, 1);
        Grid.SetRow(buttonsRow, 2);
        outer.Children.Add(_titleLabel);
        outer.Children.Add(scroll);
        outer.Children.Add(buttonsRow);

        return outer;
    }

    private View BuildPortraitLayout()
    {
        var stack = new VerticalStackLayout
        {
            Padding = new Thickness(20),
            Spacing = 16
        };
        stack.Children.Add(_titleLabel);
        stack.Children.Add(_panels[0]);
        stack.Children.Add(_panels[1]);
        stack.Children.Add(_saveBtn);
        stack.Children.Add(_configureBtn);
        stack.Children.Add(_backBtn);
        return new ScrollView { Content = stack };
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        SetBusy(true);
        var saved = await TrySaveMeasurement();
        SetBusy(false);
        if (saved is not null)
            await Shell.Current.GoToAsync("patientManagerPage");
    }

    private async void OnSaveAndConfigureClicked(object? sender, EventArgs e)
    {
        SetBusy(true);
        var saved = await TrySaveMeasurement();
        SetBusy(false);
        if (saved is null) return;
        _navState.ActiveMeasurement = saved;
        _navState.SelectedComponents = [];
        await Shell.Current.GoToAsync("wheelchairConfiguratorPage");
    }

    private void SetBusy(bool busy)
    {
        _saveBtn.IsEnabled = !busy;
        _configureBtn.IsEnabled = !busy;
        _saveBtn.Text = busy ? "Ukládám..." : "Uložit měření";
        _configureBtn.Text = busy ? "Ukládám..." : "Uložit a konfigurovat vozík";
    }

    private async Task<PatientMeasurementModel?> TrySaveMeasurement()
    {
        foreach (var panel in _panels)
        {
            if (panel is ISideBar sidebar && !sidebar.Validate())
            {
                await DisplayAlert("Chyba", "Vyplňte prosím všechna pole.", "OK");
                return null;
            }
        }

        foreach (var panel in _panels)
            if (panel is ISideBar sidebar)
                sidebar.Save();

        userInput.Date = DateTime.Today;

        var specialist = _navState.ActiveSpecialist;

        var measurement = new PatientMeasurementModel
        {
            PatientId = _patientId,
            PatientBirthNumber = _patient?.BirthNumber ?? "",
            PatientFullName = _patient?.FullName ?? "",
            MeasuredAt = DateTime.Now,
            CreatedBySpecialistId = specialist?.Id ?? 1,
            CreatedBySpecialistName = specialist?.FullName ?? "",
            BodyHeight = userInput.BodyHeight,
            PelvisWidth = userInput.PelvisWidth,
            ThighLength = userInput.ThighLength,
            Weight = userInput.Weight,
            BodyStability = userInput.BodyStability,
            HeadStability = userInput.HeadStability,
            BedsoreRisk = userInput.BedsoreRisk,
            Control = userInput.Control,
            Environment = userInput.Environment,
            Legs = userInput.Legs,
            Pain = userInput.Pain,
        };
        return await _appService.SaveMeasurementAsync(measurement);
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
