using WheelchairConfigurator.Components;

namespace WheelchairConfigurator.Pages;


/*
 * Cus Pasta, necham ti tu, jak jsem nad tim uvazoval, az to budes delat. Na tyhle strance je container pro sidebary a nahled toho vozejku. V tom containeru se toci sidebary se vstupama. Moje idea je, ze pri prepnuti toho sidebaru se ulozi vsechny vstupy, co na nem jsou. Tim, ze je to vsechno na jedne strance, budes mit ty data pohromade. Asi si muzes udelat tridu pro vozik, do ktere to vsechno ulozis, a pak s tim objektem budes pracovat dal. Snad jsem to neudelal uplne napicu a pomuze ti to :)
 */


public partial class ConfiguratorPage1 : ContentPage
{
    private List<ContentView> _panels;
    private int _currentPanelIndex = 0;


    /*
     * Constructor - creates panels
     */
    public ConfiguratorPage1()
    {
        InitializeComponent();

        _panels = [new SideBar1(), new SideBar2(), new SideBar3()];

        UpdatePanel();
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
        if (_currentPanelIndex < _panels.Count - 1)
        {
            _currentPanelIndex++;
            UpdatePanel();
        }
        else
        {
            // There is a final point of configuration.
            DisplayAlert("Hotovo", "Vozík byl úspìšnì nakonfigurován!", "OK");
        }
    }

    /*
     * OnBackClicked - BackBtn handler
     */
    private void OnBackClicked(object sender, EventArgs e)
    {
        if (_currentPanelIndex > 0)
        {
            _currentPanelIndex--;
            UpdatePanel();
        }
    }
}