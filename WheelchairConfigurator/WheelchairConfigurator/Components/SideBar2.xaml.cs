namespace WheelchairConfigurator.Components;

public partial class SideBar2 : ContentView, ISideBar
{
    private readonly UserInput _userInput;

    public SideBar2(UserInput userInput)
    {
        InitializeComponent();
        _userInput = userInput;
    }

    /*
     *  Save - saving whole sidebar
     */
    public void Save()
    {
        SaveBodyStability();
        SaveHeadStability();
        SaveBedsoreRisk();
        SavePain();
        SaveLegs();
    }

    /*
     * Saving methods
     */
    private void SaveBodyStability() =>
        _userInput.BodyStability = bodyStabilityEntry.SelectedItem as string ?? "";

    private void SaveHeadStability() =>
        _userInput.HeadStability = headStabilityEntry.SelectedItem as string == "Ano";

    private void SaveBedsoreRisk() =>
        _userInput.BedsoreRisk = bedsoresRiskEntry.SelectedItem as string ?? "";

    private void SavePain() =>
        _userInput.Pain = painEntry.SelectedItem as string ?? "";

    private void SaveLegs() =>
        _userInput.Legs = legsEntry.SelectedItem as string == "Ano";

    /*
     * Changes handlers
     */
    private void OnBodyStabilityChanged(object sender, EventArgs e) => SaveBodyStability();
    private void OnHeadStabilityChanged(object sender, EventArgs e) => SaveHeadStability();
    private void OnBedsoreRiskChanged(object sender, EventArgs e) => SaveBedsoreRisk();
    private void OnPainChanged(object sender, EventArgs e) => SavePain();
    private void OnLegsChanged(object sender, EventArgs e) => SaveLegs();
}