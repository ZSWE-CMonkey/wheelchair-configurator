using WheelchairConfigurator.Components;
using WheelchairConfigurator.ServiceLayer.Interfaces;
using WheelchairConfigurator.ServiceLayer.Models;
using WheelchairConfigurator.Services;

namespace WheelchairConfigurator.Pages;

public partial class NewPatientPage : ContentPage
{
    public UserInput userInput = new();
    private string _birthNumber = string.Empty;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;

    private readonly List<ContentView> _panels;
    private readonly NavigationState _navState;
    private readonly IAppService _appService;

    private readonly Button _continueBtn;
    private readonly Button _backBtn;

    private bool _isLandscape;

    public NewPatientPage(NavigationState navState, IAppService appService)
    {
        _navState = navState;
        _appService = appService;
        InitializeComponent();

        _panels =
        [
            new SideBarView("Informace o pacientovi",
            [
                new SideBarField { Label = "Rodné číslo",              Type = FieldType.Entry, Keyboard = Keyboard.Default, MaxLength = 20, OnSave = v => _birthNumber          = v },
                new SideBarField { Label = "Jméno",                    Type = FieldType.Entry, Keyboard = Keyboard.Default, MaxLength = 50, OnSave = v => _firstName             = v },
                new SideBarField { Label = "Příjmení",                 Type = FieldType.Entry, Keyboard = Keyboard.Default, MaxLength = 50, OnSave = v => _lastName              = v },
                new SideBarField { Label = "Výška trupu (cm)",         Type = FieldType.Entry, OnSave = v => userInput.BodyHeight   = double.TryParse(v, out var x) ? x : 0 },
                new SideBarField { Label = "Hmotnost (kg)",            Type = FieldType.Entry, OnSave = v => userInput.Weight       = double.TryParse(v, out var x) ? x : 0 },
                new SideBarField { Label = "Šířka pánve (cm)",         Type = FieldType.Entry, OnSave = v => userInput.PelvisWidth  = double.TryParse(v, out var x) ? x : 0 },
                new SideBarField { Label = "Délka stehna (cm)",        Type = FieldType.Entry, OnSave = v => userInput.ThighLength  = double.TryParse(v, out var x) ? x : 0 },
            ]),

            new SideBarView("",
            [
                new SideBarField { Label = "Stabilita trupu",             Type = FieldType.Picker, Options = ["Dobrá", "Střední", "Špatná"],   OnSave = v => userInput.BodyStability  = v },
                new SideBarField { Label = "Kontrola hlavy",              Type = FieldType.Picker, Options = ["Ano", "Ne"],                    OnSave = v => userInput.HeadStability  = v == "Ano" },
                new SideBarField { Label = "Riziko dekubitů (proleženin)",Type = FieldType.Picker, Options = ["Nízké", "Střední", "Vysoké"],   OnSave = v => userInput.BedsoreRisk    = v },
                new SideBarField { Label = "Bolesti a únava",             Type = FieldType.Picker, Options = ["Nízké", "Střední", "Vysoké"],   OnSave = v => userInput.Pain           = v },
                new SideBarField { Label = "Dolní končetiny",             Type = FieldType.Picker, Options = ["Ano", "Ne"],                    OnSave = v => userInput.Legs           = v == "Ano" },
            ]),

            new SideBarView("Informace o vozíku",
            [
                new SideBarField { Label = "Ovládání",            Type = FieldType.Picker, Options = ["Hand control", "Head control", "Sip & puff"],  OnSave = v => userInput.Control     = v },
                new SideBarField { Label = "Terén a prostředí",   Type = FieldType.Picker, Options = ["Indoor", "Outdoor", "Kombinace"],              OnSave = v => userInput.Environment = v },
            ]),
        ];

        _continueBtn = new Button
        {
            Text = "Pokračovat",
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(0, 0, 0, 8)
        };
        _continueBtn.Clicked += OnFinishClicked;

        _backBtn = new Button
        {
            Text = "Zpět",
            BackgroundColor = Colors.Red,
            HorizontalOptions = LayoutOptions.Fill
        };
        _backBtn.Clicked += OnBackClicked;
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
        View[] shared = [_panels[0], _panels[1], _panels[2], _continueBtn, _backBtn];

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
                new ColumnDefinition(GridLength.Star),
            }
        };
        Grid.SetColumn(_panels[0], 0);
        Grid.SetColumn(_panels[1], 1);
        Grid.SetColumn(_panels[2], 2);
        panelsGrid.Children.Add(_panels[0]);
        panelsGrid.Children.Add(_panels[1]);
        panelsGrid.Children.Add(_panels[2]);

        var buttonsRow = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
            }
        };
        Grid.SetColumn(_continueBtn, 0);
        Grid.SetColumn(_backBtn, 1);
        buttonsRow.Children.Add(_continueBtn);
        buttonsRow.Children.Add(_backBtn);

        var scroll = new ScrollView { Content = panelsGrid };

        var outer = new Grid
        {
            Padding = new Thickness(20),
            RowSpacing = 12,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
            }
        };
        Grid.SetRow(scroll, 0);
        Grid.SetRow(buttonsRow, 1);
        outer.Children.Add(scroll);
        outer.Children.Add(buttonsRow);

        return outer;
    }

    private View BuildPortraitLayout()
    {
        var panelsStack = new VerticalStackLayout { Spacing = 16 };
        panelsStack.Children.Add(_panels[0]);
        panelsStack.Children.Add(_panels[1]);
        panelsStack.Children.Add(_panels[2]);

        var buttonsRow = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
            }
        };
        Grid.SetColumn(_continueBtn, 0);
        Grid.SetColumn(_backBtn, 1);
        buttonsRow.Children.Add(_continueBtn);
        buttonsRow.Children.Add(_backBtn);

        var scroll = new ScrollView { Content = panelsStack };

        var outer = new Grid
        {
            Padding = new Thickness(20),
            RowSpacing = 12,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
            }
        };
        Grid.SetRow(scroll, 0);
        Grid.SetRow(buttonsRow, 1);
        outer.Children.Add(scroll);
        outer.Children.Add(buttonsRow);

        return outer;
    }

    private void SaveAllPanels()
    {
        foreach (var panel in _panels)
            if (panel is ISideBar sidebar)
                sidebar.Save();
    }

    private async void OnFinishClicked(object? sender, EventArgs e)
    {
        foreach (var panel in _panels)
        {
            if (panel is ISideBar sidebar && !sidebar.Validate())
            {
                await DisplayAlert("Chyba", "Vyplňte prosím všechna pole.", "OK");
                return;
            }
        }

        SaveAllPanels();

        var digits = _birthNumber.Replace("/", "").Replace(" ", "");
        if ((digits.Length != 9 && digits.Length != 10) || !digits.All(char.IsDigit))
        {
            await DisplayAlert("Chyba", "Rodné číslo musí mít 9 nebo 10 číslic (formát YYMMDDXXXX nebo YYMMDD/XXXX).", "OK");
            return;
        }

        userInput.Date = DateTime.Today;

        var specialist = _navState.ActiveSpecialist;

        var patientModel = new PatientModel
        {
            BirthNumber = _birthNumber,
            FirstName = _firstName,
            LastName = _lastName,
            CreatedBySpecialistId = specialist?.Id ?? 1,
            CreatedBySpecialistName = specialist?.FullName ?? "",
        };
        var savedPatient = await _appService.SavePatientAsync(patientModel);

        var measurement = new PatientMeasurementModel
        {
            PatientId = savedPatient.Id,
            PatientBirthNumber = _birthNumber,
            PatientFullName = savedPatient.FullName,
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
        var savedMeasurement = await _appService.SaveMeasurementAsync(measurement);

        _navState.ActiveMeasurement = savedMeasurement;
        _navState.SelectedComponents.Clear();

        await Shell.Current.GoToAsync("wheelchairConfiguratorPage");
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
