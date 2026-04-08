using WheelchairConfigurator.Components;

namespace WheelchairConfigurator.Pages;

public partial class ConfiguratorPage1 : ContentPage
{
    public UserInput userInput = new();
    private readonly List<ContentView> _panels;
    private int _currentPanelIndex = 0;

    /*
     * Constructor - creates panels using generic SideBarView
     */
    public ConfiguratorPage1()
    {
        InitializeComponent();
        _panels =
        [
            new SideBarView("Informace o pacientovi I.",
            [
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

            new SideBarView("Informace o pacientovi II.",
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

            new SideBarView("Vozík",
            [
                new SideBarField
                {
                    Label = "Ovládání rukou",
                    Type = FieldType.Picker,
                    Options = ["Ano", "Ne"],
                    OnSave = v => userInput.HandControl = v == "Ano"
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

        UpdatePanel();
    }

    /*
     * SaveCurrentPanel - saves all inputs in current sidebar
     */
    private void SaveCurrentPanel()
    {
        if (_panels[_currentPanelIndex] is ISideBar sidebar)
            sidebar.Save();
    }

    /*
     * UpdatePanel - updates panel and handles button visibility/text
     */
    private void UpdatePanel()
    {
        SidebarContainer.Content = _panels[_currentPanelIndex];
        BackBtn.IsVisible = _currentPanelIndex > 0;
        NextBtn.Text = _currentPanelIndex == _panels.Count - 1 ? "Dokonèit" : "Další";
    }

    /*
     * OnNextClicked - NextBtn handler
     */
    private void OnNextClicked(object sender, EventArgs e)
    {
        SaveCurrentPanel();
        if (_currentPanelIndex < _panels.Count - 1)
        {
            _currentPanelIndex++;
            UpdatePanel();
        }
        else
        {
            DisplayAlert("Hotovo", $"Výška: {userInput.BodyHeight}", "OK");
        }
    }

    /*
     * OnBackClicked - BackBtn handler
     */
    private void OnBackClicked(object sender, EventArgs e)
    {
        SaveCurrentPanel();
        if (_currentPanelIndex > 0)
        {
            _currentPanelIndex--;
            UpdatePanel();
        }
    }
}