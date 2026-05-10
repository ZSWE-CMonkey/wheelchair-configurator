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
        View[] shared = [_panels[0], _panels[1], _saveBtn, _backBtn, _titleLabel];
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
        var grid = new Grid
        {
            Padding = new Thickness(20),
            ColumnSpacing = 20,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
            }
        };

        Grid.SetColumn(_panels[0], 0);
        Grid.SetColumn(_panels[1], 1);
        grid.Children.Add(_panels[0]);

        var rightCol = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
            }
        };
        Grid.SetRow(_titleLabel, 0);
        Grid.SetRow(_panels[1], 1);
        Grid.SetRow(_saveBtn, 2);
        Grid.SetRow(_backBtn, 3);
        rightCol.Children.Add(_titleLabel);
        rightCol.Children.Add(_panels[1]);
        rightCol.Children.Add(_saveBtn);
        rightCol.Children.Add(_backBtn);

        Grid.SetColumn(rightCol, 1);
        grid.Children.Add(rightCol);

        return grid;
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
        stack.Children.Add(_backBtn);
        return new ScrollView { Content = stack };
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        foreach (var panel in _panels)
        {
            if (panel is ISideBar sidebar && !sidebar.Validate())
            {
                await DisplayAlert("Chyba", "Vyplňte prosím všechna pole.", "OK");
                return;
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
        await _appService.SaveMeasurementAsync(measurement);

        await Shell.Current.GoToAsync("patientManagerPage");
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("patientManagerPage");
    }
}
