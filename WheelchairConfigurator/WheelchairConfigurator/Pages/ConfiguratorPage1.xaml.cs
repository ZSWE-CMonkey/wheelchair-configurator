using WheelchairConfigurator.Components;

namespace WheelchairConfigurator.Pages;


/*
 * Cus Pasta, necham ti tu, jak jsem nad tim uvazoval, az to budes delat. Na tyhle strance je container pro sidebary a nahled toho vozejku. V tom containeru se toci sidebary se vstupama. Pri zmene jakehokoli vstupu se ty data rovnou ulozi do userInput. Ty data se ukladaji rovnou a nejsou nijak checkovana, takze budes muset overit nejake min a max hodnoty. Je tam ciselny input, ale asi tam pujde narvat treba + -, takze by to taky mohlo delat brikule. 
 */


public partial class ConfiguratorPage1 : ContentPage
{
    public UserInput userInput = new();
    private readonly List<ContentView> _panels;
    private int _currentPanelIndex = 0;

    /*
     * Constructor - creates panels
     */
    public ConfiguratorPage1()
    {
        InitializeComponent();
        _panels = [
            new SideBar1(userInput),
            new SideBar2(userInput),
            new SideBar3(userInput)
        ];
        UpdatePanel();
    }

    /*
     * SaveCurrentPanel - Saves all inputs in sidebar
     *                  - Inputs are saved automatically on their changes, this function does it again for sure if there is something unsaved
     */
    private void SaveCurrentPanel()
    {
        if (_panels[_currentPanelIndex] is ISideBar sidebar)
            sidebar.Save();
    }

    /*
     * UpdatePanel - updates panel in container using current id
     *             - handles button text logic
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
            // There is a final point of configuration.
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