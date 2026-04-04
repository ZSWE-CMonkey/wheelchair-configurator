namespace WheelchairConfigurator.Components;

public partial class SideBar3 : ContentView, ISideBar
{
    private readonly UserInput _userInput;

    public SideBar3(UserInput userInput)
    {
        InitializeComponent();
        _userInput = userInput;
    }

    /*
     *  Save - saving whole sidebar
     */
    public void Save()
    {
        SaveHandControl();
        SaveEnvironment();
    }

    /*
     * Saving methods
     */
    private void SaveHandControl() =>
        _userInput.HandControl = handControlEntry.SelectedItem as string == "Ano";

    private void SaveEnvironment() =>
        _userInput.Environment = enviromentEntry.SelectedItem as string ?? "";
    
    /*
     * Changes handlers
     */
    private void OnHandControlChanged(object sender, EventArgs e) => SaveHandControl();
    private void OnEnvironmentChanged(object sender, EventArgs e) => SaveEnvironment();
}