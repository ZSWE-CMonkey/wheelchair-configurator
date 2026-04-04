namespace WheelchairConfigurator.Components;

public partial class SideBar1 : ContentView, ISideBar
{
    private readonly UserInput _userInput;

    public SideBar1(UserInput userInput)
    {
        InitializeComponent();
        _userInput = userInput;
    }

    /*
     *  Save - saving whole sidebar
     */
    public void Save()
    {
        SaveBodyHeight();
        SaveWeight();
        SavePelvisWidth();
        SaveThighLength();
    }

    /*
     * Saving methods
     */
    private void SaveBodyHeight() =>
        _userInput.BodyHeight = double.TryParse(bodyHeightEntry.Text, out var v) ? v : 0;

    private void SaveWeight() =>
        _userInput.Weight = double.TryParse(weightEntry.Text, out var v) ? v : 0;

    private void SavePelvisWidth() =>
        _userInput.PelvisWidth = double.TryParse(pelvisWidthEntry.Text, out var v) ? v : 0;

    private void SaveThighLength() =>
        _userInput.ThighLength = double.TryParse(thighLengthEntry.Text, out var v) ? v : 0;

    /*
     * Changes handlers
     */
    private void OnBodyHeightChanged(object sender, TextChangedEventArgs e) => SaveBodyHeight();
    private void OnWeightChanged(object sender, TextChangedEventArgs e) => SaveWeight();
    private void OnPelvisWidthChanged(object sender, TextChangedEventArgs e) => SavePelvisWidth();
    private void OnThighLengthChanged(object sender, TextChangedEventArgs e) => SaveThighLength();
}