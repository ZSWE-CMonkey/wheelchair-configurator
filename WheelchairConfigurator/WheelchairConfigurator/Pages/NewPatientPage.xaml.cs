using WheelchairConfigurator.Components;

namespace WheelchairConfigurator.Pages;

public partial class NewPatientPage : ContentPage
{
    public UserInput userInput = new();
    private readonly List<ContentView> _panels;

    public NewPatientPage()
    {
        InitializeComponent();

        _panels =
        [
            new SideBarView("Informace o pacientovi",
            [
            new SideBarField
                {
                    Label = "Identifikátor pacienta",
                    Type = FieldType.Entry,
                    Keyboard = Keyboard.Default,
                    MaxLength = 20,
                    OnSave = v => userInput.patientIdentificator = v
                },
                new SideBarField
                {
                    Label = "Výška trupu (cm)",
                    Type = FieldType.Entry,
                    OnSave = v => userInput.BodyHeight = double.TryParse(v, out var x) ? x : 0
                },
                new SideBarField
                {
                    Label = "Hmotnost (kg)",
                    Type = FieldType.Entry,
                    OnSave = v => userInput.Weight = double.TryParse(v, out var x) ? x : 0
                },
                new SideBarField
                {
                    Label = "Šíøka pánve (cm)",
                    Type = FieldType.Entry,
                    OnSave = v => userInput.PelvisWidth = double.TryParse(v, out var x) ? x : 0
                },
                new SideBarField
                {
                    Label = "Délka stehna (cm)",
                    Type = FieldType.Entry,
                    OnSave = v => userInput.ThighLength = double.TryParse(v, out var x) ? x : 0
                },
            ]),

            new SideBarView("",
            [
                new SideBarField
                {
                    Label = "Stabilita trupu",
                    Type = FieldType.Picker,
                    Options = ["Dobrá", "Støední", "Špatná"],
                    OnSave = v => userInput.BodyStability = v
                },
                new SideBarField
                {
                    Label = "Kontrola hlavy",
                    Type = FieldType.Picker,
                    Options = ["Ano", "Ne"],
                    OnSave = v => userInput.HeadStability = v == "Ano"
                },
                new SideBarField
                {
                    Label = "Riziko dekubitù (proleženin)",
                    Type = FieldType.Picker,
                    Options = ["Nízké", "Støední", "Vysoké"],
                    OnSave = v => userInput.BedsoreRisk = v
                },
                new SideBarField
                {
                    Label = "Bolesti a únava",
                    Type = FieldType.Picker,
                    Options = ["Nízké", "Støední", "Vysoké"],
                    OnSave = v => userInput.Pain = v
                },
                new SideBarField
                {
                    Label = "Dolní konèetiny",
                    Type = FieldType.Picker,
                    Options = ["Ano", "Ne"],
                    OnSave = v => userInput.Legs = v == "Ano"
                },
            ]),

            new SideBarView("Informace o vozíku",
            [
                new SideBarField
                {
                    Label = "Ovládání",
                    Type = FieldType.Picker,
                    Options = ["Hand control", " Head control", " Sip & puff"],
                    OnSave = v => userInput.Control = v
                },
                new SideBarField
                {
                    Label = "Terén a prostøedí",
                    Type = FieldType.Picker,
                    Options = ["Indoor", "Outdoor", "Kombinace"],
                    OnSave = v => userInput.Environment = v
                },
            ]),
        ];

        Panel1.Content = _panels[0];
        Panel2.Content = _panels[1];
        Panel3.Content = _panels[2];
    }

    /*
     * SaveAllPanels - uloží vstupy ze všech tøí panelù najednou
     */
    private void SaveAllPanels()
    {
        foreach (var panel in _panels)
        {
            if (panel is ISideBar sidebar)
                sidebar.Save();
        }
    }

    /*
     * OnFinishClicked - uloží vše a pokraèuje dál
     */
    private void OnFinishClicked(object sender, EventArgs e)
    {
        foreach (var panel in _panels)
        {
            if (panel is ISideBar sidebar && !sidebar.Validate())
            {
                DisplayAlert("Chyba", "Vyplòte prosím všechna pole.", "OK");
                return;
            }
        }

        SaveAllPanels();
        userInput.Date = DateTime.Today;
        DisplayAlert("Hotovo", $"Výška: {userInput.BodyHeight}", "OK");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("mainPage");
    }
}